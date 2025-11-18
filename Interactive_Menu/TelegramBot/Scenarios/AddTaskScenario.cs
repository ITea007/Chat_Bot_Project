using Interactive_Menu.Core.Entities;
using Interactive_Menu.Core.Exceptions;
using Interactive_Menu.Core.Services;
using Interactive_Menu.TelegramBot.Dto;
using Microsoft.VisualBasic;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.ReplyMarkups;

namespace Interactive_Menu.TelegramBot.Scenarios
{
    internal class AddTaskScenario : IScenario
    {
        private IUserService _userService;
        private IToDoService _toDoService;
        private readonly IToDoListService _toDoListService;
        private Helper _helper;

        public AddTaskScenario(IUserService userService, IToDoService toDoService, IToDoListService toDoListService, Helper helper)
        {
            _userService = userService;
            _toDoService = toDoService;
            _toDoListService = toDoListService;
            _helper = helper;
        }
        public bool CanHandle(ScenarioType scenario)
        {
            if (scenario == ScenarioType.AddTask)
                return true;
            else
                return false;
        }

        public async Task<ScenarioResult> HandleMessageAsync(ITelegramBotClient bot, ScenarioContext context, Update update, CancellationToken ct)
        {
            switch (context.CurrentStep)
            {
                case null: return await OnUserStep(bot, context, update, ct);                   
                case "Name": return await OnNameStep(bot, context, update, ct);
                case "Deadline": return await OnDeadlineStep(bot, context, update, ct);
                case "ToDoList": return await OnToDoListStep(bot, context, update, ct);
                default: return ScenarioResult.Completed;
            }
        }

        //Добавить заполнение ToDoList в AddTaskScenario через отдельный шаг
        //Выбирать список нужно через Inline кнопки(см.Демонстрация работы бота)
        //Обработка update.CallbackQuery должна быть внутри AddTaskScenario
        private async Task<ScenarioResult> OnToDoListStep(ITelegramBotClient bot, ScenarioContext context, Update update, CancellationToken ct)
        {
            if (update.CallbackQuery is null || update.CallbackQuery.Message is null)
            {
                // Если пришло сообщение вместо callback, снова показываем выбор списка
                if (update.Message?.Chat.Id is long chatId)
                {
                    await ShowListSelection(bot, context, chatId, ct);
                }
                return ScenarioResult.Transition;
            }

            var callbackData = ToDoListCallbackDto.FromString(update.CallbackQuery.Data ?? "");
            if (callbackData.Action != "select")
                return ScenarioResult.Transition;

            // Получаем данные из контекста
            var user = context.Data["User"] as ToDoUser;
            var name = context.Data["Name"] as string;
            var deadline = context.Data["Deadline"] as DateTime?;

            if (user is null || name is null || !deadline.HasValue)
            {
                await bot.SendMessage(
                    update.CallbackQuery.Message!.Chat.Id,
                    "Ошибка: данные задачи утеряны. Начните заново.",
                    replyMarkup: _helper._keyboardAfterRegistration,
                    cancellationToken: ct);
                return ScenarioResult.Completed;
            }

            // Получаем выбранный список
            ToDoList? selectedList = null;
            if (callbackData.ToDoListId.HasValue)
            {
                selectedList = await _toDoListService.Get(callbackData.ToDoListId.Value, ct);
            }

            // Создаем задачу
            try
            {
                var task = await _toDoService.Add(user, name, deadline.Value, selectedList, ct);
                var listName = selectedList?.Name ?? "без списка";

                await bot.SendMessage(
                    update.CallbackQuery.Message.Chat.Id,
                    $"✅ Задача '{name}' добавлена в список '{listName}'.",
                    replyMarkup: _helper._keyboardAfterRegistration,
                    cancellationToken: ct);

                return ScenarioResult.Completed;
            }
            catch (Exception ex)
            {
                await bot.SendMessage(
                    update.CallbackQuery.Message.Chat.Id,
                    $"❌ Ошибка при добавлении задачи: {ex.Message}",
                    replyMarkup: _helper._keyboardAfterRegistration,
                    cancellationToken: ct);

                return ScenarioResult.Completed;
            }
        }

