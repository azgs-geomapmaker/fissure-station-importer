using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using System.Windows.Forms;
using ESRI.ArcGIS.Catalog;
using ESRI.ArcGIS.CatalogUI;
using ESRI.ArcGIS.Geodatabase;
using ESRI.ArcGIS.DataSourcesFile;
using ESRI.ArcGIS.DataSourcesGDB;
using ESRI.ArcGIS.esriSystem;
using ESRI.ArcGIS.Geometry;

namespace FissureBar
{
    public class NonFishbutton : ESRI.ArcGIS.Desktop.AddIns.Button
    {
        public NonFishbutton()
        {
        }

        protected override void OnClick()
        {

            #region Get Shapefile
            // Ask user to browse to a shapefile
            IGxObject openedFile = commonFunctions.OpenShapefile("Select your Non-Fissure Waypoint Shapefile");
            if (openedFile == null) { return; }

            // Open the file as an IFeatureClass
            IWorkspaceFactory wsFact = new ShapefileWorkspaceFactoryClass();
            IFeatureWorkspace ws = wsFact.OpenFromFile(openedFile.Parent.FullName, 0) as IFeatureWorkspace;

            string path = @"C:\tmp\config.txt";
            if (!File.Exists(path))
            {
                MessageBox.Show("Missing the config File !");
                return;
            }

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
            IWorkspace sdeWs = commonFunctions.OpenFissureWorkspace();
            IFeatureClass stations = commonFunctions.OpenFeatureClass(sdeWs, "Stations");

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
           // setupProgressBar(nonFissureWaypoints.FeatureCount(null));
            #endregion

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
                    newStation.set_Value(stationIndexes["Stations_ID"], stationID);
                    newStation.set_Value(stationIndexes["FieldID"], "");
                    newStation.set_Value(stationIndexes["Label"], "");
                    newStation.set_Value(stationIndexes["Symbol"], "NonFissure");
                    newStation.set_Value(stationIndexes["PlotAtScale"], 24000);
                    newStation.set_Value(stationIndexes["LocationConfidenceMeters"], sourcePoint.get_Value(sourceIndexes["Horz_Prec"]));
                    newStation.set_Value(stationIndexes["MapY"], locPoint.Y);
                    newStation.set_Value(stationIndexes["MapX"], locPoint.X);
                    newStation.set_Value(stationIndexes["DataSourceID"], "");
                    newStation.Shape = sourcePoint.ShapeCopy;
                    stationInsert.InsertFeature(newStation);

                    // Make the new FissureDescription
                    IRowBuffer newDescription = nonFissInfoTable.CreateRowBuffer();
                    newDescription.set_Value(infoIndexes["stationid"], stationID);
                    newDescription.set_Value(infoIndexes["typeoflineament"], sourcePoint.get_Value(sourceIndexes["Type_of_Li"]));
                    newDescription.set_Value(infoIndexes["datafile"], sourcePoint.get_Value(sourceIndexes["Datafile"]));
                    newDescription.set_Value(infoIndexes["nonfissdescription_id"], descriptionID);
                    stationInfoInsert.InsertRow(newDescription);

                    // Iterate
                    sourcePoint = sourcePoints.NextFeature();
                   // progress.PerformStep();
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
                //progress.Visible = false;
            }
        }

        protected override void OnUpdate()
        {
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

    }
}
