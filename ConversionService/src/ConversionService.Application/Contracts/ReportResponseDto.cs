using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ConversionService.Application.Contracts
{
    public sealed class ReportResponseDto
    {
        public Guid RequestId { get; init; }

        public string Status { get; init; } = string.Empty;

        public decimal? Ratio { get; init; }

        public int? PaymentsCount { get; init; }

        public string? ErrorMessage { get; init; }
    }
}