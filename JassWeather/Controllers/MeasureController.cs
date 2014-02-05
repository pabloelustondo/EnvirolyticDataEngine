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
    public class MeasureController : Controller
    {
        private JassWeatherContext db = new JassWeatherContext();

        //
        // GET: /Measure/

        public ActionResult Index()
        {
            return View(db.JassMeasures.ToList());
        }

        //
        // GET: /Measure/Details/5

        public ActionResult Details(int id = 0)
        {
            JassMeasure jassmeasure = db.JassMeasures.Find(id);
            if (jassmeasure == null)
            {
                return HttpNotFound();
            }
            return View(jassmeasure);
        }

        //
        // GET: /Measure/Create

        public ActionResult Create()
        {
            return View();
        }

        //
        // POST: /Measure/Create

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create(JassMeasure jassmeasure)
        {
            if (ModelState.IsValid)
            {
                db.JassMeasures.Add(jassmeasure);
                db.SaveChanges();
                return RedirectToAction("Index");
            }

            return View(jassmeasure);
        }

        //
        // GET: /Measure/Edit/5

        public ActionResult Edit(int id = 0)
        {
            JassMeasure jassmeasure = db.JassMeasures.Find(id);
            if (jassmeasure == null)
            {
                return HttpNotFound();
            }
            return View(jassmeasure);
        }

        //
        // POST: /Measure/Edit/5

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit(JassMeasure jassmeasure)
        {
            if (ModelState.IsValid)
            {
                db.Entry(jassmeasure).State = EntityState.Modified;
                db.SaveChanges();
                return RedirectToAction("Index");
            }
            return View(jassmeasure);
        }

        //
        // GET: /Measure/Delete/5

        public ActionResult Delete(int id = 0)
        {
            JassMeasure jassmeasure = db.JassMeasures.Find(id);
            if (jassmeasure == null)
            {
                return HttpNotFound();
            }
            return View(jassmeasure);
        }

        //
        // POST: /Measure/Delete/5

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(int id)
        {
            JassMeasure jassmeasure = db.JassMeasures.Find(id);
            db.JassMeasures.Remove(jassmeasure);
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