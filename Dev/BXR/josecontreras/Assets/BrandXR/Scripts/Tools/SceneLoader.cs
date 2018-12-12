using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.SceneManagement;

namespace BrandXR
{
    public class SceneLoader: MonoBehaviour
    {

        public bool showDebug = false;

        //public delegate void OnCompleteMethod(); //What do we want to call after this tween completes?
        public UnityEvent onFadeTo = new UnityEvent();
        public UnityEvent onAsyncLoad = new UnityEvent();
        public UnityEvent onFadeToAndAsyncLoad = new UnityEvent();
        public UnityEvent onLoadScene = new UnityEvent();
        public UnityEvent onFadeBack = new UnityEvent();

        private int sceneNumber = 0;
        private string sceneName = "";
        

        private float fadeBackTweenSpeed = 2f;
        private float fadeBackDelay = 0f;
        private EaseCurve.EaseType fadeBackEaseType = EaseCurve.EaseType.Linear;

        private bool shouldWePlaySFX = false;
        private AudioClip fadeBackSFX;

        private bool shouldWeFadeMusic = false;
        private AudioSource musicAudioSource;
        private float originalMusicVolume = 0f;

        private AsyncOperation async; //Store the asynchronous loading operation

        private bool loadComplete = false;
        private bool fadeToComplete = false;

        private bool levelTransitionInProgress = false;

        private bool hasCalled_onAsyncLoad_Message = false;
        private bool hasCalled_onFadeToAndAyncLoad_Message = false;
        private bool hasCalled_onLoadScene = false;

        //Singleton behavior
        private static SceneLoader _instance;

        //--------------------------------------------//
        public static SceneLoader instance
        //--------------------------------------------//
        {
            get
            {
                if( _instance == null )
                {
                    if( GameObject.FindObjectOfType<SceneLoader>() == null ) { PrefabManager.InstantiatePrefab( PrefabFactory.Prefabs.bxr_SceneLoader ); }
                    _instance = GameObject.FindObjectOfType<SceneLoader>();
                }

                return _instance;
            }

        } //END Instance

