using Interactive_Menu.Core.Entities;
using Interactive_Menu.Core.Exceptions;
using Interactive_Menu.Core.Services;
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
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace Interactive_Menu.TelegramBot.Scenarios
{
    internal class AddTaskScenario : IScenario
    {
        private IUserService _userService;
        private IToDoService _toDoService;
        private Helper _helper;

        public AddTaskScenario(IUserService userService, IToDoService toDoService, Helper helper)
        {
            _userService = userService;
            _toDoService = toDoService;
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
                case "Deadline":return await OnDeadlineStep(bot, context, update, ct);
                default: return ScenarioResult.Completed;
            }
        }

        private async Task<ScenarioResult> OnUserStep(ITelegramBotClient bot, ScenarioContext context, Update update, CancellationToken ct)
        {
            if (update.Message is null) throw new ArgumentNullException();
            if (update.Message.From is null) throw new ArgumentNullException();
            var toDoUser = await _userService.GetUser(update.Message.From.Id, ct);
            if (toDoUser is null) throw new UserNotFoundException(update.Message.From.Id);
            context.Data.Add("User", toDoUser);
            await bot.SendMessage(update.Message.Chat.Id, "Введите название задачи:", replyMarkup: _helper._cancelKeyboard, cancellationToken: ct);
            context.CurrentStep = "Name";
            return ScenarioResult.Transition;
        }

        private async Task<ScenarioResult> OnNameStep(ITelegramBotClient bot, ScenarioContext context, Update update, CancellationToken ct)
        {
            if (update.Message is null) throw new ArgumentNullException();
            if (update.Message.Text is null) throw new ArgumentNullException();
            context.Data.Add("Name", update.Message.Text.Trim());
            await bot.SendMessage(update.Message.Chat.Id, $"Введите срок выполнения задачи в формате dd.MM.yyyy", replyMarkup: _helper._cancelKeyboard, cancellationToken: ct);
            //Попробовать добавить сюда выбор даты из календаря в телеге
            context.CurrentStep = "Deadline";
            return ScenarioResult.Transition;
        }

        private async Task<ScenarioResult> OnDeadlineStep(ITelegramBotClient bot, ScenarioContext context, Update update, CancellationToken ct)
        {
            if (update.Message is null) throw new ArgumentNullException();
            if (update.Message.Text is null) throw new ArgumentNullException();
            if (DateTime.TryParseExact(update.Message.Text.Trim(), "dd.MM.yyyy", CultureInfo.InvariantCulture, DateTimeStyles.None, out DateTime deadline))
            {
                context.Data.Add("Deadline", deadline);
                var userObject = context.Data.Where(i => i.Key == "User").FirstOrDefault().Value;
                var nameObject = context.Data.Where(i => i.Key == "Name").FirstOrDefault().Value;

                if (userObject is ToDoUser user && nameObject is string name)
                {
                    await _toDoService.Add(user, name, deadline, ct);
                    await bot.SendMessage(update.Message.Chat.Id, $"Задача '{update.Message.Text}' добавлена.", replyMarkup: _helper._keyboardAfterRegistration, cancellationToken: ct);
                    return ScenarioResult.Completed;
                }
                else throw new InvalidOperationException("User object has incorrect type.");
            }
            else
            {
                await bot.SendMessage(update.Message.Chat.Id, $"Неверный формат! Введите срок выполнения задачи в формате dd.MM.yyyy", replyMarkup: _helper._cancelKeyboard, cancellationToken: ct);
                context.CurrentStep = "Deadline";
                return ScenarioResult.Transition;
            }
        }
    }
}
