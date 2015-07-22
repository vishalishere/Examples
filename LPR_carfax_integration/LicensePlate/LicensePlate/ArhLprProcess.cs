/// NOTE: If you get the error, "A first chance exception of type 'System.IO.FileLoadException' occurred in mscorlib.dll," when building, you must open the App.config file and be sure it is
/// similar to: <?xml version="1.0" encoding="utf-8"?>
///             <configuration>
///                <startup useLegacyV2RuntimeActivationPolicy="true">
///  
///                <supportedRuntime version="v4.0" sku=".NETFramework,Version=v4.0"/></startup>
///             </configuration>

/// The most important line to note is, "<startup useLegacyV2RuntimeActivationPolicy="true">." Without this, the CPD .dlls, which rely on .NET 2.xxx, will not build.

//------------------------------------------------------------------------------
// CARMEN Parking Digital Sample application for C#
// 
// This sample program shows how to use the cpd module in C# programming language.
// The program loads previously saved images from a directory and 
// adds them to the parking process of the cpd module. 
// The parking process reads the number plates, makes the statistical analysis and
// returns the parking results to the application.
//
// The main steps of the cpd processing are:
// 1. Starting the process
// 2. Adding an image to the process
// 3. Getting the status of the process
// 4. If the status is not CPD_STAT_WAIT_FOR_FIRST_IMAGE and not CPD_STAT_PROCESSING read 
//    the results and go to Step 1 or finish the processing.
// 5. If the status is CPD_STAT_WAIT_FOR_FIRST_IMAGE or CPD_STAT_PROCESSING and 
//    there are unprocessed images in the image sequence go to Step 2.
// 6. If there are no more images in the image sequence go to Step 3.
//------------------------------------------------------------------------------
using System;
using System.Threading;
using gx;
using cm;
using System.Text;
using System.IO;
using System.Drawing;
using System.Diagnostics;
using System.Collections.Generic;

namespace LicensePlate
{
    class ArhClass
    {
        public cParkingDigital cpdObject;

        private Dictionary<string, string> dResultData = new Dictionary<string, string>();
        public Dictionary<string, string> Results
        {
            get { return dResultData; }
        }

        private Bitmap m_LicenseImage;
        public Bitmap LicenseImage
        {
            get { return m_LicenseImage; }
        }

        private bool m_LicenseDiagImage = false;
        public bool LicenseDiagImage
        {
            get { return m_LicenseDiagImage; }
            set { m_LicenseDiagImage = value; }
        }

        private Int64 m_ProcessTime = new Int64();

        public Int16 Initialize()
        {
            // Creating the CPD object
            try
            {
                cpdObject = new cParkingDigital("default");
            }
            catch
            {
                return 1;
            }

            // Setting the properties of the parking process.
            cpdObject.SetProperty("anpr1/plateconf", 1);                // Jurisdiction confidence
            cpdObject.SetProperty("trigger/dt1", 500);                  //Overall time limit
            cpdObject.SetProperty("trigger/n1", 2);
            cpdObject.SetProperty("trigger/dt2", 250);
            cpdObject.SetProperty("trigger/n2", 2);
            cpdObject.SetProperty("recognize/maxplates", 1);			// Reads only one number plate from the images

            cpdObject.SetProperty("recognize/name1", "default");		// Sets the default engine for recognizing
            cpdObject.SetProperty("recognize/name2", "-");			// No second engine will be used
            cpdObject.SetProperty("recognize/mode", 0);				// Only the main engine is used to recognize
            cpdObject.SetProperty("anpr1/slant", 10);                // Jurisdiction confidence

            return 0;
        }

        public bool SetProperty(string sProperty, int iValue)
        {
            return cpdObject.SetProperty(sProperty, iValue);
        }

        public string GetProperty(string sProperty)
        {
            if (sProperty == "LicensePlate/timems")
                return m_ProcessTime.ToString();
            else if (sProperty == "anpr/timems")
            {
                if ((dResultData==null) || (dResultData.ContainsKey("time") == false))
                    return "";
                else
                    return dResultData["time"];
            }
            else
                return cpdObject.GetProperty(sProperty);
        }

        /* decipher 1 or more license plate images.  
         * If wild cards are used in the file name, all matching images will be processed and the best
         * result will be chosen (it is assumed to be several images of the same plate).
         * Returned value : The number of images processed or negative if an error occurred.
         */
        public Int16 ProcessImages(string sPath, string sFile)
        {
            Process.GetCurrentProcess().ProcessorAffinity = new IntPtr(2); // Uses the second Core or Processor for the Test
            Process.GetCurrentProcess().PriorityClass = ProcessPriorityClass.High;  	// Prevents "Normal" processes from interrupting Threads
            Thread.CurrentThread.Priority = ThreadPriority.Highest;  	// Prevents "Normal" Threads from interrupting this thread
            Stopwatch stopwatch = new Stopwatch();
            stopwatch.Reset();
            stopwatch.Start();

            var imageFiles = Directory.EnumerateFiles(sPath, sFile);

            Int32 imgID = 1;
            bool bImageFound = false;
            string sfname;
            Int16 iImgCnt = 0;
            foreach (string currentImage in imageFiles)
            {
                //extract the image name to process
                iImgCnt++;
                sfname = currentImage.Substring(sPath.Length + 1);
                string sPathAndFile = sPath + "\\" + sfname; // Adding full path to the file name

                //start the arh/cpd processing of the image
                cpdObject.Start();
                gxImage image = new gxImage("default");

                // Load the next image from the image sequence.
                try
                {
                    bImageFound = image.Load(sPathAndFile);
                }
                catch
                {
                    iImgCnt = -1;   //No image file found
                }

                // Adding the image to CPD process
                if (bImageFound)
                {
                    cpdObject.AddImage(imgID, image);

                    // Checking and displaying the result if any.
                    bool bFinished;

                    do
                    {
                        bFinished = CheckResults(imgID, sPathAndFile, ref dResultData);

                    } while (bFinished == false);
                }

                imgID++;
            }

            stopwatch.Stop();
            m_ProcessTime = stopwatch.ElapsedMilliseconds;

            return iImgCnt;
        }

