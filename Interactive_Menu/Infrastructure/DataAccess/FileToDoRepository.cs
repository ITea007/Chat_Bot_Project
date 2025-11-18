using Interactive_Menu.Core.DataAccess;
using Interactive_Menu.Core.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using System.Xml.Linq;
using Telegram.Bot.Types;

namespace Interactive_Menu.Infrastructure.DataAccess
{
    //Реализовать хранение ToDoItem в отдельных json файлах.
    //Имя файла: "{ToDoItem.Id}.json"
    //Имя базовой папки нужно получать через конструктор. Папку нужно создавать если её нет.
    //Для хранения данных в файлах использовать json формат. Для этого нужно использовать
    //библиотеку System.Text.Json и методы JsonSerializer.Serialize JsonSerializer.Deserialize.

    internal class FileToDoRepository : IToDoRepository
    {
        private readonly string _directory;
        private readonly char _directorySeparator = Path.DirectorySeparatorChar;
        private readonly string _indexFilePath;

        private record UserRecord(Guid UserId, Guid Id);

        public FileToDoRepository(string directoryName)
        {
            _directory = Directory.Exists(directoryName) ? directoryName : Directory.CreateDirectory(directoryName).Name;
            _indexFilePath = Path.Combine(_directory, "index.json");
        }

        private async Task GenerateNewIndexJSON(CancellationToken ct)
        {
            var userRecords = new List<UserRecord>();
            var directories = Directory.EnumerateDirectories(_directory);

            // Собираем записи
            foreach (var d in directories)
            {
                var files = Directory.GetFiles(d);
                foreach (var f in files)
                {
                    string[]? arr = f.Split(_directorySeparator);
                    if (arr.Length >= 3)
                    {
                        if (Guid.TryParse(arr[^2], out Guid userId) &&
                            Guid.TryParse(Path.GetFileNameWithoutExtension(arr[^1]), out Guid toDoItemId))
                        {
                            userRecords.Add(new UserRecord(userId, toDoItemId));
                        }
                    }
                }
            }
            // Записываем в файл - FileStream автоматически закроется после using
            await using (var writeStream = new FileStream(
                _indexFilePath,
                FileMode.Create,
                FileAccess.Write,
                FileShare.None))
            {
                await JsonSerializer.SerializeAsync(writeStream, userRecords, cancellationToken: ct);
                await writeStream.FlushAsync(ct); // Принудительно сбрасываем буфер
            }
        }

        public async Task Add(ToDoItem item, CancellationToken ct)
        {
            var fullFileName = Path.Combine(_directory, $"{item.User.UserId}", $"{item.Id}.json");
            var folder = Path.Combine(_directory, $"{item.User.UserId}");

            if (!Directory.Exists(folder))
                Directory.CreateDirectory(folder);

            // Запись задачи - FileStream автоматически закроется после using
            await using (var createStream = File.Create(fullFileName))
            {
                await JsonSerializer.SerializeAsync(createStream, item, cancellationToken: ct);
            }

            if (!File.Exists(_indexFilePath))
            {
                await GenerateNewIndexJSON(ct);
            }
            else
            {
                List<UserRecord>? records;

                // Чтение index.json - FileStream автоматически закроется
                await using (var readStream = File.OpenRead(_indexFilePath))
                {
                    records = await JsonSerializer.DeserializeAsync<List<UserRecord>>(readStream, cancellationToken: ct);
                }

                if (records is null)
                {
                    records = new List<UserRecord> { new UserRecord(item.User.UserId, item.Id) };
                }
                else
                {
                    records.Add(new UserRecord(item.User.UserId, item.Id));
                }

                // Запись index.json - FileStream автоматически закроется
                await using (var writeStream = File.Create(_indexFilePath))
                {
                    await JsonSerializer.SerializeAsync(writeStream, records, cancellationToken: ct);
                }
            }
        }

        public async Task Delete(Guid id, CancellationToken ct)
        {
            if (!File.Exists(_indexFilePath))
                await GenerateNewIndexJSON(ct);

            if (File.Exists(_indexFilePath))
            {
                List<UserRecord>? records;

                // Чтение index.json - FileStream автоматически закроется
                await using (var readStream = File.OpenRead(_indexFilePath))
                {
                    records = await JsonSerializer.DeserializeAsync<List<UserRecord>>(readStream, cancellationToken: ct);
                }

                if (records != null && records.Count != 0)
                {
                    var userRecord = records.FirstOrDefault(i => i.Id == id);
                    if (userRecord != null)
                    {
                        records.Remove(userRecord);

                        // Запись обновленного index.json - FileStream автоматически закроется
                        await using (var writeStream = File.Create(_indexFilePath))
                        {
                            await JsonSerializer.SerializeAsync(writeStream, records, cancellationToken: ct);
                        }

                        var userId = userRecord.UserId;
                        var folderName = $"{userId}";
                        var fileName = $"{id}.json";
                        var fullFileName = Path.Combine(_directory, folderName, fileName);

                        if (File.Exists(fullFileName))
                        {
                            File.Delete(fullFileName);
                        }
                    }
                }
            }
        }

