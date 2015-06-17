using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Xml;
using COB.LogWrapper;
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
         try
         {
            foreach (var downloadFile in MasterDopwnloadFileList)
            {
               var gameFile = downloadFile.GameXmlName;
               var inningFile = downloadFile.InningXmlName;

               var arr = inningFile.Split('/');
               if (arr.Length > 10)
               {
                  //var gid = gameFileArr[gameFileArr.Length - 2];
                  var yearDir = arr[6];
                  var monthDir = arr[7];
                  var dayDir = arr[8];
                  var gidDir = arr[9];
                  var inningDir = arr[10];

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
                     continue;
                     //Safe to assume the file already exists
                  }

                  var fullInningDir = fullGidDir + "\\" + inningDir;
                  if (!Directory.Exists(fullInningDir))
                  {
                     Directory.CreateDirectory(fullInningDir);
                     var doc = new XmlDocument();
                     doc.Load(inningFile);
                     doc.Save(fullInningDir + "\\" + Constants.AllInningsUrl);
                  }
                  else
                  {
                     Logger.Log.WarnFormat("Directory exists: {0}, not writing file...", fullInningDir);
                  }

                  Logger.Log.InfoFormat("Saved: {0} to: {1}", inningFile, fullInningDir);
               }


            }
         }
         catch (Exception ex)
         {
            Logger.LogException(ex);
         }
      }

      /// <summary>
      /// Navigate the tree structure of the mlb site. Save the url and tree structure 'MasterDopwnloadFileList'
      /// </summary>
      public void GatherAllXmlFilenames()
      {
         try
         {
            const string mlb = @"http://gd2.mlb.com/components/game/mlb/";

            DesiredCapabilities capabilities = DesiredCapabilities.Chrome();
            var options = new ChromeOptions();
            options.AddArguments("test-type");
            capabilities.SetCapability("chrome.binary", @"C:\Program Files (x86)\Google\Chrome\Application\");
            capabilities.SetCapability(ChromeOptions.Capability, options);

            var browser = new ChromeDriver(@"C:\chromedriver_win32", options);
            browser.Navigate().GoToUrl(mlb);
            Thread.Sleep(100);
            var mlbTags = browser.FindElementsByTagName("a").Select(x => x.Text).ToList();
            Logger.Log.InfoFormat("Start Looping To Gather Filenames");
            foreach (var mlbTag in mlbTags)
            {
               if (Constants.BaseballYears.Contains(mlbTag))
               {
                  browser.Navigate().GoToUrl(mlb);
                  Thread.Sleep(100);
                  NavigateToYear(browser, mlbTag.Replace("/", null));
               }
            }
            Logger.Log.InfoFormat("Start Looping To Gather Filenames");
         }
         catch (Exception ex)
         {
            Logger.LogException(ex);
         }
      }

      private void NavigateToYear(ChromeDriver browser, string year)
      {
         try
         {
            var url = browser.Url + year;
            Thread.Sleep(500);
            //for (int i = 5; i <= 5; i++)
            for (int i = 1; i <= Constants.EndMonth; i++)
            {
               browser.Navigate().GoToUrl(url);
               Thread.Sleep(100);
               NavigateToMonth(browser, i, year);
            }


         }
         catch (Exception ex)
         {
            Logger.LogException(ex);
         }
      }

      private void NavigateToMonth(ChromeDriver browser, int month, string year)
      {
         try
         {
            var monthStr = month < 10 ? "0" + month : month.ToString();
            var url = browser.Url + Constants.MonthPrefix + monthStr + "/";
            for (int i = 1; i <= Constants.EndDay; i++)
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
               browser.Navigate().GoToUrl(baseUrl + gameInfoLink);
               Thread.Sleep(100);
               var gameXmlElement = browser.FindElementsByLinkText(Constants.GameXmlUrl).FirstOrDefault();
               if (gameXmlElement == null)
                  continue;

               var downloadFile = new DownloadFile { GameXmlName = browser.Url + Constants.GameXmlUrl };

               var inningElement = browser.FindElementsByLinkText(Constants.InningDirectory).FirstOrDefault();
               if (inningElement == null)
                  continue;
               browser.Navigate().GoToUrl(browser.Url + inningElement.Text);
               Thread.Sleep(100);
               var inningAllXmlElement = browser.FindElementsByLinkText(Constants.AllInningsUrl).FirstOrDefault();
               if (inningAllXmlElement == null)
                  continue;
               downloadFile.InningXmlName = browser.Url + Constants.AllInningsUrl;
               MasterDopwnloadFileList.Add(downloadFile);
               browser.Navigate().GoToUrl(baseUrl);
            }
         }
         catch (Exception ex)
         {
            Logger.LogException(ex);
         }
      }


   }
}
