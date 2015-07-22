VehicleDataRM.dll and VehicleInterfaceMgrRM.dll 

Built with VS2005 to go on machines with WinAlign version 6-9 
because those machines have the VS2005 redistributable on them.  
The machines can't be updated with a redistributable from a 
different Visual Studio without a lot of effort because the 
OS never gets updated from what it was when it left the factory.  
So, as this binary gets updated and sent out with Hunter Vehicle 
Specification releases it has to get built with VS2005 and with 
the Visual Studio that was used for WinAlign version 10 and up.

There are actually now three versions of VehicleDataRM.dll
VS 2003 for WinAlign 6.X to 9.X
VS 2005 for WinAlign 10.X to 12.X
VS 2010 for anything newer.

There are two versions of VehicleInterfaceMgrRM.dll
VS2005 for WinAlign 11.X to 12.X
VS 2010 for anything newer.

