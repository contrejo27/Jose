using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using Sirenix.OdinInspector;
using System;
using System.IO;
using System.Linq;

#if UNITY_EDITOR
using UnityEditor;
#endif

#if NATCORDER
using NatCorderU.Core;
using NatCorderU.Core.Recorders;
#endif


namespace BrandXR
{
    public class BlockEventRecorder: BlockEventBase
    {

        //----------------- ACTIONS --------------------------------//
        public enum Actions
        {
            None,
            StartVideoRecordingToLocalStorage,
            StopVideoRecording,
            SaveImageToLocalStorage,
            DeleteLocalStorage
        }

        [TitleGroup( "Block Event - Screen Recording", "Used to record video of the Unity camera or a BlockEventExternalCamera. Requires NATCORDER scripting define symbol in Project Settings" )]
        public Actions action = Actions.None;
        private bool IsActionStartRecording() { return action == Actions.StartVideoRecordingToLocalStorage; }

        //----------------- "START RECORDING" VARIABLES ------------------------------//
        [Button( "Add Necessary Scripting Define Symbols", ButtonSizes.Large ), ShowIf( "ShowScriptingDefineSymbolWarning" ), InfoBox( "WARNING: This script requires the NATCORDER scripting define symbol be defined in your Unity Project Settings and that the NatCorder plugin be in your Assets folder to work properly", InfoMessageType.Error )]
        public void AddScriptingDefineSymbols()
        {
#if UNITY_EDITOR && !NATCORDER
            string newDefineSymbols = PlayerSettings.GetScriptingDefineSymbolsForGroup
                ( BuildPipeline.GetBuildTargetGroup( EditorUserBuildSettings.activeBuildTarget ) );

            if( !newDefineSymbols.Contains( "NATCORDER" ) )
            {
                //Debug.Log( "Define symbols = " + newDefineSymbols );
                newDefineSymbols += ";NATCORDER";
            }

            PlayerSettings.SetScriptingDefineSymbolsForGroup( BuildPipeline.GetBuildTargetGroup
                ( EditorUserBuildSettings.activeBuildTarget ), newDefineSymbols );
#endif
        }

