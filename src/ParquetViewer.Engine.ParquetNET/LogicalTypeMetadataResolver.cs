using Parquet.Meta;

namespace ParquetViewer.Engine.ParquetNET;

internal static class LogicalTypeMetadataResolver
{
    public static object? LogicalTypeToJSONObject(LogicalType? logicalType)
    {
        if (logicalType is null)
        {
            return null;
        }
        else if (logicalType.STRING is not null)
        {
            return new { Name = nameof(logicalType.STRING) };
        }
        else if (logicalType.MAP is not null)
        {
            return new { Name = nameof(logicalType.MAP) };
        }
        else if (logicalType.LIST is not null)
        {
            return new { Name = nameof(logicalType.LIST) };
        }
        else if (logicalType.ENUM is not null)
        {
            return new { Name = nameof(logicalType.ENUM) };
        }
        else if (logicalType.DECIMAL is not null)
        {
            return new
            {
                Name = nameof(logicalType.DECIMAL),
                logicalType.DECIMAL.Scale,
                logicalType.DECIMAL.Precision
            };
        }
        else if (logicalType.DATE is not null)
        {
            return new { Name = nameof(logicalType.DATE) };
        }
        else if (logicalType.TIME is not null)
        {
            return new
            {
                Name = nameof(logicalType.TIME),
                logicalType.TIME.IsAdjustedToUTC,
                Unit = TimeUnitToString(logicalType.TIME.Unit)
            };
        }
        else if (logicalType.TIMESTAMP is not null)
        {
            return new
            {
                Name = nameof(logicalType.TIMESTAMP),
                logicalType.TIMESTAMP.IsAdjustedToUTC,
                Unit = TimeUnitToString(logicalType.TIMESTAMP.Unit)
            };
        }
        else if (logicalType.INTEGER is not null)
        {
            return new
            {
                Name = nameof(logicalType.INTEGER),
                logicalType.INTEGER.BitWidth,
                logicalType.INTEGER.IsSigned
            };
        }
        else if (logicalType.JSON is not null)
        {
            return new { Name = nameof(logicalType.JSON) };
        }
        else if (logicalType.BSON is not null)
        {
            return new { Name = nameof(logicalType.BSON) };
        }
        else if (logicalType.UUID is not null)
        {
            return new { Name = nameof(logicalType.UUID) };
        }
        else if (logicalType.UNKNOWN is not null)
        {
            return new { Name = $"{logicalType.UNKNOWN.GetType().Name}" };
        }
        else
        {
            return new { Name = nameof(logicalType.UNKNOWN) };
        }
    }

    private static string TimeUnitToString(TimeUnit? timeUnit)
    {
        var timeUnitString = string.Empty;
        if (timeUnit?.MILLIS is not null)
        {
            timeUnitString = nameof(timeUnit.MILLIS);
        }
        else if (timeUnit?.MICROS is not null)
        {
            timeUnitString = nameof(timeUnit.MICROS);
        }
        else if (timeUnit?.NANOS is not null)
        {
            timeUnitString = nameof(timeUnit.NANOS);
        }
        return timeUnitString;
    }
}
