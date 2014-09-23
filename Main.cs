using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using ESRI.ArcGIS.Catalog;
using ESRI.ArcGIS.CatalogUI;
using ESRI.ArcGIS.Geodatabase;
using ESRI.ArcGIS.DataSourcesFile;
using ESRI.ArcGIS.DataSourcesGDB;
using ESRI.ArcGIS.esriSystem;
using ESRI.ArcGIS.Geometry;

namespace FissureStationImport
{
    public partial class Main : Form
    {
        public Main()
        {
            InitializeComponent();
        }

        // NOTE: the Fissure and NonFissure Click events are practically identical. 
        //    Changes to one should probably be replicated in the other

        private void btnFissure_Click(object sender, EventArgs e)
        {
            #region Get Shapefile
            // Ask user to browse to a shapefile
            IGxObject openedFile = OpenShapefile("Select your Fissure Waypoint Shapefile");
            if (openedFile == null) { return; }

            // Open the file as an IFeatureClass
            IWorkspaceFactory wsFact = new ShapefileWorkspaceFactoryClass();
            IFeatureWorkspace ws = wsFact.OpenFromFile(openedFile.Parent.FullName, 0) as IFeatureWorkspace;
            IFeatureClass fissureWaypoints = ws.OpenFeatureClass(openedFile.Name);

            // Make sure user selected a point featureclass
            if (fissureWaypoints.ShapeType != esriGeometryType.esriGeometryPoint)
            {
                MessageBox.Show("The shapefile you selected does not contain points. Try again.");
                return;
            }

            // Make sure that the Coordinate System is set
            IGeoDataset gDs = fissureWaypoints as IGeoDataset;
            IGeoDatasetSchemaEdit schemaEditor = gDs as IGeoDatasetSchemaEdit;
            ISpatialReferenceFactory2 spaRefFact = new SpatialReferenceEnvironmentClass();
            IProjectedCoordinateSystem projCs = spaRefFact.CreateProjectedCoordinateSystem((int)esriSRProjCSType.esriSRProjCS_NAD1983UTM_12N);
            schemaEditor.AlterSpatialReference(projCs);
            
            // Put all the points into a cursor
            IFeatureCursor sourcePoints = fissureWaypoints.Search(null, false);
            #endregion

            #region Prepare for Loop
            // Get a reference to the Stations featureclass in EarthFissure SDE database
            IWorkspace sdeWs = OpenFissureWorkspace();
            IFeatureClass stations = commonFunctions.OpenFeatureClass(sdeWs, "StationPoints");

            // Get a reference to the Fissure Info table in the SDE database
            ITable fissInfoTable = commonFunctions.OpenTable(sdeWs, "FissureStationDescription");

            // Get a reference to the SysInfo table for spinning up IDs
            sysInfo fissDbInfo = new sysInfo(sdeWs);

            // Get field indexes
            Dictionary<string, int> stationIndexes = GetFieldIndexes(stations as ITable);
            Dictionary<string, int> infoIndexes = GetFieldIndexes(fissInfoTable);
            Dictionary<string, int> sourceIndexes = GetFieldIndexes(fissureWaypoints as ITable);

            // Need a geographic coordinate system in the loop
            IGeographicCoordinateSystem geoCs = spaRefFact.CreateGeographicCoordinateSystem((int)esriSRGeoCSType.esriSRGeoCS_NAD1983);

            // Setup the ProgressBar
            setupProgressBar(fissureWaypoints.FeatureCount(null));
            #endregion

            #region Perform Loop
            // Start an edit session
            IWorkspaceEdit wsEditor = sdeWs as IWorkspaceEdit;
            wsEditor.StartEditing(false);
            wsEditor.StartEditOperation();

            try
            {
                // Get Insert Cursors
                IFeatureCursor stationInsert = stations.Insert(true);
                ICursor stationInfoInsert = fissInfoTable.Insert(true);

                // Loop through the source points, appending appropriately to both tables.
                IFeature sourcePoint = sourcePoints.NextFeature();

                while (sourcePoint != null)
                {
                    // Get the new station's identifier
                    string stationID = "FIS.StationPoints." + fissDbInfo.GetNextIdValue("StationPoints");

                    // Get the new station description entry's identifier
                    string descriptionID = "FIS.FissureStationDescription." + fissDbInfo.GetNextIdValue("FissureStationDescription");

                    // Get the Lat/Long values for the new point
                    IGeometry locGeom = sourcePoint.ShapeCopy as IGeometry;
                    locGeom.Project(geoCs);
                    IPoint locPoint = locGeom as IPoint;

                    // Make the new StationPoint
                    IFeatureBuffer newStation = stations.CreateFeatureBuffer();
                    newStation.set_Value(stationIndexes["StationPoints_ID"], stationID);
                    newStation.set_Value(stationIndexes["FieldID"], "");
                    newStation.set_Value(stationIndexes["Label"], "");
                    newStation.set_Value(stationIndexes["Symbol"], sourcePoint.get_Value(sourceIndexes["Waypoint_T"]));
                    newStation.set_Value(stationIndexes["PlotAtScale"], 24000);
                    newStation.set_Value(stationIndexes["LocationConfidenceMeters"], sourcePoint.get_Value(sourceIndexes["Horz_Prec"]));
                    newStation.set_Value(stationIndexes["Latitude"], locPoint.Y);
                    newStation.set_Value(stationIndexes["Longitude"], locPoint.X);
                    newStation.set_Value(stationIndexes["DataSourceID"], "");
                    newStation.Shape = sourcePoint.ShapeCopy;
                    stationInsert.InsertFeature(newStation);

                    // Make the new FissureDescription
                    IRowBuffer newDescription = fissInfoTable.CreateRowBuffer();
                    newDescription.set_Value(infoIndexes["StationID"], stationID);
                    newDescription.set_Value(infoIndexes["DateOfObservation"], sourcePoint.get_Value(sourceIndexes["Date_of_th"]));
                    newDescription.set_Value(infoIndexes["TimeOfObservation"], sourcePoint.get_Value(sourceIndexes["Time_of_th"]));
                    newDescription.set_Value(infoIndexes["SurfaceExpression"], sourcePoint.get_Value(sourceIndexes["Surface_Ex"]));
                    newDescription.set_Value(infoIndexes["Displacement"], sourcePoint.get_Value(sourceIndexes["Displaceme"]));
                    newDescription.set_Value(infoIndexes["SurfaceWidth"], sourcePoint.get_Value(sourceIndexes["Surface_Wi"]));
                    newDescription.set_Value(infoIndexes["FissureDepth"], sourcePoint.get_Value(sourceIndexes["Fissure_De"]));
                    newDescription.set_Value(infoIndexes["Vegetation"], sourcePoint.get_Value(sourceIndexes["Vegetation"]));
                    newDescription.set_Value(infoIndexes["FissureShape"], sourcePoint.get_Value(sourceIndexes["Fissure_Sh"]));
                    newDescription.set_Value(infoIndexes["LineContinuity"], sourcePoint.get_Value(sourceIndexes["Line_Conti"]));
                    newDescription.set_Value(infoIndexes["DataFile"], sourcePoint.get_Value(sourceIndexes["Datafile"]));
                    newDescription.set_Value(infoIndexes["LocationWrtFissure"], sourcePoint.get_Value(sourceIndexes["Location_w"]));
                    newDescription.set_Value(infoIndexes["VegetationType"], sourcePoint.get_Value(sourceIndexes["Vegetatio2"]));
                    newDescription.set_Value(infoIndexes["FissDescription_ID"], descriptionID);
                    stationInfoInsert.InsertRow(newDescription);

                    // Iterate
                    sourcePoint = sourcePoints.NextFeature();
                    progress.PerformStep();
                }

                // Done. Save edits.
                wsEditor.StopEditOperation();
                wsEditor.StopEditing(true);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message + ex.StackTrace);
                wsEditor.StopEditOperation();
                wsEditor.StopEditing(false);
            }
            finally
            {
                progress.Visible = false;
            }
            #endregion
        }

