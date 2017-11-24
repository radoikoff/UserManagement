using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using Excel = Microsoft.Office.Interop.Excel;

namespace UserManagement
{
    class Program
    {
        static void Main(string[] args)
        {
            List<VistwayUser> vistwayUsers = ReadVistwayUsersData(@"C:\Users\vradoyko\Desktop\users.xlsx");
            List<EnterProjUser> enterProjUsers = ReadEnterProjUsersData(@"C:\Users\vradoyko\Desktop\FTE - Oct 2017.xlsx");

            foreach (var user in vistwayUsers)
            {
                var enterProjUser = enterProjUsers.FirstOrDefault(u => u.Account.Equals(user.Account));

                UpdateUserCountryGroup(user);
                UpdateUserSkillGroups(user, enterProjUser);
            }

            //CreateOutputFile(vistwayUsers);
        }

        private static void UpdateUserSkillGroups(VistwayUser user, EnterProjUser enterProjUser)
        {
            var userAssignedLevelOneGroups = user.Groups.Where(c => c.StartsWith(GlobalValues.LEVEL_ONE_GROUP_PREFIX)).ToList();
            var userAssignedManualGroups = user.Groups.Where(c => c.StartsWith(GlobalValues.LEVEL_TWO_MANUAL_GROUP_PREFIX)).ToList();

            if (enterProjUser != null)
            {
                string userEnterProjSkill = enterProjUser.Skill;

                switch (userAssignedLevelOneGroups.Count())
                {
                    case 0:
                        user.Groups.Add(userEnterProjSkill);
                        break;
                    case 1:
                        if (!userAssignedLevelOneGroups.FirstOrDefault().Equals(userEnterProjSkill))
                        {
                            user.Groups.Add(userEnterProjSkill);
                        }
                        break;
                    default:
                        user.Groups.RemoveWhere(g => g.StartsWith(GlobalValues.LEVEL_ONE_GROUP_PREFIX));
                        user.Groups.Add(userEnterProjSkill);
                        break;
                }

                if (userAssignedManualGroups.Count() != 0)
                {
                    user.Groups.RemoveWhere(g => g.StartsWith(GlobalValues.LEVEL_TWO_MANUAL_GROUP_PREFIX));
                }
            }
            else //user has no enterProj skill
            {
                if (userAssignedManualGroups.Count() == 0 && userAssignedLevelOneGroups.Count() == 0)
                {
                    //warning "User not assigend to any group and does not have eP role" 
                }

                if (userAssignedManualGroups.Count() > 1)
                {
                    //warning "User is assigned to more than one Manual groups.
                }

                if (userAssignedLevelOneGroups.Count() > 1)
                {
                    //warning "User is assigned to more than one eP groups allthough he//she has no eP role.
                }

                if (userAssignedManualGroups.Count() == 1 && userAssignedLevelOneGroups.Count() == 1 && userAssignedManualGroups.First() != userAssignedLevelOneGroups.First())
                {
                    //warning "Missalignment between Manual and EP group exisit for user who has no eP skill"
                }
            }
        }

        private static void UpdateUserCountryGroup(VistwayUser user)
        {
            string userTrueCountryGroup = GlobalValues.COUNTRY_GROUP_PREFIX + Groups.GetCountryNameByCountryCode(user.CountryCode);
            List<string> userCurrentCountries = user.Groups.Where(c => c.StartsWith(GlobalValues.COUNTRY_GROUP_PREFIX)).ToList();

            if (userCurrentCountries.Count() > 1)
            {
                user.Groups.RemoveWhere(g => g.StartsWith(GlobalValues.COUNTRY_GROUP_PREFIX));
            }

            var userCurrentCountry = userCurrentCountries.FirstOrDefault();

            if (userCurrentCountry == null)
            {
                user.Groups.Add(userTrueCountryGroup);
                return;
            }

            if (userCurrentCountry != userTrueCountryGroup)
            {
                user.Groups.RemoveWhere(g => g.Equals(userCurrentCountry));
                user.Groups.Add(userTrueCountryGroup);
            }
        }

        public static List<VistwayUser> ReadVistwayUsersData(string fileName)
        {
            var users = new List<VistwayUser>();

            Excel.Application xlApp = new Excel.Application();
            Excel.Workbook xlWorkbook = xlApp.Workbooks.Open(fileName);
            Excel.Worksheet xlWorksheet = xlWorkbook.Sheets[1];
            Excel.Range xlCells = xlWorksheet.Cells;

            int rowCount = xlWorksheet.UsedRange.Rows.Count;

            for (int i = GlobalValues.FIRST_ROW_VISTWAY_USERS; i <= rowCount; i++)
            {
                string account = xlWorksheet.Cells[i, UsersColumn.Account].Value.ToString().Trim().ToLower();
                if (!users.Any(u => u.Account.Equals(account)))
                {
                    string firstName = xlCells[i, UsersColumn.FirstName].Text;
                    string lastName = xlCells[i, UsersColumn.LastName].Text;
                    string email = xlCells[i, UsersColumn.Email].Text;
                    string countryCode = xlCells[i, UsersColumn.CountryCode].Text;
                    // string country = GetCountryNameByCountryCode(countryCode);
                    string groups = xlCells[i, UsersColumn.Groups].Text;

                    var groupsList = groups.Split(new char[] { ';' }, StringSplitOptions.RemoveEmptyEntries).Select(s => s.Trim()).ToList();
                    var userGroups = new HashSet<string>(groupsList);

                    var user = new VistwayUser(account, firstName, lastName, email, countryCode, userGroups);
                    users.Add(user);
                }
                else
                {
                    //warning for duplicate users
                }
            }

            //clean up
            GC.Collect();
            GC.WaitForPendingFinalizers();
            Marshal.ReleaseComObject(xlWorksheet);
            xlWorkbook.Close();
            Marshal.ReleaseComObject(xlWorkbook);
            xlApp.Quit();
            Marshal.ReleaseComObject(xlApp);

            return users;
        }

        public static List<EnterProjUser> ReadEnterProjUsersData(string fileName)
        {
            var enerProjUsers = new List<EnterProjUser>();

            Excel.Application xlApp = new Excel.Application();
            Excel.Workbook xlWorkbook = xlApp.Workbooks.Open(fileName);
            Excel.Worksheet xlWorksheet = xlWorkbook.Sheets[1];

            int rowCount = xlWorksheet.UsedRange.Rows.Count;

            for (int i = GlobalValues.FIRST_ROW_ENTERPROJ_USERS; i <= rowCount; i++)
            {
                string account = xlWorksheet.Cells[i, EnterProjColumn.Account].Value.ToString().Trim().ToLower();
                if (!enerProjUsers.Any(u => u.Account.Equals(account)))
                {
                    string skill = xlWorksheet.Cells[i, EnterProjColumn.Skill].Value.ToString();
                    string groupName = xlWorksheet.Cells[i, EnterProjColumn.GroupName].Value.ToString();
                    string groupCode = xlWorksheet.Cells[i, EnterProjColumn.GroupCode].Value.ToString();

                    var user = new EnterProjUser(account, skill, groupName, groupCode);
                    enerProjUsers.Add(user);
                }
            }

            //clean up
            GC.Collect();
            GC.WaitForPendingFinalizers();
            Marshal.ReleaseComObject(xlWorksheet);
            xlWorkbook.Close();
            Marshal.ReleaseComObject(xlWorkbook);
            xlApp.Quit();
            Marshal.ReleaseComObject(xlApp);

            return enerProjUsers;
        }



    }
}

