using Sirenix.OdinInspector;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Networking;

namespace BrandXR
{
    public class BlockEventAssetBundle: BlockEventBase
    {

        #region ACTIONS
        //----------------- ACTIONS --------------------------------//
        public enum Actions
        {
            None,
            DownloadAssetBundle,
            LoadAssetBundleIntoMemory,
            UnloadAssetBundleFromMemory,
            InstantiateAssetBundleGameObject,
            DestroyInstantiatedAssetBundleGameObject,
            ClearCache
        }

        [TitleGroup( "Block Event - Asset Bundle", "Used to Download, Load, Unload, Instantiate, and Remove Unity Asset Bundles" )]
        public Actions action = Actions.None;
        private bool ShowDownloadAssetBundleAction() { return action == Actions.DownloadAssetBundle; }
        private bool ShowLoadAssetBundleIntoMemoryAction() { return action == Actions.LoadAssetBundleIntoMemory; }
        private bool ShowUnloadAssetBundleFromMemoryAction() { return action == Actions.UnloadAssetBundleFromMemory; }
        private bool ShowInstantiateAssetBundleAction() { return action == Actions.InstantiateAssetBundleGameObject; }
        private bool ShowRemoveAssetBundleAction() { return action == Actions.DestroyInstantiatedAssetBundleGameObject; }
        private bool ShowClearCacheAction() { return action == Actions.ClearCache; }
        #endregion

        #region VARIABLES - DOWNLOAD ASSET BUNDLE
        //------------- "DOWNLOAD ASSET BUNDLE" VARIABLES ---------------------------------//
        public enum DownloadFrom
        {
            Path,
            CSVData
        }
        [Space( 15f ), ShowIf( "ShowDownloadAssetBundleAction" )]
        public DownloadFrom downloadFrom = DownloadFrom.Path;
        private bool DownloadFromPath() { return ShowDownloadAssetBundleAction() && downloadFrom == DownloadFrom.Path; }
        private bool DownloadFromCSVData() { return ShowDownloadAssetBundleAction() && downloadFrom == DownloadFrom.CSVData; }

        public enum DownloadFromCSVType
        {
            AllAssetBundles,
            SingleAssetBundle
        }
        [Space( 15f ), ShowIf( "DownloadFromCSVData" )]
        public DownloadFromCSVType downloadFromCSV = DownloadFromCSVType.AllAssetBundles;
        private bool DownloadOnlySingleFromCSV() { return DownloadFromCSVData() && downloadFromCSV == DownloadFromCSVType.SingleAssetBundle; }
        private bool DownloadAllFromCSV() { return DownloadFromCSVData() && downloadFromCSV == DownloadFromCSVType.AllAssetBundles; }

        [ShowIf( "DownloadOnlySingleFromCSV" )]
        public string downloadBundleWithName = "";


        

        [Space( 15f ), ShowIf( "DownloadFromPath" ), InfoBox("The path to the AssetBundle on the web")]
        public string bundlePath = "";

        
        [Space( 15f ), ShowIf( "DownloadFromPath" ), InfoBox( "You can choose to use version control through either an integer or a hash code.\n\nIf you choose to not use a version control type, we will always try to download the latest version from the path." )]
        public WWWHelper.AssetBundleVersionType versionType = WWWHelper.AssetBundleVersionType.none;
        private bool IsVersionTypeNone() { return DownloadFromPath() && versionType == WWWHelper.AssetBundleVersionType.none; }
        private bool IsVersionTypeInteger() { return DownloadFromPath() && versionType == WWWHelper.AssetBundleVersionType.integer; }
        private bool IsVersionTypeHash() { return DownloadFromPath() && versionType == WWWHelper.AssetBundleVersionType.hash; }

        [Space( 15f ), ShowIf( "IsVersionTypeInteger" ), InfoBox( "The version of the AssetBundle to download.\n\nIf this version number is different than the cached version of the Asset Bundle we have in Local Storage, then we will attempt to download the asset bundle from the path." )]
        public uint versionNumber = 1;

        [SerializeField, Space( 15f ), ShowIf( "IsVersionTypeHash" ), InfoBox("The hash code you would like to use to verify the version of the AssetBundle.\n\nIf the hash code is different than the cached version of the Asset Bundle we have in Local Storage, then we will use the newer version\n\nWhen a new version of an asset bundle is created, it also recieves new hash values")]
        public string hash = "";
        
        private bool ShowChecksum() { return IsVersionTypeInteger() || IsVersionTypeHash(); }

        [Space( 15f ), ShowIf( "ShowChecksum" ), InfoBox( "The CRC-32 Checksum to compare the contents of the AssetBundle against. If the checksum test fails when grabbing from the web we will attempt to use what exists in the cache.\n\nIf the checksum fails against the downloaded version we will return a OnAssetBundleFailed() event\n\nBe aware that whenever you build a new version of an asset bundle, the checksum and hash values will change\n\nLeave this value at 0 to not use the checksum system when downloading asset bundles" )]
        public uint checksum = new uint();

        


        [Space( 15f ), ShowIf( "DownloadFromCSVData" ), InfoBox( "The CSVData object containing the following columns.\n\n" +
            "Name - What your Asset Bundle was named when generating it in the Unity Editor\n\n" +
            "Path - URL to the Asset Bundle on a web server\n\n" +
            "VersionType - 'int' or 'hash', lets us know if the version number should be parsed as an integer value or a hash value\n\n" +
            "Version - Use a single integer or a Hash128\n\n" +
            "Checksum - [Optional] the CRC-32 checksum to compare to ensure Asset Bundle is validated and intact\n\n" +
            "Your CSV File must have the top row with these column names\n\n" +
            "You can download a CSV data file using the Block Event - Web with it's action set to 'Download CSVFile' and use it's OnDownloadCSVCompleted() event to pass it to this scripts SetCSV() function\n\n" +
            "After you can use the same event to send this script the CallEvent() message and begin downloading the AssetBundles" )]
        public CSVData csvData = null;

        
        [Space( 15f ), ShowIf( "ShowDownloadAssetBundleAction" ), InfoBox("Warning: Do Not Modify\n\nThis list of Packages is created by parsing the CSVData.\n\nUse this as a reference to debug your CSVData, but do not manually modify the contents of this list from the Unity Editor.", InfoMessageType.Warning )]
        public List<WWWHelper.AssetBundleInfo> assetBundleInfos = new List<WWWHelper.AssetBundleInfo>();


        private int counterFailedDownloads = 0;
        private int counterSuccessfullDownloads = 0;
        private int totalNumberOfDownloadsToWaitFor = 0;
        private bool downloadsComplete = false;
        #endregion

        #region VARIABLES - LOAD ASSET BUNDLE
        //------------- "LOAD ASSET BUNDLE" VARIABLES ----------------------------------------//
        private bool ShowBlockEventAssetBundleVariable() { return ShowLoadAssetBundleIntoMemoryAction() || ShowUnloadAssetBundleFromMemoryAction() || ShowInstantiateAssetBundleAction() || ShowRemoveAssetBundleAction(); }

        [Space( 15f ), ShowIf( "ShowBlockEventAssetBundleVariable" ), InfoBox( "Impact asset bundles from this linked BlockEvent - Asset Bundle component.\n\nYou should link this to the Block Event - Asset Bundle component that originally downloaded the asset bundles you want to impact.\n\nYou can only impact Asset Bundles once they have been downloaded in this Unity session" )]
        public BlockEventAssetBundle blockEventAssetBundle = null;

        public enum ChooseBundlesUsing
        {
            AllDownloadedBundles,
            Names
        }
        [Space( 15f ), ShowIf( "ShowLoadAssetBundleIntoMemoryAction" ), InfoBox( "Choose what downloaded asset bundles you would like to load into active memory.\n\nOnce loaded an asset bundle in memory can then be used like a normal Unity resource and instantiated via the Block Event - Asset Bundle\n\nThis only works for asset bundles that have been already download in this session" )]
        public ChooseBundlesUsing loadBundlesUsing = ChooseBundlesUsing.AllDownloadedBundles;
        private bool LoadAllBundles() { return ShowLoadAssetBundleIntoMemoryAction() && loadBundlesUsing == ChooseBundlesUsing.AllDownloadedBundles; }
        private bool LoadBundlesWithSpecificNames() { return ShowLoadAssetBundleIntoMemoryAction() && loadBundlesUsing == ChooseBundlesUsing.Names; }

        [Space( 15f ), ShowIf( "LoadBundlesWithSpecificNames" ), InfoBox("Enter the names of the Asset bundles that were downloaded you would like to now load into active memory to prepare for use or instantiation")]
        public List<string> namesOfBundlesToLoad = new List<string>();

        public enum LoadUsing
        {
            AllAssetsInBundle,
            NamesOfAssetsInBundle,
            //NamesAndTypesOfAssetsInBundle
        }
        [Space( 15f ), ShowIf( "ShowLoadAssetBundleIntoMemoryAction" ), InfoBox("Choose what assets inside of the asset bundles you would like to load.\n\nYou can choose to load only assets with specific names or those from a specific type, or a combination of those requirments.")]
        public LoadUsing loadAssetsInBundlesUsing = LoadUsing.AllAssetsInBundle;
        private bool LoadAllAssets() { return ShowLoadAssetBundleIntoMemoryAction() && loadAssetsInBundlesUsing == LoadUsing.AllAssetsInBundle; }
        private bool LoadAssetsByNames() { return ShowLoadAssetBundleIntoMemoryAction() && loadAssetsInBundlesUsing == LoadUsing.NamesOfAssetsInBundle; }
        private bool LoadAssetsByNamesAndTypes() { return ShowLoadAssetBundleIntoMemoryAction() && false; }// loadAssetsInBundlesUsing == LoadUsing.NamesAndTypesOfAssetsInBundle; }

