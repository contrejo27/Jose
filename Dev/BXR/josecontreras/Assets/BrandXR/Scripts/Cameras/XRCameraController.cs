using Sirenix.OdinInspector;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.VR;

namespace BrandXR
{
    public class XRCameraController: MonoBehaviour
    {

        private Transform cameraGrandparent;
        private Transform cameraParent;
        private Camera mainCamera;

        [InfoBox( "If gyro is enabled, Mouse or Touch input is limited to horizontal movement" )]
        public List<XRCameraControlHelper> platforms = new List<XRCameraControlHelper>()
        {
            new XRCameraControlHelper( RuntimePlatform.WindowsEditor, XRCameraControlHelper.GyroOptions.Off, XRCameraControlHelper.MouseOptions.On, new KeyCode[] { KeyCode.LeftAlt, KeyCode.RightAlt, KeyCode.LeftCommand, KeyCode.RightCommand }, XRCameraControlHelper.TouchOptions.Off ),
            new XRCameraControlHelper( RuntimePlatform.OSXEditor,     XRCameraControlHelper.GyroOptions.Off, XRCameraControlHelper.MouseOptions.On, new KeyCode[] { KeyCode.LeftAlt, KeyCode.RightAlt, KeyCode.LeftCommand, KeyCode.RightCommand }, XRCameraControlHelper.TouchOptions.Off ),
            new XRCameraControlHelper( RuntimePlatform.LinuxEditor,   XRCameraControlHelper.GyroOptions.Off, XRCameraControlHelper.MouseOptions.On, new KeyCode[] { KeyCode.LeftAlt, KeyCode.RightAlt, KeyCode.LeftCommand, KeyCode.RightCommand }, XRCameraControlHelper.TouchOptions.Off )
        };

        private XRCameraControlHelper currentPlatform = null;
        XRMode.XRModeHelper.XRDevices currentXRDevice = XRMode.XRModeHelper.XRDevices.none;
        
        public enum InputType
        {
            Mouse,
            Touch
        }

        //----------- GYRO LOGIC (NEW)
        private Quaternion quatMult = Quaternion.identity;
        private Quaternion quatMap = Quaternion.identity;
        
        
        //----------- CAMERA ORIENTATION CHANGES
        ScreenOrientation currentOrientation = ScreenOrientation.Unknown;
        int currentScreenWidth = 0;
        int currentScreenHeight = 0;

        //----------- MOUSE & TOUCH INPUT VALUES
        //Created originally by FatiguedArtist on the Unity Forums, modified for mouse + touch input
        //https://forum.unity3d.com/threads/a-free-simple-smooth-mouselook.73117/

        private Vector2 _inputAbsolute;
        private Vector2 _smoothedInput;

        private Vector2 clampInDegrees = new Vector2( 360, 180 );
        private Vector2 targetDirection;

        private Vector2 mouseSensitivity = new Vector2( 2, 2 );
        private Vector2 mouseSmoothing = new Vector2( 3, 3 );

        private Vector2 touchSensitivity = new Vector2( .5f, .5f );
        private Vector2 touchSmoothing = new Vector2( .5f, .5f );


        //---------------------------------//
        public void Awake()
        //---------------------------------//
        {

            //Set the current platform
            if( platforms != null && platforms.Count > 0 )
            {
                foreach( XRCameraControlHelper platform in platforms )
                {
                    if( platform.PlatformType == Application.platform )
                    {
                        currentPlatform = platform;
                        break;
                    }
                }
            }
            
        } //END Awake

        //---------------------------------//
        public void Start()
        //---------------------------------//
        {

            Timer.instance.In( .1f, CallStart, gameObject );
            
        } //END Start

        //---------------------------------//
        private void CallStart()
        //---------------------------------//
        {
            
            //Find the current XR device
            XRMode.GetCurrentXRDevice( out currentXRDevice );

            //Set the current Screen Orientation
            currentOrientation = Screen.orientation;
            
        } //END CallStart
        

