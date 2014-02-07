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
    public class BuilderController : Controller
    {
        private JassWeatherContext db = new JassWeatherContext();

        //
        // GET: /Builder/

        public ActionResult Index()
        {
            var jassbuilders = db.JassBuilders.Include(j => j.JassVariable).Include(j => j.JassGrid);
            return View(jassbuilders.ToList());
        }

        //
        // GET: /Builder/Details/5

        public ActionResult Details(int id = 0)
        {
            JassBuilder jassbuilder = db.JassBuilders.Find(id);
            if (jassbuilder == null)
            {
                return HttpNotFound();
            }
            return View(jassbuilder);
        }

        //
        // GET: /Builder/Create

        public ActionResult Create()
        {
            ViewBag.JassVariableID = new SelectList(db.JassVariables, "JassVariableID", "Name");
            ViewBag.JassGridID = new SelectList(db.JassGrids, "JassGridID", "Name");
            return View();
        }

        //
        // POST: /Builder/Create

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create(JassBuilder jassbuilder)
        {
            if (ModelState.IsValid)
            {
                db.JassBuilders.Add(jassbuilder);
                db.SaveChanges();
                return RedirectToAction("Index");
            }

            ViewBag.JassVariableID = new SelectList(db.JassVariables, "JassVariableID", "Name", jassbuilder.JassVariableID);
            ViewBag.JassGridID = new SelectList(db.JassGrids, "JassGridID", "Name", jassbuilder.JassGridID);
            return View(jassbuilder);
        }

        //
        // GET: /Builder/Edit/5

        public ActionResult Edit(int id = 0)
        {
            JassBuilder jassbuilder = db.JassBuilders.Find(id);
            if (jassbuilder == null)
            {
                return HttpNotFound();
            }
            ViewBag.JassVariableID = new SelectList(db.JassVariables, "JassVariableID", "Name", jassbuilder.JassVariableID);
            ViewBag.JassGridID = new SelectList(db.JassGrids, "JassGridID", "Name", jassbuilder.JassGridID);
            return View(jassbuilder);
        }

        //
        // POST: /Builder/Edit/5

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit(JassBuilder jassbuilder)
        {
            if (ModelState.IsValid)
            {
                db.Entry(jassbuilder).State = EntityState.Modified;
                db.SaveChanges();
                return RedirectToAction("Index");
            }
            ViewBag.JassVariableID = new SelectList(db.JassVariables, "JassVariableID", "Name", jassbuilder.JassVariableID);
            ViewBag.JassGridID = new SelectList(db.JassGrids, "JassGridID", "Name", jassbuilder.JassGridID);
            return View(jassbuilder);
        }

        //
        // GET: /Builder/Delete/5

        public ActionResult Delete(int id = 0)
        {
            JassBuilder jassbuilder = db.JassBuilders.Find(id);
            if (jassbuilder == null)
            {
                return HttpNotFound();
            }
            return View(jassbuilder);
        }

        //
        // POST: /Builder/Delete/5

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(int id)
        {
            JassBuilder jassbuilder = db.JassBuilders.Find(id);
            db.JassBuilders.Remove(jassbuilder);
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