using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace JassWeather.Models
{
    public class JassProcessor
    {
        public static string statusNew = "new";
        public static string statusRunning = "running";
        public static string statusIdleOk = "idle_Ok";
        public static string statusIdleError = "idle_Error";

        public int JassProcessorID { get; set; }
        public string name { get; set; }
        public string status { get; set; }
        public string info { get; set; }
        public string update { get; set; }
        public DateTime? startTime { get; set; }
        public DateTime? endTime { get; set; }
        public TimeSpan? spanTime { get; set; }
        public DateTime lastUpdate { get; set; }
    }
}