using System;
using System.IO;
using System.Linq;
using System.Text.Json;
using LogFileReader_ConsoleApp.Adapters;
using LogFileReader_ConsoleApp.DB_Context;

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
                var logs = new LogFileReader().ParseLogIntoObject(File.ReadAllLines(args[0])).ToArray();
                if (args.Length == 2)
                    Console.WriteLine(JsonSerializer.Serialize(logs, new JsonSerializerOptions { WriteIndented = true }));
                else
                {
                    new CF_Writer().WriteLogToDb(logs);
                    Console.WriteLine($"Imported {logs.Length} log entries.");
                }
                return 0;
            }
            catch (Exception ex)
            {
                Console.Error.WriteLine($"Import failed: {ex.Message}");
                return 1;
            }
        }
    }
}
