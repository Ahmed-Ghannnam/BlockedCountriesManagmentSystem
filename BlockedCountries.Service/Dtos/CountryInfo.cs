using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BlockedCountries.BL.Dtos
{
    public class CountryBlockInfo
    {
        public string CountryCode { get; set; } = string.Empty;
        public string? CountryName { get; set; }
        // If null → permanent block; else → temporary block that expires
        public DateTime? ExpirationTimeUtc { get; set; } = null;

        public bool IsExpired => ExpirationTimeUtc.HasValue && ExpirationTimeUtc.Value <= DateTime.UtcNow;

        public bool IsTemporarilyBlocked => ExpirationTimeUtc.HasValue;
    }

}
