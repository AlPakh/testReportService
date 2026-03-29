using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ConversionService.Application.Contracts
{
    public sealed class ReportRequestedMessage
    {
        public Guid RequestId { get; set; }

        public string? ExternalMessageId { get; set; }

        public string ProductId { get; set; } = string.Empty;

        public string CheckoutId { get; set; } = string.Empty;

        public DateOnly PeriodFrom { get; set; }

        public DateOnly PeriodTo { get; set; }
    }
}