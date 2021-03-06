﻿using JassWeather.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;

namespace JassWeather.Controllers
{
    [Authorize]
    public class JassController : Controller
    {

        public JassWeatherAPI apiCaller;
        public string ServerName;
        public static Task task;

        public JassController()
        { 
        }

        protected override void OnActionExecuting(ActionExecutingContext filterContext)
        {

            string serverNameURL = Request.Url.ToString();
            int dotIndex = serverNameURL.IndexOf(".");
            this.ServerName  = (dotIndex > -1) ? serverNameURL.Substring(0, dotIndex) : "localhost";
            this.ServerName = this.ServerName.Replace("http://", "");
            ViewBag.ServerName = ServerName;
            
            string storageConnectionString;
            if (Session["StorageConnectionString"] == null)
            {
                    Session["StorageConnectionString"] = "StorageConnectionStringProd";
            }

            if (Session["LatLonGroupID"] == null)
            {
                Session["LatLonGroupName"] = "KeyCitiesQAPage";
                Session["LatLonGroupID"] = 1;
            }

            storageConnectionString = (string)Session["StorageConnectionString"];
            apiCaller = new JassWeatherAPI(this.ServerName, HttpContext.Server.MapPath("~/App_Data"), storageConnectionString);

            base.OnActionExecuting(filterContext);
        }
    }
}
