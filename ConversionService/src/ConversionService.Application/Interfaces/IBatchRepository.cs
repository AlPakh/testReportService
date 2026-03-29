using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using ConversionService.Domain.Entities;

namespace ConversionService.Application.Interfaces
{
    public interface IBatchRepository
    {
        Task AddAsync(ProcessingBatch batch, CancellationToken cancellationToken = default);
    }
}