        private bool ShowScriptingDefineSymbolWarning()
        {
#if NATCORDER && UNITY_EDITOR
            if( AssetDatabase.IsValidFolder( "Assets/NatCorder" ) )
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

        public enum RecordingType
        {
            UnityCamera,
            BlockEventExternalCamera
        }
        [Space(15f), ShowIf( "IsActionStartRecording" )]
        public RecordingType recordingType = RecordingType.UnityCamera;
        private bool IsRecordingTypeUnityCamera() { return action == Actions.StartVideoRecordingToLocalStorage && recordingType == RecordingType.UnityCamera;  }
        private bool IsRecordingTypeBlockEventExternalCamera() { return action == Actions.StartVideoRecordingToLocalStorage && recordingType == RecordingType.BlockEventExternalCamera; }
        private bool IsRecordingTypeCameraOrBlockEventExternalCamera() { return action == Actions.StartVideoRecordingToLocalStorage && ( recordingType == RecordingType.UnityCamera || recordingType == RecordingType.BlockEventExternalCamera ); }
        private bool IsRecordingTypeBlockEventExternalCameraOrSaveImageFromBlockEventExternalCamera()
        {
            return ( action == Actions.StartVideoRecordingToLocalStorage && recordingType == RecordingType.BlockEventExternalCamera ) ||
                     ( action == Actions.SaveImageToLocalStorage && saveImageUsing == SaveImageUsing.BlockEventExternalCamera );
        }

#if NATCORDER
        private bool IsRecordingTypeUnityCameraAndGIF() { return IsRecordingTypeUnityCamera() && container == Container.GIF; }
        private bool IsRecordingTypeUnityCameraAndMP4() { return IsRecordingTypeUnityCamera() && container == Container.MP4; }

        private bool IsRecordingTypeBlockEventExternalCameraAndGIF() { return IsRecordingTypeBlockEventExternalCamera() && container == Container.GIF; }
        private bool IsRecordingTypeBlockEventExternalCameraAndMP4() { return IsRecordingTypeBlockEventExternalCamera() && container == Container.MP4; }
#else
        private bool IsRecordingTypeUnityCameraAndGIF() { return false; }
        private bool IsRecordingTypeUnityCameraAndMP4() { return false; }

        private bool IsRecordingTypeBlockEventExternalCameraAndGIF() { return false; }
        private bool IsRecordingTypeBlockEventExternalCameraAndMP4() { return false; }
#endif



        [Space( 15f ), InfoBox("The path to videos you record will be added to this list"), ShowIf( "IsRecordingTypeCameraOrBlockEventExternalCamera" )]
        public List<string> recordedVideos = new List<string>(); 

        //-------------------- RECORD TYPE - CAMERA VARIABLES ---------------------------//

        [Space( 15f ), ShowIf( "IsRecordingTypeUnityCamera" )]
        public Camera cameraToRecord;

#if NATCORDER
        //Store the reference to the CameraRecorder variable that will grab the texture data from the cameraToRecord data
        private CameraRecorder cameraRecorder = null;

        //Store the reference to the AudioRecorder variable that will listen to the scene's AudioListener and add it's audio to the recorded video
        private AudioRecorder audioRecorder = null;
#endif

        [Space( 15f ), InfoBox("Link to the BlockEventExternalCamera that you want to record the video/photo from"), ShowIf( "IsRecordingTypeBlockEventExternalCameraOrSaveImageFromBlockEventExternalCamera" )]
        public BlockEventExternalCamera blockEventExternalCamera = null;

        [Tooltip( "We recommend leaving the settings as default" ), ShowIf( "IsRecordingTypeCameraOrBlockEventExternalCamera" )]
        public bool useDefaultSettings = true;
        private bool IsUseDefaultSettingsFalse() { return IsRecordingTypeCameraOrBlockEventExternalCamera() && action == Actions.StartVideoRecordingToLocalStorage && !useDefaultSettings; }

#if NATCORDER
        [Space( 15f ), Tooltip("You can record as .GIF or an .MP4"), ShowIf( "IsUseDefaultSettingsFalse" )]
        public Container container = Container.MP4;
        private bool IsVideoContainerGIF() { return IsUseDefaultSettingsFalse() && container == Container.GIF; }
        private bool IsVideoContainerMP4() { return IsUseDefaultSettingsFalse() && container == Container.MP4; }
#else
        private bool IsVideoContainerGIF() { return false; }
        private bool IsVideoContainerMP4() { return false; }
#endif

        [Space( 15 ), ShowIf( "IsUseDefaultSettingsFalse" )]
        public bool useDefaultVideoFormat = true;
        private bool IsUseDefaultVideoFormatTrue() { return IsUseDefaultSettingsFalse() && useDefaultVideoFormat; }
        private bool IsUseDefaultVideoFormatFalse() { return IsUseDefaultSettingsFalse() && !useDefaultVideoFormat; }

        [Tooltip("Width of the video in pixels"), ShowIf( "IsUseDefaultVideoFormatFalse" )]
        public int width = 960;

        [Tooltip( "Height of the video in pixels" ), ShowIf( "IsUseDefaultVideoFormatFalse" )]
        public int height = 540;

        [Tooltip( "Framerate of the video, recommended is 30" ), ShowIf( "IsUseDefaultVideoFormatFalse" )]
        public int framerate = 30;

        [Tooltip( "Bitrate of the video, recommended is 5909759" ), ShowIf( "IsUseDefaultVideoFormatFalse" )]
        public int bitrate = 5909759;

        [Tooltip( "KeyframeInterval of the video, recommended is 3" ), ShowIf( "IsUseDefaultVideoFormatFalse" )]
        public int keyframeInterval = 3;


        public enum AudioRecordFormat
        {
            None,
            Unity
        }
        [Space( 15f ), Tooltip( "You can choose to not record any audio or to record audio from the Unity App" ), ShowIf( "IsRecordingTypeUnityCameraAndMP4" )]
        public AudioRecordFormat audioFormat = AudioRecordFormat.Unity;
        private bool IsAudioFormatNone() { return IsRecordingTypeUnityCameraAndMP4() && audioFormat == AudioRecordFormat.None; }
        private bool IsAudioFormatUnity() { return IsRecordingTypeUnityCameraAndMP4() && audioFormat == AudioRecordFormat.Unity; }

        [Tooltip("We recommend leaving this setting to true unless you know what you're doing"), ShowIf( "IsAudioFormatUnity" )]
        public bool useDefaultAudioSettings = true;
        private bool IsUseDefaultAudioSettingsTrue() { return IsAudioFormatUnity() && useDefaultAudioSettings; }
        private bool IsUseDefaultAudioSettingsFalse() { return IsAudioFormatUnity() && !useDefaultAudioSettings; }

        [Tooltip("Recommended sample rate is 44100 for MP3-like quality"), ShowIf( "IsUseDefaultAudioSettingsFalse" )]
        public int sampleRate = 44100;

        [Tooltip("Recommended channel count is 2 for stereo seperation"), ShowIf( "IsUseDefaultAudioSettingsFalse" )]
        public int channelCount = 2;


        [Space( 15f ), Tooltip( "You can choose to record audio from Unity or no audio" ), ShowIf( "IsRecordingTypeBlockEventExternalCameraAndMP4" )]
        public AudioRecordFormat audioFormatForExtCamera = AudioRecordFormat.Unity;
        private bool IsExtCameraAudioFormatNone() { return IsRecordingTypeBlockEventExternalCameraAndMP4() && audioFormatForExtCamera == AudioRecordFormat.None; }
        private bool IsExtCameraAudioFormatUnity() { return IsRecordingTypeBlockEventExternalCameraAndMP4() && audioFormatForExtCamera == AudioRecordFormat.Unity; }

        public enum RecordAudioType
        {
            UnityAudioListener,
            UnityAudioSource,
            BlockAudio
        }

        [Tooltip("Choose AudioListener to record all Audio playing in this app, or AudioSource to choose a specific source"), ShowIf( "IsExtCameraAudioFormatUnity" )]
        public RecordAudioType recordAudioType = RecordAudioType.UnityAudioListener;
        private bool IsRecordAudioTypeAudioListener() { return IsExtCameraAudioFormatUnity() && recordAudioType == RecordAudioType.UnityAudioListener; }
        private bool IsRecordAudioTypeAudioSource() { return IsExtCameraAudioFormatUnity() && recordAudioType == RecordAudioType.UnityAudioSource; }
        private bool IsRecordAudioTypeBlockAudio() { return IsExtCameraAudioFormatUnity() && recordAudioType == RecordAudioType.BlockAudio; }

        [Tooltip("The AudioListener to record audio from"), ShowIf( "IsRecordAudioTypeAudioListener" )]
        public AudioListener audioListener = null;

        [Tooltip( "The AudioSource to record audio from" ), ShowIf( "IsRecordAudioTypeAudioSource" )]
        public AudioSource audioSource = null;

        [Tooltip( "The BlockAudio to record audio from" ), ShowIf( "IsRecordAudioTypeBlockAudio" )]
        public BlockAudio blockAudio = null;

        //Reference to the AudioGetter component we create and attach to either the AudioListener or AudioSource we want to record from.
        //Removed from the AudioListener/AudioSource when recording is complete
        private AudioGetter audioGetter = null;

        //----------------- "SAVE IMAGE TO LOCAL STORAGE" EVENTS ------------------//
        public enum SaveImageUsing
        {
            Texture,
            BlockEventExternalCamera,
            UnityCamera
        }
        [Space( 15f ), ShowIf("action", Actions.SaveImageToLocalStorage )]
        public SaveImageUsing saveImageUsing = SaveImageUsing.Texture;
        private bool SaveImageUsingTexture() { return action == Actions.SaveImageToLocalStorage && saveImageUsing == SaveImageUsing.Texture; }
        private bool SaveImageUsingBlockEventExternalCamera() { return action == Actions.SaveImageToLocalStorage && saveImageUsing == SaveImageUsing.BlockEventExternalCamera; }
        private bool SaveImageUsingUnityCamera() { return action == Actions.SaveImageToLocalStorage && saveImageUsing == SaveImageUsing.UnityCamera; }

        [Space(15f), ShowIf( "SaveImageUsingTexture" )]
        public Texture textureToSave = null;

        [Space( 15f ), ShowIf( "SaveImageUsingUnityCamera" )]
        public Camera unityCamera = null;

        //A flag set when the user sends the CallEvent() command to capture a texture from the camera in LateUpdate once the scene has been rendered
        private bool captureCameraTextureInLateUpdate = false;

        public enum SavePathType
        {
            AutoGenerateFilename,
            UseSpecific
        }
        [Space( 15f ), Tooltip("How do you want the file to be named? If auto-generated, the name will be randomized"), ShowIf("action", Actions.SaveImageToLocalStorage) ]
        public SavePathType savePathType = SavePathType.AutoGenerateFilename;
        private bool UseGeneratedPath() { return action == Actions.SaveImageToLocalStorage && savePathType == SavePathType.AutoGenerateFilename; }
        private bool UseSpecificPath() { return action == Actions.SaveImageToLocalStorage && savePathType == SavePathType.UseSpecific; }

        [Space(15f), Tooltip("What do you want the file to be called?"), ShowIf( "UseSpecificPath" )]
        public string fileName = "";

        [Space( 15f ), InfoBox("Textures you save to local storage will have their paths stored as part of this list"), ShowIf( "action", Actions.SaveImageToLocalStorage )]
        public List<string> capturedPhotos = new List<string>();

        //----------------- "DELETE LOCAL STORAGE" VARIABLES --------------------------------//
        
        public enum DeleteType
        {
            All,
            Photos,
            MP4s,
            GIFs
        }
        [Space( 15f ), ShowIf( "action", Actions.DeleteLocalStorage )]
        public DeleteType deleteType = DeleteType.All;

        //----------------- "START RECORDING" EVENTS ------------------------------//
        private bool ShowOnActionIsStartRecording() { return action == Actions.StartVideoRecordingToLocalStorage; }

        [SerializeField, ShowIf( "ShowOnActionIsStartRecording" ), FoldoutGroup( "Event Messages" )]
        public UnityEvent onRecordingStarted;


        [Serializable]
        public class OnRecordingCompleted : UnityEvent<string>{};

        [SerializeField, ShowIf( "ShowOnActionIsStartRecording" ), FoldoutGroup( "Event Messages" )]
        public OnRecordingCompleted onRecordingCompleted;

        //----------------- OTHER ACTION EVENTS ------------------------------//
        private bool ShowOnActionIsNotStartRecordingAndNotNone() { return action != Actions.StartVideoRecordingToLocalStorage && action != Actions.None; }

        [Space( 15f ), ShowIf( "ShowOnActionIsNotStartRecordingAndNotNone" ), FoldoutGroup( "Event Messages" )]
        public UnityEvent onActionCompleted;

        //---------------------------------------------------------//
        protected override void Start()
        //---------------------------------------------------------//
        {
            base.Start();

            showDebug = true;

        } //END Start

        //---------------------------------------------------------//
        public override EventType GetEventType()
        //---------------------------------------------------------//
        {
            return EventType.Recorder;

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

            if( action == Actions.StartVideoRecordingToLocalStorage )
            {
                if( recordingType == RecordingType.UnityCamera )
                {
                    //Look for an audioListener to record from
                    if( audioListener == null && GameObject.FindObjectOfType<AudioListener>() != null )
                    {
                        audioListener = GameObject.FindObjectOfType<AudioListener>();
                    }

                    //Look for a camera to record from
                    if( cameraToRecord == null && GameObject.FindObjectOfType<Camera>() != null )
                    {
                        cameraToRecord = GameObject.FindObjectOfType<Camera>();
                    }

                    if( cameraToRecord != null )
                    {
                        eventReady = true;
                    }
                }
                else if( recordingType == RecordingType.BlockEventExternalCamera )
                {
                    if( blockEventExternalCamera != null )
                    {
                        eventReady = true;
                    }
                }
            }
            else if( action == Actions.StopVideoRecording )
            {
                eventReady = true;
            }
            else if( action == Actions.SaveImageToLocalStorage )
            {
                if( SaveImageUsingTexture() )
                {
                    eventReady = true;
                }
                else if( SaveImageUsingBlockEventExternalCamera() && blockEventExternalCamera != null )
                {
                    eventReady = true;
                }
                else if( SaveImageUsingUnityCamera() )
                {
                    if( unityCamera == null && Camera.main != null ) { unityCamera = Camera.main; }

                    if( unityCamera != null )
                    {
                        eventReady = true;
                    }
                }
            }
            else if( action == Actions.DeleteLocalStorage )
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
                if( action == Actions.StartVideoRecordingToLocalStorage )
                {
                    if( IsRecordingTypeUnityCamera() )
                    {
                        CallStartRecordingUnityCameraEvent();
                    }
                    else if( IsRecordingTypeCameraOrBlockEventExternalCamera() )
                    {
                        CallStartRecordingBlockEventExternalCamera();
                    }
                }
                else if( action == Actions.StopVideoRecording )
                {
                    CallStopRecordingEvent();
                }
                else if( action == Actions.SaveImageToLocalStorage )
                {
                    if( SaveImageUsingTexture() && textureToSave != null )
                    {
                        SaveImageEvent( textureToSave );
                    }
                    else if( SaveImageUsingBlockEventExternalCamera() && blockEventExternalCamera != null )
                    {
                        SaveImageUsingBlockEventExternalCameraEvent();
                    }
                    else if( SaveImageUsingUnityCamera() && unityCamera != null )
                    {
                        captureCameraTextureInLateUpdate = true;
                    }
                }
                else if( action == Actions.DeleteLocalStorage )
                {
                    DeleteLocalStorageEvent();
                }
            }

        } //END CallEvent

