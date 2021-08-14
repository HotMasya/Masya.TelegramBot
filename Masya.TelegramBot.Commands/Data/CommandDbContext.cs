using Masya.TelegramBot.Commands.Metadata;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Masya.TelegramBot.Commands.Data
{
    public class CommandDbContext : DbContext
    {
        public virtual DbSet<Command> Commands { get; set; }

        public CommandDbContext(DbContextOptions<CommandDbContext> options)
            : base(options) { }

        public CommandDbContext(DbContextOptions options)
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
        }

        internal virtual async Task MapCommandsAsync(IList<CommandInfo> commandInfos)
        {
            var commands = await Commands.ToListAsync();
            foreach (var ci in commandInfos)
            {
                var command = commands.FirstOrDefault(
                    c => c.Name.ToLower().Equals(ci.Name.ToLower())
                );
                if (command != null)
                {
                    ci.IsEnabled = command.IsEnabled;
                    ci.Permission = command.Permission;
                    foreach (var al in command.Aliases)
                    {
                        ci.Aliases.Add(
                            new AliasInfo
                            {
                                Name = al.Name,
                                CommandInfo = ci,
                            }
                        );
                    }
                }
            }
        }
    }
}
