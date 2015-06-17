using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using COB.LogWrapper;
using Db.Utilities;
using log4net;
using log4net.Config;
using PitchFxDataImporter;

[assembly: XmlConfigurator(Watch = true)]
namespace TestConnector
{
   /// <summary>
   /// Simple console applicaiton that does a few tasks...
   /// 1. Downlaods all the xml files from the mlb site
   /// 2. Saves the xml files locally
   /// 3. Read's the xml files to the database
   /// 
   /// Note: I would run 1&2 during. Then stop application, 
   /// comment 'GatherAllXmlFilenames' and 'SaveXmlFilesToDisk' out and
   /// then run 'ReadXmlFilesToDatabase'
   /// 
   /// </summary>
   class Program
   {
      static void Main(string[] args)
      {
         try
         {
            Logger.Log.InfoFormat("Starting up PitchFx test connection...");

            Importer.Instance.GatherAllXmlFilenames();
            
            Importer.Instance.SaveXmlFilesToDisk();

            Importer.Instance.ReadXmlFilesToDatabase();
            Logger.Log.InfoFormat("Finished!.....");

         }
         catch (Exception ex)
         {
            Logger.LogException(ex);
         }
      }
   }
}
