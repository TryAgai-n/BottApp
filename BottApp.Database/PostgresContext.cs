using System.ComponentModel.DataAnnotations.Schema;
using BottApp.Database.Document;
using BottApp.Database.Document.Like;
using BottApp.Database.Document.Statistic;
using BottApp.Database.Message;
using BottApp.Database.User;
using BottApp.Database.WebUser;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace BottApp.Database
{
    public class PostgresContext : DbContext
    {
        public readonly DatabaseContainer Db;

        // public DbSet<UserBotModel> UserBot { get; set; }
        
        public DbSet<UserModel> User { get; set; }
        
        // public DbSet<MessageModel> Message { get; set; }
        //
        public DbSet<DocumentModel> Document { get; set; }
        
        // public DbSet<DocumentStatisticModel> DocumentStatistic { get; set; }
        //
        // public DbSet<LikedDocumentModel> LikedDocument { get; set; }
        
        public PostgresContext(DbContextOptions<PostgresContext> options, ILoggerFactory loggerFactory) : base(options)
        {
            Db = new DatabaseContainer(this, loggerFactory);
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            ConfigureDocumentModel(modelBuilder);
        }
        
        private void ConfigureDocumentModel(ModelBuilder builder)
        {
            builder.Entity<DocumentModel>()
                .HasKey(d => d.Id);

            builder.Entity<DocumentModel>()
                .Property(d => d.Id)
                .UseIdentityColumn();
    
            builder.Entity<DocumentModel>()
                .HasOne(x => x.UserModel)
                .WithMany(x => x.Documents)
                .HasForeignKey(d => d.UserId);
        }
    }
}
