using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace JassWeather.Models
{
    public class JassDeriver
    {
        public int JassDeriverID { get; set; }
        public string Name { get; set; }

        public int JassVariableID { get; set; }
        public virtual JassVariable JassVariable { get; set; }

        public int JassGridID { get; set; }
        public virtual JassGrid JassGrid { get; set; }

        public string X1 { get; set; }
        public string X2 { get; set; }
        public string X3 { get; set; }
        public string X4 { get; set; }

        public int? X1Level { get; set; }
        public int? X2Level { get; set; }
        public int? X3Level { get; set; }
        public int? X4Level { get; set; }

        public int? X1HistoryLength { get; set; }
        public int? X2HistoryLength { get; set; }
        public int? X3HistoryLength { get; set; }
        public int? X4HistoryLength { get; set; }

        public int JassFormulaID { get; set; }
        public virtual JassFormula JassFormula { get; set; }

        public int YearStart { get; set; }
        public int YearEnd { get; set; }
      
        public int MonthStart { get; set; }
        public int MnnthEnd { get; set; }
        
        public int DayStart { get; set; }
        public int DayEnd { get; set; }
    }
}