using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace JassWeather.Models
{
    public class JassBuilderLog
    {
        public int JassBuilderLogID { get; set; }

        public int? JassBuilderID { get; set; }
        public virtual JassBuilder JassBuilder { get; set; }

        public int? ParentJassBuilderLogID { get; set; }
        public virtual JassBuilderLog ParentJassBuilderLog { get; set; }

        public string EventType { get; set; }  //StarBuilder RunFecth EndBuilder
        public string Label { get; set; }  //Test...etc somne identification of this run

        public string Message { get; set; }
        public bool Success { get; set; }

        public int? year { get; set; }
        public int? month { get; set; }
        public int? day { get; set; }


        public int? yearEnd { get; set; }
        public int? monthEnd { get; set; }


        public DateTime? startTotalTime { get; set; }
        public DateTime? endTotalTime { get; set; }
        public TimeSpan? spanTotalTime { get; set; }

    }
}