using Interactive_Menu.Core.DataAccess;
using Interactive_Menu.Core.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Telegram.Bot;
using Telegram.Bot.Exceptions;

namespace Interactive_Menu.BackgroundTasks
{
    public class NotificationBackgroundTask : BackgroundTask
    {
        private readonly INotificationService _notificationService;
        private readonly ITelegramBotClient _bot;


        public NotificationBackgroundTask(
            INotificationService notificationService,
            ITelegramBotClient bot)
            : base(TimeSpan.FromMinutes(1), nameof(NotificationBackgroundTask))
        {
            _notificationService = notificationService;
            _bot = bot;
        }

        protected override async Task Execute(CancellationToken ct)
        {
            var notifications = await _notificationService.GetScheduledNotification(
                DateTime.UtcNow, ct);

            foreach (var notification in notifications)
            {
                try
                {
                    await _bot.SendMessage(
                        chatId: notification.User.TelegramUserId,
                        text: notification.Text,
                        cancellationToken: ct);

                    await _notificationService.MarkNotified(notification.Id, ct);

                }
                catch (ApiRequestException ex) when (ex.ErrorCode == 403)
                {
                    // Пользователь заблокировал бота, помечаем нотификацию как отправленную
                    await _notificationService.MarkNotified(notification.Id, ct);
                }
                catch (Exception ex)
                {
                    // Логируем ошибку, но не прерываем выполнение для других нотификаций
                    Console.WriteLine($"Ошибка отправки нотификации {notification.Id}: {ex.Message}");
                }
            }
        }
    }

}
