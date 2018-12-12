using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using Sirenix.OdinInspector;
using System;
using System.Linq;

#if UNITY_EDITOR
using UnityEditor;
#endif

#if NATCAM
using NatCamU.Core;
#endif

namespace BrandXR
{
    public class BlockEventExternalCamera: BlockEventBase
    {

        //----------------- ACTIONS --------------------------------//
        public enum Actions
        {
            None,
            CapturePhoto,
            ClearPhotos,
            StartRecording,
            StopRecording,
            EnableTorch,
            DisableTorch,
            SwitchCamera
        }

        [TitleGroup( "Block Event - External Camera", "Used to call external camera events. Requires NATCAM scripting define symbol in Project Settings" )]
        public Actions action = Actions.None;
        private bool IsActionCapture() { return action == Actions.CapturePhoto; }
        private bool IsActionClearPhotos() { return action == Actions.ClearPhotos; }
        private bool IsActionRecord() { return action == Actions.StartRecording; }
        private bool IsActionCaptureOrRecord() { return action == Actions.CapturePhoto || action == Actions.StartRecording; }

        //----------------- VARIABLES ------------------------------//
        [Button( "Add Necessary Scripting Define Symbols", ButtonSizes.Large ), ShowIf( "ShowScriptingDefineSymbolWarning" ), InfoBox( "WARNING: This script requires the NATCAM scripting define symbol be defined in your Unity Project Settings and that the NatCam plugin be in your Assets folder to work properly", InfoMessageType.Error )]
        public void AddScriptingDefineSymbols()
        {
#if UNITY_EDITOR && !NATCAM
            string newDefineSymbols = PlayerSettings.GetScriptingDefineSymbolsForGroup
                ( BuildPipeline.GetBuildTargetGroup( EditorUserBuildSettings.activeBuildTarget ) );

            if( !newDefineSymbols.Contains( "NATCAM" ) )
            {
                //Debug.Log( "Define symbols = " + newDefineSymbols );
                newDefineSymbols += ";NATCAM";
            }

            PlayerSettings.SetScriptingDefineSymbolsForGroup( BuildPipeline.GetBuildTargetGroup
                ( EditorUserBuildSettings.activeBuildTarget ), newDefineSymbols );
#endif
        }

        private bool ShowScriptingDefineSymbolWarning()
        {
#if NATCAM && UNITY_EDITOR
            if( AssetDatabase.IsValidFolder( "Assets/NatCam" ) )
            {
                return false;
            }
            else
            {
                return true;
            }
#else
            return true;
#endif
        }

        public enum CameraType
        {
            Front,
            Rear
        }

#if NATCAM
        [Space( 15f ), Tooltip( "Should we use the Front or Rear camera? If our choice is unavailable we will use whatever camera we can find" ), ShowIf( "IsActionCaptureOrRecord" )]
        public CameraType useCameraType = CameraType.Front;


        [Space( 15f ), Tooltip( "Change how the camera focuses" ), ShowIf( "IsActionCaptureOrRecord" )]
        public FocusMode focusMode = FocusMode.AutoFocus;


        [Space( 15f ), Tooltip( "Change how the camera will lighten or darken the recording based on light exposure" ), ShowIf( "IsActionCaptureOrRecord" )]
        public ExposureMode exposureMode = ExposureMode.AutoExpose;


        [Space( 15f ), Tooltip( "Sets the flash on the device camera if available" ), ShowIf( "IsActionCapture" )]
        public FlashMode flashMode = FlashMode.Auto;


        [Space( 15f ), Tooltip( "Max possible framerate is 60" ), ShowIf( "IsActionRecord" ), Range( 0f, 60f )]
        public float framerate = 30f;


        [Space( 15f ), ShowIf( "IsActionCaptureOrRecord" )]
        public float cameraZoom = 1f;


        [Space( 15f ), ShowIf( "IsActionRecord" )]
        public bool enableTorch = false;


        [Space( 15f ), Tooltip( "Should we show the Image/Video on a texture in this application?" ), ShowIf( "IsActionCaptureOrRecord" )]
        public bool sendTextureToColorTweener = false;
        private bool ShowColorTweener() { return ( sendTextureToColorTweener && IsActionCaptureOrRecord() ); }

        [Tooltip( "The color tweener to show the preview on" ), ShowIf( "ShowColorTweener" )]
        public UIColorTweener colorTweener;

        [Space( 15f ), ShowIf( "IsActionCapture" ), InfoBox( "WARNING: Captured Photos are not automatically garbage collected, to clear up memory use the ClearPhotos action on another BlockEvent and link it to this script. Or Call ClearCapturedPhotos() on this script." )]
        public List<Texture2D> capturedPhotos = new List<Texture2D>();


        public enum ResolutionTypes
        {
            _640x480,
            _1280x720,
            _1920x1080,
            custom,
            lowest,
            highest
        }
        [Space( 15f ), Tooltip( "Changes the resolution the camera captures at. Choose highest for the best quality possible on the device" ), ShowIf( "IsActionCaptureOrRecord" )]
        public ResolutionTypes resolutionType = ResolutionTypes.highest;
        private bool IsResolutionTypeCustom() { return ( action == Actions.CapturePhoto || action == Actions.StartRecording ) && resolutionType == ResolutionTypes.custom; }

        [ShowIf( "IsResolutionTypeCustom" )]
        public CameraResolution camResolution = new CameraResolution( 1920, 1080 );

        //When taking a photo while the camera is actively running, we don't want to stop recording
        private bool releaseNatCam = false;

        //We store the DeviceCamera that we setup for access throughout this script
        private DeviceCamera deviceCamera = null;

        //We store the BlockEventRecorder that is requesting our Texture2D frame data to be sent every OnFrame() call
        private BlockEventRecorder videoRecorder = null;

        //We store the BlockEventNativeSharing that is requesting the captured photos to save to the camera roll
        private BlockEventNativeSharing blockEventNativeSharingPhotoToCameraRoll = null;

        //Store the list of DeviceCameras on Start
        private List<DeviceCamera> deviceCameras = null;

        //Store the current camera index within the DeviceCameras list
        private int currentCameraIndex = -99;

        //Store the upcoming camera index within the DeviceCameras list (used for swapping to the next available)
        private int nextCameraIndex = -99;

