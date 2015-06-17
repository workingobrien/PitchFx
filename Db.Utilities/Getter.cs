using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using COB.LogWrapper;
using MySql.Data.MySqlClient;
using PitchFx.Contract;

namespace Db.Utilities
{
   public static class Getter
   {
      public static ConcurrentDictionary<long, Game> GetGamesByDate(DateTime since, DateTime until)
      {
         var games = new ConcurrentDictionary<long, Game>();
         var dt = GetGamesTableByDate(since, until);
         foreach (DataRow row in dt.Rows)
         {
            try
            {
               var game = new Game(row);
               if (!game.IsDeserializedFromDb)
                  throw new Exception("Could not deserialize from db, gid: " + row[Game.GamePrimarykeyCol]);

               games.TryAdd(game.GamePrimaryKey, game);
            }
            catch (Exception ex)
            {
               Logger.Log.ErrorFormat("Unable to convert row object for gid: {0} to Game object", row[Game.GidCol]);
               Logger.LogException(ex);
            }
         }
         return games;
      }

      public static List<AtBat> GetAtBatsByDate(DateTime since, DateTime until)
      {
         var atBats = new List<AtBat>();
         var dt = GetAtBatsTableByDate(since, until);
         foreach (DataRow row in dt.Rows)
         {
            try
            {
               var atBat = new AtBat(row);
               if (!atBat.IsDeserializedFromDb)
                  throw new Exception("Could not deserialize from db, gid: " + row[Game.GamePrimarykeyCol]);
               atBats.Add(atBat);

            }
            catch (Exception ex)
            {
               Logger.Log.ErrorFormat("Unable to convert row object for gid: {0} to AtBat object", row[Game.GidCol]);
               Logger.LogException(ex);
            }
         }

         return atBats;
      }

      public static List<Pitch> GetPitchesByPrimaryKey(long minPrimaryKey, long maxPrimaryKey)
      {
         var pitches = new List<Pitch>();
         var dt = GetPitchesTableByDate(minPrimaryKey, maxPrimaryKey);
         foreach (DataRow row in dt.Rows)
         {
            try
            {
               var pitch = new Pitch(row);
               if (!pitch.IsDeserializedFromDb)
                  throw new Exception("Could not deserialize from db, gid: " + row[Game.GamePrimarykeyCol]);
               pitches.Add(pitch);
            }
            catch (Exception ex)
            {
               Logger.Log.ErrorFormat("Unable to convert row object for gid: {0} to Pitch object", row[Game.GidCol]);
               Logger.LogException(ex);
            }
         }
         return pitches;
      }

      private static DataTable GetGamesTableByDate(DateTime since, DateTime until)
      {
         try
         {
            const string sql = @"SELECT * FROM GAMES WHERE game_date >= @since and game_date <= @until";
            return GetDataTable(since, until, sql, "games");
         }
         catch (Exception ex)
         {
            Logger.LogException(ex);
            return null;
         }
      }

      private static DataTable GetAtBatsTableByDate(DateTime since, DateTime until)
      {
         try
         {
            const string sql = @"select ab.* from atbats ab
                                 where ab.game_primarykey in
                                 (SELECT g.game_primarykey FROM GAMES g WHERE g.game_date >= @since and g.game_date <= @until)";
            return GetDataTable(since, until, sql, "atBats");
         }
         catch (Exception ex)
         {
            Logger.LogException(ex);
            return null;
         }
      }

      private static DataTable GetPitchesTableByDate(long minPrimaryKey, long maxPrimaryKey)
      {
         try
         {
            const string sql = @"select * from pitches
                                 where game_primarykey >= @minPrimaryKey and game_primarykey <= @maxPrimaryKey
                                 ";
                                 
            var table = new DataTable("pitches");
            using (var connection = DbConnector.Instance.SqlConnection)
            {
               var cmd = connection.CreateCommand();
               cmd.CommandText = sql;
               cmd.Parameters.AddWithValue("@minPrimaryKey", minPrimaryKey);
               cmd.Parameters.AddWithValue("@maxPrimaryKey", maxPrimaryKey);
               cmd.ExecuteNonQuery();
               var dataAdapter = new MySqlDataAdapter(cmd);
               dataAdapter.Fill(table);
            }
            return table;
         }
         catch (Exception ex)
         {
            Logger.LogException(ex);
            return null;
         }
      }

      private static DataTable GetDataTable(DateTime since, DateTime until, string sql, string tableName)
      {
         var connection = DbConnector.Instance.SqlConnection;
         var cmd = connection.CreateCommand();
         cmd.CommandText = sql;
         cmd.Parameters.AddWithValue("@since", since.Date);
         cmd.Parameters.AddWithValue("@until", until.Date);
         cmd.ExecuteNonQuery();
         var dataAdapter = new MySqlDataAdapter(cmd);
         var table = new DataTable(tableName);
         dataAdapter.Fill(table);
         return table;
      }
   }


}
