using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UserManagement
{
    public static class Groups
    {
        //Key = Level 1 groups (eP groups). Value = corresponding Level 2 groups.
        private static Dictionary<string, string> autoGroups = new Dictionary<string, string>()
        {
            {"L1 SW Engineer","L2 Software" },
            {"L1 DDDDD","L2 Software" },
            {"L1 EEEEE","L2 Software" },
            {"L1 qwertyu","L2 Systems" },
            {"L1 rrrrr","L2 Systems" },
            {"L1 ttttt","L2 Systems" }
        };

        //Key = Level 2 automatic group. Value = Corresponding Level 2 manual groups.
        private static Dictionary<string, string> manualGroups = new Dictionary<string, string>()
        {
            {"L2 Software","M2 Software" },
            {"L2 MDO","M2 MDO" }
        };

        private static Dictionary<string, string> countries = new Dictionary<string, string>()
        {
            {"IND","India" },
            {"USA","USA" },
            {"MEX","Mexico" },
            {"BGR","Bulgaria" },
            {"BRA","Brazil" },
            {"GBR","UK" },
            {"TUN","Tunisia" },
            {"JPN","Japan" },
            {"DEU","Germany" },
            {"PRT","Portugal" },
            {"FRA","France" },
            {"SVK","Slovakia" },
            {"RUS","Russia" },
            {"CHN","China" },
            {"KOR","South Korea" },
            {"THA","Thailand" },
            {"AUS","Australia" },
            {"ESP","Spain" }
        };

        public static string GetLevelTwoGroupByLevelOneGroup(string levelOneGroup)
        {
            if (autoGroups.ContainsKey(levelOneGroup))
            {
                return autoGroups[levelOneGroup];
            }
            return "NEW L2 " + levelOneGroup;
            //raise a warning
        }

        public static string GetManualGroupByLevelTwoGroup(string levelTwoGroup)
        {
            if (manualGroups.ContainsKey(levelTwoGroup))
            {
                return manualGroups[levelTwoGroup];
            }
            return "NEW M2 " + levelTwoGroup;
            //raise a warning
        }

        public static string GetCountryNameByCountryCode(string countryCode)
        {
            if (countries.ContainsKey(countryCode.Trim().ToUpper()))
            {
                return countries[countryCode];
            }
            return countryCode;
            //warning is required that a new country is added
        }

    }
}
