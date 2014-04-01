using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace JassWeather.Models
{
    public class JassLatLon
    {
        public int JassLatLonID { get; set; }
        public string Name { get; set; }
        public string StationCode { get; set; }
        public string Info { get; set; }
        public double Lat { get; set; }
        public double Lon { get; set; }
        public int narrY { get; set; }
        public int narrX { get; set; }
        public int hrDifference { get; set; }

        public int? JassLatLonGroupID { get; set; }
        public JassLatLonGroup JassLatLonGroup { get; set; }
    }
}