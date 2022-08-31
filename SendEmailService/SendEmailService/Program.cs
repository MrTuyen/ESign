using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceProcess;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace SendEmailService
{
    static class Program
    {
        static void Main()
        {
#if DEBUG
            Service myService = new Service();
            myService.OnDebug();
            Thread.Sleep(Timeout.Infinite);

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
