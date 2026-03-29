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
    public sealed class ReportRequestIngestionService
    {
        private readonly IProductRepository _productRepository;
        private readonly ICheckoutRepository _checkoutRepository;
        private readonly IReportRequestRepository _reportRequestRepository;
        private readonly IStatusCache _statusCache;
        private readonly IUnitOfWork _unitOfWork;
        private readonly IClock _clock;

        public ReportRequestIngestionService(
            IProductRepository productRepository,
            ICheckoutRepository checkoutRepository,
            IReportRequestRepository reportRequestRepository,
            IStatusCache statusCache,
            IUnitOfWork unitOfWork,
            IClock clock)
        {
            _productRepository = productRepository;
            _checkoutRepository = checkoutRepository;
            _reportRequestRepository = reportRequestRepository;
            _statusCache = statusCache;
            _unitOfWork = unitOfWork;
            _clock = clock;
        }

        public async Task<Guid> EnqueueAsync(ReportRequestedMessage message, CancellationToken cancellationToken = default)
        {
            Validate(message);

            if (await _reportRequestRepository.ExistsByIdAsync(message.RequestId, cancellationToken))
            {
                return message.RequestId;
            }

            if (!string.IsNullOrWhiteSpace(message.ExternalMessageId))
            {
                Guid? existingId = await _reportRequestRepository.GetIdByExternalMessageIdAsync(
                    message.ExternalMessageId,
                    cancellationToken);

                if (existingId.HasValue)
                {
                    return existingId.Value;
                }
            }

            if (!await _productRepository.ExistsAsync(message.ProductId, cancellationToken))
            {
                throw new InvalidOperationException($"Product '{message.ProductId}' does not exist.");
            }

            if (!await _checkoutRepository.ExistsAsync(message.CheckoutId, cancellationToken))
            {
                throw new InvalidOperationException($"Checkout '{message.CheckoutId}' does not exist.");
            }

            if (!await _productRepository.IsCheckoutAllowedAsync(message.ProductId, message.CheckoutId, cancellationToken))
            {
                throw new InvalidOperationException(
                    $"Checkout '{message.CheckoutId}' is not allowed for product '{message.ProductId}'.");
            }

            ReportRequest request = new(
                message.RequestId,
                message.ExternalMessageId,
                message.ProductId,
                message.CheckoutId,
                message.PeriodFrom,
                message.PeriodTo,
                _clock.UtcNow);

            await _reportRequestRepository.AddAsync(request, cancellationToken);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            await _statusCache.SetAsync(
                new ReportResponseDto
                {
                    RequestId = request.Id,
                    Status = request.Status.ToString()
                },
                cancellationToken);

            return request.Id;
        }

        private static void Validate(ReportRequestedMessage message)
        {
            if (message.RequestId == Guid.Empty)
            {
                throw new ArgumentException("RequestId must be specified.", nameof(message));
            }

            if (string.IsNullOrWhiteSpace(message.ProductId))
            {
                throw new ArgumentException("ProductId must be specified.", nameof(message));
            }

            if (string.IsNullOrWhiteSpace(message.CheckoutId))
            {
                throw new ArgumentException("CheckoutId must be specified.", nameof(message));
            }

            if (message.PeriodFrom > message.PeriodTo)
            {
                throw new ArgumentException("PeriodFrom must be less than or equal to PeriodTo.", nameof(message));
            }
        }
    }
}