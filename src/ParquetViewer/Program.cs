using ParquetViewer.Analytics;
using ParquetViewer.Helpers;
using System;
using System.Globalization;
using System.IO;
using System.Windows.Forms;

namespace ParquetViewer
{
    static class Program
    {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        private static int Main(string[] args)
        {
            if (IsFileAssociationMode(args))
            {
                return AttemptFileAssociation(args);
            }

            //Set language
            if (AppSettings.UserSelectedCulture is not null)
            {
                CultureInfo.CurrentUICulture = AppSettings.UserSelectedCulture;
            }

            //Enable HighDpi mode
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);

            //Prepare main form
            string? pathToOpen = GetPathToOpen(args);
            var mainForm = new MainForm(pathToOpen); //Form must be created after calling SetCompatibleTextRenderingDefault();
            AppSettings.DarkMode = AppSettings.DarkMode; // Trigger Theming

            RouteUnhandledExceptions();

            Application.Run(mainForm);
            return 0;
        }

        private static bool IsFileAssociationMode(string[] args)
            => args?.Length > 0 && AboutBox.PERFORM_FILE_ASSOCIATION.Equals(args[0]);

        private static int AttemptFileAssociation(string[] args)
        {
            try
            {
                if (args.Length > 1 && bool.TryParse(args[1], out bool associate))
                {
                    try
                    {
                        AboutBox.ToggleFileAssociation(associate);
                        return 0;
                    }
                    catch
                    {
                        return 1;
                    }
                }
                else
                {
                    return 2; //no true/false flag passed
                }
            }
            catch (Exception)
            {
                return 3;
            }
        }

        private static string? GetPathToOpen(string[] args)
        {
            if (args is null || args.Length == 0)
                return null;
            else if (File.Exists(args[0]))
                return args[0];
            else if (Directory.Exists(args[0]))
                return args[0];
            else
                return null;
        }

        /// <summary>
        /// When called, all unhandled exceptions within the runtime and winforms UI thread
        /// will be routed to the <see cref="ExceptionHandler"/> handler. 
        /// </summary>
        /// <remarks>Side effect: The application will never quit when an unhandled exception happens.</remarks>
        private static void RouteUnhandledExceptions()
        {
            //If we're not debugging, route all unhandled exceptions to our top level exception handler
            if (!System.Diagnostics.Debugger.IsAttached)
            {
                // Add the event handler for handling non-UI thread exceptions to the event. 
                AppDomain.CurrentDomain.UnhandledException += new((sender, e) => ExceptionHandler((Exception)e.ExceptionObject));

                // Add the event handler for handling UI thread exceptions to the event.
                // Warning: Winforms only surfaces the inner-most exception to this handler.
                Application.ThreadException += new((sender, e) => ExceptionHandler(e.Exception));
            }
        }

        private static void ExceptionHandler(Exception ex)
        {
            ExceptionEvent.FireAndForget(ex);
            MessageBox.Show($"{Resources.Errors.GenericErrorMessage} {Resources.Errors.CopyErrorMessageText}:{Environment.NewLine}{Environment.NewLine}{ex}", 
                ex.Message, MessageBoxButtons.OK, MessageBoxIcon.Error);
        }

