using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.Text.Json;
using ConversionService.Application.Contracts;
using ConversionService.Application.Services;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;

namespace ConversionService.Infrastructure.Messaging
{
    public sealed class RabbitMqConsumerHostedService : BackgroundService
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly RabbitMqOptions _options;
        private readonly ILogger<RabbitMqConsumerHostedService> _logger;

        private IConnection? _connection;
        private IModel? _channel;

        public RabbitMqConsumerHostedService(
            IServiceProvider serviceProvider,
            IOptions<RabbitMqOptions> options,
            ILogger<RabbitMqConsumerHostedService> logger)
        {
            _serviceProvider = serviceProvider;
            _options = options.Value;
            _logger = logger;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            EnsureConnected();

            AsyncEventingBasicConsumer consumer = new(_channel!);
            consumer.Received += async (_, eventArgs) =>
            {
                await HandleMessageAsync(eventArgs, stoppingToken);
            };

            _channel!.BasicQos(0, 1, false);
            _channel.BasicConsume(
                queue: _options.QueueName,
                autoAck: false,
                consumer: consumer);

            await Task.Delay(Timeout.Infinite, stoppingToken);
        }

        private async Task HandleMessageAsync(BasicDeliverEventArgs eventArgs, CancellationToken cancellationToken)
        {
            try
            {
                string payload = Encoding.UTF8.GetString(eventArgs.Body.ToArray());
                ReportRequestedMessage? message = JsonSerializer.Deserialize<ReportRequestedMessage>(payload);

                if (message is null)
                {
                    _channel!.BasicNack(eventArgs.DeliveryTag, false, false);
                    return;
                }

                using IServiceScope scope = _serviceProvider.CreateScope();
                ReportRequestIngestionService ingestionService =
                    scope.ServiceProvider.GetRequiredService<ReportRequestIngestionService>();

                await ingestionService.EnqueueAsync(message, cancellationToken);

                _channel!.BasicAck(eventArgs.DeliveryTag, false);
            }
            catch (Exception exception)
            {
                _logger.LogError(exception, "Failed to process message from RabbitMQ.");
                _channel!.BasicNack(eventArgs.DeliveryTag, false, false);
            }
        }

        private void EnsureConnected()
        {
            ConnectionFactory factory = new()
            {
                HostName = _options.Host,
                Port = _options.Port,
                UserName = _options.Username,
                Password = _options.Password,
                VirtualHost = _options.VirtualHost,
                DispatchConsumersAsync = true
            };

            _connection = factory.CreateConnection();
            _channel = _connection.CreateModel();

            _channel.QueueDeclare(
                queue: _options.QueueName,
                durable: true,
                exclusive: false,
                autoDelete: false,
                arguments: null);
        }

        public override void Dispose()
        {
            _channel?.Dispose();
            _connection?.Dispose();
            base.Dispose();
        }
    }
}