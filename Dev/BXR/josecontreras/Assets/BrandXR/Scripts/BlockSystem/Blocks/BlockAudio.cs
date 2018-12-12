/* BlockAudio.cs
 * 
 * Plays audio from an AudioClip or a local or web source via a Path
 * Contains options for playing audio when a button has it's collider entered or exited
 */

using Sirenix.OdinInspector;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;

namespace BrandXR
{
    public class BlockAudio: Block
    {
        protected override bool ShowFaceCameraEvent() { return false; }

        public override BlockType GetBlockType() { return BlockType.Audio; }

        private bool IsBlockButtonNotNull() { return blockGroup != null && blockGroup.GetBlock( BlockType.Button ) != null; }


        public enum AudioOptions
        {
            AudioClip,
            Path
        }

        [Space( 10f ), TitleGroup("Block Audio", "Plays audio when BlockButton is activated, or alternatively connect to any EventTrigger" )]
        public AudioOptions loadType = AudioOptions.AudioClip;
        
        [ShowIf( "loadType", AudioOptions.AudioClip )]
        public AudioClip audioClip;
        private AudioSource audioSource;
        
        [ShowIf( "loadType", AudioOptions.Path )]
        public string path;

        [ShowIf( "loadType", AudioOptions.Path )]
        public bool cacheAudioClipIfLoadedFromWeb = true;


        [Space( 20f )]

        public float volume = 1f;
        
        public bool loop = false;

        public float pitch = 1f;


        public enum PlaybackOptions
        {
            PlayEntireClip,
            EndWhenGazeExitsCollider
        }

        [Space( 20f )]
        public PlaybackOptions playbackOptions = PlaybackOptions.PlayEntireClip;

        [Space( 10f ), ShowIf( "ParentHasBlockButton" )]
        public bool activateOnBlockButtonSelect = true;
        private bool ParentHasBlockButton() { return ( transform.parent != null && transform.parent.GetComponentInChildren<BlockButton>() != null ); }
        private bool ParentDoesNotHaveBlockButtonOrActivateOnButtonSelectIsFalse() { return !ParentHasBlockButton() || ( ParentHasBlockButton() && !activateOnBlockButtonSelect ); }

        [Space( 10f ), ShowIf( "ParentDoesNotHaveBlockButtonOrActivateOnButtonSelectIsFalse" ), InfoBox("Connect a trigger to play audio when PointerClick occurs.")]
        public List<EventTrigger> eventTriggers = new List<EventTrigger>();

        public enum ExitColliderOptions
        {
            Stop,
            FadeOut
        }

        [Space( 10f )]
        [InfoBox( "If no EventTrigger is found, this block will revert to 'Play Entire Clip'" )]
        [ShowIf( "playbackOptions", PlaybackOptions.EndWhenGazeExitsCollider )]
        public ExitColliderOptions exitColliderOptions = ExitColliderOptions.FadeOut;
        private bool IsExitOnColliderAndFadeOutTrue() { return playbackOptions == PlaybackOptions.EndWhenGazeExitsCollider && exitColliderOptions == ExitColliderOptions.FadeOut; }

        [ShowIf( "IsExitOnColliderAndFadeOutTrue" )]
        public float fadeOutLength = 1f;

        [ShowIf( "IsExitOnColliderAndFadeOutTrue" )]
        public float fadeOutDelay = 0f;

        [ShowIf( "IsExitOnColliderAndFadeOutTrue" )]
        public bool fadeInOnReEnterTrigger = true;
        private bool IsExitOnColliderAndFadeOutAndFadeInOnReEnterTriggerTrue() { return playbackOptions == PlaybackOptions.EndWhenGazeExitsCollider && exitColliderOptions == ExitColliderOptions.FadeOut && fadeInOnReEnterTrigger; }

        [ShowIf( "IsExitOnColliderAndFadeOutAndFadeInOnReEnterTriggerTrue" )]
        public bool useDifferentFadeInValues = false;
        private bool IsExitOnColliderAndFadeOutAndFadeInOnReEnterTriggerAndUseDifferendFadeInValuesTrue() { return playbackOptions == PlaybackOptions.EndWhenGazeExitsCollider && exitColliderOptions == ExitColliderOptions.FadeOut && fadeInOnReEnterTrigger && useDifferentFadeInValues; }

        public enum EnterColliderOptions
        {
            Play,
            FadeIn
        }

