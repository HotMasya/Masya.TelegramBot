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
            var xmlUrls = dbContext.Agencies.Select(a => a.ImportUrl).ToList();
            var httpClient = new HttpClient();

            foreach (var url in xmlUrls)
            {
                if (!string.IsNullOrEmpty(url))
                {
                    var response = await httpClient.GetAsync(url);
                    if (response.StatusCode == HttpStatusCode.OK)
                    {
                        var realtyFeed = await xmlService.GetRealtyFeed(response.Content);
                        await xmlService.UpdateObjectsAsync(realtyFeed);
                    }

                    _logger.LogInformation("{0} - Finished with status code: {1}", url, (int)response.StatusCode);
                }
            }
        }
    }
}