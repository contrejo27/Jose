/* BlockVideo.cs
 * 
 * Plays video on a renderer from a local or web source, including YouTube
 */

using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Video;
using System;
using System.Collections;
using UnityEngine.UI;
using System.Collections.Generic;
using Sirenix.OdinInspector;

namespace BrandXR
{
    public class BlockVideo: Block
    {

        public override BlockType GetBlockType() { return BlockType.Video; }

        //----------------- Common Private Variables
        private bool isLoading = false;
        private bool isLoaded = false;
        private bool isPlaying = false;

        private bool isAudioFromVideoLoading = false;
        private bool isAudioFromVideoLoaded = false;

        #region GENERAL SETTINGS
        //------------------------------------------------------------------//
        //------------------ GENERAL SETTINGS ------------------------------//
        //------------------------------------------------------------------//
        [ShowIf( "IsPlayVideoOnPlaneOrRenderer" ), TitleGroup( "Block Video", "Plays local, web, or YouTube video in 2D, 3D, 360 formats" )]
        public int titleDummy = 0;

        #endregion

        #region PLAYER SETTINGS
        //------------------------------------------------------------------//
        //------------------ PLAYER SETTINGS -------------------------------//
        //------------------------------------------------------------------//

        //----------------- Video Player Type
        public enum VideoPlayerTypes
        {
            Unity
        }

        [FoldoutGroup( "Player Settings" )]
        public VideoPlayerTypes VideoPlayerType = VideoPlayerTypes.Unity;
        private bool IsVideoPlayerTypeUnity() { return VideoPlayerType == VideoPlayerTypes.Unity; }


        //----------------- Video Path
        public enum VideoLocation
        {
            VideoClip,
            Path
        }

        [ShowIf( "IsVideoPlayerTypeUnity" ), FoldoutGroup( "Player Settings" )]
        public VideoLocation videoLocation = VideoLocation.Path;
        private bool IsVideoLocationVideoClip() { return IsVideoPlayerTypeUnity() && videoLocation == VideoLocation.VideoClip; }
        private bool IsVideoLocationPath() { return IsVideoPlayerTypeUnity() && videoLocation == VideoLocation.Path; }

        [ShowIf( "IsVideoLocationVideoClip" ), FoldoutGroup( "Player Settings" )]
        public VideoClip VideoClip;

        [ShowIf( "IsVideoLocationPath" ), FoldoutGroup( "Player Settings" )]
        public string VideoPath;


        //------------------ Stereo 3D Settings
        public enum StereoType
        {
            Mono,
            StereoSideBySide,
            StereoTopBottom
        }

        private enum StereoPart //Used when setting up a material to be in either Side-By-Side or Top-Bottom stereo formats, each 'Part' has different settings applied to it's material
        {
            Part1,
            Part2
        }
        

        [Space(10), FoldoutGroup( "Player Settings" )]
        public StereoType stereo3DSettings = StereoType.Mono;
        public bool Is2D() { return stereo3DSettings == StereoType.Mono; }
        public bool Is3D() { return stereo3DSettings == StereoType.StereoSideBySide || stereo3DSettings == StereoType.StereoTopBottom; }
        public bool Is3DSideBySide() { return stereo3DSettings == StereoType.StereoSideBySide; }
        public bool Is3DTopBottom() { return stereo3DSettings == StereoType.StereoTopBottom; }

        public enum SetupProjectForStereo
        {
            DoNothing,
            SetupLayersForStereoButIgnoreCameraSetup,
            SetupLayersAndCamerasForStereo
        }

        [ShowIf("Is3D"), FoldoutGroup( "Player Settings" )]
        public SetupProjectForStereo setupProjectForStereo = SetupProjectForStereo.DoNothing;
        public bool IsSetupProjectFor3D_DoNothing() { return Is3D() && setupProjectForStereo == SetupProjectForStereo.DoNothing; }
        public bool IsSetupProjectFor3D_SetupOnlyLayers() { return Is3D() && setupProjectForStereo == SetupProjectForStereo.SetupLayersForStereoButIgnoreCameraSetup; }
        public bool IsSetupProjectFor3D_SetupLayersAndCameras() { return Is3D() && setupProjectForStereo == SetupProjectForStereo.SetupLayersAndCamerasForStereo; }
        public bool IsSetupProjectFor3D_SetupLayers() { return Is3D() && ( IsSetupProjectFor3D_SetupOnlyLayers() || IsSetupProjectFor3D_SetupLayersAndCameras() ); }

        [ShowIf( "IsSetupProjectFor3D_SetupLayers" ), FoldoutGroup( "Player Settings" )]
        public string LeftEyeLayerName = "LeftEye";

        [ShowIf( "IsSetupProjectFor3D_SetupLayers" ), FoldoutGroup( "Player Settings" )]
        public string RightEyeLayerName = "RightEye";




        //---------- "Play Video On This" Settings
        public enum PlayVideoOn
        {
            Renderer,
            Plane,
            Sphere,
            Fullscreen
        }
        
        [Space( 10 ), ShowIf( "IsVideoPlayerTypeUnity" ), FoldoutGroup( "Player Settings" )]
        public PlayVideoOn playVideoOn = PlayVideoOn.Plane;
        private bool IsPlayVideoOnRenderer() { return IsVideoPlayerTypeUnity() && playVideoOn == PlayVideoOn.Renderer; }
        private bool IsPlayVideoOnRendererAndIs3DSideBySideOrTopBottom() { return IsPlayVideoOnRenderer() && ( Is3DSideBySide() || Is3DTopBottom() ); }

        private bool IsPlayVideoOnPlane() { return IsVideoPlayerTypeUnity() && playVideoOn == PlayVideoOn.Plane; }
        private bool IsPlayVideoOnSphere() { return IsVideoPlayerTypeUnity() && playVideoOn == PlayVideoOn.Sphere; }
        private bool IsPlayVideoOnPlaneOrSphere() { return IsVideoPlayerTypeUnity() && ( playVideoOn == PlayVideoOn.Plane || playVideoOn == PlayVideoOn.Sphere ); }
        private bool IsPlayVideoOnPlaneOrRenderer() { return IsVideoPlayerTypeUnity() && ( playVideoOn == PlayVideoOn.Plane || playVideoOn == PlayVideoOn.Renderer ); }

        private bool IsPlayVideoOnFullscreen() { return IsVideoPlayerTypeUnity() && playVideoOn == PlayVideoOn.Fullscreen; }
        private bool IsPlayVideoOnPlaneOrFullscreen() { return IsVideoPlayerTypeUnity() && ( playVideoOn == PlayVideoOn.Plane || playVideoOn == PlayVideoOn.Fullscreen ); }

        [ShowIf( "IsPlayVideoOnSphere" ), FoldoutGroup( "Player Settings" )]
        public bool showSphereInEditor = false;

        [ShowIf( "IsPlayVideoOnRenderer" ), FoldoutGroup( "Player Settings" )]
        public Renderer videoRenderer1;
        [ShowIf( "IsPlayVideoOnRendererAndIs3DSideBySideOrTopBottom" ), FoldoutGroup( "Player Settings" )]
        public Renderer videoRenderer2;


        [ShowIf( "IsPlayVideoOnFullscreen" ), FoldoutGroup( "Player Settings" )]
        public bool showFullscreenVideoInEditor = false;

        [ShowIf( "IsPlayVideoOnFullscreen" ), FoldoutGroup( "Player Settings" )]
        public bool showSideBars = true;
        private bool ShowSideBarsOn() { return IsPlayVideoOnFullscreen() && showSideBars; }

        [ShowIf( "ShowSideBarsOn" ), FoldoutGroup( "Player Settings" )]
        public Color sideBarsColor = Color.black;

        

        [ShowIf( "IsPlayVideoOnRenderer" ), FoldoutGroup( "Player Settings" )]
        public bool overrideMaterialProperty = false;
        private bool IsPlayVideoOnRendererAndOverrideMaterialPropertyTrue() { return IsPlayVideoOnRenderer() && overrideMaterialProperty; }

        [ShowIf( "IsPlayVideoOnRendererAndOverrideMaterialPropertyTrue" ), FoldoutGroup( "Player Settings" )]
        public string setTextureToMaterialProperty = "_MainTex";



        //--------- Youtube Settings
        [Space( 10 ), ShowIf( "IsVideoLocationPath" ), FoldoutGroup( "Player Settings" )]
        public bool isYoutubeVideo = false;
        private bool IsYoutubeVideo() { return IsVideoLocationPath() && isYoutubeVideo; }

        public enum YoutubeQuality
        {
            Best,
            _144,
            _240,
            _360,
            _480,
            _640,
            _720,
            _1080,
            _1440,
            _2160,
            _4320
        }

        [ShowIf( "IsYoutubeVideo" ), FoldoutGroup( "Player Settings" )]
        public YoutubeQuality youtubePreferredQuality = YoutubeQuality.Best;

        #endregion

        #region PLAYBACK SETTINGS
        //------------------------------------------------------------------//
        //------------------ PLAYBACK SETTINGS -----------------------------//
        //------------------------------------------------------------------//
        [FoldoutGroup( "Playback Settings" )]
        public bool loadOnStart = true;


        public enum WhenFinished
        {
            DoNothing,
            Loop,
            HideVideo,
            CloseBlock
        }

        [Space(10), FoldoutGroup( "Playback Settings" )]
        public WhenFinished whenFinished = WhenFinished.DoNothing;
        private bool WhenFinishedDoNothing() { return whenFinished == WhenFinished.DoNothing; }
        private bool WhenFinishedLoop() { return whenFinished == WhenFinished.Loop; }
        private bool WhenFinishedHideVideo() { return whenFinished == WhenFinished.HideVideo; }
        private bool WhenFinishedCloseBlock() { return whenFinished == WhenFinished.CloseBlock; }

        [FoldoutGroup( "Playback Settings" )]
        public bool HideBlocksDuringPlayback = false;
        
        [Space( 10 ), ShowIf( "IsVideoPlayerTypeUnity" ), Range( 0f, 10f ), FoldoutGroup( "Playback Settings" )]
        public float playbackSpeed = 1f;
        
        public enum AspectRatio
        {
            Custom,
            _1x1,
            _4x3,
            _3x4,
            _5x4,
            _4x5,
            _16x9,
            _9x16,
            _16x10,
            _10x16,
            _18x9,
            _9x18,
            _21x9,
            _9x21,
            _1d85x1,
            _1x1d85,
            _2d39x1,
            _1x2d39
        }

        [Space( 10 ), ShowIf( "IsPlayVideoOnPlaneOrFullscreen" ), FoldoutGroup( "Playback Settings" )]
        public AspectRatio aspectRatio = AspectRatio._16x9;
        private bool IsAspectRatioCustom() { return IsPlayVideoOnPlaneOrFullscreen() && aspectRatio == AspectRatio.Custom; }

        [ShowIf( "IsAspectRatioCustom" ), FoldoutGroup( "Playback Settings" )]
        public Vector2 customAspectRatio = new Vector2( 16f, 9f );

        [ShowIf( "IsPlayVideoOnPlane" ), Range( 0f, 100f ), FoldoutGroup( "Playback Settings" )]
        public float scale = 1f;

        #endregion

        #region INTERACTION SETTINGS
        //------------------------------------------------------------------//
        //------------------ INTERACTION SETTINGS --------------------------//
        //------------------------------------------------------------------//

        public enum ShowAndHideSettings
        {
            AlwaysShow,
            AlwaysHide,
            BlockButtonEnterAndExit
        }

        [FoldoutGroup( "Interaction Settings" ), InfoBox( "'Show And Hide Video Settings' Defaults to 'Always Show' if we can't find a Block Button" )]
        public ShowAndHideSettings showAndHideSettings = ShowAndHideSettings.BlockButtonEnterAndExit;
        private bool ShowAndHideSetToAlwaysShow() { return showAndHideSettings == ShowAndHideSettings.AlwaysShow; }
        private bool ShowAndHideSetToAlwaysHide() { return showAndHideSettings == ShowAndHideSettings.AlwaysHide; }
        private bool ShowAndHideSetToBlockButton() { return showAndHideSettings == ShowAndHideSettings.BlockButtonEnterAndExit; }
        private bool ShowAndHideSetToAlwaysShowOrAlwaysHide() { return ShowAndHideSetToAlwaysShow() || ShowAndHideSetToAlwaysHide(); }

        [ShowIf( "ShowAndHideSetToAlwaysShowOrAlwaysHide" ), FoldoutGroup( "Interaction Settings" )]
        public bool playAfterLoadComplete = true;

        [ShowIf( "ShowAndHideSetToBlockButton" ), FoldoutGroup( "Interaction Settings" )]
        public bool playWhenBlockButtonSelected = true;
        

        private bool ShouldLookAwayFromVideoOptionsShow() { return IsVideoPlayerTypeUnity() && ( IsPlayVideoOnRenderer() || IsPlayVideoOnPlane() ); }
        
        public enum OnLookAwayAndBack
        {
            DoNothing,
            PauseAndResumeVideo,
            PauseAndResumeAndAlsoPlayHideAndShowAnimation,
            MuteAndUnmuteAudio,
            FadeOutAndFadeInAudio
        }

        [Space( 20 ), ShowIf( "ShouldLookAwayFromVideoOptionsShow" ), FoldoutGroup( "Interaction Settings" )]
        public OnLookAwayAndBack onLookAwayAndBack = OnLookAwayAndBack.PauseAndResumeVideo;
        private bool IsDoNothing() { return ShouldLookAwayFromVideoOptionsShow() && onLookAwayAndBack == OnLookAwayAndBack.DoNothing; }
        private bool IsPauseAndResume() { return ShouldLookAwayFromVideoOptionsShow() && onLookAwayAndBack == OnLookAwayAndBack.PauseAndResumeVideo; }
        private bool IsPlayHideAndShowAnimation() { return ShouldLookAwayFromVideoOptionsShow() && onLookAwayAndBack == OnLookAwayAndBack.PauseAndResumeAndAlsoPlayHideAndShowAnimation; }
        private bool IsMuteAndUnmute() { return ShouldLookAwayFromVideoOptionsShow() && onLookAwayAndBack == OnLookAwayAndBack.MuteAndUnmuteAudio; }
        private bool IsFadeOutAndFadeInAudio() { return ShouldLookAwayFromVideoOptionsShow() && onLookAwayAndBack == OnLookAwayAndBack.FadeOutAndFadeInAudio; }
        private bool IsDoSomethingOnLookAwayAndLookBack() { return ShouldLookAwayFromVideoOptionsShow() && !IsDoNothing(); }

        [ShowIf( "IsFadeOutAndFadeInAudio" ), FoldoutGroup( "Interaction Settings" )]
        public float fadeOutToVolume = 0f;

        [ShowIf( "IsFadeOutAndFadeInAudio" ), FoldoutGroup( "Interaction Settings" )]
        public float fadeInToVolume = 1f;

        [ShowIf( "IsFadeOutAndFadeInAudio" ), FoldoutGroup( "Interaction Settings" )]
        public float fadeOutSpeed = 1f;

        [ShowIf( "IsFadeOutAndFadeInAudio" ), FoldoutGroup( "Interaction Settings" )]
        public float fadeInSpeed = 1f;

        [ShowIf( "IsFadeOutAndFadeInAudio" ), FoldoutGroup( "Interaction Settings" )]
        public float fadeOutDelay = 0f;

        [ShowIf( "IsFadeOutAndFadeInAudio" ), FoldoutGroup( "Interaction Settings" )]
        public float fadeInDelay = 0f;
        
        [ShowIf( "IsDoSomethingOnLookAwayAndLookBack" ), FoldoutGroup( "Interaction Settings" )]
        public bool showColliderForLookAwayInEditor = false;
        private bool showColliderLookAwayColor() { return IsDoSomethingOnLookAwayAndLookBack() && showColliderForLookAwayInEditor; }

        [ShowIf( "showColliderLookAwayColor" ), FoldoutGroup( "Interaction Settings" )]
        public Color lookAwayColliderColor = new Color( Color.red.r, Color.red.g, Color.red.b, .1f );

        [ShowIf( "IsDoSomethingOnLookAwayAndLookBack" ), FoldoutGroup( "Interaction Settings" )]
        public AspectRatio lookAwayColliderRatio = AspectRatio._16x9;
        private bool IsLookAwayColliderAspectRatioCustom() { return IsDoSomethingOnLookAwayAndLookBack() && lookAwayColliderRatio == AspectRatio.Custom; }

        [ShowIf( "IsLookAwayColliderAspectRatioCustom" ), FoldoutGroup( "Interaction Settings" )]
        public Vector2 customLookAwayColliderAspectRatio = new Vector2( 16f, 9f );
        
        private bool ShouldScaleLookAwayColliderWithPlaneShow() { return IsPlayVideoOnPlane() && IsDoSomethingOnLookAwayAndLookBack(); }
        [ShowIf( "ShouldScaleLookAwayColliderWithPlaneShow" ), FoldoutGroup( "Interaction Settings" )]
        public bool scaleLookAwayColliderWithPlane = true;
        private bool ScaleLookAwayColliderWithPlane() { return ShouldScaleLookAwayColliderWithPlaneShow() && scaleLookAwayColliderWithPlane; }
        private bool ScaleLookAwayColliderIndependently() { return ShouldScaleLookAwayColliderWithPlaneShow() && !scaleLookAwayColliderWithPlane; }

        [Range( .1f, 100f ), ShowIf( "ScaleLookAwayColliderIndependently" ), FoldoutGroup( "Interaction Settings" )]
        public float lookAwayColliderScale = 1f;

        #endregion

        #region AUDIO SETTINGS
        //------------------------------------------------------------------//
        //------------------ AUDIO SETTINGS --------------------------------//
        //------------------------------------------------------------------//
        public enum AudioSourceSettings
        {
            BuiltIn,
            Custom
        }

        [ShowIf( "IsVideoPlayerTypeUnity" ), FoldoutGroup( "Audio Settings" )]
        public string audioSourceSettings = "Built-In";
        private bool IsBuiltInAudioSource() { return IsVideoPlayerTypeUnity() && audioSourceSettings == "Built-In"; }
        private bool IsCustomAudioSource() { return IsVideoPlayerTypeUnity() && audioSourceSettings == "Custom"; }

        [ShowIf( "IsCustomAudioSource" ), FoldoutGroup( "Audio Settings" )]
        public AudioSource customAudioSource;

        [Range( 0f, 1f ), FoldoutGroup( "Audio Settings" )]
        public float volume = 1f;
        private float originalVolume = 1f;

        public enum AudioListenerOptions
        {
            AlwaysAudible,
            AudibleOnlyWhenAudioListenerIsNear
        }
        
        [ShowIf( "IsPlayVideoOnPlaneOrRenderer" ), FoldoutGroup( "Audio Settings" )]
        public AudioListenerOptions audioListenerOptions = AudioListenerOptions.AlwaysAudible;
        private bool IsAlwaysAudible() { return IsPlayVideoOnPlaneOrRenderer() && audioListenerOptions == AudioListenerOptions.AlwaysAudible; }
        private bool IsAudibleWhenNear() { return IsPlayVideoOnPlaneOrRenderer() && audioListenerOptions == AudioListenerOptions.AudibleOnlyWhenAudioListenerIsNear; }

        #endregion

        #region ANIMATION SETTINGS
        //------------------------------------------------------------------//
        //------------------ ANIMATION SETTINGS ----------------------------//
        //------------------------------------------------------------------//
        public enum ShowAndHideAnimationSettings
        {
            AppearAndDisappear,
            FadeInAndOut,
            ScaleInAndOut
        }

        [FoldoutGroup( "Animation Settings" ), InfoBox( "'Scale In And Out' is not available if 'PlayVideoOn = Sphere' and will default to 'Fade In And Out' animation type" )]
        public ShowAndHideAnimationSettings showAndHideAnimationSettings = ShowAndHideAnimationSettings.FadeInAndOut;
        private bool ShowAndHideViaAppear() { return showAndHideAnimationSettings == ShowAndHideAnimationSettings.AppearAndDisappear; }
        private bool ShowAndHideViaFade() { return showAndHideAnimationSettings == ShowAndHideAnimationSettings.FadeInAndOut; }
        private bool ShowAndHideViaScale() { return showAndHideAnimationSettings == ShowAndHideAnimationSettings.ScaleInAndOut; }
        private bool ShowAndHideUsingTween() { return ShowAndHideViaFade() || ShowAndHideViaScale(); }
        
        [ShowIf( "ShowAndHideUsingTween" ), FoldoutGroup( "Animation Settings" )]
        public float showAndHideTweenSpeed = 1f;

        [ShowIf( "ShowAndHideUsingTween" ), FoldoutGroup( "Animation Settings" )]
        public float showAndHideTweenDelay = 0f;

        [ShowIf( "ShowAndHideUsingTween" ), FoldoutGroup( "Animation Settings" )]
        public EaseCurve.EaseType showAndHideTweenEaseType = EaseCurve.EaseType.ExpoEaseInOut;

        [ShowIf( "ShowAndHideViaFade" ), FoldoutGroup( "Animation Settings" )]
        public Color onShowTweenColor = Color.white;

        [ShowIf( "ShowAndHideViaFade" ), FoldoutGroup( "Animation Settings" )]
        public Color onHideTweenColor = new Color( Color.white.r, Color.white.g, Color.white.b, 0f );

        public enum SetShowScaleFrom
        {
            SetShowTweenScaleAutomatically,
            SetShowTweenScaleManually
        }

        [ShowIf( "ShowAndHideViaScale" ), FoldoutGroup( "Animation Settings" )]
        public SetShowScaleFrom setShowScaleFrom = SetShowScaleFrom.SetShowTweenScaleAutomatically;
        private bool IsShowScaleAuto() { return ShowAndHideViaScale() && setShowScaleFrom == SetShowScaleFrom.SetShowTweenScaleAutomatically; }
        private bool IsShowScaleManual() { return ShowAndHideViaScale() && setShowScaleFrom == SetShowScaleFrom.SetShowTweenScaleManually; }

        [ShowIf( "IsShowScaleManual" ), FoldoutGroup( "Animation Settings" )]
        public Vector3 onShowTweenScale = Vector3.one;

        [ShowIf( "ShowAndHideViaScale" ), FoldoutGroup( "Animation Settings" )]
        public Vector3 onHideTweenScale = Vector3.zero;

        #endregion

        #region LOADING SCREEN
        //------------------------------------------------------------------//
        //------------------ LOADING SCREEN --------------------------------//
        //------------------------------------------------------------------//
        public enum LoadingVisualOptions
        {
            DoNotChangeBasedOnLoading,
            ChangeImage,
            ShowLoadingAnimation,
            ChangeImageAndShowLoadingAnimation
        }

