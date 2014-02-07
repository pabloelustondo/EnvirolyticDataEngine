using Microsoft.Research.Science.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JassFetchClimate
{
    class Program
    {
        static void Main(string[] args)
        {
           
     
          //  double fetch_climate(int p, double latmin, double latmax, double lonmin, double lonmax, 
          //  int hourmin, int hourmax, int daymin, int daymax, int yearmin, int yearmax); 

            DateTime Start = DateTime.Now;
      
            double t = ClimateService.FetchClimate(
                ClimateParameter.FC_TEMPERATURE,
                43.7, 43.7, 79.4, 79.4,0,24,30,30,2014,2014);

            TimeSpan FetchingOneValue = DateTime.Now - Start;
            DateTime StartGrid = DateTime.Now;
            var grid = ClimateService.FetchClimateGrid(ClimateParameter.FC_LAND_AIR_TEMPERATURE, 42.7, 43.7, 78.4, 79.4, 0.01, 0.01);

            TimeSpan Fetching100Grid = DateTime.Now - StartGrid;

            DateTime StartGrid10000 = DateTime.Now;
            var grid10000 = ClimateService.FetchClimateGrid(ClimateParameter.FC_LAND_AIR_TEMPERATURE, 42.7, 43.7, 78.4, 79.4, 0.001, 0.001);

            TimeSpan Fetching10000Grid = DateTime.Now - StartGrid10000;

            Console.WriteLine(t);

            Console.ReadLine();


        }
    }
}
