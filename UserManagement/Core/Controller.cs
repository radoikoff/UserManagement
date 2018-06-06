using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UserManagement.Data;
using UserManagement.Models;
using UserManagement.Models.Users;
using UserManagement.StaticData;
using UserManagement.IO;

namespace UserManagement.Core
{
    public class Controller
    {
        private const string AUTO_GROUP_PREFIX = "eP_";
        private const string MANUAL_GROUP_PREFIX = "M_";
        private const string MANUAL_S_GROUP_PREFIX = "S_";
        private const string COUNTRY_GROUP_PREFIX = "Country:";

        private ILogger logger;
        private AppData data;

        public Controller(AppData data, ILogger logger)
        {
            this.logger = logger;
            this.data = data;
        }

        private List<VistwayUser> GetUpdatedUsers()
        {
            if (!this.data.IsValid)
            {
                return null;
            }

            var updatedUsers = new List<VistwayUser>();

            foreach (var vistwayUser in this.data.VistwayUsers)
            {
                bool isSkillUpdated = false;
                bool isCountryUpdated = false;
                var enterProjUser = this.data.EnterProjUsers.FirstOrDefault(u => u.Id == vistwayUser.Id);

                isSkillUpdated = UpdateUserSkillGroups(vistwayUser, enterProjUser, logger);
                isCountryUpdated = UpdateUserCountryGroup(vistwayUser, this.data.Countries, logger);

                if (isSkillUpdated || isCountryUpdated)
                {
                    updatedUsers.Add(vistwayUser);
                }
            }

            return updatedUsers;

        }

        private bool UpdateUserSkillGroups(VistwayUser user, EnterProjUser enterProjUser, ILogger logger)
        {
            string[] userAutoGroups = user.Groups.Where(g => g.StartsWith(AUTO_GROUP_PREFIX)).ToArray();
            string[] userManualGroups = user.Groups.Where(g => g.StartsWith(MANUAL_GROUP_PREFIX) || g.StartsWith(MANUAL_S_GROUP_PREFIX)).ToArray();

            string[] userSpecialAutoGroups = userAutoGroups.Intersect(this.data.SpecialSubGroups.Where(gn => gn.StartsWith(AUTO_GROUP_PREFIX))).ToArray();
            string[] userSpecialManualGroups = userManualGroups.Intersect(this.data.SpecialSubGroups.Where(gn => gn.StartsWith(MANUAL_GROUP_PREFIX) || gn.StartsWith(MANUAL_S_GROUP_PREFIX))).ToArray();

            if (userSpecialAutoGroups.Length > 1 || userSpecialManualGroups.Length > 1)
            {
                logger.DisplayMessage(MsgType.ERROR, Messages.UserHasMoreThanOneSpecialGroups, user.Id, string.Join(", ", userSpecialAutoGroups, userSpecialManualGroups));
                return false;
            }

            string userSpecialAutoGroup = userSpecialAutoGroups.FirstOrDefault();
            string userSpecialManualGroup = userSpecialManualGroups.FirstOrDefault();

            // remove all high-level reporting groups
            user.RemoveGroupsByNameList(this.data.ReportingGroups);

            //take initial groups
            List<string> initialUserGroups = user.Groups.ToList();

            if (enterProjUser != null)
            {
                string userTrueAutoGroup = AUTO_GROUP_PREFIX + enterProjUser.Skill;

                //check if we know the sub group
                CheckIfUserGroupExists(userTrueAutoGroup);

                user.RemoveGroupsByPrefix(AUTO_GROUP_PREFIX);
                user.AddGroup(userTrueAutoGroup);
                
                if (data.SpecialSubGroups.Contains(userTrueAutoGroup))
                {
                    if (userManualGroups.Length != 1)
                    {
                        logger.DisplayMessage(MsgType.ACTION, Messages.UserIsMemeberOfAutoSpecialGroup, user.Id, userTrueAutoGroup, (userManualGroups.Length != 0) ? string.Join(", ", userManualGroups) : "None");
                    }
                }
                else
                {
                    user.RemoveGroupsByPrefix(MANUAL_GROUP_PREFIX);
                    user.RemoveGroupsByPrefix(MANUAL_S_GROUP_PREFIX);
                    if (userSpecialManualGroup != null)
                    {
                        user.AddGroup(userSpecialManualGroup);
                    }
                }
            }
            else //user has no enterProj skill
            {
                if (userAutoGroups.Length > 1 && userAutoGroups.ToList().Contains(userSpecialAutoGroup))
                {
                    user.RemoveGroupsByPrefix(AUTO_GROUP_PREFIX);
                    user.AddGroup(userSpecialAutoGroup);
                    userAutoGroups = user.Groups.Where(g => g.StartsWith(AUTO_GROUP_PREFIX)).ToArray();
                }

                switch (userAutoGroups.Length)
                {
                    case 0:
                        if (userManualGroups.Length == 0)
                        {
                            logger.DisplayMessage(MsgType.NOTASSIGNED, Messages.UserHasNoGroupAssignment, user.Id);
                        }
                        else if (userManualGroups.Length > 1)
                        {
                            logger.DisplayMessage(MsgType.WARNING, Messages.UserAssignedToMoreThanOneManualGroup, user.Id, string.Join(", ", userManualGroups));
                        }
                        break;

                    case 1:
                        if (userAutoGroups.First() == userSpecialAutoGroup)
                        {
                            if (userManualGroups.Length != 1)
                            {
                                logger.DisplayMessage(MsgType.ACTION, Messages.UserIsMemeberOfAutoSpecialGroup, user.Id, userAutoGroups.First(), (userManualGroups.Length != 0) ? string.Join(", ", userManualGroups) : "None");
                            }
                        }
                        else
                        {
                            user.RemoveGroupsByPrefix(MANUAL_GROUP_PREFIX);
                            user.RemoveGroupsByPrefix(MANUAL_S_GROUP_PREFIX);
                            if (userSpecialManualGroup != null)
                            {
                                user.AddGroup(userSpecialManualGroup);
                            }
                        }
                        break;

                    default:
                        logger.DisplayMessage(MsgType.WARNING, Messages.UserAssignedToMoreThanOneAutoGroup, user.Id, string.Join(", ", userAutoGroups));
                        break;
                }
            }

            //check if user is updated or not
            var commonItems = initialUserGroups.Intersect(user.Groups);
            var removedItems = initialUserGroups.Where(g => !commonItems.Any(i => g.Contains(i)));
            var addedItems = user.Groups.Where(g => !commonItems.Any(i => g.Contains(i)));

            bool userUpdated = !(user.Groups.Count() == initialUserGroups.Count && initialUserGroups.All(g => user.Groups.Contains(g)));
            //bool userUpdated = !(commonItems.Count() == initialUserGroups.Count && initialUserGroups.All(g => commonItems.Contains(g)));

            if (userUpdated)
            {
                logger.DisplayMessage(MsgType.INFO, Messages.UserGroupsUpdatesResult, user.Id, string.Join(", ", addedItems), string.Join(", ", removedItems));
                return true;
            }

            return false;
        }

