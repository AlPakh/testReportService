using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ConversionService.Application.Services
{
    public sealed class ConversionCalculator
    {
        public decimal? CalculateRatio(int viewsCount, int paymentsCount)
        {
            if (paymentsCount <= 0)
            {
                return null;
            }

            return decimal.Round((decimal)viewsCount / paymentsCount, 6, MidpointRounding.AwayFromZero);
        }
    }
}