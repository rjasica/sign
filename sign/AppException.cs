using System;
using System.Runtime.Serialization;

namespace Sign
{
    [Serializable]
    public class AppException : Exception
    {
        public AppException()
        {
            this.ErrorCode = ErrorCodes.DefaultErrorCode;
        }

        public AppException(string message) : base(message)
        {
            this.ErrorCode = ErrorCodes.DefaultErrorCode;
        }

        public AppException(string message, Exception inner) : base(message, inner)
        {
            this.ErrorCode = ErrorCodes.DefaultErrorCode;
        }

        public AppException(int errorCode)
        {
            this.ErrorCode = errorCode;
        }

        public AppException(string message, int errorCode)
            : base(message)
        {
            this.ErrorCode = errorCode;
        }

        public AppException(string message, Exception inner, int errorCode)
            : base(message, inner)
        {
            this.ErrorCode = errorCode;
        }

        protected AppException(
            SerializationInfo info,
            StreamingContext context) : base(info, context)
        {
            this.ErrorCode = info.GetInt32("ErrorCode");
        }

        public int ErrorCode { get; private set; }
    }
}
