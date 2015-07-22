using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net;
using System.IO;
using System.Drawing;
using System.ComponentModel;

namespace LicensePlate
{
    class CameraCtrl : IDisposable
    {
        public event SnapShotHandler SnapShot;
        public EventArgs e = null;
        public delegate void SnapShotHandler(CameraCtrl m, EventArgs e);

        private BackgroundWorker m_BackgroundTask = new BackgroundWorker();

        private string m_ImageDirectory;
        private IEnumerable<string> m_ImageFiles;
        FileSystemWatcher m_FileWatcher;
        private int m_CurrentImageIndex = 0;
        Int16 m_Mode = 0;

        //video target variables
        long m_LostTgtTime = 2000;             //Time tgt can be blocked before assuming it is a vehicle
        long m_LastTgtTime = 0;                     //Time tgt was last seen 
        int[] m_PixelMin = new int[] { 0, 0, 0 }; //RGB values
        int[] m_PixelMax = new int[] { 0, 0, 0 }; //RGB values


        private string m_ImageFileMask;
        public string ImageFileMask
        {
            get { return m_ImageFileMask; }
        }
        private string m_VideoUrl;
        public string VideoUrl
        {
            get { return m_VideoUrl; }
            set { m_VideoUrl = value; }
        }

        private int m_CueMarkThreshold = 50;
        private Point [] m_CueOrTgtPixel = new Point[6];
        private double m_CueMarkBrightness = 0.0;
        private double m_TgtWhiteBrightness = 0.0;
        private double m_TgtBlackBrightness = 0.0;
        private double m_FrameRate = 0;
        int m_FramesRead = 0;
        int m_FramesMissed = 0;

        public Int16 Initialize()
        {
            int TgtLoc;
            for (TgtLoc = 0; TgtLoc < 6;TgtLoc++ )
            {
                m_CueOrTgtPixel[TgtLoc].X = 4;
                m_CueOrTgtPixel[TgtLoc].Y = 4;
            }
                return 0;
        }

        public void Dispose()
        {
            m_FileWatcher.Dispose();
            m_BackgroundTask.Dispose();
        }

        /*
         * This is a function that must be called for all modes in order to define what image directory
         * the class will be reading or writing image files to.
         */
        public Int16 SetImageDirectory(string ImageDirectory)
        {
            m_ImageDirectory = ImageDirectory;

            if (m_Mode == 1)
            {
                //Load the list of files for future reading
                if (m_ImageDirectory.Length > 0)
                {
                    IEnumerable<string> ImageFilesPng = Directory.EnumerateFiles(ImageDirectory, "*.png", SearchOption.AllDirectories);
                    IEnumerable<string> ImageFilesJpg = Directory.EnumerateFiles(ImageDirectory, "*.jpg", SearchOption.AllDirectories);
                    m_ImageFiles = ImageFilesPng.Concat(ImageFilesJpg);
                }
                m_CurrentImageIndex = 0;
            }
            else if(m_Mode==0)
            {
                try
                {
                    m_FileWatcher = new FileSystemWatcher();
                    m_FileWatcher.Path = m_ImageDirectory;

                    // Watch both files and subdirectories.
                    m_FileWatcher.IncludeSubdirectories = true;

                    // Watch for all changes specified in the NotifyFilters
                    //enumeration.
                    //m_FileWatcher.NotifyFilter = NotifyFilters.Attributes |
                    //NotifyFilters.CreationTime |
                    //NotifyFilters.DirectoryName |
                    //NotifyFilters.FileName |
                    //NotifyFilters.LastAccess |
                    //NotifyFilters.LastWrite |
                    //NotifyFilters.Security |
                    //NotifyFilters.Size;

                    m_FileWatcher.NotifyFilter = 
                        NotifyFilters.Attributes |
                    NotifyFilters.CreationTime |
//                    NotifyFilters.DirectoryName |
//                    NotifyFilters.FileName |
                    NotifyFilters.LastAccess;

                    // Watch for image files.
                    m_FileWatcher.Filter = "*.*";

                    // Add event handlers.
                    m_FileWatcher.Changed += new FileSystemEventHandler(OnFileChanged);
                    m_FileWatcher.Created += new FileSystemEventHandler(OnFileChanged);
                }
                catch (IOException e)
                {
                    Console.WriteLine("A Exception Occurred :" + e);
                }

                catch (Exception oe)
                {
                    Console.WriteLine("An Exception Occurred :" + oe);
                }

            }
            else
            {
                //The other modes do not require any special handling of the directory
            }

            return 0;
        }

