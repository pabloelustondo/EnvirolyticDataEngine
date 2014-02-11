using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.FtpClient;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Auth;
using Microsoft.WindowsAzure.Storage.Blob;
using System.Configuration;
using Microsoft.WindowsAzure.Storage.Table;
using Microsoft.Research.Science.Data;
using Microsoft.Research.Science.Data.Imperative;
using Microsoft.WindowsAzure;

namespace JassWeather.Models
{
    public class JassBlob {
        public string url { get; set; }
        public int length { get; set; }   
    }

    public class JassWeatherAPI
    {
        public JassBuilder builder;
        private JassWeatherContext db = new JassWeatherContext();
        public string AppDataFolder;
        DateTime startTotalTime = DateTime.UtcNow;
        DateTime endTotalTime = DateTime.UtcNow;
        TimeSpan spanTotalTime;

        public JassWeatherAPI(string appDataFolder){

            AppDataFolder = appDataFolder;
          }


        public string processBuilder(JassBuilder builder, Boolean upload)
        {

            string Message = "process builder sucessfuly";
            long StartingMemory;
            DateTime StartingTime = DateTime.Now;
            long AfterOpenMemory;
            long AfterLoadMemory;
            DateTime EndingTime = DateTime.Now;
            TimeSpan TotalDelay;
            JassBuilder jassbuilder = db.JassBuilders.Find(builder.JassBuilderID);
            try
            {
                //Let try to re-create the file...
                GC.Collect();
                StartingMemory = GC.GetTotalMemory(true);
                startTotalTime = DateTime.Now;

                string timestamp = JassWeatherAPI.fileTimeStamp();

                string url = builder.APIRequest.url;
                string inputFile1 = AppDataFolder + "/" + safeFileNameFromUrl(url);
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


                jassbuilder.Status = JassBuilderStatus.Processing;
                jassbuilder.setTotalSize = time.Length / 8;
                jassbuilder.setCurrentSize = 0;
                db.Entry<JassBuilder>(jassbuilder).State = System.Data.EntityState.Modified;
                db.SaveChanges();

                for (var df = 0; df < time.Length; df += 8)
                {
                    in_day = df / 8;

                    dayString = "" + in_year + "_" + in_month + "_" + in_day;

                    string outputFileName = fileNameBuilderByDay(builder,in_day) + ".nc";
                    string outputFilePath = AppDataFolder + "/" + outputFileName;
                    var dataset3 = DataSet.Open(outputFilePath + "?openMode=create");
                    AfterOpenMemory = GC.GetTotalMemory(true);
                    var schema3 = dataset3.GetSchema();

                    Int16[, , ,] dataset = dataset1.GetData<Int16[, , ,]>(builder.Source1VariableName,
                         DataSet.Range(0, 1, 7), /* removing first dimension from data*/
                         DataSet.FromToEnd(0), /* removing first dimension from data*/
                         DataSet.FromToEnd(0),
                         DataSet.FromToEnd(0));

                    short tempSample = (short)dataset[0, 0, 0, 0];

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


                    dataset3.Add<Int16[, , ,]>("temperature", dataset, "time", "level", "y", "x");
                    dataset3.Commit();
                    dataset3.Dispose();

                    if (upload)
                    {
                        uploadBlob(builder.JassVariable.Name, outputFileName, outputFilePath);
                    }

                    jassbuilder.setCurrentSize = df/8 + 1;
                    db.Entry<JassBuilder>(jassbuilder).State = System.Data.EntityState.Modified;
                    db.SaveChanges();

 
                }

                DateTime EndTime = DateTime.Now;

                TimeSpan TotalTime = EndTime - startTotalTime;
                jassbuilder.startTotalTime = startTotalTime;
                jassbuilder.endTotalTime = EndTime;
                jassbuilder.OnDisk = true;
                jassbuilder.spanTotalTime = TotalTime;
                jassbuilder.Status = JassBuilderStatus.Success;


                db.Entry<JassBuilder>(jassbuilder).State = System.Data.EntityState.Modified;
                db.SaveChanges();

            }

            catch (Exception e)
            {
                Message = e.Message;
                DateTime EndTime = DateTime.Now;
                TimeSpan TotalTime = EndTime - startTotalTime;
                jassbuilder.startTotalTime = startTotalTime;
                jassbuilder.endTotalTime = EndTime;
                jassbuilder.OnDisk = false;
                jassbuilder.spanTotalTime = TotalTime;
                jassbuilder.Status = JassBuilderStatus.Failure;
                jassbuilder.Message = e.Message;
            }


            return Message; 
        }

