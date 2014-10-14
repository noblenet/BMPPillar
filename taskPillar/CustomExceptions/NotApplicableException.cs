namespace PillarAPI.CustomExceptions
    {
    internal class NotApplicableException : BaseCustomException
        {
        public NotApplicableException(string errorMessage)
            : base(errorMessage)
        {
        }
        }
    }