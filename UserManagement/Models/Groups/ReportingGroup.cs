namespace UserManagement.Models.Groups
{
    using System;
    using System.Collections.Generic;
    using System.Linq;

    internal class ReportingGroup : Group
    {
        private readonly List<SubGroup> subGroups;

        public ReportingGroup(string name)
            : base(name)
        {
            this.subGroups = new List<SubGroup>();
        }

        public IReadOnlyCollection<SubGroup> SubGroups
        {
            get { return this.subGroups; }
        }


        public void AddSubGroup(SubGroup group)
        {
            if (this.subGroups.Any(sg => sg.Name == group.Name))
            {
                throw new ArgumentOutOfRangeException($"SubGroup {group.Name} is already member of reporting group {base.Name}!");
            }

            this.subGroups.Add(group);
        }
    }
}
