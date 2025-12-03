using Interactive_Menu.Core.DataAccess;
using Interactive_Menu.Core.Entities;
using Interactive_Menu.Core.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Telegram.Bot.Types;

namespace Interactive_Menu.BackgroundTasks
{
    public class TodayBackgroundTask : BackgroundTask
    {
        private readonly INotificationService _notificationService;
        private readonly IUserRepository _userRepository;
        private readonly IToDoRepository _toDoRepository;

        public TodayBackgroundTask(
            INotificationService notificationService,
            IUserRepository userRepository,
            IToDoRepository toDoRepository)
            : base(TimeSpan.FromDays(1), nameof(TodayBackgroundTask))
        {
            _notificationService = notificationService;
            _userRepository = userRepository;
            _toDoRepository = toDoRepository;
        }

        protected override async Task Execute(CancellationToken ct)
        {
            var users = await _userRepository.GetUsers(ct);
            var today = DateTime.UtcNow.Date;
            var tomorrow = today.AddDays(1);

            foreach (var user in users)
            {
                var todayTasks = await _toDoRepository.GetActiveWithDeadline(user.UserId, today, tomorrow, ct);

                if (todayTasks.Any())
                {
                    var taskList = string.Join("\n", todayTasks.Select(t => $"• {t.Name}"));
                    var text = $"Задачи на сегодня ({today:dd.MM.yyyy}):\n{taskList}";

                    await _notificationService.ScheduleNotification(
                        userId: user.UserId,
                        type: $"Today_{today}",
                        text: text,
                        scheduledAt: DateTime.UtcNow.Date.AddHours(9), // Отправляем в 9 утра
                        ct);
                }
            }
        }
    }
}
