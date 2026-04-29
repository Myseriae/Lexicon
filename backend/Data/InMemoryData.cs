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


    public Task<IEnumerable<Article>> GetArticlesAsync()
        => Task.FromResult<IEnumerable<Article>>(_articles);

    public Task<Article?> GetArticleByIdAsync(int id)
        => Task.FromResult(_articles.FirstOrDefault(a => a.Id == id));

    public Task<Article> AddArticleAsync(Article article)
    {
        article.Id = _articleIdCounter++;
        _articles.Add(article);
        return Task.FromResult(article);
    }

    public Task<bool> DeleteArticleAsync(int id)
    {
        var article = _articles.FirstOrDefault(a => a.Id == id);
        if (article == null) return Task.FromResult(false);

        _articles.Remove(article);
        return Task.FromResult(true);
    }

    public Task<bool> UpdateArticleAsync(int id, Article updated)
    {
        var article = _articles.FirstOrDefault(a => a.Id == id);
        if (article == null) return Task.FromResult(false);

        article.Title = updated.Title;
        article.Content = updated.Content;

        return Task.FromResult(true);
    }
}
