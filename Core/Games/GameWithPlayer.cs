using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.ReplyMarkups;
using TicTacToeTG.Core.Types;

namespace TicTacToeTG.Core.Games
{
    [Serializable]
    public class GameWithPlayer : Game
    {
        public BotUser firstPlayer { get; set; }
        public BotUser secondPlayer { get; set; }

        public GameWithPlayer(BotUser fPlayer, BotUser sPlayer)
        {
            firstPlayer = fPlayer;
            secondPlayer = sPlayer;
        }

        public BotUser GetOpponent(BotUser user)
        {
            if (user == firstPlayer) return secondPlayer;
            else return firstPlayer;
        }

        public override void Start()
        {
            movesCount = 0;
            firstPlayer.currentGame = this;
            secondPlayer.currentGame = this;
            firstPlayer.step = -1;
            secondPlayer.step = -1;
            firstPlayer.targetCommand = "game";
            secondPlayer.targetCommand = "game";
            gameKeyboard = BotResponses.GetGameKeyboard();
        }

        public override void EndGame()
        {
            firstPlayer.currentGame = null;
            secondPlayer.currentGame = null;
            firstPlayer.step = 0;
            secondPlayer.step = 0;
            firstPlayer.targetCommand = "";
            secondPlayer.targetCommand = "";
        }

        public override MoveResult Move(int position, BotUser user)
        {
            List<List<InlineKeyboardButton>> Keyboard = (List<List<InlineKeyboardButton>>)gameKeyboard.InlineKeyboard;
            if (user == firstPlayer && movesCount % 2 == 1)
            {
                return new MoveResult(false, false, false);
            }
            else if (user == secondPlayer && movesCount % 2 == 0)
            {
                return new MoveResult(false, false, false);
            }
            if (Keyboard[position / 10 - 1][position % 10 - 1].Text == " ")
            {
                if (movesCount % 2 == 0)
                {
                    Keyboard[position / 10 - 1][position % 10 - 1].Text = "X";
                }
                else Keyboard[position / 10 - 1][position % 10 - 1].Text = "O";
                movesCount++;
                if (CheckWinner())
                {
                    return new MoveResult(true, true, false);
                }
                else if (movesCount == 9)
                {
                    return new MoveResult(true, true, true);
                }
                else return new MoveResult(true, false, false);
            }
            else return new MoveResult(false, false, false);
        }
    }
}
