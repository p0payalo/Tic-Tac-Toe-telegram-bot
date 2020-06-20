using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Telegram.Bot.Types.ReplyMarkups;
using TicTacToeTG.Core.Exceptions;
using TicTacToeTG.Core.Types;

namespace TicTacToeTG.Core.Games
{
    [Serializable]
    public class TicTacToeAI : Game
    {
        string Human;
        string AI;
        bool userMove;

        public BotUser Player { get; set; }
        public TicTacToeAI(BotUser player, char HumanChar = 'O')
        {
            if (Char.ToUpper(HumanChar) == 'O')
            {
                Human = HumanChar.ToString();
                AI = 'X'.ToString();
            }
            else if (Char.ToUpper(HumanChar) == 'X')
            {
                Human = HumanChar.ToString();
                AI = 'O'.ToString();
            }
            else throw new UnknownCharException("Unknown player char symbol");
            Player = player;
        }

        public override void Start()
        {
            Player.step = -1;
            Player.targetCommand = "game";
            Player.currentGame = this;
            movesCount = 0;
            userMove = false;
            gameKeyboard = BotResponses.GetGameKeyboard();
        }
        public override void EndGame()
        {
            Player.step = 0;
            Player.targetCommand = "";
            Player.currentGame = null;
        }

        public override MoveResult Move(int position, BotUser user)
        {
            List<List<InlineKeyboardButton>> Keyboard = (List<List<InlineKeyboardButton>>)gameKeyboard.InlineKeyboard;
            if (user == Player && !userMove)
            {
                return new MoveResult(false, false, false);
            }
            else if (user == null)
            {
                Keyboard[position / 10 - 1][position % 10 - 1].Text = AI;
                userMove = true;
                movesCount++;
                if (CheckWinner())
                {
                    return new MoveResult(false, true, false, true);
                }
                else if (movesCount == 9)
                {
                    return new MoveResult(false, true, true);
                }
                else return new MoveResult(true, false, false);
            }
            else if (Keyboard[position / 10 - 1][position % 10 - 1].Text == " " && user == Player)
            {
                Keyboard[position / 10 - 1][position % 10 - 1].Text = Human;
                userMove = false;
                movesCount++;
                if (CheckWinner() && user != null)
                {
                    return new MoveResult(false, true, false);
                }
                else if (movesCount == 9)
                {
                    return new MoveResult(false, true, true);
                }
                else return new MoveResult(true, false, false);
            }
            else return new MoveResult(false, false, false);
        }
        public MoveResult AiMove()
        {
            List<List<InlineKeyboardButton>> Keyboard = (List<List<InlineKeyboardButton>>)gameKeyboard.InlineKeyboard;
            int bestScore = -99999;
            Move move = new Move(0, 0);
            for (int i = 0; i < 3; i++)
            {
                for(int j = 0; j < 3; j++)
                {
                    if(Keyboard[i][j].Text == " ")
                    {
                        Keyboard[i][j].Text = AI;
                        int score = minimax((List<List<InlineKeyboardButton>>)gameKeyboard.InlineKeyboard, Human, movesCount);
                        Keyboard[i][j].Text = " ";
                        if(score > bestScore)
                        {
                            bestScore = score;
                            move.i = i;
                            move.j = j;
                        }
                    }
                }
            }
            MoveResult res = Move((move.i + 1) * 10 + move.j + 1, null);
            return res;
        }

        private int minimax(List<List<InlineKeyboardButton>> keyboard, string player, int moves)
        {
            if (moves == 9) return 0;
            if(CheckWinner())
            {
                if (player == AI) return -10;
                else return 10;
            }
            if (player == AI)
            {
                int bestScore = -999999;
                for (int i = 0; i < 3; ++i)
                {
                    for (int j = 0; j < 3; ++j)
                    {
                        if(keyboard[i][j].Text == " ")
                        {
                            keyboard[i][j].Text = AI;
                            int score = minimax(keyboard, Human, moves + 1);
                            keyboard[i][j].Text = " ";
                            bestScore = Math.Max(bestScore, score);
                        }
                    }
                }
                return bestScore;
            }
            else
            {
                int bestScore = 999999;
                for (int i = 0; i < 3; ++i)
                {
                    for (int j = 0; j < 3; ++j)
                    {
                        if (keyboard[i][j].Text == " ")
                        {
                            keyboard[i][j].Text = Human;
                            int score = minimax(keyboard, AI, moves + 1);
                            keyboard[i][j].Text = " ";
                            bestScore = Math.Min(bestScore, score);
                        }
                    }
                }
                return bestScore;
            }
        }
    }

    class Move
    {
        public Move(int iter, int jter)
        {
            i = iter;
            j = jter;
        }
        public int i { get; set; }
        public int j { get; set; }
    }

}
