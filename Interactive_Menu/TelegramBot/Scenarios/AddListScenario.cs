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
            throw new NotImplementedException();
        }

        public Task<ScenarioResult> HandleMessageAsync(ITelegramBotClient bot, ScenarioContext context, Update update, CancellationToken ct)
        {
            throw new NotImplementedException();
            //Добавить обработку шагов сценария (ScenarioContext.CurrentStep) через switch case
            //case null
                //Получить ToDoUser и сохранить его в ScenarioContext.Data.
                //Отправить пользователю сообщение "Введите название списка:"
                //Обновить ScenarioContext.CurrentStep на "Name"
                //Вернуть ScenarioResult.Transition
            //case "Name"
                //Вызвать IToDoListService.Add.Передать ToDoUser из ScenarioContext.Data и name из сообщения
                //Вернуть ScenarioResult.Completed

            //При нажатии на кнопку "🆕Добавить" должен запускаться сценарий AddListScenario
        }
    }
}
