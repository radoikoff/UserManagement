using System;
using System.Collections.Generic;
using System.IO;
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
            string vistwayUsersFileName = @"C:\Users\vradoyko\Desktop\usersTest.xlsx";
            string enterProjUsersFileName = @"C:\Users\vradoyko\Desktop\FTE - Oct 2017 - Test.xlsx";

            //if (inputFilesValid)
            //{

            //}

            List<VistwayUser> vistwayUsers = ReadVistwayUsersData(vistwayUsersFileName);
            List<EnterProjUser> enterProjUsers = ReadEnterProjUsersData(enterProjUsersFileName);
            string outputFileName = @"C:\Users\vradoyko\Desktop\output.txt";

            foreach (var user in vistwayUsers)
            {
                var enterProjUser = enterProjUsers.FirstOrDefault(u => u.Account.Equals(user.Account));

                UpdateUserCountryGroup(user);
                UpdateUserSkillGroups(user, enterProjUser);
            }

            CreateOutputFile(vistwayUsers, outputFileName);
        }

        private static void CreateOutputFile(List<VistwayUser> users, string outputFileName)
        {
            var result = new StringBuilder();

            foreach (var user in users)
            {
                string groups = string.Join(";", user.Groups);
                string singeRowData = $"{user.LastName},{user.FirstName},{groups},,{user.Account},,EndUser,,True,";
                result.AppendLine(singeRowData);
            }

            if (File.Exists(outputFileName))
            {
                File.Delete(outputFileName);
            }
            File.AppendAllText(outputFileName, result.ToString());
        }

        private static void UpdateUserSkillGroups(VistwayUser user, EnterProjUser enterProjUser)
        {
            var userAssignedLevelOneGroups = user.Groups.Where(c => c.StartsWith(GlobalValues.LEVEL_ONE_GROUP_PREFIX)).ToList();
            var userAssignedManualGroups = user.Groups.Where(c => c.StartsWith(GlobalValues.LEVEL_TWO_MANUAL_GROUP_PREFIX)).ToList();

            if (enterProjUser != null)
            {
                string userTrueLevelOneGroup = GlobalValues.LEVEL_ONE_GROUP_PREFIX + enterProjUser.Skill;

                switch (userAssignedLevelOneGroups.Count())
                {
                    case 0:
                        user.Groups.Add(userTrueLevelOneGroup);
                        OutputWriter.DisplayMessageAndAddToLogFile(Messages.UserAddedToLevelOneGroup);
                        break;

                    case 1:
                        if (!userAssignedLevelOneGroups.FirstOrDefault().Equals(userTrueLevelOneGroup))
                        {
                            user.Groups.RemoveWhere(g => g.StartsWith(GlobalValues.LEVEL_ONE_GROUP_PREFIX));
                            user.Groups.Add(userTrueLevelOneGroup);
                            OutputWriter.DisplayMessageAndAddToLogFile(Messages.UserLevelOneGroupUpdated);
                        }
                        break;
                    default:
                        user.Groups.RemoveWhere(g => g.StartsWith(GlobalValues.LEVEL_ONE_GROUP_PREFIX));
                        user.Groups.Add(userTrueLevelOneGroup);
                        OutputWriter.DisplayMessageAndAddToLogFile(Messages.UserAssignedToMoreThanOneLevelOneGroups);
                        OutputWriter.DisplayMessageAndAddToLogFile(Messages.UserLevelOneGroupUpdated);
                        break;
                }

                if (userAssignedManualGroups.Count() != 0)
                {
                    user.Groups.RemoveWhere(g => g.StartsWith(GlobalValues.LEVEL_TWO_MANUAL_GROUP_PREFIX));
                    OutputWriter.DisplayMessageAndAddToLogFile(Messages.UserRemovedFromManualGroup);
                }
            }
            else //user has no enterProj skill
            {
                if (userAssignedManualGroups.Count() == 0 && userAssignedLevelOneGroups.Count() == 0)
                {
                    OutputWriter.DisplayMessageAndAddToLogFile(Messages.UserHasNoGroupAssignment);
                }

                if (userAssignedManualGroups.Count() > 1)
                {
                    OutputWriter.DisplayMessageAndAddToLogFile(Messages.UserAssignedToMoreThanOneManualGroup);
                }

                if (userAssignedLevelOneGroups.Count() > 1)
                {
                    OutputWriter.DisplayMessageAndAddToLogFile(Messages.UserAssignedToMoreThanOneLevelOneGroups);
                }

                if (userAssignedManualGroups.Count() == 1 && userAssignedLevelOneGroups.Count() == 1)
                {
                    user.Groups.RemoveWhere(g => g.StartsWith(GlobalValues.LEVEL_TWO_MANUAL_GROUP_PREFIX));
                    OutputWriter.DisplayMessageAndAddToLogFile(Messages.UserRemovedFromManualGroup);
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

        private static List<VistwayUser> ReadVistwayUsersData(string fileName)
        {
            var users = new List<VistwayUser>();

            Excel.Application xlApp = null; // = new Excel.Application();
            Excel.Workbook xlWorkbook = null; //= xlApp.Workbooks.Open(fileName);
            Excel.Worksheet xlWorksheet = null; //= xlWorkbook.Sheets[1];

            try
            {
                xlApp = new Excel.Application();
                xlWorkbook = xlApp.Workbooks.Open(fileName);
                xlWorksheet = xlWorkbook.Sheets[1];

                int rowCount = xlWorksheet.UsedRange.Rows.Count;

                for (int i = GlobalValues.FIRST_ROW_VISTWAY_USERS; i <= rowCount; i++)
                {
                    string account = xlWorksheet.Cells[i, UsersColumn.Account].Text.Trim().ToLower();
                    if (!string.IsNullOrEmpty(account))
                    {
                        if (!users.Any(u => u.Account.Equals(account)))
                        {
                            string firstName = xlWorksheet.Cells[i, UsersColumn.FirstName].Text;
                            string lastName = xlWorksheet.Cells[i, UsersColumn.LastName].Text;
                            string email = xlWorksheet.Cells[i, UsersColumn.Email].Text;
                            string countryCode = xlWorksheet.Cells[i, UsersColumn.CountryCode].Text;
                            string groups = xlWorksheet.Cells[i, UsersColumn.Groups].Text;

                            var groupsList = groups.Split(new char[] { ';' }, StringSplitOptions.RemoveEmptyEntries).Select(s => s.Trim()).ToList();
                            var userGroups = new HashSet<string>(groupsList);

                            var user = new VistwayUser(account, firstName, lastName, email, countryCode, userGroups);
                            users.Add(user);
                        }
                        else
                        {
                            OutputWriter.DisplayMessageAndAddToLogFile(Messages.DuplicateUserFound);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                OutputWriter.DisplayMessageAndAddToLogFile(ex.Source + ex.Message);
                users = null;
            }
            finally
            {
                GC.Collect();
                GC.WaitForPendingFinalizers();
                if (xlWorksheet != null)
                {
                    Marshal.ReleaseComObject(xlWorksheet);
                }
                if (xlWorkbook != null)
                {
                    xlWorkbook.Close();
                    Marshal.ReleaseComObject(xlWorkbook);
                }
                if (xlApp != null)
                {
                    xlApp.Quit();
                    Marshal.ReleaseComObject(xlApp);
                }
            }

            return users;
        }

        private static List<EnterProjUser> ReadEnterProjUsersData(string fileName)
        {
            var enerProjUsers = new List<EnterProjUser>();

            Excel.Application xlApp = null;
            Excel.Workbook xlWorkbook = null;
            Excel.Worksheet xlWorksheet = null;

            try
            {
                xlApp = new Excel.Application();
                xlWorkbook = xlApp.Workbooks.Open(fileName);
                xlWorksheet = xlWorkbook.Sheets[1];

                int rowCount = xlWorksheet.UsedRange.Rows.Count;

                for (int i = GlobalValues.FIRST_ROW_ENTERPROJ_USERS; i <= rowCount; i++)
                {
                    string account = xlWorksheet.Cells[i, EnterProjColumn.Account].Text.Trim().ToLower();
                    if (!string.IsNullOrEmpty(account) && !enerProjUsers.Any(u => u.Account.Equals(account)))
                    {
                        string skill = xlWorksheet.Cells[i, EnterProjColumn.Skill].Value.ToString();
                        string groupName = xlWorksheet.Cells[i, EnterProjColumn.GroupName].Value.ToString();
                        string groupCode = xlWorksheet.Cells[i, EnterProjColumn.GroupCode].Value.ToString();

                        var user = new EnterProjUser(account, skill, groupName, groupCode);
                        enerProjUsers.Add(user);
                    }
                }
            }
            catch (Exception ex)
            {
                OutputWriter.DisplayMessageAndAddToLogFile(ex.Source + ex.Message);
                enerProjUsers = null;
            }
            finally
            {
                GC.Collect();
                GC.WaitForPendingFinalizers();
                if (xlWorksheet != null)
                {
                    Marshal.ReleaseComObject(xlWorksheet);
                }
                if (xlWorkbook != null)
                {
                    xlWorkbook.Close();
                    Marshal.ReleaseComObject(xlWorkbook);
                }
                if (xlApp != null)
                {
                    xlApp.Quit();
                    Marshal.ReleaseComObject(xlApp);
                }
            }

            return enerProjUsers;
        }
    }
}

