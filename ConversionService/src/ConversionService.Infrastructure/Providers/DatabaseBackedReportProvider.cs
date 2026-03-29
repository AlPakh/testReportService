using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using ConversionService.Application.Contracts;
using ConversionService.Application.Interfaces;
using ConversionService.Application.Services;
using ConversionService.Domain.Entities;
using ConversionService.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace ConversionService.Infrastructure.Providers
{
    public sealed class DatabaseBackedReportProvider : IReportProvider
    {
        private readonly ConversionServiceDbContext _dbContext;
        private readonly ConversionCalculator _conversionCalculator;

        public DatabaseBackedReportProvider(
            ConversionServiceDbContext dbContext,
            ConversionCalculator conversionCalculator)
        {
            _dbContext = dbContext;
            _conversionCalculator = conversionCalculator;
        }

        public async Task<IReadOnlyCollection<ReportCalculationResult>> BuildReportsAsync(
            IReadOnlyCollection<ReportRequest> requests,
            CancellationToken cancellationToken = default)
        {
            if (requests.Count == 0)
            {
                return Array.Empty<ReportCalculationResult>();
            }

            List<ReportRequest> requestList = requests.ToList();
            DateOnly minDate = requestList.Min(x => x.PeriodFrom);
            DateOnly maxDate = requestList.Max(x => x.PeriodTo);
            string[] productIds = requestList.Select(x => x.ProductId).Distinct().ToArray();
            string[] checkoutIds = requestList.Select(x => x.CheckoutId).Distinct().ToArray();

            List<ConversionFact> facts = await _dbContext.ConversionFacts
                .AsNoTracking()
                .Where(x =>
                    x.FactDate >= minDate &&
                    x.FactDate <= maxDate &&
                    productIds.Contains(x.ProductId) &&
                    checkoutIds.Contains(x.CheckoutId))
                .ToListAsync(cancellationToken);

            List<ReportCalculationResult> results = new(requestList.Count);

            foreach (ReportRequest request in requestList)
            {
                List<ConversionFact> matchedFacts = facts
                    .Where(x =>
                        x.ProductId == request.ProductId &&
                        x.CheckoutId == request.CheckoutId &&
                        x.FactDate >= request.PeriodFrom &&
                        x.FactDate <= request.PeriodTo)
                    .ToList();

                int viewsCount = matchedFacts.Sum(x => x.ViewsCount);
                int paymentsCount = matchedFacts.Sum(x => x.PaymentsCount);
                decimal? ratio = _conversionCalculator.CalculateRatio(viewsCount, paymentsCount);

                results.Add(new ReportCalculationResult
                {
                    RequestId = request.Id,
                    ViewsCount = viewsCount,
                    PaymentsCount = paymentsCount,
                    Ratio = ratio
                });
            }

            return results;
        }
    }
}