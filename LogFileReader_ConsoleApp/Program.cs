using System;
using System.Collections.Generic;
using System.Text.Json;
using LogFileReader_ConsoleApp.Adapters;
using LogFileReader_ConsoleApp.DB_Context;
using LogFileReader_ConsoleApp.Models;

namespace LogFileReader_ConsoleApp
{
    class Program
    {
        static int Main(string[] args)
        {
            if (args.Length is < 1 or > 2 || (args.Length == 2 && args[1] != "--dry-run"))
            {
                Console.Error.WriteLine("Usage: LogFileReader_ConsoleApp <log-file> [--dry-run]");
                return 2;
            }
            try
            {
                var logs = new LogFileReader().ParseLogFile(args[0]);
                if (args.Length == 2)
                    WriteJson(logs);
                else
                {
                    var imported = new CF_Writer().WriteLogToDb(logs);
                    Console.WriteLine($"Imported {imported} log entries.");
                }
                return 0;
            }
            catch (Exception ex)
            {
                Console.Error.WriteLine($"Import failed: {ex.Message}");
                return 1;
            }
        }

        private static void WriteJson(IEnumerable<Log> logs)
        {
            using var output = Console.OpenStandardOutput();
            using var json = new Utf8JsonWriter(output, new JsonWriterOptions { Indented = true });
            json.WriteStartArray();
            foreach (var log in logs)
            {
                JsonSerializer.Serialize(json, log);
                // Bound output buffering without flushing for every small entry.
                if (json.BytesPending >= 64 * 1024) json.Flush();
            }
            json.WriteEndArray();
            json.Flush();
            Console.WriteLine();
        }
    }
}
