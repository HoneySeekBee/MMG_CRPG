using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Common
{
    public class GameErrorException : Exception
    {
        public string ErrorCode { get; }

        public GameErrorException(string code, string message) : base(message)
        {
            ErrorCode = code;
        }
    }
}