        //We store the BlockEventRecorder that is requesting our Texture frame data from a photo capture event
        private BlockEventRecorder photoRecorder = null;

#endif

        //----------------- "CLEAR PHOTOS" VARIABLES ------------------------------//
        public enum ClearPhotosAction
        {
            ClearAllPhotos,
            ClearOnlyFromBlock
        }
        [Space( 15f ), ShowIf( "IsActionClearPhotos" )]
        public ClearPhotosAction clearPhotosAction = ClearPhotosAction.ClearAllPhotos;
        private bool IsClearAllPhotos() { return IsActionClearPhotos() && clearPhotosAction == ClearPhotosAction.ClearAllPhotos; }
        private bool IsClearOnlyFromBlock() { return IsActionClearPhotos() && clearPhotosAction == ClearPhotosAction.ClearOnlyFromBlock; }

        [ShowIf( "ShouldShowBlockEventExternalCameraVariable" )]
        public BlockEventExternalCamera blockEventExternalCamera = null;

        //----------------- "SWITCH CAMERA" VARIABLES --------------------------//
        private bool ShouldShowBlockEventExternalCameraVariable()
        {
            return IsClearOnlyFromBlock() ||
( action == Actions.SwitchCamera );
        }

        public enum SwitchCameraAction
        {
            NextCamera,
            SpecificCamera
        }
        [InfoBox( "Switch the camera being used in a BlockEventExternalCamera" ), Space( 15f ), ShowIf( "action", Actions.SwitchCamera )]
        public SwitchCameraAction switchCameraTo = SwitchCameraAction.NextCamera;
        private bool SwitchCameraToNextCamera() { return action == Actions.SwitchCamera && switchCameraTo == SwitchCameraAction.NextCamera; }
        private bool SwitchCameraToSpecificCamera() { return action == Actions.SwitchCamera && switchCameraTo == SwitchCameraAction.SpecificCamera; }

        public enum ChooseCameraUsing
        {
            Type,
            Index
        }
        [Space( 15f ), ShowIf( "SwitchCameraToSpecificCamera" )]
        public ChooseCameraUsing chooseCameraUsing = ChooseCameraUsing.Type;
        private bool ChooseCameraUsingType() { return SwitchCameraToSpecificCamera() && chooseCameraUsing == ChooseCameraUsing.Type; }
        private bool ChooseCameraUsingIndex() { return SwitchCameraToSpecificCamera() && chooseCameraUsing == ChooseCameraUsing.Index; }

        [ShowIf( "ChooseCameraUsingType" )]
        public CameraType changeToCameraType = CameraType.Front;

        [ShowIf( "ChooseCameraUsingIndex" )]
        public int changeToCameraIndex = 0;

        //----------------- "CAPTURE PHOTO" EVENT ------------------------------//
        private bool ShowOnCapturePhotoEvent() { return action == Actions.CapturePhoto; }

        [Serializable]
        public class OnCapturePhoto: UnityEvent<Texture2D> { }

        [SerializeField, ShowIf( "ShowOnCapturePhotoEvent" ), FoldoutGroup( "Event Messages" )]
        public OnCapturePhoto onCapturePhotoCompleted = new OnCapturePhoto();


        //----------------- "CAPTURE VIDEO" EVENT ------------------------------//
        private bool ShowOnCaptureVideoEvent() { return action == Actions.StartRecording; }

        [Serializable]
        public class OnStartCapturingVideo: UnityEvent<Texture> { }

        [SerializeField, ShowIf( "ShowOnCaptureVideoEvent" ), FoldoutGroup( "Event Messages" )]
        public OnStartCapturingVideo onVideoCapturePreviewReady = new OnStartCapturingVideo();

        //----------------- OTHER EVENTS ------------------------------//
        private bool OnOtherActionCompleted() { return action == Actions.ClearPhotos || action == Actions.StopRecording || action == Actions.EnableTorch || action == Actions.DisableTorch || action == Actions.SwitchCamera; }

        [ShowIf( "OnOtherActionCompleted" ), FoldoutGroup( "Event Messages" )]
        public UnityEvent onActionCompleted = new UnityEvent();



        //---------------------------------------------------------//
        public override EventType GetEventType()
        //---------------------------------------------------------//
        {
            return EventType.ExternalCamera;

        } //END GetEventType

        //---------------------------------------------------------//
        public void SetAction( Actions action )
        //---------------------------------------------------------//
        {

            this.action = action;

        } //END SetAction

        //-------------------------------//
        protected override void Start()
        //-------------------------------//
        {
            base.Start();

            //showDebug = true;

            //Create an easy to reference list of the various camera this device has access to
#if NATCAM
            if( DeviceCamera.Cameras != null && DeviceCamera.Cameras.Length > 0 )
            {
                deviceCameras = DeviceCamera.Cameras.ToList();
            }
#else
            Debug.LogError( "BlockEventExternalCamera.cs Start() Missing NATCAM scripting define symbol in project settings" );
#endif

        } //END Start

        //-------------------------------//
        public override void PrepareEvent()
        //-------------------------------//
        {

            if( action == Actions.CapturePhoto )
            {
                eventReady = true;
            }
            else if( action == Actions.ClearPhotos )
            {
                if( IsClearOnlyFromBlock() )
                {
                    if( blockEventExternalCamera != null )
                    {
                        eventReady = true;
                    }
                }
                else if( IsClearAllPhotos() )
                {
                    eventReady = true;
                }
            }
            else if( action == Actions.StartRecording )
            {
                eventReady = true;
            }
            else if( action == Actions.StopRecording )
            {
                eventReady = true;
            }
            else if( action == Actions.EnableTorch )
            {
                eventReady = true;
            }
            else if( action == Actions.DisableTorch )
            {
                eventReady = true;
            }
            else if( action == Actions.SwitchCamera )
            {
                eventReady = true;
            }

        } //END PrepareEvent

