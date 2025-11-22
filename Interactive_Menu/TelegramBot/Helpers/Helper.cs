using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Telegram.Bot.Types.ReplyMarkups;

namespace Interactive_Menu.TelegramBot.Helpers
{

    internal class Helper
    {
        internal ReplyKeyboardMarkup _keyboardBeforeRegistration = new ReplyKeyboardMarkup(
         new KeyboardButton[] { "/start" })
        {
            ResizeKeyboard = true,
        };
        internal ReplyKeyboardMarkup _keyboardAfterRegistration = new ReplyKeyboardMarkup(
            new KeyboardButton[] { "/addtask", "/show", "/report" })
        {
            ResizeKeyboard = true,
        };
        internal ReplyKeyboardMarkup _cancelKeyboard = new ReplyKeyboardMarkup(
    new KeyboardButton[] { "/cancel" })
        {
            ResizeKeyboard = true,
        };
    }


}
