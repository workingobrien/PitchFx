using System;
using System.ServiceModel;
using System.ServiceProcess;
using System.Text;
using COB.LogWrapper;

namespace PitchFx.Implement.Service
{
   public class PitchFxServiceGateway : ServiceBase
   {
      public void StartWcfService()
      {
         try
         {
            var host = new ServiceHost(typeof (PitchFxService));
            host.Opened += HostOnOpened;
            host.Open();
         }
         catch (Exception ex)
         {
            Logger.LogException(ex);
         }
      }

      private void HostOnOpened(object sender, EventArgs eventArgs)
      {
         var host = (ServiceHost) sender;
         var detailBuilder = new StringBuilder();
         foreach (var baseAddress in host.BaseAddresses)
         {
            detailBuilder.AppendFormat("\r\nBase Address: {0}", baseAddress);
         }

         foreach (var endpoint in host.Description.Endpoints)
         {
            var address = endpoint.Address.ToString();
            var bindingName = endpoint.Binding.Name;
            detailBuilder.AppendFormat("\r\nEndpoint: ({0}): {1}", bindingName, address);
         }
         Logger.Log.InfoFormat(detailBuilder.ToString());
      }

      protected override void OnStart(string[] args)
      {
         Logger.Log.InfoFormat("PitchFx Loader Service 'OnStart' called");

      }

      protected override void OnStop()
      {
         Logger.Log.InfoFormat("Stopping PitchFx Loader Service");
      }


      private void InitializeComponent()
      {
         this.ServiceName = "PitchFxfLoaderService";
      }

   }
}
