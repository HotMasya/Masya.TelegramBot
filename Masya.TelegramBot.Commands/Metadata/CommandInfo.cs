using Masya.TelegramBot.Commands.Attributes;
using Masya.TelegramBot.DataAccess.Models;
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
        public MethodInfo MethodInfo { get; }
        public string Description { get; }
        public IList<AliasInfo> Aliases { get; }
        public bool? IsEnabled { get; set; }
        public Permission? Permission { get; set; }

        private static readonly string DefaultDescription = "описание отсутствует.";

        public CommandInfo(
            string name,
            string description,
            MethodInfo methodInfo,
            IList<AliasInfo> aliases = null
        )
        {
            Name = name;
            Description = description ?? DefaultDescription;
            Aliases = aliases ?? new List<AliasInfo>();
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
