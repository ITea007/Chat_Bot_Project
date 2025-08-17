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
        //Заполнять telegramUserId и telegramUserName нужно из значений Update.Message.From 
        //В конструкторе??

        private long _telegramUserId;
        private string _telegramUserName;


        public UserService(User user ) 
        {
            //как?
        }


        public ToDoUser? GetUser(long telegramUserId)
        {
            throw new NotImplementedException();
        }

        public ToDoUser RegisterUser(long telegramUserId, string telegramUserName)
        {
            return new ToDoUser(telegramUserName, telegramUserId);

        }
    }
}
