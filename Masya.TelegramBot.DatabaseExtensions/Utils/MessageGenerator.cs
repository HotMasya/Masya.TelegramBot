using System.Linq;
using System.Text;
using Masya.TelegramBot.DataAccess.Models;
using Masya.TelegramBot.DatabaseExtensions.Types;

namespace Masya.TelegramBot.DatabaseExtensions.Utils
{
    public static class MessageGenerator
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

        public static string GenerateCreateProcessMessage(CreateProcess process)
        {
            var builder = new StringBuilder();

            if (!string.IsNullOrEmpty(process.Description))
            {
                builder.AppendLine(string.Format("📃 Description:\n _{0}_", process.Description));
            }

            if (!string.IsNullOrEmpty(process.Category))
            {
                builder.AppendLine(string.Format("🏡 Category: *{0}*", process.Category));
            }

            if (!string.IsNullOrEmpty(process.District))
            {
                builder.AppendLine(string.Format("🏢 Region: *{0}*", process.District));
            }

            if (!string.IsNullOrEmpty(process.Street))
            {
                builder.AppendLine(string.Format("🏢 Address: *{0}*", process.Street));
            }

            if (!string.IsNullOrEmpty(process.State))
            {
                builder.AppendLine(string.Format("🔨 State: *{0}*", process.State));
            }

            if (!string.IsNullOrEmpty(process.WallMaterial))
            {
                builder.AppendLine(string.Format("🔨 Walls Material: *{0}*", process.WallMaterial));
            }

            if (process.Rooms.HasValue)
            {
                builder.AppendLine(string.Format("🚪 Rooms: *{0}*", process.Rooms.Value));
            }

            if (process.Floor.HasValue)
            {
                builder.AppendLine(string.Format("🏦 Floor: *{0}*", process.Floor.Value));
            }

            if (process.TotalFloors.HasValue)
            {
                builder.AppendLine(string.Format("🏦 Total Floors: *{0}*", process.TotalFloors.Value));
            }

            if (process.TotalArea.HasValue)
            {
                builder.AppendLine(string.Format("🌏 Total Area: *{0}*", process.TotalArea.Value));
            }

            if (process.LivingSpace.HasValue)
            {
                builder.Append(string.Format("🏚 Living Area: *{0}*", process.LivingSpace.Value));
            }

            if (process.KitchenSpace.HasValue)
            {
                builder.Append(string.Format("🍽 Kitchen Area: *{0}*", process.KitchenSpace.Value));
            }

            if (process.LotArea.HasValue)
            {
                builder.Append(string.Format("🏚 Lot Area: *{0}*", process.LotArea.Value));
            }

            if (process.Price.HasValue)
            {
                builder.Append(string.Format("💵 Price: *{0}*", process.Price.Value));
            }

            return builder.ToString();
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
                "Your search settings:\n\n\nv Selected categories: *{0}*\n\n🔍 Selected regions: *{1}*\n\n🏢 Floors: *{2} {3}*\n\n🚪 Rooms: *{4}*\n\n💵 Price: *{5} {6}*",
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