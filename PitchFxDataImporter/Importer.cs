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

      private DownloadFile.InfoToStoreEnum _infoToStore = DownloadFile.InfoToStoreEnum.All;
      public DownloadFile.InfoToStoreEnum InfoToStore { get { return _infoToStore; }}

      private List<string> _allGids;
      private List<string> _allYears; 

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

      public void Init(DownloadFile.InfoToStoreEnum infoToStore)
      {
         _infoToStore = infoToStore;
         Getter.LoadGameCache();
         Thread.Sleep(5000);
      }

      public void LoadPitchFxData(DateTime since, DateTime until)
      {
         GatherXmlFilenames(since,until);
         SaveAndReadToDb();
      }

      public void BulkLoadFromTestConnector()
      {
         //GatherAllXmlFilenames();
         SaveAndReadToDb();
      }

      private void SaveAndReadToDb()
      {
         //SaveXmlFilesToDisk();
         ReadXmlFilesToDatabase();
      }

      /// <summary>
      /// Reads files beginning here: C:\game\mlb
      /// </summary>
      private void ReadXmlFilesToDatabase()
      {
         try
         {
            _allGids = new List<string>();
            //foreach  (var downloadFile in MasterDopwnloadFileList)
            //{
               //var gid = downloadFile.GameXmlName.Split('/')[downloadFile.GameXmlName.Split('/').Length-2];
               //if (!_allGids.Contains(gid))
                  //_allGids.Add(gid);
            //}

            Logger.Log.InfoFormat("$$$$ \\/ $$$$");
            Logger.Log.InfoFormat("Loading Baseball objects into memory before database save....");

            var baseDirInfo = new DirectoryInfo(Constants.BaseSaveDir);
            IEnumerable<DirectoryInfo> dis = baseDirInfo.EnumerateDirectories();
            foreach (var di in dis)
            {
               //if (!_allYears.Contains(di.Name))
                  //continue;

               var breakResult = LookThroughDirectory(di);
               if (breakResult == -1)
                  break;
            }
            SendToDatabase();
         }
         catch (Exception ex)
         {
            Logger.LogException(ex);
         }
      }

      public int LookThroughDirectory(DirectoryInfo di)
      {
         var dis = di.EnumerateDirectories();
         foreach (var dInfo in dis)
         {
            //if (dInfo.Name.StartsWith("gid_2011"))
            //{
               //return -1;
            //}

            
            //if (dInfo.Name.StartsWith("gid_") && _allGids.Contains(dInfo.Name))
            if (dInfo.Name.StartsWith("gid_"))
            {
               var gameFile = dInfo.GetFiles();
               if (gameFile.Length == 1)
               {
                  try
                  {
                     FileInfo allInnings = null;
                     FileInfo[] allPitchers = null;
                     FileInfo[] allBatters = null;

                     foreach (var dir in dInfo.GetDirectories())
                     {
                        switch (dir.Name)
                        {
                           case Constants.BattersDirectoryName:
                              allBatters = dir.GetFiles();
                              break;
                           case Constants.PitchersDirectoryName:
                              allPitchers = dir.GetFiles();
                              break;
                           case Constants.InningDirectoryName:
                              allInnings = dir.GetFiles()[0];
                              break;
                        }
                     }
                     ProcessFileInfos(gameFile[0], allInnings,allPitchers,allBatters);
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
         return 0;
      }

      public ConcurrentDictionary<long, Game> LinkDeserializeAtBats(ConcurrentDictionary<long, Game> games, List<AtBat> atBats,
                                                            ref long minGamePrimaryKey, ref long maxGamePrimaryKey)
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

            Writer.UpdateTableWithGameInfo(MasterGamesInfo, _infoToStore);
            //RemoveWrittenRecordsFromMemory();


         }
         catch (Exception ex)
         {
            Logger.LogException(ex);
         }
      }


   }
}