        //-------------------------------//
        protected override void _CallEvent()
        //-------------------------------//
        {
            base._CallEvent();

            if( eventReady )
            {
                if( action == Actions.CapturePhoto )
                {
                    BeginCallCapturePhotoEvent();
                }
                else if( action == Actions.ClearPhotos )
                {
                    CallClearPhotosEvent();
                }
                else if( action == Actions.StartRecording )
                {
#if NATCAM
                    SetCurrentCameraIndex();
                    SetDeviceCamera( currentCameraIndex );
                    SetupCameraSettings( deviceCamera );
                    CallStartCaptureVideoEvent();
#else
                    Debug.LogError( "BlockEventExternalCamera.cs CallEvent(), missing Scripting Define Symbol NATCAM in project settings" );
#endif
                }
                else if( action == Actions.StopRecording )
                {
                    CallStopCaptureVideoEvent();
                }
                else if( action == Actions.EnableTorch )
                {
                    CallEnableTorchEvent();
                }
                else if( action == Actions.DisableTorch )
                {
                    CallDisableTorchEvent();
                }
                else if( action == Actions.SwitchCamera )
                {
                    if( showDebug ) { Debug.Log( "BlockEventExternalCamera.cs CallEvent() calling BeginCallSwitchCameraEvent()" ); }
                    BeginCallSwitchCameraEvent();
                }
            }

        } //END CallEvent

        //------------------------------//
        private void SetDeviceCamera( int cameraIndex )
        //------------------------------//
        {

            if( IsPlaying() )
            {
                Debug.LogError( "BlockEventExternalCamera.cs SetDeviceCamera() Cannot call SetDeviceCamera method while a camera is playing" );
                return;
            }

#if NATCAM
            //Reset our DeviceCamera
            deviceCamera = null;

            //Make sure the index passed in is valid in our list of deviceCameras
            if( DeviceCamera.Cameras != null && DeviceCamera.Cameras.Length > cameraIndex )
            {
                deviceCamera = DeviceCamera.Cameras[ cameraIndex ];
            }

#else
            Debug.LogError( "BlockEventExternalCamera.cs SetDeviceCamera() Missing NATCAM scripting define symbol in project settings" );
#endif

        } //END SetDeviceCamera

#if NATCAM
        //------------------------------//
        private void SetupCameraSettings( DeviceCamera cameraToSetup )
        //------------------------------//
        {
        
            //Double check that we have the deviceCamera at this point, if not something went wrong
            if( cameraToSetup != null )
            {
                //Set the camera we will be using as the one we found earlier
                NatCam.Camera = cameraToSetup;

                //Make sure this isn't a standard WebCam, because those devices don't support customization
                if( !IsLegacyCamera() )
                {
                    if( resolutionType == ResolutionTypes._640x480 )
                    {
                        cameraToSetup.PhotoResolution = new CameraResolution( 640, 480 );
                        cameraToSetup.PreviewResolution = new CameraResolution( 640, 480 );
                    }
                    else if( resolutionType == ResolutionTypes._1280x720 )
                    {
                        cameraToSetup.PhotoResolution = new CameraResolution( 1280, 720 );
                        cameraToSetup.PreviewResolution = new CameraResolution( 1280, 720 );
                    }
                    else if( resolutionType == ResolutionTypes._1920x1080 )
                    {
                        cameraToSetup.PhotoResolution = new CameraResolution( 1920, 1080 );
                        cameraToSetup.PreviewResolution = new CameraResolution( 1920, 1080 );
                    }
                    else if( resolutionType == ResolutionTypes.custom )
                    {
                        cameraToSetup.PhotoResolution = camResolution;
                        cameraToSetup.PreviewResolution = camResolution;
                    }
                    else if( resolutionType == ResolutionTypes.lowest )
                    {
                        cameraToSetup.PhotoResolution = CameraResolution.Lowest;
                        cameraToSetup.PreviewResolution = CameraResolution.Lowest;
                    }
                    else if( resolutionType == ResolutionTypes.highest )
                    {
                        cameraToSetup.PhotoResolution = CameraResolution.Highest;
                        cameraToSetup.PreviewResolution = CameraResolution.Highest;
                    }

                    cameraToSetup.Framerate = framerate;
                    cameraToSetup.FocusMode = focusMode;
                    cameraToSetup.FlashMode = flashMode;
                    cameraToSetup.ExposureMode = exposureMode;
                    cameraToSetup.ZoomRatio = cameraZoom;

                    if( cameraToSetup.IsTorchSupported )
                    {
                        cameraToSetup.TorchEnabled = enableTorch;
                    }
                }

            }

        } //END SetupCameraSettings
#endif

        //------------------------------//
        private void BeginCallCapturePhotoEvent()
        //------------------------------//
        {

            if( IsPlaying() )
            {
                CallCapturePhotoEvent();
            }
            else
            {
#if NATCAM
                SetCurrentCameraIndex();
                SetDeviceCamera( currentCameraIndex );
                SetupCameraSettings( deviceCamera );
                CallCapturePhotoEvent();
#else
                Debug.LogError( "BlockEventExternalCamera.cs BeginCallCapturePhotoEvent(), missing Scripting Define Symbol NATCAM in project settings" );
#endif
            }

        } //END BeginCallCapturePhotoEvent

        //------------------------------//
        private void CallCapturePhotoEvent()
        //------------------------------//
        {

#if NATCAM
            if( IsPlaying() )
            {
                if( showDebug ) Debug.Log( "BlockEventExternalCamera.cs CallCapturePhotoEvent() camera is already running, calling CapturePhoto... deviceCamera.IsFrontFacing = " + NatCam.Camera.IsFrontFacing );

                releaseNatCam = false;
                NatCam.CapturePhoto( CapturePhotoEventCompleted );
            }
            else
            {
                //If NatCam wasn't running already, then we should end it after taking the picture
                releaseNatCam = true;

                //Reset our events
                NatCam.OnStart -= OnVideoCaptureStart;
                NatCam.OnStart -= OnPhotoCaptureStart;
                NatCam.OnFrame -= OnFrame;

                //When the camera is ready, take a picture
                NatCam.OnStart += OnPhotoCaptureStart;

                //Lastly, tell the device camera to start
                if( deviceCamera != null )
                {
                    if( showDebug ) Debug.Log( "BlockEventExternalCamera.cs CallCapturePhotoEvent() starting camera for photo capture... deviceCamera.IsFrontFacing = " + NatCam.Camera.IsFrontFacing );
                    NatCam.Play( deviceCamera );
                }
                else
                {
                    Debug.LogError( "BlockEventExternalCamera.cs CallCapturePhotoEvent() Device has no available cameras, unable to capture Photo" );
                }

            }
#else
            Debug.LogError( "BlockEventExternalCamera.cs CallCapturePhotoEvent() Missing NATCAM scripting define symbol in project settings" );
#endif

        } //END CallCapturePhotoEvent

