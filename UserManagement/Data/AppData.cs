using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UserManagement.Models;
using UserManagement.Models.Users;

namespace UserManagement.Data
{
    class AppData
    {
        private Logger logger;

        public AppData(Logger logger)
        {
            this.logger = logger;
            TryReadData();
        }

        public bool IsValid { get; set; }
        public List<VistwayUser> VistwayUsers { get; set; }
        public List<EnterProjUser> EnterProjUsers { get; set; }
        public List<Group> Groups { get; set; }
        public List<Country> Countries { get; set; }
        public IReadOnlyCollection<string> SpecialUsersIds { get; set; }
        public IReadOnlyCollection<string> PriorityGroupsNames { get; set; }

        public void SaveChages()
        {
        }

        public void SaveResultFile(List<VistwayUser> users)
        {
            try
            {
                DataMapper.SaveResultFile(users);
            }
            catch (Exception ex)
            {
                this.logger.DisplayMessage(MsgType.ERROR, ex.Message);
            }
            
        }

        private void TryReadData()
        {
            try
            {
                this.VistwayUsers = DataMapper.LoadVistwayUsersFromCsv();
                this.EnterProjUsers = DataMapper.LoadEnterProjUsersFromCsv();
                this.Groups = DataMapper.LoadGroups();
                this.Countries = DataMapper.LoadCountries();
                this.SpecialUsersIds = DataMapper.LoadSpecialUsersFromCsv();
                this.PriorityGroupsNames = DataMapper.LoadPriorityGroupsFromCsv();
                this.IsValid = true;
            }
            catch (Exception ex)
            {
                this.logger.DisplayMessage(MsgType.ERROR, "DB is not valid!" + Environment.NewLine + ex.Message);
                this.IsValid = false;
            }
        }
    }
}
