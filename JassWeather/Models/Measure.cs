using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace JassWeather.Models
{
    public class JassMeasure
    {
        public int JassMeasureID { get; set; }
        public int x { get; set; }
        public int y { get; set; }
        public int day { get; set; }
        public int hour3 { get; set; }
        public string level {get;set;}
    }
}