        private bool CheckResults(Int32 imgID, string sPathAndFile, ref Dictionary<string, string> resultDict)
        {
            bool bFinished;

            // Checking the status of the processing.
            CPD_END_STATUS ProcessStatus = (CPD_END_STATUS)(cpdObject.Status);
            if ((ProcessStatus == CPD_END_STATUS.CPD_STAT_END_ERROR) || (ProcessStatus == CPD_END_STATUS.CPD_STAT_PROCESSING) || (ProcessStatus == CPD_END_STATUS.CPD_STAT_WAIT_FOR_FIRST_IMAGE))
            {
                Thread.Sleep(20);
                bFinished = false;
            }
            else if ((ProcessStatus == CPD_END_STATUS.CPD_STAT_END_TIMEOUT) || (ProcessStatus == CPD_END_STATUS.CPD_STAT_END_SUCCESS))
            {
                gxVariant res = new gxVariant();
                // Checking the result
                try
                {
                    cpdObject.GetResult(res, 100);
                    while (res._get_variant() != null)
                    {
                        resultDict = GetResultItems(res, imgID, sPathAndFile);
                        res.Dispose();
                        cpdObject.GetResult(res, 100);
                    }
                }
                catch (gxException)     // Exception occurs if there is no more result.
                {
                }
                res.Dispose();
                bFinished = true;
            }
            else     //Any of the following process finished conditions: CPD_STAT_UNDEFINED = -1; CPD_STAT_END_NOTFOUND = 3 plate number not found;
            {
                bFinished  = true;
                resultDict = null;
            }

            return bFinished;
        }

        private Dictionary<string, string> GetResultItems(gxVariant result, int cnt, string sPathAndFile)
        {
            gxVariant item = new gxVariant();

            Dictionary<string, string> resultDict = new Dictionary<string, string>();
            resultDict.Add("text", "");
            resultDict.Add("jurisdiction", "");
            resultDict.Add("confidence", "");
            resultDict.Add("time", "");

            try
            {
                // Getting the Unicode format number plate text from the result for displaying it.
                cpdObject.GetResultItem(result, item, "anpr/disptext");
                if (item._get_variant() != null)
                {
                    resultDict["text"] = item.GetUnicode(64);
                }
                item.Dispose();

                // Getting processing time
                cpdObject.GetResultItem(result, item, "anpr/timems");
                if (item._get_variant() != null)
                {
                    resultDict["time"] = item.GetInt().ToString();
                }
                item.Dispose();

                //  Getting State/Region... of the plate
                cpdObject.GetResultItem(result, item, "anpr/type");
                if (item._get_variant() != null)
                {
                    int JurisdictionCode = item.GetInt();
                    if (Jurisdiction.JurisdictionLookup.ContainsKey(JurisdictionCode / 1000))
                        resultDict["jurisdiction"] = Jurisdiction.JurisdictionLookup[JurisdictionCode / 1000];
                    else
                        resultDict["jurisdiction"] = JurisdictionCode.ToString();       //If the code isn't in our table, show the code
                }
                item.Dispose();

                //  Getting Confidence of the result
                cpdObject.GetResultItem(result, item, "anpr/confidence");
                if (item._get_variant() != null)
                {
                    resultDict["confidence"] = item.GetFloat().ToString();
                }
                item.Dispose();

                //  Get plate coordinates
                cpdObject.GetResultItem(result, item, "anpr/frame");
                if (item._get_variant() != null)
                {
                    if (item.GetNItems() == 8)
                    {
                        float y1 = item.GetGxPG4().y1;
                        float y2 = item.GetGxPG4().y3;
                        float x1 = item.GetGxPG4().x1;
                        float x2 = item.GetGxPG4().x2;

                        if (m_LicenseDiagImage==false)
                        {
                            //Add additional space around the plate data so we can see entire plate
                            y1 = y1 - (y2 - y1) / 3;
                            y2 = y2 + (y2 - y1) / 3;
                            x1 = x1 - (x2 - x1) / 5;
                            x2 = x2 + (x2 - x1) / 5;
                        }

                        Rectangle cropRect = new Rectangle(Convert.ToInt32(x1), Convert.ToInt32(y1), Convert.ToInt32(x2 - x1), Convert.ToInt32(y2 - y1));
                        Image resultImage = Image.FromFile(sPathAndFile);
                        Bitmap resultBit = new Bitmap(resultImage);
                        m_LicenseImage = resultBit.Clone(cropRect, resultBit.PixelFormat); //out of memory error here

                        resultBit.Dispose();
                        resultImage.Dispose();                  
                    }
                }
                item.Dispose();
            }
            catch (gxException e)
            {
                // Catch the GX exceptions and displays the error message.
            }

            return resultDict;
        }
    }
}
