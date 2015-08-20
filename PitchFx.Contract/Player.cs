using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;
using COB.LogWrapper;
using log4net;

namespace PitchFx.Contract
{
   public sealed class Player
   {

      #region Col Names
      private const string IdCol = "id";
      private const string TypeCol = "type";
      private const string FirstNameCol = "first_name";
      private const string LastNameCol = "last_name";
      private const string FullNameCol = "full_name";
      private const string PosCol = "pos";
      private const string BatsCol = "bats";
      private const string ThrowsCol = "throws";
      #endregion

      public Player()
      {
         
      }

      public Player(DataRow row)
      {
         try
         {
            Id = Convert.ToInt64(row[IdCol].ToString());
            Type = row[TypeCol].ToString();
            FirstName = row[FirstNameCol].ToString();
            LastName = row[LastNameCol].ToString();
            FullName = row[FullNameCol].ToString();
            Pos = row[PosCol].ToString();
            Bats = row[BatsCol].ToString();
            Throws = row[ThrowsCol].ToString();
            IsDeserializedFromDb = true;
         }
         catch (Exception ex)
         {
            Logger.Log.ErrorFormat("Could not serialize row into Player object: {0}", row.ToString());
            Logger.LogException(ex);
         }
      }

      public override string ToString()
      {
         var str = string.Format("Id: {0}, Type: {1}, FullName: {2}, Pos: {3}, Bats: {4}, Throws: {5}",Id,Type,FullName,Pos,Bats,Throws);
         return str;
      }

      [XmlAttribute("id")]
      public long Id { get; set; }

      /// <summary>
      /// batter or pitcher
      /// </summary>
      [XmlAttribute("type")]
      public string Type { get; set; }

      [XmlAttribute("first_name")]
      public string FirstName { get; set; }

      [XmlAttribute("last_name")]
      public string LastName { get; set; }

      public string FullName { get; set; }

      [XmlAttribute("pos")]
      public string Pos { get; set; }

      [XmlAttribute("bats")]
      public string Bats { get; set; }

      [XmlAttribute("throws")]
      public string Throws { get; set; }

      public void SetFullName()
      {
         if (!string.IsNullOrEmpty(FirstName) && !string.IsNullOrEmpty(LastName))
         {
            FullName = FirstName + " " + LastName;
         }
      }

      public bool IsPlayersSaved { get; set; }

      public bool IsDeserializedFromDb { get; set; }

   }
}
