using System.Data.SQLite;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using PetaPoco;

namespace PillarTest
{
    [TestClass]
    public class PutFileTest
    {
        [TestMethod]
        public void TestCreateSelectCmd()
        {
            // arrange
            //const string collectionID = "sa-test";
            //const string fileName = "wireshark.exe";
            //const string dataSource = @"Data Source=C:\Udvikling\bmpSQLiteTest.s3db;Version=3;Pooling=True;MaxPoolSize=100;";
            //var connection = new SQLiteConnection(dataSource);

            //// act
            //var selectCmd = PutFile.CreateSelectCmd(fileName, collectionID, connection);
            //connection.Open();
            //SQLiteDataReader reader = selectCmd.ExecuteReader();
            //bool fileExists = reader[0] != DBNull.Value;

            //// assert
            //Assert.IsTrue(fileExists, "fejl");
        }

        [TestMethod]
        public void TestConnectWithPetapoco()
        {
            // arrange
            const string dataSource = @"Data Source=D:\Udvikling\bmpSQLiteTest.s3db;Version=3";
            var provider = new SQLiteFactory();
            var db = new Database(dataSource, provider);
            db.CloseSharedConnection();
            // act
            var i = db.ExecuteScalar<int>("Select count(*) from sqlite_master");
            // assert
            Assert.IsTrue(i > 0);
        }
    }
}