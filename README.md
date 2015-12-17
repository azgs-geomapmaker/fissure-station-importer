This tool was developed by the Arizona Geological Survey (AZGS) to import shapefiles created by Pathfinder Office using data collected with a Trimble GPS. The shapefiles are imported into an ArcSDE NCGMP geodatabase.

All points are imported into the NCGMP-defined Stations feature class. Additionally, Fissure Waypoints are imported into the FissureStationDescription table while Non-Fissure Waypoints are imported into the NonFissureStationDescription table.

Prerequisties: **ArcObjects SDK** (from the ArcGIS installation disc) and **Visual Studio**

### Version 2.0 - ArcGIS Add-in Version

The standalone version may stop working, due to licensing changes with ESRI ArcObjects libraries.  The failure occurs when the 
dialog box  that allows shape file selection stops showing shapefiles.  As a corrective measure, the Fissure tool has been 
reconfigured as  an Add-in that can be loaded into the ArcMap.  


### Standalone Executable Setup

1. Clone this repository.
2. Open the project by double-clicking **FissureStationImport.sln**.
3. Create the following file called `config.txt` and place in the root directory.
4. Inside `config.txt` write the connection properties for your ArcSDE geodatabse.
  ```
  server=[your server location for the ArcSDE geodatabse]
  instance=sde:sqlserver:[your server]\[your path]
  database=[your database]
  authentication_mode=[your authentication mode]
  version=[your version]
  ```
  See ESRI help for [Connecting to geodatbases and databases](http://resources.arcgis.com/en/help/arcobjects-net/conceptualhelp/0001/0001000003s8000000.htm).
5. Run **Build**->**Build Solution**. (This will build **FissureStationImport.exe** in the **/bin/Debug** folder.)

### Standalaone Executable Run

1. Find the file **FissureStationImport.exe** in the **/bin/Debug** folder.
3. Double-click **FissureStationImport.exe** to run.
4. Choose either **Import Fissure Waypoints** or **Import Non-Fissure Waypoints**.
5. Select the shapefile to import.
6. Click **I'm all done.** when finished.


### ArcGIS Add-in Setup

1.  To use the existing Add-in Executable download the 2 files

     ArcMap Add-in Version/FissureBar.esriAddIn
	 ArcMap Add-in Version/config.txt
	 
2.  Copy config.txt	to C:\tmp directory.  This file provides connection setup.  You can change
    either the dbfile, which is an Arc connection file to the EarthFissure database, or
	all the connection parameters (database, server, instance,version authentication mode).
	If dbfile is present, it will use that parameter over the connection parameters.
	
3.   In ArcMap, click Customize
4.   Select Customize Mode…
5 .  Click Add from file…
8.   Navigate to and select FissureBar.esriAddIn
9.   Click Open
	
The Visual Studio project within the ArcMap Add-in Version is a standalone project in the VS 2010 environment.

The Add-in runs the same as the standalone version, there are 2 buttons that allow you to import shape files.

