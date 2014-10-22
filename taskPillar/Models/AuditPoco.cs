using System;
using PetaPoco;

namespace PillarAPI.Models
{
    [TableName("audit")]
    [PrimaryKey("sequenceNumber", autoIncrement = true)]
    public class AuditPoco
    {
        [ResultColumn]
        public DateTime actionDateTime { get; set; }

        public string actionOnFile { get; set; }
        public string actorOnFile { get; set; }
        public string auditTrailInformation { get; set; }
        public string fileName { get; set; }
        public string info { get; set; }
        public string reportingComponent { get; set; }

        [ResultColumn]
        public int sequenceNumber { get; set; }

        public int file_id { get; set; }
    }
}