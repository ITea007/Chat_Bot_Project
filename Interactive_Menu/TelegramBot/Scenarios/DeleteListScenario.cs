using Interactive_Menu.Core.Services;
using Interactive_Menu.TelegramBot.Dto;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Telegram.Bot;
using Telegram.Bot.Types;

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
            throw new NotImplementedException();
        }

        public Task<ScenarioResult> HandleMessageAsync(ITelegramBotClient bot, ScenarioContext context, Update update, CancellationToken ct)
        {
            //Добавить обработку шагов сценария(ScenarioContext.CurrentStep) через switch case
            //case null
                //Получить ToDoUser и сохранить его в ScenarioContext.Data.
                //Отправить пользователю сообщение "Выберете список для удаления:" с Inline кнопками.callbackData = ToDoListCallbackDto.ToString().Action = "deletelist"
                //Обновить ScenarioContext.CurrentStep на "Approve"
            //case "Approve"
                //Получить ToDoList и сохранить его в ScenarioContext.Data.
                //Отправить пользователю сообщение "Подтверждаете удаление списка {toDoList.Name} и всех его задач" с Inline кнопками: WithCallbackData("✅Да", "yes"), WithCallbackData("❌Нет", "no")
                //Обновить ScenarioContext.CurrentStep на "Delete"
            //case "Delete"
                //ЕСЛИ update.CallbackQuery.Data равна
                //"yes" ТО удалить все задачи по ToDoUser и ToDoList. Удалить ToDoList.
                //"no" ТО отправить сообщение "Удаление отменено".
                //Вернуть ScenarioResult.Completed.
            //При нажатии на кнопку "❌Удалить" должен запускаться сценарий DeleteListScenario


            throw new NotImplementedException();
        }
    }
}
