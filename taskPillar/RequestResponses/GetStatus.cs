using System;
using PillarAPI.Interfaces;
using bmpxsd;

namespace PillarAPI.RequestResponses
{
    public class GetStatus : IGetStatus
    {
        public void ProcessRequest(IMessageInfoContainer message)
        {
            var receivedGetStatusRequest = message.MessageObject as GetStatusRequest;
            var statusInfo = new StatusInfo
                                 {
                                     StatusCode = StatusCode.OK,
                                     StatusText = "Status fra SA pillar. Tingene kører som de skal"
                                 };
            var resultingStatus = new ResultingStatus {StatusInfo = statusInfo, StatusTimestamp = DateTime.Now};
            var responseInfo = new ResponseInfo
                                   {
                                       ResponseCode = ResponseCode.OPERATION_COMPLETED,
                                       ResponseText = "Get pillar status completed successfully"
                                   };

            var responseObject = new GetStatusFinalResponse
                                     {
                                         CollectionID = receivedGetStatusRequest.CollectionID,
                                         Contributor = Pillar.GlobalPillarApiSettings.PILLAR_ID,
                                         CorrelationID = receivedGetStatusRequest.CorrelationID,
                                         Destination = receivedGetStatusRequest.ReplyTo,
                                         From = Pillar.GlobalPillarApiSettings.PILLAR_ID,
                                         minVersion = Pillar.GlobalPillarApiSettings.MIN_MESSAGE_XSD_VERSION,
                                         ReplyTo = Pillar.GlobalPillarApiSettings.SA_PILLAR_QUEUE,
                                         ResponseInfo = responseInfo,
                                         ResultingStatus = resultingStatus,
                                         To = receivedGetStatusRequest.From,
                                         version = Pillar.GlobalPillarApiSettings.XSD_VERSION
                                     };
            new MessageInfoContainer(responseObject).Send();
        }
    }
}