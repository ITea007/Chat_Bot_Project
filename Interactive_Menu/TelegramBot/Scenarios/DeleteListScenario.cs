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
    internal class DeleteListScenario : IScenario
    {

        internal IUserService _userService;
        internal IToDoListService _toDoListService;
        internal IToDoService _toDoService;

        public DeleteListScenario(IUserService userService, IToDoListService toDoListService, IToDoService toDoService)
        {
            _userService = userService;
            _toDoListService = toDoListService;
            _toDoService = toDoService;
        }

        public bool CanHandle(ScenarioType scenario)
        {
            return scenario == ScenarioType.DeleteList;
        }

        public async Task<ScenarioResult> HandleMessageAsync(ITelegramBotClient bot, ScenarioContext context, Update update, CancellationToken ct)
        {
            switch (context.CurrentStep)
            {
                case null:
                    return await OnStartStep(bot, context, update, ct);
                case "Approve":
                    return await OnApproveStep(bot, context, update, ct);
                case "Delete":
                    return await OnDeleteStep(bot, context, update, ct);
                default:
                    return ScenarioResult.Completed;
            }
        }


        private async Task<ScenarioResult> OnStartStep(ITelegramBotClient bot, ScenarioContext context, Update update, CancellationToken ct)
        {
            if (update.CallbackQuery?.From is null || update.CallbackQuery.Message is null)
                return ScenarioResult.Completed;

            // Отвечаем на callback сразу
            await bot.AnswerCallbackQuery(update.CallbackQuery.Id, cancellationToken: ct);

            var toDoUser = await _userService.GetUser(update.CallbackQuery.From.Id, ct);
            if (toDoUser is null)
                return ScenarioResult.Completed;

            context.Data["User"] = toDoUser;
            var userLists = await _toDoListService.GetUserLists(toDoUser.UserId, ct);

            if (userLists.Count == 0)
            {
                await bot.EditMessageText(
                    update.CallbackQuery.Message.Chat.Id,
                    update.CallbackQuery.Message.MessageId,
                    "У вас нет списков для удаления.",
                    cancellationToken: ct);
                return ScenarioResult.Completed;
            }

            var keyboardButtons = userLists.Select(list =>
            {
                var callback = new ToDoListCallbackDto { Action = "select", ToDoListId = list.Id };
                return new[] { InlineKeyboardButton.WithCallbackData($"📋 {list.Name}", callback.ToString()) };
            }).ToList();

            var inlineKeyboard = new InlineKeyboardMarkup(keyboardButtons);

            await bot.EditMessageText(
                update.CallbackQuery.Message.Chat.Id,
                update.CallbackQuery.Message.MessageId,
                "Выберите список для удаления:",
                replyMarkup: inlineKeyboard,
                cancellationToken: ct);

            context.CurrentStep = "Approve";
            return ScenarioResult.Transition;
        }

        private async Task<ScenarioResult> OnApproveStep(ITelegramBotClient bot, ScenarioContext context, Update update, CancellationToken ct)
        {
            if (update.CallbackQuery is null)
                return ScenarioResult.Completed;
            if (update.CallbackQuery.Message is null) throw new ArgumentNullException(nameof(update.CallbackQuery.Message));
            var callbackData = ToDoListCallbackDto.FromString(update.CallbackQuery.Data ?? "");
            if (!callbackData.ToDoListId.HasValue)
                return ScenarioResult.Completed;

            var list = await _toDoListService.Get(callbackData.ToDoListId.Value, ct);
            if (list is null)
                return ScenarioResult.Completed;

            context.Data["SelectedList"] = list;

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
                $"Подтверждаете удаление списка '{list.Name}' и всех его задач?",
                replyMarkup: inlineKeyboard,
                cancellationToken: ct);

            context.CurrentStep = "Delete";
            return ScenarioResult.Transition;
        }

        private async Task<ScenarioResult> OnDeleteStep(ITelegramBotClient bot, ScenarioContext context, Update update, CancellationToken ct)
        {
            if (update.CallbackQuery is null)
                return ScenarioResult.Completed;
            if (update.CallbackQuery.Message is null) throw new ArgumentNullException(nameof(update.CallbackQuery.Message));

            var list = context.Data["SelectedList"] as ToDoList;
            if (list is null)
                return ScenarioResult.Completed;

            if (update.CallbackQuery.Data == "yes")
            {
                // Удаляем задачи списка и сам список
                var userTasks = await _toDoService.GetByUserIdAndList(list.User.UserId, list.Id, ct);
                foreach (var task in userTasks)
                {
                    await _toDoService.Delete(task.Id, ct);
                }

                await _toDoListService.Delete(list.Id, ct);

                await bot.EditMessageText(
                    update.CallbackQuery.Message.Chat.Id,
                    update.CallbackQuery.Message.MessageId,
                    $"Список '{list.Name}' и все его задачи удалены.",
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
