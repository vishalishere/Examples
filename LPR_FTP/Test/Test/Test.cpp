//I've written test interfaces for accessing HunterNet
//CarRetrieve.tlb is your example code turned into an interface.
//HunterNet.tlb is HunterNet compiled. It throws the runtime error.
//Then I put both versions of the VINRetriever class (one calls CarRetrieve.tlb and the other calls HunterNet.tlb)
//One can be commented out to test the other


#include "stdafx.h"
#include "stdafx.h"
#include "tchar.h"
#include <iostream>
#include <comdef.h>
#include <string>
#include <atlstr.h>
#include <comutil.h>


/*Here is the class that works*/
/*
// Import the type library.

#import "CarfaxRetrieve.tlb" raw_interfaces_only

using namespace CarfaxRetrieve;
using namespace System;
using namespace std;
using namespace System::Runtime::InteropServices;



// Calls HunterNet to get the Vin number of a provided license plate,
// state, username, and password
// After changing .cs code to .dll and then changing .dll to .tlb (with access key)
// and since the function is COM visible, this .cpp file should be able to request a
// VIN from HunterNet
// VINRetriever.cpp: Defines the entry point for the console application.
// C++ client that calls a managed DLL.


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
		plate = SysAllocString(L"sh0c1h");
		state = SysAllocString(L"mo");
		username = SysAllocString(L"Skitchen");
		password = SysAllocString(L"Sk4983");
	}

	CString getVIN()
	{
		// Initialize COM.
		HRESULT hr = CoInitialize(NULL);

		// Create the interface pointer.
		CarfaxRetrievePtr util(__uuidof(Retriever));

		// Call the Lookup method.
		util->LprVinLookup(plate, state, username, password, &lResult);

		//Convert BSTR to CString
		CString resultString = CString(lResult);

		// Uninitialize COM.
		CoUninitialize();

		//The result string contains quotations on either side. We will take those out below.
		resultString = resultString.Mid(1, resultString.GetLength() - 2);
		//Return VIN
		return resultString;
	}
};


*/

/*Here is the code that implements HunterNet.tlb ---- throws the runtime error*/

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
		plate = SysAllocString(L"sh0c1h");
		state = SysAllocString(L"mo");
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

		//Convert BSTR to CString
		CString resultString = CString(lResult);

		// Uninitialize COM.
		CoUninitialize();

		//The result string contains quotations on either side. We will take those out below.
		resultString = resultString.Mid(1, resultString.GetLength() - 2);
		//Return VIN
		return resultString;
	}
};


int _tmain(int argc, _TCHAR* argv[])
{
	VINRetriever retriever("test", "test");
	_tprintf(retriever.getVIN());
	return 0;
}


//This version of HunterNet works -- The code contained in CarfaxRetrieve.tlb
/*

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System;
using System.Net.Http;

namespace CarfaxRetrieve
{
	public interface CarfaxRetrieve
	{
		string LprVinLookup(string plate, string state, string username, string password);
	};

	public class Retriever:CarfaxRetrieve
	{
		public string LprVinLookup(string plate, string state, string username, string password)
		{
			string resultContent = "";
			using (HttpClient client = new HttpClient())
			{
				client.BaseAddress = new Uri("http://www.hunternetwork.com/hunternet/");
				client.DefaultRequestHeaders.Accept.Add(new System.Net.Http.Headers.MediaTypeWithQualityHeaderValue("applications/json"));
				HttpResponseMessage result = client.GetAsync("LPRVIN?plate=" + plate + "&state=" + state + "&auth=" + username).Result;
				if (result.IsSuccessStatusCode)
				{
					resultContent = result.Content.ReadAsStringAsync().Result;
				}
			}
			return resultContent;
		}
	}
}


*/





//This throws a runtime error -- HunterNet.tlb

/*
using System;
using System.Net.Http;
using System.Runtime.InteropServices;

	namespace HunterNet
	{
		[ComVisible(true),
		Guid("47c15a34-fa35-40ee-95bb-a8b84e5c2f0b"),
		ClassInterface(ClassInterfaceType.None)]
		public class LPRutilities : ILPRutilities
		{
			[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2234:PassSystemUriObjectsInsteadOfStrings")]
			public string LprVinLookup(string plate, string state, string userName, string password)
			{
				string retVal = "";

			// create the auth token
			ConnectionUtilities connUtil = new ConnectionUtilities();
			string authToken = connUtil.GetAuthToken(userName, password);
			using (HttpClient client = new HttpClient())
			{
				client.BaseAddress = new Uri(ConfigItems.hostName);
				client.DefaultRequestHeaders.Accept.Add(new System.Net.Http.Headers.MediaTypeWithQualityHeaderValue("applications/json"));
				HttpResponseMessage result = client.GetAsync("LPRVIN?plate=" + plate + "&state=" + state + "&auth=" + authToken).Result;
				if (result.IsSuccessStatusCode)
				{
					string resultContent = result.Content.ReadAsStringAsync().Result;
					retVal = resultContent;
				}
		}
		return retVal;
	}
}

*/