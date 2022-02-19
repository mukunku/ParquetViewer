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
        private const string ParquetReadingEngineKey = "ParquetReadingEngine";
        private const string AutoSizeColumnsModeKey = "AutoSizeColumnsMode";

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
