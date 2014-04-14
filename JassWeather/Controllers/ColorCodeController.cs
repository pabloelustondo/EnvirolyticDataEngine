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
    public class ColorCodeController : Controller
    {
        private JassWeatherContext db = new JassWeatherContext();

        //
        // GET: /ColorCode/

        public ActionResult Index()
        {
            return View(db.JassColorCodes.ToList());
        }

        //
        // GET: /ColorCode/Details/5

        public ActionResult Details(int id = 0)
        {
            JassColorCode jasscolorcode = db.JassColorCodes.Find(id);
            if (jasscolorcode == null)
            {
                return HttpNotFound();
            }
            return View(jasscolorcode);
        }

        //
        // GET: /ColorCode/Create

        public ActionResult Create()
        {
            return View();
        }

        //
        // POST: /ColorCode/Create

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create(JassColorCode jasscolorcode)
        {
            if (ModelState.IsValid)
            {
                db.JassColorCodes.Add(jasscolorcode);
                db.SaveChanges();
                return RedirectToAction("Index");
            }

            return View(jasscolorcode);
        }

        //
        // GET: /ColorCode/Edit/5

        public ActionResult Edit(int id = 0)
        {
            JassColorCode jasscolorcode = db.JassColorCodes.Find(id);
            if (jasscolorcode == null)
            {
                return HttpNotFound();
            }
            return View(jasscolorcode);
        }

        //
        // POST: /ColorCode/Edit/5

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit(JassColorCode jasscolorcode)
        {
            if (ModelState.IsValid)
            {
                db.Entry(jasscolorcode).State = EntityState.Modified;
                db.SaveChanges();
                return RedirectToAction("Index");
            }
            return View(jasscolorcode);
        }

        //
        // GET: /ColorCode/Delete/5

        public ActionResult Delete(int id = 0)
        {
            JassColorCode jasscolorcode = db.JassColorCodes.Find(id);
            if (jasscolorcode == null)
            {
                return HttpNotFound();
            }
            return View(jasscolorcode);
        }

        //
        // POST: /ColorCode/Delete/5

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(int id)
        {
            JassColorCode jasscolorcode = db.JassColorCodes.Find(id);
            db.JassColorCodes.Remove(jasscolorcode);
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