using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using UserManagement.Models;
using UserManagement.Models.Users;
using System.Configuration;
using Excel = Microsoft.Office.Interop.Excel;
using UserManagement.StaticData;
using UserManagement.Models.Groups;

namespace UserManagement.Data
{
    class DataMapper
    {
        private static readonly Dictionary<string, List<string>> configData; //section name, section data lines

        static DataMapper()
        {
            configData = LoadConfig();
        }

        private static string[] ReadLines(string path)
        {
            EnsureFile(path);
            var lines = File.ReadAllLines(path);
            return lines;
        }

        private static void EnsureFile(string path)
        {
            if (!File.Exists(path))
            {
                throw new FileNotFoundException(ExceptionMessages.FileNotFound, path);
            }
        }

        private static Dictionary<string, List<string>> LoadConfig()
        {
            var configFilePath = Path.Combine(ConfigurationManager.AppSettings.Get("DataFolderName"), ConfigurationManager.AppSettings.Get("ConfigFileName"));
            string[] configDataLines = ReadLines(configFilePath);

            var data = new Dictionary<string, List<string>>();
            string sectionName = string.Empty;

            foreach (var configLine in configDataLines)
            {
                string line = configLine.Trim();

                if (line.StartsWith(";") || line.Length == 0) //handle commented and empty lines
                {
                    continue;
                }

                if (line.StartsWith("[") && line.EndsWith("]")) // section
                {
                    sectionName = line.Trim('[', ']').ToLower();

                    if (!data.ContainsKey(sectionName))
                    {
                        data.Add(sectionName, new List<string>());
                    }

                    continue;
                }

                if (data.ContainsKey(sectionName))
                {
                    data[sectionName].Add(line);
                }
            }

            return data;
        }

        public static List<Country> LoadCountries()
        {
            const string CONFIG_SECTION_NAME = "countries";

            var countries = new List<Country>();

            foreach (var line in configData[CONFIG_SECTION_NAME])
            {

                string[] args = line.Split('=');
                string code = args[0].Trim();
                string name = args[1].Trim();

                if (countries.Any(c => c.Name == name) || countries.Any(c => c.Code == code))
                {
                    throw new ArgumentException("Country already exisits");
                }
                countries.Add(new Country(name, code));
            }

            return countries;
        }

        public static List<ReportingGroup> LoadGroups()
        {
            const string GROUPS_SECTION_NAME = "groups";
            const string SPECIAL_SECTION_NAME = "specialgroups";

            var groups = new List<ReportingGroup>();

            foreach (var line in configData[GROUPS_SECTION_NAME])
            {
                var args = line.Split('=');
                var parentName = args[0].Trim();
                var childName = args[1].Trim();

                var currentGroup = groups.FirstOrDefault(g => g.Name == parentName);
                if (currentGroup == null)
                {
                    currentGroup = new ReportingGroup(parentName);
                    groups.Add(currentGroup);
                }

                bool isSubGroupSpecial = false;
                if (configData[SPECIAL_SECTION_NAME].Contains(childName))
                {
                    isSubGroupSpecial = true;
                }
                currentGroup.AddSubGroup(new SubGroup(childName, isSubGroupSpecial));
            }
            return groups;
        }

        public static List<VistwayUser> LoadVistwayUsersFromCsv()
        {
            const int ROWS_TO_SKIP_VISTWAY_USERS = 1;

            var users = new List<VistwayUser>();
            var sourceFilePath = Path.Combine(ConfigurationManager.AppSettings.Get("DataFolderName"), ConfigurationManager.AppSettings.Get("VistwayUsersFileName"));
            var dataLines = ReadLines(sourceFilePath);

            foreach (var line in dataLines.Skip(ROWS_TO_SKIP_VISTWAY_USERS))
            {
                var args = line.Split('\t');

                string id = args[(int)VistwayUserColumnsCsv.Id].ToUpper().Trim();

                if (users.Any(u => u.Id == id))
                {
                    continue;
                }

                string cdsid = args[(int)VistwayUserColumnsCsv.Id].Trim();
                string firstName = args[(int)VistwayUserColumnsCsv.FirstName].Trim();
                string lastName = args[(int)VistwayUserColumnsCsv.LastName].Trim();

                string inputAccountType = args[(int)VistwayUserColumnsCsv.AccountType].Trim();
                string accountType = ConvertAccountType(inputAccountType);

                string countryCode = args[(int)VistwayUserColumnsCsv.CountryCode].Trim();
                string emailAddress = args[(int)VistwayUserColumnsCsv.Email].Trim();
                bool ignoreLDAP = args[(int)VistwayUserColumnsCsv.IgnoreLDAP].Trim() == "True";

                string joinedGroups = args[(int)VistwayUserColumnsCsv.Groups];
                var groups = joinedGroups.Split(new char[] { ';' }, StringSplitOptions.RemoveEmptyEntries).Select(g => g.Trim()).ToList();

                if (ignoreLDAP == true)
                {
                    continue;
                }

                users.Add(new VistwayUser(id, cdsid, firstName, lastName, accountType, countryCode, groups, emailAddress, ignoreLDAP));
            }

            return users;
        }

