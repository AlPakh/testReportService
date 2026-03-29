using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using ConversionService.Application.Interfaces;
using ConversionService.Application.Services;
using ConversionService.Infrastructure.Caching;
using ConversionService.Infrastructure.HostedServices;
using ConversionService.Infrastructure.Messaging;
using ConversionService.Infrastructure.Options;
using ConversionService.Infrastructure.Persistence;
using ConversionService.Infrastructure.Providers;
using ConversionService.Infrastructure.Repositories;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace ConversionService.Infrastructure.DependencyInjection
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddInfrastructure(
            this IServiceCollection services,
            IConfiguration configuration)
        {
            string? connectionString = configuration.GetConnectionString("Postgres");

            if (string.IsNullOrWhiteSpace(connectionString))
            {
                throw new InvalidOperationException("Connection string 'Postgres' is not configured.");
            }

            services.Configure<RabbitMqOptions>(configuration.GetSection(RabbitMqOptions.SectionName));
            services.Configure<BatchProcessingOptions>(configuration.GetSection(BatchProcessingOptions.SectionName));

            services.AddDbContext<ConversionServiceDbContext>(options =>
                options.UseNpgsql(connectionString));

            services.AddMemoryCache();

            services.AddScoped<IReportRequestRepository, ReportRequestRepository>();
            services.AddScoped<IReportResultRepository, ReportResultRepository>();
            services.AddScoped<IBatchRepository, BatchRepository>();
            services.AddScoped<IProductRepository, ProductRepository>();
            services.AddScoped<ICheckoutRepository, CheckoutRepository>();
            services.AddScoped<IUnitOfWork, EfUnitOfWork>();

            services.AddSingleton<IClock, SystemClock>();
            services.AddSingleton<IStatusCache, MemoryStatusCache>();
            services.AddScoped<ConversionCalculator>();
            services.AddScoped<IReportProvider, DatabaseBackedReportProvider>();
            services.AddSingleton<IReportRequestMessagePublisher, RabbitMqPublisher>();

            services.AddScoped<ReportRequestIngestionService>();
            services.AddScoped<ReportBatchProcessor>();
            services.AddScoped<ReportQueryService>();

            services.AddHostedService<RabbitMqConsumerHostedService>();
            services.AddHostedService<BatchSchedulerHostedService>();

            return services;
        }
    }
}