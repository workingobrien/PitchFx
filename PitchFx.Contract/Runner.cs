using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace PitchFx.Contract
{
   [XmlRoot("runner")]
   public class Runner
   {
      public Runner()
      {
         
      }

      public override string ToString()
      {
         var str = string.Format("Runner Guid: {0}, GameId: {1}",RunnerGuid,GameId);
         return str;
      }

      public string RunnerGuid { get; set; }

      public string AtBatGuid { get { return Ab != null ? Ab.AtBatGuid : string.Empty; } }

      public long GamePrimaryKey
      {
         get { return Ab != null ? Ab.GamePrimaryKey : 0; }
      }

      public string GameId
      {
         get { return Ab != null ? Ab.Gid : string.Empty; }
      }

      public AtBat Ab { get; set; }

      /// <summary>
      /// Player Id
      /// </summary>
      [XmlAttribute("id")]
      public long Id { get; set; }

      [XmlAttribute("start")]
      public string Start { get; set; }

      [XmlAttribute("end")]
      public string End { get; set; }
      
      [XmlAttribute("event")]
      public string Event { get; set; }

      [XmlAttribute("event_num")]
      public string EventNum { get; set; }

      [XmlAttribute("score")]
      public string Score { get; set; }

      [XmlAttribute("rbi")]
      public string Rbi { get; set; }

      [XmlAttribute("earned")]
      public string Earned { get; set; }

      public bool IsRunnerSaved { get; set; }

      public static string GenerateActionGuid()
      {
         var guid = Guid.NewGuid();
         return "COB-RUNNER-" + guid.ToString();
      }

   }
}
