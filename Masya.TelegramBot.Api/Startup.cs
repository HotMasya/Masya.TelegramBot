using System.Text.Json.Serialization;
using System.Text.Json;
using Masya.TelegramBot.Commands.Services;
using Masya.TelegramBot.Commands.Abstractions;
using Masya.TelegramBot.Commands.Options;
using Masya.TelegramBot.DataAccess;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Serilog;
using Microsoft.EntityFrameworkCore;
using Masya.TelegramBot.Api.Bot;
using Masya.TelegramBot.Commands.Data;

namespace Masya.TelegramBot.Api
{
    public class Startup
    {
        public IConfiguration Configuration { get; }

        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public void ConfigureServices(IServiceCollection services)
        {
            Log.Logger = new LoggerConfiguration()
                .ReadFrom.Configuration(Configuration)
                .WriteTo.Console()
                .CreateLogger();

            services.Configure<BotServiceOptions>(Configuration.GetSection("Bot"));
            services.Configure<CommandServiceOptions>(Configuration.GetSection("Commands"));
            services.AddDbContext<CommandDbContext, ApplicationDbContext>(options => {
                options.UseSqlServer(Configuration.GetConnectionString("DevelopmentDb"));
            });
            services.AddScoped<IBotService, DefaultBotService>();
            services.AddScoped<ICommandService, DatabaseCommandService>();
            services.AddScoped<BotSetup>();
            services.AddControllers().AddJsonOptions(options =>
            {
                options.JsonSerializerOptions.DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull;
                options.JsonSerializerOptions.PropertyNamingPolicy = JsonNamingPolicy.CamelCase;
                options.JsonSerializerOptions.AllowTrailingCommas = false;
            });
        }

        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            app.UseRouting();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
                endpoints.Map("*", async context =>
                {
                    context.Response.StatusCode = StatusCodes.Status404NotFound;
                    await context.Response.CompleteAsync();
                });
            });
        }
    }
}