        [FoldoutGroup( "Loading Screen" )]
        public LoadingVisualOptions loadingVisualOptions = LoadingVisualOptions.ChangeImageAndShowLoadingAnimation;
        private bool Loading_DoNotChangeWhenLoading() { return loadingVisualOptions == LoadingVisualOptions.DoNotChangeBasedOnLoading; }
        private bool Loading_ChangeImageDuringLoading() { return loadingVisualOptions == LoadingVisualOptions.ChangeImage; }
        private bool Loading_ShowLoadingAnimation() { return loadingVisualOptions == LoadingVisualOptions.ShowLoadingAnimation; }
        private bool Loading_ChangeImageAndShowLoadingAnimation() { return loadingVisualOptions == LoadingVisualOptions.ChangeImageAndShowLoadingAnimation; }

        private bool Loading_ChangeImageOrChangeImageAndShowLoadingAnimation() { return Loading_ChangeImageDuringLoading() || Loading_ChangeImageAndShowLoadingAnimation(); }
        private bool Loading_Animate() { return Loading_ShowLoadingAnimation() || Loading_ChangeImageAndShowLoadingAnimation(); }

        private bool Loading_DoAnythingWhileLoading() { return loadingVisualOptions != LoadingVisualOptions.DoNotChangeBasedOnLoading; }


        public enum ImageSource
        {
            Texture,
            Path
        }

        [ShowIf( "Loading_ChangeImageOrChangeImageAndShowLoadingAnimation" ), FoldoutGroup("Loading Screen")]
        public ImageSource changeImageUsing = ImageSource.Texture;
        private bool ChangeLoadingImageUsingTexture() { return Loading_ChangeImageOrChangeImageAndShowLoadingAnimation() && changeImageUsing == ImageSource.Texture; }
        private bool ChangeLoadingImageUsingPath() { return Loading_ChangeImageOrChangeImageAndShowLoadingAnimation() && changeImageUsing == ImageSource.Path; }

        [ShowIf( "ChangeLoadingImageUsingTexture" ), FoldoutGroup( "Loading Screen" )]
        public Texture loadingTexture;

        [ShowIf( "ChangeLoadingImageUsingPath" ), FoldoutGroup( "Loading Screen" )]
        public string loadingPath;

        [ShowIf( "ChangeLoadingImageUsingPath" ), FoldoutGroup( "Loading Screen" )]
        public bool cacheLoadingImageIfWeb = true;

        private bool loadingImageIsReady = false;

        [ShowIf( "Loading_ChangeImageOrChangeImageAndShowLoadingAnimation" ), FoldoutGroup( "Loading Screen" )]
        public bool debugShowLoadingImageInEditor = true;


        //Loading Animation Settings
        [Space( 10 ), ShowIf( "Loading_Animate" ), FoldoutGroup( "Loading Screen" )]
        public bool debugShowAnimationInEditor = true;

        [ShowIf( "Loading_Animate" ), FoldoutGroup( "Loading Screen" )]
        public ImageSource AnimationSpriteSheetFrom = ImageSource.Texture;
        private bool AnimationFromTexture() { return Loading_Animate() && AnimationSpriteSheetFrom == ImageSource.Texture; }
        private bool AnimationFromPath() { return Loading_Animate() && AnimationSpriteSheetFrom == ImageSource.Path; }

        [ShowIf( "AnimationFromTexture" ), FoldoutGroup( "Loading Screen" )]
        public Texture animationTexture;
        
        [ShowIf( "AnimationFromPath" ), FoldoutGroup( "Loading Screen" )]
        public string animationPath;

        private bool loadingAnimationIsReady = false;

        [ShowIf( "AnimationFromPath" ), FoldoutGroup( "Loading Screen" )]
        public bool cacheLoadingAnimationIfWeb = true;

        private bool ShowAnimationScaler() { return Loading_Animate() && IsPlayVideoOnPlaneOrRenderer(); }
        [ShowIf( "ShowAnimationScaler" ), Range( .001f, 1f ), FoldoutGroup( "Loading Screen" )]
        public float animationScale = .05f;

        private bool ShowAnimationSphereScaler() { return Loading_Animate() && IsPlayVideoOnSphere(); }
        [ShowIf( "ShowAnimationSphereScaler" ), Range( .1f, 10f ), FoldoutGroup( "Loading Screen" )]
        public float animationSphereScale = 1f;

        private bool ShowFullscreenScaler() { return Loading_Animate() && IsPlayVideoOnFullscreen(); }
        [ShowIf( "ShowFullscreenScaler" ), Range( .1f, 10f ), FoldoutGroup( "Loading Screen" )]
        public float animationFullscreenScale = 1f;

        private bool IsSphereOrCubeAndShowingLoadingAnimation() { return IsPlayVideoOnSphere() && Loading_Animate(); }
        
        [Space( 5 ), ShowIf( "IsSphereOrCubeAndShowingLoadingAnimation" ), FoldoutGroup( "Loading Screen" )]
        public bool showOnFrontFace = true;
        private bool ShowLoadingAnimationOnFrontFace() { return IsSphereOrCubeAndShowingLoadingAnimation() && showOnFrontFace; }

        [ShowIf( "IsSphereOrCubeAndShowingLoadingAnimation" ), FoldoutGroup( "Loading Screen" )]
        public bool showOnBackFace = true;
        private bool ShowLoadingAnimationOnBackFace() { return IsSphereOrCubeAndShowingLoadingAnimation() && showOnBackFace; }

        [ShowIf( "IsSphereOrCubeAndShowingLoadingAnimation" ), FoldoutGroup( "Loading Screen" )]
        public bool showOnLeftFace = true;
        private bool ShowLoadingAnimationOnLeftFace() { return IsSphereOrCubeAndShowingLoadingAnimation() && showOnLeftFace; }

        [ShowIf( "IsSphereOrCubeAndShowingLoadingAnimation" ), FoldoutGroup( "Loading Screen" )]
        public bool showOnRightFace = true;
        private bool ShowLoadingAnimationOnRightFace() { return IsSphereOrCubeAndShowingLoadingAnimation() && showOnRightFace; }
        
        [ShowIf( "IsSphereOrCubeAndShowingLoadingAnimation" ), FoldoutGroup( "Loading Screen" )]
        public bool showOnTopFace = true;
        private bool ShowLoadingAnimationOnTopFace() { return IsSphereOrCubeAndShowingLoadingAnimation() && showOnTopFace; }

        [ShowIf( "IsSphereOrCubeAndShowingLoadingAnimation" ), FoldoutGroup( "Loading Screen" )]
        public bool showOnBottomFace = true;
        private bool ShowLoadingAnimationOnBottomFace() { return IsSphereOrCubeAndShowingLoadingAnimation() && showOnBottomFace; }

        private int loadingAnimationIndex = 0;
        
        [Space( 5 ), ShowIf( "Loading_Animate" ), FoldoutGroup( "Loading Screen" )]
        public int columns = 1;

        [ShowIf( "Loading_Animate" ), FoldoutGroup( "Loading Screen" )]
        public int rows = 1;

        [ShowIf( "Loading_Animate" ), FoldoutGroup( "Loading Screen" )]
        public float framesPerSecond = 10f;

        public enum RotateOptions
        {
            DoNotRotate,
            RotateLeft,
            RotateRight
        }

        [ShowIf( "Loading_Animate" ), FoldoutGroup( "Loading Screen" )]
        public RotateOptions rotateOptions = RotateOptions.DoNotRotate;
        private bool LoadingAnimation_DoNotRotate() { return Loading_Animate() && rotateOptions == RotateOptions.DoNotRotate; }
        private bool LoadingAnimation_RotateLeft() { return Loading_Animate() && rotateOptions == RotateOptions.RotateLeft; }
        private bool LoadingAnimation_RotateRight() { return Loading_Animate() && rotateOptions == RotateOptions.RotateRight; }
        private bool LoadingAnimation_Rotate() { return Loading_Animate() && rotateOptions != RotateOptions.DoNotRotate; }

        [ShowIf( "LoadingAnimation_Rotate" ), Range( .1f, 500f ), FoldoutGroup( "Loading Screen" )]
        public float rotationSpeed = 1f;

        Coroutine coroutine_LoadAnimation;
        Coroutine coroutine_RotateLoadAnimation;


        public enum LoadingAudioOptions
        {
            DoNotPlayWhileLoading,
            PlayAudioWhileLoading
        }

        [Space(15), FoldoutGroup( "Loading Screen" )]
        public LoadingAudioOptions loadingAudioOptions = LoadingAudioOptions.PlayAudioWhileLoading;
        private bool LoadingAudio_DoNotPlay() { return loadingAudioOptions == LoadingAudioOptions.DoNotPlayWhileLoading; }
        private bool LoadingAudio_Play() { return loadingAudioOptions == LoadingAudioOptions.PlayAudioWhileLoading; }

        public enum AudioLocation
        {
            AudioClip,
            Path
        }

        [ShowIf( "LoadingAudio_Play" ), FoldoutGroup( "Loading Screen" )]
        public AudioLocation loadingAudioLocation = AudioLocation.AudioClip;
        private bool IsLoadingAudioClip() { return LoadingAudio_Play() && loadingAudioLocation == AudioLocation.AudioClip; }
        private bool IsLoadingAudioPath() { return LoadingAudio_Play() && loadingAudioLocation == AudioLocation.Path; }

        private bool IsLoadingAudioSourceReady = false;

        [ShowIf( "IsLoadingAudioClip" ), FoldoutGroup( "Loading Screen" )]
        public AudioClip loadingAudioClip;

        [ShowIf( "IsLoadingAudioPath" ), FoldoutGroup( "Loading Screen" )]
        public string loadingAudioPath;

        [ShowIf( "IsLoadingAudioPath" ), FoldoutGroup( "Loading Screen" )]
        public bool cacheLoadingAudioIfWeb = false;

        [ShowIf( "LoadingAudio_Play" ), Range( 0f, 1f ), FoldoutGroup( "Loading Screen" )]
        public float loadingAudioVolume = 1f;

        [ShowIf( "LoadingAudio_Play" ), FoldoutGroup( "Loading Screen" )]
        public bool loadAudioLoop = true;
        
        public enum LoadAudioListenerOptions
        {
            AlwaysAudible,
            AudibleOnlyWhenAudioListenerIsNear
        }

        [ShowIf( "LoadingAudio_Play" ), FoldoutGroup( "Loading Screen" )]
        public LoadAudioListenerOptions loadAudioListenerOptions = LoadAudioListenerOptions.AlwaysAudible;
        private bool IsLoadAudioAlwaysAudible() { return LoadingAudio_Play() && loadAudioListenerOptions == LoadAudioListenerOptions.AlwaysAudible; }
        private bool IsLoadAudioAudibleWhenNear() { return LoadingAudio_Play() && loadAudioListenerOptions == LoadAudioListenerOptions.AudibleOnlyWhenAudioListenerIsNear; }
        
        public enum StopLoadingAudioOnComplete
        {
            StopAudio,
            FadeOut
        }

        [ShowIf( "LoadingAudio_Play" ), FoldoutGroup( "Loading Screen" )]
        public StopLoadingAudioOnComplete stopLoadingAudioOnComplete = StopLoadingAudioOnComplete.StopAudio;
        private bool OnLoadCompleteStopAudio() { return LoadingAudio_Play() && stopLoadingAudioOnComplete == StopLoadingAudioOnComplete.StopAudio; }
        private bool OnLoadCompleteFadeAudio() { return LoadingAudio_Play() && stopLoadingAudioOnComplete == StopLoadingAudioOnComplete.FadeOut; }

        [ShowIf( "OnLoadCompleteFadeAudio" ), FoldoutGroup( "Loading Screen" )]
        public float fadeLength = 1f;


        public enum AudioOnLoadComplete
        {
            DoNotPlayAudioOnLoadComplete,
            PlayAudioOnLoadComplete
        }

        [Space( 10 ), FoldoutGroup( "Loading Screen" )]
        public AudioOnLoadComplete audioOnLoadComplete = AudioOnLoadComplete.DoNotPlayAudioOnLoadComplete;
        private bool DoNotPlayAudioOnLoadComplete() { return audioOnLoadComplete == AudioOnLoadComplete.DoNotPlayAudioOnLoadComplete; }
        private bool PlayAudioOnLoadComplete() { return audioOnLoadComplete == AudioOnLoadComplete.PlayAudioOnLoadComplete; }

        

        [ShowIf( "PlayAudioOnLoadComplete" ), FoldoutGroup( "Loading Screen" )]
        public AudioLocation LoadCompleteAudioSource = AudioLocation.AudioClip;
        private bool IsLoadCompleteAudioClip() { return PlayAudioOnLoadComplete() && LoadCompleteAudioSource == AudioLocation.AudioClip; }
        private bool IsLoadCompleteAudioPath() { return PlayAudioOnLoadComplete() && LoadCompleteAudioSource == AudioLocation.Path; }

        private bool IsLoadCompleteAudioSourceReady = false;

        [ShowIf( "IsLoadCompleteAudioClip" ), FoldoutGroup( "Loading Screen" )]
        public AudioClip loadCompleteAudioClip;

        [ShowIf( "IsLoadCompleteAudioPath" ), FoldoutGroup( "Loading Screen" )]
        public string loadCompleteAudioPath;

        [ShowIf( "IsLoadCompleteAudioPath" ), FoldoutGroup( "Loading Screen" )]
        public bool cacheLoadCompleteAudioIfWeb = false;

        [ShowIf( "PlayAudioOnLoadComplete" ), Range( 0f, 1f ), FoldoutGroup( "Loading Screen" )]
        public float loadCompleteAudioVolume = 1f;
        
        [ShowIf( "PlayAudioOnLoadComplete" ), FoldoutGroup( "Loading Screen" )]
        public AudioListenerOptions loadCompleteAudioListenerOptions = AudioListenerOptions.AlwaysAudible;
        private bool IsLoadCompleteAudioAlwaysAudible() { return PlayAudioOnLoadComplete() && loadCompleteAudioListenerOptions == AudioListenerOptions.AlwaysAudible; }
        private bool IsLoadCompleteAudioAudibleWhenNear() { return PlayAudioOnLoadComplete() && loadCompleteAudioListenerOptions == AudioListenerOptions.AudibleOnlyWhenAudioListenerIsNear; }

        #endregion

        #region ERROR HANDLING
        //------------------------------------------------------------------//
        //------------------ ERROR HANDLING --------------------------------//
        //------------------------------------------------------------------//

        public enum OnErrorOptions
        {
            ChangeNothing,
            TryAgain,
            ChangeImage
        }
        
        [FoldoutGroup( "Error Handling" )]
        public OnErrorOptions OnError = OnErrorOptions.ChangeNothing;
        private bool OnError_ChangeNothing() { return OnError == OnErrorOptions.ChangeNothing; }
        private bool OnError_TryAgain() { return OnError == OnErrorOptions.TryAgain; }
        private bool OnError_ChangeImage() { return OnError == OnErrorOptions.ChangeImage; }

        [ShowIf( "OnError_TryAgain" ), Range( 3, 99 ), FoldoutGroup( "Error Handling" )]
        public int retryLimit = 3;
        private int retryCounter = 1;
        private bool hasFatalErrorLogicBeenCalled = false;

        public enum OnRetryLimitReached
        {
            DoNothing,
            ChangeImage
        }

        [ShowIf( "OnError_TryAgain" ), FoldoutGroup( "Error Handling" )]
        public OnRetryLimitReached onRetryLimitReached = OnRetryLimitReached.DoNothing;
        private bool OnRetryLimitReached_DoNothing() { return OnError_TryAgain() && onRetryLimitReached == OnRetryLimitReached.DoNothing; }
        private bool OnRetryLimitReached_ChangeImage() { return OnError_TryAgain() && onRetryLimitReached == OnRetryLimitReached.ChangeImage; }

        private bool OnError_UnderAnyScenarioChangeImage() { return OnError_ChangeImage() || OnRetryLimitReached_ChangeImage(); }
        
        [Space( 10 ), ShowIf( "OnError_UnderAnyScenarioChangeImage" ), FoldoutGroup( "Error Handling" )]
        public ImageSource ChangeErrorImageUsing = ImageSource.Texture;
        private bool ChangeErrorImageUsingTexture() { return OnError_UnderAnyScenarioChangeImage() && ChangeErrorImageUsing == ImageSource.Texture; }
        private bool ChangeErrorImageUsingPath() { return OnError_UnderAnyScenarioChangeImage() && ChangeErrorImageUsing == ImageSource.Path; }

        [ShowIf( "ChangeErrorImageUsingTexture" ), FoldoutGroup( "Error Handling" )]
        public Texture errorTexture;

        [ShowIf( "ChangeErrorImageUsingPath" ), FoldoutGroup( "Error Handling" )]
        public string errorImagePath;

        [ShowIf( "ChangeErrorImageUsingPath" ), FoldoutGroup( "Error Handling" )]
        public bool cacheErrorImageIfWeb = true;

        private bool errorImageIsReady = false;

        [ShowIf( "ChangeErrorImageUsingTexture" ), FoldoutGroup( "Error Handling" )]
        public bool debugShowErrorImageInEditor = true;

        public enum OnErrorPlayAudio
        {
            DoNotPlayAudioOnError,
            PlayAudioOnError
        }

        [Space (10), FoldoutGroup( "Error Handling" )]
        public OnErrorPlayAudio onErrorPlayAudio = OnErrorPlayAudio.DoNotPlayAudioOnError;
        private bool OnErrorPlayAudio_DoNotPlay() { return onErrorPlayAudio == OnErrorPlayAudio.DoNotPlayAudioOnError; }
        private bool OnErrorPlayAudio_PlayAudio() { return onErrorPlayAudio == OnErrorPlayAudio.PlayAudioOnError; }

        [ShowIf( "OnErrorPlayAudio_PlayAudio" ), FoldoutGroup( "Error Handling" )]
        public AudioLocation errorAudioType = AudioLocation.AudioClip;
        private bool IsErrorAudioClip() { return OnErrorPlayAudio_PlayAudio() && errorAudioType == AudioLocation.AudioClip; }
        private bool IsErrorAudioPath() { return OnErrorPlayAudio_PlayAudio() && errorAudioType == AudioLocation.Path; }

        private bool IsErrorAudioSourceReady = false;

        [ShowIf( "IsErrorAudioClip" ), FoldoutGroup( "Error Handling" )]
        public AudioClip errorAudioClip;
        
        [ShowIf( "IsErrorAudioPath" ), FoldoutGroup( "Error Handling" )]
        public string errorAudioPath;

        [ShowIf( "IsErrorAudioPath" ), FoldoutGroup( "Error Handling" )]
        public bool cacheErrorAudioIfWeb = false;

        [ShowIf( "OnErrorPlayAudio_PlayAudio" ), Range( 0f, 1f ), FoldoutGroup( "Error Handling" )]
        public float errorAudioVolume = 1f;


        [ShowIf( "OnErrorPlayAudio_PlayAudio" ), FoldoutGroup( "Error Handling" )]
        public AudioListenerOptions errorAudioListenerOptions = AudioListenerOptions.AlwaysAudible;
        private bool IsErrorAudioAlwaysAudible() { return OnErrorPlayAudio_PlayAudio() && errorAudioListenerOptions == AudioListenerOptions.AlwaysAudible; }
        private bool IsErrorAudioAudibleWhenNear() { return OnErrorPlayAudio_PlayAudio() && errorAudioListenerOptions == AudioListenerOptions.AudibleOnlyWhenAudioListenerIsNear; }

        #endregion

        #region EVENTS
        //------------------------------------------------------------------//
        //------------------ EVENTS ----------------------------------------//
        //------------------------------------------------------------------//

        [ShowIf( "IsVideoPlayerTypeUnity" ), FoldoutGroup( "Events" )]
        public UnityEvent onPrepareCompleted = new UnityEvent();

        [ShowIf( "IsVideoPlayerTypeUnity" ), FoldoutGroup( "Events" )]
        public UnityEvent onStarted = new UnityEvent();

        [ShowIf( "IsVideoPlayerTypeUnity" ), FoldoutGroup( "Events" )]
        public UnityEvent onLoopPointReached = new UnityEvent();

        [ShowIf( "IsVideoPlayerTypeUnity" ), FoldoutGroup( "Events" )]
        public UnityEvent onEndPointReached = new UnityEvent();

        [ShowIf( "IsVideoPlayerTypeUnity" ), FoldoutGroup( "Events" )]
        public UnityEvent onSeekCompleted = new UnityEvent();

        [ShowIf( "IsVideoPlayerTypeUnity" ), FoldoutGroup( "Events" )]
        public UnityEvent onRetryDueToError = new UnityEvent();

        [ShowIf( "IsVideoPlayerTypeUnity" ), FoldoutGroup( "Events" )]
        public UnityEvent onFatalPlaybackError = new UnityEvent();

        [ShowIf( "IsVideoPlayerTypeUnity" ), FoldoutGroup( "Events" )]
        public UnityEvent onFrameDropped = new UnityEvent();

        #endregion

        #region HOOKS
        //------------------------------------------------------------------//
        //----------------------------- HOOKS ------------------------------//
        //------------------------------------------------------------------//

        [FoldoutGroup( "Hooks" )]
        public Transform lookAwayColliderParent;

        [FoldoutGroup( "Hooks" )]
        public Collider colliderForLookAwayAndLookBack;

        [FoldoutGroup( "Hooks" )]
        public UIScaleTweenManager uiScaleTweenManager_Video;

        [FoldoutGroup( "Hooks" )]
        public GameObject worldSpaceParent;
        [FoldoutGroup( "Hooks" )]
        public GameObject screenSpaceParent;

        [FoldoutGroup( "Hooks" )]
        public VideoPlayer videoPlayer;
        [FoldoutGroup( "Hooks" )]
        public VideoPlayer audioFromVideoPlayer;

        [FoldoutGroup( "Hooks" )]
        public Transform videoPlaneParent;
        [FoldoutGroup( "Hooks" )]
        public Renderer videoPlane1;
        [FoldoutGroup( "Hooks" )]
        public Renderer videoPlane2;

        [FoldoutGroup( "Hooks" )]
        public Transform videoSphereParent;
        [FoldoutGroup( "Hooks" )]
        public Renderer videoSphere1;
        [FoldoutGroup( "Hooks" )]
        public Renderer videoSphere2;

        [FoldoutGroup( "Hooks" )]
        public Transform fullscreenParent;
        [FoldoutGroup( "Hooks" )]
        public RawImage videoFullscreen1;
        [FoldoutGroup( "Hooks" )]
        public RawImage videoFullscreen2;
        [FoldoutGroup( "Hooks" )]
        public RawImage fullscreenSidebars;

