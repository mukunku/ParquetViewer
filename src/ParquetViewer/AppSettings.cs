using Microsoft.Win32;
using ParquetViewer.Helpers;
using System;

#nullable enable

namespace ParquetViewer
{
    public static class AppSettings
    {
        private const string RegistrySubKey = "ParquetViewer";
        private const string UseISODateFormatKey = "UseISODateFormat";
        private const string AlwaysSelectAllFieldsKey = "AlwaysSelectAllFields";
        private const string DefaultRowCountKey = "DefaultRowCount";
        private const string RememberLastRowCountKey = "RememberLastRowCount";
        private const string ParquetReadingEngineKey = "ParquetReadingEngine";
        private const string AutoSizeColumnsModeKey = "AutoSizeColumnsMode";
        private const string DateTimeDisplayFormatKey = "DateTimeDisplayFormat";
        private const string ConsentLastAskedOnVersionKey = "ConsentLastAskedOnVersion";
        private const string AnalyticsDeviceIdKey = "AnalyticsDeviceId";
        private const string AnalyticsDataGatheringConsentKey = "AnalyticsDataGatheringConsent";

        //TODO: Cleanup this setting after sufficient time has passed.
        [Obsolete($"We have more date formats now so use {nameof(DateTimeDisplayFormat)} instead.")]
        public static bool UseISODateFormat
        {
            get
            {
                try
                {
                    using (RegistryKey registryKey = Registry.CurrentUser.CreateSubKey(RegistrySubKey))
                    {
                        bool value = false;
                        bool.TryParse(registryKey.GetValue(UseISODateFormatKey)?.ToString(), out value);
                        return value;
                    }
                }
                catch
                {
                    return false;
                }
            }
            set
            {
                try
                {
                    using (RegistryKey registryKey = Registry.CurrentUser.CreateSubKey(RegistrySubKey))
                    {
                        registryKey.SetValue(UseISODateFormatKey, value.ToString());
                    }
                }
                catch { }
            }
        }

        public static DateFormat DateTimeDisplayFormat
        {
            get
            {
                try
                {
                    using (RegistryKey registryKey = Registry.CurrentUser.CreateSubKey(RegistrySubKey))
                    {
                        int? value = registryKey.GetValue(DateTimeDisplayFormatKey) as int?;
                        if (value is null)
                        {
                            //Fallback to legacy site setting until everyone switches over to this new site setting
                            var useIsoFormat = false;
                            if (bool.TryParse(registryKey.GetValue(UseISODateFormatKey)?.ToString(), out useIsoFormat))
                            {
                                value = (int)(useIsoFormat ? DateFormat.ISO8601 : DateFormat.Default);

                                //Also set the new app setting registry key value
                                DateTimeDisplayFormat = (DateFormat)value;
                            }
                            else
                            {
                                value = (int)default(DateFormat);
                            }
                        }

                        return (DateFormat)value.Value;
                    }
                }
                catch
                {
                    return default;
                }
            }
            set
            {
                try
                {
                    using (RegistryKey registryKey = Registry.CurrentUser.CreateSubKey(RegistrySubKey))
                    {
                        registryKey.SetValue(DateTimeDisplayFormatKey, (int)value);
                    }
                }
                catch { }
            }
        }

        public static bool AlwaysSelectAllFields
        {
            get
            {
                try
                {
                    using (RegistryKey registryKey = Registry.CurrentUser.CreateSubKey(RegistrySubKey))
                    {
                        bool.TryParse(registryKey.GetValue(AlwaysSelectAllFieldsKey)?.ToString(), out var value);
                        return value;
                    }
                }
                catch
                {
                    return false;
                }
            }
            set
            {
                try
                {
                    using (RegistryKey registryKey = Registry.CurrentUser.CreateSubKey(RegistrySubKey))
                    {
                        registryKey.SetValue(AlwaysSelectAllFieldsKey, value.ToString());
                    }
                }
                catch { }
            }
        }

        public static int? LastRowCount
        {
            get
            {
                try
                {
                    if (!RememberLastRowCount)
                        return null;

                    using (RegistryKey registryKey = Registry.CurrentUser.CreateSubKey(RegistrySubKey))
                    {
                        int.TryParse(registryKey.GetValue(DefaultRowCountKey)?.ToString(), out var value);
                        return value <= 0 ? null : value;
                    }
                }
                catch
                {
                    return null;
                }
            }
            set
            {
                try
                {
                    using (RegistryKey registryKey = Registry.CurrentUser.CreateSubKey(RegistrySubKey))
                    {
                        registryKey.SetValue(DefaultRowCountKey, value?.ToString() ?? string.Empty);
                    }
                }
                catch { }
            }
        }

