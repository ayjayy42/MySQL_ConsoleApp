using System.Collections.Generic;
using LogFileReader_ConsoleApp.Models;

namespace LogFileReader_ConsoleApp.DB_Context
{
    public interface IWriter
    {
        int WriteLogToDb(IEnumerable<Log> file);
    }

    public class CF_Writer : IWriter
    {
        public CF_Writer()
        { }

        public int WriteLogToDb(IEnumerable<Log> file)
        {
            using (var ctx = new CF_Context())
            {
                foreach (var log in file)
                {
                    ctx.LogFile.Add(log);
                }

                // Finish reading before any database operation, so read failures cannot create a schema.
                ctx.Database.EnsureCreated();
                return ctx.SaveChanges();
            }
        }
    }
}
