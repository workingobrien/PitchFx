using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using COB.LogWrapper;
using MySql.Data.MySqlClient;
using PitchFx.Contract;

namespace Db.Utilities
{
   public static class Getter
   {
      private static readonly object _lockObj = new object();
      private static bool _isCacheLoading = false;
      public static bool IsCachLoading { get { return _isCacheLoading; } }

      static Getter()
      {

      }

      public static void LoadGameCache()
      {
         try
         {
            var bgWorker = new BackgroundWorker();
            bgWorker.DoWork += BgWorkerOnDoWork;
            bgWorker.RunWorkerAsync();
         }
         catch (Exception ex)
         {
            Logger.LogException(ex);
         }
      }

      private static void BgWorkerOnDoWork(object sender, DoWorkEventArgs e)
      {
         _isCacheLoading = true;
         Logger.Log.Info("### \\/ Starting Cache Load \\/ ###");
         try
         {
            var since = DateTime.Today.AddDays(-100000);
            var until = DateTime.Today.AddDays(1);

            var allPlayers = GetAllPlayers();
            Logger.Log.InfoFormat("### {0} Players exist in the database.", allPlayers.Count);
            var games = GetGamesByDate(since, until);

            _loadedGids = games.Keys.ToList();
            Logger.Log.InfoFormat("### {0} Games exits in the database.", _loadedGids.Count);

            long minGamePrimaryKey = 0;
            long maxGamePrimaryKey = 0;

            //var atBats = GetAtBatsByDate(since, until);
            //Logger.Log.InfoFormat("### {0} At bats exist in the database.",atBats.Count);

            //games = LinkDeserializeAtBats(games, atBats, ref minGamePrimaryKey, ref maxGamePrimaryKey);

            //var pitches = GetPitchesByPrimaryKey(minGamePrimaryKey, maxGamePrimaryKey);
            //Logger.Log.InfoFormat("### {0} Pitches exist in the database.", pitches.Count);

            //LinkDeserializedPitches(games, pitches);

            foreach (var game in games)
            {
               _loadedAtBatsCntByGid[game.Key] = game.Value.AtBats.Count;
               _loadedPitchesCntByGid[game.Key] = game.Value.Pitches.Count;
               _loadedRunnerCntByGid[game.Key] = game.Value.Runners.Count;
            }

            foreach (var player in allPlayers)
            {
               _allPlayers.Add(player.Id);
            }

         }
         catch (Exception ex)
         {
            Logger.LogException(ex);
         }
         _isCacheLoading = false;
         Logger.Log.Info("### /\\ Finished Cache Load /\\ ###");
      }

      private static List<long> _loadedGids = new List<long>();
      public static List<long> LoadedGids
      {
         get { return _loadedGids; }
      }

      private static ConcurrentDictionary<long, long> _loadedAtBatsCntByGid = new ConcurrentDictionary<long, long>();
      public static ConcurrentDictionary<long, long> LoadedAtBatCntByGid
      {
         get { return _loadedAtBatsCntByGid; }
      }

      private static ConcurrentDictionary<long, long> _loadedPitchesCntByGid = new ConcurrentDictionary<long, long>();
      public static ConcurrentDictionary<long, long> LoadedPitchesCntByGid
      {
         get { return _loadedPitchesCntByGid; }
      }

      private static ConcurrentDictionary<long, long> _loadedRunnerCntByGid = new ConcurrentDictionary<long, long>();
      public static ConcurrentDictionary<long, long> LoadedRunnerCntByGid
      {
         get { return _loadedRunnerCntByGid; }
      }

      private static ConcurrentDictionary<long, long> _loadedBattersCntByGid = new ConcurrentDictionary<long, long>();
      public static ConcurrentDictionary<long, long> LoadedBattersCntByGid
      {
         get { return _loadedBattersCntByGid; }
      }

      private static ConcurrentDictionary<long, long> _loadedPitchersCntByGid = new ConcurrentDictionary<long, long>();
      public static ConcurrentDictionary<long, long> LoadedPitchersCntByGid
      {
         get { return _loadedPitchersCntByGid; }
      }

      private static List<long> _allPlayers = new List<long>();
      public static List<long> AllPlayers
      {
         get { return _allPlayers; }
      }

      //private static List<long> GetLoadedGameIds()
      //{
      //return GetGamesByDate(DateTime.Today.AddDays(-100000), DateTime.Today.AddDays(3)).Keys.ToList();
      //}

      public static ConcurrentDictionary<long, Game> GetGamesByDate(DateTime since, DateTime until)
      {
         lock (_lockObj)
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
      }

