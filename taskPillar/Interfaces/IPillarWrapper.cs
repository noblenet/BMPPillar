namespace PillarAPI.Interfaces
{
    public interface IPillarWrapper
    {
        void PutFile(IMessageInfoContainer message);
        void GetChecksum(IMessageInfoContainer message);
        void GetFile(IMessageInfoContainer message);
        void GetFileId(IMessageInfoContainer message);
        void GetAuditTrail(IMessageInfoContainer message);
        void GetStatus(IMessageInfoContainer message);
        void DeleteFile(IMessageInfoContainer message);
        void ReplaceFile(IMessageInfoContainer message);
    }
}