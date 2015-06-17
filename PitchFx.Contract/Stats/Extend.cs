using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PitchFx.Contract.Stats
{
   public static class Extend
   {
      public static double StandardDeviation(List<double> values)
      {
         if (values == null)
            return double.MinValue;
         return Math.Sqrt(Variance(values));
      }

      public static double Variance(List<double> values)
      {
         if (values == null)
            return double.MinValue;

         var avg = values.Average();
         var sumOfSquaresDiff = values.Select(val => (val - avg) * (val - avg)).Sum();
         return sumOfSquaresDiff/values.Count;
      }

      public static double Median(List<double> values)
      {
         if (values == null)
            return double.MinValue;

         var numCnt = values.Count();
         var halfIndex = numCnt/2;
         var sortedValues = values.OrderBy(x => x);
         if ((numCnt%2) == 0)
         {
            return ((sortedValues.ElementAt(halfIndex) + sortedValues.ElementAt((halfIndex - 1)))/2);
         }
         return sortedValues.ElementAt(halfIndex);

      }
   }
}
