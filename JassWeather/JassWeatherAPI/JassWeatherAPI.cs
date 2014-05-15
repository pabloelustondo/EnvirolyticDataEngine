
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
using System.Web.Helpers;

namespace JassWeather.Models
{    
    public class JassBlob {
        public string url { get; set; }
        public int length { get; set; }   
    }

    public class JassRGB
    {
        public int r { get; set; }
        public int g { get; set; }
        public int b { get; set; }

        public JassRGB(int rIn, int gIn, int bIn)
        {
            r = rIn; g = gIn; b = bIn;
        }
    }
    public class JassGridValues
    {
        public int timeLength { get; set; }
        public int levelLength { get; set; }
        public int yLength { get; set; }
        public int xLength { get; set; }
        public double? [, , ,] measure { get; set; }
        public double[]  measureMax { get; set; }
        public double[]  measureMin { get; set; }
        public int[] minX { get; set; }
        public int[] maxX { get; set; }
        public int[] minY { get; set; }
        public int[] maxY { get; set; }
        public int[] minT { get; set; }
        public int[] maxT { get; set; }
        public string VariableName { get; set; }
        public MetadataDictionary variableMetadata { get; set; }
 

        public JassGridValues(MetadataDictionary variableMetadataIn, string variableNameIn, int timeLengthIn, int levelLengthIn, int yLengthIn, int xLengthIn)
        {
            VariableName = variableNameIn;
            variableMetadata = variableMetadataIn;
            measureMax = new double[levelLengthIn];
            measureMin = new double[levelLengthIn];
            minX = new int[levelLengthIn];
            minY = new int[levelLengthIn];
            maxX = new int[levelLengthIn];
            maxY = new int[levelLengthIn];
            minT = new int[levelLengthIn];
            maxT = new int[levelLengthIn];
            timeLength = timeLengthIn;
            levelLength = levelLengthIn;
            yLength = yLengthIn;
            xLength = xLengthIn;
            measure = new double?[timeLengthIn, levelLengthIn, yLengthIn, xLengthIn];
        }
    }

    public class JassFileNameComponents
    {
        string ContainerName { get; set; }
        string VariableName { get; set; }
        string BlobName { get; set; }
        public int year { get; set; }
        public int month { get; set; }
        public int day { get; set; }

        public JassFileNameComponents(string blobUri)
        {
            int index = blobUri.IndexOf("_");

            this.ContainerName = blobUri.Substring(0, index).ToLower();
            this.VariableName = blobUri.Substring(0, index);
            this.BlobName = blobUri;

            string fileNameRest1 = blobUri.Substring(index+1);
            int indexRest1 = fileNameRest1.IndexOf("_");
            string yearString = fileNameRest1.Substring(0, indexRest1).ToLower();

            string fileNameRest2 = fileNameRest1.Substring(indexRest1 + 1);
            int indexRest2 = fileNameRest2.IndexOf("_");
            string monthString = fileNameRest2.Substring(0, indexRest2).ToLower();

            string fileNameRest3 = fileNameRest2.Substring(indexRest2 + 1);
            int indexRest3 = fileNameRest3.IndexOf(".");
            string dayString = fileNameRest3.Substring(0, indexRest3).ToLower();


            this.year = Int16.Parse(yearString);
            this.month = Int16.Parse(monthString);
            this.day = Int16.Parse(dayString);
        }
    }

    public class JassWeatherAPI
    {
        private JassWeatherContext db = new JassWeatherContext();
        public string AppDataFolder;
        public string AppTempFilesFolder;
        public string AppFilesFolder;
        public string ServerNameJass;
        public string storageConnectionString;
        DateTime startTotalTime = DateTime.UtcNow;
        DateTime endTotalTime = DateTime.UtcNow;
        public static JassRGB[] colors = getColors();

        #region Generate Testing File

        public string generateOutputTestFile(List<List<List<JassGridValues>>> gridValues, List<JassLatLon> locations, DateTime startDate, DateTime endDate ){

            //this function will create a file with the specified data using a file format

        TimeSpan timeSpan = endDate.AddDays(1) - startDate;
        int totalDays = (int)timeSpan.TotalDays;

        for (int l = 0; l < locations.Count; l++)
        {
            JassLatLon location = locations[l];
            string header = "locationName, lat, lon, startDate, endDate";
            string headerContent = location.Name + "," + location.Lat + "," + location.Lon + "," + startDate + "," + endDate;

            string outputFilePath = AppTempFilesFolder + "/report_"+ location.Name +".csv";
            using (System.IO.StreamWriter file = new System.IO.StreamWriter(outputFilePath))
            {
                file.WriteLine(header);
                file.WriteLine(headerContent);
                file.WriteLine("");

                string tableHeader = "Year,	Month,	Day, Date Time Index, UTC Time, LocalTime";
                for (int v = 0; v < gridValues[0][0].Count; v++)
                {
                    tableHeader = tableHeader + "," + gridValues[0][l][v].VariableName;
                }
                string tableLine;
                file.WriteLine(tableHeader);


                DateTime day = startDate; int step=0;
                for( int d=0; d< totalDays; d++){
                    for (int t = 0; t < 24; t++)
                    {
                        step = (int)t / 3;
                        int lt = t + locations[l].hrDifference;
                        if( lt<0 ) lt=24+lt;
                        string dayString = day.Month + "/" + day.Day + "/" + day.Year + " " + t + ":00";
                        tableLine = day.Year + "," + day.Month + "," + day.Day + "," + step + "," + t + "," + lt;

                        for (int v = 0; v < gridValues[d][l].Count; v++)
                        {
                            tableLine = tableLine + "," + gridValues[d][l][v].measure[step,0,0,0];
                        }

                        file.WriteLine(tableLine);
                    }
                    day = day.AddDays(1);
                }
            }

        }

        return "report for " + locations.Count + " locations where successfully generated";
        }

#endregion





        public JassWeatherAPI(string ServerNameIn, string appDataFolder, string storageConnectionStringIn){
            this.storageConnectionString = storageConnectionStringIn;
            this.AppDataFolder = appDataFolder;
            this.AppFilesFolder = appDataFolder + "\\..\\App_Files";
            this.AppTempFilesFolder = appDataFolder + "\\..\\App_TempFiles";
            this.ServerNameJass = ServerNameIn;
          }

        public static JassRGB[] getColors()
        {
            JassRGB[] color = new JassRGB[1024];

            for (int x = 0; x < 256; x++)
            {
                color[x] = new JassRGB(0, x, 255);
            }
            for (int x = 0; x < 256; x++)
            {
                color[256 + x] = new JassRGB(0, 255, 255-x);
            }
            for (int x = 0; x < 256; x++)
            {
                color[256 + 256 + x] = new JassRGB(x, 255, 0);
            }
            for (int x = 0; x < 256; x++)
            {
                color[256 + 256 + 256 + x] = new JassRGB(255, 255-x, 0);
            }

            return color;
        }

        public static JassRGB rgb(double value, double min, double max){

            double indexDouble = (value - min) / (max - min) * 1023;
            int index = Convert.ToInt16(indexDouble);

            return colors[index]; 

        }

        public bool checkIfBlobExist(string container, string fileName)
        {
            string connectionString = ConfigurationManager.AppSettings[storageConnectionString];
            CloudStorageAccount storageAccount = CloudStorageAccount.Parse(connectionString);
            CloudBlobClient blobClient = storageAccount.CreateCloudBlobClient();
            var blob = blobClient.GetContainerReference(container).GetBlockBlobReference(fileName);
            Boolean fileOnBlob = blob.Exists();

            return fileOnBlob;
        }

        public string processSource(JassBuilder builder, int year, int month, int weeky, int day, Boolean upload, Boolean overWrite, JassBuilderLog builderAllLog)
        {
            //this method will be idempotent... if nothing to do does nothing.
            //this method is to help processBulder and will produce the file on disk
            //the idea is that, first, it will check if the file is already there.
            //unless overWrite is set of true which means that it will preprocess anyway.

            //Check whether the file is on disk

                APIRequest source = builder.APIRequest;
                string url = replaceURIPlaceHolders(source.url, year, month,weeky,day);

                string fileName = safeFileNameFromUrl(url);
                string filePath = AppDataFolder + "/" + fileName;
                Boolean fileOnDisk = File.Exists(filePath);

                //check if the file is on storage

                Boolean fileOnBlob = false;
                Boolean blobAccess = true;

                try { fileOnBlob = checkIfBlobExist("ftp", fileName); }
                catch (Exception) { blobAccess = false; };

                string LogMessage = "fileOnBlob: " + fileOnBlob + "fileOnDisk: " + fileOnDisk;
                DateTime processSourceStartime = DateTime.Now;
                JassBuilderLog childBuilderLog1 = createBuilderLogChild(builderAllLog, builder, year, month, "processSource_AfterCheck", "Test", LogMessage, new TimeSpan(), true);


                if (!fileOnDisk)
                {

                    //check if the file is on blob storage
                    if (fileOnBlob)
                    {
                        DownloadFile2DiskIfNotThere(fileName, filePath);
                    }
                    else
                    {
                        //This is the case where we have to really download the file form the actual source
                        //For the moment I am assuming that this is FTP-netCDF... 

                        if (builder.APIRequest.type == "FTP-netCDF")
                        {
                            get_big_NetCDF_by_ftp2(url, AppDataFolder);
                        }
                        if (builder.APIRequest.type == "HTTP-netCDF")
                        {
                            get_big_NetCDF_by_http2(url, AppDataFolder);
                        }
                        else
                        {
                            //we have a problem here we could not find the file
                           createBuilderLogChild(builderAllLog, builder, year, month, "processSource_FILE NOT FOUND CANNOT DOWNLOAD", "Test", LogMessage, new TimeSpan(), true);
                           throw new Exception("FILE NOT FOUND CANNOT DOWNLOAD: " + fileName);
                        }

                    }
                }

                //so, here we know the file is on dis for sure (unless there is an error)
                if (upload && blobAccess && !fileOnBlob)
                {
                    uploadBlob("ftp", fileName, filePath);
                }

                JassBuilderLog childBuilderLog2 = createBuilderLogChild(builderAllLog, builder, year, month, "processSource_End", "Test", LogMessage, DateTime.Now - processSourceStartime, true);

            return filePath; 
        }

        public static double HaversineDistance(double firstLat, double firstLong, double secondLat, double secondLong)
        {
            double dLat = toRadian(secondLat - firstLat);
            double dLon = toRadian(secondLong - firstLong);
            double a = Math.Sin(dLat / 2) * Math.Sin(dLat / 2) +
            Math.Cos(toRadian(firstLat)) * Math.Cos(toRadian(secondLat)) *
            Math.Sin(dLon / 2) * Math.Sin(dLon / 2);
            double c = 2 * Math.Asin(Math.Min(1, Math.Sqrt(a)));
            double d = 6371 * 2 * Math.Asin(Math.Min(1, Math.Sqrt(a)));
            return Math.Abs(d); 
	    }

        public static double measureAtLocation(double firstLat, double firstLong){

            //this function will return the value on the macc grid
            return 0;

        }

        public class JassMaccNarrGridsCombo
        {
            public string narrSchema { get; set; }
            public string maccSchema { get; set; }

            //schema2string

            public Single[] maccLat { get; set; }
            public Single[] maccLon { get; set; }

            public string[] station { get; set; }

            public Single[,] narrLon { get; set; }
            public Single[,] narrLat { get; set; }


            public int narrXMax = 349;
            public int narrYMax = 277;
            public int narrXMin = 0;
            public int narrYMin = 0;

            public int maccLatMin = 0;
            public int maccLatMax = 90;
            public int maccLonMin = 0;
            public int maccLonMax = 320;

            public JassGridLocation[,] map = new JassGridLocation[277, 349];
            public JassGridLocation[,] map2 = new JassGridLocation[277, 349];
            public JassGridLocation[,] map3 = new JassGridLocation[277, 349];
            public JassGridLocation[,] map4 = new JassGridLocation[277, 349];
        }

        public class JassGridLocation
        {
            public int lat { get; set; }
            public int lon { get; set; }
            public double latitud { get; set; }
            public double longitud { get; set; }
            public double distance { get; set; }
        }

        public JassMaccNarrGridsCombo MapGridNarr2Macc(JassMaccNarrGridsCombo gc)
        {
            int maxDistance = 200;
            JassBuilder builder = new JassBuilder();
            DateTime start = DateTime.Now;
            JassBuilderLog builderLog = this.createBuilderLog(builder, "mapGridStart", "", "", new TimeSpan(), true);

            TimeSpan timeCalculatingDistance = start-start;
 
            double minDistance, minDistance2, minDistance3, minDistance4;
            for (int y = gc.narrYMin; y < gc.narrYMax; y++)
            {
 
                start = DateTime.Now;
                for (int x = gc.narrXMin; x < gc.narrXMax; x++)
                {
                    minDistance = maxDistance; minDistance2 = maxDistance; minDistance3 = maxDistance; minDistance4 = maxDistance;
                    
                    for (int lat = gc.maccLatMin; lat < gc.maccLatMax; lat++)   //161
                    {
                         float bestCase = Math.Abs((gc.narrLat[y, x] - gc.maccLat[lat]))*100; //this is aproximatelly in KM  
                         if (bestCase < maxDistance)
                         {
                             for (int lon = gc.maccLonMin; lon < gc.maccLonMax; lon++)  //320
                             {
                                 DateTime beforeDistance = DateTime.Now;
                                 double distance = HaversineDistance(gc.maccLat[lat], gc.maccLon[lon], gc.narrLat[y, x], gc.narrLon[y, x]);
                                 timeCalculatingDistance = timeCalculatingDistance + (DateTime.Now - beforeDistance);
                                 if (Math.Abs(distance) != distance || Math.Abs(distance) < bestCase)
                                 {
                                     throw new Exception("what??");
                                 }
                                 if (distance < minDistance)
                                 {
                                     minDistance = distance;
                                     gc.map4[y, x] = gc.map3[y, x];
                                     gc.map3[y, x] = gc.map2[y, x];
                                     gc.map2[y, x] = gc.map[y, x];
                                     gc.map[y, x] = new JassGridLocation();
                                     gc.map[y, x].distance = distance;
                                     gc.map[y, x].lat = lat;
                                     gc.map[y, x].lon = lon;
                                     gc.map[y, x].latitud = gc.maccLat[lat];
                                     gc.map[y, x].longitud = gc.maccLon[lon];
                                 }
                                 else
                                     if (distance < minDistance2)
                                     {
                                         minDistance2 = distance;
                                         gc.map4[y, x] = gc.map3[y, x];
                                         gc.map3[y, x] = gc.map2[y, x];
                                         gc.map2[y, x] = new JassGridLocation();
                                         gc.map2[y, x].distance = distance;
                                         gc.map2[y, x].lat = lat;
                                         gc.map2[y, x].lon = lon;
                                         gc.map2[y, x].latitud = gc.maccLat[lat];
                                         gc.map2[y, x].longitud = gc.maccLon[lon];
                                     }
                                     else
                                         if (distance < minDistance3)
                                         {
                                             minDistance3 = distance;
                                             gc.map4[y, x] = gc.map3[y, x];
                                             gc.map3[y, x] = new JassGridLocation();
                                             gc.map3[y, x].distance = distance;
                                             gc.map3[y, x].lat = lat;
                                             gc.map3[y, x].lon = lon;
                                             gc.map3[y, x].latitud = gc.maccLat[lat];
                                             gc.map3[y, x].longitud = gc.maccLon[lon];
                                         }
                                         else
                                             if (distance < minDistance4)
                                             {
                                                 minDistance4 = distance;
                                                 gc.map4[y, x] = new JassGridLocation();
                                                 gc.map4[y, x].distance = distance;
                                                 gc.map4[y, x].lat = lat;
                                                 gc.map4[y, x].lon = lon;
                                                 gc.map4[y, x].latitud = gc.maccLat[lat];
                                                 gc.map4[y, x].longitud = gc.maccLon[lon];
                                             }
                             }
                         }//end if best case less then max
                    }
                }
                JassBuilderLog builderLog2 = createBuilderLog(builder, "mapGridY: with timeCalcualting distance" + y, "", "", timeCalculatingDistance, true);
            }

            return gc;
        }

        public static JassMaccNarrGridsCombo MapGridNarr2Sher(JassMaccNarrGridsCombo gc)
        {
            JassBuilder builder = new JassBuilder();
            DateTime start = DateTime.Now;
            JassWeatherAPI apiCaller2 = new JassWeatherAPI("", "", "");
            JassBuilderLog builderLog = apiCaller2.createBuilderLog(builder, "mapGridStart", "", "", new TimeSpan(), true);

            double minDistance, minDistance2, minDistance3, minDistance4;
            for (int y = gc.narrYMin; y < gc.narrYMax; y++)
            {
                JassBuilderLog builderLog2 = apiCaller2.createBuilderLog(builder, "mapGridY:" + y, "", "", DateTime.Now - start, true);
                start = DateTime.Now;
                for (int x = gc.narrXMin; x < gc.narrXMax; x++)
                {
                    minDistance = 40000; minDistance2 = 40000; minDistance3 = 40000; minDistance4 = 40000;
                    for (int z = gc.maccLatMin; z < gc.maccLatMax; z++)   //161
                    {
                            double distance = HaversineDistance(gc.maccLat[z], gc.maccLon[z], gc.narrLat[y, x], gc.narrLon[y, x]);
                            if (distance < minDistance)
                            {
                                minDistance = distance;
                                gc.map4[y, x] = gc.map3[y, x];
                                gc.map3[y, x] = gc.map2[y, x];
                                gc.map2[y, x] = gc.map[y, x];
                                gc.map[y, x] = new JassGridLocation();
                                gc.map[y, x].distance = distance;
                                gc.map[y, x].lat = z;
                                gc.map[y, x].lon = z;
                                gc.map[y, x].latitud = gc.maccLat[z];
                                gc.map[y, x].longitud = gc.maccLon[z];
                            }
                            else
                                if (distance < minDistance2)
                                {
                                    minDistance2 = distance;
                                    gc.map4[y, x] = gc.map3[y, x];
                                    gc.map3[y, x] = gc.map2[y, x];
                                    gc.map2[y, x] = new JassGridLocation();
                                    gc.map2[y, x].distance = distance;
                                    gc.map2[y, x].lat = z;
                                    gc.map2[y, x].lon = z;
                                    gc.map2[y, x].latitud = gc.maccLat[z];
                                    gc.map2[y, x].longitud = gc.maccLon[z];
                                }
                                else
                                    if (distance < minDistance3)
                                    {
                                        minDistance3 = distance;
                                        gc.map4[y, x] = gc.map3[y, x];
                                        gc.map3[y, x] = new JassGridLocation();
                                        gc.map3[y, x].distance = distance;
                                        gc.map3[y, x].lat = z;
                                        gc.map3[y, x].lon = z;
                                        gc.map3[y, x].latitud = gc.maccLat[z];
                                        gc.map3[y, x].longitud = gc.maccLon[z];
                                    }
                                    else
                                        if (distance < minDistance4)
                                        {
                                            minDistance4 = distance;
                                            gc.map4[y, x] = new JassGridLocation();
                                            gc.map4[y, x].distance = distance;
                                            gc.map4[y, x].lat = z;
                                            gc.map4[y, x].lon = z;
                                            gc.map4[y, x].latitud = gc.maccLat[z];
                                            gc.map4[y, x].longitud = gc.maccLon[z];
                                        }
                    }
                }
            }

            return gc;
        }

        public static double GetDistanceBetweenPoints(double lat1, double long1, double lat2, double long2)
        {
            double distance = 0;

            double dLat = (lat2 - lat1) / 180 * Math.PI;
            double dLong = (long2 - long1) / 180 * Math.PI;

            double a = Math.Sin(dLat / 2) * Math.Sin(dLat / 2)
                        + Math.Cos(lat2) * Math.Sin(dLong / 2) * Math.Sin(dLong / 2);
            double c = 2 * Math.Atan2(Math.Sqrt(a), Math.Sqrt(1 - a));

            //Calculate radius of earth
            // For this you can assume any of the two points.
            double radiusE = 6378135; // Equatorial radius, in metres
            double radiusP = 6356750; // Polar Radius

            //Numerator part of function
            double nr = Math.Pow(radiusE * radiusP * Math.Cos(lat1 / 180 * Math.PI), 2);
            //Denominator part of the function
            double dr = Math.Pow(radiusE * Math.Cos(lat1 / 180 * Math.PI), 2)
                            + Math.Pow(radiusP * Math.Sin(lat1 / 180 * Math.PI), 2);
            double radius = Math.Sqrt(nr / dr);

            //Calaculate distance in metres.
            distance = radius * c;
            return distance;
        }

        public static double toRadian(double val)
        {
            return (Math.PI / 180) * val;
        }

       // public JassMaccNarrGridsCombo MapFromMaccToNarr(int year, int month, string fileNameMaccTemp, string fileNameNarrTemp)
       // JassBuilder builder, int year, int month, Boolean upload, Boolean overWrite, JassBuilderLog builderAllLog
        public string processGridMappingMaccToNarr(int year, int month, string fileNameMaccTemp)
        {
            string fileNameMacc = replaceURIPlaceHolders(fileNameMaccTemp, year, month,0,0);
            string fileNameNarr = replaceURIPlaceHolders("Narr_Grid.nc", year, month,0,0);

            JassMaccNarrGridsCombo gc = new JassMaccNarrGridsCombo();
            string maccFile = AppDataFolder + "/" + fileNameMacc;
            string narrFile = AppFilesFolder + "/" + fileNameNarr;
            string mapFile = AppFilesFolder + "/Narr_2_Macc_Grid_Mapper.nc";

            string smonth = (month < 10) ? "0" + month : "" + month;
            string outputFileName=null;
            string outputFilePath =null;

            Int16 missingValue = -32767;
            Int16 fillValue = -32767;

            string VariableName = null;
            Dictionary<string, MetadataDictionary> vars =
                        new Dictionary<string, MetadataDictionary>();



            using (var narrDataSet = DataSet.Open(narrFile + "?openMode=open"))
            {
                Dictionary<string, MetadataDictionary> narrVars = new Dictionary<string, MetadataDictionary>();
                foreach (var v in narrDataSet.Variables) { narrVars.Add(v.Name, v.Metadata); }

                Single[] narrY = narrDataSet.GetData<Single[]>("y");
                Single[] narrX = narrDataSet.GetData<Single[]>("x");
           //     double[] narrTime = narrDataSet.GetData<double[]>("time");
     
                using (var maccDataSet = DataSet.Open(maccFile + "?openMode=open"))
                {
                    Dictionary<string, MetadataDictionary> maccVars = new Dictionary<string, MetadataDictionary>();
                    foreach (var v in maccDataSet.Variables) { 
                        maccVars.Add(v.Name, v.Metadata);
                        if (v.Dimensions.Count > 2 && VariableName == null) { 
                            
                            VariableName = v.Name;
                            try
                            {
                                missingValue = (Int16)v.Metadata["missing_value"];
                                fillValue = (Int16)v.Metadata["_FillValue"];
                            }
                            catch (Exception) { };
                        };
                    }

                    outputFileName = VariableName + "_macc2narr_" + year + "_" + smonth + ".nc";
                    outputFilePath = AppDataFolder + "\\" + outputFileName;

                    Int32[] maccTime = maccDataSet.GetData<Int32[]>("time");
                    double[] maccNarrTime = new double[maccTime.Length];

                    DateTime day1900 = DateTime.Parse("1900-01-01 00:00:00");
                    DateTime day1800 = DateTime.Parse("1800-01-01 00:00:00");

                    TimeSpan diff19001800 = day1900 - day1800;

                    int hours19001800 = (int)diff19001800.TotalHours;

                    DateTime maccDay;
                    DateTime narrDay;
                    double narrNumber;

                    DateTime maccDayStart = day1900.AddHours(maccTime[0]);
                    DateTime narrDayStart = new DateTime(maccDayStart.Year, maccDayStart.Month, maccDayStart.Day);

                    if (maccDayStart.Year != year || maccDayStart.Month != month)
                    {
                        createBuilderLog("ERROR","maccDayStart.Year != year || maccDayStart.Month != month","maccDayStart.Year != year || maccDayStart.Month != month", false);
                        throw new Exception("maccDayStart.Year != year || maccDayStart.Month != month");
                      
                    }

                    double narrDayStartHours = (narrDayStart - day1800).TotalHours;

                    double narrDayHours = narrDayStartHours;
                    for (int t = 0; t < maccTime.Length; t++)
                    {
                         maccNarrTime[t] = narrDayHours;
                         narrDayHours += 3; 
                    }


                    //At this point we have the time dimension in the variable maccNarrTime
                    //we do not need the time dimension from narr anymore.

                    using (var mapDataSet = DataSet.Open(mapFile + "?openMode=open"))
                    {

                        var narrSchema = narrDataSet.GetSchema();
                        var maccSchema = maccDataSet.GetSchema();

                        gc.narrSchema = schema2string(narrSchema);
                        gc.maccSchema = schema2string(maccSchema);

                        gc.maccLat = maccDataSet.GetData<Single[]>("latitude");
                        gc.maccLon = maccDataSet.GetData<Single[]>("longitude");

                        gc.narrLon = narrDataSet.GetData<Single[,]>("lon");
                        gc.narrLat = narrDataSet.GetData<Single[,]>("lat");


                        var mapDistance = mapDataSet.GetData<double[,]>("mapDistance");
                        var mapLatY = mapDataSet.GetData<int[,]>("mapLatY");
                        var mapLonX = mapDataSet.GetData<int[,]>("mapLonX");


                        var map2Distance = mapDataSet.GetData<double[,]>("map2Distance");
                        var map2LatY = mapDataSet.GetData<int[,]>("map2LatY");
                        var map2LonX = mapDataSet.GetData<int[,]>("map2LonX");


                        var map3Distance = mapDataSet.GetData<double[,]>("map3Distance");
                        var map3LatY = mapDataSet.GetData<int[,]>("map3LatY");
                        var map3LonX = mapDataSet.GetData<int[,]>("map3LonX");


                        var map4Distance = mapDataSet.GetData<double[,]>("map4Distance");
                        var map4LatY = mapDataSet.GetData<int[,]>("map4LatY");
                        var map4LonX = mapDataSet.GetData<int[,]>("map4LonX");

                        //mapp the grids

                       // gc = JassWeather.Models.JassWeatherAPI.MapGridNarr2Macc(gc);

                        
                            for (int y = 0; y < gc.narrYMax; y++)
                            {
                                for (int x = 0; x < gc.narrXMax; x++)
                                {
                                    try{

                                        gc.map[y, x] = new JassGridLocation();
                                        gc.map[y, x].distance = mapDistance[y,x];
                                        gc.map[y, x].lat = mapLatY[y,x];
                                        gc.map[y, x].lon = mapLonX[y,x];
                                        gc.map[y, x].latitud = gc.maccLat[gc.map[y, x].lat];
                                        gc.map[y, x].longitud = gc.maccLon[gc.map[y, x].lon];

                                        gc.map2[y, x] = new JassGridLocation();
                                        gc.map2[y, x].distance = map2Distance[y, x];
                                        gc.map2[y, x].lat = map2LatY[y, x];
                                        gc.map2[y, x].lon = map2LonX[y, x];
                                        gc.map2[y, x].latitud = gc.maccLat[gc.map2[y, x].lat];
                                        gc.map2[y, x].longitud = gc.maccLon[gc.map2[y, x].lon];

                                        gc.map3[y, x] = new JassGridLocation();
                                        gc.map3[y, x].distance = map3Distance[y, x];
                                        gc.map3[y, x].lat = map3LatY[y, x];
                                        gc.map3[y, x].lon = map3LonX[y, x];
                                        gc.map3[y, x].latitud = gc.maccLat[gc.map3[y, x].lat];
                                        gc.map3[y, x].longitud = gc.maccLon[gc.map3[y, x].lon];
 
                                        gc.map4[y, x] = new JassGridLocation();
                                        gc.map4[y, x].distance = map4Distance[y, x];
                                        gc.map4[y, x].lat = map4LatY[y, x];
                                        gc.map4[y, x].lon = map4LonX[y, x];
                                        gc.map4[y, x].latitud = gc.maccLat[gc.map4[y, x].lat];
                                        gc.map4[y, x].longitud = gc.maccLon[gc.map4[y, x].lon];
                                    
                                    }catch (Exception e){

                                        var v = "crap";
                                    
                                    }

                                }
                            }


                        //Ok, now let's process the file converting from Macc to Narr at the measure level.

                            int thinking = 1;//


                        ////////  getting all the dimensions from Narr
                        /* this was before
                            Single[] narrY = narrDataSet.GetData<Single[]>("y");
                            Single[] narrX = narrDataSet.GetData<Single[]>("x");
                            double[] narrTime = narrDataSet.GetData<double[]>("time");
                         */ 
                        Int16[, ,] maccVariable = maccDataSet.GetData<Int16[, ,]>(VariableName,
                                DataSet.FromToEnd(0),
                                DataSet.FromToEnd(0),
                                DataSet.FromToEnd(0));

                        ////filling up the array
                            Int16[, ,] outputVariable = new Int16[maccNarrTime.Length, narrY.Length, narrX.Length];

                            for (int t = 0; t < maccNarrTime.Length; t++)
                            {
                                for (int y = 0; y < narrY.Length; y++)
                                {
                                    for (int x = 0; x < narrX.Length; x++)
                                    {
                                        try
                                        {
                                            outputVariable[t, y, x] = (Int16)interpolateValue(t,y,x,maccVariable, gc, missingValue, fillValue);
                                        }
                                        catch (Exception e)
                                        {
                                            var dosomething = 1;
                                        }
                                    }
                                }
                            }
                       /////// Writting results into file
                       //   dataset3.Add<Int16[, ,]>(builder.JassVariable.Name, dataset, "time", "y", "x");

                            //we will enter year/month as parameter
                           
                            using (var outputDataSet = DataSet.Open(outputFilePath + "?openMode=create"))
                            {
                                /*
                                 * 
                Single[] narrY = narrDataSet.GetData<Single[]>("y");
                Single[] narrX = narrDataSet.GetData<Single[]>("x");
                double[] narrTime = narrDataSet.GetData<double[]>("time");
                                 */


                                outputDataSet.Add<double[]>("time", maccNarrTime, "time");
                        //        foreach (var attr in narrVars["time"]) { if (attr.Key != "Name") outputDataSet.PutAttr("time", attr.Key, attr.Value); }
                                outputDataSet.Add<Single[]>("y", narrY, "y");
                                foreach (var attr in narrVars["y"]) { if (attr.Key != "Name") outputDataSet.PutAttr("y", attr.Key, attr.Value); }
                                outputDataSet.Add<Single[]>("x", narrX, "x");
                                foreach (var attr in narrVars["y"]) { if (attr.Key != "Name") outputDataSet.PutAttr("x", attr.Key, attr.Value); }
                                outputDataSet.Add<Int16[, ,]>(VariableName, outputVariable, "time", "y", "x");
                                foreach (var attr in maccVars[VariableName]) {
                                    if (attr.Key != "Name") {
                                        if (attr.Key != "_FillValue")
                                        {
                                            outputDataSet.PutAttr(VariableName, attr.Key, attr.Value);
                                        }
                                        else
                                        {
                                            outputDataSet.PutAttr(VariableName, "FillValue", attr.Value);
                                        }
                                    }
                                }
                     
                            }


                        //now let's test 
                            using (var testDataSet = DataSet.Open(outputFilePath + "?openMode=open"))
                            {

                                Int16[, ,] testVariable = testDataSet.GetData<Int16[, ,]>(VariableName,
                                        DataSet.FromToEnd(0),
                                        DataSet.FromToEnd(0),
                                        DataSet.FromToEnd(0));

                                Single[] testY = testDataSet.GetData<Single[]>("y");
                                Single[] testX = testDataSet.GetData<Single[]>("x");
                                double[] testTime = testDataSet.GetData<double[]>("time");
                         
                            }


                    }
                }
            }

            //return gc;
            return outputFilePath;
        }

