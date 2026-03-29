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
    public sealed class ReportBatchProcessor
    {
        private readonly IReportRequestRepository _reportRequestRepository;
        private readonly IReportResultRepository _reportResultRepository;
        private readonly IBatchRepository _batchRepository;
        private readonly IReportProvider _reportProvider;
        private readonly IStatusCache _statusCache;
        private readonly IUnitOfWork _unitOfWork;
        private readonly IClock _clock;

        public ReportBatchProcessor(
            IReportRequestRepository reportRequestRepository,
            IReportResultRepository reportResultRepository,
            IBatchRepository batchRepository,
            IReportProvider reportProvider,
            IStatusCache statusCache,
            IUnitOfWork unitOfWork,
            IClock clock)
        {
            _reportRequestRepository = reportRequestRepository;
            _reportResultRepository = reportResultRepository;
            _batchRepository = batchRepository;
            _reportProvider = reportProvider;
            _statusCache = statusCache;
            _unitOfWork = unitOfWork;
            _clock = clock;
        }

        public async Task<int> ProcessPendingAsync(int batchSize, CancellationToken cancellationToken = default)
        {
            if (batchSize <= 0)
            {
                batchSize = 100;
            }

            IReadOnlyCollection<ReportRequest> requests = await _reportRequestRepository.GetPendingBatchAsync(
                batchSize,
                cancellationToken);

            if (requests.Count == 0)
            {
                return 0;
            }

            ProcessingBatch batch = new(Guid.NewGuid(), requests.Count, _clock.UtcNow);
            await _batchRepository.AddAsync(batch, cancellationToken);

            foreach (ReportRequest request in requests)
            {
                request.MarkProcessing(batch.Id, _clock.UtcNow);

                await _statusCache.SetAsync(
                    new ReportResponseDto
                    {
                        RequestId = request.Id,
                        Status = request.Status.ToString()
                    },
                    cancellationToken);
            }

            await _unitOfWork.SaveChangesAsync(cancellationToken);

            try
            {
                IReadOnlyCollection<ReportCalculationResult> results = await _reportProvider.BuildReportsAsync(
                    requests,
                    cancellationToken);

                Dictionary<Guid, ReportCalculationResult> resultMap = results.ToDictionary(x => x.RequestId, x => x);

                foreach (ReportRequest request in requests)
                {
                    if (resultMap.TryGetValue(request.Id, out ReportCalculationResult? result))
                    {
                        ReportResult reportResult = new(
                            request.Id,
                            result.ViewsCount,
                            result.PaymentsCount,
                            result.Ratio,
                            _clock.UtcNow);

                        await _reportResultRepository.AddAsync(reportResult, cancellationToken);

                        request.MarkCompleted(_clock.UtcNow);

                        await _statusCache.SetAsync(
                            new ReportResponseDto
                            {
                                RequestId = request.Id,
                                Status = request.Status.ToString(),
                                Ratio = result.Ratio,
                                PaymentsCount = result.PaymentsCount
                            },
                            cancellationToken);
                    }
                    else
                    {
                        request.MarkFailed("No result returned for the request.", _clock.UtcNow);

                        await _statusCache.SetAsync(
                            new ReportResponseDto
                            {
                                RequestId = request.Id,
                                Status = request.Status.ToString(),
                                ErrorMessage = request.ErrorMessage
                            },
                            cancellationToken);
                    }
                }

                batch.MarkCompleted(_clock.UtcNow);
                await _unitOfWork.SaveChangesAsync(cancellationToken);

                return requests.Count;
            }
            catch (Exception exception)
            {
                foreach (ReportRequest request in requests)
                {
                    request.MarkFailed(exception.Message, _clock.UtcNow);

                    await _statusCache.SetAsync(
                        new ReportResponseDto
                        {
                            RequestId = request.Id,
                            Status = request.Status.ToString(),
                            ErrorMessage = request.ErrorMessage
                        },
                        cancellationToken);
                }

                batch.MarkFailed(_clock.UtcNow);
                await _unitOfWork.SaveChangesAsync(cancellationToken);

                throw;
            }
        }
    }
}