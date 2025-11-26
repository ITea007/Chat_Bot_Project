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
        public Guid UserId { get; set; }
        public string TelegramUserName { get; set; } = string.Empty;
        public DateTime RegisteredAt { get; set; }

    }
}
