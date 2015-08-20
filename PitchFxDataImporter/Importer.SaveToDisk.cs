using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Xml;
using COB.LogWrapper;
using log4net;
using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using OpenQA.Selenium.Remote;
using PitchFx.Contract;

namespace PitchFxDataImporter
{
   public partial class Importer
   {
      /// <summary>
      /// Load xml from link and save the structure locally....
      /// </summary>
      public void SaveXmlFilesToDisk()
      {
         foreach (var downloadFile in MasterDopwnloadFileList)
         {
            ProcessGame(downloadFile);
         }
      }

      /// <summary>
      /// Navigate the tree structure of the mlb site. Save the url and tree structure 'MasterDopwnloadFileList'
      /// </summary>
      public void GatherAllXmlFilenames()
      {
         try
         {
            _allYears = Constants.BaseballYears;

            var browser = GetBrowser();
            var mlbTags = browser.FindElementsByTagName("a").Select(x => x.Text).ToList();
            Logger.Log.InfoFormat("Start Looping To Gather Filenames");
            foreach (var mlbTag in mlbTags)
            {
               if (Constants.BaseballYears.Contains(mlbTag))
               {
                  browser.Navigate().GoToUrl(Constants.MlbLink);
                  Thread.Sleep(100);
                  NavigateToYear(browser, mlbTag.Replace("/", null), 1, Constants.EndMonth);
               }
            }
            Logger.Log.InfoFormat("Start Looping To Gather Filenames");
         }
         catch (Exception ex)
         {
            Logger.LogException(ex);
         }
      }

      /// <summary>
      /// For argument's sake... assume it's only one year
      /// </summary>
      /// <param name="since"></param>
      /// <param name="until"></param>
      public void GatherXmlFilenames(DateTime since, DateTime until)
      {
         try
         {
            _allYears = new List<string>();
            var browser = GetBrowser();
            var dtCnt = new DateTime(since.Year, since.Month, since.Day);
            while (dtCnt <= until)
            {
               var yearStr = Constants.YearPrefex + dtCnt.Year;
               if (!_allYears.Contains(yearStr))
                  _allYears.Add(yearStr);

               Logger.Log.InfoFormat("Attempting to gather xml files for date: {0}",dtCnt);
               var url = browser.Url + yearStr;
               browser.Navigate().GoToUrl(url);

               NavigateToMonth(browser, dtCnt.Month, dtCnt.Year.ToString(), dtCnt.Day, dtCnt.Day);
               dtCnt = dtCnt.AddDays(1);
            }
            Logger.Log.InfoFormat("Finished gathering files name from mlb site - since: {0}, until: {1}", since, until);
         }
         catch (Exception ex)
         {
            Logger.LogException(ex);
         }
      }