        private bool ShowNamesOfAssetsToLoad() { return LoadAssetsByNames() || LoadAssetsByNamesAndTypes(); }
        private bool ShowTypesOfAssetsToLoad() { return LoadAssetsByNamesAndTypes(); }

        [Space( 15f ), ShowIf( "ShowNamesOfAssetsToLoad" ), InfoBox("Enter the names of the assets within the asset bundles you wish to load into active memory to prepare for use or instantiation")]
        public List<string> namesOfAssetsToLoad = new List<string>();

        [Space( 15f ), ShowIf( "ShowTypesOfAssetsToLoad" ), InfoBox( "Enter the name of the asset type within the asset bundles you wish to load into active memory to prepare for use or instantiation" )]
        public string typeOfAssetsToLoad = "";
        

        private int counterFailedLoads = 0;
        private int counterSuccessfullLoads = 0;
        private int totalNumberOfLoadsToWaitFor = 0;
        #endregion

        #region VARIABLES - UNLOAD ASSET BUNDLE
        //------------- "UNLOAD ASSET BUNDLE" VARIABLES --------------------------------------//
        [Space( 15f ), ShowIf( "ShowUnloadAssetBundleFromMemoryAction" ), InfoBox( "Choose what loaded asset bundles you would like to unload from active memory.\n\nOnce unloaded you must load the asset bundle back into memory to use the asset bundle as a resource or to instantiate it." )]
        public ChooseBundlesUsing unloadBundlesUsing = ChooseBundlesUsing.AllDownloadedBundles;
        private bool UnloadAllBundles() { return ShowUnloadAssetBundleFromMemoryAction() && unloadBundlesUsing == ChooseBundlesUsing.AllDownloadedBundles; }
        private bool UnloadBundlesWithSpecificNames() { return ShowUnloadAssetBundleFromMemoryAction() && unloadBundlesUsing == ChooseBundlesUsing.Names; }

        [Space( 15f ), ShowIf( "UnloadBundlesWithSpecificNames" ), InfoBox("Enter the names of the asset bundles you wish to unload from active memory.")]
        public List<string> assetBundlesToUnload = new List<string>();

        private int counterFailedUnloads = 0;
        private int counterSuccessfullUnloads = 0;
        private int totalNumberOfUnloadsToWaitFor = 0;
        #endregion

        #region VARIABLES - INSTANTIATE ASSET BUNDLE
        //------------- "INSTANTIATE ASSET BUNDLE" VARIABLES ---------------------------------//
        [Space( 15f ), ShowIf( "ShowInstantiateAssetBundleAction" ), InfoBox("Instantiate GameObjects within these AssetBundles contained within the linked blockEventAssetBundle above")]
        public ChooseBundlesUsing instantiateUsing = ChooseBundlesUsing.AllDownloadedBundles;
        private bool InstantiateUsingNames() { return ShowInstantiateAssetBundleAction() && instantiateUsing == ChooseBundlesUsing.Names; }
        private bool InstantiateAllAssetBundles() { return ShowInstantiateAssetBundleAction() && instantiateUsing == ChooseBundlesUsing.AllDownloadedBundles; }
        
        [Space( 15f ), ShowIf( "InstantiateUsingNames" ), InfoBox("Enter the names of the Asset Bundles with GameObject assets you would like to instantiate")]
        public List<string> namesOfBundlesToInstantiate = new List<string>();

        public enum ChooseAssetsUsing
        {
            AllGameObjects,
            GameObjectNames
        }
        [Space( 15f ), ShowIf( "ShowInstantiateAssetBundleAction" ), InfoBox( "Enter the names of the GameObjects within the Asset Bundles you would like to instantiate" )]
        public ChooseAssetsUsing instantiateAssets = ChooseAssetsUsing.AllGameObjects;
        private bool ShouldInstantiateAllGameObjectAssets() { return ShowInstantiateAssetBundleAction() && instantiateAssets == ChooseAssetsUsing.AllGameObjects; }
        private bool ShouldInstantiateGameObjectAssetsBasedOnNames() { return ShowInstantiateAssetBundleAction() && instantiateAssets == ChooseAssetsUsing.GameObjectNames; }

        [Space( 15f ), ShowIf( "ShouldInstantiateGameObjectAssetsBasedOnNames" ), InfoBox( "Enter the names of the GameObject assets you would like to instantiate" )]
        public List<string> namesOfGameObjectAssetsToInstantiate = new List<string>();

        public enum InstantiationType
        {
            InstantiateWithNoParent,
            ParentToTransform,
            ChangeBlockModel
        }

        [ Space( 15f ), ShowIf( "ShowInstantiateAssetBundleAction" )]
        public InstantiationType instantiationType = InstantiationType.InstantiateWithNoParent;
        private bool InstantiateWithNoParent() { return ShowInstantiateAssetBundleAction() && instantiationType == InstantiationType.InstantiateWithNoParent; }
        private bool InstantiateWithParent() { return ShowInstantiateAssetBundleAction() && instantiationType == InstantiationType.ParentToTransform; }
        private bool InstantiateWithBlockModel() { return ShowInstantiateAssetBundleAction() && instantiationType == InstantiationType.ChangeBlockModel; }

        [Space( 15f ), ShowIf( "InstantiateWithParent" )]
        public Transform parentToThisTransform = null;

        [Space( 15f ), ShowIf( "InstantiateWithBlockModel" )]
        public BlockModel blockModelToChange = null;

        private int counterInstantiatedSuccessfully = 0;
        private int counterInstantiatedFailed = 0;
        private int totalNumberOfInstantiations = 0;
        #endregion

        #region VARIABLES - DESTROY INSTANTIATED ASSET BUNDLE GAMEOBJECTS
        //-------------------- "REMOVE ASSET BUNDLE VARIABLES" -------------------------------------//
        public enum RemoveUsing
        {
            AssetBundleNames,
            InstantiatedGameObjectNames,
            LinksToInstantiatedGameObjects
        }
        [Space( 15f ), ShowIf( "ShowRemoveAssetBundleAction" ), InfoBox( "Call this action to destroy Instantiated Unity Asset Bundle GameObjects from the scene.\n\nNote that this does not clear the asset bundle cache as that is a seperate action" )]
        public RemoveUsing destroyUsing = RemoveUsing.AssetBundleNames;
        private bool RemoveUsingAssetBundleNames() { return ShowRemoveAssetBundleAction() && destroyUsing == RemoveUsing.AssetBundleNames; }
        private bool RemoveUsingInstantiatedGameObjectNames() { return ShowRemoveAssetBundleAction() && destroyUsing == RemoveUsing.InstantiatedGameObjectNames; }
        private bool RemoveUsingLinksToInstantiatedGameObjects() { return ShowRemoveAssetBundleAction() && destroyUsing == RemoveUsing.LinksToInstantiatedGameObjects; }

        [Space( 15f ), ShowIf( "RemoveUsingAssetBundleNames" ), InfoBox("Enter the names of the instantiated Asset Bundle's GameObjects you would like destroyed in the scene.")]
        public List<string> namesOfBundlesToRemove = new List<string>();

        [Space( 15f ), ShowIf( "RemoveUsingInstantiatedGameObjectNames" ), InfoBox("Enter the names of the instantiated Game Objects that came from Asset Bundles you would like destroyed in the scene.")]
        public List<string> namesOfGameObjectsToRemove = new List<string>();

        [Space( 15f ), ShowIf( "RemoveUsingLinksToInstantiatedGameObjects" ), InfoBox("Link to the GameObjects that were instantiated from the Asset Bundles you would like destroyed in the scene.")]
        public List<GameObject> gameObjectsToRemove = new List<GameObject>();
        #endregion

        #region VARIABLES - CLEAR CACHE
        //-------------------- "CLEAR CACHE VARIABLES" -------------------------------------//
        
        public enum ClearCacheType
        {
            All,
            SpecificAssetBundleNames
        }
        [Space( 15f ), ShowIf( "ShowClearCacheAction" ), InfoBox( "Call this action to clear the Unity Caching system of cached Asset Bundles stored in the Persistent Data local storage folder" )]
        public ClearCacheType clearCacheType = ClearCacheType.All;
        private bool ShouldClearAllCaches() { return ShowClearCacheAction() && clearCacheType == ClearCacheType.All; }
        private bool ShouldClearSpecificCaches() { return ShowClearCacheAction() && clearCacheType == ClearCacheType.SpecificAssetBundleNames; }

        [Space( 15f ), ShowIf( "ShouldClearSpecificCaches" )]
        public List<string> assetBundleNamesToClearCachesOf = new List<string>();

        #endregion

        #region EVENT MESSAGES - DOWNLOAD ASSET BUNDLE
        //-------------------- "DOWNLOAD ASSET BUNDLE" EVENT MESSAGES ---------------------//
        private bool ShowDownloadSingleAssetBundleEventMessages() { return action == Actions.DownloadAssetBundle && ( downloadFrom == DownloadFrom.Path || ( downloadFrom == DownloadFrom.CSVData && downloadFromCSV == DownloadFromCSVType.SingleAssetBundle ) ); }
        private bool ShowDownloadAllAssetBundleEventMessages() { return action == Actions.DownloadAssetBundle && downloadFrom == DownloadFrom.CSVData && downloadFromCSV == DownloadFromCSVType.AllAssetBundles; }
        
