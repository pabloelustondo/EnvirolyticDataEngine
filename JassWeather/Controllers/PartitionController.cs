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
    [Authorize]
    public class PartitionController : Controller
    {
        private JassWeatherContext db = new JassWeatherContext();

        //
        // GET: /Partition/

        public ActionResult Index()
        {
            return View(db.JassPartitions.ToList());
        }

        //
        // GET: /Partition/Details/5

        public ActionResult Details(int id = 0)
        {
            JassPartition jasspartition = db.JassPartitions.Find(id);
            if (jasspartition == null)
            {
                return HttpNotFound();
            }
            return View(jasspartition);
        }

        //
        // GET: /Partition/Create

        public ActionResult Create()
        {
            return View();
        }

        //
        // POST: /Partition/Create

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create(JassPartition jasspartition)
        {
            if (ModelState.IsValid)
            {
                db.JassPartitions.Add(jasspartition);
                db.SaveChanges();
                return RedirectToAction("Index");
            }

            return View(jasspartition);
        }

        //
        // GET: /Partition/Edit/5

        public ActionResult Edit(int id = 0)
        {
            JassPartition jasspartition = db.JassPartitions.Find(id);
            if (jasspartition == null)
            {
                return HttpNotFound();
            }
            return View(jasspartition);
        }

        //
        // POST: /Partition/Edit/5

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit(JassPartition jasspartition)
        {
            if (ModelState.IsValid)
            {
                db.Entry(jasspartition).State = EntityState.Modified;
                db.SaveChanges();
                return RedirectToAction("Index");
            }
            return View(jasspartition);
        }

        //
        // GET: /Partition/Delete/5

        public ActionResult Delete(int id = 0)
        {
            JassPartition jasspartition = db.JassPartitions.Find(id);
            if (jasspartition == null)
            {
                return HttpNotFound();
            }
            return View(jasspartition);
        }

        //
        // POST: /Partition/Delete/5

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(int id)
        {
            JassPartition jasspartition = db.JassPartitions.Find(id);
            db.JassPartitions.Remove(jasspartition);
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