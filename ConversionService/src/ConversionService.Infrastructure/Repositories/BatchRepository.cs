using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using ConversionService.Application.Interfaces;
using ConversionService.Domain.Entities;
using ConversionService.Infrastructure.Persistence;

namespace ConversionService.Infrastructure.Repositories
{
    public sealed class BatchRepository : IBatchRepository
    {
        private readonly ConversionServiceDbContext _dbContext;

        public BatchRepository(ConversionServiceDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public Task AddAsync(ProcessingBatch batch, CancellationToken cancellationToken = default)
        {
            return _dbContext.ProcessingBatches.AddAsync(batch, cancellationToken).AsTask();
        }
    }
}