        //------------------------------//
        private void CallStartRecordingUnityCameraEvent()
        //------------------------------//
        {

#if NATCORDER
            //We can only record one source at a time
            if( NatCorder.IsRecording ) { return; }

            if( useDefaultSettings )
            {
                if( showDebug ) { Debug.Log( "BlockEventRecorder.cs Calling StartRecording() for default Unity Camera" ); }
                NatCorder.StartRecording( Container.MP4, VideoFormat.Screen, AudioFormat.Unity, OnRecordingComplete );
            }
            else
            {
                
                //Set our AudioFormat to the defaults
                int _sampleRate = AudioFormat.Unity.sampleRate;
                int _channelCount = AudioFormat.Unity.channelCount;

                //Check if we need to override our default AudioFormat
                if( IsUseDefaultAudioSettingsFalse() )
                {
                    _sampleRate = sampleRate;
                    _channelCount = channelCount;
                }

                //If our Video Container format is GIF, we cannot allow any kind of audio recording
                if( IsVideoContainerGIF() )
                {
                    NatCorder.StartRecording(
                    container,
                    GetVideoFormatForUnityCamera(),
                    AudioFormat.None,
                    OnRecordingComplete );
                }

                //Otherwise our audio should be based on the settings we selected
                else
                {
                    //Use the customized audio settings
                    if( IsUseDefaultAudioSettingsFalse() )
                    {
                        NatCorder.StartRecording(
                        container,
                        GetVideoFormatForUnityCamera(),
                        new AudioFormat( _sampleRate, _channelCount ),
                        OnRecordingComplete );
                    }

                    //Use the default audio settings
                    else
                    {
                        if( audioFormat == AudioRecordFormat.None )
                        {
                            NatCorder.StartRecording(
                            container,
                            GetVideoFormatForUnityCamera(),
                            AudioFormat.None,
                            OnRecordingComplete );
                        }
                        else if( audioFormat == AudioRecordFormat.Unity )
                        {
                            NatCorder.StartRecording(
                            container,
                            GetVideoFormatForUnityCamera(),
                            AudioFormat.Unity,
                            OnRecordingComplete );
                        }
                        
                    }
                }
            }

            //Start sending the texture data from the passed in camera to the recorder
            if( cameraToRecord != null )
            {
                cameraRecorder = null;
                cameraRecorder = CameraRecorder.Create( cameraToRecord );
            }

            //Start sending the audio data from the scene's AudioListener to the recorder
            if( audioListener != null )
            {
                audioRecorder = null;
                audioRecorder = AudioRecorder.Create( audioListener );
            }

            if( onRecordingStarted != null ) { onRecordingStarted.Invoke(); }
#else
            Debug.LogError( "BlockEventRecorder.cs CallStartRecordingUnityCameraEvent() missing NATCORDER scripting define symbol under project settings" );
#endif

        } //END CallStartRecordingUnityCameraEvent

#if NATCORDER
        //----------------------------------------------//
        private VideoFormat GetVideoFormatForUnityCamera()
        //----------------------------------------------//
        {

            //Set our VideoFormat to the defaults
            int _width = Screen.width;
            int _height = Screen.height;
            int _framerate = VideoFormat.Screen.framerate;
            int _bitrate = VideoFormat.Screen.bitrate;
            int _keyframeInterval = VideoFormat.Screen.keyframeInterval;

            //Check if we need to override the defaults
            if( IsUseDefaultVideoFormatFalse() )
            {
                _width = width;
                _height = height;
                _framerate = framerate;
                _bitrate = bitrate;
                _keyframeInterval = keyframeInterval;
            }

            return new VideoFormat( _width, _height, _framerate, _bitrate, _keyframeInterval );

        } //END GetVideoFormatForUnityCamera
#endif

#if NATCORDER
        //----------------------------------------------//
        private VideoFormat GetVideoFormatForBlockEventExternalCamera()
        //----------------------------------------------//
        {
            if( showDebug ) { Debug.Log( "BlockEventRecorder.cs GetVideoFormatForBlockEventExternalCamera() Screen(" + Screen.width + "x" + Screen.height + ") .. setting to ExtCam(" + blockEventExternalCamera.GetWidth() + "x" + blockEventExternalCamera.GetHeight() + ")" ); }

            //Set our VideoFormat to the defaults
            int _width = blockEventExternalCamera.GetWidth();
            int _height = blockEventExternalCamera.GetHeight();
            int _framerate = VideoFormat.Screen.framerate;
            int _bitrate = VideoFormat.Screen.bitrate;
            int _keyframeInterval = VideoFormat.Screen.keyframeInterval;

            //Check if we need to override the defaults
            if( IsUseDefaultVideoFormatFalse() )
            {
                _width = width;
                _height = height;
                _framerate = framerate;
                _bitrate = bitrate;
                _keyframeInterval = keyframeInterval;
            }

            return new VideoFormat( _width, _height, _framerate, _bitrate, _keyframeInterval );

        } //END GetVideoFormatForBlockEventExternalCamera
#endif

