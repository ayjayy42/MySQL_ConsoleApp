# MySQL log importer

Requires .NET 10 SDK and MySQL for database writes. The existing database configuration in `CF_Context.cs` is retained.

```powershell
dotnet build -c Release
dotnet run --project LogFileReader_ConsoleApp -- "C:\logs\application.log" --dry-run
dotnet run --project LogFileReader_ConsoleApp -- "C:\logs\application.log"
```

`--dry-run` streams a JSON array to standard output without connecting to MySQL. The importer preserves repeated headers, multiline messages and incomplete final entries. Unsupported preamble lines are ignored. Database import retains the existing EF schema-creation behavior and commits the batch with `SaveChanges`.

Files of every size are read lazily with `StreamReader`, using one parser rather than a size threshold. It detects UTF byte-order marks and otherwise uses UTF-8. Parsing retains the current entry and one lookahead header; disposing enumeration early closes the file. Arrays and other line sequences remain supported through `ParseLogIntoObject(IEnumerable<string>)`.

Dry-run output is written incrementally instead of building an array of every entry or one large JSON string. Memory still depends on the largest individual entry. If a read fails after output begins, the process exits with code 1 and the output may be an incomplete JSON array; check the exit code before consuming it.

Database imports stream input but keep the existing single `SaveChanges` operation: EF tracks all pending rows until commit, so database-import memory still grows with the entry count. This avoids silently changing import transaction behavior. The reported import count comes from `SaveChanges`.

Exit codes: 0 success, 1 read/database failure, 2 invalid arguments. No console test project is included, as requested.
