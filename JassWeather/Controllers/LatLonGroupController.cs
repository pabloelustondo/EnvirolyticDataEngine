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
    public class LatLonGroupController : JassController
    {
        private JassWeatherContext db = new JassWeatherContext();

        //
        // GET: /LatLonGroup/

        public ActionResult Index()
        {
            ViewBag.LatLonGroup = Session["LatLonGroupName"];
            return View(db.JassLatLonGroups.ToList());
        }

        public class ShowLocationBasedDashboardModel{
            public int year { get; set; }
            public int month { get; set; }
            public int day { get; set; }

            public JassLatLonGroup latlonGroup { get; set; }
            //we sould have a variable group as well

        }

        public ActionResult ShowLocationBasedDashboard()
        {
            ViewBag.LatLonGroupID = Session["LatLonGroupID"];
            ShowLocationBasedDashboardModel model = new ShowLocationBasedDashboardModel();
            model.latlonGroup = db.JassLatLonGroups.Find(ViewBag.LatLonGroupID);

            return View(model);
        }

        //
        // GET: /LatLonGroup/Details/5

        public ActionResult Set(int id = 0)
        {
            JassLatLonGroup jasslatlongroup = db.JassLatLonGroups.Find(id);
            Session["LatLonGroupName"] = jasslatlongroup.Name;
            Session["LatLonGroupID"] = id;
            
            if (jasslatlongroup == null)
            {
                return HttpNotFound();
            }
            return RedirectToAction("Index");
        }
        //
        // GET: /LatLonGroup/Create

        public ActionResult Create()
        {
            return View();
        }

        //
        // POST: /LatLonGroup/Create

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create(JassLatLonGroup jasslatlongroup)
        {
            if (ModelState.IsValid)
            {
                db.JassLatLonGroups.Add(jasslatlongroup);
                db.SaveChanges();
                return RedirectToAction("Index");
            }

            return View(jasslatlongroup);
        }

        //
        // GET: /LatLonGroup/Edit/5

        public ActionResult Edit(int id = 0)
        {
            JassLatLonGroup jasslatlongroup = db.JassLatLonGroups.Find(id);
            if (jasslatlongroup == null)
            {
                return HttpNotFound();
            }
            return View(jasslatlongroup);
        }

        //
        // POST: /LatLonGroup/Edit/5

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit(JassLatLonGroup jasslatlongroup)
        {
            if (ModelState.IsValid)
            {
                db.Entry(jasslatlongroup).State = EntityState.Modified;
                db.SaveChanges();
                return RedirectToAction("Index");
            }
            return View(jasslatlongroup);
        }

        //
        // GET: /LatLonGroup/Delete/5

        public ActionResult Delete(int id = 0)
        {
            JassLatLonGroup jasslatlongroup = db.JassLatLonGroups.Find(id);
            if (jasslatlongroup == null)
            {
                return HttpNotFound();
            }
            return View(jasslatlongroup);
        }

        //
        // POST: /LatLonGroup/Delete/5

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(int id)
        {
            JassLatLonGroup jasslatlongroup = db.JassLatLonGroups.Find(id);
            db.JassLatLonGroups.Remove(jasslatlongroup);
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