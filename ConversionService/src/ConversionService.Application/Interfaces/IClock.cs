using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ConversionService.Application.Interfaces
{
    public interface IClock
    {
        DateTime UtcNow { get; }
    }
}