      private ChromeDriver GetBrowser()
      {
         DesiredCapabilities capabilities = DesiredCapabilities.Chrome();
         var options = new ChromeOptions();
         options.AddArguments("test-type");
         capabilities.SetCapability("chrome.binary", @"C:\Program Files (x86)\Google\Chrome\Application\");
         capabilities.SetCapability(ChromeOptions.Capability, options);

         var browser = new ChromeDriver(@"C:\chromedriver_win32", options);
         browser.Navigate().GoToUrl(Constants.MlbLink);
         Thread.Sleep(100);
         return browser;
      }

      private void ProcessGame(DownloadFile downloadFile)
      {
         try
         {
            var gameFile = downloadFile.GameXmlName;
            var inningFile = string.Empty;

            switch (_infoToStore)
            {
               case DownloadFile.InfoToStoreEnum.All:
                  inningFile = downloadFile.InningXmlName;
                  break;
               case DownloadFile.InfoToStoreEnum.Inning:
                  inningFile = downloadFile.InningXmlName;
                  break;
               case DownloadFile.InfoToStoreEnum.Players:
                  break;
            }

            var yearDir = string.Empty;
            var monthDir = string.Empty;
            var dayDir = string.Empty;
            var gidDir = string.Empty;
            var inningDir = string.Empty;

            var arr = inningFile.Split('/');
            var gameArr = gameFile.Split('/');
            if (arr.Length > 10)
            {
               yearDir = arr[6];
               monthDir = arr[7];
               dayDir = arr[8];
               gidDir = arr[9];
               inningDir = arr[10];
            }
            else if (gameArr.Length > 10) //TODO: check this..
            {
               yearDir = gameArr[6];
               monthDir = gameArr[7];
               dayDir = gameArr[8];
               gidDir = gameArr[9];
            }

            if (!Directory.Exists(Constants.BaseSaveDir + yearDir))
            {
               Directory.CreateDirectory(Constants.BaseSaveDir + yearDir);
            }

            if (!Directory.Exists(Constants.BaseSaveDir + yearDir + "\\" + monthDir))
            {
               Directory.CreateDirectory(Constants.BaseSaveDir + yearDir + "\\" + monthDir);
            }

            if (!Directory.Exists(Constants.BaseSaveDir + yearDir + "\\" + monthDir + "\\" + dayDir))
            {
               Directory.CreateDirectory(Constants.BaseSaveDir + yearDir + "\\" + monthDir + "\\" + dayDir);
            }

            var fullGidDir = Constants.BaseSaveDir + yearDir + "\\" + monthDir + "\\" + dayDir + "\\" + gidDir;
            var localBatterDir = fullGidDir + "\\" + Constants.BattersDirectory.Replace("/", "");
            var localPitcherDir = fullGidDir + "\\" + Constants.PitchersDirectory.Replace("/", "");
            switch (_infoToStore)
            {
               case DownloadFile.InfoToStoreEnum.All:
                  SaveGameFile(fullGidDir, gameFile);
                  SaveInningsFile(fullGidDir, inningDir, inningFile);
                  SavePlayerFile(downloadFile.Batters, downloadFile.Pitchers, localBatterDir, localPitcherDir);
                  Logger.Log.InfoFormat("Saving Game, Innings, and Player files for: {0}",fullGidDir);
                  break;
               case DownloadFile.InfoToStoreEnum.Inning:
                  SaveGameFile(fullGidDir, gameFile);
                  SaveInningsFile(fullGidDir, inningDir, inningFile);
                  Logger.Log.InfoFormat("Saving Game and Innings files for: {0}", fullGidDir);
                  break;
               case DownloadFile.InfoToStoreEnum.Players:
                  SaveGameFile(fullGidDir, gameFile);
                  SavePlayerFile(downloadFile.Batters, downloadFile.Pitchers, localBatterDir, localPitcherDir);
                  Logger.Log.InfoFormat("Saving Game and Player files for: {0}", fullGidDir);
                  break;
            }
         }
         catch (Exception ex)
         {
            Logger.LogException(ex);
         }
      }

      private void SavePlayerFile(IEnumerable<string> batters, IEnumerable<string> pitchers, string localBatterDir, string localPitcherDir)
      {
         if (!Directory.Exists(localBatterDir))
         {
            Directory.CreateDirectory(localBatterDir);
            foreach (var batter in batters)
            {
               string batterFileName = string.Empty;
               try
               {
                  batterFileName = batter.Split('/')[batter.Split('/').Length - 1];
                  var doc = new XmlDocument();
                  doc.Load(batter);
                  doc.Save(localBatterDir + "\\" + batterFileName);
               }
               catch (Exception ex)
               {
                  Logger.LogException(ex);
                  Logger.Log.ErrorFormat("Unable to save: {0} in {1}", batterFileName, localBatterDir);
               }
            }
         }
         else
         {
            Logger.Log.WarnFormat("Directory exists: {0}, not writing file...", localBatterDir);
         }

         if (!Directory.Exists(localPitcherDir))
         {
            Directory.CreateDirectory(localPitcherDir);
            foreach (var pitcher in pitchers)
            {
               string pitcherFileName = string.Empty;
               try
               {
                  pitcherFileName = pitcher.Split('/')[pitcher.Split('/').Length - 1];
                  var doc = new XmlDocument();
                  doc.Load(pitcher);
                  doc.Save(localPitcherDir + "\\" + pitcherFileName);
               }
               catch (Exception ex)
               {
                  Logger.LogException(ex);
                  Logger.Log.ErrorFormat("Unable to save: {0} in {1}", pitcherFileName, localPitcherDir);
               }
            }
         }
         else
         {
            Logger.Log.WarnFormat("Directory exists: {0}, not writing file...", localPitcherDir);
         }
      }

      private void SaveInningsFile(string fullGidDir, string inningDir, string inningFile)
      {
         var fullInningDir = fullGidDir + "\\" + inningDir;
         if (!Directory.Exists(fullInningDir))
         {
            Directory.CreateDirectory(fullInningDir);
            var doc = new XmlDocument();
            doc.Load(inningFile);
            doc.Save(fullInningDir + "\\" + Constants.AllInningsUrl);
            Logger.Log.InfoFormat("Saved: {0} to: {1}", inningFile, fullInningDir);
         }
         else
         {
            Logger.Log.WarnFormat("Directory exists: {0}, not writing file...", fullInningDir);
         }
      }

      private void SaveGameFile(string fullGidDir, string gameFile)
      {
         if (!Directory.Exists(fullGidDir))
         {
            Directory.CreateDirectory(fullGidDir);
            var doc = new XmlDocument();
            doc.Load(gameFile);
            doc.Save(fullGidDir + "\\" + Constants.GameXmlUrl);
            Logger.Log.InfoFormat("Saved: {0} to: {1}", gameFile, fullGidDir);
         }
         else
         {
            Logger.Log.WarnFormat("Directory exists: {0}, not writing file...", fullGidDir);
         }
      }

      private void NavigateToYear(ChromeDriver browser, string year, int startMonth, int endMonth)
      {
         try
         {
            var url = browser.Url + year;
            Thread.Sleep(500);
            for (int i = startMonth; i <= endMonth; i++)
            {
               browser.Navigate().GoToUrl(url);
               Thread.Sleep(100);
               NavigateToMonth(browser, i, year, 1, Constants.EndDay);
            }
         }
         catch (Exception ex)
         {
            Logger.LogException(ex);
         }
      }

      private void NavigateToMonth(ChromeDriver browser, int month, string year, int startDay, int endDay)
      {
         try
         {
            var monthStr = month < 10 ? "0" + month : month.ToString();
            var url = browser.Url + Constants.MonthPrefix + monthStr + "/";
            //for (int i = 8; i <= 14; i++)
            for (int i = startDay; i <= endDay; i++)
            {
               browser.Navigate().GoToUrl(url);
               Thread.Sleep(100);
               NavigateToDay(browser, i, monthStr, year);
            }
         }
         catch (Exception ex)
         {
            Logger.LogException(ex);
         }
      }

      private void NavigateToDay(ChromeDriver browser, int day, string monthStr, string year)
      {
         try
         {
            //gid_2010_04_21 -> find all elements that start with that... 
            var dayStr = day < 10 ? "0" + day : day.ToString();
            var url = browser.Url + Constants.DayPrefex + dayStr + "/";
            browser.Navigate().GoToUrl(url);
            Thread.Sleep(100);
            year = year.Replace("year_", null);
            var gid = "gid_" + year + "_" + monthStr + "_" + dayStr;
            var allElements = browser.FindElementsByTagName("a");

            var gameInfoLinks = allElements.Where(x => x.Text.StartsWith(gid)).Select(x => x.Text).ToList();
            var baseUrl = browser.Url;
            foreach (var gameInfoLink in gameInfoLinks)
            {
               var baseGameInfoUrl = baseUrl + gameInfoLink;
               browser.Navigate().GoToUrl(baseGameInfoUrl);
               Thread.Sleep(100);
               var gameXmlElement = browser.FindElementsByLinkText(Constants.GameXmlUrl).FirstOrDefault();
               if (gameXmlElement == null)
                  continue;

               var downloadFile = new DownloadFile { GameXmlName = browser.Url + Constants.GameXmlUrl };

               if (_infoToStore == DownloadFile.InfoToStoreEnum.All)
               {
                  downloadFile.Batters = GetPlayers(browser, baseGameInfoUrl, DownloadFile.TypeEnum.Batter);
                  downloadFile.Pitchers = GetPlayers(browser, baseGameInfoUrl, DownloadFile.TypeEnum.Pitcher);

                  browser.Navigate().GoToUrl(baseGameInfoUrl);

                  var inningElement = browser.FindElementsByLinkText(Constants.InningDirectory).FirstOrDefault();
                  if (inningElement == null)
                     continue;

                  browser.Navigate().GoToUrl(baseGameInfoUrl + inningElement.Text);
                  Thread.Sleep(100);
                  var inningAllXmlElement = browser.FindElementsByLinkText(Constants.AllInningsUrl).FirstOrDefault();
                  if (inningAllXmlElement == null)
                     continue;

                  downloadFile.InningXmlName = browser.Url + Constants.AllInningsUrl;
               }
               else if (_infoToStore == DownloadFile.InfoToStoreEnum.Inning)
               {
                  var inningElement = browser.FindElementsByLinkText(Constants.InningDirectory).FirstOrDefault();
                  if (inningElement == null)
                     continue;

                  browser.Navigate().GoToUrl(baseGameInfoUrl + inningElement.Text);
                  Thread.Sleep(100);
                  var inningAllXmlElement = browser.FindElementsByLinkText(Constants.AllInningsUrl).FirstOrDefault();
                  if (inningAllXmlElement == null)
                     continue;

                  downloadFile.InningXmlName = browser.Url + Constants.AllInningsUrl;
               }
               else if (_infoToStore == DownloadFile.InfoToStoreEnum.Players)
               {
                  downloadFile.Batters = GetPlayers(browser, baseGameInfoUrl, DownloadFile.TypeEnum.Batter);
                  downloadFile.Pitchers = GetPlayers(browser, baseGameInfoUrl, DownloadFile.TypeEnum.Pitcher);
               }
               MasterDopwnloadFileList.Add(downloadFile);
               browser.Navigate().GoToUrl(baseUrl);
            }
         }
         catch (Exception ex)
         {
            Logger.LogException(ex);
         }
      }

      private List<string> GetPlayers(ChromeDriver browser, string baseGameInfoUrl, DownloadFile.TypeEnum type)
      {
         try
         {
            browser.Navigate().GoToUrl(baseGameInfoUrl);

            var dirStr = type == DownloadFile.TypeEnum.Batter ? Constants.BattersDirectory : Constants.PitchersDirectory;
            var dirElement = browser.FindElementsByLinkText(dirStr).FirstOrDefault();

            if (dirElement == null)
               return null;

            browser.Navigate().GoToUrl(baseGameInfoUrl + dirElement.Text);
            var playersElement = browser.FindElementsByTagName("a");
            var playersInfoLinks = playersElement.Select(x => x.Text).Where(x => !x.StartsWith("Parent")).ToList();
            return playersInfoLinks.Select(p => baseGameInfoUrl + dirStr + p).ToList();
         }
         catch (Exception ex)
         {
            Logger.Log.ErrorFormat("Error Getting {0} for game: {1}", type, baseGameInfoUrl);
            Logger.LogException(ex);
         }
         return null;
      }


   }
}
