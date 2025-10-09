using Interactive_Menu.Core.Entities;
using Interactive_Menu.Core.Exceptions;
using Interactive_Menu.Core.Services;
using System;
using System.Collections.Generic;
using System.Diagnostics;
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
            if (update.Message is null) throw new ArgumentNullException();
            switch (context.CurrentStep)
            {
                case null:
                    if (update.Message.From is null) throw new ArgumentNullException();
                    var toDoUser = await _userService.GetUser(update.Message.From.Id, ct);
                    if (toDoUser is null) throw new UserNotFoundException(update.Message.From.Id);
                    context.Data.Add("User", toDoUser);
                    await bot.SendMessage(update.Message.Chat.Id, "Введите название задачи:", replyMarkup: _helper._cancelKeyboard, cancellationToken: ct);
                    context.CurrentStep = "Name";
                    return ScenarioResult.Transition;
                case "Name":
                    if (update.Message.Text is null) throw new ArgumentNullException();
                    var userValue = context.Data.Where(i => i.Key == "User").FirstOrDefault().Value;
                    if (userValue is ToDoUser user)
                    {
                        await _toDoService.Add(user, update.Message.Text, ct);
                        await bot.SendMessage(update.Message.Chat.Id, $"Задача '{update.Message.Text}' добавлена.", replyMarkup: _helper._keyboardAfterRegistration, cancellationToken: ct);
                    }
                    else
                    {
                        throw new InvalidOperationException("User object has incorrect type.");
                    }
                    //bot.SendMessage(update.Message.Chat.Id, "Выберите команду:");
                    return ScenarioResult.Completed;
            }
            return ScenarioResult.Completed;
        }
    }
}
