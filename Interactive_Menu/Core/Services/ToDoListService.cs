using Interactive_Menu.Core.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Interactive_Menu.Core.Services
{
    internal class ToDoListService : IToDoListService
    {
        //Размер имени списка не может быть больше 10 символом
        //Название списка должно быть уникально в рамках одного ToDoUser

        public Task<ToDoList> Add(ToDoUser user, string name, CancellationToken ct)
        {
            throw new NotImplementedException();
        }

        public Task Delete(Guid id, CancellationToken ct)
        {
            throw new NotImplementedException();
        }

        public Task<ToDoList?> Get(Guid id, CancellationToken ct)
        {
            throw new NotImplementedException();
        }

        public Task<IReadOnlyList<ToDoList>> GetUserLists(Guid userId, CancellationToken ct)
        {
            throw new NotImplementedException();
        }
    }
}
