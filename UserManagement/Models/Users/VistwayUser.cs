using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UserManagement.Models.Users
{
    internal class VistwayUser : User
    {
        private List<string> groups;

        public VistwayUser(string id, string firstName, string lastName, string accountType, string countryCode, List<string> groups, string emailAddress, bool ignoreLDAP)
            : base(id)
        {
            this.FirstName = firstName;
            this.LastName = lastName;
            this.AccountType = accountType;
            this.CountryCode = countryCode;
            this.EmailAddress = emailAddress;
            this.IgnoreLDAP = ignoreLDAP;
            this.groups = groups;
        }

        public string FirstName { get; private set; }

        public string LastName { get; private set; }

        public string AccountType { get; private set; }

        private string countryCode;

        public string CountryCode
        {
            get { return countryCode; }
            private set
            {
                if (value == null || value.Length < 3)
                {
                    throw new ArgumentOutOfRangeException();
                }
                countryCode = value;
            }
        }

        public string EmailAddress { get; private set; }

        public IReadOnlyCollection<string> Groups
        {
            get { return this.groups; }
        }

        public bool IgnoreLDAP { get; private set; }


        public void AddGroup(string groupName)
        {
            if (string.IsNullOrWhiteSpace(groupName))
            {
                throw new ArgumentNullException();
            }
            if (!this.groups.Contains(groupName))
            {
                this.groups.Add(groupName);
            }
        }

        public void RemoveGroup(string groupName)
        {
            if (string.IsNullOrWhiteSpace(groupName))
            {
                throw new ArgumentNullException();
            }
            if (!this.groups.Contains(groupName))
            {
                this.groups.Remove(groupName);
            }
        }

        public void RemoveGroupsByPrefix(string prefix)
        {
            if (string.IsNullOrWhiteSpace(prefix))
            {
                throw new ArgumentNullException();
            }

            this.groups.RemoveAll(g => g.StartsWith(prefix));
        }

        public void ClearAllGroups()
        {
            this.groups = new List<string>();
        }

    }
}