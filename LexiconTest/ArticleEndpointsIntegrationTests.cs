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
    private static TestWebApplicationFactory _factory = null!;
    private HttpClient _client = null!;

    [OneTimeSetUp]
    public void OneTimeSetUp()
    {
        // Create the factory once for all tests in this class
        _factory = new TestWebApplicationFactory();
    }

    [SetUp]
    public async Task SetUpAsync()
    {
        // Reset the database before each test to ensure isolation
        await _factory.ResetDatabaseAsync();
        
        _client = _factory.CreateClient();
    }

    [TearDown]
    public void TearDown()
    {
        _client?.Dispose();
    }

    [OneTimeTearDown]
    public void OneTimeTearDown()
    {
        // Dispose the factory once after all tests
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
    
    
    [Test]
    public async Task SearchArticles_ReturnsMatchingArticle()
    {
        var registerRequest = new RegisterRequest(
            Email: "search@test.com",
            UserName: "search-user",
            Password: "secret1");

        var registerResponse =
            await _client.PostAsJsonAsync("/api/auth/register", registerRequest);

        // Ensure registration succeeded and payload is present before using the token
        registerResponse.StatusCode.Should().Be(HttpStatusCode.Created);

        var authPayload =
            await registerResponse.Content.ReadFromJsonAsync<AuthTokenResponse>();

        authPayload.Should().NotBeNull();
        authPayload!.AccessToken.Should().NotBeNullOrWhiteSpace();
        authPayload.UserName.Should().Be(registerRequest.UserName);
        authPayload.Role.Should().Be("Editor");

        _client.DefaultRequestHeaders.Authorization =
            new AuthenticationHeaderValue("Bearer", authPayload.AccessToken);

        var createRequest = new CreateArticleRequest
        {
            Title = "CSharp Basics",
            Content = "Learning ASP.NET Core",
            Summary = "Search test"
        };

        await _client.PostAsJsonAsync("/api/articles", createRequest);

        var searchResponse =
            await _client.GetAsync("/api/articles/search?query=CSharp");

        searchResponse.StatusCode.Should().Be(HttpStatusCode.OK);

        var articles =
            await searchResponse.Content.ReadFromJsonAsync<List<ArticleResponse>>();

        articles.Should().NotBeNull();

        articles!.Should().Contain(a =>
            a.Title == "CSharp Basics");
    }
}
