##################################################################################
#
# Script name: CopyDebugBinaries.ps1
# Author:  David Voeller
#
# Copy the debug version of WinAlign binaries to the 
# Program Files\Hunter\Aligner\Execute folder.  Normally the release
# versions of the binaries are already there but if you run a 
# debug version of WinAlign you will have to have the debug version of 
# the binaries available, particularly heclogd.dll.
#
##################################################################################
# Is this a 64 bit process
function Test-Win64() {
    return [IntPtr]::size -eq 8
}

# Is this a 32 bit process
function Test-Win32() {
    return [IntPtr]::size -eq 4
}

# Is this a Wow64 powershell host
function Test-Wow64() {
    return (Test-Win32) -and (test-path env:\PROCESSOR_ARCHITEW6432)
}

function Get-ProgramFiles32() 
{
    if (Test-Win64) {
        return ${env:ProgramFiles(x86)}
    }
    
    return $env:ProgramFiles
}



# If the path exists then perform all of the copying
$scriptpath = $MyInvocation.MyCommand.Path
$ParentDir = Split-Path $scriptpath
$DbgBin = Join-Path $ParentDir "DebugBinaries"

if (Test-Path $DbgBin)
{
    Remove-Item $DbgBin -recurse
}

Write-Host "Creating folder $DbgBin"
New-Item -Path $DbgBin -type directory

# Copy-Item will over write any file that already exists in the folder
$BinToCopy = Join-Path $ParentDir "Dll\Win32\Debug\AlignmentNETWebServiced.dll"
Copy-Item  $BinToCopy  $DbgBin
$BinToCopy = Join-Path $ParentDir "Dll\Win32\Debug\AltovaNatived.dll"
Copy-Item  $BinToCopy  $DbgBin
$BinToCopy = Join-Path $ParentDir "Dll\Win32\Debug\AltovaNativeXmld.dll"
Copy-Item  $BinToCopy  $DbgBin
$BinToCopy = Join-Path $ParentDir "Dll\Win32\Debug\HecLogd.dll"
Copy-Item  $BinToCopy  $DbgBin
$BinToCopy = Join-Path $ParentDir "Dll\Win32\Debug\Hecsekd.dll"
Copy-Item  $BinToCopy  $DbgBin
$BinToCopy = Join-Path $ParentDir "Dll\Win32\Debug\HunterDataActiveX01d.dll"
Copy-Item  $BinToCopy  $DbgBin
$BinToCopy = Join-Path $ParentDir "Dll\Win32\Debug\SetupReportDataSetd.dll"
Copy-Item  $BinToCopy  $DbgBin

Write-Host "Binaries copied to $DbgBin"

$PgmFiles = Get-ProgramFiles32
Write-Host "The Program Files directory from environment variable is $PgmFiles"
$WinAlignExecutePath = Join-Path $PgmFiles  "Hunter\Aligner\Execute"
Write-Host "The WinAlign path is $WinAlignExecutePath"

# If the path exists then perform all of the copying
# This is the path on a typicaly 32bit OS that WinAlign runs out of
if (Test-Path  $WinAlignExecutePath)
{
    # Copy-Item will over write any file that already exists in the folder
    $AllDbgBinFiles = Join-Path $DbgBin "*"
    Copy-Item $AllDbgBinFiles   $WinAlignExecutePath
}
else
{
    Write-Host "The folder $WinAlignExecutePath     doesn't exist so nothing was copied to that location."
}


$COM = Join-Path $DbgBin "COM"
if (Test-Path $COM)
{
    Write-Host "Folder $COM already exists"
}
else
{
    Write-Host "Creating folder $COM"
    New-Item -Path $COM -type directory
}

# Copy-Item will over write any file that already exists in the folder
$BinToCopy = Join-Path $ParentDir "Dll\Win32\Debug\AcqMgrDM.dll"
Copy-Item  $BinToCopy  $COM
$BinToCopy = Join-Path $ParentDir "Dll\Win32\Debug\AlignConfigMgrDM.dll"
Copy-Item  $BinToCopy  $COM
$BinToCopy = Join-Path $ParentDir "Dll\Win32\Debug\AlignDataValueDM.dll"
Copy-Item  $BinToCopy  $COM
$BinToCopy = Join-Path $ParentDir "Dll\Win32\Debug\AlignSuiteDM.dll"
Copy-Item  $BinToCopy  $COM
$BinToCopy = Join-Path $ParentDir "Dll\Win32\Debug\BMWRideHeightDM.dll"
Copy-Item  $BinToCopy  $COM
$BinToCopy = Join-Path $ParentDir "Dll\Win32\Debug\CalPlanViewAxDM.ocx"
Copy-Item  $BinToCopy  $COM
$BinToCopy = Join-Path $ParentDir "Dll\Win32\Debug\CameraLiftDU.ocx"
Copy-Item  $BinToCopy  $COM
$BinToCopy = Join-Path $ParentDir "Dll\Win32\Debug\HComCCDDisplayDM.ocx"
Copy-Item  $BinToCopy  $COM
$BinToCopy = Join-Path $ParentDir "Dll\Win32\Debug\HComMultiGridDM.ocx"
Copy-Item  $BinToCopy  $COM
$BinToCopy = Join-Path $ParentDir "Dll\Win32\Debug\HibDeviceDM.dll"
Copy-Item  $BinToCopy  $COM
$BinToCopy = Join-Path $ParentDir "Dll\Win32\Debug\ImageVwDM.ocx"
Copy-Item  $BinToCopy  $COM
$BinToCopy = Join-Path $ParentDir "Dll\Win32\Debug\LiftConsoleSuiteDM.dll"
Copy-Item  $BinToCopy  $COM
$BinToCopy = Join-Path $ParentDir "Dll\Win32\Debug\QualityDataDM.dll"
Copy-Item  $BinToCopy  $COM
$BinToCopy = Join-Path $ParentDir "Dll\Win32\Debug\RemoteSuiteDM.dll"
Copy-Item  $BinToCopy  $COM
$BinToCopy = Join-Path $ParentDir "Dll\Win32\Debug\RShimVwDU.ocx"
Copy-Item  $BinToCopy  $COM
$BinToCopy = Join-Path $ParentDir "Dll\Win32\Debug\SnrDiagDetailScreenDU.ocx"
Copy-Item  $BinToCopy  $COM
$BinToCopy = Join-Path $ParentDir "Dll\Win32\Debug\SnrDiagPVScreenDU.ocx"
Copy-Item  $BinToCopy  $COM
$BinToCopy = Join-Path $ParentDir "Dll\Win32\Debug\SnrDiagToeDiagnosticsDU.ocx"
Copy-Item  $BinToCopy  $COM
$BinToCopy = Join-Path $ParentDir "Dll\Win32\Debug\VehicleDataDM.dll"
Copy-Item  $BinToCopy  $COM
$BinToCopy = Join-Path $ParentDir "Dll\Win32\Debug\VehicleInterfaceMgrDM.dll"
Copy-Item  $BinToCopy  $COM
$BinToCopy = Join-Path $ParentDir "Dll\Win32\Debug\VocabDataDM.dll"
Copy-Item  $BinToCopy  $COM

$WinAligComPath = Join-Path $PgmFiles  "Hunter\Common\Com"
Write-Host "The WinAlign path is $WinAligComPath"

Write-Host "Binaries copied to $DbgBin but are not copied to $WinAligComPath"
Write-Host "because you likely only want certain debug versions of the ATL COM objects"
Write-Host "to be registered.  That means you must copy and register manually."


