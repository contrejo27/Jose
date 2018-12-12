using UnityEngine;
using System.Collections.Generic;
using System.Collections;
using System;
using UnityEngine.Events;

namespace BrandXR
{
    public class XRSkyboxManager: MonoBehaviour
    {
        private bool showDebug = false;

        [Tooltip( "The skybox applied to the main camera, used when we turn off VR mode in editor" )]
        private Skybox skybox_MainCamera;

        [Tooltip( "The skybox applied to the left camera" )]
        private Skybox skybox_LeftCamera;

        [Tooltip( "The skybox applied to the right camera" )]
        private Skybox skybox_RightCamera;

        [Tooltip( "The material used for the left camera skybox" )]
        public Material material_BlendedSixSidedCube_L;

        [Tooltip( "The material used for the right camera skybox" )]
        public Material material_BlendedSixSidedCube_R;

        [Tooltip( "A simple 1x1 black square texture we use when calling ForceToBlack(), this way we do not need to wait for Resources or StreamingAssets to load the texture from a bytes array." +
            " Very useful when we need to set the skyboxes to black on application start" )]
        public Texture texture_Black;

        [Tooltip( "Same as use and purpose as texture_Black" )]
        public Texture texture_White;

        public enum LoadStyle
        {
            AllAtOnce,
            Sequentially
        }
        [Tooltip( "Loading images AllAtOnce is 2x faster than waiting to load textures sequentially, but has a much higher chance of making your application hang when using large textures" )]
        public LoadStyle loadStyle = LoadStyle.AllAtOnce;

        public enum LoadFrom
        {
            DictionaryTexture,
            ResourcesTexture,
            ResourcesBytes,
            StreamingAssetsBytes
        }
        [Tooltip( "We can either look for images within the Resources or StreamingAssets folder. StreamingAssets is preferrable as it's www.nonReadableTexture loading is 3x faster than Resources.LoadImage" )]
        public LoadFrom loadFrom = LoadFrom.StreamingAssetsBytes;

        public enum FadeAndLoadStyle
        {
            JustLoad,
            LoadWithBlend,
            FadeToColorThenLoadThenFadeBackFromColor
        }
        [Tooltip( "We have different ways of transitioning between images, if you choose to fade and load together, this can cause a hiccup as textures are loaded. Otherwise we first show a black or white screen, load, and then fade in to the image" )]
        public FadeAndLoadStyle fadeAndLoadStyle = FadeAndLoadStyle.FadeToColorThenLoadThenFadeBackFromColor;


        [Tooltip( "Do you want to load the first image belonging to the current image tag?" )]
        public bool TweenToFirstImageAtStart = true;

        [Tooltip( "Should we fade an the music during fade out and then fade in (only used if FadeAndLoadStyle is set to Fade Out -> Load -> Fade In)?" )]
        private bool shouldWeFadeMusicAudioSource = true;

        private bool shouldWePlaySFX = false;
        private AudioClip fadeBackSFX;

        private AudioSource fadeMusicAudioSource;


        private float blendSpeed = 0f;

        private string string_SendMessageOnComplete;

        private bool bool_CheckForImageLoadingCompletion = false;

        private System.Diagnostics.Stopwatch stopwatch = new System.Diagnostics.Stopwatch();

        //public delegate void OnCompleteMethod(); //What do we want to call after this tween completes?
        public UnityEvent onFadeToComplete = new UnityEvent();
        public UnityEvent onLoadComplete = new UnityEvent();
        public UnityEvent onBlendToComplete = new UnityEvent();
        public UnityEvent onFadeBackComplete = new UnityEvent();

        private XRSkyboxFactory.ImageType imageToLoad = XRSkyboxFactory.ImageType.TestImageOne;
        private float fadeBackTweenSpeed = 2f;
        private float fadeBackDelay = 0f;
        private EaseCurve.EaseType fadeBackEaseType = EaseCurve.EaseType.Linear;

        public float MusicOnStartVolume = 0f;

        public AudioClip FadeInSFX;
        public AudioClip FadeOutSFX;
        
        // singleton behavior
        private static XRSkyboxManager _instance;

        //--------------------------------------------//
        public static XRSkyboxManager instance
        //--------------------------------------------//
        {
            get
            {
                if( _instance == null )
                {
                    if( GameObject.FindObjectOfType<XRSkyboxManager>() == null ) { PrefabManager.InstantiatePrefab( PrefabFactory.Prefabs.bxr_XRSkyboxManager ); }
                    _instance = GameObject.FindObjectOfType<XRSkyboxManager>();
                }

                return _instance;
            }

        } //END Instance

