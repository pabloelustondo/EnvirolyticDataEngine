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
    public class Default1Controller : Controller
    {
        private JassWeatherContext db = new JassWeatherContext();

        //
        // GET: /Default1/

        public ActionResult Index()
        {
            var apirequests = db.APIRequests.Include(a => a.APIRequestSet);
            return View(apirequests.ToList());
        }

        //
        // GET: /Default1/Details/5

        public ActionResult Details(int id = 0)
        {
            APIRequest apirequest = db.APIRequests.Find(id);
            if (apirequest == null)
            {
                return HttpNotFound();
            }
            return View(apirequest);
        }

        //
        // GET: /Default1/Create

        public ActionResult Create()
        {
            ViewBag.APIRequestSetId = new SelectList(db.APIRequestSets, "Id", "name");
            return View();
        }

        //
        // POST: /Default1/Create

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
        // GET: /Default1/Edit/5

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
        // POST: /Default1/Edit/5

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
        // GET: /Default1/Delete/5

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
        // POST: /Default1/Delete/5

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