        //----------------------------------------------//
        private void CallStartRecordingBlockEventExternalCamera()
        //----------------------------------------------//
        {

#if NATCORDER
            //We can only record one source at a time
            if( NatCorder.IsRecording ) { return; }

            //We can only record if the BlockEventExternalCamera is playing
            if( !blockEventExternalCamera.IsPlaying() )
            {
                Debug.LogError( "BlockEventRecorder.cs CallStartRecordingBlockEventExternalCamera() We attempted to record from the attached BlockEventExternalCamera however it is not currently playing a camera stream" );
                return;
            }

            //Tell our recording to begin expecting new frames

            //If our Video Container format is GIF, we cannot allow any kind of audio recording
            if( IsVideoContainerGIF() )
            {
                NatCorder.StartRecording(
                container,
                GetVideoFormatForBlockEventExternalCamera(),
                AudioFormat.None,
                OnRecordingComplete );
            }

            //If we are skipping audio recording, just record video in .MP4 format and skip audio
            else if( IsExtCameraAudioFormatNone() )
            {
                NatCorder.StartRecording(
                container,
                GetVideoFormatForBlockEventExternalCamera(),
                AudioFormat.None,
                OnRecordingComplete );
            }

            //Otherwise our audio should be based on the AudioSource or AudioListener via using the AudioGetter.cs script to grab audio samples
            else
            {
                //If our AudioListener or AudioSource has been setup properly, we can use it to grab audio samples
                if( IsRecordAudioTypeAudioListener() && audioListener != null )
                {
                    NatCorder.StartRecording(
                    container,
                    GetVideoFormatForBlockEventExternalCamera(),
                    new AudioFormat( AudioSettings.outputSampleRate, (int)AudioSettings.speakerMode ),
                    OnRecordingComplete );

                    //Remove any pre-existing audioGetter's
                    if( audioGetter != null )
                    {
                        Destroy( audioGetter );
                        audioGetter = null;
                    }

                    if( audioListener.gameObject.GetComponent<AudioGetter>() != null )
                    {
                        Destroy( audioListener.gameObject.GetComponent<AudioGetter>() );
                    }

                    //Add an AudioGetter to the linked AudioListener to recieve it's audio samples for encoding into the video
                    audioGetter = audioListener.gameObject.AddComponent<AudioGetter>();
                    audioGetter.StartRecordingAudio( this );
                }

                else if( IsRecordAudioTypeAudioSource() && audioSource != null )
                {
                    NatCorder.StartRecording(
                    container,
                    GetVideoFormatForBlockEventExternalCamera(),
                    new AudioFormat( AudioSettings.outputSampleRate, (int)AudioSettings.speakerMode ),
                    OnRecordingComplete );

                    //Remove any pre-existing audioGetter's
                    if( audioGetter != null )
                    {
                        Destroy( audioGetter );
                        audioGetter = null;
                    }

                    if( audioSource.gameObject.GetComponent<AudioGetter>() != null )
                    {
                        Destroy( audioSource.gameObject.GetComponent<AudioGetter>() );
                    }

                    //Add an AudioGetter to the linked AudioSource to recieve it's audio samples for encoding into the video
                    audioGetter = audioSource.gameObject.AddComponent<AudioGetter>();
                    audioGetter.StartRecordingAudio( this );
                }

                //If BlockAudio is selected...
                else if( IsRecordAudioTypeBlockAudio() && blockAudio != null )
                {
                    NatCorder.StartRecording(
                    container,
                    GetVideoFormatForBlockEventExternalCamera(),
                    new AudioFormat( AudioSettings.outputSampleRate, (int)AudioSettings.speakerMode ),
                    OnRecordingComplete );

                    //Remove any pre-existing audioGetter's
                    if( audioGetter != null )
                    {
                        Destroy( audioGetter );
                        audioGetter = null;
                    }

                    if( blockAudio.gameObject.GetComponent<AudioGetter>() != null )
                    {
                        Destroy( blockAudio.gameObject.GetComponent<AudioGetter>() );
                    }

                    //Add an AudioGetter to the linked blockAudio to recieve it's audio samples for encoding into the video
                    audioGetter = blockAudio.gameObject.AddComponent<AudioGetter>();
                    audioGetter.StartRecordingAudio( this );
                }

                //Otherwise set our AudioFormat to None, we do not need to recieve audio samples
                else
                {
                    NatCorder.StartRecording(
                    container,
                    GetVideoFormatForBlockEventExternalCamera(),
                    AudioFormat.None,
                    OnRecordingComplete );
                }
                
            }
            
            //Request the frames from the ExternalCamera, these frames are sent to _ExternalCameraFrameRecieved
            blockEventExternalCamera.RequestFrames( this );
#else
            Debug.LogError( "BlockEventRecorder.cs CallStartRecordingBlockEventExternalCamera() missing NATCORDER scripting define symbol under project settings" );
#endif

        } //END CallStartRecordingBlockEventExternalCamera

