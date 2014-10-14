using System;
using System.Reflection;
using PetaPoco;
using log4net;
using PillarAPI.Enums;
using PillarAPI.Models;

namespace PillarAPI
{
    public class FileInfoContainer
    {
        private static readonly ILog Log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

        public FileInfoContainer(string collectionId, string fileName)
        {
            Sql queryString = Sql.Builder
                .Append("SELECT fs.file_id, MAX(fs.file_spec_id) Max_file_spec_id, fs.file_size, fs.received, fs.archived, f.deleted, fs.filepath")
                .Append("FROM files f, file_specs fs")
                .Append("WHERE f.file_name = @0", fileName)
                .Append("AND f.user_id = (SELECT user_id FROM users WHERE collection_id = @0)", collectionId)
                .Append("AND f.file_id = fs.file_id");

            using (var db = DatabaseConnection.GetConnection())
            {
                try
                {

                    var fileInfo = db.SingleOrDefault<dynamic>(queryString);
                    //if (string.IsNullOrEmpty(fileInfo.file_id))
                    if ((fileInfo.file_id ?? 0) == 0)
                    {
                        FileStates = FileStatesEnum.NeverExisted;
                        return;
                    }
                    FileId = fileInfo.file_id.ToString();
                    FileSpecId = fileInfo.Max_file_spec_id.ToString();
                    FileSize = (long)fileInfo.file_size;
                    Received = (DateTime)fileInfo.received;
                    Archived = fileInfo.archived;
                    FileStates = fileInfo.deleted == 0 ? FileStatesEnum.ExistingInDB : FileStatesEnum.DeletedFromDB;
                    FilePath = fileInfo.filepath;
                    FileName = fileName;
                    CollectionId = collectionId;
                }
                catch (Exception e)
                {
                    Log.Error("FileInfoContainer DB error: ", e);
                }
            }
        }

        public FileStatesEnum FileStates { get; private set; }
        public string CollectionId { get; private set; }
        public string FileName { get; private set; }
        public string FileSpecId { get; private set; }
        public long FileSize { get; private set; }
        public string FileId { get; private set; }
        public DateTime Received { get; private set; }
        public bool Archived { get; private set; }
        public string FilePath { get; private set; }
    }
}