        //--------------------------------------------//
        void Awake()
        //--------------------------------------------//
        {
            if( showDebug ) { Debug.Log( "ImageManager.cs Awake() start" ); }

            DestroyDuplicateInstance();

            if( transform.parent == null )
            {
                DontDestroyOnLoad( transform.gameObject );
            }

            //Try to link to camera skyboxes. We can't use this tool without the skyboxes existing!
            FindOrCreateSkybox( "Main", ref skybox_MainCamera );
            FindOrCreateSkybox( "Left", ref skybox_LeftCamera );
            FindOrCreateSkybox( "Right", ref skybox_RightCamera );
            
            if( showDebug ) { Debug.Log( "ImageManager.cs Awake() end" ); }

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

        //--------------------------------------------//
        private void FindOrCreateSkybox( string camType, ref Skybox sky )
        //--------------------------------------------//
        {

            if( GameObject.FindObjectsOfType<Camera>() != null )
            {
                foreach( Camera cam in GameObject.FindObjectsOfType<Camera>() )
                {
                    if( cam.name.Contains( camType ) )
                    {
                        if( cam.GetComponent<Skybox>() != null )
                        {
                            sky = cam.GetComponent<Skybox>();
                        }
                        else
                        {
                            sky = cam.gameObject.AddComponent<Skybox>();
                        }
                    }
                }
            }

        } //END FindOrCreateSkybox

        //--------------------------------------------//
        public void Start()
        //--------------------------------------------//
        {
            if( skybox_MainCamera != null && skybox_LeftCamera != null && skybox_RightCamera != null )
            {
                if( showDebug ) { Debug.Log( "ImageManager.cs Start() about to call ForceMaterialsToBlack()" ); }

                ForceMaterialsToBlack();

                if( showDebug ) { Debug.Log( "ImageManager.cs Start() about to call LoadFirstImage()" ); }

                LoadFirstImage();

                if( showDebug ) { Debug.Log( "ImageManager.cs Start() finished calling LoadFirstImage()" ); }
            }

        } //END Start




        //--------------------------------------------//
        private void LoadFirstImage()
        //--------------------------------------------//
        {
            if( TweenToFirstImageAtStart )
            {
                if( showDebug ) { Debug.Log( "ImageManager.cs LoadFirstImage(), currentImageTag = " + XRSkyboxFactory.instance.currentImageTag ); }

                ScreenFadeManager.instance.Force( Color.black );

                LoadImageWithFade( GetFirstImage(), Color.black, .01f, 2f, 0f, 0f, EaseCurve.EaseType.Linear, EaseCurve.EaseType.Linear, false, null, null, false, null, 0f );
            }

        } //END LoadFirstImage




        //--------------------------------------------//
        public void ForceMaterialsToBlack()
        //--------------------------------------------//
        {

            material_BlendedSixSidedCube_L.SetFloat( "_Blend", 0f );
            material_BlendedSixSidedCube_R.SetFloat( "_Blend", 0f );

            FillImageSlot( XRSkyboxFactory.ImageSlot.First, texture_Black );
            ClearImageSlot( XRSkyboxFactory.ImageSlot.Second );

        } //END ForceMaterialsToBlack


        //--------------------------------------------//
        public void ForceMaterialsToWhite()
        //--------------------------------------------//
        {

            material_BlendedSixSidedCube_L.SetFloat( "_Blend", 0f );
            material_BlendedSixSidedCube_R.SetFloat( "_Blend", 0f );

            FillImageSlot( XRSkyboxFactory.ImageSlot.First, texture_White );
            ClearImageSlot( XRSkyboxFactory.ImageSlot.Second );

        } //END ForceMaterialsToWhite


        //--------------------------------------------//
        public void LoadImage( XRSkyboxFactory.ImageType imageToLoad )
        //--------------------------------------------//
        {
            if( showDebug ) { Debug.Log( "ImageManager.cs LoadImage() start" ); }

            XRSkyboxFactory.currentImageType = imageToLoad;
            this.blendSpeed = .01f;

            fadeAndLoadStyle = FadeAndLoadStyle.JustLoad;

            this.shouldWePlaySFX = false;
            this.fadeBackSFX = null;
            this.shouldWeFadeMusicAudioSource = false;
            this.fadeMusicAudioSource = null;

            this.onFadeToComplete = null;
            this.onLoadComplete = null;
            this.onBlendToComplete = null;
            this.onFadeBackComplete = null;

            _LoadImage( imageToLoad );

        } //END LoadImage

        //--------------------------------------------//
        public void LoadImageWithBlend( XRSkyboxFactory.ImageType imageToLoad, float blendSpeed )
        //--------------------------------------------//
        {
            if( showDebug ) { Debug.Log( "ImageManager.cs LoadImageWithBlend() start" ); }

            XRSkyboxFactory.currentImageType = imageToLoad;
            this.blendSpeed = blendSpeed;

            fadeAndLoadStyle = FadeAndLoadStyle.LoadWithBlend;

            this.shouldWePlaySFX = false;
            this.fadeBackSFX = null;
            this.shouldWeFadeMusicAudioSource = false;
            this.fadeMusicAudioSource = null;

            this.onFadeToComplete = null;
            this.onLoadComplete = null;
            this.onBlendToComplete = null;
            this.onFadeBackComplete = null;

            _LoadImage( imageToLoad );

        } //END LoadImageWithBlend

        //--------------------------------------------//
        public void LoadImageWithFade( XRSkyboxFactory.ImageType imageToLoad, Color fadeToColor, float fadeToTweenSpeed,
                float fadeBackTweenSpeed, float fadeToDelay, float fadeBackDelay, EaseCurve.EaseType fadeToEaseType, EaseCurve.EaseType fadeBackEaseType,
                bool shouldWePlaySFX, AudioClip fadeToSFX, AudioClip fadeBackSFX,
                bool shouldWeFadeMusic, AudioSource fadeMusicAudioSource, float fadeMusicToVolume )
        //--------------------------------------------//
        {
            if( showDebug ) { Debug.Log( "ImageManager.cs LoadImageWithFade() start" ); }

            XRSkyboxFactory.currentImageType = imageToLoad;
            fadeAndLoadStyle = FadeAndLoadStyle.FadeToColorThenLoadThenFadeBackFromColor;

            this.imageToLoad = imageToLoad;
            this.blendSpeed = .01f;
            this.fadeBackTweenSpeed = fadeBackTweenSpeed;
            this.fadeBackDelay = fadeBackDelay;
            this.fadeBackEaseType = fadeBackEaseType;

            this.shouldWePlaySFX = shouldWePlaySFX;
            this.fadeBackSFX = fadeBackSFX;
            this.shouldWeFadeMusicAudioSource = shouldWeFadeMusic;
            this.fadeMusicAudioSource = fadeMusicAudioSource;
            
            this.onFadeToComplete = null;
            this.onLoadComplete = null;
            this.onBlendToComplete = null;
            this.onFadeBackComplete = null;

            if( shouldWePlaySFX && fadeToSFX != null ) { AudioSource.PlayClipAtPoint( fadeToSFX, Camera.main.transform.position, 1f ); }

            if( this.shouldWeFadeMusicAudioSource && fadeMusicAudioSource != null ) { AudioHelper.instance.Fade( fadeMusicAudioSource, fadeMusicToVolume, fadeToTweenSpeed, 0f ); }

            UnityEvent _event = new UnityEvent();
            _event.AddListener( FadeToColorComplete );

            ScreenFadeManager.instance.Show( fadeToColor, fadeToTweenSpeed, fadeToDelay, fadeToEaseType, _event );

        } //END LoadImageWithFade

        //--------------------------------------------//
        public void LoadImageWithFade( XRSkyboxFactory.ImageType imageToLoad, Color fadeToColor, float fadeToTweenSpeed, float fadeBackTweenSpeed,
                                        float fadeToColorDelay, float fadeBackDelay, EaseCurve.EaseType fadeToEaseType, EaseCurve.EaseType fadeBackEaseType,
                                        bool shouldWePlaySFX, AudioClip fadeToSFX, AudioClip fadeBackSFX, 
                                        bool shouldWeFadeMusic, AudioSource fadeMusicAudioSource, float fadeMusicToVolume,
                                        UnityEvent onFadeToColorComplete, UnityEvent onLoadImageComplete, UnityEvent onBlendToImageComplete, UnityEvent onFadeBackFromColorComplete )
        //--------------------------------------------//
        {
            if( showDebug ) { Debug.Log( "ImageManager.cs LoadImageWithFade() start" ); }

            XRSkyboxFactory.currentImageType = imageToLoad;
            fadeAndLoadStyle = FadeAndLoadStyle.FadeToColorThenLoadThenFadeBackFromColor;

            this.imageToLoad = imageToLoad;
            this.blendSpeed = .01f;
            this.fadeBackTweenSpeed = fadeBackTweenSpeed;
            this.fadeBackDelay = fadeBackDelay;
            this.fadeBackEaseType = fadeBackEaseType;

            this.shouldWePlaySFX = shouldWePlaySFX;
            this.fadeBackSFX = fadeBackSFX;
            this.shouldWeFadeMusicAudioSource = shouldWeFadeMusic;
            this.fadeMusicAudioSource = fadeMusicAudioSource;

            this.onFadeToComplete = onFadeToColorComplete;
            this.onLoadComplete = onLoadImageComplete;
            this.onBlendToComplete = onBlendToImageComplete;
            this.onFadeBackComplete = onFadeBackFromColorComplete;

            if( shouldWePlaySFX && fadeToSFX != null ) { AudioSource.PlayClipAtPoint( fadeToSFX, Camera.main.transform.position, 1f ); }

            if( this.shouldWeFadeMusicAudioSource && fadeMusicAudioSource != null ) { AudioHelper.instance.Fade( fadeMusicAudioSource, fadeMusicToVolume, fadeToTweenSpeed, 0f ); }

            UnityEvent _event = new UnityEvent();
            _event.AddListener( FadeToColorComplete );

            ScreenFadeManager.instance.Show( fadeToColor, fadeToTweenSpeed, fadeToColorDelay, fadeToEaseType, _event );

        } //END LoadImageWithFade

        //--------------------------------------------//
        private void FadeToColorComplete()
        //--------------------------------------------//
        {
            if( showDebug ) { Debug.Log( "ImageManager.cs FadeToColorComplete() start" ); }

            //Send a message out to other scripts that a new image has been loaded
            Messenger.Broadcast( "ImageManager_FadeToColorComplete", MessengerMode.DONT_REQUIRE_LISTENER );

            if( showDebug ) { Debug.Log( "ImageManager.cs FadeToColorComplete() about to call onFadeToColorComplete() function is one exists" ); }

            //Send a message out that the fade to color effect has completed
            if( onFadeToComplete != null ) { onFadeToComplete.Invoke(); }

            if( showDebug ) { Debug.Log( "ImageManager.cs FadeToColorComplete() about to call _LoadImage( " + this.imageToLoad + " )" ); }

            _LoadImage( this.imageToLoad );

        } //END FadeToColorComplete


        //--------------------------------------------//
        private void _LoadImage( XRSkyboxFactory.ImageType type )
        //--------------------------------------------//
        {
            if( showDebug ) { Debug.Log( "ImageManager.cs _LoadImage( " + type + " ) start" ); }

            //For debug purposes, we track how long loading the image takes
            stopwatch.Reset();
            stopwatch.Start();
            
            //If we're tweening, stop and reset our blending to 0 (1st set of images in each skybox material)
            if( TweenManager.IsTweenLinked( material_BlendedSixSidedCube_L ) || TweenManager.IsTweenLinked( material_BlendedSixSidedCube_R ) )
            {
                material_BlendedSixSidedCube_L.TweenFloat( "_Blend", 0f, 0f, EaseCurve.Linear );
                material_BlendedSixSidedCube_R.TweenFloat( "_Blend", 0f, 0f, EaseCurve.Linear );
            }

            //Prepare for the loading procedure by  setting up some variables
            XRSkyboxFactory.instance.SetLoadedBooleansFalse();
            XRSkyboxFactory.instance.SetIsReadyToTweenToSecondSlot( false );

            //Allow Update to check for the images being completed loaded
            bool_CheckForImageLoadingCompletion = true;

            //Prevent other tweens from occuring until existing tween is completed
            XRSkyboxFactory.instance.SetIsTweening( true );

            if( showDebug ) { Debug.Log( "ImageManager.cs _LoadImage( " + type + " ) about to call SetCameraRotation()" ); }

            //Place the image we want to tween to into the Second image slot of the material so we can blend to it.
            //To do this we first need to asynchronously load in the image into memory to minimize framerate hiccups.
            //We'll check for this in an update, when it completes we can continue the tweening logic
            if( loadStyle == LoadStyle.Sequentially )
            {
                if( showDebug ) { Debug.Log( "ImageManager.cs _LoadImage( " + type + " ) about to call LoadImagesSequentially( " + XRSkyboxFactory.ImageSlot.Second + ", " + XRSkyboxFactory.currentImageType + " )" ); }
                StartCoroutine( LoadImagesSequentially( XRSkyboxFactory.ImageSlot.Second, XRSkyboxFactory.currentImageType ) );
            }
            else if( loadStyle == LoadStyle.AllAtOnce )
            {
                if( showDebug ) { Debug.Log( "ImageManager.cs _LoadImage( " + type + " ) about to call LoadAllImagesAtOnce( " + XRSkyboxFactory.ImageSlot.Second + ", " + XRSkyboxFactory.currentImageType + " )" ); }
                LoadAllImagesAtOnce( XRSkyboxFactory.ImageSlot.Second, XRSkyboxFactory.currentImageType );
            }

        } //END _LoadImage


        //--------------------------------------------//
        public void Update()
        //--------------------------------------------//
        {
            //When the images are fully loaded, continue
            if( bool_CheckForImageLoadingCompletion )
            {
                if( XRSkyboxFactory.instance.AreAllLoadedBooleansTrue() && XRSkyboxFactory.instance.IsReadyToTweenToSecondSlot() )
                {
                    LoadImagesComplete();
                }
            }

        } //END Update

        //--------------------------------------------//
        private void LoadImagesComplete()
        //--------------------------------------------//
        {
            if( showDebug ) { Debug.Log( "ImageManager.cs LoadImagesComplete() start" ); }

            //Stop the stopwatch we use for debugging how long our methods are taking
            stopwatch.Stop();

            //Stop checking on Update for the images to be loaded
            bool_CheckForImageLoadingCompletion = false;

            //Set the materials to the appropriate loaded textures
            FillImageSlot( XRSkyboxFactory.ImageSlot.Second, XRSkyboxFactory.currentImageType );

            //Send a message out to other scripts that a new image has been loaded
            Messenger.Broadcast( "ImageManager_LoadImageComplete", MessengerMode.DONT_REQUIRE_LISTENER );

            //Send a message out that the image has loaded
            if( onLoadComplete != null ) { onLoadComplete.Invoke(); }

            //The images are finished loading, now we can tween to them
            UnityEvent _event = new UnityEvent();
            _event.AddListener( TweenToImageComplete );

            material_BlendedSixSidedCube_L.TweenFloat( "_Blend", 1f, blendSpeed, EaseCurve.Linear );
            material_BlendedSixSidedCube_R.TweenFloat( "_Blend", 1f, blendSpeed, EaseCurve.Linear, material_BlendedSixSidedCube_R.GetFloat( "_Blend" ), 0f, false, _event );
            
        } //END LoadImagesComplete

        //--------------------------------------------//
        public void TweenToImageComplete()
        //--------------------------------------------//
        {
            if( showDebug ) { Debug.Log( "ImageManager.cs TweenToImageComplete() start" ); }

            //Send a message out to other scripts that a new image has been loaded
            Messenger.Broadcast( "ImageManager_OnBlendToImageComplete", MessengerMode.DONT_REQUIRE_LISTENER );

            if( showDebug ) { Debug.Log( "ImageManager.cs TweenToImageComplete() called ImageManager_OnBlendToImageComplete broadcast" ); }

            //Send a message out that we have finished blending between the two images
            if( onBlendToComplete != null ) { onBlendToComplete.Invoke(); }

            if( showDebug ) { Debug.Log( "ImageManager.cs TweenToImageComplete() called onBlendToImageComplete()" ); }

            //Once the tween completes, Set the skybox's first image slot to be of the image we just tweened to.
            FillImageSlot( XRSkyboxFactory.ImageSlot.First, XRSkyboxFactory.currentImageType );

            //And then wipe the skybox's second image slot, so that it's ready for the next time we need to tween.
            ClearImageSlot( XRSkyboxFactory.ImageSlot.Second );

            //Now force the skybox to be show the first image slot... which means we're ready to tween again!
            material_BlendedSixSidedCube_L.SetFloat( "_Blend", 0f );
            material_BlendedSixSidedCube_R.SetFloat( "_Blend", 0f );

            Resources.UnloadUnusedAssets();

            XRSkyboxFactory.instance.SetIsTweening( false );


            //If this came from Start(), then we didn't play Fade In/Out SFX, we can set this variable to false to allow these SFX to play in the future
            if( TweenToFirstImageAtStart )
            {
                TweenToFirstImageAtStart = false;
            }

            if( fadeAndLoadStyle == FadeAndLoadStyle.FadeToColorThenLoadThenFadeBackFromColor )
            {
                //Play the fade in sfx and return the bgm to normal
                if( shouldWePlaySFX && fadeBackSFX != null ) { AudioSource.PlayClipAtPoint( fadeBackSFX, Camera.main.transform.position, 1f ); }
                if( this.shouldWeFadeMusicAudioSource && fadeMusicAudioSource != null ) { AudioHelper.instance.Fade( fadeMusicAudioSource, MusicOnStartVolume, fadeBackTweenSpeed, 0f ); }

                //Fade out the color
                //Debug.Log( "TweenToImageComplete() calling ScreenFadeManager.Hide()" );
                UnityEvent _event = new UnityEvent();
                _event.AddListener( FadeBackFromColorComplete );

                ScreenFadeManager.instance.Hide( fadeBackTweenSpeed, fadeBackDelay, fadeBackEaseType, _event );
            }

        } //END TweenToImageComplete

        //--------------------------------------------//
        private void FadeBackFromColorComplete()
        //--------------------------------------------//
        {
            if( showDebug ) { Debug.Log( "ImageManager.cs FadeBackFromColorComplete() start" ); }

            //Send a message out to other scripts that a new image has been loaded
            Messenger.Broadcast( "ImageManager_FadeBackFromColorComplete", MessengerMode.DONT_REQUIRE_LISTENER );

            if( showDebug ) { Debug.Log( "ImageManager.cs FadeBackFromColorComplete() called ImageManager_FadeBackFromColorComplete broadcast()" ); }

            //Send a message out that we have finished hiding the fade effect
            if( onFadeBackComplete != null ) { onFadeBackComplete.Invoke(); }

            if( showDebug ) { Debug.Log( "ImageManager.cs FadeBackFromColorComplete() called onFadeBackFromColorComplete() function if it exists" ); }

        } //END FadeBackFromColorComplete

        //--------------------------------------------//
        public void ClearImageSlot( XRSkyboxFactory.ImageSlot slot ) //Sets the sykbox's image slot to empty
        //--------------------------------------------//
        {

            XRSkyboxFactory.CubeSide cubeSide;

            //LEFT CAMERA
            cubeSide = XRSkyboxFactory.CubeSide.front;
            material_BlendedSixSidedCube_L.SetTexture( GetTextureSlotName( slot, cubeSide ), null );

            cubeSide = XRSkyboxFactory.CubeSide.back;
            material_BlendedSixSidedCube_L.SetTexture( GetTextureSlotName( slot, cubeSide ), null );

            cubeSide = XRSkyboxFactory.CubeSide.left;
            material_BlendedSixSidedCube_L.SetTexture( GetTextureSlotName( slot, cubeSide ), null );

            cubeSide = XRSkyboxFactory.CubeSide.right;
            material_BlendedSixSidedCube_L.SetTexture( GetTextureSlotName( slot, cubeSide ), null );

            cubeSide = XRSkyboxFactory.CubeSide.top;
            material_BlendedSixSidedCube_L.SetTexture( GetTextureSlotName( slot, cubeSide ), null );

            cubeSide = XRSkyboxFactory.CubeSide.bottom;
            material_BlendedSixSidedCube_L.SetTexture( GetTextureSlotName( slot, cubeSide ), null );

            //RIGHT CAMERA
            cubeSide = XRSkyboxFactory.CubeSide.front;
            material_BlendedSixSidedCube_R.SetTexture( GetTextureSlotName( slot, cubeSide ), null );

            cubeSide = XRSkyboxFactory.CubeSide.back;
            material_BlendedSixSidedCube_R.SetTexture( GetTextureSlotName( slot, cubeSide ), null );

            cubeSide = XRSkyboxFactory.CubeSide.left;
            material_BlendedSixSidedCube_R.SetTexture( GetTextureSlotName( slot, cubeSide ), null );

            cubeSide = XRSkyboxFactory.CubeSide.right;
            material_BlendedSixSidedCube_R.SetTexture( GetTextureSlotName( slot, cubeSide ), null );

            cubeSide = XRSkyboxFactory.CubeSide.top;
            material_BlendedSixSidedCube_R.SetTexture( GetTextureSlotName( slot, cubeSide ), null );

            cubeSide = XRSkyboxFactory.CubeSide.bottom;
            material_BlendedSixSidedCube_R.SetTexture( GetTextureSlotName( slot, cubeSide ), null );

        } //END ClearImageSlot

        //--------------------------------------------//
        public void ClearImageSlot( XRSkyboxFactory.CameraType cameraType, XRSkyboxFactory.ImageSlot slot ) //Sets the sykbox's image slot to empty
        //--------------------------------------------//
        {
            XRSkyboxFactory.CubeSide cubeSide;

            //LEFT CAMERA
            if( cameraType == XRSkyboxFactory.CameraType.Left )
            {
                cubeSide = XRSkyboxFactory.CubeSide.front;
                material_BlendedSixSidedCube_L.SetTexture( GetTextureSlotName( slot, cubeSide ), null );

                cubeSide = XRSkyboxFactory.CubeSide.back;
                material_BlendedSixSidedCube_L.SetTexture( GetTextureSlotName( slot, cubeSide ), null );

                cubeSide = XRSkyboxFactory.CubeSide.left;
                material_BlendedSixSidedCube_L.SetTexture( GetTextureSlotName( slot, cubeSide ), null );

                cubeSide = XRSkyboxFactory.CubeSide.right;
                material_BlendedSixSidedCube_L.SetTexture( GetTextureSlotName( slot, cubeSide ), null );

                cubeSide = XRSkyboxFactory.CubeSide.top;
                material_BlendedSixSidedCube_L.SetTexture( GetTextureSlotName( slot, cubeSide ), null );

                cubeSide = XRSkyboxFactory.CubeSide.bottom;
                material_BlendedSixSidedCube_L.SetTexture( GetTextureSlotName( slot, cubeSide ), null );
            }

            //RIGHT CAMERA
            else if( cameraType == XRSkyboxFactory.CameraType.Right )
            {
                cubeSide = XRSkyboxFactory.CubeSide.front;
                material_BlendedSixSidedCube_R.SetTexture( GetTextureSlotName( slot, cubeSide ), null );

                cubeSide = XRSkyboxFactory.CubeSide.back;
                material_BlendedSixSidedCube_R.SetTexture( GetTextureSlotName( slot, cubeSide ), null );

                cubeSide = XRSkyboxFactory.CubeSide.left;
                material_BlendedSixSidedCube_R.SetTexture( GetTextureSlotName( slot, cubeSide ), null );

                cubeSide = XRSkyboxFactory.CubeSide.right;
                material_BlendedSixSidedCube_R.SetTexture( GetTextureSlotName( slot, cubeSide ), null );

                cubeSide = XRSkyboxFactory.CubeSide.top;
                material_BlendedSixSidedCube_R.SetTexture( GetTextureSlotName( slot, cubeSide ), null );

                cubeSide = XRSkyboxFactory.CubeSide.bottom;
                material_BlendedSixSidedCube_R.SetTexture( GetTextureSlotName( slot, cubeSide ), null );
            }

        } //END ClearImageSlot

        //--------------------------------------------//
        public void ClearImageSlot( XRSkyboxFactory.CameraType cameraType, XRSkyboxFactory.CubeSide cubeSide, XRSkyboxFactory.ImageSlot slot ) //Sets the sykbox's image slot to empty
        //--------------------------------------------//
        {
            //LEFT CAMERA
            if( cameraType == XRSkyboxFactory.CameraType.Left )
            {
                if( cubeSide == XRSkyboxFactory.CubeSide.front )
                {
                    material_BlendedSixSidedCube_L.SetTexture( GetTextureSlotName( slot, cubeSide ), null );
                }
                else if( cubeSide == XRSkyboxFactory.CubeSide.back )
                {
                    material_BlendedSixSidedCube_L.SetTexture( GetTextureSlotName( slot, cubeSide ), null );
                }
                else if( cubeSide == XRSkyboxFactory.CubeSide.left )
                {
                    material_BlendedSixSidedCube_L.SetTexture( GetTextureSlotName( slot, cubeSide ), null );
                }
                else if( cubeSide == XRSkyboxFactory.CubeSide.right )
                {
                    material_BlendedSixSidedCube_L.SetTexture( GetTextureSlotName( slot, cubeSide ), null );
                }
                else if( cubeSide == XRSkyboxFactory.CubeSide.top )
                {
                    material_BlendedSixSidedCube_L.SetTexture( GetTextureSlotName( slot, cubeSide ), null );
                }
                else if( cubeSide == XRSkyboxFactory.CubeSide.bottom )
                {
                    material_BlendedSixSidedCube_L.SetTexture( GetTextureSlotName( slot, cubeSide ), null );
                }
            }

            //RIGHT CAMERA
            else if( cameraType == XRSkyboxFactory.CameraType.Right )
            {
                if( cubeSide == XRSkyboxFactory.CubeSide.front )
                {
                    material_BlendedSixSidedCube_R.SetTexture( GetTextureSlotName( slot, cubeSide ), null );
                }
                else if( cubeSide == XRSkyboxFactory.CubeSide.back )
                {
                    material_BlendedSixSidedCube_R.SetTexture( GetTextureSlotName( slot, cubeSide ), null );
                }
                else if( cubeSide == XRSkyboxFactory.CubeSide.left )
                {
                    material_BlendedSixSidedCube_R.SetTexture( GetTextureSlotName( slot, cubeSide ), null );
                }
                else if( cubeSide == XRSkyboxFactory.CubeSide.right )
                {
                    material_BlendedSixSidedCube_R.SetTexture( GetTextureSlotName( slot, cubeSide ), null );
                }
                else if( cubeSide == XRSkyboxFactory.CubeSide.top )
                {
                    material_BlendedSixSidedCube_R.SetTexture( GetTextureSlotName( slot, cubeSide ), null );
                }
                else if( cubeSide == XRSkyboxFactory.CubeSide.bottom )
                {
                    material_BlendedSixSidedCube_R.SetTexture( GetTextureSlotName( slot, cubeSide ), null );
                }
            }

        } //END ClearImageSlot

        //--------------------------------------------//
        private void CopyImageSlot( XRSkyboxFactory.CameraType fromCamera, XRSkyboxFactory.ImageSlot fromSlot, XRSkyboxFactory.CameraType toCamera, XRSkyboxFactory.ImageSlot toSlot )
        //--------------------------------------------//
        {
            //Take the textures from one camera slot and copy them into another
            if( fromCamera == XRSkyboxFactory.CameraType.Left )
            {
                XRSkyboxFactory.CubeSide cubeSide = XRSkyboxFactory.CubeSide.front;
                FillImageSlot( toCamera, toSlot, cubeSide, material_BlendedSixSidedCube_L.GetTexture( GetTextureSlotName( fromSlot, cubeSide ) ) );

                cubeSide = XRSkyboxFactory.CubeSide.back;
                FillImageSlot( toCamera, toSlot, cubeSide, material_BlendedSixSidedCube_L.GetTexture( GetTextureSlotName( fromSlot, cubeSide ) ) );

                cubeSide = XRSkyboxFactory.CubeSide.left;
                FillImageSlot( toCamera, toSlot, cubeSide, material_BlendedSixSidedCube_L.GetTexture( GetTextureSlotName( fromSlot, cubeSide ) ) );

                cubeSide = XRSkyboxFactory.CubeSide.right;
                FillImageSlot( toCamera, toSlot, cubeSide, material_BlendedSixSidedCube_L.GetTexture( GetTextureSlotName( fromSlot, cubeSide ) ) );

                cubeSide = XRSkyboxFactory.CubeSide.top;
                FillImageSlot( toCamera, toSlot, cubeSide, material_BlendedSixSidedCube_L.GetTexture( GetTextureSlotName( fromSlot, cubeSide ) ) );

                cubeSide = XRSkyboxFactory.CubeSide.bottom;
                FillImageSlot( toCamera, toSlot, cubeSide, material_BlendedSixSidedCube_L.GetTexture( GetTextureSlotName( fromSlot, cubeSide ) ) );
            }
            else if( fromCamera == XRSkyboxFactory.CameraType.Right )
            {
                XRSkyboxFactory.CubeSide cubeSide = XRSkyboxFactory.CubeSide.front;
                FillImageSlot( toCamera, toSlot, cubeSide, material_BlendedSixSidedCube_R.GetTexture( GetTextureSlotName( fromSlot, cubeSide ) ) );

                cubeSide = XRSkyboxFactory.CubeSide.back;
                FillImageSlot( toCamera, toSlot, cubeSide, material_BlendedSixSidedCube_R.GetTexture( GetTextureSlotName( fromSlot, cubeSide ) ) );

                cubeSide = XRSkyboxFactory.CubeSide.left;
                FillImageSlot( toCamera, toSlot, cubeSide, material_BlendedSixSidedCube_R.GetTexture( GetTextureSlotName( fromSlot, cubeSide ) ) );

                cubeSide = XRSkyboxFactory.CubeSide.right;
                FillImageSlot( toCamera, toSlot, cubeSide, material_BlendedSixSidedCube_R.GetTexture( GetTextureSlotName( fromSlot, cubeSide ) ) );

                cubeSide = XRSkyboxFactory.CubeSide.top;
                FillImageSlot( toCamera, toSlot, cubeSide, material_BlendedSixSidedCube_R.GetTexture( GetTextureSlotName( fromSlot, cubeSide ) ) );

                cubeSide = XRSkyboxFactory.CubeSide.bottom;
                FillImageSlot( toCamera, toSlot, cubeSide, material_BlendedSixSidedCube_R.GetTexture( GetTextureSlotName( fromSlot, cubeSide ) ) );
            }


        } //END CopyImageSlot


        //--------------------------------------------//
        private IEnumerator LoadImagesSequentially( XRSkyboxFactory.ImageSlot slot, XRSkyboxFactory.ImageType imageType )
        //--------------------------------------------//
        {
            //We call for a coroutine to begin loading the texture for each image slot

            XRSkyboxFactory.CameraType cameraType;
            XRSkyboxFactory.CubeSide cubeSide;

            //LEFT CAMERA
            cameraType = XRSkyboxFactory.CameraType.Left;

            cubeSide = XRSkyboxFactory.CubeSide.front;
            XRSkyboxFactory.instance.BeginLoadingTexture( cameraType, cubeSide, imageType );
            while( !XRSkyboxFactory.instance.GetLoadedBool( cameraType, cubeSide ) ) { yield return null; }

            cubeSide = XRSkyboxFactory.CubeSide.back;
            XRSkyboxFactory.instance.BeginLoadingTexture( cameraType, cubeSide, imageType );
            while( !XRSkyboxFactory.instance.GetLoadedBool( cameraType, cubeSide ) ) { yield return null; }

            cubeSide = XRSkyboxFactory.CubeSide.left;
            XRSkyboxFactory.instance.BeginLoadingTexture( cameraType, cubeSide, imageType );
            while( !XRSkyboxFactory.instance.GetLoadedBool( cameraType, cubeSide ) ) { yield return null; }

            cubeSide = XRSkyboxFactory.CubeSide.right;
            XRSkyboxFactory.instance.BeginLoadingTexture( cameraType, cubeSide, imageType );
            while( !XRSkyboxFactory.instance.GetLoadedBool( cameraType, cubeSide ) ) { yield return null; }

            cubeSide = XRSkyboxFactory.CubeSide.top;
            XRSkyboxFactory.instance.BeginLoadingTexture( cameraType, cubeSide, imageType );
            while( !XRSkyboxFactory.instance.GetLoadedBool( cameraType, cubeSide ) ) { yield return null; }

            cubeSide = XRSkyboxFactory.CubeSide.bottom;
            XRSkyboxFactory.instance.BeginLoadingTexture( cameraType, cubeSide, imageType );
            while( !XRSkyboxFactory.instance.GetLoadedBool( cameraType, cubeSide ) ) { yield return null; }

            //RIGHT CAMERA
            cameraType = XRSkyboxFactory.CameraType.Right;

            cubeSide = XRSkyboxFactory.CubeSide.front;
            XRSkyboxFactory.instance.BeginLoadingTexture( cameraType, cubeSide, imageType );
            while( !XRSkyboxFactory.instance.GetLoadedBool( cameraType, cubeSide ) ) { yield return null; }

            cubeSide = XRSkyboxFactory.CubeSide.back;
            XRSkyboxFactory.instance.BeginLoadingTexture( cameraType, cubeSide, imageType );
            while( !XRSkyboxFactory.instance.GetLoadedBool( cameraType, cubeSide ) ) { yield return null; }

            cubeSide = XRSkyboxFactory.CubeSide.left;
            XRSkyboxFactory.instance.BeginLoadingTexture( cameraType, cubeSide, imageType );
            while( !XRSkyboxFactory.instance.GetLoadedBool( cameraType, cubeSide ) ) { yield return null; }

            cubeSide = XRSkyboxFactory.CubeSide.right;
            XRSkyboxFactory.instance.BeginLoadingTexture( cameraType, cubeSide, imageType );
            while( !XRSkyboxFactory.instance.GetLoadedBool( cameraType, cubeSide ) ) { yield return null; }

            cubeSide = XRSkyboxFactory.CubeSide.top;
            XRSkyboxFactory.instance.BeginLoadingTexture( cameraType, cubeSide, imageType );
            while( !XRSkyboxFactory.instance.GetLoadedBool( cameraType, cubeSide ) ) { yield return null; }

            cubeSide = XRSkyboxFactory.CubeSide.bottom;
            XRSkyboxFactory.instance.BeginLoadingTexture( cameraType, cubeSide, imageType );
            while( !XRSkyboxFactory.instance.GetLoadedBool( cameraType, cubeSide ) ) { yield return null; }

            XRSkyboxFactory.instance.SetIsReadyToTweenToSecondSlot( true );

            Resources.UnloadUnusedAssets();

        } //END LoadImagesSequentially

        //--------------------------------------------//
        private void LoadAllImagesAtOnce( XRSkyboxFactory.ImageSlot slot, XRSkyboxFactory.ImageType imageType )
        //--------------------------------------------//
        {
            //We call for a coroutine to begin loading the texture for each image slot

            XRSkyboxFactory.CameraType cameraType;
            XRSkyboxFactory.CubeSide cubeSide;

            //LEFT CAMERA
            cameraType = XRSkyboxFactory.CameraType.Left;

            cubeSide = XRSkyboxFactory.CubeSide.front;
            XRSkyboxFactory.instance.BeginLoadingTexture( cameraType, cubeSide, imageType );

            cubeSide = XRSkyboxFactory.CubeSide.back;
            XRSkyboxFactory.instance.BeginLoadingTexture( cameraType, cubeSide, imageType );

            cubeSide = XRSkyboxFactory.CubeSide.left;
            XRSkyboxFactory.instance.BeginLoadingTexture( cameraType, cubeSide, imageType );

            cubeSide = XRSkyboxFactory.CubeSide.right;
            XRSkyboxFactory.instance.BeginLoadingTexture( cameraType, cubeSide, imageType );

            cubeSide = XRSkyboxFactory.CubeSide.top;
            XRSkyboxFactory.instance.BeginLoadingTexture( cameraType, cubeSide, imageType );

            cubeSide = XRSkyboxFactory.CubeSide.bottom;
            XRSkyboxFactory.instance.BeginLoadingTexture( cameraType, cubeSide, imageType );

            //RIGHT CAMERA
            cameraType = XRSkyboxFactory.CameraType.Right;

            cubeSide = XRSkyboxFactory.CubeSide.front;
            XRSkyboxFactory.instance.BeginLoadingTexture( cameraType, cubeSide, imageType );

            cubeSide = XRSkyboxFactory.CubeSide.back;
            XRSkyboxFactory.instance.BeginLoadingTexture( cameraType, cubeSide, imageType );

            cubeSide = XRSkyboxFactory.CubeSide.left;
            XRSkyboxFactory.instance.BeginLoadingTexture( cameraType, cubeSide, imageType );

            cubeSide = XRSkyboxFactory.CubeSide.right;
            XRSkyboxFactory.instance.BeginLoadingTexture( cameraType, cubeSide, imageType );

            cubeSide = XRSkyboxFactory.CubeSide.top;
            XRSkyboxFactory.instance.BeginLoadingTexture( cameraType, cubeSide, imageType );

            cubeSide = XRSkyboxFactory.CubeSide.bottom;
            XRSkyboxFactory.instance.BeginLoadingTexture( cameraType, cubeSide, imageType );

            XRSkyboxFactory.instance.SetIsReadyToTweenToSecondSlot( true );

            Resources.UnloadUnusedAssets();

        } //END LoadAllImagesAtOnce

        //--------------------------------------------//
        public void FillImageSlot( XRSkyboxFactory.ImageSlot slot, Texture texture )
        //--------------------------------------------//
        {

            XRSkyboxFactory.CubeSide cubeSide;

            //LEFT CAMERA
            cubeSide = XRSkyboxFactory.CubeSide.front;
            material_BlendedSixSidedCube_L.SetTexture( GetTextureSlotName( slot, cubeSide ), texture );

            cubeSide = XRSkyboxFactory.CubeSide.back;
            material_BlendedSixSidedCube_L.SetTexture( GetTextureSlotName( slot, cubeSide ), texture );

            cubeSide = XRSkyboxFactory.CubeSide.left;
            material_BlendedSixSidedCube_L.SetTexture( GetTextureSlotName( slot, cubeSide ), texture );

            cubeSide = XRSkyboxFactory.CubeSide.right;
            material_BlendedSixSidedCube_L.SetTexture( GetTextureSlotName( slot, cubeSide ), texture );

            cubeSide = XRSkyboxFactory.CubeSide.top;
            material_BlendedSixSidedCube_L.SetTexture( GetTextureSlotName( slot, cubeSide ), texture );

            cubeSide = XRSkyboxFactory.CubeSide.bottom;
            material_BlendedSixSidedCube_L.SetTexture( GetTextureSlotName( slot, cubeSide ), texture );

            //RIGHT CAMERA
            cubeSide = XRSkyboxFactory.CubeSide.front;
            material_BlendedSixSidedCube_R.SetTexture( GetTextureSlotName( slot, cubeSide ), texture );

            cubeSide = XRSkyboxFactory.CubeSide.back;
            material_BlendedSixSidedCube_R.SetTexture( GetTextureSlotName( slot, cubeSide ), texture );

            cubeSide = XRSkyboxFactory.CubeSide.left;
            material_BlendedSixSidedCube_R.SetTexture( GetTextureSlotName( slot, cubeSide ), texture );

            cubeSide = XRSkyboxFactory.CubeSide.right;
            material_BlendedSixSidedCube_R.SetTexture( GetTextureSlotName( slot, cubeSide ), texture );

            cubeSide = XRSkyboxFactory.CubeSide.top;
            material_BlendedSixSidedCube_R.SetTexture( GetTextureSlotName( slot, cubeSide ), texture );

            cubeSide = XRSkyboxFactory.CubeSide.bottom;
            material_BlendedSixSidedCube_R.SetTexture( GetTextureSlotName( slot, cubeSide ), texture );

        } //END FillImageSlot

        //--------------------------------------------//
        public void FillImageSlot( XRSkyboxFactory.CameraType cameraType, XRSkyboxFactory.ImageSlot slot, XRSkyboxFactory.CubeSide cubeSide, Texture texture )
        //--------------------------------------------//
        {

            //LEFT CAMERA
            if( cameraType == XRSkyboxFactory.CameraType.Left )
            {
                if( cubeSide == XRSkyboxFactory.CubeSide.front )
                {
                    material_BlendedSixSidedCube_L.SetTexture( GetTextureSlotName( slot, cubeSide ), texture );
                }
                else if( cubeSide == XRSkyboxFactory.CubeSide.back )
                {
                    material_BlendedSixSidedCube_L.SetTexture( GetTextureSlotName( slot, cubeSide ), texture );
                }
                else if( cubeSide == XRSkyboxFactory.CubeSide.left )
                {
                    material_BlendedSixSidedCube_L.SetTexture( GetTextureSlotName( slot, cubeSide ), texture );
                }
                else if( cubeSide == XRSkyboxFactory.CubeSide.right )
                {
                    material_BlendedSixSidedCube_L.SetTexture( GetTextureSlotName( slot, cubeSide ), texture );
                }
                else if( cubeSide == XRSkyboxFactory.CubeSide.top )
                {
                    material_BlendedSixSidedCube_L.SetTexture( GetTextureSlotName( slot, cubeSide ), texture );
                }
                else if( cubeSide == XRSkyboxFactory.CubeSide.bottom )
                {
                    material_BlendedSixSidedCube_L.SetTexture( GetTextureSlotName( slot, cubeSide ), texture );
                }
            }

            //RIGHT CAMERA
            else if( cameraType == XRSkyboxFactory.CameraType.Right )
            {
                if( cubeSide == XRSkyboxFactory.CubeSide.front )
                {
                    material_BlendedSixSidedCube_R.SetTexture( GetTextureSlotName( slot, cubeSide ), texture );
                }
                else if( cubeSide == XRSkyboxFactory.CubeSide.back )
                {
                    material_BlendedSixSidedCube_R.SetTexture( GetTextureSlotName( slot, cubeSide ), texture );
                }
                else if( cubeSide == XRSkyboxFactory.CubeSide.left )
                {
                    material_BlendedSixSidedCube_R.SetTexture( GetTextureSlotName( slot, cubeSide ), texture );
                }
                else if( cubeSide == XRSkyboxFactory.CubeSide.right )
                {
                    material_BlendedSixSidedCube_R.SetTexture( GetTextureSlotName( slot, cubeSide ), texture );
                }
                else if( cubeSide == XRSkyboxFactory.CubeSide.top )
                {
                    material_BlendedSixSidedCube_R.SetTexture( GetTextureSlotName( slot, cubeSide ), texture );
                }
                else if( cubeSide == XRSkyboxFactory.CubeSide.bottom )
                {
                    material_BlendedSixSidedCube_R.SetTexture( GetTextureSlotName( slot, cubeSide ), texture );
                }
            }

        } //END FillImageSlot

        //--------------------------------------------//
        public void FillImageSlot( XRSkyboxFactory.CameraType cameraType, XRSkyboxFactory.ImageSlot slot, Dictionary<XRSkyboxFactory.CubeSide, Texture> skyboxTextures )
        //--------------------------------------------//
        {

            foreach( KeyValuePair<XRSkyboxFactory.CubeSide, Texture> pair in skyboxTextures )
            {
                FillImageSlot( cameraType, slot, pair.Key, pair.Value );
            }

        } //END FillImageSlot


        //--------------------------------------------//
        public void FillImageSlot( XRSkyboxFactory.ImageSlot slot, XRSkyboxFactory.ImageType imageType )
        //--------------------------------------------//
        {
            //We call for a coroutine to begin loading the texture for each image slot

            XRSkyboxFactory.CameraType cameraType;
            XRSkyboxFactory.CubeSide cubeSide;

            //LEFT CAMERA
            cameraType = XRSkyboxFactory.CameraType.Left;

            cubeSide = XRSkyboxFactory.CubeSide.front;
            material_BlendedSixSidedCube_L.SetTexture( GetTextureSlotName( slot, cubeSide ), XRSkyboxFactory.instance.GetLoadedTexture( cameraType, cubeSide ) );

            cubeSide = XRSkyboxFactory.CubeSide.back;
            material_BlendedSixSidedCube_L.SetTexture( GetTextureSlotName( slot, cubeSide ), XRSkyboxFactory.instance.GetLoadedTexture( cameraType, cubeSide ) );

            cubeSide = XRSkyboxFactory.CubeSide.left;
            material_BlendedSixSidedCube_L.SetTexture( GetTextureSlotName( slot, cubeSide ), XRSkyboxFactory.instance.GetLoadedTexture( cameraType, cubeSide ) );

            cubeSide = XRSkyboxFactory.CubeSide.right;
            material_BlendedSixSidedCube_L.SetTexture( GetTextureSlotName( slot, cubeSide ), XRSkyboxFactory.instance.GetLoadedTexture( cameraType, cubeSide ) );

            cubeSide = XRSkyboxFactory.CubeSide.top;
            material_BlendedSixSidedCube_L.SetTexture( GetTextureSlotName( slot, cubeSide ), XRSkyboxFactory.instance.GetLoadedTexture( cameraType, cubeSide ) );

            cubeSide = XRSkyboxFactory.CubeSide.bottom;
            material_BlendedSixSidedCube_L.SetTexture( GetTextureSlotName( slot, cubeSide ), XRSkyboxFactory.instance.GetLoadedTexture( cameraType, cubeSide ) );

            //RIGHT CAMERA
            cameraType = XRSkyboxFactory.CameraType.Right;

            cubeSide = XRSkyboxFactory.CubeSide.front;
            material_BlendedSixSidedCube_R.SetTexture( GetTextureSlotName( slot, cubeSide ), XRSkyboxFactory.instance.GetLoadedTexture( cameraType, cubeSide ) );

            cubeSide = XRSkyboxFactory.CubeSide.back;
            material_BlendedSixSidedCube_R.SetTexture( GetTextureSlotName( slot, cubeSide ), XRSkyboxFactory.instance.GetLoadedTexture( cameraType, cubeSide ) );

            cubeSide = XRSkyboxFactory.CubeSide.left;
            material_BlendedSixSidedCube_R.SetTexture( GetTextureSlotName( slot, cubeSide ), XRSkyboxFactory.instance.GetLoadedTexture( cameraType, cubeSide ) );

            cubeSide = XRSkyboxFactory.CubeSide.right;
            material_BlendedSixSidedCube_R.SetTexture( GetTextureSlotName( slot, cubeSide ), XRSkyboxFactory.instance.GetLoadedTexture( cameraType, cubeSide ) );

            cubeSide = XRSkyboxFactory.CubeSide.top;
            material_BlendedSixSidedCube_R.SetTexture( GetTextureSlotName( slot, cubeSide ), XRSkyboxFactory.instance.GetLoadedTexture( cameraType, cubeSide ) );

            cubeSide = XRSkyboxFactory.CubeSide.bottom;
            material_BlendedSixSidedCube_R.SetTexture( GetTextureSlotName( slot, cubeSide ), XRSkyboxFactory.instance.GetLoadedTexture( cameraType, cubeSide ) );

        } //END FillImageSlot


        //--------------------------------------------//
        public string GetTextureSlotName( XRSkyboxFactory.ImageSlot slot, XRSkyboxFactory.CubeSide cubeSide )
        //--------------------------------------------//
        {

            string texName = "";

            if( slot == XRSkyboxFactory.ImageSlot.First )
            {
                if( cubeSide == XRSkyboxFactory.CubeSide.front ) { texName = "_FrontTex"; }
                else if( cubeSide == XRSkyboxFactory.CubeSide.back ) { texName = "_BackTex"; }
                else if( cubeSide == XRSkyboxFactory.CubeSide.left ) { texName = "_LeftTex"; }
                else if( cubeSide == XRSkyboxFactory.CubeSide.right ) { texName = "_RightTex"; }
                else if( cubeSide == XRSkyboxFactory.CubeSide.top ) { texName = "_UpTex"; }
                else if( cubeSide == XRSkyboxFactory.CubeSide.bottom ) { texName = "_DownTex"; }
            }
            else if( slot == XRSkyboxFactory.ImageSlot.Second )
            {
                if( cubeSide == XRSkyboxFactory.CubeSide.front ) { texName = "_FrontTex2"; }
                else if( cubeSide == XRSkyboxFactory.CubeSide.back ) { texName = "_BackTex2"; }
                else if( cubeSide == XRSkyboxFactory.CubeSide.left ) { texName = "_LeftTex2"; }
                else if( cubeSide == XRSkyboxFactory.CubeSide.right ) { texName = "_RightTex2"; }
                else if( cubeSide == XRSkyboxFactory.CubeSide.top ) { texName = "_UpTex2"; }
                else if( cubeSide == XRSkyboxFactory.CubeSide.bottom ) { texName = "_DownTex2"; }
            }

            return texName;

        } //END GetMaterialTextureName


        //--------------------------------------------//
        public XRSkyboxFactory.ImageType GetFirstImage() //Gets the first image in the current tag list
        //--------------------------------------------//
        {
            return XRSkyboxFactory.instance.GetImageTypeListForTag( XRSkyboxFactory.instance.currentImageTag )[ 0 ];

        } //END GetFirstImage

        //--------------------------------------------//
        public XRSkyboxFactory.ImageType GetNextImage() //Iterates through the list of possible images to display
         //--------------------------------------------//
        {
            //Grab a reference to the list of images we're iterating through
            List<XRSkyboxFactory.ImageType> listOfImages = XRSkyboxFactory.instance.GetImageTypeListForTag( XRSkyboxFactory.instance.currentImageTag );

            //Find out where the current image resides in the list we're iterating through
            int positionInList = XRSkyboxFactory.instance.GetImageTypePositionInTagList( XRSkyboxFactory.currentImageType, listOfImages );

            //Decide whether we should move to the next image or restart the list
            if( positionInList + 1 >= listOfImages.Count )
            {
                return listOfImages[ 0 ];
            }
            else
            {
                return listOfImages[ positionInList + 1 ];
            }

        } //END GetNextImage

        //---------------------//
        private IEnumerator PlayFadeInSFX( float delay )
        //---------------------//
        {

            yield return new WaitForSeconds( delay );
            AudioSource.PlayClipAtPoint( FadeInSFX, Camera.main.transform.position, 1f );


        } //END PlayFadeInSFX


        //-------------------------------------------//
        public void ChangeMaterial( XRSkyboxFactory.CameraType cameraType, ref Material material )
        //-------------------------------------------//
        {

            if( cameraType == XRSkyboxFactory.CameraType.Left )
            {
                skybox_MainCamera.material = material;
                skybox_LeftCamera.material = material;
            }
            else
            {
                skybox_RightCamera.material = material;
            }

        } //END ChangeMaterial

        //--------------------------------------------//
        public void ResetMaterialsToBlendedSkybox()
        //--------------------------------------------//
        {

            skybox_MainCamera.material = material_BlendedSixSidedCube_L;
            skybox_LeftCamera.material = material_BlendedSixSidedCube_L;
            skybox_RightCamera.material = material_BlendedSixSidedCube_R;

        } //END ResetMaterialsToBlendedSkybox


    } //END Class

} //END Namespace