        // public JassMaccNarrGridsCombo MapFromMaccToNarr(int year, int month, string fileNameMaccTemp, string fileNameNarrTemp)
        // JassBuilder builder, int year, int month, Boolean upload, Boolean overWrite, JassBuilderLog builderAllLog
        public processGridMappingCFSRToNarrModel processGridMappingCFSRToNarr(string EnvyVariableName, int year, int month, int weeky, int day, int dayInWeeky, string fileNameMaccTemp)
        {

            processGridMappingCFSRToNarrModel result = new processGridMappingCFSRToNarrModel();
            result.variableName = EnvyVariableName;

            string fileNameMacc = replaceURIPlaceHolders(fileNameMaccTemp, year, month,weeky,day);
            string fileNameNarr = replaceURIPlaceHolders("Narr_Grid.nc", year, month,weeky,day);

            JassMaccNarrGridsCombo gc = new JassMaccNarrGridsCombo();
            string maccFile = AppDataFolder + "/" + fileNameMacc;
            string narrFile = AppFilesFolder + "/" + fileNameNarr;
            string mapFile = AppFilesFolder + "/Narr_2_CFSR_Grid_Mapper.nc";

            string smonth = (month < 10) ? "0" + month : "" + month;
            int weekyStartDay = (weeky - 1) * 5 + 1;
            int realDay = day;
            if (weeky > 0) { realDay = weekyStartDay + dayInWeeky; }

            string outputFileName = null;
            string outputFilePath = null;

            Single fillValue=Single.MaxValue;

               Dictionary<string, MetadataDictionary> vars =
                        new Dictionary<string, MetadataDictionary>();

               string VariableName=null;
    
            
            using (var narrDataSet = DataSet.Open(narrFile + "?openMode=open"))
            {
                Dictionary<string, MetadataDictionary> narrVars = new Dictionary<string, MetadataDictionary>();
                foreach (var v in narrDataSet.Variables) { narrVars.Add(v.Name, v.Metadata); }

                Single[] narrY = narrDataSet.GetData<Single[]>("y");
                Single[] narrX = narrDataSet.GetData<Single[]>("x");
                Single[, , ,] maccVariable;
                Single[, , ,] outputVariable;
                Single[, , ,] outputVariable1;
                Single[, , ,] outputVariable2;

                using (var maccDataSet = DataSet.Open(maccFile + "?openMode=open"))
                {
                    Dictionary<string, MetadataDictionary> maccVars = new Dictionary<string, MetadataDictionary>();
                    foreach (var v in maccDataSet.Variables)
                    {
                        maccVars.Add(v.Name, v.Metadata);
                        if (v.Dimensions.Count > 2 && VariableName == null)
                        {

                            VariableName = v.Name;
                            try
                            {
                                fillValue = (Single)v.Metadata["_FillValue"];
                            }
                            catch (Exception) { };
                        };
                    }

                    outputFileName = fileNameBuilderByDay(EnvyVariableName,year,month,realDay)+".nc";
                    result.outputFileName = outputFileName;
                    outputFilePath = AppDataFolder + "\\" + outputFileName;


                    Single[] maccTime = maccDataSet.GetData<Single[]>("time");
                    Single[] maccLevel = maccDataSet.GetData<Single[]>("level0");

                    double[] maccNarrTime = new double[8]; //this is two convert from 4 points a day to 8 points a day


                    DateTime day1800 = DateTime.Parse("1800-01-01 00:00:00");

  
                    DateTime maccDay;
                    DateTime narrDay;
                    double narrNumber;

                    DateTime maccDayStart = new DateTime(year, month, realDay);
                    DateTime narrDayStart = new DateTime(maccDayStart.Year, maccDayStart.Month, maccDayStart.Day);

                    if (maccDayStart.Year != year || maccDayStart.Month != month)
                    {
                        throw new Exception("maccDayStart.Year != year || maccDayStart.Month != month");
                    }

                    double narrDayStartHours = (narrDayStart - day1800).TotalHours;

                    double narrDayHours = narrDayStartHours;
                    for (int t = 0; t < 8; t++)
                    {
                        maccNarrTime[t] = narrDayHours;
                        narrDayHours += 3;
                    }


                    //At this point we have the time dimension in the variable maccNarrTime
                    //we do not need the time dimension from narr anymore.

                    using (var mapDataSet = DataSet.Open(mapFile + "?openMode=open"))
                    {

                        var narrSchema = narrDataSet.GetSchema();
                        var maccSchema = maccDataSet.GetSchema();

                        gc.narrSchema = schema2string(narrSchema);
                        gc.maccSchema = schema2string(maccSchema);

                        gc.maccLat = maccDataSet.GetData<Single[]>("lat");
                        gc.maccLon = maccDataSet.GetData<Single[]>("lon");

                        gc.narrLon = narrDataSet.GetData<Single[,]>("lon");
                        gc.narrLat = narrDataSet.GetData<Single[,]>("lat");


                        var mapDistance = mapDataSet.GetData<double[,]>("mapDistance");
                        var mapLatY = mapDataSet.GetData<int[,]>("mapLatY");
                        var mapLonX = mapDataSet.GetData<int[,]>("mapLonX");


                        var map2Distance = mapDataSet.GetData<double[,]>("map2Distance");
                        var map2LatY = mapDataSet.GetData<int[,]>("map2LatY");
                        var map2LonX = mapDataSet.GetData<int[,]>("map2LonX");


                        var map3Distance = mapDataSet.GetData<double[,]>("map3Distance");
                        var map3LatY = mapDataSet.GetData<int[,]>("map3LatY");
                        var map3LonX = mapDataSet.GetData<int[,]>("map3LonX");


                        var map4Distance = mapDataSet.GetData<double[,]>("map4Distance");
                        var map4LatY = mapDataSet.GetData<int[,]>("map4LatY");
                        var map4LonX = mapDataSet.GetData<int[,]>("map4LonX");

                        //mapp the grids

                        // gc = JassWeather.Models.JassWeatherAPI.MapGridNarr2Macc(gc);


                        for (int y = 0; y < gc.narrYMax; y++)
                        {
                            for (int x = 0; x < gc.narrXMax; x++)
                            {
                                try
                                {

                                    gc.map[y, x] = new JassGridLocation();
                                    gc.map[y, x].distance = mapDistance[y, x];
                                    gc.map[y, x].lat = mapLatY[y, x];
                                    gc.map[y, x].lon = mapLonX[y, x];
                                    gc.map[y, x].latitud = gc.maccLat[gc.map[y, x].lat];
                                    gc.map[y, x].longitud = gc.maccLon[gc.map[y, x].lon];

                                    gc.map2[y, x] = new JassGridLocation();
                                    gc.map2[y, x].distance = map2Distance[y, x];
                                    gc.map2[y, x].lat = map2LatY[y, x];
                                    gc.map2[y, x].lon = map2LonX[y, x];
                                    gc.map2[y, x].latitud = gc.maccLat[gc.map2[y, x].lat];
                                    gc.map2[y, x].longitud = gc.maccLon[gc.map2[y, x].lon];

                                    gc.map3[y, x] = new JassGridLocation();
                                    gc.map3[y, x].distance = map3Distance[y, x];
                                    gc.map3[y, x].lat = map3LatY[y, x];
                                    gc.map3[y, x].lon = map3LonX[y, x];
                                    gc.map3[y, x].latitud = gc.maccLat[gc.map3[y, x].lat];
                                    gc.map3[y, x].longitud = gc.maccLon[gc.map3[y, x].lon];

                                    gc.map4[y, x] = new JassGridLocation();
                                    gc.map4[y, x].distance = map4Distance[y, x];
                                    gc.map4[y, x].lat = map4LatY[y, x];
                                    gc.map4[y, x].lon = map4LonX[y, x];
                                    gc.map4[y, x].latitud = gc.maccLat[gc.map4[y, x].lat];
                                    gc.map4[y, x].longitud = gc.maccLon[gc.map4[y, x].lon];

                                }
                                catch (Exception e)
                                {

                                    var v = "crap";

                                }

                            }
                        }


                        ////////  getting all the dimensions from Narr
                        /* this was before
                            Single[] narrY = narrDataSet.GetData<Single[]>("y");
                            Single[] narrX = narrDataSet.GetData<Single[]>("x");
                            double[] narrTime = narrDataSet.GetData<double[]>("time");
                         */

                        GC.Collect();
                        var MemoryInitial = GC.GetTotalMemory(true);
                        startTotalTime = DateTime.Now;
                        int startTimeStep = dayInWeeky * 4;
                        int endTimeStep = startTimeStep + 3;

                        maccVariable = maccDataSet.GetData<Single[,,,]>(VariableName,
                                DataSet.Range(startTimeStep,endTimeStep),
                                DataSet.FromToEnd(0),
                                DataSet.FromToEnd(0),
                                DataSet.FromToEnd(0));


                        var MemoryAfterMacc = GC.GetTotalMemory(true);
                        startTotalTime = DateTime.Now;

                        ////filling up the array
                        outputVariable = new Single[8, maccLevel.Length, narrY.Length, narrX.Length];
                        outputVariable1 = new Single[8, maccLevel.Length, narrY.Length, narrX.Length];
                        outputVariable2 = new Single[8, maccLevel.Length, narrY.Length, narrX.Length];


                        int tt;
                        //  outputVariable[t, l, y, x] = (Int16)interpolateValueCSFR(t, l, y, x, maccVariable, gc, missingValue, fillValue);

                        Single v_mp1;
                        Single d_np_mp1;
                        bool go1;

                        Single v_mp2;
                        Single d_np_mp2;
                        bool go2; 

                        Single v_mp3;
                        Single d_np_mp3 ;
                        bool go3;

                        Single v_mp4;
                        Single d_np_mp4;
                        bool go4;

                        var MemoryAfterNarr = GC.GetTotalMemory(true);
                        TimeSpan timeSpent = new TimeSpan();

                        Single value = 0;
                        Single valueMax = 0;
                        Single valueMin = 0;

                        for (int t = 0; t < 8; t++)
                        {
                            for (int l = 0; l < maccLevel.Length; l++)
                            {
                                DateTime now = DateTime.Now;

                                for (int y = 0; y < narrY.Length; y++)
                                {

                                    for (int x = 0; x < narrX.Length; x++)
                                    {
                                       
                                        try
                                        {
                                           
                                           tt = Convert.ToInt32(t / 2);

                                         //  outputVariable[t, l, y, x] = (Int16)interpolateValueCSFR(t, l, y, x, maccVariable, gc, missingValue, fillValue);
                                           if (tt == t/2)
                                           {
                                               v_mp1 = maccVariable[tt, l, gc.map[y, x].lat, gc.map[y, x].lon];
                                               d_np_mp1 = (Single)gc.map[y, x].distance;
                                               go1 = (v_mp1 == fillValue) ? false : true;

                                               v_mp2 = maccVariable[tt, l, gc.map2[y, x].lat, gc.map2[y, x].lon];
                                               d_np_mp2 = (Single)gc.map2[y, x].distance;
                                               go2 = (v_mp2 == fillValue) ? false : true;

                                               v_mp3 = maccVariable[tt, l, gc.map3[y, x].lat, gc.map3[y, x].lon];
                                               d_np_mp3 = (Single)gc.map3[y, x].distance;
                                               go3 = (v_mp3 == fillValue) ? false : true;

                                               v_mp4 = maccVariable[tt, l, gc.map4[y, x].lat, gc.map4[y, x].lon];
                                               d_np_mp4 = (Single)gc.map4[y, x].distance;
                                               go4 = (v_mp4 == fillValue) ? false : true;

                                               if (y == 133 && x == 241)
                                               {
                                                   var xxx = 1;
                                                   var yyy = xxx + 1;

                                               }

                                               if ( EnvyVariableName.Contains("2") || d_np_mp1 < 10 )

                                               { 
                                                   value = v_mp1; }
                                               else
                                               {
                                                   value = ((go1?v_mp1 / d_np_mp1:0) +
                                                            (go2?v_mp2 / d_np_mp2:0) +
                                                            (go3?v_mp3 / d_np_mp3:0) +
                                                            (go4?v_mp4 / d_np_mp4:0)) /

                                                            ((go1? 1/d_np_mp1:0) +
                                                            (go2? 1/d_np_mp2:0) +
                                                            (go3? 1/d_np_mp3:0) +
                                                            (go4? 1/d_np_mp4:0));
                                               };

                                               outputVariable[t, l, y, x] = value;
                                               outputVariable1[t, l, y, x] = v_mp1;
                                               outputVariable2[t, l, y, x] = v_mp2;
                                               if (value > valueMax) valueMax = value;
                                               if (value < valueMin) valueMin = value;
                                           }
                                           else {
                                               outputVariable[t, l, y, x] = outputVariable[t-1, l, y, x];
                                           
                                           }
                                        }
                                        catch (Exception e)
                                        {
                                            var dosomething = 1;
                                        }

                                    }

                                }
                           }
                        }
                        /////// Writting results into file
                        //   dataset3.Add<Int16[, ,]>(builder.JassVariable.Name, dataset, "time", "y", "x");

                        //we will enter year/month as parameter





                            //    outputDataSet.Add<double[]>("time", maccNarrTime, "time");
                           //     foreach (var attr in narrVars["time"]) { if (attr.Key != "Name") outputDataSet.PutAttr("time", attr.Key, attr.Value); }

                                result.time = maccNarrTime;

                           //     outputDataSet.Add<Single[]>("level", maccLevel, "level");
                           //     foreach (var attr in maccVars["level0"]) { if (attr.Key != "Name") outputDataSet.PutAttr("level", attr.Key, attr.Value); }

                                result.level = maccLevel;

                           //     outputDataSet.Add<Single[]>("y", narrY, "y");
                           //     foreach (var attr in narrVars["y"]) { if (attr.Key != "Name") outputDataSet.PutAttr("y", attr.Key, attr.Value); }

                                result.y = narrY;

                          //      outputDataSet.Add<Single[]>("x", narrX, "x");
                          //      foreach (var attr in narrVars["y"]) { if (attr.Key != "Name") outputDataSet.PutAttr("x", attr.Key, attr.Value); }

                                result.x = narrX;

                                result.Variable = outputVariable;

                          //      outputDataSet.Add<Single[, , ,]>(VariableName,"time", "level", "y", "x");
                          //      outputDataSet.PutData<Single[,,,]>(VariableName, outputVariable);
                          //      outputDataSet.Commit();
/*
                                foreach (var attr in maccVars[VariableName])
                                {
                                    if (attr.Key != "Name")
                                    {
                                        if (attr.Key != "_FillValue")
                                        {
                                            outputDataSet.PutAttr(VariableName, attr.Key, attr.Value);
                                        }
                                        else
                                        {
                                            outputDataSet.PutAttr(VariableName, "FillValue", attr.Value);
                                        }
                                    }
                                }
 * */





                    }
                }
            }
            result.fillValue = fillValue;
            result.outputFilePath = outputFilePath;
            return result;
        }

        public string saveprocessGridMappingCFSRToNarrModel(processGridMappingCFSRToNarrModel model){

            using (var outputDataSet = DataSet.Open(model.outputFilePath + "?openMode=create"))
            {
                try {

                    outputDataSet.Add<double[]>("time", model.time, "time");
                    //     foreach (var attr in narrVars["time"]) { if (attr.Key != "Name") outputDataSet.PutAttr("time", attr.Key, attr.Value); }
                    outputDataSet.Add<Single[]>("level", model.level, "level");
                    //     foreach (var attr in maccVars["level0"]) { if (attr.Key != "Name") outputDataSet.PutAttr("level", attr.Key, attr.Value); }
                    outputDataSet.Add<Single[]>("y", model.y, "y");
                    //     foreach (var attr in narrVars["y"]) { if (attr.Key != "Name") outputDataSet.PutAttr("y", attr.Key, attr.Value); }
                    outputDataSet.Add<Single[]>("x", model.x, "x");
                    //      foreach (var attr in narrVars["y"]) { if (attr.Key != "Name") outputDataSet.PutAttr("x", attr.Key, attr.Value); }
                   
                    outputDataSet.Add<Single[, , ,]>(model.variableName,model.Variable,"time", "level", "y", "x");
                    outputDataSet.Add<Single[, , ,]>(model.variableName, model.Variable + "1", "time", "level", "y", "x");
                    outputDataSet.Add<Single[, , ,]>(model.variableName, model.Variable + "2", "time", "level", "y", "x");

                    outputDataSet.PutAttr(model.variableName, "FillValue", model.fillValue);              
                
                
                
                }
                catch (Exception e)
                {
                    var dosomethinf = 1;
                }
            }

/*
            //now let's test 
            using (var testDataSet = DataSet.Open(outputFilePath + "?openMode=open"))
            {

                Int16[, ,] testVariable = testDataSet.GetData<Int16[, ,]>(VariableName,
                        DataSet.FromToEnd(0),
                        DataSet.FromToEnd(0),
                        DataSet.FromToEnd(0));

                Single[] testY = testDataSet.GetData<Single[]>("y");
                Single[] testX = testDataSet.GetData<Single[]>("x");
                double[] testTime = testDataSet.GetData<double[]>("time");

            }

*/
            return model.outputFileName;
        }


        public class processGridMappingCFSRToNarrModel {

            public string outputFilePath { get; set; }
            public string outputFileName { get; set; }
            public string variableName { get; set; }
            public Single fillValue { get; set; }
            public double[] time { get; set; }
            public Single[] level { get; set; }
            public Single[] y { get; set; }
            public Single[] x { get; set; }
            public Single[,,,] Variable { get; set; }
            public Single[, , ,] Variable1 { get; set; }
            public Single[, , ,] Variable2 { get; set; }
    
        }

        public string napsCreateNETCDFfromTextFile(int year, int month, int weeky, string napsDataFile){
        
        //This method will read the NAPS file and create an equivalente netCDF that will be used as input to the next process
            //the idea is to use a method pretty similar to sheridan.
            //so the first part of to 'read the history' and create a model... the second part is about savoing it as netCDf

            NapsInfoModel model = new NapsInfoModel();

            string napsDataFilePath = napsDataFile;
            string[] lines = System.IO.File.ReadAllLines(napsDataFilePath);

            for (int l = 0; l < lines.Length; l++)
            {
                //process each line from the file
                string line = lines[l];

//pollutant code	3	1	3
                string pollutantCode = line.Substring(0, 3);
//station (NAPS id)	6	4	9
                string stationNAPSId = line.Substring(3, 6);
//Year	4	10	13
                string Year = line.Substring(9, 4);
//Month	2	14	15
                string Month = line.Substring(13, 2);
//Day	2	16	17
                string Day = line.Substring(15, 2);
//average for day	4	18	21
                string AverageDay = line.Substring(17, 4);
//minimum for day	4	22	25
                string MinimunDay = line.Substring(21, 4);
//maximum for day	4	26	29
                string MaximunDay = line.Substring(26, 4);
                string[] HourlyReading = new string[24];

;//hourly reading 1	4	30	33
for (int h = 0; h < 24; h++) {
    HourlyReading[h] = line.Substring(29+h*4, 4);
}

            }


                napsSaveHistory(model);
            return "ok";
        }

        public string processGridMappingNAPSToNarr(int year, int month, int weeky, string fileNameMaccTemp)
        {

            var napsFileName = napsCreateNETCDFfromTextFile(year, month, weeky, fileNameMaccTemp);

            //here we are going to read the text file and convert it to netCDF format for that 

            string fileNameMacc = fileNameMaccTemp;
            string fileNameNarr = replaceURIPlaceHolders("Narr_Grid.nc", year, month, weeky, 0);

            JassMaccNarrGridsCombo gc = new JassMaccNarrGridsCombo();
            string sherFile = AppDataFolder + "/" + fileNameMacc;
            string narrFile = AppFilesFolder + "/" + fileNameNarr;
            string mapFile = AppFilesFolder + "/Narr_2_SHER_Grid_Mapper.nc";

            string smonth = (month < 10) ? "0" + month : "" + month;
            string outputFileName = null;
            string outputFilePath = null;

            Int16 missingValue = -32767;
            Int16 fillValue = -32767;

            string VariableName = "sher";
            Dictionary<string, MetadataDictionary> vars =
                        new Dictionary<string, MetadataDictionary>();


            Single[] narrY = null;
            Single[] narrX = null;
            double[] narrTime = null;
            Dictionary<string, MetadataDictionary> narrVars = new Dictionary<string, MetadataDictionary>();

            //getting stuff from narr dataset
            using (var narrDataSet = DataSet.Open(narrFile + "?openMode=open"))
            {
                foreach (var v in narrDataSet.Variables) { narrVars.Add(v.Name, v.Metadata); }

                narrY = narrDataSet.GetData<Single[]>("y");
                narrX = narrDataSet.GetData<Single[]>("x");
                //   narrTime = narrDataSet.GetData<double[]>("time");

                var narrSchema = narrDataSet.GetSchema();
                gc.narrLon = narrDataSet.GetData<Single[,]>("lon");
                gc.narrLat = narrDataSet.GetData<Single[,]>("lat");

                gc.narrSchema = schema2string(narrSchema);
            }
            //getting stuff from input info in inpout grids

            double[] sherTime = null;
            Int16[,] sherVariable = null;

            int yearDiff = year - 2002;
            DateTime startRelevantHistoryDay = DateTime.Parse("2002-01-01");

            DateTime startingDay = new DateTime(year, month, 1);
            DateTime endingDay = startingDay.AddMonths(1);

            int startingDayIndex = (int)(startingDay - startRelevantHistoryDay).TotalDays;
            int endingDayIndex = (int)(endingDay - startRelevantHistoryDay).TotalDays;

            Dictionary<string, MetadataDictionary> sherVars = new Dictionary<string, MetadataDictionary>();
            using (var sherDataSet = DataSet.Open(sherFile + "?openMode=open"))
            {
                foreach (var v in sherDataSet.Variables)
                {
                    sherVars.Add(v.Name, v.Metadata);
                    if (v.Dimensions.Count > 2 && VariableName == null)
                    {
                        VariableName = v.Name;
                        try
                        {
                            missingValue = (Int16)v.Metadata["missing_value"];
                            fillValue = (Int16)v.Metadata["_FillValue"];
                        }
                        catch (Exception) { };
                    };
                }



                sherTime = sherDataSet.GetData<double[]>("time", DataSet.Range(startingDayIndex, endingDayIndex - 1));
                var sherSchema = sherDataSet.GetSchema();

                gc.maccSchema = schema2string(sherSchema);

                gc.maccLat = sherDataSet.GetData<Single[]>("lat");
                gc.maccLon = sherDataSet.GetData<Single[]>("lon");

                //so here is where we will calculate exactly how manby days I need.
                //I need a day starting index 2002-01-01 is 0   and a day ending index.



                sherVariable = sherDataSet.GetData<Int16[,]>(VariableName,
                    DataSet.Range(startingDayIndex, endingDayIndex - 1),
                    DataSet.FromToEnd(0));
            }

            outputFileName = VariableName + "_sher2narr_" + year + "_" + smonth + ".nc";
            outputFilePath = AppDataFolder + "\\" + outputFileName;


            double[] sherNarrTime = new double[sherTime.Length * 8];

            DateTime day1800 = DateTime.Parse("1800-01-01 00:00:00");

            DateTime sherDay;
            DateTime narrDay;
            double narrNumber;

            DateTime sherDayStart = startRelevantHistoryDay;
            DateTime narrDayStart = new DateTime(sherDayStart.Year, sherDayStart.Month, sherDayStart.Day);



            double narrDayStartHours = (narrDayStart - day1800).TotalHours;

            double narrDayHours = narrDayStartHours;
            for (int t = 0; t < sherNarrTime.Length; t++)
            {
                sherNarrTime[t] = narrDayHours;
                narrDayHours += 3;
            }

            //At this point we have the time dimension in the variable sherNarrTime
            //we do not need the time dimension from narr anymore.

            using (var mapDataSet = DataSet.Open(mapFile + "?openMode=open"))
            {

                var mapDistance = mapDataSet.GetData<double[,]>("mapDistance");
                var mapLatY = mapDataSet.GetData<int[,]>("mapLatY");
                var mapLonX = mapDataSet.GetData<int[,]>("mapLonX");


                var map2Distance = mapDataSet.GetData<double[,]>("map2Distance");
                var map2LatY = mapDataSet.GetData<int[,]>("map2LatY");
                var map2LonX = mapDataSet.GetData<int[,]>("map2LonX");


                var map3Distance = mapDataSet.GetData<double[,]>("map3Distance");
                var map3LatY = mapDataSet.GetData<int[,]>("map3LatY");
                var map3LonX = mapDataSet.GetData<int[,]>("map3LonX");


                var map4Distance = mapDataSet.GetData<double[,]>("map4Distance");
                var map4LatY = mapDataSet.GetData<int[,]>("map4LatY");
                var map4LonX = mapDataSet.GetData<int[,]>("map4LonX");

                //mapp the grids

                // gc = JassWeather.Models.JassWeatherAPI.MapGridNarr2Macc(gc);


                for (int y = 0; y < gc.narrYMax; y++)
                {
                    for (int x = 0; x < gc.narrXMax; x++)
                    {
                        try
                        {
                            if (mapLatY[y, x] < 469)  //quick hack due to problem
                            {
                                gc.map[y, x] = new JassGridLocation();
                                gc.map[y, x].distance = mapDistance[y, x];
                                gc.map[y, x].lat = mapLatY[y, x];
                                gc.map[y, x].lon = mapLonX[y, x];
                                gc.map[y, x].latitud = gc.maccLat[gc.map[y, x].lat];
                                gc.map[y, x].longitud = gc.maccLon[gc.map[y, x].lon];
                            }

                            if (map2LatY[y, x] < 469)  //quick hack due to problem
                            {
                                gc.map2[y, x] = new JassGridLocation();
                                gc.map2[y, x].distance = map2Distance[y, x];
                                gc.map2[y, x].lat = map2LatY[y, x];
                                gc.map2[y, x].lon = map2LonX[y, x];
                                gc.map2[y, x].latitud = gc.maccLat[gc.map2[y, x].lat];
                                gc.map2[y, x].longitud = gc.maccLon[gc.map2[y, x].lon];
                            }

                            if (map3LatY[y, x] < 469)  //quick hack due to problem
                            {
                                gc.map3[y, x] = new JassGridLocation();
                                gc.map3[y, x].distance = map3Distance[y, x];
                                gc.map3[y, x].lat = map3LatY[y, x];
                                gc.map3[y, x].lon = map3LonX[y, x];
                                gc.map3[y, x].latitud = gc.maccLat[gc.map3[y, x].lat];
                                gc.map3[y, x].longitud = gc.maccLon[gc.map3[y, x].lon];
                            }

                            if (map4LatY[y, x] < 469)  //quick hack due to problem
                            {
                                gc.map4[y, x] = new JassGridLocation();
                                gc.map4[y, x].distance = map4Distance[y, x];
                                gc.map4[y, x].lat = map4LatY[y, x];
                                gc.map4[y, x].lon = map4LonX[y, x];
                                gc.map4[y, x].latitud = gc.maccLat[gc.map4[y, x].lat];
                                gc.map4[y, x].longitud = gc.maccLon[gc.map4[y, x].lon];
                            }
                        }
                        catch (Exception e)
                        {

                            var v = "crap";

                        }

                    }
                }


                //Ok, now let's process the file converting from Macc to Narr at the measure level.


                ////////  getting all the dimensions from Narr
                /* this was before
                    Single[] narrY = narrDataSet.GetData<Single[]>("y");
                    Single[] narrX = narrDataSet.GetData<Single[]>("x");
                    double[] narrTime = narrDataSet.GetData<double[]>("time");
                 */

                ////filling up the array
                Int16[, ,] outputVariable = new Int16[sherNarrTime.Length, narrY.Length, narrX.Length];

                for (int t = 0; t < sherNarrTime.Length; t++)
                {
                    for (int y = 0; y < narrY.Length; y++)
                    {
                        for (int x = 0; x < narrX.Length; x++)
                        {
                            try
                            {
                                outputVariable[t, y, x] = (Int16)interpolateValueSher(t, y, x, sherVariable, gc, missingValue, fillValue);
                            }
                            catch (Exception e)
                            {
                                var dosomething = 1;
                            }
                        }
                    }
                }
                /////// Writting results into file
                //   dataset3.Add<Int16[, ,]>(builder.JassVariable.Name, dataset, "time", "y", "x");

                //we will enter year/month as parameter

                using (var outputDataSet = DataSet.Open(outputFilePath + "?openMode=create"))
                {
                    /*
                     * 
    Single[] narrY = narrDataSet.GetData<Single[]>("y");
    Single[] narrX = narrDataSet.GetData<Single[]>("x");
    double[] narrTime = narrDataSet.GetData<double[]>("time");
                     */


                    outputDataSet.Add<double[]>("time", sherNarrTime, "time");
                    //foreach (var attr in narrVars["time"]) { if (attr.Key != "Name") outputDataSet.PutAttr("time", attr.Key, attr.Value); }
                    outputDataSet.Add<Single[]>("y", narrY, "y");
                    foreach (var attr in narrVars["y"]) { if (attr.Key != "Name") outputDataSet.PutAttr("y", attr.Key, attr.Value); }
                    outputDataSet.Add<Single[]>("x", narrX, "x");
                    foreach (var attr in narrVars["y"]) { if (attr.Key != "Name") outputDataSet.PutAttr("x", attr.Key, attr.Value); }
                    outputDataSet.Add<Int16[, ,]>(VariableName, outputVariable, "time", "y", "x");
                    foreach (var attr in sherVars[VariableName])
                    {
                        if (attr.Key != "Name")
                        {
                            if (attr.Key != "_FillValue")
                            {
                                outputDataSet.PutAttr(VariableName, attr.Key, attr.Value);
                            }
                            else
                            {
                                outputDataSet.PutAttr(VariableName, "FillValue", attr.Value);
                            }
                        }
                    }

                }


                //now let's test 
                using (var testDataSet = DataSet.Open(outputFilePath + "?openMode=open"))
                {

                    Int16[, ,] testVariable = testDataSet.GetData<Int16[, ,]>(VariableName,
                            DataSet.FromToEnd(0),
                            DataSet.FromToEnd(0),
                            DataSet.FromToEnd(0));

                    Single[] testY = testDataSet.GetData<Single[]>("y");
                    Single[] testX = testDataSet.GetData<Single[]>("x");
                    double[] testTime = testDataSet.GetData<double[]>("time");

                }
            }

            //return gc;
            return outputFilePath;
        }



