using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Security.AccessControl;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Xml;
using System.Xml.Serialization;
using COB.LogWrapper;
using Db.Utilities;
using log4net;
using PitchFx.Contract;
using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using OpenQA.Selenium.Remote;

namespace PitchFxDataImporter
{
   public sealed partial class Importer
   {
      private static volatile Importer _instance;
      private static readonly object _syncRoot = new object();

      public ConcurrentDictionary<long, Game> MasterGamesInfo = new ConcurrentDictionary<long, Game>();
      public List<DownloadFile> MasterDopwnloadFileList = new List<DownloadFile>();

      public static Importer Instance
      {
         get
         {
            if (_instance == null)
            {
               lock (_syncRoot)
               {
                  _instance = new Importer();
               }
            }
            return _instance;
         }
      }

      /// <summary>
      /// Reads files beginning here: C:\game\mlb
      /// </summary>
      public void ReadXmlFilesToDatabase()
      {
         try
         {
            Logger.Log.InfoFormat("#### \\/ #####");
            Logger.Log.InfoFormat("Loading Baseball objects into memory before database save....");
            var baseDirInfo = new DirectoryInfo(Constants.BaseSaveDir);
            IEnumerable<DirectoryInfo> dis = baseDirInfo.EnumerateDirectories();
            foreach (var di in dis)
            {
               LookThroughDirectory(di);
            }
            SendToDatabase();
         }
         catch (Exception ex)
         {
            Logger.LogException(ex);
         }
      }

      public void LookThroughDirectory(DirectoryInfo di)
      {
         var dis = di.EnumerateDirectories();
         foreach (var dInfo in dis)
         {
            if (dInfo.Name.StartsWith("gid_"))
            {
               var gameFile = dInfo.GetFiles();
               if (gameFile.Length == 1)
               {
                  try
                  {
                     var allInnings = dInfo.GetDirectories()[0].GetFiles()[0];
                     ProcessFileInfos(gameFile[0], allInnings);
                  }
                  catch (Exception ex)
                  {
                     Logger.LogException(ex);
                  }
               }
               else
                  Logger.Log.WarnFormat("{0} does not have game.xml file", dInfo.Name);

            }
            else
            {
               LookThroughDirectory(dInfo);
            }
         }
      }

      public ConcurrentDictionary<long, Game> LinkDeserializeAtBats(ConcurrentDictionary<long, Game> games, List<AtBat> atBats,
                                                            ref long minGamePrimaryKey, ref long maxGamePrimaryKey)//, List<Pitch> pitches)
      {
         try
         {
            minGamePrimaryKey = games.Keys.OrderBy(x => x).FirstOrDefault();
            maxGamePrimaryKey = games.Keys.OrderByDescending(x => x).FirstOrDefault();

            var groupedAtBats =
               atBats.Where(ab => ab.IsDeserializedFromDb)
                  .GroupBy(ab => ab.GamePrimaryKey)
                  .ToDictionary(k => k.Key, v => v.ToList());


            foreach (var kvp in groupedAtBats)
            {
               var gPk = kvp.Key;
               var abs = kvp.Value;
               Game game = null;
               if (games.TryGetValue(gPk, out game))
               {
                  game.AtBats.AddRange(abs);
               }
            }
            return games;
         }
         catch (Exception ex)
         {
            Logger.LogException(ex);
            return null;
         }
      }

      public ConcurrentDictionary<long, Game> LinkDeserializedPitches(ConcurrentDictionary<long, Game> games, List<Pitch> pitches)
      {
         var groupedPitches =
         pitches.Where(p => p.IsDeserializedFromDb)
         .GroupBy(p => p.AtBatGuid)
         .ToDictionary(k => k.Key, v => v.ToList());

         var atBats = games.Values.SelectMany(g => g.AtBats);
         foreach (var atBat in atBats)
         {
            if (groupedPitches.ContainsKey(atBat.AtBatGuid))
               atBat.Pitches.AddRange(groupedPitches[atBat.AtBatGuid]);
         }

         return games;
      }

      /// <summary>
      /// I added significant error checking to make sure every xml node
      /// got written to the database. As a result, I would run the console each
      /// 1 time per 'year' to load the database... 
      /// 
      /// The commented out code is to make sure you have the option to remove data from a specific year
      /// when reading in data...
      /// 
      /// </summary>
      private void SendToDatabase()
      {
         try
         {
            //var games2010PrimaryKey = MasterGamesInfo.Values.Where(g => g.Gid.StartsWith("gid_2010")).ToList();
            //var games2011PrimaryKey = MasterGamesInfo.Values.Where(g => g.Gid.StartsWith("gid_2011")).ToList();
            //var games2012PrimaryKey = MasterGamesInfo.Values.Where(g => g.Gid.StartsWith("gid_2012")).ToList();
            //var games2013PrimaryKey = MasterGamesInfo.Values.Where(g => g.Gid.StartsWith("gid_2013")).ToList();
            //var games2014PrimaryKey = MasterGamesInfo.Values.Where(g => g.Gid.StartsWith("gid_2014")).ToList();
            //var games2015PrimaryKey = MasterGamesInfo.Values.Where(g => g.Gid.StartsWith("gid_2015")).ToList();

            //foreach (var game in games2010PrimaryKey)
            //{
            //   Game gameToRemove;
            //   MasterGamesInfo.TryRemove(game.GamePrimaryKey, out gameToRemove);
            //}

            ////foreach (var game in games2011PrimaryKey)
            //{
            //   Game gameToRemove;
            //   MasterGamesInfo.TryRemove(game.GamePrimaryKey, out gameToRemove);
            //}


            //foreach (var game in games2012PrimaryKey)
            //{
            //   Game gameToRemove;
            //   MasterGamesInfo.TryRemove(game.GamePrimaryKey, out gameToRemove);
            //}


            //foreach (var game in games2013PrimaryKey)
            //{
            //   Game gameToRemove;
            //   MasterGamesInfo.TryRemove(game.GamePrimaryKey, out gameToRemove);
            //}


            //foreach (var game in games2014PrimaryKey)
            //{
            //   Game gameToRemove;
            //   MasterGamesInfo.TryRemove(game.GamePrimaryKey, out gameToRemove);
            //}

            //foreach (var game in games2015PrimaryKey)
            //{
            //   Game gameToRemove;
            //   MasterGamesInfo.TryRemove(game.GamePrimaryKey, out gameToRemove);
            //}

            Writer.UpdateTableWithGameInfo(MasterGamesInfo);
            RemoveWrittenRecordsFromMemory();


         }
         catch (Exception ex)
         {
            Logger.LogException(ex);
         }
      }


   }
}
