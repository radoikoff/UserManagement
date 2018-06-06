namespace UserManagementTest
{
    using Moq;
    using NUnit.Framework;
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using UserManagement.Core;
    using UserManagement.Data;
    using UserManagement.Models.Users;
    using System.Reflection;
    using UserManagement.IO;
    using UserManagement.Models;

    [TestFixture]
    public class UserControllerTests
    {
        private Mock<AppData> data;
        private ILogger logger;

        [SetUp]
        public void TestInit()
        {
            this.logger = new TestLogger();

            IEnumerable<string> specialGroups = new List<string> { "eP_SPECIAL", "M_SPECIAL" };
            IEnumerable<string> reportingGroups = new List<string> { "Eng: Test", "Ops: Test2" };
            IEnumerable<string> subGroups = new List<string> { "eP_AAA", "eP_BBB", "eP_CCC", "M_Xxx", "M_Yyy", "M_Zzz", "eP_SPECIAL", "M_SPECIAL" };

            this.data = new Mock<AppData>(this.logger);
            data.SetupGet(x => x.SpecialSubGroups).Returns(specialGroups);
            data.SetupGet(x => x.ReportingGroups).Returns(reportingGroups);
            data.SetupGet(x => x.SubGroups).Returns(subGroups);
        }

        private string[] TestMessages()
        {
            FieldInfo field = this.logger.GetType().GetField("resultMessages", BindingFlags.NonPublic | BindingFlags.Instance);
            var result = (List<string>)field.GetValue(this.logger);
            return result.ToArray();
        }

        [Test]
        [TestCase(new object[] { new string[] { "Eng: Test" }, "AAA", new string[] { "eP_AAA" }, true, new string[] { "INFO: ID1 groups updated. Added:eP_AAA Deleted:" } }, TestName = "Case 1 - No groups")]

        [TestCase(new object[] { new string[] { "Eng: Test", "M_Xxx" }, "AAA", new string[] { "eP_AAA" }, true, new string[] { "INFO: ID1 groups updated. Added:eP_AAA Deleted:M_Xxx" } }, TestName = "Case 2 - One M group")]
        [TestCase(new object[] { new string[] { "Eng: Test", "M_Xxx", "M_Yyy" }, "AAA", new string[] { "eP_AAA" }, true, new string[] { "INFO: ID1 groups updated. Added:eP_AAA Deleted:M_Xxx, M_Yyy" } }, TestName = "Case 3 - More than one M group")]

        [TestCase(new object[] { new string[] { "Eng: Test", "M_SPECIAL" }, "AAA", new string[] { "eP_AAA", "M_SPECIAL" }, true, new string[] { "INFO: ID1 groups updated. Added:eP_AAA Deleted:" } }, TestName = "Case 4 - one M_SPECIAL group")]
        [TestCase(new object[] { new string[] { "Eng: Test", "M_SPECIAL", "M_Xxx" }, "AAA", new string[] { "eP_AAA", "M_SPECIAL" }, true, new string[] { "INFO: ID1 groups updated. Added:eP_AAA Deleted:M_Xxx" } }, TestName = "Case 5 - one M_SPECIAL group + M_")]

        [TestCase(new object[] { new string[] { "Eng: Test", "eP_AAA" }, "AAA", new string[] { "eP_AAA" }, false, new string[] { } }, TestName = "Case 6 - one eP_ == true eP_")]
        [TestCase(new object[] { new string[] { "Eng: Test", "eP_BBB" }, "AAA", new string[] { "eP_AAA" }, true, new string[] { "INFO: ID1 groups updated. Added:eP_AAA Deleted:eP_BBB" } }, TestName = "Case 6a - one eP_ != true eP_")]
        [TestCase(new object[] { new string[] { "Eng: Test", "eP_AAA", "eP_BBB" }, "AAA", new string[] { "eP_AAA" }, true, new string[] { "INFO: ID1 groups updated. Added: Deleted:eP_BBB" } }, TestName = "Case 7 - eP_ one of them == true eP_")]
        [TestCase(new object[] { new string[] { "Eng: Test", "eP_BBB", "eP_CCC" }, "AAA", new string[] { "eP_AAA" }, true, new string[] { "INFO: ID1 groups updated. Added:eP_AAA Deleted:eP_BBB, eP_CCC" } }, TestName = "Case 7a - eP_ none of them == true eP_")]

        [TestCase(new object[] { new string[] { "Eng: Test", "eP_SPECIAL" }, "SPECIAL", new string[] { "eP_SPECIAL" }, false, new string[] { "ACTION: ID1 is memeber of eP_SPECIAL and must be added to a single manual group. Current manual groups:None" } }, TestName = "Case 8a - one eP_SPECIAL group")]
        [TestCase(new object[] { new string[] { "Eng: Test", "eP_AAA" }, "SPECIAL", new string[] { "eP_SPECIAL" }, true, new string[] { "ACTION: ID1 is memeber of eP_SPECIAL and must be added to a single manual group. Current manual groups:None", "INFO: ID1 groups updated. Added:eP_SPECIAL Deleted:eP_AAA" } }, TestName = "Case 8b - one eP_SPECIAL group")]
        [TestCase(new object[] { new string[] { "Eng: Test", "eP_SPECIAL", "eP_AAA" }, "SPECIAL", new string[] { "eP_SPECIAL" }, true, new string[] { "ACTION: ID1 is memeber of eP_SPECIAL and must be added to a single manual group. Current manual groups:None", "INFO: ID1 groups updated. Added: Deleted:eP_AAA" } }, TestName = "Case 9 - more than one eP_ with special")]

        [TestCase(new object[] { new string[] { "Eng: Test", "M_Xxx", "M_Yyy", "eP_AAA", "eP_BBB", "eP_CCC" }, "BBB", new string[] { "eP_BBB" }, true, new string[] { "INFO: ID1 groups updated. Added: Deleted:M_Xxx, M_Yyy, eP_AAA, eP_CCC" } }, TestName = "Case 10-13 - M_ and eP_ together")]

        [TestCase(new object[] { new string[] { "Eng: Test", "M_Xxx", "eP_SPECIAL" }, "SPECIAL", new string[] { "M_Xxx", "eP_SPECIAL" }, false, new string[] { } }, TestName = "Case 14 - one M_ + one eP_ (with SP)")]
        [TestCase(new object[] { new string[] { "Eng: Test", "M_Xxx", "eP_SPECIAL" }, "AAA", new string[] { "eP_AAA" }, true, new string[] { "INFO: ID1 groups updated. Added:eP_AAA Deleted:M_Xxx, eP_SPECIAL" } }, TestName = "Case 14a - one M_ + one eP_ (with SP)")]
        [TestCase(new object[] { new string[] { "Eng: Test", "M_Xxx", "M_Yyy", "eP_SPECIAL" }, "SPECIAL", new string[] { "M_Xxx", "M_Yyy", "eP_SPECIAL" }, false, new string[] { "ACTION: ID1 is memeber of eP_SPECIAL and must be added to a single manual group. Current manual groups:M_Xxx, M_Yyy" } }, TestName = "Case 15 - many M_ + one eP_ (with SP)")]
        [TestCase(new object[] { new string[] { "Eng: Test", "M_Xxx", "eP_SPECIAL", "eP_AAA" }, "SPECIAL", new string[] { "M_Xxx", "eP_SPECIAL" }, true, new string[] { "INFO: ID1 groups updated. Added: Deleted:eP_AAA" } }, TestName = "Case 16 - one M_ + many eP_ (with SP)")]
        [TestCase(new object[] { new string[] { "Eng: Test", "M_Xxx", "M_Yyy", "eP_AAA", "eP_BBB", "eP_SPECIAL" }, "SPECIAL", new string[] { "M_Xxx", "M_Yyy", "eP_SPECIAL" }, true, new string[] { "ACTION: ID1 is memeber of eP_SPECIAL and must be added to a single manual group. Current manual groups:M_Xxx, M_Yyy", "INFO: ID1 groups updated. Added: Deleted:eP_AAA, eP_BBB" } }, TestName = "Case 17 - many M_ + many eP_ (with SP)")]

        [TestCase(new object[] { new string[] { "Eng: Test", "M_SPECIAL", "eP_AAA" }, "AAA", new string[] { "M_SPECIAL", "eP_AAA" }, false, new string[] { } }, TestName = "Case 18 - one M_SP + one P_")]
        [TestCase(new object[] { new string[] { "Eng: Test", "M_SPECIAL", "eP_AAA" }, "BBB", new string[] { "M_SPECIAL", "eP_BBB" }, true, new string[] { "INFO: ID1 groups updated. Added:eP_BBB Deleted:eP_AAA" } }, TestName = "Case 18a - one M_SP + one P_")]
        [TestCase(new object[] { new string[] { "Eng: Test", "M_SPECIAL", "M_Xxx", "eP_AAA" }, "AAA", new string[] { "M_SPECIAL", "eP_AAA" }, true, new string[] { "INFO: ID1 groups updated. Added: Deleted:M_Xxx" } }, TestName = "Case 19 - many M_SP + one P_")]
        [TestCase(new object[] { new string[] { "Eng: Test", "M_SPECIAL", "eP_AAA", "eP_BBB" }, "AAA", new string[] { "M_SPECIAL", "eP_AAA" }, true, new string[] { "INFO: ID1 groups updated. Added: Deleted:eP_BBB" } }, TestName = "Case 20 - one M_SP + many P_")]
        [TestCase(new object[] { new string[] { "Eng: Test", "M_SPECIAL", "M_Xxx", "eP_AAA", "eP_BBB" }, "AAA", new string[] { "M_SPECIAL", "eP_AAA" }, true, new string[] { "INFO: ID1 groups updated. Added: Deleted:M_Xxx, eP_BBB" } }, TestName = "Case 21 - many M_SP + many P_")]

        [TestCase(new object[] { new string[] { "Eng: Test", "M_Xxx" }, "SPECIAL", new string[] { "eP_SPECIAL", "M_Xxx" }, true, new string[] { "INFO: ID1 groups updated. Added:eP_SPECIAL Deleted:" } }, TestName = "Case 22 - M_ group assigned with true eP_SPECIAL ")]

        public void UpdateUserSkillGroupsTest_UserHasEpSkill(string[] userGroups, string userTrueGroup, string[] resultGroups, bool IsUserUpdated, string[] expectedMsgs)
        {
            var controller = new Controller(this.data.Object, this.logger);
            this.logger = new TestLogger(); //to reset the logger message;

            VistwayUser user = new VistwayUser("ID1", "ID1", "First", "Last", "End User", "BGR", userGroups.ToList(), "Test@Test", false);
            EnterProjUser eProjUser = new EnterProjUser("ID1", userTrueGroup, "NA", "NA");

            MethodInfo method = controller.GetType().GetMethod("UpdateUserSkillGroups", BindingFlags.NonPublic | BindingFlags.Instance);
            var userUpdated = (bool)method.Invoke(controller, new object[] { user, eProjUser, this.logger });

            Assert.That(user.Groups, Is.EquivalentTo(resultGroups));
            Assert.That(userUpdated, Is.EqualTo(IsUserUpdated));
            Assert.That(TestMessages(), Is.EquivalentTo(expectedMsgs));
        }


        [Test]
        [TestCase(new object[] { new string[] { "Eng: Test" }, new string[] { }, false, new string[] { "NOTASSIGNED: ID1 is not assigned to any group and does not have eP skill" } }, TestName = "Case 22 - No groups")]

        [TestCase(new object[] { new string[] { "Eng: Test", "M_Xxx" }, new string[] { "M_Xxx" }, false, new string[] { } }, TestName = "Case 23 - One M_ group")]
        [TestCase(new object[] { new string[] { "Eng: Test", "M_Xxx", "M_Yyy" }, new string[] { "M_Xxx", "M_Yyy" }, false, new string[] { "WARNING: ID1 is member of more than one manual groups (M_Xxx, M_Yyy)" } }, TestName = "Case 24 - More than one M_ group")]

        [TestCase(new object[] { new string[] { "Eng: Test", "eP_AAA" }, new string[] { "eP_AAA" }, false, new string[] { } }, TestName = "Case 25 - One eP_ group")]
        [TestCase(new object[] { new string[] { "Eng: Test", "eP_AAA", "eP_BBB" }, new string[] { "eP_AAA", "eP_BBB" }, false, new string[] { "WARNING: ID1 is member of more than one auto groups (eP_AAA, eP_BBB)" } }, TestName = "Case 26 - More than one eP_ group")]

        [TestCase(new object[] { new string[] { "Eng: Test", "eP_SPECIAL" }, new string[] { "eP_SPECIAL" }, false, new string[] { "ACTION: ID1 is memeber of eP_SPECIAL and must be added to a single manual group. Current manual groups:None" } }, TestName = "Case 27 - One special eP")]
        [TestCase(new object[] { new string[] { "Eng: Test", "eP_AAA", "eP_SPECIAL" }, new string[] { "eP_SPECIAL" }, true, new string[] { "ACTION: ID1 is memeber of eP_SPECIAL and must be added to a single manual group. Current manual groups:None", "INFO: ID1 groups updated. Added: Deleted:eP_AAA" } }, TestName = "Case 28 - One special eP + eP_")]

        [TestCase(new object[] { new string[] { "Eng: Test", "eP_AAA", "M_Xxx" }, new string[] { "eP_AAA" }, true, new string[] { "INFO: ID1 groups updated. Added: Deleted:M_Xxx" } }, TestName = "Case 29 - One eP_ + M_")]
        [TestCase(new object[] { new string[] { "Eng: Test", "eP_AAA", "M_Xxx", "M_Yyy" }, new string[] { "eP_AAA" }, true, new string[] { "INFO: ID1 groups updated. Added: Deleted:M_Xxx, M_Yyy" } }, TestName = "Case 30 - One eP_ + many M_")]
        [TestCase(new object[] { new string[] { "Eng: Test", "eP_AAA", "eP_BBB", "M_Xxx" }, new string[] { "eP_AAA", "eP_BBB", "M_Xxx" }, false, new string[] { "WARNING: ID1 is member of more than one auto groups (eP_AAA, eP_BBB)" } }, TestName = "Case 31 - Many eP_ + One M_")]
        [TestCase(new object[] { new string[] { "Eng: Test", "eP_AAA", "eP_BBB", "M_Xxx", "M_Yyy" }, new string[] { "eP_AAA", "eP_BBB", "M_Xxx", "M_Yyy" }, false, new string[] { "WARNING: ID1 is member of more than one auto groups (eP_AAA, eP_BBB)" } }, TestName = "Case 32 - Many eP_ + Many M_")]

        [TestCase(new object[] { new string[] { "Eng: Test", "eP_SPECIAL", "M_Xxx" }, new string[] { "eP_SPECIAL", "M_Xxx" }, false, new string[] { } }, TestName = "Case 33 - One eP_SP + one M_")]
        [TestCase(new object[] { new string[] { "Eng: Test", "eP_SPECIAL", "M_Xxx", "M_Yyy" }, new string[] { "eP_SPECIAL", "M_Xxx", "M_Yyy" }, false, new string[] { "ACTION: ID1 is memeber of eP_SPECIAL and must be added to a single manual group. Current manual groups:M_Xxx, M_Yyy" } }, TestName = "Case 34 - One eP_SP + many M_")]
        [TestCase(new object[] { new string[] { "Eng: Test", "eP_SPECIAL", "eP_BBB", "M_Xxx" }, new string[] { "eP_SPECIAL", "M_Xxx" }, true, new string[] { "INFO: ID1 groups updated. Added: Deleted:eP_BBB" } }, TestName = "Case 35 - Many eP_ (with SP) + One M_")]
        [TestCase(new object[] { new string[] { "Eng: Test", "eP_SPECIAL", "eP_BBB", "M_Xxx", "M_Yyy" }, new string[] { "eP_SPECIAL", "M_Xxx", "M_Yyy" }, true, new string[] { "ACTION: ID1 is memeber of eP_SPECIAL and must be added to a single manual group. Current manual groups:M_Xxx, M_Yyy", "INFO: ID1 groups updated. Added: Deleted:eP_BBB" } }, TestName = "Case 36 - Many eP_(with SP) + Many M_")]

        [TestCase(new object[] { new string[] { "Eng: Test", "eP_AAA", "M_SPECIAL" }, new string[] { "eP_AAA", "M_SPECIAL" }, false, new string[] { } }, TestName = "Case 37 - One eP_ + one M_SP")]
        [TestCase(new object[] { new string[] { "Eng: Test", "eP_AAA", "M_SPECIAL", "M_Xxx" }, new string[] { "eP_AAA", "M_SPECIAL" }, true, new string[] { "INFO: ID1 groups updated. Added: Deleted:M_Xxx" } }, TestName = "Case 38 - One eP_ + many M_ (With SP)")]
        [TestCase(new object[] { new string[] { "Eng: Test", "eP_AAA", "eP_BBB", "M_SPECIAL" }, new string[] { "eP_AAA", "eP_BBB", "M_SPECIAL" }, false, new string[] { "WARNING: ID1 is member of more than one auto groups (eP_AAA, eP_BBB)" } }, TestName = "Case 39 - Many eP_ + One M_SP")]
        [TestCase(new object[] { new string[] { "Eng: Test", "eP_AAA", "eP_BBB", "M_SPECIAL", "M_Xxx" }, new string[] { "eP_AAA", "eP_BBB", "M_SPECIAL", "M_Xxx" }, false, new string[] { "WARNING: ID1 is member of more than one auto groups (eP_AAA, eP_BBB)" } }, TestName = "Case 40 - Many eP_ + many M_ (With SP)")]

        public void UpdateUserSkillGroupsTest_UserHasNoSkill(string[] userGroups, string[] resultGroups, bool IsUserUpdated, string[] expectedMsgs)
        {
            var controller = new Controller(this.data.Object, this.logger);
            this.logger = new TestLogger(); //to reset the logger message;

            VistwayUser user = new VistwayUser("ID1", "ID1", "First", "Last", "End User", "BGR", userGroups.ToList(), "Test@Test", false);

            MethodInfo method = controller.GetType().GetMethod("UpdateUserSkillGroups", BindingFlags.NonPublic | BindingFlags.Instance);
            var userUpdated = (bool)method.Invoke(controller, new object[] { user, null, this.logger });

            Assert.That(user.Groups, Is.EquivalentTo(resultGroups));
            Assert.That(userUpdated, Is.EqualTo(IsUserUpdated));
            Assert.That(TestMessages(), Is.EquivalentTo(expectedMsgs));
        }


        [Test]
        [TestCase(new object[] { new string[] { "Eng: Test", "eP_AAA" }, "BGR", new string[] { "Eng: Test", "eP_AAA", "Country:Bulgaria" }, true, new string[] { "INFO: ID1 is added to country group Country:Bulgaria" } }, TestName = "Country Case 1 - No Country")]
        [TestCase(new object[] { new string[] { "Eng: Test", "eP_AAA", "Country:Bulgaria" }, "BGR", new string[] { "Eng: Test", "eP_AAA", "Country:Bulgaria" }, false, new string[] { } }, TestName = "Country Case 2 - Same Country")]
        [TestCase(new object[] { new string[] { "Eng: Test", "eP_AAA", "Country:France" }, "BGR", new string[] { "Eng: Test", "eP_AAA", "Country:Bulgaria" }, true, new string[] { "INFO: ID1 country group is changed from Country:France to Country:Bulgaria" } }, TestName = "Country Case 3 - Reasign Country")]
        [TestCase(new object[] { new string[] { "Eng: Test", "eP_AAA", "Country:France", "Country:Bulgaria" }, "USA", new string[] { "Eng: Test", "eP_AAA", "Country:USA" }, true, new string[] { "INFO: ID1 country group is changed from Country:France, Country:Bulgaria to Country:USA" } }, TestName = "Country Case 4 - Multiple countries")]
        [TestCase(new object[] { new string[] { "Eng: Test", "eP_AAA" }, "TTT", new string[] { "Eng: Test", "eP_AAA" }, false, new string[] { "WARNING: ID1 - country code TTT has no country name" } }, TestName = "Country Case 5 - Not Exisiting Country Code")]

        public void UpdateUserCountryGroupTest(string[] userGroups, string countryCode, string[] resultGroups, bool IsUserUpdated, string[] expectedMsgs)
        {
            IEnumerable<Country> countries = new List<Country>()
            {
                new Country("Bulgaria", "BGR"),
                new Country("France", "FRA"),
                new Country("United Kingdom", "GBR"),
                new Country("USA", "USA"),
            };

            var controller = new Controller(this.data.Object, this.logger);
            this.logger = new TestLogger(); //to reset the logger message;

            VistwayUser user = new VistwayUser("ID1", "ID1", "First", "Last", "End User", countryCode, userGroups.ToList(), "Test@Test", false);

            MethodInfo method = controller.GetType().GetMethod("UpdateUserCountryGroup", BindingFlags.NonPublic | BindingFlags.Instance);
            var userUpdated = (bool)method.Invoke(controller, new object[] { user, countries, this.logger });

            Assert.That(user.Groups, Is.EquivalentTo(resultGroups));
            Assert.That(userUpdated, Is.EqualTo(IsUserUpdated));
            Assert.That(TestMessages(), Is.EquivalentTo(expectedMsgs));
        }

        [Test]
        [TestCase(new object[] { "eP_NON EXISTING", new string[] { "WARNING: New user auto group has been used. (eP_NON EXISTING)" } }, TestName = "GroupValidation_InvalidGroup")]
        [TestCase(new object[] { "eP_AAA", new string[] { } }, TestName = "GroupValidation_ValidGroup")]

        public void CheckIfUserGroupExistsTest(string userGroup, string[] expectedMsgs)
        {
            var controller = new Controller(this.data.Object, this.logger);

            MethodInfo method = controller.GetType().GetMethod("CheckIfUserGroupExists", BindingFlags.NonPublic | BindingFlags.Instance);
            method.Invoke(controller, new object[] { userGroup });

            Assert.That(this.TestMessages().Skip(1), Is.EquivalentTo(expectedMsgs));
        }

    }
}
