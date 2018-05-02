namespace UserManagement.Models.Groups
{

    public class SubGroup : Group
    {
        public SubGroup(string name, bool isSpecial = false)
            : base(name)
        {
            this.IsSpecial = isSpecial;
        }

        public bool IsSpecial { get; private set; }
    }
}