        //-------------------------------//
        private void OnRecordingComplete( string path )
        //-------------------------------//
        {
            //If our PersistentDataPath/VideoRecordings path does not exist, create it now
            if( !Directory.Exists( DatabaseStringHelper.CreatePersistentDataPath("VideoRecordings") ) )
            {
                Directory.CreateDirectory( DatabaseStringHelper.CreatePersistentDataPath("VideoRecordings") );
            }

            //Move the recording into the PersistentData folder under the 'VideoRecordings' subfolder
            if ( File.Exists( path ) )
            {
                FileInfo fileInfo = new FileInfo( path );

                if( fileInfo != null )
                {
                    File.Move( path, DatabaseStringHelper.CreatePersistentDataPath("VideoRecordings/" + fileInfo.Name ) );
                    path = DatabaseStringHelper.CreatePersistentDataPath("VideoRecordings/" + fileInfo.Name );
                }
            }

            if( showDebug ) { Debug.Log( "BlockEventRecorder.cs OnRecordingComplete() adding path to list of captured videos = " + path ); }

            //Add our path to our list of recorded videos
            if( recordedVideos == null ) { recordedVideos = new List<string>(); }

            recordedVideos.Add( path );

#if NATCORDER
            //If we were capturing images from a passed in camera, dispose of the cameraRecorder at this time to free up memory
            if( cameraRecorder != null )
            {
                cameraRecorder.Dispose();
                cameraRecorder = null;
            }
#endif

            //If we were capturing audio samples from an AudioListener or AudioSource, destroy the AudioGetter.cs helper script attached to it
            if( audioGetter != null )
            {
                Destroy( audioGetter );
                audioGetter = null;
            }

#if NATCORDER
            //If we were capturing audio from the app, dispose of the audioRecorder at this time
            if( audioRecorder != null )
            {
                audioRecorder.Dispose();
                audioRecorder = null;
            }
#endif

            if( onRecordingCompleted != null ) { onRecordingCompleted.Invoke( path ); }

        } //END OnRecordingComplete

