using Lexicon.Model;

namespace Lexicon.Data;

public interface IDataHandler
{
        IEnumerable<Article> GetArticles();
        Article? GetArticleById(int id);
        Article AddArticle(Article article);
        bool DeleteArticle(int id);
        
        bool UpdateArticle(int id, Article article);
}