using Microsoft.Win32;
using ParquetViewer.Analytics;
using ParquetViewer.Exceptions;
using System;
using System.Net.Http;
using System.Reflection;
using System.Security.Principal;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;

namespace ParquetViewer.Helpers
{
    public static class Constants
    {
        public const string WikiURL = "https://github.com/mukunku/ParquetViewer/wiki";
    }

    public static class User
    {
        public static bool IsAdministrator =>
            new WindowsPrincipal(WindowsIdentity.GetCurrent())
                .IsInRole(WindowsBuiltInRole.Administrator);

        public static char NumberDecimalSeparator =
            Thread.CurrentThread.CurrentUICulture.NumberFormat.NumberDecimalSeparator.ToCharArray()[0];
    }

    public static class Env
    {
        #region Assembly Version
        private static SemanticVersion? _assemblyVersion = null;
        public static SemanticVersion AssemblyVersion => _assemblyVersion ??= ParseAssemblyVersion();

        private static SemanticVersion ParseAssemblyVersion()
        {
            var assemblyVersionString = Assembly.GetExecutingAssembly().GetName().Version?.ToString() ?? string.Empty;

            if (!SemanticVersion.TryParse(assemblyVersionString, out var assemblyVersion))
                UnsupportedAssemblyVersionException.Record(assemblyVersionString);

            return assemblyVersion;
        }
        #endregion

        #region Latest Release Version
        private const string RELEASES_API_URL = "https://api.github.com/repos/mukunku/ParquetViewer/releases";
        private static SemanticVersion? _latestReleaseVersion = null;
        private static Uri? _releaseUri = null;
        private static DateTime? _latestReleaseLastCheckedOn = null;
        public static async Task<(SemanticVersion? Version, Uri? Url)> FetchLatestRelease()
        {
            //We cache the http response for 30 minutes (even if it failed) so we don't spam http requests
            if (_latestReleaseLastCheckedOn.HasValue && DateTime.UtcNow.Subtract(_latestReleaseLastCheckedOn.Value) < TimeSpan.FromMinutes(30))
            {
                return (_latestReleaseVersion, _releaseUri);
            }

            //Clear previously cached values
            _latestReleaseVersion = null;
            _releaseUri = null;

            try
            {

                _latestReleaseLastCheckedOn = DateTime.UtcNow;

                using var httpClient = new HttpClient();
                httpClient.DefaultRequestHeaders.Add("User-Agent", "Parquet Viewer"); //GitHub requires a user agent
                using var response = await httpClient.GetAsync(RELEASES_API_URL);
                if (response.IsSuccessStatusCode)
                {
                    using var responseJson = JsonDocument.Parse(response.Content.ReadAsStream());
                    foreach (var release in responseJson.RootElement.EnumerateArray())
                    {
                        Uri? releaseUri = null;
                        if (release.TryGetProperty("html_url", out var releaseUrl)
                            && releaseUrl.GetString() is string url)
                        {
                            releaseUri = new Uri(url);
                        }

                        if (release.TryGetProperty("tag_name", out var releaseTag)
                            && releaseTag.GetString() is string tag
                            && SemanticVersion.TryParse(tag, out var semanticVersion))
                        {
                            _releaseUri = releaseUri;
                            _latestReleaseVersion = semanticVersion;
                            return (semanticVersion, releaseUri);
                        }

                        //We only care about the latest release so stop processing now
                        break;
                    }
                }
            }
            catch (Exception ex)
            {
                ExceptionEvent.FireAndForget(ex);
            }

            return (null, null);
        }
        #endregion

        #region Windows Theme
        /// <summary>
        /// Gets a value indicating whether the current Windows theme for apps is set to dark mode.
        /// `null` is returned if the value cannot be determined.
        /// </summary>
        public static bool? AppsUseDarkTheme
        {
            get
            {
                try
                {
                    if (Registry.GetValue("HKEY_CURRENT_USER\\SOFTWARE\\Microsoft\\Windows\\CurrentVersion\\Themes\\Personalize", "AppsUseLightTheme", -1)
                        is int appsUseLightTheme)
                    {
                        return appsUseLightTheme switch
                        {
                            0 => true, //Dark
                            1 => false, //Light
                            _ => null //Unknown
                        };
                    }
                }
                catch { /*Swallow*/ }

                return null;
            }
        }
        /// <summary>
        /// Gets a value indicating whether the current Windows theme for the system is set to dark mode.
        /// `null` is returned if the value cannot be determined.
        /// </summary>
        public static bool? SystemUsesDarkTheme
        {
            get
            {
                try
                {
                    if (Registry.GetValue("HKEY_CURRENT_USER\\SOFTWARE\\Microsoft\\Windows\\CurrentVersion\\Themes\\Personalize", "SystemUsesLightTheme", -1)
                        is int systemUsesLightTheme)
                    {
                        return systemUsesLightTheme switch
                        {
                            0 => true, //Dark
                            1 => false, //Light
                            _ => null //Unknown
                        };
                    }
                }
                catch { /*Swallow*/ }

                return null;
            }
        }
        #endregion
    }

    public enum DateFormat
    {
        Default = 0,
        ISO8601 = 2,
        //1, 3, 4, and 5 have been discontinued. Should not be reused!
        Custom = 6,
    }

    public enum FileType
    {
        CSV = 0,
        XLS,
        JSON,
        PARQUET,
        XLSX,
    }
}