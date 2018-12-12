/* XRCameraManager.cs
 * 
 * Handles the spawning and switching of cameras based on the currently used XRTechnology.
 * 
 * Works in tandem with the bxr_XRTechnologyManager prefab and requires it to exist in the scene
 */
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Sirenix.OdinInspector;
using System.Linq;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace BrandXR
{
    public class XRCameraManager: MonoBehaviour
    {

#if UNITY_EDITOR
        [ShowIf("ShowXRTechnologyMissingWarning"), Button("Create XR Technology Manager"), InfoBox("This script requires that the bxr_XRTechnologyManager prefab exists in the scene as what type of cameras you can create are based on what technology you have selected to use in this application.\n\nClick the button below to create the bxr_XRTechnologyManager now, and be sure to select your technology choices before returning to modify this prefab", InfoMessageType.Error )]
        public void CreateXRTechnologyManager()
        {
            if( GameObject.FindObjectOfType<XRTechnologyManager>() == null )
            {
                GameObject prefab = (GameObject)AssetDatabase.LoadAssetAtPath( "Assets/BrandXR/Prefabs/bxr_XRTechnologyManager.prefab", typeof( GameObject ) );
                GameObject go = Instantiate( prefab );

                go.name = "bxr_XRTechnologyManager";
                BlockHelper.AddToBrandXRTechParent( go.transform );
            }
        }
#endif

        //-----------------------------------------------//
        private bool ShowXRTechnologyMissingWarning()
        //-----------------------------------------------//
        {

            if( GameObject.FindObjectOfType<XRTechnologyManager>() == null ) { return true; }

            return false;

        } //END ShowXRTechnologyMissingWarning




        // singleton behavior
        private static XRCameraManager _instance;

        //--------------------------------------------//
        public static XRCameraManager instance
        //--------------------------------------------//
        {
            get
            {
                if( _instance == null )
                {
                    if( GameObject.FindObjectOfType<XRCameraManager>() == null ) { PrefabManager.InstantiatePrefab( PrefabFactory.Prefabs.bxr_XRCameraManager, null ); }
                    _instance = GameObject.FindObjectOfType<XRCameraManager>();
                    BlockHelper.AddToBrandXRTechParent( _instance.transform );
                }

                return _instance;
            }

        } //END Instance


#if VUFORIA
        [FoldoutGroup( "Hooks" )]
        public GameObject _vuforiaCamera;
#endif
        [FoldoutGroup( "Hooks" )]
        public GameObject _2DCamera;
        [FoldoutGroup( "Hooks" )]
        public GameObject _vrCardboardCamera;
        [FoldoutGroup( "Hooks" )]
        public GameObject _Stereo3DCamera;

        //Reference to the currently existing XRCamera prefab
        private XRCamera xrCamera = null;
        public XRCamera GetXRCamera() { return xrCamera; }


        public enum BasicCameraType
        {
            twoDimensionalCamera,
            stereoThreeDimensionalCamera
        }

        [ShowIf("ShouldShowBasicCameraType"), FoldoutGroup("Create Camera")]
        public BasicCameraType cameraType = BasicCameraType.twoDimensionalCamera;

        //--------------------------------------------//
        private bool ShouldShowBasicCameraType()
        //--------------------------------------------//
        {
            //If the XRTechnologyManager does not exist, we cannot show the "Create Camera" system
            if( GameObject.FindObjectOfType<XRTechnologyManager>() == null ) { return false; }

            //If the XRTechnologyManager has it's type set to 'None', we can only show the basic cameras
            if( GameObject.FindObjectOfType<XRTechnologyManager>() != null &&
                GameObject.FindObjectOfType<XRTechnologyManager>().xrTechnologyType == XRTechnologyManager.XRTechnologyType.None )
            {
                return true;
            }

            return false;

        } //END ShouldShowBasicCameraType






        public enum XRCameraType
        {
            vuforiaCamera
        }

        [ShowIf("ShouldShowXRCameraType"), FoldoutGroup("Create Camera")]
        public XRCameraType xrCameraType = XRCameraType.vuforiaCamera;

        //--------------------------------------------//
        private bool ShouldShowXRCameraType()
        //--------------------------------------------//
        {
            //If the XRTechnologyManager does not exist, we cannot show the "Create Camera" system
            if( GameObject.FindObjectOfType<XRTechnologyManager>() == null ) { return false; }

            //If the XRTechnologyManager has it's type set to 'Vuforia' or 'VuforiaFusion', we can show the XRCameras
            if( GameObject.FindObjectOfType<XRTechnologyManager>() != null &&
                ( GameObject.FindObjectOfType<XRTechnologyManager>().xrTechnologyType == XRTechnologyManager.XRTechnologyType.Vuforia ||
                  GameObject.FindObjectOfType<XRTechnologyManager>().xrTechnologyType == XRTechnologyManager.XRTechnologyType.VuforiaFusion ) )
            {
                return true;
            }

            return false;

        } //END ShouldShowXRCameraType





        public enum ARCoreAndARKitCameraType
        {
            twoDimensionalCamera
        }

        [ShowIf( "ShouldShowARCameraType" ), FoldoutGroup( "Create Camera" )]
        public ARCoreAndARKitCameraType arCameraType = ARCoreAndARKitCameraType.twoDimensionalCamera;

        //--------------------------------------------//
        private bool ShouldShowARCameraType()
        //--------------------------------------------//
        {
            //If the XRTechnologyManager does not exist, we cannot show the "Create Camera" system
            if( GameObject.FindObjectOfType<XRTechnologyManager>() == null ) { return false; }

            //If the XRTechnologyManager has it's type set to 'Vuforia' or 'VuforiaFusion', we can show the XRCameras
            if( GameObject.FindObjectOfType<XRTechnologyManager>() != null &&
                ( GameObject.FindObjectOfType<XRTechnologyManager>().xrTechnologyType == XRTechnologyManager.XRTechnologyType.ARCore ||
                  GameObject.FindObjectOfType<XRTechnologyManager>().xrTechnologyType == XRTechnologyManager.XRTechnologyType.ARKit ||
                  GameObject.FindObjectOfType<XRTechnologyManager>().xrTechnologyType == XRTechnologyManager.XRTechnologyType.ARCore_ARKit ) )
            {
                return true;
            }

            return false;

        } //END ShouldShowARCameraType





        [ShowIf( "ShowCreateCamera" ), FoldoutGroup("Create Camera")]
        public bool destroyExistingCameras = true;

        //-----------------------------//
        public void Awake()
        //-----------------------------//
        {
            CheckIfXRCameraExists();
            
        } //END Awake

        //-------------------------------//
        private void CheckIfXRCameraExists()
        //-------------------------------//
        {

            if( xrCamera == null && GetComponentInChildren<XRCamera>() != null )
            {
                xrCamera = GetComponentInChildren<XRCamera>();
            }

        } //END CheckIfXRCameraExists




        [ShowIf("ShowCreateCamera"), Button("Create Camera", ButtonSizes.Large), FoldoutGroup( "Create Camera" )]
        //----------------------------------------//
        public void CreateCamera()
        //----------------------------------------//
        {
            //Check if we should remove any existing cameras
            if( destroyExistingCameras )
            {
                DestroyCurrentCameras();
            }

            //Instantiate a copy of the camera reference
            GameObject prefab = GetCameraPrefab();
            GameObject go = Instantiate( prefab, transform );
            go.name = prefab.name;

            xrCamera = go.GetComponent<XRCamera>();

        } //END CreateCamera

        //----------------------------------------//
        private GameObject GetCameraPrefab()
        //----------------------------------------//
        {
            //Default to a standard 2D camera
            GameObject cameraPrefab = _2DCamera;

            //Use the XRTechnologyManager to determine what camera type to use
            if( GameObject.FindObjectOfType<XRTechnologyManager>() != null )
            {
                XRTechnologyManager xrTechnologyManager = GameObject.FindObjectOfType<XRTechnologyManager>();

                if( xrTechnologyManager.xrTechnologyType == XRTechnologyManager.XRTechnologyType.None )
                {
                    //If the user chooses to use the Stereo 3D camera, use that instead of the 2D camera
                    if( cameraType == BasicCameraType.stereoThreeDimensionalCamera )
                    {
                        cameraPrefab = _Stereo3DCamera;
                    }
                }
                else if( xrTechnologyManager.xrTechnologyType == XRTechnologyManager.XRTechnologyType.Vuforia ||
                         xrTechnologyManager.xrTechnologyType == XRTechnologyManager.XRTechnologyType.VuforiaFusion )
                {
#if VUFORIA
                    if( _vuforiaCamera != null )
                    {
                        cameraPrefab = _vuforiaCamera;
                    }
                    else
                    {
#if UNITY_EDITOR
                        cameraPrefab = (GameObject)AssetDatabase.LoadAssetAtPath( "Assets/BrandXR/Prefabs/Cameras/bxr_VuforiaCamera.prefab", typeof( GameObject ) );
#endif
                    }
#else
                    cameraPrefab = _2DCamera;
#endif
                }
                else if( xrTechnologyManager.xrTechnologyType == XRTechnologyManager.XRTechnologyType.ARCore ||
                         xrTechnologyManager.xrTechnologyType == XRTechnologyManager.XRTechnologyType.ARKit ||
                         xrTechnologyManager.xrTechnologyType == XRTechnologyManager.XRTechnologyType.ARCore_ARKit )
                {
                    if( arCameraType == ARCoreAndARKitCameraType.twoDimensionalCamera )
                    {
                        cameraPrefab = _2DCamera;
                    }
                }
            }

            return cameraPrefab;

        } //END GetCameraPrefab

        //----------------------------------------//
        private bool ShowCreateCamera()
        //----------------------------------------//
        {

            //If the XRTechnologyManager does not exist, we cannot show the "Create Camera" system
            if( GameObject.FindObjectOfType<XRTechnologyManager>() == null ) { return false; }

            return true;

        } //END ShowCreateCamera








        


        //-----------------------------------//
        public void DestroyCurrentCameras()
        //-----------------------------------//
        {

            if( GameObject.FindObjectOfType<Camera>() != null )
            {
                List<Camera> cameras = GameObject.FindObjectsOfType<Camera>().ToList();

                if( cameras != null && cameras.Count > 0 )
                {
                    foreach( Camera cam in cameras )
                    {
                        if( cam != null )
                        {
#if UNITY_EDITOR
                            //Wait a moment before calling DestroyImmediate to make sure no logic is running
                            UnityEditor.EditorApplication.delayCall += () =>
                            {
                                DestroyImmediate( cam.gameObject );
                            };
#else
                            Destroy( cam.gameObject );
#endif
                        }
                    }
                }
            }

        } //END DestroyCurrentCamera



        //---------------------------------//
        public void DisableCamera()
        //---------------------------------//
        {

            if( xrCamera != null )
            {
                xrCamera.DisableCamera();
            }

        } //END DisableCamera

        //---------------------------------//
        public void EnableCamera()
        //---------------------------------//
        {

            if( xrCamera != null )
            {
                xrCamera.EnableCamera();
            }

        } //END EnableCamera



        //---------------------------------//
        public void EnableFloorTrackerIcon()
        //---------------------------------//
        {

            xrCamera.EnableFloorTrackerIcon();

        } //END EnableFloorTrackerIcon

        //---------------------------------//
        public void DisableFloorTrackerIcon()
        //---------------------------------//
        {

            xrCamera.DisableFloorTrackerIcon();

        } //END DisableFloorTrackerIcon

        //---------------------------------//
        public void SetFloorTrackerIconTexture( Texture newFloorIconTexture )
        //---------------------------------//
        {

            xrCamera.SetFloorTrackerIconTexture( newFloorIconTexture );

        } //END SetFloorTrackerIconTexture


    } //END Class

} //END Namespace