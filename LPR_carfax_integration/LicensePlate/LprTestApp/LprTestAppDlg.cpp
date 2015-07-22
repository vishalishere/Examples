
// LprTestAppDlg.cpp : implementation file
//
//It demonstrates some sample function calls and a callback function exposed in the C# module.
//Then the MFC app has a wrapper class that manages the c++ interface to that C# module and 
//houses the callback function.  A simple dialog screen interfaces with the wrapper class.
//It has a button to test the function calls and return some dummy results.
//
//You should be able to build and run this 2 project solution without any changes except for the
// following:  You need to run regasm /tlb on the LicensePlate DLL to generate a tlb file the
// mfc app can use.  And you need to copy the LicensePlate DLL to the mfc .exe directory and 
// register it with regasm.  Make sure you use a regasm version that is made for 32 bit DLL's.

#include "stdafx.h"
#include "LprTestApp.h"
#include "LprTestAppDlg.h"
#include "afxdialogex.h"
#include "CLprInterface.h"
#include "VINRetriever.h"

#ifdef _DEBUG
#define new DEBUG_NEW
#endif

#pragma comment(lib, "GdiPlus.lib")

class VINRetriever;

// CLprTestAppDlg dialog

CLprTestAppDlg::CLprTestAppDlg(CWnd* pParent /*=NULL*/)
	: CDialogEx(CLprTestAppDlg::IDD, pParent)
{
	m_hIcon = AfxGetApp()->LoadIcon(IDR_MAINFRAME);
	m_Count = 0;
	m_TriggerCountDown = -1;
	m_PixelColorAvailable = false;
	VINisChecked = false;
}

void CLprTestAppDlg::DoDataExchange(CDataExchange* pDX)
{
	CDialogEx::DoDataExchange(pDX);
	DDX_Control(pDX, IDC_BUTTON_START, m_StartStopButton);
	DDX_Control(pDX, IDC_STATIC_STATUS, m_StatusInfo);
	DDX_Control(pDX, IDC_BUTTON_IMAGE_DIR, m_ImageDirectory);
	DDX_Control(pDX, IDC_BUTTON_PICTURE, m_PictureButton);
	DDX_Control(pDX, IDC_STATIC_PICTURE, m_VehicleImage);
	DDX_Control(pDX, IDC_BUTTON_RESULTS_FILE, m_ResultsFile);
	DDX_Control(pDX, IDC_EDIT_PROP1, m_EditProperty01);
	DDX_Control(pDX, IDC_EDIT_PROP2, m_EditProperty02);
	DDX_Control(pDX, IDC_EDIT_PROP3, m_EditProperty03);
	DDX_Control(pDX, IDC_EDIT_PROP4, m_EditProperty04);
	DDX_Control(pDX, IDC_EDIT_PROP5, m_EditProperty05);
	DDX_Control(pDX, IDC_EDIT_PROP6, m_EditProperty06);
	DDX_Control(pDX, IDC_EDIT_PROP7, m_EditProperty07);
	DDX_Control(pDX, IDC_EDIT_PROP8, m_EditProperty08);
	DDX_Control(pDX, IDC_EDIT_PROP9, m_EditProperty09);
	DDX_Control(pDX, IDC_EDIT_PROP10, m_EditProperty10);
	DDX_Control(pDX, IDC_EDIT_PROP11, m_EditProperty11);
	DDX_Control(pDX, IDC_EDIT_PROP12, m_EditProperty12);
	DDX_Control(pDX, IDC_EDIT_PROP13, m_EditProperty13);
	DDX_Control(pDX, IDC_EDIT_PROP14, m_EditProperty14);
	DDX_Control(pDX, IDC_EDIT_PROP15, m_EditProperty15);
	DDX_Control(pDX, IDC_EDIT_PROP16, m_EditProperty16);
	DDX_Control(pDX, IDC_EDIT_PROP17, m_EditProperty17);
	DDX_Control(pDX, IDC_EDIT_PROP18, m_EditProperty18);
	DDX_Control(pDX, IDC_EDIT_PROP19, m_EditProperty19);
	DDX_Control(pDX, IDC_EDIT_PROP20, m_EditProperty20);
	DDX_Control(pDX, IDC_EDIT_PROP21, m_EditProperty21);
	DDX_Control(pDX, IDC_EDIT_PROP22, m_EditProperty22);
	DDX_Control(pDX, IDC_EDIT_PROP23, m_EditProperty23);
	DDX_Control(pDX, IDC_EDIT_PROP24, m_EditProperty24);
	DDX_Control(pDX, IDC_EDIT_URL, m_Url);
	DDX_Control(pDX, IDC_EDIT_FTPADDRESS, m_FtpAddress);
	DDX_Control(pDX, IDC_EDIT_FTPPASSWORD, m_FtpPassword);
	DDX_Control(pDX, IDC_ROTATE_CW, m_RotateCw);
	DDX_Control(pDX, IDC_ROTATE_CCENTER, m_RotateCenter);
	DDX_Control(pDX, IDC_ROTATE_CCW, m_RotateCcw);
	DDX_Control(pDX, IDC_STATIC_CURSORXY, m_CursorXY);
	DDX_Control(pDX, IDC_STATIC_CURSORROT, m_CursorRotate);
	DDX_Control(pDX, IDC_STATIC_PIXELCOLOR, m_CursorColor);
	DDX_Control(pDX, IDC_STATIC_COLOR, m_StaticColor);
}

