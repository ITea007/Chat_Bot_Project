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
        private string _directory;
        private char _directorySeparator = Path.DirectorySeparatorChar;
        private record UserRecord(Guid UserId, Guid Id);

        public FileToDoRepository(string directoryName)
        {
            _directory = Directory.Exists(directoryName) ? directoryName : Directory.CreateDirectory(directoryName).Name;
        }

        private async Task GenerateNewIndexJSON(CancellationToken ct)
        {
            List<UserRecord> records = new List<UserRecord>();
            var directories = Directory.EnumerateDirectories(_directory);
            await using FileStream writeStream = new FileStream(
                _directory + _directorySeparator + "index.json",
                FileMode.Create,
                FileAccess.Write,
                FileShare.None
                );
            foreach (var d in directories)
            {
                var files = Directory.GetFiles(d);
                foreach (var f in files)
                {
                    string[]? arr = f.Split(_directorySeparator);
                    Guid userId;
                    Guid toDoItemId;
                    if (Guid.TryParse(arr[1], out userId) && Guid.TryParse(arr[2].Replace(".json", ""), out toDoItemId))
                    {
                        records.Add(new UserRecord(userId, toDoItemId));
                        Console.WriteLine($"Added to index.json UserId: {userId} ToDoItemId: {toDoItemId} as {(userId, toDoItemId)}");
                    }
                }
            }
            await JsonSerializer.SerializeAsync(writeStream, records, cancellationToken: ct);
        }

        public async Task Add(ToDoItem item, CancellationToken ct)
        {
            List<UserRecord>? records = new List<UserRecord>();
            var userRecord = new UserRecord(item.User.UserId, item.Id);
            var folderName = $"{item.User.UserId}";
            var fileName = $"{item.Id}.json";
            var fullFileName = _directory + _directorySeparator + folderName + _directorySeparator + fileName;
            if (!Directory.Exists(_directory + _directorySeparator + folderName))
                Directory.CreateDirectory(_directory + _directorySeparator + folderName);
            await using FileStream createStream = File.Create(fullFileName);
            await JsonSerializer.SerializeAsync(createStream, item, cancellationToken: ct);
            Console.WriteLine($"Задача {item.Name} - {item.CreatedAt} - записана в файл {folderName + _directorySeparator + fileName}");
            if (!File.Exists(_directory + _directorySeparator + "index.json"))
                await GenerateNewIndexJSON(ct);
            else
            {
                await using FileStream readStream = File.OpenRead(_directory + _directorySeparator + "index.json");
                records = await JsonSerializer.DeserializeAsync<List<UserRecord>>(readStream, cancellationToken: ct);
                readStream.Close();
                if (records is null)
                {
                    records = new List<UserRecord> { userRecord };
                } else
                {
                    records.Add(userRecord);
                }
                await using FileStream writeStream = File.Create(_directory + _directorySeparator + "index.json");
                await JsonSerializer.SerializeAsync(writeStream, records, cancellationToken: ct);
            }
            Console.WriteLine($"Связка {(item.User.UserId, item.Id)} - записана в файл {_directory + _directorySeparator}index.json");
        }

        public async Task Delete(Guid id, CancellationToken ct)
        {
            List<UserRecord>? records = new List<UserRecord>();
            if (!File.Exists(_directory + _directorySeparator + "index.json"))
                await GenerateNewIndexJSON(ct);
            if (File.Exists(_directory + _directorySeparator + "index.json"))
            {
                await using FileStream readStream = File.OpenRead(_directory + _directorySeparator + "index.json");
                records = await JsonSerializer.DeserializeAsync<List<UserRecord>>(readStream, cancellationToken: ct);
            }
            if (records!=null && records.Count != 0)
            {
                var userRecord = records.FirstOrDefault(i => i.Id == id);
                if (userRecord != null)
                {
                    records.Remove(userRecord);
                    await using FileStream writeStream = File.Create(_directory + _directorySeparator + "index.json");
                    await JsonSerializer.SerializeAsync(writeStream, records, cancellationToken: ct);
                    var userId = userRecord.UserId;
                    var folderName = $"{userId}";
                    var fileName = $"{id}.json";
                    var fullFileName = _directory + _directorySeparator + folderName + _directorySeparator + fileName;
                    if (File.Exists(fullFileName))
                    {
                        File.Delete(fullFileName);
                    }
                }
            }
        }

        public async Task<bool> ExistsByName(Guid userId, string name, CancellationToken ct)
        {
            if (name is null) throw new ArgumentNullException();
            var folderName = $"{_directory}+{userId}";
            if (!Directory.Exists(folderName))
                return false;
            else
            {
                var files = Directory.GetFiles(folderName);
                foreach (var file in files)
                {
                    await using FileStream readStream = File.OpenRead(file);
                    var item = await JsonSerializer.DeserializeAsync<ToDoItem>(readStream, cancellationToken: ct);
                    if (item!= null && item.Name == name && item.User.UserId == userId)
                        return true;
                }
                return false;
            }
        }

        public async Task<IReadOnlyList<ToDoItem>> Find(Guid userId, Func<ToDoItem, bool> predicate, CancellationToken ct)
        {
            var allItems = await GetAllByUserId(userId, ct);
            return allItems.Where(predicate).ToList();
        }

        public async Task<ToDoItem?> Get(Guid id, CancellationToken ct)
        {
            ValueTask<ToDoItem?> outputItem = new ValueTask<ToDoItem?>();
            var files = Directory.GetFiles(_directory);
            foreach (var file in files)
            {
                if (file.Contains(id.ToString()))
                {
                    await using FileStream readStream = File.OpenRead(file);
                    var item = JsonSerializer.DeserializeAsync<ToDoItem>(readStream, cancellationToken: ct);
                    if (item.Result != null && item.Result.Id == id)
                        outputItem = item;
                }
            }
            return outputItem.Result;
        }

        public async Task<IReadOnlyList<ToDoItem>> GetActiveByUserId(Guid userId, CancellationToken ct)
        {
            List<ToDoItem> toDoItems = new List<ToDoItem>();
            var folderName = $"{userId}";
            if (Directory.Exists(folderName))
            {
                var files = Directory.GetFiles(folderName);
                foreach (var file in files)
                {
                    await using FileStream readStream = File.OpenRead(file);
                    var item = await JsonSerializer.DeserializeAsync<ToDoItem>(readStream, cancellationToken: ct);
                    if (item != null && item.User.UserId == userId && item.State == ToDoItemState.Active)
                    {
                        toDoItems.Add(item);
                    }
                }
            }
            return toDoItems;
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
            List<ToDoItem> toDoItems = new List<ToDoItem>();
            var folderName = $"{userId}";
            if (Directory.Exists(folderName))
            {
                var files = Directory.GetFiles(folderName);
                foreach (var file in files)
                {
                    await using FileStream readStream = File.OpenRead(file);
                    var item = await JsonSerializer.DeserializeAsync<ToDoItem>(readStream, cancellationToken: ct);
                    if (item != null && item.User.UserId == userId)
                    {
                        toDoItems.Add(item);
                    }
                }
            }
            return toDoItems;
        }

        public async Task Update(ToDoItem item, CancellationToken ct)
        {
            item.State = (item.State == ToDoItemState.Active) ? ToDoItemState.Completed : ToDoItemState.Active;
            var folderName = $"{item.User.UserId}";
            var fileName = $"{item.Id}.json";
            var fullFileName = _directory + _directorySeparator + folderName + _directorySeparator + fileName;
            if (!Directory.Exists(_directory + _directorySeparator + folderName))
                Directory.CreateDirectory(_directory + _directorySeparator + folderName);
            await using FileStream writeStream = File.OpenWrite(fullFileName);
            await JsonSerializer.SerializeAsync(writeStream, item, cancellationToken: ct);
        }
    }
}
