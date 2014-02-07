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
using Microsoft.Research.Science.Data.Azure;

namespace JassWeather.Controllers
{
    public class TestController : Controller
    {
        //
        // GET: /Test/

        public ActionResult Index()
        {
            return View();
        }

        public ActionResult TestGeneral()
        {

            string commandResponse = "n/a";
            string commandResponse1 = "n/a";

            try
            {

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

        public ActionResult TestMerge1()
        {

            try
            {


                string appDataFolder = HttpContext.Server.MapPath("~/App_Data");

                string timestamp = JassWeatherDataSourceAPI.fileTimeStamp();

                string inputFile1 = appDataFolder + "/one_dimensional_measure_sample_1.csv";
                string inputFile3 = appDataFolder + "/merged_file_3_" + timestamp + ".csv";

                System.IO.File.Copy(inputFile1, inputFile3);

                var dataset1 = DataSet.Open(inputFile1 + "?appendMetadata=true");
                var dataset3 = DataSet.Open(inputFile3 + "?appendMetadata=true");

                string inputFile2 = appDataFolder + "/one_dimensional_measure_sample_2.csv";
                var dataset2 = DataSet.Open(inputFile2 + "?appendMetadata=true");

                var schema2 = dataset2.GetSchema();
                var schema1 = dataset1.GetSchema();

                var x = dataset3.GetData<double[]>("X");
                var temp = dataset3.GetData<double[]>("Temp");
                var humidity = dataset2.GetData<double[]>("Humidity");

                var humidVarID = dataset3.Add<double[]>("Humid", dataset3.Dimensions[0].Name).ID;

                dataset3.PutData<double[]>(humidVarID, humidity);

                var dim = dataset3.Dimensions;

                var x3ID = dataset3["X"].ID;
                var temp3ID = dataset3["Temp"].ID;
                var humid3ID = dataset3["Humid"].ID;

                var x2ID = dataset2["X"].ID;
                var humid2ID = dataset2["Humidity"].ID;

                var x1ID = dataset1["X"].ID;
                var temp1ID = dataset1["Temp"].ID;

                dataset3.View();

                var schema3 = dataset3.GetSchema();

                ViewBag.sschema1 = JassWeatherDataSourceAPI.schema2string(schema1);
                ViewBag.sschema2 = JassWeatherDataSourceAPI.schema2string(schema2);
                ViewBag.sschema3 = JassWeatherDataSourceAPI.schema2string(schema3);

                ViewBag.x3 = dataset3.GetData<double[]>("X");
                ViewBag.temp3 = dataset3.GetData<double[]>("Temp");
                ViewBag.humid3 = dataset3.GetData<double[]>("Humid");

                ViewBag.Message = "Test executed correctly:";

            }
            catch (Exception e)
            {
                ViewBag.Message = e.Message; ;
            }

            return View();
        }

        public ActionResult TestMerge2()
        {

            try
            {
                //Let try to re-create the file...

                string appDataFolder = HttpContext.Server.MapPath("~/App_Data");
                string timestamp = JassWeatherDataSourceAPI.fileTimeStamp();

                //tas_WRFG_example_2014_2_3_11_10_31_322.nc

                string inputFile1 = appDataFolder + "/tas_WRFG_example_2014_2_3_11_10_31_322.nc";
                string inputFile3 = appDataFolder + "/new_WRFG__" + timestamp + ".nc";

                var dataset1 = DataSet.Open(inputFile1 + "?openMode=open");

                var dataset3 = DataSet.Open(inputFile3 + "?openMode=create");

                var schema1 = dataset1.GetSchema();
                var schema3 = dataset3.GetSchema();

                double[] yc = dataset1.GetData<double[]>("yc");
                double[] xc = dataset1.GetData<double[]>("xc");
                double[] time = dataset1.GetData<double[]>("time");
                Single[, ,] temperature = dataset1.GetData<Single[, ,]>("tas");

                dataset3.Add<double[]>("time", time, "time");
                dataset3.Add<double[]>("xc", xc, "xc");
                dataset3.Add<double[]>("yc", yc, "yc");
                dataset3.Add<Single[, ,]>("temperature", temperature, "time", "yc", "xc");

                //temperature tas (time,50)(yc,109)(xc,134)

                var dataset_new = DataSet.Open(inputFile3 + "?openMode=open");
                var schema_new = dataset_new.GetSchema();
                double[] yc_new = dataset_new.GetData<double[]>("yc");
                double[] xc_new = dataset_new.GetData<double[]>("xc");
                double[] time_new = dataset_new.GetData<double[]>("time");
                Single[, ,] temperature_new = dataset_new.GetData<Single[, ,]>("temperature");

                ViewBag.sschema_new = JassWeatherDataSourceAPI.schema2string(schema_new);
                ViewBag.sschema1 = JassWeatherDataSourceAPI.schema2string(schema1);
                ViewBag.sschema3 = JassWeatherDataSourceAPI.schema2string(schema3);

                ViewBag.Message = "Test executed correctly:";
            }
            catch (Exception e)
            {
                ViewBag.Message = e.Message; ;
            }

            return View();
        }

        public ActionResult TestMerge3()
        {

            long StartingMemory;
            DateTime StartingTime = DateTime.Now;
            long AfterOpenMemory;
            long AfterLoadMemory;
            DateTime EndingTime = DateTime.Now;
            TimeSpan TotalDelay;

            try
            {
                //Let try to re-create the file...
                GC.Collect();
                StartingMemory = GC.GetTotalMemory(true);

                string appDataFolder = HttpContext.Server.MapPath("~/App_Data");
                string timestamp = JassWeatherDataSourceAPI.fileTimeStamp();

                //tas_WRFG_example_2014_2_3_11_10_31_322.nc

                string inputFile1 = appDataFolder + "/ftp___ftp.cdc.noaa.gov_Datasets_NARR_pressure_air.201201.nc";
                string inputFile3 = appDataFolder + "/new_NARR_air__" + timestamp + ".nc";

                var dataset1 = DataSet.Open(inputFile1 + "?openMode=open");

                var dataset3 = DataSet.Open(inputFile3 + "?openMode=create");

                AfterOpenMemory = GC.GetTotalMemory(true);
                var schema1 = dataset1.GetSchema();
                var schema3 = dataset3.GetSchema();

                Single[] y = dataset1.GetData<Single[]>("y");
                Single[] x = dataset1.GetData<Single[]>("x");
                double[] time = dataset1.GetData<double[]>("time");
                Single[] level = dataset1.GetData<Single[]>("level");

                // Int16[,,,] temperature = dataset1.GetData<Int16[,,,]>("air");

                Int16[, ,] temperature = dataset1.GetData<Int16[, ,]>("air",
                     DataSet.FromToEnd(0), /* removing first dimension from data*/
                     DataSet.ReduceDim(0), /* removing first dimension from data*/
                     DataSet.FromToEnd(0),
                     DataSet.FromToEnd(0));

                AfterLoadMemory = GC.GetTotalMemory(true);

                dataset3.Add<double[]>("time", time, "time");
                dataset3.Add<Single[]>("level", level, "level");
                dataset3.Add<Single[]>("y", y, "y");
                dataset3.Add<Single[]>("x", x, "x");


                dataset3.Add<Int16[, ,]>("temperature", temperature, "time", "y", "x");

                //temperature tas (time,50)(yc,109)(xc,134)

                var dataset_new = DataSet.Open(inputFile3 + "?openMode=open");
                var schema_new = dataset_new.GetSchema();
                Single[] yc_new = dataset_new.GetData<Single[]>("y");
                Single[] xc_new = dataset_new.GetData<Single[]>("x");
                double[] time_new = dataset_new.GetData<double[]>("time");
                Int16[, ,] temperature_new = dataset_new.GetData<Int16[, ,]>("temperature");


                var size = sizeof(Int16);
                var c = size;

                EndingTime = DateTime.Now;
                TotalDelay = EndingTime - StartingTime;

                ViewBag.sschema_new = JassWeatherDataSourceAPI.schema2string(schema_new);
                ViewBag.sschema1 = JassWeatherDataSourceAPI.schema2string(schema1);
                ViewBag.sschema3 = JassWeatherDataSourceAPI.schema2string(schema3);

                ViewBag.StartingMemory = StartingMemory / 1000000;
                ViewBag.AfterOpenMemory = AfterOpenMemory / 1000000;
                ViewBag.AfterLoadMemory = AfterLoadMemory / 1000000;
                ViewBag.AfterLoadDiffMemory = AfterLoadMemory - AfterOpenMemory;

                EndingTime = DateTime.Now;
                TotalDelay = EndingTime - StartingTime;

                ViewBag.TotalDelay = TotalDelay;

                ViewBag.Message = "Test executed correctly:";
            }
            catch (Exception e)
            {
                ViewBag.Message = e.Message; ;
            }

            return View();
        }

        public ActionResult TestMerge4()
        {

            long StartingMemory;
            DateTime StartingTime = DateTime.Now;
            long AfterOpenMemory;
            long AfterLoadMemory;
            DateTime EndingTime = DateTime.Now;
            TimeSpan TotalDelay;

            try
            {
                //Let try to re-create the file...
                GC.Collect();
                StartingMemory = GC.GetTotalMemory(true);

                string appDataFolder = HttpContext.Server.MapPath("~/App_Data");
                string timestamp = JassWeatherDataSourceAPI.fileTimeStamp();

                //tas_WRFG_example_2014_2_3_11_10_31_322.nc

                string inputFile1 = appDataFolder + "/ftp___ftp.cdc.noaa.gov_Datasets_NARR_pressure_air.201201.nc";
                var dataset1 = DataSet.Open(inputFile1 + "?openMode=open");
                var schema1 = dataset1.GetSchema();
                Single[] y = dataset1.GetData<Single[]>("y");
                Single[] x = dataset1.GetData<Single[]>("x");
                double[] time = dataset1.GetData<double[]>("time");
                Single[] level = dataset1.GetData<Single[]>("level");

                for (var df = 0; df < time.Length; df += 8)
                {

                    string inputFile3 = appDataFolder + "/envirolitic_air_ " + timestamp + "__2012_01_" + df + ".nc";


                    var dataset3 = DataSet.Open(inputFile3 + "?openMode=create");

                    AfterOpenMemory = GC.GetTotalMemory(true);

                    var schema3 = dataset3.GetSchema();



                    // Int16[,,,] temperature = dataset1.GetData<Int16[,,,]>("air");

                    Int16[, ,] temperature = dataset1.GetData<Int16[, ,]>("air",
                         DataSet.Range(0, 1, 7), /* removing first dimension from data*/
                         DataSet.ReduceDim(0), /* removing first dimension from data*/
                         DataSet.FromToEnd(0),
                         DataSet.FromToEnd(0));

                    AfterLoadMemory = GC.GetTotalMemory(true);

                    double[] timeday = new double[8];

                    for (var t = 0; t < 8; t++)
                    {
                        timeday[t] = time[t];
                    }

                    dataset3.Add<double[]>("time", timeday, "time");
                    dataset3.Add<Single[]>("level", level, "level");
                    dataset3.Add<Single[]>("y", y, "y");
                    dataset3.Add<Single[]>("x", x, "x");


                    dataset3.Add<Int16[, ,]>("temperature", temperature, "time", "y", "x");

                    //temperature tas (time,50)(yc,109)(xc,134)

                    var dataset_new = DataSet.Open(inputFile3 + "?openMode=open");
                    var schema_new = dataset_new.GetSchema();
                    Single[] yc_new = dataset_new.GetData<Single[]>("y");
                    Single[] xc_new = dataset_new.GetData<Single[]>("x");
                    double[] time_new = dataset_new.GetData<double[]>("time");
                    Int16[, ,] temperature_new = dataset_new.GetData<Int16[, ,]>("temperature");


                    ViewBag.sschema_new = JassWeatherDataSourceAPI.schema2string(schema_new);
                    ViewBag.sschema1 = JassWeatherDataSourceAPI.schema2string(schema1);
                    ViewBag.sschema3 = JassWeatherDataSourceAPI.schema2string(schema3);
                    ViewBag.AfterOpenMemory = AfterOpenMemory / 1000000;
                    ViewBag.AfterLoadMemory = AfterLoadMemory / 1000000;
                    ViewBag.AfterLoadDiffMemory = AfterLoadMemory - AfterOpenMemory;

                }

                EndingTime = DateTime.Now;
                TotalDelay = EndingTime - StartingTime;

                ViewBag.StartingMemory = StartingMemory / 1000000;

                ViewBag.TotalDelay = TotalDelay;

                ViewBag.Message = "Test executed correctly:";
            }
            catch (Exception e)
            {
                ViewBag.Message = e.Message; ;
            }

            return View();
        }

        public ActionResult TestMerge5()
        {

            long StartingMemory;
            DateTime StartingTime = DateTime.Now;
            long AfterOpenMemory;
            long AfterLoadMemory;
            DateTime EndingTime = DateTime.Now;
            TimeSpan TotalDelay;

            try
            {
                //Let try to re-create the file...
                GC.Collect();
                StartingMemory = GC.GetTotalMemory(true);

                string appDataFolder = HttpContext.Server.MapPath("~/App_Data");
                string timestamp = JassWeatherDataSourceAPI.fileTimeStamp();

                //tas_WRFG_example_2014_2_3_11_10_31_322.nc

                string inputFile1 = appDataFolder + "/ftp___ftp.cdc.noaa.gov_Datasets_NARR_pressure_air.201201.nc";
                var dataset1 = DataSet.Open(inputFile1 + "?openMode=open");
                var schema1 = dataset1.GetSchema();
                Single[] y = dataset1.GetData<Single[]>("y");
                Single[] x = dataset1.GetData<Single[]>("x");
                double[] time = dataset1.GetData<double[]>("time");
                Single[] level = dataset1.GetData<Single[]>("level");
                string dayString;
                int in_year = 2012;
                int in_month = 1;
                int in_day;
                for (var df = 0; df < time.Length; df += 8)
                {
                    in_day = df / 8;

                    dayString = "" + in_year + "_" + in_month + "_" + in_day;

                    string inputFile3 = appDataFolder + "/envirolitic_air_2012_1_" + in_day + ".nc";


                    var dataset3 = DataSet.Open(inputFile3 + "?openMode=create");

                    AfterOpenMemory = GC.GetTotalMemory(true);

                    var schema3 = dataset3.GetSchema();



                    // Int16[,,,] temperature = dataset1.GetData<Int16[,,,]>("air");

                    Int16[, , ,] temperature = dataset1.GetData<Int16[, , ,]>("air",
                         DataSet.Range(0, 1, 7), /* removing first dimension from data*/
                         DataSet.FromToEnd(0), /* removing first dimension from data*/
                         DataSet.FromToEnd(0),
                         DataSet.FromToEnd(0));

                    short tempSample = (short)temperature[0, 0, 0, 0];

                    AfterLoadMemory = GC.GetTotalMemory(true);

                    double[] timeday = new double[8];

                    for (var t = 0; t < 8; t++)
                    {
                        timeday[t] = time[t];
                    }

                    dataset3.Add<double[]>("time", timeday, "time");
                    dataset3.Add<Single[]>("level", level, "level");
                    dataset3.Add<Single[]>("y", y, "y");
                    dataset3.Add<Single[]>("x", x, "x");


                    dataset3.Add<Int16[, , ,]>("temperature", temperature, "time", "level", "y", "x");

                    //temperature tas (time,50)(yc,109)(xc,134)

                    var dataset_new = DataSet.Open(inputFile3 + "?openMode=open");
                    var schema_new = dataset_new.GetSchema();
                    Single[] yc_new = dataset_new.GetData<Single[]>("y");
                    Single[] xc_new = dataset_new.GetData<Single[]>("x");
                    double[] time_new = dataset_new.GetData<double[]>("time");
                    Int16[, , ,] temperature_new = dataset_new.GetData<Int16[, , ,]>("temperature");


                    ViewBag.sschema_new = JassWeatherDataSourceAPI.schema2string(schema_new);
                    ViewBag.sschema1 = JassWeatherDataSourceAPI.schema2string(schema1);
                    ViewBag.sschema3 = JassWeatherDataSourceAPI.schema2string(schema3);
                    ViewBag.AfterOpenMemory = AfterOpenMemory / 1000000;
                    ViewBag.AfterLoadMemory = AfterLoadMemory / 1000000;
                    ViewBag.AfterLoadDiffMemory = AfterLoadMemory - AfterOpenMemory;
                    ViewBag.temperature_new = temperature_new;
                }

                EndingTime = DateTime.Now;
                TotalDelay = EndingTime - StartingTime;

                ViewBag.StartingMemory = StartingMemory / 1000000;

                ViewBag.TotalDelay = TotalDelay;

                ViewBag.Message = "Test executed correctly:";
            }
            catch (Exception e)
            {
                ViewBag.Message = e.Message; ;
            }

            return View();
        }


        public ActionResult TestFetchClimate()
        {

            // double t = ClimateService.FetchClimate(ClimateParameter.FC_TEMPERATURE,52.2, 52.2, 0.1, 0.1); 


            return View();
        }

        public ActionResult TestWriteDataSet2Azure()
        {
            string rootFolder = HttpContext.Server.MapPath("~/App_Data");
            string timestamp = JassWeatherDataSourceAPI.fileTimeStamp();

            try
            {
                //This is a simple example that reads and writes netCDF files using SDS
                //We know how to do this from a normal disk and we want to learn how to do this from azure

                //1. Read an original netCDF file from Disk                
                string input_netCDF_File = rootFolder + "/ftp___ftp.cdc.noaa.gov_Datasets_NARR_pressure_air.201201.nc";
                var inputDataSet = DataSet.Open(input_netCDF_File + "?openMode=open");

                //read the dimensions that we know they have

                Single[] y = inputDataSet.GetData<Single[]>("y");
                Single[] x = inputDataSet.GetData<Single[]>("x");
                Single[] level = inputDataSet.GetData<Single[]>("level");

                //Read a portion of data to make this smaller, collapsing the time dimension to the first row

                Int16[, ,] temperature = inputDataSet.GetData<Int16[, ,]>("air",
                DataSet.ReduceDim(0), DataSet.FromToEnd(0), DataSet.FromToEnd(0), DataSet.FromToEnd(0));


                //CREATING AND READING netCDF on Azure Blob Storage
                //I First do it on Disk to warm up and who what I want to achieve.

                //Create a new netCDF file on disk for now
                string output_netCDF_File = rootFolder + "/testing_azureStorage" + timestamp + ".nc";
                var outputDataSet = DataSet.Open(output_netCDF_File + "?openMode=create");
                //add the data
                outputDataSet.Add<Single[]>("level", level, "level");
                outputDataSet.Add<Single[]>("y", y, "y");
                outputDataSet.Add<Single[]>("x", x, "x");
                outputDataSet.Add<Int16[, ,]>("temperature", temperature, "level", "y", "x");

                // Read the created file to make sure if ok. 

                var outputDataSetNew = DataSet.Open(output_netCDF_File + "?openMode=open");
                var schema_new = outputDataSetNew.GetSchema();

                Single[] yc_new = outputDataSetNew.GetData<Single[]>("y");
                Single[] xc_new = outputDataSetNew.GetData<Single[]>("x");
                Single[] level_new = outputDataSetNew.GetData<Single[]>("level");
                Int16[, ,] temperature_new = outputDataSetNew.GetData<Int16[, ,]>("temperature");

                //Now, let try to do a similar things on azure blob

                //Create a new netCDF file on disk for now
                //
                //Credentials for azure blob storage account
                //<add key="StorageConnectionString" 
                //value="DefaultEndpointsProtocol=https;
                //AccountName=envirolytic;
                //AccountKey=WN55CUrwdaYDaMoG332M6x9ekXQu4P1uEoxEGsBBXGVr9mRsoKt4FXanp1M0QEgrIqke81Bp78FX7q2EMqqHSg==" />
                //
                //Database credentials
                //"Data Source=tcp:wbio6ii80z.database.windows.net,1433;
                //Initial Catalog=envirolytic;
                //User ID=envirolytic;
                //Password=iswhatyou2!" providerName="System.Data.SqlClient" />


                //First we need to define URI

                var azureDataSet_URI = new AzureDataSetUri();
                azureDataSet_URI.OpenMode = ResourceOpenMode.Create;
                azureDataSet_URI.AccountKey = "WN55CUrwdaYDaMoG332M6x9ekXQu4P1uEoxEGsBBXGVr9mRsoKt4FXanp1M0QEgrIqke81Bp78FX7q2EMqqHSg==";
                azureDataSet_URI.AccountName = "envirolytic";
                azureDataSet_URI.DataSetID = "123456789";
                azureDataSet_URI.DataSetName = "testing" + timestamp;
                azureDataSet_URI.DefaultEndpointsProtocol = EndpointProtocol.http;

                // The next statement will fail with the following exception
                //{"Error while getting dataSets Schemas:Failed to get configuration container from blob storage:The specified container does not exist."}
                var dataset_from_Azure = AzureDataSet.Open(azureDataSet_URI);


                ViewBag.sschema_new = JassWeatherDataSourceAPI.schema2string(schema_new);
                ViewBag.Message = "Test executed correctly:";
            }
            catch (Exception e)
            {
                ViewBag.Message = e.Message; ;
            }

            return View();
        }

    }
}