BEGIN_MESSAGE_MAP(CLprTestAppDlg, CDialogEx)
	ON_WM_PAINT()
	ON_WM_QUERYDRAGICON()
	ON_WM_TIMER()
	ON_WM_CTLCOLOR()
	ON_BN_CLICKED(IDC_BUTTON_START, &CLprTestAppDlg::OnBnClickedButtonStart)
	ON_MESSAGE(WM_USER+1,ServiceLprEvent)
	ON_MESSAGE(WM_USER + 2, ServicePictureEvent)			
	ON_BN_CLICKED(IDC_BUTTON_IMAGE_DIR, &CLprTestAppDlg::OnBnClickedButtonImageDir)
	ON_BN_CLICKED(IDC_RADIO_LIVE, &CLprTestAppDlg::OnBnClickedRadioLive)
	ON_BN_CLICKED(IDC_RADIO_SIMULATE, &CLprTestAppDlg::OnBnClickedRadioSimulate)
	ON_BN_CLICKED(IDC_RADIO_VIDEOSTREAM, &CLprTestAppDlg::OnBnClickedRadioVideoStream)
	ON_BN_CLICKED(IDC_RADIO_VIDEOTARGET, &CLprTestAppDlg::OnBnClickedRadioVideoTarget)
	ON_BN_CLICKED(IDC_RADIO_FTP, &CLprTestAppDlg::OnBnClickedRadioFtp)
	ON_BN_CLICKED(IDC_BUTTON_RESULTS_FILE, &CLprTestAppDlg::OnBnClickedButtonResultsFile)
	ON_EN_KILLFOCUS(IDC_EDIT_PROP1, &CLprTestAppDlg::OnKillfocusEditProp1)
	ON_EN_KILLFOCUS(IDC_EDIT_PROP2, &CLprTestAppDlg::OnKillfocusEditProp2)
	ON_EN_KILLFOCUS(IDC_EDIT_PROP3, &CLprTestAppDlg::OnKillfocusEditProp3)
	ON_EN_KILLFOCUS(IDC_EDIT_PROP4, &CLprTestAppDlg::OnKillfocusEditProp4)
	ON_EN_KILLFOCUS(IDC_EDIT_PROP5, &CLprTestAppDlg::OnKillfocusEditProp5)
	ON_EN_KILLFOCUS(IDC_EDIT_PROP6, &CLprTestAppDlg::OnKillfocusEditProp6)
	ON_EN_KILLFOCUS(IDC_EDIT_PROP7, &CLprTestAppDlg::OnKillfocusEditProp7)
	ON_EN_KILLFOCUS(IDC_EDIT_PROP8, &CLprTestAppDlg::OnKillfocusEditProp8)
	ON_EN_KILLFOCUS(IDC_EDIT_PROP9, &CLprTestAppDlg::OnKillfocusEditProp9)
	ON_EN_KILLFOCUS(IDC_EDIT_PROP10, &CLprTestAppDlg::OnKillfocusEditProp10)
	ON_EN_KILLFOCUS(IDC_EDIT_PROP11, &CLprTestAppDlg::OnKillfocusEditProp11)
	ON_EN_KILLFOCUS(IDC_EDIT_PROP12, &CLprTestAppDlg::OnKillfocusEditProp12)
	ON_EN_KILLFOCUS(IDC_EDIT_PROP13, &CLprTestAppDlg::OnKillfocusEditProp13)
	ON_EN_KILLFOCUS(IDC_EDIT_PROP14, &CLprTestAppDlg::OnKillfocusEditProp14)
	ON_EN_KILLFOCUS(IDC_EDIT_PROP15, &CLprTestAppDlg::OnKillfocusEditProp15)
	ON_EN_KILLFOCUS(IDC_EDIT_PROP16, &CLprTestAppDlg::OnKillfocusEditProp16)
	ON_EN_KILLFOCUS(IDC_EDIT_PROP17, &CLprTestAppDlg::OnKillfocusEditProp17)
	ON_EN_KILLFOCUS(IDC_EDIT_PROP18, &CLprTestAppDlg::OnKillfocusEditProp18)
	ON_EN_KILLFOCUS(IDC_EDIT_PROP19, &CLprTestAppDlg::OnKillfocusEditProp19)
	ON_EN_KILLFOCUS(IDC_EDIT_PROP20, &CLprTestAppDlg::OnKillfocusEditProp20)
	ON_EN_KILLFOCUS(IDC_EDIT_PROP21, &CLprTestAppDlg::OnKillfocusEditProp21)
	ON_EN_KILLFOCUS(IDC_EDIT_PROP22, &CLprTestAppDlg::OnKillfocusEditProp22)
	ON_EN_KILLFOCUS(IDC_EDIT_PROP23, &CLprTestAppDlg::OnKillfocusEditProp23)
	ON_EN_KILLFOCUS(IDC_EDIT_PROP24, &CLprTestAppDlg::OnKillfocusEditProp24)
	ON_BN_CLICKED(IDC_RADIO_SINGLE_FEED, &CLprTestAppDlg::OnBnClickedRadioSingleFeed)
	ON_BN_CLICKED(IDC_RADIO_AUTO_FEED, &CLprTestAppDlg::OnBnClickedRadioAutoFeed)
	ON_BN_CLICKED(IDC_RADIO_FULL_PLATE, &CLprTestAppDlg::OnBnClickedRadioFullPlate)
	ON_BN_CLICKED(IDC_RADIO_DIAG_PLATE, &CLprTestAppDlg::OnBnClickedRadioDiagPlate)
	ON_BN_CLICKED(IDC_LEFT, &CLprTestAppDlg::OnBnClickedLeft)
	ON_BN_CLICKED(IDC_RIGHT, &CLprTestAppDlg::OnBnClickedRight)
	ON_BN_CLICKED(IDC_UP, &CLprTestAppDlg::OnBnClickedUp)
	ON_BN_CLICKED(IDC_DOWN, &CLprTestAppDlg::OnBnClickedDown)
	ON_BN_CLICKED(IDC_CENTER, &CLprTestAppDlg::OnBnClickedCenter)
	ON_BN_CLICKED(IDC_ROTATE_CCW, &CLprTestAppDlg::OnBnClickedRotateCcw)
	ON_BN_CLICKED(IDC_ROTATE_CCENTER, &CLprTestAppDlg::OnBnClickedRotateCcenter)
	ON_BN_CLICKED(IDC_ROTATE_CW, &CLprTestAppDlg::OnBnClickedRotateCw)
	ON_BN_CLICKED(IDC_RADIO_CURSOR, &CLprTestAppDlg::OnBnClickedRadioCursor)
	ON_BN_CLICKED(IDC_CHECK1, &CLprTestAppDlg::OnBnClickedCheck1)
END_MESSAGE_MAP()

// CLprTestAppDlg message handlers

BOOL CLprTestAppDlg::OnInitDialog()
{
	CDialogEx::OnInitDialog();

	// Set the icon for this dialog.  The framework does this automatically
	//  when the application's main window is not a dialog
	SetIcon(m_hIcon, TRUE);			// Set big icon
	SetIcon(m_hIcon, FALSE);		// Set small icon

	// TODO: Add extra initialization here
	CheckRadioButton(IDC_RADIO_LIVE, IDC_RADIO_FTP, IDC_RADIO_LIVE);
	CheckRadioButton(IDC_RADIO_SINGLE_FEED, IDC_RADIO_AUTO_FEED, IDC_RADIO_SINGLE_FEED);
	CheckRadioButton(IDC_RADIO_FULL_PLATE, IDC_RADIO_DIAG_PLATE, IDC_RADIO_FULL_PLATE);

	if (!AfxOleInit())		//performs CoInitialize(NULL);
	{
		AfxMessageBox(_T("OLE initialization failed."), MB_OK | MB_ICONSTOP);
		return FALSE;
	}
	
	m_pLicenseEventInterface = new CLprInterface(this);

	InitializePersistentData();

	UINT_PTR myTimer = SetTimer(1, 1000, 0); // one event every 1000 ms = 1 s

	m_RotateCw.SetBitmap((HBITMAP)LoadImage(AfxGetApp()->m_hInstance,
		MAKEINTRESOURCE(IDB_BITMAP3),
		IMAGE_BITMAP, 16, 16, LR_COLOR));

	m_RotateCcw.SetBitmap((HBITMAP)LoadImage(AfxGetApp()->m_hInstance,
		MAKEINTRESOURCE(IDB_BITMAP1),
		IMAGE_BITMAP, 16, 16, LR_COLOR));

	m_RotateCenter.SetBitmap((HBITMAP)LoadImage(AfxGetApp()->m_hInstance,
		MAKEINTRESOURCE(IDB_BITMAP2),
		IMAGE_BITMAP, 16, 16, LR_COLOR));

	return TRUE;  // return TRUE  unless you set the focus to a control
}



