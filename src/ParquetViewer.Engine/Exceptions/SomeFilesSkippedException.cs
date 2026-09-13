namespace ParquetViewer.Engine.Exceptions;

public class SomeFilesSkippedException : Exception
{
    public record SkippedFile(string FileName, Exception Exception);

    public List<SkippedFile> SkippedFiles { get; private set; }

    public SomeFilesSkippedException(IEnumerable<KeyValuePair<string, Exception>> skippedFiles) : base("Some files could not be opened.")
    {
        SkippedFiles = [];

        if (skippedFiles is not null)
        {
            foreach (var skippedFile in skippedFiles)
            {
                SkippedFiles.Add(new(skippedFile.Key, skippedFile.Value));
            }
        }
    }
}