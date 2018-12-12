using Sirenix.OdinInspector;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;
using UnityEngine.Video;

namespace BrandXR
{
    public class BlockEventXRSkybox: BlockEventBase
    {

        //----------------- ACTIONS --------------------------------//
        public enum Actions
        {
            None,
            ChangeVirtualRealityBackground
        }

        [TitleGroup( "Block Event - XR Skybox", "Used to modify a Skybox" )]
        public Actions action = Actions.ChangeVirtualRealityBackground;
        private bool ShowChangeVRBackground() { return action == Actions.ChangeVirtualRealityBackground; }

        //------------- "CHANGE VR BACKGROUND" VARIABLES ---------------------------------//
        [ShowIf( "ShowChangeVRBackground" ), FoldoutGroup( "VR Background Settings" )]
        public XRSkyboxFactory.ImageType GoToImage = XRSkyboxFactory.ImageType.TestImageOne;


        //-------------------- FADE SETTINGS ----------------------//
        private bool ShowFadeSettings() { return ShowChangeVRBackground(); }
        private bool ShowFadeSettingsAndUseDifferentValuesForFadeBack() { return ShowChangeVRBackground() && differentValuesForFadeBack; }
        private bool ShowFadeSettingsAndShouldWePlaySFX() { return ShowChangeVRBackground() && shouldWePlaySFX; }
        private bool ShowFadeSettingsAndShouldWeFadeMusic() { return ShowChangeVRBackground() && shouldWeFadeMusic; }
        
        [ShowIf( "ShowFadeSettings" ), FoldoutGroup( "Fade Settings" )]
        public Color fadeColor = Color.black;

        [ShowIf( "ShowFadeSettings" ), FoldoutGroup( "Fade Settings" )]
        public float fadeToTweenSpeed = 1f;

        [ShowIf( "ShowFadeSettings" ), FoldoutGroup( "Fade Settings" )]
        public float fadeToDelay = 0f;

        [ShowIf( "ShowFadeSettings" ), FoldoutGroup( "Fade Settings" )]
        public EaseCurve.EaseType fadeToEaseType = EaseCurve.EaseType.Linear;


        [ShowIf( "ShowFadeSettings" ), FoldoutGroup( "Fade Settings" )]
        public bool differentValuesForFadeBack = false;
        [ShowIf( "ShowFadeSettingsAndUseDifferentValuesForFadeBack" ), FoldoutGroup( "Fade Settings" )]
        public float fadeBackTweenSpeed = 1f;
        [ShowIf( "ShowFadeSettingsAndUseDifferentValuesForFadeBack" ), FoldoutGroup( "Fade Settings" )]
        public float fadeBackDelay = 0f;
        [ShowIf( "ShowFadeSettingsAndUseDifferentValuesForFadeBack" ), FoldoutGroup( "Fade Settings" )]
        public EaseCurve.EaseType fadeBackEaseType = EaseCurve.EaseType.Linear;

        [ShowIf( "ShowFadeSettings" ), FoldoutGroup( "Fade Settings" )]
        public bool shouldWePlaySFX = false;
        [ShowIf( "ShowFadeSettingsAndShouldWePlaySFX" ), FoldoutGroup( "Fade Settings" )]
        public AudioClip FadeToSFX;
        [ShowIf( "ShowFadeSettingsAndShouldWePlaySFX" ), FoldoutGroup( "Fade Settings" )]
        public AudioClip FadeBackSFX;

        [ShowIf( "ShowFadeSettings" ), FoldoutGroup( "Fade Settings" )]
        public bool shouldWeFadeMusic;
        [ShowIf( "ShowFadeSettingsAndShouldWeFadeMusic" ), FoldoutGroup( "Fade Settings" )]
        public AudioSource musicAudioSource;
        [ShowIf( "ShowFadeSettingsAndShouldWeFadeMusic" ), FoldoutGroup( "Fade Settings" )]
        public float fadeToVolume;


        //-------------------- EVENT MESSAGES ---------------------//
        private bool ShowVRBackgroundEventMessages() { return action == Actions.ChangeVirtualRealityBackground; }

        [ShowIf( "ShowVRBackgroundEventMessages" ), FoldoutGroup( "Event Messages" )]
        public UnityEvent onVRBackgroundFadeToColor = new UnityEvent();
        [ShowIf( "ShowVRBackgroundEventMessages" ), FoldoutGroup( "Event Messages" )]
        public UnityEvent onVRBackgroundLoadImage = new UnityEvent();
        [ShowIf( "ShowVRBackgroundEventMessages" ), FoldoutGroup( "Event Messages" )]
        public UnityEvent onVRBackgroundBlendToImage = new UnityEvent();
        [ShowIf( "ShowVRBackgroundEventMessages" ), FoldoutGroup( "Event Messages" )]
        public UnityEvent onVRBackgroundFadeBackFromColor = new UnityEvent();



        //---------------------------------------------------------//
        public override EventType GetEventType()
        //---------------------------------------------------------//
        {
            return EventType.XRSkybox;

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

            if( action == Actions.ChangeVirtualRealityBackground )
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
                if( action == Actions.ChangeVirtualRealityBackground )
                {
                    CallChangeVRBackground();
                }
            }
            
        } //END CallEvent

        //-----------------------------//
        private void CallChangeVRBackground()
        //-----------------------------//
        {

            float backTweenSpeed = fadeToTweenSpeed;
            float backDelay = fadeToDelay;
            EaseCurve.EaseType backEaseType = fadeToEaseType;

            if( differentValuesForFadeBack )
            {
                backTweenSpeed = fadeBackTweenSpeed;
                backDelay = fadeBackDelay;
                backEaseType = fadeBackEaseType;
            }
            
            XRSkyboxManager.instance.LoadImageWithFade( GoToImage, fadeColor, fadeToTweenSpeed, backTweenSpeed, fadeToDelay, backDelay, fadeToEaseType, backEaseType, shouldWePlaySFX, FadeToSFX, FadeBackSFX, shouldWeFadeMusic, musicAudioSource, fadeToVolume, onVRBackgroundFadeToColor, onVRBackgroundLoadImage, onVRBackgroundBlendToImage, onVRBackgroundFadeBackFromColor );

        } //END CallChangeVRBackground

    } //END BlockEventXRSkybox

} //END Namespace