using System;
using System.Collections.Generic;

namespace ParquetFileViewer.Exceptions
{
    public class SomeFilesSkippedException : Exception
    {
        public record SkippedFile(string FileName, Exception Exception);

        public List<SkippedFile> SkippedFiles { get; private set; }

        internal SomeFilesSkippedException(IEnumerable<KeyValuePair<string, Exception>> skippedFiles) : base("Some files could not be opened.")
        {
            SkippedFiles = new List<SkippedFile>();

            if (skippedFiles is not null)
            {
                foreach (var skippedFile in skippedFiles)
                {
                    SkippedFiles.Add(new(skippedFile.Key, skippedFile.Value));
                }
            }
        }

    }
}
