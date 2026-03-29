using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using ConversionService.Domain.Entities;

namespace ConversionService.Application.Interfaces
{
    public interface IReportResultRepository
    {
        Task AddAsync(ReportResult result, CancellationToken cancellationToken = default);

        Task<ReportResult?> GetByRequestIdAsync(Guid requestId, CancellationToken cancellationToken = default);
    }
}