void CLprTestAppDlg::InitializePersistentData()
{
	CString strFolder = AfxGetApp()->GetProfileString(_T("Level"), _T("ImageDir"), _T(""));
	if (strFolder.GetLength() > 0)
	{
		m_ImageDirectory.SetWindowTextW(strFolder);
		m_pLicenseEventInterface->SetImageDirectory(strFolder);
		m_csImageDirectory = strFolder;
	}
	CString strUrl = AfxGetApp()->GetProfileString(_T("Level"), _T("VideoUrl"), _T(""));
	if (strFolder.GetLength() > 0)
	{
		m_Url.SetWindowTextW(strUrl);
		m_pLicenseEventInterface->SetVideoUrl(strUrl);
		m_csVideoUrl = strUrl;
	}
	CString strFtpAddress = AfxGetApp()->GetProfileString(_T("Level"), _T("FtpAddress"), _T(""));
	if (strFolder.GetLength() > 0)
	{
		m_FtpAddress.SetWindowTextW(strFtpAddress);
		m_pLicenseEventInterface->SetVideoUrl(strFtpAddress);
		m_csFtpAddress = strFtpAddress;
	}
	CString strFtpPassword = AfxGetApp()->GetProfileString(_T("Level"), _T("FtpPassword"), _T(""));
	if (strFolder.GetLength() > 0)
	{
		m_FtpPassword.SetWindowTextW(strFtpPassword);
		m_pLicenseEventInterface->SetVideoUrl(strFtpPassword);
		m_csFtpPassword = strFtpPassword;
	}

	CString strImageAcq = AfxGetApp()->GetProfileString(_T("Level"), _T("ImageAcq"), _T(""));
	((CButton *)GetDlgItem(IDC_RADIO_LIVE))->SetCheck(BST_UNCHECKED);
	((CButton *)GetDlgItem(IDC_RADIO_SIMULATE))->SetCheck(BST_UNCHECKED);
	((CButton *)GetDlgItem(IDC_RADIO_VIDEOSTREAM))->SetCheck(BST_UNCHECKED);
	((CButton *)GetDlgItem(IDC_RADIO_VIDEOTARGET))->SetCheck(BST_UNCHECKED);
	((CButton *)GetDlgItem(IDC_RADIO_FTP))->SetCheck(BST_UNCHECKED);
	if (strImageAcq == _T("Live"))
	{
		m_pLicenseEventInterface->SetImageAcquisitionMode(0);
		((CButton *)GetDlgItem(IDC_RADIO_LIVE))->SetCheck(BST_CHECKED);
	}
	else if (strImageAcq == _T("Simulate"))
	{
		m_pLicenseEventInterface->SetImageAcquisitionMode(1);
		((CButton *)GetDlgItem(IDC_RADIO_SIMULATE))->SetCheck(BST_CHECKED);
	}
	else if (strImageAcq == _T("VideoStream"))
	{
		m_pLicenseEventInterface->SetImageAcquisitionMode(2);
		((CButton *)GetDlgItem(IDC_RADIO_VIDEOSTREAM))->SetCheck(BST_CHECKED);
	}
	else if (strImageAcq == _T("VideoTarget"))
	{
		m_pLicenseEventInterface->SetImageAcquisitionMode(3);
		((CButton *)GetDlgItem(IDC_RADIO_VIDEOTARGET))->SetCheck(BST_CHECKED);
	}
	else if (strImageAcq == _T("Ftp"))
	{
		m_pLicenseEventInterface->SetImageAcquisitionMode(4);
		((CButton *)GetDlgItem(IDC_RADIO_FTP))->SetCheck(BST_CHECKED);
	}

	CString strFeedMode = AfxGetApp()->GetProfileString(_T("Level"), _T("FeedMode"), _T(""));
	if (strFeedMode == _T("Single"))
	{
		((CButton *)GetDlgItem(IDC_RADIO_SINGLE_FEED))->SetCheck(BST_CHECKED);
		((CButton *)GetDlgItem(IDC_RADIO_AUTO_FEED))->SetCheck(BST_UNCHECKED);
	}
	else if (strFeedMode == _T("Auto"))
	{
		((CButton *)GetDlgItem(IDC_RADIO_AUTO_FEED))->SetCheck(BST_CHECKED);
		((CButton *)GetDlgItem(IDC_RADIO_SINGLE_FEED))->SetCheck(BST_UNCHECKED);
	}
	CString strPlateImage = AfxGetApp()->GetProfileString(_T("Level"), _T("PlateImage"), _T(""));
	if (strPlateImage == _T("Full"))
	{
		((CButton *)GetDlgItem(IDC_RADIO_FULL_PLATE))->SetCheck(BST_CHECKED);
		((CButton *)GetDlgItem(IDC_RADIO_DIAG_PLATE))->SetCheck(BST_UNCHECKED);
	}
	else if (strPlateImage == _T("Diag"))
	{
		((CButton *)GetDlgItem(IDC_RADIO_DIAG_PLATE))->SetCheck(BST_CHECKED);
		((CButton *)GetDlgItem(IDC_RADIO_FULL_PLATE))->SetCheck(BST_UNCHECKED);
	}

}

