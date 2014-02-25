using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.IO;
using System.Web;
using System.Web.Mvc;
using JassWeather.Models;
using Microsoft.Research.Science.Data;
using Microsoft.Research.Science.Data.Imperative;

namespace JassWeather.Controllers
{
    [Authorize]
    public class RequestController : JassController
    {
        private JassWeatherContext db = new JassWeatherContext();
        int? CurrentRequestSetID;
        string CurrentRequestSetName;

        protected override void OnActionExecuting(ActionExecutingContext filterContext)
        {
            base.OnActionExecuting(filterContext);
            CurrentRequestSetID = (int?)Session["CurrentRequestSetId"];
            CurrentRequestSetName = (string)Session["CurrentRequestSetName"];
        }

        //
        // GET: /Request/

        public ActionResult Index()
        {
            if (CurrentRequestSetID != null)
            {
                APIRequestSet apirequestset = db.APIRequestSets.Find(CurrentRequestSetID);

                return View(apirequestset);
            }
            else
            {
                return HttpNotFound();
            }

        }

        //
        // GET: /Request/Details/5

        public ActionResult Download(int id = 0)
        {
            APIRequest apiRequest = db.APIRequests.Find(id);

            string maxFileSizeS = "1000000000000000";



            int typeIndex = apiRequest.type.IndexOf("#");
            if (typeIndex > 0)
            {
                maxFileSizeS = apiRequest.type.Substring(typeIndex + 1);
                apiRequest.type = apiRequest.type.Substring(0, typeIndex);
            }

            double maxFileSize = Double.Parse(maxFileSizeS);

            DateTime StartTime = DateTime.Now;
            DateTime EndTime = DateTime.Now;

            if (apiRequest.type == "json")
            {

                ViewBag.request = apiRequest.url;
                ViewBag.response = apiCaller.ping_Json_DataSource(apiRequest.url);
                EndTime = DateTime.Now;
                TimeSpan TotalTime = EndTime - StartTime;
                ViewBag.TotalTime = "hs: " + TotalTime.TotalHours + "mins: " + TotalTime.TotalMinutes + "secs: " + TotalTime.TotalSeconds;
                return View("ShowJSON");
            }

            if (apiRequest.type == "netCDF")
            {
                string workingDirectorypath = HttpContext.Server.MapPath("~/App_Data");

                ViewBag.request = apiRequest.url;
                ViewBag.response = apiCaller.ping_small_NetCDF_file(apiRequest.url,workingDirectorypath);
                TimeSpan TotalTime = EndTime - StartTime;
                ViewBag.TotalTime = "hs: " + TotalTime.TotalHours + "mins: " + TotalTime.TotalMinutes + "secs: " + TotalTime.TotalSeconds;
                return View("ShowNetCDF");
            }

            if (apiRequest.type == "netCDFFtp")
            {
                string workingDirectorypath = HttpContext.Server.MapPath("~/App_Data");
                ViewBag.request = apiRequest.url;
                ViewBag.response = apiCaller.get_big_NetCDF_by_ftp(apiRequest.url, workingDirectorypath);
                TimeSpan TotalTime = EndTime - StartTime;
                ViewBag.TotalTime = "hs: " + TotalTime.TotalHours + "mins: " + TotalTime.TotalMinutes + "secs: " + TotalTime.TotalSeconds;
                return View("ShowNetCDF");
            }

            if (apiRequest.type == "netCDFFtp2")
            {
                string workingDirectorypath = HttpContext.Server.MapPath("~/App_Data");
                ViewBag.request = apiRequest.url;
                ViewBag.response = apiCaller.get_big_NetCDF_by_ftp2(apiRequest.url, workingDirectorypath);
                TimeSpan TotalTime = EndTime - StartTime;
                ViewBag.TotalTime = "hs: " + TotalTime.TotalHours + "mins: " + TotalTime.TotalMinutes + "secs: " + TotalTime.TotalSeconds;
                return View("ShowNetCDF");
            }

            if (apiRequest.type == "netCDFFtp3")
            {
                string workingDirectorypath = HttpContext.Server.MapPath("~/App_Data");
                ViewBag.request = apiRequest.url;
                ViewBag.response = apiCaller.get_big_NetCDF_by_ftp3(apiRequest.url, workingDirectorypath);
                TimeSpan TotalTime = EndTime - StartTime;
                ViewBag.TotalTime = "hs: " + TotalTime.TotalHours + "mins: " + TotalTime.TotalMinutes + "secs: " + TotalTime.TotalSeconds;
                return View("ShowNetCDF");
            }

            if (apiRequest.type == "netCDFFtp4")
            {
                string workingDirectorypath = HttpContext.Server.MapPath("~/App_Data");
                ViewBag.request = apiRequest.url;
                ViewBag.response = apiCaller.get_big_NetCDF_by_ftp4(apiRequest.url, workingDirectorypath);
                TimeSpan TotalTime = EndTime - StartTime;
                ViewBag.TotalTime = "hs: " + TotalTime.TotalHours + "mins: " + TotalTime.TotalMinutes + "secs: " + TotalTime.TotalSeconds;
                return View("ShowNetCDF");
            }

            if (apiRequest.type == "FTP-netCDF")
            {
                string workingDirectorypath = HttpContext.Server.MapPath("~/App_Data");
                ViewBag.request = apiRequest.url;
                ViewBag.response = apiCaller.get_big_NetCDF_by_ftp5(apiRequest.url, workingDirectorypath, maxFileSize);
                TimeSpan TotalTime = EndTime - StartTime;
                ViewBag.TotalTime = "hs: " + TotalTime.TotalHours + "mins: " + TotalTime.TotalMinutes + "secs: " + TotalTime.TotalSeconds;
                return View("ShowNetCDF");
            }


            return View("ShowError");

        }