        /*
         * Mode 0: Directory Event (Default mode).  After trigger from host, wait for any new images to be added to directory to trigger an event
         * Mode 1: Directory List. After trigger from host, read the next image file from the directory and trigger an event
         * Mode 2: Video Stream with cue mark. Read a mjpeg video stream and extract and store a jpg image when a marked frame appears
         * Mode 3: Video Stream with target. Read a mjpeg video stream and extract and store a jpg image when a target re-appears in the frame
         * Mode 4: FTP. Read a ftp directory location and copy and process any new files that appear
         */
        public Int16 SetImageAcquisitionMode(Int16 iMode)
        {
            m_Mode = iMode;

            return 0;
        }

        public bool SetProperty(string sProperty, int iValue)
        {
            switch(sProperty)
            {
                case "Camera/CueMarkThreshold":
                    m_CueMarkThreshold = iValue;
                    break;
                case "Camera/TgtPixelX0": m_CueOrTgtPixel[0].X = iValue; break;
                case "Camera/TgtPixelY0": m_CueOrTgtPixel[0].Y = iValue; break;
                case "Camera/TgtPixelX1": m_CueOrTgtPixel[1].X = iValue; break;
                case "Camera/TgtPixelY1": m_CueOrTgtPixel[1].Y = iValue; break;
                case "Camera/TgtPixelX2": m_CueOrTgtPixel[2].X = iValue; break;
                case "Camera/TgtPixelY2": m_CueOrTgtPixel[2].Y = iValue; break;
                case "Camera/TgtPixelX3": m_CueOrTgtPixel[3].X = iValue; break;
                case "Camera/TgtPixelY3": m_CueOrTgtPixel[3].Y = iValue; break;
                case "Camera/TgtPixelX4": m_CueOrTgtPixel[4].X = iValue; break;
                case "Camera/TgtPixelY4": m_CueOrTgtPixel[4].Y = iValue; break;
                case "Camera/TgtPixelX5": m_CueOrTgtPixel[5].X = iValue; break;
                case "Camera/TgtPixelY5": m_CueOrTgtPixel[5].Y = iValue; break;
                case "Camera/TgtRedMin":
                    m_PixelMin[0] = iValue;
                    break;
                case "Camera/TgtRedMax":
                    m_PixelMax[0] = iValue;
                    break;
                case "Camera/TgtGreenMin":
                    m_PixelMin[1] = iValue;
                    break;
                case "Camera/TgtGreenMax":
                    m_PixelMax[1] = iValue;
                    break;
                case "Camera/TgtBlueMin":
                    m_PixelMin[2] = iValue;
                    break;
                case "Camera/TgtBlueMax":
                    m_PixelMax[2] = iValue;
                    break;
                case "Camera/TgtLostTime":
                    m_LostTgtTime = iValue;
                    break;
            }
            return true;
        }

