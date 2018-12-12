#if UNITY_EDITOR
using System.Collections.Generic;
using UnityEngine;
using System;
using System.Net;
using System.IO;
using System.Linq;
using UnityEditor;
#endif

namespace BrandXR
{
#if UNITY_EDITOR
    public class BXRBuildAssetBundles: EditorWindow
#else
    public class BXRBuildAssetBundles
#endif
    {

#if UNITY_EDITOR
        #region VARIABLES
        //Helper class to hold onto data about an asset bundle, used when generating the CSV data for our asset bundles
        public class AssetBundleInfo
        {
            public string name = "";
            public string path = "";
            public string versionType = "hash";
            public Hash128 version = new Hash128();
            public uint checksum = new uint();
        }
        private List<AssetBundleInfo> assetBundleInfos = new List<AssetBundleInfo>();

        //Instructions Group Variables
        private bool showInstructionsFoldoutGroup = false;

        //Choose Platforms To Build For Group
        private bool showBuildForPlatformsFoldoutGroup = false;
        private Dictionary<BuildTarget, bool> buildPlatforms = new Dictionary<BuildTarget, bool>();

        public string buildToFolder = "AssetBundles";
        private string csvFileName = "CSVData";

        //When the asset bundles our built for a Build Target, Unity returns a manifest with information about the build assets
        private Dictionary<BuildTarget, AssetBundleManifest> manifests = new Dictionary<BuildTarget, AssetBundleManifest>();

        //Compression Settings Variables
        private bool showCompressionSettingsFoldoutGroup = false;
        private BuildAssetBundleOptions buildAssetBundleOptions = BuildAssetBundleOptions.UncompressedAssetBundle;

        //CSV Settings Variables
        private bool showCSVFoldoutGroup = false;

        private bool enableCSV = true;


        //Copy To Streaming Assets Path
        private bool showStreamingAssetsFoldoutGroup = false;

        private bool enableCopy = false;


        //Upload FTP Variables
        private bool showUploadFoldoutGroup = false;
        private bool enableFTPUploading = false;
        private string serverURL = "";
        private string username = "";
        private string password = "";
        private bool passiveMode = false;

        #endregion

        #region ONGUI
        [MenuItem( "BrandXR/Show Asset Bundle Builder Window" )]
        //---------------------------------------------------//
        public static void ShowAssetBundleBuilder()
        //---------------------------------------------------//
        {
            Type inspectorType = Type.GetType( "UnityEditor.InspectorWindow,UnityEditor.dll" );
            EditorWindow inspectorWindow = EditorWindow.GetWindow<EditorWindow>( new Type[] { inspectorType } );

            BXRBuildAssetBundles window = null;

            //Try to dock our new window next to the inspector
            if( inspectorWindow != null )
            {
                window = EditorWindow.GetWindow<BXRBuildAssetBundles>( "Bundle Builder", true, new Type[] { inspectorType } );
            }
            else
            {
                window = EditorWindow.GetWindow<BXRBuildAssetBundles>( "Bundle Builder", true );
            }

        } //END ShowAssetBundleBuilder

        //---------------------------------------------------//
        public void OnGUI()
        //---------------------------------------------------//
        {
            
            //Setup our dictionary for what platforms to build
            if( buildPlatforms == null ) { buildPlatforms = new Dictionary<BuildTarget, bool>(); }

            if( buildPlatforms != null && buildPlatforms.Count == 0 )
            {
                foreach( BuildTarget buildTarget in Enum.GetValues( typeof( BuildTarget ) ) )
                {
                    buildPlatforms[ buildTarget ] = false;
                }

                buildPlatforms[ EditorUserBuildSettings.activeBuildTarget ] = true;
            }

            //Change our label width for things like toggle boxes larger than normal
            EditorGUIUtility.labelWidth = 225f;

            //Setup our Menu Header label
            GUIStyle style = new GUIStyle( GUI.skin.GetStyle( "label" ) );
            style.margin = new RectOffset( 10, 10, 10, 10 );
            style.wordWrap = true;
            style.fontSize = 20;
            style.fontStyle = FontStyle.Bold;
            style.alignment = TextAnchor.MiddleCenter;

            GUILayout.Label( "BrandXR - Asset Bundle Builder", style );

            //Setup and Begin our instructions box
            GUILayout.BeginVertical( GUI.skin.box );

            //Setup our Instructions foldout group
            style = new GUIStyle( GUI.skin.GetStyle( "foldout" ) );

            showInstructionsFoldoutGroup = EditorGUILayout.Foldout( showInstructionsFoldoutGroup, "Instructions", style );


            //Setup our Instructions Header label
            style = new GUIStyle( GUI.skin.GetStyle( "label" ) );
            style.margin = new RectOffset( 10, 10, 15, 0 );
            style.wordWrap = true;
            style.fontSize = 13;
            style.fontStyle = FontStyle.Bold;
            style.alignment = TextAnchor.MiddleCenter;

            if( showInstructionsFoldoutGroup )
            {
                GUILayout.Label( "How To Build Asset Bundles", style );
            }

            //Setup our instructions label
            style = new GUIStyle( GUI.skin.GetStyle( "label" ) );
            style.margin = new RectOffset( 10, 10, 5, 10 );
            style.wordWrap = true;
            style.fontSize = 10;
            style.fontStyle = FontStyle.Italic;
            style.alignment = TextAnchor.MiddleCenter;

            if( showInstructionsFoldoutGroup )
            {
                GUILayout.Label( "Click on a prefab and press the 'Asset Bundle' option at the bottom of the inspector " +
                    "and give your prefab an Asset Bundle name.\n\n" +
                    "When you press the 'Create Asset Bundle' button, your asset bundles will be " +
                    "created for the platforms you've chosen in this window and sent to the 'AssetBundles' folder.\n\n" +
                    "Your asset bundles for each platform will be combined in the 'AssetBundles' folder. So regardless of how " +
                    "many platforms you build all the Asset Bundles will be in the same folder\n\n" +
                    "To use your asset bundles, either upload them to a web server or place them into the 'StreamingAssets' folder.\n\n" +
                    "In your app, make use of the 'Block Event - Asset Bundle' component to Download, Load, " +
                    "and Instantiate your Asset Bundles as prefabs at application runtime.", style );
            }

            //End our instructions box
            GUILayout.EndVertical();

            //Setup and Begin our 'Build For Platforms' box
            GUILayout.BeginVertical( GUI.skin.box );

            //Setup our 'Build For Platforms' foldout group
            style = new GUIStyle( GUI.skin.GetStyle( "foldout" ) );

            showBuildForPlatformsFoldoutGroup = EditorGUILayout.Foldout( showBuildForPlatformsFoldoutGroup, "Build Asset Bundles For Platforms", style );

            //Show the buttons for what platforms we want to generate asset bundles for
            style = new GUIStyle( GUI.skin.GetStyle( "toggle" ) );
            style.margin = new RectOffset( 15, 15, 15, 15 );

            if( showBuildForPlatformsFoldoutGroup )
            {
                GUILayout.Space( 10f );

                if( buildPlatforms != null && buildPlatforms.Count > 0 )
                {
                    buildPlatforms[ BuildTarget.StandaloneWindows64 ] = EditorGUILayout.Toggle( "Windows 64-Bit", buildPlatforms[ BuildTarget.StandaloneWindows64 ], style );
                    buildPlatforms[ BuildTarget.StandaloneOSX ] = EditorGUILayout.Toggle( "OSX", buildPlatforms[ BuildTarget.StandaloneOSX ], style );
                    buildPlatforms[ BuildTarget.StandaloneLinux ] = EditorGUILayout.Toggle( "Linux", buildPlatforms[ BuildTarget.StandaloneLinux ], style );
                    buildPlatforms[ BuildTarget.WSAPlayer ] = EditorGUILayout.Toggle( "WSA - Universal Windows App", buildPlatforms[ BuildTarget.WSAPlayer ], style );
                    buildPlatforms[ BuildTarget.iOS ] = EditorGUILayout.Toggle( "IOS", buildPlatforms[ BuildTarget.iOS ], style );
                    buildPlatforms[ BuildTarget.Android ] = EditorGUILayout.Toggle( "Android", buildPlatforms[ BuildTarget.Android ], style );
                    buildPlatforms[ BuildTarget.WebGL ] = EditorGUILayout.Toggle( "WebGL", buildPlatforms[ BuildTarget.WebGL ], style );
                    buildPlatforms[ BuildTarget.PSP2 ] = EditorGUILayout.Toggle( "PlayStation Vita", buildPlatforms[ BuildTarget.PSP2 ], style );
                    buildPlatforms[ BuildTarget.PS4 ] = EditorGUILayout.Toggle( "PlayStation 4", buildPlatforms[ BuildTarget.PS4 ], style );
                    buildPlatforms[ BuildTarget.XboxOne ] = EditorGUILayout.Toggle( "Xbox One", buildPlatforms[ BuildTarget.XboxOne ], style );
                    buildPlatforms[ BuildTarget.N3DS ] = EditorGUILayout.Toggle( "Nintendo 3DS", buildPlatforms[ BuildTarget.N3DS ], style );
                    buildPlatforms[ BuildTarget.Switch ] = EditorGUILayout.Toggle( "Nintendo Switch", buildPlatforms[ BuildTarget.Switch ], style );
                }

            }

            GUILayout.EndVertical();


            //Setup and Begin our 'Compression Settings' box
            GUILayout.BeginVertical( GUI.skin.box );

            //Setup our 'Compression Settings' foldout group
            style = new GUIStyle( GUI.skin.GetStyle( "foldout" ) );

            showCompressionSettingsFoldoutGroup = EditorGUILayout.Foldout( showCompressionSettingsFoldoutGroup, "Compression Settings", style );

            if( showCompressionSettingsFoldoutGroup )
            {
                //Setup button styling to look like a website url
                style = new GUIStyle( GUI.skin.label );
                style.wordWrap = true;
                style.normal.textColor = Color.blue;

                GUILayout.Space( 10f );

                if( GUILayout.Button( "Open Unity Docs on compression settings", style ) )
                {
                    Application.OpenURL( "https://docs.unity3d.com/ScriptReference/BuildAssetBundleOptions.html" );
                }

                //Setup our Compressions settings
                GUILayout.Space( 10f );
                buildAssetBundleOptions = (BuildAssetBundleOptions)EditorGUILayout.EnumPopup( "Compression Setting:", buildAssetBundleOptions );
            }

            GUILayout.EndVertical();




            //Setup and Begin our 'CSV Settings' box
            GUILayout.BeginVertical( GUI.skin.box );

            //Setup our 'CSV Settings' foldout group
            style = new GUIStyle( GUI.skin.GetStyle( "foldout" ) );

            showCSVFoldoutGroup = EditorGUILayout.Foldout( showCSVFoldoutGroup, "CSV Settings", style );

            if( showCSVFoldoutGroup )
            {
                EditorGUILayout.Space();

                enableCSV = EditorGUILayout.Toggle( "Create CSV From Asset Bundles", enableCSV, GUI.skin.toggle );

                //Setup our CSV instructions label
                style = new GUIStyle( GUI.skin.GetStyle( "label" ) );
                style.margin = new RectOffset( 10, 10, 5, 10 );
                style.wordWrap = true;
                style.fontSize = 10;
                style.fontStyle = FontStyle.Italic;
                style.alignment = TextAnchor.MiddleCenter;

                GUILayout.Label( "\n\nEnable this to build a CSV file with information about your AssetBundles.\n\n" +
                    "Used by the Block Event - Asset Bundle component to load in your AssetBundles at runtime.\n\n" +
                    "The generated CSV file will be added to the AssetBundles folder and is split into the following columns with a top row dedicated to these column names.\n\n" +
                    "Name - The name of the asset bundle, as tagged in the Unity Editor\n\n" +
                    "Path - URL to the Asset Bundle in the AssetBundle folder\n\n" +
                    "VersionType - Will be set to 'hash'\n\n" +
                    "Version - A Hash128 generated by the Unity Asset Bundle system\n\n" +
                    "Checksum - The CRC-32 checksum to compare to ensure Asset Bundle is validated and intact"
                    , style );
            }

            GUILayout.EndVertical();




            //Setup and Begin our 'Copy To Streaming Assets Settings' box
            GUILayout.BeginVertical( GUI.skin.box );

            //Setup our 'Copy To Streaming Assets Settings' foldout group
            style = new GUIStyle( GUI.skin.GetStyle( "foldout" ) );

            showStreamingAssetsFoldoutGroup = EditorGUILayout.Foldout( showStreamingAssetsFoldoutGroup, "Copy AssetBundles To Streaming Assets", style );

            if( showStreamingAssetsFoldoutGroup )
            {
                EditorGUILayout.Space();

                enableCopy = EditorGUILayout.Toggle( "Copy to Streaming Assets", enableCopy, GUI.skin.toggle );

                //Setup our Copy instructions label
                style = new GUIStyle( GUI.skin.GetStyle( "label" ) );
                style.margin = new RectOffset( 10, 10, 5, 10 );
                style.wordWrap = true;
                style.fontSize = 10;
                style.fontStyle = FontStyle.Italic;
                style.alignment = TextAnchor.MiddleCenter;

                GUILayout.Label( "\n\nEnable this option to make a copy of the 'AssetBundles' folder in the " +
                    "StreamingAssets data path when the Asset Bundles have finished being created\n\n" +
                    "WARNING: This will delete the current 'AssetBundles' folder in StreamingAssets folder and recreate it by copying the newly created 'AssetBundles' folder", style );
            }

            GUILayout.EndVertical();



            //Setup and Begin our 'FTP Upload Settings' box
            GUILayout.BeginVertical( GUI.skin.box );

            //Setup our 'FTP Upload Settings' foldout group
            style = new GUIStyle( GUI.skin.GetStyle( "foldout" ) );

            showUploadFoldoutGroup = EditorGUILayout.Foldout( showUploadFoldoutGroup, "FTP Upload Settings", style );

            if( showUploadFoldoutGroup )
            {
                EditorGUILayout.Space();

                //Setup our CSV instructions label
                style = new GUIStyle( GUI.skin.GetStyle( "label" ) );
                style.margin = new RectOffset( 10, 10, 5, 10 );
                style.wordWrap = true;
                style.fontSize = 10;
                style.fontStyle = FontStyle.Italic;
                style.alignment = TextAnchor.MiddleCenter;

                GUILayout.Label( "WARNING: Not Currently Implemented", style );


                //Show a boolean toggle to turn FTP uploading on/off
                style = new GUIStyle( GUI.skin.GetStyle( "toggle" ) );
                style.margin = new RectOffset( 15, 15, 15, 15 );

                enableFTPUploading = EditorGUILayout.Toggle( "Upload Bundles To Server Using FTP", enableFTPUploading, style );

                //Change our label width
                EditorGUIUtility.labelWidth = 100f;

                //Show text fields
                style = new GUIStyle( GUI.skin.GetStyle( "textarea" ) );
                style.margin = new RectOffset( 15, 15, 15, 15 );

                serverURL = EditorGUILayout.TextField( "Server URL", serverURL );
                username = EditorGUILayout.TextField( "Username", username );
                password = EditorGUILayout.TextField( "Password", password );
                passiveMode = EditorGUILayout.Toggle( "Passive Mode", passiveMode, GUI.skin.toggle );
            }

            GUILayout.EndVertical();




            //Setup our 'Create Asset Bundles' button
            style = new GUIStyle( GUI.skin.GetStyle( "button" ) );
            style.fontSize = 15;
            style.fixedHeight = 50f;
            style.wordWrap = true;
            style.margin = new RectOffset( 15, 15, 25, 0 );

            if( GUILayout.Button( "Create Asset Bundles", style ) )
            {
                BuildAllAssetBundles();
            }
            
            //Reset our GUI skin
            GUI.skin = EditorGUIUtility.GetBuiltinSkin(EditorSkin.Inspector);

        } //END OnGUI
#endregion

        #region CREATE ASSET BUNDLES BUTTON PRESSED
        //---------------------------------------------------//
        public void BuildAllAssetBundles()
        //---------------------------------------------------//
        {
            //If the Asset bundle folder does not exist, create it
            if( !AssetDatabase.IsValidFolder( "Assets/" + buildToFolder ) )
            {
                AssetDatabase.CreateFolder( "Assets", buildToFolder );
            }

            //Build all of our asset bundles into our AssetBundles folder
            if( buildPlatforms != null && buildPlatforms.Count > 0 )
            {
                //If our current platform is one of the ones we want to build asset bundles for, 
                //build for that platform first so we don't trigger a Unity Asset Re-import
                if( buildPlatforms.ContainsKey( EditorUserBuildSettings.activeBuildTarget ) &&
                    buildPlatforms[ EditorUserBuildSettings.activeBuildTarget ] )
                {
                    Debug.Log( "BXRBuildAssetBundles.cs BuildAllAssetBundles() Building asset bundles for " + EditorUserBuildSettings.activeBuildTarget );
                    BuildPipeline.BuildAssetBundles( Application.dataPath + "/" + buildToFolder, buildAssetBundleOptions, EditorUserBuildSettings.activeBuildTarget );
                }

                //Make all of our asset bundles now, besides the active platform (which we made a build for above)
                foreach( KeyValuePair<BuildTarget, bool> pair in buildPlatforms )
                {
                    if( pair.Value )
                    {
                        //We already made a build for the active platform in the editor above to avoid Asset Re-imports, so there's no reason to build those same assets again
                        if( pair.Key != EditorUserBuildSettings.activeBuildTarget )
                        {
                            if( BuildPipeline.IsBuildTargetSupported( BuildPipeline.GetBuildTargetGroup( pair.Key ), pair.Key ) )
                            {
                                Debug.Log( "BXRBuildAssetBundles.cs BuildAllAssetBundles() Building asset bundles for " + pair.Key.ToString() );
                                manifests.Add( pair.Key, BuildPipeline.BuildAssetBundles( Application.dataPath + "/" + buildToFolder, buildAssetBundleOptions, pair.Key ) );
                            }
                            else
                            {
                                Debug.LogError( "BXRBuildAssetBundles.cs BuildAllAssetBundles() ERROR: Building AssetBundles for " + pair.Key + " is unsupported, make sure platform is installed within Unity Editor" );
                            }
                        }
                    }
                }

                //Once we're done making asset bundles, let's generate a CSV file containing info about our Asset Bundles
                if( enableCSV )
                {
                    GetAssetBundleInfos();
                    DestroyExistingCSVFile();
                    CreateNewCSVFile();
                }

                //Once we're done making asset bundles and the CSV file, check if we need to copy the 'AssetBundles' folder to the Streaming Assets path
                if( enableCopy )
                {
                    DestroyExistingAssetBundlesFolderInStreamingAssets();
                    CopyAssetBundlesFolderToStreamingAssets();
                    OverwriteCSVDataInStreamingAssets();
                }

                //Once we're done making asset bundles, let's try to upload them to our FTP server
                if( enableFTPUploading )
                {
                    //Go through our build folder and find all of the AssetBundles

                    //Go through all of our build platforms
                    foreach( KeyValuePair<BuildTarget, bool> pair in buildPlatforms )
                    {
                        if( pair.Value )
                        {
                            //UploadFileWithFTP( Application.dataPath + "/" + buildToFolder + "/", );
                        }
                    }
                }

                //Once we've gotten to this place we should be done with out local commands, but we might still have web commands waiting
                if( enableFTPUploading )
                {
                    Debug.Log( "BXRBuildAssetBundles.cs BuildAllAssetBundles() All local AssetBundle build commands for " + EditorUserBuildSettings.activeBuildTarget + " complete. You still may have Web Upload operations ongoing." );
                }
                else
                {
                    Debug.Log( "BXRBuildAssetBundles.cs BuildAllAssetBundles() All local AssetBundle build commands for " + EditorUserBuildSettings.activeBuildTarget + " complete." );
                }
            }
            else
            {
                Debug.LogError( "BXRBuildAssetBundles.cs BuildAllAssetBundles() ERROR: Unable to build bundles, you need to select at least one platform to build for." );
            }

        } //END BuildAllAssetBundles
#endregion

        #region BUILD CSV FILE
        //---------------------------------------------------//
        private void GetAssetBundleInfos()
        //---------------------------------------------------//
        {
            //Reset our list of Asset Bundles Info's, which store data about the asset bundles which is used to generate the CSV file
            assetBundleInfos = new List<AssetBundleInfo>();

            //We will only generate a CSV if we have valid AssetBundles generated
            if( AssetDatabase.IsValidFolder( "Assets/" + buildToFolder ) )
            {
                //Get all of the asset bundles within this folder
                List<string> fileEntries = Directory.GetFiles( Application.dataPath + "/" + buildToFolder ).ToList();

                if( fileEntries != null && fileEntries.Count > 0 )
                {
                    //Debug.Log( "BXRBuildAssetBundle.cs GetAssetBundleInfos() found files!" );
                    
                    //We know the asset bundles folder isn't empty, let's find out all we can about this directory!
                    DirectoryInfo directoryInfo = new DirectoryInfo( Application.dataPath + "/" + buildToFolder );

                    List<FileInfo> fileInfos = directoryInfo.GetFiles().ToList();

                    if( fileInfos != null && fileInfos.Count > 0 )
                    {
                        foreach( FileInfo fileInfo in fileInfos )
                        {
                            if( fileInfo != null )
                            {
                                string extension = fileInfo.Extension;
                                //Debug.Log( "extension = " + extension + ", FullName = " + fileInfo.FullName + ", Name = " + fileInfo.Name );
                                
                                //Look for the main AssetBundle generated by unity that contains info about all of the "Real" AssetBundles that have been generated
                                //This "Information" asset bundle will be named the same as the folder by Unity
                                if( extension == "" && fileInfo.Name == buildToFolder )
                                {
                                    //Let's get the manifest bundle generated by Unity. 
                                    //It's located in the same folder where the bundles were built to and has the same name as the folder
                                    //It's empty besides containing manifest information about the generated AssetBundles
                                    AssetBundle manifestBundle = AssetBundle.LoadFromFile( fileInfo.FullName );

                                    if( manifestBundle != null )
                                    {
                                        //Pull the Manifest for the 'Information' Asset Bundle generated by Unity,
                                        //This manifest contains the names of all of the Asset Bundles in the same folder
                                        //The manifest is always called "assetbundlemanifest", so it's easy to find
                                        AssetBundleManifest mainManifest = manifestBundle.LoadAsset<AssetBundleManifest>( "assetbundlemanifest" );

                                        if( mainManifest != null )
                                        {
                                            List<string> assetNames = mainManifest.GetAllAssetBundles().ToList();

                                            //Finally we have a list of all the AssetBundles in this folder! Let's being adding them
                                            //to our AssetBundleInfo list
                                            if( assetNames != null && assetNames.Count > 0 )
                                            {
                                                foreach( string assetName in assetNames )
                                                {
                                                    if( assetName != "" )
                                                    {
                                                        //Create a new AssetBundleInfo to store all of the information about this assetBundle
                                                        AssetBundleInfo info = new AssetBundleInfo();
                                                        info.name = assetName;
                                                        info.path = Application.dataPath + "/" + buildToFolder + "/" + assetName;
                                                        info.versionType = "hash";
                                                        info.version = mainManifest.GetAssetBundleHash( info.name );

                                                        if( BuildPipeline.GetCRCForAssetBundle( info.path, out info.checksum ) )
                                                        {
                                                            //Debug.Log( "BXRBuildAssetBundle.cs GetAssetBundleInfos() located checksum for asset bundle( " + info.name + " )... checksum = " + info.checksum );
                                                        }
                                                        else
                                                        {
                                                            Debug.LogError( "BXRBuildAssetBundle.cs GetAssetBundleInfos() Unable to get checksum for asset bundle( " + info.name + " ) at Path = " + fileInfo.FullName );
                                                        }

                                                        //Debug.Log( "BXRBuildAssetBundle.cs GetAssetBundleInfos() Created AssetBundleInfo! name = " + info.name + ", version = " + info.version + ", checksum = " + info.checksum + ", path = " + info.path );

                                                        //And finally add the newly created AssetBundleInfo to the list
                                                        assetBundleInfos.Add( info );
                                                    }
                                                }
                                            }
                                        }
                                        else
                                        {
                                            Debug.LogError( "BXRBuildAssetBundle.cs GetAssetBundleInfos() Could not locate manifest at Path = " + fileInfo.FullName );
                                        }
                                    }
                                    else
                                    {
                                        Debug.LogError( "BXRBuildAssetBundle.cs GetAssetBundleInfos() ERROR: We located the 'Information' asset bundle generated by Unity but we were unable to retrieve it at FileInfo.FullName = " + fileInfo.FullName );
                                    }

                                    //Once we've located the 'Information' AssetBundle, we've found what we need
                                    //We can safely stop searching as everything we need for our AssetBundleInfo should be filled out
                                    break;
                                }
                            }
                            else
                            {
                                Debug.LogError( "BXRBuildAssetBundle.cs GetAssetBundleInfos() This shouldn't occur but if it does it should be harmless... There is a null FileInfo variable for Path = " + fileInfo.FullName );
                            }
                        }
                    }
                    else
                    {
                        Debug.LogError( "BXRBuildAssetBundle.cs GetAssetBundleInfos() Unable to locate any fileInfos within directory = " + Application.dataPath + "/" + "Assets/" + buildToFolder );
                    }
                    
                }
                else
                {
                    Debug.LogError( "BXRBuildAssetBundle.cs GetAssetBundleInfos() Unable to locate any files within " + Application.dataPath + "/" + "Assets/" + buildToFolder );
                }
            }
            else
            {
                Debug.LogError( "BXRBuildAssetBundle.cs GetAssetBundleInfos() Unable to get asset bundle information, our Asset Bundle Folder located at Assets/" + buildToFolder + " is missing!" );
            }

        } //END GetAssetBundleInfos


        //---------------------------------------------------//
        private void DestroyExistingCSVFile()
        //---------------------------------------------------//
        {

            //We will can only destroy the CSV file if it's in it's expected location
            if( AssetDatabase.IsValidFolder( "Assets/" + buildToFolder ) )
            {
                string path = Application.dataPath + "/" + buildToFolder + "/" + csvFileName + ".csv";

                //Check if the CSV asset exists in the folder
                if( File.Exists( path ) )
                {
                    File.Delete( path );
                    //Debug.Log( "BXRBuildAssetBundle.cs DestroyExistingCSVFile() Destroyed existing CSV file at Path = " + path );
                }
                else
                {
                    //Debug.Log( "BXRBuildAssetBundle.cs DestroyExistingCSVFile() unable to destroy CSV file, no file exists at Path = " + path );
                }
            }
            else
            {
                Debug.LogError( "BXRBuildAssetBundle.cs DestroyExistingCSVFile() unable to destroy CSV file, the Assets/" + buildToFolder + " folder does not exist" );
            }

        } //END DestroyExistingCSVFile

        //---------------------------------------------------//
        private void CreateNewCSVFile()
        //---------------------------------------------------//
        {

            //We will can only create the CSV file if the necessary folder exists
            if( AssetDatabase.IsValidFolder( "Assets/" + buildToFolder ) )
            {
                string path = Application.dataPath + "/" + buildToFolder + "/" + csvFileName + ".csv";

                //Make sure there is no pre-existing CSV Asset at the expected path
                if( !File.Exists( path ) )
                {
                    StreamWriter outStream = File.CreateText( path );

                    //Add the first line 'Headers' for this CSV
                    outStream.WriteLine( "Name, Path, VersionType, Version, Checksum" );

                    //For each assetBundleInfo we generated earlier, add that info to the CSV file
                    if( assetBundleInfos != null && assetBundleInfos.Count > 0 )
                    {
                        for( int i = 0; i < assetBundleInfos.Count; i++ )
                        {
                            AssetBundleInfo info = assetBundleInfos[ i ];

                            if( info != null )
                            {
                                //If this is the last asset bundle info, we write without adding a new line afterwards
                                if( i == assetBundleInfos.Count - 1 )
                                {
                                    outStream.Write( info.name + "," + info.path + "," + info.versionType + "," + info.version + "," + info.checksum );
                                }
                                else
                                {
                                    //This isn't the last asset bundle info, add a newline after this one is created
                                    outStream.WriteLine( info.name + "," + info.path + "," + info.versionType + "," + info.version + "," + info.checksum );
                                }
                            }
                        }
                    }

                    //Close out the file write stream
                    outStream.Close();

                    //Debug.Log( "BXRBuildAssetBundle.cs CreateNewCSVFile() generated CSVData at Path = " + path );
                }
                else
                {
                    Debug.LogError( "BXRBuildAssetBundle.cs CreateNewCSVFile() unable to create CSV file, an existing CSV file already exists at Path = " + path + ", you should call DestroyExistingCSVFile() before creating a new CSV file at the same location" );
                }
            }
            else
            {
                Debug.LogError( "BXRBuildAssetBundle.cs CreateNewCSVFile() unable to create CSV file, the Assets/" + buildToFolder + " folder does not exist" );
            }

        } //END CreateNewCSVFile

#endregion

        #region COPY TO STREAMING ASSETS
        //---------------------------------------------------//
        private void DestroyExistingAssetBundlesFolderInStreamingAssets()
        //---------------------------------------------------//
        {
            string path = Application.dataPath + "/StreamingAssets";

            //If the Streaming assets directory does not exist already in our Assets folder, create it now
            if( !Directory.Exists( path ) )
            {
                Debug.LogError( "BXRBuildAssetBundles.cs DestroyExistingAssetBundlesFolderInStreamingAssets() Created 'StreamingAssets' path, so no need to delete the 'StreamingAssets/" + buildToFolder + "' folder" );

                Directory.CreateDirectory( path );

                //If we had to create the Streaming Assets path from scratch, then we know there's no pre-existing AssetBundles there to delete
                return;
            }

            //Update our path to point to the 'StreamingAssets/AssetBundles' folder
            path = Application.dataPath + "/StreamingAssets/" + buildToFolder;

            //If the StreamingAssets/AssetBundles directory exists already in our Assets folder, destroy it now
            if( Directory.Exists( path ) )
            {
                DirectoryHelper.DeleteDirectory( path );
            }
            
        } //END DestroyExistingAssetBundlesFolderInStreamingAssets

        //---------------------------------------------------//
        private void CopyAssetBundlesFolderToStreamingAssets()
        //---------------------------------------------------//
        {
            string path = Application.dataPath + "/" + buildToFolder;

            //If the 'Assets/AssetBundles' folders does not exist, we cannot perform this operation
            if( !Directory.Exists( path ) )
            {
                Debug.LogError( "BXRBuildAssetBundles.cs CopyAssetBundlesFolderToStreamingAssets() Could not copy '" + buildToFolder + "' folder, this path does not exist = " + path );
                return;
            }

            path = Application.dataPath + "/StreamingAssets";

            //If the Streaming assets directory does not exist already in our Assets folder, then we cannot copy/create the subfolder 'AssetBundles'
            if( !Directory.Exists( path ) )
            {
                Debug.LogError( "BXRBuildAssetBundles.cs CopyAssetBundlesFolderToStreamingAssets() Could not copy '" + buildToFolder + "' folder, this path does not exist = " + path );
                return;
            }
            
            //Debug.Log( "source = " + Application.dataPath + "/" + buildToFolder + ", moveTo = " + Application.dataPath + "/StreamingAssets/" + buildToFolder );
            FileUtil.CopyFileOrDirectory( Application.dataPath + "/" + buildToFolder, Application.dataPath + "/StreamingAssets/" + buildToFolder );

        } //END CopyAssetBundlesFolderToStreamingAssets

        //--------------------------------------------------------//
        private void OverwriteCSVDataInStreamingAssets()
        //--------------------------------------------------------//
        {
            string path = Application.dataPath + "/StreamingAssets/" + buildToFolder + "/CSVData.csv";

            //Find the CSVData.csv file in StreamingAssets/AssetBundles folder
            if( File.Exists( path ) )
            {
                //Replace the CSVData.cs file in 'StreamingAssets'. Use relative paths to the folder/files in StreamingAssets instead of the absolute paths used previously in the 'Assets/AssetBundles' folder
                StreamWriter outStream = new StreamWriter( path, false );

                //Add the first line 'Headers' for this CSV
                outStream.WriteLine( "Name, Path, VersionType, Version, Checksum" );

                //For each assetBundleInfo we generated earlier, add that info to the CSV file
                if( assetBundleInfos != null && assetBundleInfos.Count > 0 )
                {
                    for( int i = 0; i < assetBundleInfos.Count; i++ )
                    {
                        AssetBundleInfo info = assetBundleInfos[ i ];

                        if( info != null )
                        {
                            //If this is the last asset bundle info, we write without adding a new line afterwards
                            if( i == assetBundleInfos.Count - 1 )
                            {
                                outStream.Write( info.name + "," + "StreamingAssets/" + buildToFolder + "/" + info.name + "," + info.versionType + "," + info.version + "," + info.checksum );
                            }
                            else
                            {
                                //This isn't the last asset bundle info, add a newline after this one is created
                                outStream.WriteLine( info.name + "," + "StreamingAssets/" + buildToFolder + "/" + info.name + "," + info.versionType + "," + info.version + "," + info.checksum );
                            }
                        }
                    }
                }

                //Close out the file write stream
                outStream.Close();
            }
            else
            {
                Debug.LogError( "BXRBuildAssetBundles.cs OverwriteCSVDataInStreamingAssets() Unable to overwrite CSVData in StreamingAssets/" + buildToFolder + " folder. It does not exist at this time" );
            }

        } //END OverwriteCSVDataInStreamingAssets

#endregion

        #region UPLOAD WITH FTP
        //---------------------------------------------------//
        private void UploadFileWithFTP(string filePath, string bundleName, BuildTarget buildTarget, Action successCallback, Action failedCallback)
        //---------------------------------------------------//
        {
            if( File.Exists(filePath) )
            {
                byte[] itemBytes = File.ReadAllBytes(filePath);

                bool success = true;
                string url = this.serverURL + "/" + buildTarget + "/" + bundleName;

                string status;

                if (!TryUpload(itemBytes, url, out status))
                {
                    success = CreateFoldersToItem(bundleName, out status) && TryUpload(itemBytes, url, out status);
                }

                EndUpload(bundleName, success, status, successCallback, failedCallback);
            }

        } //END UploadFileWithFTP

        //-----------------------------------------------------------------------//
        private bool TryUpload(byte[] itemBytes, string uri, out string status)
        //-----------------------------------------------------------------------//
        {
            try
            {
                FtpWebRequest request = CreateFtpWebRequest(uri, WebRequestMethods.Ftp.UploadFile);

                Stream stream = request.GetRequestStream();
                stream.Write(itemBytes, 0, itemBytes.Length);
                stream.Close();

                FtpWebResponse response = request.GetResponse() as FtpWebResponse;
                status = response.StatusDescription;
                return true;
            }
            catch (System.Net.WebException e)
            {
                FtpWebResponse response = e.Response as FtpWebResponse;

                if (response == null)
                {
                    status = e.Message;
                }
                else
                {
                    status = response.StatusDescription;
                }

                return false;
            }
            catch (System.Exception)
            {
                status = "Could not upload file to FTP server. Please check your FTP URL.";
                return false;
            }

        } //END TryUpload

        //-------------------------------------------------------------------------//
        private bool CreateFoldersToItem( string itemKey, out string status)
        //-------------------------------------------------------------------------//
        {
            FtpWebRequest request;

            System.Uri uri = new System.Uri( GetURL(serverURL, itemKey) );
            string[] segments = uri.Segments;

            string currentFolder = uri.GetLeftPart(System.UriPartial.Authority);

            for (int i = 0; i < segments.Length - 1; i++)
            {
                string segment = segments[i];

                try
                {
                    currentFolder = string.Concat(currentFolder, segment);

                    string requestUri = currentFolder;
                    if (currentFolder.Length > 0 && currentFolder[currentFolder.Length - 1] == '/')
                    {
                        requestUri = currentFolder.Substring(0, currentFolder.Length - 1);
                    }

                    request = CreateFtpWebRequest(requestUri, WebRequestMethods.Ftp.MakeDirectory);
                    WebResponse response = request.GetResponse();
                    response.GetResponseStream().Close();
                    response.Close();
                }
                catch (System.Net.WebException e)
                {
                    FtpWebResponse response = e.Response as FtpWebResponse;

                    if (response == null || response.StatusCode == FtpStatusCode.NotLoggedIn)
                    {
                        status = response.StatusDescription;
                        return false;
                    }
                    else if (response.StatusCode == FtpStatusCode.ActionNotTakenFileUnavailable)
                    {
                        // Folder already exists...
                        continue;
                    }
                }
                catch (System.Exception)
                {
                    status = "Could not create folder tree in FTP server. Please check your FTP URL.";
                    return false;
                }
            }

            status = "OK";
            return true;

        } //END CreateFoldersToItem

        //--------------------------------------------------------------------------//
        private FtpWebRequest CreateFtpWebRequest(string uri, string method)
        //--------------------------------------------------------------------------//
        {
            FtpWebRequest request = FtpWebRequest.Create(uri) as FtpWebRequest;
            request.Credentials = new NetworkCredential(username, password);
            request.Method = method;
            request.UsePassive = passiveMode;
            request.UseBinary = true;
            request.KeepAlive = false;

            return request;

        } //END CreateFtpWebRequest

        //--------------------------------------------------//
        public string GetURL( string url, string itemKey)
        //--------------------------------------------------//
        {
            if (url.EndsWith("/"))
            {
                return string.Format("{0}{1}", url, itemKey);
            }

            return string.Format("{0}/{1}", url, itemKey);

        }//END GetURL

        //------------------------------------------------------------------------------------------------------------//
        protected void EndUpload(string itemKey, bool success, string errorMessage, Action successCallback, Action failedCallback)
        //------------------------------------------------------------------------------------------------------------//
        {
            if (success)
            {
                Debug.Log(string.Format("\"{0}\" upload status: OK", itemKey));
                if (successCallback != null) { successCallback.Invoke(); }
            }
            else
            {
                Debug.LogError(string.Format("\"{0}\" upload status: {1}", itemKey, errorMessage));
                if (failedCallback != null) { failedCallback.Invoke(); }
            }
            
        } //END EndUpload

#endregion

        #region HELPER METHODS
        //---------------------------------------------------//
        private Texture LoadInstructionsTexture( string name )
        //---------------------------------------------------//
        {
            return (Texture)AssetDatabase.LoadAssetAtPath( "Assets/BrandXR/Textures/Instructions/ " + name + ".png", typeof( Texture ) ); ;

        } //END LoadInstructionsTexture

        #endregion
#endif

    } //END Class

} //END Namespace