        //---------------------------------//
        private void SetGyroRotationBasedOnCameraOrientation()
        //---------------------------------//
        {
            //Currently using settings found by aroha (Post #82) https://forum.unity3d.com/threads/sharing-gyroscope-controlled-camera-on-iphone-4.98828/page-2
            if( cameraParent != null && currentPlatform.IsGyroEnabled() )
            {
                if( Screen.orientation == ScreenOrientation.LandscapeLeft )
                {
                    cameraParent.transform.eulerAngles = new Vector3( 90, 180, 0 );
                    quatMult = new Quaternion( 0, 0, 1, 0 );
                }
                else if( Screen.orientation == ScreenOrientation.Portrait )
                {
                    cameraParent.transform.eulerAngles = new Vector3( 90, 180, 0 );
                    quatMult = new Quaternion( 0, 0, 1, 0 );
                }
                else if( Screen.orientation == ScreenOrientation.PortraitUpsideDown )
                {
                    cameraParent.transform.eulerAngles = new Vector3( 90, 180, 0 );
                    quatMult = new Quaternion( 0, 0, 1, 0 );
                }
                else if( Screen.orientation == ScreenOrientation.LandscapeRight )
                {
                    cameraParent.transform.eulerAngles = new Vector3( 90, 180, 0 );
                    quatMult = new Quaternion( 0, 0, 1, 0 );
                }
                else
                {
                    cameraParent.transform.eulerAngles = new Vector3( 90, 180, 0 );
                    quatMult = new Quaternion( 0, 0, 1, 0 );
                }
            }

        } //END SetGyroRotationBasedOnCameraOrientation

        //---------------------------------//
        private void SetMainCamera()
        //---------------------------------//
        {

            //Set the main camera
            if( mainCamera == null && Camera.main != null )
            {
                mainCamera = Camera.main;

                CreateCameraParent();

                CreateCameraGrandparent();

                //Parent camera to CameraParent
                mainCamera.transform.SetParent( cameraParent );

                //Child CameraParent to CameraGrandparent
                cameraParent.SetParent( cameraGrandparent );
                
                //Reset all values
                cameraGrandparent.localPosition = Vector3.zero;
                cameraGrandparent.localEulerAngles = Vector3.zero;
                cameraGrandparent.localScale = Vector3.one;

                cameraParent.localPosition = Vector3.zero;
                cameraParent.localEulerAngles = Vector3.zero;
                cameraParent.localScale = Vector3.one;

                mainCamera.transform.localPosition = Vector3.zero;
                mainCamera.transform.localEulerAngles = Vector3.zero;
                mainCamera.transform.localScale = Vector3.one;
                
                // Set target direction to the camera's initial orientation.
                targetDirection = mainCamera.transform.localRotation.eulerAngles;

                //Set the initial screen width/height
                currentScreenWidth = Screen.width;
                currentScreenHeight = Screen.height;
                
                //Set initial settings for gyro
                SetGyroRotationBasedOnCameraOrientation();
                

                //Debug.Log( "SetMainCamera() end CameraParent.localEulerAngles = " + cameraParent.localEulerAngles );
            }

        } //END SetMainCamera
        

        //------------------------------------------//
        private void CreateCameraParent()
        //------------------------------------------//
        {

            //Add parent gameObject to mainCamera if none exists
            if( mainCamera.transform.parent == null )
            {
                //Create camera
                cameraParent = new GameObject().transform;
                cameraParent.name = "Camera Parent";
            }
            else
            {
                //Capture camera parent, reset values
                cameraParent = mainCamera.transform.parent;
            }

        } //END CreateCameraParent

        //------------------------------------------//
        private void CreateCameraGrandparent()
        //------------------------------------------//
        {

            //Create a granparent object to the mainCamera if one doesn't exist
            if( cameraParent.parent == null )
            {
                //Create Grandparent Camera
                cameraGrandparent = new GameObject().transform;
                cameraGrandparent.name = "Camera Grandparent";
            }
            else
            {
                cameraGrandparent = cameraParent.parent;
            }

        } //END CreateCameraGrandparent


