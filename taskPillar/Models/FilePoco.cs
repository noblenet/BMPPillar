using PetaPoco;

namespace PillarAPI.Models
{
    [TableName("files")]
    [PrimaryKey("file_id")]
    public class FilePoco
    {
        [ResultColumn]
        public int file_id { get; set; }
        public int user_id { get; set; }
        public string file_name { get; set; }
        public bool deleted { get; set; }
    }
}
