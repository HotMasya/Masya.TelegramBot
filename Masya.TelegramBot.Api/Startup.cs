using Masya.TelegramBot.Commands.Abstractions;
using Masya.TelegramBot.Commands.Options;
using Masya.TelegramBot.DatabaseExtensions;
using Masya.TelegramBot.DataAccess;
using Masya.TelegramBot.Api.Options;
using Masya.TelegramBot.Api.Services;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.IdentityModel.Tokens;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using System.Text;
using System;
using Coravel;
using Masya.TelegramBot.DatabaseExtensions.Metadata;
using Masya.TelegramBot.DatabaseExtensions.Abstractions;
using Masya.TelegramBot.Api.Services.Abstractions;
using Serilog;
using Serilog.Sinks.MSSqlServer;
using System.Data;
using System.Collections.ObjectModel;

namespace Masya.TelegramBot.Api
{
    public class Startup
    {
        private const string CorsPolicyName = "DefaultCORSPolicy";

        public IConfiguration Configuration { get; }

        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        private ILogger ConfigureLogger()
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

            return new LoggerConfiguration()
                .ReadFrom.Configuration(Configuration)
                .WriteTo.Console()
                .WriteTo.MSSqlServer(
                    connectionString: Configuration.GetConnectionString("RemoteDb"),
                    sinkOptions: sinkOptions,
                    appConfiguration: Configuration,
                    columnOptions: columnOptions
                )
                .CreateLogger();
        }

        public void ConfigureServices(IServiceCollection services)
        {
            Log.Logger = ConfigureLogger();

            services.Configure<CommandServiceOptions>(Configuration.GetSection("Commands"));
            services.Configure<JwtOptions>(Configuration.GetSection("JwtOptions"));
            services.Configure<CacheOptions>(Configuration.GetSection("Cache"));
            services.AddScheduler();
            services.AddAutoMapper(typeof(Startup));
            services.AddCors(options =>
            {
                options.AddPolicy(
                name: CorsPolicyName,
                builder =>
                {
                    builder
                .AllowAnyHeader()
                .AllowAnyOrigin()
                .AllowAnyMethod();
                }
            );
            });
            services.AddSingleton<IDatabaseLogsService, DatabaseLogsService>();
            services.AddSingleton<IKeyboardGenerator, KeyboardGenerator>();
            services.AddStackExchangeRedisCache(options =>
            {
                options.Configuration = Configuration.GetConnectionString("Redis");
                options.InstanceName = "TelegramBot_";
            });
            services.AddDbContext<ApplicationDbContext>(options =>
            {
                options.UseSqlServer(Configuration.GetConnectionString("RemoteDb"));
            });
            services.AddSingleton<IBotService<DatabaseCommandInfo, DatabaseAliasInfo>, DatabaseBotService>();
            services.AddSingleton<ICommandService<DatabaseCommandInfo, DatabaseAliasInfo>, DatabaseCommandService>();
            services.AddControllers()
                .AddNewtonsoftJson(options =>
                {
                    options.UseCamelCasing(true);
                    options.SerializerSettings.NullValueHandling = NullValueHandling.Ignore;
                    options.SerializerSettings.ReferenceLoopHandling = ReferenceLoopHandling.Ignore;
                });
            services.AddSingleton<IJwtService, JwtService>();
            services.AddScoped<IXmlService, XmlService>();
            services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
                .AddJwtBearer(JwtBearerDefaults.AuthenticationScheme,
                options =>
                {
                    options.TokenValidationParameters = new TokenValidationParameters()
                    {
                        ValidateIssuer = true,
                        ValidateAudience = true,
                        ValidateIssuerSigningKey = true,
                        ValidateLifetime = true,
                        ValidIssuer = Configuration["JwtOptions:Issuer"],
                        ValidAudience = Configuration["JwtOptions:Audience"],
                        IssuerSigningKey = new SymmetricSecurityKey(Encoding.ASCII.GetBytes(Configuration["JwtOptions:Secret"])),
                        ClockSkew = TimeSpan.FromSeconds(30),
                    };
                });
        }

        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            var provider = app.ApplicationServices;
            provider.UseScheduler(scheduler =>
            {
                scheduler
              .Schedule<UpdateXmlImportsInvokable>()
              .Daily();
            });

            app.UseRouting();
            app.UseCors(CorsPolicyName);
            app.UseAuthentication();
            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
                endpoints.Map("/", async context =>
                {
                    await context.Response.WriteAsync("<h1>Kinda homepage</h1>");
                    await context.Response.CompleteAsync();
                });
                endpoints.MapControllerRoute(
                    name: "Wilcard_or_update",
                    pattern: "{**catchAll}",
                    defaults: new { Controller = "Bot", Action = "Index" }
                );
            });
        }
    }
}
