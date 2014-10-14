using bmpxsd;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using PillarAPI;

namespace PillarTest
{
    [TestClass]
    public class UtilitiesTest
    {
        private readonly TestPetaPoco _testPetaPoco = new TestPetaPoco();

        [TestMethod]
        public void TestInsertAudit()
        {
            // arrange
            string actionOnFile = FileAction.DELETE_FILE.ToString();
            const string actorOnFile = "actorOnFile";
            const string auditTrailInformation = "auditTrailInformation";
            const string fileName = "fileName";
            const string info = "info";
            const string reportingComponent = "reportingComponent";
            const bool expected = true;

            // act
            //bool retval = CollectedUtilities.InsertAudit(actionOnFile, actorOnFile, auditTrailInformation, fileName, info, reportingComponent);

            // assert
            //Assert.AreEqual(expected, retval);
        }
    }
}