using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Configuration;
using Serilog;
using System;
using System.Threading.Tasks;
using Masya.TelegramBot.Commands.Abstractions;
using Masya.TelegramBot.Modules;
using Masya.TelegramBot.DataAccess;
using Masya.TelegramBot.DatabaseExtensions.Metadata;
using Serilog.Sinks.MSSqlServer;
using System.Collections.ObjectModel;
using System.Data;
using Serilog.Filters;

namespace Masya.TelegramBot.Api
{
    public static class Program
    {
        public static async Task Main(string[] args)
        {
            var host = CreateHostBuilder(args).Build();
            await StartRequiredServices(host.Services);
            await host.RunAsync();
        }

        public static async Task StartRequiredServices(IServiceProvider services)
        {
            using var scope = services.CreateScope();
            var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
            await ApplicationDbContext.SeedDatabase(dbContext);
            var commandService = services.GetRequiredService<ICommandService<DatabaseCommandInfo, DatabaseAliasInfo>>();
            var botService = services.GetRequiredService<IBotService<DatabaseCommandInfo, DatabaseAliasInfo>>();
            await commandService.LoadCommandsAsync(typeof(BasicModule).Assembly);
            await botService.SetWebhookAsync();
        }

        public static IHostBuilder CreateHostBuilder(string[] args) =>
            Host.CreateDefaultBuilder(args)
                .ConfigureAppConfiguration(configBuilder =>
                {
                    configBuilder
                    .AddEnvironmentVariables()
                    .AddJsonFile(
                        path: "appsettings.json",
                        optional: false,
                        reloadOnChange: true
                    );
                })
                .UseSerilog((context, logger) =>
                {
                    var sinkOptions = new MSSqlServerSinkOptions()
                    {
                        TableName = "Serilogs",
                        AutoCreateSqlTable = true,
                    };

                    var columnOptions = new ColumnOptions()
                    {
                        AdditionalColumns = new Collection<SqlColumn> {
                            new SqlColumn { ColumnName = "AgencyId", DataType = SqlDbType.Int, AllowNull = true }
                        },
                    };

                    columnOptions.Store.Remove(StandardColumn.Properties);
                    columnOptions.Store.Remove(StandardColumn.MessageTemplate);
                    columnOptions.Store.Remove(StandardColumn.LogEvent);

                    logger
                        .MinimumLevel.Information()
                        .WriteTo.Console()
                        // .Filter.ByExcluding(Matching.FromSource("Microsoft"))
                        // .Filter.ByExcluding(Matching.FromSource("System"))
                        .WriteTo.MSSqlServer(
                            connectionString: context.Configuration.GetConnectionString("RemoteDb"),
                            sinkOptions: sinkOptions,
                            appConfiguration: context.Configuration,
                            columnOptions: columnOptions
                        );
                })
                .ConfigureWebHostDefaults(webBuilder =>
                {
                    webBuilder.UseStartup<Startup>();
                });
    }
}