        /// <summary>
        /// We only ask for consent if the user launched the app at least twice, 1 day apart.
        /// </summary>
        /// <remarks>
        /// TODO: Should we postpone asking analytics consent if this is also if we're also asking for .parquet file association?
        ///   It would be annoying if we ask to become the user's default parquet viewer then also ask if we can gather
        ///   analytics back-to-back. But chances of that happening are slim so maybe we don't need to worry about it?
        /// </remarks>
        public static void GetUserConsentToGatherAnalytics()
        {
            if (AppSettings.ConsentLastAskedOnVersion is null || AppSettings.ConsentLastAskedOnVersion < Env.AssemblyVersion)
            {
                if (AppSettings.AnalyticsDataGatheringConsent)
                {
                    //Keep user's consent asked version up to date with the current assembly version
                    AppSettings.ConsentLastAskedOnVersion = Env.AssemblyVersion;
                    return;
                }

                bool isFirstLaunch = AppSettings.ConsentLastAskedOnVersion is null;
                if (isFirstLaunch)
                {
                    //Don't ask for consent on the first launch. Record the day of the month instead so we can ask tomorrow. 
                    AppSettings.ConsentLastAskedOnVersion = new SemanticVersion(0, 0, 0, DateTime.Now.Day);
                }
                else if (AppSettings.ConsentLastAskedOnVersion != new SemanticVersion(0, 0, 0, DateTime.Now.Day))
                {
                    AppSettings.ConsentLastAskedOnVersion = Env.AssemblyVersion;
                    if (MessageBox.Show(
                        Resources.Strings.AnalyticsConsentPromptMessage,
                        Resources.Strings.AnalyticsConsentPromptTitle,
                        MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes)
                    {
                        //We got consent! Start gathering some data..
                        AppSettings.AnalyticsDataGatheringConsent = true;
                    }
                }
            }
        }

        /// <summary>
        /// We only ask for file extension association if the user opened at least 8 parquet files
        /// </summary>
        /// <remarks>
        /// We want to make sure we're not annoying the user and that the user is committed to
        /// using ParquetViewer. The user opening 8 files seemed like a decent benchmark for that.
        /// </remarks>
        public static void AskUserForFileExtensionAssociation()
        {
            if (AppSettings.OpenedFileCount == 8 && !AboutBox.IsDefaultViewerForParquetFiles)
            {
                if (MessageBox.Show(
                        Resources.Strings.FileExtensionAssociationPromptMessageFormat.Format(Application.ExecutablePath),
                        Resources.Strings.FileExtensionAssociationPromptTitle,
                        MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes)
                {
                    if (!User.IsAdministrator)
                    {
                        bool? success = AboutBox.RunElevatedExeForFileAssociation(true, out int? exitCode);
                        if (success is null)
                        {
                            MessageBox.Show(
                                Resources.Strings.FileExtensionAssociationCancelledMessage,
                                Resources.Strings.FileExtensionAssociationCancelledTitle,
                                MessageBoxButtons.OK, MessageBoxIcon.Warning);
                        }
                        else if (success == false)
                        {
                            MessageBox.Show(
                                Resources.Strings.FileExtensionAssociationFailedMessageFormat.Format(exitCode),
                                Resources.Strings.FileExtensionAssociationFailedTitle,
                                MessageBoxButtons.OK, MessageBoxIcon.Error);
                        }
                        else
                        {
                            MessageBox.Show(
                                Resources.Strings.FileExtensionAssociationSucceededMessage,
                                Resources.Strings.FileExtensionAssociationSucceededTitle,
                                MessageBoxButtons.OK, MessageBoxIcon.Information);
                        }
                    }
                    else
                    {
                        try
                        {
                            AboutBox.ToggleFileAssociation(true);
                            MessageBox.Show(
                                Resources.Strings.FileExtensionAssociationSucceededMessage,
                                Resources.Strings.FileExtensionAssociationSucceededTitle,
                                MessageBoxButtons.OK, MessageBoxIcon.Information);
                        }
                        catch (Exception ex)
                        {
                            MessageBox.Show(
                                Resources.Strings.FileExtensionAssociationFailedMessageFormat.Format(ex.Message),
                                Resources.Strings.FileExtensionAssociationFailedTitle,
                                MessageBoxButtons.OK, MessageBoxIcon.Error);
                        }
                    }
                }
            }
        }

        public static void AskUserIfTheyWantToSwitchToDarkMode()
        {
            if (!AppSettings.DarkMode
                && (AppSettings.OpenedFileCount == 30 || AppSettings.OpenedFileCount == 300) /*I'm just throwing out random numbers at this point*/
                && (Env.AppsUseDarkTheme == true || Env.SystemUsesDarkTheme == true))
            {
                if (MessageBox.Show(
                    Resources.Strings.DarkModePromptMessage,
                    Resources.Strings.DarkModePromptTitle,
                    MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes)
                {
                    AppSettings.DarkMode = true;
                }
            }
        }
    }
}
