using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace JassWeather.ViewModels
{
    public class JassErrorPageModel
    {
        public string message { get; set; }
        public string controller { get; set; }
        public string action { get; set; }
        public string type { get; set; }
    }
}