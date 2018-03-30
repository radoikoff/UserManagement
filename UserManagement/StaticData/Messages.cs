using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UserManagement
{
    public static class Messages
    {
        public const string UserAddedToAutoGroup = "{0} is added to auto group {1}";

        public const string UserAutoGroupUpdated = "{0} auto group is changed to {1}";

        public const string UserAssignedToMoreThanOneAutoGroups = "{0} was assigned to more than one auto group";

        public const string UserRemovedFromManualGroup = "{0} removed from a manual group {1}";

        public const string UserHasNoGroupAssignment = "{0} is not assigned to any group and curently does not have eP skill";

        public const string UserAssignedToMoreThanOneManualGroup = "ACTION: {0} was assigned to more than one manual group";

        public const string UserAddedToCountryGroup = "{0} is added to country group {1}";

        public const string UserCountryGroupUpdated = "{0} country group is changed to {1}";

        public const string UserAssignedToMoreThanOneCountryGroups = "{0} was assigned to more than one country group";

        public const string NotExisitingCountry = "WARNING: {0} - country code {1} has no country name";

        public const string NoUsersToUpdate = "There are no users to update!";

        public const string NewAutoUserGroupExists = "WARNING: A new auto user groups has been used. ({0})";

        public const string UserAssignedToAnAutoGroupsWithoutBeingInEP = "{0} is member of {1} group and curently does not have eP skill";
    }
}
