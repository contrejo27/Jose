using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using System;

using Sirenix.OdinInspector;

#if UNITY_EDITOR
using UnityEditor;
#endif


#if NATMIC
using NatMicU.Core;
using NatMicU.Core.Recorders;
using System.IO;
#endif


namespace BrandXR
{
    public class BlockEventMicrophone : BlockEventBase
    {
        
        #region WARNINGS
        [Button("Add Necessary Scripting Define Symbols", ButtonSizes.Large), ShowIf("ShowScriptingDefineSymbolWarning"), InfoBox("WARNING: This script requires the NATMIC scripting define symbol be defined in your Unity Project Settings and that the NatMic plugin be in your Assets folder to work properly", InfoMessageType.Error)]
        public void AddScriptingDefineSymbols()
        {
#if UNITY_EDITOR && !NATMIC
            string newDefineSymbols = PlayerSettings.GetScriptingDefineSymbolsForGroup
                ( BuildPipeline.GetBuildTargetGroup( EditorUserBuildSettings.activeBuildTarget ) );

            if( !newDefineSymbols.Contains( "NATMIC" ) )
            {
                //Debug.Log( "Define symbols = " + newDefineSymbols );
                newDefineSymbols += ";NATMIC";
            }

            PlayerSettings.SetScriptingDefineSymbolsForGroup( BuildPipeline.GetBuildTargetGroup
                ( EditorUserBuildSettings.activeBuildTarget ), newDefineSymbols );
#endif
        }