        //--------------------------------//
        private void OnPhotoCaptureStart()
        //--------------------------------//
        {

#if NATCAM
            if( NatCam.Camera != null && IsPlaying() )
            {
                NatCam.CapturePhoto( CapturePhotoEventCompleted );
            }
#else
            Debug.LogError( "BlockEventExternalCamera.cs OnPhotoCaptureStart(), missing Scripting Define Symbol NATCAM in project settings" );
#endif

        } //END OnPhotoCaptureStart


        //------------------------------//
        private void CapturePhotoEventCompleted( Texture2D photo )
        //------------------------------//
        {

#if NATCAM
            if( sendTextureToColorTweener && colorTweener != null )
            {
                colorTweener.SetTexture( photo );
            }

            //Send the captured image to the BlockEventRecorder if one is requesting the photo (for saving to local storage)
            if( photoRecorder != null && photoRecorder.action == BlockEventRecorder.Actions.SaveImageToLocalStorage )
            {
                photoRecorder.SaveImageEvent( photo );
                photoRecorder = null;
            }

            //Send the captured image to the BlockEventNativeSharing if one is requesting the photo (for sharing to device camera roll)
            if( blockEventNativeSharingPhotoToCameraRoll != null )
            {
                blockEventNativeSharingPhotoToCameraRoll.SavePhotoToCameraRoll();
                blockEventNativeSharingPhotoToCameraRoll = null;
            }

            if( onCapturePhotoCompleted != null )
            {
                onCapturePhotoCompleted.Invoke( photo );
            }

            NatCam.OnStart -= OnPhotoCaptureStart;

            //Check if we should release the NatCam texture (stop showing the preview)
            if( releaseNatCam )
            {
                NatCam.Release();
            }

            //Store the Texture of the photo for easily deletion later
            if( capturedPhotos == null )
            {
                capturedPhotos = new List<Texture2D>();
            }

            capturedPhotos.Add( photo );
#else
            Debug.LogError( "BlockEventExternalCamera.cs CapturePhotoEventCompleted(), missing Scripting Define Symbol NATCAM in project settings" );
#endif

        } //END CapturePhotoEventCompleted

        /*
        //-----------------------------------------//
        private void CapturePhotoFromPreview()
        //-----------------------------------------//
        {

            Texture2D photo = new Texture2D( NatCam.Camera.PreviewResolution.width, NatCam.Camera.PreviewResolution.height, TextureFormat.ARGB32, false );

            if( NatCam.CaptureFrame( photo ) )
            {
                CapturePhotoEventCompleted( photo );
            }

        } //END CapturePhotoFromPreview
        */

        //-----------------------------------------//
        public void CallClearPhotosEvent()
        //-----------------------------------------//
        {

            //Clear all of the photos from every BlockEvent
            if( IsClearAllPhotos() )
            {
                List<BlockEventExternalCamera> blockExtCameras = GameObject.FindObjectsOfType<BlockEventExternalCamera>().ToList();

                if( blockExtCameras != null && blockExtCameras.Count > 0 )
                {
                    foreach( BlockEventExternalCamera extCam in blockExtCameras )
                    {
                        extCam.ClearCapturedPhotos();
                    }
                }
            }

            //Clear the photos from only the passed in BlockEvent
            else if( IsClearOnlyFromBlock() )
            {
                if( blockEventExternalCamera != null )
                {
                    blockEventExternalCamera.ClearCapturedPhotos();
                }
            }

            //If there is an action that comes after this, call it
            if( onActionCompleted != null ) { onActionCompleted.Invoke(); }

        } //END CallClearPhotosEvent

        //-----------------------------------------//
        public void ClearCapturedPhotos()
        //-----------------------------------------//
        {

#if NATCAM
            if( capturedPhotos != null && capturedPhotos.Count > 0 )
            {
                foreach( Texture2D tex in capturedPhotos )
                {
                    if( tex != null )
                    {
                        Texture2D.Destroy( tex );
                    }
                }

                capturedPhotos = new List<Texture2D>();
            }
#else
            Debug.LogError( "BlockEventExternalCamera.cs ClearCapturedPhotos(), missing Scripting Define Symbol NATCAM in project settings" );
#endif

        } //END ClearCapturedPhotos

        //-----------------------------------------//
        private void CallStartCaptureVideoEvent()
        //-----------------------------------------//
        {
            //We can only have one camera playing
            if( IsPlaying() ) { return; }

#if NATCAM
            //Clear our old delegates
            NatCam.OnStart -= OnVideoCaptureStart;
            NatCam.OnStart -= OnPhotoCaptureStart;
            NatCam.OnFrame -= OnFrame;

            //Add our new delegates
            NatCam.OnStart += OnVideoCaptureStart;
            NatCam.OnFrame += OnFrame;

            if( deviceCamera != null )
            {
                if( showDebug ) Debug.Log( "BlockEventExternalCamera.cs CallStartCaptureVideoEvent() using deviceCamera.isFrontFacing = " + deviceCamera.IsFrontFacing );
                NatCam.Play( deviceCamera );
            }
            else
            {
                Debug.LogError( "BlockEventExternalCamera.cs CallStartCaptureVideoEvent() No Camera available on this device, unable to record video" );
            }

#else
            Debug.LogError( "BlockEventExternalCamera.cs CallStartCaptureVideoEvent() Missing NATCAM scripting define symbol in project settings" );
#endif

        } //END CallStartCaptureVideoEvent

        //-----------------------------------------//
        private void OnVideoCaptureStart()
        //-----------------------------------------//
        {

#if NATCAM
            if( sendTextureToColorTweener && colorTweener != null )
            {
                colorTweener.SetTexture( NatCam.Preview );
            }

            if( onVideoCapturePreviewReady != null )
            {
                onVideoCapturePreviewReady.Invoke( NatCam.Preview );
            }
#else
            Debug.LogError( "BlockEventExternalCamera.cs OnVideoCaptureStart(), missing Scripting Define Symbol NATCAM in project settings" );
#endif

        } //END OnVideoCaptureStart

