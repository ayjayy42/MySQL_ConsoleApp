using System.Collections.Generic;
using LogFileReader_ConsoleApp.Adapters;
using LogFileReader_ConsoleApp.DB_Context;
using LogFileReader_ConsoleApp.Models;

namespace LogFileReader_ConsoleApp
{
    public class AppProvider
    {
        private readonly IEnumerable<string> _file;
        private readonly ILogFileReader _reader;
        private readonly IWriter _writer;

        public AppProvider(IEnumerable<string> logFile, ILogFileReader reader, IWriter writer)
        {
            _file = logFile;
            _writer = writer;
            _reader = reader;
        }

        public IEnumerable<Log> CreateLogFileObject()
        {
            var logs = _reader.ParseLogIntoObject(_file);

            return logs;
        }

        public void WriteToDb()
        {
            var logObject = CreateLogFileObject();
            _writer.WriteLogToDb(logObject);
        }
    }
}