        //------------------------------------------//
        public void Update()
        //------------------------------------------//
        {

            CheckForCameraOrientationRatioOrSizeChange();

            if( mainCamera == null ) { SetMainCamera(); }

            if( mainCamera != null && cameraParent != null && cameraGrandparent != null && currentPlatform != null )
            {
                MoveWithMouseOrTouch();
                MoveCameraWithGyro();
            }

            //Debug.Log( cameraGrandparent.name + " " + cameraGrandparent.localEulerAngles + ", " + cameraParent.name + " " + cameraParent.localEulerAngles + ", " + mainCamera.name + " = " + mainCamera.transform.localEulerAngles );

        } //END Update

        //---------------------------------//
        private void CheckForCameraOrientationRatioOrSizeChange()
        //---------------------------------//
        {

            if( currentOrientation != Screen.orientation || currentScreenWidth != Screen.width || currentScreenHeight != Screen.height )
            {
                currentOrientation = Screen.orientation;
                currentScreenWidth = Screen.width;
                currentScreenHeight = Screen.height;
                
                ResetCameraSettings();
                SetGyroRotationBasedOnCameraOrientation();

                //Set the field of view based on the platform
                if( currentPlatform != null )
                {
                    if( currentOrientation == ScreenOrientation.LandscapeLeft || currentOrientation == ScreenOrientation.LandscapeRight )
                    {
                        Camera.main.fieldOfView = currentPlatform.landscapeFieldOfView;
                    }
                    else if( currentOrientation == ScreenOrientation.Portrait || currentOrientation == ScreenOrientation.PortraitUpsideDown )
                    {
                        Camera.main.fieldOfView = currentPlatform.portraitFieldOfView;
                    }
                }
            }

        } //END CheckForCameraOrientationRatioOrSizeChange

        //---------------------------------//
        private void ResetCameraSettings()
        //---------------------------------//
        {
            //Debug.Log( "ResetCameraSettings" );

            if( Camera.allCameras != null )
            {
                foreach( Camera item in Camera.allCameras )
                {
                    item.ResetAspect();
                }
            }

        } //END ResetCameraSettings

        /*
        //---------------------------------//
        public void RecenterGyro()
        //---------------------------------//
        {
            
            if( SystemInfo.supportsGyroscope && Input.gyro.enabled )
            {
                Input.gyro.enabled = false;
                Timer.instance.In( .01f, _RecenterGyro );
            }

        } //END RecenterGyro

        //---------------------------------//
        private void _RecenterGyro()
        //---------------------------------//
        {

            if( SystemInfo.supportsGyroscope && !Input.gyro.enabled )
            {
                Input.gyro.enabled = true;
            }

        } //END _RecenterGyro
        */

        //---------------------------------//
        private void MoveCameraWithGyro()
        //---------------------------------//
        {
            if( currentPlatform.IsGyroEnabled() && SystemInfo.supportsGyroscope )
            {
                //If not using a XR Device or simply splitting the camera renderer in 1/2, then we need to use our own gyroscope logic
                //If any other XR device, then the gyroscope logic will come from Unity automatically applied to the camera
                if( currentXRDevice == XRMode.XRModeHelper.XRDevices.none || currentXRDevice == XRMode.XRModeHelper.XRDevices.split )
                {
                    if( !Input.gyro.enabled ) { AttachGyro(); }

                    MoveCameraWithGyroUsingQuaternionMap();
                }
            }

        } //END MoveCameraWithGyro
        


        //---------------------------------//
        //Logic from Post #82 https://forum.unity3d.com/threads/sharing-gyroscope-controlled-camera-on-iphone-4.98828/page-2
        private void MoveCameraWithGyroUsingQuaternionMap()
        //---------------------------------//
        {

            if( Application.platform == RuntimePlatform.IPhonePlayer )
            {
                quatMap = Input.gyro.attitude;
            }
            else if( Application.platform == RuntimePlatform.Android )
            {
                quatMap = new Quaternion( Input.gyro.attitude.x, Input.gyro.attitude.y, Input.gyro.attitude.z, Input.gyro.attitude.w ); //Version from Post #82
            }

            //Version from Pos #82
            mainCamera.transform.localRotation = quatMap * quatMult;

        } //END MoveCameraWithGyroUsingQuaternionMap


