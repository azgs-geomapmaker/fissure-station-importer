This tool was developed by the Arizona Geological Survey (AZGS) to import shapefiles created by Pathfinder Office using data collected with a Trimble GPS. The shapefiles are imported into an ArcSDE NCGMP geodatabase.

All points are imported into the NCGMP-defined Stations feature class. Additionally, Fissure Waypoints are imported into the FissureStationDescription table while Non-Fissure Waypoints are imported into the NonFissureStationDescription table.

Prerequisties: **ArcObjects SDK** (from the ArcGIS installation disc) and **Visual Studio**

### Setup

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

### Run

1. Find the file **FissureStationImport.exe** in the **/bin/Debug** folder.
3. Double-click **FissureStationImport.exe** to run.
4. Choose either **Import Fissure Waypoints** or **Import Non-Fissure Waypoints**.
5. Select the shapefile to import.
6. Click **I'm all done.** when finished.