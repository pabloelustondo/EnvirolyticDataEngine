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
    public class FormulaController : Controller
    {
        private JassWeatherContext db = new JassWeatherContext();

        //
        // GET: /Formula/

        public ActionResult Index()
        {
            return View(db.JassFormulas.ToList());
        }

        //
        // GET: /Formula/Details/5

        public ActionResult Details(int id = 0)
        {
            JassFormula jassformula = db.JassFormulas.Find(id);
            if (jassformula == null)
            {
                return HttpNotFound();
            }
            return View(jassformula);
        }

        //
        // GET: /Formula/Create

        public ActionResult Create()
        {
            return View();
        }

        //
        // POST: /Formula/Create

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create(JassFormula jassformula)
        {
            if (ModelState.IsValid)
            {
                db.JassFormulas.Add(jassformula);
                db.SaveChanges();
                return RedirectToAction("Index");
            }

            return View(jassformula);
        }

        //
        // GET: /Formula/Edit/5

        public ActionResult Edit(int id = 0)
        {
            JassFormula jassformula = db.JassFormulas.Find(id);
            if (jassformula == null)
            {
                return HttpNotFound();
            }
            return View(jassformula);
        }

        //
        // POST: /Formula/Edit/5

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit(JassFormula jassformula)
        {
            if (ModelState.IsValid)
            {
                db.Entry(jassformula).State = EntityState.Modified;
                db.SaveChanges();
                return RedirectToAction("Index");
            }
            return View(jassformula);
        }

        //
        // GET: /Formula/Delete/5

        public ActionResult Delete(int id = 0)
        {
            JassFormula jassformula = db.JassFormulas.Find(id);
            if (jassformula == null)
            {
                return HttpNotFound();
            }
            return View(jassformula);
        }

        //
        // POST: /Formula/Delete/5

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(int id)
        {
            JassFormula jassformula = db.JassFormulas.Find(id);
            db.JassFormulas.Remove(jassformula);
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