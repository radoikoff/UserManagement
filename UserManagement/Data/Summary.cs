using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UserManagement.Data
{
    class Summary
    {
        public int InfoMsgCount { get; set; }

        public int ActionMsgCount { get; set; }

        public int WarningMsgCount { get; set; }

        public int ErrorMsgCount { get; set; }

        public int TotalUserCount { get; set; }

        public int UpdatedUsersCount { get; set; }

        public int NotAssignedMsgCount { get; set; }



        public override string ToString()
        {
            string result = $"SUMMARY:: " + 
                            $"USERS: Total:{TotalUserCount} Updated:{UpdatedUsersCount} NotAssigned:{NotAssignedMsgCount} " + 
                            $"ERRORS: {ErrorMsgCount} WARNINGS:{WarningMsgCount} ACTIONS:{ActionMsgCount} INFOS:{InfoMsgCount}";
            return result;
        }
    }
}