        public ActionResult DownloadBlob(int id = 0)
        {
            APIRequest apiRequest = db.APIRequests.Find(id);
            var blobName = apiCaller.safeFileNameFromUrl(apiRequest.url);
            var filenPath = apiCaller.AppDataFolder + "/" + apiCaller.safeFileNameFromUrl(apiRequest.url);
            apiCaller.downloadBlob("envirolytic", blobName, filenPath);
            return View();
        }

        public ActionResult UploadBlob(int id = 0)
        {
            APIRequest apiRequest = db.APIRequests.Find(id);
            var blobName = apiCaller.safeFileNameFromUrl(apiRequest.url);
            var filenPath = apiCaller.AppDataFolder + "/" + apiCaller.safeFileNameFromUrl(apiRequest.url);
            apiCaller.uploadBlob("envirolytic", blobName, filenPath);
            return View();
        }

        public ActionResult Download2Blob(int id = 0)
        {
            APIRequest apiRequest = db.APIRequests.Find(id);

            string maxFileSizeS = "1000000000000000";



            int typeIndex = apiRequest.type.IndexOf("#");
            if (typeIndex > 0)
            {
                maxFileSizeS = apiRequest.type.Substring(typeIndex + 1);
                apiRequest.type = apiRequest.type.Substring(0, typeIndex);
            }

            double maxFileSize = Double.Parse(maxFileSizeS);

            DateTime StartTime = DateTime.Now;
            DateTime EndTime = DateTime.Now;

            if (apiRequest.type == "json")
            {
 
                ViewBag.request = apiRequest.url;
                ViewBag.response = apiCaller.ping_Json_DataSource(apiRequest.url);
                EndTime = DateTime.Now;
                TimeSpan TotalTime = EndTime - StartTime;
                ViewBag.TotalTime = "hs: " + TotalTime.TotalHours + "mins: " + TotalTime.TotalMinutes + "secs: " + TotalTime.TotalSeconds;
                return View("ShowJSON");
            }

            if (apiRequest.type == "netCDF")
            {
                string workingDirectorypath = HttpContext.Server.MapPath("~/App_Data");
         
                ViewBag.request = apiRequest.url;
                ViewBag.response = apiCaller.ping_small_NetCDF_file(apiRequest.url, workingDirectorypath);
                TimeSpan TotalTime = EndTime - StartTime;
                ViewBag.TotalTime = "hs: " + TotalTime.TotalHours + "mins: " + TotalTime.TotalMinutes + "secs: " + TotalTime.TotalSeconds;
                return View("ShowNetCDF");
            }

            if (apiRequest.type == "netCDFFtp")
            {
                string workingDirectorypath = HttpContext.Server.MapPath("~/App_Data");

                ViewBag.request = apiRequest.url;
                ViewBag.response = apiCaller.get_big_NetCDF_by_ftp(apiRequest.url, workingDirectorypath);
                TimeSpan TotalTime = EndTime - StartTime;
                ViewBag.TotalTime = "hs: " + TotalTime.TotalHours + "mins: " + TotalTime.TotalMinutes + "secs: " + TotalTime.TotalSeconds;
                return View("ShowNetCDF");
            }

            if (apiRequest.type == "netCDFFtp2")
            {
                string workingDirectorypath = HttpContext.Server.MapPath("~/App_Data");
       
                ViewBag.request = apiRequest.url;
                ViewBag.response = apiCaller.get_big_NetCDF_by_ftp2(apiRequest.url, workingDirectorypath);
                TimeSpan TotalTime = EndTime - StartTime;
                ViewBag.TotalTime = "hs: " + TotalTime.TotalHours + "mins: " + TotalTime.TotalMinutes + "secs: " + TotalTime.TotalSeconds;
                return View("ShowNetCDF");
            }

            if (apiRequest.type == "netCDFFtp3")
            {
                string workingDirectorypath = HttpContext.Server.MapPath("~/App_Data");
 
                ViewBag.request = apiRequest.url;
                ViewBag.response = apiCaller.get_big_NetCDF_by_ftp3(apiRequest.url, workingDirectorypath);
                TimeSpan TotalTime = EndTime - StartTime;
                ViewBag.TotalTime = "hs: " + TotalTime.TotalHours + "mins: " + TotalTime.TotalMinutes + "secs: " + TotalTime.TotalSeconds;
                return View("ShowNetCDF");
            }

            if (apiRequest.type == "netCDFFtp4")
            {
                string workingDirectorypath = HttpContext.Server.MapPath("~/App_Data");

                ViewBag.request = apiRequest.url;
                ViewBag.response = apiCaller.get_big_NetCDF_by_ftp4(apiRequest.url, workingDirectorypath);
                TimeSpan TotalTime = EndTime - StartTime;
                ViewBag.TotalTime = "hs: " + TotalTime.TotalHours + "mins: " + TotalTime.TotalMinutes + "secs: " + TotalTime.TotalSeconds;
                return View("ShowNetCDF");
            }

            if (apiRequest.type == "FTP-netCDF")
            {
                string workingDirectorypath = HttpContext.Server.MapPath("~/App_Data");
  
                ViewBag.request = apiRequest.url;
                ViewBag.response = apiCaller.get_big_NetCDF_by_ftp5(apiRequest.url, workingDirectorypath, maxFileSize);
                TimeSpan TotalTime = EndTime - StartTime;
                ViewBag.TotalTime = "hs: " + TotalTime.TotalHours + "mins: " + TotalTime.TotalMinutes + "secs: " + TotalTime.TotalSeconds;
                return View("ShowNetCDF");
            }


            return View("ShowError");

        }


