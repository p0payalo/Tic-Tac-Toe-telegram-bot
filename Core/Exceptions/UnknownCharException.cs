using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TicTacToeTG.Core.Exceptions
{
    public class UnknownCharException : Exception
    {
        public UnknownCharException() : base() { }
        public UnknownCharException(string message) : base(message) { }
        public UnknownCharException(string message, Exception innerExeption) : base(message, innerExeption) { }
    }
}
