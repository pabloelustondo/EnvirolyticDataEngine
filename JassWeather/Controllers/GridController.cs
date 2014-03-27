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
    [Authorize]
    public class GridController : JassController
    {
        private JassWeatherContext db = new JassWeatherContext();

        //
        // GET: /Grid/

        public ActionResult Index()
        {
            var jassgrids = db.JassGrids.Include(j => j.JassPartition);
            return View(jassgrids.ToList());
        }

        [AllowAnonymous]
        public ActionResult MapMacc2NarrTest()
        {
            JassWeather.Models.JassWeatherAPI.JassMaccNarrGridsCombo result = apiCaller.MapGridNarr2MaccFromFile(
                "netcdf-web238-20140306020857-10515-0608.nc",
                "ftp___ftp.cdc.noaa.gov_Datasets_NARR_pressure_air.201201.nc");
            return View("MapGrid2NarrTest",result);
        }
        [AllowAnonymous]
        public ActionResult MapCFSR2NarrTest()
        {
            JassWeather.Models.JassWeatherAPI.JassMaccNarrGridsCombo result = apiCaller.MapGridNarr2GridFromFile(
                "pgbhnl.gdas.20101201-20101205.grb2.nc",
                "lat",
                "lon",
                "Narr_Grid.nc",
                "Narr_2_CFSR_Grid_Mapper.nc", true, false);
            return View("MapGrid2NarrTest",result);
        }

        [AllowAnonymous]
        public ActionResult MapCFSR2NarrReal()
        {
            JassWeather.Models.JassWeatherAPI.JassMaccNarrGridsCombo result = apiCaller.MapGridNarr2GridFromFile(
                "pgbhnl.gdas.20101201-20101205.grb2.nc",
                "lat",
                "lon",
                "Narr_Grid.nc",
                "Narr_2_CFSR_Grid_Mapper.nc", false, false);
            return View("MapGrid2NarrTest", result);
        }

        [AllowAnonymous]
        public ActionResult MapSher2NarrTest()
        {
            JassWeather.Models.JassWeatherAPI.JassMaccNarrGridsCombo result = apiCaller.MapGridNarr2GridFromFile(
                "sheridan_stations.nc",
                "lat",
                "lon",
                "Narr_Grid.nc",
                "Narr_2_SHER_Grid_Mapper.nc", true, true);
            return View("MapSher2NarrTest", result);
        }

        [AllowAnonymous]
        public ActionResult MapSher2NarrReal()
        {
            JassWeather.Models.JassWeatherAPI.JassMaccNarrGridsCombo result = apiCaller.MapGridNarr2GridFromFile(
                "sheridan_stations.nc",
                "lat",
                "lon",
                "Narr_Grid.nc",
                "Narr_2_SHER_Grid_Mapper.nc", false, true);
            return View("MapSher2NarrTest", result);
        }
        [AllowAnonymous]
        public ActionResult ShowGridMap()
        {
            JassWeather.Models.JassWeatherAPI.SmartGridMap result = apiCaller.getMapComboFromMapFile("Narr_2_CFSR_Grid_Mapper.nc");
            return View(result);
        }

        //
        // GET: /Grid/Details/5

        public ActionResult Details(int id = 0)
        {
            JassGrid jassgrid = db.JassGrids.Find(id);
            if (jassgrid == null)
            {
                return HttpNotFound();
            }
            return View(jassgrid);
        }

        //
        // GET: /Grid/Create

        public ActionResult Create()
        {
            ViewBag.JassPartitionID = new SelectList(db.JassPartitions, "JassPartitionID", "Name");
            return View();
        }

        //
        // POST: /Grid/Create

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create(JassGrid jassgrid)
        {
            if (ModelState.IsValid)
            {
                db.JassGrids.Add(jassgrid);
                db.SaveChanges();
                return RedirectToAction("Index");
            }

            ViewBag.JassPartitionID = new SelectList(db.JassPartitions, "JassPartitionID", "Name", jassgrid.JassPartitionID);
            return View(jassgrid);
        }

        //
        // GET: /Grid/Edit/5

        public ActionResult Edit(int id = 0)
        {
            JassGrid jassgrid = db.JassGrids.Find(id);
            if (jassgrid == null)
            {
                return HttpNotFound();
            }
            ViewBag.JassPartitionID = new SelectList(db.JassPartitions, "JassPartitionID", "Name", jassgrid.JassPartitionID);
            return View(jassgrid);
        }

        //
        // POST: /Grid/Edit/5

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit(JassGrid jassgrid)
        {
            if (ModelState.IsValid)
            {
                db.Entry(jassgrid).State = EntityState.Modified;
                db.SaveChanges();
                return RedirectToAction("Index");
            }
            ViewBag.JassPartitionID = new SelectList(db.JassPartitions, "JassPartitionID", "Name", jassgrid.JassPartitionID);
            return View(jassgrid);
        }

        //
        // GET: /Grid/Delete/5

        public ActionResult Delete(int id = 0)
        {
            JassGrid jassgrid = db.JassGrids.Find(id);
            if (jassgrid == null)
            {
                return HttpNotFound();
            }
            return View(jassgrid);
        }

        //
        // POST: /Grid/Delete/5

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(int id)
        {
            JassGrid jassgrid = db.JassGrids.Find(id);
            db.JassGrids.Remove(jassgrid);
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