        [ShowIf( "ShowDownloadAllAssetBundleEventMessages" ), FoldoutGroup( "Event Messages" )]
        public UnityEvent onAllAssetBundlesDownloadedSuccessfully = new UnityEvent();

        [ShowIf( "ShowDownloadAllAssetBundleEventMessages" ), FoldoutGroup( "Event Messages" )]
        public UnityEvent onAssetBundlesDownloadedWithPartialSuccess = new UnityEvent();

        [ShowIf( "ShowDownloadAllAssetBundleEventMessages" ), FoldoutGroup( "Event Messages" )]
        public UnityEvent onAllAssetBundleDownloadsFailed = new UnityEvent();


        [Serializable]
        public class OnProgress: UnityEvent<string> { }

        [SerializeField, ShowIf( "ShowDownloadAssetBundleAction" ), FoldoutGroup( "Event Messages" ), 
            InfoBox( "Sends the amount of progress for the downloads from 0 - 100 as a float.\n\n" +
            "This is sent as a dynamic value, so the methods you link to here will recieve only " +
            "the progress value and will ignore any other text you write in the message field in the editor" )]
        public OnProgress onAssetBundleDownloadProgressUpdate = new OnProgress();

        
        [Serializable]
        public class SingleAssetBundleDownloadComplete: UnityEvent<WWWHelper.AssetBundleInfo> { }

        [ShowIf( "ShowDownloadAssetBundleAction" ), FoldoutGroup( "Event Messages" )]
        public SingleAssetBundleDownloadComplete onSingleAssetBundleDownloadSuccess = new SingleAssetBundleDownloadComplete();

        [ShowIf( "ShowDownloadAssetBundleAction" ), FoldoutGroup( "Event Messages" )]
        public SingleAssetBundleDownloadComplete onSingleAssetBundleDownloadFailed = new SingleAssetBundleDownloadComplete();
        #endregion

        #region EVENT MESSAGES - LOAD ASSET BUNDLE
        //---------------------- "LOAD ASSET BUNDLE" EVENT MESSAGES ------------------------------//
        [ShowIf( "ShowLoadAssetBundleIntoMemoryAction" ), FoldoutGroup( "Event Messages" )]
        public UnityEvent onAllAssetBundlesLoadedSuccessfully = new UnityEvent();

        [ShowIf( "ShowLoadAssetBundleIntoMemoryAction" ), FoldoutGroup( "Event Messages" )]
        public UnityEvent onAllAssetBundlesLoadedWithPartialSuccess = new UnityEvent();

        [ShowIf( "ShowLoadAssetBundleIntoMemoryAction" ), FoldoutGroup( "Event Messages" )]
        public UnityEvent onAllAssetBundlesLoadingFailed = new UnityEvent();


        [Serializable]
        public class SingleAssetBundleLoadComplete: UnityEvent<WWWHelper.AssetBundleInfo> { }

        [ShowIf( "ShowLoadAssetBundleIntoMemoryAction" ), FoldoutGroup("Event Messages")]
        public SingleAssetBundleLoadComplete onSingleAssetBundleLoaded = new SingleAssetBundleLoadComplete();

        [ShowIf( "ShowLoadAssetBundleIntoMemoryAction" ), FoldoutGroup("Event Messages")]
        public SingleAssetBundleLoadComplete onLoadAssetBundleFailed = new SingleAssetBundleLoadComplete();
        #endregion

        #region EVENT MESSAGES - UNLOAD ASSET BUNDLE
        //---------------------- "UNLOAD ASSET BUNDLE" EVENT MESSAGES ------------------------------//
        [ShowIf( "ShowUnloadAssetBundleFromMemoryAction" ), FoldoutGroup( "Event Messages" )]
        public UnityEvent onAllAssetBundlesUnloadedSuccessfully = new UnityEvent();

        [ShowIf( "ShowUnloadAssetBundleFromMemoryAction" ), FoldoutGroup( "Event Messages" )]
        public UnityEvent onAllAssetBundlesUnloadedWithPartialSuccess = new UnityEvent();

        [ShowIf( "ShowUnloadAssetBundleFromMemoryAction" ), FoldoutGroup( "Event Messages" )]
        public UnityEvent onAllAssetBundlesUnloadingFailed = new UnityEvent();


        [Serializable]
        public class SingleAssetBundleUnloadComplete: UnityEvent<WWWHelper.AssetBundleInfo> { }

        [ShowIf( "ShowUnloadAssetBundleFromMemoryAction" ), FoldoutGroup( "Event Messages" )]
        public SingleAssetBundleUnloadComplete onSingleAssetBundleUnloaded = new SingleAssetBundleUnloadComplete();

        [ShowIf( "ShowUnloadAssetBundleFromMemoryAction" ), FoldoutGroup( "Event Messages" )]
        public SingleAssetBundleUnloadComplete onUnloadAssetBundleFailed = new SingleAssetBundleUnloadComplete();
        #endregion

        #region EVENT MESSAGES - INSTANTIATE ASSET BUNDLE GAMEOBJECT
        //--------------------- "INSTANTIATE" EVENT COMPLETE MESSAGE -----------------------------//
        [ShowIf( "ShowInstantiateAssetBundleAction" ), FoldoutGroup("Event Messages")]
        public UnityEvent onAllAssetBundleGameObjectsInstantiatedSuccessfully = new UnityEvent();

        [ShowIf( "ShowInstantiateAssetBundleAction" ), FoldoutGroup( "Event Messages")]
        public UnityEvent onAllAssetBundleGameObjectsInstantiatedWithPartialSuccess = new UnityEvent();

        [ShowIf( "ShowInstantiateAssetBundleAction" ), FoldoutGroup("Event Messages")]
        public UnityEvent onAllAssetBundleGameObjectsInstantiationFailed = new UnityEvent();


        [Serializable]
        public class SingleAssetBundleGameObjectInstantiatedComplete: UnityEvent<GameObject> { };

        [ShowIf( "ShowInstantiateAssetBundleAction" ), FoldoutGroup( "Event Messages" ), InfoBox("Passes back the Instantiated GameObject")]
        public SingleAssetBundleGameObjectInstantiatedComplete onSingleAssetBundleGameObjectInstantiatedSuccessfully = new SingleAssetBundleGameObjectInstantiatedComplete();

        [Serializable]
        public class SingleAssetBundleGameObjectInstantiatedError: UnityEvent<string> { };

        [ShowIf( "ShowInstantiateAssetBundleAction" ), FoldoutGroup( "Event Messages" ), InfoBox("Passes back the name of the GameObject we failed to Instantiate")]
        public SingleAssetBundleGameObjectInstantiatedError onSingleAssetBundleGameObjectInstantiationFailed = new SingleAssetBundleGameObjectInstantiatedError();
        #endregion

        #region EVENT MESSAGES - OTHER
        //--------------------- "OTHER" EVENT COMPLETE MESSAGE -----------------------------------//
        private bool ShowOtherEventCompleteMessage() { return ShowRemoveAssetBundleAction() || ShowClearCacheAction(); }

        [ ShowIf( "ShowOtherEventCompleteMessage" ), FoldoutGroup( "Event Messages" )]
        public UnityEvent onComplete = new UnityEvent();
        #endregion

        #region METHODS - SETUP
        //---------------------------------------------------------//
        public override EventType GetEventType()
        //---------------------------------------------------------//
        {
            return EventType.AssetBundle;

        } //END GetEventType

        //---------------------------------------------------------//
        public void SetAction( Actions action )
        //---------------------------------------------------------//
        {

            this.action = action;

        } //END SetAction

        //-------------------------------//
        public override void PrepareEvent()
        //-------------------------------//
        {

            if( action == Actions.DownloadAssetBundle )
            {
                eventReady = true;
            }
            else if( action == Actions.LoadAssetBundleIntoMemory )
            {
                eventReady = true;
            }
            else if( action == Actions.UnloadAssetBundleFromMemory )
            {
                eventReady = true;
            }
            else if( action == Actions.InstantiateAssetBundleGameObject )
            {
                eventReady = true;
            }
            else if( action == Actions.DestroyInstantiatedAssetBundleGameObject )
            {
                eventReady = true;
            }
            else if( action == Actions.ClearCache )
            {
                eventReady = true;
            }

        } //END PrepareEvent

        //-------------------------------//
        public void SetCSV( CSVData data )
        //-------------------------------//
        {

            if( data != null )
            {
                csvData = data;
            }

        } //END SetCSV

        //-------------------------------//
        protected override void _CallEvent()
        //-------------------------------//
        {
            base._CallEvent();

            if( eventReady )
            {
                if( action == Actions.DownloadAssetBundle )
                {
                    BeginCallDownloadAssetBundleEvent();
                }
                else if( action == Actions.LoadAssetBundleIntoMemory )
                {
                    BeginLoadAssetBundleEvent();
                }
                else if( action == Actions.UnloadAssetBundleFromMemory )
                {
                    BeginUnloadAssetBundleEvent();
                }
                else if( action == Actions.InstantiateAssetBundleGameObject )
                {
                    BeginInstantiateAssetBundleEvent();
                }
                else if( action == Actions.DestroyInstantiatedAssetBundleGameObject )
                {
                    BeginRemovingAssetBundleEvent();
                }
                else if( action == Actions.ClearCache )
                {
                    ClearCacheEvent();
                }
            }

        } //END CallEvent
        #endregion

