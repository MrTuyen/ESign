using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceProcess;
using System.Text;
using System.Threading.Tasks;

namespace ES_UpContacts
{
    static class Program
    {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        static void Main()
        {
#if DEBUG
                        //While debugging this section is used.
                        Service myService = new Service();
                        myService.OnDebug();
                        System.Threading.Thread.Sleep(System.Threading.Timeout.Infinite);

#else
            ServiceBase[] ServicesToRun;
            ServicesToRun = new ServiceBase[]
            {
                new Service()
            };
            ServiceBase.Run(ServicesToRun);
#endif
        }
    }
}
