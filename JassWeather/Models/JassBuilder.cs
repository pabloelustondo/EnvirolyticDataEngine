using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace JassWeather.Models
{
    public class JassBuilder
    {
        public int JassBuilderID { get; set; }
        public string Name { get; set; }
        public int JassVariableID { get; set; }
        public virtual JassVariable JassVariable { get; set; }
        public int JassGridID { get; set; }
        public virtual JassGrid JassGrid { get; set; }
        public int? x { get; set; }
        public int? y { get; set; }
        public int? year { get; set; }
        public int? month { get; set; }
        public int? day { get; set; }
        public int? hour3 { get; set; }
        public int? level { get; set; }
    }
}