        [ShowIf( "IsExitOnColliderAndFadeOutAndFadeInOnReEnterTriggerAndUseDifferendFadeInValuesTrue" )]
        public EnterColliderOptions enterColliderOptions = EnterColliderOptions.FadeIn;
        private bool IsExitOnColliderAndFadeOutAndFadeInOnReEnterTriggerAndFadeInTrue() { return playbackOptions == PlaybackOptions.EndWhenGazeExitsCollider && exitColliderOptions == ExitColliderOptions.FadeOut && fadeInOnReEnterTrigger && useDifferentFadeInValues && enterColliderOptions == EnterColliderOptions.FadeIn; }

        [ShowIf( "IsExitOnColliderAndFadeOutAndFadeInOnReEnterTriggerAndFadeInTrue" )]
        public float fadeInLength = 1f;

        [ShowIf( "IsExitOnColliderAndFadeOutAndFadeInOnReEnterTriggerAndFadeInTrue" )]
        public float fadeInDelay = 0f;


        //If we successfully find the collider during Awake() or SetValues(), then we can use the "End When Gaze Hits Collider" logic,
        //If not then we can only use the "Play Entire Clip"
        private bool foundEventTrigger = false;

        //If the EventSystem we use comes from the BlockButtonCollider, then when the BlockButton is activated, we begin playback
        //If not, then we can begin playback when we enter another trigger, assuming that the variable activateOnBlockButtonSelect = false
        private bool foundEventTriggerOnBlockButtonCollider = false;

        private bool readyToPlay = false; //Kept false if we can't find the audio file or if we're busy loading from a url
        private bool playRequested = false; //Kept false until the AudioComponent is activated to begin playback, when AudioComponent is de-activated (or we change sources) then we set this to false. We use this to check if we should begin playback right away if a download completes after we've entered the audio zone or activated the BlockGroup
        
        
        public Coroutine coroutine_OnComplete;

        [Space( 20f )]

        [ShowIf("loop", false)]
        [InfoBox("OnComplete method is not called if loop = true")]
        public UnityEvent onCompleteMethod = new UnityEvent();


        private Coroutine coroutine_Fade = null;
        

        //---------------------------------//
        public override void Start()
        //---------------------------------//
        {
            base.Start();

            AddAudioSourceIfNeeded();

            GetAudioClip();

            Timer.instance.In( .1f, SetupEventTriggers, gameObject );

        } //END Start

        

        //---------------------------------//
        private void AddAudioSourceIfNeeded()
        //---------------------------------//
        {

            if( gameObject.GetComponent<AudioSource>() != null )
            {
                audioSource = gameObject.GetComponent<AudioSource>();
            }
            else
            {
                audioSource = gameObject.AddComponent<AudioSource>();
            }

        } //END AddAudioSourceIfNeeded

        //---------------------------------//
        private void GetAudioClip()
        //---------------------------------//
        {
            //Set the 'playRequested' flag to false, this flag is used to start playback on a AudioClip if getting that resource isn't done before the player enters the collider, then we'll begin playback once the resource becomes available
            playRequested = false;

            //If this is an AudioClip call, make sure the AudioClip variable is set
            if( loadType == AudioOptions.AudioClip )
            {
                if( audioClip != null )
                {
                    if( showDebug ) { Debug.Log( "BlockAudio.cs GetAudioClip() loadType = " + loadType + ", found attached AudioClip" ); }
                    audioSource.clip = audioClip;
                    readyToPlay = true;
                }
                else
                {
                    if( showDebug ) { Debug.Log( "BlockAudio.cs GetAudioClip() loadType = " + loadType + ", unable to find any attached AudioClips!" ); }
                    readyToPlay = false;
                }
            }

            //For a streamingAssets, Resources, and Web calls we'll need to set an IEnumerator check to grab the AudioClip
            else if( loadType == AudioOptions.Path )
            {
                if( showDebug ) { Debug.Log( "BlockAudio.cs GetAudioClip() loadType = " + loadType + ", beginning to search through Local and Web resources" ); }
                WWWHelper.instance.GetAudioClip( path, cacheAudioClipIfLoadedFromWeb, onGetAudioClipSuccess, onGetAudioClipFailed );
            }

        } //END GetAudioClip

