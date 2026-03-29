using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ConversionService.Application.Interfaces
{
    public interface IProductRepository
    {
        Task<bool> ExistsAsync(string productId, CancellationToken cancellationToken = default);

        Task<bool> IsCheckoutAllowedAsync(string productId, string checkoutId, CancellationToken cancellationToken = default);
    }
}