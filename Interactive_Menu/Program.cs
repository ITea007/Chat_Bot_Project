using System.Text;

namespace Interactive_Menu
{

    internal class Program
    {
        /// <summary>
        /// Текущее имя пользователя
        /// </summary>
        private static string UserName = "";
        /// <summary>
        /// Доступные пользователю команды. Список доступных комманд.
        /// </summary>
        private static readonly List<string> AvailableCommands = ["/echo", "/start", "/help", "/info", "/exit", "/addtask", "/showtasks", "/removetask"];
        /// <summary>
        /// Индекс команды "/echo" в списке доступных команд
        /// </summary>
        private static readonly int EchoCommandIndex = 0;
        /// <summary>
        /// Список текущих задач
        /// </summary>
        private static List<string> Tasks = new List<string>();


        #region CommandsRealization


        /// <summary>
        /// Реализация команды "/start"
        /// </summary>
        /// <param name="userName">Текущее имя пользователя</param>
        static void StartCommand(ref string userName)
        {
            string? inputString;
            do
            {
                Console.WriteLine("Введите имя пользователя");
                inputString = Console.ReadLine();
            } while (string.IsNullOrEmpty(inputString));
            userName = inputString;
        }
        
        /// <summary>
        /// Реальзация команды "/help"
        /// </summary>
        static void HelpCommand()
        {
            Console.WriteLine("Cправка по программе:\r\nКоманда /start: Если пользователь вводит команду /start, программа просит его ввести своё имя." +
                "\r\nКоманда /help: Отображает краткую справочную информацию о том, как пользоваться программой." +
                "\r\nКоманда /info: Предоставляет информацию о версии программы и дате её создания." +
                "\r\nКоманда /echo: Становится доступной после ввода имени. При вводе этой команды с аргументом (например, /echo Hello), программа возвращает " +
                "введенный текст (в данном примере \"Hello\")." +
                "\r\nКоманда /addtask: После ввода команды добавьте описание задачи. После добавления задачи выводится сообщение, что задача добавлена." +
                "\r\nКоманда /showtasks: При вводе команды /showtasks бот отображает список всех добавленных задач." +
                "\r\nКоманда /removetask: После ввода команды, отображается список задач с номерами. Введите номер задачи для её удаления." +
                "\r\nКоманда /exit: Завершить программу." +
                "\r\nВводите команды строчными буквами для корректной работы приложения.\r\n");
        }

        /// <summary>
        /// Реализация команды "/info"
        /// </summary>
        static void InfoCommand()
        {
            Console.WriteLine(  "*  Текущая версия программы 2.0.  Дата создания 16-06-2025\r\n" +
                                "   Добавлены команды /addtask, /showtasks, /removetask\r\n" +
                                "*  Версия программы 1.0.  Дата создания 25-05-2025\r\n" +
                                "   Реализованы команды /start, /help, /info, /echo, /exit\r\n");
        }

        /// <summary>
        /// Реализация команды "/exit"
        /// </summary>
        static void ExitCommand()
        {
            Environment.Exit(0);
        }

        /// <summary>
        /// Реализация команды "/echo". Выводится на консоль строка, которая введена пользователем после "/echo"
        /// Реализация метода: Проверяется наличие команды /echo и имени пользователя. Выделяется подстрока, убираются 
        /// лишние пробелы вначале, выводится на консоль. 
        /// </summary>
        /// <param name="inputString">Входящая строка</param>
        /// <param name="availableCommandsList">Список доступных комманд</param>
        /// <param name="echoCommandIndex">Индекс команды "/echo" в списке доступных команд</param>
        static void EchoCommand(string inputString, List<string> availableCommandsList, int echoCommandIndex, string userName)
        {
            if (!string.IsNullOrEmpty(inputString) &&
                !string.IsNullOrEmpty(userName) && 
                inputString.StartsWith(availableCommandsList[echoCommandIndex]))
            {
                var subString = inputString.Substring(availableCommandsList[echoCommandIndex].Length);
                Console.WriteLine(subString.TrimStart());
            }
        }
        /// <summary>
        /// Добавление в список текущих задач новой задачи с описанием.
        /// </summary>
        /// <param name="tasksList">Список текущих задач</param>
        static void AddTask(List<string> tasksList)
        {
            var taskDescription = "";
            do
            {
                Console.WriteLine("Введите описание задачи");
                taskDescription = Console.ReadLine();
            } while (string.IsNullOrEmpty(taskDescription));
            tasksList.Add(taskDescription);
            Console.WriteLine($"Добавлена задача # {tasksList.Count}: {tasksList[^1]}");
        }
        /// <summary>
        /// Вывод в консоль списка текущих задач
        /// </summary>
        /// <param name="tasksList">Список текущих задач</param>
        static void ShowTasks(List<string> tasksList)
        {
            if (tasksList.Count == 0)
                Console.WriteLine("Список задач пуст");
            else
            {
                Console.WriteLine("Список текущих задач:");
                for (int i = 0; i < tasksList.Count; i++)
                    Console.WriteLine($"Задача {i + 1}: {tasksList[i]}");
            }
        }

