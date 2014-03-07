using JassWeather.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace JassWeather.Controllers
{
    [Authorize]
    public class JassController : Controller
    {

        public JassWeatherAPI apiCaller;

        public JassController()
        { 
        }

        protected override void OnActionExecuting(ActionExecutingContext filterContext)
        {
            string storageConnectionString;
            if (Session["StorageConnectionString"] == null)
            {
                if (User.IsInRole("Admin"))
                {
                    Session["StorageConnectionString"] = "StorageConnectionStringDev";
                }
                else
                {
                    Session["StorageConnectionString"] = "StorageConnectionStringProd";
                }

            }

            storageConnectionString = (string)Session["StorageConnectionString"];
            apiCaller = new JassWeatherAPI(HttpContext.Server.MapPath("~/App_Data"),storageConnectionString);

            base.OnActionExecuting(filterContext);
        }
    }
}
