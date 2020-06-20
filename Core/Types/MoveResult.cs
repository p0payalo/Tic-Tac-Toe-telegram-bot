using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TicTacToeTG.Core.Types
{
    public class MoveResult
    {
        public bool CanMove { get; set; }
        public bool GameFinished { get; set; }
        public bool Tie { get; set; }
        public bool BotWin { get; set; }

        public MoveResult(bool canMove, bool gameFinished, bool tie)
        {
            CanMove = canMove;
            GameFinished = gameFinished;
            Tie = tie;
            BotWin = false;
        }
        public MoveResult(bool canMove, bool gameFinished, bool tie, bool botWin)
        {
            CanMove = canMove;
            GameFinished = gameFinished;
            Tie = tie;
            BotWin = botWin;
        }
    }
}
