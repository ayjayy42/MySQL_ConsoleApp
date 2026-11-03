using System;
using System.Collections.Generic;
using System.IO;
using LogFileReader_ConsoleApp.Models;

namespace LogFileReader_ConsoleApp.Adapters
{
    public interface ILogFileReader
    {
        IEnumerable<Log> ParseLogFile(string path);
        IEnumerable<Log> ParseLogIntoObject(IEnumerable<string> input);
    }

    public class LogFileReader : ILogFileReader
    {
        private static readonly string[] HeaderPrefixes = { "info:", "warn:", "fail:", "dbug:", "trce:", "crit:" };

        public IEnumerable<Log> ParseLogFile(string path)
        {
            ArgumentException.ThrowIfNullOrWhiteSpace(path);
            return ParseLogIntoObject(ReadLines(path));
        }

        private static IEnumerable<string> ReadLines(string path)
        {
            // The iterator owns the reader, including when enumeration stops early.
            // StreamReader detects UTF BOMs and otherwise reads UTF-8.
            using var reader = new StreamReader(path);
            while (reader.ReadLine() is { } line)
                yield return line;
        }

        public IEnumerable<Log> ParseLogIntoObject(IEnumerable<string> input)
        {
            ArgumentNullException.ThrowIfNull(input);
            using var inputLines = input.GetEnumerator();
            var hasLine = inputLines.MoveNext();
            while (hasLine)
            {
                if (!TryHeader(inputLines.Current, out var level, out var source))
                {
                    hasLine = inputLines.MoveNext();
                    continue;
                }
                var lines = new List<string>();
                while ((hasLine = inputLines.MoveNext()) && !TryHeader(inputLines.Current, out _, out _))
                    lines.Add(inputLines.Current.Trim());

                // Keep only the current entry and the next header, even at EOF.
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
            foreach (var prefix in HeaderPrefixes)
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
