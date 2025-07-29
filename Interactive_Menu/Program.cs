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
        private static ToDoUser _user;
        /// <summary>
        /// Доступные пользователю команды
        /// </summary>
        private static readonly Dictionary<int, string> _availableCommandsDict;
        /// <summary>
        /// Индекс команды "/echo" в списке доступных команд
        /// </summary>
        private static readonly int _echoCommandIndex;
        /// <summary>
        /// /// Индекс команды "/completetask" в списке доступных команд
        /// </summary>
        private static readonly int _completetaskCommandIndex;
        /// <summary>
        /// Список задач
        /// </summary>
        private static List<ToDoItem> _tasks;
        /// <summary>
        /// Заданное максимальное количество задач
        /// </summary>
        private static int _taskCountLimit;
        /// <summary>
        /// Минимально допустимое количество задач
        /// </summary>
        private static int _minTaskCountLimit;
        /// <summary>
        /// Максимально допустимое количество задач
        /// </summary>
        private static int _maxTaskCountLimit;
        /// <summary>
        /// Заданная максимальная длина строки описания задачи
        /// </summary>
        private static int _taskLengthLimit;
        /// <summary>
        /// Минимально допустимая длина строки описания задачи
        /// </summary>
        private static int _minTaskLengthLimit;
        /// <summary>
        /// Максимально допустимая длина строки описания задачи
        /// </summary>
        private static int _maxTaskLengthLimit;


        /// <summary>
        /// Инициализация статических полей
        /// </summary>
        static Program()
        {
            _user = new ToDoUser("");
            _availableCommandsDict = new Dictionary<int, string>()
            {
                { 0, "/echo" }, { 1, "/start" }, { 2, "/help" },
                { 3, "/info" }, { 4, "/exit" }, { 5, "/addtask"},
                { 6, "/showtasks"}, { 7, "/removetask"}, { 8, "/showalltasks"},
                { 9, "/completetask"}
            };
            _echoCommandIndex = _availableCommandsDict.Where(i => i.Value.Equals("/echo")).FirstOrDefault().Key;
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
        static void StartCommand(ref ToDoUser user)
        {
            Console.WriteLine("Введите имя пользователя");
            var inputString = Console.ReadLine();
            ValidateString(inputString);
            user = new ToDoUser(inputString);
            //user.TelegramUserName = inputString;
        }

        /// <summary>
        /// Реальзация команды "/help"
        /// </summary>
        /// <param name="taskCountLimit">Заданное максимальное количество задач</param>
        /// <param name="taskLengthLimit">Заданная максимальная длина строки описания задачи</param>
        static void HelpCommand(int taskCountLimit, int taskLengthLimit)
        {
            Console.WriteLine("Cправка по программе:\r\nКоманда /start: Если пользователь вводит команду /start, программа просит его ввести своё имя." +
                "\r\nКоманда /help: Отображает краткую справочную информацию о том, как пользоваться программой." +
                "\r\nКоманда /info: Предоставляет информацию о версии программы и дате её создания." +
                "\r\nКоманда /echo: Становится доступной после ввода имени. При вводе этой команды с аргументом (например, /echo Hello), программа возвращает " +
                "введенный текст (в данном примере \"Hello\")." +
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
        }

        /// <summary>
        /// Реализация команды "/info"
        /// </summary>
        static void InfoCommand()
        {
            Console.WriteLine("\r\n" +
                                "*  Текущая версия программы 4.0.  Дата создания 28-07-2025\r\n" +
                                "   Добавлены команды /completetask, /showalltasks, изменена логика команды /showtasks(ДЗ 5) \r\n" +
                                "*  Текущая версия программы 3.0.  Дата создания 19-06-2025\r\n" +
                                "   Добавлена обработка ошибок через исключения(ДЗ 4) \r\n" +
                                "*  Текущая версия программы 2.0.  Дата создания 16-06-2025\r\n" +
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
        /// Реализация метода: Проверяется наличие команды /echo - имени пользователя. Выделяется подстрока, убираются 
        /// лишние пробелы вначале, выводится на консоль.
        /// </summary>
        /// <param name="inputString">Входящая строка</param>
        /// <param name="availableCommandsDict">Словарь доступных комманд</param>
        /// <param name="echoCommandIndex">Индекс команды "/echo" в списке доступных команд</param>
        /// <param name="user">Текущий пользователь</param>
        static void EchoCommand(string inputString, Dictionary<int, string> availableCommandsDict, int echoCommandIndex, ToDoUser user)
        {
            ValidateString(inputString);
            if (!string.IsNullOrEmpty(user.TelegramUserName) && 
                inputString.StartsWith(availableCommandsDict[echoCommandIndex]))
            {
                string subString = inputString.Substring(availableCommandsDict[echoCommandIndex].Length);
                Console.WriteLine(subString.TrimStart());
            }
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
        static void AddTask(List<ToDoItem> tasksList, int taskCountLimit, int taskLengthLimit, ToDoUser user)
        {
            if (tasksList.Count >= taskCountLimit)
                throw new TaskCountLimitException(taskCountLimit);
            else
            {
                Console.WriteLine("Введите описание задачи");
                var taskDescription = Console.ReadLine();
                ValidateString(taskDescription);
                if (taskDescription.Length > taskLengthLimit)
                    throw new TaskLengthLimitException(taskDescription.Length, taskLengthLimit);
                if (TaskInTasksListCheck(taskDescription, tasksList))
                    throw new DuplicateTaskException(taskDescription);
                else
                    tasksList.Add(new ToDoItem(user, taskDescription));
                Console.WriteLine($"Добавлена задача {tasksList[^1].Name} - {tasksList[^1].CreatedAt} - {tasksList[^1].Id}");
            }
        }
        /// <summary>
        /// Реализация команды "/showtasks". Вывод в консоль списка текущих активных задач. 
        /// </summary>
        /// <param name="tasksList">Список задач</param>
        static void ShowTasks(List<ToDoItem> tasksList)
        { 
            if ((tasksList.Count == 0) || (!tasksList.Any(i => i.State == ToDoItemState.Active)))
                Console.WriteLine("Список текущих задач пуст");
            else
            {
                Console.WriteLine("Список текущих задач:");
                for (int i = 0; i < tasksList.Count ; i++)
                    if (tasksList[i].State == ToDoItemState.Active)
                        Console.WriteLine($"Имя задачи {tasksList[i].Name} - {tasksList[i].CreatedAt} - {tasksList[i].Id}");
            }
        }

        /// <summary>
        /// Реализация команды "/showalltasks". Вывод в консоль списка всех задач.
        /// </summary>
        /// <param name="tasksList">Список задач</param>
        static void ShowAllTasks(List<ToDoItem> tasksList)
        {

            if (tasksList.Count == 0)
                Console.WriteLine("Список задач пуст");
            else
            {
                Console.WriteLine("Список всех задач:");
                for (int i = 0; i < tasksList.Count; i++)
                    Console.WriteLine($"({tasksList[i].State}) Имя задачи {tasksList[i].Name} - {tasksList[i].CreatedAt} - {tasksList[i].Id}");
            }
        }

        /// <summary>
        /// Реализация команды "/completetask". Завершение задачи. Номер задачи определяется после удаления названия команды из начала строки.
        /// </summary>
        /// <param name="tasksList">Список задач</param>
        /// <param name="input">Входящая строка</param>
        static void CompleteTask(List<ToDoItem> tasksList, string input)
        {
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
                        Console.WriteLine($"Задача \"{taskChangedState.Name}\" - {taskChangedState.CreatedAt} - {taskChangedState.Id} завершена");
                    }
                    else
                    {
                        Console.WriteLine($"Не найдена задача с номером {subString} в списке текущих задач");
                    }
                }
            }
        }


        /// <summary>
        /// Реализация команды "/removetask". Метод отображает в консоль спикок задач, запрашивает у пользователя ввод номера задачи 
        /// для удаления, удаляет её из списка задач, и выводит в консоль удаленную задачу.
        /// </summary>
        /// <param name="tasksList">Список задач</param>
        static void RemoveTask(List<ToDoItem> tasksList)
        {
            ShowAllTasks(tasksList);
            if (tasksList.Count > 0)
            {
                Console.WriteLine("Введите номер задачи из списка текущих задач для удаления и нажмите Enter");
                var inputString = Console.ReadLine();
                ValidateString(inputString);

                bool isDeleted = false;
                for (int i = 0; i < tasksList.Count; i++)
                {
                    if (String.Equals(tasksList[i].Id.ToString(), inputString))
                    {
                        var DeletedTask = tasksList[i];
                        tasksList.RemoveAt(i);
                        isDeleted = true;
                        Console.WriteLine($"Задача \"{DeletedTask.Name}\" - {DeletedTask.CreatedAt} - {DeletedTask.Id} удалена");
                    }
                }
                if (!isDeleted)
                    Console.WriteLine($"Не найдена задача с номером {inputString} в списке текущих задач");
            }
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
        /// <param name="echoCommandIndex">Ключ команды "/echo"</param>
        /// <param name="completetaskCommandIndex">Ключ команды "/completetask"</param>
        /// <returns>Возвращает ключ команды из словаря доступных команд, или -1, если её в словаре нет</returns>
        static int DefineCommandNum(string inputString, Dictionary<int, string> availableCommandsDict, int echoCommandIndex , int completetaskCommandIndex)
        {
            if (inputString.StartsWith(availableCommandsDict[echoCommandIndex]) 
                || inputString.StartsWith(availableCommandsDict[completetaskCommandIndex]))
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
        /// <param name="echoCommandIndex">Ключ команды "/echo" в словаре доступных команд</param>
        /// <param name="tasksList">Список задач</param>
        /// <param name="taskCountLimit">Максимально допустимое количество задач</param>
        /// <param name="taskLengthLimit">Максимально допустимая длина задачи</param>
        /// <param name="user">Текущий пользователь</param>
        static void ExecuteCommand(int commandNum, string inputString, Dictionary<int, string> availableCommandsDict,
                                   int echoCommandIndex, List<ToDoItem> tasksList, int taskCountLimit, int taskLengthLimit, ref ToDoUser user)
        {
            if (commandNum != echoCommandIndex)
                PrintUserName(user);
            switch (commandNum)
            {
                case 0:
                    EchoCommand(inputString, availableCommandsDict, echoCommandIndex, user);
                    return;
                case 1:
                    StartCommand(ref user);
                    return;
                case 2:
                    HelpCommand(taskCountLimit, taskLengthLimit);
                    return;
                case 3:
                    InfoCommand();
                    return;
                case 4:
                    ExitCommand();
                    return;
                case 5:
                    AddTask(tasksList, taskCountLimit, taskLengthLimit, user);
                    return;
                case 6:
                    ShowTasks(tasksList);
                    return;
                case 7:
                    RemoveTask(tasksList);
                    return;
                case 8:
                    ShowAllTasks(tasksList);
                    return;
                case 9:
                    CompleteTask(tasksList, inputString);
                    return;
            }
        }

        /// <summary>
        /// Метод выводит в консоль строку со всеми доступными командами. Если задано имя пользователя, то к выводу добавляется команда "/echo".
        /// </summary>
        /// <param name="availableCommandsDict">Словарь доступных команд</param>
        /// <param name="echoCommandIndex">Индекс команды "/echo" в списке доступных команд</param>
        /// <param name="user">Текущий пользователь</param>
        static void PrintCommands(Dictionary<int, string> availableCommandsDict, int echoCommandIndex, ToDoUser user)
        {
            Console.Write("Вам доступны команды: ");

            foreach (var item in availableCommandsDict)
            {
                if (item.Key != echoCommandIndex)
                {
                    if (item.Key > 1)
                        Console.Write(", ");
                    Console.Write(item.Value);
                }
            }
            if (!string.IsNullOrEmpty(user.TelegramUserName))
                Console.WriteLine($", {availableCommandsDict[echoCommandIndex]}");
            else
                Console.WriteLine();
        }

        /// <summary>
        /// Метод выводит на консоль имя пользователя, если оно задано.
        /// </summary>
        /// <param name="user">Пользователь</param>
        static void PrintUserName(ToDoUser user)
        {
            if (!string.IsNullOrEmpty(user.TelegramUserName))
            {
                Console.Write($"{user.TelegramUserName}, ");
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
                try
                {
                    if (_taskCountLimit == 0)
                        SetTaskCountLimit(out _taskCountLimit, _minTaskCountLimit, _maxTaskCountLimit);
                    if (_taskLengthLimit == 0)
                        SetTaskLengthLimit(out _taskLengthLimit, _minTaskLengthLimit, _maxTaskLengthLimit);
                    PrintUserName(_user);
                    PrintCommands(_availableCommandsDict, _echoCommandIndex, _user);
                    var inputString = Console.ReadLine();
                    ValidateString(inputString);
                    int commandNum = DefineCommandNum(inputString, _availableCommandsDict, _echoCommandIndex, _completetaskCommandIndex);
                    ExecuteCommand(commandNum, inputString, _availableCommandsDict, _echoCommandIndex, _tasks, _taskCountLimit, _taskLengthLimit, ref _user);
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
