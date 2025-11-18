using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Interactive_Menu.TelegramBot.DTO
{
    internal class CallbackDto
    {

        //Общий класс в котором есть Action Свойства:  
        public string Action { get; set; } = string.Empty; //с помощью него будет определять за какое действие отвечает кнопка

        //Методы:
        //На вход принимает строку ввида "{action}|{prop1}|{prop2}...".
        public static CallbackDto FromString(string input)
        {
            if (string.IsNullOrEmpty(input))
                return new CallbackDto { Action = string.Empty };

            var parts = input.Split('|', 2);
            return new CallbackDto { Action = parts[0] };
        }

        //Нужно создать CallbackDto с Action = action.
        //Нужно учесть что в строке может не быть |, тогда всю строку сохраняем в Action.
        public override string ToString()
        {
            return Action;
        }
    }
}
