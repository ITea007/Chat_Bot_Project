using Interactive_Menu.Core.DataAccess.Models;
using Interactive_Menu.Core.Entities;
using Interactive_Menu.Core.Services;
using Interactive_Menu.Infrastructure.DataAccess;
using LinqToDB;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Interactive_Menu.Infrastructure.Services
{
    public class NotificationService : INotificationService
    {
        private readonly IDataContextFactory<ToDoDataContext> _factory;

        public NotificationService(IDataContextFactory<ToDoDataContext> factory)
        {
            _factory = factory;
        }

        public async Task MarkNotified(Guid notificationId, CancellationToken ct)
        {
            using var context = _factory.CreateDataContext();

            await context.Notifications
                .Where(n => n.Id == notificationId)
                .Set(n => n.IsNotified, true)
                .Set(n => n.NotifiedAt, DateTime.UtcNow)
                .UpdateAsync(ct);
        }

        public async Task<bool> ScheduleNotification(Guid userId, string type, string text, DateTime scheduledAt, CancellationToken ct)
        {
            using var context = _factory.CreateDataContext();

            var existingNotification = await context.Notifications
                .FirstOrDefaultAsync(n => n.UserId == userId && n.Type == type, ct);

            if (existingNotification != null)
                return false;

            var notification = new NotificationModel
            {
                Id = Guid.NewGuid(),
                UserId = userId,
                Type = type,
                Text = text,
                ScheduledAt = scheduledAt,
                IsNotified = false,
                NotifiedAt = null
            };

            await context.InsertAsync(notification, token: ct);
            return true;
        }


        public async Task<IReadOnlyList<Notification>> GetScheduledNotification(DateTime scheduledBefore, CancellationToken ct)
        {
            using var context = _factory.CreateDataContext();

            var models = await context.Notifications
                .LoadWith(n => n.User)
                .Where(n => !n.IsNotified && n.ScheduledAt <= scheduledBefore)
                .ToListAsync(ct);
            var entities = models.Select(model => ModelMapper.MapFromModel(model));
            return entities.ToList().AsReadOnly();
        }
    }
}
