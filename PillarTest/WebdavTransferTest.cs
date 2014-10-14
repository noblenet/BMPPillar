using System;
using System.IO;
using WebdavTransfer;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace PillarTest
{
    [TestClass]
    public class WebdavTransferTest
    {

        private static readonly WebdavTransferSettings webdavSettings = new WebdavTransferSettings("217.198.211.150", "dav", "147cafe2c7d0b07d3a61653977a87ff7fe91f5c6", "Root", "https", 443);
        private ExecutorWithWebdav executor = new ExecutorWithWebdav(webdavSettings);
            
        [TestMethod]
        public void TestMethodGetFile()
        {
            FileInfo location2SaveFile = new FileInfo(@"D:\collectionFiles\UnitTestLib\forest.jpg");
            string currentUri = @"https://217.198.211.150:443/dav/testDir/forest.jpg";
            bool retval = executor.GetFile(currentUri, location2SaveFile);
            
            Assert.IsTrue(retval);
        }
    }
}
