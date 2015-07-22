LprTestApp and LicensePlate modules

1/28/2015 Check-in Notes
This solution represents 2 projects:  an MFC test app and a C# DLL that will be used for License Plate Recognition.
The first check-in provides stripped down modules which demonstrates the basic interface between MFC and 
the .NET C# DLL we will use for the LPR code.  Future check-in's will contain more code relevant to the LPR project
itself so it may be less obvious how the basic interface works.

It demonstrates some sample function calls and a callback function exposed in the C# module.
Then the MFC app has a wrapper class that manages the c++ interface to that C# module and houses the callback function.
A simple dialog screen interfaces with the wrapper class.  It has a button to test the function calls and return some 
dummy results that are displayed in a text box.

- Press the button once and it the app calls a single c# function that does a phony initialization of the LPR system.
- Press the button again and it calls a c# function that fires the event mechanism in the c# code which calls the c++ 
    callback function in the wrapper class which in turn posts a message to the main GUI dialog and displays the results.
- Press the button again and it calls several c# functions the return some dummy values.
- Press the button again and results window is cleared so you can repeat the process and see if it works a 2nd time.

This code was originally written with the follow MS Visual Studio software:
Microsoft Visual Studio Premium 2013
Version 12.0.21005.1 REL
Microsoft .NET Framework
Version 4.5.50938

Visual C# 2013   06191-004-0457005-02330
Microsoft Visual C# 2013

Visual C++ 2013   06191-004-0457005-02330
Microsoft Visual C++ 2013

To complete a build you will need to use the REGASM utility with the /tlb option in order to make a type library.
To run the program, you will need to use the REGASM utility to register the DLL.  Make sure the DLL is in the same
  path as the .exe or that the computer's path environment variable points to the directory where the DLL can
  be found.

================================================================================
    MICROSOFT FOUNDATION CLASS LIBRARY : LprTestApp Project Overview
===============================================================================

The application wizard has created this LprTestApp application for
you.  This application not only demonstrates the basics of using the Microsoft
Foundation Classes but is also a starting point for writing your application.

This file contains a summary of what you will find in each of the files that
make up your LprTestApp application.

LprTestApp.vcxproj
    This is the main project file for VC++ projects generated using an application wizard.
    It contains information about the version of Visual C++ that generated the file, and
    information about the platforms, configurations, and project features selected with the
    application wizard.

LprTestApp.vcxproj.filters
    This is the filters file for VC++ projects generated using an Application Wizard. 
    It contains information about the association between the files in your project 
    and the filters. This association is used in the IDE to show grouping of files with
    similar extensions under a specific node (for e.g. ".cpp" files are associated with the
    "Source Files" filter).

LprTestApp.h
    This is the main header file for the application.  It includes other
    project specific headers (including Resource.h) and declares the
    CLprTestApp application class.

LprTestApp.cpp
    This is the main application source file that contains the application
    class CLprTestApp.

LprTestApp.rc
    This is a listing of all of the Microsoft Windows resources that the
    program uses.  It includes the icons, bitmaps, and cursors that are stored
    in the RES subdirectory.  This file can be directly edited in Microsoft
    Visual C++. Your project resources are in 1033.

res\LprTestApp.ico
    This is an icon file, which is used as the application's icon.  This
    icon is included by the main resource file LprTestApp.rc.

res\LprTestApp.rc2
    This file contains resources that are not edited by Microsoft
    Visual C++. You should place all resources not editable by
    the resource editor in this file.


/////////////////////////////////////////////////////////////////////////////

The application wizard creates one dialog class:

LprTestAppDlg.h, LprTestAppDlg.cpp - the dialog
    These files contain your CLprTestAppDlg class.  This class defines
    the behavior of your application's main dialog.  The dialog's template is
    in LprTestApp.rc, which can be edited in Microsoft Visual C++.

/////////////////////////////////////////////////////////////////////////////

Other Features:

ActiveX Controls
    The application includes support to use ActiveX controls.

/////////////////////////////////////////////////////////////////////////////

Other standard files:

StdAfx.h, StdAfx.cpp
    These files are used to build a precompiled header (PCH) file
    named LprTestApp.pch and a precompiled types file named StdAfx.obj.

Resource.h
    This is the standard header file, which defines new resource IDs.
    Microsoft Visual C++ reads and updates this file.

LprTestApp.manifest
	Application manifest files are used by Windows XP to describe an applications
	dependency on specific versions of Side-by-Side assemblies. The loader uses this
	information to load the appropriate assembly from the assembly cache or private
	from the application. The Application manifest  maybe included for redistribution
	as an external .manifest file that is installed in the same folder as the application
	executable or it may be included in the executable in the form of a resource.
/////////////////////////////////////////////////////////////////////////////

Other notes:

The application wizard uses "TODO:" to indicate parts of the source code you
should add to or customize.

If your application uses MFC in a shared DLL, you will need
to redistribute the MFC DLLs. If your application is in a language
other than the operating system's locale, you will also have to
redistribute the corresponding localized resources mfc110XXX.DLL.
For more information on both of these topics, please see the section on
redistributing Visual C++ applications in MSDN documentation.

/////////////////////////////////////////////////////////////////////////////
