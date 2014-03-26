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
    public class LatLonController : JassController
    {
        private JassWeatherContext db = new JassWeatherContext();

        //
        // GET: /LatLon/

        public ActionResult Index()
        {
            string sherindanStationsFilePath = apiCaller.AppFilesFolder + "/sheridan-stations.csv";
            string[] lines = System.IO.File.ReadAllLines(sherindanStationsFilePath);

            var list = db.JassLatLons.Where(l => l.StationCode.Length == 3).ToList();
            ViewBag.Count = list.Count();
            ViewBag.TotalNumberOfStations = lines.Length;
            return View(list);
        }

        //
        // GET: /LatLon/Details/5

        public ActionResult Details(int id = 0)
        {
            JassLatLon jasslatlon = db.JassLatLons.Find(id);
            if (jasslatlon == null)
            {
                return HttpNotFound();
            }
            return View(jasslatlon);
        }

        //
        // GET: /LatLon/Create

        public ActionResult Create()
        {
            return View();
        }

        //
        // POST: /LatLon/Create

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create(JassLatLon jasslatlon)
        {
            if (ModelState.IsValid)
            {
                db.JassLatLons.Add(jasslatlon);
                db.SaveChanges();
                return RedirectToAction("Index");
            }

            return View(jasslatlon);
        }

        //
        // GET: /LatLon/Edit/5

        public ActionResult Edit(int id = 0)
        {
            JassLatLon jasslatlon = db.JassLatLons.Find(id);
            if (jasslatlon == null)
            {
                return HttpNotFound();
            }
            return View(jasslatlon);
        }

        //
        // POST: /LatLon/Edit/5

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit(JassLatLon jasslatlon)
        {
            if (ModelState.IsValid)
            {
                db.Entry(jasslatlon).State = EntityState.Modified;
                db.SaveChanges();
                return RedirectToAction("Index");
            }
            return View(jasslatlon);
        }

        //
        // GET: /LatLon/Delete/5

        public ActionResult Delete(int id = 0)
        {
            JassLatLon jasslatlon = db.JassLatLons.Find(id);
            if (jasslatlon == null)
            {
                return HttpNotFound();
            }
            return View(jasslatlon);
        }

        //
        // POST: /LatLon/Delete/5

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(int id)
        {
            JassLatLon jasslatlon = db.JassLatLons.Find(id);
            db.JassLatLons.Remove(jasslatlon);
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