using System.Linq;
using System.Text;
using Masya.TelegramBot.DataAccess.Models;

namespace Masya.TelegramBot.Modules
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
            var selCategories = string.Empty;
            foreach (var cat in userSettings.SelectedCategories)
            {
                selCategories += cat.Name + " ";
            }

            selCategories = string.IsNullOrEmpty(selCategories) ? "any" : selCategories.TrimEnd();

            var selRegionsBuilder = new StringBuilder();
            foreach (var reg in userSettings.SelectedRegions)
            {
                selRegionsBuilder.Append(reg.Value + " ");
            }

            var selRegions = selRegionsBuilder.ToString();
            selRegions = string.IsNullOrEmpty(selRegions) ? "any" : selRegions.TrimEnd();

            var selRooms = userSettings.Rooms.Any()
                ? string.Join(", ", userSettings.Rooms.Select(r => r.RoomsCount.ToString()))
                : "any";

            var minFloor = userSettings.MinFloor.HasValue
                ? "from " + userSettings.MinFloor.Value.ToString()
                : "any";

            var maxFloor = userSettings.MaxFloor.HasValue
                ? "to " + userSettings.MaxFloor.Value.ToString()
                : string.Empty;

            var minPrice = userSettings.MinPrice.HasValue
                ? "from " + userSettings.MinPrice.Value.ToString()
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