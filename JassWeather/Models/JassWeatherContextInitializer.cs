using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Data.Entity;

namespace JassWeather.Models
{
    public class JassWeatherContextInitializer: DropCreateDatabaseIfModelChanges<JassWeatherContext>
    {
        protected override void Seed(JassWeatherContext context)
        {

            APIRequestSet apiRequestSet = new APIRequestSet() { name = "Weather Underground"};
            context.APIRequestSets.Add(apiRequestSet);
            context.SaveChanges();

            context.APIRequests.Add(new APIRequest() { 
                name = "Bob", 
                url = "http://api.wunderground.com/api/501a82781dc79a42/geolookup/conditions/q/IA/Cedar_Rapids.json",
                type = "json",
                APIRequestSetId = apiRequestSet.Id
            });


            context.SaveChanges();

        }
    }
}