        //--------------------------------------------//
        public void Awake()
        //--------------------------------------------//
        {
            DestroyDuplicateInstance();

            if( transform.parent == null )
            {
                DontDestroyOnLoad( transform.gameObject );
            }
            
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

        //---------------------------------------------//
        public void LoadScene( int sceneNumber )
        //---------------------------------------------//
        {

            SceneManager.LoadScene( sceneNumber );

        } //END LoadScene

        //---------------------------------------------//
        public void LoadScene( string sceneName )
        //---------------------------------------------//
        {

            SceneManager.LoadScene( sceneName );

        } //END LoadScene

        //---------------------------------------------//
        public void LoadSceneWithFade( int sceneNumber, Color fadeToColor, float fadeToTweenSpeed,
                float fadeBackTweenSpeed, float fadeToDelay, float fadeBackDelay,
                EaseCurve.EaseType fadeToEaseType, EaseCurve.EaseType fadeBackEaseType, bool shouldWePlaySFX, AudioClip fadeToSFX, AudioClip fadeBackSFX,
                bool shouldWeFadeMusic, AudioSource musicAudioSource, float fadeMusicToVolume )
        //--------------------------------------------//
        {

            this.sceneNumber = sceneNumber;
            this.sceneName = "";

            _LoadSceneWithFade( fadeToColor, fadeToTweenSpeed,
                fadeBackTweenSpeed, fadeToDelay, fadeBackDelay,
                fadeToEaseType, fadeBackEaseType, shouldWePlaySFX, fadeToSFX, fadeBackSFX,
                shouldWeFadeMusic, musicAudioSource, fadeMusicToVolume,
                null, null, null, null, null );

        } //END LoadSceneWithFade

        //---------------------------------------------//
        public void LoadSceneWithFade( int sceneNumber, Color fadeToColor, float fadeToTweenSpeed,
                float fadeBackTweenSpeed, float fadeToDelay, float fadeBackDelay,
                EaseCurve.EaseType fadeToEaseType, EaseCurve.EaseType fadeBackEaseType, bool shouldWePlaySFX, AudioClip fadeToSFX, AudioClip fadeBackSFX,
                bool shouldWeFadeMusic, AudioSource musicAudioSource, float fadeMusicToVolume,
                UnityEvent onFadeTo, UnityEvent onAsyncLoad, UnityEvent onFadeToAndAsyncLoad, UnityEvent onLoadScene, UnityEvent onFadeBack )
        //--------------------------------------------//
        {

            this.sceneNumber = sceneNumber;
            this.sceneName = "";

            _LoadSceneWithFade( fadeToColor, fadeToTweenSpeed,
                fadeBackTweenSpeed, fadeToDelay, fadeBackDelay,
                fadeToEaseType, fadeBackEaseType, shouldWePlaySFX, fadeToSFX, fadeBackSFX,
                shouldWeFadeMusic, musicAudioSource, fadeMusicToVolume,
                onFadeTo, onAsyncLoad, onFadeToAndAsyncLoad, onLoadScene, onFadeBack );

        } //END LoadSceneWithFade

        //---------------------------------------------//
        public void LoadSceneWithFade( string sceneName, Color fadeToColor, float fadeToTweenSpeed,
                float fadeBackTweenSpeed, float fadeToDelay, float fadeBackDelay,
                EaseCurve.EaseType fadeToEaseType, EaseCurve.EaseType fadeBackEaseType, bool shouldWePlaySFX, AudioClip fadeToSFX, AudioClip fadeBackSFX,
                bool shouldWeFadeMusic, AudioSource musicAudioSource, float fadeMusicToVolume )
        //--------------------------------------------//
        {

            this.sceneNumber = -99;
            this.sceneName = sceneName;

            _LoadSceneWithFade( fadeToColor, fadeToTweenSpeed,
                fadeBackTweenSpeed, fadeToDelay, fadeBackDelay,
                fadeToEaseType, fadeBackEaseType, shouldWePlaySFX, fadeToSFX, fadeBackSFX,
                shouldWeFadeMusic, musicAudioSource, fadeMusicToVolume,
                null, null, null, null, null );

        } //END LoadSceneWithFade

        //---------------------------------------------//
        public void LoadSceneWithFade( string sceneName, Color fadeToColor, float fadeToTweenSpeed,
                float fadeBackTweenSpeed, float fadeToDelay, float fadeBackDelay,
                EaseCurve.EaseType fadeToEaseType, EaseCurve.EaseType fadeBackEaseType, bool shouldWePlaySFX, AudioClip fadeToSFX, AudioClip fadeBackSFX,
                bool shouldWeFadeMusic, AudioSource musicAudioSource, float fadeMusicToVolume,
                UnityEvent onFadeTo, UnityEvent onAsyncLoad, UnityEvent onFadeToAndAsyncLoad, UnityEvent onLoadScene, UnityEvent onFadeBack )
        //--------------------------------------------//
        {

            this.sceneNumber = -99;
            this.sceneName = sceneName;

            _LoadSceneWithFade( fadeToColor, fadeToTweenSpeed,
                fadeBackTweenSpeed, fadeToDelay, fadeBackDelay,
                fadeToEaseType, fadeBackEaseType, shouldWePlaySFX, fadeToSFX, fadeBackSFX,
                shouldWeFadeMusic, musicAudioSource, fadeMusicToVolume,
                onFadeTo, onAsyncLoad, onFadeToAndAsyncLoad, onLoadScene, onFadeBack );

        } //END LoadSceneWithFade

        //---------------------------------------------//
        private void _LoadSceneWithFade( Color fadeToColor, float fadeToTweenSpeed,
                float fadeBackTweenSpeed, float fadeToDelay, float fadeBackDelay,
                EaseCurve.EaseType fadeToEaseType, EaseCurve.EaseType fadeBackEaseType, bool shouldWePlaySFX, AudioClip fadeToSFX, AudioClip fadeBackSFX,
                bool shouldWeFadeMusic, AudioSource musicAudioSource, float fadeMusicToVolume,
                UnityEvent onFadeTo, UnityEvent onAsyncLoad, UnityEvent onFadeToAndAsyncLoad, UnityEvent onLoadScene, UnityEvent onFadeBack )
        //--------------------------------------------//
        {
            
            this.fadeBackTweenSpeed = fadeBackTweenSpeed;
            this.fadeBackDelay = fadeBackDelay;
            this.fadeBackEaseType = fadeBackEaseType;

            this.shouldWePlaySFX = shouldWePlaySFX;

            this.shouldWeFadeMusic = shouldWeFadeMusic;
            this.musicAudioSource = musicAudioSource;

            if( musicAudioSource != null )
            {
                this.originalMusicVolume = musicAudioSource.volume;
            }

            this.fadeBackSFX = fadeBackSFX;

            this.onFadeTo = onFadeTo;
            this.onAsyncLoad = onAsyncLoad;
            this.onFadeToAndAsyncLoad = onFadeToAndAsyncLoad;
            this.onLoadScene = onLoadScene;
            this.onFadeBack = onFadeBack;
            
            if( this.shouldWeFadeMusic && musicAudioSource != null && musicAudioSource.clip != null ) { AudioHelper.instance.Fade( musicAudioSource, fadeMusicToVolume, fadeToTweenSpeed, 0f ); }

            if( shouldWePlaySFX && fadeToSFX != null ) { AudioSource.PlayClipAtPoint( fadeToSFX, Camera.main.transform.position, 1f ); }

            levelTransitionInProgress = true;
            hasCalled_onAsyncLoad_Message = false;
            hasCalled_onFadeToAndAyncLoad_Message = false;
            hasCalled_onLoadScene = false;
            loadComplete = false;
            fadeToComplete = false;
            
            //Begin the async level load
            StartCoroutine( "BeginAsyncLoad" );

            //Begin the fade to color
            UnityEvent _event = new UnityEvent();
            _event.AddListener( FadeToColorComplete );

            ScreenFadeManager.instance.Show( fadeToColor, fadeToTweenSpeed, fadeToDelay, fadeToEaseType, _event );

        } //END _LoadSceneWithFade

        //--------------------------------------------//
        IEnumerator BeginAsyncLoad()
        //--------------------------------------------//
        {
            if( showDebug ) { Debug.LogWarning( "ASYNC LOAD STARTED - DO NOT EXIT PLAY MODE UNTIL SCENE LOADS... UNITY WILL CRASH" ); }
            
            
            if( sceneNumber != -99 )
            {
                //Only load the sent in scene if it exists, if not reload the current scene
                if( !SceneManager.GetSceneByBuildIndex( sceneNumber ).IsValid() )
                {
                    if( showDebug ) { Debug.Log( "SceneLoader.cs BeginAsyncLoad() We couldn't locate sceneNumber = " + sceneNumber + ", defaulting to re-loading current scene" ); }
                    sceneNumber = SceneManager.GetActiveScene().buildIndex;
                }
                else
                {
                    if( showDebug ) { Debug.Log( "SceneLoader.cs BeginAsyncLoad() sceneNumber = " + sceneNumber ); }
                }

                async = SceneManager.LoadSceneAsync( sceneNumber );
            }
            else if( sceneName != "" )
            {

                if( showDebug ) { Debug.Log( "SceneLoader.cs BeginAsyncLoad() sceneName = " + sceneName ); }

                async = SceneManager.LoadSceneAsync( sceneName );
            }
            else
            {
                if( showDebug ) { Debug.Log( "SceneLoader.cs BeginAsyncLoad() couldn't locate scene, reloading current scene" ); }

                //Force the sceneNumber and sceneName variables to be for this current scene
                sceneNumber = SceneManager.GetActiveScene().buildIndex;
                sceneName = SceneManager.GetActiveScene().name;

                async = SceneManager.LoadSceneAsync( sceneNumber );
            }
            
            async.allowSceneActivation = false;

            // Wait until done and collect progress as we go.
            while( !async.isDone )
            {
                float loadProgress = async.progress;

                if( loadProgress >= 0.9f )
                {
                    // Almost done.
                    if( showDebug ) { Debug.Log( "SceneLoader.cs BeginAsyncLoad() loadProgress >= 90%, we're ready to load!" ); }
                    loadComplete = true;
                    break;
                }
                else
                {
                    if( showDebug ) { Debug.Log( "SceneLoader.cs BeginAsyncLoad() loadProgress = " + loadProgress ); }
                }

                yield return null;
            }

            yield return async;

        } //END BeginAsyncLoad

        //--------------------------------------------//
        private void FadeToColorComplete()
        //--------------------------------------------//
        {
            if( showDebug ) { Debug.Log( "SceneLoader.cs FadeToColorComplete() start, calling onFadeTo() if function exists" ); }
            
            //Send a message out that the fade to color effect has completed
            if( onFadeTo != null ) { onFadeTo.Invoke(); } //Send to delegate
            Messenger.Broadcast( "SceneLoader_onFadeTo", MessengerMode.DONT_REQUIRE_LISTENER ); //Send to suscribed listeners

            fadeToComplete = true;

        } //END FadeToColorComplete

        //--------------------------------------------//
        public void Update()
        //--------------------------------------------//
        {

            if( levelTransitionInProgress )
            {
                if( !hasCalled_onAsyncLoad_Message && loadComplete )
                {
                    if( showDebug ) { Debug.Log( "SceneLoader.cs Update() Async load is complete" ); }
                    hasCalled_onAsyncLoad_Message = true;

                    //Send a message out that async loading is complete (but not necessarily that we've finished the fadeTo yet...)
                    if( onAsyncLoad != null ) { onAsyncLoad.Invoke(); } //Send to delegate
                    Messenger.Broadcast( "SceneLoader_onAsyncLoad", MessengerMode.DONT_REQUIRE_LISTENER ); //Send to suscribed listeners

                }
                if( !hasCalled_onFadeToAndAyncLoad_Message && loadComplete && fadeToComplete )
                {
                    if( showDebug ) { Debug.Log( "SceneLoader.cs Update() Async load and the FadeTo color are both complete!" ); }

                    hasCalled_onFadeToAndAyncLoad_Message = true;

                    //Send a message out that both aync loading and fadeTo are complete
                    if( onAsyncLoad != null ) { onFadeToAndAsyncLoad.Invoke(); } //Send to delegate
                    Messenger.Broadcast( "SceneLoader_onFadeToAndAsyncLoad", MessengerMode.DONT_REQUIRE_LISTENER ); //Send to suscribed listeners

                    //Allow the level swap
                    async.allowSceneActivation = true;
                    
                }
                if( !hasCalled_onLoadScene && hasCalled_onFadeToAndAyncLoad_Message )
                {
                    if( showDebug ) { Debug.Log( "SceneLoader.cs Update() AllowSceneActivation set, and the load has completed, about the call fadeBack" ); }

                    Scene currentScene = SceneManager.GetActiveScene();

                    if( currentScene.buildIndex == this.sceneNumber ||
                        currentScene.name       == this.sceneName )
                    {
                        if( showDebug ) { Debug.Log( "SceneLoader.cs Update() Loading complete, calling FadeBack" ); }

                        hasCalled_onLoadScene = true;

                        //Send a message out that the new level has been loaded successfully
                        if( onLoadScene != null ) { onLoadScene.Invoke(); } //Send to delegate
                        Messenger.Broadcast( "SceneLoader_onLoadScene", MessengerMode.DONT_REQUIRE_LISTENER ); //Send to suscribed listeners

                        if( this.shouldWeFadeMusic && musicAudioSource != null ) { AudioHelper.instance.Fade( musicAudioSource, originalMusicVolume, fadeBackTweenSpeed, 0f ); }

                        if( shouldWePlaySFX && fadeBackSFX != null ) { AudioSource.PlayClipAtPoint( fadeBackSFX, Camera.main.transform.position, 1f ); }

                        //Begin to fade Back from the color
                        UnityEvent _event = new UnityEvent();
                        _event.AddListener( FadeBackFromColorComplete );

                        ScreenFadeManager.instance.Hide( fadeBackTweenSpeed, fadeBackDelay, fadeBackEaseType, _event );
                    }
                }
            }

        } //END Update

        //---------------------------------------------//
        private void FadeBackFromColorComplete()
        //---------------------------------------------//
        {
            levelTransitionInProgress = false;

            //Send a message out that we've finished fading back from the color (the unity scene is now 100% visible)
            if( onFadeBack != null ) { onFadeBack.Invoke(); } //Send to delegate
            Messenger.Broadcast( "SceneLoader_onFadeBack", MessengerMode.DONT_REQUIRE_LISTENER ); //Send to suscribed listeners

        } //END FadeBackFromColorComplete

        //---------------------------------------------//
        public bool IsLevelTransitionInProgress()
        //---------------------------------------------//
        {

            return levelTransitionInProgress;

        } //END IsLevelTransitionInProgress

    } //END Class
    
} //END Namespace
