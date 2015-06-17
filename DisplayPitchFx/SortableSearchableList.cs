using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DisplayPitchFx
{
   public class SortableSearchableList<T> : BindingList<T>
   {
      protected override bool SupportsSortingCore
      {
         get { return true; }
      }

      bool isSortedValue;
      protected override bool IsSortedCore
      {
         get { return isSortedValue; }
      }
   }
}
