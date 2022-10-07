using BottApp.Database.User;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace BottApp.Database
{
    public sealed class PostgreSqlContext : DbContext
    {
        public readonly DatabaseContainer Db;

        public DbSet<UserModel> User { get; set; }


        public PostgreSqlContext(DbContextOptions<PostgreSqlContext> options, ILoggerFactory loggerFactory) :
            base(options)
        {
            Db = new DatabaseContainer(this, loggerFactory);
        }


        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<UserModel>()
                .HasIndex(x => x.Phone)
                .IsUnique();
        }
    }
}