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
    public class LatLonGroupController : Controller
    {
        private JassWeatherContext db = new JassWeatherContext();

        //
        // GET: /LatLonGroup/

        public ActionResult Index()
        {
            return View(db.JassLatLonGroups.ToList());
        }

        //
        // GET: /LatLonGroup/Details/5

        public ActionResult Details(int id = 0)
        {
            JassLatLonGroup jasslatlongroup = db.JassLatLonGroups.Find(id);
            if (jasslatlongroup == null)
            {
                return HttpNotFound();
            }
            return View(jasslatlongroup);
        }

        //
        // GET: /LatLonGroup/Create

        public ActionResult Create()
        {
            return View();
        }

        //
        // POST: /LatLonGroup/Create

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create(JassLatLonGroup jasslatlongroup)
        {
            if (ModelState.IsValid)
            {
                db.JassLatLonGroups.Add(jasslatlongroup);
                db.SaveChanges();
                return RedirectToAction("Index");
            }

            return View(jasslatlongroup);
        }

        //
        // GET: /LatLonGroup/Edit/5

        public ActionResult Edit(int id = 0)
        {
            JassLatLonGroup jasslatlongroup = db.JassLatLonGroups.Find(id);
            if (jasslatlongroup == null)
            {
                return HttpNotFound();
            }
            return View(jasslatlongroup);
        }

        //
        // POST: /LatLonGroup/Edit/5

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit(JassLatLonGroup jasslatlongroup)
        {
            if (ModelState.IsValid)
            {
                db.Entry(jasslatlongroup).State = EntityState.Modified;
                db.SaveChanges();
                return RedirectToAction("Index");
            }
            return View(jasslatlongroup);
        }

        //
        // GET: /LatLonGroup/Delete/5

        public ActionResult Delete(int id = 0)
        {
            JassLatLonGroup jasslatlongroup = db.JassLatLonGroups.Find(id);
            if (jasslatlongroup == null)
            {
                return HttpNotFound();
            }
            return View(jasslatlongroup);
        }

        //
        // POST: /LatLonGroup/Delete/5

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(int id)
        {
            JassLatLonGroup jasslatlongroup = db.JassLatLonGroups.Find(id);
            db.JassLatLonGroups.Remove(jasslatlongroup);
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