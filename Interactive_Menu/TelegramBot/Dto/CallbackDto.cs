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
        public string Action { get; set; } //с помощью него будет определять за какое действие отвечает кнопка

        //Методы:
        public static CallbackDto FromString(string input) { return new CallbackDto(); } //На вход принимает строку ввида "{action}|{prop1}|{prop2}...".

        //Нужно создать CallbackDto с Action = action.
        //Нужно учесть что в строке может не быть |, тогда всю строку сохраняем в Action.
        public override string ToString() { return ""; }//- переопределить метод. Он должен возвращать Action
    }
}
