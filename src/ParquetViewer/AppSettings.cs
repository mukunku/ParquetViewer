using Microsoft.Win32;
using ParquetViewer.Helpers;
using System;

namespace ParquetViewer
{
    public static class AppSettings
    {
        private const string RegistrySubKey = "ParquetViewer";
        private const string AlwaysSelectAllFieldsKey = "AlwaysSelectAllFields";
        private const string AutoSizeColumnsModeKey = "AutoSizeColumnsMode";
        private const string DateTimeDisplayFormatKey = "DateTimeDisplayFormat";
        private const string ConsentLastAskedOnVersionKey = "ConsentLastAskedOnVersion";
        private const string AnalyticsDeviceIdKey = "AnalyticsDeviceId";
        private const string AnalyticsDataGatheringConsentKey = "AnalyticsDataGatheringConsent";
        private const string AlwaysLoadAllRecordsKey = "AlwaysLoadAllRecords";
        private const string OpenedFileCountKey = "OpenedFileCount";
        private const string CustomDateFormatKey = "CustomDateFormat";
        private const string DarkModeKey = "DarkMode";

        public static DateFormat DateTimeDisplayFormat
        {
            get => ReadRegistryValue(DateTimeDisplayFormatKey, out int value) ? value.ToEnum(DateFormat.Default) : DateFormat.Default;
            set => SetRegistryValue(DateTimeDisplayFormatKey, (int)value);
        }

        public static bool AlwaysSelectAllFields
        {
            get => ReadRegistryValue(AlwaysSelectAllFieldsKey, out string? temp) && bool.TryParse(temp, out var value) ? value : false;
            set => SetRegistryValue(AlwaysSelectAllFieldsKey, value.ToString());
        }

        public static bool AlwaysLoadAllRecords
        {
            get => ReadRegistryValue(AlwaysLoadAllRecordsKey, out string? temp) && bool.TryParse(temp, out var value) ? value : false;
            set => SetRegistryValue(AlwaysLoadAllRecordsKey, value.ToString());
        }

        public static AutoSizeColumnsMode AutoSizeColumnsMode
        {
            get => ReadRegistryValue(AutoSizeColumnsModeKey, out int value) ? value.ToEnum(AutoSizeColumnsMode.AllCells) : AutoSizeColumnsMode.AllCells;
            set => SetRegistryValue(AutoSizeColumnsModeKey, (int)value);
        }

        public static string? ConsentLastAskedOnVersion
        {
            get => ReadRegistryValue(ConsentLastAskedOnVersionKey, out string? value) ? value : null;
            set => SetRegistryValue(ConsentLastAskedOnVersionKey, value ?? string.Empty);
        }

        public static Guid AnalyticsDeviceId 
            => ReadRegistryValue(AnalyticsDeviceIdKey, out string? temp) && Guid.TryParse(temp, out var value) ? value : SetAnalyticsDeviceId();

        private static Guid SetAnalyticsDeviceId()
        {
            try
            {
                Guid newDeviceId = Guid.NewGuid();
                SetRegistryValue(AnalyticsDeviceIdKey, newDeviceId);
                return newDeviceId;
            }
            catch
            {
                return Guid.Empty;
            }
        }

        public static bool AnalyticsDataGatheringConsent
        {
            get => ReadRegistryValue(AnalyticsDataGatheringConsentKey, out string? temp) && bool.TryParse(temp, out var value) ? value : false;
            set => SetRegistryValue(AnalyticsDataGatheringConsentKey, value.ToString());
        }

        private static int? _openedFileCount;
        public static int OpenedFileCount
        {
            get => _openedFileCount ??= ReadRegistryValue(OpenedFileCountKey, out int value) ? value : 0;
            set 
            {
                _openedFileCount = value;
                SetRegistryValue(OpenedFileCountKey, value);
            }
        }

        private static string? _customDateFormat;
        public static string? CustomDateFormat
        {
            get => _customDateFormat ??= ReadRegistryValue(CustomDateFormatKey, out string? value) && UtilityMethods.IsValidDateFormat(value) ? value : null;
            set
            {
                _customDateFormat = value;
                SetRegistryValue(CustomDateFormatKey, value ?? string.Empty);
            }
        }

        public static bool DarkMode
        {
            get => ReadRegistryValue(DarkModeKey, out string? temp) && bool.TryParse(temp, out var value) ? value : false;
            set => SetRegistryValue(DarkModeKey, value.ToString());
        }

        private static bool ReadRegistryValue<T>(string key, out T? value)
        {
            try
            {
                using var registryKey = Registry.CurrentUser.CreateSubKey(RegistrySubKey);
                if (registryKey.GetValue(key) is T castValue)
                {
                    value = castValue;
                    return true;
                }
                else
                {
                    value = default;
                    return false;
                }
            }
            catch
            {
                value = default;
                return false;
            }
        }

        private static void SetRegistryValue<T>(string key, T value)
        {
            if (value is null) //registry can't store null values
                throw new ArgumentNullException(nameof(value));

            try
            {
                using var registryKey = Registry.CurrentUser.CreateSubKey(RegistrySubKey);
                registryKey.SetValue(key, value);
            }
            catch { }
        }
    }
}