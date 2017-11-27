using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UserManagement
{
    public static class Messages
    {
        public const string UserAddedToLevelOneGroup = "User is added to a sub-group.";

        public const string UserLevelOneGroupUpdated = "User sub-group is changed.";

        public const string UserAssignedToMoreThanOneLevelOneGroups = "User is assigned to more than one sub-group.";

        public const string UserRemovedFromManualGroup = "User removed from manual user group as he/she is already member of a sub-group.";

        public const string UserHasNoGroupAssignment = "User not assigned to any group and curently does not have eP skill";

        public const string UserAssignedToMoreThanOneManualGroup = "User is assigned to more than one manual group.";

        public const string DuplicateUserFound = "Duplicate user found";
    }
}
