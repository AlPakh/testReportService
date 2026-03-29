using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ConversionService.Domain.Entities
{
    public sealed class ProductCheckout
    {
        private ProductCheckout()
        {
        }

        public ProductCheckout(string productId, string checkoutId)
        {
            if (string.IsNullOrWhiteSpace(productId))
            {
                throw new ArgumentException("ProductId must be specified.", nameof(productId));
            }

            if (string.IsNullOrWhiteSpace(checkoutId))
            {
                throw new ArgumentException("CheckoutId must be specified.", nameof(checkoutId));
            }

            ProductId = productId;
            CheckoutId = checkoutId;
        }

        public string ProductId { get; private set; } = string.Empty;

        public string CheckoutId { get; private set; } = string.Empty;
    }
}