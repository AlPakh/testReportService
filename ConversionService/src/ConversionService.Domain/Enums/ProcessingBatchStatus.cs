using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ConversionService.Domain.Enums
{
    public enum ProcessingBatchStatus : short
    {
        Started = 1,
        Completed = 2,
        Failed = 3
    }
}