        //-----------------------------------------//
        public void ExternalCameraFrameRecieved( Texture preview )
        //-----------------------------------------//
        {

#if NATCORDER
            if( preview != null )
            {
                //if( showDebug ) Debug.Log( "Adding frame = " + preview.width + "x" + preview.height );
                
                //Grab the current frame that we will encode video onto
                var frame = NatCorder.AcquireFrame();

                //Copy the current video texture to the frame
                Graphics.Blit( preview, frame );

                //Send the newly created frame to be added to the overall video
                NatCorder.CommitFrame( frame );
            }
            else
            {
                Debug.LogError( "BlockEventRecorder.cs ExternalCameraFrameRecieved() preview camera texture sent in by BlockEventExternalCamera was null! Unable to commit this frame" );
            }
#else
            Debug.LogError( "BlockEventRecorder.cs ExternalCameraFrameRecieved() missing NATCORDER scripting define symbol under project settings" );
#endif

        } //END ExternalCameraFrameRecieved


        //-------------------------------------------//
        /// <summary>
        /// Sent in via a AudioGetter.cs script. Recieves audio samples from an Internal AudioListener or AudioSource
        /// </summary>
        /// <param name="samples">The audio samples to add</param>
        /// <param name="timestamp">The current timestamp for the audio samples</param>
        public void InternalAudioSampleRecieved( float[] samples, long timestamp )
        //-------------------------------------------//
        {

#if NATCORDER
            //if( showDebug ) Debug.Log( "BlockEventRecorder.cs InternalAudioSampleRecieved() calling CommitSamples. samples.length = " + samples.Length + ", timestamp = " + timestamp );
            
            // Send to NatCorder for encoding
            NatCorder.CommitSamples( samples, timestamp );
#else
            Debug.LogError( "BlockEventRecorder.cs InternalAudioSampleRecieved() missing NATCORDER scripting define symbol under project settings" );
#endif

        } //END InternalAudioSampleRecieved

