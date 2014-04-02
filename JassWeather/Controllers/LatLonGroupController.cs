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

            public int yearEnd { get; set; }
            public int monthEnd { get; set; }
            public int dayEnd { get; set; }

            public Boolean generateFiles { get; set; }
            public Boolean generateFilesWithFixedColumns { get; set; }

            public Boolean[] variableChoices { get; set; }
            public List<JassVariable> variables { get; set; }

            public JassLatLonGroup latlonGroup { get; set; }
            //we sould have a variable group as well
            public List<JassGridValues> gridValues { get; set; }
        }


        public ActionResult ShowLocationBasedDashboard()
        {
            ShowLocationBasedDashboardModel model = new ShowLocationBasedDashboardModel();
            model.variables = db.JassVariables.ToList();
            model.variableChoices = new Boolean[model.variables.Count];
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
                model.variables = db.JassVariables.ToList();
                model.gridValues = new List<JassGridValues>();
                model.latlonGroup = db.JassLatLonGroups.Find(ViewBag.LatLonGroupID);
                //will hardcode a few variables.. and then generalize

                if (model.generateFilesWithFixedColumns) {

                    string[] variables = new string[5];
                    variables[0] = "Temperature2m";
                    variables[1] = "DewPointTemperature";
                    variables[2] = "HumidityRelative";
                    variables[3] = "WindUSpeed10m";
                    variables[4] = "WindVSpeed10m";

                      




                    for (int v = 0; v < variables.Length; v++)
                    {
                        try
                        {
                            string dayString = apiCaller.fileNameBuilderByDay(variables[v], model.year, model.month, model.day) + ".nc";
                            model.gridValues.Add(apiCaller.GetDayValues(dayString));
                        }
                        catch (Exception e)
                        {
                            model.Message += variables[v] + " could not be retrieved";
                        }
                    }
                
                
                }
                else
                {
                    for (int v = 0; v < model.variables.Count; v++)
                    {
                        if (model.variableChoices[v])
                        {
                            try
                            {
                                string dayString = apiCaller.fileNameBuilderByDay(model.variables[v].Name, model.year, model.month, model.day) + ".nc";
                                model.gridValues.Add(apiCaller.GetDayValues(dayString));
                            }                        catch (Exception e)
                        {
                            model.Message += model.variables[v].Name + " could not be retrieved";
                        
                        }
                    }
                }

                DateTime startDate = new DateTime(model.year,model.month,model.day);
                if (model.yearEnd == 0) model.yearEnd = model.year;
                if (model.monthEnd == 0) model.monthEnd = model.month;
                if (model.dayEnd == 0) model.dayEnd = model.day;

                DateTime endDate = new DateTime(model.yearEnd,model.monthEnd,model.dayEnd);
                if (endDate < startDate) { endDate = startDate; }

                if (model.generateFiles)
                {
                    string resultFromFileGeneration = apiCaller.generateOutputTestFile(model.gridValues, model.latlonGroup.JassLatLons, startDate, endDate);
                    model.Message += resultFromFileGeneration;
                }

                return View(model);
            }
            catch (Exception e)
            {
                model.Message += "An error has occured, make sure that you ask for an available day" + e.Message;
                return View(model);
            }
            finally
            {
               apiCaller.cleanAppData();
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