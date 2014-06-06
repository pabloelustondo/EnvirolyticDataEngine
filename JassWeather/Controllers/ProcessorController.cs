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
    public class ProcessorController : Controller
    {
        private JassWeatherContext db = new JassWeatherContext();

        //
        // GET: /Processor/

        public ActionResult Index()
        {
            return View(db.JassProcessors.ToList());
        }

        //
        // GET: /Processor/Details/5

        public ActionResult Details(int id = 0)
        {
            JassProcessor jassprocessor = db.JassProcessors.Find(id);
            if (jassprocessor == null)
            {
                return HttpNotFound();
            }
            return View(jassprocessor);
        }

        //
        // GET: /Processor/Create

        public ActionResult Create()
        {
            return View();
        }

        //
        // POST: /Processor/Create

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create(JassProcessor jassprocessor)
        {
            if (ModelState.IsValid)
            {
                db.JassProcessors.Add(jassprocessor);
                db.SaveChanges();
                return RedirectToAction("Index");
            }

            return View(jassprocessor);
        }

        //
        // GET: /Processor/Edit/5

        public ActionResult Edit(int id = 0)
        {
            JassProcessor jassprocessor = db.JassProcessors.Find(id);
            if (jassprocessor == null)
            {
                return HttpNotFound();
            }
            return View(jassprocessor);
        }

        //
        // POST: /Processor/Edit/5

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit(JassProcessor jassprocessor)
        {
            if (ModelState.IsValid)
            {
                db.Entry(jassprocessor).State = EntityState.Modified;
                db.SaveChanges();
                return RedirectToAction("Index");
            }
            return View(jassprocessor);
        }

        //
        // GET: /Processor/Delete/5

        public ActionResult Delete(int id = 0)
        {
            JassProcessor jassprocessor = db.JassProcessors.Find(id);
            if (jassprocessor == null)
            {
                return HttpNotFound();
            }
            return View(jassprocessor);
        }

        //
        // POST: /Processor/Delete/5

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(int id)
        {
            JassProcessor jassprocessor = db.JassProcessors.Find(id);
            db.JassProcessors.Remove(jassprocessor);
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