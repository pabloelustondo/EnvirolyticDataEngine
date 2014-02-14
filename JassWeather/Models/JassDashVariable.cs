using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace JassWeather.Models
{

     public class JassVariableYearStatus
    {
        public int year{ get; set; }
        public int percentageFill { get; set; }     
    }

     public class JassVariableStatus
     {
         public JassVariable JassVariable { get; set; }
         public string VariableName { get; set; }
         public string ContainerName { get; set; }
         public int StartYear { get; set; }
         public int EndYear { get; set; }
         public int StatusVariableLevel { get; set; }
         public List<int> StatusYearLevel { get; set; }
         public List<List<int>> StatusMonthLevel { get; set; }
         public List<List<List<int>>> StatusDayLevel { get; set; }

         public JassVariableStatus(int StartYearIn, int EndYearIn)
         {
             this.StartYear = StartYearIn;
             this.EndYear = EndYearIn;

             StatusYearLevel = new List<int>();
             StatusMonthLevel = new List<List<int>>();
             StatusDayLevel = new List<List<List<int>>>();


             for (int yearX = 0; yearX < EndYear - StartYear + 1; yearX++)
             {
                 StatusYearLevel.Add(0);    //StatusYearLevel[year]=0;
                 StatusMonthLevel.Add(new List<int>());   //StatusMonthLevel[year] = new new List<int>()
                 StatusDayLevel.Add(new List<List<int>>());   //StatusMonthLevel[year] = new new List<int>()

                 for (int monthX = 0; monthX < 12; monthX++)
                 {
                     StatusMonthLevel[yearX].Add(0); //StatusMonthLevel[year][month]=0
                     StatusDayLevel[yearX].Add(new List<int>());

                     for (int dayX = 0; dayX < DateTime.DaysInMonth(StartYear + yearX, monthX + 1); dayX++)
                     {
                         StatusDayLevel[yearX][monthX].Add(0); //StatusDayLevel[year][month][day]=0
                     }
                 }
             }
         }

         public void countBlob(JassFileNameComponents blobInfo)
         {
             this.StatusDayLevel[blobInfo.year-StartYear][blobInfo.month-1][blobInfo.day-1] = 100;
         }

         public void calcuateStatus()
         {

             for (int yearX = 0; yearX < EndYear - StartYear + 1; yearX++)
             {
                 StatusYearLevel[yearX] = 0;
                 for (int monthX = 0; monthX < 12; monthX++)
                 {
                     StatusMonthLevel[yearX][monthX] = 0;
                     for (int dayX = 0; dayX < DateTime.DaysInMonth(StartYear + yearX, monthX + 1); dayX++)
                     {
                         if (StatusDayLevel[yearX][monthX][dayX] > 0) StatusMonthLevel[yearX][monthX]++;
                     }
                     StatusMonthLevel[yearX][monthX] = StatusMonthLevel[yearX][monthX] * 100 / DateTime.DaysInMonth(StartYear + yearX, monthX + 1);
                     if (StatusMonthLevel[yearX][monthX] > 0) StatusYearLevel[yearX]++;
                 }
                 StatusYearLevel[yearX] = StatusYearLevel[yearX] * 100 / 12;
                 if (StatusYearLevel[yearX] > 0) StatusVariableLevel++;
             }
             StatusVariableLevel = StatusVariableLevel * 100 / (EndYear - StartYear);
         }
     }

}