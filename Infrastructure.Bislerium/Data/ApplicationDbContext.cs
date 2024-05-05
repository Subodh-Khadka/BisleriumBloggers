using Domain.Bislerium;
using Domain.Bislerium.ViewModels;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Bislerium.Data
{
    public class ApplicationDbContext : IdentityDbContext<ApplicationUser>
    {
        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            optionsBuilder.UseSqlServer("Server=SUBODH-PC;Database=Bislerium;Trusted_Connection=True;TrustServerCertificate=True;MultipleActiveResultSets=True",
                options =>
                {
                    options.EnableRetryOnFailure(); 
                });
        }

        public DbSet<ApplicationUser> ApplicationUsers { get; set; }
        public DbSet<Blog> Blogs { get; set; }
        public DbSet<Comment> Comments { get; set; }
        public DbSet<Reaction> Reactions { get; set; }
        public DbSet<UpVote> Uovotes{ get; set; }
        public DbSet<DownVote> DownVotes { get; set; }
        public DbSet<CommentUpVote> CommentUpVotes { get; set; }
        public DbSet<CommentDownVote> CommentDownVotes { get; set; }
    }
}
