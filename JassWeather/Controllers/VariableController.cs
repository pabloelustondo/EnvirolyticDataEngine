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
    public class VariableController : Controller
    {
        private JassWeatherContext db = new JassWeatherContext();

        //
        // GET: /Variable/

        public ActionResult Index()
        {
            return View(db.JassVariables.ToList());
        }

        //
        // GET: /Variable/Details/5

        public ActionResult Details(int id = 0)
        {
            JassVariable jassvariable = db.JassVariables.Find(id);
            if (jassvariable == null)
            {
                return HttpNotFound();
            }
            return View(jassvariable);
        }

        //
        // GET: /Variable/Create

        public ActionResult Create()
        {
            return View();
        }

        //
        // POST: /Variable/Create

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create(JassVariable jassvariable)
        {
            if (ModelState.IsValid)
            {
                db.JassVariables.Add(jassvariable);
                db.SaveChanges();
                return RedirectToAction("Index");
            }

            return View(jassvariable);
        }

        //
        // GET: /Variable/Edit/5

        public ActionResult Edit(int id = 0)
        {
            JassVariable jassvariable = db.JassVariables.Find(id);
            if (jassvariable == null)
            {
                return HttpNotFound();
            }
            return View(jassvariable);
        }

        //
        // POST: /Variable/Edit/5

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit(JassVariable jassvariable)
        {
            if (ModelState.IsValid)
            {
                db.Entry(jassvariable).State = EntityState.Modified;
                db.SaveChanges();
                return RedirectToAction("Index");
            }
            return View(jassvariable);
        }

        //
        // GET: /Variable/Delete/5

        public ActionResult Delete(int id = 0)
        {
            JassVariable jassvariable = db.JassVariables.Find(id);
            if (jassvariable == null)
            {
                return HttpNotFound();
            }
            return View(jassvariable);
        }

        //
        // POST: /Variable/Delete/5

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(int id)
        {
            JassVariable jassvariable = db.JassVariables.Find(id);
            db.JassVariables.Remove(jassvariable);
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