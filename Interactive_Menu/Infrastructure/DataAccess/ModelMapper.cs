using Interactive_Menu.Core.DataAccess.Models;
using Interactive_Menu.Core.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Interactive_Menu.Infrastructure.DataAccess
{
    internal static class ModelMapper
    {
        public static ToDoUser MapFromModel(ToDoUserModel model)
        {
            ArgumentNullException.ThrowIfNull(model);

            return new ToDoUser
            {
                UserId = model.UserId,
                TelegramUserId = model.TelegramUserId,
                TelegramUserName = model.TelegramUserName,
                RegisteredAt = model.RegisteredAt
            };
        }

        public static ToDoUserModel MapToModel(ToDoUser entity)
        {
            ArgumentNullException.ThrowIfNull(entity);

            return new ToDoUserModel
            {
                UserId = entity.UserId,
                TelegramUserId = entity.TelegramUserId,
                TelegramUserName = entity.TelegramUserName,
                RegisteredAt = entity.RegisteredAt
            };
        }

        public static ToDoItem MapFromModel(ToDoItemModel model)
        {
            ArgumentNullException.ThrowIfNull(model);

            return new ToDoItem
            {
                Id = model.Id,
                UserId = model.UserId,
                User = model.User != null ? MapFromModel(model.User) : throw new InvalidDataException("ToDoItem must have User"),
                Name = model.Name,
                CreatedAt = model.CreatedAt,
                State = (ToDoItemState)model.State,
                StateChangedAt = model.StateChangedAt,
                Deadline = model.Deadline,
                ListId = model.ListId,
                List = model.List != null ? MapFromModel(model.List) : null
            };
        }

        public static ToDoItemModel MapToModel(ToDoItem entity)
        {
            ArgumentNullException.ThrowIfNull(entity);

            return new ToDoItemModel
            {
                Id = entity.Id,
                UserId = entity.UserId,
                Name = entity.Name,
                CreatedAt = entity.CreatedAt,
                State = (int)entity.State,
                StateChangedAt = entity.StateChangedAt,
                Deadline = entity.Deadline,
                ListId = entity.ListId
            };
        }

        public static ToDoList MapFromModel(ToDoListModel model)
        {
            ArgumentNullException.ThrowIfNull(model);

            return new ToDoList
            {
                Id = model.Id,
                Name = model.Name,
                UserId = model.UserId,
                User = model.User != null ? MapFromModel(model.User) : throw new InvalidDataException("ToDoList must have User"),
                CreatedAt = model.CreatedAt
            };
        }

        public static ToDoListModel MapToModel(ToDoList entity)
        {
            ArgumentNullException.ThrowIfNull(entity);

            return new ToDoListModel
            {
                Id = entity.Id,
                Name = entity.Name,
                UserId = entity.UserId,
                CreatedAt = entity.CreatedAt
            };
        }

        public static NotificationModel MapToModel(Notification entity)
        {
            ArgumentNullException.ThrowIfNull(entity);

            return new NotificationModel
            {
                Id = entity.Id,
                UserId = entity.UserId,
                Type = entity.Type,
                Text = entity.Text,
                ScheduledAt = entity.ScheduledAt,
                IsNotified = entity.IsNotified,
                NotifiedAt = entity.NotifiedAt
            };
        }

        public static Notification MapFromModel(NotificationModel model)
        {
            ArgumentNullException.ThrowIfNull(model);

            return new Notification
            {
                Id = model.Id,
                User = model.User != null ? MapFromModel(model.User) : throw new InvalidDataException("ToDoList must have User"),
                UserId = model.UserId,
                Type = model.Type,
                Text = model.Text,
                ScheduledAt = model.ScheduledAt,
                IsNotified = model.IsNotified,
                NotifiedAt = model.NotifiedAt
            };
        }
    }
}
