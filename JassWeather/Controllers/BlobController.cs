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


        #region download

        public class DownloadSubsetModel
        {
            public string Message { get; set; }
    
            public int year { get; set; }
            public int month { get; set; }
            public int day { get; set; }

            public int yearEnd { get; set; }
            public int monthEnd { get; set; }
            public int dayEnd { get; set; }

            public int totalDays { get; set; }

            public Boolean[] variableChoices { get; set; }
            public List<JassVariable> variables { get; set; }
            public String[,] reportMessage { get; set; }
            public int[,] reportStatus { get; set; }
            public int numberOfErrors { get; set; }
        }


        public ActionResult DownloadSubset()
        {
            DownloadSubsetModel model = new DownloadSubsetModel();
            model.variables = db.JassVariables.ToList();
            model.variableChoices = new Boolean[model.variables.Count];
          
            model.year = 2010;
            model.month = 1;
            model.day = 1;

            return View("DownloadSubsetFirst", model);
        }
        [HttpPost]
        public ActionResult DownloadSubset(DownloadSubsetModel model)
        {
            model.numberOfErrors = 0;
                DateTime startDate = new DateTime(model.year, model.month, model.day);
                if (model.yearEnd == 0) model.yearEnd = model.year;
                if (model.monthEnd == 0) model.monthEnd = model.month;
                if (model.dayEnd == 0) model.dayEnd = model.day;

                DateTime endDate = new DateTime(model.yearEnd, model.monthEnd, model.dayEnd);
                if (endDate < startDate) { endDate = startDate; }
                int totalDays = (int)(endDate - startDate).TotalDays + 1;
                model.totalDays = totalDays;

                model.variables = db.JassVariables.ToList();

                model.reportMessage = new String[model.variables.Count, totalDays];
                model.reportStatus = new int[model.variables.Count, totalDays];

                for (int v = 0; v < model.variables.Count; v++)
                {
                    if (model.variableChoices[v])
                    {
                        DateTime day = startDate;
                        string fileName2Download;
                        string filePath2Download;
                        for (int d = 0; d < totalDays; d++)
                        {
                            try
                            {

                                //download
                                fileName2Download = apiCaller.fileNameBuilderByDay(model.variables[v].Name, day.Year, day.Month, day.Day)+".nc";
                                filePath2Download =  apiCaller.AppDataFolder + "/" + fileName2Download;
                                apiCaller.downloadBlob(model.variables[v].Name.ToLower(), fileName2Download, filePath2Download);
                                model.reportStatus[v, d] = 1;
                                model.reportMessage[v,d] = "ok";

                            }
                            catch (Exception e)
                            {
                                model.reportStatus[v, d] = 0;
                                model.reportMessage[v, d] = e.Message;
                                model.numberOfErrors++;
                            }
                            day = day.AddDays(1);
                        }
                    }
                }

                return View(model);
            }

        #endregion download

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
        public ActionResult Download2TempDisk(string fileName)  //list container
        {
            try
            {
                apiCaller.DownloadFile2DiskIfNotThere(fileName, apiCaller.AppTempFilesFolder + "\\" + fileName);
                ViewBag.message = "ok";
                return View();
            }
            catch (Exception e)
            {
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

            if (variableName == "Sheridan")              {
                Model.colorCode = db.JassColorCodes.Find(1);
            }

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


        public ActionResult DeleteBlobRange()
        {

            //string result = apiCaller.deleteBlob(name, containerName);
            ViewBag.Message = "Hi";

            var Model = new JassWeatherAPI.DeleteBlobRangeModel();
            return View("DeleteBlobRangeFirst", Model);
        }
        [HttpPost]
        public ActionResult DeleteBlobRange(JassWeatherAPI.DeleteBlobRangeModel Model)
        {

            DateTime day = Model.startDate;
            int totalDays = (int)(Model.endDate - Model.startDate).TotalDays + 1;
            string name;
            Model.blobNames = new List<string>();
            for (int d = 0; d < totalDays; d++)
            {
                name = apiCaller.fileNameBuilderByDay(Model.VariableName, day.Year, day.Month, day.Day) + ".nc";
                Model.blobNames.Add(name);
                day = day.AddDays(1);
            }
            if (Model.confirmed)
            {
                string result = apiCaller.deleteBlobRange(Model);
            }
            ViewBag.Message = "Ok";
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

        public ActionResult CleanAppTempFiles()
        {
            bool result = apiCaller.cleanAppTempFiles();
            List<string> files = apiCaller.listFiles_in_AppTempFiles();
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