        [FoldoutGroup( "Hooks" )]
        public AspectRatioFitter aspectRatioFitter_Fullscreen;
        [FoldoutGroup( "Hooks" )]
        public AspectRatioFitter aspectRatioFitter_Sidebars;
        
        private bool initialScreenSizeCaptured = false;
        private int screenSizeX = 0;
        private int screenSizeY = 0;

        private bool initialCameraAspectRatioCaptured = false;
        private float cameraAspectRatio;

        [FoldoutGroup( "Hooks" )]
        public UIColorTweenManager uiColorTweenManager_Plane;
        [FoldoutGroup( "Hooks" )]
        public UIColorTweenManager uiColorTweenManager_Sphere;
        [FoldoutGroup( "Hooks" )]
        public UIColorTweenManager uiColorTweenManager_LookaAwayCollider;
        [FoldoutGroup( "Hooks" )]
        public UIColorTweenManager uiColorTweenManager_Fullscreen;
        [FoldoutGroup( "Hooks" )]
        public UIColorTweenManager uiColorTweenManager_FullscreenSidebars;

        [FoldoutGroup( "Hooks" )]
        public Transform loadingAnimationParent;
        [FoldoutGroup( "Hooks" )]
        public RawImage rawImage_LoadingAnimation_Plane;

        [FoldoutGroup( "Hooks" )]
        public Transform loadingAnimation_SphereParent;

        [FoldoutGroup( "Hooks" )]
        public RawImage rawImage_LoadingAnimation_Sphere_Front;
        [FoldoutGroup( "Hooks" )]
        public RawImage rawImage_LoadingAnimation_Sphere_Back;
        [FoldoutGroup( "Hooks" )]
        public RawImage rawImage_LoadingAnimation_Sphere_Left;
        [FoldoutGroup( "Hooks" )]
        public RawImage rawImage_LoadingAnimation_Sphere_Right;
        [FoldoutGroup( "Hooks" )]
        public RawImage rawImage_LoadingAnimation_Sphere_Top;
        [FoldoutGroup( "Hooks" )]
        public RawImage rawImage_LoadingAnimation_Sphere_Bottom;

        [FoldoutGroup( "Hooks" )]
        public RawImage rawImage_LoadingAnimation_Fullscreen;

        [FoldoutGroup( "Hooks" )]
        public Material loadingAnimationMaterial;

        private Vector3 aspectRatio_1x1 = new Vector3( 1f, 1f, 1f );

        private Vector3 aspectRatio_4x3 = new Vector3( 4f, 3f, 1f );
        private Vector3 aspectRatio_3x4 = new Vector3( 3f, 4f, 1f );

        private Vector3 aspectRatio_5x4 = new Vector3( 5f, 4f, 1f );
        private Vector3 aspectRatio_4x5 = new Vector3( 4f, 5f, 1f );

        private Vector3 aspectRatio_16x9 = new Vector3( 16f, 9f, 1f );
        private Vector3 aspectRatio_9x16 = new Vector3( 9f, 16f, 1f );

        private Vector3 aspectRatio_16x10 = new Vector3( 16f, 10f, 1f );
        private Vector3 aspectRatio_10x16 = new Vector3( 10f, 16f, 1f );

        private Vector3 aspectRatio_18x9 = new Vector3( 18f, 9f, 1f );
        private Vector3 aspectRatio_9x18 = new Vector3( 9f, 18f, 1f );

        private Vector3 aspectRatio_21x9 = new Vector3( 21f, 9f, 1f );
        private Vector3 aspectRatio_9x21 = new Vector3( 9f, 21f, 1f );

        private Vector3 aspectRatio_185x1 = new Vector3( 10.85f, 10f, 1f );
        private Vector3 aspectRatio_1x185 = new Vector3( 10f, 10.85f, 1f );

        private Vector3 aspectRatio_239x1 = new Vector3( 20.39f, 10f, 1f );
        private Vector3 aspectRatio_1x239 = new Vector3( 10f, 20.39f, 1f );

        private float aspectRatio_Fullscreen_1x1 = 1f;

        private float aspectRatio_Fullscreen_4x3 = 1.335f;
        private float aspectRatio_Fullscreen_3x4 = 0.75f;

        private float aspectRatio_Fullscreen_5x4 = 1.25f;
        private float aspectRatio_Fullscreen_4x5 = 0.80f;
        
        private float aspectRatio_Fullscreen_16x9 = 1.78f;
        private float aspectRatio_Fullscreen_9x16 = 0.56f;

        private float aspectRatio_Fullscreen_16x10 = 1.60f;
        private float aspectRatio_Fullscreen_10x16 = 0.625f;

        private float aspectRatio_Fullscreen_18x9 = 2.00f;
        private float aspectRatio_Fullscreen_9x18 = 0.50f;

        private float aspectRatio_Fullscreen_21x9 = 2.35f;
        private float aspectRatio_Fullscreen_9x21 = 0.427f;

        private float aspectRatio_Fullscreen_185x1 = 1.33f;
        private float aspectRatio_Fullscreen_1x185 = 0.54f;

        private float aspectRatio_Fullscreen_239x1 = 2.385f;
        private float aspectRatio_Fullscreen_1x239 = 0.41f;

        [FoldoutGroup( "Hooks" )]
        public AudioSource audioSource;
        [FoldoutGroup( "Hooks" )]
        public AudioSource loadingAudioSource;
        [FoldoutGroup( "Hooks" )]
        public AudioSource loadCompleteAudioSource;
        [FoldoutGroup( "Hooks" )]
        public AudioSource errorAudioSource;

        [FoldoutGroup( "Hooks" )]
        public YoutubeFinder youtubeFinder;
        private bool useAudioFromVideo = false;

        private float audioPeriod = 0f;
        private float audioTimeInterval = .060f;
        private bool audioFromVideoIsPaused = false;

        private Coroutine coroutine_audioSync;

        private enum OnErrorTryPath
        {
            OriginalPath,
            StreamingAssets,
            Resources,
            PersistentDataPath,
            FinishedAllChecks
        }
        private OnErrorTryPath currentLoadingAttempt = OnErrorTryPath.OriginalPath;

        private enum OnErrorTryFileType
        {
            OriginalFileType,
            mp4,
            m4v,
            webm,
            ogg,
            avi,
            FinishedAllChecks
        }
        private OnErrorTryFileType currentFileTypeAttempt = OnErrorTryFileType.OriginalFileType;

        #endregion


        //--------------------------------//
        public void OnEnable()
        //--------------------------------//
        {

            AddDelegates();

        } //END OnEnable

        //--------------------------------//
        public void OnDisable()
        //--------------------------------//
        {

            RemoveDelegates();

        } //END OnDisable



        //--------------------------------//
        public void OnValidate()
        //--------------------------------//
        {

            SetupLayersAndCamerasForStereo3D();

            SetVideoPlayerBasedOnSettings();

            OnValidateForceRendererVisibilityBasedOnSettings();

            SetLoadingVisuals();

            SetLoadingImageBasedOnTexture();

            SetLoadingAnimationBasedOnTexture();

            SetLoadingAnimationScale();

            SetLoadingAnimationTextureScale();

            SetLoadingAnimationTextureInitialTiling();

            SetPlaneOrFullscreenTransformAspectRatio();

            SetFullscreenSidebarAspectRatio();

            SetFullscreenSidebarColors();

            SetLookAwayColliderAspectRatio();
            
            SetPlaneScale();
            
            SetLookAwayColliderScale();
            
            ForceColliderVisibilityBasedOnSettings();
            
            SetAudioSourceSettings();
            
            SetShowAndHideAnimationSettings();
            
            SetStereo3DSettings();

            SetShowTweenScale();

            ShowErrorImageForEditorDebug();

        } //END OnValidate

        //--------------------------------//
        public override void Start()
        //--------------------------------//
        {

            base.Start();

            CaptureInitialScreenSize();

            CaptureInitialCameraAspectRatio();

            SetupLayersAndCamerasForStereo3D();

            SetOriginalVolume();

            ShowOrHideBasedOnSettings();

            SetVideoPlayerBasedOnSettings();

            SetStereo3DSettings();
            
            SetLoadingVisuals();

            SetLoadingImageBasedOnTexture();

            SetLoadingAnimationBasedOnTexture();

            SetLoadingAnimationScale();
            
            ForceSphereToCameraPosition();

            SetPlaneOrFullscreenTransformAspectRatio();

            SetFullscreenSidebarAspectRatio();

            SetLookAwayColliderAspectRatio();

            SetPlaneScale();
            
            SetLookAwayColliderScale();

            ForceColliderVisibilityBasedOnSettings();

            SetAudioSourceSettings();

            SetupLoadingAudio();

            SetupLoadCompleteAudio();

            SetupErrorImage();

            SetupErrorAudio();

            SetShowAndHideAnimationSettings();
            
            LoadVideoAtStart();

        } //END Start

        //--------------------------------//
        private void CaptureInitialScreenSize()
        //--------------------------------//
        {

            if( !initialScreenSizeCaptured )
            {
                initialScreenSizeCaptured = true;
                screenSizeX = Screen.width;
                screenSizeY = Screen.height;
            }

        } //END CaptureInitialScreenSize

        //--------------------------------//
        private void CaptureInitialCameraAspectRatio()
        //--------------------------------//
        {

            if( !initialCameraAspectRatioCaptured && Camera.main != null )
            {
                initialCameraAspectRatioCaptured = true;
                cameraAspectRatio = Camera.main.aspect;
            }

        } //END CaptureInitialCameraAspectRatio

        //--------------------------------//
        public override void Update()
        //--------------------------------//
        {
            base.Update();

            //Make sure the VideoSphere's are at the correct position
            if( IsPlayVideoOnSphere() )
            {
                if( IsVideoPlayerTypeUnity() && IsPlaying() )
                {
                    ForceSphereToCameraPosition();
                }
            }

            //Make sure that the fullscreen video is actually covering the whole screen
            if( isPlaying && initialScreenSizeCaptured && IsPlayVideoOnFullscreen() )
            {
                if( Screen.width != screenSizeX || Screen.height != screenSizeY )
                {
                    //Debug.Log( "Screen Size Changed... original( " + screenSizeX + "x" + screenSizeY + " ), new( " + Screen.width + "x" + Screen.height + " )" );
                    screenSizeX = Screen.width;
                    screenSizeY = Screen.height;
                    SetPlaneOrFullscreenTransformAspectRatio();
                }
            }

            //Make sure the Fullscreen 'Sidebars' are the correct size
            if( isPlaying && initialCameraAspectRatioCaptured && IsPlayVideoOnFullscreen() )
            {
                if( Camera.main != null && Camera.main.aspect != cameraAspectRatio )
                {
                    //Debug.Log( "Screen Size Changed, changing sidebar size... original( " + cameraAspectRatio + " ), new( " + Camera.main.aspect + " )" );
                    cameraAspectRatio = Camera.main.aspect;
                    SetFullscreenSidebarAspectRatio();
                }
            }




            //Make sure Youtube video and audio stays in sync (if video texture is the same as last frame, then pause the audio player until it changes)
            if( isPlaying && useAudioFromVideo && IsYoutubeVideo() )
            {
                if( audioPeriod > audioTimeInterval )
                {
                    audioPeriod = 0;

                    if( !audioFromVideoIsPaused && !MathHelper.IsBetween( videoPlayer.frame - audioFromVideoPlayer.frame, -1, 1, MathHelper.Bounds.INCLUSIVE_INCLUSIVE )  )
                    {
                        audioFromVideoIsPaused = true;
                        audioFromVideoPlayer.time = videoPlayer.time;
                        audioFromVideoPlayer.Pause();
                        videoPlayer.Pause();

                        coroutine_audioSync = Timer.instance.In( 1f, ResumeAfterSync, gameObject );

                        if( showDebug ) { Debug.Log( "BlockVideo.cs Update() sync Frame( A:" + audioFromVideoPlayer.frame + " vs V:" + videoPlayer.frame + " ) ... Offset( " + ( videoPlayer.frame - audioFromVideoPlayer.frame ) + " )" ); } //Time( A:" + audioFromVideoPlayer.time + " vs V:" + videoPlayer.time + " ) ... 
                    }
                    
                }

                audioPeriod += Time.deltaTime;
                
            }
            
        } //END Update
        
        //------------------------------//
        private void ResumeAfterSync()
        //------------------------------//
        {
            audioFromVideoPlayer.Play();
            videoPlayer.Play();
            audioFromVideoIsPaused = false;
            if( showDebug ) { Debug.Log( "BlockVideo.cs ResumeAfterSync() calling Play()" ); }

        } //END ResumeAfterSync



        //--------------------------------//
        /// <summary>
        /// Forces the block to rotate to face the camera
        /// </summary>
        /// <param name="forceX">Should the X axis be forced to face the camera?</param>
        /// <param name="forceY">Should the Y axis be forced to face the camera?</param>
        /// <param name="forceZ">Should the Z axis be forced to face the camera?</param>
        public override void FaceCamera( bool forceX, bool forceY, bool forceZ )
        //--------------------------------//
        {
            if( !IsPlayVideoOnSphere() )
            {
                base.FaceCamera( forceX, forceY, forceZ );
            }

        } //END FaceCamera

        //--------------------------------//
        public void SwitchVideoToRenderer()
        //--------------------------------//
        {

            SwitchVideoToDifferentRenderer( PlayVideoOn.Renderer );

        } //END SwitchToVideoRenderer
        
        //--------------------------------//
        public void SwitchVideoToPlane()
        //--------------------------------//
        {
            SwitchVideoToDifferentRenderer( PlayVideoOn.Plane );

        } //END SwitchVideoToPlane

        //--------------------------------//
        public void SwitchVideoToSphere()
        //--------------------------------//
        {

            SwitchVideoToDifferentRenderer( PlayVideoOn.Sphere );

        } //END SwitchVideoToSphere

        //--------------------------------//
        public void SwitchVideoToFullscreen()
        //--------------------------------//
        {

            SwitchVideoToDifferentRenderer( PlayVideoOn.Fullscreen );

        } //END SwitchVideoToFullscreen

        //--------------------------------//
        private void SwitchVideoToDifferentRenderer( PlayVideoOn playVideoOn )
        //--------------------------------//
        {
            ForceHide();

            this.playVideoOn = playVideoOn;
            
            HookVideoPlayerToRenderer();

            ShowOrHideBasedOnSettings();

            SwitchRendererVisibilityBasedOnSettings();

        } //END SwitchVideoToDifferentRenderer

        //--------------------------------//
        private void SetVideoPlayerBasedOnSettings()
        //--------------------------------//
        {

            if( IsVideoPlayerTypeUnity() && videoPlayer != null )
            {
                videoPlayer.isLooping = WhenFinishedLoop();
                videoPlayer.playbackSpeed = playbackSpeed;
            }

        } //END SetVideoPlayerBasedOnSettings();

        //--------------------------------//
        private void SetupLayersAndCamerasForStereo3D()
        //--------------------------------//
        {

            if( Is3D() && !IsSetupProjectFor3D_DoNothing() )
            {
                if( IsSetupProjectFor3D_SetupOnlyLayers() || IsSetupProjectFor3D_SetupLayersAndCameras() )
                {
                    //Make sure LeftEye & RightEye tags exist so we can use them!
                    LayerManager.CheckLayers( new string[] { LeftEyeLayerName, RightEyeLayerName } );
                }

                if( IsSetupProjectFor3D_SetupLayersAndCameras() )
                {
                    //Make sure we have the cameras needed to render Stereo
                    CameraManager.CheckForStereoscopicCameras( showDebug );
                }
            }

        } //END SetupLayersAndCamerasForStereo3D

        //--------------------------------//
        public void SetStereoToMono()
        //--------------------------------//
        {

            SetStereoSettings( StereoType.Mono );

        } //END SetStereoToMono

        //--------------------------------//
        public void SetStereoToStereoSideBySide()
        //--------------------------------//
        {

            SetStereoSettings( StereoType.StereoSideBySide );

        } //END SetStereoToStereoSideBySide

        //--------------------------------//
        public void SetStereoToStereoTopBottom()
        //--------------------------------//
        {

            SetStereoSettings( StereoType.StereoTopBottom );

        } //END SetStereoToStereoTopBottom

        //--------------------------------//
        public void SetStereoSettings( StereoType type )
        //--------------------------------//
        {
            stereo3DSettings = type;

            SetStereo3DSettings();

        } //END SetStereoSettings

        //--------------------------------//
        private void SetStereo3DSettings()
        //--------------------------------//
        {
            StereoType stereoType = stereo3DSettings;

            //Only has an effect if we're playing back on a Plane or Sphere, works even if we're passing in custom Renderers
            if( IsPlayVideoOnPlaneOrSphere() )
            {
                if( IsPlayVideoOnPlane() )
                {
                    SetMaterialToStereoType( videoPlane1, StereoPart.Part1, stereoType );
                    SetMaterialToStereoType( videoPlane2, StereoPart.Part2, stereoType );
                }
                else if( IsPlayVideoOnSphere() )
                {
                    SetMaterialToStereoType( videoSphere1, StereoPart.Part1, stereoType );
                    SetMaterialToStereoType( videoSphere2, StereoPart.Part2, stereoType );
                }
            }
            else if( IsPlayVideoOnRenderer() )
            {
                if( videoRenderer1 != null ) { SetMaterialToStereoType( videoRenderer1, StereoPart.Part1, stereoType ); }
                if( videoRenderer2 != null ) { SetMaterialToStereoType( videoRenderer2, StereoPart.Part2, stereoType ); }
            }
            else if( IsPlayVideoOnFullscreen() )
            {
                SetMaterialToStereoType( videoFullscreen1, StereoPart.Part1, stereoType );
                SetMaterialToStereoType( videoFullscreen2, StereoPart.Part2, stereoType );
            }

        } //END SetStereo3DSettings


        //-------------------------------------//
        private StereoType GetStereoType( string stereoTypeString )
        //-------------------------------------//
        {

            if( stereoTypeString == "3D Side-By-Side" )
            {
                return StereoType.StereoSideBySide;
            }
            else if( stereoTypeString == "3D Top-Bottom" )
            {
                return StereoType.StereoTopBottom;
            }

            return StereoType.Mono;

        } //END GetStereoType

        //-------------------------------------//
        private void SetMaterialToStereoType( Renderer renderer, StereoPart stereoPart, StereoType StereoType )
        //-------------------------------------//
        {

            SetMaterialToStereoType( renderer.sharedMaterial, stereoPart, StereoType );

        } //END SetMaterialToStereoType

        //-------------------------------------//
        private void SetMaterialToStereoType( RawImage renderer, StereoPart stereoPart, StereoType StereoType )
        //-------------------------------------//
        {

            SetMaterialToStereoType( renderer.material, stereoPart, StereoType );

        } //END SetMaterialToStereoType

        //-------------------------------------//
        private void SetMaterialToStereoType( Material material, StereoPart stereoPart, StereoType StereoType )
        //-------------------------------------//
        {
            
            if( material != null )
            {
                if( StereoType == StereoType.Mono )
                {
                    material.mainTextureScale = new Vector2( 1f, 1f );
                    material.mainTextureOffset = new Vector2( 0f, 0f );
                }
                else if( StereoType == StereoType.StereoSideBySide )
                {
                    material.mainTextureScale = new Vector2( .5f, 1f );

                    if( stereoPart == StereoPart.Part1 )
                    {
                        material.mainTextureOffset = new Vector2( 0f, 0f );
                    }
                    else
                    {
                        material.mainTextureOffset = new Vector2( .5f, 0f );
                    }
                }
                else if( StereoType == StereoType.StereoTopBottom )
                {
                    material.mainTextureScale = new Vector2( 1f, .5f );

                    if( stereoPart == StereoPart.Part1 )
                    {
                        material.mainTextureOffset = new Vector2( 0f, 0f );
                    }
                    else
                    {
                        material.mainTextureOffset = new Vector2( 0f, .5f );
                    }
                }

                //if( ShowDebug ) { Debug.Log( "BlockVideo.cs SetMaterialToStereoType() StereoType = " + StereoType + ", setting " + material.name + " " + stereoPart + ", Scale = " + material.mainTextureScale + ", Offset = " + material.mainTextureOffset ); }
            }

        } //END SetMaterialToStereoType

        //--------------------------------//
        private void SetShowTweenScale()
        //--------------------------------//
        {

            if( IsShowScaleAuto() )
            {
                if( IsPlayVideoOnRenderer() && videoRenderer1 != null && videoRenderer1.transform != null )
                {
                    onShowTweenScale = videoRenderer1.transform.localScale;
                }
                else if( IsPlayVideoOnPlane() && videoPlane1 != null && videoPlane1.transform != null )
                {
                    onShowTweenScale = videoPlane1.transform.localScale;
                }
                else if( IsPlayVideoOnSphere() && videoSphere1 != null && videoSphere1.transform != null )
                {
                    onShowTweenScale = videoSphere1.transform.localScale;
                }
                else if( IsPlayVideoOnFullscreen() && videoFullscreen1 != null && videoFullscreen1.transform != null )
                {
                    onShowTweenScale = videoFullscreen1.transform.localScale;
                }
            }

        } //END SetShowTweenScale

