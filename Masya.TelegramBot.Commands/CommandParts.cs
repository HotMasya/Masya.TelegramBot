using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Reflection;
using Masya.TelegramBot.Commands.Attributes;
using Masya.TelegramBot.Commands.Options;

namespace Masya.TelegramBot.Commands
{
    public sealed class CommandParts
    {
        private string _name;
        private readonly CommandServiceOptions _options;

        private static Type[] floatingTypes = new[] { typeof(double), typeof(float), typeof(decimal) };

        public string Name => _name;
        public string FullName => _options.Prefix + _name;
        public string[] ArgsStr { get; private set; }

        public CommandParts(string content, CommandServiceOptions options)
        {
            _options = options;
            ExtractContent(content);
        }

        private void ExtractContent(string content)
        {
            if (string.IsNullOrEmpty(content))
            {
                throw new InvalidOperationException("Unable to extract message content into command parts. The content was empty.");
            }

            string[] cmdParts = content.Split(_options.ArgsSeparator);
            _name = cmdParts[0].Substring(1).ToLower();
            ArgsStr = cmdParts.Length > 1 ? cmdParts.Skip(1).ToArray() : Array.Empty<string>();
        }

        public object MatchTypeParam(ParameterInfo param, string value, int resultCount)
        {
            if (param.GetCustomAttribute<RemainderAttribute>() != null && param.GetCustomAttribute<ParamArrayAttribute>() != null)
            {
                string[] splittedValue = value.Split(_options.ArgsSeparator);
                Array arrayTypeParam = Array.CreateInstance(param.ParameterType.GetElementType(), splittedValue.Length);
                for (int i = 0; i < splittedValue.Length; i++)
                {
                    arrayTypeParam.SetValue(Convert.ChangeType(splittedValue[i], param.ParameterType.GetElementType()), i);
                }
                return arrayTypeParam;
            }

            if (param.ParameterType == typeof(TimeSpan)) return ParseAsTimespan(param, value);

            if (floatingTypes.Any(t => param.ParameterType == t) && value.Contains('.'))
            {
                value = value.Replace('.', ',');
            }

            return Convert.ChangeType(value, param.ParameterType);
        }

        public object MatchTypeParam(ParameterInfo param, int resultCount)
        {
            if (param.GetCustomAttribute<RemainderAttribute>() != null && param.GetCustomAttribute<ParamArrayAttribute>() != null)
            {
                Array arrayTypeParam = Array.CreateInstance(param.ParameterType.GetElementType(), ArgsStr.Length - resultCount);
                for (int i = resultCount, j = 0; i < ArgsStr.Length; i++, j++)
                {
                    arrayTypeParam.SetValue(Convert.ChangeType(ArgsStr[i], param.ParameterType.GetElementType()), j);
                }
                return arrayTypeParam;
            }

            if (param.IsOptional && ArgsStr.Length <= resultCount)
            {
                return Type.Missing;
            }

            string paramToCheck = ArgsStr[resultCount];
            if (floatingTypes.Any(t => param.ParameterType == t) && paramToCheck.Contains('.'))
            {
                paramToCheck = paramToCheck.Replace('.', ',');
            }

            return Convert.ChangeType(paramToCheck, param.ParameterType);
        }

        public object[] MatchParamTypes(MethodInfo info)
        {
            ParameterInfo[] mParams = info.GetParameters();

            int reqParamsCount = mParams.Where(p => !p.IsOptional && p.GetCustomAttribute<RemainderAttribute>() == null).Count();

            if (ArgsStr.Length < reqParamsCount)
            {
                throw new TargetParameterCountException("Parameter count mismatch.");
            }

            List<object> result = new List<object>();

            foreach (var p in mParams)
            {
                result.Add(MatchTypeParam(p, result.Count));
            }

            return result.ToArray();
        }

        private TimeSpan ParseAsTimespan(ParameterInfo param, string input)
        {
            if(param.ParameterType == typeof(TimeSpan) && !string.IsNullOrEmpty(input))
            {
                var formatAttr = param.GetCustomAttribute<ParseFormatAttribute>();
                if (formatAttr?.Format != null)
                {
                    return TimeSpan.ParseExact(input, formatAttr.Format, CultureInfo.InvariantCulture);
                }
                return TimeSpan.Parse(input);
            }

            throw new ArgumentNullException(nameof(input), "The timespan input was null or empty.");
        }
    }
}