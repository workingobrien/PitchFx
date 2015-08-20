using System;
using System.Collections.Generic;
using System.Data;
using System.Dynamic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Serialization;
using COB.LogWrapper;

namespace PitchFx.Contract
{
   [XmlRoot("atbat")]
   public sealed class AtBat
   {
      #region Col Names
      private const string GamePrimaryKeyCol = "game_primaryKey";
      private const string GidCol = "gid";
      private const string AtBatGuidCol = "ab_guid";
      private const string NumCol = "num";
      private const string BallsCol = "balls";
      private const string StrikesCol = "strikes";
      private const string OutsCol = "outs";
      private const string StartTfsCol = "start_tfs";
      private const string StartTfsZuluCol = "start_tfs_zulu";
      private const string BatterCol = "batter";
      private const string StandCol = "stand";
      private const string BHeightCol = "b_height";
      private const string PitcherCol = "pitcher";
      private const string PThrowsCol = "P_throws";
      private const string DesCol = "des";
      private const string EventNumCol = "event_num";
      private const string EventCol = "event";
      private const string HomeTeamRunsCol = "home_team_runs";
      private const string AwayTeamRunsCol = "away_team_runs";
      #endregion

      public AtBat()
      {

      }

      public AtBat(DataRow row)
      {
         try
         {
            GamePrimaryKey = Convert.ToInt64(row[GamePrimaryKeyCol].ToString());
            Gid = row[GidCol].ToString();
            AtBatGuid = row[AtBatGuidCol].ToString();
            Num = Convert.ToInt32(row[NumCol].ToString());
            Balls = Convert.ToInt32(row[BallsCol].ToString());
            Strikes = Convert.ToInt32(row[StrikesCol].ToString());
            Outs = Convert.ToInt32(row[OutsCol].ToString());
            StartTfs = row[StartTfsCol].ToString();
            StartTfsZulu = row[StartTfsZuluCol].ToString();
            Batter = Convert.ToInt64(row[BatterCol].ToString());
            Stand = row[StandCol].ToString();
            BHeight = row[BHeightCol].ToString();
            Pitcher = Convert.ToInt64(row[PitcherCol].ToString());
            PThrows = row[PThrowsCol].ToString();
            Des = row[DesCol].ToString();
            EventNum = row[EventNumCol].ToString();
            Event = row[EventCol].ToString();
            HomeTeamRuns = row[HomeTeamRunsCol].ToString();
            AwayTeamRuns = row[AwayTeamRunsCol].ToString();

            Pitches = new List<Pitch>();
            Runners = new List<Runner>();

            IsDeserializedFromDb = true;
         }
         catch (Exception ex)
         {
            Logger.Log.ErrorFormat("Could not serialize row into atbat object: {0}", row.ToString());
            Logger.LogException(ex);
         }
      }

      public override string ToString()
      {
         var str = string.Format("AtBatGuid: {0}, Num: {1}, Des: {3}, Gid: {4}", AtBatGuid, Num, Des, Gid);
         return str;
      }


      public long GamePrimaryKey { get; set; }
      public string Gid { get; set; }

      public List<Pitch> Pitches { get; set; }

      public string AtBatGuid { get; set; }

      [XmlAttribute("num")]
      public int Num { get; set; }

      [XmlAttribute("b")]
      public int Balls { get; set; }

      [XmlAttribute("s")]
      public int Strikes { get; set; }

      [XmlAttribute("o")]
      public int Outs { get; set; }

      [XmlAttribute("start_tfs")]
      public string StartTfs { get; set; }

      [XmlAttribute("start_tfs_zulu")]
      public string StartTfsZulu { get; set; }

      [XmlAttribute("batter")]
      public long Batter { get; set; }

      [XmlAttribute("stand")]
      public string Stand { get; set; }

      [XmlAttribute("b_height")]
      public string BHeight { get; set; }

      [XmlAttribute("pitcher")]
      public long Pitcher { get; set; }

      [XmlAttribute("p_throws")]
      public string PThrows { get; set; }

      [XmlAttribute("des")]
      public string Des { get; set; }

      [XmlAttribute("event_num")] 
      public string EventNum { get; set; }

      [XmlAttribute("event")]
      public string Event { get; set; }

      [XmlAttribute("home_team_runs")]
      public string HomeTeamRuns { get; set; }

      [XmlAttribute("away_team_runs")]
      public string AwayTeamRuns { get; set; }

      public int Inning { get; set; }

      /// <summary>
      /// 'top' or 'bottom'
      /// </summary>
      public string InningType { get; set; }

      public List<Runner> Runners { get; set; }

      public XmlNode AtBatNode { get; set; }

      public bool IsAtBatSaved { get; set; }

      public bool IsDeserializedFromDb { get; set; }

      public static string GenerateAbGuid()
      {
         var guid = Guid.NewGuid();
         return "COB-AB-" + guid.ToString();
      }

   }
}
