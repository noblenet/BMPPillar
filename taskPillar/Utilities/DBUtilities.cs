using System.Collections.Generic;
using System.Reflection;
using System.Text;
using log4net;

namespace PillarAPI.Utilities
{
    public static class DBUtilities
    {
        private static readonly ILog Log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

        /// <summary>
        /// Cleans the files from database if left hanging.
        /// </summary>
        /// <exception cref="System.NotImplementedException"></exception>
        public static void CleanFilesFromDBIfLeftHanging()
        {
            using (var db = DatabaseConnection.GetConnection())
            {
                using (var trans = db.GetTransaction())
                {
                    var result = db.Execute("DELETE FROM FILES WHERE file_id NOT IN (SELECT file_id FROM file_specs)");
                    Log.DebugFormat("Cleaning up the database for files without file_spec. Delete {0} entries",result);
                    trans.Complete();
                }
            }
        }

        public static IEnumerable<string> TableNames
        {
            get
            {
                  using (var db = DatabaseConnection.GetConnection())
            {return db.Query<string>("SELECT name FROM sqlite_master WHERE type='table';");
            }
            }
        }

        private static IEnumerable<KeyValuePair<string, int>> RowCounts
        {
            get
            {
                using (var db = DatabaseConnection.GetConnection())
                {
                    foreach (string tableName in TableNames)
                    {
                        yield return
                            new KeyValuePair<string, int>(tableName,
                                                          db.ExecuteScalar<int>(string.Format(
                                                              "Select count(1) from {0}",
                                                              tableName)));
                    }
                }
            }
        }

        public static string PrintInfo()
        {
            var tableCount = 0;
            var sb = new StringBuilder();
            sb.AppendLine("Info about tables in the DB:");
            foreach (var keyValuePair in RowCounts)
            {
                sb.AppendLine(string.Format("Table:{0} have {1} rows", keyValuePair.Key, keyValuePair.Value));
                tableCount++;
            }
            sb.AppendLine(string.Format("There are {0} tables", tableCount));
            return sb.ToString();
        }
    }
}