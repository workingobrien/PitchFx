using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml;
using System.Xml.Serialization;
using COB.LogWrapper;
using log4net;
using PitchFx.Contract;
using Action = PitchFx.Contract.Action;

namespace PitchFxDataImporter
{
   public partial class Importer
   {
      public void ProcessFileInfos(FileInfo gameFileInfo, FileInfo allInnings,
                                   FileInfo[] allPitchers, FileInfo[] allBatters)
      {
         try
         {
            var game = DerserializeGameFile(gameFileInfo);
            if (game == null)
               throw new Exception("Could not deserialize game for file: " + gameFileInfo.FullName);

            //if (game.GamePrimaryKey == 0)
               //throw new Exception("Could not deserialize game because it has no primary key: " + gameFileInfo.FullName);

            if (game.GamePrimaryKey == 0)
               return;


            if (game.Type == Constants.RealGameType)
            {
               MasterGamesInfo.TryAdd(game.GamePrimaryKey, game);

               var pitchers = new List<Player>();
               var batters = new List<Player>();

               switch (_infoToStore)
               {
                  case DownloadFile.InfoToStoreEnum.All:
                     DeserializeInningsFile(allInnings, game);
                     DeserializePlayersFile(allPitchers, allBatters, ref pitchers, ref batters);
                     break;
                  case DownloadFile.InfoToStoreEnum.Inning:
                     DeserializeInningsFile(allInnings, game);
                     break;
                  case DownloadFile.InfoToStoreEnum.Players:
                     DeserializePlayersFile(allPitchers, allBatters, ref pitchers, ref batters);
                     break;
               }

               game.Pitchers = pitchers;
               game.Batters = batters;
               Logger.Log.InfoFormat("Finished deserializing gid: {0}",game.Gid);
            }
            else
            {
               //Logger.Log.InfoFormat("Skipping Game: {0}", game);
            }
         }
         catch (Exception ex)
         {
            Logger.LogException(ex);
         }
      }

      private void DeserializeInningsFile(FileInfo allInningsFileInfo, Game game)
      {
         try
         {
            if (allInningsFileInfo == null)
            {
               Logger.Log.WarnFormat("Game: {0} has no innings file. (rain delayed or cancelled?)", game.Gid);
               return;
            }

            var doc = new XmlDocument();
            doc.Load(allInningsFileInfo.FullName);

            foreach (XmlNode node in doc.ChildNodes)
            {
               if (node.Name == Constants.GameNodeName)
               {
                  List<Action> actions = null;
                  var allGameAbs = LoopThroughInnings(node, game, ref actions);
                  GetAllPitches(allGameAbs);
                  game.AtBats = allGameAbs;
                  game.Actions = actions;
               }
               else if (node.Name != "#comment")
               {
                  var s = string.Empty;
               }
            }
         }
         catch (Exception ex)
         {
            Logger.LogException(ex);
         }
      }

      private void DeserializePlayersFile(FileInfo[] allPitchers, FileInfo[] allBatters, ref List<Player> pitchers, ref List<Player> batters)
      {
         try
         {
            foreach (var pitcherFi in allPitchers)
            {
               var doc = new XmlDocument();
               doc.Load(pitcherFi.FullName);
               var playerNodeStr = doc.ChildNodes[1].OuterXml;
               var serializer = new XmlSerializer(typeof(Player));
               Player player;
               using (var sr = new StringReader(playerNodeStr))
               {
                  player = (Player)serializer.Deserialize(sr);
               }
               if (player == null)
               {
                  Logger.Log.ErrorFormat("Unable to deserialize pitcher: {0} into Player object.", pitcherFi.FullName);
               }
               else
               {
                  player.SetFullName();
                  pitchers.Add(player);
               }
            }

            foreach (var batterFi in allBatters)
            {
               var doc = new XmlDocument();
               doc.Load(batterFi.FullName);

               var playerNodeStr = doc.ChildNodes[1].OuterXml;
               var serializer = new XmlSerializer(typeof(Player));
               Player player;
               using (var sr = new StringReader(playerNodeStr))
               {
                  player = (Player)serializer.Deserialize(sr);
               }
               if (player == null)
               {
                  Logger.Log.ErrorFormat("Unable to deserialize batter: {0} into Player object.", batterFi.FullName);
               }
               else
               {
                  player.SetFullName();
                  batters.Add(player);
               }
            }

         }
         catch (Exception ex)
         {
            Logger.LogException(ex);
         }
      }

