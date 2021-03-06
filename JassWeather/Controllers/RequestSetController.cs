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
    [Authorize]
    public class RequestSetController : JassController
    {
        private JassWeatherContext db = new JassWeatherContext();

        //
        // GET: /RequestSet/

        public ActionResult Index()
        {
            return View(db.APIRequestSets.ToList());
        }

        //
        // GET: /RequestSet/Details/5

        public ActionResult EditCalls(int id = 0)
        {
            APIRequestSet apirequestset = db.APIRequestSets.Find(id);
            if (apirequestset == null)
            {
                return HttpNotFound();
            }
            Session["CurrentRequestSetId"] = apirequestset.Id;
            Session["CurrentRequestSetName"] = apirequestset.name;
            return RedirectToAction("Index", "Request");
        }

        //
        // GET: /RequestSet/Create

        public ActionResult Create()
        {
            return View();
        }

        //
        // POST: /RequestSet/Create

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create(APIRequestSet apirequestset)
        {
            if (ModelState.IsValid)
            {
                db.APIRequestSets.Add(apirequestset);
                db.SaveChanges();
                return RedirectToAction("Index");
            }
            Session["CurrentRequestSetId"] = apirequestset.Id;
            Session["CurrentRequestSetName"] = apirequestset.name;
            return View(apirequestset);
        }

        //
        // GET: /RequestSet/Edit/5

        public ActionResult Edit(int id = 0)
        {
            APIRequestSet apirequestset = db.APIRequestSets.Find(id);
            if (apirequestset == null)
            {
                return HttpNotFound();
            }
            Session["CurrentRequestSetId"] = apirequestset.Id;
            Session["CurrentRequestSetName"] = apirequestset.name;
            return View(apirequestset);
        }

        //
        // POST: /RequestSet/Edit/5

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit(APIRequestSet apirequestset)
        {
            if (ModelState.IsValid)
            {
                db.Entry(apirequestset).State = EntityState.Modified;
                db.SaveChanges();
                return RedirectToAction("Index");
            }
            Session["CurrentRequestSetId"] = apirequestset.Id;
            Session["CurrentRequestSetName"] = apirequestset.name;
            return View(apirequestset);
        }

        //
        // GET: /RequestSet/Delete/5

        public ActionResult Delete(int id = 0)
        {
            APIRequestSet apirequestset = db.APIRequestSets.Find(id);
            if (apirequestset == null)
            {
                return HttpNotFound();
            }
            return View(apirequestset);
        }

        //
        // POST: /RequestSet/Delete/5

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(int id)
        {
            APIRequestSet apirequestset = db.APIRequestSets.Find(id);
            Session["CurrentRequestSetId"] = null;
            Session["CurrentRequestSetName"] = null;
            db.APIRequestSets.Remove(apirequestset);
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