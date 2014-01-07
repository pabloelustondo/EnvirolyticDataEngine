using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Research.Science.Data; 
using Microsoft.Research.Science.Data.Imperative;

namespace JassWeatherAPI
{
    class Program
    {

        static void Main(string[] args)
        {
        /*    
            var dataset = DataSet.Open(path("Tutorial.csv"));
            var x = dataset.GetData<double[]>("X"); 
            var y = dataset.GetData<double[]>("Observation"); 

            var xm = x.Sum() / x.Length; 
            var ym = y.Sum() / y.Length; 
            double xy = 0; 
            for (int i = 0; i < x.Length; i++) 
                xy += (x[i] - xm) * (y[i] - ym); 
                double xx = 0; 
                for (int i = 0; i < x.Length; i++) 
                xx += (x[i] - xm) * (x[i] - xm); 
                var a = xy / xx; 
                var b = ym - a * xm; 
                var model = new double[x.Length]; 
                for (int i = 0; i < x.Length; i++) 
                model[i] = a * x[i] + b; 
                // write output data 
               // dataset.Add<double[]>("Model"); 
               // dataset.PutData<double[]>("Model", model); 
*/
                var dataset = DataSet.Open(path("tas_WRFG_example.nc"));
                var yx = dataset.GetData<double[]>("yc");
                var xc = dataset.GetData<double[]>("xc");
                var time = dataset.GetData<double[]>("time");
                var schema = dataset.GetSchema();

                var level = dataset.GetData<double>("level");

            // tas[,yc=43,xc=67]
                var tas = dataset.GetData<Single[,,]>("tas");

            /*
                var xm = x.Sum() / x.Length;
                var ym = y.Sum() / y.Length;
                double xy = 0;
                for (int i = 0; i < x.Length; i++)
                    xy += (x[i] - xm) * (y[i] - ym);
                double xx = 0;
                for (int i = 0; i < x.Length; i++)
                    xx += (x[i] - xm) * (x[i] - xm);
                var a = xy / xx;
                var b = ym - a * xm;
                var model = new double[x.Length];
                for (int i = 0; i < x.Length; i++)
                    model[i] = a * x[i] + b; 
            */


                
            dataset.View(); 

        }

        static string path(string filename)
        {
            return "..\\..\\Data\\" + filename;
        }
        static void OldMain(string[] args)
        {

            string commandResponse = "";
            string commandResponse1 = "";

            try
            {
              

                DateTime startTime = DateTime.UtcNow;
                WebRequest request = WebRequest.Create("http://www.narccap.ucar.edu/data/example/tas_WRFG_example.nc");
                WebResponse response = request.GetResponse();

                DateTime t = DateTime.Now;
                string timeStamp = "_" + t.Year + "_" + t.Month + "_" + t.Day + "_" + t.Hour + "_" + t.Minute + "_" + t.Second + "_" + t.Millisecond;
                string downloadedFilePath = "C:\\TEMPORARY\\tas_WRFG_example" + timeStamp + ".nc";

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

                string command = string.Format("/c copy " + downloadedFileName + " " + downloadedFileName + ".bak");

                ProcessStartInfo cmdsi = new ProcessStartInfo("cmd.exe");
                cmdsi.WorkingDirectory = "C:\\TEMPORARY";
                cmdsi.Arguments = command;
                cmdsi.RedirectStandardOutput = true;
                cmdsi.UseShellExecute = false;
                cmdsi.CreateNoWindow = false;
                Process cmd = Process.Start(cmdsi);


                //At this point file was downloaded and backedp

           //     string executablePath = "nc2text";
           //     string command1 = string.Format("/c " + executablePath + " " + downloadedFileName + " tas[,yc=43,xc=67] > x.y");

                string executablePath = "nc2text";
                string command1 = string.Format(executablePath + " " + downloadedFileName + " tas[,yc=43,xc=67] > x.y");

                ProcessStartInfo cmdsi1 = new ProcessStartInfo(executablePath);
                cmdsi.WorkingDirectory = "C:\\TEMPORARY";
                cmdsi.Arguments = command1;
                cmdsi.RedirectStandardOutput = true;
                cmdsi.UserName = "pablo";
                cmdsi.LoadUserProfile = true;
                cmdsi.UseShellExecute = true;
                cmdsi.CreateNoWindow = false;

                Process cmd1 = Process.Start(cmdsi1);

                string command2 = string.Format("/c dir");
                ProcessStartInfo cmdsi2 = new ProcessStartInfo("cmd.exe");
                cmdsi2.WorkingDirectory = "C:\\TEMPORARY";
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
                Console.Write(e.Message + e.InnerException.ToString());
            }

            Console.Write("cool");
        }
    }
}
