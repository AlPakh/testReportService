using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Emit;
using System.Text;
using System.Threading.Tasks;

using ConversionService.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace ConversionService.Infrastructure.Persistence
{
    public sealed class ConversionServiceDbContext : DbContext
    {
        public ConversionServiceDbContext(DbContextOptions<ConversionServiceDbContext> options)
            : base(options)
        {
        }

        public DbSet<Product> Products => Set<Product>();

        public DbSet<Checkout> Checkouts => Set<Checkout>();

        public DbSet<ProductCheckout> ProductCheckouts => Set<ProductCheckout>();

        public DbSet<ConversionFact> ConversionFacts => Set<ConversionFact>();

        public DbSet<ReportRequest> ReportRequests => Set<ReportRequest>();

        public DbSet<ReportResult> ReportResults => Set<ReportResult>();

        public DbSet<ProcessingBatch> ProcessingBatches => Set<ProcessingBatch>();

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Product>(builder =>
            {
                builder.ToTable("products");
                builder.HasKey(x => x.Id);
                builder.Property(x => x.Id).HasColumnName("id");
                builder.Property(x => x.Name).HasColumnName("name");
                builder.Property(x => x.IsActive).HasColumnName("is_active");
            });

            modelBuilder.Entity<Checkout>(builder =>
            {
                builder.ToTable("checkouts");
                builder.HasKey(x => x.Id);
                builder.Property(x => x.Id).HasColumnName("id");
                builder.Property(x => x.Name).HasColumnName("name");
                builder.Property(x => x.IsActive).HasColumnName("is_active");
            });

            modelBuilder.Entity<ProductCheckout>(builder =>
            {
                builder.ToTable("product_checkouts");
                builder.HasKey(x => new { x.ProductId, x.CheckoutId });
                builder.Property(x => x.ProductId).HasColumnName("product_id");
                builder.Property(x => x.CheckoutId).HasColumnName("checkout_id");
            });

            modelBuilder.Entity<ConversionFact>(builder =>
            {
                builder.ToTable("conversion_facts");
                builder.HasKey(x => new { x.FactDate, x.ProductId, x.CheckoutId });
                builder.Property(x => x.FactDate).HasColumnName("fact_date");
                builder.Property(x => x.ProductId).HasColumnName("product_id");
                builder.Property(x => x.CheckoutId).HasColumnName("checkout_id");
                builder.Property(x => x.ViewsCount).HasColumnName("views_count");
                builder.Property(x => x.PaymentsCount).HasColumnName("payments_count");
            });

            modelBuilder.Entity<ReportRequest>(builder =>
            {
                builder.ToTable("report_requests");
                builder.HasKey(x => x.Id);

                builder.Property(x => x.Id).HasColumnName("id");
                builder.Property(x => x.ExternalMessageId).HasColumnName("external_message_id");
                builder.Property(x => x.ProductId).HasColumnName("product_id");
                builder.Property(x => x.CheckoutId).HasColumnName("checkout_id");
                builder.Property(x => x.PeriodFrom).HasColumnName("period_from");
                builder.Property(x => x.PeriodTo).HasColumnName("period_to");
                builder.Property(x => x.Status).HasColumnName("status").HasConversion<short>();
                builder.Property(x => x.BatchId).HasColumnName("batch_id");
                builder.Property(x => x.ErrorMessage).HasColumnName("error_message");
                builder.Property(x => x.CreatedAtUtc).HasColumnName("created_at_utc");
                builder.Property(x => x.UpdatedAtUtc).HasColumnName("updated_at_utc");

                builder.HasIndex(x => x.ExternalMessageId).IsUnique();
                builder.HasIndex(x => x.Status).HasDatabaseName("ix_report_requests_status");
                builder.HasIndex(x => x.CreatedAtUtc).HasDatabaseName("ix_report_requests_created_at_utc");
            });

            modelBuilder.Entity<ReportResult>(builder =>
            {
                builder.ToTable("report_results");
                builder.HasKey(x => x.ReportRequestId);
                builder.Property(x => x.ReportRequestId).HasColumnName("report_request_id");
                builder.Property(x => x.ViewsCount).HasColumnName("views_count");
                builder.Property(x => x.PaymentsCount).HasColumnName("payments_count");
                builder.Property(x => x.Ratio).HasColumnName("ratio");
                builder.Property(x => x.GeneratedAtUtc).HasColumnName("generated_at_utc");
            });

            modelBuilder.Entity<ProcessingBatch>(builder =>
            {
                builder.ToTable("processing_batches");
                builder.HasKey(x => x.Id);
                builder.Property(x => x.Id).HasColumnName("id");
                builder.Property(x => x.Status).HasColumnName("status").HasConversion<short>();
                builder.Property(x => x.ItemsCount).HasColumnName("items_count");
                builder.Property(x => x.StartedAtUtc).HasColumnName("started_at_utc");
                builder.Property(x => x.CompletedAtUtc).HasColumnName("completed_at_utc");
            });
        }
    }
}