        public string GetProperty(string sProperty)
        {
            string sValue = "";

            switch (sProperty)
            {
                case "Camera/CueMarkThreshold":
                    sValue = m_CueMarkThreshold.ToString();
                    break;
                case "Camera/TgtPixelX0": sValue = m_CueOrTgtPixel[0].X.ToString();break;
                case "Camera/TgtPixelY0": sValue = m_CueOrTgtPixel[0].Y.ToString();break;
                case "Camera/TgtPixelX1": sValue = m_CueOrTgtPixel[1].X.ToString(); break;
                case "Camera/TgtPixelY1": sValue = m_CueOrTgtPixel[1].Y.ToString(); break;
                case "Camera/TgtPixelX2": sValue = m_CueOrTgtPixel[2].X.ToString(); break;
                case "Camera/TgtPixelY2": sValue = m_CueOrTgtPixel[2].Y.ToString(); break;
                case "Camera/TgtPixelX3": sValue = m_CueOrTgtPixel[3].X.ToString(); break;
                case "Camera/TgtPixelY3": sValue = m_CueOrTgtPixel[3].Y.ToString(); break;
                case "Camera/TgtPixelX4": sValue = m_CueOrTgtPixel[4].X.ToString(); break;
                case "Camera/TgtPixelY4": sValue = m_CueOrTgtPixel[4].Y.ToString(); break;
                case "Camera/TgtPixelX5": sValue = m_CueOrTgtPixel[5].X.ToString(); break;
                case "Camera/TgtPixelY5": sValue = m_CueOrTgtPixel[5].Y.ToString(); break;
                case "Camera/TgtRedMin":
                    sValue = m_PixelMin[0].ToString();
                    break;
                case "Camera/TgtRedMax":
                    sValue = m_PixelMax[0].ToString();
                    break;
                case "Camera/TgtGreenMin":
                    sValue = m_PixelMin[1].ToString();
                    break;
                case "Camera/TgtGreenMax":
                    sValue = m_PixelMax[1].ToString();
                    break;
                case "Camera/TgtBlueMin":
                    sValue = m_PixelMin[2].ToString();
                    break;
                case "Camera/TgtBlueMax":
                    sValue = m_PixelMax[2].ToString();
                    break;
                case "Camera/TgtLostTime":
                    sValue = m_LostTgtTime.ToString();
                    break;

                //Read only properties
                case "Camera/TgtRGBMin":
                    sValue = m_PixelMin[0].ToString() + "," + m_PixelMin[1].ToString() + "," + m_PixelMin[2].ToString();
                    break;
                case "Camera/TgtRGBMax":
                    sValue = m_PixelMax[0].ToString() + "," + m_PixelMax[1].ToString() + "," + m_PixelMax[2].ToString();
                    break;
                case "Camera/CueMarkBrightness":
                    sValue = m_CueMarkBrightness.ToString("0,0.00");
                    break;
                case "Camera/TgtWhiteBrightness":
                    sValue = m_TgtWhiteBrightness.ToString("0,0.00");
                    break;
                case "Camera/TgtBlackBrightness":
                    sValue = m_TgtBlackBrightness.ToString("0,0.00");
                    break;
                case "Camera/FrameRate":
                    sValue = m_FrameRate.ToString("0,0.00");
                    break;
                case "Camera/FramesRead":
                    sValue = m_FramesRead.ToString();
                    break;
                case "Camera/FramesMissed":
                    sValue = m_FramesMissed.ToString();
                    break;
                case "Camera/FramesMissedPercent":
                    if (m_FramesRead <= 0)
                        sValue = "";
                    else
                        sValue = ((m_FramesMissed * 100) / m_FramesRead).ToString();
                    break;
            }

            return sValue;
        }

        /*
         * This routine must be used for all modes.  It initiates the acquisition of the image whether it be
         * a list of existing image files in a directory, waiting for an image file to be dropped into a directory,
         * or looking for a marked frame while reading streamed video.
         */
        public Int16 TriggerCamera()
        {
            if( m_Mode==0 )     //Respond when a file is dropped into a directory - Setting a flag will cause File Watcher event event handler to be called when a file appears.
            {
                m_FileWatcher.EnableRaisingEvents = true;
            }
            else if( m_Mode==1 )         //Read through a list of files - This is pretty simple, just read the next available file in the directory
            {
                bool bExit = false;
                while(bExit==false)
                {
                    m_ImageFileMask = m_ImageFiles.ElementAt(m_CurrentImageIndex);  // "img_0.png";
                    if (m_ImageFileMask.Contains(".plate.") == true)
                        m_CurrentImageIndex++;
                    else
                        bExit = true;
                }

                if (SnapShot != null)
                {
                    SnapShot(this, e);
                }
                m_CurrentImageIndex++;
            }
            else if ((m_Mode == 2) || (m_Mode == 3))     //Video stream modes - This has to be set up in a separate thread because it may take a while.  An event occurs when image found.
            {
                if (m_CurrentImageIndex==0)
                {
                m_BackgroundTask.WorkerReportsProgress = true;
                m_BackgroundTask.WorkerSupportsCancellation = true;

                m_BackgroundTask.DoWork += new DoWorkEventHandler(ProcessVideoStream);

                // what to do when worker completes its task (notify the user)
                m_BackgroundTask.RunWorkerCompleted += new RunWorkerCompletedEventHandler(
                delegate(object o, RunWorkerCompletedEventArgs args)
                {
                    if (SnapShot != null)
                    {
                        SnapShot(this, e);
                    }
                    m_CurrentImageIndex++;
                });
                }

                m_BackgroundTask.RunWorkerAsync();

            }
            else if( m_Mode==4 )
            {
                ProcessFtpLocation();
            }

            return 0;
        }

