using Masya.TelegramBot.DataAccess.Models;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;

namespace Masya.TelegramBot.DataAccess
{
    class ApplicationDbContext : DbContext
    {
        public DbSet<Agency> Agencies { get; set; }
        public DbSet<Category> Categories { get; set; }
        public DbSet<Directory> Directories { get; set; }
        public DbSet<DirectoryItem> DirectoryItems { get; set; }
        public DbSet<Favorites> Favorites { get; set; }
        public DbSet<PropertyObject> PropertyObjects { get; set; }
        public DbSet<Reference> References { get; set; }
        public DbSet<Report> Reports { get; set; }
        public DbSet<User> User { get; set; }
        public List<User> Agents => User.Where(u => u.AgencyId.HasValue).ToList();

        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            :base (options) { }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder
                .Entity<Category>()
                .Property(c => c.SuperType)
                .HasConversion<int>();
        }
    }
}