        //---------------------------------//
        private void SwitchRendererVisibilityBasedOnSettings()
        //---------------------------------//
        {

            if( IsPlayVideoOnRenderer() )
            {
                //Hide the other Renderers
                if( uiColorTweenManager_Plane != null )
                {
                    videoPlaneParent.gameObject.SetActive( false );
                    uiColorTweenManager_Plane.Force( UITweener.TweenValue.Hide );
                }
                if( uiColorTweenManager_Sphere != null )
                {
                    videoSphereParent.gameObject.SetActive( false );
                    uiColorTweenManager_Sphere.Force( UITweener.TweenValue.Hide );
                }
                if( uiColorTweenManager_Fullscreen != null )
                {
                    fullscreenParent.gameObject.SetActive( false );
                    uiColorTweenManager_Fullscreen.Force( UITweener.TweenValue.Hide );
                }


                if( videoRenderer1 != null && videoRenderer1.sharedMaterial != null )
                    videoRenderer1.sharedMaterial.color = new Color( videoRenderer1.sharedMaterial.color.r, videoRenderer1.sharedMaterial.color.g, videoRenderer1.sharedMaterial.color.b, 1f );

                if( Is3DSideBySide() || Is3DTopBottom() || IsCameraUsingVRMode() )
                {
                    if( videoRenderer2 != null && videoRenderer2.sharedMaterial != null )
                        videoRenderer2.sharedMaterial.color = new Color( videoRenderer2.sharedMaterial.color.r, videoRenderer2.sharedMaterial.color.g, videoRenderer2.sharedMaterial.color.b, 1f );
                }
                else
                {
                    if( videoRenderer2 != null && videoRenderer2.sharedMaterial != null )
                        videoRenderer2.sharedMaterial.color = new Color( videoRenderer2.sharedMaterial.color.r, videoRenderer2.sharedMaterial.color.g, videoRenderer2.sharedMaterial.color.b, 0f );
                }

            }
            else if( IsPlayVideoOnPlane() )
            {
                //Hide the other renderers
                if( uiColorTweenManager_Sphere != null )
                {
                    uiColorTweenManager_Sphere.Force( UITweener.TweenValue.Hide );
                }
                if( videoSphereParent != null && videoSphereParent.gameObject != null )
                {
                    videoSphereParent.gameObject.SetActive( false );
                }
                if( uiColorTweenManager_Fullscreen != null )
                {
                    fullscreenParent.gameObject.SetActive( false );
                    uiColorTweenManager_Fullscreen.Force( UITweener.TweenValue.Hide );
                }

                //Show the Plane
                if( uiColorTweenManager_Plane != null )
                {
                    videoPlaneParent.gameObject.SetActive( true );

                    if( Is3DSideBySide() || Is3DTopBottom() || IsCameraUsingVRMode() )
                    {
                        uiColorTweenManager_Plane.Force( UITweener.TweenValue.Show );
                    }
                    else
                    {
                        uiColorTweenManager_Plane.tweeners[ 0 ].Force( UITweener.TweenValue.Show );
                        uiColorTweenManager_Plane.tweeners[ 1 ].Force( UITweener.TweenValue.Hide );
                    }
                }

            }
            else if( IsPlayVideoOnSphere() )
            {
                //Hide the other renderers
                if( uiColorTweenManager_Plane != null )
                {
                    uiColorTweenManager_Plane.Force( UITweener.TweenValue.Hide );
                }
                if( videoPlaneParent != null && videoPlaneParent.gameObject != null )
                {
                    videoPlaneParent.gameObject.SetActive( false );
                }
                if( uiColorTweenManager_Fullscreen != null )
                {
                    fullscreenParent.gameObject.SetActive( false );
                    uiColorTweenManager_Fullscreen.Force( UITweener.TweenValue.Hide );
                }

                //Show the Sphere
                if( uiColorTweenManager_Sphere != null )
                {
                    videoSphereParent.gameObject.SetActive( true );

                    if( Is3DSideBySide() || Is3DTopBottom() || IsCameraUsingVRMode() )
                    {
                        uiColorTweenManager_Sphere.Force( UITweener.TweenValue.Show );
                    }
                    else
                    {
                        uiColorTweenManager_Sphere.tweeners[ 0 ].Force( UITweener.TweenValue.Show );
                        uiColorTweenManager_Sphere.tweeners[ 1 ].Force( UITweener.TweenValue.Hide );
                    }
                }
            }
            else if( IsPlayVideoOnFullscreen() )
            {
                //Hide the other renderers
                if( uiColorTweenManager_Plane != null )
                {
                    uiColorTweenManager_Plane.Force( UITweener.TweenValue.Hide );
                    videoPlaneParent.gameObject.SetActive( false );
                }
                if( uiColorTweenManager_Sphere != null )
                {
                    videoSphereParent.gameObject.SetActive( false );
                    uiColorTweenManager_Sphere.Force( UITweener.TweenValue.Hide );
                }

                //Show the Fullscreen Renderer
                if( uiColorTweenManager_Fullscreen != null && uiColorTweenManager_FullscreenSidebars != null )
                {
                    fullscreenParent.gameObject.SetActive( true );

                    if( Is3DSideBySide() || Is3DTopBottom() || IsCameraUsingVRMode() )
                    {
                        uiColorTweenManager_Fullscreen.Force( UITweener.TweenValue.Show );
                        uiColorTweenManager_FullscreenSidebars.Force( UITweener.TweenValue.Show );
                    }
                    else
                    {
                        uiColorTweenManager_Fullscreen.tweeners[ 0 ].Force( UITweener.TweenValue.Show );
                        uiColorTweenManager_Fullscreen.tweeners[ 1 ].Force( UITweener.TweenValue.Hide );

                        uiColorTweenManager_FullscreenSidebars.Force( UITweener.TweenValue.Show );
                    }

                }

            }

        } //END SwitchRendererVisibilityBasedOnSettings

        //---------------------------------//
        private void OnValidateForceRendererVisibilityBasedOnSettings()
        //---------------------------------//
        {

            if( IsPlayVideoOnRenderer() )
            {
                //Hide the other Renderers
                if( uiColorTweenManager_Plane != null )
                {
                    videoPlaneParent.gameObject.SetActive( false );
                    uiColorTweenManager_Plane.Force( UITweener.TweenValue.Hide );
                }
                if( uiColorTweenManager_Sphere != null )
                {
                    videoSphereParent.gameObject.SetActive( false );
                    uiColorTweenManager_Sphere.Force( UITweener.TweenValue.Hide );
                }
                if( uiColorTweenManager_Fullscreen != null )
                {
                    fullscreenParent.gameObject.SetActive( false );
                    uiColorTweenManager_Fullscreen.Force( UITweener.TweenValue.Hide );
                }


                if( videoRenderer1 != null && videoRenderer1.sharedMaterial != null )
                    videoRenderer1.sharedMaterial.color = new Color( videoRenderer1.sharedMaterial.color.r, videoRenderer1.sharedMaterial.color.g, videoRenderer1.sharedMaterial.color.b, 1f );

                if( Is3DSideBySide() || Is3DTopBottom() || IsCameraUsingVRMode() )
                {
                    if( videoRenderer2 != null && videoRenderer2.sharedMaterial != null)
                    videoRenderer2.sharedMaterial.color = new Color( videoRenderer2.sharedMaterial.color.r, videoRenderer2.sharedMaterial.color.g, videoRenderer2.sharedMaterial.color.b, 1f );
                }
                else
                {
                    if( videoRenderer2 != null && videoRenderer2.sharedMaterial != null )
                        videoRenderer2.sharedMaterial.color = new Color( videoRenderer2.sharedMaterial.color.r, videoRenderer2.sharedMaterial.color.g, videoRenderer2.sharedMaterial.color.b, 0f );
                }
                    
            }
            else if( IsPlayVideoOnPlane() )
            {
                //Hide the other renderers
                if( uiColorTweenManager_Sphere != null )
                {
                    uiColorTweenManager_Sphere.Force( UITweener.TweenValue.Hide );
                }
                if( videoSphereParent != null && videoSphereParent.gameObject != null )
                {
                    videoSphereParent.gameObject.SetActive( false );
                }
                if( uiColorTweenManager_Fullscreen != null )
                {
                    fullscreenParent.gameObject.SetActive( false );
                    uiColorTweenManager_Fullscreen.Force( UITweener.TweenValue.Hide );
                }

                //Show the Plane
                if( uiColorTweenManager_Plane != null )
                {
                    videoPlaneParent.gameObject.SetActive( true );

                    if( Is3DSideBySide() || Is3DTopBottom() || IsCameraUsingVRMode() )
                    {
                        uiColorTweenManager_Plane.Force( UITweener.TweenValue.Show );
                    }
                    else
                    {
                        uiColorTweenManager_Plane.tweeners[ 0 ].Force( UITweener.TweenValue.Show );
                        uiColorTweenManager_Plane.tweeners[ 1 ].Force( UITweener.TweenValue.Hide );
                    }
                }
                
            }
            else if( IsPlayVideoOnSphere() )
            {
                //Hide the other renderers
                if( uiColorTweenManager_Plane != null )
                {
                    uiColorTweenManager_Plane.Force( UITweener.TweenValue.Hide );
                }
                if( videoPlaneParent != null && videoPlaneParent.gameObject != null )
                {
                    videoPlaneParent.gameObject.SetActive( false );
                }
                if( uiColorTweenManager_Fullscreen != null )
                {
                    fullscreenParent.gameObject.SetActive( false );
                    uiColorTweenManager_Fullscreen.Force( UITweener.TweenValue.Hide );
                }

                //Show the Sphere
                if( uiColorTweenManager_Sphere != null )
                {
                    //We can disable/hide the sphere in editor if the user chooses, since it blocks so much of the view
                    if( showSphereInEditor )
                    {
                        videoSphereParent.gameObject.SetActive( true );

                        if( Is3DSideBySide() || Is3DTopBottom() || IsCameraUsingVRMode() )
                        {
                            uiColorTweenManager_Sphere.Force( UITweener.TweenValue.Show );
                        }
                        else
                        {
                            uiColorTweenManager_Sphere.tweeners[ 0 ].Force( UITweener.TweenValue.Show );
                            uiColorTweenManager_Sphere.tweeners[ 1 ].Force( UITweener.TweenValue.Hide );
                        }
                    }
                    else
                    {
                        videoSphereParent.gameObject.SetActive( false );

                        uiColorTweenManager_Sphere.Force( UITweener.TweenValue.Hide );
                    }
                }
            }
            else if( IsPlayVideoOnFullscreen() )
            {
                //Hide the other renderers
                if( uiColorTweenManager_Plane != null )
                {
                    uiColorTweenManager_Plane.Force( UITweener.TweenValue.Hide );
                    videoPlaneParent.gameObject.SetActive( false );
                }
                if( uiColorTweenManager_Sphere != null )
                {
                    videoSphereParent.gameObject.SetActive( false );
                    uiColorTweenManager_Sphere.Force( UITweener.TweenValue.Hide );
                }

                //Show the Fullscreen Renderer
                if( uiColorTweenManager_Fullscreen != null && uiColorTweenManager_FullscreenSidebars != null )
                {
                    fullscreenParent.gameObject.SetActive( true );

                    if( ( Application.isEditor && showFullscreenVideoInEditor ) || !Application.isEditor || Application.isPlaying )
                    {
                        if( Is3DSideBySide() || Is3DTopBottom() || IsCameraUsingVRMode() )
                        {
                            if( showFullscreenVideoInEditor )
                            {
                                uiColorTweenManager_Fullscreen.Force( UITweener.TweenValue.Show );
                                uiColorTweenManager_FullscreenSidebars.Force( UITweener.TweenValue.Show );
                            }
                            else
                            {
                                uiColorTweenManager_Fullscreen.Force( UITweener.TweenValue.Hide );
                                uiColorTweenManager_FullscreenSidebars.Force( UITweener.TweenValue.Hide );
                            }
                        }
                        else
                        {
                            if( showFullscreenVideoInEditor )
                            {
                                uiColorTweenManager_Fullscreen.tweeners[ 0 ].Force( UITweener.TweenValue.Show );
                                uiColorTweenManager_Fullscreen.tweeners[ 1 ].Force( UITweener.TweenValue.Hide );
                                
                                uiColorTweenManager_FullscreenSidebars.Force( UITweener.TweenValue.Show );
                            }
                            else
                            {
                                uiColorTweenManager_Fullscreen.Force( UITweener.TweenValue.Hide );
                                uiColorTweenManager_FullscreenSidebars.Force( UITweener.TweenValue.Hide );
                            }
                        }
                    }
                    else
                    {
                        uiColorTweenManager_Fullscreen.Force( UITweener.TweenValue.Hide );
                        uiColorTweenManager_FullscreenSidebars.Force( UITweener.TweenValue.Hide );
                    }
                    
                }

            }
            
        } //END ForceRendererVisibilityBasedOnSettings

        

        //--------------------------------//
        private void SetLoadingVisuals()
        //--------------------------------//
        {

            if( !hasFatalErrorLogicBeenCalled )
            {
                
                Texture texture = null;

                if( ChangeLoadingImageUsingTexture() && debugShowLoadingImageInEditor && Application.isEditor && !Application.isPlaying && loadingTexture != null )
                {
                    texture = loadingTexture;
                }

                if( IsPlayVideoOnRenderer() )
                {
                    if( videoRenderer1 != null && videoRenderer1.sharedMaterial != null ) { videoRenderer1.sharedMaterial.mainTexture = texture; }

                    if( ( Is3DSideBySide() || Is3DTopBottom() || IsCameraUsingVRMode() ) && videoRenderer2 != null && videoRenderer2.sharedMaterial != null )
                    {
                        videoRenderer2.sharedMaterial.mainTexture = texture;
                    }
                }
                else if( IsPlayVideoOnPlane() )
                {
                    if( videoPlane1 != null && videoPlane1.sharedMaterial != null ) { videoPlane1.sharedMaterial.mainTexture = texture; }
                    if( videoPlane2 != null && videoPlane2.sharedMaterial != null ) { videoPlane2.sharedMaterial.mainTexture = texture; }
                }
                else if( IsPlayVideoOnSphere() )
                {
                    if( videoSphere1 != null && videoSphere1.sharedMaterial != null ) { videoSphere1.sharedMaterial.mainTexture = texture; }
                    if( videoSphere2 != null && videoSphere2.sharedMaterial != null ) { videoSphere2.sharedMaterial.mainTexture = texture; }
                }
                else if( IsPlayVideoOnFullscreen() )
                {
                    if( videoFullscreen1 != null ) { videoFullscreen1.texture = texture; }
                    if( videoFullscreen2 != null ) { videoFullscreen2.texture = texture; }
                    //Debug.Log( "BlockVideo.cs SetLoadingVisuals() material.texture = " + videoFullscreen1.material.mainTexture + ", and rawImage.texture = " + videoFullscreen1.mainTexture );
                }




                if( loadingAnimationMaterial != null )
                {
                    if( Loading_Animate() && debugShowAnimationInEditor && Application.isEditor && !Application.isPlaying )
                    {
                        if( ShowAndHideSetToBlockButton() && playWhenBlockButtonSelected )
                        {
                            loadingAnimationMaterial.color = new Color( loadingAnimationMaterial.color.r, loadingAnimationMaterial.color.g, loadingAnimationMaterial.color.b, 0f );
                        }
                        else
                        {
                            loadingAnimationMaterial.color = new Color( loadingAnimationMaterial.color.r, loadingAnimationMaterial.color.g, loadingAnimationMaterial.color.b, 1f );
                        }
                    }
                    else
                    {
                        loadingAnimationMaterial.color = new Color( loadingAnimationMaterial.color.r, loadingAnimationMaterial.color.g, loadingAnimationMaterial.color.b, 0f );
                    }
                }
            }

        } //END SetLoadingVisuals

        //--------------------------------//
        private void SetLoadingImageBasedOnTexture()
        //--------------------------------//
        {
            loadingImageIsReady = false;

            if( ChangeLoadingImageUsingTexture() && loadingTexture != null )
            {
                loadingImageIsReady = true;
            }
            else if( ( !Application.isEditor || ( Application.isEditor && Application.isPlaying ) ) && ChangeLoadingImageUsingPath() && !string.IsNullOrEmpty( loadingPath ) )
            {
                WWWHelper.instance.GetTexture( loadingPath, cacheLoadingImageIfWeb, GetLoadingImageSuccess, GetLoadingImageFailed );
            }

        } //END SetLoadingImageBasedOnTexture
        
        //--------------------------------//
        private void GetLoadingImageSuccess( Texture texture )
        //--------------------------------//
        {

            loadingTexture = texture;
            loadingImageIsReady = true;

        } //END GetLoadingImageSuccess

        //--------------------------------//
        private void GetLoadingImageFailed()
        //--------------------------------//
        {

            loadingImageIsReady = false;

            if( showDebug ) { Debug.Log( "BlockVideo.cs GetLoadingImageFailed()" ); }

        } //END GetLoadingImageFailed

        //--------------------------------//
        private void SetLoadingAnimationBasedOnTexture()
        //--------------------------------//
        {

            loadingAnimationIsReady = false;

            if( AnimationFromTexture() && animationTexture != null )
            {
                if( loadingAnimationMaterial != null ) { loadingAnimationMaterial.mainTexture = animationTexture; }
                SetLoadingAnimationRenderers();
                loadingAnimationIsReady = true;
            }
            else if( ( !Application.isEditor || ( Application.isEditor && Application.isPlaying ) ) && AnimationFromPath() && !string.IsNullOrEmpty( animationPath ) )
            {
                WWWHelper.instance.GetTexture( animationPath, cacheLoadingAnimationIfWeb, GetLoadingAnimationSuccess, GetLoadingAnimationFailed );
            }

        } //END SetLoadingAnimationBasedOnTexture

        //--------------------------------//
        private void GetLoadingAnimationSuccess( Texture texture )
        //--------------------------------//
        {
            if( loadingAnimationMaterial != null ) { loadingAnimationMaterial.mainTexture = animationTexture; }
            animationTexture = texture;
            SetLoadingAnimationRenderers();
            loadingAnimationIsReady = true;

        } //END GetLoadingAnimationSuccess

        //--------------------------------//
        private void SetLoadingAnimationScale()
        //--------------------------------//
        {

            if( Loading_Animate() )
            {
                Vector3 scale;

                if( IsPlayVideoOnPlaneOrRenderer() )
                {
                    scale = new Vector3( animationScale, animationScale, animationScale );
                    if( rawImage_LoadingAnimation_Plane != null ) { rawImage_LoadingAnimation_Plane.transform.localScale = scale; }
                }
                else if( IsPlayVideoOnPlaneOrSphere() )
                {
                    scale = new Vector3( animationSphereScale, animationSphereScale, animationSphereScale );

                    if( ShowLoadingAnimationOnFrontFace() ) { rawImage_LoadingAnimation_Sphere_Front.transform.localScale  = scale; }
                    if( ShowLoadingAnimationOnBackFace() )  { rawImage_LoadingAnimation_Sphere_Back.transform.localScale   = scale; }
                    if( ShowLoadingAnimationOnLeftFace() )  { rawImage_LoadingAnimation_Sphere_Left.transform.localScale   = scale; }
                    if( ShowLoadingAnimationOnRightFace() ) { rawImage_LoadingAnimation_Sphere_Right.transform.localScale  = scale; }
                    if( ShowLoadingAnimationOnTopFace() )   { rawImage_LoadingAnimation_Sphere_Top.transform.localScale    = scale; }
                    if( ShowLoadingAnimationOnBottomFace() ){ rawImage_LoadingAnimation_Sphere_Bottom.transform.localScale = scale; }
                }
                else if( IsPlayVideoOnFullscreen() )
                {
                    scale = new Vector3( animationFullscreenScale, animationFullscreenScale, animationFullscreenScale );

                    if( rawImage_LoadingAnimation_Fullscreen != null ) { rawImage_LoadingAnimation_Fullscreen.transform.localScale = scale; }
                }
                
            }

        } //END SetLoadingAnimationScale

        //--------------------------------//
        private void GetLoadingAnimationFailed()
        //--------------------------------//
        {
            
            loadingAnimationIsReady = false;

            if( showDebug ) { Debug.Log( "BlockVideo.cs GetLoadingAnimationFailed()" ); }

        } //END GetLoadingAnimationFailed

        //--------------------------------//
        private void SetLoadingAnimationRenderers()
        //--------------------------------//
        {

            if( animationTexture != null )
            {
                if( IsPlayVideoOnPlaneOrRenderer() )
                {
                    //Hide the Sphere & Fullscreen loading animation renderers
                    EnableLoadingAnimationGameObject( rawImage_LoadingAnimation_Plane, true );
                    
                    EnableLoadingAnimationGameObject( rawImage_LoadingAnimation_Sphere_Front, false );
                    EnableLoadingAnimationGameObject( rawImage_LoadingAnimation_Sphere_Back, false );
                    EnableLoadingAnimationGameObject( rawImage_LoadingAnimation_Sphere_Left, false );
                    EnableLoadingAnimationGameObject( rawImage_LoadingAnimation_Sphere_Right, false );
                    EnableLoadingAnimationGameObject( rawImage_LoadingAnimation_Sphere_Top, false );
                    EnableLoadingAnimationGameObject( rawImage_LoadingAnimation_Sphere_Bottom, false );

                    EnableLoadingAnimationGameObject( rawImage_LoadingAnimation_Fullscreen, false );
                }

                if( IsPlayVideoOnSphere() )
                {
                    //Hide the Plane & Fullscreen loading animation renderer
                    EnableLoadingAnimationGameObject( rawImage_LoadingAnimation_Plane, false );
                    
                    EnableLoadingAnimationGameObject( rawImage_LoadingAnimation_Sphere_Front, showOnFrontFace );
                    EnableLoadingAnimationGameObject( rawImage_LoadingAnimation_Sphere_Back, showOnBackFace );
                    EnableLoadingAnimationGameObject( rawImage_LoadingAnimation_Sphere_Left, showOnLeftFace );
                    EnableLoadingAnimationGameObject( rawImage_LoadingAnimation_Sphere_Right, showOnRightFace );
                    EnableLoadingAnimationGameObject( rawImage_LoadingAnimation_Sphere_Top, showOnTopFace );
                    EnableLoadingAnimationGameObject( rawImage_LoadingAnimation_Sphere_Bottom, showOnBottomFace );

                    EnableLoadingAnimationGameObject( rawImage_LoadingAnimation_Fullscreen, false );
                    
                }

                if( IsPlayVideoOnFullscreen() )
                {
                    //Hide the Plane & Sphere loading animation renderers
                    EnableLoadingAnimationGameObject( rawImage_LoadingAnimation_Plane, false );

                    EnableLoadingAnimationGameObject( rawImage_LoadingAnimation_Sphere_Front, false );
                    EnableLoadingAnimationGameObject( rawImage_LoadingAnimation_Sphere_Back, false );
                    EnableLoadingAnimationGameObject( rawImage_LoadingAnimation_Sphere_Left, false );
                    EnableLoadingAnimationGameObject( rawImage_LoadingAnimation_Sphere_Right, false );
                    EnableLoadingAnimationGameObject( rawImage_LoadingAnimation_Sphere_Top, false );
                    EnableLoadingAnimationGameObject( rawImage_LoadingAnimation_Sphere_Bottom, false );

                    EnableLoadingAnimationGameObject( rawImage_LoadingAnimation_Fullscreen, true );
                }
            }

        } //END SetLoadingAnimationRenderers

        //--------------------------------//
        private void EnableLoadingAnimationGameObject( RawImage renderer, bool setActive )
        //--------------------------------//
        {

            if( renderer != null )
            {
                renderer.gameObject.SetActive( setActive );
                
                if( Application.isEditor && !Application.isPlaying ) { renderer.SetMaterialDirty(); } //Force UnityEditor to update the renderer
            }
            else if( renderer != null )
            {
                renderer.gameObject.SetActive( setActive );
            }

        } //END EnableLoadingAnimationGameObject

