using BlockedCountries.BL.Dtos;
using BlockedCountries.BL.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace BlockedCountries.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class LogsController : ControllerBase
    {
        private readonly LogService _logService;

        public LogsController(LogService logService)
        {
            _logService = logService;
        }

        [HttpGet("blocked-attempts")]
        public IActionResult GetBlockedAttempts([FromQuery] int page = 1,[FromQuery] int pageSize = 10)
        {
            try
            {
                if (page < 1 || pageSize < 1 || pageSize > 100)
                {
                    return BadRequest(ApiResponseDto<object>.ErrorResponse(
                        "Invalid pagination parameters",
                        errors: new List<string> { "INVALID_PAGINATION" }));
                }

                var allLogs = _logService.GetLogs()
                    .Where(log => log.IsBlocked)
                    .OrderByDescending(log => log.CheckedAt)
                    .ToList();

                var totalCount = allLogs.Count;
                var pagedLogs = allLogs
                    .Skip((page - 1) * pageSize)
                    .Take(pageSize)
                    .ToList();

                return Ok(ApiResponseDto<object>.SuccessResponse(
                    data: new
                    {
                        TotalCount = totalCount,
                        CurrentPage = page,
                        PageSize = pageSize,
                        Items = pagedLogs
                    },
                    message: "Blocked attempts retrieved successfully"));
            }
            catch (Exception ex)
            {
                return BadRequest(ApiResponseDto<object>.ErrorResponse(
                    "An error occurred while fetching blocked attempts",
                    errors: new List<string> { ex.Message }));
            }
        }
    }
}
