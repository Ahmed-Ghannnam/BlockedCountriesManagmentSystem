using BlockedCountries.BL.Dtos;
using BlockedCountries.BL.Managers;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace BlockedCountries.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class CountriesController : ControllerBase
    {
        private readonly BlockedCountryService _blockedCountryService;

        public CountriesController(BlockedCountryService blockedCountryService)
        {
            _blockedCountryService = blockedCountryService;
        }

        [HttpPost("block")]
        public IActionResult BlockCountry(string countryCode)
        {
            if (string.IsNullOrWhiteSpace(countryCode))
            {
                return BadRequest(ApiResponseDto<object>.ErrorResponse("Country code is required"));
            }

            if (!_blockedCountryService.AddPermanentBlockCountry(countryCode))
            {
                return Conflict(ApiResponseDto<object>.ErrorResponse($"Country '{countryCode}' is already blocked",
                    errors: new List<string> { "COUNTRY_ALREADY_BLOCKED" }));
            }

            return Ok(ApiResponseDto<object>.SuccessResponse(data: null, message: $"Country '{countryCode}' blocked successfully"));
        }

        [HttpPost("block-range")]
        public IActionResult BlockCountryRange([FromBody] BlockCountryRangeRequest request)
        {
            if (request.CountryCodes == null || request.CountryCodes.Count == 0)
            {
                return BadRequest(ApiResponseDto<object>.ErrorResponse("Country codes are required"));
            }

            var (added, alreadyBlocked) = _blockedCountryService.AddPermanentBlockCountries(request.CountryCodes);

            var result = new
            {
                Added = added,
                AlreadyBlocked = alreadyBlocked
            };

            return Ok(ApiResponseDto<object>.SuccessResponse(result, message: $"{added.Count} countries blocked successfully"));
        }



        [HttpDelete("block/{countryCode}")]
        public IActionResult UnblockCountry(string countryCode)
        {
            if (string.IsNullOrWhiteSpace(countryCode))
            {
                return BadRequest(ApiResponseDto<object>.ErrorResponse("Country code is required"));
            }

            if (!_blockedCountryService.Remove(countryCode))
            {
                return NotFound(ApiResponseDto<object>.ErrorResponse($"Country '{countryCode}' not found in blocked list", errors: new List<string> { "COUNTRY_NOT_BLOCKED" }));
            }

            return Ok(ApiResponseDto<object>.SuccessResponse(data: null, message: $"Country '{countryCode}' unblocked successfully"));
        }

        [HttpGet("blocked")]
        public IActionResult GetBlocked([FromQuery] PagedQuery query)
        {
            if (query.Page< 1 || query.PageSize< 1)
            {
                return BadRequest(ApiResponseDto<object>.ErrorResponse("Invalid pagination parameters"));
            }

            var result = _blockedCountryService.GetPaged(query.Page, query.PageSize, query.Search);

            return Ok(ApiResponseDto<PaginatedResult<CountryBlockInfo>>.SuccessResponse(
                data: result,
                message: "Blocked countries retrieved successfully"));
        }
        //
        [HttpPost("temporal-block")]
        public IActionResult TemporalBlock(string countryCode, int durationMinutes)
        {
            try
            {
                var code = countryCode?.ToUpper();

                if (!_blockedCountryService.IsValidCountryCode(code))
                {
                    return BadRequest(ApiResponseDto<object>.ErrorResponse(
                        "Invalid country code format"));
                }

                if (durationMinutes is < 1 or > 1440)
                {
                    return BadRequest(ApiResponseDto<object>.ErrorResponse(
                        "Duration must be between 1 and 1440 minutes (24 hours)",
                        errors: new List<string> { "INVALID_DURATION" }));
                }

                if (!_blockedCountryService.AddTemporaryBlock(code, durationMinutes, out var error))
                {
                    return Conflict(ApiResponseDto<object>.ErrorResponse(
                        error ?? "Country is already temporarily blocked",
                        errors: new List<string> { "ALREADY_TEMPORARILY_BLOCKED" }));
                }

                return Ok(ApiResponseDto<object>.SuccessResponse(
                    data: new
                    {
                        CountryCode = code,
                        DurationMinutes = durationMinutes,
                        ExpiresAt = DateTime.UtcNow.AddMinutes(durationMinutes)
                    },
                    message: $"Country {code} temporarily blocked for {durationMinutes} minutes"));
            }
            catch (Exception ex)
            {
                // Log error here (should be implemented)
                // _logger.LogError(ex, "Error in TemporalBlock");

                return BadRequest(ApiResponseDto<object>.ErrorResponse(
                    "An unexpected error occurred while processing your request",
                    errors: new List<string> { ex.Message }));
            }
        }


    }
}
