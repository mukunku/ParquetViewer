using Microsoft.Win32;
using ParquetViewer.Controls;
using ParquetViewer.Helpers;
using System;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;

namespace ParquetViewer
{
    public static class AppSettings
    {
        private const string RegistrySubKey = "ParquetViewer";
        private const string AlwaysSelectAllFieldsKey = "AlwaysSelectAllFields";
        private const string DateTimeDisplayFormatKey = "DateTimeDisplayFormat";
        private const string ConsentLastAskedOnVersionKey = "ConsentLastAskedOnVersion";
        private const string AnalyticsDeviceIdKey = "AnalyticsDeviceId";
        private const string AnalyticsDataGatheringConsentKey = "AnalyticsDataGatheringConsent";
        private const string AlwaysLoadAllRecordsKey = "AlwaysLoadAllRecords";
        private const string OpenedFileCountKey = "OpenedFileCount";
        private const string CustomDateFormatKey = "CustomDateFormat";
        private const string DarkModeKey = "DarkMode";
        private const string UserSelectedCultureKey = "UserSelectedCulture";
        private const string QueryEditorZoomLevelKey = "QueryEditorZoomLevel";

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

        public static SemanticVersion? ConsentLastAskedOnVersion
        {
            get => ReadRegistryValue(ConsentLastAskedOnVersionKey, out string? value) ? SemanticVersion.TryParse(value, out var semanticVersion) ? semanticVersion : null : null;
            set => SetRegistryValue(ConsentLastAskedOnVersionKey, value?.ToString() ?? string.Empty);
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
            set
            {
                SetRegistryValue(DarkModeKey, value.ToString());
                var theme = GetTheme();
                foreach (var form in FormBase.OpenForms)
                {
                    form.SetTheme(theme);
                }
            }
        }

        public static Theme GetTheme() => DarkMode ? Theme.DarkModeTheme : Theme.LightModeTheme;

        public static CultureInfo? UserSelectedCulture
        {
            get => ReadRegistryValue(UserSelectedCultureKey, out string? value) ?
                (UtilityMethods.TryParseCultureInfo(value, out CultureInfo? cultureInfo) ? cultureInfo : null)
                : null;
            set => SetRegistryValue(UserSelectedCultureKey, value?.ToString() ?? string.Empty);
        }

        public static int? QueryEditorZoomLevel
        {
            get => ReadRegistryValue(QueryEditorZoomLevelKey, out int value) ? value : null;
            set => SetRegistryValue(QueryEditorZoomLevelKey, value);
        }
        

        private static bool ReadRegistryValue<T>(string key, [NotNullWhen(true)] out T? value)
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