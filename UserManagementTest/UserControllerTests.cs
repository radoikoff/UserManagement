namespace UserManagementTest
{
    using Moq;
    using NUnit.Framework;
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.Threading.Tasks;
    using UserManagement.Core;
    using UserManagement.Data;


    [TestFixture]
    public class UserControllerTests
    {
        private Mock<AppData> data;
        private Logger logger;
        
        [SetUp]
        public void TestInit()
        {
            List<string> specialGroups = new List<string> { };
            List<string> reportingGroups = new List<string> { };
            this.data = new Mock<AppData>();
            data.SetupGet(x => x.SpecialSubGroups).Returns(specialGroups);
            data.SetupGet(x => x.ReportingGroups).Returns(reportingGroups);

            

            this.logger = new Logger();
        }


        [Test]
        public void TEST()
        {
            var controller = new Controller(this.data.Object, this.logger);

        }

    }
}
