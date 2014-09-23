using System;
using Microsoft.Win32; 
using System.Collections.Generic;
using System.Text;
using System.Linq;
using ESRI.ArcGIS.Geodatabase;
using ESRI.ArcGIS.esriSystem;
using ESRI.ArcGIS.Catalog;
using ESRI.ArcGIS.CatalogUI;

namespace FissureStationImport
{
    public class commonFunctions
    {
        #region "Registry Manipulation"

        public static void WriteReg(string Path, string Name, string Value)
        {
            RegistryKey theKey = Registry.CurrentUser.CreateSubKey(Path);
            theKey.SetValue(Name, Value);
        }

        public static string ReadReg(string Path, string Name)
        {
            RegistryKey theKey = Registry.CurrentUser.CreateSubKey(Path);
            string theValue = (string)theKey.GetValue(Name);
            if (theValue == null)
            {
                return null;
            }
            else
            {
                return theValue;
            }

        }

        #endregion

        public static IGxObject OpenArcFile(IGxObjectFilter objectFilter, string Caption)
        {
            IGxDialog fileChooser = new GxDialogClass();
            IEnumGxObject chosenFiles = null;

            fileChooser.Title = Caption;
            fileChooser.ButtonCaption = "Select";
            fileChooser.AllowMultiSelect = false;
            fileChooser.ObjectFilter = objectFilter;
            fileChooser.DoModalOpen(0, out chosenFiles);

            chosenFiles.Reset();
            return chosenFiles.Next();
        }

        public static ITable OpenTable(IWorkspace TheWorkspace, string TableName)
        {
            IFeatureWorkspace featureWorkspace = (IFeatureWorkspace)TheWorkspace;
            string theTable = QualifyClassName(TheWorkspace, TableName);

            return featureWorkspace.OpenTable(theTable);
        }

        public static IFeatureClass OpenFeatureClass(IWorkspace TheWorkspace, string FeatureClassName)
        {
            IFeatureWorkspace featureWorkspace = (IFeatureWorkspace)TheWorkspace;
            string theFC = QualifyClassName(TheWorkspace, FeatureClassName);

            return featureWorkspace.OpenFeatureClass(theFC);
        }

        public static IRelationshipClass OpenRelationshipClass(IWorkspace TheWorkspace, string RelationshipClassName)
        {
            IFeatureWorkspace featureWorkspace = (IFeatureWorkspace)TheWorkspace;
            return featureWorkspace.OpenRelationshipClass(commonFunctions.QualifyClassName(TheWorkspace, RelationshipClassName));
        }

        public static ITopology OpenTopology(IWorkspace TheWorkspace, string TopologyName)
        {
            ITopologyWorkspace topoWorkspace = (ITopologyWorkspace)TheWorkspace;
            return topoWorkspace.OpenTopology(commonFunctions.QualifyClassName(TheWorkspace, TopologyName));
        }

        public static IRepresentationWorkspaceExtension GetRepExtension(IWorkspace Workspace)
        {
            IWorkspaceExtensionManager ExtManager = (IWorkspaceExtensionManager)Workspace;
            UID theUID = new UIDClass();
            theUID.Value = "{FD05270A-8E0B-4823-9DEE-F149347C32B6}";
            return (IRepresentationWorkspaceExtension)ExtManager.FindExtension(theUID);
        }

        public static IRepresentationClass GetRepClass(IWorkspace Workspace, string RepresentationClassName)
        {
            // Get the RepresentationWorkspaceExtension from the Workspace
            IRepresentationWorkspaceExtension RepWsExt = GetRepExtension(Workspace);

            // Get and return the RepresentationClass
            return RepWsExt.OpenRepresentationClass(commonFunctions.QualifyClassName(Workspace, RepresentationClassName));
        }

        public static string QualifyClassName(IWorkspace theWorkspace, string givenClassName)
        {
            if (theWorkspace.Type == esriWorkspaceType.esriRemoteDatabaseWorkspace)
            {
                string dbName = ((IDataset)theWorkspace).Name;

                // ------- TROUBLE ---------
                // I don't know how to handle situations where the owner is not DBO.
                // ------- TROUBLE ---------
                string owner = "DBO";

                ISQLSyntax Qualifier = (ISQLSyntax)theWorkspace;
                return Qualifier.QualifyTableName(dbName, owner, givenClassName);
            }
            else { return givenClassName; }
        }

    }
}