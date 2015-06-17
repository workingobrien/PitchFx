using System;
using System.Collections.Generic;
using System.Data;
using System.Globalization;
using System.Linq;
using System.Runtime.InteropServices.ComTypes;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;
using COB.LogWrapper;

namespace PitchFx.Contract
{
   [XmlRoot("game")]
   public sealed class Game
   {
      public const string GamePrimarykeyCol = "game_primarykey";
      public const string GidCol = "gid";
      private const string HomeTeamCol = "home_team";
      private const string AwayTeamCol = "away_team";
      private const string GameDateCol = "game_date";
      private const string HomeCodeCol = "home_code";
      private const string AwayCodeCol = "away_code";
      private const string HomeAbbrevCol = "home_abbrev";
      private const string AwayAbbrevCol = "away_abbrev";
      private const string HomeIdCol = "home_id";
      private const string AwayIdCol = "away_id";

      public Game()
      {
         
      }

      public Game(DataRow row)
      {
         try
         {
            var gamePrimarykey = Convert.ToInt64(row[GamePrimarykeyCol].ToString());
            var gid = row[GidCol].ToString();
            var homeTeam = row[HomeTeamCol].ToString();
            var awayTeam = row[AwayTeamCol].ToString();
            
            //**\/ probably don't have to do this..
            const string format = "yyyyMMdd";
            var gameDateStr = row[GameDateCol].ToString();
            var gidArr = gid.Split('_');
            if (gidArr.Length > 4)
               gameDateStr = gidArr[1] + gidArr[2] + gidArr[3];

            var gameDate = DateTime.MinValue;
            DateTime.TryParseExact(gameDateStr, format, null, DateTimeStyles.None, out gameDate);
            GameDate = gameDate;

            var homeCode = row[HomeCodeCol].ToString();
            var awayCode = row[AwayCodeCol].ToString();
            var homeAbbrev = row[HomeAbbrevCol].ToString();
            var awayAbbrev = row[AwayAbbrevCol].ToString();

            var hIdStr = row[HomeIdCol].ToString();
            var hId = 0;
            int.TryParse(hIdStr, out hId);

            var aIdStr = row[AwayIdCol].ToString();
            var aId = 0;
            int.TryParse(aIdStr, out aId);
            //**
            GamePrimaryKey = gamePrimarykey;
            Gid = gid;
            Type = "R";
            Home = new Team("home", homeCode, homeAbbrev, hId,homeTeam);
            Away = new Team("away", awayCode, awayAbbrev, aId,awayTeam);

            AtBats = new List<AtBat>();
            Actions = new List<Action>();

            IsDeserializedFromDb = true;
         }
         catch (Exception ex)
         {
            Logger.Log.ErrorFormat("Could not serialize row into game object: {0}",row.ToString());
            Logger.LogException(ex);
         }
      }

      [XmlAttribute("game_pk")]
      public long GamePrimaryKey { get; set; }
      
      public string Gid { get; set; }

      private DateTime _gameDate = DateTime.MinValue;
      public DateTime GameDate
      {
         get
         {
            if (_gameDate == DateTime.MinValue)
            {
               var gidArr = Gid.Split('_');
               if (gidArr.Length > 4)
               {
                  var dtStr = gidArr[1] + gidArr[2] + gidArr[3];

                  var format = "yyyyMMdd";
                  if (!string.IsNullOrEmpty(GameTimeEt))
                  {
                     format = format + "hh:mm tt";
                     dtStr = dtStr + GameTimeEt;
                  }
                  DateTime.TryParseExact(dtStr, format, null, DateTimeStyles.AssumeLocal, out _gameDate);

                  if (_gameDate == DateTime.MinValue)
                  {
                     format = "yyyyMMdd";
                     dtStr = gidArr[1] + gidArr[2] + gidArr[3];
                     DateTime.TryParseExact(dtStr, format, null, DateTimeStyles.AssumeLocal, out _gameDate);
                  }
               }
            }
            return _gameDate;
         }
         set { _gameDate = value; }
      }

      /// <summary>
      /// 'S' is for spring training... we can ignore. Only take 'R', 'A' is allstar
      /// </summary>
      [XmlAttribute("type")]
      public string Type { get; set; }

      [XmlAttribute("game_time_et")]
      public string GameTimeEt { get; set; }

      public Team Home;
      public Team Away;

      public List<AtBat> AtBats { get; set; }

      public List<Pitch> Pitches
      {
         get { return AtBats.SelectMany(ab => ab.Pitches).ToList(); }
      }

      public List<Runner> Runners
      {
         get { return AtBats.SelectMany(ab => ab.Runners).ToList(); }
      } 
      public List<Action> Actions { get; set; } 
      
      public bool IsGameSaved { get; set; }
      public bool IsDeserializedFromDb { get; set; }

      public override string ToString()
      {
         var str = string.Format("{0}, pk: {1}. Type: {2}", Gid, GamePrimaryKey, Type);
         return str;
      }
   }
}
