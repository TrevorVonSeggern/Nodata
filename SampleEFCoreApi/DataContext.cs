using Microsoft.EntityFrameworkCore;
using SampleEFCoreApi.Database;

namespace SampleEFCoreApi
{
    public class DataContext : DbContext
    {
        public DataContext(DbContextOptions<DataContext> options) : base(options)
        {

        }

        public DbSet<Person> People { get; set; }
        public DbSet<Child> Children { get; set; }
        public DbSet<GrandChild> GrandChildren { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Person>().HasOne(x => x.Partner);
        }
    }
}