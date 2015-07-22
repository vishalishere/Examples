RShimVwRU.ocx, VehicleDataRM.dll and VehicleInterfaceMgrRM.dll 

Built using a VS2010 project that specifies VC80 under the project properties Configuration Properties-->General-->Platform Toolset.  In order to use the VS2005 compiler (VC80) with TFS we have to install a program called Dafodil which is stored under $\WinAlign\Mainline\HunterTools\TFSBuild\Daffodil.ENU.msi.

The 2005 version goes on customer alignment machines with WinAlign version 6-9 because those alignment machines have the VS2005 redistributable on them.  The machines can't be updated with a redistributable from a different Visual Studio without a lot of effort because the OS never gets updated from what it was when it left the factory.  So, as this binary gets updated and sent out with Hunter Vehicle Specification releases it has to get built with VS2005 and with the Visual Studio that was used for WinAlign version 10 and up.

There are actually now three versions of VehicleDataRM.dll
VS 2003 for WinAlign 6.X to 9.X
VS 2005 for WinAlign 10.X to 12.X
VS 2010 for anything newer.

There are two versions of VehicleInterfaceMgrRM.dll
VS 2005 for WinAlign 11.X to 12.X
VS 2010 for anything newer.

There are 3 versions of the RShimVwXX.ocx.
VS 2005 multicode (RM) for WinAlign version 9.1
VS 2005 unicode (RU) for WinAlign version 10.x to 12.x
VS 2010 unicode (RU) for anything newer


