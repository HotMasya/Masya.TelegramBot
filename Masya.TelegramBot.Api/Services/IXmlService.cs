using System;
using System.Threading;
using System.Threading.Tasks;
using Masya.TelegramBot.Api.Options;

namespace Masya.TelegramBot.Api.Services
{
    public interface IXmlService
    {
        IServiceProvider Services { get; }
        XmlOptions Options { get; }
        Task StartWatching(CancellationToken token = default);
    }
}