using ConversionService.Application.Contracts;
using ConversionService.Application.Services;
using Microsoft.AspNetCore.Mvc;

namespace ConversionService.Api.Controllers
{
    [ApiController]
    [Route("api/reports")]
    public sealed class ReportsController : ControllerBase
    {
        private readonly ReportQueryService _reportQueryService;

        public ReportsController(ReportQueryService reportQueryService)
        {
            _reportQueryService = reportQueryService;
        }

        [HttpGet("{requestId:guid}")]
        [ProducesResponseType(typeof(ReportResponseDto), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ReportResponseDto), StatusCodes.Status202Accepted)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GetById(Guid requestId, CancellationToken cancellationToken)
        {
            ReportResponseDto? response = await _reportQueryService.GetByRequestIdAsync(requestId, cancellationToken);

            if (response is null)
            {
                return NotFound();
            }

            if (string.Equals(response.Status, "Pending", StringComparison.OrdinalIgnoreCase) ||
                string.Equals(response.Status, "Processing", StringComparison.OrdinalIgnoreCase))
            {
                return Accepted(response);
            }

            return Ok(response);
        }
    }
}