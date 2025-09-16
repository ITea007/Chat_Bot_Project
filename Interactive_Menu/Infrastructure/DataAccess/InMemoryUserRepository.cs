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

        public Task Add(ToDoUser user, CancellationToken ct)
        {
            _users.Add(user);
        }

        public ToDoUser? GetUser(Guid userId)
        {
            return _users.Where(i => i.UserId == userId).FirstOrDefault();
        }

        public Task<ToDoUser?> GetUser(Guid userId, CancellationToken ct)
        {
            throw new NotImplementedException();
        }

        public ToDoUser? GetUserByTelegramUserId(long telegramUserId)
        {
            return _users.Where(i => i.TelegramUserId == telegramUserId).FirstOrDefault();
        }

        public Task<ToDoUser?> GetUserByTelegramUserId(long telegramUserId, CancellationToken ct)
        {
            throw new NotImplementedException();
        }
    }
}