        /*
         * This is a Mode 0 function that responds to a FileSystemWatcher event
         */
        public void OnFileChanged(object source, FileSystemEventArgs e)
        {
            // Specify what is done when a file is changed.
            m_ImageFileMask = e.FullPath.ToLower();       //e.ChangeType

            // filter file types 
            if (m_ImageFileMask.Contains(".plate.") == false)
            {
                if (m_ImageFileMask.EndsWith(".png") || m_ImageFileMask.EndsWith(".jpg"))
                {
                    if (SnapShot != null)
                    {
                        SnapShot(this, e);
                    }
                }

                //Stop monitoring until TriggerCamera is called by the host
                m_FileWatcher.EnableRaisingEvents = false;

                m_CurrentImageIndex++;
            }
        }

        /*
         * This is a Mode 2 function that gives the host the ability to cancel the
         * search for a marked jpeg in an mjpeg video stream.
         */
        public void CancelVideoProcess()
        {
            if( m_BackgroundTask.IsBusy )
            {
                m_BackgroundTask.CancelAsync();
            }
        }

        /*
         * This is a Mode 2 function that searches for a marked jpeg in an mjpeg video stream.
         */
        private void ProcessVideoStream(object sender, DoWorkEventArgs args)
        {
            byte jpegStartByte1 = 255, jpegStartByte2 = 216;
            byte jpegStopByte1 = 255, jpegStopByte2 = 217;
            int jpegStartIdx, jpegEndIdx;

            m_FramesRead = 0;
            m_FramesMissed = 0;

            bool bFreeze = false;

            BackgroundWorker bwAsync = sender as BackgroundWorker;      //Only needed in case host request we cancel before operation complete

            ImageConverter TheConverter = new ImageConverter();

//            string url = "http://10.2.20.32/video.mjpg";
            byte[] imageBuffer = new byte[1024 * 300];

            //Set up the video stream

            HttpWebRequest req = (HttpWebRequest)WebRequest.Create(m_VideoUrl);
            req.ContentType = "multipart/x-mixed-replace";

            Console.WriteLine("Getting response");
            WebResponse resp = req.GetResponse();

            // Writing headers to the console
            for (int i = 0; i < resp.Headers.Count; ++i)
                Console.WriteLine("\nheader name:{0}, header value :{1}", resp.Headers.Keys[i], resp.Headers[i]);


            Console.WriteLine("Getting response stream");
            Stream stream = resp.GetResponseStream();

            Console.WriteLine("Stream object initialized");

            //Process the video stream by extracting a jpeg from the stream and then checking to see if it is
            //marked by the camera trigger mechanism.  If it is, that's the image we want.

            int bytesRead;
            int size;
            int BlockSize = 1024;
            long LastTimeMark = 0;

            // This outer loop searches for the jpeg start marker
            while ((bFreeze == false) && (args.Cancel==false))
            {
                do
                {
                bytesRead = stream.Read(imageBuffer, 0, BlockSize);
                } while (bytesRead == 0);

                jpegStartIdx = FindBytePair(imageBuffer, 0, bytesRead, jpegStartByte1, jpegStartByte2);

                // If start index found, process everything until the end
                if (jpegStartIdx != -1)
                {
                    //Console.WriteLine("Found Jpeg start bytes");

                    size = BlockSize - jpegStartIdx;

                    // This inner loop searches for the jpeg end marker.  If found, it processes the jpeg
                    bool bSearching = true;
                    while ((bSearching == true) && (args.Cancel==false))
                    {
                        do
                        {
                        bytesRead = stream.Read(imageBuffer, jpegStartIdx + size, BlockSize);
                        } while (bytesRead == 0);

                        jpegEndIdx = FindBytePair(imageBuffer, jpegStartIdx + size, bytesRead, jpegStopByte1, jpegStopByte2);
                        if (jpegEndIdx == -1)   //The end marker was not found, so keep reading bytes.
                        {
                            size = size + bytesRead;

                            if (size > 300000)      //If getting too close to our generous buffer size, abort this frame
                            {
                                m_FramesMissed++;
                                bSearching = false;
                            }
                        }
                        else   //The end marker was found, so process the jpeg
                        {
                            // Index returned is of the first byte in the byte pair
                            jpegEndIdx += 1;

                            //define final size
                            size = jpegEndIdx - jpegStartIdx + 1;

                            //                            System.Diagnostics.Debug.WriteLine("Frame: {0} {1}", FrameRead, size);

                            byte[] jpegBytes = new byte[size];
                            Buffer.BlockCopy(imageBuffer, jpegStartIdx, jpegBytes, 0, size);
                            try
                            {
                                Object TheObject = TheConverter.ConvertFrom(jpegBytes);

                                Bitmap frame = (Bitmap)(TheObject);

                                if ((bwAsync.CancellationPending) || IsFrameTriggered(ref frame))
                                {
                                    m_ImageFileMask = m_ImageDirectory + string.Format("\\LPR{0:yyyyMMddHHmmss}.jpg", DateTime.Now);
                                    frame.Save(m_ImageFileMask);
                                    bFreeze = true;
                                    m_LastTgtTime = 0;

                                    if (bwAsync.CancellationPending)
                                    {
                                        // Set the e.Cancel flag so that the WorkerCompleted event knows that the process was canceled.
                                        //stream.Close();
                                        //resp.Close();
                                        args.Cancel = true;
                                        //return;
                                    }
                                }
                            }
                            catch
                            {
                                m_FramesMissed++;
                            }
                            //Image frame = (Bitmap)((new ImageConverter()).ConvertFrom(jpegBytes));

                            //Console.ReadLine();
                            if (m_FramesRead % 100 == 0)
                            {
                                //                                System.Diagnostics.Debug.WriteLine("100 frames");
                                long CurrentTime = DateTime.Now.Ticks / TimeSpan.TicksPerMillisecond;
                                if (m_FramesRead != 0)
                                {
                                    m_FrameRate = (100000.0 / (Double)(CurrentTime - LastTimeMark));
                                }
                                LastTimeMark = CurrentTime;
                            }
                            m_FramesRead++;
                            bSearching = false;
                        }
                    }
                }
            }

            stream.Close();
            resp.Close();
        }

