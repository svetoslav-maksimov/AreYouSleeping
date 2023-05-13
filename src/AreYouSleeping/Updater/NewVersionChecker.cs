using Microsoft.Extensions.Logging;
using System;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Reflection;
using System.Threading.Tasks;


namespace AreYouSleeping.Updater
{
    public class NewVersionChecker
    {
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly ILogger<NewVersionChecker> _logger;

        public NewVersionChecker(IHttpClientFactory httpClientFactory, ILogger<NewVersionChecker> logger)
        {
            _httpClientFactory = httpClientFactory;
            _logger = logger;
        }

        public async Task<ReleaseDto?> CheckForNewVersions()
        {
            try
            {
                var client = _httpClientFactory.CreateClient();
                client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/vnd.github+json"));
                client.DefaultRequestHeaders.Add("X-GitHub-Api-Version", "2022-11-28");
                client.DefaultRequestHeaders.Add("User-Agent", "request");

                const string url = "https://api.github.com/repos/svetoslav-maksimov/AreYouSleeping/releases/latest";
                var result = await client.GetFromJsonAsync<ReleaseDto>(url);

                _logger.LogDebug("Checked for new versions");

                if (result != null)
                {
                    var currentVersion = GetCurrentVersion();
                    if (Version.TryParse(result.Tag_name, out Version? latestVersion))
                    {
                        _logger.LogDebug($"Latest version is: {result.Tag_name}, current version is {currentVersion}");
                        if (currentVersion < latestVersion)
                        {
                            // only return the version result when a newer version is available
                            return result;
                        }
                    }
                    else
                    {
                        _logger.LogWarning($"Could not parse version number from tag {result.Tag_name}");
                    }
                }
            }
            catch (Exception exc)
            {
                _logger.LogError(exc, "Error while checking for new versions");
            }
            return null;
        }

        private Version GetCurrentVersion()
        {
            return Assembly.GetExecutingAssembly().GetName().Version!;
        }
    }
}