        public string checkBuilderOnDisk(JassBuilder builder)
        {
            JassBuilder jassbuilder = db.JassBuilders.Find(builder.JassBuilderID);
            try
            {
                string timestamp = JassWeatherAPI.fileTimeStamp();
                string url = builder.APIRequest.url;
                string inputFile1 = AppDataFolder + "/" + safeFileNameFromUrl(url);
                string dayString;
                int in_year = 2012;
                int in_month = 1;
                int in_day;
                int currentNumberOfFiles = 0;

                for (var df = 0; df < 24 /*time.Length*/; df += 8)
                {
                    in_day = df / 8;
                    dayString = "" + in_year + "_" + in_month + "_" + in_day;
                    string outputFile = AppDataFolder + "/" + fileNameBuilderByDay(builder, in_day) + ".nc";

                    Boolean fileExists = File.Exists(outputFile);

                    if (fileExists)
                    {
                        currentNumberOfFiles += 1;
                    }
                }


                if (currentNumberOfFiles > 27)
                {
                    jassbuilder.OnDisk = true;
                    jassbuilder.Status = JassBuilderStatus.Success;
                    jassbuilder.Message = "number of files: " + currentNumberOfFiles;
                    db.Entry<JassBuilder>(jassbuilder).State = System.Data.EntityState.Modified;
                    db.SaveChanges();
                }
                else
                {
                    jassbuilder.OnDisk = false;
                    jassbuilder.Status = JassBuilderStatus.Failure;
                    jassbuilder.Message = "number of files: " + currentNumberOfFiles;
                    db.Entry<JassBuilder>(jassbuilder).State = System.Data.EntityState.Modified;
                    db.SaveChanges();
                }

            }
            catch (Exception e)
            {
                jassbuilder.OnDisk = false;
                jassbuilder.Status = JassBuilderStatus.Failure;
                jassbuilder.Message = e.Message;
                db.Entry<JassBuilder>(jassbuilder).State = System.Data.EntityState.Modified;
                db.SaveChanges();
            }

            string MessageString =     "Status: " + jassbuilder.Status +
                                "#Files: " + jassbuilder.setCurrentSize + 
                                "OnDisk: " + jassbuilder.OnDisk +
                                "  " + jassbuilder.Message;

            return MessageString;
        }
     
        
        public string cleanBuilderOnDisk(JassBuilder builder)
        {
            JassBuilder jassbuilder = db.JassBuilders.Find(builder.JassBuilderID);
            try
            {
                string timestamp = JassWeatherAPI.fileTimeStamp();
                string url = builder.APIRequest.url;
                string inputFile1 = AppDataFolder + "/" + safeFileNameFromUrl(url);
                string dayString;
                int in_year = 2012;
                int in_month = 1;
                int in_day;
                int currentNumberOfFiles = 0;

                for (var df = 0; df < 24 /*time.Length*/; df += 8)
                {
                    in_day = df / 8;
                    dayString = "" + in_year + "_" + in_month + "_" + in_day;
                    string outputFile = AppDataFolder + "/" + fileNameBuilderByDay(builder, in_day) + ".nc";

                    File.Delete(outputFile);
                }

                    jassbuilder.OnDisk = true;
                    jassbuilder.Status = JassBuilderStatus.Success;
                    jassbuilder.Message = "number of files: " + currentNumberOfFiles;
                    db.Entry<JassBuilder>(jassbuilder).State = System.Data.EntityState.Modified;
                    db.SaveChanges();
 
            }
            catch (Exception e)
            {
                jassbuilder.OnDisk = false;
                jassbuilder.Status = JassBuilderStatus.Failure;
                jassbuilder.Message = e.Message;
                db.Entry<JassBuilder>(jassbuilder).State = System.Data.EntityState.Modified;
                db.SaveChanges();
            }

            string MessageString =     "Status: " + jassbuilder.Status + 
                                "  " + jassbuilder.Message;

            return MessageString;
        }
        public string ping_Json_DataSource(string url){

            WebRequest req = WebRequest.Create(url);
            req.Method = "GET";
            HttpWebResponse resp = req.GetResponse() as HttpWebResponse;
            if (resp.StatusCode == HttpStatusCode.OK)
            {
                using (Stream respStream = resp.GetResponseStream())
                {
                    StreamReader reader = new StreamReader(respStream, Encoding.UTF8);
                    return reader.ReadToEnd();
                }
            }
            else
            {
                return string.Format("Status Code: {0}, Status Description: {1}", resp.StatusCode, resp.StatusDescription);
            }

        }

        public string ping_small_NetCDF_file(string url, string workingDirectoryPath)
        {
            //workingDirectorypath: HttpContext.Server.MapPath("~/App_Data")

            string commandResponse = "n/a";
            string commandResponse1 = "n/a";
            string Message = "";

            try
            {

                DateTime startTime = DateTime.UtcNow;
                WebRequest request = WebRequest.Create(url);
                WebResponse response = request.GetResponse();


                string command = string.Format("/c del tas_WRFG_example*.*");
                ProcessStartInfo cmdsi = new ProcessStartInfo("cmd.exe");
                cmdsi.WorkingDirectory = workingDirectoryPath;
                cmdsi.Arguments = command;
                cmdsi.RedirectStandardOutput = true;
                cmdsi.UseShellExecute = false;
                cmdsi.CreateNoWindow = false;
                Process cmd = Process.Start(cmdsi);
                cmd.WaitForExit();


                DateTime t = DateTime.Now;
                string timeStamp = "_" + t.Year + "_" + t.Month + "_" + t.Day + "_" + t.Hour + "_" + t.Minute + "_" + t.Second + "_" + t.Millisecond;
                string safeFileName = url.Replace('/', '_').Replace(':', '_').TrimStart().TrimEnd();
                string downloadedFilePath = workingDirectoryPath + "\\" + safeFileName;

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

                var schema = dataset.GetSchema();

                
                for(int i=1;i<9;i++){
                    Message = Message + "  " + schema.Variables[i].Name + "   ";
                }

                Message = "OK net CDF downloaded Variables in Schema: " + Message;

            }
            catch (Exception e)
            {
                Message = e.Message; ;
            }
            return Message;
        }

