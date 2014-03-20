using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace JassWeather.Models
{

    public enum JassBuilderStatus : int
    {
        New = 1,
        Processing = 2,
        Success = 3,
        Failure = 4
    }

    public class JassBuilder
    {
        public int JassBuilderID { get; set; }
        public string Name { get; set; }
        public int JassVariableID { get; set; }
        public virtual JassVariable JassVariable { get; set; }
        public bool unpack { get; set; }
        public int JassGridID { get; set; }
        public virtual JassGrid JassGrid { get; set; }
        public int APIRequestId { get; set; }
        public virtual APIRequest APIRequest { get; set; }
        public string Source1VariableName { get; set; }
        public string ServerName { get; set; }
        public int? x { get; set; }
        public int? y { get; set; }
        public int? year { get; set; }
        public int? month { get; set; }
        public int? weeky { get; set; }
        public int? yearEnd { get; set; }
        public int? monthEnd { get; set; }
        public int? weekyEnd { get; set; }
        public int? day { get; set; }
        public int? hour3 { get; set; }
        public int? level { get; set; }

        public DateTime? startTotalTime { get; set; }
        public DateTime? endTotalTime { get; set; }
        public TimeSpan? spanTotalTime { get; set; }

        public Boolean OnDisk { get; set; }
        public JassBuilderStatus Status { get; set; }
        public int setTotalSize { get; set; }
        public int setCurrentSize  { get; set; }
        public string Message { get; set; }
    }
}