        public ActionResult Add2Table(int id = 0)
        {

            APIRequest apiRequest = db.APIRequests.Find(id);

            string maxFileSizeS = "1000";



            int typeIndex = apiRequest.type.IndexOf("#");
            if (typeIndex > 0)
            {
                maxFileSizeS = apiRequest.type.Substring(typeIndex + 1);
                apiRequest.type = apiRequest.type.Substring(0, typeIndex);
            }

            double maxFileSize = Double.Parse(maxFileSizeS);
            
 
            string schemaString = "";
            try
            {
                DateTime StartTime = DateTime.Now;

                string url = apiRequest.url;
                string safeFileName = url.Replace('/', '_').Replace(':', '_').TrimStart().TrimEnd();
                string downloadedFilePath = HttpContext.Server.MapPath("~/App_Data/" + safeFileName);

                schemaString = apiCaller.store2table(downloadedFilePath, (int)maxFileSize);

                DateTime EndTime = DateTime.Now;

                TimeSpan TotalTime = EndTime - StartTime;
                apiRequest.startLoadTime = StartTime;
                apiRequest.endLoadTime = EndTime;
                apiRequest.spanLoadTime = TotalTime;
                apiRequest.onDisk = "File";
                db.Entry(apiRequest).State = System.Data.EntityState.Modified;
                db.SaveChanges();
            }
            catch (Exception e)
            {
                return View("ShowError");
            }

            ViewBag.schemaString = schemaString;
            return View();
        }

