# MySQL log importer

Requires .NET 10 SDK and MySQL for database writes. The existing database configuration in `CF_Context.cs` is retained.

```powershell
dotnet build -c Release
dotnet run --project LogFileReader_ConsoleApp -- "C:\logs\application.log" --dry-run
dotnet run --project LogFileReader_ConsoleApp -- "C:\logs\application.log"
```

`--dry-run` prints parsed JSON without connecting to MySQL. The importer preserves repeated headers, multiline messages and incomplete final entries. Unsupported preamble lines are ignored. Database import retains the existing EF schema-creation behavior and commits the batch with `SaveChanges`.

Exit codes: 0 success, 1 read/database failure, 2 invalid arguments. No console test project is included, as requested.
