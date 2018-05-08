using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UserManagement.Models;
using UserManagement.Models.Groups;
using UserManagement.Models.Users;
using UserManagement.StaticData;
using UserManagement.IO;

namespace UserManagement.Data
{
    public class AppData
    {
        private ILogger logger;

        public AppData(ILogger logger)
        {
            this.logger = logger;
            TryReadData();
        }

        public bool IsValid { get; set; }
        public List<VistwayUser> VistwayUsers { get; private set; }
        public List<EnterProjUser> EnterProjUsers { get; private set; }
        public List<ReportingGroup> Groups { get; private set; }
        public List<Country> Countries { get; private set; }
        public virtual IEnumerable<string> ReportingGroups => this.Groups.Select(g => g.Name);
        public virtual IEnumerable<string> SubGroups => this.Groups.SelectMany(g => g.SubGroups).Select(sg => sg.Name);
        public virtual IEnumerable<string> SpecialSubGroups => this.Groups.SelectMany(g => g.SubGroups).Where(sg => sg.IsSpecial).Select(sg => sg.Name);


        public void SaveResultFile(List<VistwayUser> users)
        {
            try
            {
                DataMapper.SaveResultFile(users);
            }
            catch (Exception ex)
            {
                this.logger.DisplayMessage(MsgType.ERROR, ex.Message + Environment.NewLine + ex.InnerException.Message);
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