        //------------------------------//
        private void CallStopRecordingEvent()
        //------------------------------//
        {

#if NATCORDER
            if( NatCorder.IsRecording )
            {
                if( showDebug ) Debug.Log( "BlockEventRecorder.cs CallStopRecordingEvent() calling NatCorder.StopRecording()" );

                //If we were capturing audio samples from an AudioListener or AudioSource, destroy the AudioGetter.cs helper script attached to it
                if( audioGetter != null )
                {
                    audioGetter.StopRecordingAudio();
                    Destroy( audioGetter );
                    audioGetter = null;
                }

                //Stop the recording and convert the samples into a completed video file
                NatCorder.StopRecording();
                
                if( onActionCompleted != null ) { onActionCompleted.Invoke(); }
            }
#else
            Debug.LogError( "BlockEventRecorder.cs CallStopRecordingEvent() missing NATCORDER scripting define symbol under project settings" );
#endif

        } //END CallStopRecordingEvent

        //--------------------------------//
        public void SaveImageEvent( Texture tex )
        //--------------------------------//
        {
            if( tex != null )
            {
                //If our PersistentDataPath/ImageRecordings path does not exist, create it now
                if( !Directory.Exists( DatabaseStringHelper.CreatePersistentDataPath("ImageRecordings") ) )
                {
                    Directory.CreateDirectory( DatabaseStringHelper.CreatePersistentDataPath("ImageRecordings") );
                }

                string path = DatabaseStringHelper.CreatePersistentDataPath("ImageRecordings/" + fileName + ".png" );

                if( UseGeneratedPath() )
                {
                    path = DatabaseStringHelper.CreatePersistentDataPath("ImageRecordings/photo" + DateTime.Now.ToString( "dd-mm-yyyy-hh-mm-ss" ) + ".png" );
                }

                if( showDebug ) { Debug.Log( "BlockEventRecorder.cs SaveImageEvent() path = " + path ); }
                File.WriteAllBytes( path, ( tex as Texture2D ).EncodeToPNG() );

                //Add our image path to our list of saved images
                if( capturedPhotos == null ) { capturedPhotos = new List<string>(); }

                capturedPhotos.Add( path );

                //If there's an action to perform after saving this image, then do it
                if( onActionCompleted != null ) { onActionCompleted.Invoke(); }
            }

        } //END SaveImageEvent

