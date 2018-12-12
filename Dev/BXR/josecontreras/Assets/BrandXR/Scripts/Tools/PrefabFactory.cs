using UnityEngine;
using System.Collections.Generic;

namespace BrandXR
{
    public class PrefabFactory: MonoBehaviour
    {
        private bool showDebug = false;

        //Singleton behavior
        private static PrefabFactory _instance;

        //--------------------------------------------//
        public static PrefabFactory instance
        //--------------------------------------------//
        {
            get
            {
                if( _instance == null )
                {
                    if( GameObject.FindObjectOfType<PrefabFactory>() )
                    {
                        _instance = GameObject.FindObjectOfType<PrefabFactory>();
                    }
                    else
                    {
                        GameObject go = null;

                        if( GameObject.Find( "bxr_PrefabManager" ) != null )
                        {
                            go = GameObject.Find( "bxr_PrefabManager" );
                        }
                        else
                        {
                            go = new GameObject( "bxr_PrefabManager" );
                        }

                        System.Type type = ComponentHelper.FindType( "BrandXR.PrefabFactory" );

                        if( type != null )
                        {
                            _instance = (PrefabFactory)go.AddComponent( type );
                        }
                    }

                    if( _instance != null )
                    {
                        BlockHelper.AddToBrandXRTechParent( _instance.transform );
                    }
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

            if( showDebug ) { Debug.Log( "PrefabFactory.cs Awake start" ); }

            LinkPrefabsListToDictionary();

            if( showDebug ) { Debug.Log( "PrefabFactory.cs Awake finished calling LinkPrefabsListToDictionary()" ); }

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

        //An enum list used to choose what prefab you want to instantiate,
        // we .ToString() the enum to get a string, and pass that into the
        // PrefabsDictionary, which finds the proper Prefab to pass back.

        public enum Prefabs
        {
            bxr_BlockManager,
            bxr_BlockFocusManager,

            bxr_BlockView,
            bxr_BlockGroup,
            bxr_BlockButton,
            bxr_BlockText,
            bxr_BlockImage,
            bxr_BlockAudio,
            bxr_BlockVideo,
            bxr_BlockEvent,
            bxr_BlockModel,
            
            bxr_EventSystem,

            bxr_ScreenFadeManager,
            bxr_SceneLoader,
            bxr_XRSkyboxManager,
            bxr_AudioHelper,
            bxr_WWWHelper,
            bxr_Timer,

            bxr_TweenManager,
            bxr_EaseCurve,
            bxr_TweenPosition,
            bxr_TweenRotation,
            bxr_TweenScale,
            bxr_TweenColor,
            bxr_TweenInt,
            bxr_TweenFloat,

            bxr_XRCameraManager,
            bxr_XRSettings,
            bxr_XRInputManager,
            bxr_XRRingReticle,
            bxr_XRBarrelDistortionLine,

            bxr_XRTarget,
            bxr_XRTechnologyManager,
            bxr_XRTargetImage,
            bxr_XRTargetObject,
            bxr_VideoTechnologyManager

        }

        //The dictionary used to lookup the prefabs,
        //Filled during Start() with the list of prefabs
        public Dictionary<string, GameObject> PrefabsDictionary = new Dictionary<string, GameObject> { };

        //Filled in the hierarchy, drag new prefabs into this list and add them to the enum above to access them
        public List<GameObject> PrefabsList;

        //Added to during the PrefabManager.cs's InstantiatePrefab() function.
        //Keeps an active list of all of the existing prefab objects in the scene
        public Dictionary<string, GameObject> ExistingPrefabs = new Dictionary<string, GameObject> { };

        

        //------------------------------------------------------//
        public void LinkPrefabsListToDictionary()
        //------------------------------------------------------//
        {

            foreach( GameObject prefab in PrefabsList )
            {
                //Debug.Log ( "LinkPrefabsListToDictionary() prefab = " + prefab.name );
                PrefabsDictionary.Add( prefab.name, prefab );
            }

        } //END LinkPrefabsListToDictionary()

    } //END Class

} //END Namespace