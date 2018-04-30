namespace UserManagement.Models.Groups
{
    using System;

    internal abstract class Group
    {
        private string name;

        public Group(string name)
        {
            this.Name = name;
        }

        public string Name
        {
            get { return name; }
            private set
            {
                if (string.IsNullOrWhiteSpace(value))
                {
                    throw new ArgumentOutOfRangeException("Group name cannot be null or empty!");
                }
                name = value;
            }
        }
    }
}
