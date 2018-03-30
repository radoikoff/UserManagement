using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UserManagement.Models
{
    class Country
    {
        private string name;
        private string code;


        public Country(string name, string code)
        {
            this.Name = name;
            this.Code = code;
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

        public string Code
        {
            get { return code; }
            private set
            {
                if (value == null || value.Length < 3)
                {
                    throw new ArgumentOutOfRangeException();
                }
                code = value;
            }
        }
    }
}
