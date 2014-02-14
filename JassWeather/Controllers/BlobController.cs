using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using JassWeather.Models;
using Microsoft.WindowsAzure.Storage.Blob;

namespace JassWeather.Controllers
{
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
            return View(blobs);
        }

        public ActionResult ShowDashBoard()  //list container
        {
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

        public ActionResult ShowDashBoard4Day(string variableName, int yearIndex, int monthIndex, int dayIndex)  //list container
        {
            List<JassVariableStatus> variableStatusModel = apiCaller.listVariableStatus();
            ViewBag.year = yearIndex + (DateTime.Now.Year - 9);
            ViewBag.yearIndex = yearIndex;
            ViewBag.monthIndex = monthIndex;
            ViewBag.dayIndex = dayIndex;
            ViewBag.numberOfDays = variableStatusModel[0].StatusDayLevel[yearIndex][monthIndex].Count;
            string fileName = apiCaller.fileNameBuilderByDay(variableName, ViewBag.year, monthIndex, dayIndex) + ".nc";
            List<string> rows = apiCaller.listNetCDFValues(fileName);
            ViewBag.FileName = fileName;

            return View(rows);
        }

        public ActionResult ShowAppData()
        {

            List<string> files = apiCaller.listFiles_in_AppData(HttpContext.Server.MapPath("~/App_Data"));
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

        public ActionResult DeleteFromAppData(string fileName)
        {
            bool result = apiCaller.deleteFile_in_AppData(fileName);
            ViewBag.Message = result;
            return View();
        }

        public ActionResult CleanAppData()
        {
            bool result = apiCaller.deleteAll();
            ViewBag.Message = result;
            return View();
        }

        public ActionResult DeleteTable(string fileName)
        {
            bool result = apiCaller.deleteTable(fileName);
            ViewBag.Message = result;
            return View();
        }

    }
}
