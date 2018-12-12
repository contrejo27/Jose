using Sirenix.OdinInspector;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

namespace BrandXR
{
    public class BlockEventScene: BlockEventBase
    {

        //----------------- ACTIONS --------------------------------//
        public enum Actions
        {
            None,
            LoadScene
        }

        [TitleGroup( "Block Event - Scene", "Used to change scenes" )]
        public Actions action = Actions.LoadScene;


        //----------------- "LOAD SCENE" VARIABLES ------------------------------//
        public enum LoadSceneOptions
        {
            Number,
            Name
        }

        [Space( 15f ), ShowIf( "action", Actions.LoadScene )]
        public LoadSceneOptions loadSceneBy = LoadSceneOptions.Number;
        private bool IsLoadSceneByNumber() { return action == Actions.LoadScene && loadSceneBy == LoadSceneOptions.Number; }
        private bool IsLoadSceneByName() { return action == Actions.LoadScene && loadSceneBy == LoadSceneOptions.Name; }

        [ShowIf( "IsLoadSceneByNumber" )]
        public int GoToSceneNumber = 0;

        [ShowIf( "IsLoadSceneByName" )]
        public string GoToSceneName;


        //----------------- FADE SETTINGS --------------------------------------//
        private bool ShowFadeSettings() { return action == Actions.LoadScene; }

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
        private bool UseDifferentValuesForFadeBack() { return ShowFadeSettings() && differentValuesForFadeBack; }

        [ShowIf( "UseDifferentValuesForFadeBack" ), FoldoutGroup( "Fade Settings" )]
        public float fadeBackTweenSpeed = 1f;
        [ShowIf( "UseDifferentValuesForFadeBack" ), FoldoutGroup( "Fade Settings" )]
        public float fadeBackDelay = 0f;
        [ShowIf( "UseDifferentValuesForFadeBack" ), FoldoutGroup( "Fade Settings" )]
        public EaseCurve.EaseType fadeBackEaseType = EaseCurve.EaseType.Linear;
        
        [ShowIf( "ShowFadeSettings" ), FoldoutGroup( "Fade Settings" )]
        public bool shouldWePlaySFX = false;
        private bool ShouldWePlaySFX() { return ShowFadeSettings() && shouldWePlaySFX; }

        [ShowIf( "ShouldWePlaySFX" ), FoldoutGroup( "Fade Settings" )]
        public AudioClip FadeToSFX;
        [ShowIf( "ShouldWePlaySFX" ), FoldoutGroup( "Fade Settings" )]
        public AudioClip FadeBackSFX;
        
        [ShowIf( "ShowFadeSettings" ), FoldoutGroup( "Fade Settings" )]
        public bool shouldWeFadeMusic;
        private bool ShouldWeFadeMusic() { return ShowFadeSettings() && shouldWeFadeMusic; }

        [ShowIf( "ShouldWeFadeMusic" ), FoldoutGroup( "Fade Settings" )]
        public AudioSource musicAudioSource;
        [ShowIf( "ShouldWeFadeMusic" ), FoldoutGroup( "Fade Settings" )]
        public float fadeToVolume;


        //-------------- "LOAD SCENE" EVENT MESSAGES ---------------------------------------//
        private bool ShowOnActionCompletedEvent() { return action == Actions.LoadScene; }
        
        [Space( 15f ), ShowIf( "ShowOnActionCompletedEvent" ), FoldoutGroup( "Event Messages" )]
        public UnityEvent onFadeToScene = new UnityEvent();
        [ShowIf( "ShowOnActionCompletedEvent" ), FoldoutGroup( "Event Messages" )]
        public UnityEvent onAsyncLoadScene = new UnityEvent();
        [ShowIf( "ShowOnActionCompletedEvent" ), FoldoutGroup( "Event Messages" )]
        public UnityEvent onFadeToAndAsyncLoadScene = new UnityEvent();
        [ShowIf( "ShowOnActionCompletedEvent" ), FoldoutGroup( "Event Messages" )]
        public UnityEvent onLoadScene = new UnityEvent();
        [ShowIf( "ShowOnActionCompletedEvent" ), FoldoutGroup( "Event Messages" )]
        public UnityEvent onFadeBackToScene = new UnityEvent();


        //---------------------------------------------------------//
        public override BlockEventBase.EventType GetEventType()
        //---------------------------------------------------------//
        {
            return BlockEventBase.EventType.Scene;

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

            if( action == Actions.LoadScene )
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
                if( action == Actions.LoadScene )
                {
                    CallLoadSceneEvent();
                }
            }
            
        } //END CallEvent

        //----------------------------------//
        private void CallLoadSceneEvent()
        //----------------------------------//
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

            if( loadSceneBy == LoadSceneOptions.Number )
            {
                SceneLoader.instance.LoadSceneWithFade( GoToSceneNumber, fadeColor, fadeToTweenSpeed, backTweenSpeed, fadeToDelay, backDelay, fadeToEaseType, backEaseType, shouldWePlaySFX, FadeToSFX, FadeBackSFX, shouldWeFadeMusic, musicAudioSource, fadeToVolume, onFadeToScene, onAsyncLoadScene, onFadeToAndAsyncLoadScene, onLoadScene, onFadeBackToScene );
            }
            else if( loadSceneBy == LoadSceneOptions.Name )
            {
                SceneLoader.instance.LoadSceneWithFade( GoToSceneName, fadeColor, fadeToTweenSpeed, backTweenSpeed, fadeToDelay, backDelay, fadeToEaseType, backEaseType, shouldWePlaySFX, FadeToSFX, FadeBackSFX, shouldWeFadeMusic, musicAudioSource, fadeToVolume, onFadeToScene, onAsyncLoadScene, onFadeToAndAsyncLoadScene, onLoadScene, onFadeBackToScene );
            }

        } //END CallLoadSceneEvent


    } //END BlockEventScene

} //END Namespace