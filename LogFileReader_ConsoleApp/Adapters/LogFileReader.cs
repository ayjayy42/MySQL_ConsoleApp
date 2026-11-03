using System;
using System.Collections.Generic;
using LogFileReader_ConsoleApp.Models;

namespace LogFileReader_ConsoleApp.Adapters
{
    public interface ILogFileReader
    {
        IEnumerable<Log> ParseLogIntoObject(string[] input);
    }

    public class LogFileReader : ILogFileReader
    {
        public IEnumerable<Log> ParseLogIntoObject(string[] input)
        {
            ArgumentNullException.ThrowIfNull(input);
            for (var index = 0; index < input.Length;)
            {
                if (!TryHeader(input[index], out var level, out var source))
                {
                    index++;
                    continue;
                }
                index++;
                var lines = new List<string>();
                while (index < input.Length && !TryHeader(input[index], out _, out _))
                {
                    lines.Add(input[index].Trim());
                    index++;
                }
                // Keep incomplete entries visible instead of losing them or reading beyond EOF.
                var log = new Log { LogType = level, Message = lines.Count > 0 ? lines[0] : string.Empty };
                if (level == "info") log.Middleware = source;
                else log.FailedClass = source;
                if (level == "fail") log.StackTrace = string.Join(Environment.NewLine, lines.GetRange(Math.Min(1, lines.Count), Math.Max(0, lines.Count - 1)));
                else if (lines.Count > 1) log.Message = string.Join(Environment.NewLine, lines);
                yield return log;
            }
        }

        private static bool TryHeader(string line, out string level, out string source)
        {
            foreach (var prefix in new[] { "info:", "warn:", "fail:", "dbug:", "trce:", "crit:" })
            {
                if (line.StartsWith(prefix, StringComparison.Ordinal))
                {
                    level = prefix[..^1];
                    source = line[prefix.Length..].Trim();
                    return true;
                }
            }
            level = source = string.Empty;
            return false;
        }
    }
}
