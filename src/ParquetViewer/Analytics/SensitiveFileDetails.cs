using System.Collections.Generic;
using System.Linq;

namespace ParquetViewer.Analytics;

/// <summary>
/// User values from the open file or folder that must be masked out of analytics.
/// </summary>
public readonly record struct SensitiveFileDetails(string? Path, IReadOnlyList<string> FieldNames)
{
    public string Mask(string message)
    {
        //Longest first so a value that's a substring of another can't leave part of the longer one unmasked
        var sensitiveValues = FieldNames
            .Append(Path)
            .Where(value => !string.IsNullOrEmpty(value))
            .OrderByDescending(value => value!.Length);

        foreach (var sensitiveValue in sensitiveValues)
        {
            message = message.Replace(sensitiveValue!, ExceptionEvent.MASK_SENTINEL,
                System.StringComparison.OrdinalIgnoreCase);
        }

        return message;
    }
}
