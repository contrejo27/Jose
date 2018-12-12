using Sirenix.OdinInspector;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.VR;

namespace BrandXR
{
    public class XRMode: MonoBehaviour
    {
        private bool showDebug = false;

        [System.Serializable]
        public class XRModeHelper
        {
            [Tooltip( "Select the platform that you want to be affected" )]
            public RuntimePlatform PlatformType = RuntimePlatform.WindowsEditor;

            public enum XRDevices
            {
                none,
                split,
                cardboard,
                daydream,
                oculus,
                openvr,
                hololens
            }
            
            public XRDevices XRDeviceType = XRDevices.none;
            public bool XRModeEnabled = false;

            public bool useGazeInput = false;
            public bool useMouseInput = false;
            public bool useTouchInput = false;

            //Constructor
            public XRModeHelper( RuntimePlatform PlatformType, XRDevices XRDeviceType, bool XRModeEnabled,
                bool useGazeInput, bool useMouseInput, bool useTouchInput )
            {
                this.PlatformType = PlatformType;
                this.XRDeviceType = XRDeviceType;
                this.XRModeEnabled = XRModeEnabled;
                this.useGazeInput = useGazeInput;
                this.useMouseInput = useMouseInput;
                this.useTouchInput = useTouchInput;
            }
        }

        
        [SerializeField]
        List<XRModeHelper> XRModeSettings = new List<XRModeHelper>()
        {
            new XRModeHelper( RuntimePlatform.WindowsEditor, XRModeHelper.XRDevices.none, false, false, true, false ),
            new XRModeHelper( RuntimePlatform.OSXEditor, XRModeHelper.XRDevices.none, false, false, true, false ),
            new XRModeHelper( RuntimePlatform.LinuxEditor, XRModeHelper.XRDevices.none, false, false, true, false )
        };
        private XRModeHelper currentHelper = null;





        [System.Serializable]
        public class XRCameraBarrelDistortionHelper
        {
            [Tooltip( "Select the platform that you want to be affected" )]
            public RuntimePlatform PlatformType = RuntimePlatform.WindowsEditor;

            [Tooltip( "Select the XR device you want to use barrel distortion" )]
            public XRMode.XRModeHelper.XRDevices device = XRMode.XRModeHelper.XRDevices.none;

            public bool enableDistortion = false;
            public Vector2 strength = new Vector2( 1f, 1f );
            public bool showCenterLine = true;

            //Constructor
            //--------------------------------------------------------//
            public XRCameraBarrelDistortionHelper( RuntimePlatform PlatformType, XRMode.XRModeHelper.XRDevices device, bool enableDistortion, Vector2 strength, bool showCenterLine )
            //--------------------------------------------------------//
            {
                this.PlatformType = PlatformType;
                this.device = device;
                this.enableDistortion = enableDistortion;
                this.strength = strength;
                this.showCenterLine = showCenterLine;

            } //END XRCameraBarrelDistortionHelper

        }

        [SerializeField]
        [InfoBox( "Use this to enable camera distortion that fixes the distortion introduced by the lens on VR headsets. Hurts app performance by a small amount" )]
        public List<XRCameraBarrelDistortionHelper> FixLensDistortionSettings = new List<XRCameraBarrelDistortionHelper>()
        {
            new XRCameraBarrelDistortionHelper( RuntimePlatform.Android, XRMode.XRModeHelper.XRDevices.split, true, new Vector2( 1f, 1f ), true ),
            new XRCameraBarrelDistortionHelper( RuntimePlatform.IPhonePlayer, XRMode.XRModeHelper.XRDevices.split, true, new Vector2( 1f, 1f ), true )
        };
        

        [Space( 10f )]
        public Shader BarrelDistortionShader = null;






        //Singleton behavior
        private static XRMode _instance;

        //--------------------------------------------//
        public static XRMode instance
        //--------------------------------------------//
        {
            get
            {
                if( _instance == null )
                {
                    _instance = GameObject.FindObjectOfType<XRMode>();
                }

                return _instance;
            }

        } //END Instance

