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
    public class BuilderLogController : Controller
    {
        private JassWeatherContext db = new JassWeatherContext();

        //
        // GET: /BuilderLog/

        public ActionResult Index()
        {
            var jassbuilderlogs = db.JassBuilderLogs.Include(j => j.JassBuilder).OrderByDescending(log=>log.ParentJassBuilderLogID).OrderByDescending(log=>log.startTotalTime).Take(1000);
            return View(jassbuilderlogs.ToList());
        }

        //
        // GET: /BuilderLog/Details/5

        public ActionResult Details(int id = 0)
        {
            JassBuilderLog jassbuilderlog = db.JassBuilderLogs.Find(id);
            if (jassbuilderlog == null)
            {
                return HttpNotFound();
            }
            return View(jassbuilderlog);
        }

        //
        // GET: /BuilderLog/Create

        public ActionResult Create()
        {
            ViewBag.JassBuilderID = new SelectList(db.JassBuilders, "JassBuilderID", "Name");
            return View();
        }

        //
        // POST: /BuilderLog/Create

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create(JassBuilderLog jassbuilderlog)
        {
            if (ModelState.IsValid)
            {
                db.JassBuilderLogs.Add(jassbuilderlog);
                db.SaveChanges();
                return RedirectToAction("Index");
            }

            ViewBag.JassBuilderID = new SelectList(db.JassBuilders, "JassBuilderID", "Name", jassbuilderlog.JassBuilderID);
            return View(jassbuilderlog);
        }

        //
        // GET: /BuilderLog/Edit/5

        public ActionResult Edit(int id = 0)
        {
            JassBuilderLog jassbuilderlog = db.JassBuilderLogs.Find(id);
            if (jassbuilderlog == null)
            {
                return HttpNotFound();
            }
            ViewBag.JassBuilderID = new SelectList(db.JassBuilders, "JassBuilderID", "Name", jassbuilderlog.JassBuilderID);
            return View(jassbuilderlog);
        }

        //
        // POST: /BuilderLog/Edit/5

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit(JassBuilderLog jassbuilderlog)
        {
            if (ModelState.IsValid)
            {
                db.Entry(jassbuilderlog).State = EntityState.Modified;
                db.SaveChanges();
                return RedirectToAction("Index");
            }
            ViewBag.JassBuilderID = new SelectList(db.JassBuilders, "JassBuilderID", "Name", jassbuilderlog.JassBuilderID);
            return View(jassbuilderlog);
        }

        //
        // GET: /BuilderLog/Delete/5

        public ActionResult Delete(int id = 0)
        {
            JassBuilderLog jassbuilderlog = db.JassBuilderLogs.Find(id);
            if (jassbuilderlog == null)
            {
                return HttpNotFound();
            }
            return View(jassbuilderlog);
        }

        //
        // POST: /BuilderLog/Delete/5

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(int id)
        {
            JassBuilderLog jassbuilderlog = db.JassBuilderLogs.Find(id);
            db.JassBuilderLogs.Remove(jassbuilderlog);
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