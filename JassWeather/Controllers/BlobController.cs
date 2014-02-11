﻿using System;
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
        //
        // GET: /Blob/
        public ActionResult Index()
        {
            List<CloudBlockBlob> blobs = apiCaller.listBlobs_in_envirolytics();
            return View(blobs);
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

        public ActionResult Delete(string name)
        {

            string result = apiCaller.deleteBlob_in_envirolytics(name);
            ViewBag.Message = result;

            return View();
        }

        public ActionResult DeleteFromAppData(string fileName)
        {
            bool result = apiCaller.deleteFile_in_AppData(fileName);
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
