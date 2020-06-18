using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.ReplyMarkups;
using TicTacToeTG.Core.Types;

namespace TicTacToeTG.Core
{
    [Serializable]
    public class Game
    {
        public BotUser firstPlayer { get; set; }
        public BotUser secondPlayer { get; set; }
        public InlineKeyboardMarkup gameKeyboard { get; set; }
        private int movesCount;

        public Game(BotUser fPlayer, BotUser sPlayer)
        {
            firstPlayer = fPlayer;
            secondPlayer = sPlayer;
        }

        public BotUser GetOpponent(BotUser user)
        {
            if (user == firstPlayer) return secondPlayer;
            else return firstPlayer;
        }

        public void Start()
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

        public void EndGame()
        {
            firstPlayer.currentGame = null;
            secondPlayer.currentGame = null;
            firstPlayer.step = 0;
            secondPlayer.step = 0;
            firstPlayer.targetCommand = "";
            secondPlayer.targetCommand = "";
        }

        public MoveResult Move(int position, BotUser user)
        {
            if(user == firstPlayer && movesCount % 2 == 1)
            {
                return new MoveResult(false, false, false);
            }
            else if (user == secondPlayer && movesCount % 2 == 0)
            {
                return new MoveResult(false, false, false);
            }
            if (gameKeyboard.InlineKeyboard.ToList()[position / 10 - 1].ToList()[position % 10 - 1].Text == " ")
            {
                if (movesCount % 2 == 0)
                {
                    gameKeyboard.InlineKeyboard.ToList()[position / 10 - 1].ToList()[position % 10 - 1].Text = "X";
                }
                else gameKeyboard.InlineKeyboard.ToList()[position / 10 - 1].ToList()[position % 10 - 1].Text = "O";
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

        private bool CheckWinner()
        {
            int size = 3;
            bool vertical = false;
            bool horizontal = false;
            bool diagonal = false;

            List<List<InlineKeyboardButton>> map = (List<List<InlineKeyboardButton>>)gameKeyboard.InlineKeyboard;

            //check winner vertical
            for (int i = 0; i < size; ++i)
            {
                for (int j = 1; j < size; ++j)
                {
                    vertical = map[j][i].Text == map[0][i].Text && map[j][i].Text != " ";
                    if (!vertical)
                        break;
                }
                if (vertical)
                    break;
            }

            //check winner horizontal
            for (int i = 0; i < size; ++i)
            {
                for (int j = 1; j < size; ++j)
                {
                    horizontal = map[i][j].Text == map[i][0].Text && map[i][j].Text != " ";
                    if (!horizontal)
                        break;
                }
                if (horizontal)
                    break;
            }

            //check winner dialog
            for (int i = 1; i < size; ++i)
            {
                diagonal = map[0][0].Text == map[i][i].Text && map[i][i].Text != " ";
                if (diagonal == false)
                    break;
            }

            if (diagonal) return true;

            for (int i = 0; i < size; ++i)
            {
                diagonal = map[0][size - 1].Text == map[i][size - i - 1].Text && map[i][size - i - 1].Text != " ";
                if (!diagonal)
                    break;
            }

            if (diagonal || horizontal || vertical)
                return true;
            else return false;
        }
    }
}