        //---------------------------------//
        public void onGetAudioClipSuccess( AudioClip audioClip )
        //---------------------------------//
        {
            this.audioClip = audioClip;
            audioSource.clip = audioClip;
            readyToPlay = true;

            //If getting the AudioClip took too long, we might already be requesting that the audio plays back, go ahead and play it now (better late than never)
            if( playRequested )
            {
                if( showDebug ) { Debug.Log( "BlockAudio.cs onGetAudioClipSuccess() playing AudioClip late" ); }
                playRequested = false;
                PlayOrFade();
            }
            else
            {
                if( showDebug ) { Debug.Log( "BlockAudio.cs onGetAudioClipSuccess() setting readyToPlay = true" ); }
            }

        } //END onGetAudioClipSuccess

        //---------------------------------//
        public void onGetAudioClipFailed()
        //---------------------------------//
        {
            audioClip = null;
            audioSource.clip = null;
            readyToPlay = false;
            playRequested = false;

            if( showDebug ) { Debug.Log( "BlockAudio.cs onGetAudioClipFailed()" ); }

        } //END onGetAudioClipFailed



        //---------------------------------//
        private void SetupEventTriggers()
        //---------------------------------//
        {
            if( showDebug ) { Debug.Log( "SetupEventTriggers() playbackOptions = " + playbackOptions ); }

            //If there isn't a collider attached, and if we can't find the component attached to this gameObject, then try to use the BlockButton,
            //and if we can't find the BlockButton, change the playbackOptions to "Play Entire Clip"
            if( playbackOptions == PlaybackOptions.EndWhenGazeExitsCollider )
            {
                if( showDebug ) { Debug.Log( "BlockAudio.cs SetupOnExitEventTrigger() inside playBackOptions = 'End When Gaze Exits Collider'" ); }

                foundEventTrigger = false;
                foundEventTriggerOnBlockButtonCollider = false;
                List<Block> blockButtons = blockGroup.GetBlocks( BlockType.Button );

                if( ( eventTriggers == null || ( eventTriggers != null && eventTriggers.Count == 0 ) ) &&
                    blockGroup != null && blockButtons != null && blockButtons.Count > 0 )
                {
                    foreach( Block blockButton in blockButtons )
                    {
                        if( (blockButton as BlockButton).collider3D != null &&
                            (blockButton as BlockButton).collider3D.GetComponent<EventTrigger>() != null )
                        {
                            eventTriggers.Add( ( blockButton as BlockButton ).collider3D.GetComponent<EventTrigger>() );
                            foundEventTrigger = true;
                            foundEventTriggerOnBlockButtonCollider = true;
                        }
                    }
                }
                else if( eventTriggers != null )
                {
                    foundEventTrigger = true;
                }

                //If we didn't find the collider, change the playbackOptions to "Play Entire Clip"
                if( !foundEventTrigger )
                {
                    if( showDebug ) { Debug.Log( "BlockAudio.cs SetupOnExitEventTrigger() unable to find EventTrigger, settings playbackOptions = 'Play Entire Clip'" ); }
                    playbackOptions = PlaybackOptions.PlayEntireClip;
                }
                else
                {
                    if( showDebug ) { Debug.Log( "BlockAudio.cs SetupOnExitEventTrigger() found the EventTrigger we'll be attaching the PointerExit event to" ); }
                }

                //Setup the EventTrigger OnExit event to call Stop() or FadeOut() on PointerExit, and optionally Play or FadeIn() on PointerEnter
                if( eventTriggers != null )
                {
                    foreach( EventTrigger eventTrigger in eventTriggers )
                    {
                        EventTriggerHelper.AddEventTriggerListener( eventTrigger, EventTriggerType.PointerExit, BlockAudio_OnPointerExit );
                    }

                    if( fadeInOnReEnterTrigger )
                    {
                        foreach( EventTrigger eventTrigger in eventTriggers )
                        {
                            EventTriggerHelper.AddEventTriggerListener( eventTrigger, EventTriggerType.PointerEnter, BlockAudio_OnPointerEnter );
                        }
                    }
                }
            }
            
            else if( playbackOptions == PlaybackOptions.PlayEntireClip )
            {
                foundEventTrigger = false;
                foundEventTriggerOnBlockButtonCollider = false;
                List<Block> blockButtons = blockGroup.GetBlocks( BlockType.Button );

                if( showDebug ) { Debug.Log( "BlockAudio.cs SetupOnExitEventTrigger() 'Play Entire Clip' set, about to look through BlockButtons for this block and attach to their collider events... blockButtons = " + blockButtons + ", blockButtonss.Count = " + blockButtons.Count ); }

                if( ( eventTriggers == null || ( eventTriggers != null && eventTriggers.Count == 0 ) ) &&
                    blockGroup != null && blockButtons != null && blockButtons.Count > 0 )
                {
                    foreach( Block blockButton in blockButtons )
                    {
                        if( ( blockButton as BlockButton ).collider3D != null &&
                            ( blockButton as BlockButton ).collider3D.GetComponent<EventTrigger>() != null )
                        {
                            eventTriggers.Add( ( blockButton as BlockButton ).collider3D.GetComponent<EventTrigger>() );
                            foundEventTrigger = true;
                            foundEventTriggerOnBlockButtonCollider = true;
                        }
                    }
                }
                else if( eventTriggers != null )
                {
                    foundEventTrigger = true;
                }
            }
            
        } //END SetupEventTriggers