        //--------------------------------//
        private void SaveImageUsingBlockEventExternalCameraEvent()
        //--------------------------------//
        {
            //Let the BlockEventExternalCamera know that we need the texture for the captured photo when it gets one
            if( blockEventExternalCamera != null )
            {
                blockEventExternalCamera.RequestCapturedPhoto( this );
            }

        } //END SaveImageUsingBlockEventExternalCameraEvent

        //--------------------------------//
        private void LateUpdate()
        //--------------------------------//
        {
            //When the CallEvent() occurs and we're trying to capture a texture from the Unity Camera, 
            //we wait until LateUpdate() occurs so we can make sure the scene is done rendering
            if( captureCameraTextureInLateUpdate )
            {
                captureCameraTextureInLateUpdate = false;
                SaveImageUsingUnityCameraEvent();
            }

        } //END LateUpdate

        //--------------------------------//
        private void SaveImageUsingUnityCameraEvent()
        //--------------------------------//
        {
            //If the camera is not defined, try to use the main camera
            if( Camera.main != null && unityCamera == null ) { unityCamera = Camera.main; }

            //Request the camera texture
            if( unityCamera != null )
            {
                //Create a render texture and capture the unity camera texture on it
                RenderTexture renderTexture = new RenderTexture( Screen.width, Screen.height, 24 );
                unityCamera.targetTexture = renderTexture;

                //Force the camera to render
                unityCamera.Render();

                //Set our active renderTexture
                RenderTexture.active = renderTexture;

                //Create a new texture from the renderTexture
                Texture2D screenShot = new Texture2D( Screen.width, Screen.height, TextureFormat.RGB24, false );

                screenShot.ReadPixels( new Rect( 0, 0, Screen.width, Screen.height ), 0, 0 );

                //Remove the renderTexture from our camera and set it to null
                unityCamera.targetTexture = null;
                RenderTexture.active = null;
                Destroy( renderTexture );

                SaveImageEvent( screenShot );
            }

        } //END SaveImageUsingUnityCameraEvent


        //--------------------------------//
        public void DeleteLocalStorageEvent()
        //--------------------------------//
        {

            //Delete all of the videos and photos at our persistent local storage
            if( Directory.Exists( DatabaseStringHelper.CreatePersistentDataPath( "RecordedMedia" ) ) )
            {
                DeleteRecordedOrCapturedFiles( new DirectoryInfo( DatabaseStringHelper.CreatePersistentDataPath( "RecordedMedia" ) ) );
            }
            
            //Find all of the BlockEventRecorders in the scene and wipe their lists of saved photos and videos
            if( GameObject.FindObjectsOfType<BlockEventRecorder>() != null && GameObject.FindObjectsOfType<BlockEventRecorder>().Length > 1 )
            {
                List<BlockEventRecorder> recorders = GameObject.FindObjectsOfType<BlockEventRecorder>().ToList();

                foreach( BlockEventRecorder recorder in recorders )
                {
                    if( recorder != null )
                    {
                        recorder.ClearListsOfLocalStorageFiles();
                    }
                }
            }

            //There is only this BlockEventRecorder, wipe its lists
            else
            {
                ClearListsOfLocalStorageFiles();
            }

        } //END DeleteLocalStorageEvent
        
        //--------------------------------//
        private void DeleteRecordedOrCapturedFiles( DirectoryInfo directory )
        //--------------------------------//
        {

            if( directory != null )
            {
                //Get all the files in this directory
                FileInfo[] fileInfo = directory.GetFiles();

                if( fileInfo != null && fileInfo.Length > 0 )
                {
                    //Iterate through each file
                    foreach( FileInfo info in fileInfo )
                    {
                        if( info != null )
                        {
                            //Should we delete the this file?
                            if( deleteType == DeleteType.Photos &&
                                     ( info.Extension == ".png" || info.Extension == ".PNG" ) )
                            {
                                info.Delete();
                            }
                            else if( deleteType == DeleteType.MP4s &&
                                     ( info.Extension == ".mp4" || info.Extension == ".MP4" ) )
                            {
                                info.Delete();
                            }
                            else if( deleteType == DeleteType.GIFs &&
                                     ( info.Extension == ".gif" || info.Extension == ".GIF" ) )
                            {
                                info.Delete();
                            }
                            else if( deleteType == DeleteType.All )
                            {
                                info.Delete();
                            }
                        }
                    }
                }
            }
            

        } //END DeleteRecordedOrCapturedFiles

        //--------------------------------//
        public void ClearListsOfLocalStorageFiles()
        //--------------------------------//
        {

            recordedVideos = new List<string>();

            capturedPhotos = new List<string>();

        } //END ClearListsOfLocalStorageFiles

        //--------------------------------//
        public void DebugLogPath( string path )
        //--------------------------------//
        {
            Debug.Log( path );

        } //END DebugLogPath


    } //END BlockEventRecorder

} //END Namespace