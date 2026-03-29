using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ConversionService.Application.Interfaces
{
    public interface ICheckoutRepository
    {
        Task<bool> ExistsAsync(string checkoutId, CancellationToken cancellationToken = default);
    }
}