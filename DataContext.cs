using Microsoft.EntityFrameworkCore;

namespace PruebaCorta2
{
    class ChairDb : DbContext
    {
        public ChairDb(DbContextOptions<ChairDb> options)
            : base(options) { }
        public DbSet<Chair> Sillas => Set<Chair>();
    }
}