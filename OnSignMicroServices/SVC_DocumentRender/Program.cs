using System.ServiceProcess;

namespace SVC_DocumentRender
{
    static class Program
    {
        static void Main()
        {
#if DEBUG
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
