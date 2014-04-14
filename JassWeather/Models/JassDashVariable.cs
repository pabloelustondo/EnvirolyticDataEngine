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
         public long SizeVariableLevel { get; set; }
         public List<int> StatusYearLevel { get; set; }
         public List<List<int>> StatusMonthLevel { get; set; }
         public List<List<List<int>>> StatusDayLevel { get; set; }

         public List<long> SizeYearLevel { get; set; }
         public List<List<long>> SizeMonthLevel { get; set; }
         public List<List<List<long>>> SizeDayLevel { get; set; }

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


             SizeYearLevel = new List<long>();
             SizeMonthLevel = new List<List<long>>();
             SizeDayLevel = new List<List<List<long>>>();

             for (int yearX = 0; yearX < EndYear - StartYear + 1; yearX++)
             {
                 SizeYearLevel.Add(0);    //SizeYearLevel[year]=0;
                 SizeMonthLevel.Add(new List<long>());   //SizeMonthLevel[year] = new new List<int>()
                 SizeDayLevel.Add(new List<List<long>>());   //SizeMonthLevel[year] = new new List<int>()

                 for (int monthX = 0; monthX < 12; monthX++)
                 {
                     SizeMonthLevel[yearX].Add(0); //SizeMonthLevel[year][month]=0
                     SizeDayLevel[yearX].Add(new List<long>());

                     for (int dayX = 0; dayX < DateTime.DaysInMonth(StartYear + yearX, monthX + 1); dayX++)
                     {
                         SizeDayLevel[yearX][monthX].Add(0); //SizeDayLevel[year][month][day]=0
                     }
                 }
             }
         }

         public void countBlob(JassFileNameComponents blobInfo, long size)
         {
             this.StatusDayLevel[blobInfo.year-StartYear][blobInfo.month-1][blobInfo.day-1] = 100;
             this.SizeDayLevel[blobInfo.year - StartYear][blobInfo.month - 1][blobInfo.day - 1] = size;
         }

         public void calcuateStatus()
         {
             SizeVariableLevel = 0;
             for (int yearX = 0; yearX < EndYear - StartYear + 1; yearX++)
             {

                 StatusYearLevel[yearX] = 0;
                 SizeYearLevel[yearX] = 0;
                 for (int monthX = 0; monthX < 12; monthX++)
                 {
                     StatusMonthLevel[yearX][monthX] = 0;
                     SizeMonthLevel[yearX][monthX] = 0;
                     for (int dayX = 0; dayX < DateTime.DaysInMonth(StartYear + yearX, monthX + 1); dayX++)
                     {
                         if (StatusDayLevel[yearX][monthX][dayX] > 0) StatusMonthLevel[yearX][monthX]++;
                         SizeMonthLevel[yearX][monthX] += SizeDayLevel[yearX][monthX][dayX];
                     }
                     StatusMonthLevel[yearX][monthX] = StatusMonthLevel[yearX][monthX] * 100 / DateTime.DaysInMonth(StartYear + yearX, monthX + 1);
                     if (StatusMonthLevel[yearX][monthX] > 0) StatusYearLevel[yearX]++;

                     SizeYearLevel[yearX] += SizeMonthLevel[yearX][monthX];
                 }
                 StatusYearLevel[yearX] = StatusYearLevel[yearX] * 100 / 12;
                 if (StatusYearLevel[yearX] > 0) StatusVariableLevel++;
                 SizeVariableLevel += SizeYearLevel[yearX];
             }
             StatusVariableLevel = StatusVariableLevel * 100 / (EndYear - StartYear);
         }
     }

}