using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using COB.LogWrapper;
using Db.Utilities;
using PitchFx.Contract;
using PitchFx.Contract.Reports;
using PitchFxDataImporter;

namespace DisplayPitchFx
{
   public partial class FrmPitchFx : Form
   {
      private const string PitcherCol = "Pitcher";
      private const string SpeedAvgCol = "SpeedAvg";
      private const string SpeedStndDevCol = "SpeedStndDev";
      private const string BreakLenAvgCol = "BreakLenAvg";
      private const string BreakLenStndDevCol = "BreakLenStndDev";
      private const string HorizontalMovementCol = "HorizontalMovement";
      private const string HorizontalStndDevCol = "HorizontalStndDev";
      private const string SpinDirAvgCol = "SpinDirAvg";
      private const string SpinDirStndDevCol = "SpinDirStndDev";
      private const string SpinRateAvg = "SpinRateAvg";
      private const string SpinRateStndDev = "SpinRateStndDev";
      private const string PitchTypeCol = "PitchType";
      private const string TotalPitchesCol = "TotalPitches";

      public ConcurrentDictionary<long, Game> Games;
      public BindingList<MedianStandardDev> PitchStats;
      public List<MedianStandardDev> SortablePitchStats;

      public FrmPitchFx()
      {
         Logger.Log.InfoFormat("## \\/ ## Starting PitchFx Form Application ## \\/ ##");
         InitializeComponent();
         var pitches = new List<object> { "ALL", "FF", "SL", "FT", "CH", "FS", "CU", "FA", "FC", "SI", "KC", "IN", "PO", "FO", "EP", "AB", "SC" };
         var columns = new List<object>();
         columns.Add("Pitcher");
         columns.Add("SpeedAvg");
         columns.Add("SpeedStndDev");
         columns.Add("BreakLenAvg");
         columns.Add("BreakLenStndDev");
         columns.Add("HorizontalMovement");
         columns.Add("HorizontalStndDev");
         columns.Add("SpinDirAvg");
         columns.Add("SpinDirStndDev");
         columns.Add("SpinRateAvg");
         columns.Add("SpinRateStndDev");
         columns.Add("PitchType");
         columns.Add("TotalPitches");

         cbPitches.Items.AddRange(pitches.ToArray());
         cbPitches.SelectedIndex = 0;

         cbSortColumns.Items.AddRange(columns.ToArray());
         cbSortColumns.SelectedIndex = 0;

         cbOrderBy.Items.Add("Descend");
         cbOrderBy.Items.Add("Ascend");
         cbOrderBy.SelectedIndex = 0;

      }

      private void btnGetData_Click(object sender, EventArgs e)
      {
         try
         {
            PitchStats = new SortableSearchableList<MedianStandardDev>();
            btnGetData.Enabled = false;
            lblStatus.Text = "Getting Data.. please wait";
            var bgWorker = new BackgroundWorker();
            bgWorker.DoWork += BgWorkerOnDoWork;
            bgWorker.RunWorkerCompleted += BgWorkerOnRunWorkerCompleted;
            bgWorker.WorkerSupportsCancellation = true;
            bgWorker.RunWorkerAsync();

         }
         catch (Exception ex)
         {
            Logger.LogException(ex);
            btnGetData.Enabled = true;
         }
      }

      private void BgWorkerOnDoWork(object sender, DoWorkEventArgs e)
      {
         try
         {
            var since = dtpSince.Value;
            var until = dtpUntil.Value;
            var str = string.Format("Attempting to load Games from db...");
            LogFormMessage(str);
            var games = Getter.GetGamesByDate(since, until);

            str = string.Format("Loaded {0} games... Now loading At Bats.", games.Count);
            LogFormMessage(str);
            var atBats = Getter.GetAtBatsByDate(since, until);

            str = string.Format("Loaded {0} at bats... Now loading Pitches.", atBats.Count);
            LogFormMessage(str);

            long minGamePrimaryKey = 0;
            long maxGamePrimaryKey = 0;

            Games = Importer.Instance.LinkDeserializeAtBats(games, atBats, ref minGamePrimaryKey, ref  maxGamePrimaryKey);

            var pitches = Getter.GetPitchesByPrimaryKey(minGamePrimaryKey, maxGamePrimaryKey);

            Importer.Instance.LinkDeserializedPitches(Games, pitches);

            str = string.Format("Loaded {0} pitches... Finished.", pitches.Count);
            LogFormMessage(str);

         }
         catch (Exception ex)
         {
            Logger.LogException(ex);
            e.Cancel = true;
         }
      }

