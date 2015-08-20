using System;
using System.Collections.Generic;
using System.ComponentModel.Design;
using System.Data;
using System.Dynamic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;
using COB.LogWrapper;

namespace PitchFx.Contract
{
   [XmlRoot("pitch")]
   public sealed class Pitch
   {
      private const string DesCol = "des";
      private const string IdCol = "id";
      private const string TypeCol = "type";
      private const string TfsCol = "tfs";
      private const string TfsZuluCol = "tfs_zulu";
      private const string XCol = "x";
      private const string YCol = "y";
      private const string EventNumCol = "event_num";
      private const string SvIdCol = "sv_id";
      private const string StartSpeeddCol = "start_speed";
      private const string EndSpeedCol = "end_speed";
      private const string SzTopCol = "sz_top";
      private const string SzBotCol = "sz_bot";
      private const string PfxXCol = "pfx_x";
      private const string PfxZCol = "pfx_z";
      private const string PxCol = "px";
      private const string PzCol = "pz";
      private const string X0Col = "x0";
      private const string Y0Col = "y0";
      private const string Z0Col = "z0";
      private const string Vx0Col = "vx0";
      private const string Vy0Col = "vy0";
      private const string Vz0Col = "vz0";
      private const string AxCol = "ax";
      private const string AyCol = "ay";
      private const string AzCol = "az";
      private const string BreakYCol = "break_y";
      private const string BreakLengthCol = "break_length";
      private const string BreakAngleCol = "break_angle";
      private const string PitchTypeCol = "pitch_type";
      private const string TypeConfidenceCol = "type_confidence";
      private const string ZoneCol = "zone";
      private const string NastyCol = "nasty";
      private const string SpinDirCol = "spin_dir";
      private const string SpinRateCol = "spin_rate";
      private const string PitcherCol = "pitcher";

      private const string GamePrimaryKeyCol = "game_primaryKey";
      private const string GidCol = "gid";
      private const string AtBatGuidCol = "ab_guid";
      private const string PitchGuidCol = "pitch_guid";

      public Pitch()
      {

      }

      public Pitch(DataRow row)
      {
         try
         {
            Des = row[DesCol].ToString();
            Id = Convert.ToInt32(row[IdCol].ToString());
            Type = row[TypeCol].ToString();
            Tfs = row[TfsCol].ToString();
            TfsZlu = row[TfsZuluCol].ToString();
            X = GetDoubleValue(row[XCol].ToString());
            Y = GetDoubleValue(row[YCol].ToString());
            EventNum = row[EventNumCol].ToString();
            SvId = row[SvIdCol].ToString();
            StartSpeed = GetDoubleValue(row[StartSpeeddCol].ToString());
            EndSpeed = GetDoubleValue(row[EndSpeedCol].ToString());
            SzTop = GetDoubleValue(row[SzTopCol].ToString());
            SzBot = GetDoubleValue(row[SzBotCol].ToString());
            PfxX = GetDoubleValue(row[PfxXCol].ToString());
            PfxZ = GetDoubleValue(row[PfxZCol].ToString());
            Px = GetDoubleValue(row[PxCol].ToString());
            Pz = GetDoubleValue(row[PzCol].ToString());
            X0 = GetDoubleValue(row[X0Col].ToString());
            Y0 = GetDoubleValue(row[Y0Col].ToString());
            Z0 = GetDoubleValue(row[Z0Col].ToString());
            Vx0 = GetDoubleValue(row[Vx0Col].ToString());
            Vy0 = GetDoubleValue(row[Vz0Col].ToString());
            Vz0 = GetDoubleValue(row[Vz0Col].ToString());
            Ax = GetDoubleValue(row[AxCol].ToString());
            Ay = GetDoubleValue(row[AyCol].ToString());
            Az = GetDoubleValue(row[AzCol].ToString());
            BreakY = GetDoubleValue(row[BreakYCol].ToString());
            BreakLength = GetDoubleValue(row[BreakLengthCol].ToString());
            BreakAngle = GetDoubleValue(row[BreakAngleCol].ToString());
            PitchType = row[PitchTypeCol].ToString();
            TypeConfidence = GetDoubleValue(row[TypeConfidenceCol].ToString());
            Zone = GetDoubleValue(row[ZoneCol].ToString());
            Nasty = GetDoubleValue(row[ZoneCol].ToString());
            SpinDir = GetDoubleValue(row[SpinDirCol].ToString());
            SpinRate = GetDoubleValue(row[SpinRateCol].ToString());
            Pitcher = Convert.ToInt64(row[PitcherCol].ToString());

            GamePrimaryKey = Convert.ToInt64(row[GamePrimaryKeyCol].ToString());
            GameId = row[GidCol].ToString();
            AtBatGuid = row[AtBatGuidCol].ToString();
            PitchGuid = row[PitchGuidCol].ToString();
            IsDeserializedFromDb = true;
         }
         catch (Exception ex)
         {
            Logger.Log.ErrorFormat("Could not serialize row into pitcher object: {0}", row.ToString());
            Logger.LogException(ex);
         }
      }