        private static string ConvertAccountType(string input)
        {
            switch (input)
            {
                case "Administrator":
                    return "Admin";
                case "Process Author":
                    return "Author";
                case "End User":
                    return "End User";
                default:
                    return "End User";
            }
        }

        public static List<EnterProjUser> LoadEnterProjUsersFromCsv()
        {
            const int ROWS_TO_SKIP_EP_USERS = 7;

            var users = new List<EnterProjUser>();
            var sourceFilePath = Path.Combine(ConfigurationManager.AppSettings.Get("DataFolderName"), ConfigurationManager.AppSettings.Get("EnterProjUsersFileName"));
            var dataLines = ReadLines(sourceFilePath);

            foreach (var line in dataLines.Skip(ROWS_TO_SKIP_EP_USERS))
            {
                var args = line.Split('\t');

                string id = args[(int)EnterProjUserColumnsCsv.Id].ToUpper().Trim();
                if (users.Any(u => u.Id == id))
                {
                    continue;
                }

                string costCenterCode = args[(int)EnterProjUserColumnsCsv.CostCenterCode].Trim();
                string costCenterName = args[(int)EnterProjUserColumnsCsv.CostCenterName].Trim();
                string skill = args[(int)EnterProjUserColumnsCsv.Skill].Trim();

                users.Add(new EnterProjUser(id, skill, costCenterCode, costCenterName));
            }

            return users;
        }

        public static void SaveResultFile(List<VistwayUser> users)
        {
            if (users == null)
            {
                throw new ArgumentNullException(nameof(users), Messages.NoUsersToUpdate);
            }

            var outputFileName = Path.Combine(ConfigurationManager.AppSettings.Get("DataFolderName"), ConfigurationManager.AppSettings.Get("ResultFileName"));

            var sb = new StringBuilder();
            sb.AppendLine($"File Creation Date: {DateTime.Now}");

            foreach (var user in users)
            {
                string groups = string.Join(";", user.Groups);
                string singeRowData = $"{user.LastName},{user.FirstName},{groups},{user.EmailAddress},{user.CDSID},,{user.AccountType},,True,";
                sb.AppendLine(singeRowData);
            }

            if (File.Exists(outputFileName))
            {
                File.Delete(outputFileName);
            }
            File.AppendAllText(outputFileName, sb.ToString());
        }

        //public static List<VistwayUser> LoadVistwayUsersFromExcel(string fileName)
        //{
        //    var users = new List<VistwayUser>();

        //    Excel.Application xlApp = null; // = new Excel.Application();
        //    Excel.Workbook xlWorkbook = null; //= xlApp.Workbooks.Open(fileName);
        //    Excel.Worksheet xlWorksheet = null; //= xlWorkbook.Sheets[1];

        //    try
        //    {
        //        xlApp = new Excel.Application();
        //        xlWorkbook = xlApp.Workbooks.Open(fileName);
        //        xlWorksheet = xlWorkbook.Sheets[1];

        //        int rowCount = xlWorksheet.UsedRange.Rows.Count;

        //        for (int i = GlobalValues.FIRST_ROW_VISTWAY_USERS; i <= rowCount; i++)
        //        {
        //            string id = xlWorksheet.Cells[i, VistwayUserColumnsExcel.Id].Text.Trim().ToUpper();

