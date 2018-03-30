using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UserManagement.Models
{
    class Group
    {
        private string name;
        private List<string> subGroups;

        public Group(string name)
        {
            this.Name = name;
            this.subGroups = new List<string>();
        }

        public string Name
        {
            get { return name; }
            set
            {
                if (string.IsNullOrWhiteSpace(value))
                {
                    throw new ArgumentOutOfRangeException();
                }
                name = value;
            }
        }

        public IReadOnlyCollection<string> SubGroups
        {
            get { return this.subGroups; }
        }


        public void AddSubGroup(string name)
        {
            if (this.subGroups.Contains(name))
            {
                throw new ArgumentOutOfRangeException();
            }
            this.subGroups.Add(name);
        }
    }
}
