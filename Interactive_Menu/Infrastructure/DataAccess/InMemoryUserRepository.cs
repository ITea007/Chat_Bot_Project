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
        //В качестве хранилища использовать List


        public void Add(ToDoUser user)
        {
            throw new NotImplementedException();
        }

        public ToDoUser? GetUser(Guid userId)
        {
            throw new NotImplementedException();
        }

        public ToDoUser? GetUserByTelegramUserId(long telegramUserId)
        {
            throw new NotImplementedException();
        }
    }
}