        public string processGridMappingSHERToNarr(int year, int month, int weeky, string fileNameMaccTemp)
        {
            string fileNameMacc = replaceURIPlaceHolders(fileNameMaccTemp, year, month,weeky,0);
            string fileNameNarr = replaceURIPlaceHolders("Narr_Grid.nc", year, month,weeky,0);

            JassMaccNarrGridsCombo gc = new JassMaccNarrGridsCombo();
            string sherFile = AppDataFolder + "/" + fileNameMacc;
            string narrFile = AppFilesFolder + "/" + fileNameNarr;
            string mapFile = AppFilesFolder + "/Narr_2_SHER_Grid_Mapper.nc";

            string smonth = (month < 10) ? "0" + month : "" + month;
            string outputFileName = null;
            string outputFilePath = null;

            Int16 missingValue = -32767;
            Int16 fillValue = -32767;

            string VariableName = "sher";
            Dictionary<string, MetadataDictionary> vars =
                        new Dictionary<string, MetadataDictionary>();


            Single[] narrY = null;
            Single[] narrX = null;
            double[] narrTime = null;
            Dictionary<string, MetadataDictionary> narrVars = new Dictionary<string, MetadataDictionary>();

            //getting stuff from narr dataset
            using (var narrDataSet = DataSet.Open(narrFile + "?openMode=open"))
            {               
                foreach (var v in narrDataSet.Variables) { narrVars.Add(v.Name, v.Metadata); }

                narrY = narrDataSet.GetData<Single[]>("y");
                narrX = narrDataSet.GetData<Single[]>("x");
             //   narrTime = narrDataSet.GetData<double[]>("time");

                var narrSchema = narrDataSet.GetSchema();
                gc.narrLon = narrDataSet.GetData<Single[,]>("lon");
                gc.narrLat = narrDataSet.GetData<Single[,]>("lat");

                gc.narrSchema = schema2string(narrSchema);
            }
            //getting stuff from input info in inpout grids

            double[] sherTime = null;
            Int16[,] sherVariable = null;

            int yearDiff = year - 2002;
            DateTime startRelevantHistoryDay = DateTime.Parse("2002-01-01");

            DateTime startingDay = new DateTime(year, month, 1);
            DateTime endingDay = startingDay.AddMonths(1);

            int startingDayIndex = (int)(startingDay - startRelevantHistoryDay).TotalDays;
            int endingDayIndex = (int)(endingDay - startRelevantHistoryDay).TotalDays;

            Dictionary<string, MetadataDictionary> sherVars = new Dictionary<string, MetadataDictionary>();
            using (var sherDataSet = DataSet.Open(sherFile + "?openMode=open"))
            {
                foreach (var v in sherDataSet.Variables)
                {
                    sherVars.Add(v.Name, v.Metadata);
                    if (v.Dimensions.Count > 2 && VariableName == null)
                    {
                        VariableName = v.Name;
                        try
                        {
                            missingValue = (Int16)v.Metadata["missing_value"];
                            fillValue = (Int16)v.Metadata["_FillValue"];
                        }
                        catch (Exception) { };
                    };
                }



                sherTime = sherDataSet.GetData<double[]>("time",  DataSet.Range(startingDayIndex,endingDayIndex-1));
                var sherSchema = sherDataSet.GetSchema();

                gc.maccSchema = schema2string(sherSchema);

                gc.maccLat = sherDataSet.GetData<Single[]>("lat");
                gc.maccLon = sherDataSet.GetData<Single[]>("lon");

                //so here is where we will calculate exactly how manby days I need.
                //I need a day starting index 2002-01-01 is 0   and a day ending index.



                sherVariable = sherDataSet.GetData<Int16[,]>(VariableName,
                    DataSet.Range(startingDayIndex,endingDayIndex-1), 
                    DataSet.FromToEnd(0));
            }

                    outputFileName = VariableName + "_sher2narr_" + year + "_" + smonth + ".nc";
                    outputFilePath = AppDataFolder + "\\" + outputFileName;
                  

                    double[] sherNarrTime = new double[sherTime.Length*8];

                    DateTime day1800 = DateTime.Parse("1800-01-01 00:00:00");

                    DateTime sherDay;
                    DateTime narrDay;
                    double narrNumber;

                    DateTime sherDayStart = startRelevantHistoryDay;
                    DateTime narrDayStart = new DateTime(sherDayStart.Year, sherDayStart.Month, sherDayStart.Day);

               

                    double narrDayStartHours = (narrDayStart - day1800).TotalHours;

                    double narrDayHours = narrDayStartHours;
                    for (int t = 0; t < sherNarrTime.Length; t++)
                    {
                        sherNarrTime[t] = narrDayHours;
                        narrDayHours += 3;
                    }

                    //At this point we have the time dimension in the variable sherNarrTime
                    //we do not need the time dimension from narr anymore.

                    using (var mapDataSet = DataSet.Open(mapFile + "?openMode=open"))
                    {

                        var mapDistance = mapDataSet.GetData<double[,]>("mapDistance");
                        var mapLatY = mapDataSet.GetData<int[,]>("mapLatY");
                        var mapLonX = mapDataSet.GetData<int[,]>("mapLonX");


                        var map2Distance = mapDataSet.GetData<double[,]>("map2Distance");
                        var map2LatY = mapDataSet.GetData<int[,]>("map2LatY");
                        var map2LonX = mapDataSet.GetData<int[,]>("map2LonX");


                        var map3Distance = mapDataSet.GetData<double[,]>("map3Distance");
                        var map3LatY = mapDataSet.GetData<int[,]>("map3LatY");
                        var map3LonX = mapDataSet.GetData<int[,]>("map3LonX");


                        var map4Distance = mapDataSet.GetData<double[,]>("map4Distance");
                        var map4LatY = mapDataSet.GetData<int[,]>("map4LatY");
                        var map4LonX = mapDataSet.GetData<int[,]>("map4LonX");

                        //mapp the grids

                        // gc = JassWeather.Models.JassWeatherAPI.MapGridNarr2Macc(gc);


                        for (int y = 0; y < gc.narrYMax; y++)
                        {
                            for (int x = 0; x < gc.narrXMax; x++)
                            {
                                try
                                {
                                    if (mapLatY[y, x] < 469)  //quick hack due to problem
                                    {
                                        gc.map[y, x] = new JassGridLocation();
                                        gc.map[y, x].distance = mapDistance[y, x];
                                        gc.map[y, x].lat = mapLatY[y, x];
                                        gc.map[y, x].lon = mapLonX[y, x];
                                        gc.map[y, x].latitud = gc.maccLat[gc.map[y, x].lat];
                                        gc.map[y, x].longitud = gc.maccLon[gc.map[y, x].lon];
                                    }

                                    if (map2LatY[y, x] < 469)  //quick hack due to problem
                                    {
                                        gc.map2[y, x] = new JassGridLocation();
                                        gc.map2[y, x].distance = map2Distance[y, x];
                                        gc.map2[y, x].lat = map2LatY[y, x];
                                        gc.map2[y, x].lon = map2LonX[y, x];
                                        gc.map2[y, x].latitud = gc.maccLat[gc.map2[y, x].lat];
                                        gc.map2[y, x].longitud = gc.maccLon[gc.map2[y, x].lon];
                                    }

                                    if (map3LatY[y, x] < 469)  //quick hack due to problem
                                    {
                                        gc.map3[y, x] = new JassGridLocation();
                                        gc.map3[y, x].distance = map3Distance[y, x];
                                        gc.map3[y, x].lat = map3LatY[y, x];
                                        gc.map3[y, x].lon = map3LonX[y, x];
                                        gc.map3[y, x].latitud = gc.maccLat[gc.map3[y, x].lat];
                                        gc.map3[y, x].longitud = gc.maccLon[gc.map3[y, x].lon];
                                    }

                                    if (map4LatY[y, x] < 469)  //quick hack due to problem
                                    {
                                        gc.map4[y, x] = new JassGridLocation();
                                        gc.map4[y, x].distance = map4Distance[y, x];
                                        gc.map4[y, x].lat = map4LatY[y, x];
                                        gc.map4[y, x].lon = map4LonX[y, x];
                                        gc.map4[y, x].latitud = gc.maccLat[gc.map4[y, x].lat];
                                        gc.map4[y, x].longitud = gc.maccLon[gc.map4[y, x].lon];
                                    }
                                }
                                catch (Exception e)
                                {

                                    var v = "crap";

                                }

                            }
                        }


                        //Ok, now let's process the file converting from Macc to Narr at the measure level.


                        ////////  getting all the dimensions from Narr
                        /* this was before
                            Single[] narrY = narrDataSet.GetData<Single[]>("y");
                            Single[] narrX = narrDataSet.GetData<Single[]>("x");
                            double[] narrTime = narrDataSet.GetData<double[]>("time");
                         */

                        ////filling up the array
                        Int16[, ,] outputVariable = new Int16[sherNarrTime.Length, narrY.Length, narrX.Length];

                        for (int t = 0; t < sherNarrTime.Length; t++)
                        {
                            for (int y = 0; y < narrY.Length; y++)
                            {
                                for (int x = 0; x < narrX.Length; x++)
                                {
                                    try
                                    {
                                        outputVariable[t, y, x] = (Int16)interpolateValueSher(t, y, x, sherVariable, gc, missingValue, fillValue);
                                    }
                                    catch (Exception e)
                                    {
                                        var dosomething = 1;
                                    }
                                }
                            }
                        }
                        /////// Writting results into file
                        //   dataset3.Add<Int16[, ,]>(builder.JassVariable.Name, dataset, "time", "y", "x");

                        //we will enter year/month as parameter

                        using (var outputDataSet = DataSet.Open(outputFilePath + "?openMode=create"))
                        {
                            /*
                             * 
            Single[] narrY = narrDataSet.GetData<Single[]>("y");
            Single[] narrX = narrDataSet.GetData<Single[]>("x");
            double[] narrTime = narrDataSet.GetData<double[]>("time");
                             */


                            outputDataSet.Add<double[]>("time", sherNarrTime, "time");
                            //foreach (var attr in narrVars["time"]) { if (attr.Key != "Name") outputDataSet.PutAttr("time", attr.Key, attr.Value); }
                            outputDataSet.Add<Single[]>("y", narrY, "y");
                            foreach (var attr in narrVars["y"]) { if (attr.Key != "Name") outputDataSet.PutAttr("y", attr.Key, attr.Value); }
                            outputDataSet.Add<Single[]>("x", narrX, "x");
                            foreach (var attr in narrVars["y"]) { if (attr.Key != "Name") outputDataSet.PutAttr("x", attr.Key, attr.Value); }
                            outputDataSet.Add<Int16[, ,]>(VariableName, outputVariable, "time", "y", "x");
                            foreach (var attr in sherVars[VariableName])
                            {
                                if (attr.Key != "Name")
                                {
                                    if (attr.Key != "_FillValue")
                                    {
                                        outputDataSet.PutAttr(VariableName, attr.Key, attr.Value);
                                    }
                                    else
                                    {
                                        outputDataSet.PutAttr(VariableName, "FillValue", attr.Value);
                                    }
                                }
                            }

                        }


                        //now let's test 
                        using (var testDataSet = DataSet.Open(outputFilePath + "?openMode=open"))
                        {

                            Int16[, ,] testVariable = testDataSet.GetData<Int16[, ,]>(VariableName,
                                    DataSet.FromToEnd(0),
                                    DataSet.FromToEnd(0),
                                    DataSet.FromToEnd(0));

                            Single[] testY = testDataSet.GetData<Single[]>("y");
                            Single[] testX = testDataSet.GetData<Single[]>("x");
                            double[] testTime = testDataSet.GetData<double[]>("time");

                        }
                    }

            //return gc;
            return outputFilePath;
        }

        public class SmartGridMap
        {
            public double maxDistance = 0;
            public int maxX = 0;
            public int maxY = 0;
            
            public double[,] mapDistance;
            public int[,] mapLatY;
            public int[,] mapLonX;

            public double[,] map2Distance;
            public int[,] map2LatY;
            public int[,] map2LonX;

            public double[,] map3Distance;
            public int[,] map3LatY;
            public int[,] map3LonX;

            public double[,] map4Distance;
            public int[,] map4LatY;
            public int[,] map4LonX;

            public JassLatLon JassLatLon { get; set; }
            public int JassLatLonID { get; set; }
            public string fileMapper { get; set; }

            public Single[] maccLat { get; set; }
            public Single[] maccLon { get; set; }

            public Single[,] narrLat { get; set; }
            public Single[,] narrLon { get; set; }

        }

        public SmartGridMap getMapComboFromMapFile(string mapfile)
        {
            string mapFilePath = AppFilesFolder + "/" + mapfile;
            SmartGridMap sgm = new SmartGridMap();

            using (var mapDataSet = DataSet.Open(mapFilePath + "?openMode=open"))
            {

                sgm.maccLat = mapDataSet.GetData<Single[]>("maccLat");
                sgm.maccLon = mapDataSet.GetData<Single[]>("maccLon");

                sgm.narrLat = mapDataSet.GetData<Single[,]>("narrLat");
                sgm.narrLon = mapDataSet.GetData<Single[,]>("narrLon");

                sgm.mapLatY = mapDataSet.GetData<int[,]>("mapLatY");
                sgm.mapLonX = mapDataSet.GetData<int[,]>("mapLonX");

                sgm.mapDistance = mapDataSet.GetData<double[,]>("mapDistance");
                sgm.mapLatY = mapDataSet.GetData<int[,]>("mapLatY");
                sgm.mapLonX = mapDataSet.GetData<int[,]>("mapLonX");


                sgm.map2Distance = mapDataSet.GetData<double[,]>("map2Distance");
                sgm.map2LatY = mapDataSet.GetData<int[,]>("map2LatY");
                sgm.map2LonX = mapDataSet.GetData<int[,]>("map2LonX");


                sgm.map3Distance = mapDataSet.GetData<double[,]>("map3Distance");
                sgm.map3LatY = mapDataSet.GetData<int[,]>("map3LatY");
                sgm.map3LonX = mapDataSet.GetData<int[,]>("map3LonX");


                sgm.map4Distance = mapDataSet.GetData<double[,]>("map4Distance");
                sgm.map4LatY = mapDataSet.GetData<int[,]>("map4LatY");
                sgm.map4LonX = mapDataSet.GetData<int[,]>("map4LonX");


                for (int y = 0; y < 277; y++)
                {
                    for (int x = 0; x < 349; x++)
                    {
                        var mapDistance = sgm.mapDistance[y,x];
                        if (mapDistance > sgm.maxDistance)
                        {
                            sgm.maxY = y;
                            sgm.maxX = x;
                            sgm.maxDistance = mapDistance;
                        }
                        var mapLatY = sgm.mapLatY[y, x];
                        var mapLonX = sgm.mapLonX[y, x];
                    }
                }


            }//using file

            return sgm;
      }//end function

        public double interpolateValue(int t, int y, int x, Int16[,,] maccValues, JassMaccNarrGridsCombo gc, Int16 missValue, Int16 fillValue)
        {

            /*
             *  
              v(mp1) / d(np, mp1)  + v(mp2) / d(np, mp2) + v(mp3) / d(np, mp3)        
v(np)  =   ------------------------------------------------------------------------------------
               1 / d(np, mp1)  + 1 / d(np, mp2) + 1 / d(np, mp3)
             */

            Int16 v_mp1 = maccValues[t, gc.map[y, x].lat, gc.map[y, x].lon];
            double d_np_mp1 = gc.map[y, x].distance;
            int go1 = (v_mp1 == missValue || v_mp1 == missValue) ? 0 : 1; 

            Int16 v_mp2 = maccValues[t, gc.map2[y, x].lat, gc.map2[y, x].lon];
            double d_np_mp2 = gc.map2[y, x].distance;
            int go2 = (v_mp2 == missValue || v_mp2 == missValue) ? 0 : 1; 

            Int16 v_mp3 = maccValues[t, gc.map3[y, x].lat, gc.map3[y, x].lon];
            double d_np_mp3 = gc.map3[y, x].distance;
            int go3 = (v_mp3 == missValue || v_mp3 == missValue) ? 0 : 1; 

            Int16 v_mp4 = maccValues[t, gc.map4[y, x].lat, gc.map4[y, x].lon];
            double d_np_mp4 = gc.map4[y, x].distance;
            int go4 = (v_mp4 == missValue || v_mp4 == missValue) ? 0 : 1; 

            double value;

            if (d_np_mp1 < 10)

                { value = v_mp1;}
            else 
                {
                    value = (go1 * v_mp1 / d_np_mp1 + 
                             go2 * v_mp2 / d_np_mp2 + 
                             go3 * v_mp3 / d_np_mp3 + 
                             go4 * v_mp4 / d_np_mp4) /
                            
                            (go1 / d_np_mp1 + 
                             go2 / d_np_mp2 + 
                             go3 / d_np_mp3 + 
                             go4 / d_np_mp4);
                };

            return value;

        }

        public double interpolateValueCSFR(int t, int l, int y, int x, Single[, , ,] maccValues, JassMaccNarrGridsCombo gc, Int16 missValue, Int16 fillValue)
        {

            /*
             *  
              v(mp1) / d(np, mp1)  + v(mp2) / d(np, mp2) + v(mp3) / d(np, mp3)        
v(np)  =   ------------------------------------------------------------------------------------
               1 / d(np, mp1)  + 1 / d(np, mp2) + 1 / d(np, mp3)
             */

            Single v_mp1 = maccValues[t, l, gc.map[y, x].lat, gc.map[y, x].lon];
            double d_np_mp1 = gc.map[y, x].distance;
            int go1 = (v_mp1 == missValue || v_mp1 == missValue) ? 0 : 1;

            Single v_mp2 = maccValues[t, l, gc.map2[y, x].lat, gc.map2[y, x].lon];
            double d_np_mp2 = gc.map2[y, x].distance;
            int go2 = (v_mp2 == missValue || v_mp2 == missValue) ? 0 : 1;

            Single v_mp3 = maccValues[t, l, gc.map3[y, x].lat, gc.map3[y, x].lon];
            double d_np_mp3 = gc.map3[y, x].distance;
            int go3 = (v_mp3 == missValue || v_mp3 == missValue) ? 0 : 1;

            Single v_mp4 = maccValues[t, l, gc.map4[y, x].lat, gc.map4[y, x].lon];
            double d_np_mp4 = gc.map4[y, x].distance;
            int go4 = (v_mp4 == missValue || v_mp4 == missValue) ? 0 : 1;

            double value;

            if (d_np_mp1 < 10)

            { value = v_mp1; }
            else
            {
                value = (go1 * v_mp1 / d_np_mp1 +
                         go2 * v_mp2 / d_np_mp2 +
                         go3 * v_mp3 / d_np_mp3 +
                         go4 * v_mp4 / d_np_mp4) /

                        (go1 / d_np_mp1 +
                         go2 / d_np_mp2 +
                         go3 / d_np_mp3 +
                         go4 / d_np_mp4);
            };

            return value;

        }

        public double interpolateValueSher(int t, int y, int x, Int16[,] maccValues, JassMaccNarrGridsCombo gc, Int16 missValue, Int16 fillValue)
        {

            //minor hack due to 0s. 
            if ( gc.map[y, x].distance > 200 ){   //we do not want to interpolate is distance is larger than 200Kkm
                return missValue;
            }

            int day = Convert.ToInt32(t / 8);
            Int16 v_mp1 = maccValues[day, gc.map[y, x].lat];
            return v_mp1;
        }

        public  JassLatLon MapLatLonToNarr(JassLatLon latlon)
        {
            double MaxDistance = 200;

            //open narr file
            //loop on narr grid finding the closest point

            string narrFile = AppFilesFolder + "\\Narr_Grid.nc";
            Single[] narrY=null;
            Single[] narrX=null;
            Single[,] narrLat = null;
            Single[,] narrLon = null;
            using (var narrDataSet = DataSet.Open(narrFile + "?openMode=open"))
            {
                narrY = narrDataSet.GetData<Single[]>("y");
                narrX = narrDataSet.GetData<Single[]>("x");
                narrLat = narrDataSet.GetData<Single[,]>("lat");
                narrLon = narrDataSet.GetData<Single[,]>("lon");
            }
            double minDistance = MaxDistance;
            int minY = 999;
            int minX = 999;

            for (int y = 0; y < narrY.Length; y++)
            {
                for (int x = 0; x < narrX.Length; x++)
                {
                    var distance = HaversineDistance(latlon.Lat, latlon.Lon, narrLat[y, x], narrLon[y, x]);
                    if (distance < minDistance)
                    {
                        minDistance = distance;
                        minY = y;
                        minX = x;
                    }
                }
            }

            latlon.narrY = minY;
            latlon.narrX = minX;
            latlon.narrLat = narrLat[(int)latlon.narrY, (int)latlon.narrX];
            latlon.narrLon = narrLon[(int)latlon.narrY, (int)latlon.narrX];


            #region maccMapper

            string maccFile = AppFilesFolder + "\\Narr_2_Macc_Grid_Mapper.nc";

            double[,] maccMapDistance;
            int[,] maccMapLatY;
            int[,] maccMapLonX;
            Single[] maccLat;
            Single[] maccLon;

            using (var maccMapperDataSet = DataSet.Open(maccFile + "?openMode=open"))
            {
                maccMapDistance = maccMapperDataSet.GetData<double[,]>("mapDistance");
                maccMapLatY = maccMapperDataSet.GetData<int[,]>("mapLatY");
                maccMapLonX = maccMapperDataSet.GetData<int[,]>("mapLonX");
                maccLat = maccMapperDataSet.GetData<Single[]>("maccLat");
                maccLon = maccMapperDataSet.GetData<Single[]>("maccLon");
            }

            latlon.maccY = maccMapLatY[(int)latlon.narrY, (int)latlon.narrX];
            latlon.maccX = maccMapLonX[(int)latlon.narrY, (int)latlon.narrX];
            latlon.maccLat = (maccLat[(int)latlon.maccY] < 180) ? maccLat[(int)latlon.maccY] : maccLat[(int)latlon.maccY]-360;
            latlon.maccLon = (maccLon[(int)latlon.maccX] < 180) ? maccLon[(int)latlon.maccX] : maccLon[(int)latlon.maccX]-360;

            #endregion 

            #region cfsrMapper

            string cfsrFile = AppFilesFolder + "\\Narr_2_cfsr_Grid_Mapper.nc";

            double[,] cfsrMapDistance;
            int[,] cfsrMapLatY;
            int[,] cfsrMapLonX;
            Single[] cfsrLat;
            Single[] cfsrLon;

            using (var cfsrMapperDataSet = DataSet.Open(cfsrFile + "?openMode=open"))
            {
                cfsrMapDistance = cfsrMapperDataSet.GetData<double[,]>("mapDistance");
                cfsrMapLatY = cfsrMapperDataSet.GetData<int[,]>("mapLatY");
                cfsrMapLonX = cfsrMapperDataSet.GetData<int[,]>("mapLonX");
                cfsrLat = cfsrMapperDataSet.GetData<Single[]>("maccLat");
                cfsrLon = cfsrMapperDataSet.GetData<Single[]>("maccLon");
            }

            latlon.cfsrY = cfsrMapLatY[(int)latlon.narrY, (int)latlon.narrX];
            latlon.cfsrX = cfsrMapLonX[(int)latlon.narrY, (int)latlon.narrX];
            latlon.cfsrLat = (cfsrLat[(int)latlon.cfsrY] < 180) ? cfsrLat[(int)latlon.cfsrY] : cfsrLat[(int)latlon.cfsrY] - 360;
            latlon.cfsrLon = (cfsrLon[(int)latlon.cfsrX] < 180) ? cfsrLon[(int)latlon.cfsrX] : cfsrLon[(int)latlon.cfsrX] - 360;

            #endregion 

            #region sherMapper

            string sherFile = AppFilesFolder + "\\Narr_2_sher_Grid_Mapper.nc";

            double[,] sherMapDistance;
            int[,] sherMapLatY;
            int[,] sherMapLonX;
            Single[] sherLat;
            Single[] sherLon;

            using (var sherMapperDataSet = DataSet.Open(sherFile + "?openMode=open"))
            {
                sherMapDistance = sherMapperDataSet.GetData<double[,]>("mapDistance");
                sherMapLatY = sherMapperDataSet.GetData<int[,]>("mapLatY");
                sherMapLonX = sherMapperDataSet.GetData<int[,]>("mapLonX");
                sherLat = sherMapperDataSet.GetData<Single[]>("maccLat");
                sherLon = sherMapperDataSet.GetData<Single[]>("maccLon");
            }

            latlon.sherY = sherMapLatY[(int)latlon.narrY, (int)latlon.narrX];
            latlon.sherX = sherMapLonX[(int)latlon.narrY, (int)latlon.narrX];
            latlon.sherLat = (sherLat[(int)latlon.sherY] < 180) ? sherLat[(int)latlon.sherY] : sherLat[(int)latlon.sherY] - 360;
            latlon.sherLon = (sherLon[(int)latlon.sherX] < 180) ? sherLon[(int)latlon.sherX] : sherLon[(int)latlon.sherX] - 360;

            #endregion 

            return latlon;
        }


