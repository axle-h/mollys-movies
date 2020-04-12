using Microsoft.EntityFrameworkCore;

namespace MolliesMovies.Common.Data
{
    public class MolliesMoviesContext : DbContext
    {
        public MolliesMoviesContext(DbContextOptions<MolliesMoviesContext> options) : base(options)
        {
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.ApplyConfigurationsFromAssembly(typeof(MolliesMoviesContext).Assembly);
        }
    }
}