        //--------------------------------//
        private void HookVideoPlayerToRenderer()
        //--------------------------------//
        {

            if( IsVideoPlayerTypeUnity() && videoPlayer != null )
            {
                videoPlayer.targetMaterialProperty = "_MainTex";

                //We already have the Unity Video Player setting it's Texture to a renderer/material (located on the same GameObject as the VideoPlayer)
                //So what we have to do now is take whatever Renderer/Material we want to set the VideoClip texture to, and override the Material's Texture setting to
                //show the VideoClip instead
                if( IsPlayVideoOnRenderer() && videoRenderer1 != null )
                {
                    //Check if we need to set the texture to a different Material property than '_MainTex'
                    if( IsPlayVideoOnRendererAndOverrideMaterialPropertyTrue() && !string.IsNullOrEmpty( setTextureToMaterialProperty ) )
                    {
                        videoPlayer.targetMaterialProperty = setTextureToMaterialProperty;
                    }

                    //Set the videoRenderer material texture to be the same as our VideoPlayer's VideoClip Texture
                    SetToVideoPlayerTexture( videoRenderer1 );

                    //Set the Layer of the Renderer to 'LeftEye'
                    videoRenderer1.gameObject.layer = LayerMask.NameToLayer( LeftEyeLayerName );

                    //If we're playing back a 3D file, then set the Texture to the second Renderer
                    if( ( Is3DSideBySide() || Is3DTopBottom() || IsCameraUsingVRMode() ) && videoRenderer2 != null )
                    {
                        SetToVideoPlayerTexture( videoRenderer2 );

                        //Set the Layer of the Renderer to 'LeftEye'
                        videoRenderer2.gameObject.layer = LayerMask.NameToLayer( RightEyeLayerName );
                    }
                }
                else if( IsPlayVideoOnPlane() && videoPlane1 != null && videoPlane2 != null )
                {
                    //Set the videoRenderer material texture to be the same as our VideoPlayer's VideoClip Texture
                    SetToVideoPlayerTexture( videoPlane1 );
                    SetToVideoPlayerTexture( videoPlane2 );
                }
                else if( IsPlayVideoOnSphere() && videoSphere1 != null && videoSphere2 != null )
                {
                    //Set the videoRenderer material texture to be the same as our VideoPlayer's VideoClip Texture
                    SetToVideoPlayerTexture( videoSphere1 );
                    SetToVideoPlayerTexture( videoSphere2 );
                }
                else if( IsPlayVideoOnFullscreen() && videoFullscreen1 != null && videoFullscreen2 != null )
                {
                    SetToVideoPlayerTexture( videoFullscreen1 );
                    SetToVideoPlayerTexture( videoFullscreen2 );
                }
            }

        } //END HookVideoPlayerToRenderer

        //----------------------------------//
        private void SetToVideoPlayerTexture( Renderer renderer )
        //----------------------------------//
        {

            renderer.sharedMaterial.SetTexture( videoPlayer.targetMaterialProperty, videoPlayer.texture );

        } //END SetToVideoPlayerTexture

        //----------------------------------//
        private void SetToVideoPlayerTexture( RawImage renderer )
        //----------------------------------//
        {

            renderer.texture = videoPlayer.texture;
            //renderer.material.SetTexture( videoPlayer.targetMaterialProperty, videoPlayer.texture );
            //Debug.Log( "BlockVideo.cs SetToVideoPlayerTexture() material.texture = " + videoFullscreen1.material.mainTexture + ", and rawImage.texture = " + videoFullscreen1.mainTexture );

        } //END SetToVideoPlayerTexture

        //-------------------------------//
        private void ShowErrorImageForEditorDebug()
        //-------------------------------//
        {
            if( debugShowErrorImageInEditor && Application.isEditor && !Application.isPlaying && errorTexture != null )
            {
                //Debug.Log( "BlockVideo.cs ShowErrorImageForEditorDebug()" );

                if( IsPlayVideoOnRenderer() )
                {
                    if( videoRenderer1 != null && videoRenderer1.sharedMaterial != null ) { videoRenderer1.sharedMaterial.mainTexture = errorTexture; }

                    if( ( Is3DSideBySide() || Is3DTopBottom() || IsCameraUsingVRMode() ) && videoRenderer2 != null && videoRenderer2.sharedMaterial != null )
                    {
                        videoRenderer2.sharedMaterial.mainTexture = errorTexture;
                    }
                }
                else if( IsPlayVideoOnPlane() )
                {
                    if( videoPlane1 != null && videoPlane1.sharedMaterial != null ) { videoPlane1.sharedMaterial.mainTexture = errorTexture; }
                    if( videoPlane2 != null && videoPlane2.sharedMaterial != null ) { videoPlane2.sharedMaterial.mainTexture = errorTexture; }
                }
                else if( IsPlayVideoOnSphere() )
                {
                    if( videoSphere1 != null && videoSphere1.sharedMaterial != null ) { videoSphere1.sharedMaterial.mainTexture = errorTexture; }
                    if( videoSphere2 != null && videoSphere2.sharedMaterial != null ) { videoSphere2.sharedMaterial.mainTexture = errorTexture; }
                }
                else if( IsPlayVideoOnFullscreen() )
                {
                    if( videoFullscreen1 != null && videoFullscreen1.material != null ) { videoFullscreen1.texture = errorTexture; }
                    if( videoFullscreen2 != null && videoFullscreen2.material != null ) { videoFullscreen2.texture = errorTexture; }
                }

            }
            
        } //END ShowErrorImageForEditorDebug

        //-------------------------------//
        private void ForceSphereToCameraPosition()
        //-------------------------------//
        {

            if( IsPlayVideoOnSphere() && Camera.main != null && videoSphere1 != null && videoSphere2 != null )
            {
                //if( ShowDebug ) { Debug.Log( "BlockVideo.cs ForceSphereToCameraPosition() setting videoSphere to camera position" ); }
                videoSphere1.transform.position = Camera.main.transform.position;
                videoSphere2.transform.position = Camera.main.transform.position;

                if( loadingAnimation_SphereParent != null ) { loadingAnimation_SphereParent.position = Camera.main.transform.position; }
            }

        } //END ForceSphereToCameraPosition

        
        //--------------------------------//
        private void SetPlaneOrFullscreenTransformAspectRatio()
        //--------------------------------//
        {

            if( IsPlayVideoOnPlane() )
            {
                Vector3 ratio = GetAspectRatio( aspectRatio, customAspectRatio );
                videoPlane1.transform.localScale = ratio;
                videoPlane2.transform.localScale = ratio;
            }
            else if( IsPlayVideoOnFullscreen() )
            {
                aspectRatioFitter_Fullscreen.aspectRatio = GetFullscreenAspectRatio( aspectRatio, (float)( (float)customAspectRatio.x / (float)customAspectRatio.y) );
            }

        } //END SetPlaneOrFullscreenTransformAspectRatio

        //--------------------------------//
        private void SetFullscreenSidebarAspectRatio()
        //--------------------------------//
        {

            if( Camera.main != null )
            {
                aspectRatioFitter_Sidebars.aspectRatio = Camera.main.aspect;
                //Debug.Log( "SetFullscreenSidebarAspectRatio() Setting Sidebars to " + Camera.main.aspect );
            }

        } //END SetFullscreenSidebarAspectRatio

        //--------------------------------//
        private void SetFullscreenSidebarColors()
        //--------------------------------//
        {

            if( showSideBars )
            {
                uiColorTweenManager_FullscreenSidebars.SetColor( new Color( sideBarsColor.r, sideBarsColor.g, sideBarsColor.b, 0f ), UITweener.TweenValue.Hide );
                uiColorTweenManager_FullscreenSidebars.SetColor( sideBarsColor, UITweener.TweenValue.Show );
            }
            else
            {
                uiColorTweenManager_FullscreenSidebars.SetColor( new Color( sideBarsColor.r, sideBarsColor.g, sideBarsColor.b, 0f ), UITweener.TweenValue.Hide );
                uiColorTweenManager_FullscreenSidebars.SetColor( new Color( Color.white.r, Color.white.g, Color.white.b, 0f ), UITweener.TweenValue.Hide );
            }

            if( ( Application.isEditor && showFullscreenVideoInEditor ) || !Application.isEditor || Application.isPlaying )
            {
                if( !ShowAndHideSetToBlockButton() || ( ShowAndHideSetToBlockButton() && blockGroup != null && BlockFocusManager.instance.IsFocused( blockGroup ) ) )
                {
                    fullscreenSidebars.color = uiColorTweenManager_FullscreenSidebars.tweeners[ 0 ].GetEndColor( UITweener.TweenValue.Show );
                }
                else
                {
                    fullscreenSidebars.color = uiColorTweenManager_FullscreenSidebars.tweeners[ 0 ].GetEndColor( UITweener.TweenValue.Hide );
                }
            }
            else
            {
                fullscreenSidebars.color = uiColorTweenManager_FullscreenSidebars.tweeners[ 0 ].GetEndColor( UITweener.TweenValue.Hide );
            }

        } //END SetFullscreenSidebarColors

        //--------------------------------//
        private void SetLookAwayColliderAspectRatio()
        //--------------------------------//
        {

            if( IsDoSomethingOnLookAwayAndLookBack() )
            {
                colliderForLookAwayAndLookBack.transform.localScale = GetAspectRatio( lookAwayColliderRatio, customLookAwayColliderAspectRatio );
            }

        } //END SetLookAwayColliderAspectRatio

        //--------------------------------//
        private Vector3 GetAspectRatio( AspectRatio choice, Vector2 customRatio )
        //--------------------------------//
        {

            if( choice == AspectRatio.Custom ) { return new Vector3( customRatio.x, customRatio.y, 1f ); }

            else if( choice == AspectRatio._1x1 ) { return aspectRatio_1x1; }

            else if( choice == AspectRatio._4x3 ) { return aspectRatio_4x3; }
            else if( choice == AspectRatio._3x4 ) { return aspectRatio_3x4; }

            else if( choice == AspectRatio._5x4 ) { return aspectRatio_5x4; }
            else if( choice == AspectRatio._4x5 ) { return aspectRatio_4x5; }

            else if( choice == AspectRatio._16x9 ) { return aspectRatio_16x9; }
            else if( choice == AspectRatio._9x16 ) { return aspectRatio_9x16; }

            else if( choice == AspectRatio._16x10 ) { return aspectRatio_16x10; }
            else if( choice == AspectRatio._10x16 ) { return aspectRatio_10x16; }

            else if( choice == AspectRatio._18x9 ) { return aspectRatio_18x9; }
            else if( choice == AspectRatio._9x18 ) { return aspectRatio_9x18; }

            else if( choice == AspectRatio._21x9 ) { return aspectRatio_21x9; }
            else if( choice == AspectRatio._9x21 ) { return aspectRatio_9x21; }

            else if( choice == AspectRatio._1d85x1 ) { return aspectRatio_185x1; }
            else if( choice == AspectRatio._1x1d85 ) { return aspectRatio_1x185; }

            else if( choice == AspectRatio._2d39x1 ) { return aspectRatio_239x1; }
            else if( choice == AspectRatio._1x2d39 ) { return aspectRatio_1x239; }

            return aspectRatio_16x9;

        } //END GetAspectRatio

        //--------------------------------//
        private float GetFullscreenAspectRatio( AspectRatio choice, float customRatio )
        //--------------------------------//
        {

            if( choice == AspectRatio.Custom ) { return customRatio; }

            else if( choice == AspectRatio._1x1 ) { return aspectRatio_Fullscreen_1x1; }

            else if( choice == AspectRatio._4x3 ) { return aspectRatio_Fullscreen_4x3; }
            else if( choice == AspectRatio._3x4 ) { return aspectRatio_Fullscreen_3x4; }

            else if( choice == AspectRatio._5x4 ) { return aspectRatio_Fullscreen_5x4; }
            else if( choice == AspectRatio._4x5 ) { return aspectRatio_Fullscreen_4x5; }

            else if( choice == AspectRatio._16x9 ) { return aspectRatio_Fullscreen_16x9; }
            else if( choice == AspectRatio._9x16 ) { return aspectRatio_Fullscreen_9x16; }

            else if( choice == AspectRatio._16x10 ) { return aspectRatio_Fullscreen_16x10; }
            else if( choice == AspectRatio._10x16 ) { return aspectRatio_Fullscreen_10x16; }

            else if( choice == AspectRatio._18x9 ) { return aspectRatio_Fullscreen_18x9; }
            else if( choice == AspectRatio._9x18 ) { return aspectRatio_Fullscreen_9x18; }

            else if( choice == AspectRatio._21x9 ) { return aspectRatio_Fullscreen_21x9; }
            else if( choice == AspectRatio._9x21 ) { return aspectRatio_Fullscreen_9x21; }

            else if( choice == AspectRatio._1d85x1 ) { return aspectRatio_Fullscreen_185x1; }
            else if( choice == AspectRatio._1x1d85 ) { return aspectRatio_Fullscreen_1x185; }

            else if( choice == AspectRatio._2d39x1 ) { return aspectRatio_Fullscreen_239x1; }
            else if( choice == AspectRatio._1x2d39 ) { return aspectRatio_Fullscreen_1x239; }

            return aspectRatio_Fullscreen_16x9;

        } //END GetFullscreenAspectRatio

        //---------------------------------//
        private void SetPlaneScale()
        //---------------------------------//
        {

            if( IsPlayVideoOnPlane() && videoPlaneParent != null )
            {
                videoPlaneParent.localScale = new Vector3( scale, scale, 1f );
            }

        } //END SetPlaneScale
        

        //---------------------------------//
        private void SetLookAwayColliderScale()
        //---------------------------------//
        {

            if( IsDoSomethingOnLookAwayAndLookBack() && lookAwayColliderParent != null )
            {
                if( ScaleLookAwayColliderWithPlane() )
                {
                    lookAwayColliderParent.localScale = new Vector3( scale, scale, 1f );
                }
                else
                {
                    lookAwayColliderParent.localScale = new Vector3( lookAwayColliderScale, lookAwayColliderScale, 1f );
                }
            }

        } //END SetLookAwayColliderScale

        //---------------------------------//
        private void ForceColliderVisibilityBasedOnSettings()
        //---------------------------------//
        {
            
            if( IsDoSomethingOnLookAwayAndLookBack() && showColliderForLookAwayInEditor && !Application.isPlaying && uiColorTweenManager_LookaAwayCollider != null )
            {
                uiColorTweenManager_LookaAwayCollider.SetColor( lookAwayColliderColor, UITweener.TweenValue.Show );
                uiColorTweenManager_LookaAwayCollider.SetColor( new Color( lookAwayColliderColor.r, lookAwayColliderColor.g, lookAwayColliderColor.b, 0f ), UITweener.TweenValue.Hide );
                uiColorTweenManager_LookaAwayCollider.Force( UITweener.TweenValue.Show );
            }
            else if( uiColorTweenManager_LookaAwayCollider != null )
            {
                uiColorTweenManager_LookaAwayCollider.Force( UITweener.TweenValue.Hide );
            }

        } //END ForceColliderVisibilityBasedOnSettings

        //-------------------------------------------//
        private void SetAudioFromVideoPlayerAudioSource()
        //-------------------------------------------//
        {

            if( videoPlayer != null && audioFromVideoPlayer != null )
            {
                videoPlayer.SetDirectAudioVolume( 0, 0f );

                audioFromVideoPlayer.audioOutputMode = VideoAudioOutputMode.AudioSource;
                audioFromVideoPlayer.controlledAudioTrackCount = 1;
                audioFromVideoPlayer.EnableAudioTrack( 0, true );

                if( IsBuiltInAudioSource() )
                {
                    if( audioSource == null )
                    {
                        if( GetComponent<AudioSource>() != null )
                        {
                            audioSource = GetComponent<AudioSource>();
                        }
                        else
                        {
                            audioSource = gameObject.AddComponent<AudioSource>();
                        }
                    }

                    audioFromVideoPlayer.SetTargetAudioSource( 0, audioSource );
                    audioSource.Play();
                }
                else if( IsCustomAudioSource() && customAudioSource != null )
                {
                    audioFromVideoPlayer.SetTargetAudioSource( 0, customAudioSource );
                    customAudioSource.Play();
                }
            }

        } //END SetAudioFromVideoPlayerAudioSource

        //-------------------------------------------//
        private void SetVideoPlayerAudioSource()
        //-------------------------------------------//
        {

            if( videoPlayer != null )
            {
                videoPlayer.audioOutputMode = VideoAudioOutputMode.AudioSource;
                videoPlayer.controlledAudioTrackCount = 1;
                videoPlayer.EnableAudioTrack( 0, true );

                if( IsBuiltInAudioSource() )
                {
                    if( audioSource == null )
                    {
                        if( GetComponent<AudioSource>() != null )
                        {
                            audioSource = GetComponent<AudioSource>();
                        }
                        else
                        {
                            audioSource = gameObject.AddComponent<AudioSource>();
                        }
                    }
                    
                    videoPlayer.SetTargetAudioSource( 0, audioSource );
                    audioSource.Play();
                }
                else if( IsCustomAudioSource() && customAudioSource != null )
                {
                    videoPlayer.SetTargetAudioSource( 0, customAudioSource );
                    customAudioSource.Play();
                }
            }

        } //END SetVideoPlayerAudioSource

        //--------------------------------------//
        private void SetAudioSourceSettings()
        //--------------------------------------//
        {
            //Set the videoPlayer audioSource
            if( IsVideoPlayerTypeUnity() && videoPlayer != null )
            {
                if( IsBuiltInAudioSource() && audioSource != null )
                {
                    audioSource.volume = volume;

                    if( IsPlayVideoOnPlaneOrRenderer() )
                    {
                        if( IsAlwaysAudible() ) { audioSource.spatialBlend = 0f; }
                        else if( IsAudibleWhenNear() ) { audioSource.spatialBlend = 1f; }
                    }
                    else if( IsPlayVideoOnSphere() )
                    {
                        audioSource.spatialBlend = 0f;
                    }
                }
                else if( IsCustomAudioSource() && customAudioSource != null )
                {
                    customAudioSource.volume = volume;

                    if( IsPlayVideoOnPlaneOrRenderer() )
                    {
                        if( IsAlwaysAudible() ) { audioSource.spatialBlend = 0f; }
                        else if( IsAudibleWhenNear() ) { audioSource.spatialBlend = 1f; }
                    }
                    else if( IsPlayVideoOnSphere() )
                    {
                        audioSource.spatialBlend = 0f;
                    }
                }
            }
            
            if( LoadingAudio_Play() )
            {
                if( IsLoadAudioAlwaysAudible() ) { loadingAudioSource.spatialBlend = 0f; }
                else if( IsLoadAudioAudibleWhenNear() ) { loadingAudioSource.spatialBlend = 1f; }

                loadingAudioSource.volume = loadingAudioVolume;
                loadingAudioSource.loop = loadAudioLoop;
            }

            if( PlayAudioOnLoadComplete() )
            {
                if( IsLoadCompleteAudioAlwaysAudible() ) { loadCompleteAudioSource.spatialBlend = 0f; }
                else if( IsLoadCompleteAudioAudibleWhenNear() ) { loadCompleteAudioSource.spatialBlend = 1f; }

                loadCompleteAudioSource.volume = loadCompleteAudioVolume;
            }

            if( OnErrorPlayAudio_PlayAudio() )
            {
                if( IsErrorAudioAlwaysAudible() ) { errorAudioSource.spatialBlend = 0f; }
                else if( IsErrorAudioAudibleWhenNear() ) { errorAudioSource.spatialBlend = 1f; }

                errorAudioSource.volume = errorAudioVolume;
            }

        } //END SetAudioSourceSettings

        //--------------------------------------//
        private void SetupLoadingAudio()
        //--------------------------------------//
        {

            if( LoadingAudio_Play() )
            {
                IsLoadingAudioSourceReady = false;

                if( IsLoadingAudioClip() && loadingAudioClip != null )
                {
                    loadingAudioSource.clip = loadingAudioClip;
                    IsLoadingAudioSourceReady = true;
                }
                else if( IsLoadingAudioPath() && !string.IsNullOrEmpty( loadingAudioPath ) )
                {
                    WWWHelper.instance.GetAudioClip( loadingAudioPath, cacheLoadingAudioIfWeb, GetLoadingAudioSuccess, GetLoadingAudioFailed );
                }
            }

        } //END SetupLoadingAudio

        //--------------------------------------//
        private void GetLoadingAudioSuccess( AudioClip clip )
        //--------------------------------------//
        {

            loadingAudioClip = clip;
            loadingAudioSource.clip = clip;
            IsLoadingAudioSourceReady = true;

        } //END GetLoadingAudioSuccess

        //--------------------------------------//
        private void GetLoadingAudioFailed()
        //--------------------------------------//
        {

            loadingAudioClip = null;
            loadingAudioSource.clip = null;
            IsLoadingAudioSourceReady = false;

        } //END GetLoadingAudioFailed

        //--------------------------------------//
        private void SetupLoadCompleteAudio()
        //--------------------------------------//
        {

            if( PlayAudioOnLoadComplete() )
            {
                IsLoadCompleteAudioSourceReady = false;

                if( IsLoadCompleteAudioClip() && loadCompleteAudioSource != null && loadCompleteAudioClip != null )
                {
                    loadCompleteAudioSource.clip = loadCompleteAudioClip;
                    IsLoadCompleteAudioSourceReady = true;
                }
                else if( IsLoadCompleteAudioPath() && !string.IsNullOrEmpty( loadCompleteAudioPath ) )
                {
                    WWWHelper.instance.GetAudioClip( loadCompleteAudioPath, cacheLoadCompleteAudioIfWeb, GetLoadCompleteAudioSuccess, GetLoadCompleteAudioClipFailed );
                }
            }

        } //END SetupLoadCompleteAudio

        //--------------------------------------//
        private void GetLoadCompleteAudioSuccess( AudioClip clip )
        //--------------------------------------//
        {

            loadCompleteAudioClip = clip;
            loadCompleteAudioSource.clip = clip;
            IsLoadCompleteAudioSourceReady = true;

        } //END GetLoadCompleteAudioSuccess

        //--------------------------------------//
        private void GetLoadCompleteAudioClipFailed()
        //--------------------------------------//
        {

            loadCompleteAudioClip = null;
            loadCompleteAudioSource.clip = null;
            IsLoadCompleteAudioSourceReady = false;

        } //END GetLoadCompleteAudioClipFailed

        //--------------------------------------//
        private void SetupErrorImage()
        //--------------------------------------//
        {

            if( OnError_UnderAnyScenarioChangeImage() )
            {
                errorImageIsReady = false;

                if( ChangeErrorImageUsingTexture() && errorTexture != null )
                {
                    errorImageIsReady = true;
                }
                else if( ChangeErrorImageUsingPath() && !string.IsNullOrEmpty( errorImagePath ) )
                {
                    WWWHelper.instance.GetTexture( errorImagePath, cacheErrorImageIfWeb, GetErrorImageSuccess, GetErrorImageFailed );
                }
            }

        } //END SetupErrorImage

