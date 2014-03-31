using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace JassWeather.Models
{
    public class JassLatLonGroup
    {
        public int JassLatLonGroupID { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public virtual List<JassLatLon> JassLatLons { get; set; }
    }
}