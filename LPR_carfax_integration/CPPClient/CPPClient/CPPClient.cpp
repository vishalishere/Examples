// An application to test LPRVinLookup when in a .cpp file
// After changing .cs code to .dll and then changing .dll to .tlb (with access key)
// and since the function is COM visible, this .cpp file should be able to request a
// VIN from HunterNet
// CPPClient.cpp: Defines the entry point for the console application.
// C++ client that calls a managed DLL.

//NOTE: FOR namespace System to run, /clr must be turned on 
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

int _tmain(int argc, _TCHAR* argv[])
{
	// Initialize COM.
	HRESULT hr = CoInitialize(NULL);

	// Create the interface pointer.
	ILPRutilitiesPtr util(__uuidof(LPRutilities));

	//Create Strings the Way the Compiler wants it
	BSTR lResult = SysAllocString(L"Hello");
	BSTR plate = SysAllocString(L"7920044");
	BSTR state = SysAllocString(L"IL");
	BSTR username = SysAllocString(L"Skitchen");
	BSTR password = SysAllocString(L"Sk4983");

	// Call the Lookup method.
	util->LprVinLookup(plate, state, username, password, &lResult);

	//Convert BSTR to wstring
	string resultString = _bstr_t(lResult, false);

	//Print what is returned
	cout << "The result is " << resultString << endl;


	// Uninitialize COM.
	CoUninitialize();
	return 0;
}