        //---------------------------------//
        private void AttachGyro()
        //---------------------------------//
        {
            if( !SystemInfo.supportsGyroscope )
                return;

            Input.gyro.enabled = true;

        } //END AttachGyro

        //---------------------------------//
        private void DetachGyro()
        //---------------------------------//
        {
            if( !SystemInfo.supportsGyroscope )
                return;

            Input.gyro.enabled = false;

        } //END DetachGyro
        

        //---------------------------------//
        private void MoveWithMouseOrTouch()
        //---------------------------------//
        {

            if( currentPlatform.IsMouseEnabled() && Input.mousePresent )
            {
                if( currentPlatform.mouseRequiresOneOfTheseKeysHeldToUse != null && currentPlatform.mouseRequiresOneOfTheseKeysHeldToUse.Length > 0 )
                {
                    //If any of the required keys are being held down, move the camera
                    foreach( KeyCode key in currentPlatform.mouseRequiresOneOfTheseKeysHeldToUse )
                    {
                        if( Input.GetKey( key ) )
                        {
                            MoveViaMouseOrTouchInput( InputType.Mouse, new Vector2( Input.GetAxisRaw( "Mouse X" ), Input.GetAxisRaw( "Mouse Y" ) ), mouseSensitivity, mouseSmoothing );
                            break;
                        }
                    }
                }
                else
                {
                    //We don't require any keyboard keys to be held, go straight to performing the calculation
                    MoveViaMouseOrTouchInput( InputType.Mouse, new Vector2( Input.GetAxisRaw( "Mouse X" ), Input.GetAxisRaw( "Mouse Y" ) ), mouseSensitivity, mouseSmoothing );
                }
            }
            else if( currentPlatform.IsTouchEnabled() && Input.touchSupported && Input.touchCount > 0 && Input.GetTouch(0).phase == TouchPhase.Moved )
            {
                //We have drag input! Move the camera
                MoveViaMouseOrTouchInput( InputType.Touch, Input.touches[ 0 ].deltaPosition, touchSensitivity, touchSmoothing );
            }

        } //END MoveWithMouseOrTouch

