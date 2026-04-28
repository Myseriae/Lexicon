using Lexicon.Data;
using Lexicon.Model;
using Lexicon.Services;
using Moq;

namespace LexiconTest;

[TestFixture]
public class ArticleServiceTests
{
    private Mock<IDataHandler> _dataHandlerMock;
    private Mock<IWikipediaService> _wikipediaMock;
    private ArticleService _service;

    [SetUp]
    public void Setup()
    {
        _dataHandlerMock = new Mock<IDataHandler>();
        _wikipediaMock = new Mock<IWikipediaService>();
        _service = new ArticleService(_dataHandlerMock.Object, _wikipediaMock.Object);
    }

    [Test]
    public async Task AddArticleAsync_CallsWikipedia_WhenSummaryIsMissing()
    {
        var article = new Article
        {
            Title = "Cat",
            Summary = ""
        };

        _wikipediaMock
            .Setup(w => w.GetSummaryAsync("Cat"))
            .ReturnsAsync("Cat summary");

        _dataHandlerMock
            .Setup(d => d.AddArticle(It.IsAny<Article>()))
            .Returns((Article a) => a);

        var result = await _service.AddArticleAsync(article);

        Assert.That(result.Summary, Is.EqualTo("Cat summary"));
        _wikipediaMock.Verify(w => w.GetSummaryAsync("Cat"), Times.Once);
        _dataHandlerMock.Verify(d => d.AddArticle(article), Times.Once);
    }

    [Test]
    public async Task AddArticleAsync_SkipsWikipedia_WhenSummaryExists()
    {
        var article = new Article
        {
            Title = "Dog",
            Summary = "Already exists"
        };

        _dataHandlerMock
            .Setup(d => d.AddArticle(It.IsAny<Article>()))
            .Returns((Article a) => a);

        var result = await _service.AddArticleAsync(article);

        Assert.That(result.Summary, Is.EqualTo("Already exists"));
        _wikipediaMock.Verify(w => w.GetSummaryAsync(It.IsAny<string>()), Times.Never);
    }

    [Test]
    public async Task AddArticleAsync_SavesArticle_WhenWikipediaReturnsNull()
    {
        var article = new Article
        {
            Title = "Unknown",
            Summary = ""
        };

        _wikipediaMock
            .Setup(w => w.GetSummaryAsync("Unknown"))
            .ReturnsAsync((string?)null);

        _dataHandlerMock
            .Setup(d => d.AddArticle(It.IsAny<Article>()))
            .Returns((Article a) => a);

        var result = await _service.AddArticleAsync(article);

        Assert.That(result.Summary, Is.Null.Or.Empty);
        _dataHandlerMock.Verify(d => d.AddArticle(article), Times.Once);
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
            .Setup(d => d.GetArticles())
            .Returns(articles);

        var result = _service.Search("apple").ToList();

        Assert.That(result.Count, Is.EqualTo(2));
        Assert.That(result.Any(a => a.Title == "Apple"), Is.True);
        Assert.That(result.Any(a => a.Title == "Pineapple"), Is.True);
    }

    [Test]
    public void DeleteArticle_ReturnsFalse_WhenNotFound()
    {
        _dataHandlerMock
            .Setup(d => d.DeleteArticle(99))
            .Returns(false);

        var result = _service.DeleteArticle(99);

        Assert.That(result, Is.False);
    }

    [Test]
    public void UpdateArticle_ReturnsFalse_WhenNotFound()
    {
        var article = new Article { Id = 99, Title = "Test" };

        _dataHandlerMock
            .Setup(d => d.UpdateArticle(99, article))
            .Returns(false);

        var result = _service.UpdateArticle(99, article);

        Assert.That(result, Is.False);
    }
}