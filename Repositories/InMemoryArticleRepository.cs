using ArticlesNewsApi.Models;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ArticlesNewsApi.Repositories
{
    public class InMemoryArticleRepository : IArticleRepository
    {
        private static readonly List<Article> _articles = new List<Article>();
        private static int _nextId = 1;

        public InMemoryArticleRepository()
        {
            if (!_articles.Any())
            {
                _articles.Add(new Article
                {
                    Id = _nextId++,
                    Title = "Noticia: La IA transforma la industria",
                    ShortDescription = "La inteligencia artificial está revolucionando múltiples sectores económicos.",
                    Description = "Un informe reciente destaca cómo la IA está impulsando la innovación...",
                    Category = "Tecnología",
                    Source = "TechNews",
                    Path = "ia-transforma-industria",
                    PublishedDate = DateTime.UtcNow.AddDays(-5),
                    Author = "Juan Perez",
                    ImageUrl = "https://example.com/ia-news.jpg"
                });
                _articles.Add(new Article
                {
                    Id = _nextId++,
                    Title = "Nuevo récord de turismo en el país",
                    ShortDescription = "El sector turístico nacional alcanza cifras históricas.",
                    Description = "Los datos del último trimestre muestran un crecimiento sin precedentes en la llegada de visitantes...",
                    Category = "Economía",
                    Source = "EcoData",
                    Path = "turismo-record-nacional",
                    PublishedDate = DateTime.UtcNow.AddDays(-2),
                    Author = "Maria Lopez",
                    ImageUrl = "https://example.com/turismo-news.jpg"
                });
            }
        }

        public Task<IEnumerable<Article>> GetArticlesAsync()
        {
            return Task.FromResult<IEnumerable<Article>>(_articles.ToList());
        }

        public Task<Article?> GetArticleByIdAsync(int id)
        {
            return Task.FromResult(_articles.FirstOrDefault(a => a.Id == id));
        }

        public Task AddArticleAsync(Article article)
        {
            article.Id = _nextId++;
            _articles.Add(article);
            return Task.CompletedTask;
        }

        public Task UpdateArticleAsync(Article article)
        {
            var existingArticle = _articles.FirstOrDefault(a => a.Id == article.Id);
            if (existingArticle != null)
            {
                existingArticle.Title = article.Title;
                existingArticle.ShortDescription = article.ShortDescription;
                existingArticle.Description = article.Description;
                existingArticle.Category = article.Category;
                existingArticle.Source = article.Source;
                existingArticle.Path = article.Path;
                existingArticle.PublishedDate = article.PublishedDate;
                existingArticle.Author = article.Author;
                existingArticle.ImageUrl = article.ImageUrl;
            }
            return Task.CompletedTask;
        }

        public Task DeleteArticleAsync(int id)
        {
            _articles.RemoveAll(a => a.Id == id);
            return Task.CompletedTask;
        }
    }
}