void CLprTestAppDlg::SetWindowAndProperty(CEdit *pEditCtrl, CString strProperty1, CString strProperty2, CString strProperty3)
{
	CString EditText, strPropertyValue[3];
	strPropertyValue[0] = AfxGetApp()->GetProfileString(_T("Property"), strProperty1, _T(""));
	if (strPropertyValue[0].IsEmpty() == false)
	{
		EditText = strPropertyValue[0];
		int iValue = _tstoi(strPropertyValue[0].GetBuffer());
		m_pLicenseEventInterface->SetLprProperty(strProperty1, iValue);
	}
	if (strProperty2.IsEmpty() == false)
	{
		strPropertyValue[1] = AfxGetApp()->GetProfileString(_T("Property"), strProperty2, _T(""));
		if (strPropertyValue[1].IsEmpty() == false)
		{
			EditText = EditText + _T(",") + strPropertyValue[1];
			int iValue = _tstoi(strPropertyValue[1].GetBuffer());
			m_pLicenseEventInterface->SetLprProperty(strProperty2, iValue);
		}
	}
	if (strProperty3.IsEmpty() == false)
	{
		strPropertyValue[2] = AfxGetApp()->GetProfileString(_T("Property"), strProperty3, _T(""));
		if (strPropertyValue[2].IsEmpty() == false)
		{
			EditText = EditText + _T(",") + strPropertyValue[2];
			int iValue = _tstoi(strPropertyValue[2].GetBuffer());
			m_pLicenseEventInterface->SetLprProperty(strProperty3, iValue);
		}
	}

	//Set the window text from what the device has in case there was no value or incorrect value in the Profile string
	EditText = m_pLicenseEventInterface->GetLprProperty(strProperty1);
	if (strProperty2.IsEmpty()==false)
		EditText = EditText + _T(",") + m_pLicenseEventInterface->GetLprProperty(strProperty2);
	if (strProperty3.IsEmpty() == false)
		EditText = EditText + _T(",") + m_pLicenseEventInterface->GetLprProperty(strProperty3);

	pEditCtrl->SetWindowTextW(EditText);
	pEditCtrl->EnableWindow();

}

// If you add a minimize button to your dialog, you will need the code below
//  to draw the icon.  For MFC applications using the document/view model,
//  this is automatically done for you by the framework.

void CLprTestAppDlg::OnPaint()
{
	if (IsIconic())
	{
		CPaintDC dc(this); // device context for painting

		SendMessage(WM_ICONERASEBKGND, reinterpret_cast<WPARAM>(dc.GetSafeHdc()), 0);

		// Center icon in client rectangle
		int cxIcon = GetSystemMetrics(SM_CXICON);
		int cyIcon = GetSystemMetrics(SM_CYICON);
		CRect rect;
		GetClientRect(&rect);
		int x = (rect.Width() - cxIcon + 1) / 2;
		int y = (rect.Height() - cyIcon + 1) / 2;

		// Draw the icon
		dc.DrawIcon(x, y, m_hIcon);
	}
	else
	{
		CRect rect;
		m_StartStopButton.GetWindowRect(rect);
		this->ScreenToClient(&rect);
		CPaintDC dc(this);
		CBrush MyBrush(RGB(255, 0, 0));
		dc.FillRect(rect, &MyBrush);

		CDialogEx::OnPaint();
	}
}

// The system calls this function to obtain the cursor to display while the user drags
//  the minimized window.
HCURSOR CLprTestAppDlg::OnQueryDragIcon()
{
	return static_cast<HCURSOR>(m_hIcon);
}

