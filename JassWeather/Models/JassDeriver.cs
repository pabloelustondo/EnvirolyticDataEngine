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
/*
        public int X1JassVariableID { get; set; }
        public virtual JassVariable X1JassVariable { get; set; }

        public int X2JassVariableID { get; set; }
        public virtual JassVariable X2JassVariable { get; set; }
*/
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