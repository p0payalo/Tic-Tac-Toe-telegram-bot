using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Telegram.Bot;
using Telegram.Bot.Types.ReplyMarkups;

namespace TicTacToeTG.Core
{
    public static class BotResponses
    {
        public static string GetStartMessage() => 
            "This bot was creating for play tic tac toe game with your friend <b>WARNING! for playing this game you need to have username in telegram</b>";

        public static string GetHelpMessage() =>
            "/help - show commands\n/newgame - start new game";

        public static string GetUsernameErrorText() =>
            "To play this game you need have username in telegram, change it and try again";

        public static string GetUsersIsBusyMessage() =>
            "User currently in a game, wait";

        public static string GetUserNotExistMessage() =>
            "User not exist or user don't use this bot\nTry again";

        public static string GetWaitMessage() =>
            "Please wait, we wait for player response";

        public static string GetNewGameMessage() =>
            "Enter target username";


        public static InlineKeyboardMarkup GetGameKeyboard() =>
            new InlineKeyboardMarkup(new List<List<InlineKeyboardButton>>()
                    {
                        new List<InlineKeyboardButton>()
                        {
                            InlineKeyboardButton.WithCallbackData(" " , "11"),
                            InlineKeyboardButton.WithCallbackData(" " , "12"),
                            InlineKeyboardButton.WithCallbackData(" " , "13")
                        },
                        new List<InlineKeyboardButton>()
                        {
                            InlineKeyboardButton.WithCallbackData(" " , "21"),
                            InlineKeyboardButton.WithCallbackData(" " , "22"),
                            InlineKeyboardButton.WithCallbackData(" " , "23")
                        },
                        new List<InlineKeyboardButton>()
                        {
                            InlineKeyboardButton.WithCallbackData(" " , "31"),
                            InlineKeyboardButton.WithCallbackData(" " , "32"),
                            InlineKeyboardButton.WithCallbackData(" " , "33"),
                        },
                        new List<InlineKeyboardButton>()
                        {
                            InlineKeyboardButton.WithCallbackData("Exit" , "exit")
                        }
                    });

        public static InlineKeyboardMarkup GetExitKeyboard() =>
            new InlineKeyboardMarkup(new List<List<InlineKeyboardButton>>()
                    {
                        new List<InlineKeyboardButton>()
                        {
                            InlineKeyboardButton.WithCallbackData("Exit" , "exit"),
                        }
                    });

        public static InlineKeyboardMarkup GetAcceptOrCancelKeyboard() =>

            new InlineKeyboardMarkup(new List<List<InlineKeyboardButton>>()
                    {
                        new List<InlineKeyboardButton>()
                        {
                            InlineKeyboardButton.WithCallbackData("Accept" , "accept"),
                        },
                        new List<InlineKeyboardButton>()
                        {
                            InlineKeyboardButton.WithCallbackData("Cancel" , "cancel"),
                        }
                    });

    }
}
