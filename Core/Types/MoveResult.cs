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
        public bool Standoff { get; set; }

        public MoveResult(bool canMove, bool gameFinished, bool standOff)
        {
            CanMove = canMove;
            GameFinished = gameFinished;
            Standoff = standOff;
        }
    }
}
