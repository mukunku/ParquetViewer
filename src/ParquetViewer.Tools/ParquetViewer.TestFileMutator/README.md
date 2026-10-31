# ParquetViewer.TestFileMutator

A small console tool for manually testing ParquetViewer's **`ReloadEngineIfRequiredAsync()`**
file-modification-reload flow (`src/ParquetViewer/MainForm.cs`).

Parquet.Net's `ParquetReader` caches the thrift footer when a file is first opened, so
ParquetViewer must be reloaded with a brand-new engine instance whenever an already-open
file is modified in place on disk. This tool lets you reproduce that scenario by hand:

1. `create` a parquet file with a baseline schema.
2. Open it in ParquetViewer (all columns selected).
3. `same-columns` rewrites the file in place with the **same** columns but different rows
   and values. ParquetViewer should show "(modified)" in the title; triggering a
   reload (Ctrl+R, offset change, etc.) must show the new rows while keeping your selected
   columns intact.
4. `different-columns` rewrites the file in place with **different** columns. A reload must
   detect that the previously selected columns no longer exist and reset the selection to
   the new columns.

Writes replace the target via a temp file + `File.Copy(overwrite: true)`, which works even
while ParquetViewer has the file open for reading (`FileShare.ReadWrite | FileShare.Delete`).

## Usage

```text
dotnet run --project src\ParquetViewer.Tools\ParquetViewer.TestFileMutator\ParquetViewer.TestFileMutator.csproj [command] [path] [rows]
```

| Command             | Description                                                              |
| ------------------- | -------------------------------------------------------------------------|
| `create <path>`     | Write the baseline schema (Id int, Name string, Score double, IsActive bool, Rank int?). |
| `same-columns`      | Rewrite in place with the same columns, different rows/values (Score values are negated so stale vs fresh data is obvious). |
| `different-columns` | Rewrite in place with different columns (Code long, Label string, Rank int, Ratio double). |
| `info <path>`       | Print schema + row count while the file is open in the viewer.           |
| `help` / `exit`     | Interactive-mode commands.                                               |

Run with no arguments for interactive (scenario-by-scenario) mode.