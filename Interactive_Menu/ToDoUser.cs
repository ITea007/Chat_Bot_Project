using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Interactive_Menu
{
    //  Добавить класс ToDoUser
    //  Свойства
    //      Guid UserId //Заполняется в конструкторе. Guid.NewGuid()
    //      string TelegramUserName //Имя пользователя, которое он указал (готовим шаблон для телеграм бота)
    //      DateTime RegisteredAt //Заполняется в конструкторе. DateTime.UtcNow
    //  У класса должен быть один конструктор с аргументом string telegramUserName
    //  Добавить использование класса ToDoUser для сохранения информации о пользователе вместо хранения только имени.

    internal class ToDoUser
    {
        public Guid UserId { get; }
        public string TelegramUserName { get; set; }
        public DateTime RegisteredAt { get; }

        public ToDoUser(string telegramUserName)
        {
            UserId = Guid.NewGuid();
            TelegramUserName = telegramUserName;
            RegisteredAt = DateTime.UtcNow;
        }
    }
}
