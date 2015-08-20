using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using COB.LogWrapper;
using MySql.Data.MySqlClient;
using PitchFx.Contract;
using Action = PitchFx.Contract.Action;

namespace Db.Utilities
{
   public static class Writer
   {
      private static List<long> _playerIdsWritten = new List<long>();

      public static void UpdateTableWithGameInfo(ConcurrentDictionary<long, Game> allGames, DownloadFile.InfoToStoreEnum infoToStore)
      {
         Logger.Log.InfoFormat("Attempting to save the following to the database:");
         Logger.Log.InfoFormat("Games: {0}", allGames.Count);
         Logger.Log.InfoFormat("At Bats: {0}", allGames.Values.SelectMany(g => g.AtBats).ToList().Count);
         Logger.Log.InfoFormat("Pitches: {0}", allGames.Values.SelectMany(g => g.Pitches).ToList().Count);
         Logger.Log.InfoFormat("Runners: {0}", allGames.Values.SelectMany(g => g.Runners).ToList().Count);
         Logger.Log.InfoFormat("Batters: {0}", allGames.Values.SelectMany(g => g.Batters).ToList().Count);
         Logger.Log.InfoFormat("Pitchers: {0}", allGames.Values.SelectMany(g => g.Pitchers).ToList().Count);

         while (Getter.IsCachLoading)
         {
            Logger.Log.Warn("Cache is still loading... wait 10 seconds.");
            Thread.Sleep(10000);
         }

         var allGids = Getter.LoadedGids;

         foreach (var game in allGames.Values)
         {
            try
            {
               var totalAtBats = game.AtBats.Count;
               var totalPitches = game.Pitches.Count;
               var totalRunners = game.Runners.Count;
               var totalBatters = game.Batters.Count;
               var totalPitchers = game.Pitchers.Count;

               int totalAtBatsWritten = 0;
               int totalPitchesWritten = 0;
               int totalRunnersWritten = 0;
               int totalBattersWritten = 0;
               int totalPitchersWritten = 0;

               var gid = game.GamePrimaryKey;
               if (!allGids.Contains(gid) && infoToStore != DownloadFile.InfoToStoreEnum.Players)
               {
                  WriteGame(game);
               }

               long atBatsLoaded;
               long pitchesLoaded;
               long runnersLoaded;
               long battersLoaded;
               long pitchersLoaded;

               Getter.LoadedAtBatCntByGid.TryGetValue(gid, out atBatsLoaded);
               Getter.LoadedPitchesCntByGid.TryGetValue(gid, out pitchesLoaded);
               Getter.LoadedRunnerCntByGid.TryGetValue(gid, out runnersLoaded);
               Getter.LoadedBattersCntByGid.TryGetValue(gid, out battersLoaded);
               Getter.LoadedPitchersCntByGid.TryGetValue(gid, out pitchersLoaded);

               //TODO: prevent dupilicates.. 

               switch (infoToStore)
               {
                  case DownloadFile.InfoToStoreEnum.All:
                     if (atBatsLoaded != game.AtBats.Count)
                        totalAtBatsWritten = WriteAtBats(game.AtBats);
                     else
                        Logger.Log.WarnFormat("Not writing at bats for gid: {0}, already has {1} at bats loaded. Would have tried saving: {2} at bats", gid, atBatsLoaded, game.AtBats.Count);

                     if (pitchesLoaded != game.Pitches.Count)
                        totalPitchesWritten = WritePitches(game.Pitches);
                     else
                        Logger.Log.WarnFormat("Not writing pitches for gid: {0}, already has {1} pitches loaded. Would have tried saving: {2} pitches", gid, pitchesLoaded, game.Pitches.Count);

                     if (runnersLoaded != game.Runners.Count)
                        totalRunnersWritten = WriteRunners(game.Runners);
                     else
                        Logger.Log.WarnFormat("Not writing runners for gid: {0}, already has {1} runners loaded. Would have tried saving: {2} runners", gid, runnersLoaded, game.Runners.Count);

                     if (battersLoaded != game.Batters.Count)
                        totalBattersWritten = WriteBatters(game.Batters);
                     else
                        Logger.Log.WarnFormat("Not writing batters for gid: {0}, already has {1} batters loaded. Would have tried saving: {2} batters", gid, battersLoaded, game.Batters.Count);

                     if (pitchersLoaded != game.Pitchers.Count)
                        totalPitchersWritten = WritePitchers(game.Pitchers);
                     else
                        Logger.Log.WarnFormat("Not writing pitchers for gid: {0}, already has {1} pitchers loaded. Would have tried saving: {2} pitchers", gid, pitchersLoaded, game.Pitchers.Count);
                     break;
                  case DownloadFile.InfoToStoreEnum.Inning:
                     if (atBatsLoaded != game.AtBats.Count)
                        totalAtBatsWritten = WriteAtBats(game.AtBats);
                     else
                        Logger.Log.WarnFormat("Not writing at bats for gid: {0}, already has {1} at bats loaded. Would have tried saving: {2} at bats", gid, atBatsLoaded, game.AtBats.Count);

                     if (pitchesLoaded != game.Pitches.Count)
                        totalPitchesWritten = WritePitches(game.Pitches);
                     else
                        Logger.Log.WarnFormat("Not writing pitches for gid: {0}, already has {1} pitches loaded. Would have tried saving: {2} pitches", gid, pitchesLoaded, game.Pitches.Count);

                     if (runnersLoaded != game.Runners.Count)
                        totalRunnersWritten = WriteRunners(game.Runners);
                     else
                        Logger.Log.WarnFormat("Not writing runners for gid: {0}, already has {1} runners loaded. Would have tried saving: {2} runners", gid, runnersLoaded, game.Runners.Count);
                     break;
                  case DownloadFile.InfoToStoreEnum.Players:
                     if (battersLoaded != game.Batters.Count)
                        totalBattersWritten = WriteBatters(game.Batters);
                     else
                        Logger.Log.WarnFormat("Not writing batters for gid: {0}, already has {1} batters loaded. Would have tried saving: {2} batters", gid, battersLoaded, game.Batters.Count);

                     if (pitchersLoaded != game.Pitchers.Count)
                        totalPitchersWritten = WritePitchers(game.Pitchers);
                     else
                        Logger.Log.WarnFormat("Not writing pitchers for gid: {0}, already has {1} pitchers loaded. Would have tried saving: {2} pitchers", gid, pitchersLoaded, game.Pitchers.Count);
                     break;
               }

               var str = string.Format("{0}: Wrote {1}/{2} Atbats, {3}/{4} Pitches, {5}/{6} Runners, {7}/{8} Batters, {9}/{10} Pitchers",
                  game.Gid, totalAtBatsWritten, totalAtBats, totalPitchesWritten, totalPitches, totalRunnersWritten, totalRunners, totalBattersWritten, totalBatters, totalPitchersWritten, totalPitchersWritten);
               Logger.Log.Info(str);

               var errorStr = new StringBuilder();

               if (totalAtBats != totalAtBatsWritten)
               {
                  errorStr.AppendLine("Not all at bats written for Gid: " + game.Gid);
               }
               if (totalPitches != totalPitchesWritten)
               {
                  errorStr.AppendLine("Not all pitches written for Gid: " + game.Gid);
               }
               if (totalRunners != totalRunnersWritten)
               {
                  errorStr.AppendLine("Not all runners written for Gid: " + game.Gid);
               }

               if (totalBatters != totalBattersWritten)
               {
                  errorStr.AppendLine("Not all batters written for Gid: " + game.Gid);
               }

               if (totalPitchers != totalPitchersWritten)
               {
                  errorStr.AppendLine("Not all pitchers written for Gid: " + game.Gid);
               }

               if (errorStr.Length != 0)
                  Logger.Log.Warn(errorStr.ToString());

            }
            catch (Exception ex)
            {
               Logger.LogException(ex);
            }
         }
      }

