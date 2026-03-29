using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ConversionService.Application.Contracts
{
    public sealed class ReportCalculationResult
    {
        public Guid RequestId { get; init; }

        public int ViewsCount { get; init; }

        public int PaymentsCount { get; init; }

        public decimal? Ratio { get; init; }
    }
}