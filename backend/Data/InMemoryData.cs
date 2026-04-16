using Lexicon.Model;

namespace Lexicon.Data;

public class InMemoryData : IDataHandler
{
    private static int _articleIdCounter = 1;
    private readonly List<Article> _articles = new();

    public InMemoryData()
    {
        _articles.AddRange(new List<Article>
        {
            new Article
            {
                Id = _articleIdCounter++,
                Title = "C#",
                Content = "A modern programming language."
            },
            new Article
            {
                Id = _articleIdCounter++,
                Title = "ASP.NET Core",
                Content = "You use it to build a web application."
            },
            new Article
            {
                Id = _articleIdCounter++,
                Title = "REST API",
                Content = "Used to build RESTful APIs."
            }
        });
    }
    

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
    
    public bool UpdateArticle(int id, Article updated)
    {
        var article = GetArticleById(id);
        if (article == null) return false;

        article.Title = updated.Title;
        article.Content = updated.Content;

        return true;
    }
}