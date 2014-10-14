using System;

namespace PillarAPI.CustomExceptions
{
    class SigningVerifyingMessageException : BaseCustomException
    {
        public SigningVerifyingMessageException()
            : base()
        {
        }

        public SigningVerifyingMessageException(string errorMessage) : base(errorMessage)
        {
        }

        public SigningVerifyingMessageException(string errorMessage, Exception innerException) : base(errorMessage, innerException)
        {
        }
    }
}
