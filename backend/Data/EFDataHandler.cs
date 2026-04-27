using Lexicon.Model;

namespace Lexicon.Data;

public class EFDataHandler : IDataHandler
{
    private readonly LexiconDbContext _context;
    
    public EFDataHandler(LexiconDbContext context)
    {
        _context = context;
    }
    
    public IEnumerable<Article> GetArticles() => _context.Articles.ToList();
    
    public Article? GetArticleById(int id) => _context.Articles.Find(id);

    public Article AddArticle(Article article)
    {
        _context.Articles.Add(article);
        _context.SaveChanges();
        return article;
    }

    public bool DeleteArticle(int id)
    {
        var article = GetArticleById(id);
        if (article == null) return false;

        _context.Articles.Remove(article);
        _context.SaveChanges();
        return true;
    }

    public bool UpdateArticle(int id, Article article)
    {
        var existing = GetArticleById(id);
        if (existing == null) return false;

        existing.Title = article.Title;
        existing.Content = article.Content;
        _context.SaveChanges();
        return true;
    }
}