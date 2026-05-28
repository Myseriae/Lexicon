using Lexicon.Services;
using Microsoft.Extensions.Logging.Abstractions;
using NUnit.Framework;
using System.Net;

namespace LexiconTest;

[TestFixture]
public class WikipediaServiceTests
{
    [Test]
    public async Task ReturnsNull_OnFailedStatusCode()
    {
        var client = CreateClient(HttpStatusCode.NotFound, "{}");
        var service = new WikipediaService(client, NullLogger<WikipediaService>.Instance);

        var result = await service.GetSummaryAsync("Test");

        Assert.That(result, Is.Null);
    }

    [Test]
    public async Task ReturnsNull_OnDisambiguation()
    {
        var json = """
        {
            "extract": "Something",
            "type": "disambiguation"
        }
        """;

        var client = CreateClient(HttpStatusCode.OK, json);
        var service = new WikipediaService(client, NullLogger<WikipediaService>.Instance);

        var result = await service.GetSummaryAsync("Mercury");

        Assert.That(result, Is.Null);
    }

    [Test]
    public async Task ReturnsNull_OnException()
    {
        var client = new HttpClient(new ThrowingHandler());
        var service = new WikipediaService(client, NullLogger<WikipediaService>.Instance);

        var result = await service.GetSummaryAsync("Cat");

        Assert.That(result, Is.Null);
    }

    [Test]
    public async Task ReturnsExtract_WhenValid()
    {
        var json = """
        {
            "extract": "Cat is an animal",
            "type": "standard"
        }
        """;

        var client = CreateClient(HttpStatusCode.OK, json);
        var service = new WikipediaService(client, NullLogger<WikipediaService>.Instance);

        var result = await service.GetSummaryAsync("Cat");

        Assert.That(result, Is.EqualTo("Cat is an animal\nSummaries may not be correct as they are received from outside sources."));
    }

    private HttpClient CreateClient(HttpStatusCode statusCode, string content)
    {
        return new HttpClient(new FakeHandler(statusCode, content));
    }

    private class FakeHandler : HttpMessageHandler
    {
        private readonly HttpStatusCode _status;
        private readonly string _content;

        public FakeHandler(HttpStatusCode status, string content)
        {
            _status = status;
            _content = content;
        }

        protected override Task<HttpResponseMessage> SendAsync(
            HttpRequestMessage request,
            CancellationToken cancellationToken)
        {
            return Task.FromResult(new HttpResponseMessage(_status)
            {
                Content = new StringContent(_content)
            });
        }
    }

    private class ThrowingHandler : HttpMessageHandler
    {
        protected override Task<HttpResponseMessage> SendAsync(
            HttpRequestMessage request,
            CancellationToken cancellationToken)
        {
            throw new HttpRequestException("boom");
        }
    }
}
