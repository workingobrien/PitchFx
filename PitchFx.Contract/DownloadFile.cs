using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PitchFx.Contract
{
   public class DownloadFile
   {
      public enum TypeEnum
      {
         Pitcher = 0,
         Batter = 1
      }

      public enum InfoToStoreEnum
      {
         All = 0,
         Inning = 1,
         Players = 2,
      }

      public DownloadFile()
      {
         Batters = new List<string>();
         Pitchers = new List<string>();
      }

      public string GameXmlName { get; set; }
      public string InningXmlName { get; set; }
      public List<string> Batters { get; set; }
      public List<string> Pitchers { get; set; } 

   }
}
