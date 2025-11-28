using Interactive_Menu.TelegramBot.Helpers;
using Interactive_Menu.TelegramBot.Scenarios;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Telegram.Bot;

namespace Interactive_Menu.BackgroundTasks
{
    internal class ResetScenarioBackgroundTask : BackgroundTask
    {
        private readonly TimeSpan _resetScenarioTimeout;
        private readonly IScenarioContextRepository _scenarioRepository;
        private readonly ITelegramBotClient _bot;
        private readonly Helper _helper;

        public ResetScenarioBackgroundTask(
            TimeSpan resetScenarioTimeout,
            IScenarioContextRepository scenarioRepository,
            ITelegramBotClient bot,
            Helper helper)
            : base(TimeSpan.FromHours(1), nameof(ResetScenarioBackgroundTask))
        {
            _resetScenarioTimeout = resetScenarioTimeout;
            _scenarioRepository = scenarioRepository;
            _bot = bot;
            _helper = helper;
        }

        protected override async Task Execute(CancellationToken ct)
        {
            var contexts = await _scenarioRepository.GetContexts(ct);
            var now = DateTime.UtcNow;

            foreach (var context in contexts)
            {
                if (now - context.CreatedAt > _resetScenarioTimeout)
                {
                    try
                    {
                        await _scenarioRepository.ResetContext(context.UserId, ct);

                        await _bot.SendMessage(
                            context.UserId,
                            $"Сценарий отменен, так как не поступил ответ в течение {_resetScenarioTimeout}",
                            replyMarkup: _helper._keyboardAfterRegistration,
                            cancellationToken: ct);

                        Console.WriteLine($"Reset scenario for user {context.UserId} due to timeout");
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"Error resetting scenario for user {context.UserId}: {ex.Message}");
                    }
                }
            }
        }

    }
}
