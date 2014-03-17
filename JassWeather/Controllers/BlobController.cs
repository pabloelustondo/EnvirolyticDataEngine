using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using JassWeather.Models;
using Microsoft.WindowsAzure.Storage.Blob;
using System.IO;

namespace JassWeather.Controllers
{
    [Authorize]
    public class BlobController : JassController
    {

        // GET: /Blob/
        public ActionResult Index()  //list container
        {
            List<CloudBlobContainer> blobs = apiCaller.listContainers();
            return View(blobs);
        }

        public ActionResult ListBlobs(string containerName)
        {

            List<CloudBlockBlob> blobs = apiCaller.listBlobs(containerName);
            ViewBag.containerName = containerName;
            return View(blobs);
        }

        public ActionResult ShowDashBoardExt()  //list container
        {
            Session["StorageConnectionString"] = "StorageConnectionStringProd";
            apiCaller = new JassWeatherAPI(ServerName, HttpContext.Server.MapPath("~/App_Data"), (string)Session["StorageConnectionString"]);
            List<JassVariableStatus> variableStatusModel = apiCaller.listVariableStatus();
            return View("ShowDashBoard",variableStatusModel);
        }

        public ActionResult ShowDashBoard()  //list container
        {
            Session["StorageConnectionString"] = "StorageConnectionStringDev";
            apiCaller = new JassWeatherAPI(ServerName, HttpContext.Server.MapPath("~/App_Data"), (string)Session["StorageConnectionString"]);
            List<JassVariableStatus> variableStatusModel = apiCaller.listVariableStatus();
            return View(variableStatusModel);
        }


        public ActionResult ShowDashBoard4Year(int yearIndex)  //list container
        {
            List<JassVariableStatus> variableStatusModel = apiCaller.listVariableStatus();
            ViewBag.year = yearIndex + (DateTime.Now.Year - 9);
            ViewBag.yearIndex = yearIndex;
            return View(variableStatusModel);
        }

        public ActionResult ShowDashBoard4Month(int yearIndex, int monthIndex)  //list container
        {
            List<JassVariableStatus> variableStatusModel = apiCaller.listVariableStatus();
            ViewBag.year = yearIndex + (DateTime.Now.Year - 9);
            ViewBag.yearIndex = yearIndex;
            ViewBag.monthIndex = monthIndex;
            ViewBag.numberOfDays = variableStatusModel[0].StatusDayLevel[yearIndex][monthIndex].Count;
            return View(variableStatusModel);
        }

        public class ShowDashBoard4DayViewModel
        {
            public int year { get; set; }
            public int yearIndex { get; set; }
            public int monthIndex { get; set; }
            public int dayIndex { get; set; }
            public int stepIndex { get; set; }
            public int levelIndex { get; set; }
            public int numberOfDays { get; set; }
            public string fileName { get; set; }
            public string variableName { get; set; }
            public JassGridValues gridValues  { get; set; }

        }
        public ActionResult ShowDashBoard4Day(string variableName, int yearIndex, int monthIndex, int dayIndex, int stepIndex, int levelIndex)  //list container
        {
            List<JassVariableStatus> variableStatusModel = apiCaller.listVariableStatus();

            ShowDashBoard4DayViewModel Model = new ShowDashBoard4DayViewModel();
            Model.year = yearIndex + (DateTime.Now.Year - 9);
            Model.yearIndex = yearIndex;
            Model.monthIndex = monthIndex;
            Model.dayIndex = dayIndex;
            Model.stepIndex = stepIndex;
            Model.levelIndex = levelIndex;
            Model.numberOfDays = variableStatusModel[0].StatusDayLevel[yearIndex][monthIndex].Count;
            string fileName = apiCaller.fileNameBuilderByDay(variableName, Model.year, monthIndex + 1, dayIndex + 1) + ".nc";
            Model.gridValues = apiCaller.GetDayValues(fileName);
            Model.fileName = fileName;
            Model.variableName = variableName;

            return View(Model);
        }

        public ActionResult ShowDashBoard4DayForm()  //list container
        {
            List<JassVariableStatus> variableStatusModel = apiCaller.listVariableStatus();
            ShowDashBoard4DayViewModel Model = new ShowDashBoard4DayViewModel();
            return View(Model);
        }

        public ActionResult ShowDashBoard4DayForm(ShowDashBoard4DayViewModel Model)  //list container
        {
            List<JassVariableStatus> variableStatusModel = apiCaller.listVariableStatus();
            Model.gridValues = apiCaller.GetDayValues(Model.fileName);
            return View(Model);
        }

        public ActionResult ShowAppData()
        {
            List<string> files = new List<string>();
            try
            {
                files = apiCaller.listFiles_in_AppData();
            }
            catch (Exception e)
            {
                ViewBag.Message = e.Message;

            }
            return View(files);
        }

        public ActionResult ShowTables()
        {
            List<string> files = apiCaller.listTables();
            return View(files);
        }

        public ActionResult ShowTable(string tableName)
        {
            List<string> files = apiCaller.listTableValues(tableName);
            ViewBag.TableName = tableName;
            return View(files);
        }


        public ActionResult SampleNetCDF(string fileName)
        {
            List<string> rows = apiCaller.listNetCDFValues(fileName);
            ViewBag.FileName = fileName;
            return View(rows);
        }

        public ActionResult DeleteContainer(string name)
        {

            string result = apiCaller.deleteContainer(name);
            ViewBag.Message = result;

            return View();
        }

        public ActionResult DeleteBlob(string name, string containerName)
        {

            string result = apiCaller.deleteBlob(name, containerName);
            ViewBag.Message = result;

            return View();
        }

        public ActionResult DeleteFromAppData(string fileName)
        {
            bool result = apiCaller.deleteFile_in_AppData(fileName);
            ViewBag.Message = result;
            return View("Index");
        }

        public ActionResult CleanAppData()
        {
            bool result = apiCaller.cleanAppData();
            List<string> files = apiCaller.listFiles_in_AppData();
            return View(files);
        }

        public ActionResult DeleteTable(string fileName)
        {
            bool result = apiCaller.deleteTable(fileName);
            ViewBag.Message = result;
            return View();
        }

    }
}
