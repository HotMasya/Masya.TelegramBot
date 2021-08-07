using Microsoft.AspNetCore.Routing;

namespace Microsoft.AspNetCore.Builder
{
    public static class EndpointExtensions
    {
        public static ControllerActionEndpointConventionBuilder MapTelegramUpdatesRoute(
            this IEndpointRouteBuilder endpoint,
             string botToken)
        {
            return endpoint.MapControllerRoute("telegram_bot_update_route",
                $"/telegram/update{botToken}",
                new { Controller = "Bot", Action = "Index" });
        }
    }
}