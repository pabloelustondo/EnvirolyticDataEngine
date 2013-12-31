using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace JassWeather.Models
{
    public class APIRequest
    {
        public int Id { get; set; }
        public int APIRequestSetId { get; set; }
        public virtual APIRequestSet APIRequestSet { get; set; }

        [DataType(DataType.MultilineText)]
        public string url { get; set; }
        public string name { get; set; }

        [DataType(DataType.MultilineText)]
        public string description { get; set; }
    }
}