using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using COB.LogWrapper;
using log4net.Config;
using PitchFx.Implement.Service;

[assembly: XmlConfigurator(Watch = true)]
namespace PitchFx.Load.ConsoleApp
{
   class Program
   {
      static void Main(string[] args)
      {
         AppDomain.CurrentDomain.UnhandledException += CurrentDomainOnUnhandledException;
         Logger.Log.InfoFormat(" ");
         Logger.Log.InfoFormat("#############");
         Logger.Log.InfoFormat("Starting PitchFx Loader from Console. Date: {0}",DateTime.Today);

         var service = new PitchFxServiceGateway();
         service.StartWcfService();
         Thread.Sleep(Timeout.Infinite);
      }

      private static void CurrentDomainOnUnhandledException(object sender, UnhandledExceptionEventArgs args)
      {
         var ex = (Exception) args.ExceptionObject;
         Logger.LogException(ex);
      }
   }
}