        private bool ShowScriptingDefineSymbolWarning()
        {
#if NATMIC && UNITY_EDITOR
            if( AssetDatabase.IsValidFolder( "Assets/NatMic" ) )
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
        #endregion

        #region SETUP
        //----------------- ACTIONS --------------------------------//
        public enum Actions
        {
            None,
            StartRecording,
            StopRecording,
            DeleteRecordingsInLocalStorage,
            AttachRecordingToAudioSource
        }
        [TitleGroup("Block Event - Microphone Recording", "Used to record audio from the external microphone")]
        public Actions action = Actions.None;
        private bool IsActionNone() { return action == Actions.None; }
        private bool IsActionStartRecording() { return action == Actions.StartRecording; }
        private bool IsActionStopRecording() { return action == Actions.StopRecording; }
        private bool IsActionDeleteRecording() { return action == Actions.DeleteRecordingsInLocalStorage; }
        private bool IsActionAttachRecording() { return action == Actions.AttachRecordingToAudioSource; }
        #endregion

        #region START RECORDING VARIABLES
        //-------------------------- START RECORDING VARIABLES --------------------------------------------------//
        public enum RecordTo
        {
            File,
            BlockEventRecorder
        }
        [Space( 15f ), ShowIf("IsActionStartRecording"), InfoBox("If you choose to save to the BlockEventRecorder,")]
        public RecordTo recordTo = RecordTo.File;
        private bool RecordToFile() { return IsActionStartRecording() && recordTo == RecordTo.File; }
        private bool RecordToBlockEventRecorder() { return IsActionStartRecording() && recordTo == RecordTo.BlockEventRecorder; }

#if NATMIC
        //Used to save the recording to local storage
        private IRecorder micRecorder = null;
#endif

        public enum OverlayAudioOverMicrophone
        {
            None,
            AudioListener,
            AudioSource
        }
        [Space(15f), ShowIf("IsActionStartRecording"), InfoBox("You can choose to mix the microphone recording with the audio from an AudioListener or AudioSource")]
        public OverlayAudioOverMicrophone overlayAudioOverMicrophone = OverlayAudioOverMicrophone.None;
        private bool ShouldOverlayAudioListenerOverMicrophone() { return IsActionStartRecording() && overlayAudioOverMicrophone == OverlayAudioOverMicrophone.AudioListener; }
        private bool ShouldOverlayAudioSourceOverMicrophone() { return IsActionStartRecording() && overlayAudioOverMicrophone == OverlayAudioOverMicrophone.AudioSource; }

        [ShowIf("ShouldOverlayAudioListenerOverMicrophone")]
        public AudioListener overlayAudioListener = null;

        [ShowIf("ShouldOverlayAudioSourceOverMicrophone")]
        public AudioSource overlayAudioSource = null;

        public enum FileNameSettings
        {
            AutomaticallyGenerated,
            CustomFileName
        }
        [Space(15f), ShowIf("RecordToFile")]
        public FileNameSettings fileNameSettings = FileNameSettings.AutomaticallyGenerated;
        private bool IsFileNameAutomaticallyGenerated() { return RecordToFile() && fileNameSettings == FileNameSettings.AutomaticallyGenerated; }
        private bool IsFileNameCustom() { return RecordToFile() && fileNameSettings == FileNameSettings.CustomFileName; }

        [ShowIf("IsFileNameCustom")]
        public string customFileName = "";

        [Space( 15f ), ShowIf("RecordToBlockEventRecorder")]
        public BlockEventRecorder blockEventRecorder = null;

        public enum RecordingFormat
        {
            Default,
            DefaultForAudioMixing,
            Custom
        }
        [Space(15f), ShowIf("IsActionStartRecording")]
        public RecordingFormat recordingFormat = RecordingFormat.Default;
        private bool IsRecordingFormatDefault() { return IsActionStartRecording() && recordingFormat == RecordingFormat.Default; }
        private bool IsRecordingFormatDefaultForAudioMixing() { return IsActionStartRecording() && recordingFormat == RecordingFormat.DefaultForAudioMixing; }
        private bool IsRecordingFormatCustom() { return IsActionStartRecording() && recordingFormat == RecordingFormat.Custom; }

        [ShowIf("IsRecordingFormatCustom")]
        public int sampleRate = 0;

        [ShowIf("IsRecordingFormatCustom")]
        public int channelCount = 2;

        public enum RecordingFileType
        {
            WAV
        }
        [Space(15f), ShowIf("RecordToFile")]
        public RecordingFileType recordingFileType = RecordingFileType.WAV;

        [Space(15f), ShowIf("RecordToFile"), InfoBox("Do not modify this list of recordings, it is automatically generated when recording to a file and serves as a reference. Only contains audio recordings from this session")]
        public List<string> audioRecordingsInLocalStorage = new List<string>();
        #endregion

        #region STOP RECORDING VARIABLES
        //--------------------------- STOP RECORDING VARIABLES --------------------------------------------------//
        [Space(15f), ShowIf("IsActionStopRecording"), InfoBox("Connect this variable to a BlockEventMicrophone component that is set to the 'StartRecording' action")]
        public BlockEventMicrophone stopBlockEventMicrophone = null;
        #endregion

        #region DELETE RECORDING VARIABLES
        //------------------------------------- DELETE RECORDINGS VARIABLES ---------------------------------------//
        public enum DeleteSettings
        {
            DeleteAllInLocalStorage,
            DeleteRecordingsByName,
            DeleteLastRecordingInBlockEventMicrophone,
            DeleteAllRecordingsInBlockEventMicrophone
        }
        [Space(15f), ShowIf("IsActionDeleteRecording")]
        public DeleteSettings deleteSettings = DeleteSettings.DeleteAllInLocalStorage;
        private bool IsSetToDeleteAllInLocalStorage() { return IsActionDeleteRecording() && deleteSettings == DeleteSettings.DeleteAllInLocalStorage; }
        private bool IsSetToDeleteRecordingsByName() { return IsActionDeleteRecording() && deleteSettings == DeleteSettings.DeleteRecordingsByName; }
        private bool IsSetToDeleteLastRecordingInBlockEventMicrophone() { return IsActionDeleteRecording() && deleteSettings == DeleteSettings.DeleteLastRecordingInBlockEventMicrophone; }
        private bool IsSetToDeleteAllRecordingsInBlockEventMicrophone() { return IsActionDeleteRecording() && deleteSettings == DeleteSettings.DeleteAllRecordingsInBlockEventMicrophone; }
        private bool IsSetToDeleteLastorAllRecordingsInBlockEventMicrophone() { return IsActionDeleteRecording() && 
                ( deleteSettings == DeleteSettings.DeleteLastRecordingInBlockEventMicrophone || deleteSettings == DeleteSettings.DeleteAllRecordingsInBlockEventMicrophone ); }

        [Space(15f), ShowIf("IsSetToDeleteRecordingsByName")]
        public List<string> namesOfRecordingsToDelete = new List<string>();

        [Space(15f), ShowIf("IsSetToDeleteLastorAllRecordingsInBlockEventMicrophone")]
        public BlockEventMicrophone deleteFromBlockEventMicrophone = null;

        #endregion

        #region ATTACH RECORDING VARIABLES
        public enum AttachTo
        {
            AudioSource,
            BlockAudio
        }
        [Space(15f), ShowIf("IsActionAttachRecording")]
        public AttachTo attachTo = AttachTo.AudioSource;
        private bool IsAttachToAudioSource() { return IsActionAttachRecording() && attachTo == AttachTo.AudioSource; }
        private bool IsAttachToBlockAudio() { return IsActionAttachRecording() && attachTo == AttachTo.BlockAudio; }

        [Space(15f), ShowIf("IsActionAttachRecording"), InfoBox("Link this to the BlockEventMicrophone that is set to start recording audio")]
        public BlockEventMicrophone attachRecordingFromBlockEventMicrophone = null;

        [Space(15f), ShowIf("IsAttachToAudioSource")]
        public AudioSource attachToAudioSource = null;

        [Space(15f), ShowIf("IsAttachToBlockAudio")]
        public BlockAudio attachToBlockAudio = null;

        #endregion

        #region START RECORDING EVENT MESSAGES
        //--------------------------- START RECORDING EVENT MESSAGES ----------------------------------------------------//
        [SerializeField, ShowIf("IsActionStartRecording"), FoldoutGroup("Event Messages")]
        public UnityEvent onRecordingStart = new UnityEvent();

#if NATMIC
        //DOB NOTE: Disabled, the PlaybackBuffer occurs on seperate thread, so we cannot call a UnityEvent from it
        //[Serializable]
        //public class OnBufferVariables : UnityEvent<NatMicU.Core.AudioEvent, float[], long, NatMicU.Core.Format> { };

        //[Space(15f), SerializeField, ShowIf("IsActionStartRecording"), FoldoutGroup("Event Messages"), InfoBox("Sends the\n( AudioEvent, float[] sampleBuffer, long timestamp, AudioFormat )\nwhile recording is ongoing")]
        //public OnBufferVariables onPlaybackBufferEvent = new OnBufferVariables();
#endif

        [Serializable]
        public class RecordToFileComplete : UnityEvent<string> { };

        [Space( 15f ), SerializeField, ShowIf("IsActionStartRecording"), FoldoutGroup("Event Messages"), InfoBox("Sends the path to the recorded file when recording is stopped")]
        public RecordToFileComplete onRecordToFileComplete = new RecordToFileComplete();
        #endregion

        #region GENERIC EVENT MESSAGES
        //--------------------------- GENERIC EVENT MESSAGES ------------------------------------------------------//
        private bool ShowOnActionCompleted() { return action == Actions.StopRecording || action == Actions.DeleteRecordingsInLocalStorage || action == Actions.AttachRecordingToAudioSource; }

        [ShowIf("IsActionStopRecording"), FoldoutGroup("Event Messages")]
        public UnityEvent onActionCompleted = new UnityEvent();
        #endregion

        #region SETUP METHODS
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
            return EventType.Microphone;

        } //END GetEventType

