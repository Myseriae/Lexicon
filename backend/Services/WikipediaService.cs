using System.Net.Http.Json;
using System.Text.Json;

namespace Lexicon.Services;

public class WikipediaService : IWikipediaService
{
    private readonly HttpClient _httpClient;

    public WikipediaService(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    public async Task<string?> GetSummaryAsync(string title)
    {
        var url =
            $"https://en.wikipedia.org/api/rest_v1/page/summary/{Uri.EscapeDataString(title)}";

        Console.WriteLine($"Wikipedia URL: {url}");

        try
        {
            var response = await _httpClient.GetAsync(url);

            Console.WriteLine($"Status code: {response.StatusCode}");

            var body = await response.Content.ReadAsStringAsync();
            Console.WriteLine($"Response body: {body}");

            if (!response.IsSuccessStatusCode)
                return null;

            var data = JsonSerializer.Deserialize<WikipediaResponse>(
                body,
                new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                });

            Console.WriteLine($"Extract: {data?.Extract}");

            if (data?.Type == "disambiguation")
                return null;

            if (string.IsNullOrEmpty(data?.Extract))
                return data?.Extract;

            return $"{data.Extract}\nSummaries may not be correct as they are received from outside sources.";
        }
        catch (Exception ex)
        {
            Console.WriteLine($"ERROR: {ex.Message}");
            return null;
        }
    }

    private class WikipediaResponse
    {
        public string? Extract { get; set; }
        public string? Type { get; set; }
    }
}