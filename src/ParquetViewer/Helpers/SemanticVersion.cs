using System;
using System.Text.RegularExpressions;

namespace ParquetViewer.Helpers;

public struct SemanticVersion : IComparable<SemanticVersion>, IComparable
{
    private const string VALIDATION_REGEX = @"^v?\d+\.\d+\.\d+(\.\d+)?$";

    public int Major { get; }
    public int Minor { get; }
    public int Patch { get; }
    public int Build { get; }

    public SemanticVersion(int majorVersion, int minorVersion, int patchVersion, int buildVersion)
    {
        ArgumentOutOfRangeException.ThrowIfLessThan(majorVersion, 0);
        ArgumentOutOfRangeException.ThrowIfLessThan(minorVersion, 0);
        ArgumentOutOfRangeException.ThrowIfLessThan(patchVersion, 0);
        ArgumentOutOfRangeException.ThrowIfLessThan(buildVersion, 0);

        Major = majorVersion;
        Minor = minorVersion;
        Patch = patchVersion;
        Build = buildVersion;
    }

    public static bool TryParse(string version, out SemanticVersion semanticVersion)
    {
        ArgumentNullException.ThrowIfNull(version);
        semanticVersion = default;

        if (!Regex.IsMatch(version, VALIDATION_REGEX))
            return false;

        var parts = version.Replace("v", string.Empty, StringComparison.OrdinalIgnoreCase).Split('.');

        //we should end up with 3 or 4 numbers
        if (parts.Length != 3 && parts.Length != 4)
            return false;

        if (!int.TryParse(parts[0], out var majorVersion)
            || !int.TryParse(parts[1], out var minorVersion)
            || !int.TryParse(parts[2], out var patchVersion))
            return false;

        var buildVersion = 0;
        if (parts.Length == 4 && !int.TryParse(parts[3], out buildVersion))
            return false;

        semanticVersion = new SemanticVersion(majorVersion, minorVersion, patchVersion, buildVersion);
        return true;
    }

    public readonly int CompareTo(SemanticVersion other)
    {
        if (Major < other.Major)
            return -1;

        if (Major > other.Major)
            return 1;

        if (Minor < other.Minor)
            return -1;

        if (Minor > other.Minor)
            return 1;

        if (Patch < other.Patch)
            return -1;

        if (Patch > other.Patch)
            return 1;

        if (Build < other.Build)
            return -1;

        if (Build > other.Build)
            return 1;

        return 0;
    }

    public readonly int CompareTo(object? obj)
    {
        if (obj is SemanticVersion semanticVersion)
            return CompareTo(semanticVersion);
        else
            return -1;
    }

    public static bool operator <(SemanticVersion left, SemanticVersion right) =>
        left.CompareTo(right) < 0;

    public static bool operator >(SemanticVersion left, SemanticVersion right) =>
        left.CompareTo(right) > 0;

    public static bool operator <=(SemanticVersion left, SemanticVersion right) =>
        left.CompareTo(right) <= 0;

    public static bool operator >=(SemanticVersion left, SemanticVersion right) =>
        left.CompareTo(right) >= 0;

    public static bool operator ==(SemanticVersion left, SemanticVersion right) =>
        left.Equals(right);

    public static bool operator !=(SemanticVersion left, SemanticVersion right) =>
        !left.Equals(right);

    // Override Equals and GetHashCode for value comparison
    public override readonly bool Equals(object? obj) =>
        obj is SemanticVersion other && Equals(other);

    public readonly bool Equals(SemanticVersion other) =>
        Major == other.Major
        && Minor == other.Minor
        && Patch == other.Patch
        && Build == other.Build;

    public readonly override int GetHashCode()
    {
        var hashcode = new HashCode();
        hashcode.Add(Major);
        hashcode.Add(Minor);
        hashcode.Add(Patch);
        hashcode.Add(Build);
        return hashcode.ToHashCode();
    }

    public readonly override string ToString()
        => $"{Major}.{Minor}.{Patch}.{Build}";
}