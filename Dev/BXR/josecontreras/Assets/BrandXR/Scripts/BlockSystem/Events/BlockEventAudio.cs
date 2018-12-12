using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.Events;

namespace BrandXR
{
    public class BlockEventAudio: BlockEventBase
    {
        //----------------- ACTIONS --------------------------------//
        public enum Actions
        {
            None,
            ChangeAudio,
            PlayBlockAudio,
            StopBlockAudio
        }

        [TitleGroup( "Block Event - Audio", "Used to modify Audio Blocks or Audio Sources" )]
        public Actions action = Actions.ChangeAudio;
        private bool ShowChangeAudioAction() { return action == Actions.ChangeAudio; }

        //------------- VARIABLES ---------------------------------//
        public enum SetNewAudioClipOn
        {
            AudioSource,
            BlockAudio
        }
        
        [ShowIf( "ShowChangeAudioAction" ), FoldoutGroup( "Audio Settings" )]
        public SetNewAudioClipOn setNewAudioClipOn = SetNewAudioClipOn.AudioSource;
        private bool IsSetNewAudioClipOnAudioSource() { return ShowChangeAudioAction() && setNewAudioClipOn == SetNewAudioClipOn.AudioSource; }
        private bool IsSetNewAudioClipOnAudioBlock() { return ShowChangeAudioAction() && setNewAudioClipOn == SetNewAudioClipOn.BlockAudio; }

        private bool ShowChangeThisAudioBlock() { return IsSetNewAudioClipOnAudioBlock() || action == Actions.PlayBlockAudio || action == Actions.StopBlockAudio; }

        [ShowIf( "IsSetNewAudioClipOnAudioSource" ), FoldoutGroup( "Audio Settings" )]
        public AudioSource changeThisAudioSource;

        [ShowIf( "ShowChangeThisAudioBlock" ), FoldoutGroup( "Audio Settings" )]
        public BlockAudio changeThisAudioBlock;

        public enum ChangeAudioUsing
        {
            AudioClip,
            Path
        }

        [ShowIf( "ShowChangeAudioAction" ), FoldoutGroup( "Audio Settings" )]
        public ChangeAudioUsing changeAudioUsing = ChangeAudioUsing.AudioClip;
        private bool IsChangeAudioUsingAudioClip() { return ShowChangeAudioAction() && changeAudioUsing == ChangeAudioUsing.AudioClip; }
        private bool IsChangeAudioUsingPath() { return ShowChangeAudioAction() && changeAudioUsing == ChangeAudioUsing.Path; }

        [ShowIf( "IsChangeAudioUsingAudioClip" ), FoldoutGroup( "Audio Settings" )]
        public AudioClip changeToAudioClip;

        [ShowIf( "IsChangeAudioUsingPath" ), FoldoutGroup( "Audio Settings" )]
        public string audioPath;

        [ShowIf( "IsChangeAudioUsingPath" ), FoldoutGroup( "Audio Settings" )]
        public bool cacheAudioIfLoadedFromWeb = true;

        public enum AudioPlaybackOptions
        {
            ChangeAudioClip,
            ChangeAndPlay,
            ChangeAndFadeIn
        }

        [ShowIf( "ShowChangeAudioAction" ), FoldoutGroup( "Audio Settings" )]
        public AudioPlaybackOptions audioPlaybackOptions = AudioPlaybackOptions.ChangeAndPlay;
        private bool ChangeAudioClipSelected() { return ShowChangeAudioAction() && audioPlaybackOptions == AudioPlaybackOptions.ChangeAudioClip; }
        private bool ChangeAndPlayAudioSelected() { return ShowChangeAudioAction() && audioPlaybackOptions == AudioPlaybackOptions.ChangeAndPlay; }
        private bool ChangeAndFadeInAudioSelected() { return ShowChangeAudioAction() && audioPlaybackOptions == AudioPlaybackOptions.ChangeAndFadeIn; }
        private bool ChangeAndPlayOrFadeInAudioSelected() { return ShowChangeAudioAction() && ( audioPlaybackOptions == AudioPlaybackOptions.ChangeAndPlay || audioPlaybackOptions == AudioPlaybackOptions.ChangeAndFadeIn ); }

