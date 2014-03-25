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
    public class DeriverController : Controller
    {
        private JassWeatherContext db = new JassWeatherContext();

        //
        // GET: /Deriver/

        public ActionResult Index()
        {
            var jassderivers = db.JassDerivers.Include(j => j.JassVariable).Include(j => j.JassFormula);
            return View(jassderivers.ToList());
        }

        //
        // GET: /Deriver/Details/5

        public ActionResult Details(int id = 0)
        {
            JassDeriver jassderiver = db.JassDerivers.Find(id);
            if (jassderiver == null)
            {
                return HttpNotFound();
            }
            return View(jassderiver);
        }

        //
        // GET: /Deriver/Create

        public ActionResult Create()
        {
            ViewBag.JassVariableID = new SelectList(db.JassVariables, "JassVariableID", "Name");
            ViewBag.JassFormulaID = new SelectList(db.JassFormulas, "JassFormulaID", "Name");
            return View();
        }

        //
        // POST: /Deriver/Create

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create(JassDeriver jassderiver)
        {
            if (ModelState.IsValid)
            {
                db.JassDerivers.Add(jassderiver);
                db.SaveChanges();
                return RedirectToAction("Index");
            }

            ViewBag.JassVariableID = new SelectList(db.JassVariables, "JassVariableID", "Name", jassderiver.JassVariableID);
            ViewBag.JassFormulaID = new SelectList(db.JassFormulas, "JassFormulaID", "Name", jassderiver.JassFormulaID);
            return View(jassderiver);
        }

        //
        // GET: /Deriver/Edit/5

        public ActionResult Edit(int id = 0)
        {
            JassDeriver jassderiver = db.JassDerivers.Find(id);
            if (jassderiver == null)
            {
                return HttpNotFound();
            }
            ViewBag.JassVariableID = new SelectList(db.JassVariables, "JassVariableID", "Name", jassderiver.JassVariableID);
            ViewBag.JassFormulaID = new SelectList(db.JassFormulas, "JassFormulaID", "Name", jassderiver.JassFormulaID);
            return View(jassderiver);
        }

        //
        // POST: /Deriver/Edit/5

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit(JassDeriver jassderiver)
        {
            if (ModelState.IsValid)
            {
                db.Entry(jassderiver).State = EntityState.Modified;
                db.SaveChanges();
                return RedirectToAction("Index");
            }
            ViewBag.JassVariableID = new SelectList(db.JassVariables, "JassVariableID", "Name", jassderiver.JassVariableID);
            ViewBag.JassFormulaID = new SelectList(db.JassFormulas, "JassFormulaID", "Name", jassderiver.JassFormulaID);
            return View(jassderiver);
        }

        //
        // GET: /Deriver/Delete/5

        public ActionResult Delete(int id = 0)
        {
            JassDeriver jassderiver = db.JassDerivers.Find(id);
            if (jassderiver == null)
            {
                return HttpNotFound();
            }
            return View(jassderiver);
        }

        //
        // POST: /Deriver/Delete/5

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(int id)
        {
            JassDeriver jassderiver = db.JassDerivers.Find(id);
            db.JassDerivers.Remove(jassderiver);
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