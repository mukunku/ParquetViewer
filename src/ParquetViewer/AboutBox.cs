using DotnetFileAssociator;
using ParquetViewer.Analytics;
using ParquetViewer.Controls;
using ParquetViewer.Helpers;
using System;
using System.Diagnostics;
using System.Drawing;
using System.Reflection;
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

        private static string _exePath => Application.ExecutablePath;

        private static FileAssociator _fileAssociator;

        public static bool IsDefaultViewerForParquetFiles => _fileAssociator.IsFileAssociationSet(new(".parquet"));

        static AboutBox()
        {
            _fileAssociator = new FileAssociator(_exePath);
            _fileAssociator.OverideProgramId("ParquetViewer"); //We used this program id before switching to this library
        }

        public AboutBox()
        {
            InitializeComponent();
            this.Text = this.Text.Format(AssemblyTitle);
            this.labelProductName.Text = AssemblyProduct;
            this.labelVersion.Text = this.labelVersion.Text.Format(AssemblyVersion, _isSelfContainedExe ? " SC" : string.Empty);
            this.labelCopyright.Text = AssemblyCopyright;
            this.textBoxDescription.Text = AssemblyDescription.Replace($"Privacy policy:", $"{Resources.Strings.PrivacyPolicyLabelText}:"); //HACK: to translate privacy policy text
            this.newVersionLabel.Image = null;

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
        #endregion

        private void AboutBox_KeyUp(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Escape)
            {
                this.Close();
            }
        }

        /// <summary>
        /// Toggles .parquet file association for the current executable. Requires the app to be running with administrator rights.
        /// </summary>
        /// <param name="associate">Whether to associate or disassociate ParquetViewer as the default app for .parquet files</param>

        public static void ToggleFileAssociation(bool associate)
        {
            if (!User.IsAdministrator)
            {
                return;
            }

            if (associate)
                _fileAssociator.SetFileAssociation(new(".parquet", "Apache Parquet File"));
            else
                _fileAssociator.RemoveFileAssociation(new(".parquet"));
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
                    MessageBox.Show(this, 
                        Resources.Errors.FileAssociationFailedErrorMessageFormat.Format(exitCode), 
                        Resources.Errors.FileAssociationFailedErrorTitle, 
                        MessageBoxButtons.OK, MessageBoxIcon.Error);
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
                this.newVersionLabel.Visible = false;
                var latestRelease = await Env.FetchLatestRelease();
                this.newVersionLabel.Text = this.newVersionLabel.Text.Format(latestRelease.Version);
                this.newVersionLabel.Visible = true;

                if (latestRelease.Version > Env.AssemblyVersion)
                {
                    this.newVersionLabel.Tag = latestRelease.Url;
                    this.newVersionLabel.Image = Resources.Icons.external_link_icon;
                }
                else if (latestRelease.Version == Env.AssemblyVersion)
                {   
                    this.newVersionLabel.Enabled = false;
                }
            }
            catch (Exception ex)
            {
                ExceptionEvent.FireAndForget(ex);
                this.newVersionLabel.Text = string.Empty;
                this.newVersionLabel.Tag = null;
                this.newVersionLabel.Enabled = false;
            }
        }

        /// <summary>
        /// Toggles .parquet file association for the current executable
        /// </summary>
        /// <param name="associate">Whether to associate or disassociate ParquetViewer as the default app for .parquet files</param>
        /// <param name="exitCode">Exit code if the elevated exe was successfully launched</param>
        /// <returns>True if association succeeded, False if it failed, and Null if the user canceled the operation</returns>
        public static bool? RunElevatedExeForFileAssociation(bool associate, out int? exitCode)
        {
            exitCode = null;
            using Process proc = new();
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

        private void newVersionLabel_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            if (this.newVersionLabel.Tag is Uri url)
                Process.Start(new ProcessStartInfo(url.ToString()) { UseShellExecute = true });
        }
    }
}
