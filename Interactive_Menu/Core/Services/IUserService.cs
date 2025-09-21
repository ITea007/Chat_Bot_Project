using Interactive_Menu.Core.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Interactive_Menu.Core.Services
{
    internal interface IUserService
    {
            Task<ToDoUser> RegisterUser(long telegramUserId, string telegramUserName, CancellationToken ct);
            Task<ToDoUser?> GetUser(long telegramUserId, CancellationToken ct);
    }
}
