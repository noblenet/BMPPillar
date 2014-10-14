using System;
using System.Data.SQLite;
using System.Reflection;
using PetaPoco;
using log4net;

namespace PillarAPI
{
    internal static class DatabaseConnection
    {
        private static readonly ILog Log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

        public static Database GetConnection()
        {
            Log.Debug("Connecting to the Database...");
            try
            {
                using (var db = new Database(Pillar.GlobalPillarApiSettings.SQLITE_CONNECTION_STRING , new SQLiteFactory()))
                {
                    db.OpenSharedConnection();
                    db.EnableAutoSelect = true;
                    db.EnableNamedParams = false;
                    return db;
                }
            }
            catch (Exception e)
            {
                Log.Fatal(e);
                throw;
            }
        }
    }
}