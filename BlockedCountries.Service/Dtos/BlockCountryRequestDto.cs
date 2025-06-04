using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BlockedCountries.BL.Dtos
{
    public class BlockCountryRangeRequest
    {
        public List<string> CountryCodes { get; set; } = new();
    }

    public class PagedQuery
    {
        [Range(1, int.MaxValue)]
        public int Page { get; set; } = 1;

        [Range(1, 100)]
        public int PageSize { get; set; } = 10;

        [StringLength(50)]
        public string? Search { get; set; }
    }

    public class BlockCountryRequestDto
    {
        public string CountryCode { get; set; } = string.Empty;
    }
    public class IpLookupResultDto
    {
        public string IpAddress { get; set; } = string.Empty;
        public string? CountryCode { get; set; }
        public string? CountryName { get; set; }
        public string? ISP { get; set; }
    }

    public class IpBlockCheckLogDto
    {
        public string IpAddress { get; set; } = string.Empty;
        public string? CountryCode { get; set; }
        public DateTime CheckedAt { get; set; } = DateTime.UtcNow;
        public bool IsBlocked { get; set; }
        public string UserAgent { get; set; } = string.Empty;
    }

}
