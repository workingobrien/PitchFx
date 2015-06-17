using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace PitchFx.Contract
{
   /// <summary>
   /// Not part of an at node.. but part of the inning
   /// </summary>
   [XmlRoot("action")]
   public class Action
   {
      [XmlAttribute("play_guid")]
      public string ActionGuid { get; set; }

      public long GamePrimaryKey
      {
         get { return Game != null ? Game.GamePrimaryKey : 0; }
      }

      public string GameId
      {
         get { return Game != null ? Game.Gid : string.Empty; }
         
      }

      public Game Game { get; set; }

      [XmlAttribute("b")]
      public int Balls { get; set; }

      [XmlAttribute("s")]
      public int Strikes { get; set; }

      [XmlAttribute("o")]
      public int Outs { get; set; }

      [XmlAttribute("event")]
      public string Event { get; set; }

      [XmlAttribute("des")]
      public string Description { get; set; }

      [XmlAttribute("tfs")]
      public string Tfs { get; set; }

      [XmlAttribute("tfs_zulu")]
      public string TfsZluStr { get; set; }

      [XmlAttribute("player")]
      public long Player { get; set; }

      [XmlAttribute("pitch")]
      public long Pitch { get; set; }

      [XmlAttribute("event_num")]
      public int EventNumber { get; set; }
      
      [XmlAttribute("home_team_runs")]
      public int HomeTeamRuns { get; set; }

      [XmlAttribute("away_team_runs")]
      public int AwayTeamRuns { get; set; }

      public int Inning { get; set; }

      /// <summary>
      /// 'top' or 'bottom'
      /// </summary>
      public string InningType { get; set; }

      public bool IsActionSave { get; set; }

      public static string GenerateActionGuid()
      {
         var guid = Guid.NewGuid();
         return "COB-ACTION-" + guid.ToString();
      }


   }
}
