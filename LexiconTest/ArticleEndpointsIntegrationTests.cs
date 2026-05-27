using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using Lexicon.DTOs;
using FluentAssertions;

namespace LexiconTest;

[TestFixture]
public class ArticleEndpointsIntegrationTests
{
    private TestWebApplicationFactory _factory = null!;
    private HttpClient _client = null!;

    [SetUp]
    public void SetUp()
    {
        _factory = new TestWebApplicationFactory();
        _client = _factory.CreateClient();
    }

    [TearDown]
    public void TearDown()
    {
        _client?.Dispose();
        _factory?.Dispose();
    }

    [Test]
    public async Task Register_CreateArticle_AndGetArticle_ReturnsPersistedArticle()
    {
        var registerRequest = new RegisterRequest(
            Email: "integration@test.com",
            UserName: "integration-user",
            Password: "secret1");

        var registerResponse = await _client.PostAsJsonAsync("/api/auth/register", registerRequest);

        registerResponse.StatusCode.Should().Be(HttpStatusCode.Created);

        var authPayload = await registerResponse.Content.ReadFromJsonAsync<AuthTokenResponse>();
        authPayload.Should().NotBeNull();
        authPayload!.AccessToken.Should().NotBeNullOrWhiteSpace();
        authPayload.UserName.Should().Be(registerRequest.UserName);
        authPayload.Role.Should().Be("Editor");

        _client.DefaultRequestHeaders.Authorization =
            new AuthenticationHeaderValue("Bearer", authPayload.AccessToken);

        var createRequest = new CreateArticleRequest
        {
            Title = "Integration testing with SQLite",
            Content = "This article was created through the full API pipeline.",
            Summary = "A persisted test article."
        };

        var createResponse = await _client.PostAsJsonAsync("/api/articles", createRequest);

        createResponse.StatusCode.Should().Be(HttpStatusCode.Created);

        var createdArticle = await createResponse.Content.ReadFromJsonAsync<ArticleResponse>();
        createdArticle.Should().NotBeNull();
        createdArticle!.Id.Should().BeGreaterThan(0);
        createdArticle.Title.Should().Be(createRequest.Title);
        createdArticle.Content.Should().Be(createRequest.Content);
        createdArticle.Summary.Should().Be(createRequest.Summary);
        createdArticle.AuthorUsername.Should().Be(registerRequest.UserName);

        var getResponse = await _client.GetAsync($"/api/articles/{createdArticle.Id}");

        getResponse.StatusCode.Should().Be(HttpStatusCode.OK);

        var fetchedArticle = await DeserializeAsync<ArticleResponse>(getResponse);
        fetchedArticle.Id.Should().Be(createdArticle.Id);
        fetchedArticle.Title.Should().Be(createRequest.Title);
        fetchedArticle.Content.Should().Be(createRequest.Content);
        fetchedArticle.Summary.Should().Be(createRequest.Summary);
        fetchedArticle.AuthorUsername.Should().Be(registerRequest.UserName);
        fetchedArticle.AuthorId.Should().Be(createdArticle.AuthorId);
        fetchedArticle.CollaboratorIds.Should().BeEmpty();
    }

    private static async Task<T> DeserializeAsync<T>(HttpResponseMessage response)
    {
        await using var stream = await response.Content.ReadAsStreamAsync();
        var payload = await JsonSerializer.DeserializeAsync<T>(stream, new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        });

        payload.Should().NotBeNull();
        return payload!;
    }
}
