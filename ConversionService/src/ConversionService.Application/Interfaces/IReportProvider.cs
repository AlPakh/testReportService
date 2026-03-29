using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using ConversionService.Application.Contracts;
using ConversionService.Domain.Entities;

namespace ConversionService.Application.Interfaces
{
    public interface IReportProvider
    {
        Task<IReadOnlyCollection<ReportCalculationResult>> BuildReportsAsync(
            IReadOnlyCollection<ReportRequest> requests,
            CancellationToken cancellationToken = default);
    }
}