        private void CheckIfUserGroupExists(string userTrueAutoGroup)
        {
            bool isUserGroupExists = this.data.SubGroups.Contains(userTrueAutoGroup);
            if (!isUserGroupExists)
            {
                logger.DisplayMessage(MsgType.WARNING, Messages.NewAutoUserGroupExists, userTrueAutoGroup);
            }
        }

        private bool UpdateUserCountryGroup(VistwayUser user, IEnumerable<Country> countries, ILogger logger)
        {
            bool isUserUpdated = false;
            Country userCountry = countries.FirstOrDefault(c => c.Code == user.CountryCode);

            if (userCountry == null)
            {
                logger.DisplayMessage(MsgType.WARNING, Messages.NotExisitingCountry, user.Id, user.CountryCode);
                return false;
            }

            string userTrueCountryGroup = COUNTRY_GROUP_PREFIX + userCountry.Name;
            var userCurrentCountries = user.Groups.Where(c => c.StartsWith(COUNTRY_GROUP_PREFIX)).ToList();


            switch (userCurrentCountries.Count())
            {
                case 0:
                    user.AddGroup(userTrueCountryGroup);
                    isUserUpdated = true;
                    logger.DisplayMessage(MsgType.INFO, Messages.UserAddedToCountryGroup, user.Id, userTrueCountryGroup);
                    break;
                case 1:
                    if (userCurrentCountries.First() != userTrueCountryGroup)
                    {
                        user.RemoveGroupsByPrefix(COUNTRY_GROUP_PREFIX);
                        user.AddGroup(userTrueCountryGroup);
                        isUserUpdated = true;
                        logger.DisplayMessage(MsgType.INFO, Messages.UserCountryGroupUpdated, user.Id, userCurrentCountries.First(), userTrueCountryGroup);
                    }
                    break;
                default:
                    user.RemoveGroupsByPrefix(COUNTRY_GROUP_PREFIX);
                    user.AddGroup(userTrueCountryGroup);
                    isUserUpdated = true;
                    logger.DisplayMessage(MsgType.INFO, Messages.UserCountryGroupUpdated, user.Id, string.Join(", ", userCurrentCountries), userTrueCountryGroup);
                    break;
            }

            return isUserUpdated;
        }

        public void Execute()
        {
            var users = GetUpdatedUsers();
            data.SaveResultFile(users);
            logger.DisplaySummaryStats();
        }

    }
}
