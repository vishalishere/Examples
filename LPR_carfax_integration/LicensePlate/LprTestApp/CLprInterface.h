#pragma once
// CLicenseEvents wrapper class
#import "..\LicensePlate\bin\x86\Debug\LicensePlate.tlb"  raw_interfaces_only, named_guids, no_namespace

class CLprInterface : public ILicenseEvents
{
public:

	CLprInterface(void);
	CLprInterface(CWnd *pParent);	//Use this constructor if parent is to receive event messages
	~CLprInterface(void);

	void TestEvent(void);

	HRESULT __stdcall QueryInterface(const IID &, void **);
	ULONG __stdcall AddRef(void) { return 1; }
	ULONG __stdcall Release(void) { return 1; }
	HRESULT __stdcall LicenseAcquired(void);

	// ILicenseEvents methods
public:

	short Initialize();
	short TriggerCamera();
	short GetConfidence();
	CString GetLicenseNumber();
	CString GetLicenseState();
	CString GetSnapShotFileName();
	short SetImageDirectory(CString csImageDirectory);
	short SetVideoUrl(CString csVideoUrl);
	short SetImageAcquisitionMode(short iMode);
	short SetLicenseDiagImage(bool bDiagImage);
	HBITMAP GetLicenseImage();
	CString SaveLicenseImage(CString csFilename);
	bool SetLprProperty(CString csProperty, int iValue);
	CString GetLprProperty(CString csProperty);
	short CancelVideoProcess();

	// ILicenseEvents properties
private:
	ILicensePlateInterfacePtr m_LicensePlateInterface;
	int m_EmptyEventTester;
	CWnd *m_pParent;
	bool m_EventHookedUp;

};
