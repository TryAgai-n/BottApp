using BottApp.Database.Document;
using BottApp.Database.Message;
using BottApp.Database.User;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace BottApp.Database
{

    public class PostgreSqlContext : DbContext
    {
        public readonly DatabaseContainer Db;

        public DbSet<UserModel> User { get; set; }
        
         public DbSet<MessageModel> Message { get; set; }
        


        public PostgreSqlContext(DbContextOptions<PostgreSqlContext> options, ILoggerFactory loggerFactory) : base(options)
        {
            Db = new DatabaseContainer(this, loggerFactory);
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<UserModel>().HasIndex(x => x.UId).IsUnique();

            modelBuilder.Entity<MessageModel>()
                .HasOne(x => x.UserModel)
                .WithMany(x => x.Messages);
            
            modelBuilder.Entity<DocumentModel>()
                .HasOne(x => x.UserModel)
                .WithMany(x => x.Documents);
        }

    }
}
