using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ConversionService.Domain.Entities
{
    public sealed class ConversionFact
    {
        private ConversionFact()
        {
        }

        public ConversionFact(DateOnly factDate, string productId, string checkoutId, int viewsCount, int paymentsCount)
        {
            if (string.IsNullOrWhiteSpace(productId))
            {
                throw new ArgumentException("ProductId must be specified.", nameof(productId));
            }

            if (string.IsNullOrWhiteSpace(checkoutId))
            {
                throw new ArgumentException("CheckoutId must be specified.", nameof(checkoutId));
            }

            if (viewsCount < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(viewsCount));
            }

            if (paymentsCount < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(paymentsCount));
            }

            FactDate = factDate;
            ProductId = productId;
            CheckoutId = checkoutId;
            ViewsCount = viewsCount;
            PaymentsCount = paymentsCount;
        }

        public DateOnly FactDate { get; private set; }

        public string ProductId { get; private set; } = string.Empty;

        public string CheckoutId { get; private set; } = string.Empty;

        public int ViewsCount { get; private set; }

        public int PaymentsCount { get; private set; }
    }
}