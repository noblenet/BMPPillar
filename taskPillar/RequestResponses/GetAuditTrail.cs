using System;
using System.Globalization;
using System.Linq;
using System.Reflection;
using PetaPoco;
using bmpxsd;
using log4net;
using PillarAPI.Interfaces;
using PillarAPI.Models;

namespace PillarAPI.RequestResponses
{
    public class GetAuditTrail : IGetAuditTrail
    {
        private static readonly ILog Log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

        public void ProcessRequest(IMessageInfoContainer message)
        {

            var receivedGetAuditTrailsRequest = message.MessageObject as GetAuditTrailsRequest;
            var responseInfo = new ResponseInfo();
            if (receivedGetAuditTrailsRequest != null)
            {
                bool whereBool = true;
                var sql = Sql.Builder
                .Append("SELECT actionDateTime, actionOnFile, actorOnFile, auditTrailInformation, fileName, info, reportingComponent, sequenceNumber, file_id FROM Audit");

                if (!string.IsNullOrEmpty(receivedGetAuditTrailsRequest.FileID))
                {
                    sql.Append("WHERE fileName = @0 ", receivedGetAuditTrailsRequest.FileID);
                    whereBool = false;
                }

                if (receivedGetAuditTrailsRequest.MinTimestampSpecified)
                {
                    sql.Append(whereBool ? "WHERE actionDateTIme >= @0" : "AND actionDateTIme >= @0 ", receivedGetAuditTrailsRequest.MinTimestamp);
                }

                if (receivedGetAuditTrailsRequest.MaxTimestampSpecified)
                {
                    sql.Append(whereBool ? "WHERE actionDateTIme <= @0 " : "AND actionDateTIme <= @0 ", receivedGetAuditTrailsRequest.MaxTimestamp);
                }

                if (!string.IsNullOrEmpty(receivedGetAuditTrailsRequest.MinSequenceNumber))
                {
                    sql.Append(whereBool ? "WHERE sequenceNumber >= @0" : "AND sequenceNumber >= @0 ", receivedGetAuditTrailsRequest.MinSequenceNumber);
                }

                if (!string.IsNullOrEmpty(receivedGetAuditTrailsRequest.MaxSequenceNumber))
                {
                    sql.Append(whereBool ? "WHERE sequenceNumber <= @0" : "AND sequenceNumber <= @0", receivedGetAuditTrailsRequest.MaxSequenceNumber);
                }
                AuditTrailEvent[] auditTrailEventContainer;

                using (var db = DatabaseConnection.GetConnection())
                {
                    try
                    {
                        var auditPocos = db.Query<AuditPoco>(sql);
                        var rowCount = auditPocos.Count();
                        auditTrailEventContainer = new AuditTrailEvent[rowCount];
                        if (auditPocos.Any())
                        {
                            int rowPointer = 0;
                            foreach (var auditTrailEvent in from AuditPoco audit in auditPocos
                                                            select new AuditTrailEvent
                                                                {
                                                                    ActionDateTime = audit.actionDateTime,
                                                                    ActionOnFile = (FileAction)Enum.Parse(typeof(FileAction), audit.actionOnFile),
                                                                    ActorOnFile = audit.actorOnFile,
                                                                    AuditTrailInformation = audit.auditTrailInformation,
                                                                    FileID = audit.file_id.ToString(CultureInfo.InvariantCulture),
                                                                    Info = audit.info,
                                                                    ReportingComponent = audit.reportingComponent,
                                                                    SequenceNumber = audit.sequenceNumber.ToString()
                                                                })
                            {
                                auditTrailEventContainer[rowPointer] = auditTrailEvent;
                                rowPointer++;
                            }
                        }
                        else
                        {
                            responseInfo.ResponseCode = ResponseCode.OPERATION_COMPLETED;
                            responseInfo.ResponseText = "Audittrail request completed successfully. But no audit matches specified request.";
                            GetAuditTrailReply(receivedGetAuditTrailsRequest, responseInfo, null);
                            return;
                        }
                    }
                    catch (Exception e)
                    {
                        responseInfo.ResponseCode = ResponseCode.FAILURE;
                        responseInfo.ResponseText = "Audittrail request failed";
                        GetAuditTrailReply(receivedGetAuditTrailsRequest, responseInfo, null);
                        Log.Error("Get audittrail DB error: ", e);
                        return;
                    }
                }

                responseInfo.ResponseCode = ResponseCode.OPERATION_COMPLETED;
                responseInfo.ResponseText = "Audittrail request completed successfully";
                var auditTrailEvents = new AuditTrailEvents { AuditTrailEvent = auditTrailEventContainer };
                ResultingAuditTrails resultingAuditTrails;
                if (string.IsNullOrEmpty(receivedGetAuditTrailsRequest.ResultAddress))
                {
                    resultingAuditTrails = new ResultingAuditTrails {Item = auditTrailEvents};
                }
                else
                {
                    resultingAuditTrails = new ResultingAuditTrails {Item = receivedGetAuditTrailsRequest.ResultAddress};
                }
                GetAuditTrailReply(receivedGetAuditTrailsRequest, responseInfo, resultingAuditTrails);
            }
        }

        private static void GetAuditTrailReply(GetAuditTrailsRequest receivedGetAuditTrailsRequest, ResponseInfo responseInfo, ResultingAuditTrails resultingAuditTrails)
        {
            GetAuditTrailsFinalResponse responseObject = new GetAuditTrailsFinalResponse
            {
                CollectionID = receivedGetAuditTrailsRequest.CollectionID,
                Contributor =Pillar.GlobalPillarApiSettings.PILLAR_ID,
                CorrelationID = receivedGetAuditTrailsRequest.CorrelationID,
                Destination = receivedGetAuditTrailsRequest.ReplyTo,
                From =Pillar.GlobalPillarApiSettings.PILLAR_ID,
                ReplyTo =Pillar.GlobalPillarApiSettings.SA_PILLAR_QUEUE,
                ResponseInfo = responseInfo,
                To = receivedGetAuditTrailsRequest.From,
                minVersion =Pillar.GlobalPillarApiSettings.MIN_MESSAGE_XSD_VERSION,
                version =Pillar.GlobalPillarApiSettings.XSD_VERSION,
                PartialResult = false,
                PartialResultSpecified = false,
                ResultingAuditTrails = resultingAuditTrails
            };

            var returnMessage = new MessageInfoContainer(responseObject);
            returnMessage.Send();
        }
    }
}