      public override string ToString()
      {
         var str = string.Format("Pitch Guid: {0}, des: {1}, Id: {2}, Type: {3}, Gid: {4}", PitchGuid, Des, Id, Type, GameId);
         return str;
      }

      [XmlAttribute("play_guid")]
      public string PitchGuid { get; set; }

      private string _atBatGuid = string.Empty;
      public string AtBatGuid
      {
         get
         {
            if (_atBatGuid == string.Empty)
               _atBatGuid = Ab != null ? Ab.AtBatGuid : string.Empty;

            return _atBatGuid;
         }
         set { _atBatGuid = value; }
      }

      private long _gamePrimaryKey;
      public long GamePrimaryKey
      {
         get
         {
            if (_gamePrimaryKey == 0)
               _gamePrimaryKey = Ab != null ? Ab.GamePrimaryKey : 0;

            return _gamePrimaryKey;
         }
         set { _gamePrimaryKey = value; }
      }

      private string _gameId;
      public string GameId
      {
         get
         {
            if (string.IsNullOrEmpty(_gameId))
               _gameId = Ab != null ? Ab.Gid : string.Empty;
           
            return _gameId;
         }
         set { _gameId = value; }
      }

      public AtBat Ab { get; set; }

      [XmlAttribute("des")]
      public string Des { get; set; }

      [XmlAttribute("id")]
      public long Id { get; set; }

      [XmlAttribute("type")]
      public string Type { get; set; }

      /// <summary>
      /// I'm surprised Des Id doesn't exist...
      /// </summary>
      public int DesId { get; set; }

      [XmlAttribute("tfs")]
      public string Tfs { get; set; }

      [XmlAttribute("tfs_zulu")]
      public string TfsZlu { get; set; }

      [XmlAttribute("x")]
      public double X { get; set; }

      [XmlAttribute("y")]
      public double Y { get; set; }

      [XmlAttribute("event_num")]
      public string EventNum { get; set; }

      [XmlAttribute("sv_id")]
      public string SvId { get; set; }

      [XmlAttribute("start_speed")]
      public double StartSpeed { get; set; }

      [XmlAttribute("end_speed")]
      public double EndSpeed { get; set; }

      [XmlAttribute("sz_top")]
      public double SzTop { get; set; }

      [XmlAttribute("sz_bot")]
      public double SzBot { get; set; }

      [XmlAttribute("pfx_x")]
      public double PfxX { get; set; }

      [XmlAttribute("pfx_z")]
      public double PfxZ { get; set; }

      [XmlAttribute("px")]
      public double Px { get; set; }

      [XmlAttribute("pz")]
      public double Pz { get; set; }

      [XmlAttribute("x0")]
      public double X0 { get; set; }

      [XmlAttribute("y0")]
      public double Y0 { get; set; }

      [XmlAttribute("z0")]
      public double Z0 { get; set; }

      [XmlAttribute("vx0")]
      public double Vx0 { get; set; }

      [XmlAttribute("vy0")]
      public double Vy0 { get; set; }

      [XmlAttribute("vz0")]
      public double Vz0 { get; set; }

      [XmlAttribute("ax")]
      public double Ax { get; set; }

      [XmlAttribute("ay")]
      public double Ay { get; set; }

      [XmlAttribute("az")]
      public double Az { get; set; }

      [XmlAttribute("break_y")]
      public double BreakY { get; set; }

      [XmlAttribute("break_length")]
      public double BreakLength { get; set; }

      [XmlAttribute("break_angle")]
      public double BreakAngle { get; set; }

      [XmlAttribute("pitch_type")]
      public string PitchType { get; set; }

      [XmlAttribute("type_confidence")]
      public double TypeConfidence { get; set; }

      [XmlAttribute("zone")]
      public double Zone { get; set; }

      [XmlAttribute("nasty")]
      public double Nasty { get; set; }

      [XmlAttribute("spin_dir")]
      public double SpinDir { get; set; }

      [XmlAttribute("spin_rate")]
      public double SpinRate { get; set; }

      private long _pitcher;
      public long Pitcher
      {
         get
         {
            if (_pitcher != 0)
               return _pitcher;

            return Ab == null ? 0 : Ab.Pitcher;
         }
         set { _pitcher = value; }
      }

      public bool IsPitchSaved { get; set; }
      public bool IsDeserializedFromDb { get; set; }

      private double GetDoubleValue(string val)
      {
         var doubleVal = double.MinValue;
         double.TryParse(val, out doubleVal);
         return doubleVal;
      }


      public static string GeneratePitchGuid()
      {
         var guid = Guid.NewGuid();
         return "COB-PITCH-" + guid.ToString();
      }

   }
}