        public async Task<bool> ExistsByName(Guid userId, string name, CancellationToken ct)
        {
            if (name is null)
                throw new ArgumentNullException(nameof(name));

            var folderName = Path.Combine(_directory, $"{userId}");

            if (!Directory.Exists(folderName))
                return false;

            var files = Directory.GetFiles(folderName, "*.json");

            foreach (var file in files)
            {
                // FileStream автоматически закроется после using
                await using (var readStream = File.OpenRead(file))
                {
                    var item = await JsonSerializer.DeserializeAsync<ToDoItem>(readStream, cancellationToken: ct);
                    if (item != null && item.Name == name && item.User.UserId == userId)
                        return true;
                }
            }

            return false;
        }

        public async Task<IReadOnlyList<ToDoItem>> Find(Guid userId, Func<ToDoItem, bool> predicate, CancellationToken ct)
        {
            var allItems = await GetAllByUserId(userId, ct);
            return allItems.Where(predicate).ToList();
        }

        public async Task<ToDoItem?> Get(Guid id, CancellationToken ct)
        {
            if (!Directory.Exists(_directory))
                return null;

            var directories = Directory.GetDirectories(_directory);

            foreach (var directory in directories)
            {
                var files = Directory.GetFiles(directory, "*.json");
                foreach (var file in files)
                {
                    // Более надежная проверка по имени файла
                    var fileName = Path.GetFileNameWithoutExtension(file);
                    if (Guid.TryParse(fileName, out Guid fileTaskId) && fileTaskId == id)
                    {
                        // FileStream автоматически закроется после using
                        await using (var readStream = File.OpenRead(file))
                        {
                            var item = await JsonSerializer.DeserializeAsync<ToDoItem>(readStream, cancellationToken: ct);
                            if (item != null && item.Id == id)
                                return item;
                        }
                    }
                }
            }

            return null;
        }

        public async Task<IReadOnlyList<ToDoItem>> GetActiveByUserId(Guid userId, CancellationToken ct)
        {
            var activeItems = new List<ToDoItem>();
            var userFolder = Path.Combine(_directory, $"{userId}");

            if (!Directory.Exists(userFolder))
                return activeItems;

            foreach (var file in Directory.GetFiles(userFolder, "*.json"))
            {
                await using (var stream = File.OpenRead(file))
                {
                    var item = await JsonSerializer.DeserializeAsync<ToDoItem>(stream, cancellationToken: ct);
                    if (item?.State == ToDoItemState.Active)
                        activeItems.Add(item);
                }
            }

            return activeItems.AsReadOnly();
        }

        public async Task<int> CountActive(Guid userId, CancellationToken ct)
        {
            var list = await GetActiveByUserId(userId, ct);
            if (list == null) return 0;
            var count = list.Count;
            return count;
        }

        public async Task<IReadOnlyList<ToDoItem>> GetAllByUserId(Guid userId, CancellationToken ct)
        {
            var userTasks = new List<ToDoItem>();
            var userFolder = Path.Combine(_directory, userId.ToString());

            if (Directory.Exists(userFolder))
            {
                foreach (var file in Directory.GetFiles(userFolder, "*.json"))
                {
                    await using (var stream = File.OpenRead(file))
                    {
                        var task = await JsonSerializer.DeserializeAsync<ToDoItem>(stream, cancellationToken: ct);
                        if (task != null)
                            userTasks.Add(task);
                    }
                }
            }

            return userTasks.AsReadOnly();
        }

        public async Task Update(ToDoItem item, CancellationToken ct)
        {
            item.State = item.State == ToDoItemState.Active
                ? ToDoItemState.Completed
                : ToDoItemState.Active;
            item.StateChangedAt = DateTime.UtcNow;

            var taskFile = Path.Combine(_directory, item.User.UserId.ToString(), $"{item.Id}.json");
            var taskDir = Path.GetDirectoryName(taskFile);

            if (!string.IsNullOrEmpty(taskDir) && !Directory.Exists(taskDir))
                Directory.CreateDirectory(taskDir);

            await using (var stream = File.Create(taskFile))
            {
                await JsonSerializer.SerializeAsync(stream, item, cancellationToken: ct);
            }
        }
    }
}
