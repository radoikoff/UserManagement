namespace UserManagement.Models
{
    using System;

    internal class Country
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
            private set
            {
                if (string.IsNullOrWhiteSpace(value))
                {
                    throw new ArgumentOutOfRangeException("Country name cannot be null or empty!");
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
                    throw new ArgumentOutOfRangeException("Country name cannot be less than 3 symbols!");
                }
                code = value;
            }
        }
    }
}