      private static void WriteGame(Game game)
      {
         try
         {
            var connection = DbConnector.Instance.SqlConnection;
            var cmd = connection.CreateCommand();
            const string sql = @"insert into games
                                 (game_primarykey, gid, home_team, away_team, game_date, home_code, away_code, home_abbrev, away_abbrev,home_id, away_id)
                                 values
                                 (@game_primarykey,@gid,@home_team,@away_team,@game_date,@home_code,@away_code,@home_abbrev,@away_abbrev,@home_id,@away_id)
                                ";
            cmd.CommandText = sql;
            cmd.Parameters.AddWithValue("@game_primarykey", game.GamePrimaryKey);
            cmd.Parameters.AddWithValue("@gid", game.Gid);
            cmd.Parameters.AddWithValue("@home_team", game.Home.NameFull);
            cmd.Parameters.AddWithValue("@away_team", game.Away.NameFull);
            cmd.Parameters.AddWithValue("@game_date", game.GameDate);
            cmd.Parameters.AddWithValue("@home_code", game.Home.Code);
            cmd.Parameters.AddWithValue("@away_code", game.Away.Code);
            cmd.Parameters.AddWithValue("@home_abbrev", game.Home.Abbrev);
            cmd.Parameters.AddWithValue("@away_abbrev", game.Away.Abbrev);
            cmd.Parameters.AddWithValue("@home_id", game.Home.Id);
            cmd.Parameters.AddWithValue("@away_id", game.Away.Id);

            var results = cmd.ExecuteNonQuery();
            if (results != 1)
               throw new Exception("Could not write game: " + game.ToString());
            else
               game.IsGameSaved = true;
         }
         catch (Exception ex)
         {
            Logger.LogException(ex);
         }
      }

