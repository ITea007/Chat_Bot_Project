using Otus.ToDoList.ConsoleBot;
using Otus.ToDoList.ConsoleBot.Types;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Interactive_Menu
{
    //IUpdateHandler отвечает за обработку обновлений.
    //var updateHandler = new UpdateHandler(bot, userService, toDoService)


    internal class UpdateHandler : IUpdateHandler
    {


        public void HandleUpdateAsync(ITelegramBotClient botClient, Update update)
        {
            botClient.SendMessage(update.Message.Chat, $"Получил '{update.Message.Text}'");
        }


        /*
        private IUserService _userService;

        public UpdateHandler (ITelegramBotClient botClient, IUserService userService, IToDoService toDoService)
        {
            _userService = userService;
        }

        public void HandleUpdateAsync(ITelegramBotClient botClient, Update update)
        {
            //throw new NotImplementedException();

            //перенести в метод HandleUpdateAsync обработку всех команд.Вместо Console.WriteLine использовать SendMessage у ITelegramBotClient

            //Перенести try/catch в HandleUpdateAsync. В Main оставить catch(Exception)

            //Для вывода в конcоль сообщений использовать метод ITelegramBotClient.SendMessage

            //Заполнять telegramUserId и telegramUserName нужно из значений Update.Message.From"?
            //Нужно создавать List<ToDoUser> и через метод GetUser искать с этой коллекции этого юзера? - Да



            try
            {
                _userService.GetUser(update.Message.From.Id);

                var inputString = update.Message.Text;
                var inputChat = update.Message.Chat;
                int commandNum = Program.DefineCommandNum(inputString, Program._availableCommandsDict, Program._completetaskCommandIndex);
                var outputString = Program.ExecuteCommand(commandNum, inputString, Program._availableCommandsDict, Program._tasks, Program._taskCountLimit, Program._taskLengthLimit, ref Program._user, ref _userService);
                botClient.SendMessage(inputChat, outputString);
            }
            catch (Exception Ex)
            {
                botClient.SendMessage(update.Message.Chat, "Произошла непредвиденная ошибка:");
                botClient.SendMessage(update.Message.Chat, $"Тип исключения: {Ex.GetType()}");
                botClient.SendMessage(update.Message.Chat, $"Исключение: {Ex.Message}");
                botClient.SendMessage(update.Message.Chat, $"Трассировка стека: {Ex.StackTrace}");
                botClient.SendMessage(update.Message.Chat, $"Внутреннее исключение: {Ex.InnerException}");
            }
        }
        */


    }
}
