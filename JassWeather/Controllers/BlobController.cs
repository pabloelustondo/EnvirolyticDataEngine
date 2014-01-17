using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using JassWeather.Models;
using Microsoft.WindowsAzure.Storage.Blob;

namespace JassWeather.Controllers
{
    public class BlobController : Controller
    {
        //
        // GET: /Blob/

        public ActionResult Index()
        {

            JassWeatherDataSourceAPI apiCaller = new JassWeatherDataSourceAPI();

            List<CloudBlockBlob> blobs = apiCaller.listBlobs_in_envirolytics();

            return View(blobs);
        }

        public ActionResult Delete(string name)
        {

            JassWeatherDataSourceAPI apiCaller = new JassWeatherDataSourceAPI();
            string result = apiCaller.deleteBlob_in_envirolytics(name);
            ViewBag.Message = result;

            return View();
        }

    }
}
