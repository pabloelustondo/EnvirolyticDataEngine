using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace JassWeather.Models
{
    public class JassGrid
    {
        public int JassGridID { get; set; }
        public string Name { get; set; }
        public string Type { get; set; }

        public int Xsize { get; set; }
        public string Xname { get; set; }

        public int Ysize { get; set; }
        public string Yname { get; set; }

        public int Levelsize { get; set; }
        public string Levelname { get; set; }

        public int Timesize { get; set; }
        public string Timename { get; set; }

        public int StepsInDay { get; set; }

        public int JassPartitionID { get; set; }
        public virtual JassPartition JassPartition { get; set; }
    }
}