namespace ParquetViewer.Engine.Types;

/// <summary>
/// A value that can render a shortened string of itself, so displaying large values doesn't require formatting all of them.
/// </summary>
public interface ITruncatable
{
    /// <summary>
    /// Returns the value as a string of roughly <paramref name="desiredLength"/> characters (NOT GUARANTEED).
    /// </summary>
    string ToStringTruncated(int desiredLength);
}
