using System;

namespace PillarAPI.CustomExceptions
{
    internal class QueueNotAccessibleException : BaseCustomException
    {
        public QueueNotAccessibleException()
        {
        }

        public QueueNotAccessibleException(string errorMessage)
            : base(errorMessage)
        {
        }

        public QueueNotAccessibleException(string errorMessage, Exception innerEx)
            : base(errorMessage, innerEx)
        {
        }

        public new string ErrorMessage
        {
            get { return base.Message; }
        }
    }
}