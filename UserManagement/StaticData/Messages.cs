using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UserManagement
{
    public static class Messages
    {
        public const string UserHasMoreThanOneSpecialGroups = "{0} has multiple special groups assigned ({1})";

        //public const string UserAddedToAutoGroup = "{0} is added to auto group {1}";

        //public const string UserUpdatedToAutoGroup = "{0} is added to auto group {1} and removed from manual groups: {2}";

        //public const string UserManualGroupsUpdated = "{0} manual groups {1} updated to: {2}";

        //public const string UserAutoGroupUpdated = "{0} auto group is changed from {1} to {2}";

        public const string UserIsMemeberOfAutoSpecialGroup = "{0} is memeber of {1} and must be added to a single manual group. Current manual groups:{2}";

        //public const string UserRemovedFromManualGroups = "{0} removed from manual groups: {1}";

        //public const string UserForManualAssignmentHasMoreThanOneManualGroup = "ACTION: {0} Manually assigned user is member of more than one manual group ({1})";

        //public const string UserRemovedFromAutoGroups = "{0} removed from auto groups {1} because it is memebr of {2}";

        public const string UserHasNoGroupAssignment = "{0} is not assigned to any group and does not have eP skill";

        public const string UserAssignedToMoreThanOneManualGroup = "{0} is member of more than one manual groups ({1})";

        public const string UserAssignedToMoreThanOneAutoGroup = "{0} is member of more than one auto groups ({1})";

        public const string UserGroupsUpdatesResult = "{0} groups updated. Added:{1} Deleted:{2}";


        //public const string UserAssignedToAnAutoGroupsWithoutBeingInEP = "{0} is member of {1} groups and curently does not have eP skill";

        //public const string UserAssignedToSpecialAutoGroupsWithoutBeingInManualGroup = "{0} is meber of special group {1} but has no M_ group.";

        //public const string UserAssignedToMoreThanOneManualOrAutoGroup = "{0} assigned to the following groups: {1}";

        //public const string UserManuallyAssignedToMoreThanOneManualGroup = "{0} manually assigned user is memebr of more than one manual groups {1}";

        // const string UserRemovedFromAllGRoupsExceptManualSpecialGroup = "{0} left only in {1}";


        public const string UserAddedToCountryGroup = "{0} is added to country group {1}";

        public const string UserCountryGroupUpdated = "{0} country group is changed from {1} to {2}";

        //public const string UserAssignedToMoreThanOneCountryGroups = "{0} was assigned to more than one country group";

        public const string NotExisitingCountry = "{0} - country code {1} has no country name";

        public const string NoUsersToUpdate = "There are no users to update!";

        public const string NewAutoUserGroupExists = "New user auto group has been used. ({0})";




    }
}
