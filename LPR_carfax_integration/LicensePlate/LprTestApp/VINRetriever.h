// Calls HunterNet to get the Vin number of a provided license plate,
// state, username, and password
// After changing .cs code to .dll and then changing .dll to .tlb (with access key)
// and since the function is COM visible, this .cpp file should be able to request a
// VIN from HunterNet
// VINRetriever.cpp: Defines the entry point for the console application.
// C++ client that calls a managed DLL.

#include "stdafx.h"
#include "tchar.h"
#include <iostream>
#include <comdef.h>
#include <string>

// Import the type library.

#import "HunterNet.tlb" raw_interfaces_only
using namespace HunterNet;
using namespace System;
using namespace std;
using namespace System::Runtime::InteropServices;

class VINRetriever
{
private:
	BSTR lResult;
	BSTR plate;
	BSTR state;
	BSTR username;
	BSTR password;

public:
	VINRetriever(CString license, CString pState)
	{
		lResult = SysAllocString(L"");
		plate = SysAllocString(license);
		state = SysAllocString(pState);
		username = SysAllocString(L"Skitchen");
		password = SysAllocString(L"Sk4983");
	}

	CString getVIN()
	{
		// Initialize COM.
		HRESULT hr = CoInitialize(NULL);

		// Create the interface pointer.
		ILPRutilitiesPtr util(__uuidof(LPRutilities));

		// Call the Lookup method.
		util->LprVinLookup(plate, state, username, password, &lResult);

		//Convert BSTR to wstring
		CString resultString = _bstr_t(lResult, false);

		// Uninitialize COM.
		CoUninitialize();

		//The result string contains quotations on either side. We will take those out below.
		resultString = resultString.Mid(1, resultString.GetLength()-2);
		//Return VIN
		return resultString;
	}
};