        public ActionResult Download2Disk(int id = 0)
        {
            APIRequest apiRequest = db.APIRequests.Find(id);

            string maxFileSizeS = "1000000000000000";



            int typeIndex = apiRequest.type.IndexOf("#");
            if (typeIndex > 0)
            {
                maxFileSizeS = apiRequest.type.Substring(typeIndex + 1);
                apiRequest.type = apiRequest.type.Substring(0, typeIndex);
            }

            double maxFileSize = Double.Parse(maxFileSizeS);

            DateTime StartTime = DateTime.Now;
            DateTime EndTime = DateTime.Now;

            if (apiRequest.type == "json")
            {

                ViewBag.request = apiRequest.url;
                ViewBag.response = apiCaller.ping_Json_DataSource(apiRequest.url);
                EndTime = DateTime.Now;
                TimeSpan TotalTime = EndTime - StartTime;
                ViewBag.TotalTime = "hs: " + TotalTime.TotalHours + "mins: " + TotalTime.TotalMinutes + "secs: " + TotalTime.TotalSeconds;
                return View("ShowJSON");
            }

            if (apiRequest.type == "netCDF")
            {
                string workingDirectorypath = HttpContext.Server.MapPath("~/App_Data");

                ViewBag.request = apiRequest.url;
                ViewBag.response = apiCaller.ping_small_NetCDF_file(apiRequest.url, workingDirectorypath);
                TimeSpan TotalTime = EndTime - StartTime;
                ViewBag.TotalTime = "hs: " + TotalTime.TotalHours + "mins: " + TotalTime.TotalMinutes + "secs: " + TotalTime.TotalSeconds;
                return View("ShowNetCDF");
            }

            if (apiRequest.type == "netCDFFtp")
            {
                string workingDirectorypath = HttpContext.Server.MapPath("~/App_Data");

                ViewBag.request = apiRequest.url;
                ViewBag.response = apiCaller.get_big_NetCDF_by_ftp(apiRequest.url, workingDirectorypath);
                TimeSpan TotalTime = EndTime - StartTime;
                ViewBag.TotalTime = "hs: " + TotalTime.TotalHours + "mins: " + TotalTime.TotalMinutes + "secs: " + TotalTime.TotalSeconds;
                return View("ShowNetCDF");
            }

            if (apiRequest.type == "netCDFFtp2")
            {
                string workingDirectorypath = HttpContext.Server.MapPath("~/App_Data");

                ViewBag.request = apiRequest.url;
                ViewBag.response = apiCaller.get_big_NetCDF_by_ftp2(apiRequest.url, workingDirectorypath);
                TimeSpan TotalTime = EndTime - StartTime;
                ViewBag.TotalTime = "hs: " + TotalTime.TotalHours + "mins: " + TotalTime.TotalMinutes + "secs: " + TotalTime.TotalSeconds;
                return View("ShowNetCDF");
            }

            if (apiRequest.type == "netCDFFtp3")
            {
                string workingDirectorypath = HttpContext.Server.MapPath("~/App_Data");

                ViewBag.request = apiRequest.url;
                ViewBag.response = apiCaller.get_big_NetCDF_by_ftp3(apiRequest.url, workingDirectorypath);
                TimeSpan TotalTime = EndTime - StartTime;
                ViewBag.TotalTime = "hs: " + TotalTime.TotalHours + "mins: " + TotalTime.TotalMinutes + "secs: " + TotalTime.TotalSeconds;
                return View("ShowNetCDF");
            }

            if (apiRequest.type == "netCDFFtp4")
            {
                string workingDirectorypath = HttpContext.Server.MapPath("~/App_Data");

                ViewBag.request = apiRequest.url;
                ViewBag.response = apiCaller.get_big_NetCDF_by_ftp4(apiRequest.url, workingDirectorypath);
                TimeSpan TotalTime = EndTime - StartTime;
                ViewBag.TotalTime = "hs: " + TotalTime.TotalHours + "mins: " + TotalTime.TotalMinutes + "secs: " + TotalTime.TotalSeconds;
                return View("ShowNetCDF");
            }

            if (apiRequest.type == "FTP-netCDF")
            {
                string workingDirectorypath = HttpContext.Server.MapPath("~/App_Data");

                ViewBag.request = apiRequest.url;
                ViewBag.response = apiCaller.get_big_NetCDF_by_ftp2(apiRequest.url, workingDirectorypath);

                EndTime = DateTime.Now;

                TimeSpan TotalTime = EndTime - StartTime;
                apiRequest.startGetTime = StartTime;
                apiRequest.endGetTime = EndTime;
                apiRequest.spanGetTime = TotalTime;
                apiRequest.onDisk = "File";
                db.Entry(apiRequest).State = System.Data.EntityState.Modified;
                db.SaveChanges();


                DataSetSchema schema = AnalyzeFileDiskAction(id);


                ViewBag.TotalTime = "hs: " + TotalTime.TotalHours + "mins: " + TotalTime.TotalMinutes + "secs: " + TotalTime.TotalSeconds;
                return View("ShowNetCDF");
            }


            return View("ShowError");

        }


