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
        /// Последняя введеная пользователем строка
        /// </summary>
        private static string? CurrentString;
        /// <summary>
        /// Доступные пользователю команды. Массив доступных комманд. Команда "/echo" всегда последняя команда в массиве.
        /// </summary>
        private static readonly string[] AvailableCommands = ["/start", "/help", "/info", "/exit", "/echo"];
        /// <summary>
        /// Индекс команды "/echo" в массиве доступных команд
        /// </summary>
        private static readonly int EchoCommandNumber = AvailableCommands.Length - 1;


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
            Console.WriteLine("Cправка по программе:\r\nКоманда /start: Если пользователь вводит команду /start, программа просит его ввести своё имя. \r\nКоманда /help: Отображает краткую справочную информацию о том, как пользоваться программой.\r\nКоманда /info: Предоставляет информацию о версии программы и дате её создания.\r\nКоманда /echo: Становится доступной после ввода имени. При вводе этой команды с аргументом (например, /echo Hello), программа возвращает введенный текст (в данном примере \"Hello\").\r\nКоманда /exit: Завершить программу.\r\nВводите команды строчными буквами для корректной работы приложения.\r\n");
        }

        /// <summary>
        /// Реализация команды "/info"
        /// </summary>
        static void InfoCommand()
        {
            Console.WriteLine("Текущая версия программы 1.0.  Дата создания 25-05-2025\r\n");
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
        /// <param name="availableCommandsArray">Массив доступных комманд</param>
        /// <param name="echoCommandNumber">Индекс команды "/echo" в массиве доступных команд</param>
        static void EchoCommand(string inputString, string[] availableCommandsArray, int echoCommandNumber, string userName)
        {
            if (!string.IsNullOrEmpty(inputString) && 
                inputString.StartsWith(availableCommandsArray[echoCommandNumber]) && 
                !string.IsNullOrEmpty(userName))
            {
                var subString = inputString.Substring(availableCommandsArray[echoCommandNumber].Length);
                Console.WriteLine(subString.TrimStart());
            }
        }
        #endregion

        /// <summary>
        /// Метод определяет, является ли входящая строка командой, и возвращает порядковый номер введеной команды из массива доступных команд. 
        /// </summary>
        /// <param name="inputString"></param>
        /// <param name="availableCommandsArray">Массив доступных команд</param>
        /// <param name="echoCommandNumber">Индекс команды "/echo" в массиве доступных команд</param>
        /// <returns>Возвращает порядковый номер команды из массива доступных команд, -1 если не команда найдена</returns>
        static int DefineCommandNum(string inputString, string[] availableCommandsArray, int echoCommandNumber)
       {
            for (int i = 0; i < availableCommandsArray.Length; i++)
            {
                if (string.Equals(inputString, availableCommandsArray[i]) ||
                        (!string.IsNullOrEmpty(inputString) &&
                        inputString.StartsWith(availableCommandsArray[i]) &&
                        (i == echoCommandNumber)))
                    return i;
            }
            return -1;
        }


        /// <summary>
        /// Метод выполняет вызов команды по её порядковому номеру из массива доступных команд.
        /// </summary>
        /// <param name="commandNum">Порядковый номер команды из массива доступных команд</param>
        /// <param name="inputString">Входящая строка</param>
        /// <param name="availableCommandsArray">Массив доступных команд</param>
        /// <param name="echoCommandNumber">Индекс команды "/echo" в массиве доступных команд</param>
        /// <param name="userName">Текущее имя пользователя</param>
        static void ExecuteCommand(int commandNum, string inputString, string[] availableCommandsArray,
                                   int echoCommandNumber, ref string userName)
        {
                    switch (commandNum)
                    {
                        case 0:
                            PrintUserName(userName);
                            StartCommand(ref userName);
                            return;
                        case 1:
                            PrintUserName(userName);
                            HelpCommand();
                            return;
                        case 2:
                            PrintUserName(userName);
                            InfoCommand();
                            return;
                        case 3:
                            ExitCommand();
                            return;
                        case 4:
                            EchoCommand(inputString, availableCommandsArray, echoCommandNumber, userName);
                            return;
                        case -1:
                            return;
                    }
        }

        /// <summary>
        /// Метод выводит в консоль строку со всеми доступными командами. Если задано имя пользователя, то к выводу добавляется команда 
        /// "/echo" - последняя из массива доступных команд.
        /// </summary>
        /// <param name="availableCommandsArray">Массив доступных команд</param>
        /// <param name="echoCommandNumber">Индекс команды "/echo" в массиве доступных команд</param>
        /// <param name="userName">Текущее имя пользователя</param>
        static void PrintCommands(string[] availableCommandsArray, int echoCommandNumber, string userName)
        {
            Console.Write("Вам доступны команды: ");
            for (int i = 0; i < availableCommandsArray.Length-1; i++)
            {
                if (i > 0) 
                    Console.Write(", ");
                Console.Write(availableCommandsArray[i]);
            }
            if (!string.IsNullOrEmpty(userName))
                Console.WriteLine($", {availableCommandsArray[echoCommandNumber]}");
            else
                Console.WriteLine();
        }

        /// <summary>
        /// Метод выводит имя пользователя, если оно задано.
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
            Console.Write("Добрый день! ");
            do
            {
                if (!string.IsNullOrEmpty(UserName))
                    PrintUserName(UserName);
                PrintCommands(AvailableCommands, EchoCommandNumber, UserName);

                var inputString = Console.ReadLine();
                if (!string.IsNullOrEmpty(inputString))
                {
                    CurrentString = inputString;
                    var commandNum = DefineCommandNum(CurrentString, AvailableCommands, EchoCommandNumber);
                    ExecuteCommand(commandNum, CurrentString, AvailableCommands, EchoCommandNumber, ref UserName);
                }
            }
            while (true);
        }
    }
}
