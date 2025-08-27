using Microsoft.Win32;
using ParquetViewer.Analytics;
using ParquetViewer.Controls;
using ParquetViewer.Helpers;
using System;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Text;
using System.Windows.Forms;

namespace ParquetViewer
{
    public partial class AboutBox : FormBase
    {
        public const string PERFORM_FILE_ASSOCIATION = "PERFORM_FILE_ASSOCIATION";

#if RELEASE_SELFCONTAINED
        private bool _isSelfContainedExe = true;
#else
        private bool _isSelfContainedExe = false;
#endif

        private bool isLoading = false;

        private static string _exePath => System.Windows.Forms.Application.ExecutablePath;

        private static string _exeName => Path.GetFileName(_exePath);

        public static bool IsDefaultViewerForParquetFiles => AssociateParquetFileExtension(true);

        public AboutBox()
        {
            InitializeComponent();
            this.Text = String.Format("About {0}", AssemblyTitle);
            this.labelProductName.Text = AssemblyProduct;
            this.labelVersion.Text = String.Format("Version {0}{1}", AssemblyVersion, _isSelfContainedExe ? " SC" : string.Empty);
            this.labelCopyright.Text = AssemblyCopyright;
            this.labelCompanyName.Text = AssemblyCompany;
            this.textBoxDescription.Text = AssemblyDescription;
            this.newVersionLabel.Text = string.Empty;

            if (!AmplitudeEvent.HasApiKey)
            {
                this.textBoxDescription.Text = $"No Amplitude API Key!{Environment.NewLine}{Environment.NewLine}"
                    + this.textBoxDescription.Text;
            }
        }

        #region Assembly Attribute Accessors

        public string AssemblyTitle
        {
            get
            {
                object[] attributes = Assembly.GetExecutingAssembly().GetCustomAttributes(typeof(AssemblyTitleAttribute), false);
                if (attributes.Length > 0)
                {
                    AssemblyTitleAttribute titleAttribute = (AssemblyTitleAttribute)attributes[0];
                    if (titleAttribute.Title != string.Empty)
                    {
                        return titleAttribute.Title;
                    }
                }
                return "ParquetViewer.exe";
            }
        }

        public string AssemblyVersion => Env.AssemblyVersion.ToString();

        public string AssemblyDescription
        {
            get
            {
                object[] attributes = Assembly.GetExecutingAssembly().GetCustomAttributes(typeof(AssemblyDescriptionAttribute), false);
                if (attributes.Length == 0)
                {
                    return string.Empty;
                }
                return ((AssemblyDescriptionAttribute)attributes[0]).Description;
            }
        }

        public string AssemblyProduct
        {
            get
            {
                object[] attributes = Assembly.GetExecutingAssembly().GetCustomAttributes(typeof(AssemblyProductAttribute), false);
                if (attributes.Length == 0)
                {
                    return string.Empty;
                }
                return ((AssemblyProductAttribute)attributes[0]).Product;
            }
        }

        public string AssemblyCopyright
        {
            get
            {
                object[] attributes = Assembly.GetExecutingAssembly().GetCustomAttributes(typeof(AssemblyCopyrightAttribute), false);
                if (attributes.Length == 0)
                {
                    return string.Empty;
                }
                return ((AssemblyCopyrightAttribute)attributes[0]).Copyright;
            }
        }

        public string AssemblyCompany
        {
            get
            {
                object[] attributes = Assembly.GetExecutingAssembly().GetCustomAttributes(typeof(AssemblyCompanyAttribute), false);
                if (attributes.Length == 0)
                {
                    return string.Empty;
                }
                return ((AssemblyCompanyAttribute)attributes[0]).Company;
            }
        }

        public string AssemblyPublicKey
        {
            get
            {
                var publicKey = Assembly.GetExecutingAssembly().GetName().GetPublicKeyToken();
                if (publicKey is null || publicKey.Length == 0)
                {
                    return string.Empty;
                }

                var sb = new StringBuilder();
                for (int i = 0; i < publicKey.Length; i++)
                {
                    sb.AppendFormat("{0:x2}", publicKey[i]);
                }
                return sb.ToString();
            }
        }
        #endregion

