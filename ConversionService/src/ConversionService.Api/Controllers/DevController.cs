using ConversionService.Application.Contracts;
using ConversionService.Application.Interfaces;
using ConversionService.Application.Services;
using Microsoft.AspNetCore.Mvc;

namespace ConversionService.Api.Controllers
{
    [ApiController]
    [Route("dev")]
    public sealed class DevController : ControllerBase
    {
        private readonly IWebHostEnvironment _environment;
        private readonly IReportRequestMessagePublisher _publisher;
        private readonly ReportBatchProcessor _reportBatchProcessor;

        public DevController(
            IWebHostEnvironment environment,
            IReportRequestMessagePublisher publisher,
            ReportBatchProcessor reportBatchProcessor)
        {
            _environment = environment;
            _publisher = publisher;
            _reportBatchProcessor = reportBatchProcessor;
        }

        [HttpPost("report-requests")]
        [ProducesResponseType(StatusCodes.Status202Accepted)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> PublishReportRequest(
            [FromBody] ReportRequestedMessage message,
            CancellationToken cancellationToken)
        {
            if (!_environment.IsDevelopment())
            {
                return NotFound();
            }

            if (message.RequestId == Guid.Empty)
            {
                message.RequestId = Guid.NewGuid();
            }

            if (string.IsNullOrWhiteSpace(message.ExternalMessageId))
            {
                message.ExternalMessageId = Guid.NewGuid().ToString("N");
            }

            await _publisher.PublishAsync(message, cancellationToken);

            return Accepted(new
            {
                requestId = message.RequestId
            });
        }

        [HttpPost("process-pending")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> ProcessPending(
            [FromQuery] int batchSize = 100,
            CancellationToken cancellationToken = default)
        {
            if (!_environment.IsDevelopment())
            {
                return NotFound();
            }

            int processedCount = await _reportBatchProcessor.ProcessPendingAsync(batchSize, cancellationToken);

            return Ok(new
            {
                processedCount
            });
        }
    }
}