using System.Net.Http.Json;
using System.Text.Json;
using Microsoft.Extensions.Logging;

namespace Lexicon.Services;

public class WikipediaService : IWikipediaService
{
    private readonly HttpClient _httpClient;
    private readonly ILogger<WikipediaService> _logger;

    public WikipediaService(HttpClient httpClient, ILogger<WikipediaService> logger)
    {
        _httpClient = httpClient;
        _logger = logger;
    }

    public async Task<string?> GetSummaryAsync(string title)
    {
        var url =
            $"https://en.wikipedia.org/api/rest_v1/page/summary/{Uri.EscapeDataString(title)}";

        _logger.LogInformation("Wikipedia URL: {Url}", url);

        try
        {
            var response = await _httpClient.GetAsync(url);

            _logger.LogInformation("Wikipedia status code: {StatusCode}", response.StatusCode);

            var body = await response.Content.ReadAsStringAsync();
            _logger.LogInformation("Wikipedia response body: {Body}", body);

            if (!response.IsSuccessStatusCode)
                return null;

            var data = JsonSerializer.Deserialize<WikipediaResponse>(
                body,
                new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                });

            _logger.LogInformation("Wikipedia extract: {Extract}", data?.Extract);

            if (data?.Type == "disambiguation")
                return null;

            if (string.IsNullOrEmpty(data?.Extract))
                return data?.Extract;

            return $"{data.Extract}\nSummaries may not be correct as they are received from outside sources.";
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Wikipedia summary lookup failed for {Title}.", title);
            return null;
        }
    }

    private class WikipediaResponse
    {
        public string? Extract { get; set; }
        public string? Type { get; set; }
    }
}
