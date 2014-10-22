using System.Reflection;
using PillarAPI.CustomExceptions;
using PillarAPI.Interfaces;
using log4net;

namespace PillarAPI.RequestResponses
{
    internal class ChecksumGetFile : IGetFile
    {
        private static readonly ILog Log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);


        public void ProcessRequest(IMessageInfoContainer message)
        {
            Log.Debug("Ignoring request");
            throw new NotApplicableException("This method is not supported in this context");
        }
    }
}