using Interactive_Menu.Core.DataAccess;
using Interactive_Menu.Core.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
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
            _directory = Directory.Exists(directoryName) ? directoryName : Directory.CreateDirectory("filerep").Name;
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
                await using FileStream stream = new FileStream(
                    _directory + _directorySeparator + "index.json",
                    FileMode.OpenOrCreate,
                    FileAccess.ReadWrite,
                    FileShare.None
                );
                stream.Position = stream.Length;
                if (stream.Position == 0)
                {
                    await stream.WriteAsync(System.Text.Encoding.UTF8.GetBytes("["), cancellationToken: ct);
                }
                else if (stream.Position > 20)
                {
                    stream.Position -= 1;
                    await stream.WriteAsync(System.Text.Encoding.UTF8.GetBytes(", "), cancellationToken: ct);
                }
                await JsonSerializer.SerializeAsync(stream, new UserRecord(item.User.UserId, item.Id), cancellationToken: ct);
                await stream.WriteAsync(System.Text.Encoding.UTF8.GetBytes("]"), cancellationToken: ct);
                Console.WriteLine($"Связка {(item.User.UserId, item.Id)} - записана в файл {_directory + _directorySeparator}index.json");
            }
        }

        public Task<int> CountActive(Guid userId, CancellationToken ct)
        {
            throw new NotImplementedException();
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
                    var userId = userRecord.UserId;
                    var folderName = $"{userId}";
                    var fileName = $"{id}.json";
                    var fullFileName = _directory + _directorySeparator + folderName + _directorySeparator + fileName;
                    if (File.Exists(fullFileName))
                    {
                        File.Delete(fullFileName);
                        records.Remove(userRecord);
                        await using FileStream writeStream = File.Create(_directory + _directorySeparator + "index.json");
                        await JsonSerializer.SerializeAsync(writeStream, records, cancellationToken: ct);
                    }
                }
            }
        }

        public Task<bool> ExistsByName(Guid userId, string name, CancellationToken ct)
        {
            throw new NotImplementedException();
        }

        public Task<IReadOnlyList<ToDoItem>> Find(Guid userId, Func<ToDoItem, bool> predicate, CancellationToken ct)
        {
            throw new NotImplementedException();
        }

        public Task<ToDoItem?> Get(Guid id, CancellationToken ct)
        {
            throw new NotImplementedException();
        }

        public Task<IReadOnlyList<ToDoItem>> GetActiveByUserId(Guid userId, CancellationToken ct)
        {
            throw new NotImplementedException();
        }

        public Task<IReadOnlyList<ToDoItem>> GetAllByUserId(Guid userId, CancellationToken ct)
        {
            throw new NotImplementedException();
        }

        public Task Update(ToDoItem item, CancellationToken ct)
        {
            throw new NotImplementedException();
        }
    }
}
