using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Newtonsoft.Json;
using System.IO;
using JassWeather.Models;
using System.Diagnostics;
using System.Net;
using Microsoft.Research.Science.Data;
using Microsoft.Research.Science.Data.Imperative;
using Microsoft.Research.Science.Data.Azure;
using System.Reflection;
using JassWeather.ViewModels;

namespace JassWeather.Controllers
{
    [Authorize]
    public class HomeController : JassController
    {

        public ActionResult RedirectToNarr()
        {
            Session["CurrentRequestSetId"] = 2;
            Session["CurrentRequestSetName"] = "NCEP-NARR";

            //Response.Redirect("ControllerName/ActionName");
            return RedirectToAction("ShowDashBoard", "Blob");
        }

        

        public ActionResult ErrorPage(JassErrorPageModel model)
        {
            return View(model);
        }

        public ActionResult Index()
        {
            var versionNyumber = Assembly.GetAssembly(typeof(JassController)).GetName().Version;
            return View();
        }

        [AllowAnonymous]
        public ActionResult AboutUsers()
        {
            ViewBag.Message = "Your app description page.";

            return View();
        }

        public ActionResult Tools()
        {
            ViewBag.Message = "Your app description page.";

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