        [ShowIf( "ChangeAndPlayOrFadeInAudioSelected" ), FoldoutGroup( "Audio Settings" )]
        public float audioVolume = 1f;
        [ShowIf( "ChangeAndPlayOrFadeInAudioSelected" ), FoldoutGroup( "Audio Settings" )]
        public bool audioLoop = false;
        [ShowIf( "ChangeAndPlayOrFadeInAudioSelected" ), FoldoutGroup( "Audio Settings" )]
        public float audioPitch = 1f;

        [ShowIf( "ChangeAndFadeInAudioSelected" ), FoldoutGroup( "Audio Settings" )]
        public float audioFadeInLength = 1f;
        [ShowIf( "ChangeAndFadeInAudioSelected" ), FoldoutGroup( "Audio Settings" )]
        public float audioFadeInDelay = 0f;

        private bool playRequested = false; //Kept false until the BlockEvent is activated to switch audio AND told to Play/Fade once the switch is complete, once the AudioClip is loaded we set this to false and begin playback.


        //-------------------- "CHANGE AUDIO" EVENT MESSAGES ---------------------//
        private bool ShowChangeAudioEventMessages() { return action == Actions.ChangeAudio; }

        [ShowIf( "IsChangeAudioUsingPath" ), FoldoutGroup( "Event Messages" )]
        public UnityEvent LoadAudioFromPathSuccess = new UnityEvent();

        [ShowIf( "IsChangeAudioUsingPath" ), FoldoutGroup( "Event Messages" )]
        public UnityEvent LoadAudioFromPathFailed = new UnityEvent();

        [ShowIf( "ShowChangeAudioEventMessages" ), FoldoutGroup( "Event Messages" )]
        public UnityEvent onChangeAudio = new UnityEvent();

        [ShowIf( "ChangeAndPlayAudioSelected" ), FoldoutGroup( "Event Messages" )]
        public UnityEvent onPlayAudio = new UnityEvent();

        [ShowIf( "ChangeAndFadeInAudioSelected" ), FoldoutGroup( "Event Messages" )]
        public UnityEvent onFadeAudio = new UnityEvent();



        //---------------------------------------------------------//
        public override EventType GetEventType()
        //---------------------------------------------------------//
        {
            return EventType.Audio;

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

            if( action == Actions.ChangeAudio )
            {
                //This flag sets Audio/Video to begin playback if the Resource we're requesting finishes
                //loading AFTER we've already told it to start playing
                playRequested = false;

                if( changeAudioUsing == ChangeAudioUsing.AudioClip )
                {
                    //Do nothing, audioClip is already set
                    if( changeToAudioClip != null )
                    {
                        eventReady = true;
                    }
                    else
                    {
                        eventReady = false;
                    }
                }
                else if( changeAudioUsing == ChangeAudioUsing.Path )
                {
                    WWWHelper.instance.GetAudioClip( audioPath, cacheAudioIfLoadedFromWeb, OnGetAudioSuccess, OnGetAudioFailed );
                }
            }
            else if( action == Actions.PlayBlockAudio )
            {
                if( changeThisAudioBlock != null )
                {
                    eventReady = true;
                }
            }
            else if( action == Actions.StopBlockAudio )
            {
                if( changeThisAudioBlock != null )
                {
                    eventReady = true;
                }
            }
            
        } //END PrepareEvent

        //---------------------------------//
        private void OnGetAudioSuccess( AudioClip audioClip )
        //---------------------------------//
        {

            changeToAudioClip = audioClip;
            eventReady = true;

            if( LoadAudioFromPathSuccess != null ) { LoadAudioFromPathSuccess.Invoke(); }

            if( playRequested )
            {
                playRequested = false;
                _CallEvent();
            }

        } //END onGetAudioSuccess

        //---------------------------------//
        private void OnGetAudioFailed()
        //---------------------------------//
        {

            changeToAudioClip = null;
            eventReady = false;
            playRequested = false;

            if( LoadAudioFromPathFailed != null ) { LoadAudioFromPathFailed.Invoke(); }

        } //END onGetAudioFailed


