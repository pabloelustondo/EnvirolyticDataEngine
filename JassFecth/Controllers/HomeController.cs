using Microsoft.Research.Science.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace JassFecth.Controllers
{
    public class HomeController : Controller
    {
        public ActionResult Index()
        {
            double Response1 = ClimateService.FetchClimate(ClimateParameter.FC_TEMPERATURE, 52.2, 52.2, 0.1, 0.1);



            ViewBag.Request1 = "FC_TEMPERATURE, 52.2, 52.2, 0.1, 0.1";
            ViewBag.Response1 = Response1;


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
