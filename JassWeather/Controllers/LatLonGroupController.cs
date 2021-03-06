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

            public int yearEnd { get; set; }
            public int monthEnd { get; set; }
            public int dayEnd { get; set; }

            public Boolean generateFiles { get; set; }
            public Boolean generateFilesWithFixedColumns { get; set; }

            public Boolean[] variableChoices { get; set; }
            public string[] variableLevels { get; set; }
            public List<JassVariable> variables { get; set; }

            public JassLatLonGroup latlonGroup { get; set; }
            public List<JassLatLon> locations { get; set; } 

            public int? JassLatLonID { get; set; }
            public JassLatLon JassLatLon { get; set; }

            //we sould have a variable group as well
            public List<List<List<JassGridValues>>> gridValues { get; set; }
        }


        public ActionResult ShowLocationBasedDashboard()
        {
            ShowLocationBasedDashboardModel model = new ShowLocationBasedDashboardModel();
            model.variables = db.JassVariables.ToList();
            model.variableChoices = new Boolean[model.variables.Count];
            model.variableLevels = new string[model.variables.Count];
            model.generateFiles = true;

            ViewBag.LatLonGroupID = Session["LatLonGroupID"];
            int LatLonGroupID = (int)ViewBag.LatLonGroupID;

            var locations = db.JassLatLons.Where(l => l.JassLatLonGroupID == LatLonGroupID);
            model.latlonGroup = db.JassLatLonGroups.Find(ViewBag.LatLonGroupID);
            model.year = 2013;
            model.month = 1;
            model.day = 1;

            ViewBag.JassLatLonID = new SelectList(locations, "JassLatLonID", "Name",1358);

            return View("ShowLocationBasedDashboardFirst", model);
        }
        [HttpPost]
        public ActionResult ShowLocationBasedDashboard(ShowLocationBasedDashboardModel model)
        {
            ViewBag.LatLonGroupID = Session["LatLonGroupID"];
            int LatLonGroupID = (int)ViewBag.LatLonGroupID;
            model.latlonGroup = db.JassLatLonGroups.Find(ViewBag.LatLonGroupID);

            var locations = db.JassLatLons.Where(l=>l.JassLatLonGroupID==LatLonGroupID);
            ViewBag.JassLatLonID = new SelectList(locations, "JassLatLonID", "Name");
            try
            {
                if (model.JassLatLonID != null)
                {

                    model.locations = new List<JassLatLon>();
                    var latlon = db.JassLatLons.Find(model.JassLatLonID);
                    model.locations.Add(latlon);
                }
                else
                {
                    model.locations = model.latlonGroup.JassLatLons;
                }

                DateTime startDate = new DateTime(model.year, model.month, model.day);
                if (model.yearEnd == 0) model.yearEnd = model.year;
                if (model.monthEnd == 0) model.monthEnd = model.month;
                if (model.dayEnd == 0) model.dayEnd = model.day;

                DateTime endDate = new DateTime(model.yearEnd, model.monthEnd, model.dayEnd);
                if (endDate < startDate) { endDate = startDate; }
                int totalDays = (int)(endDate - startDate ).TotalDays + 1;

                ViewBag.LatLonGroupID = Session["LatLonGroupID"];
                model.variables = db.JassVariables.ToList();
                model.gridValues = new List<List<List<JassGridValues>>>();
                for (int d = 0; d < totalDays; d++) { 
                    model.gridValues.Add(new List<List<JassGridValues>>());
                    for (int l = 0; l < model.locations.Count; l++) { model.gridValues[d].Add(new List<JassGridValues>()); }          
                }
                

                model.latlonGroup = db.JassLatLonGroups.Find(ViewBag.LatLonGroupID);
                //will hardcode a few variables.. and then generalize

                    for (int v = 0; v < model.variables.Count; v++)
                    {
                        if (model.variableChoices[v])
                        {
                            string[] levels = new string[1];
                            levels[0] = "0";
                            Boolean hasLevels = false;

                            if (model.variableLevels[v] != null && model.variableLevels[v] != "")
                            {
                                levels = model.variableLevels[v].Split(',');
                                hasLevels = true;
                            }

                            for (int ll = 0; ll < levels.Length; ll++)
                            {
                                int d = 0;
                                try
                                {
                                    int level = Convert.ToInt16(levels[ll]);
                                    DateTime day = startDate;

                                    for (d = 0; d < totalDays; d++)
                                    {
                                        for (int l = 0; l < model.locations.Count; l++)
                                        {
                                            string dayString = apiCaller.fileNameBuilderByDay(model.variables[v].Name, day.Year, day.Month, day.Day) + ".nc";
                                            var gridResultValues = apiCaller.GetDayValues(dayString, model.locations[l], level);

                                            if (hasLevels)
                                            {
                                                gridResultValues.VariableName += "_" + level;
                                            }
                                            model.gridValues[d][l].Add(gridResultValues);

                                            day = day.AddDays(1);
                                        }
                                    }
                                }
                                catch (Exception e)
                                {
                                    apiCaller.createBuilderLog("EXCEPTION", "Error getting values v:" + v + " d: " + d, e.Message, new TimeSpan(), false);
                                    ViewBag.JassMessage = "ERROR when getting values";
                                }
                            }
                        }
                    }


                if (model.generateFiles)
                {

                   string resultFromFileGeneration = apiCaller.generateOutputTestFile(model.gridValues, model.locations, startDate, endDate);
                    model.Message += resultFromFileGeneration;
                }

                return View(model);
            }

             catch (Exception e)
            {   
                apiCaller.createBuilderLog("EXCEPTION", "LocationStatus", e.Message, new TimeSpan(), false);
                ViewBag.JassMessage = "ERROR when creating LocationStatus";
            }

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