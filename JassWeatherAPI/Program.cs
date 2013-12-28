using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace JassWeatherAPI
{
    class Program
    {
        static void Main(string[] args)
        {
            //Get method

            APICaller apiCaller = new APICaller();
            string request = "http://api.wunderground.com/api/501a82781dc79a42/geolookup/conditions/q/IA/Cedar_Rapids.json";
            string response = apiCaller.callAPI(request);
            Console.WriteLine(response);
            Console.Read();


            request = "http://api.wunderground.com/api/501a82781dc79a42/history_20060405/q/CA/San_Francisco.json";
            response = apiCaller.callAPI(request);
            Console.WriteLine(response);
            Console.Read();


        }
    }
}
