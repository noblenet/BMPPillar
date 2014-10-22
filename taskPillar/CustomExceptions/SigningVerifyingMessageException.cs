using System;

namespace PillarAPI.CustomExceptions
{
    internal class SigningVerifyingMessageException : BaseCustomException
    {
        public SigningVerifyingMessageException()
        {
        }

        public SigningVerifyingMessageException(string errorMessage) : base(errorMessage)
        {
        }

        public SigningVerifyingMessageException(string errorMessage, Exception innerException)
            : base(errorMessage, innerException)
        {
        }
    }
}