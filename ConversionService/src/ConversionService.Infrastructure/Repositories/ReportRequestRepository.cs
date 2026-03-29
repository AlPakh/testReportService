using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using ConversionService.Application.Interfaces;
using ConversionService.Domain.Entities;
using ConversionService.Domain.Enums;
using ConversionService.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace ConversionService.Infrastructure.Repositories
{
    public sealed class ReportRequestRepository : IReportRequestRepository
    {
        private readonly ConversionServiceDbContext _dbContext;

        public ReportRequestRepository(ConversionServiceDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public Task<bool> ExistsByIdAsync(Guid requestId, CancellationToken cancellationToken = default)
        {
            return _dbContext.ReportRequests.AnyAsync(x => x.Id == requestId, cancellationToken);
        }

        public async Task<Guid?> GetIdByExternalMessageIdAsync(
            string externalMessageId,
            CancellationToken cancellationToken = default)
        {
            return await _dbContext.ReportRequests
                .AsNoTracking()
                .Where(x => x.ExternalMessageId == externalMessageId)
                .Select(x => (Guid?)x.Id)
                .FirstOrDefaultAsync(cancellationToken);
        }

        public Task AddAsync(ReportRequest request, CancellationToken cancellationToken = default)
        {
            return _dbContext.ReportRequests.AddAsync(request, cancellationToken).AsTask();
        }

        public Task<ReportRequest?> GetByIdAsync(Guid requestId, CancellationToken cancellationToken = default)
        {
            return _dbContext.ReportRequests
                .AsNoTracking()
                .FirstOrDefaultAsync(x => x.Id == requestId, cancellationToken);
        }

        public async Task<IReadOnlyCollection<ReportRequest>> GetPendingBatchAsync(
            int batchSize,
            CancellationToken cancellationToken = default)
        {
            return await _dbContext.ReportRequests
                .Where(x => x.Status == ReportRequestStatus.Pending)
                .OrderBy(x => x.CreatedAtUtc)
                .Take(batchSize)
                .ToListAsync(cancellationToken);
        }
    }
}