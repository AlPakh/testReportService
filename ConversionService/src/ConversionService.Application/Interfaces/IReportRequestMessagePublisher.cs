using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using ConversionService.Application.Contracts;

namespace ConversionService.Application.Interfaces
{
    public interface IReportRequestMessagePublisher
    {
        Task PublishAsync(ReportRequestedMessage message, CancellationToken cancellationToken = default);
    }
}