        //---------------------------------//
        private void MoveViaMouseOrTouchInput( InputType inputType, Vector2 moveDelta, Vector2 sensitivity, Vector2 smoothing )
        //---------------------------------//
        {

            // Allow the script to clamp based on a desired target value.
            var targetOrientation = Quaternion.Euler( targetDirection );

            // Scale input against the sensitivity setting and multiply that against the smoothing value.
            moveDelta = Vector2.Scale( moveDelta, new Vector2( sensitivity.x * smoothing.x, sensitivity.y * smoothing.y ) );

            // Interpolate mouse movement over time to apply smoothing delta.
            _smoothedInput.x = Mathf.Lerp( _smoothedInput.x, moveDelta.x, 1f / smoothing.x );
            _smoothedInput.y = Mathf.Lerp( _smoothedInput.y, moveDelta.y, 1f / smoothing.y );

            // Find the absolute mouse movement value from point zero.
            _inputAbsolute += _smoothedInput;

            // Clamp and apply the local x value first, so as not to be affected by world transforms.
            if( clampInDegrees.x < 360 )
                _inputAbsolute.x = Mathf.Clamp( _inputAbsolute.x, -clampInDegrees.x * 0.5f, clampInDegrees.x * 0.5f );

            // Then clamp and apply the global y value.
            if( clampInDegrees.y < 360 )
                _inputAbsolute.y = Mathf.Clamp( _inputAbsolute.y, -clampInDegrees.y * 0.5f, clampInDegrees.y * 0.5f );


            
            //If gyro is being used, only allow horizontal movement
            if( currentPlatform.IsGyroEnabled() && SystemInfo.supportsGyroscope && Input.gyro.enabled )
            {
                if ( ( inputType == InputType.Mouse && currentPlatform.mouseOptions == XRCameraControlHelper.MouseOptions.VerticalOnly ) || 
                     ( inputType == InputType.Touch && currentPlatform.touchOptions == XRCameraControlHelper.TouchOptions.VerticalOnly ) )
                {
                    return;
                }
                else if( ( inputType == InputType.Mouse && currentPlatform.mouseOptions == XRCameraControlHelper.MouseOptions.On ) ||
                         ( inputType == InputType.Touch && currentPlatform.touchOptions == XRCameraControlHelper.TouchOptions.On ) ||
                         ( inputType == InputType.Mouse && currentPlatform.mouseOptions == XRCameraControlHelper.MouseOptions.HorizontalOnly ) ||
                         ( inputType == InputType.Touch && currentPlatform.touchOptions == XRCameraControlHelper.TouchOptions.HorizontalOnly ) )
                {
                    //Horizontal
                    cameraGrandparent.localRotation = Quaternion.AngleAxis( _inputAbsolute.x, cameraGrandparent.InverseTransformDirection( Vector3.up ) );
                    return;
                }
                else
                {
                    return;
                }
            }
            
            //If gyro is not enabled...
            if( ( inputType == InputType.Mouse && currentPlatform.mouseOptions == XRCameraControlHelper.MouseOptions.HorizontalOnly ) ||
                ( inputType == InputType.Touch && currentPlatform.touchOptions == XRCameraControlHelper.TouchOptions.HorizontalOnly ) )
            {
                //Horizontal
                cameraGrandparent.localRotation = Quaternion.AngleAxis( _inputAbsolute.x, cameraGrandparent.InverseTransformDirection( Vector3.up ) );
            }
            else if( ( inputType == InputType.Mouse && currentPlatform.mouseOptions == XRCameraControlHelper.MouseOptions.VerticalOnly ) ||
                     ( inputType == InputType.Touch && currentPlatform.touchOptions == XRCameraControlHelper.TouchOptions.VerticalOnly ) )
            {
                //Vertical
                cameraGrandparent.localRotation = Quaternion.AngleAxis( -_inputAbsolute.y, targetOrientation * Vector3.right ) * targetOrientation;
            }
            else
            {
                //Vertical
                cameraGrandparent.localRotation = Quaternion.AngleAxis( -_inputAbsolute.y, targetOrientation * Vector3.right ) * targetOrientation;

                //Horizontal
                cameraGrandparent.localRotation *= Quaternion.AngleAxis( _inputAbsolute.x, cameraGrandparent.InverseTransformDirection( Vector3.up ) );
            }

        } //END MoveViaMouseOrTouchInput
        



        //------------------------------------//
        public void SetGyroOptions( XRCameraControlHelper.GyroOptions options )
        //------------------------------------//
        {
            if( currentPlatform == null ) return;

            currentPlatform.gyroOptions = options;

        } //END SetGyroOptions

        //------------------------------------//
        public void SetMouseOptions( XRCameraControlHelper.MouseOptions options )
        //------------------------------------//
        {
            if( currentPlatform == null ) return;

            currentPlatform.mouseOptions = options;

        } //END SetMouseOptions

        //------------------------------------//
        public void SetMouseRequiredKeys( KeyCode[] keys )
        //------------------------------------//
        {
            if( currentPlatform == null ) return;

            currentPlatform.mouseRequiresOneOfTheseKeysHeldToUse = keys;

        } //END SetMouseRequiredKeys

        //------------------------------------//
        public void SetTouchOptions( XRCameraControlHelper.TouchOptions options )
        //------------------------------------//
        {
            if( currentPlatform == null ) return;

            currentPlatform.touchOptions = options;

        } //END SetTouchOptions

    } //END Class

} //END Namespace
                 