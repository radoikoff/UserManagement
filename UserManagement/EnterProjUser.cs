using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UserManagement
{
    public class EnterProjUser
    {
        public EnterProjUser()
        {
        }

        public EnterProjUser(string account, string skill, string groupName, string groupCode)
        {
            Account = account;
            Skill = skill;
            GroupName = groupName;
            GroupCode = groupCode;
        }

        public string Account { get; set; }
        public string Skill { get; set; }
        public string GroupCode { get; set; }
        public string GroupName { get; set; }
    }
}