        //-----------------------------------------//
        private void CallStopCaptureVideoEvent()
        //-----------------------------------------//
        {

#if NATCAM
            if( IsPlaying() )
            {
                NatCam.OnStart -= OnVideoCaptureStart;
                NatCam.OnFrame -= OnFrame;
                NatCam.Release();

                NatCam.Camera = null;

                if( onActionCompleted != null ) { onActionCompleted.Invoke(); }
            }
#else
            Debug.LogError( "BlockEventExternalCamera.cs CallStopCaptureVideoEvent() Missing NATCAM scripting define symbol in project settings" );
#endif

        } //END CallStopCaptureVideoEvent

        //-----------------------------------------//
        private void CallEnableTorchEvent()
        //-----------------------------------------//
        {

#if NATCAM
            if( !IsLegacyCamera() )
            {
                //If the camera is not started yet, start it up!
                if( !IsPlaying() )
                {
                    //Find a camera that supports the torch feature
                    if( DeviceCamera.Cameras != null && DeviceCamera.Cameras.Length > 0 )
                    {
                        DeviceCamera cameraThatSupportsTorch = null;

                        List<DeviceCamera> dCameras = DeviceCamera.Cameras.ToList();

                        foreach( DeviceCamera dCam in dCameras )
                        {
                            if( dCam.IsTorchSupported )
                            {
                                cameraThatSupportsTorch = dCam;
                                break;
                            }
                        }

                        if( cameraThatSupportsTorch != null )
                        {
                            NatCam.Play( cameraThatSupportsTorch );
                        }
                    }
                }

                //If this camera that is playing supports the torch feature, enable it
                if( IsPlaying() && NatCam.Camera != null && NatCam.Camera.IsTorchSupported )
                {
                    NatCam.Camera.TorchEnabled = true;
                }

                if( onActionCompleted != null ) { onActionCompleted.Invoke(); }
            }
#else
            Debug.LogError( "BlockEventExternalCamera.cs CallEnableTorchEvent() Missing NATCAM scripting define symbol in project settings" );
#endif

        } //END CallEnableTorchEvent

        //-----------------------------------------//
        private void CallDisableTorchEvent()
        //-----------------------------------------//
        {

#if NATCAM

            if( !IsLegacyCamera() )
            {
                if( IsPlaying() && NatCam.Camera != null && NatCam.Camera.IsTorchSupported )
                {
                    NatCam.Camera.TorchEnabled = false;
                }

                if( onActionCompleted != null ) { onActionCompleted.Invoke(); }
            }

#else
            Debug.LogError( "BlockEventExternalCamera.cs CallDisableTorchEvent() Missing NATCAM scripting define symbol in project settings" );
#endif

        } //END CallDisableTorchEvent

        //-----------------------------------------//
        private void BeginCallSwitchCameraEvent()
        //-----------------------------------------//
        {

            //This event can be called on either this script or another BlockEventCamera
            if( blockEventExternalCamera != null )
            {
                if( showDebug ) { Debug.Log( "BlockEventExternalCamera.cs BeginCallSwitchCameraEvent() calling blockEventExternalCamera.CallSwitchCameraEvent()" ); }
                blockEventExternalCamera.CallSwitchCameraEvent( SwitchCameraToNextCamera(), SwitchCameraToSpecificCamera() );
            }
            else
            {
                if( showDebug ) { Debug.Log( "BlockEventExternalCamera.cs BeginCallSwitchCameraEvent() calling CallSwitchCameraEvent()" ); }
                CallSwitchCameraEvent( SwitchCameraToNextCamera(), SwitchCameraToSpecificCamera() );
            }

        } //END BeginCallSwitchCameraEvent

        //-----------------------------------------//
        public void CallSwitchCameraEvent( bool switchCameraToNextCamera, bool switchCameraToSpecificCamera )
        //-----------------------------------------//
        {

            //If we are intending to switch to the next camera in our list of camera devices...
            if( switchCameraToNextCamera )
            {
                if( showDebug ) { Debug.Log( "BlockEventExternalCamera.cs CallSwitchCameraEvent() calling SwitchToNextCamera()" ); }
                SwitchToNextCamera();
            }

            //Otherwise if we are trying to change to a specific camera...
            else if( switchCameraToSpecificCamera )
            {
                if( showDebug ) { Debug.Log( "BlockEventExternalCamera.cs CallSwitchCameraEvent() calling SwitchCameraToSpecificCamera()" ); }
                SwitchToSpecificCamera();
            }

        } //END CallSwitchCameraEvent

        //-----------------------------------------//
        private void SwitchToNextCamera()
        //-----------------------------------------//
        {

#if NATCAM
            if( showDebug ) { Debug.Log( "BlockEventExternalCamera.cs SwitchToNextCamera() Start of method" ); }

            //Store whether or not we are calling Switch Camera while actively showing the camera feed
            bool isPlaying = IsPlaying();

            //Record the current and next camera index's in the DeviceCamera list
            SetCurrentCameraIndex();
            SetNextCameraIndex();

            if( showDebug ) { Debug.Log( "BlockEventExternalCamera.cs SwitchToNextCamera() currentIndex = " + currentCameraIndex + ", nextIndex = " + nextCameraIndex ); }

            //If our current and next camera index's are the same, then there is only one camera available, and our Swap command should be ignored
            if( currentCameraIndex == nextCameraIndex )
            {
                if( showDebug ) { Debug.Log( "BlockEventExternalCamera.cs SwitchToNextCamera() Unable to call Switch command, the current camera is the same as the next camera in our device list" ); }
                return;
            }

            //If the camera preview is playing, stop it in order to swap to the next camera
            if( IsPlaying() && NatCam.Camera != null )
            {
                NatCam.OnStart -= OnVideoCaptureStart;
                NatCam.OnFrame -= OnFrame;
                NatCam.Release();

                NatCam.Camera = null;

                if( showDebug ) { Debug.Log( "BlockEventExternalCamera.cs SwitchToNextCamera() camera was playing, stopping it to prepare to switch" ); }
            }

            //Change the camera we want to use to our "Next" camera
            SetDeviceCamera( nextCameraIndex );

            //Make sure that camera has the settings we chose
            SetupCameraSettings( deviceCamera );

            //If this BlockEventExternalCamera is set to Start Video Playback or Take a Picture, then change it's preferred cameraType to our new camera
            if( deviceCamera.IsFrontFacing )
            {
                useCameraType = CameraType.Front;
            }
            else
            {
                useCameraType = CameraType.Rear;
            }

            //Now that everythings setup, if we were playing back video previously, then play the new camera
            if( isPlaying )
            {
                if( showDebug ) { Debug.Log( "BlockEventExternalCamera.cs SwitchToNextCamera() Camera has been switched for this BlockEventExternalCamera, calling CallStartCaptureVideoEvent(), deviceCamera.isFrontFacing = " + deviceCamera.IsFrontFacing ); }
                CallStartCaptureVideoEvent();
            }
            else
            {
                if( showDebug ) { Debug.Log( "BlockEventExternalCamera.cs SwitchToNextCamera() Camera has been switched for this BlockEventExternalCamera, but there was no camera previously playing so we won't start playing this new one, deviceCamera.isFrontFacing = " + deviceCamera.IsFrontFacing ); }
            }

            if( onActionCompleted != null ) { onActionCompleted.Invoke(); }

#else
            Debug.LogError( "BlockEventExternalCamera.cs SwitchToNextCamera() Missing NATCAM scripting define symbol in project settings" );
#endif

        } //END SwitchToNextCamera