        /// <summary>
        /// Метод отображает в консоль спикок текущих задач, запрашивает у пользователя ввод номера задачи для удаления, удаляет её, и выводит в консоль 
        /// удаленную команду.
        /// </summary>
        /// <param name="tasksList">Список текущих задач</param>
        static void RemoveTask(List<string> tasksList)
        {
            if (tasksList.Count == 0)
                Console.WriteLine("Список задач пуст");
            else
            {
                Console.WriteLine("Список текущих задач:");
                for (int i = 0; i < tasksList.Count; i++)
                    Console.WriteLine($"Задача {i+1}: {tasksList[i]}");
                int taskNum = -1;
                do
                    Console.WriteLine("Введите номер задачи из списка текущих задач для удаления и нажмите Enter");
                while ((!int.TryParse(Console.ReadLine(), out taskNum)) ||
                    taskNum > tasksList.Count ||
                    taskNum < 1);
                    
                var DeletedTask = tasksList[taskNum - 1];
                tasksList.RemoveAt(taskNum - 1);
                Console.WriteLine($"Задача {taskNum} \"{DeletedTask}\"  удалена");
            }
        }

        #endregion

        /// <summary>
        /// Метод определяет, является ли входящая строка командой, и возвращает порядковый номер введеной команды из списка доступных команд. 
        /// </summary>
        /// <param name="inputString"></param>
        /// <param name="availableCommandsList">Список доступных команд</param>
        /// <param name="echoCommandIndex">Индекс команды "/echo" в списке доступных команд</param>
        /// <returns>Возвращает индекс команды из списка доступных команд, -1 если не команда найдена</returns>
        static int DefineCommandNum(string inputString, List<string> availableCommandsList, int echoCommandIndex)
        {
            for (int i = 0; i < availableCommandsList.Count; i++)
            {
                if (string.Equals(inputString, availableCommandsList[i]) ||
                        (!string.IsNullOrEmpty(inputString) &&
                        inputString.StartsWith(availableCommandsList[i]) &&
                        (i == echoCommandIndex)))
                    return i;
            }
            return -1;
        }


        /// <summary>
        /// Метод выполняет вызов команды по её порядковому номеру из списка доступных команд.
        /// </summary>
        /// <param name="commandNum">Порядковый номер команды из списка доступных команд</param>
        /// <param name="inputString">Входящая строка</param>
        /// <param name="availableCommandsList">Список доступных команд</param>
        /// <param name="echoCommandIndex">Индекс команды "/echo" в списке доступных команд</param>
        /// <param name="userName">Текущее имя пользователя</param>
        /// <param name="tasksList">Список текущих задач</param>
        static void ExecuteCommand(int commandNum, string inputString, List<string> availableCommandsList,
                                   int echoCommandIndex, ref string userName, List<string> tasksList)
        {
            if (commandNum != echoCommandIndex)
                PrintUserName(userName);
            switch (commandNum)
            {
                
                case 0:
                    EchoCommand(inputString, availableCommandsList, echoCommandIndex, userName);
                    return;
                case 1:
                    StartCommand(ref userName);
                    return;
                case 2:
                    HelpCommand();
                    return;
                case 3:
                    InfoCommand();
                    return;
                case 4:
                    ExitCommand();
                    return;
                case 5:
                    AddTask(tasksList);
                    return;
                case 6:
                    ShowTasks(tasksList);
                    return;
                case 7:
                    RemoveTask(tasksList);
                    return;
                case -1:
                    return;
            }
        }

        /// <summary>
        /// Метод выводит в консоль строку со всеми доступными командами. Если задано имя пользователя, то к выводу добавляется команда 
        /// "/echo".
        /// </summary>
        /// <param name="availableCommandsList">Список доступных команд</param>
        /// <param name="echoCommandIndex">Индекс команды "/echo" в списке доступных команд</param>
        /// <param name="userName">Текущее имя пользователя</param>
        static void PrintCommands(List<string> availableCommandsList, int echoCommandIndex, string userName)
        {
            Console.Write("Вам доступны команды: ");
            for (int i = 1; i < availableCommandsList.Count; i++)
            {
                if (i > 1) 
                    Console.Write(", ");
                Console.Write(availableCommandsList[i]);
            }
            if (!string.IsNullOrEmpty(userName))
                Console.WriteLine($", {availableCommandsList[echoCommandIndex]}");
            else
                Console.WriteLine();
        }

        /// <summary>
        /// Метод выводит на консоль имя пользователя, если оно задано.
        /// </summary>
        /// <param name="userName">Текущее имя пользователя</param>
        static void PrintUserName(string userName)
        {
            if (!string.IsNullOrEmpty(userName))
            {
                Console.Write($"{userName}, ");
            }
        }

        /// <summary>
        /// Последовательность действий: Подключаем кодировку для работы с русским языком. Выводим приветствие и запускаем основной цикл программы, в котором  
        /// оповещаем пользователя о доступных командах и обрабатываем пользовательский ввод (определяем команду и выполняем её).  
        /// </summary>
        public static void Main()
        {
            Console.InputEncoding = Encoding.GetEncoding("UTF-16");
            Console.Write("Добро пожаловать! ");
            do
            {
                PrintUserName(UserName);
                PrintCommands(AvailableCommands, EchoCommandIndex, UserName);

                var inputString = Console.ReadLine();
                if (!string.IsNullOrEmpty(inputString))
                {
                    var commandNum = DefineCommandNum(inputString, AvailableCommands, EchoCommandIndex);
                    if (commandNum >= 0)
                        ExecuteCommand(commandNum, inputString, AvailableCommands, EchoCommandIndex, ref UserName, Tasks);
                }
            }
            while (true);
        }
    }
}
