using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceModel;
using System.Text;
using System.Threading.Tasks;
using COB.LogWrapper;
using log4net;
using PitchFx.Contract;
using PitchFx.Contract.Interface;
using PitchFxDataImporter;

namespace PitchFx.Implement.Service
{
   [ServiceBehavior(InstanceContextMode = InstanceContextMode.PerSession, ConcurrencyMode = ConcurrencyMode.Multiple)]
   public class PitchFxService : IPitchFxService
    {
      public void LoadYesterdaysPitchFxData()
      {
         var yesterday = DateTime.Today.AddDays(-2);
         Logger.Log.InfoFormat("");
         Logger.Log.InfoFormat("## \\/ ##");
         Logger.Log.InfoFormat("Load Yesterdays PitchFx data for date: {0}",yesterday);
         LoadPitchFxData(yesterday,yesterday);
      }

      public void LoadPitchFxDataByDate(DateTime date)
      {
         Logger.Log.InfoFormat("");
         Logger.Log.InfoFormat("## \\/ ##");
         Logger.Log.InfoFormat("Load PitchFx data for date: {0}", date);
         LoadPitchFxData(date, date);
      }

      public void LoadPitchFxDataByDateRange(DateTime since, DateTime until)
      {
         Logger.Log.InfoFormat("");
         Logger.Log.InfoFormat("## \\/ ##");
         Logger.Log.InfoFormat("Load Pitch Fx data for range - since: {0}, until: {1}",since,until);
         LoadPitchFxData(since,until);
      }

      private void LoadPitchFxData(DateTime since, DateTime until)
      {
         Importer.Instance.Init(DownloadFile.InfoToStoreEnum.All);
         Importer.Instance.LoadPitchFxData(since, until);
      }

    }
}
