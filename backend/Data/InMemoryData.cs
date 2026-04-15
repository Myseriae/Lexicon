using Lexicon.Model;

namespace Lexicon.Data;

public class InMemoryData : IDataHandler
{
    private static int _articleIdCounter = 1;
    private readonly List<Article> _articles = new();

    public IEnumerable<Article> GetArticles() => _articles;

    public Article? GetArticleById(int id)
        => _articles.FirstOrDefault(a => a.Id == id);

    public Article AddArticle(Article article)
    {
        article.Id = _articleIdCounter++;
        _articles.Add(article);
        return article;
    }

    public bool DeleteArticle(int id)
    {
        var article = GetArticleById(id);
        if (article == null) return false;

        _articles.Remove(article);
        return true;
    }
}