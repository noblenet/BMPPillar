using System.Reflection;
using PillarAPI.Interfaces;
using log4net;

namespace PillarAPI.IdentifyResponses
{
    internal class ChecksumResponseBuilderForIdentifyPillarsForGetFileRequest :
        IResponseBuilderForIdentifyPillarsForGetFileRequest
    {
        private static readonly ILog Log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

        public void SendResponse(IMessageInfoContainer message)
        {
            // Do nothing a.k.a. Ignore request
            Log.Debug("Ignoring request");
        }
    }
}