        //-----------------------------------------//
        private void SwitchToSpecificCamera()
        //-----------------------------------------//
        {

#if NATCAM
            //Store whether or not we are calling Switch Camera while actively showing the camera feed
            bool isPlaying = IsPlaying();

            //Store the index of the camera we are switching to
            int cameraIndexToSwitchTo = -99;

            //Check that the camera we want to switch to exists
            if( ChooseCameraUsingType() && !DoesCameraExist( changeToCameraType ) )
            {
                if( showDebug ) { Debug.Log( "BlockEventExternalCamera.cs SwitchToSpecificCamera() unable to switch to specific camera type (" + changeToCameraType + "), it doesn't exist" ); }
                return;
            }
            else if( ChooseCameraUsingIndex() && !DoesCameraExist( changeToCameraIndex ) )
            {
                if( showDebug ) { Debug.Log( "BlockEventExternalCamera.cs SwitchToSpecificCamera() unable to switch to specific camera index (" + changeToCameraIndex + "), it doesn't exist" ); }
                return;
            }

            //If the camera we want to switch to already has a defined index, use that
            if( ChooseCameraUsingIndex() )
            {
                cameraIndexToSwitchTo = changeToCameraIndex;
            }

            //Otherwise if we're switching to a specific camera, find it's index
            if( ChooseCameraUsingType() )
            {
                cameraIndexToSwitchTo = GetCameraIndex( changeToCameraType );
            }

            //If our index is still -99, something wen't wrong, end the process
            if( cameraIndexToSwitchTo == -99 )
            {
                if( showDebug ) { Debug.Log( "BlockEventExternalCamera.cs SwitchToSpecificCamera() unable to locate camera index, cancelling SwitchCamera command" ); }
                return;
            }

            //If we are currently using a camera, make sure it's different than the one we want to switch to
            if( IsPlaying() && NatCam.Camera != null && currentCameraIndex != -99 )
            {
                //If our cameraIndexToSwitchTo and currentCameraIndex are the same, 
                //and our Swap command should be ignored
                if( currentCameraIndex == cameraIndexToSwitchTo )
                {
                    if( showDebug ) { Debug.Log( "BlockEventExternalCamera.cs SwitchToSpecificCamera() Unable to call Switch command, the cameraIndexToSwitchTo is the same as the current active camera" ); }
                    return;
                }
            }

            //And now we're ready to switch the camera!
            //If the camera preview is playing, stop it in order to swap to the next camera
            if( IsPlaying() && NatCam.Camera != null )
            {
                NatCam.OnStart -= OnVideoCaptureStart;
                NatCam.OnFrame -= OnFrame;
                NatCam.Release();

                NatCam.Camera = null;
            }

            //Change the camera we want to use to our "Next" camera
            SetDeviceCamera( cameraIndexToSwitchTo );

            //Make sure that camera has the settings we chose
            SetupCameraSettings( deviceCamera );

            //Now that everythings setup, if we were playing back video previously, then play the new camera
            if( isPlaying )
            {
                CallStartCaptureVideoEvent();
            }

            if( onActionCompleted != null ) { onActionCompleted.Invoke(); }

#else
            Debug.LogError( "BlockEventExternalCamera.cs SwitchToSpecificCamera() Missing NATCAM scripting define symbol in project settings" );
#endif

        } //END SwitchToSpecificCamera

        //-----------------------------------------//
        private int GetCameraIndex( CameraType type )
        //-----------------------------------------//
        {

#if NATCAM
            if( deviceCameras != null && deviceCameras.Count > 0 )
            {
                for( int i = 0; i < deviceCameras.Count; i++ )
                {
                    if( deviceCameras[ i ] != null )
                    {
                        if( type == CameraType.Front && deviceCameras[ i ].IsFrontFacing )
                        {
                            return i;
                        }
                        else if( type == CameraType.Rear && !deviceCameras[ i ].IsFrontFacing )
                        {
                            return i;
                        }
                    }
                }
            }

            return -99;
#else
            Debug.LogError( "BlockEventExternalCamera.cs GetCameraIndex() Missing NATCAM scripting define symbol in project settings" );
            return -99;
#endif

        } //END GetCameraIndex

#if NATCAM
        //-----------------------------------------//
        private int GetCameraIndex( DeviceCamera camera )
        //-----------------------------------------//
        {

            if( camera == null )
            {
                Debug.LogError( "BlockEventExternalCamera.cs GetCameraIndex() passed in camera does not exist in list of DeviceCameras" );
                return -99;
            }

            if( deviceCameras != null && deviceCameras.Count > 0 )
            {
                for( int i = 0; i < deviceCameras.Count; i++ )
                {
                    if( deviceCameras[ i ] != null && deviceCameras[ i ] == camera )
                    {
                        return i;
                    }
                }
            }

            return -99;

        } //END GetCameraIndex
#endif

