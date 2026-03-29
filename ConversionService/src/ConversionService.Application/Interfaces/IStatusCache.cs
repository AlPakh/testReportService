using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using ConversionService.Application.Contracts;

namespace ConversionService.Application.Interfaces
{
    public interface IStatusCache
    {
        Task<ReportResponseDto?> GetAsync(Guid requestId, CancellationToken cancellationToken = default);

        Task SetAsync(ReportResponseDto response, CancellationToken cancellationToken = default);
    }
}