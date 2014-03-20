
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
        public string AppFilesFolder;
        public string ServerNameJass;
        public string storageConnectionString;
        DateTime startTotalTime = DateTime.UtcNow;
        DateTime endTotalTime = DateTime.UtcNow;
        public static JassRGB[] colors = getColors();

        public JassWeatherAPI(string ServerNameIn, string appDataFolder, string storageConnectionStringIn){
            this.storageConnectionString = storageConnectionStringIn;
            this.AppDataFolder = appDataFolder;
            this.AppFilesFolder = appDataFolder + "\\..\\App_Files";
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

        public string processSource(JassBuilder builder, int year, int month, Boolean upload, Boolean overWrite, JassBuilderLog builderAllLog)
        {
            //this method will be idempotent... if nothing to do does nothing.
            //this method is to help processBulder and will produce the file on disk
            //the idea is that, first, it will check if the file is already there.
            //unless overWrite is set of true which means that it will preprocess anyway.

            //Check whether the file is on disk

                APIRequest source = builder.APIRequest;
                string url = replaceURIPlaceHolders(source.url, year, month);

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

                        get_big_NetCDF_by_ftp2(url, AppDataFolder);

                    }
                }

                //so, here we know the file is on dis for sure (unless there is an error)
                if (upload && blobAccess)
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
            return d; 
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

        public static JassMaccNarrGridsCombo MapGridNarr2Macc(JassMaccNarrGridsCombo gc)
        {
            JassBuilder builder = new JassBuilder();
            DateTime start = DateTime.Now;
            JassWeatherAPI apiCaller2 = new JassWeatherAPI("","","");
            JassBuilderLog builderLog = apiCaller2.createBuilderLog(builder, "mapGridStart", "", "", new TimeSpan(), true);

            double minDistance, minDistance2, minDistance3, minDistance4;
            for (int y = gc.narrYMin; y < gc.narrYMax; y++)
            {
                JassBuilderLog builderLog2 = apiCaller2.createBuilderLog(builder, "mapGridY:" + y, "", "", DateTime.Now - start , true);
                start = DateTime.Now;
                for (int x = gc.narrXMin; x < gc.narrXMax; x++)
                {
                    minDistance = 2000; minDistance2 = 2000; minDistance3 = 2000; minDistance4 = 2000;
                    for (int lat = gc.maccLatMin; lat < gc.maccLatMax; lat++)   //161
                    {
                         for (int lon = gc.maccLonMin; lon < gc.maccLonMax; lon++)  //320
                         {
                               double distance = HaversineDistance(gc.maccLat[lat], gc.maccLon[lon], gc.narrLat[y, x], gc.narrLon[y, x]);
                               if (distance < minDistance) { 
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
                               } else
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
                                   } else
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
            string fileNameMacc = replaceURIPlaceHolders(fileNameMaccTemp, year, month);
            string fileNameNarr = replaceURIPlaceHolders("Narr_Grid.nc", year, month);

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
                double[] narrTime = narrDataSet.GetData<double[]>("time");
     
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
                        throw new Exception("maccDayStart.Year != year || maccDayStart.Month != month");
                    }

                    double narrDayStartHours = (narrDayStart - day1800).TotalHours;

                    double narrDayHours = narrDayStartHours;
                    for (int t = 0; t < maccTime.Length; t++)
                    {
                         maccNarrTime[t] = narrDayHours;
                         narrDayHours += 3; 
                    }

                    for (int t = 0; t < maccTime.Length; t++)
                    {
                        if (maccNarrTime[t] != narrTime[t])
                        {
                            var crap = 1;
                        }
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
                                foreach (var attr in narrVars["time"]) { if (attr.Key != "Name") outputDataSet.PutAttr("time", attr.Key, attr.Value); }
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
        public string processGridMappingCFSRToNarr(int year, int month, int weeky, string fileNameMaccTemp)
        {
            string fileNameMacc = replaceURIPlaceHolders(fileNameMaccTemp, year, month);
            string fileNameNarr = replaceURIPlaceHolders("Narr_Grid.nc", year, month);

            JassMaccNarrGridsCombo gc = new JassMaccNarrGridsCombo();
            string maccFile = AppDataFolder + "/" + fileNameMacc;
            string narrFile = AppFilesFolder + "/" + fileNameNarr;
            string mapFile = AppFilesFolder + "/Narr_2_CFSR_Grid_Mapper.nc";

            string smonth = (month < 10) ? "0" + month : "" + month;
            string outputFileName = null;
            string outputFilePath = null;

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
                double[] narrTime = narrDataSet.GetData<double[]>("time");

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
                        throw new Exception("maccDayStart.Year != year || maccDayStart.Month != month");
                    }

                    double narrDayStartHours = (narrDayStart - day1800).TotalHours;

                    double narrDayHours = narrDayStartHours;
                    for (int t = 0; t < maccTime.Length; t++)
                    {
                        maccNarrTime[t] = narrDayHours;
                        narrDayHours += 3;
                    }

                    for (int t = 0; t < maccTime.Length; t++)
                    {
                        if (maccNarrTime[t] != narrTime[t])
                        {
                            var crap = 1;
                        }
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
                                        outputVariable[t, y, x] = (Int16)interpolateValue(t, y, x, maccVariable, gc, missingValue, fillValue);
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
                            foreach (var attr in narrVars["time"]) { if (attr.Key != "Name") outputDataSet.PutAttr("time", attr.Key, attr.Value); }
                            outputDataSet.Add<Single[]>("y", narrY, "y");
                            foreach (var attr in narrVars["y"]) { if (attr.Key != "Name") outputDataSet.PutAttr("y", attr.Key, attr.Value); }
                            outputDataSet.Add<Single[]>("x", narrX, "x");
                            foreach (var attr in narrVars["y"]) { if (attr.Key != "Name") outputDataSet.PutAttr("x", attr.Key, attr.Value); }
                            outputDataSet.Add<Int16[, ,]>(VariableName, outputVariable, "time", "y", "x");
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


        public class SmartGridMap
        {
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
        }

        public SmartGridMap getMapComboFromMapFile(string mapfile)
        {
            string mapFilePath = AppFilesFolder + "/" + mapfile;
            SmartGridMap sgm = new SmartGridMap();

            using (var mapDataSet = DataSet.Open(mapFilePath + "?openMode=open"))
            {

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


                for (int y = 128; y < 136; y++)
                {
                    for (int x = 230; x < 250; x++)
                    {
                        var mapDistance = sgm.mapDistance[y,x];
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

        public JassMaccNarrGridsCombo MapGridNarr2GridFromFile(string fileNameInputGrid, string gridLatName, string gridLonName, string fileNameNarr, string fileNameMapper, bool testAroundToronto)
        {
            JassBuilder builder = new JassBuilder();
            DateTime start = DateTime.Now;
            JassBuilderLog builderLog = createBuilderLog(builder, "mapGridStart", "", "", new TimeSpan(), true);
    

            JassMaccNarrGridsCombo gc = new JassMaccNarrGridsCombo();
            string maccFile = AppDataFolder + "/" + fileNameInputGrid;
            string narrFile = AppFilesFolder + "/" + fileNameNarr;
            string mapFile = AppFilesFolder + "/" + fileNameMapper;
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

                        gc = JassWeather.Models.JassWeatherAPI.MapGridNarr2Macc(gc);

                        JassBuilderLog builderLog2 = createBuilderLog(builder, "mapGridAfterGC", "", "", DateTime.Now - start, true);

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

                            gc = JassWeather.Models.JassWeatherAPI.MapGridNarr2Macc(gc);

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

        public string replaceURIPlaceHolders(string urlTemplate, int year, int month)
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

            int index = url.IndexOf("$");
            if (index > -1) throw new Exception("url still has template variables, forgot to put year or month?");
            return url;

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
                if (builder.weekyEnd == null) builder.weekyEnd = builder.weeky;

                for (int year = (int)builder.year; year < (int)builder.yearEnd + 1; year++)
                {
                    for (int month = (int)builder.month; month < (int)builder.monthEnd + 1; month++)
                    {
                        for (int weeky = (int)builder.weeky; weeky < (int)builder.weekyEnd + 1; weeky++)
                        {

                        //here is where we start the real builder
                        DateTime startedAt = DateTime.Now;
                        JassBuilderLog childBuilderLog0 = createBuilderLogChild(builderLog, builder, year, month, "processBuilder_Start", builder.JassVariable.Name, "", new TimeSpan(), true);

                        try
                        {
                            MessageBuilder = processBuilder(builder, year, month, weeky, upload, builderLog);


                            JassBuilderLog childBuilderLog1 = createBuilderLogChild(builderLog, builder, year, month, "processBuilder_End", builder.JassVariable.Name, "", new TimeSpan(), true);

                            Message += "  processBuilder(" + year + ",  " + month + ") =>" + MessageBuilder;
                        }
                        catch (Exception e)
                        {

                            JassBuilderLog childBuilderLog1 = createBuilderLogChild(builderLog, builder, year, month, "processBuilder_End", builder.JassVariable.Name, e.Message, DateTime.Now - startedAt, false);

                        }
                        finally
                        {
                            //clean disk
                            if (clean) cleanAppData();
                            int filesInAppData = Directory.GetFiles(AppDataFolder).Count();
                            JassBuilderLog childBuilderLog10 = createBuilderLogChild(builderLog, builder, year, month, "processBuilderAll_CleanAppData", builder.JassVariable.Name, "filesInAppData: " + filesInAppData, new TimeSpan(), true);

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


            return Message; 
        }

        public string processBuilder(JassBuilder builder, int year, int month, int weeky, Boolean upload, JassBuilderLog builderAllLog)
        {

            string Message = "process builder sucessfuly";
            long StartingMemory;
            DateTime StartingTime = DateTime.Now;
            long AfterOpenMemory;
            long AfterLoadMemory;
            DateTime EndingTime = DateTime.Now;
            TimeSpan TotalDelay;
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
                string inputFile1 = processSource(builder, year, month, upload, false, builderAllLog);
                #region logs
                JassBuilderLog childBuilderLog1 = createBuilderLogChild(builderAllLog, builder, year, month, "processBuilder_AfterProcessSource", builder.JassVariable.Name, "", DateTime.Now - startTimeProcessSource, true);
                #endregion logs

                Boolean input_Grid_Is_Different = (builder.APIRequest.JassGrid.Type != "NARR");

                if (input_Grid_Is_Different)
                {  try
                    {
                        if (builder.APIRequest.JassGrid.Type == "MACC")
                        {
                        string inputFileTemplateBeforeTransformation = builder.APIRequest.url;
                        inputFile1 = processGridMappingMaccToNarr(year, month,inputFileTemplateBeforeTransformation);
                        }

                        if (builder.APIRequest.JassGrid.Type == "CFSR")
                        {
                            string inputFileTemplateBeforeTransformation = builder.APIRequest.url;
                            inputFile1 = processGridMappingCFSRToNarr(year, month, weeky, inputFileTemplateBeforeTransformation);
                        }

                        //if we are here the type was wrong
                        throw new Exception("We cannot handle the supplied type of GRID: " + builder.APIRequest.JassGrid.Type);
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
                    int in_day = 1;
                    string variableName = builder.JassVariable.Name;

                    DateTime day = new DateTime(in_year, in_month, in_day);


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
                        in_year = day.Year;
                        in_month = day.Month;
                        in_day = day.Day;

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

                            day = day.AddDays(1);
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
                if (container.Name != "ftp")
                {
                    variableStatus = new JassVariableStatus(DateTime.Now.Year - 9, DateTime.Now.Year);
                    variableStatus.ContainerName = container.Name;

                    variableStatus.JassVariable = (from v in db.JassVariables where v.Name.ToLower() == container.Name select v).First();
                    variableStatus.VariableName = variableStatus.JassVariable.Name;

                    //now we need to see which days we actuall have, the idea will be to fill up this status structure
                    JassFileNameComponents dayMeasureNameComponents;

                    foreach (IListBlobItem dayMeasure in container.ListBlobs(null, false))
                    {
                        dayMeasureNameComponents = new JassFileNameComponents(dayMeasure.Uri.ToString());
                        variableStatus.countBlob(dayMeasureNameComponents);
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

                string ContainerName = fileName.Substring(0,index).ToLower();
                string BlobName = fileName;
                downloadBlob(ContainerName,BlobName,filePath);
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

        public JassGridValues GetDayValues(JassGrid grid, string fileName)
        {
            //HERE

            //IMPORTANT: The good thing about this function is that is generic... but the bad thing is that is to sample.
            //So, we are reducing the time and level dimension to only read the first 'Timesize" points and only 3 levels

            JassGridValues dayGridValues;
            string filePath = AppDataFolder + "/" + fileName;
            DownloadFile2DiskIfNotThere(fileName, filePath);
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
                    }
                    if (v.Name == grid.Levelname)
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

                Single[] y = dataset1.GetData<Single[]>(grid.Yname);
                Single[] x = dataset1.GetData<Single[]>(grid.Xname);
                double[] time = new double[grid.Timesize];

                Single[] level = new Single[1];

                //IMPORTANT WE ARE SAMPLING ONLY 3 LEVELS OF PRESSURE
                if (hasLevel) level = new Single[3];

                dayGridValues = new JassGridValues(keyVariable.Metadata, keyVariable.Name, time.Length, level.Length, y.Length, x.Length);

                //how to get the scale/factor value.

                string outPutString = schema2string(schema1);

                for (int ll = 0; ll < level.Length; ll++)
                {
                    dayGridValues.measureMin[ll] = add_offset + scale_factor * 32768;
                    dayGridValues.measureMax[ll] = add_offset + scale_factor * (-32768);
                }

                if (hasLevel)
                {
                    dynamic values;
                    try
                    {
                        values = dataset1.GetData<Int16[, , ,]>(keyVariable.Name,
                                 DataSet.Range(0, time.Length-1),
                                 DataSet.Range(0, level.Length-1),
                                 DataSet.Range(0, y.Length-1),
                                 DataSet.Range(0, x.Length-1) );
                    }
                    catch (Exception)
                    {
                        values = dataset1.GetData<Single[, , ,]>(keyVariable.Name,
                                 DataSet.Range(0, time.Length-1),
                                 DataSet.Range(0, level.Length-1),
                                 DataSet.Range(0, y.Length-1),
                                 DataSet.Range(0, x.Length-1) );
                    }
                    for (int tt = 0; tt < time.Length; tt++)
                    {
                        for (int ll = 0; ll < level.Length; ll++)
                        {
                            for (int yy = 0; yy < y.Length; yy++)
                            {
                                for (int xx = 0; xx < x.Length; xx++)
                                {
                                    if (values[tt, ll, yy, xx] != missing_value &&
                                        values[tt, ll, yy, xx] != FillValue)
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
                    dynamic values;
                    try
                    {
                        values = dataset1.GetData<Int16[, ,]>(keyVariable.Name,
                                 DataSet.Range(0, time.Length - 1),
                                 DataSet.Range(0, y.Length - 1),
                                 DataSet.Range(0, x.Length - 1));
                    }
                    catch (Exception)
                    {
                        values = dataset1.GetData<Single[, ,]>(keyVariable.Name,
                                 DataSet.Range(0, time.Length - 1),
                                 DataSet.Range(0, y.Length - 1),
                                 DataSet.Range(0, x.Length - 1));
                    }
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

                dayGridValues = new JassGridValues(keyVariable.Metadata, keyVariable.Name, time.Length, level.Length, y.Length, x.Length);

                //how to get the scale/factor value.

                string outPutString = schema2string(schema1);

                for (int ll = 0; ll < level.Length; ll++) { 
                    dayGridValues.measureMin[ll] = add_offset + scale_factor * 32768;
                    dayGridValues.measureMax[ll] = add_offset + scale_factor * (-32768); }

                if (hasLevel)
                {
                    Int16[, , ,] values = dataset1.GetData<Int16[, , ,]>(keyVariable.Name);
                    for (int tt = 0; tt < time.Length; tt++)
                    {
                        for (int ll = 0; ll < level.Length; ll++)
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
                    Int16[, ,] values = dataset1.GetData<Int16[, ,]>(keyVariable.Name);
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
