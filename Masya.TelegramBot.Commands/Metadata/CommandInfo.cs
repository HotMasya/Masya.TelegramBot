using Masya.TelegramBot.Commands.Attributes;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Reflection;
using System.Text;

namespace Masya.TelegramBot.Commands.Metadata
{
    public class CommandInfo<TAliasInfo> : IFormattable where TAliasInfo : AliasInfo
    {
        public string Name { get; set; }
        public MethodInfo MethodInfo { get; set; }
        public string Description { get; set; }
        public IList<TAliasInfo> Aliases { get; }
        public bool IsEnabled { get; set; }

        private static readonly string DefaultDescription = "описание отсутствует.";

        public CommandInfo() { }

        public CommandInfo(
            string name,
            string description,
            MethodInfo methodInfo,
            IList<TAliasInfo> aliases = null
        )
        {
            Name = name;
            Description = description ?? DefaultDescription;
            Aliases = aliases ?? new List<TAliasInfo>();
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
