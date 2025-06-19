using System.ComponentModel;
using System.Text;

namespace Interactive_Menu
{
    internal class Program
    {
        /// <summary>
        /// Текущее имя пользователя
        /// </summary>
        private static string _userName;
        /// <summary>
        /// Доступные пользователю команды
        /// </summary>
        private static readonly Dictionary<int, string> _availableCommandsDict;
        /// <summary>
        /// Индекс команды "/echo" в списке доступных команд
        /// </summary>
        private static readonly int _echoCommandIndex;
        /// <summary>
        /// Список текущих задач
        /// </summary>
        private static List<string> _tasks;
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
            _userName = "";
            _availableCommandsDict = new Dictionary<int, string>()
            {
                { 0, "/echo" }, { 1, "/start" }, { 2, "/help" },
                { 3, "/info" }, { 4, "/exit" }, { 5, "/addtask"},
                { 6, "/showtasks"}, { 7, "/removetask"}
            };
            _echoCommandIndex = _availableCommandsDict.Where(i => i.Value.Equals("/echo")).FirstOrDefault().Key;
            _tasks = new List<string>();
            _minTaskCountLimit = 1;
            _maxTaskCountLimit = 100;
            _minTaskLengthLimit = 1;
            _maxTaskLengthLimit = 100;
        }

        #region CommandsRealization

