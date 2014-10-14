using PetaPoco;

namespace PillarAPI.Models
{
    [TableName("checksum_types")]
    [PrimaryKey("algorithm_id")]
    public class ChecksumsTypePoco
    {
        public int algorithm_id { get; set; }
        public string algorithm { get; set; }
    }
}