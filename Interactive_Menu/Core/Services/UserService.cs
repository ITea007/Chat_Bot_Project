using Interactive_Menu.Core.DataAccess;
using Interactive_Menu.Core.Entities;
using Otus.ToDoList.ConsoleBot.Types;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Interactive_Menu.Core.Services
{
    internal class UserService : IUserService
    {

        private IUserRepository _userRepository;

        public UserService(IUserRepository userRepository)
        {
            _userRepository = userRepository;
        }

        public ToDoUser? GetUser(long telegramUserId)
        {
            return _userRepository.GetUserByTelegramUserId(telegramUserId);       
        }

        public Task<ToDoUser?> GetUser(long telegramUserId, CancellationToken ct)
        {
            throw new NotImplementedException();
        }

        public ToDoUser RegisterUser(long telegramUserId, string telegramUserName)
        {
            var user = new ToDoUser(telegramUserId, telegramUserName);
            _userRepository.Add(user);
            return user;
        }

        public Task<ToDoUser> RegisterUser(long telegramUserId, string telegramUserName, CancellationToken ct)
        {
            throw new NotImplementedException();
        }
    }
}
