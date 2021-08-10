using Masya.TelegramBot.Commands.Attributes;
using Masya.TelegramBot.Commands.Data;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Reflection;
using System.Text;

namespace Masya.TelegramBot.Commands.Metadata
{
    public class CommandInfo : IFormattable
    {
        public string Name { get; }
        [JsonIgnore]
        public MethodInfo MethodInfo { get; }
        public string Description { get; }
        public IList<Alias> Aliases { get; }

        private static readonly string DefaultDescription = "описание отсутствует.";

        public CommandInfo(
            string name,
            string description,
            IList<Alias> aliases,
            MethodInfo methodInfo
        )
        {
            Name = name;
            Description = description ?? DefaultDescription;
            Aliases = aliases ?? new List<Alias>();
            MethodInfo = methodInfo;
        }

        private string MethodParamsToString()
        {
            var builder = new StringBuilder();

            foreach (var param in MethodInfo.GetParameters())
            {
                string paramName = param.GetCustomAttribute<ParamNameAttribute>()?.Name ?? param.Name;
                string textParam = string.Format(
                    " {0}{1}{2}",
                    param.IsOptional ? '[' : '<',
                    paramName,
                    param.IsOptional ? ']' : '>'
                    );
                builder.Append(textParam);
            }

            return builder.ToString();
        }

        public override string ToString()
        {
            return string.Format("{0} {1} - {2}", Name, MethodParamsToString(), Description);
        }

        //  Command formatting
        private static readonly string CommandFormat = "cmd";
        private static readonly string ParamsFormat = "params";
        private static readonly string DescriptionFormat = "descr";

        public string ToString(string format, IFormatProvider formatProvider)
        {
            return format
                .ToLowerInvariant()
                .Replace(CommandFormat, Name)
                .Replace(ParamsFormat, MethodParamsToString())
                .Replace(DescriptionFormat, Description)
                .ToString(formatProvider ?? CultureInfo.CurrentCulture);
        }
    }
}
