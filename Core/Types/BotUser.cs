using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Telegram.Bot.Types.ReplyMarkups;

namespace TicTacToeTG.Core.Types
{
    [Serializable]
    public class BotUser
    {
        public BotUser()
        {

        }
        public long chatId { get; set; }
        public string username { get; set; }
        public int step { get; set; }
        public string targetCommand { get; set; }
        public Game currentGame { get; set; }
        public int gameMessageId { get; set; }
    }
}