      /// <summary>
      /// Not in love with the return type... since I don't assign the pitches 
      /// The individual abs get assigned the pitches...
      /// </summary>
      /// <param name="allGameAbs"></param>
      /// <returns></returns>
      private List<Pitch> GetAllPitches(IEnumerable<AtBat> allGameAbs)
      {
         try
         {
            var pitches = new List<Pitch>();
            foreach (var ab in allGameAbs)
            {
               var atBatNode = ab.AtBatNode;
               if (atBatNode.HasChildNodes)
               {
                  var runnners = new List<Runner>();
                  foreach (XmlNode node in atBatNode.ChildNodes)
                  {
                     if (node.Name == Constants.PitchNode)
                     {
                        var pitch = DeserializePitchNode(ab, node);
                        if (pitch != null)
                           pitches.Add(pitch);
                        else
                           Logger.Log.WarnFormat("Can not Deserialize node into pitch object: {0}", node.OuterXml);
                     }
                     else if (node.Name == Constants.RunnerNode)
                     {
                        var runner = DeserializeRunnerNode(ab, node);
                        if (runner != null)
                           runnners.Add(runner);
                        else
                           Logger.Log.WarnFormat("Can not Deserialize node into runner object: {0}", node.OuterXml);

                     }
                     else
                     {
                        var s = string.Empty;
                     }
                  }
                  ab.Pitches = pitches.Where(p => p.AtBatGuid == ab.AtBatGuid).ToList();
                  ab.Runners = runnners;
               }
            }
            return pitches;
         }
         catch (Exception ex)
         {
            Logger.LogException(ex);
         }
         return null;
      }

      private Pitch DeserializePitchNode(AtBat ab, XmlNode pitchNode)
      {
         try
         {
            Pitch pitch = null;
            var serializer = new XmlSerializer(typeof(Pitch));

            using (var xmlReader = XmlReader.Create(new StringReader(pitchNode.OuterXml)))
            {
               if (serializer.CanDeserialize(xmlReader))
               {
                  pitch = (Pitch)serializer.Deserialize(xmlReader);
               }
               else if (pitchNode.Name != "action")
               {
                  Logger.Log.WarnFormat("Can not deserialize node: {0} in atBatNode. Name: {1}", pitchNode.OuterXml, pitchNode.Name);
               }
               else if (pitchNode.Name == "action")
               {
                  var s = string.Empty;
               }

               if (pitch == null)
                  return null;

               if (string.IsNullOrEmpty(pitch.AtBatGuid))
                  pitch.PitchGuid = Pitch.GeneratePitchGuid();
               pitch.Ab = ab;
               return pitch;
            }

         }
         catch (Exception ex)
         {
            Logger.LogException(ex);
         }
         return null;
      }

      private Runner DeserializeRunnerNode(AtBat ab, XmlNode runnerNode)
      {
         Runner runner = null;
         try
         {
            var serializer = new XmlSerializer(typeof(Runner));
            using (var xmlReader = XmlReader.Create(new StringReader(runnerNode.OuterXml)))
            {
               if (serializer.CanDeserialize(xmlReader))
               {
                  runner = (Runner)serializer.Deserialize(xmlReader);
                  if (string.IsNullOrEmpty(runner.RunnerGuid))
                     runner.RunnerGuid = Runner.GenerateActionGuid();
                  runner.Ab = ab;
               }
            }
         }
         catch (Exception ex)
         {
            Logger.LogException(ex);
         }
         return runner;
      }

