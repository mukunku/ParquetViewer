using Microsoft.Win32;
using System;

namespace ParquetFileViewer
{
    public static class AppSettings
    {
        private const string UseISODateFormatKey = "UseISODateFormat";
        private const string AlwaysSelectAllFieldsKey = "AlwaysSelectAllFields";
        private const string ParquetReadingEngineKey = "ParquetReadingEngine";
        public static bool UseISODateFormat
        {
            get
            {
                try
                {
                    using (RegistryKey registryKey = Registry.CurrentUser.CreateSubKey("ParquetViewer"))
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
                    using (RegistryKey registryKey = Registry.CurrentUser.CreateSubKey("ParquetViewer"))
                    {
                        registryKey.SetValue(UseISODateFormatKey, value.ToString());
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
                    using (RegistryKey registryKey = Registry.CurrentUser.CreateSubKey("ParquetViewer"))
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
                    using (RegistryKey registryKey = Registry.CurrentUser.CreateSubKey("ParquetViewer"))
                    {
                        registryKey.SetValue(AlwaysSelectAllFieldsKey, value.ToString());
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
                    using (RegistryKey registryKey = Registry.CurrentUser.CreateSubKey("ParquetViewer"))
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
                    using (RegistryKey registryKey = Registry.CurrentUser.CreateSubKey("ParquetViewer"))
                    {
                        registryKey.SetValue(ParquetReadingEngineKey, value.ToString());
                    }
                }
                catch { }
            }
        }
    }
}
