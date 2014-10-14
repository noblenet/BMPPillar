using System.Reflection;
using log4net;
using PillarAPI.CustomExceptions;
using PillarAPI.Interfaces;

namespace PillarAPI.RequestResponses
    {
    class ChecksumGetFile : IGetFile
        {
        private static readonly ILog Log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

        
        public void ProcessRequest(IMessageInfoContainer message)
        {
            Log.Debug("Ignoring request");
            throw new NotApplicableException("This method is not supported in this context");
        }
        }
    }