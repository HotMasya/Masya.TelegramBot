using System.Linq;
using System.Text;
using Masya.TelegramBot.DataAccess.Models;

namespace Masya.TelegramBot.DatabaseExtensions
{
    public static class MessageGenerators
    {
        public static string GenerateMenuMessage(User user)
        {
            var fullName = user.TelegramFirstName + (
                string.IsNullOrEmpty(user.TelegramLastName)
                    ? ""
                    : " " + user.TelegramLastName
            );

            return string.Format(
                "Welcome back, *{0}*!\nYour status: *{1}*.\nYour are in main menu now.",
                fullName,
                user.Permission.ToString()
            );
        }

        public static string GenerateSearchSettingsMessage(UserSettings userSettings)
        {
            var selCategories = userSettings.SelectedCategories != null && userSettings.SelectedCategories.Count > 0
                ? string.Join(", ", userSettings.SelectedCategories.Select(c => c.Name))
                : "any";

            var selRegions = userSettings.SelectedRegions != null && userSettings.SelectedRegions.Count > 0
                ? string.Join(", ", userSettings.SelectedRegions.Select(r => r.Value))
                : "any";

            var selRooms = userSettings.Rooms != null && userSettings.Rooms.Any()
                ? string.Join(", ", userSettings.Rooms.Select(r => r.RoomsCount.ToString()).OrderBy(r => r))
                : "any";

            var minFloor = userSettings.MinFloor.HasValue
                ? "from " + userSettings.MinFloor.Value.ToString()
                : userSettings.MaxFloor.HasValue
                    ? ""
                    : "any";

            var maxFloor = userSettings.MaxFloor.HasValue
                ? "to " + userSettings.MaxFloor.Value.ToString()
                : string.Empty;

            var minPrice = userSettings.MinPrice.HasValue
                ? "from " + userSettings.MinPrice.Value.ToString()
                : userSettings.MaxPrice.HasValue
                    ? ""
                    : "any";

            var maxPrice = userSettings.MaxPrice.HasValue
                ? "to " + userSettings.MaxPrice.Value.ToString()
                : string.Empty;

            return string.Format(
                "Your search settings:\n\n\n🏡 Selected categories: *{0}*\n\n🔍 Selected regions: *{1}*\n\n🏢 Floors: *{2} {3}*\n\n🚪 Rooms: *{4}*\n\n💵 Price: *{5} {6}*",
                selCategories,
                selRegions,
                minFloor,
                maxFloor,
                selRooms,
                minPrice,
                maxPrice
            );
        }
    }
}