        public static bool RememberLastRowCount
        {
            get
            {
                try
                {
                    using (RegistryKey registryKey = Registry.CurrentUser.CreateSubKey(RegistrySubKey))
                    {
                        bool.TryParse(registryKey.GetValue(RememberLastRowCountKey)?.ToString(), out var value);
                        return value;
                    }
                }
                catch
                {
                    return false;
                }
            }
            set
            {
                try
                {
                    using (RegistryKey registryKey = Registry.CurrentUser.CreateSubKey(RegistrySubKey))
                    {
                        registryKey.SetValue(RememberLastRowCountKey, value.ToString());
                    }
                }
                catch { }
            }
        }

        [Obsolete("This setting is no longer being used. We try to automatically switch between engines now.")]
        public static ParquetEngine ReadingEngine
        {
            get
            {
                try
                {
                    using (RegistryKey registryKey = Registry.CurrentUser.CreateSubKey(RegistrySubKey))
                    {
                        ParquetEngine value = default;
                        if (!Enum.TryParse(registryKey.GetValue(ParquetReadingEngineKey)?.ToString(), out value))
                            value = default;

                        return value;
                    }
                }
                catch
                {
                    return default;
                }
            }
            set
            {
                try
                {
                    using (RegistryKey registryKey = Registry.CurrentUser.CreateSubKey(RegistrySubKey))
                    {
                        registryKey.SetValue(ParquetReadingEngineKey, value.ToString());
                    }
                }
                catch { }
            }
        }

        public static AutoSizeColumnsMode AutoSizeColumnsMode
        {
            get
            {
                try
                {
                    using (RegistryKey registryKey = Registry.CurrentUser.CreateSubKey(RegistrySubKey))
                    {
                        int? value = registryKey.GetValue(AutoSizeColumnsModeKey) as int?;
                        if (value != null && Enum.IsDefined(typeof(AutoSizeColumnsMode), value))
                            return (AutoSizeColumnsMode)value;
                        else
                            return AutoSizeColumnsMode.None;
                    }
                }
                catch
                {
                    return AutoSizeColumnsMode.None;
                }
            }
            set
            {
                try
                {
                    using (RegistryKey registryKey = Registry.CurrentUser.CreateSubKey(RegistrySubKey))
                    {
                        registryKey.SetValue(AutoSizeColumnsModeKey, (int)value);
                    }
                }
                catch { }
            }
        }

        public static string? ConsentLastAskedOnVersion
        {
            get
            {
                try
                {
                    using (RegistryKey registryKey = Registry.CurrentUser.CreateSubKey(RegistrySubKey))
                    {
                        return registryKey.GetValue(ConsentLastAskedOnVersionKey)?.ToString();
                    }
                }
                catch
                {
                    return null;
                }
            }
            set
            {
                try
                {
                    using (RegistryKey registryKey = Registry.CurrentUser.CreateSubKey(RegistrySubKey))
                    {
                        registryKey.SetValue(ConsentLastAskedOnVersionKey, value ?? string.Empty);
                    }
                }
                catch { }
            }
        }

        public static Guid AnalyticsDeviceId
        {
            get
            {
                try
                {
                    using (RegistryKey registryKey = Registry.CurrentUser.CreateSubKey(RegistrySubKey))
                    {
                        if (Guid.TryParse(registryKey.GetValue(AnalyticsDeviceIdKey)?.ToString(), out var value))
                        {
                            return value;
                        }
                        else
                        {
                            //This user doesn't have an analytics device id yet, so create one
                            Guid newDeviceId = Guid.NewGuid();
                            registryKey.SetValue(AnalyticsDeviceIdKey, newDeviceId);
                            return newDeviceId;
                        }
                    }
                }
                catch
                {
                    return Guid.Empty;
                }
            }
        }
    
        public static bool AnalyticsDataGatheringConsent
        {
            get
            {
                try
                {
                    using (RegistryKey registryKey = Registry.CurrentUser.CreateSubKey(RegistrySubKey))
                    {
                        bool.TryParse(registryKey.GetValue(AnalyticsDataGatheringConsentKey)?.ToString(), out var value);
                        return value;
                    }
                }
                catch
                {
                    return false;
                }
            }
            set
            {
                try
                {
                    using (RegistryKey registryKey = Registry.CurrentUser.CreateSubKey(RegistrySubKey))
                    {
                        registryKey.SetValue(AnalyticsDataGatheringConsentKey, value.ToString());
                    }
                }
                catch { }
            }
        }
    }
}