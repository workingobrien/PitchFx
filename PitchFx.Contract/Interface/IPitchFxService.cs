using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceModel;
using System.Text;
using System.Threading.Tasks;

namespace PitchFx.Contract.Interface
{
   [ServiceContract]
   public interface IPitchFxService
   {
      [OperationContract]
      void LoadYesterdaysPitchFxData();

      [OperationContract]
      void LoadPitchFxDataByDate(DateTime date);

      [OperationContract]
      void LoadPitchFxDataByDateRange(DateTime since, DateTime until);

   }
}
