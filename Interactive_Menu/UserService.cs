using Otus.ToDoList.ConsoleBot.Types;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Interactive_Menu
{
    internal class UserService : IUserService
    {
        private static List<ToDoUser> _users = new List<ToDoUser> ();

        public ToDoUser? GetUser(long telegramUserId)
        {
            return _users.Where(i => i.TelegramUserId == telegramUserId).FirstOrDefault();
        }

        public ToDoUser RegisterUser(long telegramUserId, string telegramUserName)
        {
            var user = new ToDoUser(telegramUserId, telegramUserName);
            _users.Add(user);
            return user;
        }
    }
}