        //---------------------------------------------------------//
        public void SetAction(Actions action)
        //---------------------------------------------------------//
        {

            this.action = action;

        } //END SetAction

        //-------------------------------//
        public override void PrepareEvent()
        //-------------------------------//
        {

            if( action == Actions.StartRecording )
            {
                if( RecordToFile() )
                {
                    eventReady = true;
                }
                else if( RecordToBlockEventRecorder() && blockEventRecorder != null )
                {
                    eventReady = true;
                }
            }
            else if( action == Actions.StopRecording )
            {
                if( stopBlockEventMicrophone != null )
                {
                    eventReady = true;
                }
            }
            else if( action == Actions.DeleteRecordingsInLocalStorage )
            {
                if(IsSetToDeleteLastorAllRecordingsInBlockEventMicrophone() )
                {
                    if(deleteFromBlockEventMicrophone != null )
                    {
                        eventReady = true;
                    }
                }
                else
                {
                    eventReady = true;
                }
            }
            else if(action == Actions.AttachRecordingToAudioSource )
            {
                if(attachRecordingFromBlockEventMicrophone != null )
                {
                    if (IsAttachToAudioSource() && attachToAudioSource != null)
                    {
                        eventReady = true;
                    }
                    else if( IsAttachToBlockAudio() && attachToBlockAudio != null )
                    {
                        eventReady = true;
                    }
                }
                
            }

        } //END PrepareEvent
        #endregion

