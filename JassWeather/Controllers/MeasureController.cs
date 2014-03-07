using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using JassWeather.Models;
using Microsoft.Research.Science.Data;
using Microsoft.Research.Science.Data.Imperative;

namespace JassWeather.Controllers
{
    [Authorize]
    public class MeasureController : JassController
    {
        private JassWeatherContext db = new JassWeatherContext();

        //
        // GET: /Measure/

        public ActionResult Index()
        {
            var jassmeasures = db.JassMeasures.Include(j => j.JassVariable);
            return View(jassmeasures.ToList());
        }

        //
        // GET: /Measure/Details/5

        public ActionResult Details(int id = 0)
        {
            JassMeasure jassmeasure = db.JassMeasures.Find(id);
            if (jassmeasure == null)
            {
                return HttpNotFound();
            }
            return View(jassmeasure);
        }

        //
        // GET: /Measure/Create

        public ActionResult Create()
        {
            ViewBag.JassVariableID = new SelectList(db.JassVariables, "JassVariableID", "Name");
            return View();
        }

        //
        // POST: /Measure/Create

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create(JassMeasure jassmeasure)
        {
            if (ModelState.IsValid)
            {
                db.JassMeasures.Add(jassmeasure);
                db.SaveChanges();
                return RedirectToAction("Index");
            }

            ViewBag.JassVariableID = new SelectList(db.JassVariables, "JassVariableID", "Name", jassmeasure.JassVariableID);
            return View(jassmeasure);
        }

        //
        // GET: /Measure/Edit/5

        public ActionResult Edit(int id = 0)
        {
            JassMeasure jassmeasure = db.JassMeasures.Find(id);
            if (jassmeasure == null)
            {
                return HttpNotFound();
            }
            ViewBag.JassVariableID = new SelectList(db.JassVariables, "JassVariableID", "Name", jassmeasure.JassVariableID);
            return View(jassmeasure);
        }

        //
        // POST: /Measure/Edit/5

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit(JassMeasure jassmeasure)
        {
            if (ModelState.IsValid)
            {
                db.Entry(jassmeasure).State = EntityState.Modified;
                db.SaveChanges();
                return RedirectToAction("Index");
            }
            ViewBag.JassVariableID = new SelectList(db.JassVariables, "JassVariableID", "Name", jassmeasure.JassVariableID);
            return View(jassmeasure);
        }

        //
        // GET: /Measure/Delete/5

        public ActionResult Delete(int id = 0)
        {
            JassMeasure jassmeasure = db.JassMeasures.Find(id);
            if (jassmeasure == null)
            {
                return HttpNotFound();
            }
            return View(jassmeasure);
        }

        //
        // POST: /Measure/Delete/5

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(int id)
        {
            JassMeasure jassmeasure = db.JassMeasures.Find(id);
            db.JassMeasures.Remove(jassmeasure);
            db.SaveChanges();
            return RedirectToAction("Index");
        }

        protected override void Dispose(bool disposing)
        {
            db.Dispose();
            base.Dispose(disposing);
        }



        public ActionResult GetMeasureValue(int id)
        {
            JassMeasure jassmeasure = db.JassMeasures.Find(id);

                int in_x = jassmeasure.x;
                int in_y = jassmeasure.y;
                int in_year = jassmeasure.year;
                int in_month = jassmeasure.month;
                int in_day = jassmeasure.day;
                int in_hour3 = jassmeasure.hour3;
                int in_level = jassmeasure.level;

                string dayString = "" + in_year + "_" + in_month + "_" + in_day;


            long StartingMemory;
            DateTime StartingTime = DateTime.Now;
            long AfterOpenMemory;
            long AfterLoadMemory;
            DateTime EndingTime = DateTime.Now;
            TimeSpan TotalDelay;

            try
            {
                //Let try to re-create the file...
                GC.Collect();
                StartingMemory = GC.GetTotalMemory(true);

                string appDataFolder = HttpContext.Server.MapPath("~/App_Data");
                string timestamp = JassWeatherAPI.fileTimeStamp();

                //tas_WRFG_example_2014_2_3_11_10_31_322.nc

                List<string> files = apiCaller.listFiles_in_AppData();

                string inputFile3 = appDataFolder + "/envirolitic_air_" + dayString + ".nc";
                var dataset3 = Microsoft.Research.Science.Data.DataSet.Open(inputFile3 + "?openMode=open");

                AfterOpenMemory = GC.GetTotalMemory(true);

                short[] temperature = dataset3.GetData<short[]>("temperature",
                    Microsoft.Research.Science.Data.DataSet.FromToEnd(0),  //time
                    Microsoft.Research.Science.Data.DataSet.ReduceDim(in_level),  //level
                    Microsoft.Research.Science.Data.DataSet.ReduceDim(in_y),  //y
                    Microsoft.Research.Science.Data.DataSet.ReduceDim(in_x)); //x


                ViewBag.temperature = temperature;


                AfterLoadMemory = GC.GetTotalMemory(true);

                //To compare

                string inputFile1 = appDataFolder + "/ftp___ftp.cdc.noaa.gov_Datasets_NARR_pressure_air.201201.nc";
                var dataset1 = Microsoft.Research.Science.Data.DataSet.Open(inputFile1 + "?openMode=open");
                var schema1 = dataset1.GetSchema();

                Single[] y = dataset1.GetData<Single[]>("y");
                Single[] x = dataset1.GetData<Single[]>("x");
                double[] time = dataset1.GetData<double[]>("time");
                Single[] level = dataset1.GetData<Single[]>("level");

                short[] air = dataset1.GetData<short[]>("air",
                         Microsoft.Research.Science.Data.DataSet.Range(0,1,7),
                         Microsoft.Research.Science.Data.DataSet.ReduceDim(in_level),
                         Microsoft.Research.Science.Data.DataSet.ReduceDim(in_y),
                         Microsoft.Research.Science.Data.DataSet.ReduceDim(in_x));

                Int16[,,,] airSample = dataset1.GetData<Int16[,,,]>("air",
         Microsoft.Research.Science.Data.DataSet.Range(0, 1, 7),
         Microsoft.Research.Science.Data.DataSet.Range(0, 1, 7),
         Microsoft.Research.Science.Data.DataSet.Range(0, 1, 7),
         Microsoft.Research.Science.Data.DataSet.Range(0, 1, 7));

                ViewBag.air = air;
                ViewBag.airSample = airSample;

                EndingTime = DateTime.Now;
                TotalDelay = EndingTime - StartingTime;

                ViewBag.StartingMemory = StartingMemory / 1000000;
                ViewBag.TotalDelay = TotalDelay;
                ViewBag.AfterOpenMemory = AfterOpenMemory / 1000000;
                ViewBag.AfterLoadMemory = AfterLoadMemory / 1000000;
                ViewBag.AfterLoadDiffMemory = (AfterLoadMemory - AfterOpenMemory) / 1000000;
                ViewBag.Message = "Test executed correctly:";

            }
            catch (Exception e)
            {
                ViewBag.Message = e.Message; ;
            }
 
            return View();
        }
    }
}