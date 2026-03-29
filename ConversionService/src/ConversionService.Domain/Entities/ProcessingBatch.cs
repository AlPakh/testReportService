using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using ConversionService.Domain.Enums;

namespace ConversionService.Domain.Entities
{
    public sealed class ProcessingBatch
    {
        private ProcessingBatch()
        {
        }

        public ProcessingBatch(Guid id, int itemsCount, DateTime startedAtUtc)
        {
            if (id == Guid.Empty)
            {
                throw new ArgumentException("Batch id must be specified.", nameof(id));
            }

            if (itemsCount < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(itemsCount));
            }

            Id = id;
            ItemsCount = itemsCount;
            StartedAtUtc = startedAtUtc;
            Status = ProcessingBatchStatus.Started;
        }

        public Guid Id { get; private set; }

        public ProcessingBatchStatus Status { get; private set; }

        public int ItemsCount { get; private set; }

        public DateTime StartedAtUtc { get; private set; }

        public DateTime? CompletedAtUtc { get; private set; }

        public void MarkCompleted(DateTime utcNow)
        {
            Status = ProcessingBatchStatus.Completed;
            CompletedAtUtc = utcNow;
        }

        public void MarkFailed(DateTime utcNow)
        {
            Status = ProcessingBatchStatus.Failed;
            CompletedAtUtc = utcNow;
        }
    }
}
