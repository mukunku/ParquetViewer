using Microsoft.Win32;

namespace ParquetFileViewer
{
    public static class AppSettings
    {
        private const string UseISODateFormatKey = "UseISODateFormat";
        private const string AlwaysSelectAllFieldsKey = "AlwaysSelectAllFields";
        public static bool UseISODateFormat
        {
            get
            {
                using (RegistryKey registryKey = Registry.CurrentUser.CreateSubKey("ParquetViewer"))
                {
                    bool value = false;
                    bool.TryParse(registryKey.GetValue(UseISODateFormatKey)?.ToString(), out value);
                    return value;
                }
            }
            set
            {
                using (RegistryKey registryKey = Registry.CurrentUser.CreateSubKey("ParquetViewer"))
                {
                    registryKey.SetValue(UseISODateFormatKey, value.ToString());
                }
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
                using (RegistryKey registryKey = Registry.CurrentUser.CreateSubKey("ParquetViewer"))
                {
                    registryKey.SetValue(AlwaysSelectAllFieldsKey, value.ToString());
                }
            }
        }
    }
}
