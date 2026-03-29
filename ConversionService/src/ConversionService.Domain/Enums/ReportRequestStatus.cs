using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ConversionService.Domain.Enums
{
    public enum ReportRequestStatus : short
    {
        Pending = 1,
        Processing = 2,
        Completed = 3,
        Failed = 4
    }
}