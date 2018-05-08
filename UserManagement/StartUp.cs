using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UserManagement.Core;
using UserManagement.Data;
using UserManagement.IO;

namespace UserManagement
{
    class StartUp
    {
        static void Main(string[] args)
        {
            ILogger logger = new Logger();
            AppData data = new AppData(logger);


            Controller controller = new Controller(data, logger);

            controller.Execute();
        }
    }
}
