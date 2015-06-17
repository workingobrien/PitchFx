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
using Action = PitchFx.Contract.Action;

namespace Db.Utilities
{
   public static class Writer
   {

      public static void UpdateTableWithGameInfo(ConcurrentDictionary<long, Game> allGames)
      {
         Logger.Log.InfoFormat("Attempting to save the following to the database:");
         Logger.Log.InfoFormat("Games: {0}", allGames.Count);
         Logger.Log.InfoFormat("At Bats: {0}", allGames.Values.SelectMany(g => g.AtBats).ToList().Count);
         Logger.Log.InfoFormat("Pitches: {0}", allGames.Values.SelectMany(g => g.Pitches).ToList().Count);
         Logger.Log.InfoFormat("Runners: {0}", allGames.Values.SelectMany(g => g.Runners).ToList().Count);

         foreach (var game in allGames.Values)
         {
            try
            {
               var totalAtBats = game.AtBats.Count;
               var totalPitches = game.Pitches.Count;
               var totalRunners = game.Runners.Count;

               WriteGame(game);
               var totalAtBatsWritten = WriteAtBats(game.AtBats);
               var totalPitchesWritten = WritePitches(game.Pitches);
               var totalRunnersWritten = WriteRunners(game.Runners);

               var str = string.Format("{0}: Wrote {1}/{2} Atbats, {3}/{4} Pitches, {5}/{6} Runners",
                  game.Gid, totalAtBatsWritten, totalAtBats, totalPitchesWritten, totalPitches, totalRunnersWritten, totalRunners);
               Logger.Log.Info(str);

               if (totalAtBats != totalAtBatsWritten)
               {
                  throw new Exception("Not all at bats written for Gid: " + game.Gid);
               }
               if (totalPitches != totalPitchesWritten)
               {
                  throw new Exception("Not all pitches written for Gid: " + game.Gid);
               }
               if (totalRunners != totalRunnersWritten)
               {
                  throw new Exception("Not all runners written for Gid: " + game.Gid);
               }
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
                         pfx_x, pfx_y, vx0 ,vy0, vz0, px, pz, x0, y0, z0, ax, ay, az, break_y, break_length, break_angle, pitch_type, type_confidence, zone, nasty, 
                         spin_dir, spin_rate, pitcher)
                     values
                        (@pitch_guid, @ab_guid, @gid, @game_primarykey, @des, @id, @type, @tfs, @tfs_zulu, @x, @y, @event_num, @sv_id, @start_speed, @end_speed, @sz_top, @sz_bot,
                         @pfx_x, @pfx_y, @vx0 ,@vy0, @vz0, @px, @pz, @x0, @y0, @z0, @ax, @ay, @az, @break_y, @break_length, @break_angle, @pitch_type, @type_confidence, @zone, @nasty, 
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
            cmd.Parameters.AddWithValue("@pfx_y", pitch.PfxY);
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