      private List<AtBat> LoopThroughInnings(XmlNode gameNode, Game game, ref List<Action> actions)
      {
         try
         {
            var atBats = new List<AtBat>();
            actions = new List<Action>();
            for (int i = 0; i < gameNode.ChildNodes.Count; i++)
            {
               var inningNode = gameNode.ChildNodes[i];
               var topBotCnt = inningNode.ChildNodes.Count;
               if (topBotCnt == 1)
               {
                  var topNode = inningNode.ChildNodes[Constants.TopInningIndex];
                  atBats.AddRange(topNode.ChildNodes.Cast<XmlNode>()
                     .Select(atBatNode => DeserializeAtBatNode(atBatNode, game, i + 1, Constants.TopNode))
                     .Where(atBat => atBat != null));

                  actions.AddRange(topNode.ChildNodes.Cast<XmlNode>()
                     .Select(actionNode => DeserializeActionNode(actionNode, game, i + 1, Constants.TopNode))
                     .Where(action => action != null));
               }
               else if (topBotCnt == 2)
               {
                  var topNode = inningNode.ChildNodes[Constants.TopInningIndex];
                  atBats.AddRange(topNode.ChildNodes.Cast<XmlNode>()
                     .Select(atBatNode => DeserializeAtBatNode(atBatNode, game, i + 1, Constants.TopNode))
                     .Where(atBat => atBat != null));

                  actions.AddRange(topNode.ChildNodes.Cast<XmlNode>()
                     .Select(actionNode => DeserializeActionNode(actionNode, game, i + 1, Constants.TopNode))
                     .Where(action => action != null));

                  var bottomNode = inningNode.ChildNodes[Constants.BottomInningIndex];
                  atBats.AddRange(bottomNode.ChildNodes.Cast<XmlNode>()
                     .Select(atBatNode => DeserializeAtBatNode(atBatNode, game, i + 1, Constants.BottomNode))
                     .Where(atBat => atBat != null));

                  actions.AddRange(bottomNode.ChildNodes.Cast<XmlNode>()
                     .Select(actionNode => DeserializeActionNode(actionNode, game, i + 1, Constants.BottomNode))
                     .Where(action => action != null));

               }
            }
            return atBats;
         }
         catch (Exception ex)
         {
            Logger.LogException(ex);
         }
         return null;
      }

      private Action DeserializeActionNode(XmlNode actionNode, Game game, int inning, string inningType)
      {
         Action action = null;
         try
         {
            var serializer = new XmlSerializer(typeof(Action));

            using (var xmlReader = XmlReader.Create(new StringReader(actionNode.OuterXml)))
            {
               if (serializer.CanDeserialize(xmlReader))
               {
                  action = (Action)serializer.Deserialize(xmlReader);
                  if (string.IsNullOrEmpty(action.ActionGuid))
                     action.ActionGuid = Action.GenerateActionGuid();
                  action.Inning = inning;
                  action.InningType = inningType;
                  action.Game = game;
               }
            }
            if (action == null)
            {
               var s = string.Empty;
            }
         }
         catch (Exception ex)
         {
            Logger.LogException(ex);
         }
         return action;
      }

      private AtBat DeserializeAtBatNode(XmlNode atBatNode, Game game, int inning, string inningType)
      {
         try
         {
            AtBat ab = null;
            var serializer = new XmlSerializer(typeof(AtBat));

            using (var xmlReader = XmlReader.Create(new StringReader(atBatNode.OuterXml)))
            {
               if (serializer.CanDeserialize(xmlReader))
                  ab = (AtBat)serializer.Deserialize(xmlReader);
               else if (atBatNode.Name == "action")
               {

               }
               else
               {
                  Logger.Log.WarnFormat("Can not deserialize node: {0} in atBatNode. Name: {1}", atBatNode.OuterXml, atBatNode.Name);
               }
            }

            if (ab == null)
               return null;

            ab.Inning = inning;
            ab.InningType = inningType;
            if (string.IsNullOrEmpty(ab.AtBatGuid))
               ab.AtBatGuid = AtBat.GenerateAbGuid();
            ab.GamePrimaryKey = game.GamePrimaryKey;
            ab.Gid = game.Gid;
            ab.AtBatNode = atBatNode;
            return ab;
         }
         catch (Exception ex)
         {
            Logger.LogException(ex);
         }
         return null;
      }

