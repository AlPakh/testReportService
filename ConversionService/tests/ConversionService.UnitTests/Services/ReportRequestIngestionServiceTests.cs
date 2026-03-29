using ConversionService.Application.Contracts;
using ConversionService.Application.Interfaces;
using ConversionService.Application.Services;
using ConversionService.Domain.Entities;
using ConversionService.Domain.Enums;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace ConversionService.UnitTests.Application.Services
{
    [TestClass]
    public class ReportRequestIngestionServiceTests
    {
        [TestMethod]
        public async Task EnqueueAsync_WhenMessageIsValid_CreatesPendingRequest()
        {
            DateTime fixedUtcNow = new(2026, 3, 30, 0, 0, 0, DateTimeKind.Utc);
            Guid requestId = Guid.NewGuid();

            FakeProductRepository productRepository = new()
            {
                ProductExists = true,
                CheckoutAllowed = true
            };

            FakeCheckoutRepository checkoutRepository = new()
            {
                CheckoutExists = true
            };

            FakeReportRequestRepository requestRepository = new();
            FakeStatusCache statusCache = new();
            FakeUnitOfWork unitOfWork = new();
            FakeClock clock = new(fixedUtcNow);

            ReportRequestIngestionService service = new(
                productRepository,
                checkoutRepository,
                requestRepository,
                statusCache,
                unitOfWork,
                clock);

            ReportRequestedMessage message = new()
            {
                RequestId = requestId,
                ExternalMessageId = "msg-001",
                ProductId = "PROD-001",
                CheckoutId = "CHK-STD",
                PeriodFrom = new DateOnly(2026, 3, 1),
                PeriodTo = new DateOnly(2026, 3, 3)
            };

            Guid result = await service.EnqueueAsync(message);

            Assert.AreEqual(requestId, result);
            Assert.AreEqual(1, requestRepository.AddedRequests.Count);
            Assert.AreEqual(1, unitOfWork.SaveChangesCalls);

            ReportRequest addedRequest = requestRepository.AddedRequests.Single();
            Assert.AreEqual(ReportRequestStatus.Pending, addedRequest.Status);
            Assert.AreEqual("PROD-001", addedRequest.ProductId);
            Assert.AreEqual("CHK-STD", addedRequest.CheckoutId);
            Assert.AreEqual(fixedUtcNow, addedRequest.CreatedAtUtc);
            Assert.AreEqual(fixedUtcNow, addedRequest.UpdatedAtUtc);

            Assert.AreEqual(1, statusCache.StoredResponses.Count);
            Assert.AreEqual("Pending", statusCache.StoredResponses[requestId].Status);
        }

        [TestMethod]
        public async Task EnqueueAsync_WhenRequestIdAlreadyExists_ReturnsExistingIdWithoutCreatingNewRequest()
        {
            Guid existingRequestId = Guid.NewGuid();

            FakeProductRepository productRepository = new()
            {
                ProductExists = true,
                CheckoutAllowed = true
            };

            FakeCheckoutRepository checkoutRepository = new()
            {
                CheckoutExists = true
            };

            FakeReportRequestRepository requestRepository = new();
            requestRepository.ExistingRequestIds.Add(existingRequestId);

            FakeStatusCache statusCache = new();
            FakeUnitOfWork unitOfWork = new();
            FakeClock clock = new(DateTime.UtcNow);

            ReportRequestIngestionService service = new(
                productRepository,
                checkoutRepository,
                requestRepository,
                statusCache,
                unitOfWork,
                clock);

            ReportRequestedMessage message = new()
            {
                RequestId = existingRequestId,
                ExternalMessageId = "msg-duplicate",
                ProductId = "PROD-001",
                CheckoutId = "CHK-STD",
                PeriodFrom = new DateOnly(2026, 3, 1),
                PeriodTo = new DateOnly(2026, 3, 3)
            };

            Guid result = await service.EnqueueAsync(message);

            Assert.AreEqual(existingRequestId, result);
            Assert.AreEqual(0, requestRepository.AddedRequests.Count);
            Assert.AreEqual(0, unitOfWork.SaveChangesCalls);
            Assert.AreEqual(0, statusCache.StoredResponses.Count);
        }

        [TestMethod]
        public async Task EnqueueAsync_WhenExternalMessageIdAlreadyExists_ReturnsExistingIdWithoutCreatingNewRequest()
        {
            Guid existingRequestId = Guid.NewGuid();

            FakeProductRepository productRepository = new()
            {
                ProductExists = true,
                CheckoutAllowed = true
            };

            FakeCheckoutRepository checkoutRepository = new()
            {
                CheckoutExists = true
            };

            FakeReportRequestRepository requestRepository = new();
            requestRepository.ExternalMessageIds["msg-001"] = existingRequestId;

            FakeStatusCache statusCache = new();
            FakeUnitOfWork unitOfWork = new();
            FakeClock clock = new(DateTime.UtcNow);

            ReportRequestIngestionService service = new(
                productRepository,
                checkoutRepository,
                requestRepository,
                statusCache,
                unitOfWork,
                clock);

            ReportRequestedMessage message = new()
            {
                RequestId = Guid.NewGuid(),
                ExternalMessageId = "msg-001",
                ProductId = "PROD-001",
                CheckoutId = "CHK-STD",
                PeriodFrom = new DateOnly(2026, 3, 1),
                PeriodTo = new DateOnly(2026, 3, 3)
            };

            Guid result = await service.EnqueueAsync(message);

            Assert.AreEqual(existingRequestId, result);
            Assert.AreEqual(0, requestRepository.AddedRequests.Count);
            Assert.AreEqual(0, unitOfWork.SaveChangesCalls);
        }

        [TestMethod]
        public async Task EnqueueAsync_WhenProductDoesNotExist_ThrowsInvalidOperationException()
        {
            FakeProductRepository productRepository = new()
            {
                ProductExists = false,
                CheckoutAllowed = true
            };

            FakeCheckoutRepository checkoutRepository = new()
            {
                CheckoutExists = true
            };

            FakeReportRequestRepository requestRepository = new();
            FakeStatusCache statusCache = new();
            FakeUnitOfWork unitOfWork = new();
            FakeClock clock = new(DateTime.UtcNow);

            ReportRequestIngestionService service = new(
                productRepository,
                checkoutRepository,
                requestRepository,
                statusCache,
                unitOfWork,
                clock);

            ReportRequestedMessage message = new()
            {
                RequestId = Guid.NewGuid(),
                ExternalMessageId = "msg-001",
                ProductId = "PROD-404",
                CheckoutId = "CHK-STD",
                PeriodFrom = new DateOnly(2026, 3, 1),
                PeriodTo = new DateOnly(2026, 3, 3)
            };

            await Assert.ThrowsExceptionAsync<InvalidOperationException>(() => service.EnqueueAsync(message));

            Assert.AreEqual(0, requestRepository.AddedRequests.Count);
            Assert.AreEqual(0, unitOfWork.SaveChangesCalls);
        }

        [TestMethod]
        public async Task EnqueueAsync_WhenCheckoutIsNotAllowedForProduct_ThrowsInvalidOperationException()
        {
            FakeProductRepository productRepository = new()
            {
                ProductExists = true,
                CheckoutAllowed = false
            };

            FakeCheckoutRepository checkoutRepository = new()
            {
                CheckoutExists = true
            };

            FakeReportRequestRepository requestRepository = new();
            FakeStatusCache statusCache = new();
            FakeUnitOfWork unitOfWork = new();
            FakeClock clock = new(DateTime.UtcNow);

            ReportRequestIngestionService service = new(
                productRepository,
                checkoutRepository,
                requestRepository,
                statusCache,
                unitOfWork,
                clock);

            ReportRequestedMessage message = new()
            {
                RequestId = Guid.NewGuid(),
                ExternalMessageId = "msg-001",
                ProductId = "PROD-001",
                CheckoutId = "CHK-FAST",
                PeriodFrom = new DateOnly(2026, 3, 1),
                PeriodTo = new DateOnly(2026, 3, 3)
            };

            await Assert.ThrowsExceptionAsync<InvalidOperationException>(() => service.EnqueueAsync(message));

            Assert.AreEqual(0, requestRepository.AddedRequests.Count);
            Assert.AreEqual(0, unitOfWork.SaveChangesCalls);
        }

        [TestMethod]
        public async Task EnqueueAsync_WhenPeriodIsInvalid_ThrowsArgumentException()
        {
            FakeProductRepository productRepository = new()
            {
                ProductExists = true,
                CheckoutAllowed = true
            };

            FakeCheckoutRepository checkoutRepository = new()
            {
                CheckoutExists = true
            };

            FakeReportRequestRepository requestRepository = new();
            FakeStatusCache statusCache = new();
            FakeUnitOfWork unitOfWork = new();
            FakeClock clock = new(DateTime.UtcNow);

            ReportRequestIngestionService service = new(
                productRepository,
                checkoutRepository,
                requestRepository,
                statusCache,
                unitOfWork,
                clock);

            ReportRequestedMessage message = new()
            {
                RequestId = Guid.NewGuid(),
                ExternalMessageId = "msg-001",
                ProductId = "PROD-001",
                CheckoutId = "CHK-STD",
                PeriodFrom = new DateOnly(2026, 3, 4),
                PeriodTo = new DateOnly(2026, 3, 3)
            };

            await Assert.ThrowsExceptionAsync<ArgumentException>(() => service.EnqueueAsync(message));

            Assert.AreEqual(0, requestRepository.AddedRequests.Count);
            Assert.AreEqual(0, unitOfWork.SaveChangesCalls);
        }

        private sealed class FakeProductRepository : IProductRepository
        {
            public bool ProductExists { get; set; }

            public bool CheckoutAllowed { get; set; }

            public Task<bool> ExistsAsync(string productId, CancellationToken cancellationToken = default)
            {
                return Task.FromResult(ProductExists);
            }

            public Task<bool> IsCheckoutAllowedAsync(
                string productId,
                string checkoutId,
                CancellationToken cancellationToken = default)
            {
                return Task.FromResult(CheckoutAllowed);
            }
        }

        private sealed class FakeCheckoutRepository : ICheckoutRepository
        {
            public bool CheckoutExists { get; set; }

            public Task<bool> ExistsAsync(string checkoutId, CancellationToken cancellationToken = default)
            {
                return Task.FromResult(CheckoutExists);
            }
        }

        private sealed class FakeReportRequestRepository : IReportRequestRepository
        {
            public HashSet<Guid> ExistingRequestIds { get; } = new();

            public Dictionary<string, Guid> ExternalMessageIds { get; } = new();

            public List<ReportRequest> AddedRequests { get; } = new();

            public Task<bool> ExistsByIdAsync(Guid requestId, CancellationToken cancellationToken = default)
            {
                return Task.FromResult(ExistingRequestIds.Contains(requestId));
            }

            public Task<Guid?> GetIdByExternalMessageIdAsync(string externalMessageId, CancellationToken cancellationToken = default)
            {
                if (ExternalMessageIds.TryGetValue(externalMessageId, out Guid requestId))
                {
                    return Task.FromResult<Guid?>(requestId);
                }

                return Task.FromResult<Guid?>(null);
            }

            public Task AddAsync(ReportRequest request, CancellationToken cancellationToken = default)
            {
                AddedRequests.Add(request);
                ExistingRequestIds.Add(request.Id);

                if (!string.IsNullOrWhiteSpace(request.ExternalMessageId))
                {
                    ExternalMessageIds[request.ExternalMessageId] = request.Id;
                }

                return Task.CompletedTask;
            }

            public Task<ReportRequest?> GetByIdAsync(Guid requestId, CancellationToken cancellationToken = default)
            {
                ReportRequest? request = AddedRequests.FirstOrDefault(x => x.Id == requestId);
                return Task.FromResult(request);
            }

            public Task<IReadOnlyCollection<ReportRequest>> GetPendingBatchAsync(int batchSize, CancellationToken cancellationToken = default)
            {
                return Task.FromResult<IReadOnlyCollection<ReportRequest>>(Array.Empty<ReportRequest>());
            }
        }

        private sealed class FakeStatusCache : IStatusCache
        {
            public Dictionary<Guid, ReportResponseDto> StoredResponses { get; } = new();

            public Task<ReportResponseDto?> GetAsync(Guid requestId, CancellationToken cancellationToken = default)
            {
                StoredResponses.TryGetValue(requestId, out ReportResponseDto? response);
                return Task.FromResult(response);
            }

            public Task SetAsync(ReportResponseDto response, CancellationToken cancellationToken = default)
            {
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