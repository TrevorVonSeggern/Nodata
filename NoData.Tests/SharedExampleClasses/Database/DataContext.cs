using Microsoft.EntityFrameworkCore;
using NoData.Tests.SharedExampleClasses.Database.Entity;

namespace NoData.Tests.SharedExampleClasses.Database
{
    public class DataContext : DbContext
    {
        public static DataContext GetInMemoryDatabase()
        {
            var databaseOptions = new DbContextOptionsBuilder<DataContext>()
                .UseInMemoryDatabase(databaseName: System.Guid.NewGuid().ToString())
                .Options;
            return new DataContext(databaseOptions);
        }

        public DataContext(DbContextOptions<DataContext> options) : base(options)
        {
        }

        public DbSet<Person> People { get; set; }
        public DbSet<Child> Children { get; set; }
        public DbSet<GrandChild> GrandChildren { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Person>().HasOne(x => x.Partner);
            modelBuilder.Entity<Person>().HasOne(x => x.Favorite);
            modelBuilder.Entity<Person>().HasMany(x => x.Children);
            modelBuilder.Entity<Child>();
        }
    }
}