        //            //if (users.Any(u => u.Id.Equals(id)))
        //            //{
        //            //    throw new ArgumentOutOfRangeException(Messages.DuplicateUserFound);
        //            //}
        //            string firstName = xlWorksheet.Cells[i, VistwayUserColumnsExcel.FirstName].Text.Trim();
        //            string lastName = xlWorksheet.Cells[i, VistwayUserColumnsExcel.LastName].Text.Trim();
        //            string emailAddress = xlWorksheet.Cells[i, VistwayUserColumnsExcel.Email].Text.Trim();

        //            string accountTypeAsString = xlWorksheet.Cells[i, VistwayUserColumnsExcel.AccountType].Text.Trim();
        //            AccountType accountType;
        //            if (!Enum.TryParse(accountTypeAsString, out accountType))
        //            {
        //                throw new ArgumentOutOfRangeException();
        //            }

        //            string countryCode = xlWorksheet.Cells[i, VistwayUserColumnsExcel.CountryCode].Text.Trim();
        //            countries = LoadCountries();
        //            string country = countries[countryCode];

        //            string combinedGroups = xlWorksheet.Cells[i, VistwayUserColumnsExcel.Groups].Text;
        //            var groups = combinedGroups.Split(new char[] { ';' }, StringSplitOptions.RemoveEmptyEntries).Select(s => s.Trim()).ToList();

        //            bool ignoreLDAP = xlWorksheet.Cells[i, VistwayUserColumnsExcel.Groups].Text.Trim() == "True";

        //            users.Add(new VistwayUser(id, firstName, lastName, accountType, country, groups, emailAddress, ignoreLDAP));
        //        }
        //    }
        //    catch (Exception ex)
        //    {
        //        OutputWriter.DisplayMessageAndAddToLogFile(ex.Source + ex.Message);
        //        //users = null;
        //    }
        //    finally
        //    {
        //        GC.Collect();
        //        GC.WaitForPendingFinalizers();
        //        if (xlWorksheet != null)
        //        {
        //            Marshal.ReleaseComObject(xlWorksheet);
        //        }
        //        if (xlWorkbook != null)
        //        {
        //            xlWorkbook.Close();
        //            Marshal.ReleaseComObject(xlWorkbook);
        //        }
        //        if (xlApp != null)
        //        {

        //            Marshal.ReleaseComObject(xlApp);
        //        }
        //    }
        //    xlWorkbook.Close();
        //    xlApp.Quit();

        //    return users;
        //}

        private enum VistwayUserColumnsCsv
        {
            Id = 1,
            FirstName = 3,
            LastName = 4,
            AccountType = 8,
            CountryCode = 10,
            Email = 17,
            Groups = 19,
            IgnoreLDAP = 22
        }

        //private enum VistwayUserColumnsExcel
        //{
        //    Id = 2,
        //    FirstName = 4,
        //    LastName = 5,
        //    AccountType = 9,
        //    CountryCode = 11,
        //    Email = 18,
        //    Groups = 20,
        //    IgnoreLDAP = 23
        //}

        private enum EnterProjUserColumnsCsv
        {
            Id = 1,
            CostCenterCode = 4,
            CostCenterName = 5,
            Skill = 7
        }

        //private static List<VistwayUser> ReadVistwayUsersData(string fileName)
        //{
        //    var users = new List<VistwayUser>();

        //    Excel.Application xlApp = null; // = new Excel.Application();
        //    Excel.Workbook xlWorkbook = null; //= xlApp.Workbooks.Open(fileName);
        //    Excel.Worksheet xlWorksheet = null; //= xlWorkbook.Sheets[1];

        //    try
        //    {
        //        xlApp = new Excel.Application();
        //        xlWorkbook = xlApp.Workbooks.Open(fileName);
        //        xlWorksheet = xlWorkbook.Sheets[1];

        //        int rowCount = xlWorksheet.UsedRange.Rows.Count;

