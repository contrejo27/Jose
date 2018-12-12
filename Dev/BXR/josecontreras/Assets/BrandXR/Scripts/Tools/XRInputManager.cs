/* XRInputManager.cs
 * 
 * Handles the adding of plugins used to detect input
 */

using UnityEngine;
using System.Collections;
using Sirenix.OdinInspector;
using System.Collections.Generic;
using System;

#if LEANTOUCH
using Lean.Touch;
#endif

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace BrandXR
{
    public class XRInputManager: MonoBehaviour
    {
       
        public bool showDebug = false;

        //Singleton behavior
        private static XRInputManager _instance;

        //--------------------------------------------//
        public static XRInputManager instance
        //--------------------------------------------//
        {
            get
            {
                if( _instance == null )
                {
                    if( GameObject.FindObjectOfType<XRInputManager>() == null ) { PrefabManager.InstantiatePrefab( PrefabFactory.Prefabs.bxr_XRInputManager ); }
                    _instance = GameObject.FindObjectOfType<XRInputManager>();
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

            if( xrTouchSupportType == XRTouchSupportType.LeanTouch )
            {
                AddLeanTouchGameObject();
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
        [SerializeField, FoldoutGroup( "Use XR Input On Platforms" ), InfoBox( "All of the build platforms you want to setup to use XR Input.\n\nDefaults to Android and iOS\n\nUsed for setting Scripting symbols for those platforms in build settings" )]
        List<BuildTargetGroup> useXRTouchSupportOnPlatforms = new List<BuildTargetGroup>()
        {
            BuildTargetGroup.Android,
            BuildTargetGroup.iOS
        };
#endif

        public enum XRTouchType
        {
            LEANTOUCH
        }

        public enum XRTouchSupportType
        {
            None,
            LeanTouch
        }
        [OnValueChanged( "SetupTechnologyInProject" ), InfoBox( "Determines what XR touch support will be enabled for this application" )]
        public XRTouchSupportType xrTouchSupportType = XRTouchSupportType.None;

        //-------------------------------------------------//
        private void SetupTechnologyInProject()
        //-------------------------------------------------//
        {

#if UNITY_EDITOR
            if( useXRTouchSupportOnPlatforms != null && useXRTouchSupportOnPlatforms.Count > 0 )
            {
                foreach( BuildTargetGroup group in useXRTouchSupportOnPlatforms )
                {
                    //if( showDebug ) { Debug.Log( "XRInputManager.cs SetupTechnologyInProject() group = " + group ); }
                    string defineSymbols = "";

                    //Regardless of what choice was made, we remove all of our XR Input define symbols to get a clean stake
                    defineSymbols = RemoveAllSymbols( group );

                    //Add the appropriate define symbols depending on what technology was chosen
                    if( xrTouchSupportType == XRTouchSupportType.LeanTouch )
                    {
                        defineSymbols = AddSymbol( group, XRTouchType.LEANTOUCH.ToString() );

                        //Change the project settings with the new symbols
                        PlayerSettings.SetScriptingDefineSymbolsForGroup(group, defineSymbols);
                    }

                    if( showDebug ) Debug.Log( "XRInputManager.cs SetupTechnologyInProject() group = " + group + ", newSymbols = " + defineSymbols );
                    
                }
                
            }
#endif

            //Timer.instance.In(.1f, CheckIfLeanTouchGameObjectShouldBeAdded, this.gameObject );
            
        } //END SetupTechnologyInProject

        //-------------------------------------------------//
        private void CheckIfLeanTouchGameObjectShouldBeAdded()
        //-------------------------------------------------//
        {

#if LEANTOUCH
            //Add the LeanTouch GameObject
            AddLeanTouchGameObject();
#else
            DestroyLeanTouchGameObject();
#endif

        } //END CheckIfLeanTouchGameObjectShouldBeAdded

#if UNITY_EDITOR
    //-------------------------------------------------//
    private string RemoveAllSymbols( BuildTargetGroup group )
        //-------------------------------------------------//
        {

            string newDefineSymbols = PlayerSettings.GetScriptingDefineSymbolsForGroup( group );

            //if( showDebug ) Debug.Log( "XRInputManager.cs RemoveAllSymbols() original symbols = " + newDefineSymbols );

            //Remove any scripting define symbols based on XR technology
            foreach( XRTouchType symbol in Enum.GetValues( typeof( XRTouchType ) ) )
            {
                if( newDefineSymbols.Contains( symbol.ToString() ) )
                {
                    //if( showDebug ) Debug.Log( "XRInputManager.cs RemoveAllSymbols() removing symbol = " + newDefineSymbols );
                    newDefineSymbols = newDefineSymbols.Replace( symbol.ToString(), "" );
                }
            }

            //Change the project settings with the new symbols
            //if( showDebug ) Debug.Log( "XRInputManager.cs RemoveAllSymbols() newSymbols = " + newDefineSymbols );
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



        [ShowIf( "DisplayLeanTouchFolderMissingWarning" ), InfoBox( "ERROR: Missing LeanTouch plugin folder in assets folder", InfoMessageType.Error )]
        private int leanTouchWarning1 = 0;

        //-------------------------------------------------//
        private bool DisplayLeanTouchFolderMissingWarning()
        //-------------------------------------------------//
        {
            //If this is using Vuforia, check to make sure the technology is enabled
            if( xrTouchSupportType == XRTouchSupportType.LeanTouch )
            {

                //Check that the LeanTouch folder exists in the Assets database
#if UNITY_EDITOR
                if( !AssetDatabase.IsValidFolder( "Assets/LeanTouch" ) )
                {
                    return true;
                }
#endif
            }

            return false;

        } //END DisplayLeanTouchFolderMissingWarning


        //-------------------------------------------//
        private void AddLeanTouchGameObject()
        //-------------------------------------------//
        {

#if LEANTOUCH

            //Debug.Log("XRInputManager.cs AddLeanTouchGameObject() inside #if LEANTOUCH");

            if( GameObject.FindObjectOfType<LeanTouch>() == null )
            {
                //Debug.Log("XRInputManager.cs AddLeanTouchGameObject() inside if LeanTouch gameObject is null");

                GameObject go = new GameObject();
                go.name = "LeanTouch";

                go.transform.parent = transform;

                go.AddComponent<LeanTouch>();

                LeanSelect leanSelect = go.AddComponent<LeanSelect>();
                leanSelect.Search = LeanSelect.SearchType.GetComponentInParent;
                leanSelect.AutoDeselect = true;

                //go.AddComponent<LeanSelectUpdate>();
            }
            else
            {
                //Debug.Log("XRInputManager.cs AddLeanTouchGameObject() No need to add LeanTouch GameObject, it already exists");
            }
#else
            Debug.Log("XRInputManager.cs AddLeanTouchGameObject() Unable to Add Lean Touch GameObject, the #LEANTOUCH scripting define symbol is missing");
#endif

        } //END AddLeanTouchGameObject

        //-------------------------------------------//
        private void DestroyLeanTouchGameObject()
        //-------------------------------------------//
        {

#if LEANTOUCH
            //Remove the LeanTouch GameObject
            if( GameObject.FindObjectOfType<LeanTouch>() != null )
            {
#if UNITY_EDITOR
                //Wait a moment before calling DestroyImmediate to make sure no logic is running
                UnityEditor.EditorApplication.delayCall += () =>
                {
                    if( GameObject.FindObjectOfType<LeanTouch>().gameObject != null )
                    {
                            DestroyImmediate( GameObject.FindObjectOfType<LeanTouch>().gameObject );
                    }
                };
#else
                if( GameObject.FindObjectOfType<LeanTouch>().gameObject != null )
                {
                    Destroy( GameObject.FindObjectOfType<LeanTouch>().gameObject );
                }
#endif
            }
#endif

        } //END DestroyLeanTouchGameObject


    } //END Class

} //END BrandXR