        //---------------------------------//
        private void BlockAudio_OnPointerEnter( BaseEventData baseEvent )
        //---------------------------------//
        {
            if( showDebug ) Debug.Log( "BlockAudio.cs BlockAudio_OnPointerEnter() activateOnBlockButtonSelect = " + activateOnBlockButtonSelect + ", IsBlockButtonNotNull() = " + IsBlockButtonNotNull() + ", IsFocused( blockGroup ) = " + BlockFocusManager.instance.IsFocused( blockGroup ) + ", activateOnBlockGroupSelect = " + activateOnBlockButtonSelect + ", readyToPlay = " + readyToPlay );

            //If we require the blockGroup to be activated, make sure it is.
            //Otherwise, activate once the trigger is entered
            //Only perform this action if the audio is ready to play
            if( ( ( activateOnBlockButtonSelect && IsBlockButtonNotNull() && BlockFocusManager.instance.IsFocused( blockGroup ) ) || !activateOnBlockButtonSelect ) && readyToPlay )
            {
                PlayOrFade();
            }
            //If we want to play the audio but it isn't ready, set a flag to tell the audio to play once it's available
            else if( ( activateOnBlockButtonSelect && IsBlockButtonNotNull() && BlockFocusManager.instance.IsFocused( blockGroup ) ) || !activateOnBlockButtonSelect && !readyToPlay )
            {
                playRequested = true;
            }

        } //END BlockAudio_OnPointerEnter

        //---------------------------------//
        private void BlockAudio_OnPointerExit( BaseEventData baseEvent )
        //---------------------------------//
        {
            if( showDebug ) Debug.Log( "BlockAudio.cs BlockAudio_OnPointerExit() activateOnBlockGroupSelect = " + activateOnBlockButtonSelect + ", IsBlockButtonNotNull() = " + IsBlockButtonNotNull() + ", IsFocused( blockGroup ) = " + BlockFocusManager.instance.IsFocused( blockGroup ) + ", activateOnBlockButtonSelect = " + activateOnBlockButtonSelect + ", readyToPlay = " + readyToPlay );

            //If we require the blockGroup to be activated, make sure it is.
            //Otherwise, activate once the trigger is exited
            //Only perform this action is the audio is ready to play
            if( ( ( activateOnBlockButtonSelect && IsBlockButtonNotNull() && BlockFocusManager.instance.IsFocused( blockGroup ) ) || !activateOnBlockButtonSelect ) && readyToPlay )
            {
                if( showDebug ) Debug.Log( "BlockAudio_OnPointerExit() exitColliderOptions = " + exitColliderOptions + ", activateOnBlockButtonSelect = " + activateOnBlockButtonSelect );
                
                if( exitColliderOptions == ExitColliderOptions.Stop )
                {
                    Stop();
                }
                else if( exitColliderOptions == ExitColliderOptions.FadeOut )
                {
                    FadeVolume( 0f, fadeOutLength, fadeOutDelay );
                }
            }

            //If we were waiting for an audio clip to load so we could play and had previously tried to play the audio clip early, then we would have this flag set to true. However since we're now fading out/stopping playback we can set this flag to false
            if( playRequested )
            {
                playRequested = false;
            }

        } //END BlockAudio_OnPointerExit

