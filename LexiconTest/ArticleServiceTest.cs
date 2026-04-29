using Lexicon.Data;
using Lexicon.DTOs;
using Lexicon.Model;
using Lexicon.Services;
using Microsoft.Extensions.Logging;
using Moq;

namespace LexiconTest;

[TestFixture]
public class ArticleServiceTests
{
    private Mock<IDataHandler> _dataHandlerMock;
    private Mock<IWikipediaService> _wikipediaMock;
    private Mock<ILogger<ArticleService>> _loggerMock;
    private ArticleService _service;

    [SetUp]
    public void Setup()
    {
        _dataHandlerMock = new Mock<IDataHandler>();
        _wikipediaMock = new Mock<IWikipediaService>();
        _loggerMock = new Mock<ILogger<ArticleService>>();
        _service = new ArticleService(_dataHandlerMock.Object, _wikipediaMock.Object, _loggerMock.Object);
    }

    [Test]
    public async Task AddArticleAsync_CallsWikipedia_WhenSummaryIsMissing()
    {
        var request = new CreateArticleRequest { Title = "Cat", Content = "Info about cats.", Summary = "" };

        _wikipediaMock
            .Setup(w => w.GetSummaryAsync("Cat"))
            .ReturnsAsync("Cat summary");

        _dataHandlerMock
            .Setup(d => d.AddArticleAsync(It.IsAny<Article>()))
            .ReturnsAsync((Article a) => a);

        var result = await _service.AddArticleAsync(request);

        Assert.That(result.Summary, Is.EqualTo("Cat summary"));
        _wikipediaMock.Verify(w => w.GetSummaryAsync("Cat"), Times.Once);
        _dataHandlerMock.Verify(d => d.AddArticleAsync(It.IsAny<Article>()), Times.Once);
    }

    [Test]
    public async Task AddArticleAsync_SkipsWikipedia_WhenSummaryExists()
    {
        var request = new CreateArticleRequest { Title = "Dog", Content = "Info about dogs.", Summary = "Already exists" };

        _dataHandlerMock
            .Setup(d => d.AddArticleAsync(It.IsAny<Article>()))
            .ReturnsAsync((Article a) => a);

        var result = await _service.AddArticleAsync(request);

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

        _dataHandlerMock
            .Setup(d => d.AddArticleAsync(It.IsAny<Article>()))
            .ReturnsAsync((Article a) => a);

        var result = await _service.AddArticleAsync(request);

        Assert.That(result.Summary, Is.Null.Or.Empty);
        _dataHandlerMock.Verify(d => d.AddArticleAsync(It.IsAny<Article>()), Times.Once);
    }

    [Test]
    public void Search_FiltersCorrectly_AndCaseInsensitive()
    {
        var articles = new List<Article>
        {
            new Article { Title = "Apple" },
            new Article { Title = "Banana" },
            new Article { Title = "Pineapple" }
        };

        _dataHandlerMock
            .Setup(d => d.GetArticlesAsync())
            .ReturnsAsync(articles);

        var result = _service.Search("apple").ToList();

        Assert.That(result.Count, Is.EqualTo(2));
        Assert.That(result.Any(a => a.Title == "Apple"), Is.True);
        Assert.That(result.Any(a => a.Title == "Pineapple"), Is.True);
    }

    [Test]
    public async Task DeleteArticleAsync_ReturnsFalse_WhenNotFound()
    {
        _dataHandlerMock
            .Setup(d => d.DeleteArticleAsync(99))
            .ReturnsAsync(false);

        var result = await _service.DeleteArticleAsync(99);

        Assert.That(result, Is.False);
    }

    [Test]
    public async Task UpdateArticleAsync_ReturnsFalse_WhenNotFound()
    {
        var request = new UpdateArticleRequest { Title = "Test", Content = "Some content." };

        _dataHandlerMock
            .Setup(d => d.GetArticleByIdAsync(99))
            .ReturnsAsync((Article?)null);

        var result = await _service.UpdateArticleAsync(99, request);

        Assert.That(result, Is.False);
    }
}
