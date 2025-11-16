using Interactive_Menu.Core.Entities;
using Interactive_Menu.Core.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;
using Telegram.Bot;
using Telegram.Bot.Types;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace Interactive_Menu.TelegramBot.Scenarios
{
    internal class AddListScenario : IScenario
    {
        internal IUserService _userService;
        internal IToDoListService _toDoListService;

        public AddListScenario(IUserService userService, IToDoListService toDoListService) 
        {
            _userService = userService;
            _toDoListService = toDoListService;
        }

        public bool CanHandle(ScenarioType scenario)
        {
            return scenario == ScenarioType.AddList;
        }

        //Обработка шагов сценария (ScenarioContext.CurrentStep) через switch case
        public async Task<ScenarioResult> HandleMessageAsync(ITelegramBotClient bot, ScenarioContext context, Update update, CancellationToken ct)
        {
            switch (context.CurrentStep)
            {
                case null:
                    return await OnStartStep(bot, context, update, ct);
                case "Name":
                    return await OnNameStep(bot, context, update, ct);
                default:
                    return ScenarioResult.Completed;
            }
        }

        //Получить ToDoUser и сохранить его в ScenarioContext.Data.
        //Отправить пользователю сообщение "Введите название списка:"
        //Обновить ScenarioContext.CurrentStep на "Name"
        //Вернуть ScenarioResult.Transition
        private async Task<ScenarioResult> OnStartStep(ITelegramBotClient bot, ScenarioContext context, Update update, CancellationToken ct)
        {
            long telegramUserId;
            long chatId;

            if (update.CallbackQuery?.From is not null && update.CallbackQuery.Message is not null)
            {
                telegramUserId = update.CallbackQuery.From.Id;
                chatId = update.CallbackQuery.Message.Chat.Id;

                // Отвечаем на callback сразу
                await bot.AnswerCallbackQuery(update.CallbackQuery.Id, cancellationToken: ct);
            }
            else if (update.Message?.From is not null)
            {
                telegramUserId = update.Message.From.Id;
                chatId = update.Message.Chat.Id;
            }
            else
            {
                return ScenarioResult.Completed;
            }

            var toDoUser = await _userService.GetUser(telegramUserId, ct);
            if (toDoUser is null)
                return ScenarioResult.Completed;

            context.Data["User"] = toDoUser;

            await bot.SendMessage(chatId, "Введите название списка (максимум 10 символов):", cancellationToken: ct);

            context.CurrentStep = "Name";
            return ScenarioResult.Transition;
        }


        //Вызвать IToDoListService.Add.Передать ToDoUser из ScenarioContext.Data и name из сообщения
        //Вернуть ScenarioResult.Completed
        private async Task<ScenarioResult> OnNameStep(ITelegramBotClient bot, ScenarioContext context, Update update, CancellationToken ct)
        {
            if (update.Message is null || update.Message.Text is null)
                return ScenarioResult.Completed;

            var user = context.Data["User"] as ToDoUser;
            if (user is null)
                return ScenarioResult.Completed;

            try
            {
                var listName = update.Message.Text.Trim();
                var newList = await _toDoListService.Add(user, listName, ct);

                await bot.SendMessage(
                    update.Message.Chat.Id,
                    $"Список '{newList.Name}' успешно создан!",
                    cancellationToken: ct);

                return ScenarioResult.Completed;
            }
            catch (Exception ex)
            {
                await bot.SendMessage(
                    update.Message.Chat.Id,
                    $"Ошибка при создании списка: {ex.Message}",
                    cancellationToken: ct);

                return ScenarioResult.Completed;
            }
        }

    }
}
