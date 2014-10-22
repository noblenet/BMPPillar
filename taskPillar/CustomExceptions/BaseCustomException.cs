using System;

namespace PillarAPI.CustomExceptions
{
    [Serializable]
    public class BaseCustomException : Exception
    {
        public BaseCustomException()
        {
        }

        public BaseCustomException(string errorMessage) : base(errorMessage)
        {
        }

        public BaseCustomException(string errorMessage, Exception innerEx) : base(errorMessage, innerEx)
        {
        }

        public string ErrorMessage
        {
            get { return base.Message; }
        }
    }
}