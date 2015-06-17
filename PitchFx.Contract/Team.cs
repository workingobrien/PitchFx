using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace PitchFx.Contract
{
   [XmlRoot("team")]
   public class Team
   {
      public Team()
      {
         
      }

      public Team(string type, string code, string abbrev, int id, string nameFull)
      {
         Type = type;
         Code = code;
         Abbrev = abbrev;
         Id = id;
         NameFull = nameFull;
      }

      [XmlAttribute("type")]
      public string Type { get; set; }

      [XmlAttribute("code")]
      public string Code { get; set; }

      [XmlAttribute("file_code")]
      public string FileCode { get; set; }

      [XmlAttribute("id")]
      public int Id { get; set; }

      [XmlAttribute("abbrev")]
      public string Abbrev { get; set; }

      [XmlAttribute("name")]
      public string Name { get; set; }

      [XmlAttribute("name_full")]
      public string NameFull { get; set; }

      [XmlAttribute("name_brief")]
      public string NameBrief { get; set; }
      
      [XmlAttribute("w")]
      public int Wins { get; set; }

      [XmlAttribute("l")]
      public int Loses { get; set; }

      //[XmlAttribute("division_id")]
      //public int DivisionId { get; set; }

      [XmlAttribute("league_id")]
      public int LeagueId { get; set; }

      [XmlAttribute("league")]
      public string League { get; set; }
   }
}