void CLprTestAppDlg::OnBnClickedButtonStart()
{
	CString csButtonText;
	CString csResults;
	short InitResults = 0;
	m_StartStopButton.GetWindowTextW(csButtonText);
	if (csButtonText == _T("Test LPR Interface"))
	{
		m_StatusInfo.SetWindowTextW(_T("Event test results should be shown immediately"));
		m_pLicenseEventInterface->TestEvent();
		m_StartStopButton.SetWindowTextW(_T("Clear Results Window"));
	}
	else if (csButtonText == _T("Clear Results Window"))
	{
		m_StatusInfo.SetWindowTextW(_T(""));
		m_StartStopButton.SetWindowTextW(_T("Trigger Camera"));
		m_VehicleImage.LoadFromFile(CString(_T("")));
		m_PictureButton.SetBitmap(NULL);

	}
	else if (csButtonText == _T("Trigger Camera"))
	{
		if (IsDlgButtonChecked(IDC_RADIO_SIMULATE))
		{
			m_StatusInfo.SetWindowTextW(_T("LPR results should be shown in about 1 second."));
		}
		else if(IsDlgButtonChecked(IDC_RADIO_LIVE))
		{
			m_StatusInfo.SetWindowTextW(_T("LPR results should be shown as soon as an png file is added to the directory."));
		}
		else
		{
			m_StatusInfo.SetWindowTextW(_T("LPR results should be shown as soon as a marked jpg image is extracted from a video stream."));
		}
		m_pLicenseEventInterface->TriggerCamera();
		if (IsDlgButtonChecked(IDC_RADIO_AUTO_FEED))
			m_StartStopButton.SetWindowTextW(_T("Stop Acquisition"));
		else if (IsDlgButtonChecked(IDC_RADIO_VIDEOSTREAM) || IsDlgButtonChecked(IDC_RADIO_VIDEOTARGET))
			m_StartStopButton.SetWindowTextW(_T("Stop Acquisition"));
		else
			m_StartStopButton.SetWindowTextW(_T("Clear Results Window"));
	}
	else if (csButtonText == _T("Stop Acquisition"))
	{
		m_TriggerCountDown = -1;

		if (IsDlgButtonChecked(IDC_RADIO_VIDEOSTREAM) || IsDlgButtonChecked(IDC_RADIO_VIDEOTARGET))
		{
			m_pLicenseEventInterface->CancelVideoProcess();
			m_StartStopButton.SetWindowTextW(_T("Clear Results Window"));
		}
		else
		{
			m_StartStopButton.SetWindowTextW(_T("Trigger Camera"));
		}
	}
	else
	{
		int InitResultsIndex;
		TCHAR *sInitResults[] = { _T("Success"), _T("ARH Init Failed"), _T("Camera Init Failed"), _T("Unknown Failure")};
		InitResults = m_pLicenseEventInterface->Initialize();
		if ((InitResults<0) || (InitResults>2))
			InitResultsIndex = 3;
		else
			InitResultsIndex = InitResults;
		if (IsDlgButtonChecked(IDC_RADIO_SIMULATE))
		{
			m_pLicenseEventInterface->SetImageAcquisitionMode(1);		//default in DLL is 0 
			if (IsDlgButtonChecked(IDC_RADIO_SINGLE_FEED))
			{
				csResults.Format(_T("LPR Initialize returned: %d %s\nMode is Simulated from a list of files - Single Feed"), InitResults, sInitResults[InitResultsIndex]);
			}
			else
			{
				csResults.Format(_T("LPR Initialize returned: %d %s\nMode is Simulated from a list of files - Auto Feed"), InitResults, sInitResults[InitResultsIndex]);
			}

		}
		else if (IsDlgButtonChecked(IDC_RADIO_LIVE))
		{
			if (IsDlgButtonChecked(IDC_RADIO_SINGLE_FEED))
			{
				csResults.Format(_T("LPR Initialize returned: %d %s\nMode is Live from a file added to a directory - Single Feed"), InitResults, sInitResults[InitResultsIndex]);
			}
			else
			{
				csResults.Format(_T("LPR Initialize returned: %d %s\nMode is Live from a file added to a directory - Auto Feed"), InitResults, sInitResults[InitResultsIndex]);
			}

		}
		else
		{
			if (IsDlgButtonChecked(IDC_RADIO_SINGLE_FEED))
			{
				csResults.Format(_T("LPR Initialize returned: %d %s\nMode is Live from a file added to a directory - Single Feed"), InitResults, sInitResults[InitResultsIndex]);
			}
			else
			{
				csResults.Format(_T("LPR Initialize returned: %d %s\nMode is Live from a file added to a directory - Auto Feed"), InitResults, sInitResults[InitResultsIndex]);
			}

		}

		if (IsDlgButtonChecked(IDC_RADIO_DIAG_PLATE))
			m_pLicenseEventInterface->SetLicenseDiagImage(true);

		if (m_csImageDirectory.GetLength()>0)
			m_pLicenseEventInterface->SetImageDirectory(m_csImageDirectory);

		m_Url.GetWindowTextW(m_csVideoUrl);
		if (m_csVideoUrl.GetLength() > 0)
		{
			m_pLicenseEventInterface->SetVideoUrl(m_csVideoUrl);
			AfxGetApp()->WriteProfileString(_T("Level"), _T("VideoUrl"), m_csVideoUrl);
		}
		m_FtpAddress.GetWindowTextW(m_csFtpAddress);
		if (m_csFtpAddress.GetLength() > 0)
		{
			m_pLicenseEventInterface->SetVideoUrl(m_csFtpAddress);
			AfxGetApp()->WriteProfileString(_T("Level"), _T("VideoUrl"), m_csFtpAddress);
		}
		m_FtpPassword.GetWindowTextW(m_csFtpPassword);
		if (m_csFtpPassword.GetLength() > 0)
		{
			m_pLicenseEventInterface->SetVideoUrl(m_csFtpPassword);
			AfxGetApp()->WriteProfileString(_T("Level"), _T("VideoUrl"), m_csFtpPassword);
		}

		if (InitResults == 0)		//If successful initialization, move onto the next button
		{
			m_StartStopButton.SetWindowTextW(_T("Test LPR Interface"));
//			m_StartStopButton.SetWindowTextW(_T("Trigger Camera"));
			m_StatusInfo.SetWindowTextW(csResults);
		}

		SetWindowAndProperty(&m_EditProperty01, _T("anpr1/plateconf"));
		SetWindowAndProperty(&m_EditProperty02, _T("trigger/dt1"), _T("trigger/n1"));
		SetWindowAndProperty(&m_EditProperty03, _T("trigger/dt2"), _T("trigger/n2"));
		SetWindowAndProperty(&m_EditProperty04, _T("Camera/CueMarkThreshold"));
		SetWindowAndProperty(&m_EditProperty05, _T("Camera/TgtLostTime"));
		SetWindowAndProperty(&m_EditProperty06, _T("capture/priority"));
		SetWindowAndProperty(&m_EditProperty07, _T("Camera/TgtPixelX0"), _T("Camera/TgtPixelY0"));
		SetWindowAndProperty(&m_EditProperty08, _T("Camera/TgtPixelX1"), _T("Camera/TgtPixelY1"));
		SetWindowAndProperty(&m_EditProperty09, _T("Camera/TgtPixelX2"), _T("Camera/TgtPixelY2"));
		SetWindowAndProperty(&m_EditProperty10, _T("anpr1/slant"));
		SetWindowAndProperty(&m_EditProperty11, _T("anpr1/slant_max"));
		SetWindowAndProperty(&m_EditProperty12, _T("anpr1/slant_min"));
		SetWindowAndProperty(&m_EditProperty13, _T("anpr1/size"));
		SetWindowAndProperty(&m_EditProperty14, _T("anpr1/size_max"));
		SetWindowAndProperty(&m_EditProperty15, _T("anpr1/size_min"));
		SetWindowAndProperty(&m_EditProperty16, _T("anpr1/gamma"));
		SetWindowAndProperty(&m_EditProperty17, _T("anpr1/nchar_max"));
		SetWindowAndProperty(&m_EditProperty18, _T("anpr1/nchar_min"));
		SetWindowAndProperty(&m_EditProperty19, _T("anpr1/slope"));
		SetWindowAndProperty(&m_EditProperty20, _T("anpr1/slope_max"));
		SetWindowAndProperty(&m_EditProperty21, _T("anpr1/slope_min"));
		SetWindowAndProperty(&m_EditProperty22, _T("Camera/TgtPixelX3"), _T("Camera/TgtPixelY3"));
		SetWindowAndProperty(&m_EditProperty23, _T("Camera/TgtPixelX4"), _T("Camera/TgtPixelY4"));
		SetWindowAndProperty(&m_EditProperty24, _T("Camera/TgtPixelX5"), _T("Camera/TgtPixelY5"));
	}

}

LRESULT CLprTestAppDlg::ServicePictureEvent(UINT wParam, LONG lParam)
{
	OnBnClickedMove(-1);

	return 0;
}

