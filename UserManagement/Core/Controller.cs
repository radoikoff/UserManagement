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

        Logger logger;
        AppData data;

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
            bool isUserUpdated = false;

            var userAutoGroups = user.Groups.Where(g => g.StartsWith(AUTO_GROUP_PREFIX)).ToList();
            var userManualGroups = user.Groups.Where(g => g.StartsWith(MANUAL_GROUP_PREFIX)).ToList();
            bool isUserSpecial = data.SpecialUsersIds.Contains(user.Id);

            if (enterProjUser != null)
            {
                string userTrueAutoGroup = AUTO_GROUP_PREFIX + enterProjUser.Skill;

                //check if we know the sub group
                CheckIfUserGroupExists(userTrueAutoGroup);

                //handle special user groups
                bool hasSpecialGroup = data.SpecialGroupsNames.Intersect(userManualGroups).Any();
                if (hasSpecialGroup)
                {
                    if (userAutoGroups.Any())
                    {
                        user.RemoveGroupsByPrefix(AUTO_GROUP_PREFIX);
                        logger.DisplayMessage(MsgType.INFO, Messages.SpecialUserUpdated, user.Id);
                        return true;
                    }                   
                    return false;
                }

                switch (userAutoGroups.Count())
                {
                    case 0:
                        user.AddGroup(userTrueAutoGroup);
                        isUserUpdated = true;
                        logger.DisplayMessage(MsgType.INFO, Messages.UserAddedToAutoGroup, user.Id, userTrueAutoGroup);
                        break;

                    case 1:
                        if (!userAutoGroups.First().Equals(userTrueAutoGroup))
                        {
                            user.RemoveGroupsByPrefix(AUTO_GROUP_PREFIX);
                            user.AddGroup(userTrueAutoGroup);
                            isUserUpdated = true;
                            logger.DisplayMessage(MsgType.INFO, Messages.UserAutoGroupUpdated, user.Id, userTrueAutoGroup);
                        }
                        break;
                    default:
                        user.RemoveGroupsByPrefix(AUTO_GROUP_PREFIX);
                        user.AddGroup(userTrueAutoGroup);
                        isUserUpdated = true;
                        logger.DisplayMessage(MsgType.INFO, Messages.UserAssignedToMoreThanOneAutoGroups, user.Id);
                        logger.DisplayMessage(MsgType.INFO, Messages.UserAddedToAutoGroup, user.Id, userTrueAutoGroup);
                        break;
                }

                if (userManualGroups.Count() != 0)
                {
                    if (!isUserSpecial)
                    {
                        user.RemoveGroupsByPrefix(MANUAL_GROUP_PREFIX);
                        isUserUpdated = true;
                        logger.DisplayMessage(MsgType.INFO, Messages.UserRemovedFromManualGroup, user.Id, string.Join(", ", userManualGroups));
                    }
                }
            }
            else //user has no enterProj skill
            {
                if (userManualGroups.Count() == 0 && userAutoGroups.Count() == 0)
                {
                    logger.DisplayMessage(MsgType.NOTASSIGNED, Messages.UserHasNoGroupAssignment, user.Id);
                }

                if (userAutoGroups.Count() == 1)
                {
                    user.RemoveGroupsByPrefix(AUTO_GROUP_PREFIX);
                    isUserUpdated = true;
                    logger.DisplayMessage(MsgType.INFO, Messages.UserAssignedToAnAutoGroupsWithoutBeingInEP, user.Id, userAutoGroups.First());
                }

                if (userAutoGroups.Count() > 1)
                {
                    user.RemoveGroupsByPrefix(AUTO_GROUP_PREFIX);
                    isUserUpdated = true;
                    logger.DisplayMessage(MsgType.INFO, Messages.UserAssignedToMoreThanOneAutoGroups, user.Id);
                }

                if (userManualGroups.Count() > 1)
                {
                    logger.DisplayMessage(MsgType.ACTION, Messages.UserAssignedToMoreThanOneManualGroup, user.Id);
                }
            }

            return isUserUpdated;
        }

        private void CheckIfUserGroupExists(string userTrueAutoGroup)
        {
            bool isUserGroupExists = this.data.Groups.SelectMany(g => g.SubGroups).Contains(userTrueAutoGroup);
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
