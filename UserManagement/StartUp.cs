using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UserManagement.Core;
using UserManagement.Data;

namespace UserManagement
{
    class StartUp
    {
        static void Main(string[] args)
        {
            Logger logger = new Logger();
            AppData data = new AppData(logger);


            Controller controller = new Controller(data, logger);

            controller.Execute();
        }
    }
}
