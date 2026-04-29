using Lexicon.DTOs;

namespace Lexicon.Services;

public interface IArticleService
{
    IEnumerable<ArticleResponse> GetArticles();
    ArticleResponse? GetArticleById(int id);
    Task<ArticleResponse> AddArticleAsync(CreateArticleRequest request);
    Task<bool> DeleteArticleAsync(int id);
    Task<bool> UpdateArticleAsync(int id, UpdateArticleRequest request);
    IEnumerable<ArticleResponse> Search(string query);
}
