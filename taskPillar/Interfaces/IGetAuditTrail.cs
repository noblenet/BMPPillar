namespace PillarAPI.Interfaces
{
public interface IGetAuditTrail
    {
        void ProcessRequest(IMessageInfoContainer message);
    }
}
