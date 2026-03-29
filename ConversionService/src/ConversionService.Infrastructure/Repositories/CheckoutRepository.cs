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
    public sealed class CheckoutRepository : ICheckoutRepository
    {
        private readonly ConversionServiceDbContext _dbContext;

        public CheckoutRepository(ConversionServiceDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public Task<bool> ExistsAsync(string checkoutId, CancellationToken cancellationToken = default)
        {
            return _dbContext.Checkouts.AnyAsync(x => x.Id == checkoutId, cancellationToken);
        }
    }
}