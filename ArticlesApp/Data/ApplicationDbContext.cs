using ArticlesApp.Models;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace ArticlesApp.Data
{
    public class ApplicationDbContext : IdentityDbContext<ApplicationUser>
    {
        public DbSet<ApplicationUser> ApplicationUsers { get; set; }
        public DbSet<Article> Articles { get; set; }
        public DbSet<Category> Categories { get; set; }
        public DbSet<Comment> Comments { get; set; }
        public DbSet<Bookmark> Bookmarks { get; set; }
        public DbSet<ArticleBookmark> ArticleBookmarks { get; set; }
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        protected override void OnModelCreating(ModelBuilder
modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
            // definire primary key compus
            modelBuilder.Entity<ArticleBookmark>()
            .HasKey(ab => new {
                ab.Id,
                ab.ArticleId,
                ab.BookmarkId
            });
            // definire relatii cu modelele Bookmark si Article (FK)
            modelBuilder.Entity<ArticleBookmark>()
            .HasOne(ab => ab.Article)
            .WithMany(ab => ab.ArticleBookmarks)
            .HasForeignKey(ab => ab.ArticleId);
            modelBuilder.Entity<ArticleBookmark>()
            .HasOne(ab => ab.Bookmark)
            .WithMany(ab => ab.ArticleBookmarks)
            .HasForeignKey(ab => ab.BookmarkId);
        }
    }

}
