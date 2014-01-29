using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Newtonsoft.Json;
using System.IO;
using JassWeather.Models;
using System.Diagnostics;
using System.Net;
using Microsoft.Research.Science.Data;
using Microsoft.Research.Science.Data.Imperative;

namespace JassWeather.Controllers
{
    public class HomeController : Controller
    {

        public ActionResult Index()
        {
            //Response.Redirect("ControllerName/ActionName")
            //return RedirectToAction("Index", "Request");

            Session["CurrentRequestSetId"] = 2;
            Session["CurrentRequestSetName"]="NCEP-NARR";
            return RedirectToAction("Index", "Request");
        }
        public ActionResult TestGeneral()       
        {
          
            string commandResponse = "n/a";
            string commandResponse1 = "n/a";

            try{

            JassWeatherDataSourceAPI apiCaller = new JassWeatherDataSourceAPI();
            string request1 = "http://api.wunderground.com/api/501a82781dc79a42/geolookup/conditions/q/IA/Cedar_Rapids.json";
            string response1 = apiCaller.ping_Json_DataSource(request1);

            ViewBag.request1 = request1;
            ViewBag.response1 = response1;

            DateTime startTime = DateTime.UtcNow;
            WebRequest request = WebRequest.Create("http://www.narccap.ucar.edu/data/example/tas_WRFG_example.nc");
            WebResponse response = request.GetResponse();


                string command = string.Format("/c del tas_WRFG_example*.*");
                ProcessStartInfo cmdsi = new ProcessStartInfo("cmd.exe");
                cmdsi.WorkingDirectory = HttpContext.Server.MapPath("~/App_Data");
                cmdsi.Arguments = command;
                cmdsi.RedirectStandardOutput = true;
                cmdsi.UseShellExecute = false;
                cmdsi.CreateNoWindow = false;
                Process cmd = Process.Start(cmdsi);
                cmd.WaitForExit();

               
DateTime t = DateTime.Now;
string timeStamp ="_" + t.Year + "_" + t.Month + "_" + t.Day + "_" + t.Hour + "_" + t.Minute + "_" + t.Second + "_" + t.Millisecond;
string downloadedFilePath = HttpContext.Server.MapPath("~/App_Data/tas_WRFG_example" + timeStamp + ".nc");
 
using (Stream responseStream = response.GetResponseStream())
{
    using (Stream fileStream = System.IO.File.OpenWrite(downloadedFilePath))
    {
        byte[] buffer = new byte[4096];
        int bytesRead = responseStream.Read(buffer, 0, 4096);
        while (bytesRead > 0)
        {
            fileStream.Write(buffer, 0, bytesRead);
            DateTime nowTime = DateTime.UtcNow;
            if ((nowTime - startTime).TotalMinutes > 5)
            {
                throw new ApplicationException(
                    "Download timed out");
            }
            bytesRead = responseStream.Read(buffer, 0, 4096);
        }
    }
}

             
string downloadedFileName ="tas_WRFG_example" + timeStamp + ".nc";

var dataset = DataSet.Open(downloadedFilePath);

ViewBag.yc = dataset.GetData<double[]>("yc");
ViewBag.xc = dataset.GetData<double[]>("xc");
ViewBag.time = dataset.GetData<double[]>("time");
var schema = dataset.GetSchema();

ViewBag.schema = schema;

ViewBag.level = dataset.GetData<double>("level");

// tas[,yc=43,xc=67]
ViewBag.tas = dataset.GetData<Single[, ,]>("tas");

string command2 = string.Format("/c dir");
ProcessStartInfo cmdsi2 = new ProcessStartInfo("cmd.exe");
cmdsi2.WorkingDirectory = HttpContext.Server.MapPath("~/App_Data");
cmdsi2.Arguments = command2;
cmdsi2.RedirectStandardOutput = true;
cmdsi2.UseShellExecute = false;
cmdsi2.CreateNoWindow = false;
Process cmd2 = Process.Start(cmdsi2);
cmd2.WaitForExit();

commandResponse = cmd2.StandardOutput.ReadToEnd() + commandResponse1;

            }
        catch (Exception e)
        {
            ViewBag.Message = e.Message; ;
        } 

            ViewBag.Message = "Test executed correctly:" + commandResponse;
            return View();
        }


