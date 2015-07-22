
// LprTestAppDlg.h : header file
//

#pragma once
#include "afxwin.h"
#include "PictureCtrl.h"

class CLprInterface;

// CLprTestAppDlg dialog
class CLprTestAppDlg : public CDialogEx
{
// Construction
public:
	CLprTestAppDlg(CWnd* pParent = NULL);	// standard constructor

// Dialog Data
	enum { IDD = IDD_LPRTESTAPP_DIALOG };

	protected:
	virtual void DoDataExchange(CDataExchange* pDX);	// DDX/DDV support


// Implementation
protected:
	HICON m_hIcon;

	// Generated message map functions
	virtual BOOL OnInitDialog();
	afx_msg void OnPaint();
	afx_msg HCURSOR OnQueryDragIcon();
	afx_msg LRESULT ServiceLprEvent(UINT wParam, LONG lParam);
	afx_msg LRESULT ServicePictureEvent(UINT wParam, LONG lParam);
	afx_msg void OnBnClickedButtonStart();
	afx_msg void OnTimer(UINT_PTR nIdEvent);
	DECLARE_MESSAGE_MAP()
public:
	CButton m_StartStopButton;
//	CMFCButton m_StartStopButton;
	CStatic m_StatusInfo;

private:
	CLprInterface *m_pLicenseEventInterface;
	CString m_csImageDirectory;
	CString m_csVideoUrl;
	CString m_csFtpAddress;
	CString m_csFtpPassword;
	CString m_csResultsFile;
	int m_Count;
	int m_TriggerCountDown;
	bool VINisChecked;

	COLORREF m_PixelColor;
	bool m_PixelColorAvailable;

	void StoreResults(CString csHeader, CString csResults);
	void InitializePersistentData();
	void OnKillfocusEditProp(CEdit *EditCtrl, CString strProperty1, CString strProperty2 = _T(""), CString strProperty3 = _T(""));
	void SetWindowAndProperty(CEdit *pEditCtrl, CString strProperty1, CString strProperty2 = _T(""), CString strProperty3 = _T(""));
	void OnBnClickedMove(int MoveDirection);

public:
	afx_msg void OnBnClickedButtonImageDir();
	CButton m_ImageDirectory;
	CPictureCtrl m_VehicleImage;
	afx_msg void OnBnClickedRadioLive();
	afx_msg void OnBnClickedRadioSimulate();
	afx_msg void OnBnClickedRadioVideoStream();
	afx_msg void OnBnClickedRadioVideoTarget();
	afx_msg void OnBnClickedRadioFtp();
	afx_msg void OnBnClickedButtonResultsFile();

	CButton m_PictureButton;
	CButton m_ResultsFile;

	CEdit m_EditProperty01;
	CEdit m_EditProperty02;
	CEdit m_EditProperty03;
	CEdit m_EditProperty04;
	CEdit m_EditProperty05;
	CEdit m_EditProperty06;
	CEdit m_EditProperty07;
	CEdit m_EditProperty08;
	CEdit m_EditProperty09;
	CEdit m_EditProperty10;
	CEdit m_EditProperty11;
	CEdit m_EditProperty12;
	CEdit m_EditProperty13;
	CEdit m_EditProperty14;
	CEdit m_EditProperty15;
	CEdit m_EditProperty16;
	CEdit m_EditProperty17;
	CEdit m_EditProperty18;
	CEdit m_EditProperty19;
	CEdit m_EditProperty20;
	CEdit m_EditProperty21;
	CEdit m_EditProperty22;
	CEdit m_EditProperty23;
	CEdit m_EditProperty24;
	afx_msg void OnKillfocusEditProp1();
	afx_msg void OnKillfocusEditProp2();
	afx_msg void OnKillfocusEditProp3();
	afx_msg void OnKillfocusEditProp4();
	afx_msg void OnKillfocusEditProp5();
	afx_msg void OnKillfocusEditProp6();
	afx_msg void OnKillfocusEditProp7();
	afx_msg void OnKillfocusEditProp8();
	afx_msg void OnKillfocusEditProp9();
	afx_msg void OnKillfocusEditProp10();
	afx_msg void OnKillfocusEditProp11();
	afx_msg void OnKillfocusEditProp12();
	afx_msg void OnKillfocusEditProp13();
	afx_msg void OnKillfocusEditProp14();
	afx_msg void OnKillfocusEditProp15();
	afx_msg void OnKillfocusEditProp16();
	afx_msg void OnKillfocusEditProp17();
	afx_msg void OnKillfocusEditProp18();
	afx_msg void OnKillfocusEditProp19();
	afx_msg void OnKillfocusEditProp20();
	afx_msg void OnKillfocusEditProp21();
	afx_msg void OnKillfocusEditProp22();
	afx_msg void OnKillfocusEditProp23();
	afx_msg void OnKillfocusEditProp24();

	afx_msg void OnBnClickedRadioSingleFeed();
	afx_msg void OnBnClickedRadioAutoFeed();
	afx_msg void OnBnClickedRadioFullPlate();
	afx_msg void OnBnClickedRadioDiagPlate();
	CEdit m_Url;
	CEdit m_FtpAddress;
	CEdit m_FtpPassword;
	afx_msg void OnBnClickedLeft();
	afx_msg void OnBnClickedRight();
	afx_msg void OnBnClickedUp();
	afx_msg void OnBnClickedDown();
	afx_msg void OnBnClickedCenter();
	CButton m_RotateCw;
	CButton m_RotateCenter;
	CButton m_RotateCcw;
	afx_msg void OnBnClickedRotateCcw();
	afx_msg void OnBnClickedRotateCcenter();
	afx_msg void OnBnClickedRotateCw();
	afx_msg void OnBnClickedRadioCursor();
	afx_msg HBRUSH OnCtlColor(CDC* pDC, CWnd* pWnd, UINT nCtlColor);
	CStatic m_CursorXY;
	CStatic m_CursorRotate;
	CStatic m_CursorColor;
	CStatic m_StaticColor;
	afx_msg void OnBnClickedCheck1();
};
