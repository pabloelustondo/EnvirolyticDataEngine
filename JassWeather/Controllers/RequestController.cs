using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using JassWeather.Models;

namespace JassWeather.Controllers
{
    public class RequestController : Controller
    {
        private JassWeatherContext db = new JassWeatherContext();
        int? CurrentRequestSetID;
        string CurrentRequestSetName;

        protected override void OnActionExecuting(ActionExecutingContext filterContext)
        {
            CurrentRequestSetID = (int?)Session["CurrentRequestSetId"];
            CurrentRequestSetName = (string)Session["CurrentRequestSetName"];
        }

        //
        // GET: /Request/

        public ActionResult Index()
        {
            if (CurrentRequestSetID != null)
            {
                APIRequestSet apirequestset = db.APIRequestSets.Find(CurrentRequestSetID);
                return View(apirequestset);
            }
            else
            {
                return HttpNotFound();
            }

        }

        //
        // GET: /Request/Details/5

        public ActionResult Call(int id = 0)
        {
            APIRequest apiRequest = db.APIRequests.Find(id); 

            if (apiRequest.type == "json")
            {
                JassWeatherDataSourceAPI apiCaller = new JassWeatherDataSourceAPI();
                ViewBag.request = apiRequest.url;
                ViewBag.response = apiCaller.ping_Json_DataSource(apiRequest.url);
                return View("ShowJSON");
            }

            if (apiRequest.type == "netCDF")
            {
                string workingDirectorypath = HttpContext.Server.MapPath("~/App_Data");
                JassWeatherDataSourceAPI apiCaller = new JassWeatherDataSourceAPI();
                ViewBag.request = apiRequest.url;
                ViewBag.response = apiCaller.ping_small_NetCDF_file(apiRequest.url,workingDirectorypath);
                return View("ShowNetCDF");
            }

            if (apiRequest.type == "netCDFFtp")
            {
                string workingDirectorypath = HttpContext.Server.MapPath("~/App_Data");
                JassWeatherDataSourceAPI apiCaller = new JassWeatherDataSourceAPI();
                ViewBag.request = apiRequest.url;
                ViewBag.response = apiCaller.get_big_NetCDF_by_ftp(apiRequest.url, workingDirectorypath);
                return View("ShowNetCDF");
            }

            return View("ShowError");

        }

        //
        // GET: /Request/Create

        public ActionResult Create()
        {
            ViewBag.APIRequestSetId = new SelectList(db.APIRequestSets, "Id", "name", CurrentRequestSetID);
            return View();
        }

        //
        // POST: /Request/Create

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create(APIRequest apirequest)
        {
            if (ModelState.IsValid)
            {
                db.APIRequests.Add(apirequest);
                db.SaveChanges();
                return RedirectToAction("Index");
            }

            ViewBag.APIRequestSetId = new SelectList(db.APIRequestSets, "Id", "name", apirequest.APIRequestSetId);
            return View(apirequest);
        }

        //
        // GET: /Request/Edit/5

        public ActionResult Edit(int id = 0)
        {
            APIRequest apirequest = db.APIRequests.Find(id);
            if (apirequest == null)
            {
                return HttpNotFound();
            }
            ViewBag.APIRequestSetId = new SelectList(db.APIRequestSets, "Id", "name", apirequest.APIRequestSetId);
            return View(apirequest);
        }

        //
        // POST: /Request/Edit/5

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit(APIRequest apirequest)
        {
            if (ModelState.IsValid)
            {
                db.Entry(apirequest).State = EntityState.Modified;
                db.SaveChanges();
                return RedirectToAction("Index");
            }
            ViewBag.APIRequestSetId = new SelectList(db.APIRequestSets, "Id", "name", apirequest.APIRequestSetId);
            return View(apirequest);
        }

        //
        // GET: /Request/Delete/5

        public ActionResult Delete(int id = 0)
        {
            APIRequest apirequest = db.APIRequests.Find(id);
            if (apirequest == null)
            {
                return HttpNotFound();
            }
            return View(apirequest);
        }

        //
        // POST: /Request/Delete/5

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(int id)
        {
            APIRequest apirequest = db.APIRequests.Find(id);
            db.APIRequests.Remove(apirequest);
            db.SaveChanges();
            return RedirectToAction("Index");
        }

        protected override void Dispose(bool disposing)
        {
            db.Dispose();
            base.Dispose(disposing);
        }
    }
}