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
        public virtual DbSet<Alias> Aliases { get; set; }

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
        }

        internal virtual async Task AttachCommandsAsync(IList<CommandInfo> commandInfos)
        {
            var commands = await Commands.ToListAsync();
            foreach(var ci in commandInfos)
            {
                var command = commands.FirstOrDefault(c => c.Name == ci.Name);
                if(command != null)
                {
                    foreach(var al in command.Aliases)
                    {
                        ci.Aliases.Add(al);
                    }    
                }
            }
        }
    }
}