      private Game DerserializeGameFile(FileInfo gameFileInfo)
      {
         try
         {
            var doc = new XmlDocument();
            doc.Load(gameFileInfo.FullName);

            var gameNodeStr = doc.ChildNodes[1].OuterXml;
            var serializer = new XmlSerializer(typeof(Game));
            Game game;
            using (var sr = new StringReader(gameNodeStr))
            {
               game = (Game)serializer.Deserialize(sr);
               if (game.Type != Constants.RealGameType)
                  return game;
            }

            if (gameFileInfo.DirectoryName != null)
            {
               var dnArr = gameFileInfo.DirectoryName.Split('\\');
               game.Gid = dnArr[dnArr.Length - 1];
            }

            var homeXml = doc.ChildNodes[1].ChildNodes[0].OuterXml;
            var awayXml = doc.ChildNodes[1].ChildNodes[1].OuterXml;
            DeserializeTeamsFromGame(game, homeXml, awayXml);
            return game;
         }
         catch (Exception ex)
         {
            Logger.LogException(ex);
         }
         return null;
      }

      private void DeserializeTeamsFromGame(Game game, string homeXml, string awayXml)
      {
         try
         {
            var serializer = new XmlSerializer(typeof(Team));
            Team home;
            using (var sr = new StringReader(homeXml))
            {
               home = (Team)serializer.Deserialize(sr);
            }

            Team away;
            using (var sr = new StringReader(awayXml))
            {
               away = (Team)serializer.Deserialize(sr);
            }
            game.Home = home;
            game.Away = away;
         }
         catch (Exception ex)
         {
            Logger.LogException(ex);
         }
      }


      private void RemoveWrittenRecordsFromMemory()
      {
         try
         {
            var gamePks = new List<long>();
            foreach (var game in MasterGamesInfo.Values)
            {
               foreach (var atBat in game.AtBats)
               {
                  atBat.Pitches.RemoveAll(p => p.IsPitchSaved);
                  atBat.Runners.RemoveAll(p => p.IsRunnerSaved);

                  var x = string.Empty;
               }

               var isRemovedAll = true;

               foreach (var pitch in game.Pitches)
               {
                  Logger.Log.ErrorFormat("Unable to save pitch: {0}", pitch);
                  isRemovedAll = false;
               }

               foreach (var runner in game.Runners)
               {
                  Logger.Log.ErrorFormat("Unable to save runner: {0}", runner);
                  isRemovedAll = false;
               }

               if (isRemovedAll)
               {
                  game.AtBats.RemoveAll(ab => ab.IsAtBatSaved);
                  foreach (var atBat in game.AtBats)
                  {
                     Logger.Log.ErrorFormat("Unablet to save at bat: {0}", atBat);
                     isRemovedAll = false;
                  }
               }
               if (isRemovedAll)
                  gamePks.Add(game.GamePrimaryKey);
            }

            foreach (var gamePk in gamePks)
            {
               Game game;
               MasterGamesInfo.TryRemove(gamePk, out game);
            }

            foreach (var game in MasterGamesInfo.Values)
            {
               Logger.Log.ErrorFormat("Could not write records corresponding with Game: {0}", game.Gid);
            }
            MasterGamesInfo.Clear();
            var s = string.Empty;

         }
         catch (Exception ex)
         {
            Logger.LogException(ex);
         }
      }


   }
}