        #region CALL EVENT
        //-------------------------------//
        protected override void _CallEvent()
        //-------------------------------//
        {
            base._CallEvent();

            if (eventReady)
            {
                if( action == Actions.StartRecording )
                {
                    CallStartRecordingMicrophoneEvent();
                }
                else if( action == Actions.StopRecording )
                {
                    CallStopRecordingMicrophoneEvent();
                }
                else if( action == Actions.DeleteRecordingsInLocalStorage )
                {
                    if( IsSetToDeleteAllInLocalStorage() )
                    {
                        CallDeleteAllRecordingsEvent();
                    }
                    else if( IsSetToDeleteRecordingsByName() )
                    {
                        CallDeleteRecordingsByNameEvent();
                    }
                    else if( IsSetToDeleteLastRecordingInBlockEventMicrophone() )
                    {
                        CallDeleteLastRecordingInBlockEventMicrophoneEvent();
                    }
                    else if( IsSetToDeleteAllRecordingsInBlockEventMicrophone() )
                    {
                        CallDeleteAllRecordingsInBlockEventMicrophoneEvent();
                    }
                    
                }
                else if( action == Actions.AttachRecordingToAudioSource )
                {
                    if( IsAttachToAudioSource() )
                    {
                        CallAttachRecordingToAudioSourceEvent();
                    }
                    else if( IsAttachToBlockAudio() )
                    {
                        CallAttachRecordingToBlockAudioEvent();
                    }
                    
                }
            }

        } //END _CallEvent
        #endregion

        #region START RECORDING EVENT
        //------------------------------------------------------//
        private void CallStartRecordingMicrophoneEvent()
        //------------------------------------------------------//
        {

#if NATMIC
            if( !NatMic.IsRecording )
            {
                if (onRecordingStart != null) { onRecordingStart.Invoke(); }

                if (ShouldOverlayAudioListenerOverMicrophone() && overlayAudioListener != null)
                {
                    NatMic.StartRecording( overlayAudioListener, GetAudioFormat(), OnSampleRecieved );
                }
                else if (ShouldOverlayAudioSourceOverMicrophone() && overlayAudioSource != null)
                {
                    NatMic.StartRecording( overlayAudioSource, GetAudioFormat(), OnSampleRecieved );
                }
                else
                {
                    NatMic.StartRecording( GetAudioFormat(), OnSampleRecieved );
                }
            }
            else
            {
                Debug.LogError("BlockEventMicrophone.cs CallStartRecordingMicrophoneEvent() ERROR: Microphone is already recording, you can only have one microphone recording event at a time.");
            }
#else
            Debug.LogError("BlockEventMicrophone.cs CallStartRecordingMicrophoneEvent() ERROR: Unable to begin microphone recording. NATMIC scripting define symbol is missing");
#endif

        } //END CallStartRecordingMicrophoneEvent

#if NATMIC
        //-------------------------------------------------------//
        private Format GetAudioFormat()
        //-------------------------------------------------------//
        {

            //Create the audioFormat based on our settings
            Format audioFormat = new Format();

            if (IsRecordingFormatDefault())
            {
                audioFormat = Format.Default;
            }
            else if (IsRecordingFormatDefaultForAudioMixing())
            {
                audioFormat = Format.DefaultForMixing;
            }
            else if (IsRecordingFormatCustom())
            {
                audioFormat.channelCount = channelCount;
                audioFormat.sampleRate = sampleRate;
            }

            return audioFormat;

        } //END GetAudioFormat
#endif
        
#if NATMIC
        //-------------------------------------------------------//
        private void OnSampleRecieved( AudioEvent audioEvent, float[] sampleBuffer, long timestamp, Format format )
        //-------------------------------------------------------//
        {

            if( audioEvent == AudioEvent.OnInitialize )
            {
                if( RecordToFile() )
                {
                    // Create a WAV recorder to record the audio to a file
                    if( recordingFileType == RecordingFileType.WAV )
                    {
                        micRecorder = new WAVRecorder(format);
                    }
                    else
                    {
                        micRecorder = new WAVRecorder(format);
                    }
                    
                    micRecorder.StartRecording( OnRecordToFileComplete );
                }

                //if (onRecordingStart != null) { onRecordingStart.Invoke(); }
            }
            else if( audioEvent == AudioEvent.OnSampleBuffer )
            {
                if( RecordToFile() )
                {
                    // Commit the sample buffer to the WAV recorder
                    micRecorder.CommitSamples( sampleBuffer, timestamp );
                }

                //Disabled, this method runs in a coroutine so we can't call a UnityEvent method from it!
                //if (onPlaybackBufferEvent != null) { onPlaybackBufferEvent.Invoke(audioEvent, sampleBuffer, timestamp, format); }
            }
            else if( audioEvent == AudioEvent.OnFinalize )
            {
                if( RecordToFile() )
                {
                    // Stop recording the WAV file and dispose the recorder
                    micRecorder.Dispose();
                }
            }

        } //END OnSampleRecieved

