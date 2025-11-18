using Interactive_Menu.Core.DataAccess;
using Interactive_Menu.Core.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace Interactive_Menu.Infrastructure.DataAccess
{
    internal class FileToDoListRepository : IToDoListRepository
    {

        private readonly string _directory;

        public FileToDoListRepository(string directoryName)
        {
            _directory = Directory.Exists(directoryName) ? directoryName : Directory.CreateDirectory(directoryName).Name;
        }

        public async Task Add(ToDoList list, CancellationToken ct)
        {
            var fileName = $"{list.Id}.json";
            var fullFileName = Path.Combine(_directory, fileName);

            if (!Directory.Exists(_directory))
                Directory.CreateDirectory(_directory);

            await using FileStream createStream = File.Create(fullFileName);
            await JsonSerializer.SerializeAsync(createStream, list, cancellationToken: ct);
        }

        public Task Delete(Guid id, CancellationToken ct)
        {
            var fileName = $"{id}.json";
            var fullFileName = Path.Combine(_directory, fileName);

            if (File.Exists(fullFileName))
            {
                File.Delete(fullFileName);
            }

            return Task.CompletedTask;
        }

        public async Task<bool> ExistsByName(Guid userId, string name, CancellationToken ct)
        {
            if (!Directory.Exists(_directory))
                return false;

            var files = Directory.GetFiles(_directory, "*.json");
            foreach (var file in files)
            {
                await using FileStream readStream = File.OpenRead(file);
                var list = await JsonSerializer.DeserializeAsync<ToDoList>(readStream, cancellationToken: ct);
                if (list != null && list.User.UserId == userId && list.Name == name)
                    return true;
            }
            return false;
        }

        public async Task<ToDoList?> Get(Guid id, CancellationToken ct)
        {
            var fileName = $"{id}.json";
            var fullFileName = Path.Combine(_directory, fileName);

            if (!File.Exists(fullFileName))
                return null;

            await using FileStream readStream = File.OpenRead(fullFileName);
            return await JsonSerializer.DeserializeAsync<ToDoList>(readStream, cancellationToken: ct);
        }

        public async Task<IReadOnlyList<ToDoList>> GetByUserId(Guid userId, CancellationToken ct)
        {
            var result = new List<ToDoList>();

            if (!Directory.Exists(_directory))
                return result.AsReadOnly();

            var files = Directory.GetFiles(_directory, "*.json");
            foreach (var file in files)
            {
                await using FileStream readStream = File.OpenRead(file);
                var list = await JsonSerializer.DeserializeAsync<ToDoList>(readStream, cancellationToken: ct);
                if (list != null && list.User.UserId == userId)
                    result.Add(list);
            }

            return result.AsReadOnly();
        }
    }
}