        public DataSetSchema AnalyzeFileDiskAction(int id)
        {
            APIRequest apiRequest = db.APIRequests.Find(id);
            DataSetSchema schema = null;
            try
            {
                string url = apiRequest.url;
                string safeFileName = url.Replace('/', '_').Replace(':', '_').TrimStart().TrimEnd();

                string downloadedFilePath = HttpContext.Server.MapPath("~/App_Data/" + safeFileName);



                apiRequest.onDisk = "N/A";
                bool fileExists = System.IO.File.Exists(downloadedFilePath);

                if (System.IO.File.Exists(downloadedFilePath))
                {
                    string schemaString = apiCaller.AnalyzeFileDisk(downloadedFilePath);

                    long length = new System.IO.FileInfo(downloadedFilePath).Length;

                    var dataset = Microsoft.Research.Science.Data.DataSet.Open(downloadedFilePath);
                    schema = dataset.GetSchema();
                    if (schema.Variables.Length > 1)
                    {
                        apiRequest.onDisk = "OK";
                        apiRequest.fileSize = (int)length / 1000000;
                        apiRequest.schema = schemaString;
                    }
                }
            }
            catch (Exception e)
            {
                apiRequest.onDisk = "Error: " + e.Message;
            }

            db.Entry(apiRequest).State = System.Data.EntityState.Modified;
            db.SaveChanges();
            return schema;
        }
        public ActionResult AnalyzeFileDisk(int id = 0)
        {
            DataSetSchema schema = AnalyzeFileDiskAction(id);               
            return View(schema);
        }

        public ActionResult AnalyzeFileBlob(int id = 0)
        {
            APIRequest apiRequest = db.APIRequests.Find(id);
            string url = apiRequest.url;
            string safeFileName = url.Replace('/', '_').Replace(':', '_').TrimStart().TrimEnd();
            string downloadedFilePath = HttpContext.Server.MapPath("~/App_Data/" + safeFileName);
            string result = apiCaller.AnalyzeFileBlob(downloadedFilePath);
            ViewBag.Message = result;
            return View();
        }

        //
        // GET: /Request/Create

        public ActionResult Create()
        {
            ViewBag.APIRequestSetId = new SelectList(db.APIRequestSets, "Id", "name", CurrentRequestSetID);
            return View();
        }

        //
        // POST: /Request/Create

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create(APIRequest apirequest)
        {
            if (ModelState.IsValid)
            {
                db.APIRequests.Add(apirequest);
                db.SaveChanges();
                return RedirectToAction("Index");
            }

            ViewBag.APIRequestSetId = new SelectList(db.APIRequestSets, "Id", "name", apirequest.APIRequestSetId);
            return View(apirequest);
        }

        //
        // GET: /Request/Edit/5

        public ActionResult Edit(int id = 0)
        {
            APIRequest apirequest = db.APIRequests.Find(id);
            if (apirequest == null)
            {
                return HttpNotFound();
            }
            ViewBag.APIRequestSetId = new SelectList(db.APIRequestSets, "Id", "name", apirequest.APIRequestSetId);
            return View(apirequest);
        }

        //
        // POST: /Request/Edit/5

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit(APIRequest apirequest)
        {
            if (ModelState.IsValid)
            {
                db.Entry(apirequest).State = System.Data.EntityState.Modified;
                db.SaveChanges();
                return RedirectToAction("Index");
            }
            ViewBag.APIRequestSetId = new SelectList(db.APIRequestSets, "Id", "name", apirequest.APIRequestSetId);
            return View(apirequest);
        }

        //
        // GET: /Request/Delete/5

        public ActionResult Delete(int id = 0)
        {
            APIRequest apirequest = db.APIRequests.Find(id);
            if (apirequest == null)
            {
                return HttpNotFound();
            }
            return View(apirequest);
        }

        //
        // POST: /Request/Delete/5

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(int id)
        {
            APIRequest apirequest = db.APIRequests.Find(id);
            db.APIRequests.Remove(apirequest);
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