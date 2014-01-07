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

        public ActionResult Details(int id = 0)
        {
            APICaller apiCaller = new APICaller();
            string request1 = "http://api.wunderground.com/api/501a82781dc79a42/geolookup/conditions/q/IA/Cedar_Rapids.json";
            string response1 = apiCaller.callAPI(request1);

            ViewBag.request1 = request1;
            ViewBag.response1 = response1;

            return View();
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