        public JassMaccNarrGridsCombo MapGridNarr2GridFromFile(string fileNameInputGrid, string gridLatName, string gridLonName, string fileNameNarr, string fileNameMapper, bool testAroundToronto, bool sher)
        {
            JassBuilder builder = new JassBuilder();
            DateTime start = DateTime.Now;
            JassBuilderLog builderLog = createBuilderLog(builder, "mapGridStart", "", "", new TimeSpan(), true);
    

            JassMaccNarrGridsCombo gc = new JassMaccNarrGridsCombo();
            string maccFile = AppFilesFolder + "/" + fileNameInputGrid;
            string narrFile = AppFilesFolder + "/" + fileNameNarr;
            string mapFile = AppFilesFolder + "/" + DateTime.Now.Millisecond + fileNameMapper;
            int MissingValue = 999999;

            using (var narrDataSet = DataSet.Open(narrFile + "?openMode=open"))
            {
                Dictionary<string, MetadataDictionary> narrVars = new Dictionary<string, MetadataDictionary>();
                foreach (var v in narrDataSet.Variables) { narrVars.Add(v.Name, v.Metadata); }

                Single[] narrY = narrDataSet.GetData<Single[]>("y");
                Single[] narrX = narrDataSet.GetData<Single[]>("x");

                using (var maccDataSet = DataSet.Open(maccFile + "?openMode=open"))
                {
                    Dictionary<string, MetadataDictionary> maccVars = new Dictionary<string, MetadataDictionary>();
                    foreach (var v in maccDataSet.Variables) { maccVars.Add(v.Name, v.Metadata); }

                    using (var mapDataSet = DataSet.Open(mapFile + "?openMode=create"))
                    {

                        var narrSchema = narrDataSet.GetSchema();
                        var maccSchema = maccDataSet.GetSchema();

                        gc.narrSchema = schema2string(narrSchema);
                        gc.maccSchema = schema2string(maccSchema);

                        gc.maccLat = maccDataSet.GetData<Single[]>(gridLatName);
                        gc.maccLon = maccDataSet.GetData<Single[]>(gridLonName);
                        if (sher)
                        {
                            gc.station = maccDataSet.GetData<string[]>("station");
                        }
                        gc.maccLatMax = gc.maccLat.Length;
                        gc.maccLatMin = 0;
                        gc.maccLonMax = gc.maccLon.Length;
                        gc.maccLonMin = 0;

                        gc.narrLon = narrDataSet.GetData<Single[,]>("lon");
                        gc.narrLat = narrDataSet.GetData<Single[,]>("lat");


                        gc.narrYMin = 0;
                        gc.narrYMax = narrY.Length;
                        gc.narrXMin = 0;
                        gc.narrXMax = narrX.Length;

                        if (testAroundToronto){
                        gc.narrYMin = 128;
                        gc.narrYMax = 136;
                        gc.narrXMin = 230;
                        gc.narrXMax = 250;
                        }
 

                        //mapp the grids
                        DateTime beforeGrid = DateTime.Now;
                        if (sher)
                        {
                            gc = JassWeather.Models.JassWeatherAPI.MapGridNarr2Sher(gc);
                        }
                        else
                        {
                            gc = MapGridNarr2Macc(gc);
                        }
                        DateTime afterGrid = DateTime.Now;
                        TimeSpan gridMappingTime = afterGrid - beforeGrid;

                        JassBuilderLog builderLog2 = createBuilderLog(builder, "after mapping grid with time", "", "", gridMappingTime, true);

                        gc.narrYMin = 0;
                        gc.narrYMax = narrY.Length;
                        gc.narrXMin = 0;
                        gc.narrXMax = narrX.Length;

                        //build the resulting dataset
                        //dataset3.Add<double[]>("time", timeday, "time");

                        //and then we want the maps, map(x,y)/
                        //but I cannot return a pair..so map(x,y) will ne mapX(x,y) mapY(x,y).

                        int[,] mapLonX = new int[gc.narrYMax, gc.narrXMax];
                        int[,] mapLatY = new int[gc.narrYMax, gc.narrXMax];
                        double[,] mapDistance = new double[gc.narrYMax, gc.narrXMax];


                        /////////////////
                        int[,] map2LonX = new int[gc.narrYMax, gc.narrXMax];
                        int[,] map2LatY = new int[gc.narrYMax, gc.narrXMax];
                        double[,] map2Distance = new double[gc.narrYMax, gc.narrXMax];


                        /////////////////
                        int[,] map3LonX = new int[gc.narrYMax, gc.narrXMax];
                        int[,] map3LatY = new int[gc.narrYMax, gc.narrXMax];
                        double[,] map3Distance = new double[gc.narrYMax, gc.narrXMax];

                        //////////////////////

                        int[,] map4LonX = new int[gc.narrYMax, gc.narrXMax];
                        int[,] map4LatY = new int[gc.narrYMax, gc.narrXMax];
                        double[,] map4Distance = new double[gc.narrYMax, gc.narrXMax];


                        for (int y = gc.narrYMin; y < gc.narrYMax; y++)
                        {
                            for (int x = gc.narrXMin; x < gc.narrXMax; x++)
                            {
                                if (gc.map[y, x] != null)
                                {
                                    try
                                    {
                                        mapLatY[y, x] = gc.map[y, x].lat;
                                        mapLonX[y, x] = gc.map[y, x].lon;
                                        mapDistance[y, x] = gc.map[y, x].distance;
                                    }
                                    catch (Exception)
                                    {
                                        mapLatY[y, x] = MissingValue;
                                        mapLonX[y, x] = MissingValue;
                                        mapDistance[y, x] = MissingValue;
                                    }
                                }
                                if (gc.map2[y, x] != null)
                                {
                                    try
                                    {
                                        map2LatY[y, x] = gc.map2[y, x].lat;
                                        map2LonX[y, x] = gc.map2[y, x].lon;
                                        map2Distance[y, x] = gc.map2[y, x].distance;
                                    }
                                    catch (Exception)
                                    {
                                        map2LatY[y, x] = MissingValue;
                                        map2LonX[y, x] = MissingValue;
                                        map2Distance[y, x] = MissingValue;
                                    }
                                }
                                if (gc.map3[y, x] != null)
                                {
                                    try
                                    {
                                        map3LatY[y, x] = gc.map3[y, x].lat;
                                        map3LonX[y, x] = gc.map3[y, x].lon;
                                        map3Distance[y, x] = gc.map3[y, x].distance;
                                    }
                                    catch (Exception)
                                    {
                                        map3LatY[y, x] = MissingValue;
                                        map3LonX[y, x] = MissingValue;
                                        map3Distance[y, x] = MissingValue;

                                    }
                                }

                                if (gc.map4[y, x] != null)
                                {
                                    try
                                    {
                                        map4LatY[y, x] = gc.map4[y, x].lat;
                                        map4LonX[y, x] = gc.map4[y, x].lon;
                                        map4Distance[y, x] = gc.map4[y, x].distance;
                                    }
                                    catch (Exception)
                                    {
                                        map4LatY[y, x] = MissingValue;
                                        map4LonX[y, x] = MissingValue;
                                        map4Distance[y, x] = MissingValue;
                                    }
                                }
                            }
                        }

                        //narr  we want narrX, narrY, narrLon, narrLat

                        JassBuilderLog builderLog3 = createBuilderLog(builder, "mapGridBeforeCreatingnetCDFGC", "", "", DateTime.Now - start, true);


                        var autocommit = mapDataSet.IsAutocommitEnabled;

                        mapDataSet.Add<Single[]>("narrX", narrX, "narrX");
                        mapDataSet.Add<Single[]>("narrY", narrY, "narrY");

                        mapDataSet.Add<Single[,]>("narrLon", gc.narrLon, "narrY", "narrX");
                        mapDataSet.Add<Single[,]>("narrLat", gc.narrLat, "narrY", "narrX");

                        //mac  we want macLon, macLat

                        mapDataSet.Add<Single[]>("maccLon", gc.maccLon, "maccLon");
                        mapDataSet.Add<Single[]>("maccLat", gc.maccLat, "maccLat");

                        mapDataSet.Add<int[,]>("mapLonX", mapLonX, "narrY", "narrX");
                        mapDataSet.Add<int[,]>("mapLatY", mapLatY, "narrY", "narrX");
                        mapDataSet.Add<double[,]>("mapDistance", mapDistance, "narrY", "narrX");

                        mapDataSet.Add<int[,]>("map2LonX", map2LonX, "narrY", "narrX");
                        mapDataSet.Add<int[,]>("map2LatY", map2LatY, "narrY", "narrX");
                        mapDataSet.Add<double[,]>("map2Distance", map2Distance, "narrY", "narrX");

                        mapDataSet.Add<int[,]>("map3LonX", map3LonX, "narrY", "narrX");
                        mapDataSet.Add<int[,]>("map3LatY", map3LatY, "narrY", "narrX");
                        mapDataSet.Add<double[,]>("map3Distance", map3Distance, "narrY", "narrX");

                        mapDataSet.Add<int[,]>("map4LonX", map4LonX, "narrY", "narrX");
                        mapDataSet.Add<int[,]>("map4LatY", map4LatY, "narrY", "narrX");
                        mapDataSet.Add<double[,]>("map4Distance", map4Distance, "narrY", "narrX");

                        mapDataSet.Commit();
                        //add metadata

                        for (int y = 128; y < 136; y++)
                        {
                            for (int x = 230; x < 250; x++)
                            {
                                var mapDistanceValue = mapDistance[y, x];
                                var mapLatYValue = mapLatY[y, x];
                                var mapLonXvalue = mapLonX[y, x];
                            }
                        }


                    }
                }
            }

            JassBuilderLog builderLog4 = createBuilderLog(builder, "mapGridDone", "", "", DateTime.Now - start, true);

            return gc;
        }


        public JassMaccNarrGridsCombo MapGridNarr2MaccFromFile(string fileNameMacc, string fileNameNarr)
        {
           
            
                JassMaccNarrGridsCombo gc = new JassMaccNarrGridsCombo();
                string maccFile = AppDataFolder + "/" + fileNameMacc;
                string narrFile = AppDataFolder + "/" + fileNameNarr;
                string mapFile = AppDataFolder + "/mapGridNarr2Macc.nc";
                int MissingValue = 999999;

                using (var narrDataSet = DataSet.Open(narrFile + "?openMode=open"))
                {
                    Dictionary<string, MetadataDictionary> narrVars = new Dictionary<string, MetadataDictionary>();
                    foreach (var v in narrDataSet.Variables){narrVars.Add(v.Name, v.Metadata);}

                    Single[] narrY = narrDataSet.GetData<Single[]>("y");
                    Single[] narrX = narrDataSet.GetData<Single[]>("x");

                    using (var maccDataSet = DataSet.Open(maccFile + "?openMode=open"))
                    {
                        Dictionary<string, MetadataDictionary> maccVars = new Dictionary<string, MetadataDictionary>();
                        foreach (var v in maccDataSet.Variables) { maccVars.Add(v.Name, v.Metadata); }

                        using (var mapDataSet = DataSet.Open(mapFile + "?openMode=create"))
                        {

                            var narrSchema = narrDataSet.GetSchema();
                            var maccSchema = maccDataSet.GetSchema();

                            gc.narrSchema = schema2string(narrSchema);
                            gc.maccSchema = schema2string(maccSchema);

                            gc.maccLat = maccDataSet.GetData<Single[]>("latitude");
                            gc.maccLon = maccDataSet.GetData<Single[]>("longitude");

                            gc.narrLon = narrDataSet.GetData<Single[,]>("lon");
                            gc.narrLat = narrDataSet.GetData<Single[,]>("lat");

                            //mapp the grids

                            gc = MapGridNarr2Macc(gc);

                            //build the resulting dataset
                            //dataset3.Add<double[]>("time", timeday, "time");

                            //and then we want the maps, map(x,y)/
                            //but I cannot return a pair..so map(x,y) will ne mapX(x,y) mapY(x,y).

                            int[,] mapLonX = new int[gc.narrYMax, gc.narrXMax];
                            int[,] mapLatY = new int[gc.narrYMax, gc.narrXMax];
                            double[,] mapDistance = new double[gc.narrYMax, gc.narrXMax];

          
                            /////////////////
                            int[,] map2LonX = new int[gc.narrYMax, gc.narrXMax];
                            int[,] map2LatY = new int[gc.narrYMax, gc.narrXMax];
                            double[,] map2Distance = new double[gc.narrYMax, gc.narrXMax];


                            /////////////////
                            int[,] map3LonX = new int[gc.narrYMax, gc.narrXMax];
                            int[,] map3LatY = new int[gc.narrYMax, gc.narrXMax];
                            double[,] map3Distance = new double[gc.narrYMax, gc.narrXMax];

                            //////////////////////

                            int[,] map4LonX = new int[gc.narrYMax, gc.narrXMax];
                            int[,] map4LatY = new int[gc.narrYMax, gc.narrXMax];
                            double[,] map4Distance = new double[gc.narrYMax, gc.narrXMax];

                             for (int y = 0; y < gc.narrYMax; y++)
                             //for (int y = 5; y < 7; y++)
                            {
                                for (int x = 0; x < gc.narrXMax; x++)
                                //for (int x = 5; x < 7; x++)
                                {
                                    try
                                    {
                                    mapLatY[y, x] = gc.map[y, x].lat;
                                    mapLonX[y,x] = gc.map[y, x].lon;
                                    mapDistance[y, x] = gc.map[y, x].distance;
                                    }
                                    catch (Exception)
                                    {
                                        mapLatY[y, x] = MissingValue;
                                        mapLonX[y, x] = MissingValue;
                                        mapDistance[y, x] = MissingValue;
                                    }
                                    try
                                    {
                                        map2LatY[y, x] = gc.map2[y, x].lat;
                                        map2LonX[y, x] = gc.map2[y, x].lon;
                                        map2Distance[y, x] = gc.map2[y, x].distance;
                                    }
                                    catch (Exception)
                                    {
                                        map2LatY[y, x] = MissingValue;
                                        map2LonX[y, x] = MissingValue;
                                        map2Distance[y, x] = MissingValue;
                                    }

                                    try
                                    {
                                        map3LatY[y, x] = gc.map3[y, x].lat;
                                        map3LonX[y, x] = gc.map3[y, x].lon;
                                        map3Distance[y, x] = gc.map3[y, x].distance;
                                    }
                                    catch (Exception)
                                    {
                                        map3LatY[y, x] = MissingValue;
                                        map3LonX[y, x] = MissingValue;
                                        map3Distance[y, x] = MissingValue;

                                    }

                                    try
                                    {
                                        map4LatY[y, x] = gc.map4[y, x].lat;
                                        map4LonX[y, x] = gc.map4[y, x].lon;
                                        map4Distance[y, x] = gc.map4[y, x].distance;
                                    }
                                    catch (Exception)
                                    {
                                        map4LatY[y, x] = MissingValue;
                                        map4LonX[y, x] = MissingValue;
                                        map4Distance[y, x] = MissingValue;
                                    }
                                }
                            }

                            //narr  we want narrX, narrY, narrLon, narrLat

                            mapDataSet.Add<Single[]>("narrX", narrX, "narrX");
                            mapDataSet.Add<Single[]>("narrY", narrY, "narrY");

                            mapDataSet.Add<Single[,]>("narrLon", gc.narrLon, "narrY","narrX");
                            mapDataSet.Add<Single[,]>("narrLat", gc.narrLat, "narrY", "narrX");

                            //mac  we want macLon, macLat

                            mapDataSet.Add<Single[]>("maccLon", gc.maccLon, "maccLon");
                            mapDataSet.Add<Single[]>("maccLat", gc.maccLat, "maccLat");

                            mapDataSet.Add<int[,]>("mapLonX", mapLonX, "narrY", "narrX");
                            mapDataSet.Add<int[,]>("mapLatY", mapLatY, "narrY", "narrX");
                            mapDataSet.Add<double[,]>("mapDistance", mapDistance, "narrY", "narrX");

                            mapDataSet.Add<int[,]>("map2LonX", map2LonX, "narrY", "narrX");
                            mapDataSet.Add<int[,]>("map2LatY", map2LatY, "narrY", "narrX");
                            mapDataSet.Add<double[,]>("map2Distance", map2Distance, "narrY", "narrX");

                            mapDataSet.Add<int[,]>("map3LonX", map3LonX, "narrY", "narrX");
                            mapDataSet.Add<int[,]>("map3LatY", map3LatY, "narrY", "narrX");
                            mapDataSet.Add<double[,]>("map3Distance", map3Distance, "narrY", "narrX");

                            mapDataSet.Add<int[,]>("map4LonX", map4LonX, "narrY", "narrX");
                            mapDataSet.Add<int[,]>("map4LatY", map4LatY, "narrY", "narrX");
                            mapDataSet.Add<double[,]>("map4Distance", map4Distance, "narrY", "narrX");

                            //add metadata

                        }
                    }
                }

                return gc;
        }


        public string testBuilderOnDisk(JassBuilder builder, Boolean upload)
        {
            //This method assumes that all the files are in the DISK.
            //The idea is to use it right after processing.
            //So, the idea, among other things is to open both files and compare the values
            //The easy way will be to pick some location and days and see if the have the same value for those days

            //Let do it first for files without level like pressure.

            string Message="OK";
            try
            {
                //open the original file

                string originalFile = AppDataFolder + "/" + this.safeFileNameFromUrl(builder.APIRequest.url);
                using (var originalDataSet = DataSet.Open(originalFile + "?openMode=open"))
                {
                    double[] time = originalDataSet.GetData<double[]>("time");

                    int entriesInDay = builder.JassGrid.Timesize;
                    DateTime startDay = DateTime.Parse("1800-1-1 00:00:00");
                    DateTime day;
                    int originalMeasure;
                    int generatedMeasure;

                    if (builder.JassGrid.JassPartition.Name == "ByDay")
                    {
                        for (int t = 0; t + entriesInDay < time.Length - 1; t += entriesInDay)
                        {

                            day = startDay.AddHours(time[t]);
                            string generatedFileName = AppDataFolder + "/" + fileNameBuilderByDay(builder.JassVariable.Name, day.Year, day.Month, day.Day) + ".nc";
                            using (var generatedDataSet = DataSet.Open(generatedFileName + "?openMode=open"))
                            {

                                if (builder.JassGrid.Levelsize == 0)
                                {
                                    Int16[, ,] originalDataSetValues = originalDataSet.GetData<Int16[, ,]>(builder.Source1VariableName,
                                    DataSet.Range(t, 1, t + entriesInDay - 1), /* removing first dimension from data*/
                                    DataSet.FromToEnd(0),
                                    DataSet.FromToEnd(0));

                                    Int16[, ,] generatedDataSetValues = generatedDataSet.GetData<Int16[, ,]>(builder.JassVariable.Name,
                                    DataSet.FromToEnd(0), /* removing first dimension from data*/
                                    DataSet.FromToEnd(0),
                                    DataSet.FromToEnd(0));

                                    //now we go for each location.....I will hardcode for now.. but I need 

                                    for (int y = 0; y < builder.JassGrid.Ysize; y++)
                                    {
                                        for (int x = 0; x < builder.JassGrid.Xsize; x++)
                                        {
                                            for (int tt = 0; tt < entriesInDay; tt++)
                                            {
                                                generatedMeasure = generatedDataSetValues[tt, y, x];
                                                originalMeasure = originalDataSetValues[tt, y, x];
                                                if (generatedMeasure != originalMeasure)
                                                {
                                                    Message = "Wrong Value at: " + t + " " + tt + " " + y + " " + x;
                                                    return Message;
                                                }
                                            }
                                        }
                                    }
                                }
                                else
                                {
                                    Int16[,,,] originalDataSetValues = originalDataSet.GetData<Int16[,,,]>(builder.Source1VariableName,
                                    DataSet.Range(t, 1, t + entriesInDay - 1), /* removing first dimension from data*/
                                    DataSet.FromToEnd(0),
                                    DataSet.FromToEnd(0),
                                    DataSet.FromToEnd(0));

                                    Int16[,,,] generatedDataSetValues = generatedDataSet.GetData<Int16[,,,]>(builder.JassVariable.Name,
                                    DataSet.FromToEnd(0), /* removing first dimension from data*/
                                    DataSet.FromToEnd(0),
                                    DataSet.FromToEnd(0),
                                    DataSet.FromToEnd(0));

                                    //now we go for each location.....I will hardcode for now.. but I need 

                                    for (int l = 0; l < builder.JassGrid.Levelsize; l++)
                                    {
                                        for (int y = 0; y < builder.JassGrid.Ysize; y++)
                                        {
                                            for (int x = 0; x < builder.JassGrid.Xsize; x++)
                                            {
                                                for (int tt = 0; tt < entriesInDay; tt++)
                                                {
                                                    generatedMeasure = generatedDataSetValues[tt, l, y, x];
                                                    originalMeasure = originalDataSetValues[tt, l, y, x];
                                                    if (generatedMeasure != originalMeasure)
                                                    {
                                                        Message = "Wrong Value at: " + t + " " + tt + " " + y + " " + x;
                                                        return Message;
                                                    }
                                                }
                                            }
                                        }
                                    }


                                }
                            }
                        }
                    }
                    else
                    {

                        Message = "We do not know how to test this partition" + builder.JassGrid.JassPartition.Name;
                    }
                }
                
            }
            catch (Exception e)
            {
                Message = e.Message;
            }

            return Message;
        }


        public string replaceURIPlaceHolders(string urlTemplate, int year, int month, int weeky, int day)
        {
            string url = String.Copy(urlTemplate);
            if (year != 0)
            {
                string yearString = "" + year;
                url = url.Replace("$YYYY", yearString);
            }
            if (month != 0)
            {
                string monthString = "" + month;
                if (month < 10) monthString = "0" + month;
                url = url.Replace("$MM", monthString);
            }

            if (weeky != 0)
            {
                var weekyDay = (weeky - 1) * 5 + 1;
                string weekyString = "" + weekyDay;
                if (weekyDay < 10) weekyString = "0" + weekyDay;
                url = url.Replace("$WW", weekyString);

                int weekyDayEnd=weekyDay+4;
                if (weekyDay > 25) weekyDayEnd = DateTime.DaysInMonth(year, month);
                string weekyEndString = "" + weekyDayEnd;
                if (weekyDayEnd < 10) weekyEndString = "0" + weekyDayEnd;
                url = url.Replace("$ZZ", weekyEndString);

            }

            if (day != 0)
            {
                string dayString = "" + day;
                if (day < 10) dayString = "0" + day;
                url = url.Replace("$DD", dayString);
            }

            int index = url.IndexOf("$");
            if (index > -1) throw new Exception("url still has template variables, forgot to put year or month?");
            return url;

        }

        public int lastDayOfMonth(int year, int month)
        {
            DateTime thisMonthStart = new DateTime(year,month,1);          
            DateTime nextMonthStart =  thisMonthStart.AddMonths(1);

            return (int)(nextMonthStart - thisMonthStart).TotalDays-1;
        }

        public JassBuilderLog createBuilderLog(JassBuilder builder, string eventType, string Label, string Message, TimeSpan span, Boolean success)
        {
            JassBuilderLog jassBuilderLog = new JassBuilderLog();

            jassBuilderLog.JassBuilderID = (builder.JassBuilderID > 0) ? builder.JassBuilderID : (int?)null;
            jassBuilderLog.EventType = eventType;
            jassBuilderLog.ServerName = ServerNameJass;
            jassBuilderLog.Label = eventType;
            jassBuilderLog.startTotalTime = DateTime.Now;
            jassBuilderLog.Message = Message;
            jassBuilderLog.spanTotalTime = span;
            jassBuilderLog.Success = success;

            db.JassBuilderLogs.Add(jassBuilderLog);
            db.SaveChanges();

            return jassBuilderLog;
        }

        public void CreateEnvirolyticNarrGrid()
        {
            string narrFile = AppFilesFolder + "/Narr_Grid.nc";
            string newNarrFile = AppFilesFolder + "/Narr_Grid_New.nc";

            using (var narrDataSet = DataSet.Open(narrFile + "?openMode=open"))
            {

                Single[] narrY = narrDataSet.GetData<Single[]>("y");
                Single[] narrX = narrDataSet.GetData<Single[]>("x");

                Single[,] narrLon = narrDataSet.GetData<Single[,]>("lon");
                Single[,] narrLat = narrDataSet.GetData<Single[,]>("lat");

                using (var newNarrDataSet = DataSet.Open(newNarrFile + "?openMode=create"))
                {


                    newNarrDataSet.Add<Single[]>("y", narrY, "y");
                    newNarrDataSet.Add<Single[]>("x", narrX, "x");

                    newNarrDataSet.Add<Single[,]>("lat", narrLat, "y", "x");
                    newNarrDataSet.Add<Single[,]>("lon", narrLon, "y", "x");
                }



            }
        }

        public JassBuilderLog createBuilderLog(string eventType, string Label, string Message, TimeSpan span, Boolean success)
        {
            JassBuilderLog jassBuilderLog = new JassBuilderLog();

            jassBuilderLog.JassBuilderID = null;
            jassBuilderLog.EventType = eventType;
            jassBuilderLog.ServerName = ServerNameJass;
            jassBuilderLog.Label = eventType;
            jassBuilderLog.startTotalTime = DateTime.Now;
            jassBuilderLog.Message = Message;
            jassBuilderLog.spanTotalTime = span;
            jassBuilderLog.Success = success;

            db.JassBuilderLogs.Add(jassBuilderLog);
            db.SaveChanges();

            return jassBuilderLog;
        }
        public JassBuilderLog createBuilderLog(string eventType, string Label, string Message, Boolean success)
        {
            JassBuilderLog jassBuilderLog = new JassBuilderLog();

            jassBuilderLog.JassBuilderID = null;
            jassBuilderLog.EventType = eventType;
            jassBuilderLog.ServerName = ServerNameJass;
            jassBuilderLog.Label = eventType;
            jassBuilderLog.startTotalTime = DateTime.Now;
            jassBuilderLog.Message = Message;
            jassBuilderLog.spanTotalTime = new TimeSpan();
            jassBuilderLog.Success = success;

            db.JassBuilderLogs.Add(jassBuilderLog);
            db.SaveChanges();

            return jassBuilderLog;
        }

        public JassBuilderLog createBuilderLogChild(JassBuilderLog parentLog, JassBuilder builder, int year, int month, string eventType, string Label, string Message, TimeSpan span, Boolean success)
        {
            JassBuilderLog jassBuilderLog = new JassBuilderLog();

            jassBuilderLog.JassBuilderID = (builder.JassBuilderID > 0) ? builder.JassBuilderID : (int?)null;
            jassBuilderLog.ParentJassBuilderLogID = (int)parentLog.JassBuilderLogID;
            jassBuilderLog.year = year;
            jassBuilderLog.ServerName = ServerNameJass;
            jassBuilderLog.month = month;
            jassBuilderLog.EventType = eventType;
            jassBuilderLog.Label = eventType;
            jassBuilderLog.startTotalTime = DateTime.Now;
            jassBuilderLog.Message = Message;
            jassBuilderLog.spanTotalTime = span;
            jassBuilderLog.Success = success;


            db.JassBuilderLogs.Add(jassBuilderLog);
            db.SaveChanges();

            return jassBuilderLog;
        }