        //---------------------------------//
        private void PlayOrFade()
        //---------------------------------//
        {

            if( showDebug ) Debug.Log( "PlayOrFade() enterColliderOptions = " + enterColliderOptions );

            CancelExistingFade();
            
            if( enterColliderOptions == EnterColliderOptions.Play )
            {
                Play();
            }
            else if( enterColliderOptions == EnterColliderOptions.FadeIn )
            {
                //Debug.Log( "BlockAudio FadeIn()" );
                if( useDifferentFadeInValues )
                {
                    if( audioSource != null && !audioSource.isPlaying ) { Play( 0f, 0, pitch, loop ); }
                    FadeVolume( volume, fadeInLength, fadeInDelay );
                }
                else
                {
                    if( audioSource != null && !audioSource.isPlaying ) { Play( 0f, 0, pitch, loop ); }
                    FadeVolume( volume, fadeOutLength, fadeOutDelay );
                }
            }

        } //END PlayOrFade

        //---------------------------------//
        protected override void OtherBlockButtonSelected( BlockButton block )
        //---------------------------------//
        {
            if( showDebug ) Debug.Log( "BlockAudio.cs OtherComponentEntered( " + block.name + " ) readyToPlay = " + readyToPlay + ", activateOnBlockButtonSelect = " + activateOnBlockButtonSelect + ", IsBlockButtonNotNull() = " + IsBlockButtonNotNull() + ", foundEventTriggerOnBlockButtonCollider = " + foundEventTriggerOnBlockButtonCollider + ", blockGroup.IsBlockInGroup( block ) = " + blockGroup.IsInGroup( block ) );

            if( readyToPlay )
            {
                if( activateOnBlockButtonSelect && IsBlockButtonNotNull() )
                {
                    if( foundEventTriggerOnBlockButtonCollider && blockGroup.IsInGroup( block ) )
                    {
                        if( showDebug ) Debug.Log( "BlockAudio.cs OtherComponentEntered( " + block.name + " ) calling Play()" );
                        Play();
                    }
                }
            }

            //If we've entered the BlockButton collider but we're not ready to begin playback, then when the source does finish loading, begin playback
            else if( !readyToPlay )
            {
                if( activateOnBlockButtonSelect && IsBlockButtonNotNull() )
                {
                    if( foundEventTriggerOnBlockButtonCollider && blockGroup.IsInGroup( block ) )
                    {
                        playRequested = true;
                    }
                }
            }

        } //END OtherBlockButtonSelected


        //---------------------------------//
        protected override void OtherBlockButtonGazeExit( BlockButton blockButton )
        //---------------------------------//
        {

            //If we've requested playback due to the BlockButton being selected, but the AudioClip is still loading, then we set the 'playRequested' flag to true and begin playback when the AudioClip becomes ready
            //However if we exit the BlockButton before the AudioClip has a chance to begin playback, then we set the 'playRequested' flag to false
            if( playRequested )
            {
                if( activateOnBlockButtonSelect && IsBlockButtonNotNull() )
                {
                    if( foundEventTriggerOnBlockButtonCollider && blockGroup.IsInGroup( blockButton ) )
                    {
                        playRequested = false;
                    }
                }
            }

        } //END OtherBlockButtonGazeExit

        //--------------------------------//
        public override void ForceShow()
        //--------------------------------//
        {
            base.ForceShow();

            Show();

        } //END ForceShow

        //--------------------------------//
        public override void Show()
        //--------------------------------//
        {
            base.Show();

            if( showDebug ) { Debug.Log( "BlockAudio.cs Show() readyToPlay = " + readyToPlay + ", activateOnBlockButtonSelect = " + activateOnBlockButtonSelect + ", IsBlockButtonNotNull() = " + IsBlockButtonNotNull() + ", foundEventTriggerOnBlockButtonCollider = " + foundEventTriggerOnBlockButtonCollider + ", IsFocused( blockGroup ) = " + BlockFocusManager.instance.IsFocused( blockGroup ) + ", audioSource.isPlaying = " + audioSource.isPlaying ); }

            if( readyToPlay )
            {
                if( activateOnBlockButtonSelect && IsBlockButtonNotNull() )
                {
                    if( foundEventTriggerOnBlockButtonCollider && BlockFocusManager.instance.IsFocused( blockGroup ) && !audioSource.isPlaying )
                    {
                        if( showDebug ) { Debug.Log( "BlockButtonAudio.cs Show() calling PlayOrFade()" ); }
                        PlayOrFade();
                    }
                }
            }

        } //END Show

        //--------------------------------//
        public override void ForceHide()
        //--------------------------------//
        {
            base.ForceHide();

            if( audioSource != null ) { audioSource.Stop(); }
            playRequested = false;

        } //END ForceHide

