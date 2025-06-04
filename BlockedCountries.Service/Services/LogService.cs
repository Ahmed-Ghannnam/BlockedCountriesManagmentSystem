using BlockedCountries.BL.Dtos;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BlockedCountries.BL.Services
{    public class LogService
    {
        private readonly ConcurrentBag<IpBlockCheckLogDto> _logs = new();

        public void AddLog(IpBlockCheckLogDto log)
        {
            _logs.Add(log);
        }

        public IEnumerable<IpBlockCheckLogDto> GetLogs() => _logs.OrderByDescending(l => l.CheckedAt);
    }

}
