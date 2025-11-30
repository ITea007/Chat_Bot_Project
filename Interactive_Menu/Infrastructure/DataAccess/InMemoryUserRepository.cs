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
            return Task.CompletedTask;
        }

        public Task<ToDoUser?> GetUser(Guid userId, CancellationToken ct)
        {
            var user = _users.Where(i => i.UserId == userId).FirstOrDefault();
            return Task.FromResult(user);
        }

        public Task<ToDoUser?> GetUserByTelegramUserId(long telegramUserId, CancellationToken ct)
        {
            var user = _users.Where(i => i.TelegramUserId == telegramUserId).FirstOrDefault();
            return Task.FromResult(user);
        }

        public Task<IReadOnlyList<ToDoUser>> GetUsers(CancellationToken ct)
        {
            throw new NotImplementedException();
        }
    }
}
