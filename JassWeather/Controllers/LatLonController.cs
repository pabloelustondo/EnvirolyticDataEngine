﻿using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using JassWeather.Models;

namespace JassWeather.Controllers
{
    public class LatLonController : JassController
    {
        private JassWeatherContext db = new JassWeatherContext();

        //
        // GET: /LatLon/

        public ActionResult Index()
        {
            try{
            int latLonGroupID = (int)Session["LatLonGroupID"];
            ViewBag.LatLonGroup = Session["LatLonGroupName"];
            JassLatLonGroup jasslatlongroup = db.JassLatLonGroups.Find(latLonGroupID);

            var jasslatlons = db.JassLatLons.Include(j => j.JassLatLonGroup).Where(j => j.JassLatLonGroupID == latLonGroupID);
            return View(jasslatlons.ToList());
            }
            catch(Exception e){               
                 List<JassLatLon> fakelist = new List<JassLatLon>();
                 ViewBag.JassMessage = e.Message;
                 return View();                
                }
        }

        //
        // GET: /LatLon/Details/5

        public ActionResult Details(int id = 0)
        {
            JassLatLon jasslatlon = db.JassLatLons.Find(id);
            if (jasslatlon == null)
            {
                return HttpNotFound();
            }
            return View(jasslatlon);
        }

        //
        // GET: /LatLon/Create

        public ActionResult Create()
        {
            ViewBag.JassLatLonGroupID = new SelectList(db.JassLatLonGroups, "JassLatLonGroupID", "Name", (int)Session["LatLonGroupID"]);
            return View();
        }

        //
        // POST: /LatLon/Create

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create(JassLatLon jasslatlon)
        {
            ViewBag.JassLatLonGroupID = new SelectList(db.JassLatLonGroups, "JassLatLonGroupID", "Name", jasslatlon.JassLatLonGroupID);

            try
            {
                if (ModelState.IsValid)
                {
                    jasslatlon = apiCaller.MapLatLonToNarr(jasslatlon);
                    db.JassLatLons.Add(jasslatlon);
                    db.SaveChanges();
                    return View("Edit",jasslatlon);
                }

            }
            catch (Exception e)
            {   
                apiCaller.createBuilderLog("EXCEPTION", "Location", e.Message, new TimeSpan(), false);
                ViewBag.JassMessage = "ERROR when creating";
            }
            
               return View(jasslatlon);
        }

        //
        // GET: /LatLon/Edit/5

        public ActionResult Edit(int id = 0)
        {
            JassLatLon jasslatlon = db.JassLatLons.Find(id);
            if (jasslatlon == null)
            {
                return HttpNotFound();
            }
            ViewBag.JassLatLonGroupID = new SelectList(db.JassLatLonGroups, "JassLatLonGroupID", "Name", jasslatlon.JassLatLonGroupID);

            jasslatlon = apiCaller.MapLatLonToNarr(jasslatlon);
            return View(jasslatlon);
        }

        //
        // POST: /LatLon/Edit/5

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit(JassLatLon jasslatlon)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    jasslatlon = apiCaller.MapLatLonToNarr(jasslatlon);
                    db.Entry(jasslatlon).State = EntityState.Modified;
                    db.SaveChanges();
                    return RedirectToAction("Index");
                }
                ViewBag.JassLatLonGroupID = new SelectList(db.JassLatLonGroups, "JassLatLonGroupID", "Name", jasslatlon.JassLatLonGroupID);

            }
              catch (Exception e)
            {   
                apiCaller.createBuilderLog("EXCEPTION", "Location", e.Message, new TimeSpan(), false);
                ViewBag.message = "ERROR when editing";
            }

               return View(jasslatlon);
        }

        //
        // GET: /LatLon/Delete/5

        public ActionResult Delete(int id = 0)
        {
            JassLatLon jasslatlon = db.JassLatLons.Find(id);
            if (jasslatlon == null)
            {
                return HttpNotFound();
            }
            return View(jasslatlon);
        }

        //
        // POST: /LatLon/Delete/5

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(int id)
        {
            JassLatLon jasslatlon = db.JassLatLons.Find(id);
            db.JassLatLons.Remove(jasslatlon);
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