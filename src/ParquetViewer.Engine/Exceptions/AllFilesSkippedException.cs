namespace ParquetViewer.Engine.Exceptions
{
    public class AllFilesSkippedException : Exception
    {
        public record SkippedFile(string FileName, Exception Exception);

        public List<SkippedFile> SkippedFiles { get; private set; }

        internal AllFilesSkippedException(IEnumerable<KeyValuePair<string, Exception>> skippedFiles) : base("Could not open any files in directory.")
        {
            SkippedFiles = new List<SkippedFile>();
            if (skippedFiles is not null)
            {
                foreach (var skippedFile in skippedFiles)
                {
                    SkippedFiles.Add(new(skippedFile.Key, skippedFile.Value));
                }
            }

            if (SkippedFiles.Count == 0)
            {
                throw new ArgumentException("No files were skipped", nameof(skippedFiles));
            }
        }
    }
}