        //        for (int i = GlobalValues.FIRST_ROW_VISTWAY_USERS; i <= rowCount; i++)
        //        {
        //            string account = xlWorksheet.Cells[i, UsersColumn.Account].Text.Trim().ToLower();
        //            if (!string.IsNullOrEmpty(account))
        //            {
        //                if (!users.Any(u => u.Account.Equals(account)))
        //                {
        //                    string firstName = xlWorksheet.Cells[i, UsersColumn.FirstName].Text;
        //                    string lastName = xlWorksheet.Cells[i, UsersColumn.LastName].Text;
        //                    string email = xlWorksheet.Cells[i, UsersColumn.Email].Text;
        //                    string countryCode = xlWorksheet.Cells[i, UsersColumn.CountryCode].Text;
        //                    string groups = xlWorksheet.Cells[i, UsersColumn.Groups].Text;

        //                    var groupsList = groups.Split(new char[] { ';' }, StringSplitOptions.RemoveEmptyEntries).Select(s => s.Trim()).ToList();
        //                    var userGroups = new HashSet<string>(groupsList);

        //                    var user = new VistwayUser(account, firstName, lastName, email, countryCode, userGroups);
        //                    users.Add(user);
        //                }
        //                else
        //                {
        //                    OutputWriter.DisplayMessageAndAddToLogFile(Messages.DuplicateUserFound);
        //                }
        //            }
        //        }
        //    }
        //    catch (Exception ex)
        //    {
        //        OutputWriter.DisplayMessageAndAddToLogFile(ex.Source + ex.Message);
        //        users = null;
        //    }
        //    finally
        //    {
        //        GC.Collect();
        //        GC.WaitForPendingFinalizers();
        //        if (xlWorksheet != null)
        //        {
        //            Marshal.ReleaseComObject(xlWorksheet);
        //        }
        //        if (xlWorkbook != null)
        //        {
        //            xlWorkbook.Close();
        //            Marshal.ReleaseComObject(xlWorkbook);
        //        }
        //        if (xlApp != null)
        //        {
        //            xlApp.Quit();
        //            Marshal.ReleaseComObject(xlApp);
        //        }
        //    }

        //    return users;
        //}

        //private static List<EnterProjUser> ReadEnterProjUsersData(string fileName)
        //{
        //    var enerProjUsers = new List<EnterProjUser>();

        //    Excel.Application xlApp = null;
        //    Excel.Workbook xlWorkbook = null;
        //    Excel.Worksheet xlWorksheet = null;

        //    try
        //    {
        //        xlApp = new Excel.Application();
        //        xlWorkbook = xlApp.Workbooks.Open(fileName);
        //        xlWorksheet = xlWorkbook.Sheets[1];

        //        int rowCount = xlWorksheet.UsedRange.Rows.Count;

        //        for (int i = GlobalValues.FIRST_ROW_ENTERPROJ_USERS; i <= rowCount; i++)
        //        {
        //            string account = xlWorksheet.Cells[i, EnterProjColumn.Account].Text.Trim().ToLower();
        //            if (!string.IsNullOrEmpty(account) && !enerProjUsers.Any(u => u.Account.Equals(account)))
        //            {
        //                string skill = xlWorksheet.Cells[i, EnterProjColumn.Skill].Value.ToString();
        //                string groupName = xlWorksheet.Cells[i, EnterProjColumn.GroupName].Value.ToString();
        //                string groupCode = xlWorksheet.Cells[i, EnterProjColumn.GroupCode].Value.ToString();

        //                var user = new EnterProjUser(account, skill, groupName, groupCode);
        //                enerProjUsers.Add(user);
        //            }
        //        }
        //    }
        //    catch (Exception ex)
        //    {
        //        OutputWriter.DisplayMessageAndAddToLogFile(ex.Source + ex.Message);
        //        enerProjUsers = null;
        //    }
        //    finally
        //    {
        //        GC.Collect();
        //        GC.WaitForPendingFinalizers();
        //        if (xlWorksheet != null)
        //        {
        //            Marshal.ReleaseComObject(xlWorksheet);
        //        }
        //        if (xlWorkbook != null)
        //        {
        //            xlWorkbook.Close();
        //            Marshal.ReleaseComObject(xlWorkbook);
        //        }
        //        if (xlApp != null)
        //        {
        //            xlApp.Quit();
        //            Marshal.ReleaseComObject(xlApp);
        //        }
        //    }

        //    return enerProjUsers;
        //}


    }
}
