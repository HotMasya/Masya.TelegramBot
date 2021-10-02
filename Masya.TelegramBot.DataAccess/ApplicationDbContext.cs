using Masya.TelegramBot.DataAccess.Models;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Masya.TelegramBot.DataAccess
{
    public class ApplicationDbContext : DbContext
    {
        public DbSet<Agency> Agencies { get; set; }
        public DbSet<Category> Categories { get; set; }
        public DbSet<Directory> Directories { get; set; }
        public DbSet<DirectoryItem> DirectoryItems { get; set; }
        public DbSet<Favorites> Favorites { get; set; }
        public DbSet<RealtyObject> RealtyObjects { get; set; }
        public DbSet<Reference> References { get; set; }
        public DbSet<Report> Reports { get; set; }
        public DbSet<User> Users { get; set; }
        public DbSet<Command> Commands { get; set; }
        public DbSet<BotSettings> BotSettings { get; set; }
        public DbSet<UserSettings> UserSettings { get; set; }
        public DbSet<Floor> Floors { get; set; }
        public DbSet<Price> Prices { get; set; }
        public DbSet<Image> Images { get; set; }
        public DbSet<Room> Rooms { get; set; }

        public List<User> GetAgents() => Users
            .AsQueryable()
            .Where(u => u.AgencyId.HasValue)
            .ToList();

        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options) { }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder
                .Entity<Command>()
                .Property(c => c.Permission)
                .HasConversion<int>();

            modelBuilder.Entity<Command>(entity =>
            {
                entity
                    .HasMany(e => e.Aliases)
                    .WithOne(e => e.ParentCommand)
                    .HasForeignKey(e => e.ParentId);
            });

            modelBuilder
                .Entity<Category>()
                .Property(c => c.SuperType)
                .HasConversion<int>();
        }

        public static async Task SeedDatabase(DbContext context)
        {
            if (context.Database.GetPendingMigrations().Any())
            {
                await context.Database.MigrateAsync();
            }
        }
    }
}