      public static void UpdateGamePitchesAndAtBats(Game game)
      {
         try
         {
            if (game.TotalAtBats.HasValue && game.TotalPitches.HasValue)
            {
               var connection = DbConnector.Instance.SqlConnection;
               var cmd = connection.CreateCommand();
               const string sql = @"update games 
                                 set total_atbats = @total_atbats, total_pitches = @total_pitches
                                 where game_primarykey = @game_primarykey";
               cmd.CommandText = sql;
               cmd.Parameters.AddWithValue("@total_atbats", game.TotalAtBats.Value);
               cmd.Parameters.AddWithValue("@total_pitches", game.TotalPitches.Value);
               cmd.Parameters.AddWithValue("@game_primarykey", game.GamePrimaryKey);

               var results = cmd.ExecuteNonQuery();
               if (results != 1)
                  throw new Exception("Could not update game: " + game.ToString());

               Logger.Log.InfoFormat("Updated gid: {0} with total atbats: {1} and total pitches:{2}", game.Gid, game.TotalAtBats, game.TotalPitches);
            }
            else
            {
               Logger.Log.WarnFormat("Not updating gid: {0}, missing total atbats or total pitches", game.Gid);
            }

         }
         catch (Exception ex)
         {
            Logger.LogException(ex);
         }
      }

      private static int WriteAtBats(IEnumerable<AtBat> atBats)
      {
         var connection = DbConnector.Instance.SqlConnection;
         return atBats.Sum(atBat => WriteAtBat(connection, atBat));
      }

      private static int WriteAtBat(MySqlConnection connection, AtBat atBat)
      {
         try
         {
            var cmd = connection.CreateCommand();
            const string sql = @"
                     insert into atbats
                        (ab_guid, gid, game_primarykey, num, balls, strikes, outs, start_tfs, start_tfs_zulu, batter, stand, pitcher,
                         b_height, p_throws, des, event_num, event, home_team_runs, away_team_runs, inning, inning_type)
                        values
                        (@ab_guid, @gid, @game_primarykey, @num, @balls, @strikes, @outs, @start_tfs, @start_tfs_zulu, @batter, @stand, @pitcher,
                         @b_height, @p_throws, @des, @event_num, @event, @home_team_runs, @away_team_runs, @inning, @inning_type)";

            cmd.CommandText = sql;
            cmd.Parameters.AddWithValue("@ab_guid", atBat.AtBatGuid);
            cmd.Parameters.AddWithValue("@gid", atBat.Gid);
            cmd.Parameters.AddWithValue("@game_primarykey", atBat.GamePrimaryKey);
            cmd.Parameters.AddWithValue("@num", atBat.Num);
            cmd.Parameters.AddWithValue("@balls", atBat.Balls);
            cmd.Parameters.AddWithValue("@strikes", atBat.Strikes);
            cmd.Parameters.AddWithValue("@outs", atBat.Outs);
            cmd.Parameters.AddWithValue("@start_tfs", atBat.StartTfs);
            cmd.Parameters.AddWithValue("@start_tfs_zulu", atBat.StartTfsZulu);
            cmd.Parameters.AddWithValue("@batter", atBat.Batter);
            cmd.Parameters.AddWithValue("@stand", atBat.Stand);
            cmd.Parameters.AddWithValue("@pitcher", atBat.Pitcher);
            cmd.Parameters.AddWithValue("@b_height", atBat.BHeight);
            cmd.Parameters.AddWithValue("@p_throws", atBat.PThrows);
            cmd.Parameters.AddWithValue("@des", atBat.Des);
            cmd.Parameters.AddWithValue("@event_num", atBat.EventNum);
            cmd.Parameters.AddWithValue("@event", atBat.Event);
            cmd.Parameters.AddWithValue("@home_team_runs", atBat.HomeTeamRuns);
            cmd.Parameters.AddWithValue("@away_team_runs", atBat.AwayTeamRuns);
            cmd.Parameters.AddWithValue("@inning", atBat.Inning);
            cmd.Parameters.AddWithValue("@inning_type", atBat.InningType);

            var results = cmd.ExecuteNonQuery();
            if (results != 1)
               throw new Exception("Could not write ab: " + atBat);
            else
               atBat.IsAtBatSaved = true;
         }
         catch (Exception ex)
         {
            Logger.LogException(ex);
            return 0;
         }
         return 1;
      }

