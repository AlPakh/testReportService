using ConversionService.Application.Contracts;
using ConversionService.Application.Interfaces;
using ConversionService.Application.Services;
using ConversionService.Domain.Entities;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace ConversionService.UnitTests.Application.Services
{
    [TestClass]
    public class ReportQueryServiceTests
    {
        [TestMethod]
        public async Task GetByRequestIdAsync_WhenCachedValueExists_ReturnsCachedValueWithoutQueryingRepositories()
        {
            Guid requestId = Guid.NewGuid();

            FakeStatusCache statusCache = new();
            statusCache.StoredResponses[requestId] = new ReportResponseDto
            {
                RequestId = requestId,
                Status = "Completed",
                Ratio = 20m,
                PaymentsCount = 15
            };

            FakeReportRequestRepository requestRepository = new();
            FakeReportResultRepository resultRepository = new();

            ReportQueryService service = new(statusCache, requestRepository, resultRepository);

            ReportResponseDto? response = await service.GetByRequestIdAsync(requestId);

            Assert.IsNotNull(response);
            Assert.AreEqual("Completed", response.Status);
            Assert.AreEqual(20m, response.Ratio);
            Assert.AreEqual(15, response.PaymentsCount);
            Assert.AreEqual(0, requestRepository.GetByIdCalls);
            Assert.AreEqual(0, resultRepository.GetByRequestIdCalls);
        }

        [TestMethod]
        public async Task GetByRequestIdAsync_WhenRequestDoesNotExist_ReturnsNull()
        {
            FakeStatusCache statusCache = new();
            FakeReportRequestRepository requestRepository = new();
            FakeReportResultRepository resultRepository = new();

            ReportQueryService service = new(statusCache, requestRepository, resultRepository);

            ReportResponseDto? response = await service.GetByRequestIdAsync(Guid.NewGuid());

            Assert.IsNull(response);
            Assert.AreEqual(1, requestRepository.GetByIdCalls);
            Assert.AreEqual(0, resultRepository.GetByRequestIdCalls);
        }

        [TestMethod]
        public async Task GetByRequestIdAsync_WhenRequestCompleted_ReturnsCompletedResponseAndCachesIt()
        {
            DateTime fixedUtcNow = new(2026, 3, 30, 0, 23, 36, DateTimeKind.Utc);
            Guid requestId = Guid.NewGuid();

            ReportRequest request = new(
                requestId,
                "msg-001",
                "PROD-001",
                "CHK-STD",
                new DateOnly(2026, 3, 1),
                new DateOnly(2026, 3, 3),
                fixedUtcNow);

            request.MarkCompleted(fixedUtcNow);

            ReportResult result = new(
                requestId,
                300,
                15,
                20m,
                fixedUtcNow);

            FakeStatusCache statusCache = new();
            FakeReportRequestRepository requestRepository = new();
            requestRepository.Requests[requestId] = request;

            FakeReportResultRepository resultRepository = new();
            resultRepository.Results[requestId] = result;

            ReportQueryService service = new(statusCache, requestRepository, resultRepository);

            ReportResponseDto? response = await service.GetByRequestIdAsync(requestId);

            Assert.IsNotNull(response);
            Assert.AreEqual("Completed", response.Status);
            Assert.AreEqual(20m, response.Ratio);
            Assert.AreEqual(15, response.PaymentsCount);

            Assert.AreEqual(1, statusCache.SetCalls);
            Assert.IsTrue(statusCache.StoredResponses.ContainsKey(requestId));
        }

        [TestMethod]
        public async Task GetByRequestIdAsync_WhenRequestFailed_ReturnsFailedResponse()
        {
            DateTime fixedUtcNow = new(2026, 3, 30, 0, 23, 36, DateTimeKind.Utc);
            Guid requestId = Guid.NewGuid();

            ReportRequest request = new(
                requestId,
                "msg-001",
                "PROD-001",
                "CHK-STD",
                new DateOnly(2026, 3, 1),
                new DateOnly(2026, 3, 3),
                fixedUtcNow);

            request.MarkFailed("Validation error.", fixedUtcNow);

            FakeStatusCache statusCache = new();
            FakeReportRequestRepository requestRepository = new();
            requestRepository.Requests[requestId] = request;

            FakeReportResultRepository resultRepository = new();

            ReportQueryService service = new(statusCache, requestRepository, resultRepository);

            ReportResponseDto? response = await service.GetByRequestIdAsync(requestId);

            Assert.IsNotNull(response);
            Assert.AreEqual("Failed", response.Status);
            Assert.AreEqual("Validation error.", response.ErrorMessage);
            Assert.IsNull(response.Ratio);
            Assert.IsNull(response.PaymentsCount);
            Assert.AreEqual(0, resultRepository.GetByRequestIdCalls);
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

        private sealed class FakeReportRequestRepository : IReportRequestRepository
        {
            public Dictionary<Guid, ReportRequest> Requests { get; } = new();

            public int GetByIdCalls { get; private set; }

            public Task<bool> ExistsByIdAsync(Guid requestId, CancellationToken cancellationToken = default)
            {
                return Task.FromResult(Requests.ContainsKey(requestId));
            }

            public Task<Guid?> GetIdByExternalMessageIdAsync(string externalMessageId, CancellationToken cancellationToken = default)
            {
                Guid? requestId = Requests.Values
                    .Where(x => x.ExternalMessageId == externalMessageId)
                    .Select(x => (Guid?)x.Id)
                    .FirstOrDefault();

                return Task.FromResult(requestId);
            }

            public Task AddAsync(ReportRequest request, CancellationToken cancellationToken = default)
            {
                Requests[request.Id] = request;
                return Task.CompletedTask;
            }

            public Task<ReportRequest?> GetByIdAsync(Guid requestId, CancellationToken cancellationToken = default)
            {
                GetByIdCalls++;
                Requests.TryGetValue(requestId, out ReportRequest? request);
                return Task.FromResult(request);
            }

            public Task<IReadOnlyCollection<ReportRequest>> GetPendingBatchAsync(int batchSize, CancellationToken cancellationToken = default)
            {
                IReadOnlyCollection<ReportRequest> pending = Requests.Values
                    .Where(x => x.Status == Domain.Enums.ReportRequestStatus.Pending)
                    .Take(batchSize)
                    .ToList();

                return Task.FromResult(pending);
            }
        }

        private sealed class FakeReportResultRepository : IReportResultRepository
        {
            public Dictionary<Guid, ReportResult> Results { get; } = new();

            public int GetByRequestIdCalls { get; private set; }

            public Task AddAsync(ReportResult result, CancellationToken cancellationToken = default)
            {
                Results[result.ReportRequestId] = result;
                return Task.CompletedTask;
            }

            public Task<ReportResult?> GetByRequestIdAsync(Guid requestId, CancellationToken cancellationToken = default)
            {
                GetByRequestIdCalls++;
                Results.TryGetValue(requestId, out ReportResult? result);
                return Task.FromResult(result);
            }
        }
    }
}