      public static List<Player> GetAllPlayers()
      {
         const string sql = @"select * from players";
         var table = new DataTable("players");
         using (var connection = DbConnector.Instance.SqlConnection)
         {
            var cmd = connection.CreateCommand();
            cmd.CommandText = sql;
            cmd.ExecuteNonQuery();
            var dataAdapter = new MySqlDataAdapter(cmd);
            dataAdapter.Fill(table);
         }

         var players = new List<Player>();
         foreach (DataRow row in table.Rows)
         {
            try
            {
               var player = new Player(row);
               if (!player.IsDeserializedFromDb)
                  throw new Exception("Could not deserialize from db, row: " + row["id"].ToString());
               players.Add(player);
            }
            catch (Exception ex)
            {
               Logger.LogException(ex);
            }
         }
         return players;
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
         var ds = GetPitchesTableByDate(minPrimaryKey, maxPrimaryKey);

         foreach (DataTable dt in ds.Tables)
         {
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
         }
         return pitches;
      }

      private static DataTable GetGamesTableByDate(DateTime since, DateTime until)
      {
         try
         {
            const string sql = @"SELECT * FROM GAMES WHERE date(game_date) >= @since and date(game_date) <= @until";
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
                                 (SELECT g.game_primarykey FROM GAMES g WHERE date(g.game_date) >= @since and date(g.game_date) <= @until)";
            return GetDataTable(since, until, sql, "atBats");
         }
         catch (Exception ex)
         {
            Logger.LogException(ex);
            return null;
         }
      }

      /// <summary>
      /// I'm not a database guy.... I have not thought of the best way to query for a large number of pitches
      /// So I'm going to break this up into pulling pitches per 150 games...
      /// </summary>
      /// <param name="minPrimaryKey"></param>
      /// <param name="maxPrimaryKey"></param>
      /// <returns></returns>
      private static DataSet GetPitchesTableByDate(long minPrimaryKey, long maxPrimaryKey)
      {
         try
         {
            var ds = new DataSet();
            var minMaxKeys = new List<Tuple<long, long>>();

            long chunks = (((maxPrimaryKey - minPrimaryKey) + 1) / 150) + 1;
            Logger.Log.InfoFormat("Breaking up: {0} games in: {1} chunks.",maxPrimaryKey-minPrimaryKey+1,chunks);

            long startingKey = minPrimaryKey;

            for (long i = 0; i < chunks; i++)
            {
               long endingKey = Math.Min(startingKey + 150, maxPrimaryKey);
               minMaxKeys.Add(new Tuple<long, long>(startingKey, endingKey));
               startingKey = endingKey + 1;
            }


            const string sql = @"select * from pitches where game_primarykey >= @minPrimaryKey and game_primarykey <= @maxPrimaryKey";
            var chunkCnt = 1;

            using (var connection = DbConnector.Instance.SqlConnection)
            {
               foreach (var minMaxKey in minMaxKeys)
               {
                  var minPk = minMaxKey.Item1;
                  var maxPk = minMaxKey.Item2;
                  var table = new DataTable(minPk.ToString() + "-" + maxPk.ToString());

                  var cmd = connection.CreateCommand();
                  cmd.CommandText = sql;
                  cmd.Parameters.AddWithValue("@minPrimaryKey", minPk);
                  cmd.Parameters.AddWithValue("@maxPrimaryKey", maxPk);
                  cmd.ExecuteNonQuery();
                  var dataAdapter = new MySqlDataAdapter(cmd);
                  dataAdapter.Fill(table);
                  ds.Tables.Add(table);
                  Logger.Log.InfoFormat("Finished chunk cnt: {0} with {1} pitches.",chunkCnt,table.Rows.Count);
                  chunkCnt++;
               }
            }
            return ds;

            //var table = new DataTable("pitches");
            //using (var connection = DbConnector.Instance.SqlConnection)
            //{
            //   var cmd = connection.CreateCommand();
            //   cmd.CommandText = sql;
            //   cmd.Parameters.AddWithValue("@minPrimaryKey", minPrimaryKey);
            //   cmd.Parameters.AddWithValue("@maxPrimaryKey", maxPrimaryKey);
            //   cmd.ExecuteNonQuery();
            //   var dataAdapter = new MySqlDataAdapter(cmd);
            //   dataAdapter.Fill(table);
            //}
            //return table;
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


      private static ConcurrentDictionary<long, Game> LinkDeserializeAtBats(ConcurrentDictionary<long, Game> games, List<AtBat> atBats,
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

      private static ConcurrentDictionary<long, Game> LinkDeserializedPitches(ConcurrentDictionary<long, Game> games, List<Pitch> pitches)
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

   }


}
