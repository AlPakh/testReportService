using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Text.Json;

using ConversionService.Application.Contracts;
using ConversionService.Application.Interfaces;
using Microsoft.Extensions.Options;
using RabbitMQ.Client;

namespace ConversionService.Infrastructure.Messaging
{
    public sealed class RabbitMqPublisher : IReportRequestMessagePublisher
    {
        private readonly RabbitMqOptions _options;

        public RabbitMqPublisher(IOptions<RabbitMqOptions> options)
        {
            _options = options.Value;
        }

        public Task PublishAsync(ReportRequestedMessage message, CancellationToken cancellationToken = default)
        {
            ConnectionFactory factory = new()
            {
                HostName = _options.Host,
                Port = _options.Port,
                UserName = _options.Username,
                Password = _options.Password,
                VirtualHost = _options.VirtualHost
            };

            using IConnection connection = factory.CreateConnection();
            using IModel channel = connection.CreateModel();

            channel.QueueDeclare(
                queue: _options.QueueName,
                durable: true,
                exclusive: false,
                autoDelete: false,
                arguments: null);

            byte[] body = Encoding.UTF8.GetBytes(JsonSerializer.Serialize(message));
            IBasicProperties properties = channel.CreateBasicProperties();
            properties.Persistent = true;

            channel.BasicPublish(
                exchange: string.Empty,
                routingKey: _options.QueueName,
                basicProperties: properties,
                body: body);

            return Task.CompletedTask;
        }
    }
}