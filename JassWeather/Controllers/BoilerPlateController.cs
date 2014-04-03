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
    public class BoilderPlateController : JassController
    {
        private JassWeatherContext db = new JassWeatherContext();
       

        #region Boilder Plate Open Close DropDown


        public class BoilerPlateModel
        {
            public JassLatLon JassLatLon { get; set; }
            public int JassLatLonId { get; set; }
        }

        public ActionResult BoilerPlate()
        {
            #region location selector
            ViewBag.LatLonGroupID = Session["LatLonGroupID"];
            int LatLonGroupID = (int)ViewBag.LatLonGroupID;
            var locationChoices = db.JassLatLons.Where(l => l.JassLatLonGroupID == LatLonGroupID);
            ViewBag.JassLatLonID = new SelectList(locationChoices, "JassLatLonID", "Name");
            #endregion

            BoilerPlateModel model = new BoilerPlateModel();

            return View(model);
        }

        [HttpPost]
        public ActionResult BoilerPlate(BoilerPlateModel model)
        {
            #region location selector
            ViewBag.LatLonGroupID = Session["LatLonGroupID"];
            int LatLonGroupID = (int)ViewBag.LatLonGroupID;
            var locationChoices = db.JassLatLons.Where(l => l.JassLatLonGroupID == LatLonGroupID);
            ViewBag.JasslatLonID = new SelectList(locationChoices, "JasslatLonID", "Name");
            #endregion

            return View(model);
        }


        #endregion

    }
}