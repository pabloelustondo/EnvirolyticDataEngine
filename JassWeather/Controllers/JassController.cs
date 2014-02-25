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
            apiCaller = new JassWeatherAPI(HttpContext.Server.MapPath("~/App_Data"));

            base.OnActionExecuting(filterContext);
        }
    }
}
