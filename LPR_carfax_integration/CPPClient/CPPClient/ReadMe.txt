========================================================================
    CONSOLE APPLICATION : CPPClient Project Overview
========================================================================

This is a test Visual C++ project for carfax integration into the LPR application.


CPPClient.vcxproj
    This is the main project file for VC++ projects generated using an Application Wizard.
    It contains information about the version of Visual C++ that generated the file, and
    information about the platforms, configurations, and project features selected with the
    Application Wizard.

CPPClient.vcxproj.filters
    This is the filters file for VC++ projects generated using an Application Wizard. 
    It contains information about the association between the files in your project 
    and the filters. This association is used in the IDE to show grouping of files with
    similar extensions under a specific node (for e.g. ".cpp" files are associated with the
    "Source Files" filter).

CPPClient.cpp
    An application to test LPRVinLookup when in a .cpp file
    After changing .cs code to .dll and then changing .dll to .tlb (with access key)
    and since the function is COM visible, this .cpp file should be able to request a
    VIN from HunterNet

/////////////////////////////////////////////////////////////////////////////
Other standard files:

StdAfx.h, StdAfx.cpp
    These files are used to build a precompiled header (PCH) file
    named CPPClient.pch and a precompiled types file named StdAfx.obj.

/////////////////////////////////////////////////////////////////////////////
Other notes:

AppWizard uses "TODO:" comments to indicate parts of the source code you
should add to or customize.

/////////////////////////////////////////////////////////////////////////////