        private void btnNonFissure_Click(object sender, EventArgs e)
        {
            #region Get Shapefile
            // Ask user to browse to a shapefile
            IGxObject openedFile = OpenShapefile("Select your Non-Fissure Waypoint Shapefile");
            if (openedFile == null) { return; }

            // Open the file as an IFeatureClass
            IWorkspaceFactory wsFact = new ShapefileWorkspaceFactoryClass();
            IFeatureWorkspace ws = wsFact.OpenFromFile(openedFile.Parent.FullName, 0) as IFeatureWorkspace;
            IFeatureClass nonFissureWaypoints = ws.OpenFeatureClass(openedFile.Name);

            // Make sure user selected a point featureclass
            if (nonFissureWaypoints.ShapeType != esriGeometryType.esriGeometryPoint)
            {
                MessageBox.Show("The shapefile you selected does not contain points. Try again.");
                return;
            }

            // Make sure that the Coordinate System is set
            IGeoDataset gDs = nonFissureWaypoints as IGeoDataset;
            IGeoDatasetSchemaEdit schemaEditor = gDs as IGeoDatasetSchemaEdit;
            ISpatialReferenceFactory2 spaRefFact = new SpatialReferenceEnvironmentClass();
            IProjectedCoordinateSystem projCs = spaRefFact.CreateProjectedCoordinateSystem((int)esriSRProjCSType.esriSRProjCS_NAD1983UTM_12N);
            schemaEditor.AlterSpatialReference(projCs);

            // Put all the points into a cursor
            IFeatureCursor sourcePoints = nonFissureWaypoints.Search(null, false);
            #endregion

            #region Prepare for Loop
            // Get a reference to the Stations featureclass in EarthFissure SDE database
            IWorkspace sdeWs = OpenFissureWorkspace();
            IFeatureClass stations = commonFunctions.OpenFeatureClass(sdeWs, "StationPoints");

            // Get a reference to the Fissure Info table in the SDE database
            ITable nonFissInfoTable = commonFunctions.OpenTable(sdeWs, "NonFissureStationDescription");

            // Get a reference to the SysInfo table for spinning up IDs
            sysInfo fissDbInfo = new sysInfo(sdeWs);

            // Get field indexes
            Dictionary<string, int> stationIndexes = GetFieldIndexes(stations as ITable);
            Dictionary<string, int> infoIndexes = GetFieldIndexes(nonFissInfoTable);
            Dictionary<string, int> sourceIndexes = GetFieldIndexes(nonFissureWaypoints as ITable);

            // Need a geographic coordinate system in the loop
            IGeographicCoordinateSystem geoCs = spaRefFact.CreateGeographicCoordinateSystem((int)esriSRGeoCSType.esriSRGeoCS_NAD1983);

            // Setup the ProgressBar
            setupProgressBar(nonFissureWaypoints.FeatureCount(null));
            #endregion

            #region Perform Loop
            // Start an edit session
            IWorkspaceEdit wsEditor = sdeWs as IWorkspaceEdit;
            wsEditor.StartEditing(false);
            wsEditor.StartEditOperation();

            try
            {
                // Get Insert Cursors
                IFeatureCursor stationInsert = stations.Insert(true);
                ICursor stationInfoInsert = nonFissInfoTable.Insert(true);

                // Loop through the source points, appending appropriately to both tables.
                IFeature sourcePoint = sourcePoints.NextFeature();

                while (sourcePoint != null)
                {
                    // Get the new station's identifier
                    string stationID = "FIS.StationPoints." + fissDbInfo.GetNextIdValue("StationPoints");

                    // Get the new station description entry's identifier
                    string descriptionID = "FIS.NonFissureStationDescription." + fissDbInfo.GetNextIdValue("NonFissureStationDescription");

                    // Get the Lat/Long values for the new point
                    IGeometry locGeom = sourcePoint.ShapeCopy as IGeometry;
                    locGeom.Project(geoCs);
                    IPoint locPoint = locGeom as IPoint;

                    // Make the new StationPoint
                    IFeatureBuffer newStation = stations.CreateFeatureBuffer();
                    newStation.set_Value(stationIndexes["StationPoints_ID"], stationID);
                    newStation.set_Value(stationIndexes["FieldID"], "");
                    newStation.set_Value(stationIndexes["Label"], "");
                    newStation.set_Value(stationIndexes["Symbol"], "NonFissure");
                    newStation.set_Value(stationIndexes["PlotAtScale"], 24000);
                    newStation.set_Value(stationIndexes["LocationConfidenceMeters"], sourcePoint.get_Value(sourceIndexes["Horz_Prec"]));
                    newStation.set_Value(stationIndexes["Latitude"], locPoint.Y);
                    newStation.set_Value(stationIndexes["Longitude"], locPoint.X);
                    newStation.set_Value(stationIndexes["DataSourceID"], "");
                    newStation.Shape = sourcePoint.ShapeCopy;
                    stationInsert.InsertFeature(newStation);

                    // Make the new FissureDescription
                    IRowBuffer newDescription = nonFissInfoTable.CreateRowBuffer();
                    newDescription.set_Value(infoIndexes["StationID"], stationID);
                    newDescription.set_Value(infoIndexes["TypeOfLineament"], sourcePoint.get_Value(sourceIndexes["Type_of_Li"]));
                    newDescription.set_Value(infoIndexes["Datafile"], sourcePoint.get_Value(sourceIndexes["Datafile"]));
                    newDescription.set_Value(infoIndexes["NonFissDescription_ID"], descriptionID);
                    stationInfoInsert.InsertRow(newDescription);

                    // Iterate
                    sourcePoint = sourcePoints.NextFeature();
                    progress.PerformStep();
                }

                // Done. Save edits.
                wsEditor.StopEditOperation();
                wsEditor.StopEditing(true);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message + ex.StackTrace);
                wsEditor.StopEditOperation();
                wsEditor.StopEditing(false);
            }
            finally
            {
                progress.Visible = false;
            }
            #endregion
        }

        private void btnCancel_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        #region Helper Functions
        private IGxObject OpenShapefile(string Caption)
        {
            IGxDialog fileChooser = new GxDialogClass();
            IEnumGxObject chosenFiles = null;

            fileChooser.Title = Caption;
            fileChooser.ButtonCaption = "Select";
            fileChooser.AllowMultiSelect = false;
            fileChooser.ObjectFilter = new GxFilterShapefilesClass();
            fileChooser.DoModalOpen(0, out chosenFiles);

            chosenFiles.Reset();
            return chosenFiles.Next();
        }

        private IWorkspace OpenFissureWorkspace()
        {
            // Read in the database connection properties from config.txt
            string[] lines = System.IO.File.ReadAllLines(@"../../config.txt");
            Dictionary<string, string> dbConn = new Dictionary<string, string>();
            foreach (string line in lines)
            {
                dbConn.Add(line.Split('=')[0], line.Split('=')[1]);
            }

            IWorkspaceFactory wsFact = new SdeWorkspaceFactoryClass();
            IPropertySet connectionProperties = new PropertySetClass();
            connectionProperties.SetProperty("SERVER", dbConn["server"]);
            connectionProperties.SetProperty("INSTANCE", dbConn["instance"]);
            connectionProperties.SetProperty("DATABASE", dbConn["database"]);
            connectionProperties.SetProperty("AUTHENTICATION_MODE", dbConn["authentication_mode"]);
            connectionProperties.SetProperty("VERSION", dbConn["version"]);

            return wsFact.Open(connectionProperties, 0);
        }

        private Dictionary<string, int> GetFieldIndexes(ITable theTable)
        {
            Dictionary<string, int> theResult = new Dictionary<string, int>();
            IFields theFields = theTable.Fields;

            for (int i = 0; i < theFields.FieldCount; i++)
            {
                theResult.Add(theFields.Field[i].Name, i);
            }

            return theResult;
        }

        private void setupProgressBar(int iterations)
        {
            progress.Minimum = 0;
            progress.Maximum = iterations;
            progress.Step = 1;
            progress.Visible = true;
        }
        #endregion

    }
}