        #region METHODS - DOWNLOAD ASSET BUNDLE EVENT
        //---------------------------//
        private void BeginCallDownloadAssetBundleEvent()
        //---------------------------//
        {

            if( DownloadFromPath() )
            {
                CallDownloadAssetBundleFromPathEvent();
            }
            else if( DownloadFromCSVData() )
            {
                CallDownloadAssetBundleFromCSVEvent();
            }
            
        } //END CallDownloadAssetBundleEvent

        //---------------------------//
        private void CallDownloadAssetBundleFromPathEvent()
        //---------------------------//
        {
            //Reset our downloads counters
            counterFailedDownloads = 0;
            counterSuccessfullDownloads = 0;
            downloadsComplete = false;

            //Create a new PackageInfo object to store information about the Asset Bundle and make that data viewable in the editor
            //Instead of using CSVData, do this with the specific information set up in the editor
            WWWHelper.AssetBundleInfo package = new WWWHelper.AssetBundleInfo();

            package.name = Path.GetFileNameWithoutExtension( bundlePath );
            package.path = bundlePath;

            if( IsVersionTypeInteger() )
            {
                package.versionType = WWWHelper.AssetBundleVersionType.integer;
                package.versionNumber = versionNumber;
            }
            else if( IsVersionTypeHash() )
            {
                package.versionType = WWWHelper.AssetBundleVersionType.hash;
                package.versionHash = hash;
            }
            else
            {
                package.versionType = WWWHelper.AssetBundleVersionType.none;
            }

            package.checksum = checksum;

            //Now that our PackageInfo is set up, add it to our list of packages to be downloaded
            if( IsPackageValid( package ) )
            {
                assetBundleInfos.Add( package );
            }

            //Now that we have all of the data from the CSV file, let's start downloading the asset bundle
            if( assetBundleInfos != null && assetBundleInfos.Count > 0 )
            {
                foreach( WWWHelper.AssetBundleInfo info in assetBundleInfos )
                {
                    WWWHelper.instance.DownloadAssetBundle( info, OnSingleAssetBundleDownloadSuccess, OnSingleAssetBundleDownloadFailed, OnAssetBundleDownloadProgressUpdate );
                }
            }
            else
            {
                Debug.LogError( "BlockEventAssetBundle.cs CallDownloadAssetBundleFromPathEvent() ERROR: Unable to download single asset bundle '" + package.name + "', IsPackageValid = " + IsPackageValid( package ) );

                //If there are no assetBundleInfos to download, there's an error!
                if( onSingleAssetBundleDownloadFailed != null ) { onSingleAssetBundleDownloadFailed.Invoke( package ); }
            }

        } //END CallDownloadAssetBundleFromPathEvent

        //---------------------------//
        private void CallDownloadAssetBundleFromCSVEvent()
        //---------------------------//
        {
            
            if( csvData != null )
            {
                //Reset our downloads counters
                counterFailedDownloads = 0;
                counterSuccessfullDownloads = 0;
                downloadsComplete = false;

                //Should we download all of the Asset Packages?
                if( DownloadAllFromCSV() )
                {
                    //Go through all our CSV data and parse out the Asset Bundle information
                    ParseCSVIntoPackageInfo( csvData );

                    //Now that we have all of the data from the CSV file, let's start downloading the asset bundles
                    if( assetBundleInfos != null && assetBundleInfos.Count > 0 )
                    {
                        foreach( WWWHelper.AssetBundleInfo info in assetBundleInfos )
                        {
                            WWWHelper.instance.DownloadAssetBundle( info, OnMultipleAssetBundleDownloadComplete, OnMultipleAssetBundleDownloadFailed, OnAssetBundleDownloadProgressUpdate );
                        }
                    }
                }
                else if( DownloadOnlySingleFromCSV() ) //Otherwise we're only interested in one package
                {
                    //Go through our CSV Data and parse out the Asset Bundle information for only a single package
                    ParseCSVIntoPackageInfo( csvData, downloadBundleWithName );

                    //Now that we have all of the data from the CSV file, let's start downloading the asset bundle
                    if( assetBundleInfos != null && assetBundleInfos.Count > 0 )
                    {
                        foreach( WWWHelper.AssetBundleInfo info in assetBundleInfos )
                        {
                            WWWHelper.instance.DownloadAssetBundle( info, OnSingleAssetBundleDownloadSuccess, OnSingleAssetBundleDownloadFailed, OnAssetBundleDownloadProgressUpdate );
                        }
                    }
                }
            }
            else
            {
                Debug.LogError( "BlockEventAssetBundle.cs CallDownloadAssetBundleFromCSVEvent() ERROR: csvData is null" );
            }
            
        } //END CallDownloadAssetBundleFromCSVEvent

        //-------------------------------------------//
        private void ParseCSVIntoPackageInfo( CSVData csvData )
        //-------------------------------------------//
        {
            //Reset the list of packages to be filled out by going through the CSVData
            assetBundleInfos = new List<WWWHelper.AssetBundleInfo>();

            //Grab the columns from the CSVData
            DataColumn nameColumn = csvData.GetColumn( this.csvData.headers[ 0 ] );
            DataColumn pathColumn = csvData.GetColumn( this.csvData.headers[ 1 ] );
            DataColumn versionTypeColumn = csvData.GetColumn( this.csvData.headers[ 2 ] );
            DataColumn versionColumn = csvData.GetColumn( this.csvData.headers[ 3 ] );
            DataColumn checksumColumn = csvData.GetColumn( this.csvData.headers[ 4 ] );

            if( nameColumn != null && nameColumn.data.Length > 0 )
            {
                for( int i = 0; i < nameColumn.data.Length; i++ )
                {
                    //Create a new PackageInfo object to store information about the Asset Bundle and make that data viewable in the editor
                    WWWHelper.AssetBundleInfo package = CreateAssetBundleInfoFromCSVDataRow
                        ( i, nameColumn, pathColumn, versionTypeColumn, versionColumn, checksumColumn );
                    
                    //Now that our PackageInfo is set up, add it to our list of packages to be downloaded
                    if( IsPackageValid( package ) )
                    {
                        assetBundleInfos.Add( package );
                    }
                }
            }
            
        } //END ParseCSVIntoPackageInfo

        //----------------------------------//
        private void ParseCSVIntoPackageInfo( CSVData csvData, string parseOnlyThisPackage )
        //----------------------------------//
        {

            //Reset the list of packages to be filled out by going through the CSVData
            assetBundleInfos = new List<WWWHelper.AssetBundleInfo>();

            //Grab the columns from the CSVData
            DataColumn nameColumn = csvData.GetColumn( this.csvData.headers[ 0 ] );
            DataColumn pathColumn = csvData.GetColumn( this.csvData.headers[ 1 ] );
            DataColumn versionTypeColumn = csvData.GetColumn( this.csvData.headers[ 2 ] );
            DataColumn versionColumn = csvData.GetColumn( this.csvData.headers[ 3 ] );
            DataColumn checksumColumn = csvData.GetColumn( this.csvData.headers[ 4 ] );

            //Store where in the array we located this Asset bundle name we're looking for
            int row = -99;

            //Check if the package we care about is listed in the CSVData
            for( int i = 0; i < nameColumn.data.Length; i++ )
            {
                //If we find the row number for the package we're looking for, store where we found it
                if( nameColumn.data[i] == parseOnlyThisPackage )
                {
                    row = i;
                    break;
                }
            }

            //If we didn't find the package by name, we cannot continue!
            if( row == -99 )
            {
                Debug.LogError( "BlockEventAssetBundle.cs ParseCSVIntoPackageInfo() Unable to locate " + parseOnlyThisPackage + " within CSVData, unable to continue Asset Bundle download logic" );
                return;
            }

            //Create a new PackageInfo object to store information about the Asset Bundle and make that data viewable in the editor
            WWWHelper.AssetBundleInfo package = CreateAssetBundleInfoFromCSVDataRow
                ( row, nameColumn, pathColumn, versionTypeColumn, versionColumn, checksumColumn );

            //Now that our PackageInfo is set up, add it to our list of packages to be downloaded
            if( IsPackageValid( package ) )
            {
                assetBundleInfos.Add( package );
            }
            
        } //END ParseCSVIntoPackageInfo

        //----------------------------------//
        private WWWHelper.AssetBundleInfo CreateAssetBundleInfoFromCSVDataRow( int row, DataColumn nameColumn, DataColumn pathColumn, DataColumn versionTypeColumn, DataColumn versionColumn, DataColumn checksumColumn )
        //----------------------------------//
        {
            if( row != -99 )
            {
                //Create a new PackageInfo object to store information about the Asset Bundle and make that data viewable in the editor
                WWWHelper.AssetBundleInfo package = new WWWHelper.AssetBundleInfo();
                package.name = nameColumn.data[ row ];
                package.path = pathColumn.data[ row ];

                //Figure out what kind of version control system we are using, either a Integer or a Hash128
                if( versionTypeColumn.data[ row ] == "int" )
                {
                    package.versionType = WWWHelper.AssetBundleVersionType.integer;

                    uint version = new uint();

                    if( uint.TryParse( versionColumn.data[ row ], out version ) )
                    {
                        package.versionNumber = version;
                    }
                }
                else if( versionTypeColumn.data[ row ] == "hash" )
                {
                    package.versionType = WWWHelper.AssetBundleVersionType.hash;

                    package.versionHash = versionColumn.data[ row ];
                }

                //Figure out if this Asset Bundle uses a checksum
                uint checksum = 0;
                uint.TryParse( checksumColumn.data[ row ], out checksum );

                if( checksumColumn.data[ row ] != "" && checksum != 0 )
                {
                    package.checksum = uint.Parse( checksumColumn.data[ row ] );
                }

                return package;
            }

            return null;

        } //END CreateAssetBundleInfoFromCSVDataRow

