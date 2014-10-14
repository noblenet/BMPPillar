using System;
using System.Collections.Generic;
using System.Data.SQLite;
using System.IO;
using Apache.NMS;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using PetaPoco;
using pillarAPI;
using PillarAPI.ActiveMQ;
using PillarAPI.Utilities;

namespace PillarTest
{
    [TestClass]
    public class TestPetaPoco
        {
        private PillarApiSettings _pillarApiSettings;
        [ClassInitialize]
        public void  TestInitialize()
        {
            _pillarApiSettings = (PillarApiSettings)SerializationUtilities.DeserializeObject(File.ReadAllText(Properties.Settings.Default.Path2Settingsfile), typeof(PillarApiSettings));
            
        }

       
            [TestMethod]
        public void TestCleanDBWithPetapoco()
        {
            // arrange
            var db = new Database(_pillarApiSettings.SQLITE_CONNECTION_STRING, new SQLiteFactory());

            db.CloseSharedConnection();
            var tableNames = new[] { "files", "checksums", "file_specs", "audit" };

            // act
            foreach (string tableName in tableNames)
            {
                db.Execute(string.Format("DELETE FROM {0}", tableName));
            }
            int collectedCounts = 0;
            foreach (string tableName in tableNames)
            {
                collectedCounts = +db.Execute(string.Format("SELECT count(*) FROM {0}", tableName));
            }
            db.CompleteTransaction();
            // assert
            Assert.IsTrue(collectedCounts == 0);
        }
        [TestMethod]
        public void TestCleanDir()
        {
            Directory.Delete(_pillarApiSettings.COLLECTION_FILE_DIRECTORY);
            Assert.IsFalse(Directory.Exists(_pillarApiSettings.COLLECTION_FILE_DIRECTORY));
        }
        [Microsoft.VisualStudio.TestTools.UnitTesting.Ignore]
        [TestMethod]
        public void CleanTestFiles()
        {
            const int initNoOfFiles = 1;
            IEnumerable<string> dirs = Directory.EnumerateDirectories(_pillarApiSettings.COLLECTION_FILE_DIRECTORY);
            foreach (string dir in dirs)
            {
                Directory.Delete(dir, true);
            }
            Assert.AreEqual(initNoOfFiles, Directory.GetFiles(_pillarApiSettings.COLLECTION_FILE_DIRECTORY).GetLongLength(0));
        }

        [Microsoft.VisualStudio.TestTools.UnitTesting.Ignore]
        [TestMethod]
        public void CleanQue()
        {
            using (IConnection connection = ActiveMQSetup.Connection)
            {
                using (ISession session = ActiveMQSetup.GetSession(connection))
                {
                    session.DeleteQueue(_pillarApiSettings.SA_PILLAR_QUEUE);
                    session.DeleteQueue(@"queue://sa_test_client");
                    ITopic topic = session.GetTopic(@"topic://sa-test");
                    Assert.IsNotNull(topic);
                    session.DeleteTopic(topic.TopicName);
                }
            }
        }

 [Microsoft.VisualStudio.TestTools.UnitTesting.Ignore]
        [TestMethod]
        public void PutFileTester()
        {
            int fileCount = 100001;
            var data = new byte[8192];
            var rand = new Random();
            string dir = @"C:\BitTestFilesMany";

            if (!Directory.Exists(dir))
            {
                Directory.CreateDirectory(dir);
            }

             CreateFiles(fileCount, dir, rand, data);
            
            Assert.AreEqual(Directory.GetFiles(dir).GetLongLength(0), fileCount);
        }

        private static void CreateFiles(int fileCount, string dir, Random rand, byte[] data)
        {
            for (int i = 0; i < fileCount; i++)
            {
                using (FileStream stream = File.OpenWrite(Path.Combine(dir, Path.GetRandomFileName() + ".bit")))
                {
                    int fileSize = rand.Next(2, 8)*128;
                    const int fixedSize = 11;
                    fileSize = fixedSize;
                    for (int a = 10; a < fileSize; a++)
                    {
                        rand.NextBytes(data);
                        stream.Write(data, 0, data.Length);
                    }
                }
            }
        }

        private void CleanTestFiles(string dirSti)
        {
            string[] dirs = Directory.GetFiles(dirSti);
            foreach (string dir in dirs)
            {
                File.Delete(dir);
            }
        }
    }
}