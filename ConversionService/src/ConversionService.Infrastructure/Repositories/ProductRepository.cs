using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using ConversionService.Application.Interfaces;
using ConversionService.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace ConversionService.Infrastructure.Repositories
{
    public sealed class ProductRepository : IProductRepository
    {
        private readonly ConversionServiceDbContext _dbContext;

        public ProductRepository(ConversionServiceDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public Task<bool> ExistsAsync(string productId, CancellationToken cancellationToken = default)
        {
            return _dbContext.Products.AnyAsync(x => x.Id == productId, cancellationToken);
        }

        public Task<bool> IsCheckoutAllowedAsync(
            string productId,
            string checkoutId,
            CancellationToken cancellationToken = default)
        {
            return _dbContext.ProductCheckouts.AnyAsync(
                x => x.ProductId == productId && x.CheckoutId == checkoutId,
                cancellationToken);
        }
    }
}