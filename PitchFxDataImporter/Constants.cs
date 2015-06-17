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
       public const string MonthPrefix = "month_";
       public const string DayPrefex = "day_";
       public const string GameXmlUrl = "game.xml";
       public const string AllInningsUrl = "inning_all.xml";
       public const string InningDirectory = "inning/";
       public const string InningNodeName = "inning";
       public const string GameNodeName = "game";
       public const string TopNode = "top";
       public const string BottomNode = "bottom";
       public const string PitchNode = "pitch";
       public const string RunnerNode = "runner";
       

       static Constants()
       {
          BaseballYears = new List<string>();
          BaseballYears.Add("year_2010/");
          BaseballYears.Add("year_2011/");
          BaseballYears.Add("year_2012/");
          BaseballYears.Add("year_2013/");
          //BaseballYears.Add("year_2014/");
          BaseballYears.Add("year_2015/");

          BaseSaveDir = ConfigurationManager.AppSettings["baseSaveDirectory"];

       }

    }
}
