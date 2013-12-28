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

            var apiRequest = new APIRequest() { name = "Bob", APIRequestSetId = apiRequestSet.Id};

            context.APIRequests.Add(apiRequest);
            context.SaveChanges();

        }
    }
}