        public string processBuilderAll(JassBuilder builder, Boolean upload, Boolean clean)
        {
            JassBuilderLog builderLog = createBuilderLog(builder, "processBuilderAll_Start", builder.JassVariable.Name, "Start", DateTime.Now - DateTime.Now, true);

            string processInfo = "Variable: " + builder.JassVariable.Name + " Range: "
               + builder.year + "-" + builder.month + +builder.weeky + "-" + " ==> " +
                     +builder.yearEnd + "-" + builder.monthEnd + "-" + builder.weekyEnd;

            var allowed = markProcessStarts("Builder " + builder.JassVariable.Name, processInfo);

            if (!allowed)
            {
                return "Cannot Run - Another Process Running";;
            }


            int yearLog = (builder.year != null)?(int)builder.year: 1800;
            int monthLog = (builder.month != null) ? (int)builder.month : 0;

            JassBuilderLog builderLog2 = createBuilderLogChild(builderLog, builder, yearLog, monthLog, "processBuilder_Start", builder.JassVariable.Name, "", new TimeSpan(), true);


            string Message = "process builder sucessfuly";
            string MessageBuilder = "";
            DateTime StartingTime = DateTime.Now;
            DateTime EndingTime = DateTime.Now;
            JassBuilder jassbuilder = db.JassBuilders.Find(builder.JassBuilderID);
            DataSet dataset3=null;

            //here start the logs for the whole thing



            try
            {
                int startDay, endDay;
                if (builder.year == null)
                {
                    builder.year = DateTime.Now.Year;
                    builder.yearEnd = builder.year;
                }
                if (builder.month == null)
                {
                    builder.month = 0;
                    builder.monthEnd = 0;
                }
                if (builder.monthEnd == null) builder.monthEnd = builder.month;

                if (builder.yearEnd == null) builder.yearEnd=builder.year;
                if (builder.month == null) {
                    builder.month = 0;
                    builder.monthEnd = 0; }
                if (builder.monthEnd == null) builder.monthEnd = builder.month;

                if (builder.weeky == null)
                {
                    builder.weeky = 0;
                    builder.weekyEnd = 0;
                }
                if (builder.day == null)
                {
                    startDay = 0;
                    endDay = 0;
                }
                else
                {
                    startDay = (int)builder.day;
                    if (builder.dayEnd == null)
                    {
                        endDay = DateTime.DaysInMonth((int)builder.year, (int)builder.month);
                    }
                    else
                    {
                        endDay = (int)builder.dayEnd;
                    }
                }


                if (builder.weekyEnd == null) builder.weekyEnd = builder.weeky;

                for (int year = (int)builder.year; year < (int)builder.yearEnd + 1; year++)
                {
                    for (int month = (int)builder.month; month < (int)builder.monthEnd + 1; month++)
                    {
                        for (int weeky = (int)builder.weeky; weeky < (int)builder.weekyEnd + 1; weeky++)
                        {
                            for (int day = startDay; day < endDay + 1; day++)
                            {
                                //here is where we start the real builder
                                DateTime startedAt = DateTime.Now;
                                JassBuilderLog childBuilderLog0 = createBuilderLogChild(builderLog, builder, year, month, "processBuilder_Start weeky" + weeky, builder.JassVariable.Name, "", new TimeSpan(), true);

                                try
                                {
                                    MessageBuilder = processBuilder(builder, year, month, weeky, day, upload, builderLog);


                                    JassBuilderLog childBuilderLog1 = createBuilderLogChild(builderLog, builder, year, month, "processBuilder_End", builder.JassVariable.Name, "", new TimeSpan(), true);

                                    Message += "  processBuilder(" + year + ",  " + month + ") =>" + MessageBuilder;
                                }
                                catch (Exception e)
                                {

                                    JassBuilderLog childBuilderLog1 = createBuilderLogChild(builderLog, builder, year, month, "processBuilder_MANAGED EXCEPTION Could not process ", builder.JassVariable.Name, e.Message, DateTime.Now - startedAt, false);

                                }
                                finally
                                {
                                    //clean disk
                                    if (clean) cleanAppData();
                                    int filesInAppData = Directory.GetFiles(AppDataFolder).Count();
                                    JassBuilderLog childBuilderLog10 = createBuilderLogChild(builderLog, builder, year, month, "processBuilderAll_CleanAppData", builder.JassVariable.Name, "filesInAppData: " + filesInAppData, new TimeSpan(), true);

                                }
                            }

                        }//END WEEK

                    }//END MONTH
                }
                //end YEAR

                JassBuilderLog childBuilderLog3 = createBuilderLogChild(builderLog, builder, (int)builder.yearEnd + 1, (int)builder.monthEnd + 1, "processBuilder_EndOk", builder.JassVariable.Name, "", DateTime.Now - StartingTime, true);


            }

            catch (Exception e)
            {
                JassBuilderLog builderLogEx = createBuilderLog(builder, "processBuilderAll_EndError", builder.JassVariable.Name, e.Message, DateTime.Now - StartingTime, false);


                if (dataset3 != null)
                {
                    dataset3.Dispose();
                }
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

            var resultCode = markProcessEnd("JassDeriver " + builder.JassVariable.Name, processInfo);
            if (!resultCode)
            {
                Message = "Something was wrong with files in the File Forlder but maybe processed did run fine? ";
            }

            return Message; 
        }

        public class ProcessDeriverModel
        {
            public string Message { get; set; }
        }


        public class KeyMetadataModel
        {
            public double scale_factor { get; set; }
            public double add_offset { get; set; }
            public double missing_value { get; set; }
            public double FillValue { get; set; }
        }

        public KeyMetadataModel getKeyMetadata(DataSet dataset)
        {
            var schema1 = dataset.GetSchema();
            VariableSchema keyVariable=null;

            foreach (var v in schema1.Variables)
            {
                if (v.Dimensions.Count > 2)
                {
                    keyVariable = v;
                }
            }

            double scale_factor = 1;
            double add_offset = 0;
            double missing_value = 32766;
            double FillValue = 32766;

            try { scale_factor = Convert.ToDouble(keyVariable.Metadata["scale_factor"]); }
            catch (Exception e)
            {
                var n = e;
            };
            try { add_offset = Convert.ToDouble(keyVariable.Metadata["add_offset"]); }
            catch (Exception e)
            {
                var n = e;
            };
            try { missing_value = Convert.ToDouble(keyVariable.Metadata["missing_value"]); }
            catch (Exception e)
            {
                var n = e;
            };

            try { FillValue = Convert.ToDouble(keyVariable.Metadata["FillValue"]); }
            catch (Exception e)
            {
                var n = e;
            };
            try { FillValue = Convert.ToDouble(keyVariable.Metadata["_FillValue"]); }
            catch (Exception e)
            {
                var n = e;
            };

            KeyMetadataModel result = new KeyMetadataModel();
            result.add_offset = add_offset;
            result.scale_factor = scale_factor;
            result.missing_value = missing_value;
            result.FillValue = FillValue;

            return result;

        }
        public Boolean markProcessStarts(string info, string processInfo)
        {

            string path = AppTempFilesFolder + "/PROCESS_RUNNING";
            if (File.Exists(path)) { return false; }
            string path2 = AppTempFilesFolder + "/PROCESS_INFO_" + info + createTimestamp();

            File.WriteAllText(path, "");
            File.WriteAllText(path2, processInfo);

            return true;
        }

        public Boolean markProcessEnd(string info, string processInfo)
        {

            string path = AppTempFilesFolder + "/PROCESS_RUNNING";
            if (!File.Exists(path)) { return false; }
            string path2 = AppTempFilesFolder + "/PROCESS_RESULT_" + info + createTimestamp();

            File.Delete(path);
            File.WriteAllText(path2, processInfo);

            return true;
        }
            

        public ProcessDeriverModel processDeriverAll(JassDeriver deriver, Boolean upload, Boolean clean)
        {
 
             ProcessDeriverModel result = new ProcessDeriverModel();
             result.Message = "OK";

             string processInfo = "Variable: " + deriver.JassVariable.Name + " Range: " 
                 + deriver.YearStart + "-" + deriver.MonthStart + "-" + deriver.DayStart + " ==> " +
                 + deriver.YearEnd + "-" + deriver.MnnthEnd + "-" + deriver.DayEnd;

             var allowed = markProcessStarts("JassDeriver " + deriver.JassVariable.Name, processInfo);  

             if (!allowed){

                  result.Message = "Cannot Run - Another Process Running";
                  return result;
             }


            int X4HistoryLength = 1;
            if (deriver.X4HistoryLength != null) X4HistoryLength = (int)deriver.X4HistoryLength+1;

            int X5Length = 0;
            string[] X5Variables = new string[1];

            if (deriver.X5 != null) {

                X5Variables = deriver.X5.Split(',');
                X5Length = X5Variables.Length;
            }

             int numberOfMissingValues = 0;
             dynamic resultValues=null;

           //The ideas of this process is to loop over the year, month and day, get the necessary files and create the new one.
             string outputFileName = null; string outputFilePath = null;
             string X1FileName = null;  string X1FilePath = null;
             string X2FileName = null;  string X2FilePath = null;
             string X3FileName = null; string X3FilePath = null;
             string[] X4FileName = new string[X4HistoryLength]; string X4FilePath = null;
             string[] X5FileName = new string[X5Length]; string X5FilePath = null;

             Single[] yDim = null;
             Single[] xDim = null;
             double[] timeDim = null;
             Single[] levelDim = null;

             int year;
             int month;
             int day;

             DateTime dayLooper = new DateTime(deriver.YearStart, deriver.MonthStart, deriver.DayStart);
             DateTime dayEnd = new DateTime(deriver.YearEnd, deriver.MnnthEnd, deriver.DayEnd);
             while (dayLooper <= dayEnd){

                 year = dayLooper.Year;
                 month = dayLooper.Month;
                 day = dayLooper.Day;

                        //first open the necessary file with something like process source.

                 Boolean X1 = (deriver.X1 != null);
                 Boolean X2 = (deriver.X2 != null);
                 Boolean X3 = (deriver.X3 != null);
                 Boolean X4 = (deriver.X4 != null);
                 Boolean X5 = (deriver.X5 != null);


                         outputFileName = fileNameBuilderByDay(deriver.JassVariable.Name, year, month, day) + ".nc";
                         outputFilePath = AppDataFolder + "\\" + outputFileName;
                         DateTime day1 = new DateTime(year, month, day);

                         X1FileName = fileNameBuilderByDay(deriver.X1, year, month, day) + ".nc";
                         X2FileName = fileNameBuilderByDay(deriver.X2, year, month, day) + ".nc";
                         X3FileName = fileNameBuilderByDay(deriver.X3, year, month, day) + ".nc";
                         X1FilePath = AppDataFolder + "\\" + X1FileName;
                         X2FilePath = AppDataFolder + "\\" + X2FileName;
                         X3FilePath = AppDataFolder + "\\" + X3FileName;

                         for (int h = 0; h < X4HistoryLength; h++)
                         {
                             X4FileName[h] = fileNameBuilderByDay(deriver.X4, day1.Year, day1.Month, day1.Day) + ".nc";
                             day1 = day1.AddDays(-1);
                         }

                         for (int h = 0; h < X5Length; h++)
                         {
                             X5FileName[h] = fileNameBuilderByDay(X5Variables[h].Trim(), year, month, day) + ".nc";
                             day1 = day1.AddDays(-1);
                         }

                         if (X1) DownloadFile2DiskIfNotThere(X1FileName, X1FilePath);
                         if (X2) DownloadFile2DiskIfNotThere(X2FileName, X2FilePath);
                         if (X3) DownloadFile2DiskIfNotThere(X3FileName, X3FilePath);
                         if (X4)
                         {
                             for (int h = 0; h < X4HistoryLength; h++)
                             {
                                 DownloadFile2DiskIfNotThere(X4FileName[h], AppDataFolder + "\\" + X4FileName[h]);
                             }
                         }
                         if (X5)
                         {
                             for (int h = 0; h < X5Length; h++)
                             {
                                 DownloadFile2DiskIfNotThere(X5FileName[h], AppDataFolder + "\\" + X5FileName[h]);
                             }
                         }

                         //Now, we will iterate on our grid (we know we are on our grid) and apply the formula
                         //here we need to open the files

                         dynamic x1Values=null;
                         KeyMetadataModel x1Meta=null;
                         if (X1)
                         {
                             using (var x1DataSet = DataSet.Open(X1FilePath + "?openMode=open"))
                             {

                                 yDim = x1DataSet.GetData<Single[]>("y");
                                 xDim = x1DataSet.GetData<Single[]>("x");
                                 timeDim = x1DataSet.GetData<double[]>("time");
                                 if (deriver.X1Level != null)
                                 {
                                     levelDim = x1DataSet.GetData<Single[]>("level");
                                 }

                                 x1Meta = getKeyMetadata(x1DataSet);

                                 //NOTE: This version cannot handle multiple presure.. generalize!
                                 if (deriver.X1Level == null)
                                 {
                                     try
                                     {
                                         x1Values = x1DataSet.GetData<Int16[, ,]>(deriver.X1,
                                               DataSet.FromToEnd(0),
                                               DataSet.FromToEnd(0),
                                               DataSet.FromToEnd(0));
                                     }
                                     catch (Exception) {
                                         x1Values = x1DataSet.GetData<Single[, ,]>(deriver.X1,
                                              DataSet.FromToEnd(0),
                                              DataSet.FromToEnd(0),
                                              DataSet.FromToEnd(0));                                  
                                     }
                                    }
                                 else
                                 {
                                     try
                                     {
                                         x1Values = x1DataSet.GetData<Int16[, ,]>(deriver.X1,
                                         DataSet.FromToEnd(0),
                                         DataSet.ReduceDim((int)deriver.X1Level),
                                         DataSet.FromToEnd(0),
                                         DataSet.FromToEnd(0));
                                     }
                                     catch (Exception) {
                                         x1Values = x1DataSet.GetData<Single[, ,]>(deriver.X1,
                                          DataSet.FromToEnd(0),
                                          DataSet.ReduceDim((int)deriver.X1Level),
                                          DataSet.FromToEnd(0),
                                          DataSet.FromToEnd(0));
                                     }
                                 }
                             }
                         }

                         dynamic x2Values=null;
                         KeyMetadataModel x2Meta=null;
                         if (X2)
                         {
                             using (var x2DataSet = DataSet.Open(X2FilePath + "?openMode=open"))
                             {
                                 x2Meta = getKeyMetadata(x2DataSet);
                                 //NOTE: This version cannot handle multiple presure.. generalize!
                                 if (deriver.X2Level == null)
                                 {
                                     try
                                     {
                                         x2Values = x2DataSet.GetData<Int16[, ,]>(deriver.X2,
                                               DataSet.FromToEnd(0),
                                               DataSet.FromToEnd(0),
                                               DataSet.FromToEnd(0));
                                     }
                                     catch (Exception) {
                                         x2Values = x2DataSet.GetData<Single[, ,]>(deriver.X2,
                                             DataSet.FromToEnd(0),
                                             DataSet.FromToEnd(0),
                                             DataSet.FromToEnd(0));                       
                                     }
                                 }
                                 else
                                 {
                                     try
                                     {
                                         x2Values = x2DataSet.GetData<Int16[, ,]>(deriver.X2,
                                         DataSet.FromToEnd(0),
                                         DataSet.ReduceDim((int)deriver.X2Level),
                                         DataSet.FromToEnd(0),
                                         DataSet.FromToEnd(0));
                                     }
                                     catch (Exception) {
                                         x2Values = x2DataSet.GetData<Single[, ,]>(deriver.X2,
                                        DataSet.FromToEnd(0),
                                        DataSet.ReduceDim((int)deriver.X2Level),
                                        DataSet.FromToEnd(0),
                                        DataSet.FromToEnd(0));
                                     }
                                 }
                             }
                         }

                         dynamic x3Values=null;
                         KeyMetadataModel x3Meta=null;
                         if (X3)
                         {
                             using (var x3DataSet = DataSet.Open(X3FilePath + "?openMode=open"))
                             {
                                 x3Meta = getKeyMetadata(x3DataSet);
                                 //NOTE: This version cannot handle multiple presure.. generalize!
                                 if (deriver.X3Level == null)
                                 {
                                     try
                                     {
                                         x3Values = x3DataSet.GetData<Int16[, ,]>(deriver.X3,
                                               DataSet.FromToEnd(0),
                                               DataSet.FromToEnd(0),
                                               DataSet.FromToEnd(0));
                                     }
                                     catch (Exception) {
                                         x3Values = x3DataSet.GetData<Single[, ,]>(deriver.X3,
                                           DataSet.FromToEnd(0),
                                           DataSet.FromToEnd(0),
                                           DataSet.FromToEnd(0));                                                                  
                                     }
                                 }
                                 else
                                 {
                                     try
                                     {
                                         x3Values = x3DataSet.GetData<Int16[, ,]>(deriver.X3,
                                         DataSet.FromToEnd(0),
                                         DataSet.ReduceDim((int)deriver.X3Level),
                                         DataSet.FromToEnd(0),
                                         DataSet.FromToEnd(0));
                                     }
                                     catch (Exception) {
                                         x3Values = x3DataSet.GetData<Single[, ,]>(deriver.X3,
                                        DataSet.FromToEnd(0),
                                        DataSet.ReduceDim((int)deriver.X3Level),
                                        DataSet.FromToEnd(0),
                                        DataSet.FromToEnd(0));
                                     }
                                 }
                             }
                         }

                         dynamic[] x4Values = new dynamic[X4HistoryLength];
                         KeyMetadataModel x4Meta = null;
                         if (X4)
                         {
                             for (int h = 0; h < X4HistoryLength; h++)
                             {
                                 string filePath = AppDataFolder + "\\" + X4FileName[h];
                                 using (var x4DataSet = DataSet.Open(filePath + "?openMode=open"))
                                 {
                                     if (yDim == null) {
                                         yDim = x4DataSet.GetData<Single[]>("y");
                                         xDim = x4DataSet.GetData<Single[]>("x");
                                         timeDim = x4DataSet.GetData<double[]>("time");
                                    
                                     }

                                     x4Meta = getKeyMetadata(x4DataSet);
                                     //NOTE: This version cannot handle multiple presure.. generalize!
                                     if (deriver.X4Level == null)
                                     {
                                         try
                                         {
                                             x4Values[h] = x4DataSet.GetData<Int16[, ,]>(deriver.X4,
                                               DataSet.FromToEnd(0),
                                               DataSet.FromToEnd(0),
                                               DataSet.FromToEnd(0));
                                         }
                                         catch (Exception) {
                                             x4Values[h] = x4DataSet.GetData<Single[, ,]>(deriver.X4,
                                              DataSet.FromToEnd(0),
                                              DataSet.FromToEnd(0),
                                              DataSet.FromToEnd(0));
                                         }
                                     }
                                     else
                                     {
                                         try
                                         {
                                             x4Values[h] = x4DataSet.GetData<Int16[, ,]>(deriver.X4,
                                             DataSet.FromToEnd(0),
                                             DataSet.ReduceDim((int)deriver.X3Level),
                                             DataSet.FromToEnd(0),
                                             DataSet.FromToEnd(0));
                                         }
                                         catch (Exception) {
                                             x4Values[h] = x4DataSet.GetData<Int16[, ,]>(deriver.X4,
                                            DataSet.FromToEnd(0),
                                            DataSet.ReduceDim((int)deriver.X3Level),
                                            DataSet.FromToEnd(0),
                                            DataSet.FromToEnd(0));
                                         
                                         }
                                     }
                                 }
                             }
                         }

                         dynamic[] x5Values = new dynamic[X5Length];
                         KeyMetadataModel x5Meta = null;
                         if (X5)
                         {
                             for (int h = 0; h < X5Length; h++)
                             {
                                 string filePath = AppDataFolder + "\\" + X5FileName[h];
                                 using (var x5DataSet = DataSet.Open(filePath + "?openMode=open"))
                                 {
                                     if (yDim == null)
                                     {
                                         yDim = x5DataSet.GetData<Single[]>("y");
                                         xDim = x5DataSet.GetData<Single[]>("x");
                                         timeDim = x5DataSet.GetData<double[]>("time");
                                     }

                                     x5Meta = getKeyMetadata(x5DataSet);

                                         try
                                         {
                                             x5Values[h] = x5DataSet.GetData<Int16[, ,]>(X5Variables[h].Trim(),
                                               DataSet.FromToEnd(0),
                                               DataSet.FromToEnd(0),
                                               DataSet.FromToEnd(0));
                                         }
                                         catch (Exception)
                                         {
                                             x5Values[h] = x5DataSet.GetData<Single[, ,]>(X5Variables[h].Trim(),
                                              DataSet.FromToEnd(0),
                                              DataSet.FromToEnd(0),
                                              DataSet.FromToEnd(0));
                                         }

                                 }
                             }
                         }




                         dynamic x1Value = null, x2Value = null, x3Value = null, resultValue;
                         dynamic[] x4Value = new dynamic[X4HistoryLength];
                         dynamic[] x4Value2 = new dynamic[(X4HistoryLength-1)*8];
                         dynamic[] x5Value = new dynamic[X5Length];
                         Single missingvalue = Single.MaxValue;

                             if (deriver.JassGrid.Levelsize==0){
                         resultValues = new Single[deriver.JassGrid.Timesize, deriver.JassGrid.Ysize, deriver.JassGrid.Xsize];
                             }else{
                         resultValues = new Single[deriver.JassGrid.Timesize, deriver.JassGrid.Levelsize, deriver.JassGrid.Ysize, deriver.JassGrid.Xsize];
                             }
                         int safeLevel = (deriver.JassGrid.Levelsize == 0) ? 1 : deriver.JassGrid.Levelsize;
                         for (int t = 0; t < deriver.JassGrid.Timesize; t++)
                         {
                             for (int l = 0; l < safeLevel; l++)
                             {
                                 for (int y = 0; y < deriver.JassGrid.Ysize; y++)
                                 {
                                     for (int x = 0; x < deriver.JassGrid.Xsize; x++)
                                     {
                                         //add_offset + scale_factor * values[tt, ll, yy, xx];

                                         if (
                                             (!X1 || x1Values[t, y, x] != x1Meta.missing_value &&
                                             x1Values[t, y, x] != x1Meta.FillValue)                                        
                                             &&
                                             (!X2 || x2Values[t, y, x] != x2Meta.missing_value &&
                                             x2Values[t, y, x] != x2Meta.FillValue)
                                             &&
                                             (!X3 || x3Values[t, y, x] != x3Meta.missing_value &&
                                             x3Values[t, y, x] != x3Meta.FillValue)
                                             )
                                         {
                                             if (X1) x1Value = x1Meta.add_offset + x1Meta.scale_factor * x1Values[t, y, x];
                                             if (X2) x2Value = x2Meta.add_offset + x2Meta.scale_factor * x2Values[t, y, x];
                                             if (X3) x3Value = x3Meta.add_offset + x3Meta.scale_factor * x3Values[t, y, x];
                                             if (X4)
                                             {
                                                 try
                                                 {
                                                     for (int h = 0; h < X4HistoryLength; h++)
                                                     {
                                                         x4Value[h] = x4Meta.add_offset + x4Meta.scale_factor * x4Values[h][t, y, x];
                                                     }
                                                     int d = 0;
                                                     int tttt;
                                                     for (int ttt = 0; ttt < (X4HistoryLength -1)* 8; ttt++)
                                                     {
                                                         //x4value 2 is the history hor by hour. so we map ttt=0 to this time and day
                                                         //for d=0 and ttt=0, tttt=t.
                                                         //now, when ttt becomes > twe will go into negatives. so, say t is 3.. 2 1 0 7(previous day)
                                                         if (ttt > t + d * 8) { d++; }
                                                         tttt = t + d * 8 - ttt;
                                                         x4Value2[ttt] = x4Values[d][tttt, y, x];

                                                     }
                                                 }
                                                 catch (Exception e) {
                                                     var message = e;
                                                 }
                                             }
                                             if (X5) {
                                                 for (int h = 0; h < X5Length; h++) {
                                                      x5Value[h] = x5Meta.add_offset + x5Meta.scale_factor * x5Values[h][t, y, x];
                                                 }
                                             }

                                             resultValue = processFormula(deriver, x1Value, x2Value, x3Value, x4Value, x4Value2, x5Value);
                                         }
                                         else
                                         {
                                             resultValue = missingvalue;
                                             numberOfMissingValues++;

                                         }

                                         if (deriver.JassGrid.Levelsize == 0)
                                         {
                                             resultValues[t, y, x] = resultValue;
                                         }
                                         else
                                         {
                                             resultValues[t, l, y, x] = resultValue;
                                         }
                                     }
                                 }
                             }
                         }


                         //finally, here is where we write the file
                         //
                         using (var resultDataSet = DataSet.Open(outputFilePath + "?openMode=create"))
                         {
                             resultDataSet.Add<double[]>("time", timeDim, "time");
                             resultDataSet.Add<Single[]>("y",yDim,"y");
                             resultDataSet.Add<Single[]>("x", xDim, "x");
                             resultDataSet.Add<Single[, ,]>(deriver.JassVariable.Name, (Single[, ,])resultValues, "time", "y", "x");


                             //metadata



                             resultDataSet.PutAttr(deriver.JassVariable.Name, "missing_value", missingvalue);

                         }

                         if (upload)
                         {
                             uploadBlob(deriver.JassVariable.Name, outputFileName, outputFilePath);
                         }

                         dayLooper = dayLooper.AddDays(1);
                     }//end while day

             if (clean) cleanAppData();
             result.Message += " number of missing values: " + numberOfMissingValues;


             var resultCode = markProcessEnd("JassDeriver " + deriver.JassVariable.Name, processInfo);
             if (!resultCode)
             {
                 result.Message = "Something was wrong with files in the File Forlder but maybe processed did run fine? ";
             }

             return result;        
        }

        public Single processFormula(JassDeriver deriver, dynamic x1, dynamic x2, dynamic x3, dynamic[] x4, dynamic[] x42, dynamic[] x5)
        {
            if (deriver.JassFormula.Name == "12hrChange")
            { 
            //value of varia
                var d_result = x42[0] - x42[4];
                return Convert.ToSingle(d_result);       
            }
            
            if (deriver.JassFormula.Name == "GermanClasses")
            {
                /* 
                 * Variables Needed:
                 * V850, V500, DV, DTD2M, DT2M, D12V
                 * 
               The weather classes for every grid point are derived from the following conditions:
 
               Class 1:
 
               (V850 < -500 10-6s-1) AND (V500 < -300 10-6s-1) AND (DV < 0 10-6s-1)
 
               Class 2:
 
               (DV < -250 10-6s-1) AND                (DTD2M > 0 °C) AND (DT2M > 0°C)
                                                            OR
               (D12V < -250 10-6s-1) AND (DTD2M > 0°C) AND (DT2M > 3°C)
                                                            OR
               (DV > 200 10-6s-1) AND (DTD2M > 0°C) AND (DT2M > 4°C)
 
               Class 3:
 
               (V850 > 250 10-6s-1) AND (V500 > 200 10-6s-1) AND (DV > -250 10-6s-1) AND (D12V < 250 10-6s-1)            AND (-2°C < DT2M < 2°C)
 
               Class 4:
 
               (DT2M < -6°C) AND (DTD2M < 0 °C)
                                             OR
               (D12V > 250 10-6s-1) AND (DT2M < DTD2M)         (first half of the day)
                                             OR
               (D12V > 250 10-6s-1) AND (DT2M < 0°C)                (second half of the day)
                                             OR
               (DV > 500 10-6s-1) AND (DTD2M < 0°C) 

               Class 5:
               All other cases…
                */
                int classNumber = 5;
                var V850 = x1;
                var V500 = x2;
                var DV = x3;
                var DT2M = x5[0];
                var DTD2M = x5[1];
                var D12V = x5[2];

                //POW(10,-6) = 0.000001       500 10-6s-1 = 0.0005

                // Class1 if (V850 < -500 10-6s-1) AND (V500 < -300 10-6s-1) AND (DV < 0 10-6s-1)
                if ((V850 < -0.0005) && (V500 < -0.0003) && (DV < 0)) classNumber = 1;

                // Class2 if  (DV < -250 10-6s-1) AND (DTD2M > 0 °C) AND (DT2M > 0°C)
                if ((DV < -0.00025) && (DTD2M > 0) && (DT2M > 0 )) classNumber = 2;

                // Class2 if (D12V < -250 10-6s-1) AND (DTD2M > 0°C) <0))AND (DT2M > 3°C)
                if ((D12V < -0.00025) && (DTD2M > 0) && (DT2M > 3)) classNumber = 2;

                //  Class2 if (DV > 200 10-6s-1) AND (DTD2M > 0°C) AND (DT2M > 4°C)                
                if ((DV >  0.00020) && (DTD2M > 0) && (DT2M > 4)) classNumber = 2;

                // Class3 if (V850 > 250 10-6s-1) AND (V500 > 200 10-6s-1) AND (DV > -250 10-6s-1) AND (D12V < 250 10-6s-1) AND (-2°C < DT2M < 2°C)
                if ((V850 > 0.00025) && (V500 > 0.0002) && (DV > -0.00025) && (D12V < 0.00025) && (-2 < DT2M && DT2M < 2)) classNumber = 3;

               // Class4 if (DT2M < -6°C) AND (DTD2M < 0 °C)
                if ((DT2M < -6) && (DTD2M < 0)) classNumber = 4;
                                            
                // Class4 (D12V > 250 10-6s-1) AND (DT2M < DTD2M)         (first half of the day)
                if ((D12V > 0.00025) && (DT2M < DTD2M)) classNumber = 4;
                                             
                // Class4  (D12V > 250 10-6s-1) AND (DT2M < 0°C)                (second half of the day)
                if ((D12V > 0.00025) && (DT2M < 0)) classNumber = 4;
                                             
                // Class4  (DV > 500 10-6s-1) AND (DTD2M < 0°C) 
                if ((DV > 0.0005) && (DTD2M < 0)) classNumber = 4;

                return classNumber;

            }


            if (deriver.JassFormula.Name == "Humidex")
            {
                /*
                 * airtemp  +   0.5555 * (   6.11 * e^ ( 5417.7530 x ( 1 / 273.16 - 1 / dewpointtemp) )  - 10  )
                 */
                var airtemp = x1;
                var dewpointtemp = x2;
                var d_result = airtemp + 0.5555 * (6.11 * Math.Exp(5417.7530 * (1 / 273.16 - 1 / dewpointtemp)) - 10);
                return Convert.ToSingle(d_result);
            }

            if (deriver.JassFormula.Name == "WindChill")
            {
                var airTemp = x1;     //Kelvin
                var windUSpeed = x2;  //meter/sec
                var windVSpeed = x3;  //meter/sec

                var T = airTemp - 273.15;             //from Kelvin to Celcius
                var windUSpeedKmh = windUSpeed * 3.6;       //from meter/sec to km/h
                var windVSpeedKmh = windVSpeed * 3.6;       //from meter/sec to km/h
                var V = Math.Sqrt( Math.Pow(windUSpeedKmh,2) + Math.Pow(windVSpeedKmh,2) );
                var V016 = Math.Pow(V,0.16);
                
                var windChill = 13.12 + 0.6215 * T  - 11.37 * V016  + 0.3965* T * V016 ;
                return  Convert.ToSingle(windChill);
            }

            if (deriver.JassFormula.Name == "WindSpeed")
            {
                var windUSpeed = x1;  //meter/sec
                var windVSpeed = x2;  //meter/sec
                var windUSpeedKmh = windUSpeed * 3.6;       //from meter/sec to km/h
                var windVSpeedKmh = windVSpeed * 3.6;       //from meter/sec to km/h
                var V = Math.Sqrt(Math.Pow(windUSpeedKmh, 2) + Math.Pow(windVSpeedKmh, 2));
                var V016 = Math.Pow(V, 0.16);

                var windSpeed = Math.Sqrt(Math.Pow(windUSpeedKmh, 2) + Math.Pow(windVSpeedKmh, 2)); ;
                return  Convert.ToSingle(windSpeed);
            }


            if (deriver.JassFormula.Name == "Difference2WeightedMean")
            {
                //DT2M = T2M(day) – 1/28 * (T2M(day-1)*7+T2M(day-2)*6+ … + T2M(day-7)*1)

                var mean =  (x4[1] * 7 + x4[2] * 6 + x4[3] * 5 + x4[4] * 4 + x4[5] * 3 + x4[6] * 2 + x4[7] * 1)/28;
                var value = x4[0] - mean;
                return  Convert.ToSingle(value);
            }
            //x1-x2

            if (deriver.JassFormula.Name == "x1-x2")
            {

                var value = x1 - x2;
                return Convert.ToSingle(value);
            }

            throw new Exception("We do not have an algorithm to calculate the specified formula");
        }

        public string processBuilder(JassBuilder builder, int year, int month, int weeky, int day, Boolean upload, JassBuilderLog builderAllLog)
        {

            string Message = "process builder sucessfuly";
            long StartingMemory;
            DateTime StartingTime = DateTime.Now;
            long AfterOpenMemory;
            long AfterLoadMemory;
            DateTime EndingTime = DateTime.Now;

            JassBuilder jassbuilder = db.JassBuilders.Find(builder.JassBuilderID);
            DataSet dataset3 = null;

            try
            {
                #region logs
                //Let try to re-create the file...
                GC.Collect();
                StartingMemory = GC.GetTotalMemory(true);
                startTotalTime = DateTime.Now;

                string timestamp = JassWeatherAPI.fileTimeStamp();
                int entriesInDay = builder.JassGrid.Timesize;

                DateTime startTimeProcessSource = DateTime.Now;
                JassBuilderLog childBuilderLog0 = createBuilderLogChild(builderAllLog, builder, year, month, "processBuilder_BeforeProcessSource", builder.JassVariable.Name, "", startTimeProcessSource - startTimeProcessSource, true);
                #endregion logs
                string inputFile1 = processSource(builder, year, month, weeky, day, upload, false, builderAllLog);
                #region logs
                JassBuilderLog childBuilderLog1 = createBuilderLogChild(builderAllLog, builder, year, month, "processBuilder_AfterProcessSource", builder.JassVariable.Name, "", DateTime.Now - startTimeProcessSource, true);
                #endregion logs

                Boolean input_Grid_Is_Different = (builder.APIRequest.JassGrid.Type != "NARR" );

                if (input_Grid_Is_Different)
                {  try
                    {
                        if (builder.APIRequest.JassGrid.Type == "MACC")
                        {
                        string inputFileTemplateBeforeTransformation = builder.APIRequest.url;
                        inputFile1 = processGridMappingMaccToNarr(year, month,inputFileTemplateBeforeTransformation);
                        }else
                        if (builder.APIRequest.JassGrid.Type == "CFSR")
                        {

                            int firstDayOfWeeky = (weeky-1)*5 + 1;
                            int daysInMonth = DateTime.DaysInMonth(year,month);
                            int daysInWeeky = 5;
                            if (firstDayOfWeeky > 25) daysInWeeky = daysInMonth - 25;
                            if (weeky == 0) { daysInWeeky = 1; }

                            string inputFileTemplateBeforeTransformation = safeFileNameFromUrl(builder.APIRequest.url);
                            for (int d = 0; d < daysInWeeky; d++)
                            {
                                try
                                {
                                    processGridMappingCFSRToNarrModel result = processGridMappingCFSRToNarr(builder.JassVariable.Name, year, month, weeky, day, d, inputFileTemplateBeforeTransformation);
                                    inputFile1 = saveprocessGridMappingCFSRToNarrModel(result);
                                    createBuilderLogChild(builderAllLog, builder, year, month, "transfor+generate weeky: " + weeky + " d:" + d, builder.JassVariable.Name, "", DateTime.Now - startTimeProcessSource, true);
                                }
                                catch (Exception e)
                                {   
                                    createBuilderLog("EXCEPTION", "processGridMappingCFSRToNarr", e.Message, new TimeSpan(), false);
                                }
                                if (upload)
                                {
                                    uploadBlob(builder.JassVariable.Name, inputFile1, this.AppDataFolder + "/" + inputFile1);
                                    createBuilderLogChild(builderAllLog, builder, year, month, " UPLOAD weeky: " + weeky + " d:" + d, builder.JassVariable.Name, "", DateTime.Now - startTimeProcessSource, true);

                                }

                            }

                            return Message;


                        }else

                            if (builder.APIRequest.JassGrid.Type == "SHER")
                            {
                                string inputFileTemplateBeforeTransformation = builder.APIRequest.url;
                                inputFile1 = processGridMappingSHERToNarr(year, month, weeky, inputFileTemplateBeforeTransformation);
                            }
                            else
                                if (builder.APIRequest.JassGrid.Type == "NAPS")
                                {
                                    inputFile1 = processGridMappingNAPSToNarr(year, month, weeky, inputFile1);
                                }
                                else
                            {
                                //if we are here the type was wrong
                                throw new Exception("We cannot handle the supplied type of GRID: " + builder.APIRequest.JassGrid.Type);
                            }
                    }
                       catch (Exception e)
                    {

                    JassBuilderLog childBuilderLog122 = createBuilderLogChild(builderAllLog, builder, year, month, "processBuilder_AfterTransformingGridERROR", builder.JassVariable.Name, "", DateTime.Now - startTimeProcessSource, false);
                    return "error";
                    }

                }

                JassBuilderLog childBuilderLog11 = createBuilderLogChild(builderAllLog, builder, year, month, "processBuilder_AfterTransformingGrid", builder.JassVariable.Name, "", DateTime.Now - startTimeProcessSource, true);


                using (var dataset1 = DataSet.Open(inputFile1 + "?openMode=open"))
                {
                    DateTime startTimeAfterOpenFile = DateTime.Now;
                    JassBuilderLog childBuilderLog2 = createBuilderLogChild(builderAllLog, builder, year, month, "processBuilder_AfterOpenFile", builder.JassVariable.Name, "", startTimeProcessSource - startTimeProcessSource, true);


                    var schema1 = dataset1.GetSchema();
                    MetadataDictionary metaDataSet = dataset1.Metadata;



                    Dictionary<string, MetadataDictionary> vars =
                        new Dictionary<string, MetadataDictionary>();

                    foreach (var v in dataset1.Variables)
                    {
                        vars.Add(v.Name, v.Metadata);
                    }


                    //Here we get all the information form the datasource1

                    Single[] y = dataset1.GetData<Single[]>("y");
                    MetadataDictionary metaY; vars.TryGetValue("y", out metaY);
                    Single[] x = dataset1.GetData<Single[]>("x");
                    MetadataDictionary metaX; vars.TryGetValue("x", out metaX);
                    double[] time = dataset1.GetData<double[]>("time");
                    MetadataDictionary metaTime; vars.TryGetValue("time", out metaTime);
                    Single[] level = new Single[0];
                    MetadataDictionary metaLevel = null;

                    if (builder.JassGrid.Levelsize != 0)
                    {
                        level = dataset1.GetData<Single[]>("level");
                        vars.TryGetValue("level", out metaLevel);
                    }

                    MetadataDictionary metaVariable; vars.TryGetValue(builder.Source1VariableName, out metaVariable);



                    string dayString;
                    int in_year = (year != 0) ? (int)year : DateTime.Now.Year;
                    int in_month = (month != 0) ? (int)month : 1;
                    int in_day = (day != 0) ? (int)day : 1;
                    string variableName = builder.JassVariable.Name;

                    DateTime currentDay = new DateTime(in_year, in_month, in_day);


                    jassbuilder.Status = JassBuilderStatus.Processing;
                    jassbuilder.ServerName = ServerNameJass;
                    jassbuilder.setTotalSize = time.Length / 8;
                    jassbuilder.setCurrentSize = 0;
                    jassbuilder.Message = "";
                    db.Entry<JassBuilder>(jassbuilder).State = System.Data.EntityState.Modified;
                    db.SaveChanges();

                    double[] timeday = new double[8];

                    for (var df = 0; df < time.Length; df += 8)
                    {
                        in_year = currentDay.Year;
                        in_month = currentDay.Month;
                        in_day = currentDay.Day;

                        dayString = "" + in_year + "_" + in_month + "_" + in_day;

                        string outputFileName = fileNameBuilderByDay(variableName, in_year, in_month, in_day) + ".nc";

                        string outputFilePath = AppDataFolder + "/" + outputFileName;

                        JassBuilderLog childBuilderLog3 = createBuilderLogChild(builderAllLog, builder, year, month, "processBuilder_BeforeOpenWrittingFile", "Test", dayString, startTimeProcessSource - startTimeProcessSource, true);

                        using (dataset3 = DataSet.Open(outputFilePath + "?openMode=create"))
                        {
                            JassBuilderLog childBuilderLog4 = createBuilderLogChild(builderAllLog, builder, year, month, "processBuilder_AfterOpenWrittingFile", "Test", dayString, startTimeProcessSource - startTimeProcessSource, true);

                            foreach (var attr in dataset1.Metadata)
                            {
                                if (attr.Key != "Name")
                                {
                                    dataset3.Metadata.AsDictionary().Add(attr.Key, attr.Value);
                                }
                                else
                                {

                                    dataset3.Metadata.AsDictionary()[attr.Key] = outputFileName;
                                }
                            }

                            AfterOpenMemory = GC.GetTotalMemory(true);
                            var schema3 = dataset3.GetSchema();

                            for (var t = 0; t < 8; t++)
                            {
                                timeday[t] = time[df + t];
                            }

                            //here we create the new outpout datasource depending on whether we hae or not level dimension

                            if (builder.JassGrid.Levelsize != 0)
                            {
                                Int16[, , ,] dataset = dataset1.GetData<Int16[, , ,]>(builder.Source1VariableName,
                                     DataSet.Range(df, 1, df + entriesInDay - 1), /* removing first dimension from data*/
                                     DataSet.FromToEnd(0), /* removing first dimension from data*/
                                     DataSet.FromToEnd(0),
                                     DataSet.FromToEnd(0));

                                dataset3.Add<Single[]>("level", level, "level");
                                foreach (var attr in metaLevel)
                                {
                                    if (attr.Key != "Name") dataset3.PutAttr("level", attr.Key, attr.Value);
                                }


                                dataset3.Add<Int16[, , ,]>(builder.JassVariable.Name, dataset, "time", "level", "y", "x");


                            }
                            else
                            {
                                Int16[, ,] dataset = dataset1.GetData<Int16[, ,]>(builder.Source1VariableName,
                                DataSet.Range(df, 1, df + entriesInDay - 1), /* removing first dimension from data*/
                                DataSet.FromToEnd(0),
                                DataSet.FromToEnd(0));

                                dataset3.Add<Int16[, ,]>(builder.JassVariable.Name, dataset, "time", "y", "x");
                            }

                            //dataset3.PutAttr(builder.JassVariable.Name, "Name", builder.JassVariable.Name);
                            foreach (var attr in metaVariable)
                            {
                                //problem with _FillValue
                                if (attr.Key != "Name")
                                {
                                    try
                                    {
                                        dataset3.PutAttr(builder.JassVariable.Name, cleanMetadataKey(attr.Key), attr.Value);
                                    }
                                    catch (Exception e)
                                    {
                                        try
                                        {
                                            dataset3.PutAttr(builder.JassVariable.Name, attr.Key, e.Message);
                                        }
                                        catch (Exception) { }
                                    }
                                }
                            }

                            dataset3.Add<double[]>("time", timeday, "time");
                            foreach (var attr in metaTime) { if (attr.Key != "Name") dataset3.PutAttr("time", attr.Key, attr.Value); }
                            dataset3.Add<Single[]>("y", y, "y");
                            foreach (var attr in metaY) { if (attr.Key != "Name") dataset3.PutAttr("y", attr.Key, attr.Value); }
                            dataset3.Add<Single[]>("x", x, "x");
                            foreach (var attr in metaX) { if (attr.Key != "Name") dataset3.PutAttr("x", attr.Key, attr.Value); }


                            dataset3.Commit();
                            dataset3.Dispose();

                            AfterLoadMemory = GC.GetTotalMemory(true);



                            if (upload)
                            {
                                uploadBlob(builder.JassVariable.Name, outputFileName, outputFilePath);
                            }

                            jassbuilder.setCurrentSize = df / 8 + 1;
                            db.Entry<JassBuilder>(jassbuilder).State = System.Data.EntityState.Modified;
                            db.SaveChanges();

                            currentDay = currentDay.AddDays(1);
                            //end using
                        }
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
                    //end using
                }

                JassBuilderLog childBuilderLog10 = createBuilderLogChild(builderAllLog, builder, year, month, "processBuilder_EndOk", builder.JassVariable.Name, "", DateTime.Now - startTimeProcessSource, true);

            }

            catch (Exception e)
            {
                JassBuilderLog childBuilderLog9 = createBuilderLogChild(builderAllLog, builder, year, month, "processBuilder_EndException", builder.JassVariable.Name, e.Message, new TimeSpan(), true);

                if (dataset3 != null)
                {
                    dataset3.Dispose();
                }
                Message = e.Message;
                DateTime EndTime = DateTime.Now;
                TimeSpan TotalTime = EndTime - startTotalTime;
                jassbuilder.startTotalTime = startTotalTime;
                jassbuilder.endTotalTime = EndTime;
                jassbuilder.OnDisk = false;
                jassbuilder.spanTotalTime = TotalTime;
                jassbuilder.Status = JassBuilderStatus.Failure;
                jassbuilder.Message = e.Message;

                db.Entry<JassBuilder>(jassbuilder).State = System.Data.EntityState.Modified;
                db.SaveChanges();
            }

            return Message;
        }

        public string cleanMetadataKey(string rawKey)
        {
            string newKey = rawKey;
            int indexOfUnderscore = rawKey.IndexOf("_");
            if (indexOfUnderscore == 0) newKey = newKey.Substring(1);
            return newKey;
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
                int in_year = (builder.year != null) ? (int)builder.year : DateTime.Now.Year;
                int in_month = (builder.month != null) ? (int)builder.month : 1;
                int in_day = 1;
                DateTime day = new DateTime(in_year, in_month, in_day);
                int currentNumberOfFiles = 0;
                string variableName = builder.JassVariable.Name;

                for (var df = 0; df < builder.JassGrid.Timesize; df += 8)
                {
                    in_year = day.Year;
                    in_month = day.Month;
                    in_day = day.Day;

                    dayString = "" + in_year + "_" + in_month + "_" + in_day;
                    string outputFile = AppDataFolder + "/" + fileNameBuilderByDay(variableName, in_year, in_month, in_day) + ".nc";

                    Boolean fileExists = File.Exists(outputFile);

                    if (fileExists)
                    {
                        currentNumberOfFiles += 1;
                    }
                    day.AddDays(1);
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
                int in_year = (builder.year != null) ? (int)builder.year : DateTime.Now.Year;
                int in_month = (builder.month != null) ? (int)builder.month : 1;
                int in_day = 1;
                DateTime day = new DateTime(in_year, in_month, in_day);
                int currentNumberOfFiles = 0;
                string variableName = builder.JassVariable.Name;

                for (var df = 0; df < builder.JassGrid.Timesize; df += 8)
                {
                    in_year = day.Year;
                    in_month = day.Month;
                    in_day = day.Day;

                    dayString = "" + in_year + "_" + in_month + "_" + in_day;
                    string outputFile = AppDataFolder + "/" + fileNameBuilderByDay(variableName, in_year, in_month, in_day) + ".nc";

                    File.Delete(outputFile);

                    day.AddDays(1);
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

        public class NapsInfoModel
        {
            public string response { get; set; }
            public string[] lines { get; set; }
            public bool[] success { get; set; }
            public string[] city { get; set; }
            public string[] code { get; set; }
            public string[] urls { get; set; }
            public string[] availability { get; set; }
            public string[] measures { get; set; }
            public int testMax { get; set; }
            public Int16[,] data { get; set; }
            public TimeSpan timeSpan;
            public int totalTimePoints { get; set; }
            public int totalCodes { get; set; }
        }

        public class SheridanInfoModel
        {
            public string response { get; set; }
            public string[] lines { get; set; }
            public bool[] success { get; set; }
            public string[] city { get; set; }
            public string[] code { get; set; }
            public string[] urls { get; set; }
            public string[] availability { get; set; }
            public string[] measures  { get; set; }
            public int testMax { get; set; }
            public Int16[,] data { get; set; }
            public TimeSpan timeSpan;
            public int totalTimePoints { get; set; }
            public int totalCodes { get; set; }
        }

        public string[] SheridanGetLatLon(){

                int numberOfStationsGeocoded = 0;
                string sherindanStationsFilePath = AppFilesFolder + "/sheridan-stations.csv";
                string[] lines = System.IO.File.ReadAllLines(sherindanStationsFilePath);
                string[] addresses = new string[lines.Length];
                string[] formatted_addresses =new string[lines.Length];
                double[] lats=new double[lines.Length];
                double[] lons=new double[lines.Length];            
                string[] results = new string[lines.Length+1];
                string[] urls = new string[lines.Length];

                int delay = 3000;
                for (int l = 0; l < lines.Length; l++)
                {
                    //if (l%10==0) { delay=10000; };
                    //System.Threading.Thread.Sleep(delay);
                    
                    var line = lines[l].Split('\t');
                   // addresses[l] = (line[0] +" "+ line[1]).Replace(" ","%20");
                    addresses[l] = (line[0]).Replace(" ", "%20");
                    string code = line[1];
                    //here we see if we have this in the DB

                    JassLatLonGroup jasslatlonGroup = db.JassLatLonGroups.Where(j => j.Name == "SheridanStations").First();
                    List<JassLatLon> jasslatlonList = db.JassLatLons.Where(j => j.StationCode==code).ToList();
                    if (jasslatlonList.Count > 0)
                    {
                        JassLatLon jassLatLon = jasslatlonList[0];
                        jassLatLon.JassLatLonGroupID = jasslatlonGroup.JassLatLonGroupID;
                        db.Entry(jassLatLon).State = System.Data.EntityState.Modified;
                        db.SaveChanges();
                    }

                    if (jasslatlonList.Count == 0)
                    {

                        string responseString;
                        dynamic response;
                        string formatted_address = "n/a";
                        double lat = 0;
                        double lon = 0;

                        string url = "http://maps.googleapis.com/maps/api/geocode/json?address=" + addresses[l] + "&sensor=false";
                        WebRequest req = WebRequest.Create(url);
                        req.Method = "GET";
                        try
                        {
                            HttpWebResponse resp = req.GetResponse() as HttpWebResponse;
                            if (resp.StatusCode == HttpStatusCode.OK)
                            {
                                using (Stream respStream = resp.GetResponseStream())
                                {
                                    StreamReader reader = new StreamReader(respStream, Encoding.UTF8);
                                    responseString = reader.ReadToEnd();
                                    response = Json.Decode(responseString).results[0];
                                    formatted_address = response.formatted_address;
                                    lat = Convert.ToDouble(response.geometry.location.lat);
                                    lon = Convert.ToDouble(response.geometry.location.lng);
                                    results[l] = addresses[l] + " ==>> lat:" + lat + "lon:" + lon + "formatted_address: " + formatted_address;

                                    //here, we are saving the found values in the database assuming this value is not in the DB

                                    JassLatLon jasslatlon = new JassLatLon();
                                    jasslatlon.StationCode = code;
                                    jasslatlon.Lat = lat;
                                    jasslatlon.Lon = lon;
                                    jasslatlon.Info = "NO STATION " + formatted_address;
                                    db.JassLatLons.Add(jasslatlon);
                                    db.SaveChanges();

                                    numberOfStationsGeocoded++;

                                }
                            }
                            else
                            {
                                results[l] = string.Format("Status Code: {0}, Status Description: {1}", resp.StatusCode, resp.StatusDescription);
                            }
                        }
                        catch (Exception e)
                        {
                            results[l] = "            ---Error: " + addresses[l] + " URL:" + url;
                        }

                    }//end if jasslatlonList==0
                }//end of for lines

                int numberOfLatLons = db.JassLatLons.ToList().Count();
                results[lines.Length] = " Total Lines: " + lines.Length + " Total in DB: " + numberOfLatLons + " Geocoded now: " + numberOfStationsGeocoded;                
                return results;
        }




        public SheridanInfoModel sheridanGetHistory(int testMax)
        {
            DateTime startDateTime = DateTime.Now;
            SheridanInfoModel vm = new SheridanInfoModel();

            DateTime startRelevantHistoryDay = DateTime.Parse("2002-01-01");

            Int16 missingValue = 32767;
            int totalDays = 366*13;
            string sherindanStationsFilePath = AppFilesFolder + "/sheridan-stations.csv";
            string[] lines = System.IO.File.ReadAllLines(sherindanStationsFilePath);
            bool[] success = new bool[lines.Length];
            string[] city = new string[lines.Length];
            string[] code = new string[lines.Length];
            string[] urls = new string[lines.Length];
            string[] availability = new string[lines.Length];
            string[] measures = new string[lines.Length];
            Int16[,] data = new short[code.Length,totalDays];
            string[] splitMeasuresArray;
            string[] splitMeasure;
            int problems = 0;

            int totalCodes = 0;

            for (int l = 0; l < lines.Length && l < testMax; l++)
            {
                var line = lines[l].Split('\t');
                city[l] = line[0];
                code[l] = line[1];
                availability[l] = line[2];

                              

                string url = "http://sheridan.geog.kent.edu/ssc/files/" + code[l] + ".dbdmt";
                urls[l] = url;
                WebRequest req = WebRequest.Create(url);
                req.Method = "GET";
                try
                {
                    HttpWebResponse resp = req.GetResponse() as HttpWebResponse;
                    if (resp.StatusCode == HttpStatusCode.OK)
                    {
                        using (Stream respStream = resp.GetResponseStream())
                        {
                            StreamReader reader = new StreamReader(respStream, Encoding.UTF8);
                            measures[l] = reader.ReadToEnd();
                            success[l] = true;
                        }
                    }
                    else
                    {
                        measures[l] = string.Format("Status Code: {0}, Status Description: {1}", resp.StatusCode, resp.StatusDescription);
                        problems++;
                        success[l] = false;
                    }
                }
                catch (Exception e)
                {
                    measures[l] = e.Message;
                    problems++;
                    success[l] = false;
                }

                //Now, if success, let's do something with the measures
                if (success[l])
                {
                    splitMeasuresArray = measures[l].Split('\n');

                    splitMeasure = splitMeasuresArray[0].Split(' ');
                    DateTime dataStartDay = getDataFromString(splitMeasure[1]);
                  
                    int irrelevantTotalDays=0;
                    int missingTotalDays = 0;

                    if (dataStartDay < startRelevantHistoryDay)
                    {
                        TimeSpan irrelevantDays = startRelevantHistoryDay - dataStartDay;
                        irrelevantTotalDays = Convert.ToInt16(irrelevantDays.TotalDays);
                    }
                    else
                    {
                        TimeSpan missingDays = dataStartDay - startRelevantHistoryDay;
                        missingTotalDays = Convert.ToInt16(missingDays.TotalDays);
                    }

                    for(int dd=0;dd<missingTotalDays;dd++){
                        data[l, dd] = missingValue;
                        }

                    int dayIndex=missingTotalDays;

                    for(int d=0+irrelevantTotalDays; d < splitMeasuresArray.Length-1; d++){
                        string splitMeasureString = splitMeasuresArray[d];
                        splitMeasure = splitMeasureString.Split(' ');
                        var codeString = splitMeasure[0];
                        var dayString = splitMeasure[1];
                        var measureString = splitMeasure[2];

                        DateTime inputTime = getDataFromString(dayString);
                        DateTime historyTime = startRelevantHistoryDay.AddDays(dayIndex);
                        if (historyTime != inputTime)
                        {
                            var problem = "problem";
                        }

                        string currentCode = code[l];
                        Int16 currentMeasure = Convert.ToInt16(measureString);
                        data[l, dayIndex] = currentMeasure;

                        dayIndex++;
                    }

                    totalCodes++;
                }
            }

            if (problems > 0)
            {
                vm.response = "Number of problems: " + problems;
            }
            else { vm.response = "OK"; }
            vm.lines = lines;
            vm.success = success;
            vm.city = city;
            vm.availability = availability;
            vm.code = code;
            vm.measures = measures;
            vm.testMax = testMax;
            vm.data = data;
            vm.urls = urls;
            vm.totalTimePoints = totalDays;
            vm.totalCodes = totalCodes;

            DateTime endDateTime = DateTime.Now;
            vm.timeSpan = endDateTime - startDateTime;

            return vm;

        }

        public string napsSaveHistory(NapsInfoModel vm)
        {

            //this process will save this information in a grid-friendly way.
            //we will have data, lat, lon, time and no level

            string ReturnMessage = "Ok";
            string fileNameNarr = "Narr_Grid.nc";
            string narrFile = AppFilesFolder + "/" + fileNameNarr;

            double[] time = new double[vm.totalTimePoints];
            string[] station = new string[vm.totalCodes];
            Single[] lat = new Single[vm.totalCodes];
            Single[] lon = new Single[vm.totalCodes];
            Int16[,] sher = new Int16[vm.totalTimePoints, vm.totalCodes];

            using (var narrDataSet = DataSet.Open(narrFile + "?openMode=open"))
            {
                string outputFilePath = AppDataFolder + "\\sherindan-history.nc";
                using (var sheridanOutputDataSet = DataSet.Open(outputFilePath + "?openMode=create"))
                {

                    for (int s = 0; s < vm.totalCodes; s++)
                    {
                        station[s] = vm.code[s];
                        for (int t = 0; t < vm.totalTimePoints; t++)
                        {
                            sher[t, s] = vm.data[s, t];
                        }
                    }

                    sheridanOutputDataSet.Add<double[]>("time", time, "time");
                    sheridanOutputDataSet.Add<string[]>("station", station, "station"); ;
                    sheridanOutputDataSet.Add<Single[]>("lat", lat, "station");
                    sheridanOutputDataSet.Add<Single[]>("lon", lon, "station");
                    sheridanOutputDataSet.Add<Int16[,]>("sher", sher, "time", "station");
                }
            }


            return ReturnMessage;
        }
     

        public string sheridanSaveHistory(SheridanInfoModel vm){

            //this process will save this information in a grid-friendly way.
            //we will have data, lat, lon, time and no level

            string ReturnMessage = "Ok";
            string fileNameNarr = "Narr_Grid.nc";
            string narrFile = AppFilesFolder + "/" + fileNameNarr;

            double[] time = new double[vm.totalTimePoints];
            string[] station = new string[vm.totalCodes];
            Single[] lat = new Single[vm.totalCodes];
            Single[] lon = new Single[vm.totalCodes];
            Int16[,] sher = new Int16[vm.totalTimePoints,vm.totalCodes];

            using (var narrDataSet = DataSet.Open(narrFile + "?openMode=open"))
            {
                string outputFilePath = AppDataFolder + "\\sherindan-history.nc";
                using (var sheridanOutputDataSet = DataSet.Open(outputFilePath + "?openMode=create"))
                {

                    for (int s = 0; s < vm.totalCodes; s++)
                    {
                        station[s] = vm.code[s];
                        for (int t = 0; t < vm.totalTimePoints; t++) {
                        sher[t, s] = vm.data[s,t];
                        }
                    }

                    sheridanOutputDataSet.Add<double[]>("time", time, "time");
                    sheridanOutputDataSet.Add<string[]>("station", station, "station");;
                    sheridanOutputDataSet.Add<Single[]>("lat", lat, "station");
                    sheridanOutputDataSet.Add<Single[]>("lon", lon, "station");
                    sheridanOutputDataSet.Add<Int16[,]>("sher", sher, "time","station");
                }
            }
                       

            return ReturnMessage;
        }

        public string sheridanSaveLatLongFromDB()
        {
            string ReturnMessage = "ok";
            //the idea here is to loop through all the found codes in the DB and store them in a file
            //the idea is to create the dimensions 

            string sherindanStationsFilePath = AppFilesFolder + "/sheridan-stations.csv";
            string[] lines = System.IO.File.ReadAllLines(sherindanStationsFilePath);

            string[] station = new string[lines.Length];
            Single[] lat = new Single[lines.Length];
            Single[] lon = new Single[lines.Length];

            List<JassLatLon> latlons = db.JassLatLons.Where(l => l.StationCode.Length == 3).ToList();
            Dictionary<string, JassLatLon> latlonsDic = new Dictionary<string, JassLatLon>();
            string code;

            foreach( var latlon in latlons){
                code = latlon.StationCode.Substring(0, 3);
                latlonsDic.Add(code, latlon);
            }


            for (int l = 0; l < lines.Length; l++)
            {
                var line = lines[l].Split('\t');
                code = line[1];
                station[l] = code;
                lat[l] = Convert.ToSingle(latlonsDic[code].Lat);
                lon[l] = Convert.ToSingle(latlonsDic[code].Lon);


            }


            string outputFilePath = AppDataFolder + "\\sherindan_stations.nc";
                using (var sheridanOutputDataSet = DataSet.Open(outputFilePath + "?openMode=create"))
                {
                    sheridanOutputDataSet.Add<string[]>("station", station, "station"); ;
                    sheridanOutputDataSet.Add<Single[]>("lat", lat, "station");
                    sheridanOutputDataSet.Add<Single[]>("lon", lon, "station");
                }


            return ReturnMessage;
        }

        //napsSaveLatLongFromFile

        public string napsSaveLatLongFromFile()
        {
            string ReturnMessage = "ok";
            //the idea here is to loop through all the codes in the file

            string napsStationsFilePath = AppFilesFolder + "/naps-stations.txt";  //tab delimited
            string[] allNapsStationsLines = System.IO.File.ReadAllLines(napsStationsFilePath);

            string[] station = new string[allNapsStationsLines.Length];
            string[] stationName = new string[allNapsStationsLines.Length];
            string[] stationInfo = new string[allNapsStationsLines.Length];

            Single[] lat = new Single[allNapsStationsLines.Length];
            Single[] lon = new Single[allNapsStationsLines.Length];

            Single[] latDB = new Single[allNapsStationsLines.Length];
            Single[] lonDB = new Single[allNapsStationsLines.Length];

            Single[] latOfficial = new Single[allNapsStationsLines.Length];
            Single[] lonOfficial = new Single[allNapsStationsLines.Length];

            double[] dist = new double[allNapsStationsLines.Length];
            string[] lines4QAFile = new string[allNapsStationsLines.Length];

            string errors = "";
            int numberofbadlines = 0;

      
            string slat, slon;
            for (int l = 1; l < allNapsStationsLines.Length; l++)
            {

                var line = allNapsStationsLines[l].Split('\t');     
                JassLatLon originalLatLon = new JassLatLon();
                try
                {
                    station[l] = line[0];
                    stationName[l] = line[1];
                    stationInfo[l] = line[6] + "--" + line[7] + "--" + line[8] + "--" + line[9] + "--" + line[11];
                    slat = line[13];
                    slon = line[14];
                    lat[l] = Convert.ToSingle(line[13]);
                    lon[l] = Convert.ToSingle(line[14]);
                }
                catch (Exception e)
                {
                    numberofbadlines++;
                    errors += "," + l;

                }
            }

            string outputFilePath = AppDataFolder + "\\naps_stations.nc";
            using (var napsOutputDataSet = DataSet.Open(outputFilePath + "?openMode=create"))
            {
                napsOutputDataSet.Add<string[]>("station", station, "station");
                napsOutputDataSet.Add<string[]>("stationName", stationName, "station");
                napsOutputDataSet.Add<string[]>("stationInfo", stationInfo, "station");
                napsOutputDataSet.Add<Single[]>("lat", lat, "station");
                napsOutputDataSet.Add<Single[]>("lon", lon, "station");
            }


            return ReturnMessage;
        }

        public string sheridanSaveLatLongFrom_DB_Or_OriginalLatLons()
        {
            string ReturnMessage = "ok";
            //the idea here is to loop through all the codes in the file

            string sherindanOriginalStationsFilePath = AppFilesFolder + "/allsta.csv";
            string[] originallines = System.IO.File.ReadAllLines(sherindanOriginalStationsFilePath);

            string sherindanStationsFilePath = AppFilesFolder + "/sheridan-stations.csv";
            string[] allSheridanStationsLines = System.IO.File.ReadAllLines(sherindanStationsFilePath);

            string[] station = new string[allSheridanStationsLines.Length];

            Single[] lat = new Single[allSheridanStationsLines.Length];
            Single[] lon = new Single[allSheridanStationsLines.Length];

            Single[] latDB = new Single[allSheridanStationsLines.Length];
            Single[] lonDB = new Single[allSheridanStationsLines.Length];

            Single[] latOfficial = new Single[allSheridanStationsLines.Length];
            Single[] lonOfficial = new Single[allSheridanStationsLines.Length];

            double[] dist = new double[allSheridanStationsLines.Length];
            string[] lines4QAFile = new string[allSheridanStationsLines.Length];

            List<JassLatLon> latlons = db.JassLatLons.Where(l => l.StationCode.Length == 3).ToList();
            Dictionary<string, JassLatLon> DBlatlonsDic = new Dictionary<string, JassLatLon>();
            string code;

            foreach (var latlon in latlons)
            {
                code = latlon.StationCode.Substring(0, 3);
                DBlatlonsDic.Add(code, latlon);
            }

            Dictionary<string, JassLatLon> originaLatLonsDic = new Dictionary<string, JassLatLon>();

            for (int l = 0; l < originallines.Length; l++)
            {
                
                var line = originallines[l].Replace("  "," ").Replace("  "," ").Split(' ');
                code = line[0];
                if (line.Length > 3) throw new Exception("line.Length > 3 is not true");
                JassLatLon originalLatLon = new JassLatLon();

                originalLatLon.StationCode = code;
                originalLatLon.Lat = Convert.ToDouble(line[1]);
                originalLatLon.Lon = Convert.ToDouble(line[2]);
                originaLatLonsDic.Add(code,originalLatLon);
            }

            for (int l = 0; l < allSheridanStationsLines.Length; l++)
            {
                var line = allSheridanStationsLines[l].Split('\t');
                code = line[1];
                station[l] = code;

                if (originaLatLonsDic.ContainsKey(code))
                {

                    latOfficial[l] = Convert.ToSingle(originaLatLonsDic[code].Lat);
                    lonOfficial[l] = Convert.ToSingle(originaLatLonsDic[code].Lon);

                    latDB[l] = Convert.ToSingle(DBlatlonsDic[code].Lat);
                    lonDB[l] = Convert.ToSingle(DBlatlonsDic[code].Lon);

                    var distance = dist[l] = Math.Abs(HaversineDistance(latDB[l], lonDB[l], latOfficial[l], lonOfficial[l]));

                    if (distance > 30)
                    {
                        lat[l] = latOfficial[l];
                        lon[l] = lonOfficial[l];
                    }
                    else {
                        lat[l] = latDB[l];
                        lon[l] = lonDB[l];
             
                    }


                }
                else
                {

                    latDB[l] = Convert.ToSingle(DBlatlonsDic[code].Lat);
                    lonDB[l] = Convert.ToSingle(DBlatlonsDic[code].Lon);

                    lat[l] = Convert.ToSingle(DBlatlonsDic[code].Lat);
                    lon[l] = Convert.ToSingle(DBlatlonsDic[code].Lon);

                    var distance = dist[l] = -999999;

                    latOfficial[l] = -999999;
                    lonOfficial[l] = -999999;


                 }

                lines4QAFile[l] = code + "," + latOfficial[l] + "," + lonOfficial[l] + "," + latDB[l] + "," + lonDB[l] + "," + dist[l] + "," + lat[l] + "," + lon[l]; 



            }

            string outputQAFilePath = AppFilesFolder + "\\sheridanStationsLatLonQA.csv";
            File.WriteAllLines(outputQAFilePath, lines4QAFile);

            string outputFilePath = AppDataFolder + "\\sheridan_stations.nc";
            using (var sheridanOutputDataSet = DataSet.Open(outputFilePath + "?openMode=create"))
            {
                sheridanOutputDataSet.Add<string[]>("station", station, "station"); ;
                sheridanOutputDataSet.Add<Single[]>("lat", lat, "station");
                sheridanOutputDataSet.Add<Single[]>("lon", lon, "station");
                sheridanOutputDataSet.Add<Single[]>("latDB", latDB, "station");
                sheridanOutputDataSet.Add<Single[]>("lonDB", lonDB, "station");
                sheridanOutputDataSet.Add<Single[]>("latOfficial", latOfficial, "station");
                sheridanOutputDataSet.Add<Single[]>("lonOfficial", lonOfficial, "station");
                sheridanOutputDataSet.Add<double[]>("dist", dist, "station");
            }


            return ReturnMessage;
        }

        public class SheridanInspect
        {
            public string[] station { get; set; }
            public double[] time {get;set;}
            public Single[] lat {get;set;}
            public Single[] lon {get;set;}
            public Int16[,] sher { get; set; }
        }

        public SheridanInspect sheridanInspectFile()
        {
                SheridanInspect si = new SheridanInspect();

                string outputFilePath = AppDataFolder + "\\sherindan.nc";
                using (var sheridanOutputDataSet = DataSet.Open(outputFilePath + "?openMode=open"))
                {

                    si.sher = sheridanOutputDataSet.GetData<Int16[,]>("sher");
                    si.time = sheridanOutputDataSet.GetData<double[]>("time");
                    si.lat = sheridanOutputDataSet.GetData<Single[]>("lat");
                    si.lon = sheridanOutputDataSet.GetData<Single[]>("lon");
                    si.station = sheridanOutputDataSet.GetData<string[]>("station");
                }

                return si;
        }

        public DateTime getDataFromString(string dateTime){

            var startDayString = dateTime;
            int startDayYear = Convert.ToInt16(startDayString.Substring(0, 4));
            int startDayMonth = Convert.ToInt16(startDayString.Substring(4, 2));
            int startDayDay = Convert.ToInt16(startDayString.Substring(6, 2));

            DateTime dataStartDay = new DateTime(startDayYear, startDayMonth, startDayDay);

            return dataStartDay;
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

        public string get_big_NetCDF_by_http2(string url, string workingDirectoryPath)
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

                //hardcoded for now
                string loginUrl = "https://rda.ucar.edu/cgi-bin/login?email=pelustondo@envirolytic.com&passwd=iswhatyou2!&action=login";

       
                HttpWebRequest loginRequest = (HttpWebRequest)HttpWebRequest.Create(loginUrl);
                loginRequest.CookieContainer = new CookieContainer();

                loginRequest.Method = WebRequestMethods.Http.Get;
                HttpWebResponse loginResponse = (HttpWebResponse)loginRequest.GetResponse();
                var cookies = new CookieContainer();
                cookies.Add(loginResponse.Cookies);


                HttpWebRequest request = (HttpWebRequest)HttpWebRequest.Create(url);
                request.CookieContainer = cookies;
                request.Method = WebRequestMethods.Http.Get;
             
                WebResponse response = (WebResponse)request.GetResponse();
 

                Stream responseStream = response.GetResponseStream();
                using (FileStream file = File.Create(downloadedFilePath))
                {
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
        using (FileStream file = File.Create(downloadedFilePath))
        {
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
        }


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

            string connectionString = ConfigurationManager.AppSettings[storageConnectionString];
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

            string connectionString = ConfigurationManager.AppSettings[storageConnectionString];
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

        public string fileNameBuilderByDay(string variableName, int year, int month, int day){

            var dayString = (day).ToString();
            if (dayString.Length < 2) dayString = "0" + dayString;

            var monthString = month.ToString();
            if (monthString.Length < 2) monthString = "0" + monthString;

            return variableName + "_" + year + "_" + monthString + "_" + dayString;
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

                string connectionString = ConfigurationManager.AppSettings[storageConnectionString];
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

               string connectionString = ConfigurationManager.AppSettings[storageConnectionString];
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

                using (var dataset = DataSet.Open(downloadedFilePath))
                {

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
                    }//for each var
                }//using

            }//try
            catch (Exception e)
            {
                return "Error: " + e.Message;
            }//catch
            return schemaString;
        }

        public class VariableValueModel
        {
            public JassVariable JassVariable { get; set; }
            public int? JassVariableID { get; set; }
            public string fileName { get; set; }
            public string schema { get; set; }
            public string keyVariable { get; set; }
            public string generalMetadata { get; set; } 
            public JassGrid JassGrid { get; set; }
            public int? JassGridID { get; set; }
            public int year { get; set; }
            public int yearIndex { get; set; }
            public int monthIndex { get; set; }
            public int dayIndex { get; set; }
            public int stepIndex { get; set; }
            public int levelIndex { get; set; }
            public int numberOfDays { get; set; }
            public int stepsInADay { get; set; }
            public int maxXaroundLoc { get; set; }
            public int maxYaroundLoc { get; set; }
            public string variableName { get; set; }
            public JassGridValues gridValues { get; set; }
            public string message { get; set; }
            public DateTime startingDate { get; set; }
            public DateTime endingDate { get; set; }
            public DateTime startingDate2 { get; set; }
            public DateTime endingDate2 { get; set; }
            public Boolean envirolyticFile { get; set; }
            public JassColorCode colorCode { get; set; }

            public int? JassLatLonID { get; set; }
            public JassLatLon JassLatLon { get; set; }

        }

        public VariableValueModel AnalyzeFileOnDisk(string filename)
        {
            VariableValueModel vm = new VariableValueModel();
            vm.fileName = filename;
            string downloadedFilePath = AppDataFolder + "\\" + filename;
            string schemaString = "";
            string dimensionsString = "";
            vm.message = "file structure could not be mapped to a known grid";
            vm.envirolyticFile = false;
            var jassgrids = db.JassGrids;


            try
            {

                using (var dataset = DataSet.Open(downloadedFilePath))
                {

                    var schema = dataset.GetSchema();

                    foreach (var v in schema.Variables)
                    {
                        if (v.Name != "" && v.Dimensions.Count > 2)
                        {
                            vm.keyVariable = v.Name;
                            schemaString += v.Name;
                            dimensionsString = "  ";
                            foreach (var d in v.Dimensions)
                            {
                                dimensionsString += "(" + d.Name + "," + d.Length + ")";
                            }
                            schemaString += dimensionsString;
                        }
                    }//for each var
                    vm.generalMetadata = "";
                    foreach (var m in dataset.Metadata)
                    {
                        vm.generalMetadata = "" + m.Key + ":" + m.Value;
                        if (m.Key == "title" && (string)m.Value == "8x Daily NARR"){
                            vm.JassGrid = db.JassGrids.Where(g => g.Name == "NARR-32km-3hr-ByDay").First();
                            vm.JassGridID = vm.JassGrid.JassGridID;
                        }
                       
                    }

                    if (downloadedFilePath.Contains("_macc_")){
                            vm.JassGrid = db.JassGrids.Where(g => g.Name.Contains("MACC")).First();
                            vm.JassGridID = vm.JassGrid.JassGridID;
                    }

                    if (downloadedFilePath.Contains("sheridan"))
                    {
                        vm.JassGrid = db.JassGrids.Where(g => g.Name.Contains("SHER")).First();
                        vm.JassGridID = vm.JassGrid.JassGridID;
                    }
                    //rda.ucar.edu for MACC

                    if (downloadedFilePath.Contains("rda.ucar.edu"))
                    {
                        vm.JassGrid = db.JassGrids.Where(g => g.Name.Contains("CFSR")).First();
                        vm.JassGridID = vm.JassGrid.JassGridID;
                    }

                    //horrible hack to make this work for now. need to add metadata to file.
                    if (dataset.Metadata.Count < 2 && vm.JassGrid == null)
                    {
                        vm.envirolyticFile = true;
                        vm.JassGrid = db.JassGrids.Where(g => g.Name == "NARR-32km-3hr-ByDay").First();
                        vm.JassGridID = vm.JassGrid.JassGridID;

                        if (schemaString.Contains("(level,5)"))
                        {
                            vm.JassGrid = db.JassGrids.Where(g => g.Name == "NARR-32km-3hr-5level-ByDay").First();
                            vm.JassGridID = vm.JassGrid.JassGridID;
                        }
                        else
                            if (schemaString.Contains("(level,29)"))
                            {
                                vm.JassGrid = db.JassGrids.Where(g => g.Name == "NARR-32km-3hr-29level-ByDay").First();
                                vm.JassGridID = vm.JassGrid.JassGridID;
                            }
                            else
                                if (schemaString.Contains("(level,23)"))
                                {
                                    vm.JassGrid = db.JassGrids.Where(g => g.Name == "NARR-32km-3hr-23level-ByDay").First();
                                    vm.JassGridID = vm.JassGrid.JassGridID;
                                }
                    }
                    try
                    {
                        if (vm.JassGrid.Type == "NARR")
                        {
                            double[] time = dataset.GetData<double[]>("time");
                            DateTime day1800 = DateTime.Parse("1800-01-01 00:00:00");



                            var beginingHours = time[0];
                            var endingHours = beginingHours + (time.Length-1) * 3;
                            var endingHours2 = time[time.Length - 1];

                            vm.endingDate = day1800.AddHours(endingHours);
                            vm.startingDate = vm.startingDate2 = day1800.AddHours(beginingHours);
                            vm.endingDate2 = day1800.AddHours(endingHours2);

                            if (vm.envirolyticFile)
                            {
                                //here we will hack the starting date.. based on file due to problem.
                                vm.startingDate2 = vm.startingDate;
                                vm.startingDate = getDateTimeFromFileName(filename);
                                vm.message += "file generated by envirolytic"; 
                                
                            }
                           
                            vm.year = vm.startingDate.Year;
                            vm.monthIndex = vm.startingDate.Month;
                            vm.dayIndex = vm.startingDate.Day;
                            vm.message = "successfully parsed as a NARR Grid";

                            if (vm.endingDate != vm.endingDate2) vm.message = " ending date discrepancy: " + vm.endingDate2.ToShortDateString();
                            if (vm.startingDate != vm.startingDate2) vm.message = " starting date discrepancy: " + vm.startingDate2.ToShortDateString();
                        } else
                            if (vm.JassGrid.Type == "MACC")
                            {
                                int[] time = dataset.GetData<int[]>("time");
                                DateTime day1900 = DateTime.Parse("1900-01-01 00:00:00");
                                var beginingHours = time[0];


                                vm.startingDate = vm.startingDate2 = day1900.AddHours(beginingHours);
                                var endingHours = beginingHours + (time.Length - 1) * 3;
                                var endingHours2 = time[time.Length - 1];

                                vm.endingDate = day1900.AddHours(endingHours);
                                
                                vm.endingDate2 = day1900.AddHours(endingHours2);

                                vm.year = vm.startingDate.Year;
                                vm.monthIndex = vm.startingDate.Month;
                                vm.dayIndex = vm.startingDate.Day;
                                vm.message = "successfully parsed as a MACC Grid";

                                if (vm.endingDate != vm.endingDate2) vm.message = " ending date discrepancy: " + vm.endingDate2.ToShortDateString();
                                if (vm.startingDate != vm.startingDate2) vm.message = " starting date discrepancy: " + vm.startingDate2.ToShortDateString();
                            }
                            else
                                if (vm.JassGrid.Type == "SHER")
                                {
                                    double[] time = dataset.GetData<double[]>("time");
                                    DateTime day2002 = DateTime.Parse("2002-01-01 00:00:00");
                                    var beginingHours = time[0];

                                    vm.startingDate = vm.startingDate2 = day2002;
                                    var endingHours = beginingHours + (time.Length - 1);
                                    var endingHours2 = time[time.Length - 1];

                                    vm.endingDate =  vm.endingDate2 = day2002.AddHours(endingHours2);

                                    vm.year = vm.startingDate.Year;
                                    vm.monthIndex = vm.startingDate.Month;
                                    vm.dayIndex = vm.startingDate.Day;
                                    vm.message = "successfully parsed as a SHERIDAN Grid";

                                    if (vm.endingDate != vm.endingDate2) vm.message = " ending date discrepancy: " + vm.endingDate2.ToShortDateString();
                                    if (vm.startingDate != vm.startingDate2) vm.message = " starting date discrepancy: " + vm.startingDate2.ToShortDateString();
                                }
                                else
                                    if (vm.JassGrid.Type == "CFSR")
                                    {
                                        Single[] time = dataset.GetData<Single[]>("time");

                                        string yearmonthstring = null;
                                        string yearstring = null;
                                        string monthstring = null;
                                        string daystring = null;

                                        int year = 0;
                                        int month = 0;
                                        int day = 0;

                                        //decode end date
                                        string yearmonthstring2 = null;
                                        string yearstring2 = null;
                                        string monthstring2 = null;
                                        string daystring2 = null;

                                        int year2 =0;
                                        int month2 =0;
                                        int day2 = 0;
                                        
                                        Boolean oldformat=true;

                                        try
                                        {
                                            //decode day from file name (if this is an old file
                                            yearmonthstring = filename.Substring(filename.IndexOf(".gdas.") + 6, 8);
                                            yearstring = yearmonthstring.Substring(0, 4);
                                            monthstring = yearmonthstring.Substring(4, 2);
                                            daystring = yearmonthstring.Substring(6, 2);
                                            
                                            year = Convert.ToInt32(yearstring);
                                            month = Convert.ToInt32(monthstring);
                                            day = Convert.ToInt32(daystring);

                                            //decode end date
                                            yearmonthstring2 = filename.Substring(filename.IndexOf(".grb2.") - 8, 8);
                                            yearstring2 = yearmonthstring2.Substring(0, 4);
                                            monthstring2 = yearmonthstring2.Substring(4, 2);
                                            daystring2 = yearmonthstring2.Substring(6, 2);

                                            year2 = Convert.ToInt32(yearstring2);
                                            month2 = Convert.ToInt32(monthstring2);
                                            day2 = Convert.ToInt32(daystring2);

                                        } catch(Exception){ oldformat=false;}

                                        if(!oldformat)
                                        {
                                            vm.JassGrid = db.JassGrids.Where(g => g.Name.Contains("CFSR") && g.Name.Contains("ByDay")).First();
                                            vm.JassGridID = vm.JassGrid.JassGridID;

                                            //decode day from file name (if this is an old file
                                            yearmonthstring = filename.Substring(filename.IndexOf("cdas1.") + 6, 8);
                                            yearstring = yearmonthstring.Substring(0, 4);
                                            monthstring = yearmonthstring.Substring(4, 2);
                                            daystring = yearmonthstring.Substring(6, 2);

                                            year = Convert.ToInt32(yearstring);
                                            month = Convert.ToInt32(monthstring);
                                            day = Convert.ToInt32(daystring);

                                            year2 = year;
                                            month2 = month;
                                            day2 = day;
                                        } 


                                        vm.startingDate = new DateTime(year, month, day);
                                        vm.endingDate = new DateTime(year2, month2, day2);

                                        vm.year = vm.startingDate.Year;
                                        vm.monthIndex = vm.startingDate.Month;
                                        vm.dayIndex = vm.startingDate.Day;
                                        vm.message = "successfully parsed as a CFSR Grid";

                                        if (vm.endingDate != vm.endingDate2) vm.message = " ending date discrepancy: " + vm.endingDate2.ToShortDateString();
                                        if (vm.startingDate != vm.startingDate2) vm.message = " starting date discrepancy: " + vm.startingDate2.ToShortDateString();
                                    }

                        if (vm.JassGrid != null) {
                            vm.stepsInADay = vm.JassGrid.StepsInDay;
                        }
                    }
                    catch (Exception e)
                    {
                        vm.message = e.Message;
                    }
                }//using



            }//try
            catch (Exception e)
            {
                vm.message = e.Message;
                return vm;
            }//catch


            vm.schema = schemaString;
            return vm;

        }

        public DateTime getDateTimeFromFileName(string filename)
        {
            int indexOf_1 = filename.IndexOf("_");
            string yearString = filename.Substring(indexOf_1+1, 4);
            string monthString = filename.Substring(indexOf_1+6, 2);
            string dayString = filename.Substring(indexOf_1+9, 2);

            int year = Convert.ToInt16(yearString);           
            int month = Convert.ToInt16(monthString);
            int day = Convert.ToInt16(dayString);

            DateTime result = new DateTime(year, month, day);
            return result;
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

                string connectionString = ConfigurationManager.AppSettings[storageConnectionString];
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

                string connectionString = ConfigurationManager.AppSettings[storageConnectionString];
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

        #region Dashboard operations

        public List<JassVariableStatus> listVariableStatus()
        {
            List<JassVariableStatus> variableStatusList = new List<JassVariableStatus>();

            string connectionString = ConfigurationManager.AppSettings[storageConnectionString];
            CloudStorageAccount storageAccount = CloudStorageAccount.Parse(connectionString);
            CloudBlobClient blobClient = storageAccount.CreateCloudBlobClient();
            List<CloudBlobContainer> containers = blobClient.ListContainers().ToList<CloudBlobContainer>();

            //now I have the containers/variables... so I will loop on them

            JassVariableStatus variableStatus;

            foreach (var container in containers)
            {
                if (container.Name != "ftp" && container.Name != "trash")
                {
                    variableStatus = new JassVariableStatus(DateTime.Now.Year - 9, DateTime.Now.Year);
                    variableStatus.ContainerName = container.Name;

                    variableStatus.JassVariable = (from v in db.JassVariables where v.Name.ToLower() == container.Name select v).First();
                    variableStatus.VariableName = variableStatus.JassVariable.Name;

                    //now we need to see which days we actuall have, the idea will be to fill up this status structure
                    JassFileNameComponents dayMeasureNameComponents;

                    foreach (CloudBlockBlob dayMeasure in container.ListBlobs(useFlatBlobListing: true))
                    {
                        long size = dayMeasure.Properties.Length;
                        dayMeasureNameComponents = new JassFileNameComponents(dayMeasure.Uri.ToString());
                        variableStatus.countBlob(dayMeasureNameComponents, size);
                    }

                    variableStatus.calcuateStatus();

                    variableStatusList.Add(variableStatus);
                }
            }

            return variableStatusList;
        }


        #endregion

        #region Blob Operations

        public List<CloudBlobContainer> listContainers()
        {
            string connectionString = ConfigurationManager.AppSettings[storageConnectionString];
            CloudStorageAccount storageAccount = CloudStorageAccount.Parse(connectionString);
            CloudBlobClient blobClient = storageAccount.CreateCloudBlobClient();
            List<CloudBlobContainer> containers = blobClient.ListContainers().ToList<CloudBlobContainer>(); 
            return containers;
        }

        public List<CloudBlockBlob> listBlobs(string containerName)
        {

            List<CloudBlockBlob> blobs = new List<CloudBlockBlob>();

            string connectionString = ConfigurationManager.AppSettings[storageConnectionString];
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

            string connectionString = ConfigurationManager.AppSettings[storageConnectionString];
            CloudStorageAccount storageAccount = CloudStorageAccount.Parse(connectionString);

            CloudBlobClient blobClient = storageAccount.CreateCloudBlobClient();
 
            CloudBlobContainer container = blobClient.GetContainerReference(containerName);

      
            container.Delete();


            return "Container Deleted";

        }

        public class DeleteBlobRangeModel
        {
            public string VariableName { get; set; }  //same as container name
            public DateTime startDate { get; set; }
            public DateTime endDate { get; set; }
            public List<string> blobNames { get; set; }
            public Boolean confirmed { get; set; }
        }


        public string createTimestamp()
        {
            DateTime now = DateTime.Now;
            return now.Year + "_" + now.Month + "_" + now.Day + "_" + now.Hour + "_" + now.Minute;
        }
        public string deleteBlobRange(DeleteBlobRangeModel blobRange )
        {
            DateTime now = DateTime.Now;
            string timeStamp = createTimestamp() + "___";  
            foreach (var blobName in blobRange.blobNames)
            {
                string filePath = AppDataFolder + "/" +blobName;
                downloadBlob(blobRange.VariableName.ToLower(), blobName, filePath);
                uploadBlob("trash", timeStamp + blobName, filePath);
                deleteBlob(blobName, blobRange.VariableName.ToLower());
            }
           


            return "Blob Range Trashed";

        }

        public string deleteBlob(string blobName, string containerName)
        {


            List<CloudBlockBlob> blobs = new List<CloudBlockBlob>();

            string connectionString = ConfigurationManager.AppSettings[storageConnectionString];
            CloudStorageAccount storageAccount = CloudStorageAccount.Parse(connectionString);

            CloudBlobClient blobClient = storageAccount.CreateCloudBlobClient();
            // Retrieve a reference to a container. 
            CloudBlobContainer container = blobClient.GetContainerReference(containerName);

            CloudBlockBlob blockBlob = container.GetBlockBlobReference(blobName);

            blockBlob.Delete();


            return "Blob Deleted";

        }

        public string uploadBlob(string blobContainerIn, string blobName, string filePath)
        {
            DateTime Start = DateTime.Now;
            string blobContainer = blobContainerIn.ToLower();
            // Retrieve storage account from connection string.
            CloudStorageAccount storageAccount = CloudStorageAccount.Parse(
                CloudConfigurationManager.GetSetting(storageConnectionString));

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
                CloudConfigurationManager.GetSetting(storageConnectionString));

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

        public List<string> listFiles_in_AppData()
        {
            List<string> response = new List<string>();


            string[] array1 = Directory.GetFiles(AppDataFolder);

            return array1.ToList();
        
         }


        public List<string> listFiles_in_AppTempFiles()
        {
            List<string> response = new List<string>();


            string[] array1 = Directory.GetFiles(AppTempFilesFolder);

            return array1.ToList();

        }

        public List<string> listTables()
        {
            List<string> response = new List<string>();


            string connectionString = ConfigurationManager.AppSettings[storageConnectionString];
            CloudStorageAccount storageAccount = CloudStorageAccount.Parse(connectionString);

            CloudTableClient tableClient = storageAccount.CreateCloudTableClient();

            var tables = tableClient.ListTables();

            foreach (var t in tables)
            {
                response.Add(t.Name);
            }

            return response;
        }


        public void DownloadFile2DiskIfNotThere(string fileName, string filePath)
        {

            Boolean fileOnDisk = File.Exists(filePath);

            if (!fileOnDisk)
            {
                int index = fileName.IndexOf("_");

                string fileNamePrefix;

                if (index > 0)
                {
                    fileNamePrefix = fileName.Substring(0, index).ToLower();
                }
                else {
                    fileNamePrefix = "ftp";
                }
                string BlobName = fileName;

                string containerName = fileNamePrefix;
                if (fileNamePrefix == "http") containerName = "ftp";

                downloadBlob(containerName,BlobName,filePath);
            }

        }

        public void DownloadFile2DiskIfNotThere(string containerName, string fileName, string filePath)
        {

            Boolean fileOnDisk = File.Exists(filePath);

            if (!fileOnDisk)
            {
                downloadBlob(containerName, fileName, filePath);
            }

        }

        public List<string> listNetCDFValues(string fileName)
        {
   
            List<string> listOfValues = new List<string>();
            string filePath = AppDataFolder + "/" + fileName;
            DownloadFile2DiskIfNotThere(fileName, filePath);
            using (var dataset1 = DataSet.Open(filePath + "?openMode=open"))
            {
                var schema1 = dataset1.GetSchema();
                int maxSample = 10;
                //first let's select the key variable by having various dimensions
                VariableSchema keyVariable = null;
                Boolean hasLevel = false;

                foreach (var v in schema1.Variables)
                {
                    if (v.Dimensions.Count > 2)
                    {
                        keyVariable = v;
                    }
                    if (v.Name == "level")
                    {
                        hasLevel = true;
                    }
                }

                Single[] y = dataset1.GetData<Single[]>("y");
                Single[] x = dataset1.GetData<Single[]>("x");
                double[] time = dataset1.GetData<double[]>("time");
                Single[] level = new Single[1];
                if (hasLevel) level = dataset1.GetData<Single[]>("level");

                string outPutString = schema2string(schema1);

                listOfValues.Add(outPutString);

                if (hasLevel)
                {
                    Int16[, , ,] values = dataset1.GetData<Int16[, , ,]>(keyVariable.Name);
                    for (int tt = 0; tt < maxSample & tt < time.Length; tt++)
                    {
                        for (int ll = 0; ll < maxSample & ll < level.Length; ll++)
                        {
                            for (int xx = 0; xx < maxSample & xx < x.Length; xx++)
                            {
                                for (int yy = 0; yy < maxSample & yy < y.Length; yy++)
                                {
                                    outPutString = "time: " + tt + " level: " + ll + " x: " + xx + " y: " + yy + " value: " + values[tt, ll, xx, yy];
                                    listOfValues.Add(outPutString);
                                }
                            }
                        }
                    }
                }
                else
                {
                    Int16[, ,] values = dataset1.GetData<Int16[, ,]>(keyVariable.Name);
                    for (int tt = 0; tt < maxSample & tt < time.Length; tt++)
                    {
                        for (int xx = 0; xx < maxSample & xx < x.Length; xx++)
                        {
                            for (int yy = 0; yy < maxSample & yy < y.Length; yy++)
                            {
                                outPutString = "time: " + tt + " x: " + xx + " y: " + yy + " value: " + values[tt, xx, yy];
                                listOfValues.Add(outPutString);
                            }
                        }
                    }
                }

            }

            return listOfValues;
        }

        public JassGridValues GetDayValues(JassGrid grid, string fileName, DateTime startingDate, DateTime requestDate, int stepIndex, int levelIndex)
        {
            //HERE

            //IMPORTANT: The good thing about this function is that is generic... but the bad thing is that is to sample.
            //So, we are reducing the time and level dimension to only read the first 'Timesize" points and only 3 levels

            JassGridValues dayGridValues;
            dynamic values;
            string filePath = AppDataFolder + "/" + fileName;
            DownloadFile2DiskIfNotThere(fileName, filePath);
            string typeOfData = "Int16";
            using (var dataset1 = DataSet.Open(filePath + "?openMode=open"))
            {
                var schema1 = dataset1.GetSchema();

                //first let's select the key variable by having various dimensions
                VariableSchema keyVariable = null;
                Boolean hasLevel = false;

                foreach (var v in schema1.Variables)
                {
                    if (v.Dimensions.Count > 2)
                    {
                        keyVariable = v;
                        typeOfData = v.TypeOfData.Name;
                    }

                    if (grid.Type=="SHER" && v.Name=="sher")
                    {
                        keyVariable = v;
                        typeOfData = v.TypeOfData.Name;
                    }

                    if (v.Name == grid.Levelname)
                    {
                        hasLevel = true;
                    }
                }

                double scale_factor = 1;
                double add_offset = 0;
                double missing_value = 32767;
                double FillValue = 32766;

                try { scale_factor = Convert.ToDouble(keyVariable.Metadata["scale_factor"]); }
                catch (Exception e)
                {
                    var n = e;
                };
                try { add_offset = Convert.ToDouble(keyVariable.Metadata["add_offset"]); }
                catch (Exception e)
                {
                    var n = e;
                };
                try { missing_value = Convert.ToDouble(keyVariable.Metadata["missing_value"]); }
                catch (Exception e)
                {
                    var n = e;
                };

                try { FillValue = Convert.ToDouble(keyVariable.Metadata["FillValue"]); }
                catch (Exception e)
                {
                    var n = e;
                };
                try { FillValue = Convert.ToDouble(keyVariable.Metadata["_FillValue"]); }
                catch (Exception e)
                {
                    var n = e;
                }; 

                Single[] y = dataset1.GetData<Single[]>(grid.Yname);
                Single[] x = new Single[1];
                if (grid.Xsize > 0)
                {
                    x = dataset1.GetData<Single[]>(grid.Xname);
                }
                dynamic time;
                int timeIndex = 0;
                int timeLength = 0;

                DateTime commonStartingDate = new DateTime(startingDate.Year, startingDate.Month, startingDate.Day);
                int daysDifference = Convert.ToInt32((requestDate - commonStartingDate).TotalDays);
  
                timeIndex = daysDifference * grid.StepsInDay + stepIndex;
             
                Single[] level = new Single[1];


                dayGridValues = new JassGridValues(keyVariable.Metadata, keyVariable.Name, 1, 1, y.Length, x.Length);

                //how to get the scale/factor value.

                string outPutString = schema2string(schema1);

                for (int ll = 0; ll < 1; ll++)
                {
                    dayGridValues.measureMin[ll] = Double.MaxValue;
                    dayGridValues.measureMax[ll] = Double.MinValue;
                }

                if (hasLevel)
                {
                    if (typeOfData == "Int16")
                    {
                        values = dataset1.GetData<Int16[, , ,]>(keyVariable.Name,
                                 DataSet.Range(timeIndex, timeIndex),
                                 DataSet.Range(levelIndex, levelIndex),
                                 DataSet.Range(0, y.Length-1),
                                 DataSet.Range(0, x.Length-1) );
                    }
                    else
                    {
                        values = dataset1.GetData<Single[, , ,]>(keyVariable.Name,
                                 DataSet.Range(timeIndex, timeIndex),
                                 DataSet.Range(levelIndex, levelIndex),
                                 DataSet.Range(0, y.Length-1),
                                 DataSet.Range(0, x.Length-1) );
                    }
                    for (int tt = 0; tt < 1; tt++)
                    {
                        for (int ll = 0; ll < 1; ll++)
                        {
                            for (int yy = 0; yy < y.Length; yy++)
                            {
                                for (int xx = 0; xx < x.Length; xx++)
                                {
                                    if (values[tt, ll, yy, xx] != missing_value &&
                                        values[tt, ll, yy, xx] != FillValue)

                                        if ((values[tt, ll, yy, xx]) > 32765 || 
                                            (values[tt, ll, yy, xx]) < 0 )                                          
                                        {
                                            var tempvalue = values[tt, ll, yy, xx];
                                        }
                                    {
                                        dayGridValues.measure[tt, ll, yy, xx] = add_offset + scale_factor * values[tt, ll, yy, xx];

                                        if (dayGridValues.measure[tt, ll, yy, xx] > dayGridValues.measureMax[ll])
                                        {
                                            dayGridValues.measureMax[ll] = (double)dayGridValues.measure[tt, ll, yy, xx];
                                            dayGridValues.maxX[ll] = xx; dayGridValues.maxY[ll] = yy; dayGridValues.maxT[ll] = tt;
                                        }
                                        if (dayGridValues.measure[tt, ll, yy, xx] < dayGridValues.measureMin[ll])
                                        {
                                            dayGridValues.measureMin[ll] = (double)dayGridValues.measure[tt, ll, yy, xx];
                                            dayGridValues.minX[ll] = xx; dayGridValues.minY[ll] = yy; dayGridValues.minT[ll] = tt;
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
                else
                {
                    if (typeOfData == "Int16")
                    {
                        values = dataset1.GetData<Int16[, ,]>(keyVariable.Name,
                                 DataSet.Range(timeIndex, timeIndex),
                                 DataSet.Range(0, y.Length - 1),
                                 DataSet.Range(0, x.Length - 1));
                    }
                    else
                    {
                        try
                        {
                            values = dataset1.GetData<Single[, ,]>(keyVariable.Name,
                                 DataSet.Range(timeIndex, timeIndex),
                                 DataSet.Range(0, y.Length - 1),
                                 DataSet.Range(0, x.Length - 1));
                        }
                        catch (Exception e)
                        {
                            values = dataset1.GetData<Int16[,]>(keyVariable.Name,
                                       DataSet.Range(timeIndex, timeIndex),
                                       DataSet.Range(0, y.Length - 1));
                        }
                    }
                    for (int tt = 0; tt < 1; tt++)
                    {
                        for (int ll = 0; ll < 1; ll++)
                        {
                            for (int yy = 0; yy < y.Length; yy++)
                            {
                                for (int xx = 0; xx < x.Length; xx++)
                                {
                                    dynamic value;

                                    if (x.Length > 1) { value = values[tt, yy, xx]; }
                                    else {value = values[tt, yy];}

                                    if (value != missing_value &&
                                        value != FillValue)
                                    {
                                        dayGridValues.measure[tt, ll, yy, xx] = add_offset + scale_factor * value;
                                        //0.0500015563699208


                                        if (dayGridValues.measure[tt, ll, yy, xx] > dayGridValues.measureMax[ll])
                                        {
                                            dayGridValues.measureMax[ll] = (double)dayGridValues.measure[tt, ll, yy, xx];
                                            dayGridValues.maxX[ll] = xx; dayGridValues.maxY[ll] = yy; dayGridValues.maxT[ll] = tt;
                                        }
                                        if (dayGridValues.measure[tt, ll, yy, xx] < dayGridValues.measureMin[ll])
                                        {
                                            dayGridValues.measureMin[ll] = (double)dayGridValues.measure[tt, ll, yy, xx];
                                            dayGridValues.minX[ll] = xx; dayGridValues.minY[ll] = yy; dayGridValues.minT[ll] = tt;
                                        }
                                    }
                                }
                            }
                        }
                    }
                }

            }

            return dayGridValues;
        }

        public JassGridValues GetDayValues(string fileName) 
            //this method is old and may become obsolete. It assumes our data is formated with our own grid.
        {
           
            //HERE
            JassGridValues dayGridValues;
            string filePath = AppDataFolder + "/" + fileName;
            DownloadFile2DiskIfNotThere(fileName, filePath);
            using (var dataset1 = DataSet.Open(filePath + "?openMode=open"))
            {
                var schema1 = dataset1.GetSchema();

                //first let's select the key variable by having various dimensions
                VariableSchema keyVariable = null;
                Boolean hasLevel = false;
                string typeOfData = "Int16";
                foreach (var v in schema1.Variables)
                {
                    if (v.Dimensions.Count > 2)
                    {
                        keyVariable = v;
                        typeOfData = v.TypeOfData.Name;
                    }
                    if (v.Name == "level")
                    {
                        hasLevel = true;
                    }
                }

                double scale_factor = 1;
                double add_offset = 0;
                double missing_value = 32766;
                double FillValue = 32766;

                try { scale_factor = Convert.ToDouble(keyVariable.Metadata["scale_factor"]); }
                catch (Exception e) {
                    var n = e; };
                try { add_offset = Convert.ToDouble(keyVariable.Metadata["add_offset"]); }
                catch (Exception e) { 
                    var n = e; };
                try { missing_value = Convert.ToDouble(keyVariable.Metadata["missing_value"]); }
                catch (Exception e) { 
                    var n = e; };

                try { FillValue = Convert.ToDouble(keyVariable.Metadata["FillValue"]); }
                catch (Exception e) { 
                    var n = e; };
                try { FillValue = Convert.ToDouble(keyVariable.Metadata["_FillValue"]); }
                catch (Exception e) { 
                    var n = e; }; 

                Single[] y = dataset1.GetData<Single[]>("y");
                Single[] x = dataset1.GetData<Single[]>("x");
                double[] time = dataset1.GetData<double[]>("time");
                Single[] level = new Single[1];
                if (hasLevel) level = dataset1.GetData<Single[]>("level");
                int levelLength = level.Length;
                if (levelLength > 15) { levelLength = 15; }
                dayGridValues = new JassGridValues(keyVariable.Metadata, keyVariable.Name, time.Length, levelLength, y.Length, x.Length);

                //how to get the scale/factor value.

                string outPutString = schema2string(schema1);

                for (int ll = 0; ll < levelLength; ll++) { 
                    dayGridValues.measureMin[ll] = add_offset + scale_factor * 32768;
                    dayGridValues.measureMax[ll] = add_offset + scale_factor * (-32768); }
                dynamic values=null;
                if (hasLevel)
                {
                    if (typeOfData =="Int16" )  {values = dataset1.GetData<Int16[, , ,]>(keyVariable.Name);}
                    else if(typeOfData == "Single") { values = dataset1.GetData<Single[, , ,]>(keyVariable.Name);}
                    for (int tt = 0; tt < time.Length; tt++)
                    {
                        for (int ll = 0; ll < levelLength; ll++)
                        {
                            for (int yy = 0; yy < y.Length; yy++)
                            {
                                for (int xx = 0; xx < x.Length; xx++)
                                {
                                    if (values[tt, ll, yy, xx] != missing_value && 
                                        values[tt, ll, yy, xx] != FillValue)
                                    {
                                        dayGridValues.measure[tt, ll, yy, xx] = add_offset + scale_factor * values[tt, ll, yy, xx];
                                   
                                        if (dayGridValues.measure[tt, ll, yy, xx] > dayGridValues.measureMax[ll]) { 
                                            dayGridValues.measureMax[ll] = (double) dayGridValues.measure[tt, ll, yy, xx];
                                            dayGridValues.maxX[ll] = xx; dayGridValues.maxY[ll] = yy; dayGridValues.maxT[ll] = tt; 
                                        }
                                        if (dayGridValues.measure[tt, ll, yy, xx] < dayGridValues.measureMin[ll]) { 
                                            dayGridValues.measureMin[ll] = (double) dayGridValues.measure[tt, ll, yy, xx];
                                            dayGridValues.minX[ll] = xx; dayGridValues.minY[ll] = yy; dayGridValues.minT[ll] = tt;
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
                else
                {
                    if (typeOfData == "Int16") { values = dataset1.GetData<Int16[, ,]>(keyVariable.Name); }
                    else if (typeOfData == "Single") { values = dataset1.GetData<Single[, ,]>(keyVariable.Name); }

                    for (int tt = 0; tt < time.Length; tt++)
                    {
                        for (int ll = 0; ll < 1; ll++)
                        {
                            for (int yy = 0; yy < y.Length; yy++)
                            {
                                for (int xx = 0; xx < x.Length; xx++)
                                {
                                    if (values[tt, yy, xx] != missing_value && 
                                        values[tt, yy, xx] != FillValue)
                                    {
                                        dayGridValues.measure[tt, ll, yy, xx] = add_offset + scale_factor * values[tt, yy, xx];
                                        //0.0500015563699208


                                        if (dayGridValues.measure[tt, ll, yy, xx] > dayGridValues.measureMax[ll]) { 
                                            dayGridValues.measureMax[ll] = (double)dayGridValues.measure[tt, ll, yy, xx];
                                            dayGridValues.maxX[ll] = xx; dayGridValues.maxY[ll] = yy; dayGridValues.maxT[ll] = tt; 
                                        }
                                        if (dayGridValues.measure[tt, ll, yy, xx] < dayGridValues.measureMin[ll]) { 
                                            dayGridValues.measureMin[ll] = (double)dayGridValues.measure[tt, ll, yy, xx];
                                            dayGridValues.minX[ll] = xx; dayGridValues.minY[ll] = yy; dayGridValues.minT[ll] = tt;
                                        }
                                    }
                                }
                            }
                        }
                    }
                }

            }

            return dayGridValues;
        }

        public JassGridValues GetDayValues(string fileName, JassLatLon location, int levelIndex)
        //this method is old and may become obsolete. It assumes our data is formated with our own grid.
        {

            //HERE

            int locX = (int)location.narrX;  //this will throw an exception if not..
            int locY = (int)location.narrY;

            JassGridValues dayGridValues;
            string filePath = AppDataFolder + "/" + fileName;
            DownloadFile2DiskIfNotThere(fileName, filePath);
            using (var dataset1 = DataSet.Open(filePath + "?openMode=open"))
            {
                var schema1 = dataset1.GetSchema();

                //first let's select the key variable by having various dimensions
                VariableSchema keyVariable = null;
                Boolean hasLevel = false;
                string typeOfData = "Int16";

                foreach (var v in schema1.Variables)
                {
                    if (v.Dimensions.Count > 2)
                    {
                        keyVariable = v;
                        typeOfData = v.TypeOfData.Name;
                    }
                    if (v.Name == "level")
                    {
                        hasLevel = true;
                    }
                }

                double scale_factor = 1;
                double add_offset = 0;
                double missing_value = 32766;
                double FillValue = 32766;

                try { scale_factor = Convert.ToDouble(keyVariable.Metadata["scale_factor"]); }
                catch (Exception e)
                {
                    var n = e;
                };
                try { add_offset = Convert.ToDouble(keyVariable.Metadata["add_offset"]); }
                catch (Exception e)
                {
                    var n = e;
                };
                try { missing_value = Convert.ToDouble(keyVariable.Metadata["missing_value"]); }
                catch (Exception e)
                {
                    var n = e;
                };

                try { FillValue = Convert.ToDouble(keyVariable.Metadata["FillValue"]); }
                catch (Exception e)
                {
                    var n = e;
                };
                try { FillValue = Convert.ToDouble(keyVariable.Metadata["_FillValue"]); }
                catch (Exception e)
                {
                    var n = e;
                };

                Single[] y = dataset1.GetData<Single[]>("y");
                Single[] x = dataset1.GetData<Single[]>("x");
                double[] time = dataset1.GetData<double[]>("time");
                Single[] level = new Single[1];
                dynamic values;
                if (hasLevel) level = dataset1.GetData<Single[]>("level");

                dayGridValues = new JassGridValues(keyVariable.Metadata, keyVariable.Name, time.Length, level.Length, 1, 1);

                //how to get the scale/factor value.

                string outPutString = schema2string(schema1);

                for (int ll = 0; ll < level.Length; ll++)
                {
                    dayGridValues.measureMin[ll] = add_offset + scale_factor * 32768;
                    dayGridValues.measureMax[ll] = add_offset + scale_factor * (-32768);
                }

                if (hasLevel)
                {
                    if (typeOfData=="Int16"){
                    values = dataset1.GetData<Int16[,,]>(keyVariable.Name,
     DataSet.FromToEnd(0), /* time*/
     DataSet.ReduceDim(levelIndex), /* level */
     DataSet.Range(locY, locY + 1),
     DataSet.Range(locX, locX + 1));
                    } else{
                    values = dataset1.GetData<Single[,,]>(keyVariable.Name,
     DataSet.FromToEnd(0), /* time*/
     DataSet.ReduceDim(levelIndex), /* level */
     DataSet.Range(locY, locY + 1),
     DataSet.Range(locX, locX + 1));                                   
                }

                    for (int tt = 0; tt < time.Length; tt++)
                    {
                            for (int yy= 0; yy < 1; yy++)
                            {
                                for (int xx = 0; xx < 1; xx++)
                                {
                                    if (values[tt, yy, xx] != missing_value &&
                                        values[tt, yy, xx] != FillValue)
                                    {
                                        dayGridValues.measure[tt, 0, yy, xx] = add_offset + scale_factor * values[tt, yy, xx];

                                        if (dayGridValues.measure[tt, 0, yy, xx] > dayGridValues.measureMax[0])
                                        {
                                            dayGridValues.measureMax[0] = (double)dayGridValues.measure[tt, 0, yy, xx];
                                            dayGridValues.maxX[0] = xx; dayGridValues.maxY[0] = yy; dayGridValues.maxT[0] = tt;
                                        }
                                        if (dayGridValues.measure[tt, 0, yy, xx] < dayGridValues.measureMin[0])
                                        {
                                            dayGridValues.measureMin[0] = (double)dayGridValues.measure[tt, 0, yy, xx];
                                            dayGridValues.minX[0] = xx; dayGridValues.minY[0] = yy; dayGridValues.minT[0] = tt;
                                        }
                                    }
                                }
                            }
                    }
                }
                else
                {
                    if (typeOfData == "Int16")
                    {
                        values = dataset1.GetData<Int16[, ,]>(keyVariable.Name,
                            DataSet.FromToEnd(0), /* time*/
                            DataSet.Range(locY, locY + 1),
                            DataSet.Range(locX, locX + 1));
                    }
                    else {
                        values = dataset1.GetData<Single[, ,]>(keyVariable.Name,
        DataSet.FromToEnd(0), /* time*/
        DataSet.Range(locY, locY + 1),
        DataSet.Range(locX, locX + 1));
                    
                    }
                    for (int tt = 0; tt < time.Length; tt++)
                    {
                        for (int ll = 0; ll < 1; ll++)
                        {
                            for (int yy = 0; yy < 1; yy++)
                            {
                                for (int xx = 0; xx < 1; xx++)
                                {
                                    if (values[tt, yy, xx] != missing_value &&
                                        values[tt, yy, xx] != FillValue)
                                    {
                                        dayGridValues.measure[tt, ll, yy, xx] = add_offset + scale_factor * values[tt, yy, xx];
                                        //0.0500015563699208


                                        if (dayGridValues.measure[tt, ll, yy, xx] > dayGridValues.measureMax[ll])
                                        {
                                            dayGridValues.measureMax[ll] = (double)dayGridValues.measure[tt, ll, yy, xx];
                                            dayGridValues.maxX[ll] = xx; dayGridValues.maxY[ll] = yy; dayGridValues.maxT[ll] = tt;
                                        }
                                        if (dayGridValues.measure[tt, ll, yy, xx] < dayGridValues.measureMin[ll])
                                        {
                                            dayGridValues.measureMin[ll] = (double)dayGridValues.measure[tt, ll, yy, xx];
                                            dayGridValues.minX[ll] = xx; dayGridValues.minY[ll] = yy; dayGridValues.minT[ll] = tt;
                                        }
                                    }
                                }
                            }
                        }
                    }
                }

            }

            return dayGridValues;
        }

        public List<string> listTableValues(string tableName)
        {
            List<string> response = new List<string>();


            string connectionString = ConfigurationManager.AppSettings[storageConnectionString];
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

        public bool cleanAppData()
        {
            foreach(string fileName in Directory.GetFiles(AppDataFolder)){
                File.Delete(fileName);
            }

            return true;
        }

        public bool cleanAppTempFiles()
        {
            foreach (string fileName in Directory.GetFiles(AppTempFilesFolder))
            {
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
            string connectionString = ConfigurationManager.AppSettings[storageConnectionString];
            CloudStorageAccount storageAccount = CloudStorageAccount.Parse(connectionString);

            CloudTableClient tableClient = storageAccount.CreateCloudTableClient();

            CloudTable table = tableClient.GetTableReference(tableName);

            table.DeleteIfExists();

            return true;
        }

        #region External API



        public class ENIResponse<T>
        {
            public T Data;
            public Boolean Status;
            public string Message;
        }

        public class ENIVariable
        {
            public string Name;           
        }

        public ENIResponse<List<ENIVariable>> ENIGetAllVariables()
        {
            List<CloudBlobContainer> listOfContainers = listContainers();

            var response = new ENIResponse<List<ENIVariable>>();

            foreach (var container in listOfContainers)
            {
                response.Data.Add(new ENIVariable { Name = container.Name });
            }

            return response;
        }

        #endregion


    }
}
