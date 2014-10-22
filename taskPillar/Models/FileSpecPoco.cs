using System;
using PetaPoco;

namespace PillarAPI.Models
{
    [TableName("file_specs")]
    [PrimaryKey("file_spec_id", autoIncrement = true)]
    public class FileSpecPoco
    {
        [ResultColumn] // ignored on inserts
        public int file_spec_id { get; set; }

        public int file_id { get; set; }
        public bool archived { get; set; }
        public int file_size { get; set; }
        public bool active { get; set; }

        [ResultColumn] // ignored on inserts
        public DateTime received { get; set; }

        public string filepath { get; set; }
    }
}