        public string get_big_NetCDF_by_ftp(string url, string workingDirectoryPath)
        {
            //workingDirectorypath: HttpContext.Server.MapPath("~/App_Data")

            string Message = "";

            try
            {

                DateTime startTime = DateTime.UtcNow;
                WebClient request = new WebClient();


                DateTime t = DateTime.Now;
                string timeStamp = "_" + t.Year + "_" + t.Month + "_" + t.Day + "_" + t.Hour + "_" + t.Minute + "_" + t.Second + "_" + t.Millisecond;
                string safeFileName = url.Replace('/', '_').Replace(':', '_').TrimStart().TrimEnd();
                string downloadedFilePath = workingDirectoryPath + "\\" + safeFileName;

                using (Stream responseStream = FtpClient.OpenRead(new Uri(url)))
                    {
                          using (Stream fileStream = System.IO.File.OpenWrite(downloadedFilePath))
                            {
                                byte[] buffer = new byte[8192];
                                int bytesRead = responseStream.Read(buffer, 0, 4096);
                                while (bytesRead > 0)
                                {
                                    fileStream.Write(buffer, 0, bytesRead);
                                    bytesRead = responseStream.Read(buffer, 0, 4096);
                                }
                            }
                    }

                Message = "OK net CDF downloaded Variables in Schema: " + Message;

            }
            catch (Exception e)
            {
                Message = e.Message; ;
            }
            return Message;
        }


        public string get_big_NetCDF_by_ftp2(string url, string workingDirectoryPath)
        {
            //workingDirectorypath: HttpContext.Server.MapPath("~/App_Data")

            string Message = "";

            try
            {

                DateTime startTime = DateTime.UtcNow;

                DateTime t = DateTime.Now;
                string timeStamp = "_" + t.Year + "_" + t.Month + "_" + t.Day + "_" + t.Hour + "_" + t.Minute + "_" + t.Second + "_" + t.Millisecond;
                string safeFileName = url.Replace('/', '_').Replace(':', '_').TrimStart().TrimEnd();
                string downloadedFilePath = workingDirectoryPath + "\\" + safeFileName;

                 FtpWebRequest request = (FtpWebRequest)FtpWebRequest.Create(url);
        request.Method = WebRequestMethods.Ftp.DownloadFile;
        request.Credentials = new NetworkCredential("anonymous", "pasword");
        FtpWebResponse response = (FtpWebResponse)request.GetResponse();
        Stream responseStream = response.GetResponseStream();
        FileStream file = File.Create(downloadedFilePath);
        byte[] buffer = new byte[32 * 1024];
        int read;
        //reader.Read(

        while ((read = responseStream.Read(buffer, 0, buffer.Length)) > 0)
        {
            file.Write(buffer, 0, read);
        }

        file.Close();
        responseStream.Close();
        response.Close();


                Message = "OK net CDF downloaded Variables in Schema: " + Message;

            }
            catch (Exception e)
            {
                Message = e.Message; ;
            }
            return Message;
        }


        public string get_big_NetCDF_by_ftp3(string url, string workingDirectoryPath)
        {
            //workingDirectorypath: HttpContext.Server.MapPath("~/App_Data")

            string Message = "";

            string connectionString = ConfigurationManager.AppSettings["StorageConnectionString"];
            CloudStorageAccount storageAccount = CloudStorageAccount.Parse(connectionString);

            CloudBlobClient blobClient = storageAccount.CreateCloudBlobClient();
            // Retrieve a reference to a container. 
            CloudBlobContainer container = blobClient.GetContainerReference("envirolytic");

   



            try
            {

                DateTime startTime = DateTime.UtcNow;

                WebRequest request = WebRequest.Create(url);
                WebResponse response = request.GetResponse();

                DateTime t = DateTime.Now;
                string timeStamp = "_" + t.Year + "_" + t.Month + "_" + t.Day + "_" + t.Hour + "_" + t.Minute + "_" + t.Second + "_" + t.Millisecond;
                string safeFileName = url.Replace('/', '_').Replace(':', '_').TrimStart().TrimEnd();
                string downloadedFilePath = workingDirectoryPath + "\\" + safeFileName;

                CloudBlockBlob blockBlob = container.GetBlockBlobReference(safeFileName);

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

                response.Close();

                using (var fileStream = System.IO.File.OpenRead(downloadedFilePath))
                {
                    blockBlob.UploadFromStream(fileStream);
                }

                Message = "OK net CDF downloaded Variables in Schema: " + Message;

            }
            catch (Exception e)
            {
                Message = e.Message; ;
            }
            return Message;
        }

