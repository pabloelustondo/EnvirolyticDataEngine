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
        public string status { get; set; }

        public DateTime? startGetTime { get; set; }
        public DateTime? endGetTime { get; set; }
        public TimeSpan? spanGetTime { get; set; }

        public DateTime? startLoadTime { get; set; }
        public DateTime? endLoadTime { get; set; }
        public TimeSpan? spanLoadTime { get; set; }

        public string onDisk { get; set; }
        public string onBlob { get; set; }
        public string onTable { get; set; }
        public string schema { get; set; }


        public int? fileSize { get; set; }   //in MB
        public string type { get; set; }
        public string schedule { get; set; }


        public string zenType { get; set; }
        public string variable { get; set; }
        public string variableConsolidated { get; set; }

        public string statistic { get; set; }
        public string level { get; set; }

        public int? impactHealth { get; set; }
        public int? impactBusiness { get; set; }
        public int? impactConsumer { get; set; }
        public int? impactAgriculture { get; set; }

        [DataType(DataType.MultilineText)]
        public string description { get; set; }
        public string dataSource { get; set; }

        public bool isHistorical { get; set; }
        public bool isCurrent { get; set; }
        public bool isForecast { get; set; }

        public string geographicResolution { get; set; }
        public string frecuency { get; set; }
        public string typeOfMeasure { get; set; }
        public string cost { get; set; }
        public string fileFormat { get; set; }

    }
}