        //--------------------------------------//
        private void GetErrorImageSuccess( Texture texture )
        //--------------------------------------//
        {

            errorTexture = texture;
            errorImageIsReady = true;

        } //END GetErrorImageSuccess

        //--------------------------------------//
        private void GetErrorImageFailed()
        //--------------------------------------//
        {
            
            errorTexture = null;
            errorImageIsReady = false;

        } //END GetErrorImageFailed

        //--------------------------------------//
        private void SetupErrorAudio()
        //--------------------------------------//
        {

            if( OnErrorPlayAudio_PlayAudio() )
            {
                IsErrorAudioSourceReady = false;

                if( IsErrorAudioClip() && errorAudioSource != null && errorAudioClip != null )
                {
                    errorAudioSource.clip = errorAudioClip;
                    IsErrorAudioSourceReady = true;
                }
                else if( IsErrorAudioPath() && !string.IsNullOrEmpty( loadCompleteAudioPath ) )
                {
                    WWWHelper.instance.GetAudioClip( errorAudioPath, cacheErrorAudioIfWeb, GetErrorAudioSuccess, GetErrorAudioClipFailed );
                }
            }

        } //END SetupErrorAudio

        //--------------------------------------//
        private void GetErrorAudioSuccess( AudioClip clip )
        //--------------------------------------//
        {

            errorAudioSource.clip = clip;
            errorAudioClip = clip;
            IsErrorAudioSourceReady = true;

        } //END GetLoadCompleteAudioSuccess

        //--------------------------------------//
        private void GetErrorAudioClipFailed()
        //--------------------------------------//
        {

            errorAudioSource.clip = null;
            errorAudioClip = null;
            IsErrorAudioSourceReady = false;

        } //END GetErrorAudioClipFailed

        //--------------------------------------//
        private void SetShowAndHideAnimationSettings()
        //--------------------------------------//
        {

            //We cannot play video on a sphere and play Show/Hide animations via a Scale Tween, it has to be via a Color Tween
            if( IsPlayVideoOnSphere() && ShowAndHideViaScale() )
            {
                showAndHideAnimationSettings = ShowAndHideAnimationSettings.FadeInAndOut;
            }

            if( uiColorTweenManager_Plane != null && uiScaleTweenManager_Video != null )
            {
                if( ShowAndHideViaFade() )
                {
                    if( IsPlayVideoOnPlane() )
                    {
                        uiColorTweenManager_Plane.SetTweenSpeed( fadeInSpeed, UITweener.TweenValue.Show );
                        uiColorTweenManager_Plane.SetTweenSpeed( fadeOutSpeed, UITweener.TweenValue.Hide );
                        uiColorTweenManager_Plane.SetDelay( fadeInDelay, UITweener.TweenValue.Show );
                        uiColorTweenManager_Plane.SetDelay( fadeOutDelay, UITweener.TweenValue.Hide );
                        uiColorTweenManager_Plane.SetEaseType( showAndHideTweenEaseType, UITweener.TweenValue.Show );
                        uiColorTweenManager_Plane.SetEaseType( showAndHideTweenEaseType, UITweener.TweenValue.Hide );

                        uiColorTweenManager_Plane.SetColor( onShowTweenColor, UITweener.TweenValue.Show );
                        uiColorTweenManager_Plane.SetColor( onHideTweenColor, UITweener.TweenValue.Hide );
                    }
                    else if( IsPlayVideoOnSphere() )
                    {
                        uiColorTweenManager_Sphere.SetTweenSpeed( fadeInSpeed, UITweener.TweenValue.Show );
                        uiColorTweenManager_Sphere.SetTweenSpeed( fadeOutSpeed, UITweener.TweenValue.Hide );
                        uiColorTweenManager_Sphere.SetDelay( fadeInDelay, UITweener.TweenValue.Show );
                        uiColorTweenManager_Sphere.SetDelay( fadeOutDelay, UITweener.TweenValue.Hide );
                        uiColorTweenManager_Sphere.SetEaseType( showAndHideTweenEaseType, UITweener.TweenValue.Show );
                        uiColorTweenManager_Sphere.SetEaseType( showAndHideTweenEaseType, UITweener.TweenValue.Hide );

                        uiColorTweenManager_Sphere.SetColor( onShowTweenColor, UITweener.TweenValue.Show );
                        uiColorTweenManager_Sphere.SetColor( onHideTweenColor, UITweener.TweenValue.Hide );
                    }
                }
                else if( ShowAndHideViaScale() )
                {
                    uiScaleTweenManager_Video.SetTweenSpeed( fadeInSpeed, UITweener.TweenValue.Show );
                    uiScaleTweenManager_Video.SetTweenSpeed( fadeOutSpeed, UITweener.TweenValue.Hide );
                    uiScaleTweenManager_Video.SetDelay( fadeInDelay, UITweener.TweenValue.Show );
                    uiScaleTweenManager_Video.SetDelay( fadeOutDelay, UITweener.TweenValue.Hide );
                    uiScaleTweenManager_Video.SetEaseType( showAndHideTweenEaseType, UITweener.TweenValue.Show );
                    uiScaleTweenManager_Video.SetEaseType( showAndHideTweenEaseType, UITweener.TweenValue.Hide );

                    uiScaleTweenManager_Video.SetScale( onShowTweenScale, UITweener.TweenValue.Show );
                    uiScaleTweenManager_Video.SetScale( onHideTweenScale, UITweener.TweenValue.Hide );
                }
            }

        } //END SetShowAndHideAnimationSettings

        //--------------------------------------//
        private void AddDelegates()
        //--------------------------------------//
        {

            if( IsVideoPlayerTypeUnity() && videoPlayer != null )
            {
                videoPlayer.prepareCompleted += PrepareCompleted;
                videoPlayer.started += Started;
                videoPlayer.loopPointReached += LoopPointReached;
                videoPlayer.seekCompleted += SeekCompleted;
                videoPlayer.errorReceived += ErrorRecieved;
                videoPlayer.frameDropped += FrameDropped;

                audioFromVideoPlayer.prepareCompleted += PrepareCompleted;
            }

        } //END AddDelegates

        //--------------------------------------//
        private void RemoveDelegates()
        //--------------------------------------//
        {

            if( IsVideoPlayerTypeUnity() && videoPlayer != null )
            {
                videoPlayer.prepareCompleted -= PrepareCompleted;
                videoPlayer.started -= Started;
                videoPlayer.loopPointReached -= LoopPointReached;
                videoPlayer.seekCompleted -= SeekCompleted;
                videoPlayer.errorReceived -= ErrorRecieved;
                videoPlayer.frameDropped -= FrameDropped;

                audioFromVideoPlayer.prepareCompleted -= PrepareCompleted;
            }

        } //END RemoveDelegates
        
        //--------------------------------------//
        private void Started( VideoPlayer vPlayer )
        //--------------------------------------//
        {
            if( showDebug ) Debug.Log( "BlockVideo.cs Started()" );

            if( IsVideoPlayerTypeUnity() && onStarted != null && onStarted.GetPersistentEventCount() > 0 )
            {
                if( showDebug ) Debug.Log( "BlockVideo.cs Started() calling onStarted = " + onStarted );
                onStarted.Invoke();
            }

        } //END Started

        //--------------------------------------//
        private void LoopPointReached( VideoPlayer vPlayer )
        //--------------------------------------//
        {
            if( showDebug ) Debug.Log( "BlockVideo.cs LoopPointReached()" );

            if( !WhenFinishedLoop() )
            {
                EndPointReached( vPlayer );
            }

            if( IsVideoPlayerTypeUnity() && onLoopPointReached != null && onLoopPointReached.GetPersistentEventCount() > 0 )
            {
                if( showDebug ) Debug.Log( "BlockVideo.cs LoopPointReached() calling onLoopPointReached = " + onLoopPointReached );
                onLoopPointReached.Invoke();
            }

        } //END LoopPointReached

        //--------------------------------------//
        private void EndPointReached( VideoPlayer vPlayer )
        //--------------------------------------//
        {

            if( showDebug ) Debug.Log( "BlockVideo.cs EndPointReached()" );

            isPlaying = false;

            if( WhenFinishedHideVideo() )
            {
                Hide();
            }
            else if( WhenFinishedCloseBlock() && blockGroup != null )
            {
                blockGroup.SendCommand( CommandType.Hide );
                blockGroup.SendCommand( CommandType.Show, new List<BlockType>() { BlockType.Button } );
            }

            if( HideBlocksDuringPlayback )
            {
                if( blockGroup != null ) { blockGroup.SendCommand( CommandType.Hide ); }
                if( BlockManager.instance != null ) { BlockManager.instance.SendCommand( CommandType.Show, new List<BlockType>() { BlockType.Button } ); }
            }

            if( IsVideoPlayerTypeUnity() && onEndPointReached != null && onEndPointReached.GetPersistentEventCount() > 0 )
            {
                if( showDebug ) Debug.Log( "BlockVideo.cs EndPointReached() calling onEndPointReached = " + onEndPointReached );
                onEndPointReached.Invoke();
            }

        } //END EndPointReached

        //--------------------------------------//
        private void SeekCompleted( VideoPlayer vPlayer )
        //--------------------------------------//
        {
            if( showDebug ) Debug.Log( "BlockVideo.cs SeekCompleted()" );

            if( IsVideoPlayerTypeUnity() && onSeekCompleted != null && onSeekCompleted.GetPersistentEventCount() > 0 )
            {
                if( showDebug ) Debug.Log( "BlockVideo.cs SeekCompleted() calling onSeekCompleted = " + onSeekCompleted );
                onSeekCompleted.Invoke();
            }

        } //END SeekCompleted

        //--------------------------------------//
        private void ErrorRecieved( VideoPlayer vPlayer, string error )
        //--------------------------------------//
        {
            if( showDebug ) Debug.Log( "BlockVideo.cs ErrorRecieved() error = " + error );
            
            bool fatalError = false;

            //If this is the Unity video player, we try to load from StreamingAssets, and PersistentDataPath AFTER we've checked the initial path
            if( IsVideoLocationPath() && error.Contains( "Can't play movie" ))
            {
                SetVideoClipPathToNextLocationOrFileType();
                if( currentLoadingAttempt != OnErrorTryPath.FinishedAllChecks )
                {
                    fatalError = false;
                    LoadVideo();
                }
                else
                {
                    fatalError = true;
                    if( showDebug ) { Debug.Log( "BlockVideo.cs ErrorRecieved(), We've tried playing from every Path and FileType we can, we cannot locate the asset" ); }
                }
            }
            else if( !error.Contains( "Can't Play Movie" ) )
            {
                fatalError = true;
                if( showDebug ) { Debug.Log( "BlockVideo.cs ErrorRecieved(), A fatal error occured. Unity Video Player Error = " + error ); }
            }

            //An error occured that's beyond just not being able to locate the asset (or we've tried all the possible location possibilities)
            if( fatalError )
            {
                bool allowRetry = false;

                //Check if we're allowed to retry
                if( OnError_TryAgain() )
                {
                    //Check if there is a retry limit, and if we've reached it...
                    if( retryCounter < retryLimit )
                    {
                        allowRetry = true;
                        retryCounter++;
                    }
                    else if( retryCounter >= retryLimit )
                    {
                        allowRetry = false;
                    }
                }
                
                //So if we're allowed to retry, then lets do that
                if( allowRetry )
                {
                    currentLoadingAttempt = OnErrorTryPath.OriginalPath;
                    currentFileTypeAttempt = OnErrorTryFileType.OriginalFileType;
                    
                    RetryLoadVideoClip();

                    //Whenever we retry we call the onRetryDueToError event
                    if( onRetryDueToError != null && onRetryDueToError.GetPersistentEventCount() > 0 )
                    {
                        if( showDebug ) Debug.Log( "BlockVideo.cs ErrorRecieved() calling onRetryDueToError = " + onRetryDueToError );
                        onRetryDueToError.Invoke();
                    }
                }
                //If we're not allowed to retry (or we've exhausted our retry limit), and if this has never been called before for this fatalError (just in case, prevents Unity errors from calling this event too many times)
                else if( !hasFatalErrorLogicBeenCalled )
                {
                    hasFatalErrorLogicBeenCalled = true;

                    StopLoadingAudioSource();
                    StopLoadingAnimation();

                    //Check if we should set the Video Renderer to different Image
                    if( OnError_UnderAnyScenarioChangeImage() && errorImageIsReady && errorTexture != null )
                    {
                        //Debug.Log( "Fatal Error with no more retry's, changing texture to ErrorImage" );
                        SetRendererToErrorImage();
                    }

                    //Check if we should play an AudioClip when a fatal error occurs
                    if( OnErrorPlayAudio_PlayAudio() && IsErrorAudioSourceReady && errorAudioSource != null && errorAudioSource.clip != null )
                    {
                        //If we're set to only show if a BlockButton has been activated, then make sure that BlockButton has been activated
                        if( ShowAndHideSetToAlwaysShow() ) { errorAudioSource.Play(); }
                        else if( ShowAndHideSetToBlockButton() && blockGroup != null && BlockFocusManager.instance.IsFocused( blockGroup ) ) { errorAudioSource.Play(); }

                        //Debug.Log( "Fatal error with no more retry's.. counter = " + retryCounter + ", limit = " + retryLimit );
                        
                    }

                    //Only when a fatal error occurs and we cannot retry any longer do we call the onErrorRecieved Event
                    if( onFatalPlaybackError != null && onFatalPlaybackError.GetPersistentEventCount() > 0 )
                    {
                        if( showDebug ) Debug.Log( "BlockVideo.cs ErrorRecieved() calling onFatalPlaybackError = " + onFatalPlaybackError );
                        onFatalPlaybackError.Invoke();
                    }
                }
                else
                {
                    if( showDebug ) Debug.Log( "BlockVideo.cs ErrorRecieved() inside of if( fatalError ), got to else... counter = " + retryCounter + ", limit = " + retryLimit );
                }
                
            }
            
        } //END ErrorRecieved

        //--------------------------------------//
        private void SetRendererToErrorImage()
        //--------------------------------------//
        {

            //Debug.Log( "BlockVideo.cs SetRendererToErrorImage() ... hasFatalErrorLogicBeenCalled = " + hasFatalErrorLogicBeenCalled );

            if( IsPlayVideoOnRenderer() )
            {
                if( videoRenderer1 != null && videoRenderer1.sharedMaterial != null ) { videoRenderer1.sharedMaterial.mainTexture = errorTexture; }

                if( ( Is3DSideBySide() || Is3DTopBottom() || IsCameraUsingVRMode() ) && videoRenderer2 != null && videoRenderer2.sharedMaterial != null )
                {
                    videoRenderer2.sharedMaterial.mainTexture = errorTexture;
                }
            }
            else if( IsPlayVideoOnPlane() )
            {
                if( videoPlane1 != null && videoPlane1.sharedMaterial != null ) { videoPlane1.sharedMaterial.mainTexture = errorTexture; }
                if( videoPlane2 != null && videoPlane2.sharedMaterial != null ) { videoPlane2.sharedMaterial.mainTexture = errorTexture; }
            }
            else if( IsPlayVideoOnSphere() )
            {
                if( videoSphere1 != null && videoSphere1.sharedMaterial != null ) { videoSphere1.sharedMaterial.mainTexture = errorTexture; }
                if( videoSphere2 != null && videoSphere2.sharedMaterial != null ) { videoSphere2.sharedMaterial.mainTexture = errorTexture; }
            }
            else if( IsPlayVideoOnFullscreen() )
            {
                if( videoFullscreen1 != null && videoFullscreen1.material.mainTexture != null ) { videoFullscreen1.texture = errorTexture; }
                if( videoFullscreen2 != null && videoFullscreen2.material.mainTexture != null ) { videoFullscreen2.texture = errorTexture; }
            }

        } //END SetRendererToErrorImage

        //--------------------------------------//
        private void SetVideoClipPathToNextLocationOrFileType()
        //--------------------------------------//
        {

            //If we just tried loading the original VideoClip path, then start checking through StreamingAssets
            if( currentLoadingAttempt == OnErrorTryPath.OriginalPath )
            {
                currentLoadingAttempt = OnErrorTryPath.StreamingAssets;
                currentFileTypeAttempt = OnErrorTryFileType.mp4;
            }

            
            else if( currentLoadingAttempt == OnErrorTryPath.StreamingAssets )
            {
                //We finished checking StreamingAssets for the MP4 file type, let's try the other file types every time we fail
                if     ( currentFileTypeAttempt == OnErrorTryFileType.mp4 ) { currentFileTypeAttempt = OnErrorTryFileType.m4v; }
                else if( currentFileTypeAttempt == OnErrorTryFileType.m4v ) { currentFileTypeAttempt = OnErrorTryFileType.webm; }
                else if( currentFileTypeAttempt == OnErrorTryFileType.webm ) { currentFileTypeAttempt = OnErrorTryFileType.ogg; }
                else if( currentFileTypeAttempt == OnErrorTryFileType.ogg ) { currentFileTypeAttempt = OnErrorTryFileType.avi; }
                else if( currentFileTypeAttempt == OnErrorTryFileType.avi )
                {
                    //That's the last of the File type checks for StreamingAssets, let's move onto checking Resources path
                    currentLoadingAttempt = OnErrorTryPath.Resources;
                    currentFileTypeAttempt = OnErrorTryFileType.OriginalFileType; //Resources doesn't use File Types, just the name of the file
                }
            }

            //We checked through Resources, let's go through the PersistentDataPath next
            else if( currentLoadingAttempt == OnErrorTryPath.Resources )
            {
                currentLoadingAttempt = OnErrorTryPath.PersistentDataPath;
                currentFileTypeAttempt = OnErrorTryFileType.mp4; //PersistentDataPath uses file types for it's loading
            }

            else if( currentLoadingAttempt == OnErrorTryPath.PersistentDataPath )
            {
                //We finished checking PersistentDataPath for the MP4 file type, let's try the other file types every time we fail
                if( currentFileTypeAttempt == OnErrorTryFileType.mp4 ) { currentFileTypeAttempt = OnErrorTryFileType.m4v; }
                else if( currentFileTypeAttempt == OnErrorTryFileType.m4v ) { currentFileTypeAttempt = OnErrorTryFileType.webm; }
                else if( currentFileTypeAttempt == OnErrorTryFileType.webm ) { currentFileTypeAttempt = OnErrorTryFileType.ogg; }
                else if( currentFileTypeAttempt == OnErrorTryFileType.ogg ) { currentFileTypeAttempt = OnErrorTryFileType.avi; }
                else if( currentFileTypeAttempt == OnErrorTryFileType.avi )
                {
                    //We've finished all of our checks, we can be pretty sure that the file doesn't exist!
                    currentLoadingAttempt = OnErrorTryPath.FinishedAllChecks;
                    currentFileTypeAttempt = OnErrorTryFileType.FinishedAllChecks;
                }
            }

        } //END SetVideoClipPathToNextLocationOrFileType

        //--------------------------------------//
        private void FrameDropped( VideoPlayer vPlayer )
        //--------------------------------------//
        {
            if( showDebug ) Debug.Log( "BlockVideo.cs FrameDropped()" );

            if( IsVideoPlayerTypeUnity() && onFrameDropped != null && onFrameDropped.GetPersistentEventCount() > 0 )
            {
                if( showDebug ) Debug.Log( "BlockVideo.cs FrameDropped() calling onFrameDropped = " + onFrameDropped );
                onFrameDropped.Invoke();
            }

        } //END FrameDropped

        //--------------------------------------//
        public void LoadVideoAtStart()
        //--------------------------------------//
        {

            if( loadOnStart )
            {
                currentLoadingAttempt = OnErrorTryPath.OriginalPath;
                currentFileTypeAttempt = OnErrorTryFileType.OriginalFileType;
                retryCounter = 1;
                hasFatalErrorLogicBeenCalled = false;

                LoadVideo();
            }

        } //END LoadVideoAtStart

        //--------------------------------------//
        public void LoadVideo( string path, bool playWhenReady )
        //--------------------------------------//
        {

            if( IsPlaying() ) { Pause(); }

            currentLoadingAttempt = OnErrorTryPath.OriginalPath;
            currentFileTypeAttempt = OnErrorTryFileType.OriginalFileType;
            retryCounter = 1;
            hasFatalErrorLogicBeenCalled = false;

            if( ShowAndHideSetToAlwaysShowOrAlwaysHide() )
            {
                this.playAfterLoadComplete = playWhenReady;
            }
            else if( ShowAndHideSetToBlockButton() )
            {
                this.playWhenBlockButtonSelected = playWhenReady;
            }
            
            videoLocation = VideoLocation.Path;

            VideoPath = path;

            LoadVideo();

        } //END LoadVideo

        //--------------------------------------//
        private void LoadVideo( VideoClip clip, bool playWhenReady )
        //--------------------------------------//
        {

            if( IsPlaying() ) { Pause(); }

            currentLoadingAttempt = OnErrorTryPath.OriginalPath;
            currentFileTypeAttempt = OnErrorTryFileType.OriginalFileType;
            retryCounter = 1;
            hasFatalErrorLogicBeenCalled = false;

            if( ShowAndHideSetToAlwaysShowOrAlwaysHide() )
            {
                this.playAfterLoadComplete = playWhenReady;
            }
            else if( ShowAndHideSetToBlockButton() )
            {
                this.playWhenBlockButtonSelected = playWhenReady;
            }

            videoLocation = VideoLocation.VideoClip;

            VideoClip = clip;

            LoadVideo();

        } //END LoadVideo

