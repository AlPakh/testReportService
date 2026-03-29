using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using ConversionService.Domain.Enums;

namespace ConversionService.Domain.Entities
{
    public sealed class ReportRequest
    {
        private ReportRequest()
        {
        }

        public ReportRequest(
            Guid id,
            string? externalMessageId,
            string productId,
            string checkoutId,
            DateOnly periodFrom,
            DateOnly periodTo,
            DateTime createdAtUtc)
        {
            if (id == Guid.Empty)
            {
                throw new ArgumentException("Request id must be specified.", nameof(id));
            }

            if (string.IsNullOrWhiteSpace(productId))
            {
                throw new ArgumentException("ProductId must be specified.", nameof(productId));
            }

            if (string.IsNullOrWhiteSpace(checkoutId))
            {
                throw new ArgumentException("CheckoutId must be specified.", nameof(checkoutId));
            }

            if (periodFrom > periodTo)
            {
                throw new ArgumentException("PeriodFrom must be less than or equal to PeriodTo.");
            }

            Id = id;
            ExternalMessageId = externalMessageId;
            ProductId = productId;
            CheckoutId = checkoutId;
            PeriodFrom = periodFrom;
            PeriodTo = periodTo;
            Status = ReportRequestStatus.Pending;
            CreatedAtUtc = createdAtUtc;
            UpdatedAtUtc = createdAtUtc;
        }

        public Guid Id { get; private set; }

        public string? ExternalMessageId { get; private set; }

        public string ProductId { get; private set; } = string.Empty;

        public string CheckoutId { get; private set; } = string.Empty;

        public DateOnly PeriodFrom { get; private set; }

        public DateOnly PeriodTo { get; private set; }

        public ReportRequestStatus Status { get; private set; }

        public Guid? BatchId { get; private set; }

        public string? ErrorMessage { get; private set; }

        public DateTime CreatedAtUtc { get; private set; }

        public DateTime UpdatedAtUtc { get; private set; }

        public void MarkProcessing(Guid batchId, DateTime utcNow)
        {
            BatchId = batchId;
            Status = ReportRequestStatus.Processing;
            ErrorMessage = null;
            UpdatedAtUtc = utcNow;
        }

        public void MarkCompleted(DateTime utcNow)
        {
            Status = ReportRequestStatus.Completed;
            ErrorMessage = null;
            UpdatedAtUtc = utcNow;
        }

        public void MarkFailed(string errorMessage, DateTime utcNow)
        {
            Status = ReportRequestStatus.Failed;
            ErrorMessage = errorMessage;
            UpdatedAtUtc = utcNow;
        }
    }
}