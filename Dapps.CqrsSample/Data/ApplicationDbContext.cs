using Microsoft.EntityFrameworkCore;

namespace Dapps.CqrsSample.Data
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options)
        {

        }
        public DbSet<Article> Articles { get; set; }

    }
}
