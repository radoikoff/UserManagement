using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UserManagement
{
    public class User
    {
        public User()
        {
            Groups = new HashSet<string>();
        }

        public User(string account, string firstName, string lastName, string email, string country, HashSet<string> groups)
        {
            Account = account;
            FirstName = firstName;
            LastName = lastName;
            Email = email;
            CountryCode = country;
            Groups = groups;
        }

        public string Account { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string Email { get; set; }
        public string CountryCode { get; set; }
        public HashSet<string> Groups { get; set; }
    }
}