        public string get_big_NetCDF_by_ftp4(string url, string workingDirectoryPath)
        {
            //workingDirectorypath: HttpContext.Server.MapPath("~/App_Data")

            string Message = "";

            string connectionString = ConfigurationManager.AppSettings["StorageConnectionString"];
            CloudStorageAccount storageAccount = CloudStorageAccount.Parse(connectionString);

            CloudBlobClient blobClient = storageAccount.CreateCloudBlobClient();
            // Retrieve a reference to a container. 
            CloudBlobContainer container = blobClient.GetContainerReference("envirolytic");

            try
            {

                DateTime startTime = DateTime.UtcNow;

                WebRequest request = WebRequest.Create(url);
                WebResponse response = request.GetResponse();

                DateTime t = DateTime.Now;
                string timeStamp = "_" + t.Year + "_" + t.Month + "_" + t.Day + "_" + t.Hour + "_" + t.Minute + "_" + t.Second + "_" + t.Millisecond;
                string safeFileName = url.Replace('/', '_').Replace(':', '_').TrimStart().TrimEnd();
                string downloadedFilePath = workingDirectoryPath + "\\" + safeFileName;

                CloudBlockBlob blockBlob = container.GetBlockBlobReference(safeFileName);


                using (Stream responseStream = response.GetResponseStream())
                {
                    using (CloudBlobStream fileStream = blockBlob.OpenWrite())
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

                response.Close();

                Message = "OK net CDF downloaded Variables in Schema: " + Message;

            }
            catch (Exception e)
            {
                Message = e.Message; ;
            }
            return Message;
        }

        public string safeFileNameFromUrl(string url)
        {
            return url.Replace('/', '_').Replace(':', '_').TrimStart().TrimEnd();
        }

        public string fileNameBuilderByDay(JassBuilder builder, int day){

            var dayString = (day+1).ToString();
            if (dayString.Length < 2) dayString = "0" + dayString;

            var monthString = builder.month.ToString();
            if (monthString.Length < 2) monthString = "0" + monthString;

            return builder.JassVariable.Name + "_" + builder.year + "_" + monthString + "_" + dayString;
        }

        public string get_big_NetCDF_by_ftp5(string url, string workingDirectoryPath, double maxFileSizeToDownload)
        {
            //workingDirectorypath: HttpContext.Server.MapPath("~/App_Data")

            string Message = "";

            try
            {

                DateTime startTime = DateTime.UtcNow;

                DateTime t = DateTime.Now;
                string timeStamp = "_" + t.Year + "_" + t.Month + "_" + t.Day + "_" + t.Hour + "_" + t.Minute + "_" + t.Second + "_" + t.Millisecond;
                string safeFileName = safeFileNameFromUrl(url);
                string downloadedFilePath = workingDirectoryPath + "\\" + safeFileName;


                //workingDirectorypath: HttpContext.Server.MapPath("~/App_Data")

                string connectionString = ConfigurationManager.AppSettings["StorageConnectionString"];
                CloudStorageAccount storageAccount = CloudStorageAccount.Parse(connectionString);

                CloudBlobClient blobClient = storageAccount.CreateCloudBlobClient();
                // Retrieve a reference to a container. 
                CloudBlobContainer container = blobClient.GetContainerReference("envirolytic");

                                CloudBlockBlob blockBlob = container.GetBlockBlobReference(safeFileName);

                                CloudBlobStream fileStream = blockBlob.OpenWrite();

                FtpWebRequest request = (FtpWebRequest)FtpWebRequest.Create(url);
                request.Method = WebRequestMethods.Ftp.DownloadFile;
                request.Credentials = new NetworkCredential("anonymous", "pasword");
                FtpWebResponse response = (FtpWebResponse)request.GetResponse();
                Stream responseStream = response.GetResponseStream();
          //      FileStream file = File.Create(downloadedFilePath);
                byte[] buffer = new byte[32 * 1024];
                int read;
                //reader.Read(
                double count = 0;
                while ((read = responseStream.Read(buffer, 0, buffer.Length)) > 0 && count < maxFileSizeToDownload)
                {
                    fileStream.Write(buffer, 0, read);
                    count += buffer.Length;
                }

                fileStream.Close();
                responseStream.Close();
                response.Close();


                Message = "OK net CDF downloaded - file size in MB: " + (count/1000000).ToString();

            }
            catch (Exception e)
            {
                Message = e.Message; ;
            }
            return Message;
        }

        public string AnalyzeFileBlob(string blogUri)
        {
           try{

               string connectionString = ConfigurationManager.AppSettings["StorageConnectionString"];
               CloudStorageAccount storageAccount = CloudStorageAccount.Parse(connectionString);

               CloudBlobClient blobClient = storageAccount.CreateCloudBlobClient();
               // Retrieve a reference to a container. 
               CloudBlobContainer container = blobClient.GetContainerReference("envirolytic");

               CloudBlockBlob blockBlob = container.GetBlockBlobReference(blogUri);

               string uri = blockBlob.Uri.ToString();
               string uri2 = "msds:nc?file=http://envirolytic.blob.core.windows.net/envirolytic/ftp___ftp.cdc.noaa.gov_Projects_Datasets_ncep.reanalysis_pressure_air.2013.nc";
               var dataset = DataSet.Open(uri2);

            /*
                ViewBag.yc = dataset.GetData<double[]>("yc");
                ViewBag.xc = dataset.GetData<double[]>("xc");
                ViewBag.time = dataset.GetData<double[]>("time");
                var schema = dataset.GetSchema();

                ViewBag.schema = schema;

                ViewBag.level = dataset.GetData<double>("level");

                // tas[,yc=43,xc=67]
                ViewBag.tas = dataset.GetData<Single[, ,]>("tas");
             */ 

            }
            catch (Exception e)
            {
              return "Error: " + e.Message;  
            }
           return "Hi";
        }

        public string AnalyzeFileDisk(string downloadedFilePath)
        {
            string schemaString = "";
            string dimensionsString = "";
            try
            {

               var dataset = DataSet.Open(downloadedFilePath);

               var schema = dataset.GetSchema();

               foreach (var v in schema.Variables)
               {
                   if (v.Name != "" && v.Dimensions.Count > 1)
                   {
                       schemaString += v.Name;
                       dimensionsString = "  ";
                       foreach (var d in v.Dimensions)
                       {
                           dimensionsString += "(" + d.Name + "," + d.Length + ")";
                       }
                       schemaString += dimensionsString;
                   }
               }

            }
            catch (Exception e)
            {
                return "Error: " + e.Message;
            }
            return schemaString;
        }

        public string store2table_0(string downloadedFilePath, int max)
        {
            string schemaString = "";
            string dimensionsString = "";

            long StartTotalMemory = GC.GetTotalMemory(true);
            DateTime StartTime = DateTime.Now;
            TimeSpan Total = StartTime - StartTime;

            DateTime ReadSDSFactsStart;
            TimeSpan ReadSDSFactsSpan;
            long ReadSDSFactsTotalMemory;

            DateTime WriteFactStart = DateTime.Now;
            TimeSpan WriteFactSpan = DateTime.Now - DateTime.Now;
            long WriteFactTotalMemory = 0;
            long WriteFactMaxMemory = 0;

            CloudTable table;
            try
            {
                #region Accessing the Table Storage

                string connectionString = ConfigurationManager.AppSettings["StorageConnectionString"];
                CloudStorageAccount storageAccount = CloudStorageAccount.Parse(connectionString);
                CloudTableClient tableClient = storageAccount.CreateCloudTableClient();

                //Testing... just creating a table for this specific file

                DateTime ahora = DateTime.Now;
                string timestamp = ahora.Hour.ToString() + ahora.Minute.ToString() + ahora.Second.ToString();
                table = tableClient.GetTableReference("et" + timestamp);
                table.CreateIfNotExists();

                #endregion


                #region Opening the DataSet and BS


                var dataset = DataSet.Open(downloadedFilePath);

                var schema = dataset.GetSchema();

                foreach (var v in schema.Variables)
                {
                    if (v.Name != "" && v.Dimensions.Count > 1)
                    {
                        schemaString += v.Name;
                        dimensionsString = "  ";
                        foreach (var d in v.Dimensions)
                        {
                            dimensionsString += "(" + d.Name + "," + d.Length + ")";
                        }
                        schemaString += dimensionsString;
                    }
                }

                Single[] yDim = dataset.GetData<Single[]>("y");
                Single[] xDim = dataset.GetData<Single[]>("x");
                double[] timeDim = dataset.GetData<double[]>("time");
                Single[] levelDim = dataset.GetData<Single[]>("level");

                ReadSDSFactsStart = DateTime.Now;

                var facts = dataset.GetData<Int16[, ,]>("air",
                    DataSet.FromToEnd(0), /* removing first dimension from data*/
                    DataSet.ReduceDim(0), /* removing first dimension from data*/
                    DataSet.FromToEnd(0),
                    DataSet.FromToEnd(0));

                ReadSDSFactsSpan = DateTime.Now - ReadSDSFactsStart;
                ReadSDSFactsTotalMemory = GC.GetTotalMemory(false);



                int xMax = (xDim.Length < max) ? xDim.Length : (int)max;
                int yMax = (yDim.Length < max) ? yDim.Length : (int)max;
                int tMax = (timeDim.Length < max) ? timeDim.Length : (int)max;
                TableOperation insertOperation;

                EnviromentalFact fact;

                for (int x = 0; x < xMax; x++)
                {
                    for (int y = 0; y < yMax; y++)
                    {
                        for (int t = 0; t < tMax; t++)
                        {
                            fact = new EnviromentalFact();
                            fact.x = (int)xDim[x];
                            fact.y = (int)yDim[y];
                            fact.time = timeDim[t];
                            fact.level = (int)levelDim[1];
                            fact.air = facts[t, y, x];
                            fact.PartitionKey = fact.x.ToString() + "-" + fact.y.ToString() + "-" + fact.level;
                            fact.RowKey = fact.time.ToString();


                            try
                            {

                                WriteFactStart = DateTime.Now;

                                insertOperation = TableOperation.Insert(fact);
                                // Execute the insert operation.
                                table.Execute(insertOperation);

                                WriteFactSpan = WriteFactSpan + (DateTime.Now - WriteFactStart);
                                WriteFactTotalMemory = GC.GetTotalMemory(false);
                                if (WriteFactMaxMemory < WriteFactTotalMemory) WriteFactMaxMemory = WriteFactTotalMemory;

                            }
                            catch (Exception e)
                            {
                                var kk = 1;
                            }
                        }
                    }
                }

                #endregion






            }
            catch (Exception e)
            {
                return "Error: " + e.Message;
            }

            // Construct the query operation for all customer entities where PartitionKey="Smith".
            TableQuery<EnviromentalFact> query = new TableQuery<EnviromentalFact>().Where(TableQuery.GenerateFilterCondition("PartitionKey", QueryComparisons.NotEqual, ""));

            TableQuery<EnviromentalFact> numberOfRowsQuery = new TableQuery<EnviromentalFact>();
            var numberOfRows = table.ExecuteQuery(numberOfRowsQuery).Count();


            Total = DateTime.Now - StartTime;

            string performance = " Number of Rows in Table: " + numberOfRows +
                " Total Span: " + Total +
                " Time and memory Reading SDS: " +
            ReadSDSFactsSpan + " - " + ReadSDSFactsTotalMemory +

            " Time and max memory Writting to Tables: " +
            WriteFactSpan + " - " + WriteFactMaxMemory;

            return schemaString + "    performance: " + performance;
        }

        public string store2table(string downloadedFilePath, int max)
        {
            string schemaString = "";
            string dimensionsString = "";

            long StartTotalMemory = GC.GetTotalMemory(true);
            DateTime StartTime = DateTime.Now;
            TimeSpan Total = StartTime- StartTime;

            DateTime ReadSDSFactsStart;
            TimeSpan ReadSDSFactsSpan;
            long ReadSDSFactsTotalMemory;

            DateTime WriteFactStart = DateTime.Now;
            TimeSpan WriteFactSpan = DateTime.Now - DateTime.Now; 
            long WriteFactTotalMemory = 0;
            long WriteFactMaxMemory = 0;

            CloudTable table;
            try
            {
                #region Accessing the Table Storage

                string connectionString = ConfigurationManager.AppSettings["StorageConnectionString"];
                CloudStorageAccount storageAccount = CloudStorageAccount.Parse(connectionString);
                CloudTableClient tableClient = storageAccount.CreateCloudTableClient();

                //Testing... just creating a table for this specific file

                DateTime ahora = DateTime.Now;
                string timestamp = ahora.Hour.ToString() + ahora.Minute.ToString() +  ahora.Second.ToString();
                table = tableClient.GetTableReference("et" + timestamp);
                table.CreateIfNotExists();

                #endregion


            #region Opening the DataSet and BS

               
                var dataset = DataSet.Open(downloadedFilePath);

                var schema = dataset.GetSchema();

                foreach (var v in schema.Variables)
                {
                    if (v.Name != "" && v.Dimensions.Count > 1)
                    {
                        schemaString += v.Name;
                        dimensionsString = "  ";
                        foreach (var d in v.Dimensions)
                        {
                            dimensionsString += "(" + d.Name + "," + d.Length + ")";
                        }
                        schemaString += dimensionsString;
                    }
                }

                Single[] yDim = dataset.GetData<Single[]>("y");
                Single[] xDim = dataset.GetData<Single[]>("x");
                double[] timeDim = dataset.GetData<double[]>("time");
                Single[] levelDim = dataset.GetData<Single[]>("level");

    ReadSDSFactsStart = DateTime.Now;

                var facts = dataset.GetData<Int16[,,]>("air",
                    DataSet.FromToEnd(0), /* removing first dimension from data*/
                    DataSet.ReduceDim(0), /* removing first dimension from data*/
                    DataSet.FromToEnd(0),
                    DataSet.FromToEnd(0));

    ReadSDSFactsSpan = DateTime.Now - ReadSDSFactsStart;
    ReadSDSFactsTotalMemory = GC.GetTotalMemory(false);

     

                int xMax = (xDim.Length<max)?xDim.Length:(int)max;
                int yMax = (yDim.Length < max) ? yDim.Length : (int)max;
                int tMax = (timeDim.Length<max)?timeDim.Length:(int)max;
                TableOperation insertOperation;
                TableBatchOperation batchOperation;

                EnviromentalFact fact;

                for (int x = 0; x < xMax; x++)
                {
                    for (int y = 0; y < yMax; y++)
                    {
                        batchOperation = new TableBatchOperation();
                        WriteFactStart = DateTime.Now;
                        for (int t = 0; t < tMax; t++)
                        {
                            fact = new EnviromentalFact();
                            fact.x = (int)xDim[x];
                            fact.y = (int)yDim[y];
                            fact.time = timeDim[t];
                            fact.level = (int)levelDim[1];
                            fact.air = facts[t, y, x];
                            fact.PartitionKey = fact.x.ToString() + "-" + fact.y.ToString() + "-" + fact.level;
                            fact.RowKey = fact.time.ToString();
                            batchOperation.Insert(fact);

                        }
                        // Execute the insert operation.
                        table.BeginExecuteBatch(batchOperation, null,null);

                        WriteFactSpan = WriteFactSpan + (DateTime.Now - WriteFactStart);
                        WriteFactTotalMemory = GC.GetTotalMemory(false);
                        if (WriteFactMaxMemory < WriteFactTotalMemory) WriteFactMaxMemory = WriteFactTotalMemory;
                    }
                }

            #endregion






            }
            catch (Exception e)
            {
                return "Error: " + e.Message;
            }

            // Construct the query operation for all customer entities where PartitionKey="Smith".
            TableQuery<EnviromentalFact> query = new TableQuery<EnviromentalFact>().Where(TableQuery.GenerateFilterCondition("PartitionKey", QueryComparisons.NotEqual, ""));

            TableQuery<EnviromentalFact> numberOfRowsQuery = new TableQuery<EnviromentalFact>();
            var numberOfRows = table.ExecuteQuery(numberOfRowsQuery).Count();


            Total = DateTime.Now - StartTime;

            string performance = " Number of Rows in Table: " + numberOfRows +
                " Total Span: "  + Total +               
                " Time and memory Reading SDS: " + 
            ReadSDSFactsSpan + " - " + ReadSDSFactsTotalMemory +

            " Time and max memory Writting to Tables: " + 
            WriteFactSpan + " - " + WriteFactMaxMemory;

            return schemaString + "    performance: " + performance;
        }


        public class EnviromentalFact : TableEntity
        {
            public EnviromentalFact() { }
            public int air { get; set; }
            public int x { get; set; }
            public int y { get; set; }
            public double time { get; set; }
            public int level { get; set; }
        }


        #region Blob Operations

        public List<CloudBlobContainer> listContainers()
        {
            string connectionString = ConfigurationManager.AppSettings["StorageConnectionString"];
            CloudStorageAccount storageAccount = CloudStorageAccount.Parse(connectionString);
            CloudBlobClient blobClient = storageAccount.CreateCloudBlobClient();
            List<CloudBlobContainer> containers = blobClient.ListContainers().ToList<CloudBlobContainer>(); 
            return containers;
        }

        public List<CloudBlockBlob> listBlobs(string containerName)
        {

            List<CloudBlockBlob> blobs = new List<CloudBlockBlob>();

            string connectionString = ConfigurationManager.AppSettings["StorageConnectionString"];
            CloudStorageAccount storageAccount = CloudStorageAccount.Parse(connectionString);

            CloudBlobClient blobClient = storageAccount.CreateCloudBlobClient();
            // Retrieve a reference to a container. 
            CloudBlobContainer container = blobClient.GetContainerReference(containerName);

  
            foreach (IListBlobItem item in container.ListBlobs(null, false))
            {
                if (item.GetType() == typeof(CloudBlockBlob))
                {
                    CloudBlockBlob blob = (CloudBlockBlob)item;
                    blobs.Add(blob);
                }
                else if (item.GetType() == typeof(CloudPageBlob))
                {
                    CloudPageBlob pageBlob = (CloudPageBlob)item;
                }
                else if (item.GetType() == typeof(CloudBlobDirectory))
                {
                    CloudBlobDirectory directory = (CloudBlobDirectory)item;
                }
            }

            return blobs;
 
        }

        public string deleteContainer(string containerName)
        {


            List<CloudBlockBlob> blobs = new List<CloudBlockBlob>();

            string connectionString = ConfigurationManager.AppSettings["StorageConnectionString"];
            CloudStorageAccount storageAccount = CloudStorageAccount.Parse(connectionString);

            CloudBlobClient blobClient = storageAccount.CreateCloudBlobClient();
 
            CloudBlobContainer container = blobClient.GetContainerReference(containerName);

      
            container.Delete();


            return "Container Deleted";

        }


        public string deleteBlob_in_envirolytics(string blobName)
        {


            List<CloudBlockBlob> blobs = new List<CloudBlockBlob>();

            string connectionString = ConfigurationManager.AppSettings["StorageConnectionString"];
            CloudStorageAccount storageAccount = CloudStorageAccount.Parse(connectionString);

            CloudBlobClient blobClient = storageAccount.CreateCloudBlobClient();
            // Retrieve a reference to a container. 
            CloudBlobContainer container = blobClient.GetContainerReference("envirolytic");

            CloudBlockBlob blockBlob = container.GetBlockBlobReference(blobName);

            blockBlob.Delete();


            return "Blob Deleted";

        }

        public string uploadBlob(string blobContainer, string blobName, string filePath)
        {
            DateTime Start = DateTime.Now;
            // Retrieve storage account from connection string.
            CloudStorageAccount storageAccount = CloudStorageAccount.Parse(
                CloudConfigurationManager.GetSetting("StorageConnectionString"));

            // Create the blob client.
            CloudBlobClient blobClient = storageAccount.CreateCloudBlobClient();

            // Retrieve reference to a previously created container.
            CloudBlobContainer container = blobClient.GetContainerReference(blobContainer.ToLower());
            container.CreateIfNotExists();

            // Retrieve reference to a blob named "myblob".
            CloudBlockBlob blockBlob = container.GetBlockBlobReference(blobName);

            // Create or overwrite the "myblob" blob with contents from a local file.
            using (var fileStream = System.IO.File.OpenRead(filePath))
            {
                blockBlob.UploadFromStream(fileStream);
            }


            DateTime End = DateTime.Now;
            TimeSpan Delay = End - Start;

            return "ok: " + Delay;
        }

        public string downloadBlob(string blobContainer, string blobName, string filePath)
        {
            DateTime Start = DateTime.Now;
            // Retrieve storage account from connection string.
            CloudStorageAccount storageAccount = CloudStorageAccount.Parse(
                CloudConfigurationManager.GetSetting("StorageConnectionString"));

            // Create the blob client.
            CloudBlobClient blobClient = storageAccount.CreateCloudBlobClient();

            // Retrieve reference to a previously created container.
            CloudBlobContainer container = blobClient.GetContainerReference(blobContainer);

            // Retrieve reference to a blob named "photo1.jpg".
            CloudBlockBlob blockBlob = container.GetBlockBlobReference(blobName);

            // Save blob contents to a file.
            using (var fileStream = System.IO.File.OpenWrite(filePath))
            {
                blockBlob.DownloadToStream(fileStream);
            }

            DateTime End = DateTime.Now;
            TimeSpan Delay = End - Start;

            return "ok: " + Delay;  
                    return "ok";
        }



        #endregion Blob Operations

        public List<string> listFiles_in_AppData(string appDataFolder)
        {
            List<string> response = new List<string>();


            string[] array1 = Directory.GetFiles(@appDataFolder);

            return array1.ToList();
        
         }

        public List<string> listTables()
        {
            List<string> response = new List<string>();


            string connectionString = ConfigurationManager.AppSettings["StorageConnectionString"];
            CloudStorageAccount storageAccount = CloudStorageAccount.Parse(connectionString);

            CloudTableClient tableClient = storageAccount.CreateCloudTableClient();

            var tables = tableClient.ListTables();

            foreach (var t in tables)
            {
                response.Add(t.Name);
            }

            return response;
        }

        public List<string> listTableValues(string tableName)
        {
            List<string> response = new List<string>();


            string connectionString = ConfigurationManager.AppSettings["StorageConnectionString"];
            CloudStorageAccount storageAccount = CloudStorageAccount.Parse(connectionString);

            CloudTableClient tableClient = storageAccount.CreateCloudTableClient();

            CloudTable table = tableClient.GetTableReference(tableName);

            // Construct the query operation for all customer entities where PartitionKey="Smith".
            TableQuery<EnviromentalFact> query = new TableQuery<EnviromentalFact>().Where(TableQuery.GenerateFilterCondition("PartitionKey", QueryComparisons.NotEqual, ""));

            TableQuery<EnviromentalFact> numberOfRowsQuery = new TableQuery<EnviromentalFact>();
            var numberOfRows = table.ExecuteQuery(numberOfRowsQuery).Count();

            string factString = "number of rows: " + numberOfRows;
            response.Add(factString);

            foreach (EnviromentalFact entity in table.ExecuteQuery(query).Take(100))
            {
                factString =
                    "PKey/Location: " + entity.PartitionKey + " | " +
                    "RKey/Time: " + entity.RowKey + " | " +
                    "x: " + entity.x + " | " +
                    "y: " + entity.y + " | " +
                    "l: " + entity.level + " | " +
                    "t: " + entity.time + " | " +
                    "air temp: " + entity.air;

                response.Add(factString);
            }

            return response;
        }

        public bool deleteFile_in_AppData(string fileName)
        {
            File.Delete(fileName);

             return true;
        }

        public bool deleteAll()
        {
            foreach(string fileName in Directory.GetFiles(AppDataFolder)){
                File.Delete(fileName);
            }

            return true;
        }

        public static string fileTimeStamp()
        {
            DateTime t = DateTime.Now;
            string timeStamp = "_" + t.Year + "_" + t.Month + "_" + t.Day + "_" + t.Hour + "_" + t.Minute + "_" + t.Second + "_" + t.Millisecond;

            return timeStamp; 
        }

        public static string schema2string(DataSetSchema schema)
        {
            string schemaString = "";
            string dimensionsString = "";

            foreach (var v in schema.Variables)
            {
                if (v.Name != "")
                {
                    schemaString += v.Name;
                    dimensionsString = "  ";
                    foreach (var d in v.Dimensions)
                    {
                        dimensionsString += "(" + d.Name + "," + d.Length + ")";
                    }
                    schemaString += dimensionsString;
                }
            }

            return schemaString;
        }

        public bool deleteTable(string tableName)
        {
            string connectionString = ConfigurationManager.AppSettings["StorageConnectionString"];
            CloudStorageAccount storageAccount = CloudStorageAccount.Parse(connectionString);

            CloudTableClient tableClient = storageAccount.CreateCloudTableClient();

            CloudTable table = tableClient.GetTableReference(tableName);

            table.DeleteIfExists();

            return true;
        }

    }
}
