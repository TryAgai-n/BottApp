
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using VoteApp.Database.Document;
using VoteApp.Database.User;

namespace VoteApp.Database
{
    public class PostgresContext : DbContext
    {
        public readonly DatabaseContainer Db;
        
        
        public DbSet<UserModel> User { get; set; }
        
        public DbSet<DocumentModel> Document { get; set; }
        
        
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
