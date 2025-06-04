using BlockedCountries.BL.Dtos;
using BlockedCountries.BL.Managers;
using BlockedCountries.BL.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Net;

namespace BlockedCountries.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class IpController : ControllerBase
    {
        private readonly GeoLocationService _geoService;
        private readonly BlockedCountryService _blockedService;
        private readonly LogService _logService;

        public IpController(GeoLocationService geoService, BlockedCountryService blockedService, LogService logService)
        {
            _geoService = geoService;
            _blockedService = blockedService;
            _logService = logService;
        }
        [HttpGet("lookup")]
        public async Task<IActionResult> Lookup([FromQuery] string? ipAddress = null)
        {
            try
            {
                ipAddress ??= HttpContext.Connection.RemoteIpAddress?.ToString();

                if (string.IsNullOrWhiteSpace(ipAddress) || !IPAddress.TryParse(ipAddress, out _))
                {
                    return BadRequest(ApiResponseDto<object>.ErrorResponse("Invalid or missing IP address."));
                }

                var result = await _geoService.GetFullIpInfoAsync(ipAddress);

                if (result == null)
                {
                    return NotFound(ApiResponseDto<object>.ErrorResponse("IP information not found."));
                }

                return Ok(ApiResponseDto<object>.SuccessResponse(result, "IP lookup successful"));
            }
            catch (Exception ex)
            {           
                return BadRequest( ApiResponseDto<object>.ErrorResponse(
                   $"{ ex.InnerException}",
                    new List<string> { ex.Message }));
            }
        }

        [HttpGet("check-block")]
        public async Task<IActionResult> CheckBlocked()
        {
            try
            {
                var ip = HttpContext.Request.Headers["X-Forwarded-For"].FirstOrDefault()
                        ?? HttpContext.Connection.RemoteIpAddress?.ToString();

                if (string.IsNullOrWhiteSpace(ip) || ip == "::1" || ip == "127.0.0.1")
                {
                    ip = await _geoService.GetServerExternalIpAsync();
                }

                var geoResult = await _geoService.GetFullIpInfoAsync(ip);
                if (geoResult == null)
                {
                    return BadRequest(ApiResponseDto<object>.ErrorResponse("IP lookup failed") );
                }

                var isBlocked = !string.IsNullOrWhiteSpace(geoResult.CountryCode) &&
                               _blockedService.IsBlocked(geoResult.CountryCode);

                _logService.AddLog(new IpBlockCheckLogDto
                {
                    IpAddress = ip,
                    CountryCode = geoResult.CountryCode,
                    IsBlocked = isBlocked,
                    UserAgent = Request.Headers["User-Agent"].ToString()
                });

                return Ok(ApiResponseDto<object>.SuccessResponse(
                    data: new
                    {
                        geoResult.IpAddress,
                        geoResult.CountryCode,
                        geoResult.CountryName,
                        geoResult.ISP,
                        IsBlocked = isBlocked
                    },
                    message: "IP check completed successfully"));
            }
            catch (Exception ex)
            {            
                return BadRequest( ApiResponseDto<object>.ErrorResponse(
                    "An error occurred while checking IP status",
                    errors: new List<string> { ex.Message }));
            }
        }
       
    }
}
