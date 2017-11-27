using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UserManagement
{
    public class GlobalValues
    {
        public const int FIRST_ROW_VISTWAY_USERS = 2;

        public const int FIRST_ROW_ENTERPROJ_USERS = 8;

        public const string COUNTRY_GROUP_PREFIX = "Country: ";

        public const string LEVEL_ONE_GROUP_PREFIX = "SubGroup: ";

        public const string LEVEL_TWO_GROUP_PREFIX = "Group: ";

        public const string LEVEL_TWO_MANUAL_GROUP_PREFIX = "ManualGroup: ";
    }

    public enum UsersColumn
    {
        Account = 2,
        FirstName = 4,
        LastName = 5,
        Email = 18,
        CountryCode = 11,
        Groups = 20
    }

    public enum EnterProjColumn
    {
        Account = 2,
        Skill = 8,
        GroupCode = 5,
        GroupName = 6
    }
}