        /*
         * This is a Mode 4 function that searches an ftp location for a file
         */
        private void ProcessFtpLocation()          //object sender, DoWorkEventArgs args)
        {
            /* Create Object Instance */
            //Ftp ftpClient = new Ftp(@"ftp://10.2.3.74/", "admin", "H3cosvc!");

            ///* Get Contents of a Directory (Names Only) */
            //string temp;
            //string[] simpleDirectoryListing = ftpClient.directoryListDetailed("/etc");
            //for (int i = 0; i < simpleDirectoryListing.Count(); i++) 
            //{ 
            //    temp = simpleDirectoryListing[i]; 
            //}
        }

        private bool IsFrameTriggered(ref Bitmap frame)
        {
            bool bTriggered = false;

            Color Pixel;
            if (m_Mode == 2)    //Video mode with cue mark trigger
            {
                Pixel = frame.GetPixel(m_CueOrTgtPixel[0].X, m_CueOrTgtPixel[0].Y);
                float PixelBrightness = Pixel.GetBrightness();
                if (PixelBrightness > Convert.ToDouble(m_CueMarkThreshold) / 100.0)
                {
                    m_CueMarkBrightness = PixelBrightness;
                    bTriggered = true;
                }
            }
            else if (m_Mode == 3)    //Video mode with target trigger
            {
                bTriggered = true;

                //This code assume the array of points is in the order: white stripe, black stripe, white stripe, etc.,
                int TgtStripe = 0;

                //This routine uses an absolute brightness threshold.  Pixels below the threshold are considered black and those above are considered white
                //while ((TgtStripe < 6) && (bTriggered==true))
                //{
                //    Pixel = frame.GetPixel(m_CueOrTgtPixel[TgtStripe].X, m_CueOrTgtPixel[TgtStripe].Y);
                //    float PixelBrightness = Pixel.GetBrightness();
                //    if( (TgtStripe&1)==0 )  //If it's a white stripe
                //    {
                //        if (PixelBrightness < Convert.ToDouble(m_CueMarkThreshold) / 100.0)
                //        {
                //            m_CueMarkBrightness = PixelBrightness;
                //            bTriggered = false;
                //        }
                //        else if ((PixelBrightness < m_TgtWhiteBrightness) || (TgtStripe<2))
                //        {
                //            m_TgtWhiteBrightness = PixelBrightness; //save the darkest white value
                //        }
                //    }
                //    else  //else it's a black stripe
                //    {
                //        if (PixelBrightness > Convert.ToDouble(m_CueMarkThreshold) / 100.0)
                //        {
                //            m_CueMarkBrightness = PixelBrightness;
                //            bTriggered = false;
                //        }
                //        else if ((PixelBrightness > m_TgtBlackBrightness) || (TgtStripe<2))
                //        {
                //            m_TgtBlackBrightness = PixelBrightness; //save the brightest black value
                //        }
                //    }
                //    TgtStripe++;
                //}

                //This routine uses contrast between bars to determine peaks (whites) and valleys (black)
                // The average brightness for all pixels is calculated.  
                // Then pixels below that average minus a tolerance are considered black while those above the average plus a tolerance are considered white
                double AveBrightness = 0.0;
                double [] PixelBrightness = new double[6];
                for(TgtStripe=0;TgtStripe<6;TgtStripe++)
                {
                    Pixel = frame.GetPixel(m_CueOrTgtPixel[TgtStripe].X, m_CueOrTgtPixel[TgtStripe].Y);
                    PixelBrightness[TgtStripe] = Pixel.GetBrightness();
                    AveBrightness = AveBrightness + PixelBrightness[TgtStripe];
                }
                AveBrightness = AveBrightness/6.0;
                double WhiteLimit = AveBrightness + Convert.ToDouble(m_CueMarkThreshold) / 100.0;
                double BlackLimit = AveBrightness - Convert.ToDouble(m_CueMarkThreshold) / 100.0;
                for(TgtStripe=0;TgtStripe<6;TgtStripe++)
                {
                    if( (TgtStripe&1)==0 )  //If it's a white stripe
                    {
                        if (PixelBrightness[TgtStripe] < WhiteLimit)
                        {
                            m_CueMarkBrightness = PixelBrightness[TgtStripe];
                            bTriggered = false;
                        }
                        else if ((PixelBrightness[TgtStripe] < m_TgtWhiteBrightness) || (TgtStripe < 2))
                        {
                            m_TgtWhiteBrightness = PixelBrightness[TgtStripe]; //save the darkest white value
                        }
                    }
                    else  //else it's a black stripe
                    {
                        if (PixelBrightness[TgtStripe] > BlackLimit)
                        {
                            m_CueMarkBrightness = PixelBrightness[TgtStripe];
                            bTriggered = false;
                        }
                        else if ((PixelBrightness[TgtStripe] > m_TgtBlackBrightness) || (TgtStripe < 2))
                        {
                            m_TgtBlackBrightness = PixelBrightness[TgtStripe]; //save the brightest black value
                        }
                    }
                }

                //If all stripes were appropriately above or below the threshold value, the target was found
                if (bTriggered == true)
                {
                    bTriggered = false;

                    //Found target so decide if it's a trigger condition or just record the current time
                    long CurrentTime = DateTime.Now.Ticks / TimeSpan.TicksPerMillisecond;
                    if ((m_LastTgtTime > 0) && ((CurrentTime - m_LastTgtTime) > m_LostTgtTime))      //If target was found after missing for 2 seconds, assume a vehicle blocked it
                    {
                        bTriggered = true;
                    }
                    else                                        //else assume a vehicle did not block target so update our stored timer to the current time
                    {
                        m_LastTgtTime = CurrentTime;
                    }
                }
            }

            return bTriggered;
        }