        //----------------------------------//
        private bool IsPackageValid( WWWHelper.AssetBundleInfo package )
        //----------------------------------//
        {

            if( package == null )
            {
                Debug.LogError( "BlockEventAssetBundle.cs IsPackageValid() Package is invalid, the packageInfo object is null" );
                return false;
            }

            if( package.name == "" )
            {
                Debug.LogError( "BlockEventAssetBundle.cs IsPackageValid() Package is invalid, the package name is empty" );
                return false;
            }

            if( package.path == "" )
            {
                Debug.LogError( "BlockEventAssetBundle.cs IsPackageValid() Package is invalid, the package path is empty" );
                return false;
            }

            if( package.versionType == WWWHelper.AssetBundleVersionType.integer )
            {
                if( package.versionNumber == new uint() )
                {
                    Debug.LogError( "BlockEventAssetBundle.cs IsPackageValid() Package is invalid, the package version number is the same as newly created uint value" );
                    return false;
                }
            }

            if( package.versionType == WWWHelper.AssetBundleVersionType.hash )
            {
                if( package.versionHash == "" )
                {
                    Debug.LogError( "BlockEventAssetBundle.cs IsPackageValid() Package is invalid, the package hash is the same as a newly created Hash128 object" );
                    return false;
                }
            }

            /*
            if( !IsVersionTypeNone() && package.checksum == new uint() )
            {
                Debug.LogError( "BlockEventAssetBundle.cs IsPackageValid() Package is invalid, the checksum is the same as a newly created checksum object" );
                return false;
            }
            */

            return true;

        } //END IsPackageValid

        //---------------------------------------------//
        private void OnSingleAssetBundleDownloadSuccess( WWWHelper.AssetBundleInfo assetBundleInfo )
        //---------------------------------------------//
        {
            //Debug.Log( "BlockEventAssetBundle.cs OnSingleAssetBundleDownloadSuccess() assetBundleInfo.assetBundle = " + assetBundleInfo.assetBundle );

            if( assetBundleInfo != null && assetBundleInfo.assetBundle != null )
            {
                //Was there only one download to wait for? If so we are done! Set a flag marking that
                if( DownloadFromPath() || DownloadOnlySingleFromCSV() )
                {
                    downloadsComplete = true;
                }

                if( onSingleAssetBundleDownloadSuccess != null )
                {
                    onSingleAssetBundleDownloadSuccess.Invoke( assetBundleInfo );
                }
            }
            else
            {
                OnSingleAssetBundleDownloadFailed( assetBundleInfo );
            }

        } //END OnSingleAssetBundleDownloadSuccess

        //---------------------------------------------//
        private void OnSingleAssetBundleDownloadFailed( WWWHelper.AssetBundleInfo assetBundleInfo )
        //---------------------------------------------//
        {

            if( assetBundleInfo != null )
            {
                if( onSingleAssetBundleDownloadFailed != null )
                {
                    onSingleAssetBundleDownloadFailed.Invoke( assetBundleInfo );
                }
            }
            else
            {
                if( onSingleAssetBundleDownloadFailed != null )
                {
                    onSingleAssetBundleDownloadFailed.Invoke( null );
                }
            }

        } //END OnSingleAssetBundleDownloadFailed

        //---------------------------------------------//
        private void OnMultipleAssetBundleDownloadComplete( WWWHelper.AssetBundleInfo assetBundleInfo )
        //---------------------------------------------//
        {

            if( assetBundleInfo.assetBundle != null )
            {
                //Send a message out that this individual download has completed
                OnSingleAssetBundleDownloadSuccess( assetBundleInfo );
                
                //Iterate the success counter
                counterSuccessfullDownloads++;
                
                //If this was the last download, check what event we should send out
                if( counterFailedDownloads + counterSuccessfullDownloads == assetBundleInfos.Count )
                {
                    downloadsComplete = true;
                    
                    //If some of the downloads failed, send a message out to that effect
                    if( counterFailedDownloads > 0 )
                    {
                        if( onAssetBundlesDownloadedWithPartialSuccess != null )
                        {
                            onAssetBundlesDownloadedWithPartialSuccess.Invoke();
                        }
                    }
                    else //Otherwise if all of the downloads succeeded, inform the user
                    {
                        if( onAllAssetBundlesDownloadedSuccessfully != null )
                        {
                            onAllAssetBundlesDownloadedSuccessfully.Invoke();
                        }
                    }
                }

            }
            else
            {
                OnMultipleAssetBundleDownloadFailed( assetBundleInfo );
            }
            
        } //END OnMultipleAssetBundleDownloadComplete

        //---------------------------------------------//
        private void OnMultipleAssetBundleDownloadFailed( WWWHelper.AssetBundleInfo assetBundleInfo )
        //---------------------------------------------//
        {
            //Let the user know that this download failed
            if( assetBundleInfo != null )
            {
                OnSingleAssetBundleDownloadFailed( assetBundleInfo );
            }

            //Iterate the failed counter
            counterFailedDownloads++;

            //If this was the last download, check what event we should send out
            if( counterFailedDownloads + counterSuccessfullDownloads == assetBundleInfos.Count )
            {
                //If some of the downloads failed, send a message out to that effect
                if( counterFailedDownloads != assetBundleInfos.Count )
                {
                    if( onAssetBundlesDownloadedWithPartialSuccess != null )
                    {
                        onAssetBundlesDownloadedWithPartialSuccess.Invoke();
                    }
                }
                else //Otherwise if all of the downloads failed, inform the user
                {
                    if( onAllAssetBundleDownloadsFailed != null )
                    {
                        onAllAssetBundleDownloadsFailed.Invoke();
                    }
                }
            }

        } //END OnMultipleAssetBundleDownloadFailed

        //---------------------------------------------//
        private void OnAssetBundleDownloadProgressUpdate( WWWHelper.AssetBundleInfo assetBundleInfo )
        //---------------------------------------------//
        {

            if( onAssetBundleDownloadProgressUpdate != null )
            {
                float totalProgress = 0f;
                float maxProgress = assetBundleInfos.Count * 100f;
                
                //Check what the progress is for all of the asset bundles we are downloading
                if( assetBundleInfos != null && assetBundleInfos.Count > 0 )
                {
                    foreach( WWWHelper.AssetBundleInfo info in assetBundleInfos )
                    {
                        totalProgress += info.progress;
                    }
                }
                
                //Update the user on the current Asset Bundle Progress, and the Overall Progress
                onAssetBundleDownloadProgressUpdate.Invoke( MathHelper.Map( totalProgress, 0f, maxProgress, 0f, 100f ).ToString() );
            }

        } //END OnAssetBundleDownloadProgressUpdate
        #endregion

        #region METHODS - LOAD ASSET BUNDLE EVENT
        //------------------------------------------------//
        private void BeginLoadAssetBundleEvent()
        //------------------------------------------------//
        {
            //Reset the variable we use for keeping track of the number of loads remain
            counterFailedLoads = 0;
            counterSuccessfullLoads = 0;
            totalNumberOfLoadsToWaitFor = 0;

            if( blockEventAssetBundle != null )
            {
                if( blockEventAssetBundle.assetBundleInfos != null && blockEventAssetBundle.assetBundleInfos.Count > 0 )
                {
                    if( LoadAllBundles() )
                    {
                        totalNumberOfLoadsToWaitFor = blockEventAssetBundle.assetBundleInfos.Count;

                        foreach( WWWHelper.AssetBundleInfo assetBundleInfo in blockEventAssetBundle.assetBundleInfos )
                        {
                            LoadAssetBundle( assetBundleInfo );
                        }
                    }
                    else if( LoadBundlesWithSpecificNames() )
                    {
                        if( namesOfBundlesToLoad != null && namesOfBundlesToLoad.Count > 0 )
                        {
                            //Find out how many asset bundles we need to wait to load
                            foreach( WWWHelper.AssetBundleInfo assetBundleInfo in blockEventAssetBundle.assetBundleInfos )
                            {
                                if( namesOfBundlesToLoad.Contains( assetBundleInfo.name ) )
                                {
                                    totalNumberOfLoadsToWaitFor++;
                                }
                            }
                            
                            //Make sure there are actual asset bundles to load, if not log an error and skip the loading process
                            if( totalNumberOfLoadsToWaitFor > 0 )
                            {
                                //Now that we know the total number of assets to load, let's tell them to load
                                foreach( WWWHelper.AssetBundleInfo assetBundleInfo in blockEventAssetBundle.assetBundleInfos )
                                {
                                    if( namesOfBundlesToLoad.Contains( assetBundleInfo.name ) )
                                    {
                                        LoadAssetBundle( assetBundleInfo );
                                    }
                                }
                            }
                            else
                            {
                                Debug.LogError( "BlockEventAssetBundle.cs BeginLoadAssetBundleEvent() Cannot load asset bundle(s), the totalNumberOfLoadsToWaitFor is 0" );
                            }
                        }
                        else
                        {
                            Debug.LogError( "BlockEventAssetBundle.cs BeginLoadAssetBundleEvent() Cannot load asset bundle(s), the namesOfBundlesToLoad list is null or empty" );
                        }
                    }
                }
                else
                {
                    Debug.LogError( "BlockEventAssetBundle.cs BeginLoadAssetBundleEvent() Cannot load asset bundle(s), the assetBundleInfo list is null or empty" );
                }
            }
            else
            {
                Debug.LogError( "BlockEventAssetBundle.cs BeginLoadAssetBundleEvent() Cannot load asset bundle(s), the linked to blockEventAssetBundle component is null" );
            }

        } //END BeginLoadAssetBundleEvent