LRESULT CLprTestAppDlg::ServiceLprEvent(UINT wParam, LONG lParam)
{
	
	short Confidence = m_pLicenseEventInterface->GetConfidence();
	CString ArhTime = m_pLicenseEventInterface->GetLprProperty(_T("anpr/timems"));
	CString ProcessingTime = m_pLicenseEventInterface->GetLprProperty(_T("LicensePlate/timems"));
	CString FrameRate = m_pLicenseEventInterface->GetLprProperty(_T("Camera/FrameRate"));
	CString PercentFramesMissed = m_pLicenseEventInterface->GetLprProperty(_T("Camera/FramesMissedPercent"));
	CString CueMarkBrightness = m_pLicenseEventInterface->GetLprProperty(_T("Camera/CueMarkBrightness"));
	CString TgtWhiteBrightness = m_pLicenseEventInterface->GetLprProperty(_T("Camera/TgtWhiteBrightness"));
	CString TgtBlackBrightness = m_pLicenseEventInterface->GetLprProperty(_T("Camera/TgtBlackBrightness"));
	CString License = m_pLicenseEventInterface->GetLicenseNumber();
	CString State = m_pLicenseEventInterface->GetLicenseState(); 
	CString SnapShotFile = m_pLicenseEventInterface->GetSnapShotFileName();
	SnapShotFile = SnapShotFile.Mid(m_csImageDirectory.GetLength()+1);			//No need to display the full path
	CString csResults;
	csResults.Format(_T("LPR Event %02d Received\nConfidence Test: %d\nAnpr Time and LPR Processing Time: "), m_Count, Confidence);
	csResults = csResults + ArhTime + _T("ms.,  ") + ProcessingTime + _T("ms.");
	csResults = csResults + _T("\nFrame Rate: ") + FrameRate + _T("  Missed Frames: ") + PercentFramesMissed + _T("%");
	csResults = csResults + _T("\nCue mark brightness: ") + CueMarkBrightness + _T(" White brightness: ") + TgtWhiteBrightness + _T(" Black brightness: ") + TgtBlackBrightness;
	csResults = csResults + _T("\nLicense: ") + State + _T(" ") + License;
	if (VINisChecked)
	{
		// The state also includes the country, but the retriever only uses the state, so we will substring until after the country
		VINRetriever retriever(License, State.Mid(3));
		CString VIN = retriever.getVIN();
		csResults = csResults + _T(" VIN: ") + VIN;
	}
	csResults = csResults + _T("\nImage File: ") + SnapShotFile;
	m_StatusInfo.SetWindowTextW(csResults);

	if (m_Count > 0 )
	{
		HBITMAP oldHB = m_PictureButton.SetBitmap(m_pLicenseEventInterface->GetLicenseImage());
		if (oldHB)		//Clean up the old object to prevent resource leak
			DeleteObject(oldHB);
		m_PictureButton.Invalidate();

		m_pLicenseEventInterface->SaveLicenseImage(_T(""));		//Save the extracted license plate image to the default location/filename

		m_VehicleImage.LoadFromFile(m_pLicenseEventInterface->GetSnapShotFileName());
	}

	if (IsDlgButtonChecked(IDC_RADIO_AUTO_FEED))
	{
		CString csButtonText;
		m_StartStopButton.GetWindowTextW(csButtonText);
		if (csButtonText == "Stop Acquisition")
			m_TriggerCountDown = 1;		//This number will cause a delay before the next trigger is done.  Use 1 or greater
	}

	//Event "0" is the interface test - no need to store that sample data
	if ((m_Count>0) && (m_csResultsFile.GetLength() > 0))
	{
		CTime time = CTime::GetCurrentTime();
		CString csTime = time.Format(_T("%m/%d/%y %H:%M:%S"));
		CString csHeader = _T("Event #,Time,Confidence,Arh Time(ms),Plate Time(ms),Frame Rate,% Frames Missed,State,License,Image File\015\012");

		csResults.Format(_T("%03d,%s,%02d,%s,%s,%s,%s,%s,%s,%s\015\012"), 
			m_Count, csTime.GetBuffer(), Confidence, 
			ArhTime.GetBuffer(), ProcessingTime.GetBuffer(), FrameRate.GetBuffer(), PercentFramesMissed.GetBuffer(),
			State.GetBuffer(), License.GetBuffer(), SnapShotFile.GetBuffer());
		StoreResults(csHeader,csResults);
	}

	m_Count++;

	return 0;
}

void CLprTestAppDlg::StoreResults(CString csHeader, CString csResults)
{
	CFileException pError;

	CFile ResultsFile;
	if (ResultsFile.Open(m_csResultsFile, CFile::modeCreate | CFile::modeNoTruncate | CFile::modeWrite, &pError) == 0)
	{
		//error opening file
	}	
	else
	{
		if (ResultsFile.GetLength() <= 0)
		{
//			ResultsFile.Write("\xFF\xFE", 2); // UTF-16LE BOM, use ANSI string for  raw bytes
			CT2A asHeader(csHeader);
			CT2A asResults(csResults);
			ResultsFile.Write(asHeader.m_psz, strlen(asHeader.m_psz));
			ResultsFile.Write(asResults.m_psz, strlen(asResults.m_psz));
		}
		else
		{
			ResultsFile.SeekToEnd();
			CT2A asResults(csResults);
			ResultsFile.Write(asResults.m_psz, strlen(asResults.m_psz));
		}
		ResultsFile.Close();
	}
}

void CLprTestAppDlg::OnBnClickedButtonImageDir()
{
	CString strFolder;
	if (afxShellManager->BrowseForFolder(strFolder))
	{
		m_ImageDirectory.SetWindowTextW(strFolder);
		m_pLicenseEventInterface->SetImageDirectory(strFolder);
		m_csImageDirectory = strFolder;

		AfxGetApp()->WriteProfileString(_T("Level"), _T("ImageDir"), m_csImageDirectory);
	}
}


void CLprTestAppDlg::OnBnClickedRadioLive()		//Clicked on Directory Event
{
	m_pLicenseEventInterface->SetImageAcquisitionMode(0);
	((CButton *)GetDlgItem(IDC_RADIO_SIMULATE))->SetCheck(BST_UNCHECKED);
	((CButton *)GetDlgItem(IDC_RADIO_VIDEOSTREAM))->SetCheck(BST_UNCHECKED);
	((CButton *)GetDlgItem(IDC_RADIO_VIDEOTARGET))->SetCheck(BST_UNCHECKED);
	((CButton *)GetDlgItem(IDC_RADIO_FTP))->SetCheck(BST_UNCHECKED);
	AfxGetApp()->WriteProfileString(_T("Level"), _T("ImageAcq"), _T("Live"));
}


void CLprTestAppDlg::OnBnClickedRadioSimulate()		//Clicked on Directory List
{
	m_pLicenseEventInterface->SetImageAcquisitionMode(1);
	((CButton *)GetDlgItem(IDC_RADIO_LIVE))->SetCheck(BST_UNCHECKED);
	((CButton *)GetDlgItem(IDC_RADIO_VIDEOSTREAM))->SetCheck(BST_UNCHECKED);
	((CButton *)GetDlgItem(IDC_RADIO_VIDEOTARGET))->SetCheck(BST_UNCHECKED);
	((CButton *)GetDlgItem(IDC_RADIO_FTP))->SetCheck(BST_UNCHECKED);
	AfxGetApp()->WriteProfileString(_T("Level"), _T("ImageAcq"), _T("Simulate"));
}

void CLprTestAppDlg::OnBnClickedRadioVideoStream()		//Clicked on Video cue mark
{
	m_pLicenseEventInterface->SetImageAcquisitionMode(2);
	((CButton *)GetDlgItem(IDC_RADIO_LIVE))->SetCheck(BST_UNCHECKED);
	((CButton *)GetDlgItem(IDC_RADIO_SIMULATE))->SetCheck(BST_UNCHECKED);
	((CButton *)GetDlgItem(IDC_RADIO_VIDEOTARGET))->SetCheck(BST_UNCHECKED);
	((CButton *)GetDlgItem(IDC_RADIO_FTP))->SetCheck(BST_UNCHECKED);
	AfxGetApp()->WriteProfileString(_T("Level"), _T("ImageAcq"), _T("VideoStream"));
}

