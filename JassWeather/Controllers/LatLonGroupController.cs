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
            public string Message { get; set; }
            public int year { get; set; }
            public int month { get; set; }
            public int day { get; set; }

            public JassLatLonGroup latlonGroup { get; set; }
            //we sould have a variable group as well
            public List<JassGridValues> gridValues { get; set; }
        }


        public ActionResult ShowLocationBasedDashboard()
        {
            ShowLocationBasedDashboardModel model = new ShowLocationBasedDashboardModel();
            ViewBag.LatLonGroupID = Session["LatLonGroupID"];
            model.latlonGroup = db.JassLatLonGroups.Find(ViewBag.LatLonGroupID);
            model.year = 2013;
            model.month = 1;
            model.day = 1;
            return View("ShowLocationBasedDashboardFirst", model);
        }
        [HttpPost]
        public ActionResult ShowLocationBasedDashboard(ShowLocationBasedDashboardModel model)
        {
            try
            {
                ViewBag.LatLonGroupID = Session["LatLonGroupID"];
                model.gridValues = new List<JassGridValues>();
                model.latlonGroup = db.JassLatLonGroups.Find(ViewBag.LatLonGroupID);
                //will hardcode a few variables.. and then generalize
                string dayString = apiCaller.fileNameBuilderByDay("Temperature2m", model.year, model.month, model.day) + ".nc";
                model.gridValues.Add(apiCaller.GetDayValues(dayString));
                dayString = apiCaller.fileNameBuilderByDay("WindUSpeed10m", model.year, model.month, model.day) + ".nc";
                model.gridValues.Add(apiCaller.GetDayValues(dayString));
                dayString = apiCaller.fileNameBuilderByDay("WindVSpeed10m", model.year, model.month, model.day) + ".nc";
                model.gridValues.Add(apiCaller.GetDayValues(dayString));
                dayString = apiCaller.fileNameBuilderByDay("WindChill", model.year, model.month, model.day) + ".nc";
                model.gridValues.Add(apiCaller.GetDayValues(dayString));
                return View(model);
            }
            catch (Exception e)
            {
                model.Message = "An error has occured, make sure that you ask for an available day" + e.Message;
                return View(model);
            }

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