using BlockedCountries.BL.Dtos;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace BlockedCountries.BL.Managers;

public class BlockedCountryService
{
    private readonly ConcurrentDictionary<string, CountryBlockInfo> _blockedCountries = new(StringComparer.OrdinalIgnoreCase);


    public bool AddPermanentBlockCountry(string countryCode)
    {
        return _blockedCountries.TryAdd(countryCode, new CountryBlockInfo
        {
            CountryCode = countryCode,
            ExpirationTimeUtc = null
        });
    }

    public bool IsBlocked(string countryCode)
    {
        var code = countryCode.ToUpper();

        if (_blockedCountries.TryGetValue(code, out var info))
        {
            if (info.IsExpired)
            {
                _blockedCountries.TryRemove(code, out _); // Optional cleanup
                return false;
            }

            return true;
        }

        return false;
    }

    public bool Remove(string code)
    {
        return _blockedCountries.TryRemove(code.ToUpper(), out _);
    }


    public IEnumerable<CountryBlockInfo> GetAll(string? search = null)
    {
        var data = _blockedCountries.Values/*.Where(c => !c.IsExpired)*/;

        if (!string.IsNullOrWhiteSpace(search))
        {
            var lowerSearch = search.ToLower();
            data = (ICollection<CountryBlockInfo>)data.Where(x => x.CountryCode.ToLower().Contains(lowerSearch));
        }

        return data.OrderBy(x => x.CountryCode);
    }


    public PaginatedResult<CountryBlockInfo> GetPaged(int page = 1, int pageSize = 5, string? search = null)
    {
        var filtered = GetAll(search);
        var total = filtered.Count();
        var items = filtered.Skip((page - 1) * pageSize).Take(pageSize);

        return new PaginatedResult<CountryBlockInfo>
        {
            Page = page,
            PageSize = pageSize,
            TotalCount = total,
            Items = items
        };
    }
    public (List<string> Added, List<string> AlreadyBlocked) AddPermanentBlockCountries(IEnumerable<string> countryCodes)
    {
        var added = new List<string>();
        var alreadyBlocked = new List<string>();

        foreach (var code in countryCodes)
        {
            if (AddPermanentBlockCountry(code))
                added.Add(code.ToUpper());
            else
                alreadyBlocked.Add(code.ToUpper());
        }

        return (added, alreadyBlocked);
    }

    public bool AddTemporaryBlock(string countryCode, int durationMinutes, out string? error)
    {
        error = null;

        if (_blockedCountries.TryGetValue(countryCode, out var existing) && !existing.IsExpired)
        {
            error = "Country already blocked.";
            return false;
        }

        _blockedCountries[countryCode] = new CountryBlockInfo
        {
            CountryCode = countryCode,
            ExpirationTimeUtc = DateTime.UtcNow.AddMinutes(durationMinutes)
        };

        return true;
    }
    public int RemoveExpiredBlocks()
    {
        int removedCount = 0;
        var now = DateTime.UtcNow;

        foreach (var kvp in _blockedCountries)
        {
            if (kvp.Value.IsExpired)
            {
                if (_blockedCountries.TryRemove(kvp.Key, out _))
                {
                    removedCount++;
                }
            }
        }

        return removedCount;
    }

    public bool IsValidCountryCode(string countryCode)
    {
        return !Regex.IsMatch(countryCode, "^[A-Z]{2}$");     
    }
}