        //-----------------------------------------------//
        private void LoadAssetBundle( WWWHelper.AssetBundleInfo assetBundleInfo )
        //-----------------------------------------------//
        {

            if( assetBundleInfo != null )
            {
                if( assetBundleInfo.assetBundle != null )
                {
                    if( LoadAllAssets() )
                    {
                        WWWHelper.instance.LoadAllAssetBundleIntoMemory( assetBundleInfo, LoadAssetBundleSuccess, LoadAssetBundleFailed );
                    }
                    else if( LoadAssetsByNames() )
                    {
                        WWWHelper.instance.LoadAssetBundleIntoMemory( assetBundleInfo, namesOfAssetsToLoad, LoadAssetBundleSuccess, LoadAssetBundleFailed );
                    }
                    else if( LoadAssetsByNamesAndTypes() )
                    {
                        //Check if the Type we are trying to load is valid, if not revert to only loading based on name
                        //https://forum.unity.com/threads/cant-get-type-from-string.484723/
                        Type type = DirectoryHelper.FindTypeInAssemblies( typeOfAssetsToLoad );

                        if( type != null )
                        {
                            WWWHelper.instance.LoadAssetBundleIntoMemory( assetBundleInfo, namesOfAssetsToLoad, type, LoadAssetBundleSuccess, LoadAssetBundleFailed );
                        }
                        else
                        {
                            Debug.LogError( "BlockEventAssetBundle.cs LoadAssetBundle() Cannot load asset bundle using specified Type. Attempting to Load the AssetBundle using just the name instead" );
                            WWWHelper.instance.LoadAssetBundleIntoMemory( assetBundleInfo, namesOfAssetsToLoad, LoadAssetBundleSuccess, LoadAssetBundleFailed );
                        }
                    }
                }
                else
                {
                    Debug.LogWarning( "BlockEventAssetBundle.cs LoadAssetBundle() Failed to load asset bundle from assetBundleInfo. Attempting to re-download asset bundle now and then immediately load it" );
                    WWWHelper.instance.DownloadAssetBundle( assetBundleInfo, LoadAssetBundle, LoadAssetBundleFailed, null );
                }
            }
            else
            {
                Debug.LogError( "BlockEventAssetBundle.cs LoadAssetBundle() Cannot load asset bundle, the passed in assetBundleInfo is null" );
                LoadAssetBundleFailed( null );
            }

        } //END LoadAssetBundle

        //-----------------------------------------------//
        private void LoadAssetBundleSuccess( WWWHelper.AssetBundleInfo assetBundleInfo )
        //-----------------------------------------------//
        {

            if( assetBundleInfo != null )
            {
                if( onSingleAssetBundleLoaded != null ) { onSingleAssetBundleLoaded.Invoke( assetBundleInfo ); }

                counterSuccessfullLoads++;

                if( counterSuccessfullLoads + counterFailedLoads >= totalNumberOfLoadsToWaitFor )
                {
                    //If some of the loads failed, send a message out to that effect
                    if( counterFailedLoads > 0 )
                    {
                        if( onAllAssetBundlesLoadedWithPartialSuccess != null )
                        {
                            onAllAssetBundlesLoadedWithPartialSuccess.Invoke();
                        }
                    }
                    else //Otherwise if all of the loading succeeded, inform the user
                    {
                        if( onAllAssetBundlesLoadedSuccessfully != null )
                        {
                            onAllAssetBundlesLoadedSuccessfully.Invoke();
                        }
                    }
                }
            }
            else
            {
                LoadAssetBundleFailed( null );
            }

        } //END LoadAssetBundleSuccess

        //-----------------------------------------------//
        private void LoadAssetBundleFailed( WWWHelper.AssetBundleInfo assetBundleInfo )
        //-----------------------------------------------//
        {
            
            counterFailedLoads++;

            if( assetBundleInfo != null )
            {
                if( onLoadAssetBundleFailed != null ) { onLoadAssetBundleFailed.Invoke( assetBundleInfo ); }

                if( counterSuccessfullLoads + counterFailedLoads >= totalNumberOfLoadsToWaitFor )
                {
                    //If some of the loads failed, send a message out to that effect
                    if( counterFailedLoads != totalNumberOfLoadsToWaitFor )
                    {
                        if( onAllAssetBundlesLoadedWithPartialSuccess != null )
                        {
                            onAllAssetBundlesLoadedWithPartialSuccess.Invoke();
                        }
                    }
                    else //Otherwise if all of the loads failed, inform the user
                    {
                        if( onAllAssetBundlesLoadingFailed != null )
                        {
                            onAllAssetBundlesLoadingFailed.Invoke();
                        }
                    }
                }
                
            }
            else
            {
                if( onLoadAssetBundleFailed != null ) { onLoadAssetBundleFailed.Invoke( null ); }
            }

        } //END LoadAssetBundleFailed
        #endregion

        #region METHODS - UNLOAD ASSET BUNDLE EVENT
        //-----------------------------------------------//
        private void BeginUnloadAssetBundleEvent()
        //-----------------------------------------------//
        {
            //Reset the variables used for tracking the unload progress
            counterFailedUnloads = 0;
            counterSuccessfullUnloads = 0;
            totalNumberOfUnloadsToWaitFor = 0;

            if( UnloadAllBundles() )
            {
                if( blockEventAssetBundle.assetBundleInfos != null && blockEventAssetBundle.assetBundleInfos.Count > 0 )
                {
                    totalNumberOfUnloadsToWaitFor = blockEventAssetBundle.assetBundleInfos.Count;

                    foreach( WWWHelper.AssetBundleInfo assetBundleInfo in blockEventAssetBundle.assetBundleInfos )
                    {
                        if( assetBundleInfo != null )
                        {
                            WWWHelper.instance.UnloadAssetBundleFromMemory( assetBundleInfo, UnloadAssetBundleSuccess, UnloadAssetBundleFailed );
                        }
                    }
                }
                else
                {
                    Debug.LogError( "BlockEventAssetBundle.cs BeginUnloadAssetBundleEvent() Unable to unload asset bundles, the list of assetBundleInfos is null or empty within the linked blockEventAssetBundle" );

                    if( onAllAssetBundlesUnloadingFailed != null ) { onAllAssetBundlesUnloadingFailed.Invoke(); }
                }
            }
            else if( UnloadBundlesWithSpecificNames() )
            {
                if( assetBundlesToUnload != null && assetBundlesToUnload.Count > 0 )
                {
                    if( blockEventAssetBundle.assetBundleInfos != null && blockEventAssetBundle.assetBundleInfos.Count > 0 )
                    {
                        //Count the number of unloads to wait for
                        foreach( WWWHelper.AssetBundleInfo assetBundleInfo in blockEventAssetBundle.assetBundleInfos )
                        {
                            if( assetBundleInfo != null && assetBundlesToUnload.Contains( assetBundleInfo.name ) )
                            {
                                totalNumberOfUnloadsToWaitFor++;
                            }
                        }

                        //Begin the unload process
                        foreach( WWWHelper.AssetBundleInfo assetBundleInfo in blockEventAssetBundle.assetBundleInfos )
                        {
                            if( assetBundleInfo != null && assetBundlesToUnload.Contains( assetBundleInfo.name ) )
                            {
                                WWWHelper.instance.UnloadAssetBundleFromMemory( assetBundleInfo, UnloadAssetBundleSuccess, UnloadAssetBundleFailed );
                            }
                        }
                    }
                    else
                    {
                        Debug.LogError( "BlockEventAssetBundle.cs BeginUnloadAssetBundleEvent() Unable to unload asset bundles, the list of assetBundleInfos is null or empty within the linked blockEventAssetBundle" );
                    }
                }
                else
                {
                    Debug.LogError( "BlockEventAssetBundle.cs BeginUnloadAssetBundleEvent() Unable to unload asset bundles, the assetBundlesToUnload list is null or empty" );

                    if( onAllAssetBundlesUnloadingFailed != null ) { onAllAssetBundlesUnloadingFailed.Invoke(); }
                }
            }
            
        } //END BeginUnloadAssetBundleEvent

