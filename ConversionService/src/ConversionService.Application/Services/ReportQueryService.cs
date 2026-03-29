using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using ConversionService.Application.Contracts;
using ConversionService.Application.Interfaces;
using ConversionService.Domain.Entities;

namespace ConversionService.Application.Services
{
    public sealed class ReportQueryService
    {
        private readonly IStatusCache _statusCache;
        private readonly IReportRequestRepository _reportRequestRepository;
        private readonly IReportResultRepository _reportResultRepository;

        public ReportQueryService(
            IStatusCache statusCache,
            IReportRequestRepository reportRequestRepository,
            IReportResultRepository reportResultRepository)
        {
            _statusCache = statusCache;
            _reportRequestRepository = reportRequestRepository;
            _reportResultRepository = reportResultRepository;
        }

        public async Task<ReportResponseDto?> GetByRequestIdAsync(Guid requestId, CancellationToken cancellationToken = default)
        {
            ReportResponseDto? cached = await _statusCache.GetAsync(requestId, cancellationToken);

            if (cached is not null)
            {
                return cached;
            }

            ReportRequest? request = await _reportRequestRepository.GetByIdAsync(requestId, cancellationToken);

            if (request is null)
            {
                return null;
            }

            ReportResponseDto response;

            if (request.Status == Domain.Enums.ReportRequestStatus.Completed)
            {
                ReportResult? result = await _reportResultRepository.GetByRequestIdAsync(requestId, cancellationToken);

                response = new ReportResponseDto
                {
                    RequestId = request.Id,
                    Status = request.Status.ToString(),
                    Ratio = result?.Ratio,
                    PaymentsCount = result?.PaymentsCount
                };
            }
            else
            {
                response = new ReportResponseDto
                {
                    RequestId = request.Id,
                    Status = request.Status.ToString(),
                    ErrorMessage = request.ErrorMessage
                };
            }

            await _statusCache.SetAsync(response, cancellationToken);

            return response;
        }
    }
}