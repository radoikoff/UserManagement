using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UserManagement.Models.Users
{
    public class EnterProjUser : User
    {
        public EnterProjUser(string id, string skill, string costCenterCode, string costCenterName)
            : base(id)
        {
            this.Skill = skill;
            this.CostCenterCode = costCenterCode;
            this.CostCenterName = costCenterName;
        }

        private string skill;

        public string Skill
        {
            get { return skill; }
            private set
            {
                if (string.IsNullOrWhiteSpace(value))
                {
                    throw new ArgumentNullException();
                }
                skill = value;
            }
        }

        public string CostCenterCode { get; private set; }

        public string CostCenterName { get; private set; }
    }
}

