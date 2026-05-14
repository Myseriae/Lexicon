using Lexicon.Data;
using Lexicon.DTOs;
using Lexicon.Model;
using Lexicon.Services;
using Lexicon.Services.Auth;
using Microsoft.Extensions.Logging;
using Moq;

namespace LexiconTest;

[TestFixture]
public class ArticleServiceTests
{
    private Mock<IArticleRepository> _articleRepoMock;
    private Mock<IRevisionRepository> _revisionRepoMock;
    private Mock<IWikipediaService> _wikipediaMock;
    private Mock<IAuthService> _authServiceMock;
    private Mock<ILogger<ArticleService>> _loggerMock;
    private ArticleService _service;

    [SetUp]
    public void Setup()
    {
        _articleRepoMock = new Mock<IArticleRepository>();
        _revisionRepoMock = new Mock<IRevisionRepository>();
        _wikipediaMock = new Mock<IWikipediaService>();
        _authServiceMock = new Mock<IAuthService>();
        _loggerMock = new Mock<ILogger<ArticleService>>();
        _authServiceMock
            .Setup(a => a.GetUsernameByIdAsync(It.IsAny<string>()))
            .ReturnsAsync((string id) => id);
        _service = new ArticleService(_articleRepoMock.Object, _revisionRepoMock.Object, _wikipediaMock.Object, _authServiceMock.Object, _loggerMock.Object);
    }

    [Test]
    public async Task AddArticleAsync_CallsWikipedia_WhenSummaryIsMissing()
    {
        var request = new CreateArticleRequest { Title = "Cat", Content = "Info about cats.", Summary = "" };

        _wikipediaMock
            .Setup(w => w.GetSummaryAsync("Cat"))
            .ReturnsAsync("Cat summary");

        _articleRepoMock
            .Setup(d => d.AddArticleAsync(It.IsAny<Article>()))
            .ReturnsAsync((Article a) => a);

        var result = await _service.AddArticleAsync(request, "test-user");

        Assert.That(result.Summary, Is.EqualTo("Cat summary"));
        _wikipediaMock.Verify(w => w.GetSummaryAsync("Cat"), Times.Once);
        _articleRepoMock.Verify(d => d.AddArticleAsync(It.IsAny<Article>()), Times.Once);
    }

    [Test]
    public async Task AddArticleAsync_SkipsWikipedia_WhenSummaryExists()
    {
        var request = new CreateArticleRequest { Title = "Dog", Content = "Info about dogs.", Summary = "Already exists" };

        _articleRepoMock
            .Setup(d => d.AddArticleAsync(It.IsAny<Article>()))
            .ReturnsAsync((Article a) => a);

        var result = await _service.AddArticleAsync(request, "test-user");

        Assert.That(result.Summary, Is.EqualTo("Already exists"));
        _wikipediaMock.Verify(w => w.GetSummaryAsync(It.IsAny<string>()), Times.Never);
    }

    [Test]
    public async Task AddArticleAsync_SavesArticle_WhenWikipediaReturnsNull()
    {
        var request = new CreateArticleRequest { Title = "Unknown", Content = "Some content.", Summary = "" };

        _wikipediaMock
            .Setup(w => w.GetSummaryAsync("Unknown"))
            .ReturnsAsync((string?)null);

        _articleRepoMock
            .Setup(d => d.AddArticleAsync(It.IsAny<Article>()))
            .ReturnsAsync((Article a) => a);

        var result = await _service.AddArticleAsync(request, "test-user");

        Assert.That(result.Summary, Is.Null.Or.Empty);
        _articleRepoMock.Verify(d => d.AddArticleAsync(It.IsAny<Article>()), Times.Once);
    }

    [Test]
    public async Task SearchAsync_FiltersCorrectly_AndCaseInsensitive()
    {
        var articles = new List<Article>
        {
            new Article { Title = "Apple", Tags = new List<Tag>() },
            new Article { Title = "Banana", Tags = new List<Tag>() },
            new Article { Title = "Pineapple", Tags = new List<Tag>() }
        };

        _articleRepoMock
            .Setup(d => d.SearchArticlesAsync("apple"))
            .ReturnsAsync(articles.Where(a => a.Title.ToLower().Contains("apple")).ToList());

        var result = (await _service.SearchAsync("apple")).ToList();

        Assert.That(result.Count, Is.EqualTo(2));
        Assert.That(result.Any(a => a.Title == "Apple"), Is.True);
        Assert.That(result.Any(a => a.Title == "Pineapple"), Is.True);
    }

    [Test]
    public async Task DeleteArticleAsync_ReturnsFalse_WhenNotFound()
    {
        _articleRepoMock
            .Setup(d => d.GetArticleByIdAsync(99))
            .ReturnsAsync((Article?)null);

        var result = await _service.DeleteArticleAsync(99, "test-user", false);

        Assert.That(result, Is.False);
    }

    [Test]
    public async Task UpdateArticleAsync_ReturnsFalse_WhenNotFound()
    {
        var request = new UpdateArticleRequest { Title = "Test", Content = "Some content." };

        _articleRepoMock
            .Setup(d => d.GetArticleByIdAsync(99))
            .ReturnsAsync((Article?)null);

        var result = await _service.UpdateArticleAsync(99, request, "test-user", false);

        Assert.That(result, Is.False);
    }
}
