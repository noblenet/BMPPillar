using System;

namespace PillarAPI.CustomExceptions
{
    [Serializable]
    public class NotFromValidCollectionException : BaseCustomException
    {
        public NotFromValidCollectionException(string correlationID)
        {
            CorrelationID = correlationID;
        }

        public string CorrelationID { get; private set; }
    }
}