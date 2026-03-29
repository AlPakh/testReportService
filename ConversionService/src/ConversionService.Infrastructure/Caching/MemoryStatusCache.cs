using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using ConversionService.Application.Contracts;
using ConversionService.Application.Interfaces;
using Microsoft.Extensions.Caching.Memory;

namespace ConversionService.Infrastructure.Caching
{
    public sealed class MemoryStatusCache : IStatusCache
    {
        private static readonly TimeSpan CacheLifetime = TimeSpan.FromHours(24);

        private readonly IMemoryCache _memoryCache;

        public MemoryStatusCache(IMemoryCache memoryCache)
        {
            _memoryCache = memoryCache;
        }

        public Task<ReportResponseDto?> GetAsync(Guid requestId, CancellationToken cancellationToken = default)
        {
            _memoryCache.TryGetValue(GetKey(requestId), out ReportResponseDto? response);
            return Task.FromResult(response);
        }

        public Task SetAsync(ReportResponseDto response, CancellationToken cancellationToken = default)
        {
            MemoryCacheEntryOptions options = new()
            {
                AbsoluteExpirationRelativeToNow = CacheLifetime
            };

            _memoryCache.Set(GetKey(response.RequestId), response, options);
            return Task.CompletedTask;
        }

        private static string GetKey(Guid requestId)
        {
            return $"report-status:{requestId}";
        }
    }
}