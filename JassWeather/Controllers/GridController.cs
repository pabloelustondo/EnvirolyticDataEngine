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
       
        
        #region Testing Grids


        public class PlotGridPointsAroundLocationModel
        {
            public JassLatLon JassLatLon { get; set; }
            public int JassLatLonID { get; set; }
            public JassWeatherAPI.JassMaccNarrGridsCombo GridNarrCombo { get; set; }

        }

        public ActionResult PlotGridPointsAroundLocation()
        {
            //The purpose of this method is to make sure we are mapping grids correctly
            //one way of doing this is by taking one location.
            //ploting google maps points in one grid then on another grid. OS basicaly, it relies on ploting
            //so i will start by that, taking a location, and ploting it on the google map. something i almost have   

            #region location selector
            ViewBag.LatLonGroupID = Session["LatLonGroupID"];
            int LatLonGroupID = (int)ViewBag.LatLonGroupID;
            var locationChoices = db.JassLatLons.Where(l => l.JassLatLonGroupID == LatLonGroupID);
            ViewBag.JassLatLonID = new SelectList(locationChoices, "JassLatLonID", "Name");
            #endregion

            PlotGridPointsAroundLocationModel model = new PlotGridPointsAroundLocationModel();
 
            return View("PlotGridPointsAroundLocationFirst",model);
        }

        [HttpPost]
        public ActionResult PlotGridPointsAroundLocation(PlotGridPointsAroundLocationModel model)
        {
            //The purpose of this method is to make sure we are mapping grids correctly
            //one way of doing this is by taking one location.
            //ploting google maps points in one grid then on another grid. OS basicaly, it relies on ploting
            //so i will start by that, taking a location, and ploting it on the google map. something i almost have   

            #region location selector
            ViewBag.LatLonGroupID = Session["LatLonGroupID"];
            int LatLonGroupID = (int)ViewBag.LatLonGroupID;
            var locationChoices = db.JassLatLons.Where(l => l.JassLatLonGroupID == LatLonGroupID);
            ViewBag.JasslatLonID = new SelectList(locationChoices, "JasslatLonID", "Name");

            model.JassLatLon = db.JassLatLons.Find(model.JassLatLonID);
            #endregion

            return View(model);
        }

        public ActionResult ShowGridMap(string mapperFileName, int JassLatLonID)
        {
            JassWeatherAPI.SmartGridMap result = apiCaller.getMapComboFromMapFile(mapperFileName);
            result.JassLatLonID = JassLatLonID;
            result.JassLatLon = db.JassLatLons.Find(JassLatLonID);
            return View(result);
        }

        #endregion


        public ActionResult Index()
        {
            var jassgrids = db.JassGrids.Include(j => j.JassPartition);
            return View(jassgrids.ToList());
        }

       
        public ActionResult MapMacc2NarrTest()
        {
            JassWeather.Models.JassWeatherAPI.JassMaccNarrGridsCombo result = apiCaller.MapGridNarr2MaccFromFile(
                "netcdf-web238-20140306020857-10515-0608.nc",
                "ftp___ftp.cdc.noaa.gov_Datasets_NARR_pressure_air.201201.nc");
            return View("MapGrid2NarrTest",result);
        }
   
        public ActionResult MapCFSR2NarrTest()
        {
            DateTime startTime = DateTime.Now;
            JassWeather.Models.JassWeatherAPI.JassMaccNarrGridsCombo result = apiCaller.MapGridNarr2GridFromFile(
                "pgbhnl.gdas.20101201-20101205.grb2.nc",
                "lat",
                "lon",
                "Narr_Grid.nc",
                "Narr_2_CFSR_Grid_Mapper.nc", true, false);
            DateTime endTime = DateTime.Now;
            ViewBag.elapsedTime = (endTime - startTime);
            return View("MapGrid2NarrTest",result);
        }

      
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


        public ActionResult CreateEnvirolyticNarrGrid()
        {
            apiCaller.CreateEnvirolyticNarrGrid();
            return View();
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