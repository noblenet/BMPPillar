namespace StorageSuite
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;

    using NUnit.Framework;

    /// <summary>
    /// A test class for ...
    /// </summary>
    [TestFixture]
    public class SampleTest
    {
        #region Methods

        /// <summary>
        /// This runs only once at the end of all tests and is used for all tests in the class.
        /// </summary>
        [TestFixtureTearDown]
        public void FinalTearDown()
        {
        }

        [Test]
        public void FirstTest()
        {
            // Step 1 - Arrange

            // Step 2 - Act

            // Step 3 - Assert
        }

        /// <summary>
        /// This runs only once at the beginning of all tests and is used for all tests in the 
        /// class.
        /// </summary>
        [TestFixtureSetUp]
        public void InitialSetup()
        {
        }

        /// <summary>
        /// This setup funcitons runs before each test method
        /// </summary>
        [SetUp]
        public void SetupForEachTest()
        {
        }

        /// <summary>
        /// This setup funcitons runs after each test method
        /// </summary>
        [TearDown]
        public void TearDownForEachTest()
        {
        }

        #endregion Methods
    }
}