      private void BgWorkerOnRunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
      {
         var pitchStats = new List<MedianStandardDev>();
         try
         {
            btnGetData.Enabled = true;
            if (e.Error != null)
               throw new Exception("BgWorker!", e.Error);


            var allAtBats = Games.Values.SelectMany(g => g.AtBats).ToList();
            var allPitches = allAtBats.SelectMany(p => p.Pitches).ToList();

            Dictionary<long, List<Pitch>> groupedPitches;
            if (cbPitches.SelectedIndex == 0)
            {
               groupedPitches = allPitches.GroupBy(p => p.Pitcher).ToDictionary(k => k.Key, v => v.ToList());
            }
            else
            {
               var selectedPitchType = cbPitches.SelectedItem.ToString();
               groupedPitches = allPitches.Where(p => p.PitchType == selectedPitchType).GroupBy(p => p.Pitcher).ToDictionary(k => k.Key, v => v.ToList());
            }


            foreach (var pitchesByPitcher in groupedPitches)
            {
               var pitches = pitchesByPitcher.Value;
               if (pitches.Count >= Convert.ToInt32(txtPitchMin.Text))
               {
                  List<List<Pitch>> pitchesByType = pitches.GroupBy(p => p.PitchType).ToDictionary(k => k.Key, v => v.ToList()).Values.ToList();
                  foreach (var allPitchesOfTypes in pitchesByType)
                  {
                     if (allPitchesOfTypes.Count >= Convert.ToInt32(txtPitchMin.Text))
                        pitchStats.Add(new MedianStandardDev(allPitchesOfTypes));
                  }
               }
            }

         }
         catch (Exception ex)
         {
            Logger.LogException(ex);
         }
         SortablePitchStats = pitchStats;
         var sortColumn = cbSortColumns.SelectedItem.ToString();
         this.Invoke(new EventHandler((o, args) => LoadPitches(SortablePitchStats, sortColumn)));
      }

      private void LoadPitches(IEnumerable<MedianStandardDev> pitchStats, string sortColumn)
      {
         try
         {
            PitchStats.Clear();
            foreach (var pitchStat in GetSortedPitches(pitchStats, sortColumn))
               PitchStats.Add(pitchStat);

            dgvMain.DataSource = PitchStats;
            dgvMain.Refresh();

         }
         catch (Exception ex)
         {
            Logger.LogException(ex);
         }
      }

      private void btnSort_Click(object sender, EventArgs e)
      {
         var sortColumn = cbSortColumns.SelectedItem.ToString();
         LoadPitches(SortablePitchStats, sortColumn);
      }

      private IEnumerable<MedianStandardDev> GetSortedPitches(IEnumerable<MedianStandardDev> pitchStats, string sortColumn)
      {
         var orderbyStr = cbOrderBy.SelectedItem.ToString();
         var isAscendIng = orderbyStr != "Descend";

         switch (sortColumn)
         {
            case PitcherCol:
               if (isAscendIng)
                  return pitchStats.OrderBy(x => x.Pitcher).ToList();
               return pitchStats.OrderByDescending(x => x.Pitcher).ToList();
            case SpeedAvgCol:
               if (isAscendIng)
                  return pitchStats.OrderBy(x => x.SpeedAvg).ToList();
               return pitchStats.OrderByDescending(x => x.SpeedAvg).ToList();
            case SpeedStndDevCol:
               if (isAscendIng)
                  return pitchStats.OrderBy(x => x.SpeedStndDev).ToList();
               return pitchStats.OrderByDescending(x => x.SpeedStndDev).ToList();
            case BreakLenAvgCol:
               if (isAscendIng)
                  return pitchStats.OrderBy(x => x.BreakLenAvg).ToList();
               return pitchStats.OrderByDescending(x => x.BreakLenAvg);
            case BreakLenStndDevCol:
               if (isAscendIng)
                  return pitchStats.OrderBy(x => x.BreakLenStndDev).ToList();
               return pitchStats.OrderByDescending(x => x.BreakLenStndDev);
            case HorizontalMovementCol:
               if (isAscendIng)
                  return pitchStats.OrderBy(x => x.HorizontalMovementAvg).ToList();
               return pitchStats.OrderByDescending(x => x.HorizontalMovementAvg);
            case HorizontalStndDevCol:
               if (isAscendIng)
                  return pitchStats.OrderBy(x => x.HorizontalStndDev).ToList();
               return pitchStats.OrderByDescending(x => x.HorizontalStndDev);
            case SpinDirAvgCol:
               if (isAscendIng)
                  return pitchStats.OrderBy(x => x.SpinDirAvg).ToList();
               return pitchStats.OrderByDescending(x => x.SpinDirAvg);
            case SpinDirStndDevCol:
               if (isAscendIng)
                  return pitchStats.OrderBy(x => x.SpinDirStndDev).ToList();
               return pitchStats.OrderByDescending(x => x.SpinDirStndDev);
            case SpinRateAvg:
               if (isAscendIng)
                  return pitchStats.OrderBy(x => x.SpinRateAvg).ToList();
               return pitchStats.OrderByDescending(x => x.SpinRateAvg);
            case SpinRateStndDev:
               if (isAscendIng)
                  return pitchStats.OrderBy(x => x.SpinRateStndDev).ToList();
               return pitchStats.OrderByDescending(x => x.SpinRateStndDev);
            case PitchTypeCol:
               if (isAscendIng)
                  return pitchStats.OrderBy(x => x.PitchType).ToList();
               return pitchStats.OrderByDescending(x => x.PitchType);
            case TotalPitchesCol:
               if (isAscendIng)
                  return pitchStats.OrderBy(x => x.TotalPitches).ToList();
               return pitchStats.OrderByDescending(x => x.TotalPitches);

         }
         return null;
      }



      private void LogFormMessage(string str)
      {
         Logger.Log.Info(str);
         this.Invoke(new MethodInvoker(delegate { this.lblStatus.Text = str; }));
      }
   }
}
