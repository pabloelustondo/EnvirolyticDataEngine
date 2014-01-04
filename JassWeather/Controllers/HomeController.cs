using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using JassWeatherAPI;
using Newtonsoft.Json;
using System.IO;
using JassWeather.Models;

namespace JassWeather.Controllers
{
    public class HomeController : Controller
    {
        public ActionResult Index()       
        {



            APICaller apiCaller = new APICaller();
            string request1 = "http://api.wunderground.com/api/501a82781dc79a42/geolookup/conditions/q/IA/Cedar_Rapids.json";
            string response1 = apiCaller.callAPI(request1);

            ViewBag.request1 = request1;
            ViewBag.response1 = response1;

            ViewBag.Message = "Modify this template to jump-start your ASP.NET MVC application.";

            return View();
        }

        public ActionResult About()
        {
            ViewBag.Message = "Your app description page.";

            return View();
        }

        public ActionResult Contact()
        {
            ViewBag.Message = "Your contact page.";

            return View();
        }
    }
}
