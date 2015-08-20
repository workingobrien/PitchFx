using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PitchFxDataImporter
{
    public static class Constants
    {
       public static List<string> BaseballYears;
       public static string BaseSaveDir;

       public const int EndMonth = 12;
       public const int EndDay = 31;
       public const int TopInningIndex = 0;
       public const int BottomInningIndex = 1;
        
       public const string RealGameType = "R";
       public const string YearPrefex = "year_";
       public const string MonthPrefix = "month_";
       public const string DayPrefex = "day_";
       public const string GameXmlUrl = "game.xml";
       public const string AllInningsUrl = "inning_all.xml";
       public const string InningDirectory = "inning/";
       public const string InningDirectoryName = "inning";
       public const string BattersDirectory = "batters/";
       public const string PitchersDirectory = "pitchers/";
       public const string BattersDirectoryName = "batters";
       public const string PitchersDirectoryName = "pitchers";
       public const string InningNodeName = "inning";
       public const string GameNodeName = "game";
       public const string PlayerNodeName = "player";
       public const string TopNode = "top";
       public const string BottomNode = "bottom"; 
       public const string PitchNode = "pitch";
       public const string RunnerNode = "runner";
       public const string MlbLink = @"http://gd2.mlb.com/components/game/mlb/";

       static Constants()
       {
          BaseballYears = new List<string>();
          //BaseballYears.Add("year_2010/");
          //BaseballYears.Add("year_2011/");
          //BaseballYears.Add("year_2012/");
          //BaseballYears.Add("year_2013/");
          //BaseballYears.Add("year_2014/");
          BaseballYears.Add("year_2015/");

          BaseSaveDir = ConfigurationManager.AppSettings["baseSaveDirectory"];

       }

    }
}