        //-----------------------------------------------//
        private void UnloadAssetBundleSuccess( WWWHelper.AssetBundleInfo assetBundleInfo )
        //-----------------------------------------------//
        {

            if( assetBundleInfo != null )
            {
                if( onSingleAssetBundleUnloaded != null ) { onSingleAssetBundleUnloaded.Invoke( assetBundleInfo ); }

                counterSuccessfullUnloads++;

                if( counterSuccessfullUnloads + counterFailedUnloads >= totalNumberOfUnloadsToWaitFor )
                {
                    //If some of the unloads failed, send a message out to that effect
                    if( counterFailedUnloads > 0 )
                    {
                        if( onAllAssetBundlesUnloadedWithPartialSuccess != null )
                        {
                            onAllAssetBundlesUnloadedWithPartialSuccess.Invoke();
                        }
                    }
                    else //Otherwise if all of the unloading succeeded, inform the user
                    {
                        if( onAllAssetBundlesUnloadedSuccessfully != null )
                        {
                            onAllAssetBundlesUnloadedSuccessfully.Invoke();
                        }
                    }
                }
            }
            else
            {
                UnloadAssetBundleFailed( assetBundleInfo );
            }

        } //END UnloadAssetBundleSuccess

        //-----------------------------------------------//
        private void UnloadAssetBundleFailed( WWWHelper.AssetBundleInfo assetBundleInfo )
        //-----------------------------------------------//
        {

            if( assetBundleInfo != null )
            {
                if( onUnloadAssetBundleFailed != null ) { onUnloadAssetBundleFailed.Invoke( assetBundleInfo ); }

                if( counterSuccessfullUnloads + counterFailedUnloads >= totalNumberOfUnloadsToWaitFor )
                {
                    //If some of the unloads failed, send a message out to that effect
                    if( counterFailedUnloads != totalNumberOfUnloadsToWaitFor )
                    {
                        if( onAllAssetBundlesUnloadedWithPartialSuccess != null )
                        {
                            onAllAssetBundlesUnloadedWithPartialSuccess.Invoke();
                        }
                    }
                    else //Otherwise if all of the unloads failed, inform the user
                    {
                        if( onAllAssetBundlesUnloadingFailed != null )
                        {
                            onAllAssetBundlesUnloadingFailed.Invoke();
                        }
                    }
                }
            }
            else
            {
                if( onUnloadAssetBundleFailed != null ) { onUnloadAssetBundleFailed.Invoke( null ); }
            }

        } //END UnloadAssetBundleFailed
        #endregion

        #region METHODS - INSTANTIATE ASSET BUNDLE GAMEOBJECT EVENT
        //-----------------------------------------------//
        private void BeginInstantiateAssetBundleEvent()
        //-----------------------------------------------//
        {
            //Reset our variables for this event
            counterInstantiatedSuccessfully = 0;
            counterInstantiatedFailed = 0;
            totalNumberOfInstantiations = 0;

            //We can only Instantiate asset bundles once they have been downloaded
            if( blockEventAssetBundle != null && blockEventAssetBundle.downloadsComplete )
            {
                if( InstantiateUsingNames() )
                {
                    if( namesOfBundlesToInstantiate != null && namesOfBundlesToInstantiate.Count > 0 )
                    {
                        if( blockEventAssetBundle.assetBundleInfos != null && blockEventAssetBundle.assetBundleInfos.Count > 0 )
                        {
                            //Now that we're done finding out how many Instantiate's we need to wait for, let's being instantiating!
                            foreach( WWWHelper.AssetBundleInfo assetBundleInfo in blockEventAssetBundle.assetBundleInfos )
                            {
                                if( assetBundleInfo != null && assetBundleInfo.name != "" )
                                {
                                    if( namesOfBundlesToInstantiate.Contains( assetBundleInfo.name ) )
                                    {
                                        InstantiateBundle( assetBundleInfo );
                                    }
                                }
                                else
                                {
                                    Debug.LogError( "BlockEventAssetBundle.cs BeginInstantiateAssetBundleEvent() Error: Unable to instantiate asset bundles, the linked Block Event - Asset Bundle component has completed its download of the asset bundles but the assetBundleInfo container does not have a name for the asset bundle to compare to our list" );
                                }
                            }
                            
                        }
                        else
                        {
                            Debug.LogError( "BlockEventAssetBundle.cs BeginInstantiateAssetBundleEvent() Error: Unable to instantiate asset bundles, the linked Block Event - Asset Bundle component has completed its download of the asset bundles but the list of assetBundleInfo's is empty" );
                        }
                    }
                    else
                    {
                        Debug.LogError( "BlockEventAssetBundle.cs BeginInstantiateAssetBundleEvent() Error: Unable to instantiate asset bundles, the list of asset bundles to create in this component is empty. We do not know what bundles to instantiate." );
                    }
                }
                else if( InstantiateAllAssetBundles() )
                {
                    if( blockEventAssetBundle.assetBundleInfos != null && blockEventAssetBundle.assetBundleInfos.Count > 0 )
                    {
                        foreach( WWWHelper.AssetBundleInfo assetBundleInfo in blockEventAssetBundle.assetBundleInfos )
                        {
                            InstantiateBundle( assetBundleInfo );
                        }
                    }
                    else
                    {
                        Debug.LogError( "BlockEventAssetBundle.cs BeginInstantiateAssetBundleEvent() Error: Unable to instantiate asset bundles, the linked Block Event - Asset Bundle component has completed its download of the asset bundles but the list of assetBundleInfo's is empty" );
                    }
                }
            }
            else
            {
                Debug.LogError( "BlockEventAssetBundle.cs BeginInstantiateAssetBundleEvent() Error: Unable to instantiate asset bundles, the linked Block View - Asset Bundle component has not completed its download of the asset bundles" );
            }

            //We've finished Instantiating the GameObjects, let's see what event message we should send out, based on the number of success and failed events there were
            if( totalNumberOfInstantiations != 0 )
            {
                //If we some Instantiations failed but some succeeded
                if( counterInstantiatedFailed > 0 && counterInstantiatedSuccessfully > 0 )
                {
                    if( onAllAssetBundleGameObjectsInstantiatedWithPartialSuccess != null ) { onAllAssetBundleGameObjectsInstantiatedWithPartialSuccess.Invoke(); }
                }

                //If all of our instantions succeeded
                else if( counterInstantiatedFailed == 0 )
                {
                    if( onAllAssetBundleGameObjectsInstantiatedSuccessfully != null ) { onAllAssetBundleGameObjectsInstantiatedSuccessfully.Invoke(); }
                }

                //Otherwise if all of our instantions failed
                else if( counterInstantiatedFailed > 0 )
                {
                    if( onAllAssetBundleGameObjectsInstantiationFailed != null ) { onAllAssetBundleGameObjectsInstantiationFailed.Invoke(); }
                }
            }

        } //END BeginInstantiateAssetBundleEvent

        //--------------------------------------------//
        private void InstantiateBundle( WWWHelper.AssetBundleInfo assetBundleInfo )
        //--------------------------------------------//
        {
            //Find the asset bundle requests with assets we want to instantiate as GameObjects
            if( assetBundleInfo != null )
            {
                if( assetBundleInfo.assetBundleRequests != null && assetBundleInfo.assetBundleRequests.Count > 0 )
                {
                    
                    //Go through each Asset Bundle Request that was already loaded into memory and try to Instantiate the GameObjects we locate
                    foreach( AssetBundleRequest request in assetBundleInfo.assetBundleRequests )
                    {
                        if( request != null && request.asset )
                        {
                            //Debug.Log( "BlockEventAssetBundle.cs InstantiateBundle() The bundle asset(" + assetBundleInfo.name + ") has an AssetBundleRequest with an asset, let's see if this is an asset we want to Instantiate" );

                            try
                            {
                                //Check if we should limit what GameObjects get instantiated based on their names
                                if( ShouldInstantiateAllGameObjectAssets() ||
                                    ( ShouldInstantiateGameObjectAssetsBasedOnNames() && 
                                        namesOfGameObjectAssetsToInstantiate != null && 
                                        namesOfGameObjectAssetsToInstantiate.Contains( request.asset.name ) ) )
                                {
                                    
                                    //If we are supposed to parent this new GameObject to a transform or blockModel, make sure to do so now
                                    if( InstantiateWithParent() && parentToThisTransform != null )
                                    {
                                        //Try instantiating the GameObject via the request
                                        assetBundleInfo.instantiatedGameObject = Instantiate( request.asset as GameObject );

                                        //Remove (Clone) from the GameObject's name
                                        assetBundleInfo.instantiatedGameObject.name = request.asset.name;

                                        //Parent the GameObject to the transform
                                        assetBundleInfo.instantiatedGameObject.transform.parent = parentToThisTransform;
                                    }
                                    else if( InstantiateWithBlockModel() && blockModelToChange != null )
                                    {
                                        //Instantiate the GameObject using the BlockModel
                                        blockModelToChange.ChangeModel( request.asset as GameObject );

                                        //Add a reference to the GameObject
                                        assetBundleInfo.instantiatedGameObject = blockModelToChange.model;
                                    }

                                    //If our Instantiation was successfull, make a note of it!
                                    if( assetBundleInfo.instantiatedGameObject != null )
                                    {
                                        //Debug.Log( "BlockEventAssetBundle.cs InstantiateBundle() The bundle asset(" + assetBundleInfo.name + ") has instantiated a GameObject.name = " + assetBundleInfo.instantiatedGameObject.name );
                                        counterInstantiatedSuccessfully++;
                                        totalNumberOfInstantiations++;

                                        //Let the user know this GameObject instantiated successfully
                                        if( onSingleAssetBundleGameObjectInstantiatedSuccessfully != null ) { onSingleAssetBundleGameObjectInstantiatedSuccessfully.Invoke( assetBundleInfo.instantiatedGameObject ); }
                                    }
                                    
                                }
                                else
                                {
                                    counterInstantiatedFailed++;
                                    totalNumberOfInstantiations++;
                                    //Debug.Log( "BlockEventAssetBundle.cs InstantiateBundle() The bundle asset(" + assetBundleInfo.name + ") was unable to instantiate the requested GameObject. The Request.asset.name = " + request.asset.name + "... ShouldInstantiateAllGameObjectAssets() = " + ShouldInstantiateAllGameObjectAssets() + ", ShouldInstantiateGameObjectAssetsBasedOnNames() = " + ShouldInstantiateGameObjectAssetsBasedOnNames() );

                                    if( onSingleAssetBundleGameObjectInstantiationFailed != null ) { onSingleAssetBundleGameObjectInstantiationFailed.Invoke( request.asset.name ); }
                                }
                            }
                            catch( Exception )
                            {
                                counterInstantiatedFailed++;
                                totalNumberOfInstantiations++;
                                Debug.LogError( "BlockEventAssetBundle.cs InstantiateBundle() The bundle asset(" + assetBundleInfo.name + ") could not be instantiated as a GameObject" );
                            }
                        }
                        else
                        {
                            counterInstantiatedFailed++;
                            totalNumberOfInstantiations++;
                            Debug.LogError( "BlockEventAssetBundle.cs InstantiateBundle() The bundle asset(" + assetBundleInfo.name + ") has an assetBundleRequest with no asset!" );
                        }
                    }

                }
                else
                {
                    counterInstantiatedFailed++;
                    totalNumberOfInstantiations++;
                    Debug.LogError( "BlockEventAssetBundle.cs InstantiateBundle() Unable to instantiate, the passed in assetBundleInfo has a null asset bundle request" );
                }
            }
            else
            {
                counterInstantiatedFailed++;
                totalNumberOfInstantiations++;
                Debug.LogError( "BlockEventAssetBundle.cs InstantiateBundle() Unable to instantiate, the passed in assetBundleInfo is null" );
            }

        } //END InstantiateBundle
        #endregion