        //--------------------------------------//
        private void LoadVideo()
        //--------------------------------------//
        {
            //Debug.Log( "LoadVideo()" );
            
            if( IsVideoPlayerTypeUnity() && videoPlayer != null )
            {

                //When we start Loading the Video, we check if we should play a looping audioClip
                PlayLoadingAudioSource();

                isLoading = true;

                //If we're loading from a VideoClip, then this is simple
                if( IsVideoLocationVideoClip() && VideoClip != null )
                {
                    videoPlayer.source = VideoSource.VideoClip;
                    videoPlayer.clip = VideoClip;
                    SetVideoPlayerAudioSource();
                    SetLoadingTexture();
                    StartLoadingAnimation();
                    CheckIfVideoRendererShouldBeShownDuringLoading();

                    try { videoPlayer.Prepare(); } catch { ErrorRecieved( videoPlayer, "Unable To Play Video File" ); }
                }

                //If we're loading from Path, then things are more complicated
                //We check the initial path the user entered
                //If that doesn't work, then we try StreamingAssets, Resources, and PersistentDataPath in that order
                else if( IsVideoLocationPath() && !string.IsNullOrEmpty( VideoPath ) )
                {
                    if( showDebug && currentLoadingAttempt == OnErrorTryPath.OriginalPath ) { Debug.Log( "BlockVideo.cs LoadVideo() calling videoPlayer.Prepare() for url = " + VideoPath ); }

                    if( currentLoadingAttempt != OnErrorTryPath.Resources )
                    {
                        videoPlayer.source = VideoSource.Url;

                        //If the video url points to Youtube, we need to find the real URL for the video, and we also need to find the audio URL.
                        //These are both played on two different Unity Video Players
                        if( IsYoutubeVideoURL( VideoPath ) )
                        {
                            SetLoadingTexture();
                            StartLoadingAnimation();
                            CheckIfVideoRendererShouldBeShownDuringLoading();

                            //The Youtube URL is parsed by a seperate thread, we need to wait for that to complete before continuing
                            BeginParsingYoutubeVideoPath( VideoPath, youtubePreferredQuality, showDebug );
                        }
                        else
                        {
                            videoPlayer.url = ParseVideoPath();

                            SetVideoPlayerAudioSource();
                            SetLoadingTexture();
                            StartLoadingAnimation();
                            CheckIfVideoRendererShouldBeShownDuringLoading();

                            try { videoPlayer.Prepare(); } catch { ErrorRecieved( videoPlayer, "Unable To Play Video File" ); }
                        }
                    }

                    //If we're trying to load from resources (and we're successful), then we'll get a VideoClip when we're done, so we have to change some settings around (only if we find the file in Resources)
                    else if( currentLoadingAttempt == OnErrorTryPath.Resources )
                    {
                        if( Resources.Load<VideoClip>( ParseVideoPath() ) != null )
                        {
                            videoPlayer.source = VideoSource.VideoClip;
                            videoPlayer.clip = Resources.Load<VideoClip>( ParseVideoPath() );
                            SetVideoPlayerAudioSource();
                            SetLoadingTexture();
                            StartLoadingAnimation();
                            CheckIfVideoRendererShouldBeShownDuringLoading();

                            try { videoPlayer.Prepare(); } catch { ErrorRecieved( videoPlayer, "Unable To Play Video File" ); }
                        }

                    }
                }

            }
            
        } //END LoadVideo

        //--------------------------------------//
        private void RetryLoadVideoClip()
        //--------------------------------------//
        {

            if( IsVideoPlayerTypeUnity() && videoPlayer != null )
            {
                //We only try Youtube loading the first time, if we ever get to this function then our audio should be playing from the original video file
                if( IsVideoLocationPath() && IsYoutubeVideoURL( VideoPath ) )
                {
                    isAudioFromVideoLoading = false;
                    SetVideoPlayerAudioSource();
                }

                //When we start Loading the Video, we check if we should play a looping audioClip
                PlayLoadingAudioSource();

                isLoading = true;

                //If we're loading from a VideoClip, then this is simple
                if( IsVideoLocationVideoClip() && VideoClip != null )
                {
                    try { videoPlayer.Prepare(); } catch { ErrorRecieved( videoPlayer, "Unable To Play Video File" ); }
                }

                //If we're loading from Path, then things are more complicated
                //We check the initial path the user entered
                //If that doesn't work, then we try StreamingAssets, Resources, and PersistentDataPath in that order
                else if( IsVideoLocationPath() && !string.IsNullOrEmpty( VideoPath ) )
                {
                    
                    if( currentLoadingAttempt != OnErrorTryPath.Resources )
                    {
                        videoPlayer.source = VideoSource.Url;
                        videoPlayer.url = ParseVideoPath();

                        try { videoPlayer.Prepare(); } catch { ErrorRecieved( videoPlayer, "Unable To Play Video File" ); }
                    }

                    //If we're trying to load from resources (and we're successful), then we'll get a VideoClip when we're done, so we have to change some settings around (only if we find the file in Resources)
                    else if( currentLoadingAttempt == OnErrorTryPath.Resources )
                    {
                        if( Resources.Load<VideoClip>( ParseVideoPath() ) != null )
                        {
                            videoPlayer.source = VideoSource.VideoClip;
                            videoPlayer.clip = Resources.Load<VideoClip>( ParseVideoPath() );

                            try { videoPlayer.Prepare(); } catch { ErrorRecieved( videoPlayer, "Unable To Play Video File" ); }
                        }

                    }
                }

            }

        } //END RetryLoadVideoClip

        //--------------------------------------//
        private void PlayLoadingAudioSource()
        //--------------------------------------//
        {

            if( LoadingAudio_Play() && IsLoadingAudioSourceReady && loadingAudioSource != null )
            {
                loadingAudioSource.Play();
            }

        } //END PlayLoadingAudioSource

        //--------------------------------------//
        private void StopLoadingAudioSource()
        //--------------------------------------//
        {

            if( LoadingAudio_Play() && IsLoadingAudioSourceReady && loadingAudioSource != null )
            {
                if( OnLoadCompleteStopAudio() )
                {
                    loadingAudioSource.Stop();
                }
                else if( OnLoadCompleteFadeAudio() )
                {
                    AudioHelper.instance.Fade( loadingAudioSource, 0f, .5f, 0f );
                }
            }

        } //END StopLoadingAudioSource

        //--------------------------------------//
        private void PlayLoadCompleteAudioClip()
        //--------------------------------------//
        {

            if( PlayAudioOnLoadComplete() && IsLoadCompleteAudioSourceReady && loadCompleteAudioSource != null && loadCompleteAudioSource.clip != null )
            {
                loadCompleteAudioSource.Play();
            }

        } //END PlayLoadCompleteAudioClip

        //--------------------------------------//
        private string ParseVideoPath()
        //--------------------------------------//
        {

            if( currentLoadingAttempt == OnErrorTryPath.StreamingAssets )
            {
                return DatabaseStringHelper.CreateStreamingAssetsPath( VideoPath + "." + currentFileTypeAttempt.ToString(), DatabaseStringHelper.StringStyle.NoSettings );
            }
            else if( currentLoadingAttempt == OnErrorTryPath.Resources )
            {
                return System.IO.Path.ChangeExtension( VideoPath, null );
            }
            else if( currentLoadingAttempt == OnErrorTryPath.PersistentDataPath )
            {
                return DatabaseStringHelper.CreatePersistentDataPath( VideoPath + "." + currentFileTypeAttempt.ToString() );
            }

            return VideoPath;

        } //END ParseVideoPath
        
        //--------------------------------------//
        private void BeginParsingYoutubeVideoPath( string path, YoutubeQuality quality, bool showDebug )
        //--------------------------------------//
        {

            //If the user did not mark this as a youtube video...
            if( !IsYoutubeVideo() && !path.Contains( "youtube" ) )
            {
                if( showDebug ) { Debug.Log( "BlockVideo.cs BeginParsingYoutubeVideoPath() video player does not have isYoutubeVideo set to true, so we're calling ParseYoutubeVideoPathFailed()" ); }
                ParseYoutubeVideoPathFailed();
            }
            
            //Otherwise, let's try to parse this as a youtube video
            else if( IsYoutubeVideo() || path.Contains("youtube") )
            {
                if( showDebug ) { Debug.Log( "BlockVideo.cs BeginParsingYoutubeVideoPath() about to call youtubeFinder.RequestVideoURL()" ); }
                youtubeFinder.RequestURL( path, GetYouTubeQuality( quality ), showDebug, ParseYoutubeVideoPathSuccess, ParseYoutubeVideoPathFailed );
            }
            
        } //END BeginParsingYoutubeVideoPath

        //-------------------------------------//
        private string GetYouTubeQuality( YoutubeQuality quality )
        //-------------------------------------//
        {

            if( quality == YoutubeQuality.Best ) { return "Best Quality"; }
            else if( quality == YoutubeQuality._144 ) { return "144"; }
            else if( quality == YoutubeQuality._240 ) { return "240"; }
            else if( quality == YoutubeQuality._360 ) { return "360"; }
            else if( quality == YoutubeQuality._480 ) { return "480"; }
            else if( quality == YoutubeQuality._640 ) { return "640"; }
            else if( quality == YoutubeQuality._720 ) { return "720"; }
            else if( quality == YoutubeQuality._1080 ) { return "1080"; }
            else if( quality == YoutubeQuality._1440 ) { return "1440"; }
            else if( quality == YoutubeQuality._2160 ) { return "2160"; }
            else if( quality == YoutubeQuality._4320 ) { return "4320"; }

            return "1080";
        } //END GetYouTubeQuality

        //--------------------------------------//
        private void ParseYoutubeVideoPathSuccess()
        //--------------------------------------//
        {
            if( showDebug ) { Debug.Log( "BlockVideo.cs ParseYoutubeVideoPathSuccess()" ); }

            //At this point, the youtube video and audio is ready to be prepared()
            videoPlayer.url = youtubeFinder.GetVideoURL();
            
            isAudioFromVideoLoaded = false;
            
            //If the audioFromVideoPlayer URL is empty, then try to play audio from the video player stream
            if( string.IsNullOrEmpty( youtubeFinder.GetAudioURL() ) )
            {
                isAudioFromVideoLoading = false;

                useAudioFromVideo = false;
                SetVideoPlayerAudioSource();
            }
            else //Play audio on a 2nd video player
            {
                isAudioFromVideoLoading = true;

                audioFromVideoPlayer.url = youtubeFinder.GetAudioURL();
                useAudioFromVideo = true;
                SetAudioFromVideoPlayerAudioSource();
                audioFromVideoPlayer.Prepare();
            }

            if( showDebug ) { Debug.Log( "BlockVideo.cs ParseYoutubeVideoPathSuccess() videoURL( " + videoPlayer.url + " ), audioURL( " + audioFromVideoPlayer.url + " )" ); }

            try { videoPlayer.Prepare(); } catch { ErrorRecieved( videoPlayer, "Unable To Play Video File" ); }

        } //END ParseYoutubeVideoPathSuccess

        //--------------------------------------//
        private void ParseYoutubeVideoPathFailed()
        //--------------------------------------//
        {

            if( showDebug ) { Debug.Log( "BlockVideo.cs ParseYoutubeVideoPathFailed() calling RetryLoadVideoClip()" ); }

            RetryLoadVideoClip();

        } //END ParseYoutubeVideoPathFailed

        //--------------------------------------//
        private bool IsYoutubeVideoURL( string url )
        //--------------------------------------//
        {

            //Check if this is a Youtube video
            if( IsYoutubeVideo() ) //&& ( url.Contains( "youtube" ) || url.Contains( "youtu.be" ) )
            {
                return true;
            }

            return false;

        } //END IsYoutubeVideoURL

        //--------------------------------------//
        private void SetLoadingTexture()
        //--------------------------------------//
        {

            if( Loading_ChangeImageOrChangeImageAndShowLoadingAnimation() && loadingImageIsReady && loadingTexture != null && !hasFatalErrorLogicBeenCalled )
            {
                
                if( IsPlayVideoOnRenderer() )
                {
                    if( videoRenderer1 != null && videoRenderer1.sharedMaterial != null ) { videoRenderer1.sharedMaterial.mainTexture = loadingTexture; }

                    if( Is3DTopBottom() || Is3DSideBySide() || IsCameraUsingVRMode() )
                    {
                        if( videoRenderer2 != null && videoRenderer2.sharedMaterial != null ) { videoRenderer2.sharedMaterial.mainTexture = loadingTexture; }
                    }
                }
                else if( IsPlayVideoOnPlane() )
                {
                    if( videoPlane1 != null && videoPlane1.sharedMaterial != null ) { videoPlane1.sharedMaterial.mainTexture = loadingTexture; }
                    if( videoPlane2 != null && videoPlane2.sharedMaterial != null ) { videoPlane2.sharedMaterial.mainTexture = loadingTexture; }
                }
                else if( IsPlayVideoOnSphere() )
                {
                    if( videoSphere1 != null && videoSphere1.sharedMaterial != null ) { videoSphere1.sharedMaterial.mainTexture = loadingTexture; }
                    if( videoSphere2 != null && videoSphere2.sharedMaterial != null ) { videoSphere2.sharedMaterial.mainTexture = loadingTexture; }
                }
                else if( IsPlayVideoOnFullscreen() )
                {
                    if( videoFullscreen1 != null && videoFullscreen1.material != null ) { videoFullscreen1.texture = loadingTexture; }
                    if( videoFullscreen2 != null && videoFullscreen2.material != null ) { videoFullscreen2.texture = loadingTexture; }
                }
            }

        } //END SetLoadingTexture

        //--------------------------------------//
        private void StartLoadingAnimation()
        //--------------------------------------//
        {

            if( loadingAnimationIsReady && coroutine_LoadAnimation == null && !hasFatalErrorLogicBeenCalled )
            {
                bool shouldShow = false;

                //If we're set to show the video when the BlockButton is selected, then we need to make sure that the block is selected to show the animation, otherwise show the animation regardless
                if( ShowAndHideSetToBlockButton() && blockGroup != null && BlockFocusManager.instance.IsFocused( blockGroup ) )
                {
                    shouldShow = true;
                }
                else if( ShowAndHideSetToBlockButton() && blockGroup == null ) { shouldShow = false; }
                else if( ShowAndHideSetToBlockButton() && blockGroup != null && !BlockFocusManager.instance.IsFocused( blockGroup ) ) { shouldShow = false; }
                else
                {
                    shouldShow = true;
                }

                if( shouldShow )
                {
                    //Show the Loading Animation
                    if( loadingAnimationMaterial != null ) { loadingAnimationMaterial.color = new Color( loadingAnimationMaterial.color.r, loadingAnimationMaterial.color.g, loadingAnimationMaterial.color.b, 1f ); }

                    SetLoadingAnimationTextureScale();

                    loadingAnimationIndex = 0;
                    coroutine_LoadAnimation = StartCoroutine( UpdateLoadingAnimationTiling() );

                    if( LoadingAnimation_Rotate() ) { coroutine_RotateLoadAnimation = StartCoroutine( UpdateLoadingAnimationRotation() ); }
                }
                
            }

        } //END StartLoadingAnimation
        
        //--------------------------------------//
        private void SetLoadingAnimationTextureScale()
        //--------------------------------------//
        {
            
            if( !Application.isEditor || ( Application.isEditor && Application.isPlaying ) || ( Application.isEditor && !Application.isPlaying && debugShowAnimationInEditor ) )
            {
                //Set the tile size of the texture (in UV units), based on the rows and columns
                Vector2 size = new Vector2( 1f / columns, 1f / rows );

                if( loadingAnimationMaterial != null ) { loadingAnimationMaterial.SetTextureScale( "_MainTex", size ); }

            }
            
        } //END SetLoadingAnimationTextureScale

        //--------------------------------------//
        private void SetLoadingAnimationTextureInitialTiling()
        //--------------------------------------//
        {

            if( !Application.isEditor || ( Application.isEditor && Application.isPlaying ) || ( Application.isEditor && !Application.isPlaying && debugShowAnimationInEditor ) )
            {
                //Debug.Log( "Tiling" );
                loadingAnimationIndex = 0;

                //split into x and y indexes
                Vector2 offset = new Vector2( (float)loadingAnimationIndex / columns - ( loadingAnimationIndex / columns ), //x index
                                              ( loadingAnimationIndex / columns ) / (float)rows );          //y index

                if( loadingAnimationMaterial != null ) { loadingAnimationMaterial.SetTextureOffset( "_MainTex", offset ); }

            }

        } //END SetLoadingAnimationTextureInitialTiling

        //--------------------------------------//
        private IEnumerator UpdateLoadingAnimationTiling()
        //--------------------------------------//
        {

            while( true )
            {
                //move to the next index
                loadingAnimationIndex++;

                if( loadingAnimationIndex >= rows * columns )
                    loadingAnimationIndex = 0;

                //split into x and y indexes
                Vector2 offset = new Vector2( (float)loadingAnimationIndex / columns - ( loadingAnimationIndex / columns ), //x index
                                              ( loadingAnimationIndex / columns ) / (float)rows );          //y index

                if( loadingAnimationMaterial != null ) { loadingAnimationMaterial.SetTextureOffset( "_MainTex", offset ); }

                yield return new WaitForSeconds( 1f / framesPerSecond );
            }

        } //END UpdateLoadingAnimationTiling

        //--------------------------------------//
        private IEnumerator UpdateLoadingAnimationRotation()
        //--------------------------------------//
        {
            
            Vector3 direction = Vector3.back;

            while( true )
            {
                direction = Vector3.back;
                if( LoadingAnimation_RotateLeft() ) { direction = Vector3.forward; }

                if( IsPlayVideoOnPlaneOrRenderer() )
                {
                    rawImage_LoadingAnimation_Plane.transform.Rotate( direction, rotationSpeed * Time.deltaTime );
                }
                else if( IsPlayVideoOnPlaneOrSphere() )
                {
                    if( ShowLoadingAnimationOnFrontFace() ) { rawImage_LoadingAnimation_Sphere_Front.transform.Rotate ( direction, rotationSpeed * Time.deltaTime ); }
                    if( ShowLoadingAnimationOnBackFace() )  { rawImage_LoadingAnimation_Sphere_Back.transform.Rotate  ( direction, rotationSpeed * Time.deltaTime ); }
                    if( ShowLoadingAnimationOnLeftFace() )  { rawImage_LoadingAnimation_Sphere_Left.transform.Rotate  ( direction, rotationSpeed * Time.deltaTime ); }
                    if( ShowLoadingAnimationOnRightFace() ) { rawImage_LoadingAnimation_Sphere_Right.transform.Rotate ( direction, rotationSpeed * Time.deltaTime ); }
                    if( ShowLoadingAnimationOnTopFace() )   { rawImage_LoadingAnimation_Sphere_Top.transform.Rotate   ( direction, rotationSpeed * Time.deltaTime ); }
                    if( ShowLoadingAnimationOnBottomFace() ){ rawImage_LoadingAnimation_Sphere_Bottom.transform.Rotate( direction, rotationSpeed * Time.deltaTime ); }
                }
                else if( IsPlayVideoOnFullscreen() )
                {
                    rawImage_LoadingAnimation_Fullscreen.transform.Rotate( direction, rotationSpeed * Time.deltaTime );
                }

                yield return new WaitForEndOfFrame();
            }

        } //END UpdateLoadingAnimationRotation

        //--------------------------------------//
        private void CheckIfVideoRendererShouldBeShownDuringLoading()
        //--------------------------------------//
        {
            
            //Depending on the settings, we call Show() while loading the video
            if( ShowAndHideSetToAlwaysShow() ) { Show(); return; }
            else if( ShowAndHideSetToAlwaysHide() ) { Hide(); return; }
            else if( ShowAndHideSetToBlockButton() ) { return; }
            else
            {
                //If our LoadingScreen settings are set to change the image color or show a loading symbol, then we Show() the VideoRenderer
                if( Loading_ChangeImageOrChangeImageAndShowLoadingAnimation() ) { Show(); return; }
            }
            
        } //END CheckIfVideoRendererShouldBeShownDuringLoading

        //--------------------------------------//
        private void CheckIfVideoRendererShouldBeShownDuringPlayback()
        //--------------------------------------//
        {

            //Depending on the settings, we call Show() when loading has completed
            if( ShowAndHideSetToAlwaysHide() ) { Hide(); return; }
            else { Show(); return; }

        } //END CheckIfVideoRendererShouldBeShownDuringPlayback
        

        //--------------------------------------//
        private void PrepareCompleted( VideoPlayer vPlayer )
        //--------------------------------------//
        {

            if( vPlayer == videoPlayer )
            {
                if( showDebug ) Debug.Log( "BlockVideo.cs PrepareCompleted() videoPlayer prepare completed" );

                isLoading = false;
                isLoaded = true;
                
                //If we're loading from Youtube, we need to wait for the audio, but if both the youtube audio and video are ready then call FinalizePrepare() (which calls Play)
                if( IsVideoLocationPath() && IsYoutubeVideoURL( VideoPath ) )
                {
                    //If the audio has finished loading, or if we're not loading the audio seperately...
                    if( ( !isAudioFromVideoLoading && isAudioFromVideoLoaded ) )
                    {
                        if( showDebug ) Debug.Log( "BlockVideo.cs PrepareCompleted() videoPlayer and the audioFromVideoPlayer are both ready, calling FinalizePrepare()" );
                        FinalizePrepare();
                    }
                    else if( string.IsNullOrEmpty( audioFromVideoPlayer.url ) )
                    {
                        if( showDebug ) Debug.Log( "BlockVideo.cs PrepareCompleted() videoPlayer ready and we do not require audioFromVideoPlayer, calling FinalizePrepare()" );
                        FinalizePrepare();
                    }
                    else
                    {
                        if( showDebug ) Debug.Log( "BlockVideo.cs PrepareCompleted() videoPlayer ready but we need to wait for audioFromVideoPlayer... audioURL = " + audioFromVideoPlayer.url );
                    }
                }

                //We're not loading from Youtube (which requires a video and audio stream to be loaded), so we can move straight into calling FinalizePrepare()
                else
                {
                    if( showDebug ) Debug.Log( "BlockVideo.cs PrepareCompleted() we're not loading from Youtube, so we are ready to call FinalizePrepare()" );
                    FinalizePrepare();
                }
                
            }
            else
            {
                if( showDebug ) Debug.Log( "BlockVideo.cs PrepareCompleted() audioFromVideo prepare completed" );

                isAudioFromVideoLoading = false;
                isAudioFromVideoLoaded = true;

                //We're loading from Youtube and the audio is prepared, if the video is also ready then we'll call FinalizePrepare()
                if( !isLoading && isLoaded )
                {
                    FinalizePrepare();
                }
            }
            
        } //END PrepareCompleted

        //-----------------------------------//
        private void FinalizePrepare()
        //-----------------------------------//
        {

            StopLoadingAudioSource();
            PlayLoadCompleteAudioClip();

            StopLoadingAnimation();

            if( IsVideoPlayerTypeUnity() && onPrepareCompleted != null && onPrepareCompleted.GetPersistentEventCount() > 0 )
            {
                //if( ShowDebug ) Debug.Log( "BlockVideo.cs FinalizePrepare() calling onPrepareCompleted = " + onPrepareCompleted );
                onPrepareCompleted.Invoke();
            }

            if( ShowAndHideSetToBlockButton() && playWhenBlockButtonSelected && blockGroup != null && BlockFocusManager.instance.IsFocused( blockGroup ) )
            {
                if( showDebug ) { Debug.Log( "BlockVideo.cs FinalizePrepare() Set to show when BlockButton is activated AND playWhenBlockButtonSelected so we're calling Play()" ); }
                Play();
            }
            else if( ShowAndHideSetToAlwaysShowOrAlwaysHide() && playAfterLoadComplete )
            {
                if( showDebug ) { Debug.Log( "BlockVideo.cs FinalizePrepare() Set to Always Show or Hide AND playAfterLoadComplete so we're calling Play()" ); }
                Play();
            }

        } //END FinalizePrepare

