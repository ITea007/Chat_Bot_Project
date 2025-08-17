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
        /// Текущее имя пользователя
        /// </summary>
        internal static ToDoUser _user;

        internal static List<ToDoUser> _users;

        /// <summary>
        /// Доступные пользователю команды
        /// </summary>
        internal static readonly Dictionary<int, string> _availableCommandsDict;
        /// <summary>
        /// Индекс команды "/echo" в списке доступных команд
        /// </summary>
        //private static readonly int _echoCommandIndex;
        /// <summary>
        /// /// Индекс команды "/completetask" в списке доступных команд
        /// </summary>
        internal static readonly int _completetaskCommandIndex;
        /// <summary>
        /// Список задач
        /// </summary>
        internal static List<ToDoItem> _tasks;
        /// <summary>
        /// Заданное максимальное количество задач
        /// </summary>
        internal static int _taskCountLimit;
        /// <summary>
        /// Минимально допустимое количество задач
        /// </summary>
        internal static int _minTaskCountLimit;
        /// <summary>
        /// Максимально допустимое количество задач
        /// </summary>
        internal static int _maxTaskCountLimit;
        /// <summary>
        /// Заданная максимальная длина строки описания задачи
        /// </summary>
        internal static int _taskLengthLimit;
        /// <summary>
        /// Минимально допустимая длина строки описания задачи
        /// </summary>
        internal static int _minTaskLengthLimit;
        /// <summary>
        /// Максимально допустимая длина строки описания задачи
        /// </summary>
        internal static int _maxTaskLengthLimit;


        /// <summary>
        /// Инициализация статических полей
        /// </summary>
        static Program()
        {
            //_user = new ToDoUser("");
            _availableCommandsDict = new Dictionary<int, string>()
            {
                { 0, "/echo" }, { 1, "/start" }, { 2, "/help" },
                { 3, "/info" }, { 4, "/exit" }, { 5, "/addtask"},
                { 6, "/showtasks"}, { 7, "/removetask"}, { 8, "/showalltasks"},
                { 9, "/completetask"}
            };
            //_echoCommandIndex = _availableCommandsDict.Where(i => i.Value.Equals("/echo")).FirstOrDefault().Key;
            _completetaskCommandIndex = _availableCommandsDict.Where(i => i.Value.Equals("/completetask")).FirstOrDefault().Key;
            _tasks = new List<ToDoItem>();
            _minTaskCountLimit = 1;
            _maxTaskCountLimit = 100;
            _minTaskLengthLimit = 1;
            _maxTaskLengthLimit = 100;
        }

        #region CommandsRealization

        /// <summary>
        /// Реализация команды "/start"
        /// </summary>
        /// <param name="user">Пользователь</param>
        static string StartCommand(ref IUserService userService)
        {
            //Console.WriteLine("Введите имя пользователя");
            var inputString = Console.ReadLine();
            ValidateString(inputString);
            //user = new ToDoUser(inputString);
            //user.TelegramUserName = inputString;

           // var user = userService.GetUser();


            return "";

        }

        /// <summary>
        /// Реальзация команды "/help"
        /// </summary>
        /// <param name="taskCountLimit">Заданное максимальное количество задач</param>
        /// <param name="taskLengthLimit">Заданная максимальная длина строки описания задачи</param>
        static string HelpCommand(int taskCountLimit, int taskLengthLimit)
        {
            StringBuilder outputBuilder = new StringBuilder();
            outputBuilder.AppendLine("Cправка по программе:\r\nКоманда /start: Если пользователь вводит команду /start, программа просит его ввести своё имя." +
                "\r\nКоманда /help: Отображает краткую справочную информацию о том, как пользоваться программой." +
                "\r\nКоманда /info: Предоставляет информацию о версии программы и дате её создания." +
                "\r\nКоманда /addtask: После ввода команды добавьте описание задачи. После добавления задачи выводится сообщение, что задача добавлена." +
                $"\r\n\tМаксимальная длина задачи:{taskLengthLimit}" +
                $"\r\n\tМаксимальное количество задач:{taskCountLimit}" +
                "\r\nКоманда /showtasks: После ввода команды отображается список всех активных задач." +
                "\r\nКоманда /showalltasks: После ввода команды отображается список всех задач." +
                "\r\nКоманда /removetask: После ввода команды отображается список задач с номерами. Введите номер задачи для её удаления." +
                "\r\nКоманда /complete: Используется для завершения задачи. При вводе этой команды с номером задачи " +
                "\r\n(например, /complete 0167b785-b830-4d02-b82a-881b0b678034), программа завершает задачу, её статус станосится Completed." +
                "\r\nКоманда /exit: Завершить программу." +
                "\r\nВводите команды строчными буквами для корректной работы приложения.\r\n");

            return outputBuilder.ToString();
        }

        /// <summary>
        /// Реализация команды "/info"
        /// </summary>
        static string InfoCommand()
        {
            StringBuilder outputBuilder = new StringBuilder();
            outputBuilder.AppendLine("\r\n" +
                                "*  Текущая версия программы 5.0.  Дата создания 02-08-2025\r\n" +
                                "   Удалена команда /echo, (ДЗ 6) \r\n" +
                                "*  Текущая версия программы 4.0.  Дата создания 28-07-2025\r\n" +
                                "   Добавлены команды /completetask, /showalltasks, изменена логика команды /showtasks(ДЗ 5) \r\n" +
                                "*  Текущая версия программы 3.0.  Дата создания 19-06-2025\r\n" +
                                "   Добавлена обработка ошибок через исключения(ДЗ 4) \r\n" +
                                "*  Текущая версия программы 2.0.  Дата создания 16-06-2025\r\n" +
                                "   Добавлены команды /addtask, /showtasks, /removetask\r\n" +
                                "*  Версия программы 1.0.  Дата создания 25-05-2025\r\n" +
                                "   Реализованы команды /start, /help, /info, /echo, /exit\r\n");

            return outputBuilder.ToString();
        }

        /// <summary>
        /// Реализация команды "/exit"
        /// </summary>
        static void ExitCommand()
        {
            Environment.Exit(0);
        }

        /// <summary>
        /// Реализация команды "/addtask". Добавление в Список задач новой задачи.
        /// </summary>
        /// <param name="tasksList">Список задач</param>
        /// <param name="taskCountLimit">Заданное максимальное количество задач</param>
        /// <param name="taskLengthLimit">Заданная максимальная длина строки описания задачи</param>
        /// <param name="user">Текущий пользователь</param>
        /// <exception cref="TaskCountLimitException"></exception>
        /// <exception cref="TaskLengthLimitException"></exception>
        /// <exception cref="DuplicateTaskException"></exception>
        static string AddTask(List<ToDoItem> tasksList, int taskCountLimit, int taskLengthLimit, ToDoUser user)
        {
            StringBuilder outputBuilder = new StringBuilder();

            if (tasksList.Count >= taskCountLimit)
                throw new TaskCountLimitException(taskCountLimit);
            else
            {
                //Console.WriteLine("Введите описание задачи");
                var taskDescription = Console.ReadLine();
                ValidateString(taskDescription);
                if (taskDescription.Length > taskLengthLimit)
                    throw new TaskLengthLimitException(taskDescription.Length, taskLengthLimit);
                if (TaskInTasksListCheck(taskDescription, tasksList))
                    throw new DuplicateTaskException(taskDescription);
                else
                    tasksList.Add(new ToDoItem(user, taskDescription));

                outputBuilder.AppendLine($"Добавлена задача {tasksList[^1].Name} - {tasksList[^1].CreatedAt} - {tasksList[^1].Id}");
            }

            return outputBuilder.ToString();
        }

        /// <summary>
        /// Реализация команды "/showtasks". Вывод в консоль списка текущих активных задач. 
        /// </summary>
        /// <param name="tasksList">Список задач</param>
        static string ShowTasks(List<ToDoItem> tasksList)
        { 
            StringBuilder outputBuilder = new StringBuilder();

            if ((tasksList.Count == 0) || (!tasksList.Any(i => i.State == ToDoItemState.Active)))
                outputBuilder.AppendLine("Список текущих задач пуст");
            else
            {
                outputBuilder.AppendLine("Список текущих задач:");
                for (int i = 0; i < tasksList.Count ; i++)
                    if (tasksList[i].State == ToDoItemState.Active)
                        outputBuilder.AppendLine($"Имя задачи {tasksList[i].Name} - {tasksList[i].CreatedAt} - {tasksList[i].Id}");
            }
            return outputBuilder.ToString();
        }

        /// <summary>
        /// Реализация команды "/showalltasks". Вывод в консоль списка всех задач.
        /// </summary>
        /// <param name="tasksList">Список задач</param>
        static string ShowAllTasks(List<ToDoItem> tasksList)
        {
            StringBuilder outputBuilder = new StringBuilder();

            if (tasksList.Count == 0)
                outputBuilder.AppendLine("Список задач пуст");
            else
            {
                outputBuilder.AppendLine("Список всех задач:");
                for (int i = 0; i < tasksList.Count; i++)
                    outputBuilder.AppendLine($"({tasksList[i].State}) Имя задачи {tasksList[i].Name} - {tasksList[i].CreatedAt} - {tasksList[i].Id}");
            }
            return outputBuilder.ToString();
        }

        /// <summary>
        /// Реализация команды "/completetask". Завершение задачи. Номер задачи определяется после удаления названия команды из начала строки.
        /// </summary>
        /// <param name="tasksList">Список задач</param>
        /// <param name="input">Входящая строка</param>
        static string CompleteTask(List<ToDoItem> tasksList, string input)
        {
            StringBuilder outputBuilder = new StringBuilder();

            ShowTasks(tasksList);
            if (tasksList.Count > 0)
            {
                if (input.StartsWith("/completetask"))
                {
                    string subString = input.Substring("/completetask".Length).TrimStart();
                    var taskChangedState = tasksList.Where(i => String.Equals(i.Id.ToString(), subString)).FirstOrDefault();
                    if (taskChangedState != null)
                    {
                        taskChangedState.State = ToDoItemState.Completed;
                        taskChangedState.StateChangedAt = DateTime.UtcNow;
                        outputBuilder.AppendLine($"Задача \"{taskChangedState.Name}\" - {taskChangedState.CreatedAt} - {taskChangedState.Id} завершена");
                    }
                    else
                    {
                        outputBuilder.AppendLine($"Не найдена задача с номером {subString} в списке текущих задач");
                    }
                }
            }

            return outputBuilder.ToString();
        }


        /// <summary>
        /// Реализация команды "/removetask". Метод отображает в консоль спикок задач, запрашивает у пользователя ввод номера задачи 
        /// для удаления, удаляет её из списка задач, и выводит в консоль удаленную задачу.
        /// </summary>
        /// <param name="tasksList">Список задач</param>
        static string RemoveTask(List<ToDoItem> tasksList, string inputString)
        {
            StringBuilder outputBuilder = new StringBuilder();

            ShowAllTasks(tasksList);
            if (tasksList.Count > 0)
            {
                outputBuilder.AppendLine("Введите номер задачи из списка текущих задач для удаления и нажмите Enter");
                //var inputString = Console.ReadLine();
                ValidateString(inputString);

                bool isDeleted = false;
                for (int i = 0; i < tasksList.Count; i++)
                {
                    if (String.Equals(tasksList[i].Id.ToString(), inputString))
                    {
                        var DeletedTask = tasksList[i];
                        tasksList.RemoveAt(i);
                        isDeleted = true;
                        outputBuilder.AppendLine($"Задача \"{DeletedTask.Name}\" - {DeletedTask.CreatedAt} - {DeletedTask.Id} удалена");
                    }
                }
                if (!isDeleted)
                    outputBuilder.AppendLine($"Не найдена задача с номером {inputString} в списке текущих задач");
            }

            return outputBuilder.ToString();
        }
        #endregion

        /// <summary>
        /// Проверяет, что строка не равна null, не равна пустой строке и имеет какие-то символы кроме проблема. 
        /// В противном случае выбрасывает ArgumentException с сообщением. 
        /// </summary>
        /// <param name="str">Строка для проверки</param>
        /// <exception cref="ArgumentException"></exception>
        static void ValidateString(string? str)
        {
            if (string.IsNullOrWhiteSpace(str))
                throw new ArgumentException("Ошибка: Пустая строка. Пожалуйста, вводите непустое значение");
        }

        /// <summary>
        /// Проверка, имеется ли задача в списке задач
        /// </summary>
        /// <param name="inputTask">Задача для проверки</param>
        /// <param name="tasksList">Список задач</param>
        /// <returns></returns>
        static bool TaskInTasksListCheck(string inputTask, List<ToDoItem> tasksList)
        {
            bool result = false;
            foreach (ToDoItem task in tasksList)
            {
                if (task.Name.Equals(inputTask))
                    result = true;
            }
            return result;
        }

        /// <summary>
        /// Метод приводит полученную строку к int и проверяет, что оно находится в диапазоне min и max. 
        /// В противном случае выбрасывает ArgumentException с сообщением.
        /// </summary>
        /// <param name="str">Строка для обработки</param>
        /// <param name="min">Минимальное значение диапазона</param>
        /// <param name="max">Максимальное значение диапазона</param>
        /// <returns></returns>
        /// <exception cref="ArgumentException"></exception>
        static int ParseAndValidateInt(string? str, int min, int max)
        {
            int result = 0;
            bool isParsed = int.TryParse(str, out result);
            if (isParsed && result >= min && result <= max)
                return result;
            else 
                throw new ArgumentException($"Значение должно быть в диапазоне от {min} до {max}");
        }

        /// <summary>
        /// Установка ограничения на максимальное количество задач в диапазоне.
        /// </summary>
        /// <param name="taskCountLimit">Максимально допустимое количество задач</param>
        /// <param name="min">Минимальное значение диапазона</param>
        /// <param name="max">Максимальное значение диапазона</param>
        static void SetTaskCountLimit(out int taskCountLimit, int min, int max)
        {
            Console.WriteLine($"Введите максимально допустимое количество задач (от {min} до {max})");
            var inputString = Console.ReadLine();
            ValidateString(inputString);
            taskCountLimit = ParseAndValidateInt(inputString, min, max);
        }

        /// <summary>
        /// Установка ограничения на допустимую длину задачи.
        /// </summary>
        /// <param name="taskLengthLimit">Максимально допустимая длина задачи</param>
        /// <param name="min">Минимальное значение диапазона</param>
        /// <param name="max">Максимальное значение диапазона</param>
        static void SetTaskLengthLimit(out int taskLengthLimit, int min, int max)
        {
            Console.WriteLine($"Введите максимально допустимую длину задачи (от {min} до {max})");
            var inputString = Console.ReadLine();
            ValidateString(inputString);
            taskLengthLimit = ParseAndValidateInt(inputString, min, max);
        }

        /// <summary>
        /// Метод определяет, является ли входящая строка командой, и возвращает порядковый номер введеной команды из словаря доступных команд.
        /// </summary>
        /// <param name="inputString">Входящая строка</param>
        /// <param name="availableCommandsDict">Словарь доступных команд</param>
        /// <param name="completetaskCommandIndex">Ключ команды "/completetask"</param>
        /// <returns>Возвращает ключ команды из словаря доступных команд, или -1, если её в словаре нет</returns>
        internal static int DefineCommandNum(string inputString, Dictionary<int, string> availableCommandsDict, int completetaskCommandIndex)
        {
            if (inputString.StartsWith(availableCommandsDict[completetaskCommandIndex]))
            {
                return availableCommandsDict.
                    Where(i =>
                        inputString.StartsWith(i.Value)).
                    FirstOrDefault().
                    Key;
            }
            else
            {
                var defaultKeyValuePair = new KeyValuePair<int, string>(-1, "");
                return availableCommandsDict.Where(i => i.Value.Equals(inputString)).FirstOrDefault(defaultKeyValuePair).Key;
            }
        }


        /// <summary>
        /// Метод выполняет вызов команды по её ключу из словаря доступных команд.
        /// </summary>
        /// <param name="commandNum">Порядковый номер команды из словаря доступных команд</param>
        /// <param name="inputString">Входящая строка</param>
        /// <param name="availableCommandsDict">Словарь доступных команд</param>
        /// <param name="tasksList">Список задач</param>
        /// <param name="taskCountLimit">Максимально допустимое количество задач</param>
        /// <param name="taskLengthLimit">Максимально допустимая длина задачи</param>
        /// <param name="user">Текущий пользователь</param>
        internal static string ExecuteCommand(int commandNum, string inputString, Dictionary<int, string> availableCommandsDict,
                                   List<ToDoItem> tasksList, int taskCountLimit, int taskLengthLimit, ref ToDoUser user, ref IUserService userService)
        {
            StringBuilder outputBuilder = new StringBuilder();

            outputBuilder.Append( PrintUserName(user));

            switch (commandNum)
            {
               /* case 0:
                    EchoCommand(inputString, availableCommandsDict, echoCommandIndex, user);
                    return;
               */
                case 1:
                    outputBuilder.AppendLine( StartCommand(ref userService) );
                    return outputBuilder.ToString();
                case 2:
                    outputBuilder.AppendLine(HelpCommand(taskCountLimit, taskLengthLimit));
                    return outputBuilder.ToString();
                case 3:
                    outputBuilder.AppendLine( InfoCommand());
                    return outputBuilder.ToString();
                case 4:
                    ExitCommand();
                    return outputBuilder.ToString();
                case 5:
                    outputBuilder.AppendLine( AddTask(tasksList, taskCountLimit, taskLengthLimit, user));
                    return outputBuilder.ToString();
                case 6:
                    outputBuilder.AppendLine( ShowTasks(tasksList));
                    return outputBuilder.ToString();
                case 7:
                    outputBuilder.AppendLine( RemoveTask(tasksList, inputString));
                    return outputBuilder.ToString();
                case 8:
                    outputBuilder.AppendLine( ShowAllTasks(tasksList));
                    return outputBuilder.ToString();
                case 9:
                    outputBuilder.AppendLine( CompleteTask(tasksList, inputString));
                    return outputBuilder.ToString();
            }
            return outputBuilder.ToString();
        }

        /// <summary>
        /// Метод выводит в консоль строку со всеми доступными командами. Если задано имя пользователя, то к выводу добавляется команда "/echo".
        /// </summary>
        /// <param name="availableCommandsDict">Словарь доступных команд</param>
        /// <param name="user">Текущий пользователь</param>
        static void PrintCommands(Dictionary<int, string> availableCommandsDict, ToDoUser user)
        {
            Console.Write("Вам доступны команды: ");

            foreach (var item in availableCommandsDict)
            {
                if (item.Key > 1)
                    Console.Write(", ");
                Console.Write(item.Value);
            }
            Console.WriteLine();
        }

        /// <summary>
        /// Метод выводит на консоль имя пользователя, если оно задано.
        /// </summary>
        /// <param name="user">Пользователь</param>
        static string PrintUserName(ToDoUser user)
        {
            if (!string.IsNullOrEmpty(user.TelegramUserName))
                return ($"{user.TelegramUserName}, ");
            else
                return "";
        }

        /// <summary>
        /// Последовательность действий: Подключаем кодировку для работы с русским языком. Выводим приветствие и запускаем основной цикл программы, в котором  
        /// оповещаем пользователя о доступных командах и обрабатываем пользовательский ввод (определяем команду и выполняем её).  
        /// </summary>
        public static void Main()
        {
            Console.InputEncoding = Encoding.GetEncoding("UTF-16");
            //Console.Write("Добро пожаловать! ");

            //Если вы используете Main, тогда все экземляры(UserService, ToDoService, UpdateHandler и тд) нужно создавать
            //в нем и вызывать там StartReceiving (как в примере)



            //SetTaskCountLimit(out _taskCountLimit, _minTaskCountLimit, _maxTaskCountLimit);
            //SetTaskLengthLimit(out _taskLengthLimit, _minTaskLengthLimit, _maxTaskLengthLimit);
            do
            {
                try
                {                        
                   // PrintUserName(_user);
                   // PrintCommands(_availableCommandsDict, _user);
                   // var inputString = Console.ReadLine();
                   // ValidateString(inputString);
                    //int commandNum = DefineCommandNum(inputString, _availableCommandsDict, _completetaskCommandIndex);
                    //ExecuteCommand(commandNum, inputString, _availableCommandsDict, _tasks, _taskCountLimit, _taskLengthLimit, ref _user);

                    var handler = new UpdateHandler();
                    var botClient = new ConsoleBotClient();
                    botClient.StartReceiving(handler);


                }
                catch (ArgumentException Ex)
                {
                    Console.WriteLine(Ex.Message);
                }
                catch (TaskCountLimitException Ex)
                {
                    Console.WriteLine(Ex.Message);
                }
                catch (TaskLengthLimitException Ex)
                {
                    Console.WriteLine(Ex.Message);
                }
                catch (DuplicateTaskException Ex)
                {
                    Console.WriteLine(Ex.Message);
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
            while (true);
        }
    }
}