        //------------------------------------------------//
        private void OnRecordToFileComplete( string path )
        //------------------------------------------------//
        {
            
            //If our PersistentDataPath/MicrophoneRecordings path does not exist, create it now
            if (!Directory.Exists( DatabaseStringHelper.CreatePersistentDataPath("MicrophoneRecordings") ))
            {
                Directory.CreateDirectory( DatabaseStringHelper.CreatePersistentDataPath("MicrophoneRecordings") );
            }

            //Move the recording into the PersistentData folder under the 'MicrophoneRecordings' subfolder
            if( File.Exists(path) )
            {
                FileInfo fileInfo = new FileInfo(path);

                if (fileInfo != null)
                {
                    string name = fileInfo.Name;

                    if( IsFileNameAutomaticallyGenerated() )
                    {
                        name = DateTime.Now.ToString("dd-mm-yyyy-hh-mm-ss") + ".wav";
                    }
                    else if( IsFileNameCustom() && customFileName != "" )
                    {
                        name = customFileName + ".wav";
                    }

                    File.Move(path, DatabaseStringHelper.CreatePersistentDataPath("MicrophoneRecordings/" + name));
                    path = DatabaseStringHelper.CreatePersistentDataPath("MicrophoneRecordings/" + name);
                }
            }

            //Store the final path of the audio file for reference
            if ( audioRecordingsInLocalStorage == null ) { audioRecordingsInLocalStorage = new List<string>(); }

            audioRecordingsInLocalStorage.Add( path );

            //Let other system know that the recording has completed
            if( onRecordToFileComplete != null) { onRecordToFileComplete.Invoke( path ); }

        } //END OnRecordToFileComplete

#endif

        #endregion

        #region STOP RECORDING EVENT
        //-------------------------------------------------------//
        private void CallStopRecordingMicrophoneEvent()
        //-------------------------------------------------------//
        {

#if NATMIC
            if(NatMic.IsRecording)
            {
                NatMic.StopRecording();

                if(onActionCompleted != null) { onActionCompleted.Invoke(); }
            }
#endif

        } //END CallStopRecordingMicrophoneEvent
        #endregion

        #region DELETE RECORDING EVENT

        //----------------------------------------//
        private void CallDeleteAllRecordingsEvent()
        //----------------------------------------//
        {



        } //END CallDeleteRecordingsEvent

        //---------------------------------------//
        private void CallDeleteRecordingsByNameEvent()
        //---------------------------------------//
        {



        } //END CallDeleteRecordingsByNameEvent

        //---------------------------------------//
        private void CallDeleteLastRecordingInBlockEventMicrophoneEvent()
        //---------------------------------------//
        {



        } //END CallDeleteLastRecordingInBlockEventMicrophoneEvent

        //---------------------------------------//
        private void CallDeleteAllRecordingsInBlockEventMicrophoneEvent()
        //---------------------------------------//
        {



        } //END CallDeleteAllRecordingsInBlockEventMicrophoneEvent

        #endregion

        #region ATTACH RECORDING EVENT

        //----------------------------------------------//
        public void CallAttachRecordingToAudioSourceEvent()
        //----------------------------------------------//
        {



        } //END CallAttachRecordingToAudioSourceEvent

        //----------------------------------------------//
        public void CallAttachRecordingToBlockAudioEvent()
        //----------------------------------------------//
        {



        } //END CallAttachRecordingToBlockAudioEvent

        #endregion

    } //END Class

} //END Namespace