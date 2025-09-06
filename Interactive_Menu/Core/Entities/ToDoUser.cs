using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Interactive_Menu.Core.Entities
{
    /// <summary>
    ///  Класс пользователя ToDo
    /// </summary>
     internal class ToDoUser
    {
        public long TelegramUserId { get; set; }
        public Guid UserId { get; }
        public string TelegramUserName { get; set; }
        public DateTime RegisteredAt { get; }

        public ToDoUser(long telegramUserId, string telegramUserName)
        {
            UserId = Guid.NewGuid();
            TelegramUserName = telegramUserName;
            RegisteredAt = DateTime.UtcNow;
            TelegramUserId = telegramUserId;
        }
    }
}