        //-----------------------------------------//
        /// <summary>
        /// Gets the index of the currently active camera
        /// </summary>
        /// <returns>camera index with the DeviceCameras list</returns>
        private int GetCameraIndex()
        //-----------------------------------------//
        {

#if NATCAM
            if( NatCam.Camera == null )
            {
                Debug.LogError( "BlockEventExternalCamera.cs GetCameraIndex() no camera is currently being used" );
                return -99;
            }

            if( deviceCameras != null && deviceCameras.Count > 0 )
            {
                for( int i = 0; i < deviceCameras.Count; i++ )
                {
                    if( deviceCameras[ i ] != null && deviceCameras[ i ] == NatCam.Camera )
                    {
                        return i;
                    }
                }
            }

            return -99;

#else
            Debug.LogError( "BlockEventExternalCamera.cs GetCameraIndex() passed in camera does not exist in list of DeviceCameras" );
            return -99;
#endif

        } //END GetCameraIndex

#if NATCAM
        //-----------------------------------------//
        private bool DoesCameraExist( CameraType type )
        //-----------------------------------------//
        {
        
            if( deviceCameras != null && deviceCameras.Count > 0 )
            {
                for( int i = 0; i < deviceCameras.Count; i++ )
                {
                    if( deviceCameras[ i ] != null )
                    {
                        if( type == CameraType.Front && deviceCameras[ i ].IsFrontFacing )
                        {
                            return true;
                        }
                        else if( type == CameraType.Rear && !deviceCameras[ i ].IsFrontFacing )
                        {
                            return true;
                        }
                    }
                }
            }

            return false;

        } //END DoesCameraExist
#endif

        //-----------------------------------------//
        private bool DoesCameraExist( int index )
        //-----------------------------------------//
        {

#if NATCAM
            if( deviceCameras != null && deviceCameras.Count > index )
            {
                return true;
            }

            return false;
#else
            Debug.LogError( "BlockEventExternalCamera.cs DoesCameraExist(), missing Scripting Define Symbol NATCAM in project settings" );
            return false;
#endif

        } //END DoesCameraExist

        //-----------------------------------------//
        private void SetCurrentCameraIndex( int cameraIndexToSwitchTo )
        //-----------------------------------------//
        {

#if NATCAM
            if( deviceCameras != null && deviceCameras.Count > cameraIndexToSwitchTo )
            {
                currentCameraIndex = cameraIndexToSwitchTo;
            }

#else
            Debug.LogError( "BlockEventExternalCamera.cs SetCurrentCameraIndex(" + cameraIndexToSwitchTo + ") Missing NATCAM scripting define symbol in project settings" );
#endif

        } //END SetCurrentCameraIndex

        //-----------------------------------------//
        private void SetCurrentCameraIndex()
        //-----------------------------------------//
        {

#if NATCAM
            currentCameraIndex = -99;

            //Find the active camera's index

            //If we already have an active camera, this is pretty easy, just find where the existing camera is in the list of cameras
            if( NatCam.Camera != null )
            {
                for( int i = 0; i < deviceCameras.Count; i++ )
                {
                    if( deviceCameras[ i ] != null && deviceCameras[ i ] == NatCam.Camera )
                    {
                        if( showDebug ) { Debug.Log( "BlockEventExternalCamera.cs SetCurrentCameraIndex() found current camera at index[" + i + "]" ); }
                        currentCameraIndex = i;
                        break;
                    }
                }
            }

            //If we don't have an active camera, try to find the camera selected by the user when setting up this BlockEventExternalCamera script
            else if( NatCam.Camera == null )
            {
                for( int i = 0; i < deviceCameras.Count; i++ )
                {
                    if( deviceCameras[ i ] != null )
                    {
                        //If we're looking for the "Front" camera...
                        if( useCameraType == CameraType.Front && deviceCameras[ i ].IsFrontFacing )
                        {
                            if( showDebug ) { Debug.Log( "BlockEventExternalCamera.cs SetCurrentCameraIndex() found initial camera at index[" + i + "]" ); }
                            currentCameraIndex = i;
                            break;
                        }

                        //Otherwise if we're looking for the "Rear" camera
                        else if( useCameraType == CameraType.Rear && !deviceCameras[ i ].IsFrontFacing )
                        {
                            if( showDebug ) { Debug.Log( "BlockEventExternalCamera.cs SetCurrentCameraIndex() found initial camera at index[" + i + "]" ); }
                            currentCameraIndex = i;
                            break;
                        }
                    }
                }

                //If we didn't find the initial camera we would prefer, just grab any camera you can find
                if( currentCameraIndex == -99 )
                {
                    for( int i = 0; i < deviceCameras.Count; i++ )
                    {
                        if( deviceCameras[ i ] != null )
                        {
                            if( showDebug ) { Debug.Log( "BlockEventExternalCamera.cs SetCurrentCameraIndex() unable to find preferred initial camera, using the first we could find at index = " + i ); }
                            currentCameraIndex = i;

                            //Is the camera we found Front or Rear facing? Whichever it is, 
                            //store it as our chosen camera type to use to make this searching process easier in the future
                            if( deviceCameras[ i ].IsFrontFacing )
                            {
                                useCameraType = CameraType.Front;
                            }
                            else
                            {
                                useCameraType = CameraType.Rear;
                            }

                            break;
                        }
                    }
                }
            }
#else
            Debug.LogError( "BlockEventExternalCamera.cs SetCurrentCameraIndex() Missing NATCAM scripting define symbol in project settings" );
#endif

        } //END SetCurrentCameraIndex

        //-----------------------------------------//
        private void SetNextCameraIndex()
        //-----------------------------------------//
        {

#if NATCAM
            if( currentCameraIndex != -99 )
            {
                //Switch to the next camera in the list
                if( currentCameraIndex + 1 < deviceCameras.Count )
                {
                    if( showDebug ) { Debug.Log( "BlockEventExternalCamera.cs SetNextCameraIndex() CurrentCamera[" + currentCameraIndex + "] + 1 < deviceCameras.Count(" + deviceCameras.Count + "), setting to next in list" ); }
                    nextCameraIndex = currentCameraIndex + 1;
                }

                //If we reached the end of the list, go back to the beginning
                else
                {
                    if( showDebug ) { Debug.Log( "BlockEventExternalCamera.cs SetNextCameraIndex() CurrentCamera[" + currentCameraIndex + "] > deviceCameras.Count(" + deviceCameras.Count + "), setting first camera in the list" ); }
                    nextCameraIndex = 0;
                }
            }
#else
            Debug.LogError( "BlockEventExternalCamera.cs SetNextCameraIndex(), missing Scripting Define Symbol NATCAM in project settings" );
#endif

        } //END SetNextCameraIndex

