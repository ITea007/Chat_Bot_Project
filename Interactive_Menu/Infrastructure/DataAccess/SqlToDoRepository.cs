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
    internal class SqlToDoRepository : IToDoRepository
    {
        private readonly IDataContextFactory<ToDoDataContext> _factory;

        public SqlToDoRepository(IDataContextFactory<ToDoDataContext> factory)
        {
            _factory = factory;
        }

        public async Task Add(ToDoItem item, CancellationToken ct)
        {
            using var dbContext = _factory.CreateDataContext();
            var model = ModelMapper.MapToModel(item);
            await dbContext.InsertAsync(model);
        }

        public async Task<int> CountActive(Guid userId, CancellationToken ct)
        {
            using var dbContext = _factory.CreateDataContext();
            return await dbContext.ToDoItems
                .CountAsync(i => i.UserId == userId && i.State == (int)ToDoItemState.Active, ct);
        }

        public async Task Delete(Guid id, CancellationToken ct)
        {
            using var dbContext = _factory.CreateDataContext();
            await dbContext.ToDoItems
                .Where(i => i.Id == id)
                .DeleteAsync(ct);
        }

        public async Task<bool> ExistsByName(Guid userId, string name, CancellationToken ct)
        {
            using var dbContext = _factory.CreateDataContext();
            return await dbContext.ToDoItems
                .AnyAsync(i => i.UserId == userId && i.Name == name, ct);
        }

        public async Task<IReadOnlyList<ToDoItem>> Find(Guid userId, Func<ToDoItem, bool> predicate, CancellationToken ct)
        {
            using var dbContext = _factory.CreateDataContext();
            var models = await dbContext.ToDoItems
                .LoadWith(i => i.User)
                .LoadWith(i => i.List)
                .LoadWith(i => i.List!.User)
                .Where(i => i.UserId == userId)
                .ToListAsync(ct);

            var entities = models.Select(model => ModelMapper.MapFromModel(model));
            return entities.Where(predicate).ToList().AsReadOnly();
        }

        public async Task<ToDoItem?> Get(Guid id, CancellationToken ct)
        {
            using var dbContext = _factory.CreateDataContext();
            var model = await dbContext.ToDoItems
                .LoadWith(i => i.User)
                .LoadWith(i => i.List)
                .LoadWith(i => i.List!.User)
                .FirstOrDefaultAsync(i => i.Id == id, ct);

            return model != null ? ModelMapper.MapFromModel(model) : null;
        }

        public async Task<IReadOnlyList<ToDoItem>> GetActiveByUserId(Guid userId, CancellationToken ct)
        {
            using var dbContext = _factory.CreateDataContext();
            var models = await dbContext.ToDoItems
                .LoadWith(i => i.User)
                .LoadWith(i => i.List)
                .LoadWith(i => i.List!.User)
                .Where(i => i.UserId == userId && i.State == (int)ToDoItemState.Active)
                .ToListAsync(ct);

            var entities = models.Select(model => ModelMapper.MapFromModel(model));
            return entities.ToList().AsReadOnly();
        }

        public async Task<IReadOnlyList<ToDoItem>> GetAllByUserId(Guid userId, CancellationToken ct)
        {
            using var dbContext = _factory.CreateDataContext();
            var models = await dbContext.ToDoItems
                .LoadWith(i => i.User)
                .LoadWith(i => i.List)
                .LoadWith(i => i.List!.User)
                .Where(i => i.UserId == userId)
                .ToListAsync(ct);

            var entities = models.Select(model => ModelMapper.MapFromModel(model));
            return entities.ToList().AsReadOnly();
        }

        public async Task Update(ToDoItem item, CancellationToken ct)
        {
            using var dbContext = _factory.CreateDataContext();

            var existingModel = await dbContext.ToDoItems
                .FirstOrDefaultAsync(m => m.Id == item.Id, ct);
            if (existingModel == null) throw new ArgumentException($"Задача с ID {item.Id} не найдена");

            // Меняем состояние на противоположное (как было в InMemory)
            var newState = existingModel.State == (int)ToDoItemState.Active
                ? (int)ToDoItemState.Completed
                : (int)ToDoItemState.Active;

            // Обновляем только нужные поля
            await dbContext.ToDoItems
                .Where(m => m.Id == item.Id)
                .Set(m => m.State, newState)
                .Set(m => m.StateChangedAt, DateTime.UtcNow)
                .UpdateAsync(ct);
        }
    }
}
