using ParquetViewer.Helpers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Windows.Forms;

namespace ParquetViewer.Controls
{
    public abstract class FormBase : Form
    {
        private static List<FormBase> _openForms = new();
        public static IEnumerable<FormBase> OpenForms => _openForms.Where(form => form?.IsDisposed == false);

        [DllImport("dwmapi.dll")]
        private static extern int DwmSetWindowAttribute(IntPtr hwnd, int attr, ref int attrValue, int attrSize);

        private const int DWMWA_USE_IMMERSIVE_DARK_MODE_BEFORE_20H1 = 19;
        private const int DWMWA_USE_IMMERSIVE_DARK_MODE = 20;

        public void UseDarkModeTitleBar() => UseImmersiveDarkMode(this.Handle, true);
        public void UseLightModeTitleBar() => UseImmersiveDarkMode(this.Handle, false);

        public virtual void SetTheme(Theme theme)
        {
            if (theme == Constants.LightModeTheme)
            {
                this.UseLightModeTitleBar();
            }
            else
            {
                this.UseDarkModeTitleBar();
            }
        }

        // Source: https://stackoverflow.com/a/62811758/1458738
        private static bool UseImmersiveDarkMode(IntPtr handle, bool enabled)
        {
            if (IsWindows10OrGreater(17763))
            {
                var attribute = DWMWA_USE_IMMERSIVE_DARK_MODE_BEFORE_20H1;
                if (IsWindows10OrGreater(18985))
                {
                    attribute = DWMWA_USE_IMMERSIVE_DARK_MODE;
                }

                int useImmersiveDarkMode = enabled ? 1 : 0;
                return DwmSetWindowAttribute(handle, (int)attribute, ref useImmersiveDarkMode, sizeof(int)) == 0;
            }

            return false;
        }

        private static bool IsWindows10OrGreater(int build = -1)
        {
            return Environment.OSVersion.Version.Major >= 10 && Environment.OSVersion.Version.Build >= build;
        }

        protected FormBase()
        {
            this.Load += (object? _, EventArgs _) =>
            {
                _openForms.Add(this);
                SetTheme(AppSettings.GetTheme());
            };
        }

        protected override void Dispose(bool disposing)
        {
            base.Dispose(disposing);
            _openForms.Remove(this);
        }
    }
}