        /// <summary>
        /// Реализация команды "/start"
        /// </summary>
        /// <param name="userName">Текущее имя пользователя</param>
        static void StartCommand(out string userName)
        {
            Console.WriteLine("Введите имя пользователя");
            var inputString = Console.ReadLine();
            ValidateString(inputString);
            userName = inputString;
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
                "\r\nКоманда /showtasks: После ввода команды отображается список всех добавленных задач." +
                "\r\nКоманда /removetask: После ввода команды отображается список задач с номерами. Введите номер задачи для её удаления." +
                "\r\nКоманда /exit: Завершить программу." +
                "\r\nВводите команды строчными буквами для корректной работы приложения.\r\n");
        }

        /// <summary>
        /// Реализация команды "/info"
        /// </summary>
        static void InfoCommand()
        {
            Console.WriteLine("\r\n" +
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
        /// <param name="userName">Текущее имя пользователя</param>
        static void EchoCommand(string inputString, Dictionary<int, string> availableCommandsDict, int echoCommandIndex, string userName)
        {
            ValidateString(inputString);
            if (!string.IsNullOrEmpty(userName) && 
                inputString.StartsWith(availableCommandsDict[echoCommandIndex]))
            {
                string subString = inputString.Substring(availableCommandsDict[echoCommandIndex].Length);
                Console.WriteLine(subString.TrimStart());
            }
        }

        /// <summary>
        /// Реализация команды "/addtask". Добавление в список текущих задач новой задачи.
        /// </summary>
        /// <param name="tasksList">Список текущих задач</param>
        /// <param name="taskCountLimit">Заданное максимальное количество задач</param>
        /// <param name="taskLengthLimit">Заданная максимальная длина строки описания задачи</param>
        /// <exception cref="TaskCountLimitException"></exception>
        /// <exception cref="TaskLengthLimitException"></exception>
        /// <exception cref="DuplicateTaskException"></exception>
        static void AddTask(List<string> tasksList, int taskCountLimit, int taskLengthLimit)
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
                    tasksList.Add(taskDescription);
                Console.WriteLine($"Добавлена задача # {tasksList.Count}: {tasksList[^1]}");
            }
        }
        /// <summary>
        /// Реализация команды "/showtasks". Вывод в консоль списка текущих задач.
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
        /// Реализация команды "/removetask". Метод отображает в консоль спикок текущих задач, запрашивает у пользователя ввод номера задачи 
        /// для удаления, удаляет её, и выводит в консоль удаленную команду.
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
                Console.WriteLine("Введите номер задачи из списка текущих задач для удаления и нажмите Enter");
                var inputString = Console.ReadLine();
                ValidateString(inputString);
                int taskNum = ParseAndValidateInt(inputString, 1, tasksList.Count);
                var DeletedTask = tasksList[taskNum - 1];
                tasksList.RemoveAt(taskNum - 1);
                Console.WriteLine($"Задача {taskNum} \"{DeletedTask}\"  удалена");
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
            if (string.IsNullOrEmpty(str) || string.IsNullOrEmpty(str.Trim()))
                throw new ArgumentException("Ошибка: Пустая строка. Пожалуйста, вводите непустое значение");
        }

        /// <summary>
        /// Проверка, имеется ли задача в списке задач
        /// </summary>
        /// <param name="inputTask">Задача для проверки</param>
        /// <param name="tasksList">Список задач</param>
        /// <returns></returns>
        static bool TaskInTasksListCheck(string inputTask, List<string> tasksList)
        {
            bool result = false;
            foreach (string task in tasksList)
            {
                if (task.Equals(inputTask))
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
        /// <param name="inputString"></param>
        /// <param name="availableCommandsDict">Словарь доступных команд</param>
        /// <returns>Возвращает индекс команды из списка доступных команд</returns>
        static int DefineCommandNum(string inputString, Dictionary<int, string> availableCommandsDict)
        {
            return availableCommandsDict.Where(i => i.Value.Equals(inputString)).FirstOrDefault().Key;
        }

        /// <summary>
        /// Метод выполняет вызов команды по её порядковому номеру из словаря доступных команд.
        /// </summary>
        /// <param name="commandNum">Порядковый номер команды из списка доступных команд</param>
        /// <param name="inputString">Входящая строка</param>
        /// <param name="availableCommandsDict">Словарь доступных команд</param>
        /// <param name="echoCommandIndex">Индекс команды "/echo" в списке доступных команд</param>
        /// <param name="userName">Текущее имя пользователя</param>
        /// <param name="tasksList">Список текущих задач</param>
        static void ExecuteCommand(int commandNum, string inputString, Dictionary<int, string> availableCommandsDict,
                                   int echoCommandIndex, ref string userName, List<string> tasksList, int taskCountLimit, int taskLengthLimit)
        {
            if (commandNum != echoCommandIndex)
                PrintUserName(userName);
            switch (commandNum)
            {
                case 0:
                    EchoCommand(inputString, availableCommandsDict, echoCommandIndex, userName);
                    return;
                case 1:
                    StartCommand(out userName);
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
                    AddTask(tasksList, taskCountLimit, taskLengthLimit);
                    return;
                case 6:
                    ShowTasks(tasksList);
                    return;
                case 7:
                    RemoveTask(tasksList);
                    return;
            }
        }

        /// <summary>
        /// Метод выводит в консоль строку со всеми доступными командами. Если задано имя пользователя, то к выводу добавляется команда "/echo".
        /// </summary>
        /// <param name="availableCommandsDict">Словарь доступных команд</param>
        /// <param name="echoCommandIndex">Индекс команды "/echo" в списке доступных команд</param>
        /// <param name="userName">Текущее имя пользователя</param>
        static void PrintCommands(Dictionary<int, string> availableCommandsDict, int echoCommandIndex, string userName)
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
            if (!string.IsNullOrEmpty(userName))
                Console.WriteLine($", {availableCommandsDict[echoCommandIndex]}");
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
                try
                {
                    if (_taskCountLimit == 0)
                        SetTaskCountLimit(out _taskCountLimit, _minTaskCountLimit, _maxTaskCountLimit);
                    if (_taskLengthLimit == 0)
                        SetTaskLengthLimit(out _taskLengthLimit, _minTaskLengthLimit, _maxTaskLengthLimit);
                    PrintUserName(_userName);
                    PrintCommands(_availableCommandsDict, _echoCommandIndex, _userName);
                    var inputString = Console.ReadLine();
                    ValidateString(inputString);
                    int commandNum = DefineCommandNum(inputString, _availableCommandsDict);
                    ExecuteCommand(commandNum, inputString, _availableCommandsDict, _echoCommandIndex, ref _userName, _tasks, _taskCountLimit, _taskLengthLimit);
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
