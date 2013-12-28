using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace JassWeather.Models
{
    public class APIRequestSet
    {
        public int Id { get; set; }
        public string name { get; set; }
        public string description { get; set; }
        public virtual List<APIRequest> APIRequests { get; set; }
    }
}