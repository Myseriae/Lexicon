using Lexicon.Data;
using Lexicon.DTOs;
using Lexicon.Model;
using Lexicon.Services;
using Lexicon.Services.Auth;
using Microsoft.Extensions.Logging;
using Moq;
using FluentAssertions;

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

        result.Summary.Should().Be("Cat summary");
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

        result.Summary.Should().Be("Already exists");
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

        result.Summary.Should().BeNullOrEmpty();
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

        result.Should().HaveCount(2);
        result.Should().Contain(a => a.Title == "Apple");
        result.Should().Contain(a => a.Title == "Pineapple");
    }

    [Test]
    public async Task DeleteArticleAsync_ReturnsFalse_WhenNotFound()
    {
        _articleRepoMock
            .Setup(d => d.GetArticleByIdAsync(99))
            .ReturnsAsync((Article?)null);

        var result = await _service.DeleteArticleAsync(99, "test-user", false);

        result.Should().BeFalse();
    }

    [Test]
    public async Task UpdateArticleAsync_ReturnsFalse_WhenNotFound()
    {
        var request = new UpdateArticleRequest { Title = "Test", Content = "Some content." };

        _articleRepoMock
            .Setup(d => d.GetArticleByIdAsync(99))
            .ReturnsAsync((Article?)null);

        var result = await _service.UpdateArticleAsync(99, request, "test-user", false);

        result.Should().BeFalse();
    }

    [Test]
    public async Task UpdateArticleAsync_CreatesRevisionFromPreviousSummaryAndContent_WhenTrackedFieldsChange()
    {
        var current = new Article
        {
            Id = 7,
            AuthorId = "author-id",
            Title = "Original title",
            Content = "Old content",
            Summary = "Old summary"
        };

        var request = new UpdateArticleRequest
        {
            Title = "Original title",
            Content = "New content",
            Summary = "New summary"
        };

        _articleRepoMock
            .Setup(d => d.GetArticleByIdAsync(current.Id))
            .ReturnsAsync(current);
        _revisionRepoMock
            .Setup(d => d.GetRevisionsAsync(current.Id))
            .ReturnsAsync(new List<Revision>());
        _articleRepoMock
            .Setup(d => d.UpdateArticleAsync(current.Id, It.IsAny<Article>()))
            .ReturnsAsync(true);

        var result = await _service.UpdateArticleAsync(current.Id, request, "author-id", false);

        result.Should().BeTrue();
        _revisionRepoMock.Verify(d => d.AddRevisionAsync(It.Is<Revision>(revision =>
            revision.ArticleId == current.Id &&
            revision.Content == current.Content &&
            revision.Summary == current.Summary &&
            revision.VersionNumber == 1
        )), Times.Once);
    }

    [Test]
    public async Task UpdateArticleAsync_DoesNotCreateRevision_WhenOnlyTitleChanges()
    {
        var current = new Article
        {
            Id = 8,
            AuthorId = "author-id",
            Title = "Original title",
            Content = "Same content",
            Summary = "Same summary"
        };

        var request = new UpdateArticleRequest
        {
            Title = "New title",
            Content = current.Content,
            Summary = current.Summary
        };

        _articleRepoMock
            .Setup(d => d.GetArticleByIdAsync(current.Id))
            .ReturnsAsync(current);
        _articleRepoMock
            .Setup(d => d.UpdateArticleAsync(current.Id, It.IsAny<Article>()))
            .ReturnsAsync(true);

        var result = await _service.UpdateArticleAsync(current.Id, request, "author-id", false);

        result.Should().BeTrue();
        _revisionRepoMock.Verify(d => d.AddRevisionAsync(It.IsAny<Revision>()), Times.Never);
    }

    [Test]
    public async Task RollbackArticleAsync_SavesCurrentArticleAsRevision_AndRestoresSelectedRevision()
    {
        var current = new Article
        {
            Id = 9,
            AuthorId = "author-id",
            Title = "Rollback title",
            Content = "Current content",
            Summary = "Current summary"
        };
        var revision = new Revision
        {
            Id = 3,
            ArticleId = current.Id,
            Content = "Previous content",
            Summary = "Previous summary",
            VersionNumber = 1
        };

        _articleRepoMock
            .Setup(d => d.GetArticleByIdAsync(current.Id))
            .ReturnsAsync(current);
        _revisionRepoMock
            .Setup(d => d.GetRevisionAsync(current.Id, revision.Id))
            .ReturnsAsync(revision);
        _revisionRepoMock
            .Setup(d => d.GetRevisionsAsync(current.Id))
            .ReturnsAsync(new List<Revision> { revision });
        _articleRepoMock
            .Setup(d => d.UpdateArticleAsync(current.Id, It.IsAny<Article>()))
            .ReturnsAsync(true);

        var result = await _service.RollbackArticleAsync(current.Id, revision.Id, "author-id", false);

        result.Should().BeTrue();
        _revisionRepoMock.Verify(d => d.AddRevisionAsync(It.Is<Revision>(saved =>
            saved.Content == current.Content &&
            saved.Summary == current.Summary &&
            saved.VersionNumber == 2
        )), Times.Once);
        _articleRepoMock.Verify(d => d.UpdateArticleAsync(current.Id, It.Is<Article>(updated =>
            updated.Title == current.Title &&
            updated.Content == revision.Content &&
            updated.Summary == revision.Summary
        )), Times.Once);
    }
}