        //--------------------------------------------//
        public void Awake()
        //--------------------------------------------//
        {

            DestroyDuplicateInstance();
            
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

        //--------------------------------------//
        public void Start()
        //--------------------------------------//
        {
            if( showDebug ) Debug.Log( "XRMode.cs Start()" );

            SetCurrentXRModeHelper();
            
            //Add XRCameraBarrelDistortion script to the camera
            AddBarrelDistortionScriptToCamera();
            
            //Timer.instance.In( 5f, CallStart );
            CallStart();

        } //END Start

        //--------------------------------------//
        private void SetCurrentXRModeHelper()
        //--------------------------------------//
        {

            if( XRModeSettings != null && XRModeSettings.Count > 0 )
            {
                foreach( XRModeHelper helper in XRModeSettings )
                {
                    if( helper.PlatformType == Application.platform )
                    {
                        if( showDebug ) Debug.Log( "XRMode.cs SetCurrentXRModeHelper() found helper for Platform = " + Application.platform );
                        currentHelper = helper;
                        return;
                    }
                    else
                    {
                        if( showDebug ) Debug.Log( "XRMode.cs SetCurrentXRModeHelper() helper.Platform( " + helper.PlatformType + " ) != " + Application.platform );
                    }
                }
            }

            currentHelper = new XRModeHelper( Application.platform, XRModeHelper.XRDevices.none, false, false, true, true );

        } //END SetCurrentXRModeHelper
        
        //--------------------------------------//
        private void AddBarrelDistortionScriptToCamera()
        //--------------------------------------//
        {

            if( Camera.main != null )
            {
                if( XRCameraBarrelDistortion.instance == null && BarrelDistortionShader != null )
                {
                    Camera.main.gameObject.AddComponent<XRCameraBarrelDistortion>();
                    XRCameraBarrelDistortion.instance.enableDistortion = false;
                    XRCameraBarrelDistortion.instance.SetBarrelShader( ref BarrelDistortionShader );
                }

                if( PrefabManager.instance != null )
                {
                    XRCameraBarrelDistortionLine splitLine = PrefabManager.InstantiatePrefab( PrefabFactory.Prefabs.bxr_XRBarrelDistortionLine ).GetComponent<XRCameraBarrelDistortionLine>();
                    splitLine.transform.SetParent( Camera.main.transform );
                    splitLine.SetDistortionLine( false );
                }
            }

        } //END AddBarrelDistortionScriptToCamera

        //--------------------------------------//
        public void CallStart()
        //--------------------------------------//
        {

            if( currentHelper != null )
            {
                if( showDebug )Debug.Log( "XRMode.cs CallStart() calling SetXRMode(), XRDeviceType = " + currentHelper.XRDeviceType.ToString() + ", XRModeEnabled = " + currentHelper.XRModeEnabled );
                SetXRMode( currentHelper.XRDeviceType.ToString(), currentHelper.XRModeEnabled, currentHelper.useGazeInput, currentHelper.useMouseInput, currentHelper.useTouchInput );
            }
            else
            {
                if( showDebug ) Debug.Log( "XRMode.cs CallStart() unable to call SetXRMode because currentHelper is null!" );
            }
            
            //Enable gaze input if requested by the currentXRDevice
            if( IsGazeInputEnabled() )
            {
                if( XRGazeInputModule.instance == null )
                {
                    gameObject.AddComponent<XRGazeInputModule>();
                }
            }
            else if( XRGazeInputModule.instance != null )
            {
                if( gameObject.GetComponent<XRGazeInputModule>() != null )
                {
                    #if UNITY_EDITOR
                        //Wait a moment before calling DestroyImmediate to make sure no logic is running
                        UnityEditor.EditorApplication.delayCall+=()=>
                        {
                            DestroyImmediate( gameObject.GetComponent<XRGazeInputModule>() );
                        };
                    #else
                        Destroy( gameObject.GetComponent<XRGazeInputModule>() );
                    #endif
                }
            }

        } //END CallStart

        //-------------------------------------//
        public void SetXRMode( bool enabled )
        //-------------------------------------//
        {

            if( enabled ) { SetXRMode( XRModeHelper.XRDevices.split.ToString(), true, true, false, false ); }
            else { SetXRMode( XRModeHelper.XRDevices.none.ToString(), false, false, false, false ); }
            
        } //END SetVRMode

        //----------------------------------//
        private void SetXRMode( string newDevice, bool vrModeActive, bool useGazeInput, bool useMouseInput, bool useTouchInput )
        //----------------------------------//
        {

            StartCoroutine( LoadDevice( newDevice, vrModeActive, useGazeInput, useMouseInput, useTouchInput ) );
            
        } //END _SetVRMode

        //----------------------------------//
        private IEnumerator LoadDevice( string newDevice, bool vrModeActive, bool useGazeInput, bool useMouseInput, bool useTouchInput )
        //----------------------------------//
        {
            //If we're using a VR view, force the view into landscape left to prevent a camera FOV bug
            if( newDevice != "none" && Screen.orientation != ScreenOrientation.LandscapeLeft )
            {
                Screen.orientation = ScreenOrientation.LandscapeLeft;
                yield return new WaitForSeconds(.1f);
                if( showDebug ) Debug.Log( "LoadDevice( split ), Set Screen.orientation to LandscapeLeft" );
            }
            else
            {
                if( showDebug ) Debug.Log( "LoadDevice( " + newDevice + " ) else" );
            }


            if( UnityEngine.XR.XRSettings.loadedDeviceName != newDevice )
            {
                yield return null;
                UnityEngine.XR.XRSettings.LoadDeviceByName( newDevice );
                if( showDebug ) Debug.Log( "XRMode.cs LoadDevice( " + newDevice + " ), VRSettings.enabled = " + vrModeActive );
            }

            yield return null;

            if( newDevice == "none" )
            {
                Screen.autorotateToLandscapeLeft = true;
                Screen.autorotateToLandscapeRight = true;
                Screen.autorotateToPortrait = true;
                Screen.autorotateToPortraitUpsideDown = true;
                Screen.orientation = ScreenOrientation.AutoRotation;
                yield return new WaitForSeconds( .1f );
                if( showDebug ) Debug.Log( "LoadDevice( none ), Set Screen.orientation to Auto" );
            }

            UnityEngine.XR.XRSettings.enabled = vrModeActive;

            //Set the currentHelper to the latest settings
            XRModeHelper.XRDevices deviceType = XRModeHelper.XRDevices.none;

            if( GetCurrentXRDevice( out deviceType ) )
            {
                currentHelper = new XRModeHelper( Application.platform, deviceType, vrModeActive, useGazeInput, useMouseInput, useTouchInput );
            }
            
            //Force cameras to deal with aspect ratio changes
            if( Camera.allCameras != null )
            {
                foreach( Camera item in Camera.allCameras )
                {
                    item.ResetAspect();
                }
            }

            SetDistortionBasedOnSettings();

        } //END LoadDevice
        
        //----------------------------------//
        public static bool IsVRModeOn()
        //----------------------------------//
        {

            return UnityEngine.XR.XRSettings.enabled;

        } //END IsVRModeOn
        
        //----------------------------------//
        public static bool GetCurrentXRDevice( out XRModeHelper.XRDevices currentDevice )
        //----------------------------------//
        {
            XRModeHelper.XRDevices device = default( XRModeHelper.XRDevices );
            string deviceName = UnityEngine.XR.XRSettings.loadedDeviceName;

            if( string.IsNullOrEmpty( UnityEngine.XR.XRSettings.loadedDeviceName ) )
            {
                deviceName = "none";
            }

            foreach( XRModeHelper.XRDevices emn in Enum.GetValues( typeof(XRModeHelper.XRDevices) ) )
            {
                
                if( EnumHelper.TryParseEnum<XRModeHelper.XRDevices>( deviceName, out device ) )
                {
                    currentDevice = device;
                    return true;
                }
            }

            currentDevice = device;
            return false;
            
        } //END GetCurrentXRDevice

        //------------------------------------//
        public bool IsGazeInputEnabled()
        //------------------------------------//
        {
            SetCurrentXRModeHelper();

            if( currentHelper != null )
            {
                return currentHelper.useGazeInput;
            }

            return false;

        } //END IsGazeInputEnabled

        //------------------------------------//
        public bool IsMouseInputEnabled()
        //------------------------------------//
        {
            SetCurrentXRModeHelper();

            if( currentHelper != null )
            {
                return currentHelper.useMouseInput;
            }

            return false;

        } //END IsMouseInputEnabled

        //------------------------------------//
        public bool IsTouchInputEnabled()
        //------------------------------------//
        {
            SetCurrentXRModeHelper();

            if( currentHelper != null )
            {
                return currentHelper.useTouchInput;
            }

            return false;

        } //END IsTouchInputEnabled




        //-----------------------------------//
        public void SetDistortionBasedOnSettings()
        //-----------------------------------//
        {

            //Make sure that the proper scripts exist in the scene
            AddBarrelDistortionScriptToCamera();

            //Find the current XR device, if we're supposed to, then set the barrel settings based on the current XR device
            if( FixLensDistortionSettings != null && FixLensDistortionSettings.Count > 0 && XRMode.GetCurrentXRDevice( out currentHelper.XRDeviceType ) )
            {
                
                foreach( XRCameraBarrelDistortionHelper helper in FixLensDistortionSettings )
                {
                    if( showDebug ) Debug.Log( "SetDistortionBasedOnSettings() helper.PlatformType( " + helper.PlatformType + " ) == Application.Platform( " + Application.platform + ") = " + ( helper.PlatformType == Application.platform ) + ", ......... and helper.device( " + helper.device + " ) == currentHelper.XRDeviceType( " + currentHelper.XRDeviceType + " ) = " + ( helper.device == currentHelper.XRDeviceType ) + ", ....... enableDistortion = " + helper.enableDistortion );

                    if( helper.PlatformType == Application.platform && helper.device == currentHelper.XRDeviceType )
                    {
                        if( XRCameraBarrelDistortion.instance != null )
                        {
                            XRCameraBarrelDistortion.instance.SetCameraDistortionCorrection( helper.enableDistortion, helper.strength );
                        }
                        
                        if( XRCameraBarrelDistortionLine.instance != null )
                        {
                            XRCameraBarrelDistortionLine.instance.SetDistortionLine( helper.showCenterLine );
                        }

                        return;
                    }
                }
            }

            //We didn't find any matching settings, disable distortion and the center line
            if( XRCameraBarrelDistortion.instance != null )
            {
                XRCameraBarrelDistortion.instance.enableDistortion = false;
            }

            if( XRCameraBarrelDistortionLine.instance != null )
            {
                XRCameraBarrelDistortionLine.instance.SetDistortionLine( false );
            }

        } //END SetDistortionBasedOnSettings



    } //END Class

} //END Namespace