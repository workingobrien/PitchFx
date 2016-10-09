using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceProcess;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using COB.LogWrapper;
using log4net.Config;
using PitchFx.Implement.Service;

[assembly: XmlConfigurator(Watch = true)]
namespace PitchFx.Load.ServiceHost
{
   class Program
   {
      static void Main(string[] args)
      {
         AppDomain.CurrentDomain.UnhandledException += CurrentDomainOnUnhandledException;
         Logger.Log.InfoFormat(" ");
         Logger.Log.InfoFormat("#############");
         Logger.Log.InfoFormat("Starting PitchFx Loader as Service. Date: {0}", DateTime.Today);

         var service = new PitchFxServiceGateway();
         service.StartWcfService();
         var servicesToRun = new ServiceBase[] {service};
         ServiceBase.Run(servicesToRun);
      }

      private static void CurrentDomainOnUnhandledException(object sender, UnhandledExceptionEventArgs args)
      {
         var ex = (Exception)args.ExceptionObject;
         Logger.LogException(ex);
      }
   }
}
