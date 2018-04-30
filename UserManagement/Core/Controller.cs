using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UserManagement.Data;
using UserManagement.Models;
using UserManagement.Models.Users;

namespace UserManagement.Core
{
    class Controller
    {
        private const string AUTO_GROUP_PREFIX = "eP_";
        private const string MANUAL_GROUP_PREFIX = "M_";
        private const string COUNTRY_GROUP_PREFIX = "Country:";

        private Logger logger;
        private AppData data;

        public Controller(AppData data, Logger logger)
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

            this.logger.Summary.TotalUserCount = this.data.VistwayUsers.Count;
            this.logger.Summary.UpdatedUsersCount = updatedUsers.Count;

            return updatedUsers;

        }

        private bool UpdateUserSkillGroups(VistwayUser user, EnterProjUser enterProjUser, Logger logger)
        {
            string[] userAutoGroups = user.Groups.Where(g => g.StartsWith(AUTO_GROUP_PREFIX)).ToArray();
            string[] userManualGroups = user.Groups.Where(g => g.StartsWith(MANUAL_GROUP_PREFIX)).ToArray();

            string[] userSpecialAutoGroups = userAutoGroups.Intersect(this.data.SpecialSubGroups.Where(gn => gn.StartsWith(AUTO_GROUP_PREFIX))).ToArray();
            string[] userSpecialManualGroups = userManualGroups.Intersect(this.data.SpecialSubGroups.Where(gn => gn.StartsWith(MANUAL_GROUP_PREFIX))).ToArray();

            if (userSpecialAutoGroups.Length > 1 || userSpecialManualGroups.Length > 1)
            {
                logger.DisplayMessage(MsgType.ERROR, Messages.UserHasMoreThanOneSpecialGroups, user.Id, string.Join(", ", userSpecialAutoGroups, userSpecialManualGroups));
                return false;
            }

            string userSpecialAutoGroup = userSpecialAutoGroups.FirstOrDefault();
            string userSpecialManualGroup = userSpecialManualGroups.FirstOrDefault();

            // remove all high-level reporting groups
            user.RemoveGroupsByNameList(this.data.ReportingGroups);

            if (enterProjUser != null)
            {
                string userTrueAutoGroup = AUTO_GROUP_PREFIX + enterProjUser.Skill;

                //check if we know the sub group
                CheckIfUserGroupExists(userTrueAutoGroup);

                if (userAutoGroups.Length == 0 && userManualGroups.Length == 0)  //Case 1
                {
                    user.AddGroup(userTrueAutoGroup);
                    logger.DisplayMessage(MsgType.INFO, Messages.UserAddedToAutoGroup, user.Id, userTrueAutoGroup);
                    return true;
                }

                if (userAutoGroups.Length == 0 && userSpecialManualGroup == null)  //Case 2,3
                {
                    user.RemoveGroupsByPrefix(MANUAL_GROUP_PREFIX);
                    user.AddGroup(userTrueAutoGroup);
                    logger.DisplayMessage(MsgType.INFO, Messages.UserUpdatedToAutoGroup, user.Id, userTrueAutoGroup, string.Join(", ", userManualGroups));
                    return true;
                }

                if (userAutoGroups.Length == 0 && userManualGroups.Length > 1 && userSpecialManualGroup != null)  //Case 5
                {
                    user.RemoveGroupsByPrefix(MANUAL_GROUP_PREFIX);
                    user.AddGroup(userSpecialManualGroup);
                    logger.DisplayMessage(MsgType.INFO, Messages.UserManualGroupsUpdated, user.Id, string.Join(", ", userManualGroups), userSpecialManualGroup);
                    return true;
                }

                if (userManualGroups.Length == 0 && userAutoGroups.Length == 1 && userSpecialAutoGroup == null) //Case 6
                {
                    if (userTrueAutoGroup != userAutoGroups.First())
                    {
                        user.RemoveGroupsByPrefix(AUTO_GROUP_PREFIX);
                        user.AddGroup(userTrueAutoGroup);
                        logger.DisplayMessage(MsgType.INFO, Messages.UserAutoGroupUpdated, user.Id, userAutoGroups.First(), userTrueAutoGroup);
                        return true;
                    }
                    return false;
                }

                if (userManualGroups.Length == 0 && userAutoGroups.Length > 1 && userSpecialAutoGroup == null) //Case 7
                {
                    user.RemoveGroupsByPrefix(AUTO_GROUP_PREFIX);
                    user.AddGroup(userTrueAutoGroup);
                    logger.DisplayMessage(MsgType.INFO, Messages.UserAutoGroupUpdated, user.Id, string.Join(", ", userAutoGroups), userTrueAutoGroup);
                    return true;
                }

                if (userManualGroups.Length == 0 && userAutoGroups.Length == 1 && userSpecialAutoGroup != null) //Case 8
                {
                    bool isUpdated = false;
                    if (userTrueAutoGroup != userAutoGroups.First())
                    {
                        user.RemoveGroupsByPrefix(AUTO_GROUP_PREFIX);
                        user.AddGroup(userTrueAutoGroup);
                        logger.DisplayMessage(MsgType.INFO, Messages.UserAutoGroupUpdated, user.Id, userAutoGroups.First(), userTrueAutoGroup);
                        isUpdated = true;
                    }
                    logger.DisplayMessage(MsgType.ACTION, Messages.UserIsMemeberOfEpSpecialGroup, user.Id, userTrueAutoGroup);
                    return isUpdated;
                }

                if (userManualGroups.Length == 0 && userAutoGroups.Length > 1 && userSpecialAutoGroup != null) //Case 9
                {
                    user.RemoveGroupsByPrefix(AUTO_GROUP_PREFIX);
                    user.AddGroup(userTrueAutoGroup);
                    logger.DisplayMessage(MsgType.INFO, Messages.UserAutoGroupUpdated, user.Id, string.Join(", ", userAutoGroups), userTrueAutoGroup);
                    logger.DisplayMessage(MsgType.ACTION, Messages.UserIsMemeberOfEpSpecialGroup, user.Id, userTrueAutoGroup);
                    return true;
                }

                if (userManualGroups.Length > 0 && userAutoGroups.Length == 1 && userSpecialAutoGroup == null && userSpecialManualGroup == null) //Case 10,11,
                {
                    user.RemoveGroupsByPrefix(MANUAL_GROUP_PREFIX);
                    logger.DisplayMessage(MsgType.INFO, Messages.UserRemovedFromManualGroups, user.Id, string.Join(", ", userManualGroups));

                    if (userTrueAutoGroup != userAutoGroups.First())
                    {
                        user.RemoveGroupsByPrefix(AUTO_GROUP_PREFIX);
                        user.AddGroup(userTrueAutoGroup);
                        logger.DisplayMessage(MsgType.INFO, Messages.UserAutoGroupUpdated, user.Id, userAutoGroups.First(), userTrueAutoGroup);
                    }
                    return true;
                }

                if (userManualGroups.Length > 0 && userAutoGroups.Length > 1 && userSpecialAutoGroup == null && userSpecialManualGroup == null) //Case 12,13,
                {
                    user.RemoveGroupsByPrefix(MANUAL_GROUP_PREFIX);
                    logger.DisplayMessage(MsgType.INFO, Messages.UserRemovedFromManualGroups, user.Id, string.Join(", ", userManualGroups));
                    user.RemoveGroupsByPrefix(AUTO_GROUP_PREFIX);
                    user.AddGroup(userTrueAutoGroup);
                    logger.DisplayMessage(MsgType.INFO, Messages.UserAutoGroupUpdated, user.Id, string.Join(", ", userAutoGroups), userTrueAutoGroup);
                    return true;
                }

                if (userManualGroups.Length == 1 && userAutoGroups.Length == 1 && userSpecialManualGroup == null && userSpecialAutoGroup != null) //Case 14
                {
                    if (userTrueAutoGroup != userAutoGroups.First())
                    {
                        user.RemoveGroupsByPrefix(AUTO_GROUP_PREFIX);
                        user.AddGroup(userTrueAutoGroup);
                        logger.DisplayMessage(MsgType.INFO, Messages.UserAutoGroupUpdated, user.Id, userAutoGroups.First(), userTrueAutoGroup);
                        return true;
                    }
                    return false;
                }

                if (userManualGroups.Length > 1 && userAutoGroups.Length == 1 && userSpecialManualGroup == null && userSpecialAutoGroup != null) //Case 15
                {
                    logger.DisplayMessage(MsgType.ACTION, Messages.UserForManualAssignmentHasMoreThanOneManualGroup, user.Id, string.Join(", ", userManualGroups));

                    if (userTrueAutoGroup != userAutoGroups.First())
                    {
                        user.RemoveGroupsByPrefix(AUTO_GROUP_PREFIX);
                        user.AddGroup(userTrueAutoGroup);
                        logger.DisplayMessage(MsgType.INFO, Messages.UserAutoGroupUpdated, user.Id, userAutoGroups.First(), userTrueAutoGroup);
                        return true;
                    }
                    return false;
                }

                if (userManualGroups.Length == 1 && userAutoGroups.Length > 1 && userSpecialManualGroup == null && userSpecialAutoGroup != null) //Case 16
                {
                    user.RemoveGroupsByPrefix(AUTO_GROUP_PREFIX);
                    user.AddGroup(userTrueAutoGroup);
                    logger.DisplayMessage(MsgType.INFO, Messages.UserAutoGroupUpdated, user.Id, string.Join(", ", userAutoGroups), userTrueAutoGroup);
                    return true;
                }

                if (userManualGroups.Length > 1 && userAutoGroups.Length > 1 && userSpecialManualGroup == null && userSpecialAutoGroup != null) //Case 17
                {
                    logger.DisplayMessage(MsgType.ACTION, Messages.UserForManualAssignmentHasMoreThanOneManualGroup, user.Id, string.Join(", ", userManualGroups));

                    user.RemoveGroupsByPrefix(AUTO_GROUP_PREFIX);
                    user.AddGroup(userTrueAutoGroup);
                    logger.DisplayMessage(MsgType.INFO, Messages.UserAutoGroupUpdated, user.Id, string.Join(", ", userAutoGroups), userTrueAutoGroup);
                    return true;
                }

                if (userManualGroups.Length == 1 && userAutoGroups.Length > 0 && userSpecialManualGroup != null) //Case 18, 20
                {
                    user.RemoveGroupsByPrefix(AUTO_GROUP_PREFIX);
                    logger.DisplayMessage(MsgType.INFO, Messages.UserRemovedFromAutoGroups, user.Id, string.Join(", ", userAutoGroups), userSpecialManualGroup);
                }

                if (userManualGroups.Length > 1 && userAutoGroups.Length > 0 && userSpecialManualGroup != null) //Case 19, 21
                {
                    user.RemoveGroupsByPrefix(AUTO_GROUP_PREFIX);
                    logger.DisplayMessage(MsgType.INFO, Messages.UserRemovedFromAutoGroups, user.Id, string.Join(", ", userAutoGroups), userSpecialManualGroup);

                    user.RemoveGroupsByPrefix(MANUAL_GROUP_PREFIX);
                    user.AddGroup(userSpecialManualGroup);
                    logger.DisplayMessage(MsgType.INFO, Messages.UserManualGroupsUpdated, user.Id, string.Join(", ", userManualGroups), userSpecialManualGroup);
                }

            }
            else //user has no enterProj skill
            {
                if (userManualGroups.Count() == 0 && userAutoGroups.Count() == 0)
                {
                    logger.DisplayMessage(MsgType.NOTASSIGNED, Messages.UserHasNoGroupAssignment, user.Id); //case 22
                    return false;
                }

                if (userManualGroups.Count() > 1 && userAutoGroups.Count() == 0) //Case 24
                {
                    logger.DisplayMessage(MsgType.ACTION, Messages.UserAssignedToMoreThanOneManualGroup, user.Id);
                    return false;
                }

                if (userManualGroups.Count() == 0 && userAutoGroups.Count() > 0 && userSpecialAutoGroup == null) //Case 25, 26
                {
                    logger.DisplayMessage(MsgType.INFO, Messages.UserAssignedToAnAutoGroupsWithoutBeingInEP, user.Id, string.Join(", ", userAutoGroups));
                    return false;
                }

                if (userManualGroups.Count() == 0 && userAutoGroups.Count() > 0 && userSpecialAutoGroup != null) //Case 27, 28
                {
                    logger.DisplayMessage(MsgType.INFO, Messages.UserAssignedToAnAutoGroupsWithoutBeingInEP, user.Id, string.Join(", ", userAutoGroups));
                    logger.DisplayMessage(MsgType.INFO, Messages.UserAssignedToSpecialAutoGroupsWithoutBeingInManualGroup, user.Id, userSpecialAutoGroup);
                    return false;
                }

                if (userManualGroups.Count() > 0 && userAutoGroups.Count() == 1 && userSpecialAutoGroup == null && userSpecialManualGroup == null) //Case 29, 30
                {
                    user.RemoveGroupsByPrefix(MANUAL_GROUP_PREFIX);
                    logger.DisplayMessage(MsgType.INFO, Messages.UserRemovedFromManualGroups, user.Id, string.Join(", ", userManualGroups));
                    return true;
                }

                if (userManualGroups.Count() > 0 && userAutoGroups.Count() > 1 && userSpecialAutoGroup == null && userSpecialManualGroup == null) //Case 31, 32
                {
                    logger.DisplayMessage(MsgType.INFO, Messages.UserAssignedToMoreThanOneManualOrAutoGroup, user.Id, string.Join(", ", userManualGroups, userAutoGroups));
                    return false;
                }

                if (userManualGroups.Count() > 1 && userAutoGroups.Count() == 1 && userSpecialAutoGroup != null && userSpecialManualGroup == null) //Case 34
                {
                    logger.DisplayMessage(MsgType.INFO, Messages.UserManuallyAssignedToMoreThanOneManualGroup, user.Id, string.Join(", ", userManualGroups));
                    return false;
                }

                if (userManualGroups.Count() == 1 && userAutoGroups.Count() > 1 && userSpecialAutoGroup != null && userSpecialManualGroup == null) //Case 35
                {
                    user.RemoveGroupsByPrefix(AUTO_GROUP_PREFIX);
                    user.AddGroup(userSpecialAutoGroup);
                    logger.DisplayMessage(MsgType.INFO, Messages.UserAutoGroupUpdated, user.Id, string.Join(", ", userAutoGroups), userSpecialAutoGroup);
                    return true;
                }

                if (userManualGroups.Count() > 1 && userAutoGroups.Count() > 1 && userSpecialAutoGroup != null && userSpecialManualGroup == null) //Case 36
                {
                    user.RemoveGroupsByPrefix(AUTO_GROUP_PREFIX);
                    user.AddGroup(userSpecialAutoGroup);
                    logger.DisplayMessage(MsgType.INFO, Messages.UserAutoGroupUpdated, user.Id, string.Join(", ", userAutoGroups), userSpecialAutoGroup);
                    logger.DisplayMessage(MsgType.INFO, Messages.UserManuallyAssignedToMoreThanOneManualGroup, user.Id, string.Join(", ", userManualGroups));
                    return true;
                }

                if (userManualGroups.Count() > 0 && userAutoGroups.Count() > 0 && userSpecialManualGroup != null) //Case 37,38,39,40
                {
                    user.RemoveGroupsByPrefix(AUTO_GROUP_PREFIX);
                    user.RemoveGroupsByPrefix(MANUAL_GROUP_PREFIX);
                    user.AddGroup(userSpecialManualGroup);
                    logger.DisplayMessage(MsgType.INFO, Messages.UserRemovedFromAllGRoupsExceptManualSpecialGroup, user.Id, userSpecialManualGroup);
                    return true;
                }

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

        private bool UpdateUserCountryGroup(VistwayUser user, IEnumerable<Country> countries, Logger logger)
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
                        logger.DisplayMessage(MsgType.INFO, Messages.UserCountryGroupUpdated, user.Id, userTrueCountryGroup);
                    }
                    break;
                default:
                    user.RemoveGroupsByPrefix(COUNTRY_GROUP_PREFIX);
                    user.AddGroup(userTrueCountryGroup);
                    isUserUpdated = true;
                    logger.DisplayMessage(MsgType.INFO, Messages.UserAssignedToMoreThanOneCountryGroups, user.Id);
                    logger.DisplayMessage(MsgType.INFO, Messages.UserAddedToCountryGroup, user.Id, userTrueCountryGroup);
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
