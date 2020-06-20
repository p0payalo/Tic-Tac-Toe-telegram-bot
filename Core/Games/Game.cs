using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Telegram.Bot.Types.ReplyMarkups;
using TicTacToeTG.Core.Types;

namespace TicTacToeTG.Core.Games
{
    [Serializable]
    public abstract class Game
    {
        protected int movesCount;
        public InlineKeyboardMarkup gameKeyboard;
        public abstract MoveResult Move(int position, BotUser user);
        public abstract void Start();
        public abstract void EndGame();
        protected bool CheckWinner()
        {
            const int size = 3;
            bool vertical = false;
            bool horizontal = false;
            bool diagonal = false;

            List<List<InlineKeyboardButton>> Keyboard = (List<List<InlineKeyboardButton>>)gameKeyboard.InlineKeyboard;
            //check winner vertical
            for (int i = 0; i < size; ++i)
            {
                for (int j = 1; j < size; ++j)
                {
                    vertical = Keyboard[j][i].Text == Keyboard[0][i].Text && Keyboard[j][i].Text != " ";
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
                    horizontal = Keyboard[i][j].Text == Keyboard[i][0].Text && Keyboard[i][j].Text != " ";
                    if (!horizontal)
                        break;
                }
                if (horizontal)
                    break;
            }

            //check winner dialog
            for (int i = 1; i < size; ++i)
            {
                diagonal = Keyboard[0][0].Text == Keyboard[i][i].Text && Keyboard[i][i].Text != " ";
                if (diagonal == false)
                    break;
            }

            if (diagonal) return true;

            for (int i = 0; i < size; ++i)
            {
                diagonal = Keyboard[0][size - 1].Text == Keyboard[i][size - i - 1].Text && Keyboard[i][size - i - 1].Text != " ";
                if (!diagonal)
                    break;
            }

            if (diagonal || horizontal || vertical)
                return true;
            else return false;
        }
    }
}
