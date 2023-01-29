using Microsoft.Win32;
using System;
using System.Windows.Forms;

namespace ParquetFileViewer
{
    public static class AppSettings
    {
        private const string RegistrySubKey = "ParquetViewer";
        private const string UseISODateFormatKey = "UseISODateFormat";
        private const string AlwaysSelectAllFieldsKey = "AlwaysSelectAllFields";
        private const string DefaultRowCountKey = "DefaultRowCount";
        private const string ParquetReadingEngineKey = "ParquetReadingEngine";
        private const string AutoSizeColumnsModeKey = "AutoSizeColumnsMode";
        private const string DateTimeDisplayFormatKey = "DateTimeDisplayFormat";

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
                        bool value = false;
                        bool.TryParse(registryKey.GetValue(AlwaysSelectAllFieldsKey)?.ToString(), out value);
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

        public static int DefaultRowCount
        {
            get
            {
                try
                {
                    using (RegistryKey registryKey = Registry.CurrentUser.CreateSubKey(RegistrySubKey))
                    {
                        int value = 1000;
                        int.TryParse(registryKey.GetValue(DefaultRowCountKey)?.ToInt32(), out value);
                        return value;
                    }
                }
                catch
                {
                    return 1000;
                }
            }
            set
            {
                try
                {
                    using (RegistryKey registryKey = Registry.CurrentUser.CreateSubKey(RegistrySubKey))
                    {
                        registryKey.SetValue(DefaultRowCountKey, value.ToInt32());
                    }
                }
                catch { }
            }
        }

        public static ParquetEngine ReadingEngine
        {
            get
            {
                try
                {
                    using (RegistryKey registryKey = Registry.CurrentUser.CreateSubKey(RegistrySubKey))
                    {
                        ParquetEngine value = default;
                        if (!Enum.TryParse<ParquetEngine>(registryKey.GetValue(ParquetReadingEngineKey)?.ToString(), out value))
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

        public static DataGridViewAutoSizeColumnsMode AutoSizeColumnsMode
        {
            get
            {
                try
                {
                    using (RegistryKey registryKey = Registry.CurrentUser.CreateSubKey(RegistrySubKey))
                    {
                        int? value = registryKey.GetValue(AutoSizeColumnsModeKey) as int?;
                        if (value != null && Enum.IsDefined(typeof(DataGridViewAutoSizeColumnsMode), value))
                            return (DataGridViewAutoSizeColumnsMode)value;
                        else
                            return DataGridViewAutoSizeColumnsMode.Fill;
                    }
                }
                catch
                {
                    return DataGridViewAutoSizeColumnsMode.Fill;
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
    }
}
