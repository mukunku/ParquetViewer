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
        private const string REGISTRY_SUB = "ParquetViewer";
        private const string ALWAYS_SELECT_ALL_FIELDS = "AlwaysSelectAllFields";
        private const string DATE_TIME_DISPLAY_FORMAT = "DateTimeDisplayFormat";
        private const string CONSENT_LAST_ASKED_ON_VERSION = "ConsentLastAskedOnVersion";
        private const string ANALYTICS_DEVICE_ID = "AnalyticsDeviceId";
        private const string ANALYTICS_DATA_GATHERING_CONSENT = "AnalyticsDataGatheringConsent";
        private const string ALWAYS_LOAD_ALL_RECORDS = "AlwaysLoadAllRecords";
        private const string OPENED_FILE_COUNT = "OpenedFileCount";
        private const string CUSTOM_DATE_FORMAT = "CustomDateFormat";
        private const string DARK_MODE = "DarkMode";
        private const string USER_SELECTED_CULTURE = "UserSelectedCulture";

        public static DateFormat DateTimeDisplayFormat
        {
            get => ReadRegistryValue(DATE_TIME_DISPLAY_FORMAT, out int value) ? value.ToEnum(DateFormat.Default) : DateFormat.Default;
            set => SetRegistryValue(DATE_TIME_DISPLAY_FORMAT, (int)value);
        }

        public static bool AlwaysSelectAllFields
        {
            get => ReadRegistryValue(ALWAYS_SELECT_ALL_FIELDS, out string? temp) && bool.TryParse(temp, out var value) ? value : false;
            set => SetRegistryValue(ALWAYS_SELECT_ALL_FIELDS, value.ToString());
        }

        public static bool AlwaysLoadAllRecords
        {
            get => ReadRegistryValue(ALWAYS_LOAD_ALL_RECORDS, out string? temp) && bool.TryParse(temp, out var value) ? value : false;
            set => SetRegistryValue(ALWAYS_LOAD_ALL_RECORDS, value.ToString());
        }

        public static SemanticVersion? ConsentLastAskedOnVersion
        {
            get => ReadRegistryValue(CONSENT_LAST_ASKED_ON_VERSION, out string? value) ? SemanticVersion.TryParse(value, out var semanticVersion) ? semanticVersion : null : null;
            set => SetRegistryValue(CONSENT_LAST_ASKED_ON_VERSION, value?.ToString() ?? string.Empty);
        }

        public static Guid AnalyticsDeviceId
            => ReadRegistryValue(ANALYTICS_DEVICE_ID, out string? temp) && Guid.TryParse(temp, out var value) ? value : SetAnalyticsDeviceId();

        private static Guid SetAnalyticsDeviceId()
        {
            try
            {
                Guid newDeviceId = Guid.NewGuid();
                SetRegistryValue(ANALYTICS_DEVICE_ID, newDeviceId);
                return newDeviceId;
            }
            catch
            {
                return Guid.Empty;
            }
        }

        public static bool AnalyticsDataGatheringConsent
        {
            get => ReadRegistryValue(ANALYTICS_DATA_GATHERING_CONSENT, out string? temp) && bool.TryParse(temp, out var value) ? value : false;
            set => SetRegistryValue(ANALYTICS_DATA_GATHERING_CONSENT, value.ToString());
        }

        private static int? _openedFileCount;
        public static int OpenedFileCount
        {
            get => _openedFileCount ??= ReadRegistryValue(OPENED_FILE_COUNT, out int value) ? value : 0;
            set
            {
                _openedFileCount = value;
                SetRegistryValue(OPENED_FILE_COUNT, value);
            }
        }

        private static string? _customDateFormat;
        public static string? CustomDateFormat
        {
            get => _customDateFormat ??= ReadRegistryValue(CUSTOM_DATE_FORMAT, out string? value) && UtilityMethods.IsValidDateFormat(value) ? value : null;
            set
            {
                _customDateFormat = value;
                SetRegistryValue(CUSTOM_DATE_FORMAT, value ?? string.Empty);
            }
        }

        public static bool DarkMode
        {
            get => ReadRegistryValue(DARK_MODE, out string? temp) && bool.TryParse(temp, out var value) ? value : false;
            set
            {
                SetRegistryValue(DARK_MODE, value.ToString());
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
            get => ReadRegistryValue(USER_SELECTED_CULTURE, out string? value) ?
                (UtilityMethods.TryParseCultureInfo(value, out CultureInfo? cultureInfo) ? cultureInfo : null)
                : null;
            set => SetRegistryValue(USER_SELECTED_CULTURE, value?.ToString() ?? string.Empty);
        }

        private static bool ReadRegistryValue<T>(string key, [NotNullWhen(true)] out T? value)
        {
            try
            {
                using var registryKey = Registry.CurrentUser.CreateSubKey(REGISTRY_SUB);
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
                using var registryKey = Registry.CurrentUser.CreateSubKey(REGISTRY_SUB);
                registryKey.SetValue(key, value);
            }
            catch { }
        }
    }
}