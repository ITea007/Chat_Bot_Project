using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Telegram.Bot.Types.ReplyMarkups;

namespace Interactive_Menu.TelegramBot
{

    internal class Helper
    {
        internal ReplyKeyboardMarkup _keyboardBeforeRegistration = new ReplyKeyboardMarkup(
         new KeyboardButton[] { "/start" })
        {
            ResizeKeyboard = true,
        };
        internal ReplyKeyboardMarkup _keyboardAfterRegistration = new ReplyKeyboardMarkup(
            new KeyboardButton[] { "/showalltasks", "/showtasks", "/report" })
        {
            ResizeKeyboard = true,
        };
    }


}
