/* XRTechnologyManager.cs
 * 
 * Enables the user to select what XR technologies they would like to include in the project.
 * 
 * Changes what Scripting define symbols are enabled for this project across the following platforms...
 * Android, and iOS
 * 
 * This changes certain Block system components based on your choices.
 * EXAMPLE: If you enable Vuforia Fusion technology, 
 *          then Vuforia XR targets will be enabled in the XRTarget component
 *          
 */
using Sirenix.OdinInspector;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace BrandXR
{
    public class XRTechnologyManager: MonoBehaviour
    {
        public bool showDebug = false;

        //Singleton behavior
        private static XRTechnologyManager _instance;

        //--------------------------------------------//
        public static XRTechnologyManager instance
        //--------------------------------------------//
        {
            get
            {
                if( _instance == null )
                {
                    if( GameObject.FindObjectOfType<XRTechnologyManager>() == null ) { PrefabManager.InstantiatePrefab( PrefabFactory.Prefabs.bxr_XRTechnologyManager ); }
                    _instance = GameObject.FindObjectOfType<XRTechnologyManager>();
                    BlockHelper.AddToBrandXRTechParent( _instance.transform );
                }
                
                return _instance;
            }

        } //END Instance

        //------------------------------------------------------//
        public void Awake()
        //------------------------------------------------------//
        {
            DestroyDuplicateInstance();

            if( transform.parent == null )
            {
                DontDestroyOnLoad( transform.gameObject );
            }

        } //END Awake

        //--------------------------------------------//
        private void DestroyDuplicateInstance()
        //--------------------------------------------//
        {

            //Ensure only one instance exists
            if( _instance == null )
            {
                _instance = this;
            }
            else if( this != _instance )
            {
                Destroy( this.gameObject );
            }

        } //END DestroyDuplicateInstance


#if UNITY_EDITOR
        [SerializeField, FoldoutGroup("Use XR Tech On Platforms" ), InfoBox("All of the build platforms you want to setup to use XR technology.\n\nDefaults to Android and iOS\n\nUsed for setting Scripting symbols for those platforms in build settings")]
        List<BuildTargetGroup> useXRTechOnPlatforms = new List<BuildTargetGroup>()
        {
            BuildTargetGroup.Android,
            BuildTargetGroup.iOS
        };
        
#endif

        public enum XRSymbol
        {
            VUFORIA,
            ARCORE,
            ARKIT
        }

        public enum XRTechnologyType
        {
            None,
            Vuforia,
            VuforiaFusion,
            ARCore,
            ARKit,
            ARCore_ARKit
        }
        [OnValueChanged( "SetupTechnologyInProject" ), InfoBox("Determines what XR technology will be enabled for this application")]
        public XRTechnologyType xrTechnologyType = XRTechnologyType.None;


        //-------------------------------------------------//
        private void SetupTechnologyInProject()
        //-------------------------------------------------//
        {

#if UNITY_EDITOR
            if( useXRTechOnPlatforms != null && useXRTechOnPlatforms.Count > 0 )
            {
                foreach( BuildTargetGroup group in useXRTechOnPlatforms )
                {
                    //if( showDebug ) { Debug.Log( "XRTechnologyManager.cs SetupTechnologyInProject() group = " + group ); }
                    string defineSymbols = "";

                    //Regardless of what choice was made, we remove all of our XR Technology define symbols to get a clean stake
                    defineSymbols = RemoveAllSymbols( group );

                    //Add the appropriate define symbols depending on what technology was chosen
                    if( xrTechnologyType == XRTechnologyType.Vuforia )
                    {
                        defineSymbols = AddSymbol( group, XRSymbol.VUFORIA.ToString() );
                    }
                    else if( xrTechnologyType == XRTechnologyType.VuforiaFusion )
                    {
                        defineSymbols = AddSymbol( group, XRSymbol.VUFORIA.ToString() );
                        //defineSymbols = AddSymbol( group, XRSymbol.ARCORE.ToString() );
                        //defineSymbols = AddSymbol( group, XRSymbol.ARKIT.ToString() );
                    }
                    else if( xrTechnologyType == XRTechnologyType.ARCore )
                    {
                        defineSymbols = AddSymbol( group, XRSymbol.ARCORE.ToString() );
                    }
                    else if( xrTechnologyType == XRTechnologyType.ARKit )
                    {
                        defineSymbols = AddSymbol( group, XRSymbol.ARKIT.ToString() );
                    }
                    else if( xrTechnologyType == XRTechnologyType.ARCore_ARKit )
                    {
                        defineSymbols = AddSymbol( group, XRSymbol.ARCORE.ToString() );
                        defineSymbols = AddSymbol( group, XRSymbol.ARKIT.ToString() );
                    }

                    if( showDebug ) Debug.Log( "XRTechnologyManager.cs SetupTechnologyInProject() group = " + group + ", newSymbols = " + defineSymbols );
                    
                }
            }
#endif

        } //END SetupTechnologyInProject

#if UNITY_EDITOR
        //-------------------------------------------------//
        private string RemoveAllSymbols( BuildTargetGroup group )
        //-------------------------------------------------//
        {

            string newDefineSymbols = PlayerSettings.GetScriptingDefineSymbolsForGroup( group );

            //if( showDebug ) Debug.Log( "XRTechnologyManager.cs SetupTechnologyInProject() original symbols = " + newDefineSymbols );

            //Remove any scripting define symbols based on XR technology
            foreach( XRSymbol symbol in Enum.GetValues( typeof( XRSymbol ) ) )
            {
                if( newDefineSymbols.Contains( symbol.ToString() ) )
                {
                    //if( showDebug ) Debug.Log( "XRTechnologyManager.cs SetupTechnologyInProject() removing symbol = " + newDefineSymbols );
                    newDefineSymbols = newDefineSymbols.Replace( symbol.ToString(), "" );
                }
            }

            //Change the project settings with the new symbols
            //if( showDebug ) Debug.Log( "XRTechnologyManager.cs SetupTechnologyInProject() newSymbols = " + newDefineSymbols );
            PlayerSettings.SetScriptingDefineSymbolsForGroup( group, newDefineSymbols );

            return PlayerSettings.GetScriptingDefineSymbolsForGroup( group );

        } //END RemoveAllSymbols
#endif

#if UNITY_EDITOR
        //-------------------------------------------------//
        private string AddSymbol( BuildTargetGroup group, string newSymbol )
        //-------------------------------------------------//
        {
            string newSymbols = PlayerSettings.GetScriptingDefineSymbolsForGroup( group ) + ";" + newSymbol;

            PlayerSettings.SetScriptingDefineSymbolsForGroup( group, newSymbols );

            return PlayerSettings.GetScriptingDefineSymbolsForGroup( group );

        } //END AddSymbol
#endif



        [ShowIf( "DisplayVuforiaFolderMissingWarning" ), InfoBox("ERROR: Missing Vuforia plugin folder in assets folder", InfoMessageType.Error )]
        private int vuforiaWarning1 = 0;

        //-------------------------------------------------//
        private bool DisplayVuforiaFolderMissingWarning()
        //-------------------------------------------------//
        {
            //If this is using Vuforia, check to make sure the technology is enabled
            if( xrTechnologyType == XRTechnologyType.Vuforia ||
                xrTechnologyType == XRTechnologyType.VuforiaFusion )
            {
                
                //Check that the Vuforia folder exists in the Assets database
#if UNITY_EDITOR
                if( !AssetDatabase.IsValidFolder( "Assets/Vuforia" ) )
                {
                    return true;
                }
#endif
            }

            return false;

        } //END DisplayVuforiaFolderMissingWarning

        //public int vuforiaWarning2 = 0;


    } //END Class

} //END Namespace