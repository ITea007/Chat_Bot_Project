using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Interactive_Menu.Core.Entities
{
    internal class Notification
    {
        Guid Id;
        ToDoUser User;
        string Type; //Тип нотификации. Например: DeadLine_{ToDoItem.Id}, Today_{DateOnly.FromDateTime(DateTime.UtcNow)}
        string Text; //Текст, который будет отправлен
        DateTime ScheduledAt; //Запланированная дата отправки
        bool IsNotified; //Флаг отправки
        DateTime? NotifiedAt; //Фактическая дата отправки
    }
}
