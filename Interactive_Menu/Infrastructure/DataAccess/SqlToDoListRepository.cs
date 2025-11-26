using Interactive_Menu.Core.DataAccess;
using Interactive_Menu.Core.Entities;
using LinqToDB;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Interactive_Menu.Infrastructure.DataAccess
{
    internal class SqlToDoListRepository : IToDoListRepository
    {
        private readonly IDataContextFactory<ToDoDataContext> _factory;
        public SqlToDoListRepository(IDataContextFactory<ToDoDataContext> factory)
        {
            _factory = factory;
        }

        public async Task Add(ToDoList list, CancellationToken ct)
        {
            using var dbContext = _factory.CreateDataContext();
            var model = ModelMapper.MapToModel(list);
            await dbContext.InsertAsync(model);
        }

        public async Task Delete(Guid id, CancellationToken ct)
        {
            using var dbContext = _factory.CreateDataContext();
            await dbContext.ToDoLists
                .Where(l => l.Id == id)
                .DeleteAsync(ct);
        }

        public async Task<bool> ExistsByName(Guid userId, string name, CancellationToken ct)
        {
            using var dbContext = _factory.CreateDataContext();
            return await dbContext.ToDoLists
                .AnyAsync(l => l.UserId == userId && l.Name == name, ct);
        }

        public async Task<ToDoList?> Get(Guid id, CancellationToken ct)
        {
            using var dbContext = _factory.CreateDataContext();
            var model = await dbContext.ToDoLists
                .LoadWith(l => l.User)
                .FirstOrDefaultAsync(l => l.Id == id, ct);

            return model != null ? ModelMapper.MapFromModel(model) : null;
        }

        public async Task<IReadOnlyList<ToDoList>> GetByUserId(Guid userId, CancellationToken ct)
        {
            using var dbContext = _factory.CreateDataContext();
            var models = await dbContext.ToDoLists
                .LoadWith(l => l.User)
                .Where(l => l.UserId == userId)
                .ToListAsync(ct);

            return models.Select(ModelMapper.MapFromModel).ToList().AsReadOnly();
        }
    }
}
