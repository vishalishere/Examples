// This class is a wrapper class to provide a simple interface to the C# LPR DLL.

#include "StdAfx.h"
#include <stdio.h>

#include "CLprInterface.h"

CLprInterface::CLprInterface(void)
{
	HRESULT hr = S_OK;
	m_pParent = NULL;
	m_EventHookedUp = false;

	hr = m_LicensePlateInterface.CreateInstance(__uuidof(HCLicensePlateCtrl));
	ATLASSERT(SUCCEEDED(hr));
}

CLprInterface::CLprInterface(CWnd *pParent)
{
	HRESULT hr = S_OK;
	m_pParent = pParent;
	m_EventHookedUp = false;

	hr = m_LicensePlateInterface.CreateInstance(__uuidof(HCLicensePlateCtrl));
	ATLASSERT(SUCCEEDED(hr));
}
CLprInterface::~CLprInterface(void)
{
}

HRESULT CLprInterface::QueryInterface(const IID & iid, void ** pp)
{
	if (iid == __uuidof(ILicenseEvents) ||
		iid == __uuidof(IUnknown))
	{
		*pp = this;
		AddRef();
		return S_OK;
	}
	return E_NOINTERFACE;
}

// This callback function is run by the LicensPlate C# DLL module when
// an event occurs that requires this mfc client to be interrupted.
// A message is posted to the parent in order to relay the event to the
// class that does the GUI.
HRESULT CLprInterface::LicenseAcquired(void)
{
	m_EmptyEventTester++;
	m_pParent->PostMessage(WM_USER + 1, 0, 0);
	return S_OK;
}

// This function causes the c# sharp to fire an event in order to ensure
//  the callback interface is working properly.
void CLprInterface::TestEvent(void)
{
	HRESULT hr = S_OK;

	short Results;
	if (m_EventHookedUp == false)
	{
		m_LicensePlateInterface->AddEventHookup(this, &Results);
		m_EventHookedUp = true;
	}

	m_EmptyEventTester = 0;

	m_LicensePlateInterface->FireEvent();

	ATLASSERT(m_EmptyEventTester != 0);
}

// The following functions simply call functions in the C# DLL module.
// The are simple wrappers for those interface functions so the parent
// can make simple function calls into the DLL.
short CLprInterface::Initialize()
{
	short Results;
	HRESULT retVal;
	retVal = m_LicensePlateInterface->Initialize(&Results);
	return Results;
}

short CLprInterface::GetConfidence()
{
	short Results;
	HRESULT retVal;
	retVal = m_LicensePlateInterface->GetConfidence(&Results);
	return Results;
}

bool CLprInterface::SetLprProperty(CString csProperty, int iValue)
{
	HRESULT retVal;
	VARIANT_BOOL Results;
	BSTR bsProperty = csProperty.AllocSysString();
	retVal = m_LicensePlateInterface->SetLprProperty(bsProperty, iValue, &Results);
	return Results==VARIANT_TRUE;
}

CString CLprInterface::GetLprProperty(CString csProperty)
{
	HRESULT retVal;
	CString Results;
	BSTR bsResults = SysAllocStringLen(NULL, 100);
	BSTR bsProperty = csProperty.AllocSysString();
	retVal = m_LicensePlateInterface->GetLprProperty(bsProperty, &bsResults);
	Results = bsResults;
	SysFreeString(bsResults);
	return Results;
}

CString CLprInterface::GetLicenseNumber()
{
	CString Results;
	BSTR bsResults = SysAllocStringLen(NULL, 16);
	HRESULT retVal;
	retVal = m_LicensePlateInterface->GetLicenseNumber(&bsResults);
	Results = bsResults;
	SysFreeString(bsResults);
	return Results;
}

CString CLprInterface::GetLicenseState()
{
	CString Results;
	BSTR bsResults = SysAllocStringLen(NULL, 16);
	HRESULT retVal;
	retVal = m_LicensePlateInterface->GetLicenseState(&bsResults);
	Results = bsResults;
	SysFreeString(bsResults);
	return Results;
}

CString CLprInterface::GetSnapShotFileName()
{
	CString Results;
	BSTR bsResults = SysAllocStringLen(NULL, 16);
	HRESULT retVal;
	retVal = m_LicensePlateInterface->GetSnapShotFileName(&bsResults);
	Results = bsResults;
	SysFreeString(bsResults);
	return Results;
}

short CLprInterface::TriggerCamera()
{
	short Results;
	HRESULT retVal;
	retVal = m_LicensePlateInterface->TriggerCamera(&Results);
	return Results;
}

short CLprInterface::SetImageDirectory(CString csImageDirectory)
{
	short Results;
	HRESULT retVal;
	BSTR bsImageDirectory = csImageDirectory.AllocSysString();
	retVal = m_LicensePlateInterface->SetImageDirectory(bsImageDirectory,&Results);
	SysFreeString(bsImageDirectory);
	return Results;
}

short CLprInterface::SetVideoUrl(CString csVideoUrl)
{
	short Results;
	HRESULT retVal;
	BSTR bsVideoUrl = csVideoUrl.AllocSysString();
	retVal = m_LicensePlateInterface->SetVideoUrl(bsVideoUrl, &Results);
	SysFreeString(bsVideoUrl);
	return Results;
}

short CLprInterface::SetImageAcquisitionMode(short iMode)
{
	short Results;
	HRESULT retVal;
	retVal = m_LicensePlateInterface->SetImageAcquisitionMode(iMode, &Results);

	return Results;
}

HBITMAP CLprInterface::GetLicenseImage()
{
	long Results;
	HRESULT retVal;
	retVal = m_LicensePlateInterface->GetLicenseImage(&Results);
	return (HBITMAP) Results;
}

CString CLprInterface::SaveLicenseImage(CString csFilename)
{
	HRESULT retVal;
	CString Results;
	BSTR bsResults = SysAllocStringLen(NULL, 265);
	BSTR bsFilename = csFilename.AllocSysString();
	retVal = m_LicensePlateInterface->SaveLicenseImage(bsFilename, &bsResults);
	Results = bsResults;
	SysFreeString(bsResults);
	return Results;
}

short CLprInterface::SetLicenseDiagImage(bool bDiagImage)
{
	short Results;
	HRESULT retVal;
	retVal = m_LicensePlateInterface->SetLicenseDiagImage(bDiagImage, &Results);

	return Results;
}

short CLprInterface::CancelVideoProcess()
{
	short Results;
	HRESULT retVal;
	retVal = m_LicensePlateInterface->CancelVideoProcess(&Results);
	return Results;
}