        //-------------------------------//
        protected override void _CallEvent()
        //-------------------------------//
        {
            base._CallEvent();

            if( eventReady )
            {
                if( action == Actions.ChangeAudio )
                {
                    CallChangeAudioEvent();
                }
                else if( action == Actions.PlayBlockAudio )
                {
                    CallPlayBlockAudioEvent();
                }
                else if( action == Actions.StopBlockAudio )
                {
                    CallStopBlockAudioEvent();
                }
            }
            
        } //END CallEvent

        //---------------------------------//
        private void CallChangeAudioEvent()
        //---------------------------------//
        {

            if( changeToAudioClip != null )
            {
                if( setNewAudioClipOn == SetNewAudioClipOn.AudioSource && changeThisAudioSource != null )
                {
                    changeThisAudioSource.clip = changeToAudioClip;

                    if( audioPlaybackOptions != AudioPlaybackOptions.ChangeAudioClip )
                    {
                        PlayOrFadeAudio();
                    }
                    else
                    {
                        if( onChangeAudio != null ) { onChangeAudio.Invoke(); }
                    }
                }
                else if( setNewAudioClipOn == SetNewAudioClipOn.BlockAudio && changeThisAudioBlock != null )
                {
                    changeThisAudioBlock.audioClip = changeToAudioClip;

                    if( audioPlaybackOptions != AudioPlaybackOptions.ChangeAudioClip )
                    {
                        PlayOrFadeAudio();
                    }
                    else
                    {
                        if( onChangeAudio != null ) { onChangeAudio.Invoke(); }
                    }
                }
            }

            //We're ready to change, but the AudioClip isn't ready yet, setup a flag that will call PlayOrFadeAudio once the clip is finished loading
            else
            {
                playRequested = true;
            }

        } //END CallChangeAudioEvent

        //---------------------------------//
        private void PlayOrFadeAudio()
        //---------------------------------//
        {

            if( setNewAudioClipOn == SetNewAudioClipOn.AudioSource && changeThisAudioSource != null )
            {

                changeThisAudioSource.loop = audioLoop;
                changeThisAudioSource.pitch = audioPitch;

                if( audioPlaybackOptions == AudioPlaybackOptions.ChangeAndPlay )
                {
                    changeThisAudioSource.volume = audioVolume;
                    changeThisAudioSource.Play();
                    if( onPlayAudio != null ) { onPlayAudio.Invoke(); }
                }
                else if( audioPlaybackOptions == AudioPlaybackOptions.ChangeAndFadeIn )
                {
                    AudioHelper.instance.Fade( changeThisAudioSource, audioVolume, audioFadeInLength, audioFadeInDelay );
                    if( onFadeAudio != null ) { onFadeAudio.Invoke(); }
                }
            }
            else if( setNewAudioClipOn == SetNewAudioClipOn.BlockAudio && changeThisAudioBlock != null )
            {
                changeThisAudioBlock.loop = audioLoop;
                changeThisAudioBlock.pitch = audioPitch;

                if( audioPlaybackOptions == AudioPlaybackOptions.ChangeAndPlay )
                {
                    changeThisAudioBlock.volume = audioVolume;
                    changeThisAudioBlock.Play();
                    if( onPlayAudio != null ) { onPlayAudio.Invoke(); }
                }
                else if( audioPlaybackOptions == AudioPlaybackOptions.ChangeAndFadeIn )
                {
                    changeThisAudioBlock.FadeVolume( audioVolume, audioFadeInLength, audioFadeInDelay );
                    if( onFadeAudio != null ) { onFadeAudio.Invoke(); }
                }

            }

        } //END PlayOrFadeAudio

        //---------------------------------//
        private void CallPlayBlockAudioEvent()
        //---------------------------------//
        {

            if( changeThisAudioBlock != null )
            {
                changeThisAudioBlock.Play();
            }

        } //END CallPlayBlockAudioEvent

        //---------------------------------//
        private void CallStopBlockAudioEvent()
        //---------------------------------//
        {

            if( changeThisAudioBlock != null )
            {
                changeThisAudioBlock.Stop();
            }

        } //END CallStopBlockAudioEvent


    } //END BlockEventAudio

} //END Namespace