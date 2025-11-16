using Interactive_Menu.Core.DataAccess;
using Interactive_Menu.Core.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Interactive_Menu.Infrastructure.DataAccess
{
    internal class FileToDoListRepository : IToDoListRepository
    {
        public FileToDoListRepository(string v)
        {
            V = v;
        }

        public string V { get; }

        public Task Add(ToDoList list, CancellationToken ct)
        {
            throw new NotImplementedException();
        }

        public Task Delete(Guid id, CancellationToken ct)
        {
            throw new NotImplementedException();
        }

        public Task<bool> ExistsByName(Guid userId, string name, CancellationToken ct)
        {
            throw new NotImplementedException();
        }

        public Task<ToDoList?> Get(Guid id, CancellationToken ct)
        {
            throw new NotImplementedException();
        }

        public Task<IReadOnlyList<ToDoList>> GetByUserId(Guid userId, CancellationToken ct)
        {
            throw new NotImplementedException();
        }
    }
}
