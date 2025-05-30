using ArticlesNewsApi.Models;
using Microsoft.EntityFrameworkCore;

namespace ArticlesNewsApi.Data
{
    public class NewsDbContext : DbContext
    {
        public NewsDbContext(DbContextOptions<NewsDbContext> options) : base(options) { }
        public DbSet<Article> Articles { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            // here you can configure the model
            modelBuilder.Entity<Article>()
                .Property(a => a.Title)
                .IsRequired()
                .HasMaxLength(250);

            modelBuilder.Entity<Article>()
                .HasIndex(a => a.Path)
                .IsUnique();

            base.OnModelCreating(modelBuilder);
        }
    }
}