        public ActionResult TestSDS()
        {

            string commandResponse = "n/a";
            string commandResponse1 = "n/a";

            try
            {

                DateTime startTime = DateTime.UtcNow;
                WebRequest request = WebRequest.Create("http://www.narccap.ucar.edu/data/example/tas_WRFG_example.nc");
                WebResponse response = request.GetResponse();


                string command = string.Format("/c del tas_WRFG_example*.*");
                ProcessStartInfo cmdsi = new ProcessStartInfo("cmd.exe");
                cmdsi.WorkingDirectory = HttpContext.Server.MapPath("~/App_Data");
                cmdsi.Arguments = command;
                cmdsi.RedirectStandardOutput = true;
                cmdsi.UseShellExecute = false;
                cmdsi.CreateNoWindow = false;
                Process cmd = Process.Start(cmdsi);
                cmd.WaitForExit();


                DateTime t = DateTime.Now;
                string timeStamp = "_" + t.Year + "_" + t.Month + "_" + t.Day + "_" + t.Hour + "_" + t.Minute + "_" + t.Second + "_" + t.Millisecond;
                string downloadedFilePath = HttpContext.Server.MapPath("~/App_Data/tas_WRFG_example" + timeStamp + ".nc");

                using (Stream responseStream = response.GetResponseStream())
                {
                    using (Stream fileStream = System.IO.File.OpenWrite(downloadedFilePath))
                    {
                        byte[] buffer = new byte[4096];
                        int bytesRead = responseStream.Read(buffer, 0, 4096);
                        while (bytesRead > 0)
                        {
                            fileStream.Write(buffer, 0, bytesRead);
                            DateTime nowTime = DateTime.UtcNow;
                            if ((nowTime - startTime).TotalMinutes > 5)
                            {
                                throw new ApplicationException(
                                    "Download timed out");
                            }
                            bytesRead = responseStream.Read(buffer, 0, 4096);
                        }
                    }
                }


                string downloadedFileName = "tas_WRFG_example" + timeStamp + ".nc";

                var dataset = DataSet.Open(downloadedFilePath);

                ViewBag.yc = dataset.GetData<double[]>("yc");
                ViewBag.xc = dataset.GetData<double[]>("xc");
                ViewBag.time = dataset.GetData<double[]>("time");
                var schema = dataset.GetSchema();

                ViewBag.schema = schema;

                ViewBag.level = dataset.GetData<double>("level");

                // tas[,yc=43,xc=67]
                ViewBag.tas = dataset.GetData<Single[, ,]>("tas");

                var tas = dataset.GetData<Single[, ,]>("tas");

                var prec0 = dataset.GetData<Single[,]>("tas",
DataSet.ReduceDim(0), /* removing first dimension from data*/
DataSet.FromToEnd(0),
DataSet.FromToEnd(0));

                ViewBag.prec0 = prec0;

                var prec1 = dataset.GetData<Single[,]>("tas",
DataSet.ReduceDim(1), /* removing first dimension from data*/
DataSet.FromToEnd(0),
DataSet.FromToEnd(0));

                ViewBag.prec1 = prec1;


                string command2 = string.Format("/c dir");
                ProcessStartInfo cmdsi2 = new ProcessStartInfo("cmd.exe");
                cmdsi2.WorkingDirectory = HttpContext.Server.MapPath("~/App_Data");
                cmdsi2.Arguments = command2;
                cmdsi2.RedirectStandardOutput = true;
                cmdsi2.UseShellExecute = false;
                cmdsi2.CreateNoWindow = false;
                Process cmd2 = Process.Start(cmdsi2);
                cmd2.WaitForExit();

                commandResponse = cmd2.StandardOutput.ReadToEnd() + commandResponse1;

            }
            catch (Exception e)
            {
                ViewBag.Message = e.Message; ;
            }

            ViewBag.Message = "Test executed correctly:" + commandResponse;
            return View();
        }

        public ActionResult About()
        {
            ViewBag.Message = "Your app description page.";

            return View();
        }

        public ActionResult Contact()
        {
            ViewBag.Message = "Your contact page.";

            return View();
        }
    }
}
