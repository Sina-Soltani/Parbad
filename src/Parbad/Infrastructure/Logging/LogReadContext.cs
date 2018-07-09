using System.Collections.Generic;
using System.Linq;

namespace Parbad.Infrastructure.Logging
{
    public class LogReadContext
    {
        public LogReadContext(int totalLogCount, IEnumerable<Log> logs)
        {
            if (totalLogCount < 0)
            {
                totalLogCount = 0;
            }

            TotalLogCount = totalLogCount;

            Logs = logs?.ToList() ?? Enumerable.Empty<Log>();
        }

        public static LogReadContext Empty => new LogReadContext(0, null);

        public int TotalLogCount { get; }

        public IEnumerable<Log> Logs { get; }
    }
}