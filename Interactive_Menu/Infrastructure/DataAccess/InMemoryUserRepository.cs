using Interactive_Menu.Core.DataAccess;
using Interactive_Menu.Core.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Interactive_Menu.Infrastructure.DataAccess
{
    internal class InMemoryUserRepository : IUserRepository
    {
        private static List<ToDoUser> _users = new List<ToDoUser>();

        public void Add(ToDoUser user)
        {
            _users.Add(user);
        }

        public ToDoUser? GetUser(Guid userId)
        {
            return _users.Where(i => i.UserId == userId).FirstOrDefault();
        }

        public ToDoUser? GetUserByTelegramUserId(long telegramUserId)
        {
            return _users.Where(i => i.TelegramUserId == telegramUserId).FirstOrDefault();
        }
    }
}
