namespace PillarAPI.Interfaces
{
    public interface IResponseBuilderForIdentifyPillarsForGetFileRequest
    {
        void SendResponse(IMessageInfoContainer message);
    }
}