void CLprTestAppDlg::OnBnClickedRadioVideoTarget()		//Clicked on Video Target
{
	m_pLicenseEventInterface->SetImageAcquisitionMode(3);
	((CButton *)GetDlgItem(IDC_RADIO_LIVE))->SetCheck(BST_UNCHECKED);
	((CButton *)GetDlgItem(IDC_RADIO_SIMULATE))->SetCheck(BST_UNCHECKED);
	((CButton *)GetDlgItem(IDC_RADIO_VIDEOSTREAM))->SetCheck(BST_UNCHECKED);
	((CButton *)GetDlgItem(IDC_RADIO_FTP))->SetCheck(BST_UNCHECKED);
	AfxGetApp()->WriteProfileString(_T("Level"), _T("ImageAcq"), _T("VideoTarget"));
}

void CLprTestAppDlg::OnBnClickedRadioFtp()		//Clicked on FTP event
{
	m_pLicenseEventInterface->SetImageAcquisitionMode(4);
	((CButton *)GetDlgItem(IDC_RADIO_LIVE))->SetCheck(BST_UNCHECKED);
	((CButton *)GetDlgItem(IDC_RADIO_SIMULATE))->SetCheck(BST_UNCHECKED);
	((CButton *)GetDlgItem(IDC_RADIO_VIDEOSTREAM))->SetCheck(BST_UNCHECKED);
	((CButton *)GetDlgItem(IDC_RADIO_VIDEOTARGET))->SetCheck(BST_UNCHECKED);
	AfxGetApp()->WriteProfileString(_T("Level"), _T("ImageAcq"), _T("Ftp"));
}

void CLprTestAppDlg::OnTimer(UINT_PTR nIdEvent)
{
	if (m_TriggerCountDown > 0)
	{
		m_TriggerCountDown--;

		if (m_TriggerCountDown == 0)
			m_pLicenseEventInterface->TriggerCamera();
	}
}

void CLprTestAppDlg::OnBnClickedButtonResultsFile()
{
	CString strResultsFile = m_csImageDirectory + _T("\\Run1.csv");
	CFileDialog *cFileOpen;
	cFileOpen = new CFileDialog(TRUE, _T("csv"), strResultsFile,
		OFN_HIDEREADONLY | OFN_OVERWRITEPROMPT | OFN_NOCHANGEDIR, _T("Text Files (*.txt)|*.txt|Install Shield Files (*.shl)|*.shl|All Files (*.*)|*.*||"));

	if (cFileOpen->DoModal() == IDOK)
	{
		m_ResultsFile.SetWindowTextW(strResultsFile);
		m_csResultsFile = strResultsFile;
	}
}

void CLprTestAppDlg::OnKillfocusEditProp(CEdit *EditCtrl, CString strProperty1, CString strProperty2, CString strProperty3)
{
	CString EditText, csPropValue;
	int iValue;
	int iFound;

	EditCtrl->GetWindowTextW(EditText);
	iFound = EditText.Find(',');
	if (iFound > 0)
	{
		csPropValue = EditText.Left(iFound);
		EditText = EditText.Mid(iFound + 1);
	}
	else
		csPropValue = EditText;
	iValue = _tstoi(csPropValue.GetBuffer());
	m_pLicenseEventInterface->SetLprProperty(strProperty1, iValue);
	AfxGetApp()->WriteProfileString(_T("Property"), strProperty1, csPropValue);

	if (strProperty2.IsEmpty() == false)
	{
		iFound = EditText.Find(',');
		if (iFound > 0)
		{
			csPropValue = EditText.Left(iFound);
			EditText = EditText.Mid(iFound + 1);
		}
		else
			csPropValue = EditText;
		iValue = _tstoi(csPropValue.GetBuffer());
		m_pLicenseEventInterface->SetLprProperty(strProperty2, iValue);
		AfxGetApp()->WriteProfileString(_T("Property"), strProperty2, csPropValue);
	}
	if (strProperty3.IsEmpty() == false)
	{
		iFound = EditText.Find(',');
		if (iFound > 0)
		{
			csPropValue = EditText.Left(iFound);
			EditText = EditText.Mid(iFound + 1);
		}
		else
			csPropValue = EditText;
		iValue = _tstoi(csPropValue.GetBuffer());
		m_pLicenseEventInterface->SetLprProperty(strProperty3, iValue);
		AfxGetApp()->WriteProfileString(_T("Property"), strProperty3, csPropValue);
	}

}

void CLprTestAppDlg::OnKillfocusEditProp1()
{
	OnKillfocusEditProp(&m_EditProperty01, _T("anpr1/plateconf"));
}

void CLprTestAppDlg::OnKillfocusEditProp2()
{
	OnKillfocusEditProp(&m_EditProperty02, _T("trigger/dt1"), _T("trigger/n1"));
}

void CLprTestAppDlg::OnKillfocusEditProp3()
{
	OnKillfocusEditProp(&m_EditProperty03, _T("trigger/dt2"), _T("trigger/n2"));
}

void CLprTestAppDlg::OnKillfocusEditProp4()
{
	OnKillfocusEditProp(&m_EditProperty04, _T("Camera/CueMarkThreshold"));
}

void CLprTestAppDlg::OnKillfocusEditProp5()
{
	OnKillfocusEditProp(&m_EditProperty05, _T("Camera/TgtLostTime"));
}

void CLprTestAppDlg::OnKillfocusEditProp6()
{
	OnKillfocusEditProp(&m_EditProperty06, _T("capture/priority"));
}

void CLprTestAppDlg::OnKillfocusEditProp7()
{
	OnKillfocusEditProp(&m_EditProperty07, _T("Camera/TgtPixelX0"), _T("Camera/TgtPixelY0"));
}

void CLprTestAppDlg::OnKillfocusEditProp8()
{
	OnKillfocusEditProp(&m_EditProperty08, _T("Camera/TgtPixelX1"), _T("Camera/TgtPixelY1"));
}

void CLprTestAppDlg::OnKillfocusEditProp9()
{
	OnKillfocusEditProp(&m_EditProperty09, _T("Camera/TgtPixelX2"), _T("Camera/TgtPixelY2"));
}

void CLprTestAppDlg::OnKillfocusEditProp10()
{
	OnKillfocusEditProp(&m_EditProperty10, _T("anpr1/slant"));
}

