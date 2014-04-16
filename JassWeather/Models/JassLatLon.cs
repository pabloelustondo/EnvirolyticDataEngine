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

        public int? narrY { get; set; }
        public int? narrX { get; set; }

        public double? narrLat { get; set; }
        public double? narrLon { get; set; }

        public int? maccY { get; set; }
        public int? maccX { get; set; }

        public double? maccLat { get; set; }
        public double? maccLon { get; set; }

        public int? cfsrY { get; set; }
        public int? cfsrX { get; set; }

        public double? cfsrLat { get; set; }
        public double? cfsrLon { get; set; }

        public int? sherY { get; set; }
        public int? sherX { get; set; }

        public double? sherLat { get; set; }
        public double? sherLon { get; set; }

        public int hrDifference { get; set; }

        public int? JassLatLonGroupID { get; set; }
        public JassLatLonGroup JassLatLonGroup { get; set; }
    }
}