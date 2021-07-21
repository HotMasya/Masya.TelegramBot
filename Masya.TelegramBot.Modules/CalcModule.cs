using Masya.RPNCalculator.Core.Abstractions;
using Masya.TelegramBot.Commands;
using Masya.TelegramBot.Commands.Attributes;
using System.Threading.Tasks;

namespace Masya.TelegramBot.Modules
{
    public class CalcModule : Module
    {
        private readonly ICalculatorFactory _factory;

        public CalcModule(ICalculatorFactory factory)
        {
            _factory = factory;
        }

        [Command("calculator")]
        [Alias("calc", "clc")]
        public async Task CalculatorCommandAsync([ParamName("выражение")]string expression)
        {
            var calc = _factory.CreateRPNCalculator();
            double result = calc.Calculate(expression);
            await ReplyAsync("Результат вычисления: " + result);
        }

        [Command("notation")]
        [Alias("not")]
        public async Task NotationCommandAsync([ParamName("выражение")] string expression)
        {
            var parser = _factory.CreateExpressionParser();
            var result = parser.Parse(expression);
            await ReplyAsync("Выражение в обратной польской нотации: " + result.ToString());
        }
    }
}