      private static int WritePitches(IEnumerable<Pitch> pitches)
      {
         var connection = DbConnector.Instance.SqlConnection;
         return pitches.Sum(pitch => WritePitch(connection, pitch));
      }

      private static int WritePitch(MySqlConnection connection, Pitch pitch)
      {
         try
         {
            var cmd = connection.CreateCommand();
            #region Pitch Sql

            const string sql = @"
                     insert into pitches
                        (pitch_guid, ab_guid, gid, game_primarykey, des, id, type, tfs, tfs_zulu, x, y, event_num, sv_id, start_speed, end_speed, sz_top, sz_bot,
                         pfx_x, pfx_z, vx0 ,vy0, vz0, px, pz, x0, y0, z0, ax, ay, az, break_y, break_length, break_angle, pitch_type, type_confidence, zone, nasty, 
                         spin_dir, spin_rate, pitcher)
                     values
                        (@pitch_guid, @ab_guid, @gid, @game_primarykey, @des, @id, @type, @tfs, @tfs_zulu, @x, @y, @event_num, @sv_id, @start_speed, @end_speed, @sz_top, @sz_bot,
                         @pfx_x, @pfx_z, @vx0 ,@vy0, @vz0, @px, @pz, @x0, @y0, @z0, @ax, @ay, @az, @break_y, @break_length, @break_angle, @pitch_type, @type_confidence, @zone, @nasty, 
                         @spin_dir, @spin_rate, @pitcher)";

            cmd.CommandText = sql;
            cmd.Parameters.AddWithValue("@pitch_guid", pitch.PitchGuid);
            cmd.Parameters.AddWithValue("@ab_guid", pitch.AtBatGuid);
            cmd.Parameters.AddWithValue("@gid", pitch.GameId);
            cmd.Parameters.AddWithValue("@game_primarykey", pitch.GamePrimaryKey);
            cmd.Parameters.AddWithValue("@des", pitch.Des);
            cmd.Parameters.AddWithValue("@id", pitch.Id);
            cmd.Parameters.AddWithValue("@type", pitch.Type);
            cmd.Parameters.AddWithValue("@tfs", pitch.Tfs);
            cmd.Parameters.AddWithValue("@tfs_zulu", pitch.TfsZlu);
            cmd.Parameters.AddWithValue("@x", pitch.X);
            cmd.Parameters.AddWithValue("@y", pitch.Y);
            cmd.Parameters.AddWithValue("@event_num", pitch.EventNum);
            cmd.Parameters.AddWithValue("@sv_id", pitch.SvId);
            cmd.Parameters.AddWithValue("@start_speed", pitch.StartSpeed);
            cmd.Parameters.AddWithValue("@end_speed", pitch.EndSpeed);
            cmd.Parameters.AddWithValue("@sz_top", pitch.SzTop);
            cmd.Parameters.AddWithValue("@sz_bot", pitch.SzBot);
            cmd.Parameters.AddWithValue("@pfx_x", pitch.PfxX);
            cmd.Parameters.AddWithValue("@pfx_z", pitch.PfxZ);
            cmd.Parameters.AddWithValue("@vx0", pitch.Vx0);
            cmd.Parameters.AddWithValue("@vy0", pitch.Vy0);
            cmd.Parameters.AddWithValue("@vz0", pitch.Vz0);
            cmd.Parameters.AddWithValue("@px", pitch.Px);
            cmd.Parameters.AddWithValue("@pz", pitch.Pz);
            cmd.Parameters.AddWithValue("@x0", pitch.X0);
            cmd.Parameters.AddWithValue("@y0", pitch.Y0);
            cmd.Parameters.AddWithValue("@z0", pitch.Z0);
            cmd.Parameters.AddWithValue("@ax", pitch.Ax);
            cmd.Parameters.AddWithValue("@ay", pitch.Ay);
            cmd.Parameters.AddWithValue("@az", pitch.Az);
            cmd.Parameters.AddWithValue("@break_y", pitch.BreakY);
            cmd.Parameters.AddWithValue("@break_length", pitch.BreakLength);
            cmd.Parameters.AddWithValue("@break_angle", pitch.BreakAngle);
            cmd.Parameters.AddWithValue("@pitch_type", pitch.PitchType);
            cmd.Parameters.AddWithValue("@type_confidence", pitch.TypeConfidence);
            cmd.Parameters.AddWithValue("@zone", pitch.Zone);
            cmd.Parameters.AddWithValue("@nasty", pitch.Nasty);
            cmd.Parameters.AddWithValue("@spin_dir", pitch.SpinDir);
            cmd.Parameters.AddWithValue("@spin_rate", pitch.SpinRate);
            cmd.Parameters.AddWithValue("@pitcher", pitch.Pitcher);

            #endregion

            var results = cmd.ExecuteNonQuery();
            if (results != 1)
               throw new Exception("Could not write pitch: " + pitch);
            else
               pitch.IsPitchSaved = true;
         }
         catch (Exception ex)
         {
            Logger.LogException(ex);
            return 0;
         }
         return 1;
      }

