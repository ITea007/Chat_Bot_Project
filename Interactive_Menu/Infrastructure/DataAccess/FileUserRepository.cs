using Interactive_Menu.Core.DataAccess;
using Interactive_Menu.Core.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using Telegram.Bot.Types;

namespace Interactive_Menu.Infrastructure.DataAccess
{
    internal class FileUserRepository : IUserRepository
    {
        private string _directory;
        private char _directorySeparator = Path.DirectorySeparatorChar;

        public FileUserRepository(string directoryName)
        {
            _directory = Directory.Exists(directoryName) ? directoryName : Directory.CreateDirectory(directoryName).Name;
        }

        public async Task Add(ToDoUser user, CancellationToken ct)
        {
            var fileName = $"{user.UserId}.json";
            var fullFileName = _directory + _directorySeparator + fileName;
            if (!Directory.Exists(_directory))
                Directory.CreateDirectory(_directory);
            await using FileStream createStream = File.Create(fullFileName);
            await JsonSerializer.SerializeAsync(createStream, user, cancellationToken: ct);
            Console.WriteLine($"Пользователь {user.UserId} - *{user.TelegramUserName}* - {user.TelegramUserId} - записан в файл {fullFileName}");
        }

        public async Task<ToDoUser?> GetUser(Guid userId, CancellationToken ct)
        {
            var fileName = $"{userId.ToString()}.json";
            var fullFileName = _directory + _directorySeparator + fileName;
            if (Directory.Exists(_directory))
            {
                await using FileStream readStream = File.OpenRead(fullFileName);
                var outputUser = await JsonSerializer.DeserializeAsync<ToDoUser>(readStream, cancellationToken: ct);
            }
            return null;
        }

        public async Task<ToDoUser?> GetUserByTelegramUserId(long telegramUserId, CancellationToken ct)
        {
            if (Directory.Exists(_directory))
            {
                var files = Directory.GetFiles(_directory);
                foreach (var f in files)
                {
                    await using FileStream readStream = File.OpenRead(f);
                    var user = await JsonSerializer.DeserializeAsync<ToDoUser>(readStream, cancellationToken: ct);
                    if (user != null && user.TelegramUserId == telegramUserId)
                        return user;
                }
            }
            return null;
        }
    }
}