        /* Use this function to determine if a value is equal to or between 2 endpoint values regardless which endpoint is greater than the other */
        private bool IsValueBetween(int Value, int Endpoint1, int Endpoint2)
        {
            bool bValueIsBetween = false;
            if( Endpoint1<Endpoint2)    //Endpoint 1 is less than 2
            {
                if ((Value >= Endpoint1) && (Value <= Endpoint2))
                    bValueIsBetween = true;
            }
            else    //Endpoint 1 is greater than 2
            {
                if ((Value <= Endpoint1) && (Value >= Endpoint2))
                    bValueIsBetween = true;
            }

            return bValueIsBetween;
        }

        private int FindBytePair(byte[] buffer, int startIdx, int NumBytes, byte firstByte, byte secondByte)
        {
            if (startIdx >= buffer.Length)
            {
                throw new ArgumentOutOfRangeException("startIdx", "Invalid startIdx. Greater than or equal to buffer length.");
            }

            int currentIdx = startIdx;

            while (true)
            {
                currentIdx = Array.IndexOf(buffer, firstByte, currentIdx);

                // Return if didn't find the first byte
                if (currentIdx == -1)
                {
                    return -1;
                }

                // Return if reach the end of the buffer
                if ((currentIdx + 1) >= (startIdx + NumBytes))
                {
                    return -1;
                }

                if (buffer[currentIdx + 1] == secondByte)
                {
                    return currentIdx;
                }
                else
                {
                    currentIdx++;
                }
            }
        }
    }
}
