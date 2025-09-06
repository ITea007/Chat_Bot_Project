using Interactive_Menu.Core.DataAccess;
using Interactive_Menu.Core.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Interactive_Menu.Infrastructure.DataAccess
{
    internal class InMemoryToDoRepository : IToDoRepository
    {
        //В качестве хранилища использовать List

        public void Add(ToDoItem item)
        {
            throw new NotImplementedException();
        }

        public int CountActive(Guid userId)
        {
            throw new NotImplementedException();
        }

        public void Delete(Guid id)
        {
            throw new NotImplementedException();
        }

        public bool ExistsByName(Guid userId, string name)
        {
            throw new NotImplementedException();
        }

        public IReadOnlyList<ToDoItem> Find(Guid userId, Func<ToDoItem, bool> predicate)
        {
            throw new NotImplementedException();
        }

        public ToDoItem? Get(Guid id)
        {
            throw new NotImplementedException();
        }

        public IReadOnlyList<ToDoItem> GetActiveByUserId(Guid userId)
        {
            throw new NotImplementedException();
        }

        public IReadOnlyList<ToDoItem> GetAllByUserId(Guid userId)
        {
            throw new NotImplementedException();
        }

        public void Update(ToDoItem item)
        {
            throw new NotImplementedException();
        }
    }
}
