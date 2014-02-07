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
    public class GridController : Controller
    {
        private JassWeatherContext db = new JassWeatherContext();

        //
        // GET: /Grid/

        public ActionResult Index()
        {
            var jassgrids = db.JassGrids.Include(j => j.JassPartition);
            return View(jassgrids.ToList());
        }

        //
        // GET: /Grid/Details/5

        public ActionResult Details(int id = 0)
        {
            JassGrid jassgrid = db.JassGrids.Find(id);
            if (jassgrid == null)
            {
                return HttpNotFound();
            }
            return View(jassgrid);
        }

        //
        // GET: /Grid/Create

        public ActionResult Create()
        {
            ViewBag.JassPartitionID = new SelectList(db.JassPartitions, "JassPartitionID", "Name");
            return View();
        }

        //
        // POST: /Grid/Create

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create(JassGrid jassgrid)
        {
            if (ModelState.IsValid)
            {
                db.JassGrids.Add(jassgrid);
                db.SaveChanges();
                return RedirectToAction("Index");
            }

            ViewBag.JassPartitionID = new SelectList(db.JassPartitions, "JassPartitionID", "Name", jassgrid.JassPartitionID);
            return View(jassgrid);
        }

        //
        // GET: /Grid/Edit/5

        public ActionResult Edit(int id = 0)
        {
            JassGrid jassgrid = db.JassGrids.Find(id);
            if (jassgrid == null)
            {
                return HttpNotFound();
            }
            ViewBag.JassPartitionID = new SelectList(db.JassPartitions, "JassPartitionID", "Name", jassgrid.JassPartitionID);
            return View(jassgrid);
        }

        //
        // POST: /Grid/Edit/5

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit(JassGrid jassgrid)
        {
            if (ModelState.IsValid)
            {
                db.Entry(jassgrid).State = EntityState.Modified;
                db.SaveChanges();
                return RedirectToAction("Index");
            }
            ViewBag.JassPartitionID = new SelectList(db.JassPartitions, "JassPartitionID", "Name", jassgrid.JassPartitionID);
            return View(jassgrid);
        }

        //
        // GET: /Grid/Delete/5

        public ActionResult Delete(int id = 0)
        {
            JassGrid jassgrid = db.JassGrids.Find(id);
            if (jassgrid == null)
            {
                return HttpNotFound();
            }
            return View(jassgrid);
        }

        //
        // POST: /Grid/Delete/5

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(int id)
        {
            JassGrid jassgrid = db.JassGrids.Find(id);
            db.JassGrids.Remove(jassgrid);
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