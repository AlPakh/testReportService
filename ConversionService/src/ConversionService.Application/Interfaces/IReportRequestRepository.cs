using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using ConversionService.Domain.Entities;

namespace ConversionService.Application.Interfaces
{
    public interface IReportRequestRepository
    {
        Task<bool> ExistsByIdAsync(Guid requestId, CancellationToken cancellationToken = default);

        Task<Guid?> GetIdByExternalMessageIdAsync(string externalMessageId, CancellationToken cancellationToken = default);

        Task AddAsync(ReportRequest request, CancellationToken cancellationToken = default);

        Task<ReportRequest?> GetByIdAsync(Guid requestId, CancellationToken cancellationToken = default);

        Task<IReadOnlyCollection<ReportRequest>> GetPendingBatchAsync(int batchSize, CancellationToken cancellationToken = default);
    }
}