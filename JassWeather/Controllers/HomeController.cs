﻿using System;
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

namespace JassWeather.Controllers
{
    public class HomeController : Controller
    {

        public ActionResult RedirectToNarr()
        {
            //Response.Redirect("ControllerName/ActionName")
            //return RedirectToAction("Index", "Request");

            Session["CurrentRequestSetId"] = 2;
            Session["CurrentRequestSetName"]="NCEP-NARR";
            return RedirectToAction("Index", "Request");
        }

        public ActionResult Index()
        {
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
