using Otus.ToDoList.ConsoleBot;
using Otus.ToDoList.ConsoleBot.Types;
using System;
using System.ComponentModel;
using System.Text;
using Interactive_Menu;
using Interactive_Menu.Infrastructure.DataAccess;
using Interactive_Menu.Core.Services;

namespace Interactive_Menu.TelegramBot
{
    internal class Program
    {
        /// <summary>
        /// Точка запуска программы. Создаём клиента, сервисы, начинаем получать и обрабатывать сообщения.
        /// </summary>
        public static void Main()
        {
            var cts = new CancellationTokenSource();
            var ct = cts.Token;
            var botClient = new ConsoleBotClient();
            var userRepository = new InMemoryUserRepository();
            var userService = new UserService(userRepository);
            var toDoRepository = new InMemoryToDoRepository();
            var toDoService = new ToDoService(toDoRepository);
            var toDoReportService = new ToDoReportService(toDoService);
            var handler = new UpdateHandler(botClient, userService, toDoService, toDoReportService);
            handler.OnHandleEventStarted += (message) => { Console.WriteLine($"Началась обработка сообщения '{message}'"); };
            handler.OnHandleEventCompleted += (message) => { Console.WriteLine($"Закончилась обработка сообщения '{message}'"); };

            try
            {
                Console.InputEncoding = Encoding.GetEncoding("UTF-16");
                botClient.StartReceiving(handler, ct);
            }
            catch (Exception Ex)
            {
                Console.WriteLine("Произошла непредвиденная ошибка:");
                Console.WriteLine($"Тип исключения: {Ex.GetType()}");
                Console.WriteLine($"Исключение: {Ex.Message}");
                Console.WriteLine($"Трассировка стека: {Ex.StackTrace}");
                Console.WriteLine($"Внутреннее исключение: {Ex.InnerException}");
            }
            finally
            {
                handler.OnHandleEventStarted -= (message) => { Console.WriteLine($"Началась обработка сообщения '{message}'"); };
                handler.OnHandleEventCompleted -= (message) => { Console.WriteLine($"Закончилась обработка сообщения '{message}'"); };
            }
        }

    }


}