        //--------------------------------//
        public override void Hide()
        //--------------------------------//
        {
            base.Hide();

            FadeVolume( 0f, fadeOutLength, fadeOutDelay );
            playRequested = false;

        } //END Hide

        //--------------------------------//
        public override void PrepareForDestroy()
        //--------------------------------//
        {

            //Stops any running tweens or timers associated with this block
            if( audioSource != null )
            {
                audioSource.Stop();
            }

            if( audioClip != null )
            {
                audioClip = null;
            }

            if( coroutine_OnComplete != null )
            {
                Timer.instance.Cancel( coroutine_OnComplete );
            }

            if( coroutine_Fade != null )
            {
                if( AudioHelper.instance != null )
                {
                    AudioHelper.instance.StopCoroutine( coroutine_Fade );
                }

                coroutine_Fade = null;
            }

        } //END PrepareForDestroy

        //---------------------------------//
        public override void SetValues( string jsonData )
        //---------------------------------//
        {
            
            AddAudioSourceIfNeeded();

            GetAudioClip();

            SetupEventTriggers();

        } //END SetValues
        

        //------------------------------------//
        public void Play()
        //------------------------------------//
        {

            Play( volume, 0, pitch, loop );

        } //END Play
        

        //------------------------------------//
        public void Play( float volume, ulong delay, float pitch, bool loop )
        //------------------------------------//
        {

            if( audioSource != null && audioClip != null )
            {
                if( showDebug ) Debug.Log( "BlockAudio.cs Play()" );
                audioSource.loop = loop;
                audioSource.pitch = pitch;
                audioSource.volume = volume;
                audioSource.Play( delay );

                if( !loop ) { StartOnCompleteTimer(); }
            }
            else
            {
                if( showDebug ) Debug.Log( "BlockAudio.cs Play(), cannot play audio... audioSource = " + audioSource + ", audioClip = " + audioClip );
            }

        } //END Play

        //------------------------------------//
        public void Pause()
        //------------------------------------//
        {

            if( audioSource != null && audioClip != null )
            {
                audioSource.Pause();
            }
            
        } //END Pause

        //------------------------------------//
        public void Resume()
        //------------------------------------//
        {

            if( audioSource != null && audioClip != null )
            {
                audioSource.UnPause();
            }

        } //END Resume

        //------------------------------------//
        public void Stop()
        //------------------------------------//
        {

            if( audioSource != null && audioClip != null )
            {
                audioSource.Stop();
            }

        } //END Stop

        //------------------------------------//
        public void SetVolume( float volume )
        //------------------------------------//
        {

            if( audioSource != null && audioClip != null )
            {
                audioSource.volume = volume;
            }

        } //END Stop

        //------------------------------------//
        public float GetVolume()
        //------------------------------------//
        {

            if( audioSource != null && audioClip != null )
            {
                return audioSource.volume;
            }

            return 1f;

        } //END GetVolume

        //------------------------------------//
        private void CancelExistingFade()
        //------------------------------------//
        {

            if( coroutine_Fade != null )
            {
                if( AudioHelper.instance != null )
                {
                    AudioHelper.instance.StopCoroutine( coroutine_Fade );
                }

                coroutine_Fade = null;
            }

        } //END CancelExistingFade

        //------------------------------------//
        public void FadeVolume( float volume, float length, float delay )
        //------------------------------------//
        {
            CancelExistingFade();
            
            if( audioSource != null && audioClip != null )
            {
                if( showDebug ) { Debug.Log( "BlockAudio.cs FadeVolume() volume = " + volume + ", length = " + length + ", delay = " + delay ); }

                if( AudioHelper.instance != null )
                {
                    coroutine_Fade = AudioHelper.instance.Fade( audioSource, volume, length, delay );
                }
            }

        } //END FadeVolume
        
        


        //-------------------------------------//
        private void StartOnCompleteTimer()
        //-------------------------------------//
        {

            if( onCompleteMethod != null )
            {
                coroutine_OnComplete = Timer.instance.In( audioSource.clip.length, OnComplete, gameObject );
            }

        } //END StartOnCompleteTimer

        //--------------------------------------//
        private void OnComplete()
        //--------------------------------------//
        {
            
            if( onCompleteMethod != null )
            {
                onCompleteMethod.Invoke();
            }

        } //END OnComplete


        

    } //END Class

} //END Namespace