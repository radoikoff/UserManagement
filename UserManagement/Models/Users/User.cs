using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UserManagement.Models.Users
{
    internal abstract class User
    {
        private string id;

        public User(string id)
        {
            this.Id = id;
        }

        public string Id
        {
            get { return id; }
            set
            {
                if (string.IsNullOrWhiteSpace(value))
                {
                    throw new ArgumentNullException();
                }
                id = value;
            }
        }

    }
}
