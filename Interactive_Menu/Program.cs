using Otus.ToDoList.ConsoleBot;
using Otus.ToDoList.ConsoleBot.Types;
using System;
using System.ComponentModel;
using System.Text;

namespace Interactive_Menu
{
    internal class Program
    {

        /// <summary>
        /// Точка запуска программы. Создаём клиента, сервисы, начинаем получать и обрабатывать сообщения.
        /// </summary>
        public static void Main()
        {
            try
            {
                Console.InputEncoding = Encoding.GetEncoding("UTF-16");

                    var botClient = new ConsoleBotClient();
                    var userService = new UserService();
                    var toDoService = new ToDoService();
                    var handler = new UpdateHandler(botClient, userService, toDoService);
                    
                    botClient.StartReceiving(handler);
            }
            catch (Exception Ex)
            {
                Console.WriteLine("Произошла непредвиденная ошибка:");
                Console.WriteLine($"Тип исключения: {Ex.GetType()}");
                Console.WriteLine($"Исключение: {Ex.Message}");
                Console.WriteLine($"Трассировка стека: {Ex.StackTrace}");
                Console.WriteLine($"Внутреннее исключение: {Ex.InnerException}");
            }
        }
    }
}
