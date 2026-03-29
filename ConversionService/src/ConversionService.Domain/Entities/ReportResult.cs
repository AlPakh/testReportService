using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ConversionService.Domain.Entities
{
    public sealed class ReportResult
    {
        private ReportResult()
        {
        }

        public ReportResult(
            Guid reportRequestId,
            int viewsCount,
            int paymentsCount,
            decimal? ratio,
            DateTime generatedAtUtc)
        {
            if (reportRequestId == Guid.Empty)
            {
                throw new ArgumentException("ReportRequestId must be specified.", nameof(reportRequestId));
            }

            if (viewsCount < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(viewsCount));
            }

            if (paymentsCount < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(paymentsCount));
            }

            ReportRequestId = reportRequestId;
            ViewsCount = viewsCount;
            PaymentsCount = paymentsCount;
            Ratio = ratio;
            GeneratedAtUtc = generatedAtUtc;
        }

        public Guid ReportRequestId { get; private set; }

        public int ViewsCount { get; private set; }

        public int PaymentsCount { get; private set; }

        public decimal? Ratio { get; private set; }

        public DateTime GeneratedAtUtc { get; private set; }
    }
}