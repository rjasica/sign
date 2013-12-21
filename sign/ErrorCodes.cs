using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Sign
{
    public static class ErrorCodes
    {
        public const int Ok = 0;

        public const int DefaultErrorCode = 1;

        public const int InternalError = 2;

        public const int ParserError = 3;

        public const int KeyError = 4;

        public const int AssemblyReadError = 5;
    }
}
