using Interactive_Menu.Core.Entities;
using Interactive_Menu.Core.Services;
using Interactive_Menu.TelegramBot.Dto;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.ReplyMarkups;

namespace Interactive_Menu.TelegramBot.Scenarios
{
    internal class DeleteTaskScenario : IScenario
    {
        private readonly IToDoService _toDoService;

        public DeleteTaskScenario(IToDoService toDoService)
        {
            _toDoService = toDoService;
        }

        public bool CanHandle(ScenarioType scenario)
        {
            return scenario == ScenarioType.DeleteTask;
        }

        public async Task<ScenarioResult> HandleMessageAsync(ITelegramBotClient bot, ScenarioContext context, Update update, CancellationToken ct)
        {
            switch (context.CurrentStep)
            {
                case null:
                    return await OnStartStep(bot, context, update, ct);
                case "Approve":
                    return await OnApproveStep(bot, context, update, ct);
                default:
                    return ScenarioResult.Completed;
            }
        }

        private async Task<ScenarioResult> OnStartStep(ITelegramBotClient bot, ScenarioContext context, Update update, CancellationToken ct)
        {
            if (update.CallbackQuery?.From is null || update.CallbackQuery.Message is null)
                return ScenarioResult.Completed;

            await bot.AnswerCallbackQuery(update.CallbackQuery.Id, cancellationToken: ct);

            if (!context.Data.ContainsKey("ToDoItemId"))
            {
                var itemCallback = ToDoItemCallbackDto.FromString(update.CallbackQuery.Data ?? "");
                context.Data["ToDoItemId"] = itemCallback.ToDoItemId;
            }

            var taskId = (Guid)context.Data["ToDoItemId"];
            var task = await _toDoService.Get(taskId, ct);

            if (task is null)
            {
                await bot.EditMessageText(
                    update.CallbackQuery.Message.Chat.Id,
                    update.CallbackQuery.Message.MessageId,
                    "Задача не найдена.",
                    cancellationToken: ct);
                return ScenarioResult.Completed;
            }

            context.Data["Task"] = task;

            var inlineKeyboard = new InlineKeyboardMarkup(new[]
            {
                new[]
                {
                    InlineKeyboardButton.WithCallbackData("✅ Да", "yes"),
                    InlineKeyboardButton.WithCallbackData("❌ Нет", "no")
                }
            });

            await bot.EditMessageText(
                update.CallbackQuery.Message.Chat.Id,
                update.CallbackQuery.Message.MessageId,
                $"Подтверждаете удаление задачи '{task.Name}'?",
                replyMarkup: inlineKeyboard,
                cancellationToken: ct);

            context.CurrentStep = "Approve";
            return ScenarioResult.Transition;
        }

        private async Task<ScenarioResult> OnApproveStep(ITelegramBotClient bot, ScenarioContext context, Update update, CancellationToken ct)
        {
            if (update.CallbackQuery is null || update.CallbackQuery.Message is null)
                return ScenarioResult.Completed;

            var task = context.Data["Task"] as ToDoItem;
            if (task is null)
                return ScenarioResult.Completed;

            if (update.CallbackQuery.Data == "yes")
            {
                await _toDoService.Delete(task.Id, ct);

                await bot.EditMessageText(
                    update.CallbackQuery.Message.Chat.Id,
                    update.CallbackQuery.Message.MessageId,
                    $"Задача '{task.Name}' удалена.",
                    cancellationToken: ct);
            }
            else
            {
                await bot.EditMessageText(
                    update.CallbackQuery.Message.Chat.Id,
                    update.CallbackQuery.Message.MessageId,
                    "Удаление отменено.",
                    cancellationToken: ct);
            }

            return ScenarioResult.Completed;
        }

    }
}