        #region METHODS - DELETE INSTANTIATED ASSET BUNDLE GAMEOBJECT EVENT
        //-------------------------------------------//
        private void BeginRemovingAssetBundleEvent()
        //-------------------------------------------//
        {

            if( RemoveUsingAssetBundleNames() )
            {
                _RemoveUsingAssetBundleNames();
            }
            else if( RemoveUsingInstantiatedGameObjectNames() )
            {
                _RemoveUsingInstantiatedGameObjectNames();
            }
            else if( RemoveUsingLinksToInstantiatedGameObjects() )
            {
                _RemoveUsingLinksToInstantiatedGameObjects();
            }

            if( onComplete != null ) { onComplete.Invoke(); }

        } //END RemoveAssetBundleEvent

        //------------------------------------------//
        private void _RemoveUsingAssetBundleNames()
        //------------------------------------------//
        {

            if( namesOfBundlesToRemove != null && namesOfBundlesToRemove.Count > 0 )
            {
                if( blockEventAssetBundle.assetBundleInfos != null && blockEventAssetBundle.assetBundleInfos.Count > 0 )
                {
                    foreach( WWWHelper.AssetBundleInfo assetBundleInfo in blockEventAssetBundle.assetBundleInfos )
                    {
                        if( assetBundleInfo != null && namesOfBundlesToRemove.Contains( assetBundleInfo.name ) )
                        {
                            RemoveAssetBundle( assetBundleInfo );
                        }
                    }
                }
                else
                {
                    Debug.LogError( "BlockEventAssetBundle.cs _RemoveUsingAssetBundleNames() Unable to remove asset bundles, the list of assetBundleInfos from the BlockEvent - Asset Bundle is null or empty" );
                }
            }
            else
            {
                Debug.LogError( "BlockEventAssetBundle.cs _RemoveUsingAssetBundleNames() Unable to remove asset bundles, your list of namesOfBundlesToRemove is null or empty" );
            }

        } //END _RemoveUsingAssetBundleNames

        //------------------------------------------//
        private void _RemoveUsingInstantiatedGameObjectNames()
        //------------------------------------------//
        {

            if( namesOfGameObjectsToRemove != null && namesOfGameObjectsToRemove.Count > 0 )
            {
                if( blockEventAssetBundle.assetBundleInfos != null && blockEventAssetBundle.assetBundleInfos.Count > 0 )
                {
                    foreach( WWWHelper.AssetBundleInfo assetBundleInfo in blockEventAssetBundle.assetBundleInfos )
                    {
                        if( assetBundleInfo != null && assetBundleInfo.instantiatedGameObject != null )
                        {
                            if( namesOfGameObjectsToRemove.Contains( assetBundleInfo.instantiatedGameObject.name ) )
                            {
                                RemoveAssetBundle( assetBundleInfo );
                            }
                        }
                        else
                        {
                            Debug.LogError( "BlockEventAssetBundle.cs _RemoveUsingInstantiatedGameObjectNames() Unable to remove Instantiated GameObject derived from AssetBundle. The GameObject has not been instantiated yet" );
                        }
                    }
                }
                else
                {
                    Debug.LogError( "BlockEventAssetBundle.cs _RemoveUsingAssetBundleNames() Unable to remove asset bundles, the list of assetBundleInfos from the BlockEvent - Asset Bundle is null or empty" );
                }
            }
            else
            {
                Debug.LogError( "BlockEventAssetBundle.cs _RemoveUsingAssetBundleNames() Unable to remove asset bundles, your list of namesOfBundlesToRemove is null or empty" );
            }

        } //END _RemoveUsingInstantiatedGameObjectNames

        //------------------------------------------//
        private void _RemoveUsingLinksToInstantiatedGameObjects()
        //------------------------------------------//
        {

            if( gameObjectsToRemove != null && gameObjectsToRemove.Count > 0 )
            {
                if( blockEventAssetBundle.assetBundleInfos != null && blockEventAssetBundle.assetBundleInfos.Count > 0 )
                {
                    foreach( WWWHelper.AssetBundleInfo assetBundleInfo in blockEventAssetBundle.assetBundleInfos )
                    {
                        if( assetBundleInfo != null && assetBundleInfo.instantiatedGameObject != null )
                        {
                            if( gameObjectsToRemove.Contains( assetBundleInfo.instantiatedGameObject ) )
                            {
                                RemoveAssetBundle( assetBundleInfo );
                            }
                        }
                        else
                        {
                            Debug.LogError( "BlockEventAssetBundle.cs _RemoveUsingLinksToInstantiatedGameObjects() Unable to remove Instantiated GameObject derived from AssetBundle. The GameObject has not been instantiated yet" );
                        }
                    }
                }
                else
                {
                    Debug.LogError( "BlockEventAssetBundle.cs _RemoveUsingLinksToInstantiatedGameObjects() Unable to remove asset bundles, the list of assetBundleInfos from the BlockEvent - Asset Bundle is null or empty" );
                }
            }
            else
            {
                Debug.LogError( "BlockEventAssetBundle.cs _RemoveUsingLinksToInstantiatedGameObjects() Unable to remove asset bundles, your list of gameObjectsToRemove is null or empty" );
            }

        } //END _RemoveUsingLinksToInstantiatedGameObjects

        //--------------------------------------------//
        private void RemoveAssetBundle( WWWHelper.AssetBundleInfo assetBundleInfo )
        //--------------------------------------------//
        {

            if( assetBundleInfo != null )
            {
                //Destroy the asset bundle GameObject if it have been instantiated
                if( assetBundleInfo.instantiatedGameObject != null )
                {
#if UNITY_EDITOR
                    //Wait a moment before calling DestroyImmediate to make sure no logic is running
                    UnityEditor.EditorApplication.delayCall += () =>
                    {
                        DestroyImmediate( assetBundleInfo.instantiatedGameObject );
                    };
#else
                    Destroy( assetBundleInfo.instantiatedGameObject );
#endif
                    
                }
                else
                {
                    Debug.LogError( "BlockEventAssetBundle.cs RemoveAssetBundle() Unable to remove asset bundle's instantiated GameObject, it has not been instantiated yet" );
                }

                //Remove the asset bundle from memory

                
            }
            else
            {
                Debug.LogError( "BlockEventAssetBundle.cs RemoveAssetBundle() unable to remove asset bundle, the passed in assetBundleInfo is null" );
            }

        } //END RemoveAssetBundle
        #endregion

        #region METHODS - CLEAR CACHE EVENT
        //--------------------------------------------//
        private void ClearCacheEvent()
        //--------------------------------------------//
        {
            bool success = false;

            if( ShouldClearAllCaches() )
            {
                success = WWWHelper.instance.ClearAssetBundleCache();
            }
            else if( ShouldClearSpecificCaches() )
            {
                success = WWWHelper.instance.ClearAssetBundleCache( assetBundleNamesToClearCachesOf );
            }

            if( success )
            {
                //Debug.Log( "BlockEventAssetBundle.cs ClearCacheEvent() Success" );
            }
            else
            {
                Debug.LogError( "BlockEventAssetBundle.cs ClearCacheEvent() Failed" );
            }

            if( onComplete != null ) { onComplete.Invoke(); }

        } //END ClearCacheEvent
        #endregion


    } //END BlockEventAssetBundle

} //END Namespace