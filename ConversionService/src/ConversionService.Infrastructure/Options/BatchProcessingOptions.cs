using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ConversionService.Infrastructure.Options
{
    public sealed class BatchProcessingOptions
    {
        public const string SectionName = "BatchProcessing";

        public int BatchSize { get; set; } = 100;

        public int ScanIntervalSeconds { get; set; } = 30;
    }
}