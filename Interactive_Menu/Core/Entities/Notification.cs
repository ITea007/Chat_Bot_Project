using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Interactive_Menu.Core.Entities
{
    public class Notification
    {
        public Guid Id { get; set; }
        public Guid UserId { get; set; }
        public string Type { get; set; } = string.Empty; //Тип нотификации. Например: DeadLine_{ToDoItem.Id}, Today_{DateOnly.FromDateTime(DateTime.UtcNow)}
        public string Text { get; set; } = string.Empty; //Текст, который будет отправлен
        public DateTime ScheduledAt { get; set; } //Запланированная дата отправки
        public bool IsNotified { get; set; } //Флаг отправки
        public DateTime? NotifiedAt { get; set; } //Фактическая дата отправки
        public ToDoUser User { get; set; } = null!;
    }
}
