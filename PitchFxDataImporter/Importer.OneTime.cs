using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using COB.LogWrapper;
using Db.Utilities;
using PitchFx.Contract;

namespace PitchFxDataImporter
{
   public partial class Importer
   {
      public void SaveNumberOfAtBatsAndPitchesOnGame()
      {
         try
         {
            var since = new DateTime(2014, 1, 1);
            var until = new DateTime(2015, 1, 1);

            //var since = new DateTime(2010, 1, 1);
            //var until = new DateTime(2010, 6, 1);

            var games = Getter.GetGamesByDate(since, until);

            Logger.Log.InfoFormat("Pulled: {0} games.",games.Count);

            var atBats = Getter.GetAtBatsByDate(since, until);

            Logger.Log.InfoFormat("Pulled: {0} at bats.", atBats.Count);

            long minGamePrimaryKey = 0;
            long maxGamePrimaryKey = 0; 

            games = LinkDeserializeAtBats(games, atBats, ref minGamePrimaryKey, ref maxGamePrimaryKey);

            Logger.Log.InfoFormat("Min Game Primary Key: {0}, Max Game Primary Key: {1}", minGamePrimaryKey,maxGamePrimaryKey);
            
            var pitches = Getter.GetPitchesByPrimaryKey(minGamePrimaryKey, maxGamePrimaryKey);

            Logger.Log.InfoFormat("Pulled {0} pitches.", pitches.Count);

            LinkDeserializedPitches(games, pitches);

            Logger.Log.InfoFormat("Finished putting pitches, at bats, to game");


            foreach (var game in games.Values)
            {
               Writer.UpdateGamePitchesAndAtBats(game);
            }

         }
         catch (Exception ex)
         {
            Logger.LogException(ex);
         }
      }
   }
}
