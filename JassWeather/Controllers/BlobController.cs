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
        private JassWeatherContext db = new JassWeatherContext();


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
        //Download2Disk

        public ActionResult Download2Disk(string fileName)  //list container
        {
            try
            {
                apiCaller.DownloadFile2DiskIfNotThere(fileName, apiCaller.AppDataFolder + "\\" + fileName);
                ViewBag.message = "ok";
                return View();
            }
            catch (Exception e) {
                ViewBag.message = e.Message;
                return View();
            }
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

        public ActionResult ShowDashBoard4Day(string variableName, int yearIndex, int monthIndex, int dayIndex, int stepIndex, int levelIndex)  //list container
        {
            List<JassVariableStatus> variableStatusModel = apiCaller.listVariableStatus();

            JassWeatherAPI.VariableValueModel Model = new JassWeatherAPI.VariableValueModel();
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

        public ActionResult ShowDashBoard4DayFromFile(string fileName)  //list container
        {
            apiCaller.DownloadFile2DiskIfNotThere(fileName, apiCaller.AppDataFolder + "\\" + fileName);

            JassWeatherAPI.VariableValueModel Model = apiCaller.AnalyzeFileOnDisk(fileName);
            ViewBag.JassGridID = new SelectList(db.JassGrids, "JassGridID", "Name", Model.JassGridID);
            //hack for now:
            Model.fileName = fileName;
            return View("ShowDashBoard4DayFromFileFirst", Model);
        }

        [HttpPost]
        public ActionResult ShowDashBoard4DayFromFile(JassWeatherAPI.VariableValueModel Model)  //list container
        {
            if (Model.JassGridID != null)
            {

                Model.JassGrid = db.JassGrids.Find(Model.JassGridID);

                apiCaller.DownloadFile2DiskIfNotThere(Model.fileName, apiCaller.AppDataFolder + "\\" + Model.fileName);
                DateTime requestDate = new DateTime(Model.year, Model.monthIndex, Model.dayIndex);
                Model.gridValues = apiCaller.GetDayValues(Model.JassGrid, Model.fileName, Model.startingDate, requestDate);
                return View(Model);
            }
            else
            {
                //inspect file schema
                string filePath = apiCaller.AppDataFolder + "\\" + Model.fileName;
                string schema = apiCaller.AnalyzeFileDisk(filePath);
                Model.schema = schema;


                ViewBag.JassGridID = new SelectList(db.JassGrids, "JassGridID", "Name", Model.JassGridID);
                return View("ShowDashBoard4DayFromFileFirst", Model);
            }
        }

        public ActionResult ShowDashBoard4DayForm()  //list container
        {
            List<JassVariableStatus> variableStatusModel = apiCaller.listVariableStatus();
            JassWeatherAPI.VariableValueModel Model = new JassWeatherAPI.VariableValueModel();
            return View(Model);
        }

        public ActionResult ShowDashBoard4DayForm(JassWeatherAPI.VariableValueModel Model)  //list container
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

        public ActionResult ShowAppTempFiles()
        {
            List<string> files = new List<string>();
            try
            {
                files = apiCaller.listFiles_in_AppTempFiles();
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

        public class DeleteBlobRangeModel
        {
            public string VariableName { get; set; }
            public DateTime startDate { get; set; }
            public DateTime endDate { get; set; }
        }
        public ActionResult DeleteBlobRange()
        {

            //string result = apiCaller.deleteBlob(name, containerName);
            ViewBag.Message = "Hi";

            var Model = new DeleteBlobRangeModel();
            return View("DeleteBlobRangeFirst", Model);
        }
        [HttpPost]
        public ActionResult DeleteBlobRange(DeleteBlobRangeModel Model)
        {

            DateTime day = Model.startDate;
            int totalDays = (int)(Model.endDate - Model.startDate).TotalDays;
            string name;
            string containerName = Model.VariableName.ToLower();
            string allNames = "";
            for (int d = 0; d < totalDays; d++)
            {
                name = apiCaller.fileNameBuilderByDay(containerName, day.Year, day.Month, day.Day) + ".nc";
                allNames = allNames + name;
            }
            //string result = apiCaller.deleteBlob(name, containerName);
            ViewBag.Message = allNames;
            return View(Model);
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
