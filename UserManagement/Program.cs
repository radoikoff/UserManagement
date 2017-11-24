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
            var vistwayUsers = ReadVistwayUsersData(@"C:\Users\vradoyko\Desktop\users.xlsx");
            // var enterProjUsers = ReadEnterProjUsersData(@"C:\Users\vradoyko\Desktop\FTE - Oct 2017.xlsx");

            foreach (var user in vistwayUsers)
            {
                UpdateUserCountryGroup(user);
            }

        }

        private static void UpdateUserCountryGroup(User user)
        {
            string userTrueCountryGroup = "Country:" + Groups.GetCountryNameByCountryCode(user.CountryCode);
            List<string> userCurrentCountries = user.Groups.Where(c => c.Contains("Country:")).ToList();

            if (userCurrentCountries.Count() > 1)
            {
                user.Groups.RemoveWhere(g => g.Contains("Country:"));
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

        public static List<User> ReadVistwayUsersData(string fileName)
        {
            var users = new List<User>();

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

                    var user = new User(account, firstName, lastName, email, countryCode, userGroups);
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