void CLprTestAppDlg::OnKillfocusEditProp11()
{
	OnKillfocusEditProp(&m_EditProperty11, _T("anpr1/slant_max"));
}

void CLprTestAppDlg::OnKillfocusEditProp12()
{
	OnKillfocusEditProp(&m_EditProperty12, _T("anpr1/slant_min"));
}

void CLprTestAppDlg::OnKillfocusEditProp13()
{
	OnKillfocusEditProp(&m_EditProperty13, _T("anpr1/size"));
}

void CLprTestAppDlg::OnKillfocusEditProp14()
{
	OnKillfocusEditProp(&m_EditProperty14, _T("anpr1/size_max"));
}

void CLprTestAppDlg::OnKillfocusEditProp15()
{
	OnKillfocusEditProp(&m_EditProperty15, _T("anpr1/size_min"));
}

void CLprTestAppDlg::OnKillfocusEditProp16()
{
	OnKillfocusEditProp(&m_EditProperty16, _T("anpr1/gamma"));
}

void CLprTestAppDlg::OnKillfocusEditProp17()
{
	OnKillfocusEditProp(&m_EditProperty17, _T("anpr1/nchar_max"));
}

void CLprTestAppDlg::OnKillfocusEditProp18()
{
	OnKillfocusEditProp(&m_EditProperty18, _T("anpr1/nchar_min"));
}

void CLprTestAppDlg::OnKillfocusEditProp19()
{
	OnKillfocusEditProp(&m_EditProperty19, _T("anpr1/slope"));
}

void CLprTestAppDlg::OnKillfocusEditProp20()
{
	OnKillfocusEditProp(&m_EditProperty20, _T("anpr1/slope_max"));
}

void CLprTestAppDlg::OnKillfocusEditProp21()
{
	OnKillfocusEditProp(&m_EditProperty21, _T("anpr1/slope_min"));
}

void CLprTestAppDlg::OnKillfocusEditProp22()
{
	OnKillfocusEditProp(&m_EditProperty22, _T("Camera/TgtPixelX3"), _T("Camera/TgtPixelY3"));
}

void CLprTestAppDlg::OnKillfocusEditProp23()
{
	OnKillfocusEditProp(&m_EditProperty23, _T("Camera/TgtPixelX4"), _T("Camera/TgtPixelY4"));
}

void CLprTestAppDlg::OnKillfocusEditProp24()
{
	OnKillfocusEditProp(&m_EditProperty24, _T("Camera/TgtPixelX5"), _T("Camera/TgtPixelY5"));
}

void CLprTestAppDlg::OnBnClickedRadioSingleFeed()
{
	AfxGetApp()->WriteProfileString(_T("Level"), _T("FeedMode"), _T("Single"));
}


void CLprTestAppDlg::OnBnClickedRadioAutoFeed()
{
	AfxGetApp()->WriteProfileString(_T("Level"), _T("FeedMode"), _T("Auto"));
}


void CLprTestAppDlg::OnBnClickedRadioFullPlate()
{
	AfxGetApp()->WriteProfileString(_T("Level"), _T("PlateImage"), _T("Full"));
}


void CLprTestAppDlg::OnBnClickedRadioDiagPlate()
{
	AfxGetApp()->WriteProfileString(_T("Level"), _T("PlateImage"), _T("Diag"));
}


void CLprTestAppDlg::OnBnClickedLeft()
{
	OnBnClickedMove(MOVE_LEFT);
}


void CLprTestAppDlg::OnBnClickedRight()
{
	OnBnClickedMove(MOVE_RIGHT);
}


void CLprTestAppDlg::OnBnClickedUp()
{
	OnBnClickedMove(MOVE_UP);
}


void CLprTestAppDlg::OnBnClickedDown()
{
	OnBnClickedMove(MOVE_DOWN);
}

void CLprTestAppDlg::OnBnClickedMove(int MoveDirection)
{
	m_CursorXY.SetWindowTextW(m_VehicleImage.MoveCursor(MoveDirection));
	m_PixelColor = m_VehicleImage.GetPixel();
	CString ColorText;
	ColorText.Format(_T("%03d,%03d,%03d"), GetRValue(m_PixelColor), GetGValue(m_PixelColor), GetBValue(m_PixelColor));
	m_CursorColor.SetWindowTextW(ColorText);
	m_PixelColorAvailable = true;
	m_StaticColor.Invalidate();
}

void CLprTestAppDlg::OnBnClickedCenter()
{
	m_CursorXY.SetWindowTextW(m_VehicleImage.MoveCursor(MOVE_CENTER));
}

void CLprTestAppDlg::OnBnClickedRotateCcw()
{
	m_CursorRotate.SetWindowTextW(m_VehicleImage.RotateCursor(ROTATE_CCW));
}

void CLprTestAppDlg::OnBnClickedRotateCcenter()
{
	m_CursorRotate.SetWindowTextW(m_VehicleImage.RotateCursor(ROTATE_CENTER));
}

void CLprTestAppDlg::OnBnClickedRotateCw()
{
	m_CursorRotate.SetWindowTextW(m_VehicleImage.RotateCursor(ROTATE_CW));
}


void CLprTestAppDlg::OnBnClickedRadioCursor()
{
	bool bEnable = ((CButton *)GetDlgItem(IDC_RADIO_CURSOR))->GetCheck() == BST_CHECKED;
	if (bEnable)
		((CButton *)GetDlgItem(IDC_RADIO_CURSOR))->SetCheck(BST_UNCHECKED);
	else
		((CButton *)GetDlgItem(IDC_RADIO_CURSOR))->SetCheck(BST_CHECKED);

	m_VehicleImage.EnableCursor(!bEnable);

	m_CursorXY.SetWindowTextW(m_VehicleImage.GetCursorPosition());
	m_CursorRotate.SetWindowTextW(m_VehicleImage.GetCursorRotation());
}

HBRUSH CLprTestAppDlg::OnCtlColor(CDC* pDC, CWnd* pWnd, UINT nCtlColor)
{
	HBRUSH hbr = CDialog::OnCtlColor(pDC, pWnd, nCtlColor);

	if ((m_PixelColorAvailable) && (pWnd->GetDlgCtrlID() == IDC_STATIC_COLOR))
	{
		pDC->SetTextColor(m_PixelColor);
		pDC->SetBkColor(m_PixelColor);

		CBrush   m_brBkgnd;
		m_brBkgnd.DeleteObject();
		m_brBkgnd.CreateSolidBrush(m_PixelColor);

		hbr = m_brBkgnd;
	}

	return hbr;
}

void CLprTestAppDlg::OnBnClickedCheck1()
{
	if (VINisChecked == false)
	{
		VINisChecked = true;
	}
	else
	{
		VINisChecked = false;
	}
}
