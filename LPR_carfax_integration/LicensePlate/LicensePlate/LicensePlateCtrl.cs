using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Runtime.InteropServices;
using System.Drawing;


namespace LicensePlate
{

    // COM interop modeled from: http://www.codeproject.com/KB/COM/InteropEvents.aspx?msg=3846086

    // Event Handlers that C++ class will implement to get various notifications of events
    //  in DotNet.  C++ class should use AddEventHookup and RemoveEventHookup to register 
    //  their implementation of these functions within this namespace
    // Refer to ".NET and COM - The Complete Interoperability Guide" by Adam Nathan (p594) 
    [ComVisible(true)]
    [InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
    public interface ILicenseEvents
    {
        void LicenseAcquired();     //Fired when the LPR system has finished processing a result
    }

    // Functions that the C++ class will call to control LPR and get results
    [ComVisible(true)]
    [InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
    public interface ILicensePlateInterface
    {
        Int16 Initialize();
        string GetLicenseNumber();
        string GetLicenseState();
        string GetSnapShotFileName();
        Int16 GetConfidence();
        Int16 TriggerCamera();
        Int16 SetImageDirectory(string ImageDirectory);
        Int16 SetVideoUrl(string VideoUrl);
        Int16 SetImageAcquisitionMode(Int16 iMode);
        Int16 SetLicenseDiagImage(bool bDiagImage);
        IntPtr GetLicenseImage();
        string SaveLicenseImage(string filename);
        bool SetLprProperty(string sProperty, int iValue);
        string GetLprProperty(string sProperty);
        Int16 CancelVideoProcess();

        Int16 AddEventHookup(ILicenseEvents HostObject);
        void FireEvent();
        void RemoveEventHookup();
    }

    [ComVisible(true)]
    [ClassInterface(ClassInterfaceType.None)]
    public class HCLicensePlateCtrl : ILicensePlateInterface, IDisposable
    {
        private Dictionary<string, string> LprProcessResults = new Dictionary<string, string>()
        {
            {"text","123-456"},
            {"jurisdiction","MO"},
            {"confidence","99"},
            {"time","246"}
        };

        private string m_ImageDirectory;
        private string m_VideoUrl;
        private Int16 m_iMode;

        // Define an event associated with the LicenseAcquiredDelegate 
        public List<ILicenseEvents> m_LicenseEvent = new List<ILicenseEvents>();
        private LprCtrl LprController;
        private CameraCtrl CameraController;

        public Int16 Initialize()
        {
            LprController = new LprCtrl();
            short iResult = LprController.Initialize();

            if( iResult==0 )
            {
                CameraController = new CameraCtrl();
                iResult = CameraController.Initialize();
                CameraController.SetImageDirectory(m_ImageDirectory);
                CameraController.VideoUrl = m_VideoUrl;
                CameraController.SetImageAcquisitionMode(m_iMode);

                //Subscribe to the Camera's event handler so that our "OnCameraEvent" will respond
                CameraController.SnapShot += new CameraCtrl.SnapShotHandler(OnCameraEvent);
            }
             return iResult;
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        private void Dispose(bool disposing)
        {
            // call dispose on the context and any of its members here
            CameraController.Dispose();
            LprController.Dispose();
        }
        
        public Int16 CancelVideoProcess()
        {
            if (CameraController!=null)
                CameraController.CancelVideoProcess();

            return 0;
        }

        /* The Get and Set Lpr Property functions are used for lower level
         * camera and ARH query and control.  By relying on a property string that can
         * be anything we define, it makes it easy to add new settings without having to
         * modify the interface design to the host application.  We're using the ARH convention
         * for naming the property:  Category/Property
         */
        public bool SetLprProperty(string sProperty, int iValue)
        {
            bool bResult;

            if (sProperty.StartsWith("Camera"))
                bResult = CameraController.SetProperty(sProperty, iValue);
            else
                bResult = LprController.SetProperty(sProperty, iValue);

            return bResult;
        }

        public string GetLprProperty(string sProperty)
        {
            if (sProperty.StartsWith("Camera"))
                return CameraController.GetProperty(sProperty);
            else
                return LprController.GetProperty(sProperty);
        }

        public string GetLicenseNumber()
        {
            if ((LprProcessResults!=null) && (LprProcessResults.ContainsKey("text")))
                return LprProcessResults["text"];
            else
                return "";
        }

        public string GetLicenseState()
        {
            if ((LprProcessResults!=null) && (LprProcessResults.ContainsKey("jurisdiction")))
                return LprProcessResults["jurisdiction"];
            else
                return "";
        }

        public string GetSnapShotFileName()
        {
            return CameraController.ImageFileMask;
        }

        public Int16 GetConfidence()
        {
            if ((LprProcessResults!=null) && (LprProcessResults.ContainsKey("confidence")))
                return Convert.ToInt16(LprProcessResults["confidence"], 16);
            else
                return 0;
        }

        public Int16 TriggerCamera()
        {
            CameraController.TriggerCamera();

            return 0;
        }

        public Int16 SetImageDirectory(string ImageDirectory)
        {
            m_ImageDirectory = ImageDirectory;
            if (CameraController!=null)
                CameraController.SetImageDirectory(m_ImageDirectory);
            return 0;
        }

        public Int16 SetVideoUrl(string VideoUrl)
        {
            m_VideoUrl = VideoUrl;
            if (CameraController != null)
                CameraController.VideoUrl = VideoUrl;
            return 0;
        }

        /*
         * Mode 0: Default mode.  After trigger from host, wait for any new images to be added to directory to trigger an event
         * Mode 1: Sim mode 1. After trigger from host, read the next image file from the directory and trigger an event
         * Mode 2: Streaming mode. Read a mjpeg video stream and extract and store a jpg image when a marked frame appears
         */
        public Int16 SetImageAcquisitionMode(Int16 iMode)
        {
            m_iMode = iMode;
            if (CameraController != null)
                CameraController.SetImageAcquisitionMode(iMode);

            return 0;
        }
        public Int16 SetLicenseDiagImage(bool bDiagImage)
        {
            LprController.LicenseDiagImage = bDiagImage;

            return 0;
        }

        public IntPtr GetLicenseImage()
        {
            if (LprController.LicenseImage == null)
                return (System.IntPtr)0;
            else
                return LprController.LicenseImage.GetHbitmap();
        }
        
        /*
         * Save the license image extracted from the larger snap shot image to the filename provided.
         * If "filename" provided is blank, the image is saved to a file with the same name as the
         * snap shot image except with ".plate" inserted in the name.
         */
        public string SaveLicenseImage(string filename)
        {
            if (filename.Length <= 0)
            {
                filename = GetSnapShotFileName();
                int iIndex = filename.LastIndexOf('.');
                if( iIndex>=0 )
                {
                    filename = filename.Insert(iIndex, ".plate");
                    if ((LprController.LicenseImage != null) && (System.IO.File.Exists(filename) == false))
                        LprController.LicenseImage.Save(filename);
                }
            }
            else if (LprController.LicenseImage != null)
                LprController.LicenseImage.Save(filename);

            return filename;
        }

        public Int16 AddEventHookup(ILicenseEvents HostObject)
        {
            m_LicenseEvent.Add(HostObject);
            return 0;
        }

        //Call this function to notify c++ host that a license has been acquired and processing finished
        public void FireEvent()
        {
            foreach (ILicenseEvents HostObject in m_LicenseEvent)
            {
                HostObject.LicenseAcquired();
            }
        }
        
        public void RemoveEventHookup()
        {
        }

        //Respond to a camera event that signals acquisition of an image
        private void OnCameraEvent(CameraCtrl m, EventArgs e)
        {
            if (LprController != null)
            {
                if ((m.ImageFileMask!=null) && (m.ImageFileMask.Length > 0))    //Make sure camera didn't abort acquisition
                {
                    int PathBreakPoint = m.ImageFileMask.LastIndexOf(@"\");
                    string sImageDirectory = m.ImageFileMask.Substring(0, PathBreakPoint);
                    string sImageFileMask = m.ImageFileMask.Substring(PathBreakPoint + 1);

                    LprController.ResetResults();
                    LprController.ProcessImages(sImageDirectory, sImageFileMask);

                    LprProcessResults = LprController.Results;
                }

                FireEvent();
            }
        }
    }
}
