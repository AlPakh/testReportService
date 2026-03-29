using ConversionService.Application.Contracts;
using ConversionService.Application.Interfaces;
using ConversionService.Application.Services;
using ConversionService.Domain.Entities;
using ConversionService.Domain.Enums;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace ConversionService.UnitTests.Application.Services
{
    [TestClass]
    public class ReportBatchProcessorTests
    {
        [TestMethod]
        public async Task ProcessPendingAsync_WhenNoPendingRequests_ReturnsZero()
        {
            FakeReportRequestRepository requestRepository = new();
            FakeReportResultRepository resultRepository = new();
            FakeBatchRepository batchRepository = new();
            FakeReportProvider reportProvider = new();
            FakeStatusCache statusCache = new();
            FakeUnitOfWork unitOfWork = new();
            FakeClock clock = new(DateTime.UtcNow);

            ReportBatchProcessor service = new(
                requestRepository,
                resultRepository,
                batchRepository,
                reportProvider,
                statusCache,
                unitOfWork,
                clock);

            int processedCount = await service.ProcessPendingAsync(100);

            Assert.AreEqual(0, processedCount);
            Assert.AreEqual(0, batchRepository.Batches.Count);
            Assert.AreEqual(0, resultRepository.Results.Count);
            Assert.AreEqual(0, reportProvider.CallCount);
            Assert.AreEqual(0, unitOfWork.SaveChangesCalls);
        }

        [TestMethod]
        public async Task ProcessPendingAsync_WhenProviderReturnsResults_CompletesRequestsAndStoresResults()
        {
            DateTime fixedUtcNow = new(2026, 3, 30, 0, 23, 36, DateTimeKind.Utc);

            ReportRequest request = new(
                Guid.NewGuid(),
                "msg-001",
                "PROD-001",
                "CHK-STD",
                new DateOnly(2026, 3, 1),
                new DateOnly(2026, 3, 3),
                fixedUtcNow);

            FakeReportRequestRepository requestRepository = new();
            requestRepository.PendingRequests.Add(request);

            FakeReportResultRepository resultRepository = new();
            FakeBatchRepository batchRepository = new();
            FakeReportProvider reportProvider = new();
            reportProvider.ResultsToReturn.Add(new ReportCalculationResult
            {
                RequestId = request.Id,
                ViewsCount = 300,
                PaymentsCount = 15,
                Ratio = 20m
            });

            FakeStatusCache statusCache = new();
            FakeUnitOfWork unitOfWork = new();
            FakeClock clock = new(fixedUtcNow);

            ReportBatchProcessor service = new(
                requestRepository,
                resultRepository,
                batchRepository,
                reportProvider,
                statusCache,
                unitOfWork,
                clock);

            int processedCount = await service.ProcessPendingAsync(100);

            Assert.AreEqual(1, processedCount);
            Assert.AreEqual(Domain.Enums.ReportRequestStatus.Completed, request.Status);

            Assert.AreEqual(1, batchRepository.Batches.Count);
            Assert.AreEqual(Domain.Enums.ProcessingBatchStatus.Completed, batchRepository.Batches[0].Status);

            Assert.AreEqual(1, resultRepository.Results.Count);
            Assert.AreEqual(300, resultRepository.Results[0].ViewsCount);
            Assert.AreEqual(15, resultRepository.Results[0].PaymentsCount);
            Assert.AreEqual(20m, resultRepository.Results[0].Ratio);

            Assert.AreEqual(2, unitOfWork.SaveChangesCalls);

            Assert.AreEqual(1, statusCache.StoredResponses.Count);
            Assert.IsTrue(statusCache.StoredResponses.ContainsKey(request.Id));
            Assert.AreEqual("Completed", statusCache.StoredResponses[request.Id].Status);
        }

        [TestMethod]
        public async Task ProcessPendingAsync_WhenProviderDoesNotReturnResult_MarksRequestAsFailed()
        {
            DateTime fixedUtcNow = new(2026, 3, 30, 0, 23, 36, DateTimeKind.Utc);

            ReportRequest request = new(
                Guid.NewGuid(),
                "msg-001",
                "PROD-001",
                "CHK-STD",
                new DateOnly(2026, 3, 1),
                new DateOnly(2026, 3, 3),
                fixedUtcNow);

            FakeReportRequestRepository requestRepository = new();
            requestRepository.PendingRequests.Add(request);

            FakeReportResultRepository resultRepository = new();
            FakeBatchRepository batchRepository = new();
            FakeReportProvider reportProvider = new();
            FakeStatusCache statusCache = new();
            FakeUnitOfWork unitOfWork = new();
            FakeClock clock = new(fixedUtcNow);

            ReportBatchProcessor service = new(
                requestRepository,
                resultRepository,
                batchRepository,
                reportProvider,
                statusCache,
                unitOfWork,
                clock);

            int processedCount = await service.ProcessPendingAsync(100);

            Assert.AreEqual(1, processedCount);
            Assert.AreEqual(ReportRequestStatus.Failed, request.Status);
            Assert.AreEqual("No result returned for the request.", request.ErrorMessage);
            Assert.AreEqual(0, resultRepository.Results.Count);
            Assert.AreEqual(ProcessingBatchStatus.Completed, batchRepository.Batches[0].Status);
            Assert.AreEqual(2, unitOfWork.SaveChangesCalls);
        }

        [TestMethod]
        public async Task ProcessPendingAsync_WhenProviderThrows_MarksRequestsAsFailedAndRethrows()
        {
            DateTime fixedUtcNow = new(2026, 3, 30, 0, 23, 36, DateTimeKind.Utc);

            ReportRequest request = new(
                Guid.NewGuid(),
                "msg-001",
                "PROD-001",
                "CHK-STD",
                new DateOnly(2026, 3, 1),
                new DateOnly(2026, 3, 3),
                fixedUtcNow);

            FakeReportRequestRepository requestRepository = new();
            requestRepository.PendingRequests.Add(request);

            FakeReportResultRepository resultRepository = new();
            FakeBatchRepository batchRepository = new();
            FakeReportProvider reportProvider = new()
            {
                ExceptionToThrow = new InvalidOperationException("Provider failure.")
            };

            FakeStatusCache statusCache = new();
            FakeUnitOfWork unitOfWork = new();
            FakeClock clock = new(fixedUtcNow);

            ReportBatchProcessor service = new(
                requestRepository,
                resultRepository,
                batchRepository,
                reportProvider,
                statusCache,
                unitOfWork,
                clock);

            await Assert.ThrowsExceptionAsync<InvalidOperationException>(() => service.ProcessPendingAsync(100));

            Assert.AreEqual(ReportRequestStatus.Failed, request.Status);
            Assert.AreEqual("Provider failure.", request.ErrorMessage);
            Assert.AreEqual(0, resultRepository.Results.Count);
            Assert.AreEqual(1, batchRepository.Batches.Count);
            Assert.AreEqual(ProcessingBatchStatus.Failed, batchRepository.Batches[0].Status);
            Assert.AreEqual(2, unitOfWork.SaveChangesCalls);
        }

        private sealed class FakeReportRequestRepository : IReportRequestRepository
        {
            public List<ReportRequest> PendingRequests { get; } = new();

            public Task<bool> ExistsByIdAsync(Guid requestId, CancellationToken cancellationToken = default)
            {
                return Task.FromResult(PendingRequests.Any(x => x.Id == requestId));
            }

            public Task<Guid?> GetIdByExternalMessageIdAsync(string externalMessageId, CancellationToken cancellationToken = default)
            {
                Guid? requestId = PendingRequests
                    .Where(x => x.ExternalMessageId == externalMessageId)
                    .Select(x => (Guid?)x.Id)
                    .FirstOrDefault();

                return Task.FromResult(requestId);
            }

            public Task AddAsync(ReportRequest request, CancellationToken cancellationToken = default)
            {
                PendingRequests.Add(request);
                return Task.CompletedTask;
            }

            public Task<ReportRequest?> GetByIdAsync(Guid requestId, CancellationToken cancellationToken = default)
            {
                ReportRequest? request = PendingRequests.FirstOrDefault(x => x.Id == requestId);
                return Task.FromResult(request);
            }

            public Task<IReadOnlyCollection<ReportRequest>> GetPendingBatchAsync(int batchSize, CancellationToken cancellationToken = default)
            {
                IReadOnlyCollection<ReportRequest> batch = PendingRequests.Take(batchSize).ToList();
                return Task.FromResult(batch);
            }
        }

        private sealed class FakeReportResultRepository : IReportResultRepository
        {
            public List<ReportResult> Results { get; } = new();

            public Task AddAsync(ReportResult result, CancellationToken cancellationToken = default)
            {
                Results.Add(result);
                return Task.CompletedTask;
            }

            public Task<ReportResult?> GetByRequestIdAsync(Guid requestId, CancellationToken cancellationToken = default)
            {
                ReportResult? result = Results.FirstOrDefault(x => x.ReportRequestId == requestId);
                return Task.FromResult(result);
            }
        }

        private sealed class FakeBatchRepository : IBatchRepository
        {
            public List<ProcessingBatch> Batches { get; } = new();

            public Task AddAsync(ProcessingBatch batch, CancellationToken cancellationToken = default)
            {
                Batches.Add(batch);
                return Task.CompletedTask;
            }
        }

        private sealed class FakeReportProvider : IReportProvider
        {
            public List<ReportCalculationResult> ResultsToReturn { get; } = new();

            public Exception? ExceptionToThrow { get; set; }

            public int CallCount { get; private set; }

            public Task<IReadOnlyCollection<ReportCalculationResult>> BuildReportsAsync(
                IReadOnlyCollection<ReportRequest> requests,
                CancellationToken cancellationToken = default)
            {
                CallCount++;

                if (ExceptionToThrow is not null)
                {
                    throw ExceptionToThrow;
                }

                return Task.FromResult<IReadOnlyCollection<ReportCalculationResult>>(ResultsToReturn);
            }
        }

        private sealed class FakeStatusCache : IStatusCache
        {
            public Dictionary<Guid, ReportResponseDto> StoredResponses { get; } = new();

            public int SetCalls { get; private set; }

            public Task<ReportResponseDto?> GetAsync(Guid requestId, CancellationToken cancellationToken = default)
            {
                StoredResponses.TryGetValue(requestId, out ReportResponseDto? response);
                return Task.FromResult(response);
            }

            public Task SetAsync(ReportResponseDto response, CancellationToken cancellationToken = default)
            {
                SetCalls++;
                StoredResponses[response.RequestId] = response;
                return Task.CompletedTask;
            }
        }

        private sealed class FakeUnitOfWork : IUnitOfWork
        {
            public int SaveChangesCalls { get; private set; }

            public Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
            {
                SaveChangesCalls++;
                return Task.FromResult(1);
            }
        }

        private sealed class FakeClock : IClock
        {
            public FakeClock(DateTime utcNow)
            {
                UtcNow = utcNow;
            }

            public DateTime UtcNow { get; }
        }
    }
}