        private async Task<ScenarioResult> OnUserStep(ITelegramBotClient bot, ScenarioContext context, Update update, CancellationToken ct)
        {
            if (update.Message is null) throw new ArgumentNullException(nameof(update.Message));
            if (update.Message.From is null) throw new ArgumentNullException(nameof(update.Message.From));
            var toDoUser = await _userService.GetUser(update.Message.From.Id, ct);
            if (toDoUser is null) throw new UserNotFoundException(update.Message.From.Id);
            context.Data.Add("User", toDoUser);
            await bot.SendMessage(update.Message.Chat.Id, "Введите название задачи:", replyMarkup: _helper._cancelKeyboard, cancellationToken: ct);
            context.CurrentStep = "Name";
            return ScenarioResult.Transition;
        }

        private async Task<ScenarioResult> OnNameStep(ITelegramBotClient bot, ScenarioContext context, Update update, CancellationToken ct)
        {
            if (update.Message is null) throw new ArgumentNullException(nameof(update.Message));
            if (update.Message.Text is null) throw new ArgumentNullException(nameof(update.Message.Text));
            context.Data.Add("Name", update.Message.Text.Trim());
            await bot.SendMessage(update.Message.Chat.Id, $"Введите срок выполнения задачи в формате dd.MM.yyyy", replyMarkup: _helper._cancelKeyboard, cancellationToken: ct);
            //Попробовать добавить сюда выбор даты из календаря в телеге
            context.CurrentStep = "Deadline";
            return ScenarioResult.Transition;
        }

        private async Task<ScenarioResult> OnDeadlineStep(ITelegramBotClient bot, ScenarioContext context, Update update, CancellationToken ct)
        {
            if (update.Message is null || update.Message.Text is null)
                return ScenarioResult.Transition;

            if (DateTime.TryParseExact(update.Message.Text.Trim(), "dd.MM.yyyy", CultureInfo.InvariantCulture, DateTimeStyles.None, out DateTime deadline))
            {
                context.Data["Deadline"] = deadline;

                // Переход к выбору списка
                await ShowListSelection(bot, context, update.Message.Chat.Id, ct);
                context.CurrentStep = "ToDoList";
                return ScenarioResult.Transition;
            }
            else
            {
                await bot.SendMessage(
                    update.Message.Chat.Id,
                    $"Неверный формат! Введите срок выполнения задачи в формате dd.MM.yyyy",
                    replyMarkup: _helper._cancelKeyboard,
                    cancellationToken: ct);
                return ScenarioResult.Transition;
            }
        }

        // Метод для показа выбора списка:
        private async Task ShowListSelection(ITelegramBotClient bot, ScenarioContext context, long chatId, CancellationToken ct)
        {
            var user = context.Data["User"] as ToDoUser;
            if (user is null) return;

            var userLists = await _toDoListService.GetUserLists(user.UserId, ct);
            var keyboardButtons = new List<InlineKeyboardButton[]>();

            // Кнопка "Без списка"
            var noListCallback = new ToDoListCallbackDto { Action = "select", ToDoListId = null };
            keyboardButtons.Add(new[] { InlineKeyboardButton.WithCallbackData("📌 Без списка", noListCallback.ToString()) });

            // Кнопки для каждого списка
            foreach (var list in userLists)
            {
                var listCallback = new ToDoListCallbackDto { Action = "select", ToDoListId = list.Id };
                keyboardButtons.Add(new[] { InlineKeyboardButton.WithCallbackData($"📋 {list.Name}", listCallback.ToString()) });
            }

            var inlineKeyboard = new InlineKeyboardMarkup(keyboardButtons);

            await bot.SendMessage(
                chatId,
                "Выберите список для задачи (или 'Без списка'):",
                replyMarkup: inlineKeyboard,
                cancellationToken: ct);
        }
    }
}
