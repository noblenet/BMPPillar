using System.Runtime.CompilerServices;

namespace StorageSuite
{
    using System.IO;

    using NUnit.Framework;

    using StorageHandler;

    [TestFixture]
    public class TestSave
    {
        #region Fields
        // Denne test forudsætter at der er et Drev (her I) med en række foldere der har foldere.
        // Altså et "Drev med foldere med under foldere - det skal emulere SAN'ets opsætning
        private const string folder4Testfiles = "Testfiles";
        private const string testDrive = "I";
        private const int maxFilesPrFolder = 3;

        #endregion Fields

        #region Methods

        [TestFixtureTearDown]
        public void FinalTearDown()
        {
            Directory.Delete(folder4Testfiles,true);
            CleanReceivingFolders();
        }


        [TestCase(1)]
        [TestCase(2)]
        [TestCase(3)]
        [TestCase(4)]
        [TestCase(5)]
        public void FirstTest(int time)
        {
            // Step 1 - Arrange
            var testObject = new Handler(testDrive) { MaxFilesPrFolder = maxFilesPrFolder };
            var fileName = Path.Combine(folder4Testfiles, "Slet.mig" + time);
            CreateFile(fileName);

            var testFile = new FileInfo(fileName);
            // Step 2 - Act
            var result = testObject.Savefile(testFile,"New"+testFile.Name);

            // Step 3 - Assert
            Assert.True(File.Exists(result));
        }

        [TestFixtureSetUp]
        public void InitialSetup()
        {
            Directory.CreateDirectory(folder4Testfiles);
        }

        private static void CreateFile(string sletMig)
        {
            string path = sletMig;
            using (StreamWriter sw = File.AppendText(path))
            {
                sw.WriteLine("This is a test file and can be deleted");
            }
        }
        private void CleanReceivingFolders()
        {
            foreach (var file in new DirectoryInfo(testDrive+":").EnumerateFiles("*.*",SearchOption.AllDirectories))
            {
                file.Delete();
            }
        }

        #endregion Methods
    }
}