        //-----------------------------------------//
        private bool IsLegacyCamera()
        //-----------------------------------------//
        {

#if UNITY_EDITOR || UNITY_STANDALONE
            return true;
#else
            return false;
#endif

        } //END IsLegacyCamera

        //------------------------------------------//
        /// <summary>
        /// Is the external camera streaming into the Unity app?
        /// </summary>
        /// <returns></returns>
        public bool IsPlaying()
        //------------------------------------------//
        {

#if NATCAM
            return NatCam.IsPlaying;
#else
            Debug.LogError( "BlockEventExternalCamera.cs IsPlaying() Missing NATCAM scripting define symbol in project settings" );
            return false;
#endif

        } //END IsPlaying

#if NATCAM
        //------------------------------------------//
        public void RequestCapturedPhoto( BlockEventNativeSharing blockEventNativeSharing )
        //------------------------------------------//
        {
            //If we already have a defined variable to store, reset it to null
            if( blockEventNativeSharingPhotoToCameraRoll != null )
            {
                blockEventNativeSharingPhotoToCameraRoll = null;
            }

            //Store the Block Event to send the completed url to
            if( blockEventNativeSharing != null )
            {
                blockEventNativeSharingPhotoToCameraRoll = blockEventNativeSharing;
            }

        } //END RequestCapturedPhoto

        //------------------------------------------//
        public void StopRequestingCapturedPhoto()
        //------------------------------------------//
        {
            //Reset the stored variable to null
            if( blockEventNativeSharingPhotoToCameraRoll != null )
            {
                blockEventNativeSharingPhotoToCameraRoll = null;
            }

        } //END StopRequestingCapturedPhoto
#endif

        //------------------------------------------//
        /// <summary>
        /// Informs this BlockEventExternalCamera that a BlockEventRecorder is requesting it's Texture frames. Frames will be sent via the OnFrame() method
        /// </summary>
        /// <param name="recorder">The BlockEventRecorder that the Texture2D camera preview will be sent to every OnFrame()</param>
        public void RequestFrames( BlockEventRecorder recorder )
        //------------------------------------------//
        {

#if NATCAM
            //Remove any previous reference to other BlockEventRecorder's
            videoRecorder = null;

            //Store the new BlockEventRecorder
            if( recorder != null )
            {
                videoRecorder = recorder;
            }
#else
            Debug.LogError( "BlockEventExternalCamera.cs RequestFrames(), missing Scripting Define Symbol NATCAM in project settings" );
#endif

        } //END RequestFrames

        //-------------------------------------------//
        /// <summary>
        /// Stops this scripts OnFrame() method from sending the camera Texture2D preview data to the BlockEventRecorder that was sent in during the RequestFrames() method
        /// </summary>
        public void StopRequestingFrames()
        //-------------------------------------------//
        {

#if NATCAM
            //Removes the reference to any linked BlockEventRecorder so that OnFrame() will no longer send it's Texture2D camera data
            videoRecorder = null;
#else
            Debug.LogError( "BlockEventExternalCamera.cs StopRequestingFrames(), missing Scripting Define Symbol NATCAM in project settings" );
#endif

        } //END StopRequestingFrames

        //-------------------------------------------//
        public void OnFrame()
        //-------------------------------------------//
        {

#if NATCAM

            if( videoRecorder != null && IsPlaying() && videoRecorder.action == BlockEventRecorder.Actions.StartVideoRecordingToLocalStorage )
            {
                //Debug.Log( "OnFrame sending preview = " + NatCam.Preview.width + "x" + NatCam.Preview.height );
                videoRecorder.ExternalCameraFrameRecieved( NatCam.Preview );
            }
            else
            {
                //Debug.Log( "OnFrame cannot send preview, videoRecorder != null(" + ( videoRecorder != null ) + ") and IsPlaying = " + IsPlaying() );
            }

#else
            Debug.LogError( "BlockEventExternalCamera.cs OnFrame() Missing NATCAM scripting define symbol in project settings" );
#endif

        } //END OnFrame

        //---------------------------------------------//
        public Texture GetCurrentFrame()
        //---------------------------------------------//
        {

#if NATCAM
            if( IsPlaying() )
            {
                return NatCam.Preview;
            }

            return null;
#else
            Debug.LogError( "BlockEventExternalCamera.cs GetCurrentFrame() Missing NATCAM scripting define symbol in project settings" );
            return null;
#endif

        } //END GetCurrentFrame

        //---------------------------------------------//
        public int GetWidth()
        //---------------------------------------------//
        {

#if NATCAM
            
            if( NatCam.Preview != null )
            {
                return NatCam.Preview.width;
            }

            return Screen.width;
#else
            Debug.LogError( "BlockEventExternalCamera.cs GetWidth(), missing Scripting Define Symbol NATCAM in project settings" );
            return -99;
#endif

        } //END GetWidth

        //---------------------------------------------//
        public int GetHeight()
        //---------------------------------------------//
        {

#if NATCAM
            
            if( NatCam.Preview != null )
            {
                return NatCam.Preview.height;
            }

            return Screen.height;
#else
            Debug.LogError( "BlockEventExternalCamera.cs GetHeight(), missing Scripting Define Symbol NATCAM in project settings" );
            return -99;
#endif

        } //END GetHeight

        //--------------------------------------------//
        public void RequestCapturedPhoto( BlockEventRecorder photoRecorder )
        //--------------------------------------------//
        {

#if NATCAM
            //Store a reference to this recorder, we will send it the photo texture when we capture it
            this.photoRecorder = photoRecorder;
#else
            Debug.LogError( "BlockEventExternalCamera.cs RequestCapturedPhoto(), missing Scripting Define Symbol NATCAM in project settings" );
#endif

        } //END RequestCapturedPhoto

        //------------------------------------------//
        public Texture GetVideoPreview()
        //------------------------------------------//
        {

#if NATCAM
            if( NatCam.Preview != null )
            {
                return NatCam.Preview;
            }
#endif

            return null;

        } //END GetVideoPreview

    } //END BlockEventExternalCamera

} //END Namespace