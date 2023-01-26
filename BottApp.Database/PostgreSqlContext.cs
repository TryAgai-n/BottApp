using BottApp.Database.Document;
using BottApp.Database.Document.Like;
using BottApp.Database.Document.Statistic;
using BottApp.Database.User;
using BottApp.Database.User.UserFlag;
using BottApp.Database.UserMessage;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace BottApp.Database
{
    public class PostgreSqlContext : DbContext
    {
        public readonly DatabaseContainer Db;

        public DbSet<UserModel> User { get; set; }
        
        public DbSet<UserFlagModel> UserFlag { get; set; }

        public DbSet<MessageModel> Message { get; set; }
        
        public DbSet<DocumentModel> Document { get; set; }
        
        public DbSet<DocumentStatisticModel> DocumentStatistic { get; set; }
        
        public DbSet<LikedDocumentModel> LikedDocument { get; set; }



        public PostgreSqlContext(DbContextOptions<PostgreSqlContext> options, ILoggerFactory loggerFactory, IServiceScopeFactory scopeFactory) : base(options)
        {
            Db = new DatabaseContainer(this, loggerFactory, scopeFactory);
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

            modelBuilder.Entity<DocumentModel>()
                .HasOne(x => x.DocumentStatisticModel)
                .WithOne(x => x.DocumentModel)
                .IsRequired();

            modelBuilder.Entity<DocumentModel>()
                .HasMany(x => x.Likes)
                .WithOne(x => x.DocumentModel)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<UserFlagModel>()
                .HasOne(u => u.UserModel)
                .WithOne(uf=> uf.UserFlag);
        }

    }
}
