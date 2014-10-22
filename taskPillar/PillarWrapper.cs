using System;
using PillarAPI.Interfaces;

namespace PillarAPI
{
    public class PillarWrapper : IPillarWrapper
    {
        private readonly IGetAuditTrail _getAuditTrail;
        private readonly IGetChecksum _getChecksum;
        private readonly IGetFile _getFile;
        private readonly IGetFileId _getFileId;
        private readonly IGetStatus _getStatus;
        private readonly IPutFile _putFile;


        /// <summary>
        ///     Initializes a new instance of the <see cref="PillarWrapper" /> class.
        /// </summary>
        public PillarWrapper(IPutFile putFile, IGetFile getFile, IGetChecksum getChecksum, IGetStatus getStatus,
                             IGetAuditTrail getAuditTrail, IGetFileId getFileId)
        {
            _putFile = putFile;
            _getFile = getFile;
            _getChecksum = getChecksum;
            _getStatus = getStatus;
            _getAuditTrail = getAuditTrail;
            _getFileId = getFileId;
        }


        public void PutFile(IMessageInfoContainer message)
        {
            _putFile.ProcessRequest(message);
        }

        public void GetChecksum(IMessageInfoContainer message)
        {
            _getChecksum.ProcessRequest(message);
        }

        public void GetFile(IMessageInfoContainer message)
        {
            _getFile.ProcessRequest(message);
        }

        public void GetFileId(IMessageInfoContainer message)
        {
            _getFileId.ProcessRequest(message);
        }

        public void GetAuditTrail(IMessageInfoContainer message)
        {
            _getAuditTrail.ProcessRequest(message);
        }

        public void GetStatus(IMessageInfoContainer message)
        {
            _getStatus.ProcessRequest(message);
        }

        public void DeleteFile(IMessageInfoContainer message)
        {
            throw new NotImplementedException();
        }

        public void ReplaceFile(IMessageInfoContainer message)
        {
            throw new NotImplementedException();
        }

        public void ProcessRequest(IMessageInfoContainer message)
        {
            throw new NotImplementedException();
        }
    }
}