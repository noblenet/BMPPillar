using System;
using PetaPoco;

namespace PillarAPI.Models
{
    [TableName("checksums")]
    public class ChecksumPoco
    {
        public int file_spec_id { get; set; }
        public int algorithm_id { get; set; }
        public byte[] checksum { get; set; }
        public byte[] salt { get; set; }
        public DateTime date { get; set; }
    }
}