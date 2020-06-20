using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Telegram.Bot.Types.ReplyMarkups;
using TicTacToeTG.Core.Games;

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
        [field : NonSerialized]
        public int step { get; set; }
        public string targetCommand { get; set; }
        public Game currentGame { get; set; }
        public int gameMessageId { get; set; }
        public void Clear()
        {
            step = 0;
            targetCommand = "";
            currentGame = null;
            gameMessageId = 0;
        }
    }
}