        private void AboutBox_KeyUp(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Escape)
            {
                this.Close();
            }
        }

        public static bool ToggleFileAssociation(bool associate)
        {
            if (!User.IsAdministrator)
            {
                return false;
            }

            bool result;
            if (associate)
            {
                result = AssociateParquetFileExtension(false);
            }
            else
            {
                Registry.CurrentUser.DeleteSubKeyTreeIfExists("SOFTWARE\\Microsoft\\Windows\\CurrentVersion\\Explorer\\FileExts\\.parquet");

                //Also delete from "Open With" list
                Registry.ClassesRoot.DeleteSubKeyTreeIfExists("ParquetViewer");
                Registry.ClassesRoot.DeleteSubKeyTreeIfExists(".parquet");
                result = true;
            }

            //Tell Windows about the change
            SHChangeNotify(0x08000000, 0x0000, IntPtr.Zero, IntPtr.Zero);

            return result;
        }

        private void associateFileExtensionCheckBox_CheckedChanged(object? sender, EventArgs? e)
        {
            if (this.isLoading)
            {
                return;
            }

            if (!User.IsAdministrator)
            {
                bool? success = RunElevatedExeForFileAssociation(associateFileExtensionCheckBox.Checked, out int? exitCode);
                if (success is null)
                {
                    //User cancelled the operation. Rollback checkbox
                    SetCheckboxSilent(!associateFileExtensionCheckBox.Checked);
                }
                else if (success == false)
                {
                    MessageBox.Show(this, $"Something went wrong (Error code: {exitCode}). " +
                        $"Try running ParquetViewer as administrator and try again.",
                        "File association failed", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    SetCheckboxSilent(!associateFileExtensionCheckBox.Checked);
                }
                else
                {
                    //Success!
                }
            }
            else
            {
                ToggleFileAssociation(associateFileExtensionCheckBox.Checked);
            }
        }

        //I tested this logic on both Windows 10 & 11 and it works for both
        private static bool AssociateParquetFileExtension(bool dryRun)
        {
            if (!User.IsAdministrator && !dryRun)
            {
                throw new InvalidOperationException("Can't create file association without admin privileges");
            }

            if (User.IsAdministrator)
            {
                //Add ParquetViewer to "Open With" list
                using var parquetKey = Registry.ClassesRoot.CreateSubKey(".parquet");
                if (!"ParquetViewer".Equals(parquetKey.GetValue(null)))
                {
                    if (dryRun) return false;

                    parquetKey.SetValue(null, "ParquetViewer");
                }

                using var parquetViewerKey = Registry.ClassesRoot.CreateSubKey("ParquetViewer");
                if (!"Apache Parquet file".Equals(parquetViewerKey.GetValue(null)))
                {
                    if (dryRun) return false;

                    parquetViewerKey.SetValue(null, "Apache Parquet file");
                }

                using var openKey = parquetViewerKey.CreateSubKey("shell\\open");
                using var commandKey = openKey.CreateSubKey("command");
                if (!$"\"{_exePath}\" \"%1\"".Equals(commandKey.GetValue(null)))
                {
                    if (dryRun) return false;

                    openKey.SetValue("icon", $"{_exePath},0");
                    commandKey.SetValue(null, $"\"{_exePath}\" \"%1\"");
                }
            }

            if (!dryRun)
            {
                //Clear any existing settings
                Registry.CurrentUser.DeleteSubKeyTreeIfExists("SOFTWARE\\Microsoft\\Windows\\CurrentVersion\\Explorer\\FileExts\\.parquet");
            }

            using var parquetExtKey
                = Registry.CurrentUser.CreateSubKey("SOFTWARE\\Microsoft\\Windows\\CurrentVersion\\Explorer\\FileExts\\.parquet");

            //Make ParquetViewer the default for doubleclick
            using var openWithListKey = parquetExtKey.CreateSubKey("OpenWithList");
            var mostRecentlyUsedList = openWithListKey.GetValue("MRUList") as string;
            if (string.IsNullOrWhiteSpace(mostRecentlyUsedList))
            {
                if (dryRun) return false;

                openWithListKey.SetValue("a", _exeName);
                openWithListKey.SetValue("MRUList", "a");
            }
            else
            {
                var mostRecentlyUsed = mostRecentlyUsedList[0];
                var mostRecentlyUsedProgram = openWithListKey.GetValue(mostRecentlyUsed.ToString()) as string;
                if (string.IsNullOrWhiteSpace(mostRecentlyUsedProgram))
                {
                    if (dryRun) return false;

                    openWithListKey.SetValue(mostRecentlyUsedProgram, _exeName);
                }
                else if (!mostRecentlyUsedProgram.Equals(_exeName))
                {
                    if (dryRun)
                        return false;
                    else
                        throw new InvalidOperationException("We should never get here on non-dry runs");
                }
                else
                {
                    //all set!
                }
            }

            using var openWithProgIdsKey = parquetExtKey.CreateSubKey("OpenWithProgids");
            if (openWithProgIdsKey.GetValue("ParquetViewer") is null)
            {
                if (dryRun) return false;

                openWithProgIdsKey.SetValue("ParquetViewer", 0); //references the 'parquetViewerKey' registry key we created earlier
            }

            return true; //file association exists
        }

        [DllImport("shell32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        private static extern void SHChangeNotify(uint wEventId, uint uFlags, IntPtr dwItem1, IntPtr dwItem2);

        private async void AboutBox_Load(object sender, EventArgs e)
        {
            try
            {
                SetCheckboxSilent(IsDefaultViewerForParquetFiles);
            }
            catch
            {
                this.associateFileExtensionCheckBox.Enabled = false;
            }

            try
            {
                var latestRelease = await Env.FetchLatestRelease();
                if (latestRelease.Version > Env.AssemblyVersion)
                {
                    this.newVersionLabel.Text = $"New version {latestRelease.Version.ToString()} available for update!";
                    this.newVersionLabel.Tag = latestRelease.Url?.ToString();
                    //TODO: Set image for linklabel? like alarm icon or exclamation
                }
                else if (latestRelease.Version == Env.AssemblyVersion)
                {
                    this.newVersionLabel.Text = $"Your version is up-to-date 👍";
                    this.newVersionLabel.Tag = null;
                    this.newVersionLabel.LinkBehavior = LinkBehavior.NeverUnderline;
                    this.newVersionLabel.ForeColor = this.labelCompanyName.ForeColor;
                }
                else
                {
                    this.newVersionLabel.Text = string.Empty;
                    this.newVersionLabel.Tag = null;
                }
            }
            catch (Exception ex)
            {
                ExceptionEvent.FireAndForget(ex);
                this.newVersionLabel.Text = string.Empty;
                this.newVersionLabel.Tag = null;
            }
        }

        public static bool? RunElevatedExeForFileAssociation(bool associate, out int? exitCode)
        {
            exitCode = null;
            Process proc = new();
            proc.StartInfo.FileName = _exePath;
            proc.StartInfo.UseShellExecute = true;
            proc.StartInfo.Verb = "runas";
            proc.StartInfo.Arguments = $"{PERFORM_FILE_ASSOCIATION} {associate}";

            try
            {
                proc.Start();
            }
            catch (System.ComponentModel.Win32Exception ex)
            {
                if (ex.NativeErrorCode == 1223)
                {
                    //user clicked 'No' to the UAC prompt
                    return null;
                }
                return false;
            }
            catch
            {
                return false;
            }

            if (proc.WaitForExit(TimeSpan.FromSeconds(6)))
            {
                exitCode = proc.ExitCode;
                return exitCode == 0;
            }
            else
            {
                return false;
            }
        }

        private void SetCheckboxSilent(bool @checked)
        {
            this.isLoading = true;
            this.associateFileExtensionCheckBox.Checked = @checked;
            this.isLoading = false;
        }

        public override void SetTheme(Theme theme)
        {
            if (DesignMode)
            {
                return;
            }

            base.SetTheme(theme);
            this.okButton.BackColor = Color.White;
            this.okButton.ForeColor = Color.Black;
        }
    }
}
