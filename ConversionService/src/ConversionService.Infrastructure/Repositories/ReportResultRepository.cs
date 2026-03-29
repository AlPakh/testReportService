using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using ConversionService.Application.Interfaces;
using ConversionService.Domain.Entities;
using ConversionService.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace ConversionService.Infrastructure.Repositories
{
    public sealed class ReportResultRepository : IReportResultRepository
    {
        private readonly ConversionServiceDbContext _dbContext;

        public ReportResultRepository(ConversionServiceDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public Task AddAsync(ReportResult result, CancellationToken cancellationToken = default)
        {
            return _dbContext.ReportResults.AddAsync(result, cancellationToken).AsTask();
        }

        public Task<ReportResult?> GetByRequestIdAsync(Guid requestId, CancellationToken cancellationToken = default)
        {
            return _dbContext.ReportResults
                .AsNoTracking()
                .FirstOrDefaultAsync(x => x.ReportRequestId == requestId, cancellationToken);
        }
    }
}