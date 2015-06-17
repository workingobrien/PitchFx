using System;
using System.Collections.Generic;
using System.Linq;
using System.Security;
using System.Text;
using System.Threading.Tasks;
using PitchFx.Contract.Stats;

namespace PitchFx.Contract.Reports
{
   public sealed class MedianStandardDev
   {
      public MedianStandardDev(List<Pitch> pitches)
      {
         var startSpeedValues = pitches.Select(x => x.StartSpeed).ToList();
         var breakLenValues = pitches.Select(x => x.BreakLength).ToList();
         var horizontalMovementValues = pitches.Select(x => x.PfxX).ToList();
         var spinDirValues = pitches.Select(x => x.SpinDir).ToList();
         var spinRateValues = pitches.Select(x => x.SpinRate).ToList();

         SpeedAvg = Extend.Median(startSpeedValues);
         SpeedStndDev = Extend.StandardDeviation(startSpeedValues);

         HorizontalMovementAvg = Extend.Median(horizontalMovementValues);
         HorizontalStndDev = Extend.StandardDeviation(horizontalMovementValues);

         BreakLenAvg = Extend.Median(breakLenValues);
         BreakLenStndDev = Extend.StandardDeviation(breakLenValues);

         SpinDirAvg = Extend.Median(spinDirValues);
         SpinDirStndDev = Extend.StandardDeviation(spinDirValues);

         SpinRateAvg = Extend.Median(spinRateValues);
         SpinRateStndDev = Extend.StandardDeviation(spinRateValues);

         var firstPitch= pitches.FirstOrDefault();
         PitchType = firstPitch == null ? string.Empty : firstPitch.PitchType;
         Pitcher = firstPitch == null ? 0: firstPitch.Pitcher;
         TotalPitches = pitches.Count;

      }
      
      public long Pitcher { get; set; }
      public double SpeedAvg { get; set; }
      public double SpeedStndDev { get; set; }
      
      public double BreakLenAvg { get; set; }
      public double BreakLenStndDev { get; set; }
      
      public double HorizontalMovementAvg { get; set; }
      public double HorizontalStndDev { get; set; }

      public double SpinDirAvg { get; set; }
      public double SpinDirStndDev { get; set; }

      public double SpinRateAvg { get; set; }
      public double SpinRateStndDev { get; set; }

      public string PitchType { get; set; }
      public double TotalPitches { get; set; }

   }
}
