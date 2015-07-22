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
    class CameraSimulator
    {
        public event SnapShotHandler SnapShot;
        public EventArgs e = null;
        public delegate void SnapShotHandler(CameraSimulator m, EventArgs e);

        private BackgroundWorker m_BackgroundTask = new BackgroundWorker();

        private string m_ImageDirectory;
        private IEnumerable<string> m_ImageFiles;
        FileSystemWatcher m_FileWatcher;
        private int m_CurrentImageIndex = 0;
        Int16 m_Mode = 0;

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
        private int [] m_CueMarkPixel = new int [2] {4,4};
        private double m_CueMarkBrightness = 0.0;
        private double m_FrameRate = 0;
        int m_FramesRead = 0;
        int m_FramesMissed = 0;

        public Int16 Initialize()
        {
            return 0;
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
            else
            {
                try
                {
                    m_FileWatcher = new FileSystemWatcher();
                    m_FileWatcher.Path = m_ImageDirectory;

                    // Watch both files and subdirectories.
                    m_FileWatcher.IncludeSubdirectories = true;

                    // Watch for all changes specified in the NotifyFilters
                    //enumeration.
                    m_FileWatcher.NotifyFilter = NotifyFilters.Attributes |
                    NotifyFilters.CreationTime |
                    NotifyFilters.DirectoryName |
                    NotifyFilters.FileName |
                    NotifyFilters.LastAccess |
                    NotifyFilters.LastWrite |
                    NotifyFilters.Security |
                    NotifyFilters.Size;

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

            return 0;
        }

        /*
         * Mode 0: Default mode.  After trigger from host, wait for any new images to be added to directory to trigger an event
         * Mode 1: Sim mode 1. After trigger from host, read the next image file from the directory and trigger an event
         * Mode 2: Streaming mode. Read a mjpeg video stream and extract and store a jpg image when a marked frame appears
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
                case "Camera/CueMarkPixelX":
                    m_CueMarkPixel[0] = iValue;
                    break;
                case "Camera/CueMarkPixelY":
                    m_CueMarkPixel[1] = iValue;
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
                case "Camera/CueMarkPixelX":
                    sValue = m_CueMarkPixel[0].ToString();
                    break;
                case "Camera/CueMarkPixelY":
                    sValue = m_CueMarkPixel[1].ToString();
                    break;
                case "Camera/CueMarkBrightness":
                    sValue = m_CueMarkBrightness.ToString();
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
            if( m_Mode==1 )         //Read through a list of files - This is pretty simple, just read the next available file in the directory
            {
                m_ImageFileMask = m_ImageFiles.ElementAt(m_CurrentImageIndex);  // "img_0.png";
                if (SnapShot != null)
                {
                    SnapShot(this, e);
                }
                m_CurrentImageIndex++;
            }
            else if (m_Mode == 2)      //Video stream mode - This has to be set up in a separate thread because it may take a while.  An event occurs when image found.
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
            else      //Respond when a file is dropped into a directory - Setting a flag will cause File Watcher event event handler to be called when a file appears.
            {
                m_FileWatcher.EnableRaisingEvents = true;
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
            while (bFreeze == false)
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
                    while (bSearching == true)
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

                                Color Pixel = frame.GetPixel(m_CueMarkPixel[0], m_CueMarkPixel[1]);
                                float PixelBrightness = Pixel.GetBrightness();
                                if ((bwAsync.CancellationPending) ||(PixelBrightness > Convert.ToDouble(m_CueMarkThreshold) / 100.0))
                                {
                                    m_ImageFileMask = m_ImageDirectory + string.Format("\\LPR{0:yyyyMMddHHmmss}.jpg", DateTime.Now);
                                    frame.Save(m_ImageFileMask);
                                    bFreeze = true;
                                    m_CueMarkBrightness = PixelBrightness;

                                    if (bwAsync.CancellationPending)
                                    {
                                        // Set the e.Cancel flag so that the WorkerCompleted event knows that the process was canceled.
                                        stream.Close();
                                        resp.Close();
                                        args.Cancel = true;
                                        return;
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

                // Periodically check for a cancellation
                //if (bwAsync.CancellationPending)
                //{
                //    // Set the e.Cancel flag so that the WorkerCompleted event knows that the process was canceled.
                //    args.Cancel = true;
                //    return;
                //}
            }

            stream.Close();
            resp.Close();

//            return bFreeze;
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