      private static int WriteRunners(IEnumerable<Runner> runners)
      {
         var connection = DbConnector.Instance.SqlConnection;
         return runners.Sum(runner => WriteRunner(connection, runner));
      }

      private static int WriteRunner(MySqlConnection connection, Runner runner)
      {
         try
         {
            var cmd = connection.CreateCommand();
            const string sql = @"
                           insert into runners
                              (runner_guid, ab_guid, game_primarykey, id, start, end, event, event_num, score, rbi, earned)
                           values
                              (@runner_guid, @ab_guid, @game_primarykey, @id, @start, @end, @event, @event_num, @score, @rbi, @earned)";

            cmd.CommandText = sql;
            cmd.Parameters.AddWithValue("@runner_guid", runner.RunnerGuid);
            cmd.Parameters.AddWithValue("@ab_guid", runner.AtBatGuid);
            cmd.Parameters.AddWithValue("@game_primarykey", runner.AtBatGuid);
            cmd.Parameters.AddWithValue("@id", runner.Id);
            cmd.Parameters.AddWithValue("@start", runner.Start);
            cmd.Parameters.AddWithValue("@end", runner.End);
            cmd.Parameters.AddWithValue("@event", runner.Event);
            cmd.Parameters.AddWithValue("@event_num", runner.EventNum);
            cmd.Parameters.AddWithValue("@score", runner.Score);
            cmd.Parameters.AddWithValue("@rbi", runner.Rbi);
            cmd.Parameters.AddWithValue("@earned", runner.Earned);

            var results = cmd.ExecuteNonQuery();
            if (results != 1)
               throw new Exception("Could not write runner: " + runner);
            else
               runner.IsRunnerSaved = true;
         }
         catch (Exception ex)
         {
            Logger.LogException(ex);
            return 0;
         }
         return 1;
      }

      private static int WritePitchers(IEnumerable<Player> pitchers)
      {
         var connection = DbConnector.Instance.SqlConnection;
         return pitchers.Sum(pitcher => WritePlayer(connection, pitcher));
      }

      private static int WriteBatters(IEnumerable<Player> batters)
      {
         var connection = DbConnector.Instance.SqlConnection;
         return batters.Sum(batter => WritePlayer(connection, batter));
      }

      private static int WritePlayer(MySqlConnection connection, Player player)
      {
         if (_playerIdsWritten.Contains(player.Id) || Getter.AllPlayers.Contains(player.Id))
         {
            //Logger.Log.WarnFormat("Player Id: {0} already written...",player.Id);
            return 1;
         }

         try
         {
            var cmd = connection.CreateCommand();
            const string sql = @"insert into players
                                 (id, type, first_name, last_name, full_name, pos, bats, throws)
                                 values
                                 (@id, @type, @first_name, @last_name, @full_name, @pos, @bats, @throws)";
            cmd.CommandText = sql;
            cmd.Parameters.AddWithValue("@id", player.Id);
            cmd.Parameters.AddWithValue("@type", player.Type);
            cmd.Parameters.AddWithValue("@first_name", player.FirstName);
            cmd.Parameters.AddWithValue("@last_name", player.LastName);
            cmd.Parameters.AddWithValue("@full_name", player.FullName);
            cmd.Parameters.AddWithValue("@pos", player.Pos);
            cmd.Parameters.AddWithValue("@bats", player.Bats);
            cmd.Parameters.AddWithValue("@throws", player.Throws);

            var results = cmd.ExecuteNonQuery();
            if (results != 1)
               throw new Exception("Could not write player: " + player);
            else
            {
               player.IsPlayersSaved = true;
               _playerIdsWritten.Add(player.Id);
            }

         }
         catch (Exception ex)
         {
            Logger.LogException(ex);
            return 0;
         }
         return 1;
      }

      private static void WriteAction(Action abAction)
      {
         try
         {

         }
         catch (Exception ex)
         {
            Logger.LogException(ex);
         }
      }

   }
}
