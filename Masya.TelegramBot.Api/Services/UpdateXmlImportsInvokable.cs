using System;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Coravel.Invocable;
using Masya.TelegramBot.Api.Services.Abstractions;
using Masya.TelegramBot.DataAccess;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Masya.TelegramBot.Api.Services
{
    public sealed class UpdateXmlImportsInvokable : IInvocable, ICancellableInvocable
    {
        public CancellationToken CancellationToken { get; set; }

        private readonly IServiceProvider _services;
        private readonly ILogger<UpdateXmlImportsInvokable> _logger;

        public UpdateXmlImportsInvokable(
            IServiceProvider services,
            ILogger<UpdateXmlImportsInvokable> logger
        )
        {
            _services = services;
            _logger = logger;
        }

        public async Task Invoke()
        {
            using var scope = _services.CreateScope();
            var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
            var xmlService = scope.ServiceProvider.GetRequiredService<IXmlService>();
            var agenciesData = dbContext.Agencies.Select(a => new { a.ImportUrl, a.Id }).ToList();
            var botSettings = dbContext.BotSettings.First();

            if (botSettings.IsImporting)
            {
                return;
            }

            botSettings.IsImporting = true;

            await dbContext.SaveChangesAsync();

            var httpClient = new HttpClient();

            foreach (var agencyData in agenciesData)
            {
                if (!string.IsNullOrEmpty(agencyData.ImportUrl))
                {
                    _logger.LogInformation("Starting import from url \"{url}\". {AgencyId}");
                    var response = await httpClient.GetAsync(agencyData.ImportUrl);
                    if (response.StatusCode == HttpStatusCode.OK)
                    {
                        var realtyFeed = await xmlService.GetRealtyFeed(response.Content);
                        await xmlService.UpdateObjectsAsync(realtyFeed, agencyData.Id);
                    }

                    _logger.LogInformation(
                        "Import from url \"{url}\" finished with status code {statusCode}. {AgencyId}",
                        agencyData.ImportUrl,
                        (int)response.StatusCode,
                        agencyData.Id
                    );
                }
            }

            dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
            botSettings = dbContext.BotSettings.First();
            botSettings.IsImporting = false;
            await dbContext.SaveChangesAsync();
        }
    }
}