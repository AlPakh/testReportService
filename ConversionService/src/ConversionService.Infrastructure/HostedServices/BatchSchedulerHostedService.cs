using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using ConversionService.Application.Services;
using ConversionService.Infrastructure.Options;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace ConversionService.Infrastructure.HostedServices
{
    public sealed class BatchSchedulerHostedService : BackgroundService
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly BatchProcessingOptions _options;
        private readonly ILogger<BatchSchedulerHostedService> _logger;

        public BatchSchedulerHostedService(
            IServiceProvider serviceProvider,
            IOptions<BatchProcessingOptions> options,
            ILogger<BatchSchedulerHostedService> logger)
        {
            _serviceProvider = serviceProvider;
            _options = options.Value;
            _logger = logger;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            TimeSpan interval = TimeSpan.FromSeconds(Math.Max(1, _options.ScanIntervalSeconds));

            using PeriodicTimer timer = new(interval);

            while (await timer.WaitForNextTickAsync(stoppingToken))
            {
                try
                {
                    using IServiceScope scope = _serviceProvider.CreateScope();
                    ReportBatchProcessor processor = scope.ServiceProvider.GetRequiredService<ReportBatchProcessor>();

                    await processor.ProcessPendingAsync(_options.BatchSize, stoppingToken);
                }
                catch (OperationCanceledException)
                {
                    break;
                }
                catch (Exception exception)
                {
                    _logger.LogError(exception, "Scheduled batch processing failed.");
                }
            }
        }
    }
}