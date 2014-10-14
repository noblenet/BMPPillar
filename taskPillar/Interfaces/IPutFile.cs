namespace PillarAPI.Interfaces
{
    public interface IPutFile
    {
        /// <summary>
        /// </summary>
        /// <param name="message"></param>
        /// ++++++++++++ Tjekkes der for om en anden proces er ved at lægge den samme fil op? +++++++++++++++
        void ProcessRequest(IMessageInfoContainer message);
    }
}