        //-----------------------------------//
        private void StopLoadingAnimation()
        //-----------------------------------//
        {

            if( coroutine_LoadAnimation != null )
            {
                //Hide the Loading Animation
                if( loadingAnimationMaterial != null ) { loadingAnimationMaterial.color = new Color( loadingAnimationMaterial.color.r, loadingAnimationMaterial.color.g, loadingAnimationMaterial.color.b, 0f ); }
                
                StopCoroutine( coroutine_LoadAnimation );
                coroutine_LoadAnimation = null;
            }

            if( coroutine_RotateLoadAnimation != null )
            {
                StopCoroutine( coroutine_RotateLoadAnimation );
                coroutine_RotateLoadAnimation = null;
            }

        } //END StopLoadingAnimation

        //-----------------------------------//
        public void Play( string path )
        //-----------------------------------//
        {

            if( isPlaying ) { Stop(); }

            LoadVideo( path, true );

        } //END Play

        //-----------------------------------//
        public void Play( VideoClip clip )
        //-----------------------------------//
        {

            if( isPlaying ) { Stop(); }

            LoadVideo( clip, true );

        } //END Play
        

        //---------------------------------//
        private void Play()
        //---------------------------------//
        {

            //Video is loaded already
            if( !isLoading && isLoaded && !isPlaying )
            {
                if( IsVideoPlayerTypeUnity() )
                {
                    if( showDebug ) { Debug.Log( "BlockVideo.cs Play() ... the video has loaded! Calling videoPlayer.Play()" ); }

                    isPlaying = true;
                    videoPlayer.Play();
                    HookVideoPlayerToRenderer();
                    CheckIfVideoRendererShouldBeShownDuringPlayback();
                    SetPlaneOrFullscreenTransformAspectRatio();
                    SetFullscreenSidebarAspectRatio();

                    if( IsVideoLocationPath() && IsYoutubeVideoURL( VideoPath ) && !string.IsNullOrEmpty( audioFromVideoPlayer.url ) && ( !isAudioFromVideoLoading && isAudioFromVideoLoaded ) )
                    {
                        if( showDebug ) { Debug.Log( "BlockVideo.cs Play() ... the video has loaded! Calling videoPlayer.Play()" ); }
                        audioFromVideoPlayer.Play();
                    }
                }

                Play_HideBlocks();
            }

        } //END Play
        
        //---------------------------------//
        private void Play_HideBlocks()
        //---------------------------------//
        {

            //Check if we should Hide Blocks
            if( HideBlocksDuringPlayback )
            {
                if( BlockManager.instance != null )
                {
                    if( ShowAndHideSetToAlwaysShowOrAlwaysHide() )
                    {
                        if( blockGroup != null )
                        {
                            BlockManager.instance.SendCommand( CommandType.Hide, null, new List<BlockView>() { blockGroup.GetView() } );
                            blockGroup.SendCommand( CommandType.Hide, new List<BlockType>() { BlockType.Button }, null, true );
                        }
                        else
                        {
                            BlockManager.instance.SendCommand( CommandType.Hide );
                        }
                    }
                    else if( ShowAndHideSetToBlockButton() )
                    {
                        if( blockGroup != null )
                        {
                            BlockManager.instance.SendCommand( CommandType.Hide, null, new List<BlockView>() { blockGroup.GetView() } );
                        }
                        else
                        {
                            BlockManager.instance.SendCommand( CommandType.Hide );
                        }
                    }
                }
            }

        } //END Play_HideBlocks





        //--------------------------------//
        private void ShowOrHideBasedOnSettings()
        //--------------------------------//
        {

            if( ShowAndHideSetToAlwaysShow() )
            {
                ForceShow();
            }
            else if( ShowAndHideSetToAlwaysHide() )
            {
                ForceHide();
            }
            else if( ShowAndHideSetToBlockButton() )
            {
                ForceHide();
            }

        } //END ShowOrHideBasedOnSettings

        //--------------------------------//
        public override void Show()
        //--------------------------------//
        {
            base.Show();

            if( showDebug ) { Debug.Log( "BlockVideo.cs Show() scale = " + onShowTweenScale + ", speed = " + showAndHideTweenSpeed + ", delay = " + showAndHideTweenDelay + ", easeType = " + showAndHideTweenEaseType ); }

            _ShowOrHide( 1f, UITweener.TweenValue.Show, onShowTweenScale, showAndHideTweenSpeed, showAndHideTweenDelay, showAndHideTweenEaseType );
            
        } //END Show

        //--------------------------------//
        public override void Hide()
        //--------------------------------//
        {
            base.Hide();

            if( showDebug ) { Debug.Log( "BlockVideo.cs Hide() scale = " + onHideTweenScale + ", speed = " + showAndHideTweenSpeed + ", delay = " + showAndHideTweenDelay + ", easeType = " + showAndHideTweenEaseType ); }

            _ShowOrHide( 0f, UITweener.TweenValue.Hide, onHideTweenScale, showAndHideTweenSpeed, showAndHideTweenDelay, showAndHideTweenEaseType );

        } //END Hide

        //--------------------------------//
        private void _ShowOrHide( float alpha, UITweener.TweenValue tweenValue, Vector3 scale, float tweenSpeed, float tweenDelay, EaseCurve.EaseType easeType )
        //--------------------------------//
        {

            if( IsPlayVideoOnRenderer() )
            {
                if( ShowAndHideViaAppear() )
                {
                    if( videoRenderer1 != null && videoRenderer1.sharedMaterial != null ) { videoRenderer1.sharedMaterial.color = new Color( videoRenderer1.sharedMaterial.color.r, videoRenderer1.sharedMaterial.color.g, videoRenderer1.sharedMaterial.color.b, alpha ); }

                    if( Is3DSideBySide() || Is3DTopBottom() || IsCameraUsingVRMode() )
                    {
                        if( videoRenderer2 != null && videoRenderer2.sharedMaterial != null ) { videoRenderer2.sharedMaterial.color = new Color( videoRenderer2.sharedMaterial.color.r, videoRenderer2.sharedMaterial.color.g, videoRenderer2.sharedMaterial.color.b, alpha ); }
                    }
                }
                else if( ShowAndHideViaFade() )
                {
                    if( videoRenderer1 != null && videoRenderer1.sharedMaterial != null )
                    {
                        TweenManager.Color( videoRenderer1.sharedMaterial, new Color( videoRenderer1.sharedMaterial.color.r, videoRenderer1.sharedMaterial.color.g, videoRenderer1.sharedMaterial.color.b, alpha ), tweenSpeed, easeType, videoRenderer1.sharedMaterial.color, tweenDelay, false, null );
                    }

                    if( Is3DSideBySide() || Is3DTopBottom() || IsCameraUsingVRMode() )
                    {
                        if( videoRenderer2 != null && videoRenderer2.sharedMaterial != null )
                        {
                            TweenManager.Color( videoRenderer2.sharedMaterial, new Color( videoRenderer2.sharedMaterial.color.r, videoRenderer2.sharedMaterial.color.g, videoRenderer2.sharedMaterial.color.b, alpha ), tweenSpeed, easeType, videoRenderer2.sharedMaterial.color, tweenDelay, false, null );
                        }
                    }
                }
                else if( ShowAndHideViaScale() )
                {
                    if( videoRenderer1 != null && videoRenderer1.transform != null )
                    {
                        TweenManager.Scale( videoRenderer1.transform, scale, tweenSpeed, easeType, videoRenderer1.transform.localScale, tweenDelay, false, null );
                    }

                    if( Is3DSideBySide() || Is3DTopBottom() || IsCameraUsingVRMode() )
                    {
                        if( videoRenderer2 != null && videoRenderer2.transform != null )
                        {
                            TweenManager.Scale( videoRenderer2.transform, scale, tweenSpeed, easeType, videoRenderer2.transform.localScale, tweenDelay, false, null );
                        }
                    }
                }
            }
            else
            {
                UIColorTweenManager manager = uiColorTweenManager_Plane;
                GameObject rendererParent = videoPlaneParent.gameObject;

                if( IsPlayVideoOnSphere() )
                {
                    manager = uiColorTweenManager_Sphere;
                    rendererParent = videoSphereParent.gameObject;
                }
                else if( IsPlayVideoOnFullscreen() )
                {
                    manager = uiColorTweenManager_Fullscreen;
                    rendererParent = fullscreenParent.gameObject;
                }

                if( showDebug ) { Debug.Log( "BlockVideo.cs _ShowOrHide() rendererParent.gameObject = " + rendererParent.name ); }
                rendererParent.SetActive( true );

                if( ShowAndHideViaAppear() )
                {
                    manager.Force( tweenValue );
                }
                else if( ShowAndHideViaFade() )
                {
                    manager.Play( tweenValue );
                }
                else if( ShowAndHideViaScale() )
                {
                    uiScaleTweenManager_Video.Play( tweenValue );
                }
            }

        } //END _ShowOrHide

        //--------------------------------//
        public override void ForceShow()
        //--------------------------------//
        {
            base.ForceShow();

            if( showDebug ) { Debug.Log( "BlockVideo.cs ForceShow()" ); }

            _ForceShowOrHide( 1f, UITweener.TweenValue.Show, onShowTweenScale );

        } //END ForceShow

        //--------------------------------//
        public override void ForceHide()
        //--------------------------------//
        {
            base.ForceHide();

            if( showDebug ) { Debug.Log( "BlockVideo.cs ForceHide()" ); }

            _ForceShowOrHide( 0f, UITweener.TweenValue.Hide, onHideTweenScale );

        } //END ForceHide

        //--------------------------------//
        private void _ForceShowOrHide( float alpha, UITweener.TweenValue tweenValue, Vector3 scale )
        //--------------------------------//
        {

            if( IsPlayVideoOnRenderer() )
            {
                if( ShowAndHideViaAppear() || ShowAndHideViaFade() )
                {
                    if( videoRenderer1 != null && videoRenderer1.sharedMaterial != null ) { videoRenderer1.sharedMaterial.color = new Color( videoRenderer1.sharedMaterial.color.r, videoRenderer1.sharedMaterial.color.g, videoRenderer1.sharedMaterial.color.b, alpha ); }

                    if( Is3DSideBySide() || Is3DTopBottom() || IsCameraUsingVRMode() )
                    {
                        if( videoRenderer2 != null && videoRenderer2.sharedMaterial != null ) { videoRenderer2.sharedMaterial.color = new Color( videoRenderer2.sharedMaterial.color.r, videoRenderer2.sharedMaterial.color.g, videoRenderer2.sharedMaterial.color.b, alpha ); }
                    }
                }
                else if( ShowAndHideViaScale() )
                {
                    if( videoRenderer1 != null && videoRenderer1.transform != null ) { videoRenderer1.transform.localScale = scale; }

                    if( Is3DSideBySide() || Is3DTopBottom() || IsCameraUsingVRMode() )
                    {
                        if( videoRenderer2 != null && videoRenderer2.transform != null ) { videoRenderer2.transform.localScale = scale; }
                    }
                }
            }
            else
            {
                UIColorTweenManager manager = uiColorTweenManager_Plane;

                if( IsPlayVideoOnSphere() ) { manager = uiColorTweenManager_Sphere; }

                if( ShowAndHideViaAppear() || ShowAndHideViaFade() )
                {
                    manager.Force( tweenValue );
                }
                else if( ShowAndHideViaScale() )
                {
                    uiScaleTweenManager_Video.Force( tweenValue );
                }
            }

        } //END _ForceShowOrHide



        //---------------------------------//
        protected override void OtherBlockButtonSelected( BlockButton button )
        //---------------------------------//
        {
            if( button.GetGroup() == this.blockGroup )
            {
                if( ShowAndHideSetToBlockButton() && playWhenBlockButtonSelected )
                {
                    if( loadOnStart )
                    {
                        Show();
                        Play();
                        //Debug.Log( "BlockButtonSelected() calling Show() and Play() because loadOnStart = true" );
                    }
                    else
                    {
                        currentLoadingAttempt = OnErrorTryPath.OriginalPath;
                        currentFileTypeAttempt = OnErrorTryFileType.OriginalFileType;

                        Show();
                        LoadVideo();
                        //Debug.Log( "BlockButtonSelected() calling Load because !loadOnStart" );
                    }
                }
                else if( ShowAndHideSetToBlockButton() && !playWhenBlockButtonSelected )
                {
                    Show();
                    //Debug.Log( "BlockButtonSelected() calling just Show() because ShowAndHideSetToBlockButton = true but !playWhenBlockButtonSelected" );
                }
                else
                {
                    //Debug.Log( "BlockButtonSelected() else" );
                }
            }
            else
            {
                if( ShowAndHideSetToBlockButton() && playWhenBlockButtonSelected )
                {
                    Pause();
                    Hide();
                }
                else if( ShowAndHideSetToBlockButton() && !playWhenBlockButtonSelected )
                {
                    Hide();
                }
            }

        } //END OtherBlockButtonSelected
        


        
        //--------------------------------//
        public void LookAwayAndBackColliderEntered()
        //--------------------------------//
        {
            //if( ShowDebug ) { Debug.Log( "BlockVideo.cs LookAwayAndBackColliderEntered() IsPlaying = " + IsPlaying() + ", IsLoaded = " + isLoaded ); }

            //This logic only occurs if we're either always active, or if we're set to only perform logic if the block is activated
            if( IsPlayVideoOnPlaneOrRenderer() && ( ( ShowAndHideSetToAlwaysShowOrAlwaysHide() || ( ShowAndHideSetToBlockButton() && blockGroup != null && BlockFocusManager.instance.IsFocused( blockGroup ) ) ) ) )
            {
                if( !IsPlaying() && isLoaded )
                {
                    if( IsPauseAndResume() )
                    {
                        //if( ShowDebug ) { Debug.Log( "BlockVideo.cs LookAwayAndBackColliderEntered() calling Resume()" ); }
                        Resume();
                        Show();
                    }
                    else if( IsPlayHideAndShowAnimation() )
                    {
                        if( showDebug ) { Debug.Log( "BlockVideo.cs LookAwayAndBackColliderEntered() calling Resume() and Show" ); }
                        Resume();
                        Show();
                    }
                }
                else if( IsPlaying() && isLoaded )
                {
                    if( IsMuteAndUnmute() )
                    {
                        //if( ShowDebug ) { Debug.Log( "BlockVideo.cs LookAwayAndBackColliderEntered() calling SetVolume( " + originalVolume + " )" ); }
                        SetVolume( originalVolume );
                    }
                    else if( IsFadeOutAndFadeInAudio() )
                    {
                        //if( ShowDebug ) { Debug.Log( "BlockVideo.cs LookAwayAndBackColliderEntered() calling FadeVolume( " + fadeInToVolume + " )" ); }
                        FadeVolume( fadeInToVolume, fadeInSpeed, fadeInDelay );
                    }
                }
            }
            

        } //END LookAwayAndBackColliderEntered

        //--------------------------------//
        public void LookAwayAndBackColliderExited()
        //--------------------------------//
        {
            //if( ShowDebug ) { Debug.Log( "BlockVideo.cs LookAwayAndBackColliderExited() IsPlaying = " + IsPlaying() + ", IsLoaded = " + isLoaded ); }

            //This logic only occurs if we're either always active, or if we're set to only perform logic if the block is activated
            if( IsPlayVideoOnPlaneOrRenderer() && ( ShowAndHideSetToAlwaysShowOrAlwaysHide() || ( ShowAndHideSetToBlockButton() && blockGroup != null && BlockFocusManager.instance.IsFocused( blockGroup ) ) ) )
            {
                if( IsPlaying() )
                {
                    if( IsPauseAndResume() )
                    {
                        //if( ShowDebug ) { Debug.Log( "BlockVideo.cs LookAwayAndBackColliderExited() calling Pause()" ); }
                        Pause();
                    }
                    else if( IsPlayHideAndShowAnimation() )
                    {
                        //if( ShowDebug ) { Debug.Log( "BlockVideo.cs LookAwayAndBackColliderExited() calling Pause() and Hide" ); }
                        Pause();
                        Hide();
                    }
                    else if( IsMuteAndUnmute() )
                    {
                        //if( ShowDebug ) { Debug.Log( "BlockVideo.cs LookAwayAndBackColliderExited() calling SetVolume( 0 )" ); }
                        SetVolume( 0f );
                    }
                    else if( IsFadeOutAndFadeInAudio() )
                    {
                        //if( ShowDebug ) { Debug.Log( "BlockVideo.cs LookAwayAndBackColliderExited() calling FadeVolume( " + fadeOutToVolume + " )" ); }
                        FadeVolume( fadeOutToVolume, fadeOutSpeed, fadeOutDelay );
                    }
                }
            }
            
        } //END LookAwayAndBackColliderExited
        

        //--------------------------------//
        private bool IsCameraUsingVRMode()
        //--------------------------------//
        {

            //Check for VR being on or off
            return ( XRMode.IsVRModeOn() );

        } //END IsCameraUsingVRMode

        //--------------------------------//
        public override void PrepareForDestroy()
        //--------------------------------//
        {

            Stop();

        } //END PrepareForDestroy
        
        
        

        //-------------------------------------------//
        public bool IsPlaying()
        //-------------------------------------------//
        {

            return isPlaying;

        } //END IsPlaying

        //-------------------------------------------//
        public void Resume()
        //-------------------------------------------//
        {

            if( isLoaded && !isPlaying )
            {
                Play();
            }

        } //END Resume

        //-------------------------------------------//
        public void Pause()
        //-------------------------------------------//
        {

            if( isLoaded && IsPlaying() )
            {
                isPlaying = false;

                if( IsVideoPlayerTypeUnity() && videoPlayer != null ) { videoPlayer.Pause(); }

                if( useAudioFromVideo && IsYoutubeVideo() && audioFromVideoPlayer != null ) { audioFromVideoPlayer.Pause(); }

                if( coroutine_audioSync != null ) { coroutine_audioSync = null; }
            }

        } //END Pause

        //---------------------------------//
        public void Stop()
        //---------------------------------//
        {

            if( IsVideoPlayerTypeUnity() && videoPlayer != null )
            {
                StopLoadingAnimation();
                StopLoadingAudioSource();
                videoPlayer.Stop();

                if( useAudioFromVideo && IsYoutubeVideo() && audioFromVideoPlayer != null ) { audioFromVideoPlayer.Stop(); }

                if( coroutine_audioSync != null ) { coroutine_audioSync = null; }

                isPlaying = false;
            }

        } //END Stop

        //---------------------------------//
        private void SetOriginalVolume()
        //---------------------------------//
        {

            originalVolume = volume;

        } //END SetOriginalVolume

        //---------------------------------//
        public void FadeVolume( float newVolume, float tweenLength, float tweenDelay )
        //---------------------------------//
        {
            volume = newVolume;

            if( IsBuiltInAudioSource() && audioSource != null )
            {
                AudioHelper.instance.Fade( audioSource, newVolume, tweenLength, tweenDelay );
            }
            else if( IsCustomAudioSource() && customAudioSource != null )
            {
                AudioHelper.instance.Fade( customAudioSource, newVolume, tweenLength, tweenDelay );
            }

        } //END FadeVolume

        //---------------------------------//
        public void SetVolume( float newVolume )
        //---------------------------------//
        {
            volume = newVolume;

            if( IsBuiltInAudioSource() && audioSource != null )
            {
                audioSource.volume = newVolume;
            }
            else if( IsCustomAudioSource() && customAudioSource != null )
            {
                customAudioSource.volume = newVolume;
            }

        } //END SetVolume

        //---------------------------------//
        public float GetVolume()
        //---------------------------------//
        {

            return volume;

        } //END GetVolume



        //---------------------------------//
        public bool GetLooping()
        //---------------------------------//
        {
            return WhenFinishedLoop();

        } //END GetLooping

        //---------------------------------//
        public WhenFinished GetWhenFinishedAction()
        //---------------------------------//
        {
            
            if( WhenFinishedLoop() ) { return WhenFinished.Loop; }
            else if( WhenFinishedHideVideo() ) { return WhenFinished.HideVideo; }
            else if( WhenFinishedCloseBlock() ) { return WhenFinished.CloseBlock; }
            else
            {
                return WhenFinished.DoNothing;
            }
            
        } //END GetWhenFinishedAction

        //---------------------------------//
        public void SetWhenFinishedAction( WhenFinished whenFinished )
        //---------------------------------//
        {
            if( whenFinished == WhenFinished.DoNothing ) { this.whenFinished = WhenFinished.DoNothing; }
            else if( whenFinished == WhenFinished.Loop ) { this.whenFinished = WhenFinished.Loop; }
            else if( whenFinished == WhenFinished.HideVideo ) { this.whenFinished = WhenFinished.HideVideo; }
            else if( whenFinished == WhenFinished.CloseBlock ) { this.whenFinished = WhenFinished.CloseBlock; }

            if( IsVideoPlayerTypeUnity() && videoPlayer != null )
            {
                videoPlayer.isLooping = WhenFinishedLoop();
            }

        } //END SetWhenFinishedAction


        //---------------------------------//
        public float GetPlaybackSpeed()
        //---------------------------------//
        {
            return playbackSpeed;

        } //END GetPlaybackSpeed

        //---------------------------------//
        public void SetPlaybackSpeed( float setPlaybackSpeed )
        //---------------------------------//
        {
            playbackSpeed = setPlaybackSpeed;

            if( IsVideoPlayerTypeUnity() && videoPlayer != null )
            {
                videoPlayer.playbackSpeed = setPlaybackSpeed;

                if( useAudioFromVideo && IsYoutubeVideo() )
                {
                    audioFromVideoPlayer.playbackSpeed = setPlaybackSpeed;
                }
            }

        } //END SetPlaybackSpeed


        //---------------------------------//
        public double GetSeekTime()
        //---------------------------------//
        {
            if( IsVideoPlayerTypeUnity() && videoPlayer != null )
            {
                return videoPlayer.time;
            }

            return 0;

        } //END GetSeekTime

        //---------------------------------//
        public void SetSeekTime( double setSeekTime )
        //---------------------------------//
        {

            if( IsVideoPlayerTypeUnity() && videoPlayer != null )
            {
                videoPlayer.time = setSeekTime;

                if( useAudioFromVideo && IsYoutubeVideo() )
                {
                    audioFromVideoPlayer.time = setSeekTime;
                }
            }

        } //END SetPlaybackSpeed




    } //END Class

} //END Namespace