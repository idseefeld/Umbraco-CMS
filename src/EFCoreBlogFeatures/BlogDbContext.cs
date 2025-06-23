using EFCoreBlogFeatures.Dto;
using Microsoft.EntityFrameworkCore;

namespace EFCoreBlogFeatures
{
    public class BlogDbContext : DbContext
    {
        public BlogDbContext(DbContextOptions<BlogDbContext> options)
            : base(options)
        {
        }

        public required DbSet<BlogComment> BlogComments { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder) =>
            modelBuilder.Entity<BlogComment>(entity =>
            {
                entity.ToTable("blogComment");
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Id).HasColumnName("id");
                entity.Property(e => e.BlogPostUmbracoKey).HasColumnName("blogPostUmbracoKey");
                entity.Property(e => e.Message).HasColumnName("message");
                entity.Property(e => e.Website).HasColumnName("website");
                entity.Property(e => e.Email).HasColumnName("email");
            });
    }
}
