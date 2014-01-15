using Microsoft.Research.Science.Data;
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

namespace JassWeather.Models
{
    public class JassWeatherDataSourceAPI
    {
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
                string downloadedFilePath = workingDirectoryPath + "tas_WRFG_example" + timeStamp + ".nc";

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
                string safeFileName = url.Replace('/', '_').Replace(':', '_');
                string downloadedFilePath = workingDirectoryPath + "/" + safeFileName;

                using (Stream responseStream = FtpClient.OpenRead(new Uri("ftp://ftp.cdc.noaa.gov/Datasets/NARR/pressure/air.197901.nc")))
                    {
                          double total = 0;
                          using (Stream fileStream = System.IO.File.OpenWrite(downloadedFilePath))
                            {
                                byte[] buffer = new byte[8192];
                                int bytesRead = responseStream.Read(buffer, 0, 4096);
                                while (bytesRead > 0)
                                {
                                    fileStream.Write(buffer, 0, bytesRead);
                                    total += bytesRead;
                                    Console.WriteLine(total);
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
    }
}
