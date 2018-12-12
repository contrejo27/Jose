/* BlockButton.cs
 * 
 * A collider used for triggering events
 * Contains customization options for visuals, audio, and animations based on interactions
 */
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;
using Sirenix.OdinInspector;
using System.Collections.Generic;

namespace BrandXR
{

    [RequireComponent( typeof( AudioSource ) )]
    public class BlockButton: Block
    {
        #region MISC SETTINGS
        //-----------------------------------------------------------//
        //------------------ MISC SETTINGS --------------------------//
        //-----------------------------------------------------------//

        private string debugWhenName = ""; //bxr_BlockButton

        public override BlockType GetBlockType() { return BlockType.Button; }

        [Space( 10f ), TitleGroup( "Block Button", "Interact via Gaze, Mouse, or Touch input to select. Used to activate other blocks in the same group" )] //, FoldoutGroup( "Image Settings" )
        public int titleDummy = 0;
        private bool initialGroupTypeSet = false; //Also used for determining what variables should be reset or restored

        //Original values in 3D world space, stored when switching to 2D in case we want to change back to 3D
        private Vector3 _position;
        private Quaternion _rotation;
        private Vector3 _scale;

        
        [Space(10f)]
        public ShowState visibleAtStart = ShowState.Showing;

        [Space(10f)]
        public EnabledSate enabledAtStart = EnabledSate.Enabled;

        #endregion

        #region COLLIDER SETTINGS
        //------------------------------------------------------------------//
        //------------------ COLLIDER SETTINGS ------------------------------//
        //------------------------------------------------------------------//
        public enum ColliderType
        {
            CirlceOrSphere,
            SquareOrCube,
            Custom
        }
        [OnValueChanged( "ValidateColliders" ), FoldoutGroup( "Collider Settings" ), InfoBox("BlockButton's work in 2D Canvas or 3D world space and contain colliders for each. The appropriate collider will be used automatically depending on the BlockGroup settings")]
        public ColliderType colliderType = ColliderType.CirlceOrSphere;
        private bool IsColliderTypeCircleOrSphere() { return colliderType == ColliderType.CirlceOrSphere; }
        private bool IsColliderTypeSquareOrCube() { return colliderType == ColliderType.SquareOrCube; }
        private bool IsColliderTypeCustom() { return colliderType == ColliderType.Custom; }
        private void ValidateColliders()
        {
            if( collider3D == null ) { return; }

            if( IsColliderTypeCircleOrSphere() )
            {
                if( colliderMeshSphere != null && colliderMeshFilter != null )
                {
                    ( collider3D as MeshCollider ).sharedMesh = colliderMeshSphere;
                    colliderMeshFilter.mesh = colliderMeshSphere;
                }
                if( collider2DImage != null && circleTexture )
                {
                    collider2DImage.texture = circleTexture;
                }
            }
            else if( IsColliderTypeSquareOrCube() )
            {
                if( colliderMeshCube != null && colliderMeshFilter != null )
                {
                    ( collider3D as MeshCollider ).sharedMesh = colliderMeshCube;
                    colliderMeshFilter.mesh = colliderMeshCube;
                }
                if( collider2DImage != null )
                {
                    collider2DImage.texture = null;
                }
            }
            else if( IsColliderTypeCustom() )
            {
                if( customCollider != null && colliderMeshFilter != null )
                {
                    ( collider3D as MeshCollider ).sharedMesh = customCollider;
                    colliderMeshFilter.mesh = customCollider;
                }

                if( collider2DImage != null )
                {
                    if( customColliderTexture != null )
                    {
                        collider2DImage.texture = customColliderTexture;
                    }
                    else
                    {
                        collider2DImage.texture = null;
                    }
                }
            }

            ColliderColorChanged();

        }

        [ShowIf( "IsColliderTypeCustom", true ), OnValueChanged( "ValidateColliders" ), FoldoutGroup( "Collider Settings" )]
        public Mesh customCollider;

        [ShowIf( "IsColliderTypeCustom", true ), OnValueChanged( "ValidateColliders" ), FoldoutGroup( "Collider Settings" )]
        public Texture customColliderTexture;

        [Space( 10f ), FoldoutGroup( "Collider Settings" ), OnValueChanged( "ColliderSizeChanged" ), Range( .1f, 500f )]
        public float collider3DWidth = 50f;
        private void ColliderSizeChanged()
        {
            if( collider3D != null ) { collider3D.transform.localScale = new Vector3( collider3DWidth, collider3DHeight, collider3DDepth ); }
            if( collider2DImage != null ) { collider2DImage.transform.localScale = new Vector3( collider2DWidth, collider2DHeight, 0f ); }
        }

        [FoldoutGroup( "Collider Settings" ), OnValueChanged( "ColliderSizeChanged" ), Range( .1f, 500f )]
        public float collider3DHeight = 50f;
        [FoldoutGroup( "Collider Settings" ), OnValueChanged( "ColliderSizeChanged" ), Range( .1f, 500f )]
        public float collider3DDepth = 1f;

        [Space( 10f ),FoldoutGroup( "Collider Settings" ), OnValueChanged( "ColliderSizeChanged" ), Range( .1f, 100f )]
        public float collider2DWidth = 1f;
        [FoldoutGroup( "Collider Settings" ), OnValueChanged( "ColliderSizeChanged" ), Range( .1f, 100f )]
        public float collider2DHeight = 1f;

        [Space( 10f ), FoldoutGroup( "Collider Settings" ), OnValueChanged( "ColliderColorChanged" )]
        public bool showColliderInEditor = true;
        private void ColliderColorChanged()
        {
            Color color = showColliderInEditorColor;
            if( Application.isPlaying || !showColliderInEditor ) { color = new Color( color.r, color.g, color.b, 0f ); }

            if( colliderMeshRenderer != null ) { colliderMeshRenderer.sharedMaterial.color = color; }

            if( collider2DImage != null ) { collider2DImage.color = color; }
        }

        [ShowIf( "showColliderInEditor", true ), OnValueChanged( "ColliderColorChanged" ), FoldoutGroup( "Collider Settings" )]
        public Color showColliderInEditorColor = new Color( Color.red.r, Color.red.g, Color.red.b, .1f );

        #endregion

        #region INTERACTION SETTINGS
        //------------------------------------------------------------------//
        //------------------ INTERACTION SETTINGS --------------------------//
        //------------------------------------------------------------------//
        [FoldoutGroup( "Interaction Settings" ), OnValueChanged("UseGazeInputChanged"), InfoBox("Gaze Input is generally used in VR, requires a VR camera rig setup via the bxr_XRSettings prefab")]
        public bool useGazeInput = true;
        private void UseGazeInputChanged()
        {
            ValidateFillImage();
            ValidateFillBackgroundImage();
        }


        [ FoldoutGroup( "Interaction Settings" ), ShowIf( "useGazeInput" ), Tooltip("How many seconds you should have to look at this button to select it")]
        public float gazeSelectionSpeed = 1f;
        

        [Space( 15f ), InfoBox("Used to enable mouse and touch input, common to Desktop and Mobile devices"), FoldoutGroup( "Interaction Settings" )]
        public bool useMouseAndTouchInput = true;
        


        public enum ThisBlockButtonSelectedResponse
        {
            DoNothing,
            HideThisButton,
            DisableThisButton
        }
        [Space( 15f ), InfoBox("Below are convenience options for Showing/Hiding/Disabling/Enabling this Block, to call a BlockEvent or a UnityEvent, check the Events section of this Prefab"), FoldoutGroup( "Interaction Settings" )]
        public ThisBlockButtonSelectedResponse OnThisButtonSelected = ThisBlockButtonSelectedResponse.DoNothing;
        private bool ActionOnThisButtonSelectedIsDoNothing() { return OnThisButtonSelected == ThisBlockButtonSelectedResponse.DoNothing; }
        private bool ActionOnThisButtonSelectedIsHide() { return OnThisButtonSelected == ThisBlockButtonSelectedResponse.HideThisButton; }
        private bool ActionOnThisButtonSelectedIsDisable() { return OnThisButtonSelected == ThisBlockButtonSelectedResponse.DisableThisButton; }




        public enum ActionOnSelection
        {
            Hide,
            Show,
            Disable,
            Enable
        }

        public enum ActionOnSelectionOptions
        {
            DoNothing,
            WhenAnyOtherButtonSelected,
            WhenSpecificButtonsSelected,
            WhenAnyOtherButtonInThisGroupSelected,
            WhenAnyOtherButtonInSpecificGroupSelected,
        }

        [Space( 15f ), FoldoutGroup( "Interaction Settings" )]
        public ActionOnSelectionOptions hideWhen = ActionOnSelectionOptions.DoNothing;

        [ShowIf( "hideWhen", ActionOnSelectionOptions.WhenSpecificButtonsSelected ), FoldoutGroup( "Interaction Settings" )]
        public List<BlockButton> otherBlockButtonsThatCauseHide = new List<BlockButton>();

        [ShowIf( "hideWhen", ActionOnSelectionOptions.WhenAnyOtherButtonInSpecificGroupSelected ), FoldoutGroup( "Interaction Settings" )]
        public List<BlockGroup> otherBlockGroupsThatCauseHide = new List<BlockGroup>();



        [Space( 15f ), FoldoutGroup( "Interaction Settings" )]
        public ActionOnSelectionOptions showWhen = ActionOnSelectionOptions.DoNothing;

        [ShowIf( "showWhen", ActionOnSelectionOptions.WhenSpecificButtonsSelected ), FoldoutGroup( "Interaction Settings" )]
        public List<BlockButton> otherBlockButtonsThatCauseShow = new List<BlockButton>();

        [ShowIf( "showWhen", ActionOnSelectionOptions.WhenAnyOtherButtonInSpecificGroupSelected ), FoldoutGroup( "Interaction Settings" )]
        public List<BlockGroup> otherBlockGroupsThatCauseShow = new List<BlockGroup>();




        [Space( 15f ), FoldoutGroup( "Interaction Settings" )]
        public ActionOnSelectionOptions disableWhen = ActionOnSelectionOptions.DoNothing;

        [ShowIf( "disableWhen", ActionOnSelectionOptions.WhenSpecificButtonsSelected ), FoldoutGroup( "Interaction Settings" )]
        public List<BlockButton> otherBlockButtonsThatCauseDisable = new List<BlockButton>();

        [ShowIf( "disableWhen", ActionOnSelectionOptions.WhenAnyOtherButtonInSpecificGroupSelected ), FoldoutGroup( "Interaction Settings" )]
        public List<BlockGroup> otherBlockGroupsThatCauseDisable = new List<BlockGroup>();




        [Space( 15f ), FoldoutGroup( "Interaction Settings" )]
        public ActionOnSelectionOptions enableWhen = ActionOnSelectionOptions.DoNothing;

        [ShowIf( "enableWhen", ActionOnSelectionOptions.WhenSpecificButtonsSelected ), FoldoutGroup( "Interaction Settings" )]
        public List<BlockButton> otherBlockButtonsThatCauseEnable = new List<BlockButton>();

        [ShowIf( "enableWhen", ActionOnSelectionOptions.WhenAnyOtherButtonInSpecificGroupSelected ), FoldoutGroup( "Interaction Settings" )]
        public List<BlockGroup> otherBlockGroupsThatCauseEnable = new List<BlockGroup>();

        #endregion

        #region IMAGE SETTINGS
        //------------------------------------------------------------------//
        //------------------ IMAGE SETTINGS ------------------------------//
        //------------------------------------------------------------------//

        [FoldoutGroup( "Image Settings" ), InfoBox("The Center image is generally used to display a logo or icon"), OnValueChanged( "ValidateCenterImage" )]
        public bool useCenterImage = true;
        private bool IsUseCenterImageTrue() { return useCenterImage; }
        
        //Used to change the image in editor based on settings.
        //Called via OnValidate method instead of using the standard OnValueChanged methods as they don't update the GameView() window
        private void ValidateCenterImage()
        {
            if( useCenterImage )
            {
                SetCenterImageActive( true );
                SetCenterImageTexture();
                SetCenterImageScale();
                SetCenterImageColor();
                SetCenterImageTweenSpeed();
                SetCenterImageEaseType();
            }
            else
            {
                SetCenterImageActive( false );
            }
        }

        private void SetCenterImageActive( bool active )
        {
            if( uiColorTweenManager_CenterImage != null && uiColorTweenManager_CenterImage.gameObject != null )
            {
                uiColorTweenManager_CenterImage.gameObject.SetActive( active );
            }
        }

        public enum ImageOptions
        {
            Texture,
            Sprite,
            Path
        }

        [ShowIf( "IsUseCenterImageTrue" ), OnValueChanged( "SetCenterImageTexture" ), FoldoutGroup( "Image Settings" )]
        public ImageOptions centerImageFormat = ImageOptions.Texture;
        private bool IsCenterImageFormatTexture() { return IsUseCenterImageTrue() && centerImageFormat == ImageOptions.Texture; }
        private bool IsCenterImageFormatSprite() { return IsUseCenterImageTrue() && centerImageFormat == ImageOptions.Sprite; }
        private bool IsCenterImageFormatPath() { return IsUseCenterImageTrue() && centerImageFormat == ImageOptions.Path; }
        private void SetCenterImageTexture()
        {
            if( useCenterImage )
            {
                if( uiColorTweenManager_CenterImage != null && uiColorTweenManager_CenterImage.tweeners != null && uiColorTweenManager_CenterImage.tweeners.Count > 0 && uiColorTweenManager_CenterImage.tweeners[ 0 ].image != null )
                {
                    if( IsCenterImageFormatTexture() && centerImageTexture != null )
                    {
                        uiColorTweenManager_CenterImage.tweeners[ 0 ].image.sprite = TextureHelper.CreateSpriteFromTexture( ref centerImageTexture );
                    }
                    else if( IsCenterImageFormatSprite() && centerImageSprite != null )
                    {
                        uiColorTweenManager_CenterImage.tweeners[ 0 ].image.sprite = centerImageSprite;
                    }
                    else if( IsCenterImageFormatPath() )
                    {
                        uiColorTweenManager_CenterImage.tweeners[ 0 ].image.sprite = null;
                    }
                }
            }
        }

        [ ShowIf( "IsCenterImageFormatTexture" ), OnValueChanged( "SetCenterImageTexture" ), FoldoutGroup( "Image Settings" )]
        public Texture centerImageTexture;
        [ShowIf( "IsCenterImageFormatSprite" ), OnValueChanged( "SetCenterImageTexture" ), FoldoutGroup( "Image Settings" )]
        public Sprite centerImageSprite;
        [ShowIf( "IsCenterImageFormatPath" ), OnValueChanged( "SetCenterImageTexture" ), FoldoutGroup( "Image Settings" )]
        public string centerImagePath;

        [ShowIf( "IsCenterImageFormatPath" ), FoldoutGroup( "Image Settings" )]
        public bool centerImageCacheIfWeb = true;

        private bool isCenterImageReady = false;

        [ShowIf( "IsUseCenterImageTrue" ), OnValueChanged( "SetCenterImageScale" ), FoldoutGroup( "Image Settings" ), Range(.1f, 10f)]
        public float centerImageScale = 1f;
        private void SetCenterImageScale()
        {
            if( useCenterImage )
            {
                SetScaleTweener( uiScaleTweener_CenterImage, centerImageScale );
            }
        }

        private void SetScaleTweener( UIScaleTweener uiScaleTweener, float showScaleSize )
        {
            if( uiScaleTweener != null && uiScaleTweener.GetTransform() != null )
            {
                Vector3 scale = new Vector3( showScaleSize, showScaleSize, showScaleSize );

                uiScaleTweener.SetScale( Vector3.zero, UITweener.TweenValue.Hide );
                uiScaleTweener.SetScale( scale, UITweener.TweenValue.Default );
                uiScaleTweener.SetScale( scale, UITweener.TweenValue.Show );
                uiScaleTweener.SetScale( scale, UITweener.TweenValue.Enabled );
                uiScaleTweener.SetScale( scale, UITweener.TweenValue.HoverExit );

                uiScaleTweener.Force( UITweener.TweenValue.Show );
            }
        }

        [ ShowIf( "IsUseCenterImageTrue" ), OnValueChanged("SetCenterImageColor"), FoldoutGroup( "Image Settings" )]
        public Color centerImageColor = Color.white;
        private void SetCenterImageColor()
        {
            if( useCenterImage )
            {
                if( uiColorTweenManager_CenterImage != null && uiColorTweenManager_CenterImage.tweeners.Count > 0 && uiColorTweenManager_CenterImage.tweeners[ 0 ].image != null )
                {
                    uiColorTweenManager_CenterImage.SetColor( centerImageColor, UITweener.TweenValue.Show );
                    uiColorTweenManager_CenterImage.SetColor( centerImageColor, UITweener.TweenValue.Default );
                    uiColorTweenManager_CenterImage.SetColor( centerImageColor, UITweener.TweenValue.Enabled );
                    uiColorTweenManager_CenterImage.SetColor( centerImageColor, UITweener.TweenValue.HoverExit );
                    uiColorTweenManager_CenterImage.SetColor( new Color( centerImageColor.r, centerImageColor.g, centerImageColor.b, 0f ), UITweener.TweenValue.Hide );

                    uiColorTweenManager_CenterImage.Force( UITweener.TweenValue.Show );
                }
            }
        }

        [ShowIf( "IsUseCenterImageTrue" ), OnValueChanged( "SetCenterImageTweenSpeed" ), FoldoutGroup( "Image Settings" )]
        public float centerImageTweenSpeed = 1f;
        private void SetCenterImageTweenSpeed()
        {
            if( useCenterImage )
            {
                if( uiColorTweenManager_CenterImage != null )
                {
                    SetTweenSpeed( uiColorTweenManager_CenterImage, centerImageTweenSpeed );
                }
                if( uiScaleTweener_CenterImage != null )
                {
                    SetTweenSpeed( uiScaleTweener_CenterImage, centerImageTweenSpeed );
                }
            }
        }

        [ShowIf( "IsUseCenterImageTrue" ), OnValueChanged( "SetCenterImageEaseType" ), FoldoutGroup( "Image Settings" )]
        public EaseCurve.EaseType centerImageEaseType = EaseCurve.EaseType.Linear;
        private void SetCenterImageEaseType()
        {
            if( useCenterImage )
            {
                if( uiColorTweenManager_CenterImage != null )
                {
                    SetEaseType( uiColorTweenManager_CenterImage, centerImageEaseType );
                }
                if( uiScaleTweener_CenterImage != null )
                {
                    SetEaseType( uiScaleTweener_CenterImage, centerImageEaseType );
                }
            }
        }

        private void SetTweenSpeed( UIColorTweenManager tweener, float tweenSpeed )
        {
            SetTweenSpeed( tweener.tweeners[ 0 ], tweenSpeed );
        }

        private void SetTweenSpeed( UIScaleTweenManager tweener, float tweenSpeed )
        {
            SetTweenSpeed( tweener.tweeners[ 0 ], tweenSpeed );
        }

        private void SetTweenSpeed( UITweener tweener, float tweenSpeed )
        {
            if( tweener != null )
            {
                tweener.SetTweenSpeed( tweenSpeed, UITweener.TweenValue.Hide );
                tweener.SetTweenSpeed( tweenSpeed, UITweener.TweenValue.Default );
                tweener.SetTweenSpeed( tweenSpeed, UITweener.TweenValue.Enabled );
                tweener.SetTweenSpeed( tweenSpeed, UITweener.TweenValue.HoverExit );
                tweener.SetTweenSpeed( tweenSpeed, UITweener.TweenValue.Show );
            }
        }


        private void SetEaseType( UIColorTweenManager tweener, EaseCurve.EaseType easeType )
        {
            if( tweener != null ) { SetEaseType( tweener.tweeners[ 0 ], easeType ); }
        }

        private void SetEaseType( UIScaleTweenManager tweener, EaseCurve.EaseType easeType )
        {
            if( tweener != null ) { SetEaseType( tweener.tweeners[ 0 ], easeType ); }
        }

        private void SetEaseType( UITweener tweener, EaseCurve.EaseType easeType )
        {
            if( tweener != null )
            {
                tweener.SetEaseType( easeType, UITweener.TweenValue.Hide );
                tweener.SetEaseType( easeType, UITweener.TweenValue.Default );
                tweener.SetEaseType( easeType, UITweener.TweenValue.Enabled );
                tweener.SetEaseType( easeType, UITweener.TweenValue.HoverExit );
                tweener.SetEaseType( easeType, UITweener.TweenValue.Show );
            }
        }






        [ Space( 15 ), InfoBox( "The Background image is generally used to display a backdrop to the Center Image" ), OnValueChanged( "ValidateBackgroundImage" ), FoldoutGroup( "Image Settings" )]
        public bool useBackgroundImage = true;
        private bool IsUseBackgroundImageTrue() { return useBackgroundImage; }

        //Used to change the image in editor based on settings.
        //Called via OnValidate method instead of using the standard OnValueChanged methods as they don't update the GameView() window
        private void ValidateBackgroundImage()
        {
            if( useBackgroundImage )
            {
                SetBackgroundImageActive( true );
                SetBackgroundImageTexture();
                SetBackgroundImageScale();
                SetBackgroundImageColor();
                SetBackgroundImageTweenSpeed();
                SetBackgroundImageEaseType();
            }
            else
            {
                SetBackgroundImageActive( false );
            }
        }

        private void SetBackgroundImageActive( bool active )
        {
            if( uiColorTweenManager_BackgroundImage != null && uiColorTweenManager_BackgroundImage.gameObject != null )
            {
                uiColorTweenManager_BackgroundImage.gameObject.SetActive( active );
            }
        }

        [ShowIf( "IsUseBackgroundImageTrue" ), OnValueChanged( "SetBackgroundImageTexture" ), FoldoutGroup( "Image Settings" ) ]
        public ImageOptions backgroundImageFormat = ImageOptions.Texture;
        private bool IsBackgroundImageFormatTexture() { return IsUseBackgroundImageTrue() && backgroundImageFormat == ImageOptions.Texture; }
        private bool IsBackgroundImageFormatSprite() { return IsUseBackgroundImageTrue() && backgroundImageFormat == ImageOptions.Sprite; }
        private bool IsBackgroundImageFormatPath() { return IsUseBackgroundImageTrue() && backgroundImageFormat == ImageOptions.Path; }
        private void SetBackgroundImageTexture()
        {
            if( useBackgroundImage )
            {
                if( uiColorTweenManager_BackgroundImage != null && uiColorTweenManager_BackgroundImage.tweeners != null && uiColorTweenManager_BackgroundImage.tweeners.Count > 0 && uiColorTweenManager_BackgroundImage.tweeners[ 0 ].image != null )
                {
                    if( IsBackgroundImageFormatTexture() && backgroundImageTexture != null )
                    {
                        uiColorTweenManager_BackgroundImage.tweeners[ 0 ].image.sprite = TextureHelper.CreateSpriteFromTexture( ref backgroundImageTexture );
                    }
                    else if( IsBackgroundImageFormatSprite() && backgroundImageSprite != null )
                    {
                        uiColorTweenManager_BackgroundImage.tweeners[ 0 ].image.sprite = backgroundImageSprite;
                    }
                    else if( IsBackgroundImageFormatPath() )
                    {
                        uiColorTweenManager_BackgroundImage.tweeners[ 0 ].image.sprite = null;
                    }
                }
            }
        }


        [ShowIf( "IsBackgroundImageFormatTexture" ), OnValueChanged( "SetBackgroundImageTexture"), FoldoutGroup( "Image Settings" )]
        public Texture backgroundImageTexture;
        [ShowIf( "IsBackgroundImageFormatSprite" ), OnValueChanged( "SetBackgroundImageTexture"), FoldoutGroup( "Image Settings" )]
        public Sprite backgroundImageSprite;
        [ShowIf( "IsBackgroundImageFormatPath" ), OnValueChanged( "SetBackgroundImageTexture"), FoldoutGroup( "Image Settings" )]
        public string backgroundImagePath;

        [ShowIf( "IsBackgroundImageFormatPath" ), FoldoutGroup( "Image Settings" )]
        public bool backgroundImageCacheIfWeb = true;

        private bool isBackgroundImageReady = false;

        [ShowIf( "IsUseBackgroundImageTrue" ), OnValueChanged( "SetBackgroundImageScale" ), FoldoutGroup( "Image Settings" ), Range( .1f, 10f )]
        public float backgroundImageScale = 1f;
        private void SetBackgroundImageScale()
        {
            if( useBackgroundImage )
            {
                SetScaleTweener( uiScaleTweener_BackgroundImage, backgroundImageScale );
            }
        }

        [ShowIf( "IsUseBackgroundImageTrue" ), OnValueChanged( "SetBackgroundImageColor" ), FoldoutGroup( "Image Settings" )]
        public Color backgroundImageColor = Color.black;
        private void SetBackgroundImageColor()
        {
            if( useBackgroundImage )
            {
                if( uiColorTweenManager_BackgroundImage != null && uiColorTweenManager_BackgroundImage.tweeners.Count > 0 && uiColorTweenManager_BackgroundImage.tweeners[ 0 ].image != null )
                {
                    uiColorTweenManager_BackgroundImage.SetColor( backgroundImageColor, UITweener.TweenValue.Show );
                    uiColorTweenManager_BackgroundImage.SetColor( backgroundImageColor, UITweener.TweenValue.Default );
                    uiColorTweenManager_BackgroundImage.SetColor( backgroundImageColor, UITweener.TweenValue.Enabled );
                    uiColorTweenManager_BackgroundImage.SetColor( backgroundImageColor, UITweener.TweenValue.HoverExit );
                    uiColorTweenManager_BackgroundImage.SetColor( new Color( backgroundImageColor.r, backgroundImageColor.g, backgroundImageColor.b, 0f ), UITweener.TweenValue.Hide );
                    uiColorTweenManager_BackgroundImage.Force( UITweener.TweenValue.Show );
                }
            }
        }

        [ShowIf( "IsUseBackgroundImageTrue" ), OnValueChanged( "SetBackgroundImageTweenSpeed" ), FoldoutGroup( "Image Settings" )]
        public float backgroundImageTweenSpeed = 1f;
        private void SetBackgroundImageTweenSpeed()
        {
            if( useBackgroundImage )
            {
                if( uiColorTweenManager_BackgroundImage != null )
                {
                    SetTweenSpeed( uiColorTweenManager_BackgroundImage, backgroundImageTweenSpeed );
                }
                if( uiScaleTweener_BackgroundImage != null )
                {
                    SetTweenSpeed( uiScaleTweener_BackgroundImage, backgroundImageTweenSpeed );
                }
            }
        }

        [ShowIf( "IsUseBackgroundImageTrue" ), OnValueChanged( "SetBackgroundImageEaseType" ), FoldoutGroup( "Image Settings" )]
        public EaseCurve.EaseType backgroundImageEaseType = EaseCurve.EaseType.Linear;
        private void SetBackgroundImageEaseType()
        {
            if( useBackgroundImage )
            {
                if( uiColorTweenManager_BackgroundImage != null )
                {
                    SetEaseType( uiColorTweenManager_BackgroundImage, backgroundImageEaseType );
                }
                if( uiScaleTweener_BackgroundImage != null )
                {
                    SetEaseType( uiScaleTweener_BackgroundImage, backgroundImageEaseType );
                }
            }
        }









        //[Space( 15 ), InfoBox("The Parallax image a background that shifts subtly using a parallax feature to show depth"),  OnValueChange("ValidateParallaxImage"), FoldoutGroup( "Image Settings" )]
        private bool useParallaxImage = false; //DOB: Disabled Parallax image feature as it's not currently implemented (acts as another background image with no parallax feature)
        private bool IsUseParallaxImageTrue() { return useParallaxImage; }

        //Used to change the image in editor based on settings.
        //Called via OnValidate method instead of using the standard OnValueChanged methods as they don't update the GameView() window
        private void ValidateParallaxImage()
        {
            if( useParallaxImage )
            {
                SetParallaxImageActive( true );
                SetParallaxImageTexture();
                SetParallaxImageScale();
                SetParallaxImageColor();
                SetParallaxImageDepth();
                SetParallaxImageTweenSpeed();
                SetParallaxImageEaseType();
            }
            else
            {
                SetParallaxImageActive( false );
            }
        }

        private void SetParallaxImageActive( bool active )
        {
            if( uiColorTweenManager_ParallaxImage != null && uiColorTweenManager_ParallaxImage.gameObject != null )
            {
                uiColorTweenManager_ParallaxImage.gameObject.SetActive( active );
            }
        }

        [ShowIf( "IsUseParallaxImageTrue" ), OnValueChanged( "SetParallaxImageTexture" ), FoldoutGroup( "Image Settings" )]
        public ImageOptions parallaxImageFormat = ImageOptions.Texture;
        private bool IsParallaxImageFormatTexture() { return IsUseParallaxImageTrue() && parallaxImageFormat == ImageOptions.Texture; }
        private bool IsParallaxImageFormatSprite() { return IsUseParallaxImageTrue() && parallaxImageFormat == ImageOptions.Sprite; }
        private bool IsParallaxImageFormatPath() { return IsUseParallaxImageTrue() && parallaxImageFormat == ImageOptions.Path; }
        private void SetParallaxImageTexture()
        {
            if( useParallaxImage )
            {
                if( uiColorTweenManager_ParallaxImage != null && uiColorTweenManager_ParallaxImage.tweeners != null && uiColorTweenManager_ParallaxImage.tweeners.Count > 0 && uiColorTweenManager_ParallaxImage.tweeners[ 0 ].image != null )
                {
                    if( IsParallaxImageFormatTexture() && parallaxImageTexture != null )
                    {
                        uiColorTweenManager_ParallaxImage.tweeners[ 0 ].image.sprite = TextureHelper.CreateSpriteFromTexture( ref parallaxImageTexture );
                    }
                    else if( IsParallaxImageFormatSprite() && parallaxImageSprite != null )
                    {
                        uiColorTweenManager_ParallaxImage.tweeners[ 0 ].image.sprite = parallaxImageSprite;
                    }
                    else if( IsParallaxImageFormatPath() )
                    {
                        uiColorTweenManager_ParallaxImage.tweeners[ 0 ].image.sprite = null;
                    }
                }
            }
        }

        [ShowIf( "IsParallaxImageFormatTexture" ), OnValueChanged( "SetParallaxImageTexture" ), FoldoutGroup( "Image Settings" )]
        public Texture parallaxImageTexture;
        [ShowIf( "IsParallaxImageFormatSprite" ), OnValueChanged( "SetParallaxImageTexture" ), FoldoutGroup( "Image Settings" )]
        public Sprite parallaxImageSprite;
        [ShowIf( "IsParallaxImageFormatPath" ), OnValueChanged( "SetParallaxImageTexture" ), FoldoutGroup( "Image Settings" )]
        public string parallaxImagePath;

        [ShowIf( "IsParallaxImageFormatPath" ), FoldoutGroup( "Image Settings" )]
        public bool parallaxImageCacheIfWeb = true;

        private bool isParallaxImageReady = false;
        
        [ShowIf( "IsUseParallaxImageTrue" ), OnValueChanged( "SetParallaxImageScale" ), FoldoutGroup( "Image Settings" ), Range( .1f, 10f )]
        public float parallaxImageScale = 1f;
        private void SetParallaxImageScale()
        {
            if( useParallaxImage )
            {
                SetScaleTweener( uiScaleTweener_ParallaxImage, parallaxImageScale );
            }
        }

        [ShowIf( "IsUseParallaxImageTrue" ), OnValueChanged( "SetParallaxImageColor" ), FoldoutGroup( "Image Settings" )]
        public Color parallaxImageColor = Color.white;
        private void SetParallaxImageColor()
        {
            if( useParallaxImage )
            {
                if( uiColorTweenManager_ParallaxImage != null && uiColorTweenManager_ParallaxImage.tweeners.Count > 0 && uiColorTweenManager_ParallaxImage.tweeners[ 0 ].image != null )
                {
                    uiColorTweenManager_ParallaxImage.SetColor( parallaxImageColor, UITweener.TweenValue.Show );
                    uiColorTweenManager_ParallaxImage.SetColor( parallaxImageColor, UITweener.TweenValue.Default );
                    uiColorTweenManager_ParallaxImage.SetColor( parallaxImageColor, UITweener.TweenValue.Enabled );
                    uiColorTweenManager_ParallaxImage.SetColor( parallaxImageColor, UITweener.TweenValue.HoverExit );
                    uiColorTweenManager_ParallaxImage.SetColor( new Color( parallaxImageColor.r, parallaxImageColor.g, parallaxImageColor.b, 0f ), UITweener.TweenValue.Hide );
                    uiColorTweenManager_ParallaxImage.Force( UITweener.TweenValue.Show );
                }
            }
        }

        [ShowIf( "IsUseParallaxImageTrue" ), OnValueChanged( "SetParallaxImageDepth" ), FoldoutGroup( "Image Settings" ), Range( 0f, 1f )]
        public float parallaxImageDepth = .5f;
        private void SetParallaxImageDepth()
        {
            if( useParallaxImage )
            {
                if( uiColorTweenManager_ParallaxImage != null )
                {
                    uiColorTweenManager_ParallaxImage.transform.localPosition = new Vector3( uiColorTweenManager_ParallaxImage.transform.localPosition.x, uiColorTweenManager_ParallaxImage.transform.localPosition.y, parallaxImageDepth );
                }
            }
        }
        
        [ShowIf( "IsUseParallaxImageTrue" ), OnValueChanged( "SetParallaxImageTweenSpeed" ), FoldoutGroup( "Image Settings" )]
        public float parallaxImageTweenSpeed = 1f;
        private void SetParallaxImageTweenSpeed()
        {
            if( useParallaxImage )
            {
                if( uiColorTweenManager_ParallaxImage != null )
                {
                    SetTweenSpeed( uiColorTweenManager_ParallaxImage, parallaxImageTweenSpeed );
                }
                if( uiScaleTweener_ParallaxImage != null )
                {
                    SetTweenSpeed( uiScaleTweener_ParallaxImage, parallaxImageTweenSpeed );
                }
            }
        }

        [ShowIf( "IsUseParallaxImageTrue" ), OnValueChanged( "SetParallaxImageEaseType" ), FoldoutGroup( "Image Settings" )]
        public EaseCurve.EaseType parallaxImageEaseType = EaseCurve.EaseType.Linear;
        private void SetParallaxImageEaseType()
        {
            if( useParallaxImage )
            {
                if( uiColorTweenManager_ParallaxImage != null )
                {
                    SetEaseType( uiColorTweenManager_ParallaxImage, parallaxImageEaseType );
                }
                if( uiScaleTweener_ParallaxImage != null )
                {
                    SetEaseType( uiScaleTweener_ParallaxImage, parallaxImageEaseType );
                }
            }
        }








        [Space( 15 ), OnValueChanged( "ValidateFillImage" ), InfoBox("Fill Image is used to show your progress in selecting this button, used primarily for VR Gazing"), ShowIf("useGazeInput"), FoldoutGroup( "Image Settings" )]
        public bool useFillImage;
        private bool IsUseFillImageTrue() { return useGazeInput && useFillImage; }

        //Used to change the image in editor based on settings.
        //Called via OnValidate method instead of using the standard OnValueChanged methods as they don't update the GameView() window
        private void ValidateFillImage()
        {
            if( IsUseFillImageTrue() )
            {
                SetFillImageActive( true );
                SetFillImageTexture();
                SetFillImageScale();
                SetFillImageColor();
                SetFillMethod();
                SetFillOriginMethod();
                SetFillImageTweenSpeed();
                SetFillImageEaseType();
            }
            else
            {
                SetFillImageActive( false );
            }
        }

        private void SetFillImageActive( bool active )
        {
            if( uiColorTweenManager_FillImage != null && uiColorTweenManager_FillImage.gameObject != null )
            {
                uiColorTweenManager_FillImage.gameObject.SetActive( active );
            }
        }

        [ShowIf( "IsUseFillImageTrue" ), OnValueChanged( "SetFillImageTexture" ), FoldoutGroup("Image Settings") ]
        public ImageOptions fillImageFormat = ImageOptions.Texture;
        private bool IsFillImageFormatTexture() { return IsUseFillImageTrue() && fillImageFormat == ImageOptions.Texture; }
        private bool IsFillImageFormatSprite() { return IsUseFillImageTrue() && fillImageFormat == ImageOptions.Sprite; }
        private bool IsFillImageFormatPath() { return IsUseFillImageTrue() && fillImageFormat == ImageOptions.Path; }
        private void SetFillImageTexture()
        {
            if( useFillImage )
            {
                if( uiColorTweenManager_FillImage != null && uiColorTweenManager_FillImage.tweeners != null && uiColorTweenManager_FillImage.tweeners.Count > 0 && uiColorTweenManager_FillImage.tweeners[ 0 ].image != null )
                {
                    if( IsFillImageFormatTexture() && fillImageTexture != null )
                    {
                        uiColorTweenManager_FillImage.tweeners[ 0 ].image.sprite = TextureHelper.CreateSpriteFromTexture( ref fillImageTexture );
                    }
                    else if( IsFillImageFormatSprite() && fillImageSprite != null )
                    {
                        uiColorTweenManager_FillImage.tweeners[ 0 ].image.sprite = fillImageSprite;
                    }
                    else if( IsFillImageFormatPath() )
                    {
                        uiColorTweenManager_FillImage.tweeners[ 0 ].image.sprite = null;
                    }
                }
            }
        }
        
        [ShowIf( "IsFillImageFormatTexture" ), OnValueChanged( "SetFillImageTexture" ), FoldoutGroup( "Image Settings" )]
        public Texture fillImageTexture;
        [ShowIf( "IsFillImageFormatSprite" ), OnValueChanged( "SetFillImageTexture" ), FoldoutGroup( "Image Settings" )]
        public Sprite fillImageSprite;
        [ShowIf( "IsFillImageFormatPath" ), OnValueChanged( "SetFillImageTexture" ), FoldoutGroup( "Image Settings" )]
        public string fillImagePath;

        [ShowIf( "IsFillImageFormatPath" ), OnValueChanged( "SetFillImageTexture" ), FoldoutGroup( "Image Settings" )]
        public bool fillImageCacheIfWeb = true;

        private bool isFillImageReady = false;

        [ShowIf( "IsUseFillImageTrue" ), OnValueChanged( "SetFillImageScale" ), FoldoutGroup( "Image Settings" ), Range( .1f, 10f )]
        public float fillImageScale = 1f;
        private void SetFillImageScale()
        {
            if( useFillImage )
            {
                SetScaleTweener( uiScaleTweener_FillImage, fillImageScale );
            }
        }

        [ShowIf( "IsUseFillImageTrue" ), OnValueChanged( "SetFillImageColor" ), FoldoutGroup( "Image Settings" )]
        public Color fillImageColor = Color.white;
        private void SetFillImageColor()
        {
            if( useFillImage )
            {
                if( uiColorTweenManager_FillImage != null && uiColorTweenManager_FillImage.tweeners.Count > 0 && uiColorTweenManager_FillImage.tweeners[ 0 ].image != null )
                {
                    uiColorTweenManager_FillImage.SetColor( fillImageColor, UITweener.TweenValue.Show );
                    uiColorTweenManager_FillImage.SetColor( fillImageColor, UITweener.TweenValue.Default );
                    uiColorTweenManager_FillImage.SetColor( fillImageColor, UITweener.TweenValue.Enabled );
                    uiColorTweenManager_FillImage.SetColor( fillImageColor, UITweener.TweenValue.HoverExit );
                    uiColorTweenManager_FillImage.SetColor( new Color( fillImageColor.r, fillImageColor.g, fillImageColor.b, 0f ), UITweener.TweenValue.Hide );
                    uiColorTweenManager_FillImage.Force( UITweener.TweenValue.Show );
                }
            }
        }

        [ShowIf( "IsUseFillImageTrue" ), OnValueChanged( "SetFillMethod" ), FoldoutGroup( "Image Settings" )]
        public Image.FillMethod fillMethod = Image.FillMethod.Radial360;
        private void SetFillMethod()
        {
            if( useFillImage )
            {
                if( uiColorTweenManager_FillImage != null && uiColorTweenManager_FillImage.tweeners.Count > 0 && uiColorTweenManager_FillImage.tweeners[ 0 ].image != null )
                {
                    uiColorTweenManager_FillImage.tweeners[ 0 ].image.fillMethod = fillMethod;
                }
            }
        }

        private bool ShowFillOrigin360() { return IsUseFillImageTrue() && fillMethod == Image.FillMethod.Radial360; }
        private bool ShowFillOrigin180() { return IsUseFillImageTrue() && fillMethod == Image.FillMethod.Radial180; }
        private bool ShowFillOrigin90() { return IsUseFillImageTrue() && fillMethod == Image.FillMethod.Radial90; }
        private bool ShowFillOriginVertical() { return IsUseFillImageTrue() && fillMethod == Image.FillMethod.Vertical; }
        private bool ShowFillOriginHorizontal() { return IsUseFillImageTrue() && fillMethod == Image.FillMethod.Horizontal; }

        [ShowIf( "ShowFillOrigin360" ), OnValueChanged( "SetFillOriginMethod" ), FoldoutGroup( "Image Settings" )]
        public Image.Origin360 fillOrigin360 = Image.Origin360.Bottom;

        [ShowIf( "ShowFillOrigin180" ), OnValueChanged( "SetFillOriginMethod" ), FoldoutGroup( "Image Settings" )]
        public Image.Origin180 fillOrigin180 = Image.Origin180.Bottom;

        [ShowIf( "ShowFillOrigin90" ), OnValueChanged( "SetFillOriginMethod" ), FoldoutGroup( "Image Settings" )]
        public Image.Origin90 fillOrigin90 = Image.Origin90.BottomLeft;

        [ShowIf( "ShowFillOriginVertical" ), OnValueChanged( "SetFillOriginMethod" ), FoldoutGroup( "Image Settings" )]
        public Image.OriginVertical fillOriginVertical = Image.OriginVertical.Bottom;

        [ShowIf( "ShowFillOriginHorizontal" ), OnValueChanged( "SetFillOriginMethod" ), FoldoutGroup( "Image Settings" )]
        public Image.OriginHorizontal fillOriginHorizontal = Image.OriginHorizontal.Left;

        private void SetFillOriginMethod()
        {
            if( useFillImage )
            {
                if( uiColorTweenManager_FillImage != null && uiColorTweenManager_FillImage.tweeners.Count > 0 && uiColorTweenManager_FillImage.tweeners[ 0 ].image != null )
                {
                    uiColorTweenManager_FillImage.tweeners[ 0 ].image.fillOrigin = GetFillOrigin();
                }
            }
        }
        
        private int GetFillOrigin()
        {
            int origin = 0;

            if( fillMethod == Image.FillMethod.Radial360 ) { origin = (int)fillOrigin360; }
            else if( fillMethod == Image.FillMethod.Radial180 ) { origin = (int)fillOrigin180; }
            else if( fillMethod == Image.FillMethod.Radial90 ) { origin = (int)fillOrigin90; }
            else if( fillMethod == Image.FillMethod.Vertical ) { origin = (int)fillOriginVertical; }
            else if( fillMethod == Image.FillMethod.Horizontal ) { origin = (int)fillOriginHorizontal; }

            return origin;
        }

        [ ShowIf( "IsUseFillImageTrue" ), OnValueChanged( "SetFillImageTweenSpeed" ), FoldoutGroup( "Image Settings" )]
        public float fillImageTweenSpeed = 1f;
        private void SetFillImageTweenSpeed()
        {
            if( useFillImage )
            {
                if( uiColorTweenManager_FillImage != null )
                {
                    SetTweenSpeed( uiColorTweenManager_FillImage, fillImageTweenSpeed );
                }
                if( uiScaleTweener_FillImage != null )
                {
                    SetTweenSpeed( uiScaleTweener_FillImage, fillImageTweenSpeed );
                }
            }
        }

        [ShowIf( "IsUseFillImageTrue" ), OnValueChanged( "SetFillImageEaseType" ), FoldoutGroup( "Image Settings" )]
        public EaseCurve.EaseType fillImageEaseType = EaseCurve.EaseType.Linear;
        private void SetFillImageEaseType()
        {
            if( useFillImage )
            {
                if( uiColorTweenManager_FillImage != null )
                {
                    SetEaseType( uiColorTweenManager_FillImage, fillImageEaseType );
                }
                if( uiScaleTweener_FillImage != null )
                {
                    SetEaseType( uiScaleTweener_FillImage, fillImageEaseType );
                }
            }
        }







        [Space( 15 ), OnValueChanged( "ValidateFillBackgroundImage" ), InfoBox( "This Fill Background will appear below your Fill Image, generally used as the background of a loading bar during VR Gaze input" ), ShowIf( "useGazeInput" ), FoldoutGroup( "Image Settings" )]
        public bool useFillBackgroundImage;
        private bool IsUseFillBackgroundImageTrue() { return useGazeInput && useFillBackgroundImage; }

        //Used to change the image in editor based on settings.
        //Called via OnValidate method instead of using the standard OnValueChanged methods as they don't update the GameView() window
        private void ValidateFillBackgroundImage()
        {
            if( IsUseFillBackgroundImageTrue() )
            {
                SetFillBackgroundImageActive( true );
                SetFillBackgroundImageTexture();
                SetFillBackgroundImageScale();
                SetFillBackgroundImageColor();
                SetFillBackgroundImageTweenSpeed();
                SetFillBackgroundImageEaseType();
            }
            else
            {
                SetFillBackgroundImageActive( false );
            }
        }

        private void SetFillBackgroundImageActive( bool active )
        {
            if( uiColorTweenManager_FillBackgroundImage != null && uiColorTweenManager_FillBackgroundImage.gameObject != null )
            {
                uiColorTweenManager_FillBackgroundImage.gameObject.SetActive( active );
            }
        }

        [ShowIf( "IsUseFillBackgroundImageTrue" ), OnValueChanged( "SetFillBackgroundImageTexture" ), FoldoutGroup( "Image Settings" )]
        public ImageOptions fillBackgroundImageFormat = ImageOptions.Texture;
        private bool IsFillBackgroundImageFormatTexture() { return IsUseFillBackgroundImageTrue() && fillBackgroundImageFormat == ImageOptions.Texture; }
        private bool IsFillBackgroundImageFormatSprite() { return IsUseFillBackgroundImageTrue() && fillBackgroundImageFormat == ImageOptions.Sprite; }
        private bool IsFillBackgroundImageFormatPath() { return IsUseFillBackgroundImageTrue() && fillBackgroundImageFormat == ImageOptions.Path; }
        private void SetFillBackgroundImageTexture()
        {
            if( useFillBackgroundImage )
            {
                if( uiColorTweenManager_FillBackgroundImage != null && uiColorTweenManager_FillBackgroundImage.tweeners != null && uiColorTweenManager_FillBackgroundImage.tweeners.Count > 0 && uiColorTweenManager_FillBackgroundImage.tweeners[ 0 ].image != null )
                {
                    if( IsFillBackgroundImageFormatTexture() && fillBackgroundImageTexture != null )
                    {
                        uiColorTweenManager_FillBackgroundImage.tweeners[ 0 ].image.sprite = TextureHelper.CreateSpriteFromTexture( ref fillBackgroundImageTexture );
                    }
                    else if( IsFillBackgroundImageFormatSprite() && fillBackgroundImageSprite != null )
                    {
                        uiColorTweenManager_FillBackgroundImage.tweeners[ 0 ].image.sprite = fillBackgroundImageSprite;
                    }
                    else if( IsFillBackgroundImageFormatPath() )
                    {
                        uiColorTweenManager_FillBackgroundImage.tweeners[ 0 ].image.sprite = null;
                    }
                }
            }
        }

        [ShowIf( "IsFillBackgroundImageFormatTexture" ), OnValueChanged( "SetFillBackgroundImageTexture" ), FoldoutGroup( "Image Settings" )]
        public Texture fillBackgroundImageTexture;
        [ShowIf( "IsFillBackgroundImageFormatSprite" ), OnValueChanged( "SetFillBackgroundImageTexture" ), FoldoutGroup( "Image Settings" )]
        public Sprite fillBackgroundImageSprite;
        [ShowIf( "IsFillBackgroundImageFormatPath" ), OnValueChanged( "SetFillBackgroundImageTexture" ), FoldoutGroup( "Image Settings" )]
        public string fillBackgroundImagePath;

        [ShowIf( "IsFillBackgroundImageFormatPath" ), FoldoutGroup( "Image Settings" )]
        public bool fillBackgroundImageCacheIfWeb = true;

        private bool isFillBackgroundImageReady = false;

        [ShowIf( "IsUseFillBackgroundImageTrue" ), OnValueChanged( "SetFillBackgroundImageScale" ), FoldoutGroup( "Image Settings" ), Range( .1f, 10f )]
        public float fillBackgroundImageScale = 1f;
        private void SetFillBackgroundImageScale()
        {
            if( useFillBackgroundImage )
            {
                SetScaleTweener( uiScaleTweener_FillBackgroundImage, fillBackgroundImageScale );
            }
        }

        [ShowIf( "IsUseFillBackgroundImageTrue" ), FoldoutGroup( "Image Settings" )]
        public Color fillBackgroundImageColor = Color.white;
        private void SetFillBackgroundImageColor()
        {
            if( useFillBackgroundImage )
            {
                if( uiColorTweenManager_FillBackgroundImage != null && uiColorTweenManager_FillBackgroundImage.tweeners.Count > 0 && uiColorTweenManager_FillBackgroundImage.tweeners[ 0 ].image != null )
                {
                    uiColorTweenManager_FillBackgroundImage.SetColor( fillBackgroundImageColor, UITweener.TweenValue.Show );
                    uiColorTweenManager_FillBackgroundImage.SetColor( fillBackgroundImageColor, UITweener.TweenValue.Default );
                    uiColorTweenManager_FillBackgroundImage.SetColor( fillBackgroundImageColor, UITweener.TweenValue.Enabled );
                    uiColorTweenManager_FillBackgroundImage.SetColor( fillBackgroundImageColor, UITweener.TweenValue.HoverExit );
                    uiColorTweenManager_FillBackgroundImage.SetColor( new Color( fillBackgroundImageColor.r, fillBackgroundImageColor.g, fillBackgroundImageColor.b, 0f ), UITweener.TweenValue.Hide );
                    uiColorTweenManager_FillBackgroundImage.Force( UITweener.TweenValue.Show );
                }
            }
        }
        
        [ShowIf( "IsUseFillBackgroundImageTrue" ), OnValueChanged( "SetFillBackgroundImageTweenSpeed" ), FoldoutGroup( "Image Settings" )]
        public float fillBackgroundImageTweenSpeed = 1f;
        private void SetFillBackgroundImageTweenSpeed()
        {
            if( useFillBackgroundImage )
            {
                if( uiColorTweenManager_FillBackgroundImage != null )
                {
                    SetTweenSpeed( uiColorTweenManager_FillBackgroundImage, fillBackgroundImageTweenSpeed );
                }
                if( uiScaleTweener_FillBackgroundImage != null )
                {
                    SetTweenSpeed( uiScaleTweener_FillBackgroundImage, fillBackgroundImageTweenSpeed );
                }
            }
        }

        [ShowIf( "IsUseFillBackgroundImageTrue" ), OnValueChanged( "SetFillBackgroundImageEaseType" ), FoldoutGroup( "Image Settings" )]
        public EaseCurve.EaseType fillBackgroundImageEaseType = EaseCurve.EaseType.Linear;
        private void SetFillBackgroundImageEaseType()
        {
            if( useFillBackgroundImage )
            {
                if( uiColorTweenManager_FillBackgroundImage != null )
                {
                    SetEaseType( uiColorTweenManager_FillBackgroundImage, fillBackgroundImageEaseType );
                }
                if( uiScaleTweener_FillBackgroundImage != null )
                {
                    SetEaseType( uiScaleTweener_FillBackgroundImage, fillBackgroundImageEaseType );
                }
            }
        }




        #endregion

        #region TEXT SETTINGS
        //------------------------------------------------------------------//
        //---------------------- TEXT SETTINGS -----------------------------//
        //------------------------------------------------------------------//

        [FoldoutGroup("Text Settings"), OnValueChanged( "ValidateText" )]
        public bool showText = false;
        private bool IsUseTextTrue() { return showText; }
        private void SetTextActive( bool active )
        {
            uiColorTweenManager_TextMessage.gameObject.SetActive( active );
        }

        //Called by OnValidate, which is necessary for now because OdinInspector
        //does not set the GameView as dirty when a variable is changed
        private void ValidateText()
        {
            if( showText )
            {
                SetTextActive( true );
                SetText();
                SetTextScale();
                SetTextColor();
                SetFont();
                SetFontSize();
                SetTextTweenSpeed();
                SetTextEaseType();
            }
            else
            {
                SetTextActive( false );
            }
        }

        [ShowIf( "showText" ), FoldoutGroup( "Text Settings" ), OnValueChanged( "SetText" )]
        public string textMessage = "Play";
        private void SetText()
        {
            if( name == debugWhenName ) { Debug.Log( "SetText() text = " + textMessage ); }
            uiTextField_Line1.text = textMessage;
        }

        [Range( .1f, 10f ), ShowIf( "showText" ), OnValueChanged( "SetTextScale" ), FoldoutGroup( "Text Settings" )]
        public float textScale = 1f;
        private void SetTextScale()
        {
            if( showText )
            {
                if( uiScaleTweener_TextMessage != null && uiScaleTweener_TextMessage.GetTransform() != null )
                {
                    SetScaleTweener( uiScaleTweener_TextMessage, textScale );
                }
            }
        }
        
        [ShowIf( "showText" ), FoldoutGroup( "Text Settings" ), OnValueChanged( "SetTextColor" )]
        public Color textColor = Color.white;
        private void SetTextColor()
        {
            uiColorTweenManager_TextMessage.SetColor( textColor, UITweener.TweenValue.Show );
            uiColorTweenManager_TextMessage.SetColor( new Color( textColor.r, textColor.g, textColor.b, 0f ), UITweener.TweenValue.Hide );
            uiColorTweenManager_TextMessage.Force( UITweener.TweenValue.Show );
        }

        [ShowIf( "showText" ), FoldoutGroup( "Text Settings" ), OnValueChanged( "SetFont" ), InfoBox("If no font is selected, Liberation will be used")]
        public Font font;
        private void SetFont()
        {
            if( uiTextField_Line1 != null && uiTextField_Line1.textField != null )
            {
                if( font != null )
                {
                    uiTextField_Line1.textField.font = font;
                }
                else
                {
                    uiTextField_Line1.textField.font = textFont;
                }
            }
        }

        [ShowIf( "showText" ), FoldoutGroup( "Text Settings" ), OnValueChanged( "SetFontSize" )]
        public int textFontSize = 35;
        private void SetFontSize() { uiTextField_Line1.textField.fontSize = textFontSize; }


        [ShowIf( "showText" ), OnValueChanged( "SetTextTweenSpeed" ), FoldoutGroup( "Text Settings" )]
        public float textTweenSpeed = 1f;
        private void SetTextTweenSpeed()
        {
            if( showText )
            {
                if( uiColorTweenManager_TextMessage != null )
                {
                    SetTweenSpeed( uiColorTweenManager_TextMessage, textTweenSpeed );
                }
                if( uiScaleTweener_TextMessage != null )
                {
                    SetTweenSpeed( uiScaleTweener_TextMessage, textTweenSpeed );
                }
            }
        }

        [ShowIf( "showText" ), OnValueChanged( "SetTextTweenSpeed" ), FoldoutGroup( "Text Settings" )]
        public EaseCurve.EaseType textEaseType = EaseCurve.EaseType.Linear;
        private void SetTextEaseType()
        {
            if( showText )
            {
                if( uiColorTweenManager_TextMessage != null )
                {
                    SetEaseType( uiColorTweenManager_TextMessage, textEaseType );
                }
                if( uiScaleTweener_TextMessage != null )
                {
                    SetEaseType( uiScaleTweener_TextMessage, textEaseType );
                }
            }
        }

        #endregion

        #region DISABLED SETTINGS
        //------------------------------------------------------------------//
        //---------------------- DISABLED SETTINGS -------------------------//
        //------------------------------------------------------------------//

        public enum WhenDisabledOptions
        {
            DoNotChange,
            Hide,
            ChangeImagesAndText
        }

        [FoldoutGroup( "Disabled Settings" ), InfoBox( "Regardless of what options you choose below, calling the Disable() method on this block will prevent Mouse, Touch, and Gaze interaction. Call Enable() to return to normal" )]
        public WhenDisabledOptions WhenDisabled = WhenDisabledOptions.Hide;
        private bool WhenDisabledDoNotChange() { return WhenDisabled == WhenDisabledOptions.DoNotChange; }
        private bool WhenDisabledChange() { return WhenDisabled != WhenDisabledOptions.DoNotChange; }
        private bool WhenDisabledHide() { return WhenDisabled == WhenDisabledOptions.Hide; }
        private bool WhenDisabledChangeImagesAndText() { return WhenDisabled == WhenDisabledOptions.ChangeImagesAndText; }

        //[Space(10), OnValueChanged( "SetDisabledStateInEditor" ), InfoBox("Turn this option on to preview what the Disabled state will look like in the Editor. If you leave this option 'On' the button will revert to it's normal settings on Start()"), ShowIf( "WhenDisabledChangeImagesAndText" ), FoldoutGroup( "Disabled Settings" )]
        private bool showDisabledStateInEditor = false;

        private void SetDisabledStateInEditor()
        {
            if( showDisabledStateInEditor ) { ShowDisabledStateInEditor(); }
            else { HideDisabledStateInEditor(); }
        }

        [Button( "Show Disabled State In Editor (Disabled when playing)", ButtonSizes.Large ), FoldoutGroup("Disabled Settings")]
        private void ShowDisabledStateInEditor()
        {
            //If the hover state is being shown, hide it so we can show the disabled state
            if( showHoverStateInEditor ) { HideHoverStateInEditor(); }

            if( IsInEditMode() )
            {
                showDisabledStateInEditor = true;
                ValidateDisabledCenterImage();
                ValidateDisabledBackgroundImage();
                ValidateDisabledParallaxImage();
                ValidateDisabledFillImage();
                ValidateDisabledFillBackgroundImage();
                ValidateDisabledText();
            }
        }

        [Button( "Hide Disabled State In Editor (Disabled when playing)", ButtonSizes.Large ), FoldoutGroup( "Disabled Settings" )]
        private void HideDisabledStateInEditor()
        {
            if( IsInEditMode() )
            {
                showDisabledStateInEditor = false;
                ValidateCenterImage();
                ValidateBackgroundImage();
                ValidateParallaxImage();
                ValidateFillImage();
                ValidateFillBackgroundImage();
                ValidateText();
            }
        }

        public bool IsInEditMode()
        {
            #if UNITY_EDITOR
                if( Application.isEditor && 
                    !UnityEditor.EditorApplication.isPlaying && 
                    !UnityEditor.EditorApplication.isPlayingOrWillChangePlaymode )
                {
                    return true;
                }
                else
                {
                    return false;
                }
            #else
                return false;
            #endif
        }

        private bool ShowDisabledImagesAndTextChangesInEditor()
        {
            if( IsInEditMode() )
            {
                return showDisabledStateInEditor;
            }
            else
            {
                return false;
            }
        }

        [Space(10f), InfoBox("Set this boolean to force Show() to be called whenever Enable() is called "), ShowIf( "WhenDisabledHide" ), FoldoutGroup( "Disabled Settings" )]
        public bool ShowAfterBeingReEnabled = true;

        
        
        private bool WhenDisabledChangeImagesAndTextAndShowingCenterImage() { return WhenDisabledChangeImagesAndText() && IsUseCenterImageTrue(); }

        [Space(10), ShowIf( "WhenDisabledChangeImagesAndTextAndShowingCenterImage" ), OnValueChanged( "ValidateDisabledCenterImage" ), FoldoutGroup("Disabled Settings") ]
        public bool changeCenterImage = false;
        private bool ShowChangeCenterImageOptions() { return WhenDisabledChangeImagesAndTextAndShowingCenterImage() && changeCenterImage; }
        
        //Used to change the image in editor based on settings.
        //Called via OnValidate method instead of using the standard OnValueChanged methods as they don't update the GameView() window
        private void ValidateDisabledCenterImage()
        {
            if( ShowDisabledImagesAndTextChangesInEditor() && useCenterImage && changeCenterImage )
            {
                SetDisabledCenterImageTexture();
                SetDisabledCenterImageScale();
                SetDisabledCenterImageColor();
                SetDisabledCenterImageTweenSpeed();
                SetDisabledCenterImageEaseType();
            }
            else
            {
                ValidateCenterImage();
            }
        }

        [Space(10), ShowIf( "ShowChangeCenterImageOptions" ), OnValueChanged( "SetDisabledCenterImageTexture"), FoldoutGroup( "Disabled Settings" ), InfoBox("Leave texture/sprite as blank to keep it the same when disabled. All other image options will change to these settings when this block is disabled.")]
        public ImageOptions disabledCenterImageFormat = ImageOptions.Texture;
        private bool IsDisabledCenterImageFormatTexture() { return ShowChangeCenterImageOptions() && disabledCenterImageFormat == ImageOptions.Texture; }
        private bool IsDisabledCenterImageFormatSprite() { return ShowChangeCenterImageOptions() && disabledCenterImageFormat == ImageOptions.Sprite; }
        private bool IsDisabledCenterImageFormatPath() { return ShowChangeCenterImageOptions() && disabledCenterImageFormat == ImageOptions.Path; }
        private void SetDisabledCenterImageTexture()
        {
            if( ShowDisabledImagesAndTextChangesInEditor() && useCenterImage && changeCenterImage )
            {
                if( uiColorTweenManager_CenterImage != null && uiColorTweenManager_CenterImage.tweeners != null && uiColorTweenManager_CenterImage.tweeners.Count > 0 && uiColorTweenManager_CenterImage.tweeners[0].image != null )
                {
                    if( IsDisabledCenterImageFormatTexture() && disabledCenterImageTexture != null )
                    {
                        uiColorTweenManager_CenterImage.tweeners[ 0 ].image.sprite = TextureHelper.CreateSpriteFromTexture( ref disabledCenterImageTexture );
                    }
                    else if( IsDisabledCenterImageFormatSprite() && disabledCenterImageSprite != null )
                    {
                        uiColorTweenManager_CenterImage.tweeners[ 0 ].image.sprite = disabledCenterImageSprite;
                    }
                    else if( IsDisabledCenterImageFormatPath() )
                    {
                        uiColorTweenManager_CenterImage.tweeners[ 0 ].image.sprite = null;
                    }
                }
            }
            else
            {
                ValidateCenterImage();
            }
        }

        [ShowIf( "IsDisabledCenterImageFormatTexture" ), OnValueChanged( "SetDisabledCenterImageTexture" ), FoldoutGroup( "Disabled Settings" )]
        public Texture disabledCenterImageTexture;
        [ShowIf( "IsDisabledCenterImageFormatSprite" ), OnValueChanged( "SetDisabledCenterImageTexture" ), FoldoutGroup( "Disabled Settings" )]
        public Sprite disabledCenterImageSprite;
        [ShowIf( "IsDisabledCenterImageFormatPath" ), OnValueChanged( "SetDisabledCenterImageTexture" ), FoldoutGroup( "Disabled Settings" )]
        public string disabledCenterImagePath;

        [ShowIf( "IsDisabledCenterImageFormatPath" ), FoldoutGroup( "Disabled Settings" )]
        public bool disabledCenterImageCacheIfWeb = true;

        private bool isDisabledCenterImageReady = false;
        
        [Range( .1f, 10f ), OnValueChanged( "SetDisabledCenterImageScale" ), ShowIf( "ShowChangeCenterImageOptions" ), FoldoutGroup( "Disabled Settings" )]
        public float disabledCenterImageScale = 1f;
        private void SetDisabledCenterImageScale()
        {
            if( uiScaleTweener_CenterImage != null )
            {
                uiScaleTweener_CenterImage.SetScale( new Vector3( disabledCenterImageScale, disabledCenterImageScale, disabledCenterImageScale ), UITweener.TweenValue.Disabled );
            }

            if( ShowDisabledImagesAndTextChangesInEditor() && useCenterImage && changeCenterImage &&
                uiScaleTweener_CenterImage != null)
            {
                uiScaleTweener_CenterImage.Force( UITweener.TweenValue.Disabled );
            }
            else
            {
                ValidateCenterImage();
            }
        }

        [ ShowIf( "ShowChangeCenterImageOptions" ), OnValueChanged( "SetDisabledCenterImageColor" ), FoldoutGroup( "Disabled Settings" )]
        public Color disabledCenterImageColor = Color.white;
        private void SetDisabledCenterImageColor()
        {
            if( uiColorTweenManager_CenterImage != null )
            {
                uiColorTweenManager_CenterImage.SetColor( disabledCenterImageColor, UITweener.TweenValue.Disabled );
            }

            if( ShowDisabledImagesAndTextChangesInEditor() && useCenterImage && changeCenterImage &&
                uiColorTweenManager_CenterImage != null)
            {
                uiColorTweenManager_CenterImage.Force( UITweener.TweenValue.Disabled );
            }
            else
            {
                ValidateCenterImage();
            }
        }
        
        [ShowIf( "ShowChangeCenterImageOptions" ), OnValueChanged( "SetDisabledCenterImageTweenSpeed" ), FoldoutGroup( "Disabled Settings" )]
        public float disabledCenterImageTweenSpeed = 1f;
        private void SetDisabledCenterImageTweenSpeed()
        {
            if( uiColorTweenManager_CenterImage != null )
            {
                uiColorTweenManager_CenterImage.SetTweenSpeed( disabledCenterImageTweenSpeed, UITweener.TweenValue.Disabled );
            }
            if( uiScaleTweener_CenterImage != null )
            {
                uiScaleTweener_CenterImage.SetTweenSpeed( disabledCenterImageTweenSpeed, UITweener.TweenValue.Disabled );
            }
        }

        [ShowIf( "ShowChangeCenterImageOptions" ), OnValueChanged( "SetDisabledCenterImageEaseType" ), FoldoutGroup( "Disabled Settings" )]
        public EaseCurve.EaseType disabledCenterImageEaseType = EaseCurve.EaseType.Linear;
        private void SetDisabledCenterImageEaseType()
        {
            if( uiColorTweenManager_CenterImage != null )
            {
                uiColorTweenManager_CenterImage.SetEaseType( disabledCenterImageEaseType, UITweener.TweenValue.Disabled );
            }
            if( uiScaleTweener_CenterImage != null )
            {
                uiScaleTweener_CenterImage.SetEaseType( disabledCenterImageEaseType, UITweener.TweenValue.Disabled );
            }
        }








        private bool WhenDisabledChangeImagesAndTextAndShowingBackgroundImage() { return WhenDisabledChangeImagesAndText() && IsUseBackgroundImageTrue(); }

        [Space( 10 ), ShowIf( "WhenDisabledChangeImagesAndTextAndShowingBackgroundImage" ), OnValueChanged( "ValidateDisabledBackgroundImage" ), FoldoutGroup( "Disabled Settings" )]
        public bool changeBackgroundImage = false;
        private bool ShowChangeBackgroundImageOptions() { return WhenDisabledChangeImagesAndTextAndShowingBackgroundImage() && changeBackgroundImage; }
        
        //Used to change the image in editor based on settings.
        //Called via OnValidate method instead of using the standard OnValueChanged methods as they don't update the GameView() window
        private void ValidateDisabledBackgroundImage()
        {
            if( ShowDisabledImagesAndTextChangesInEditor() && useBackgroundImage && changeBackgroundImage )
            {
                SetDisabledBackgroundImageTexture();
                SetDisabledBackgroundImageScale();
                SetDisabledBackgroundImageColor();
                SetDisabledBackgroundImageTweenSpeed();
                SetDisabledBackgroundImageEaseType();
            }
            else
            {
                ValidateBackgroundImage();
            }
        }

        [Space(10), ShowIf( "ShowChangeBackgroundImageOptions" ), OnValueChanged( "SetDisabledBackgroundImageTexture" ), FoldoutGroup( "Disabled Settings" )]
        public ImageOptions disabledBackgroundImageFormat = ImageOptions.Texture;
        private bool IsDisabledBackgroundImageFormatTexture() { return ShowChangeBackgroundImageOptions() && disabledBackgroundImageFormat == ImageOptions.Texture; }
        private bool IsDisabledBackgroundImageFormatSprite() { return ShowChangeBackgroundImageOptions() && disabledBackgroundImageFormat == ImageOptions.Sprite; }
        private bool IsDisabledBackgroundImageFormatPath() { return ShowChangeBackgroundImageOptions() && disabledBackgroundImageFormat == ImageOptions.Path; }
        private void SetDisabledBackgroundImageTexture()
        {
            if( ShowDisabledImagesAndTextChangesInEditor() && useBackgroundImage && changeBackgroundImage )
            {
                if( uiColorTweenManager_BackgroundImage != null && uiColorTweenManager_BackgroundImage.tweeners != null && uiColorTweenManager_BackgroundImage.tweeners.Count > 0 && uiColorTweenManager_BackgroundImage.tweeners[ 0 ].image != null )
                {
                    if( IsDisabledBackgroundImageFormatTexture() && disabledBackgroundImageTexture != null )
                    {
                        uiColorTweenManager_BackgroundImage.tweeners[ 0 ].image.sprite = TextureHelper.CreateSpriteFromTexture( ref disabledBackgroundImageTexture );
                    }
                    else if( IsDisabledBackgroundImageFormatSprite() && disabledBackgroundImageSprite != null )
                    {
                        uiColorTweenManager_BackgroundImage.tweeners[ 0 ].image.sprite = disabledBackgroundImageSprite;
                    }
                    else if( IsDisabledBackgroundImageFormatPath() )
                    {
                        uiColorTweenManager_BackgroundImage.tweeners[ 0 ].image.sprite = null;
                    }
                }
            }
            else
            {
                ValidateBackgroundImage();
            }
        }


        [ShowIf( "IsDisabledBackgroundImageFormatTexture" ), OnValueChanged( "SetDisabledBackgroundImageTexture" ), FoldoutGroup( "Disabled Settings" )]
        public Texture disabledBackgroundImageTexture;
        [ShowIf( "IsDisabledBackgroundImageFormatSprite" ), OnValueChanged( "SetDisabledBackgroundImageTexture" ), FoldoutGroup( "Disabled Settings" )]
        public Sprite disabledBackgroundImageSprite;
        [ShowIf( "IsDisabledBackgroundImageFormatPath" ), OnValueChanged( "SetDisabledBackgroundImageTexture" ), FoldoutGroup( "Disabled Settings" )]
        public string disabledBackgroundImagePath;

        [ShowIf( "IsDisabledBackgroundImageFormatPath" ), FoldoutGroup( "Disabled Settings" )]
        public bool disabledBackgroundImageCacheIfWeb = true;

        private bool isDisabledBackgroundImageReady = false;
        
        [Range( .1f, 10f ), OnValueChanged( "SetDisabledBackgroundImageScale" ), ShowIf( "ShowChangeBackgroundImageOptions" ), FoldoutGroup( "Disabled Settings" )]
        public float disabledBackgroundImageScale = 1f;
        private void SetDisabledBackgroundImageScale()
        {
            if( uiScaleTweener_BackgroundImage != null )
            {
                uiScaleTweener_BackgroundImage.SetScale( new Vector3( disabledBackgroundImageScale, disabledBackgroundImageScale, disabledBackgroundImageScale ), UITweener.TweenValue.Disabled );
            }

            if( ShowDisabledImagesAndTextChangesInEditor() && useBackgroundImage && changeBackgroundImage &&
                uiScaleTweener_BackgroundImage != null )
            {
                uiScaleTweener_BackgroundImage.Force( UITweener.TweenValue.Disabled );
            }
            else
            {
                ValidateBackgroundImage();
            }
        }

        [ShowIf( "ShowChangeBackgroundImageOptions" ), OnValueChanged( "SetDisabledBackgroundImageColor" ), FoldoutGroup( "Disabled Settings" )]
        public Color disabledBackgroundImageColor = Color.black;
        private void SetDisabledBackgroundImageColor()
        {
            if( uiColorTweenManager_BackgroundImage != null )
            {
                uiColorTweenManager_BackgroundImage.SetColor( disabledBackgroundImageColor, UITweener.TweenValue.Disabled );
            }

            if( ShowDisabledImagesAndTextChangesInEditor() && useBackgroundImage && changeBackgroundImage &&
                uiColorTweenManager_BackgroundImage != null)
            {
                uiColorTweenManager_BackgroundImage.Force( UITweener.TweenValue.Disabled );
            }
            else
            {
                ValidateBackgroundImage();
            }
        }
        
        [ShowIf( "ShowChangeBackgroundImageOptions" ), OnValueChanged( "SetDisabledBackgroundImageTweenSpeed" ), FoldoutGroup( "Disabled Settings" )]
        public float disabledBackgroundImageTweenSpeed = 1f;
        private void SetDisabledBackgroundImageTweenSpeed()
        {
            if( uiColorTweenManager_BackgroundImage != null )
            {
                uiColorTweenManager_BackgroundImage.SetTweenSpeed( disabledBackgroundImageTweenSpeed, UITweener.TweenValue.Disabled );
            }
            if( uiScaleTweener_BackgroundImage != null )
            {
                uiScaleTweener_BackgroundImage.SetTweenSpeed( disabledBackgroundImageTweenSpeed, UITweener.TweenValue.Disabled );
            }
        }

        [ShowIf( "ShowChangeBackgroundImageOptions" ), OnValueChanged( "SetDisabledBackgroundImageEaseType" ), FoldoutGroup( "Disabled Settings" )]
        public EaseCurve.EaseType disabledBackgroundImageEaseType = EaseCurve.EaseType.Linear;
        private void SetDisabledBackgroundImageEaseType()
        {
            if( uiColorTweenManager_BackgroundImage != null )
            {
                uiColorTweenManager_BackgroundImage.SetEaseType( disabledBackgroundImageEaseType, UITweener.TweenValue.Disabled );
            }
            if( uiScaleTweener_BackgroundImage != null )
            {
                uiScaleTweener_BackgroundImage.SetEaseType( disabledBackgroundImageEaseType, UITweener.TweenValue.Disabled );
            }
        }








        private bool WhenDisabledChangeImagesAndTextAndShowingParallaxImage() { return WhenDisabledChangeImagesAndText() && IsUseParallaxImageTrue(); }

        [Space( 10 ), ShowIf( "WhenDisabledChangeImagesAndTextAndShowingParallaxImage" ), OnValueChanged( "ValidateDisabledParallaxImage" ), FoldoutGroup( "Disabled Settings" )]
        public bool changeParallaxImage = false;
        private bool ShowChangeParallaxImageOptions() { return WhenDisabledChangeImagesAndTextAndShowingParallaxImage() && changeParallaxImage; }
        
        //Used to change the image in editor based on settings.
        //Called via OnValidate method instead of using the standard OnValueChanged methods as they don't update the GameView() window
        private void ValidateDisabledParallaxImage()
        {
            if( ShowDisabledImagesAndTextChangesInEditor() && useParallaxImage && changeParallaxImage )
            {
                SetDisabledParallaxImageTexture();
                SetDisabledParallaxImageScale();
                SetDisabledParallaxImageColor();
                SetDisabledParallaxImageTweenSpeed();
                SetDisabledParallaxImageEaseType();
            }
            else
            {
                ValidateParallaxImage();
            }
        }

        [Space( 10 ), ShowIf( "ShowChangeParallaxImageOptions" ), OnValueChanged( "SetDisabledParallaxImageTexture" ), FoldoutGroup( "Disabled Settings" )]
        public ImageOptions disabledParallaxImageFormat = ImageOptions.Texture;
        private bool IsDisabledParallaxImageFormatTexture() { return ShowChangeParallaxImageOptions() && disabledParallaxImageFormat == ImageOptions.Texture; }
        private bool IsDisabledParallaxImageFormatSprite() { return ShowChangeParallaxImageOptions() && disabledParallaxImageFormat == ImageOptions.Sprite; }
        private bool IsDisabledParallaxImageFormatPath() { return ShowChangeParallaxImageOptions() && disabledParallaxImageFormat == ImageOptions.Path; }
        private void SetDisabledParallaxImageTexture()
        {
            if( ShowDisabledImagesAndTextChangesInEditor() && useParallaxImage && changeParallaxImage )
            {
                if( uiColorTweenManager_ParallaxImage != null && uiColorTweenManager_ParallaxImage.tweeners != null && uiColorTweenManager_ParallaxImage.tweeners.Count > 0 && uiColorTweenManager_ParallaxImage.tweeners[ 0 ].image != null )
                {
                    if( IsDisabledParallaxImageFormatTexture() && disabledParallaxImageTexture != null )
                    {
                        uiColorTweenManager_ParallaxImage.tweeners[ 0 ].image.sprite = TextureHelper.CreateSpriteFromTexture( ref disabledParallaxImageTexture );
                    }
                    else if( IsDisabledParallaxImageFormatSprite() && disabledParallaxImageSprite != null )
                    {
                        uiColorTweenManager_ParallaxImage.tweeners[ 0 ].image.sprite = disabledParallaxImageSprite;
                    }
                    else if( IsDisabledParallaxImageFormatPath() )
                    {
                        uiColorTweenManager_ParallaxImage.tweeners[ 0 ].image.sprite = null;
                    }
                }
            }
            else
            {
                ValidateParallaxImage();
            }
        }

        [ShowIf( "IsDisabledParallaxImageFormatTexture" ), OnValueChanged( "SetDisabledParallaxImageTexture" ), FoldoutGroup( "Disabled Settings" )]
        public Texture disabledParallaxImageTexture;
        [ShowIf( "IsDisabledParallaxImageFormatSprite" ), OnValueChanged( "SetDisabledParallaxImageTexture" ), FoldoutGroup( "Disabled Settings" )]
        public Sprite disabledParallaxImageSprite;
        [ShowIf( "IsDisabledParallaxImageFormatPath" ), OnValueChanged( "SetDisabledParallaxImageTexture" ), FoldoutGroup( "Disabled Settings" )]
        public string disabledParallaxImagePath;

        [ShowIf( "IsDisabledParallaxImageFormatPath" ), FoldoutGroup( "Disabled Settings" )]
        public bool disabledParallaxImageCacheIfWeb = true;

        private bool isDisabledParallaxImageReady = false;

        [Range( .1f, 10f ), ShowIf( "ShowChangeParallaxImageOptions" ), OnValueChanged( "SetDisabledParallaxImageScale" ), FoldoutGroup( "Disabled Settings" )]
        public float disabledParallaxImageScale = 1f;
        private void SetDisabledParallaxImageScale()
        {
            if( uiScaleTweener_ParallaxImage != null )
            {
                uiScaleTweener_ParallaxImage.SetScale( new Vector3( disabledParallaxImageScale, disabledParallaxImageScale, disabledParallaxImageScale ), UITweener.TweenValue.Disabled );
            }

            if( ShowDisabledImagesAndTextChangesInEditor() && useParallaxImage && changeParallaxImage &&
                uiScaleTweener_ParallaxImage != null )
            {
                uiScaleTweener_ParallaxImage.Force( UITweener.TweenValue.Disabled );
            }
            else
            {
                ValidateParallaxImage();
            }
        }

        [ShowIf( "ShowChangeParallaxImageOptions" ), OnValueChanged( "SetDisabledParallaxImageColor" )]
        public Color disabledParallaxImageColor = Color.white;
        private void SetDisabledParallaxImageColor()
        {
            if( uiColorTweenManager_ParallaxImage != null )
            {
                uiColorTweenManager_ParallaxImage.SetColor( disabledParallaxImageColor, UITweener.TweenValue.Disabled );
            }

            if( ShowDisabledImagesAndTextChangesInEditor() && useParallaxImage && changeParallaxImage &&
                uiColorTweenManager_ParallaxImage != null)
            {
                uiColorTweenManager_ParallaxImage.Force( UITweener.TweenValue.Disabled );
            }
            else
            {
                ValidateParallaxImage();
            }
        }
        
        [ShowIf( "ShowChangeParallaxImageOptions" ), OnValueChanged( "SetDisabledParallaxImageTweenSpeed" ), FoldoutGroup( "Disabled Settings" )]
        public float disabledParallaxImageTweenSpeed = 1f;
        private void SetDisabledParallaxImageTweenSpeed()
        {
            if( uiColorTweenManager_ParallaxImage != null )
            {
                uiColorTweenManager_ParallaxImage.SetTweenSpeed( disabledParallaxImageTweenSpeed, UITweener.TweenValue.Disabled );
            }
            if( uiScaleTweener_ParallaxImage != null )
            {
                uiScaleTweener_ParallaxImage.SetTweenSpeed( disabledParallaxImageTweenSpeed, UITweener.TweenValue.Disabled );
            }
        }

        [ShowIf( "ShowChangeParallaxImageOptions" ), OnValueChanged( "SetDisabledParallaxImageEaseType" ), FoldoutGroup( "Disabled Settings" )]
        public EaseCurve.EaseType disabledParallaxImageEaseType = EaseCurve.EaseType.Linear;
        private void SetDisabledParallaxImageEaseType()
        {
            if( uiColorTweenManager_ParallaxImage != null )
            {
                uiColorTweenManager_ParallaxImage.SetEaseType( disabledParallaxImageEaseType, UITweener.TweenValue.Disabled );
            }
            if( uiScaleTweener_ParallaxImage != null )
            {
                uiScaleTweener_ParallaxImage.SetEaseType( disabledParallaxImageEaseType, UITweener.TweenValue.Disabled );
            }
        }






        private bool WhenDisabledChangeImagesAndTextAndShowText() { return showText && WhenDisabledChangeImagesAndText(); }

        [Space( 15f ), ShowIf( "WhenDisabledChangeImagesAndTextAndShowText" ), OnValueChanged( "ValidateDisabledText" ), FoldoutGroup("Disabled Settings")]
        public bool changeText = false;
        private bool ShowChangeTextOptions() { return WhenDisabledChangeImagesAndTextAndShowText() && changeText; }

        //Used to change the image in editor based on settings.
        //Called via OnValidate method instead of using the standard OnValueChanged methods as they don't update the GameView() window
        private void ValidateDisabledText()
        {
            if( ShowDisabledImagesAndTextChangesInEditor() && showText && changeText )
            {
                SetDisabledTextMessage();
                SetDisabledTextScale();
                SetDisabledTextColor();
                SetDisabledTextFont();
                SetDisabledTextSize();
                SetDisabledTextTweenSpeed();
                SetDisabledTextEaseType();
            }
            else
            {
                ValidateText();
            }
        }

        [ ShowIf( "ShowChangeTextOptions" ), OnValueChanged( "SetDisabledTextMessage" ), FoldoutGroup( "Disabled Settings" )]
        public string disabledTextMessage = "Disabled";
        private void SetDisabledTextMessage()
        {
            if( ShowDisabledImagesAndTextChangesInEditor() && showText && changeText )
            {
                if( name == debugWhenName ) Debug.Log( "SetDisabledTextMessage() text = " + disabledTextMessage );
                uiTextField_Line1.text = disabledTextMessage;
            }
            else
            {
                if( name == debugWhenName ) Debug.Log( "SetDisabledTextMessage() unable to set, calling ValidateText()" );
                ValidateText();
            }
        }

        [Range( .1f, 10f ), ShowIf( "ShowChangeTextOptions" ), OnValueChanged( "SetDisabledTextScale" ), FoldoutGroup( "Disabled Settings" )]
        public float disabledTextScale = 1f;
        private void SetDisabledTextScale()
        {
            if( uiScaleTweener_TextMessage != null && uiScaleTweener_TextMessage.GetTransform() != null )
            {
                uiScaleTweener_TextMessage.SetScale( new Vector3( disabledTextScale, disabledTextScale, disabledTextScale ), UITweener.TweenValue.Disabled );
            }

            if( showDisabledStateInEditor && showText && changeText &&
                uiScaleTweener_TextMessage != null )
            {
                uiScaleTweener_TextMessage.Force( UITweener.TweenValue.Disabled );
            }
            else
            {
                ValidateText();
            }
        }
        
        [ ShowIf( "ShowChangeTextOptions" ), OnValueChanged( "SetDisabledTextColor" ), FoldoutGroup( "Disabled Settings" )]
        public Color disabledTextColor = Color.white;
        private void SetDisabledTextColor()
        {
            if( uiColorTweenManager_TextMessage != null )
            {
                uiColorTweenManager_TextMessage.SetColor( disabledTextColor, UITweener.TweenValue.Disabled );
            }

            if( showDisabledStateInEditor && showText && changeText &&
                uiColorTweenManager_TextMessage != null)
            {
                uiColorTweenManager_TextMessage.Force( UITweener.TweenValue.Disabled );
            }
            else
            {
                ValidateText();
            }
        }

        [ShowIf( "ShowChangeTextOptions" ), OnValueChanged( "SetDisabledTextFont"), InfoBox( "If font is left blank, Liberation will be used" ), FoldoutGroup( "Disabled Settings" )]
        public Font disabledTextFont;
        private void SetDisabledTextFont()
        {
            if( showDisabledStateInEditor && showText && changeText )
            {
                uiTextField_Line1.textField.font = disabledTextFont;
            }
            else
            {
                ValidateText();
            }
        }

        [ ShowIf( "ShowChangeTextOptions" ), OnValueChanged("SetDisabledTextSize"), FoldoutGroup( "Disabled Settings" )]
        public int disabledTextFontSize = 35;
        private void SetDisabledTextSize()
        {
            if( showDisabledStateInEditor && showText && changeText )
            {
                uiTextField_Line1.textField.fontSize = disabledTextFontSize;
            }
            else
            {
                ValidateText();
            }
        }
        
        [ShowIf( "ShowChangeTextOptions" ), OnValueChanged( "SetDisabledTextTweenSpeed" ), FoldoutGroup( "Disabled Settings" )]
        public float disabledTextTweenSpeed = 1f;
        private void SetDisabledTextTweenSpeed()
        {
            if( uiColorTweenManager_TextMessage != null )
            {
                uiColorTweenManager_TextMessage.SetTweenSpeed( disabledTextTweenSpeed, UITweener.TweenValue.Disabled );
            }
            if( uiScaleTweener_TextMessage != null )
            {
                uiScaleTweener_TextMessage.SetTweenSpeed( disabledTextTweenSpeed, UITweener.TweenValue.Disabled );
            }
        }

        [ShowIf( "ShowChangeTextOptions" ), OnValueChanged( "SetTextImageEaseType" ), FoldoutGroup( "Disabled Settings" )]
        public EaseCurve.EaseType disabledTextEaseType = EaseCurve.EaseType.Linear;
        private void SetDisabledTextEaseType()
        {
            if( uiColorTweenManager_TextMessage != null )
            {
                uiColorTweenManager_TextMessage.SetEaseType( disabledTextEaseType, UITweener.TweenValue.Disabled );
            }
            if( uiScaleTweener_TextMessage != null )
            {
                uiScaleTweener_TextMessage.SetEaseType( disabledTextEaseType, UITweener.TweenValue.Disabled );
            }
        }







        private bool ShowWhenDisabledHideFillImageOption() { return WhenDisabledChangeImagesAndText() && useFillImage; }

        [Space( 15f ), InfoBox("All selection logic (including gaze to fill) is turned off when disabled, these two options will merely hide the fill image and it's background"), ShowIf( "ShowWhenDisabledHideFillImageOption" ), OnValueChanged( "ValidateDisabledFillImage" ), FoldoutGroup( "Disabled Settings" )]
        public bool whenDisabledHideFillImage = false;
        private void ValidateDisabledFillImage()
        {
            if( ShowDisabledImagesAndTextChangesInEditor() )
            {
                if( useFillImage && !whenDisabledHideFillImage )
                {
                    uiColorTweenManager_FillImage.gameObject.SetActive( true );
                }
                else if( useFillImage && whenDisabledHideFillImage )
                {
                    uiColorTweenManager_FillImage.gameObject.SetActive( false );
                }
                else
                {
                    uiColorTweenManager_FillImage.gameObject.SetActive( false );
                }
            }
            else
            {
                ValidateFillImage();
            }
        }

        private bool ShowWhenDisabledHideFillBackgroundImageOption() { return WhenDisabledChangeImagesAndText() && useFillBackgroundImage; }

        [ShowIf( "ShowWhenDisabledHideFillBackgroundImageOption" ), OnValueChanged( "ValidateDisabledFillBackgroundImage" ), FoldoutGroup( "Disabled Settings" )]
        public bool whenDisabledHideFillBackgroundImage = false;
        private void ValidateDisabledFillBackgroundImage()
        {
            if( ShowDisabledImagesAndTextChangesInEditor() )
            {
                if( useFillBackgroundImage && !whenDisabledHideFillBackgroundImage )
                {
                    uiColorTweenManager_FillBackgroundImage.gameObject.SetActive( true );
                }
                else if( useFillBackgroundImage && whenDisabledHideFillBackgroundImage )
                {
                    uiColorTweenManager_FillBackgroundImage.gameObject.SetActive( false );
                }
                else
                {
                    uiColorTweenManager_FillBackgroundImage.gameObject.SetActive( false );
                }
            }
            else
            {
                ValidateFillBackgroundImage();
            }
        }



        private bool ShowWhenDisabledStopIdleAnimationOption() { return WhenDisabledChangeImagesAndText() && playAnimationDuringIdleState != AnimationOptions.NoAnimation; }
        
        [Space( 15f ), ShowIf( "ShowWhenDisabledStopIdleAnimationOption" ), FoldoutGroup( "Disabled Settings" )]
        public bool whenDisabledStopIdleAnimation = false;

        

        [Space(15f), ShowIf( "WhenDisabledChange" ), FoldoutGroup("Disabled Settings")]
        public bool disableWhenNoInternetAccess = false;

        #endregion

        #region HOVER SETTINGS
        //------------------------------------------------------------------//
        //------------------ HOVER SETTINGS --------------------------------//
        //------------------------------------------------------------------//
        public enum WhenHoveringOptions
        {
            DoNotChange,
            ChangeImagesAndText
        }

        [FoldoutGroup( "Hover Settings" ), InfoBox("The Hover settings below will be applied and or tweened to when a Mouse input enters and exits the collider for this BlockButton") ]
        public WhenHoveringOptions WhenHovering = WhenHoveringOptions.DoNotChange;
        private bool WhenHoveringDoNotChange() { return WhenHovering == WhenHoveringOptions.DoNotChange; }
        private bool WhenHoveringChange() { return WhenHovering != WhenHoveringOptions.DoNotChange; }
        private bool WhenHoveringChangeImagesAndText() { return WhenHovering == WhenHoveringOptions.ChangeImagesAndText; }

        //[Space(10), OnValueChanged( "ShowHoverStateInEditor" ), InfoBox("Turn this option on to preview what the Hover state will look like in the Editor. If you leave this option 'On' the button will revert to it's normal settings on Start()"), ShowIf( "WhenHoveringChangeImagesAndText" ), FoldoutGroup( "Hover Settings" )]
        private bool showHoverStateInEditor = false;
        
        [Button( "Show Hover State In Editor (Disabled when playing)", ButtonSizes.Large ), FoldoutGroup( "Hover Settings" )]
        private void ShowHoverStateInEditor()
        {
            //If the disabled state is being shown in the editor, hide it!
            if( showDisabledStateInEditor ) { HideDisabledStateInEditor(); }

            if( IsInEditMode() )
            {
                showHoverStateInEditor = true;
                ValidateHoverCenterImage();
                ValidateHoverBackgroundImage();
                ValidateHoverParallaxImage();
                ValidateHoverFillImage();
                ValidateHoverFillBackgroundImage();
                ValidateHoverText();
            }
            
        }

        [Button( "Hide Hover State In Editor (Disabled when playing)", ButtonSizes.Large ), FoldoutGroup( "Hover Settings" )]
        private void HideHoverStateInEditor()
        {
            if( IsInEditMode() )
            {
                showDisabledStateInEditor = false;
                ValidateCenterImage();
                ValidateBackgroundImage();
                ValidateParallaxImage();
                ValidateFillImage();
                ValidateFillBackgroundImage();
                ValidateText();
            }
            
        }

        private bool ShowHoverImagesAndTextChangesInEditor()
        {
            if( IsInEditMode() )
            {
                return showHoverStateInEditor;
            }
            else
            {
                return false;
            }
        }

        private bool WhenHoveringChangeImagesAndTextAndShowingCenterImage() { return WhenHoveringChangeImagesAndText() && IsUseCenterImageTrue(); }

        [Space( 10 ), ShowIf( "WhenHoveringChangeImagesAndTextAndShowingCenterImage" ), OnValueChanged( "ValidateHoverCenterImage" ), FoldoutGroup( "Hover Settings" )]
        public bool changeCenterImageDuringHover = false;
        private bool ShowChangeCenterImageDuringHoverOptions() { return WhenHoveringChangeImagesAndTextAndShowingCenterImage() && changeCenterImageDuringHover; }

        //Used to change the image in editor based on settings.
        private void ValidateHoverCenterImage()
        {
            if( ShowHoverImagesAndTextChangesInEditor() && useCenterImage && changeCenterImageDuringHover )
            {
                SetHoverCenterImageTexture();
                SetHoverCenterImageScale();
                SetHoverCenterImageColor();
                SetHoverEnterCenterImageTweenSpeed();
                SetHoverExitCenterImageTweenSpeed();
                SetHoverEnterCenterImageEaseType();
                SetHoverExitCenterImageEaseType();
            }
            else
            {
                ValidateCenterImage();
            }
            
        }

        [Space( 10 ), ShowIf( "ShowChangeCenterImageDuringHoverOptions" ), OnValueChanged( "SetHoverCenterImageTexture" ), FoldoutGroup( "Hover Settings" ), InfoBox( "Leave texture/sprite as blank to keep it the same when being hovered over. All other image options will change to these settings when this block is being hovered over." )]
        public ImageOptions hoverCenterImageFormat = ImageOptions.Texture;
        private bool IsHoverCenterImageFormatTexture() { return ShowChangeCenterImageDuringHoverOptions() && hoverCenterImageFormat == ImageOptions.Texture; }
        private bool IsHoverCenterImageFormatSprite() { return ShowChangeCenterImageDuringHoverOptions() && hoverCenterImageFormat == ImageOptions.Sprite; }
        private bool IsHoverCenterImageFormatPath() { return ShowChangeCenterImageDuringHoverOptions() && hoverCenterImageFormat == ImageOptions.Path; }
        private void SetHoverCenterImageTexture()
        {
            if( ShowHoverImagesAndTextChangesInEditor() && useCenterImage && changeCenterImageDuringHover )
            {
                if( uiColorTweenManager_CenterImage != null && uiColorTweenManager_CenterImage.tweeners != null && uiColorTweenManager_CenterImage.tweeners.Count > 0 && uiColorTweenManager_CenterImage.tweeners[ 0 ].image != null )
                {
                    if( IsHoverCenterImageFormatTexture() && hoverCenterImageTexture != null )
                    {
                        uiColorTweenManager_CenterImage.tweeners[ 0 ].image.sprite = TextureHelper.CreateSpriteFromTexture( ref hoverCenterImageTexture );
                    }
                    else if( IsHoverCenterImageFormatSprite() && hoverCenterImageSprite != null )
                    {
                        uiColorTweenManager_CenterImage.tweeners[ 0 ].image.sprite = hoverCenterImageSprite;
                    }
                    else if( IsHoverCenterImageFormatPath() )
                    {
                        uiColorTweenManager_CenterImage.tweeners[ 0 ].image.sprite = null;
                    }
                }
            }
            else
            {
                ValidateCenterImage();
            }
        }

        [ShowIf( "IsHoverCenterImageFormatTexture" ), OnValueChanged( "SetHoverCenterImageTexture" ), FoldoutGroup( "Hover Settings" )]
        public Texture hoverCenterImageTexture;
        [ShowIf( "IsHoverCenterImageFormatSprite" ), OnValueChanged( "SetHoverCenterImageTexture" ), FoldoutGroup( "Hover Settings" )]
        public Sprite hoverCenterImageSprite;
        [ShowIf( "IsHoverCenterImageFormatPath" ), OnValueChanged( "SetHoverCenterImageTexture" ), FoldoutGroup( "Hover Settings" )]
        public string hoverCenterImagePath;

        [ShowIf( "IsDisabledCenterImageFormatPath" ), FoldoutGroup( "Hover Settings" )]
        public bool hoverCenterImageCacheIfWeb = true;

        private bool isHoverCenterImageReady = false;

        [Range( .1f, 10f ), OnValueChanged( "SetHoverCenterImageScale" ), ShowIf( "ShowChangeCenterImageDuringHoverOptions" ), FoldoutGroup( "Hover Settings" )]
        public float hoverCenterImageScale = 1;
        private void SetHoverCenterImageScale()
        {
            if( uiScaleTweener_CenterImage != null )
            {
                uiScaleTweener_CenterImage.SetScale( new Vector3( hoverCenterImageScale, hoverCenterImageScale, hoverCenterImageScale ), UITweener.TweenValue.HoverEnter );
            }

            if( ShowHoverImagesAndTextChangesInEditor() && useCenterImage && changeCenterImageDuringHover &&
                uiScaleTweener_CenterImage != null )
            {
                uiScaleTweener_CenterImage.Force( UITweener.TweenValue.HoverEnter );
            }
            else
            {
                ValidateCenterImage();
            }
        }

        [ShowIf( "ShowChangeCenterImageDuringHoverOptions" ), OnValueChanged( "SetHoverCenterImageColor" ), FoldoutGroup( "Hover Settings" )]
        public Color hoverCenterImageColor = Color.white;
        private void SetHoverCenterImageColor()
        {
            if( uiColorTweenManager_CenterImage != null )
            {
                uiColorTweenManager_CenterImage.SetColor( hoverCenterImageColor, UITweener.TweenValue.HoverEnter );
            }

            if( ShowHoverImagesAndTextChangesInEditor() && useCenterImage && changeCenterImageDuringHover &&
                uiColorTweenManager_CenterImage != null )
            {
                uiColorTweenManager_CenterImage.Force( UITweener.TweenValue.HoverEnter );
            }
            else
            {
                ValidateCenterImage();
            }
        }
        
        [ShowIf( "ShowChangeCenterImageDuringHoverOptions" ), OnValueChanged( "SetHoverEnterCenterImageTweenSpeed" ), FoldoutGroup( "Hover Settings" )]
        public float hoverEnterCenterImageTweenSpeed = .5f; // = .5f
        private void SetHoverEnterCenterImageTweenSpeed()
        {
            if( uiColorTweenManager_CenterImage != null )
            {
                if( debugWhenName == name ) { Debug.Log( "SetHoverEnterCenterTweenSpeed = " + hoverEnterCenterImageTweenSpeed ); }
                uiColorTweenManager_CenterImage.SetTweenSpeed( hoverEnterCenterImageTweenSpeed, UITweener.TweenValue.HoverEnter );
            }
            if( uiScaleTweener_CenterImage != null )
            {
                uiScaleTweener_CenterImage.SetTweenSpeed( hoverEnterCenterImageTweenSpeed, UITweener.TweenValue.HoverEnter );
            }
        }

        [ShowIf( "ShowChangeCenterImageDuringHoverOptions" ), OnValueChanged( "SetHoverExitCenterImageTweenSpeed" ), FoldoutGroup( "Hover Settings" )]
        public float hoverExitCenterImageTweenSpeed = .5f;
        private void SetHoverExitCenterImageTweenSpeed()
        {
            if( uiColorTweenManager_CenterImage != null )
            {
                uiColorTweenManager_CenterImage.SetTweenSpeed( hoverExitCenterImageTweenSpeed, UITweener.TweenValue.HoverExit );
            }
            if( uiScaleTweener_CenterImage != null )
            {
                uiScaleTweener_CenterImage.SetTweenSpeed( hoverExitCenterImageTweenSpeed, UITweener.TweenValue.HoverExit );
            }
        }

        [ShowIf( "ShowChangeCenterImageDuringHoverOptions" ), OnValueChanged( "SetHoverEnterCenterImageEaseType" ), FoldoutGroup( "Hover Settings" )]
        public EaseCurve.EaseType hoverEnterCenterImageEaseType = EaseCurve.EaseType.Linear;
        private void SetHoverEnterCenterImageEaseType()
        {
            if( uiColorTweenManager_CenterImage != null )
            {
                uiColorTweenManager_CenterImage.SetEaseType( hoverEnterCenterImageEaseType, UITweener.TweenValue.HoverEnter );
            }
            if( uiScaleTweener_CenterImage != null )
            {
                uiScaleTweener_CenterImage.SetEaseType( hoverEnterCenterImageEaseType, UITweener.TweenValue.HoverEnter );
            }
        }

        [ShowIf( "ShowChangeCenterImageDuringHoverOptions" ), OnValueChanged( "SetHoverExitCenterImageEaseType" ), FoldoutGroup( "Hover Settings" )]
        public EaseCurve.EaseType hoverExitCenterImageEaseType = EaseCurve.EaseType.Linear;
        private void SetHoverExitCenterImageEaseType()
        {
            if( uiColorTweenManager_CenterImage != null )
            {
                uiColorTweenManager_CenterImage.SetEaseType( hoverExitCenterImageEaseType, UITweener.TweenValue.HoverExit );
            }
            if( uiScaleTweener_CenterImage != null )
            {
                uiScaleTweener_CenterImage.SetEaseType( hoverExitCenterImageEaseType, UITweener.TweenValue.HoverExit );
            }
        }








        private bool WhenHoveringChangeImagesAndTextAndShowingBackgroundImage() { return WhenHoveringChangeImagesAndText() && IsUseBackgroundImageTrue(); }

        [Space( 10 ), ShowIf( "WhenHoveringChangeImagesAndTextAndShowingBackgroundImage" ), OnValueChanged( "ValidateHoverBackgroundImage" ), FoldoutGroup( "Hover Settings" )]
        public bool changeBackgroundImageDuringHover = false;
        private bool ShowChangeBackgroundImageDuringHoverOptions() { return WhenHoveringChangeImagesAndTextAndShowingBackgroundImage() && changeBackgroundImageDuringHover; }

        //Used to change the image in editor based on settings.
        private void ValidateHoverBackgroundImage()
        {
            if( ShowHoverImagesAndTextChangesInEditor() && useBackgroundImage && changeBackgroundImageDuringHover )
            {
                SetHoverBackgroundImageTexture();
                SetHoverBackgroundImageScale();
                SetHoverBackgroundImageColor();
                SetHoverEnterBackgroundImageTweenSpeed();
                SetHoverExitBackgroundImageTweenSpeed();
                SetHoverEnterBackgroundImageEaseType();
                SetHoverExitBackgroundImageEaseType();
            }
            else
            {
                ValidateBackgroundImage();
            }
        }
        
        [Space( 10 ), ShowIf( "ShowChangeBackgroundImageDuringHoverOptions" ), OnValueChanged( "SetHoverBackgroundImageTexture" ), FoldoutGroup( "Hover Settings" )]
        public ImageOptions hoverBackgroundImageFormat = ImageOptions.Texture;
        private bool IsHoverBackgroundImageFormatTexture() { return ShowChangeBackgroundImageDuringHoverOptions() && hoverBackgroundImageFormat == ImageOptions.Texture; }
        private bool IsHoverBackgroundImageFormatSprite() { return ShowChangeBackgroundImageDuringHoverOptions() && hoverBackgroundImageFormat == ImageOptions.Sprite; }
        private bool IsHoverBackgroundImageFormatPath() { return ShowChangeBackgroundImageDuringHoverOptions() && hoverBackgroundImageFormat == ImageOptions.Path; }
        private void SetHoverBackgroundImageTexture()
        {
            if( ShowHoverImagesAndTextChangesInEditor() && useBackgroundImage && changeBackgroundImageDuringHover )
            {
                if( uiColorTweenManager_BackgroundImage != null && uiColorTweenManager_BackgroundImage.tweeners != null && uiColorTweenManager_BackgroundImage.tweeners.Count > 0 && uiColorTweenManager_BackgroundImage.tweeners[ 0 ].image != null )
                {
                    if( IsHoverBackgroundImageFormatTexture() && hoverBackgroundImageTexture != null )
                    {
                        uiColorTweenManager_BackgroundImage.tweeners[ 0 ].image.sprite = TextureHelper.CreateSpriteFromTexture( ref hoverBackgroundImageTexture );
                    }
                    else if( IsHoverBackgroundImageFormatSprite() && hoverBackgroundImageSprite != null )
                    {
                        uiColorTweenManager_BackgroundImage.tweeners[ 0 ].image.sprite = hoverBackgroundImageSprite;
                    }
                    else if( IsHoverBackgroundImageFormatPath() )
                    {
                        uiColorTweenManager_BackgroundImage.tweeners[ 0 ].image.sprite = null;
                    }
                }
            }
            else
            {
                ValidateBackgroundImage();
            }
        }



        [ShowIf( "IsHoverBackgroundImageFormatTexture" ), OnValueChanged( "SetHoverBackgroundImageTexture" ), FoldoutGroup( "Hover Settings" )]
        public Texture hoverBackgroundImageTexture;
        [ShowIf( "IsHoverBackgroundImageFormatSprite" ), OnValueChanged( "SetHoverBackgroundImageTexture" ), FoldoutGroup( "Hover Settings" )]
        public Sprite hoverBackgroundImageSprite;
        [ShowIf( "IsHoverBackgroundImageFormatPath" ), OnValueChanged( "SetHoverBackgroundImageTexture" ), FoldoutGroup( "Hover Settings" )]
        public string hoverBackgroundImagePath;

        [ShowIf( "IsHoverBackgroundImageFormatPath" ), FoldoutGroup( "Hover Settings" )]
        public bool hoverBackgroundImageCacheIfWeb = true;

        private bool isHoverBackgroundImageReady = false;

        [Range( .1f, 10f ), OnValueChanged( "SetHoverBackgroundImageScale" ), ShowIf( "ShowChangeBackgroundImageDuringHoverOptions" ), FoldoutGroup( "Hover Settings" )]
        public float hoverBackgroundImageScale = 1f;
        private void SetHoverBackgroundImageScale()
        {
            if( uiScaleTweener_BackgroundImage != null )
            {
                uiScaleTweener_BackgroundImage.SetScale( new Vector3( hoverBackgroundImageScale, hoverBackgroundImageScale, hoverBackgroundImageScale ), UITweener.TweenValue.HoverEnter );
            }

            if( ShowHoverImagesAndTextChangesInEditor() && useBackgroundImage && changeBackgroundImageDuringHover &&
                uiScaleTweener_BackgroundImage != null )
            {
                uiScaleTweener_BackgroundImage.Force( UITweener.TweenValue.HoverEnter );
            }
            else
            {
                ValidateBackgroundImage();
            }
        }


        [ShowIf( "ShowChangeBackgroundImageDuringHoverOptions" ), OnValueChanged( "SetHoverBackgroundImageColor" ), FoldoutGroup( "Hover Settings" )]
        public Color hoverBackgroundImageColor = Color.black;
        private void SetHoverBackgroundImageColor()
        {
            if( uiColorTweenManager_BackgroundImage != null )
            {
                uiColorTweenManager_BackgroundImage.SetColor( hoverBackgroundImageColor, UITweener.TweenValue.HoverEnter );
            }

            if( ShowHoverImagesAndTextChangesInEditor() && useBackgroundImage && changeBackgroundImageDuringHover &&
                uiColorTweenManager_BackgroundImage != null )
            {
                uiColorTweenManager_BackgroundImage.Force( UITweener.TweenValue.HoverEnter );
            }
            else
            {
                ValidateBackgroundImage();
            }
        }
        
        [ShowIf( "ShowChangeBackgroundImageDuringHoverOptions" ), OnValueChanged( "SetHoverEnterBackgroundImageTweenSpeed" ), FoldoutGroup( "Hover Settings" )]
        public float hoverEnterBackgroundImageTweenSpeed = 1f;
        private void SetHoverEnterBackgroundImageTweenSpeed()
        {
            if( uiColorTweenManager_BackgroundImage != null )
            {
                uiColorTweenManager_BackgroundImage.SetTweenSpeed( hoverEnterBackgroundImageTweenSpeed, UITweener.TweenValue.HoverEnter );
            }
            if( uiScaleTweener_BackgroundImage != null )
            {
                uiScaleTweener_BackgroundImage.SetTweenSpeed( hoverEnterBackgroundImageTweenSpeed, UITweener.TweenValue.HoverEnter );
            }
        }

        [ShowIf( "ShowChangeBackgroundImageDuringHoverOptions" ), OnValueChanged( "SetHoverExitBackgroundImageTweenSpeed" ), FoldoutGroup( "Hover Settings" )]
        public float hoverExitBackgroundImageTweenSpeed = 1f;
        private void SetHoverExitBackgroundImageTweenSpeed()
        {
            if( uiColorTweenManager_BackgroundImage != null )
            {
                uiColorTweenManager_BackgroundImage.SetTweenSpeed( hoverExitBackgroundImageTweenSpeed, UITweener.TweenValue.HoverExit );
            }
            if( uiScaleTweener_BackgroundImage != null )
            {
                uiScaleTweener_BackgroundImage.SetTweenSpeed( hoverExitBackgroundImageTweenSpeed, UITweener.TweenValue.HoverExit );
            }
        }

        [ShowIf( "ShowChangeBackgroundImageDuringHoverOptions" ), OnValueChanged( "SetHoverEnterBackgroundImageEaseType" ), FoldoutGroup( "Hover Settings" )]
        public EaseCurve.EaseType hoverEnterBackgroundImageEaseType = EaseCurve.EaseType.Linear;
        private void SetHoverEnterBackgroundImageEaseType()
        {
            if( uiColorTweenManager_BackgroundImage != null )
            {
                uiColorTweenManager_BackgroundImage.SetEaseType( hoverEnterBackgroundImageEaseType, UITweener.TweenValue.HoverEnter );
            }
            if( uiScaleTweener_BackgroundImage != null )
            {
                uiScaleTweener_BackgroundImage.SetEaseType( hoverEnterBackgroundImageEaseType, UITweener.TweenValue.HoverEnter );
            }
        }

        [ShowIf( "ShowChangeBackgroundImageDuringHoverOptions" ), OnValueChanged( "SetHoverExitBackgroundImageEaseType" ), FoldoutGroup( "Hover Settings" )]
        public EaseCurve.EaseType hoverExitBackgroundImageEaseType = EaseCurve.EaseType.Linear;
        private void SetHoverExitBackgroundImageEaseType()
        {
            if( uiColorTweenManager_BackgroundImage != null )
            {
                uiColorTweenManager_BackgroundImage.SetEaseType( hoverExitBackgroundImageEaseType, UITweener.TweenValue.HoverExit );
            }
            if( uiScaleTweener_BackgroundImage != null )
            {
                uiScaleTweener_BackgroundImage.SetEaseType( hoverExitBackgroundImageEaseType, UITweener.TweenValue.HoverExit );
            }
        }









        private bool WhenHoveringChangeImagesAndTextAndShowingParallaxImage() { return WhenHoveringChangeImagesAndText() && IsUseParallaxImageTrue(); }

        [Space( 10 ), ShowIf( "WhenHoveringChangeImagesAndTextAndShowingParallaxImage" ), OnValueChanged( "ValidateHoverParallaxImage" ), FoldoutGroup( "Hover Settings" )]
        public bool changeParallaxImageDuringHover = false;
        private bool ShowChangeParallaxImageDuringHoverOptions() { return WhenHoveringChangeImagesAndTextAndShowingParallaxImage() && changeParallaxImageDuringHover; }

        //Used to change the image in editor based on settings.
        private void ValidateHoverParallaxImage()
        {
            if( ShowHoverImagesAndTextChangesInEditor() && useParallaxImage && changeParallaxImageDuringHover )
            {
                SetHoverParallaxImageTexture();
                SetHoverParallaxImageScale();
                SetHoverParallaxImageColor();
                SetHoverEnterParallaxImageTweenSpeed();
                SetHoverExitParallaxImageTweenSpeed();
                SetHoverEnterParallaxImageEaseType();
                SetHoverExitParallaxImageEaseType();
            }
            else
            {
                ValidateParallaxImage();
            }
        }

        [Space( 10 ), ShowIf( "ShowChangeParallaxImageDuringHoverOptions" ), OnValueChanged( "SetHoverParallaxImageTexture" ), FoldoutGroup( "Hover Settings" )]
        public ImageOptions hoverParallaxImageFormat = ImageOptions.Texture;
        private bool IsHoverParallaxImageFormatTexture() { return ShowChangeParallaxImageDuringHoverOptions() && hoverParallaxImageFormat == ImageOptions.Texture; }
        private bool IsHoverParallaxImageFormatSprite() { return ShowChangeParallaxImageDuringHoverOptions() && hoverParallaxImageFormat == ImageOptions.Sprite; }
        private bool IsHoverParallaxImageFormatPath() { return ShowChangeParallaxImageDuringHoverOptions() && hoverParallaxImageFormat == ImageOptions.Path; }
        private void SetHoverParallaxImageTexture()
        {
            if( ShowHoverImagesAndTextChangesInEditor() && useParallaxImage && changeParallaxImageDuringHover )
            {
                if( uiColorTweenManager_ParallaxImage != null && uiColorTweenManager_ParallaxImage.tweeners != null && uiColorTweenManager_ParallaxImage.tweeners.Count > 0 && uiColorTweenManager_ParallaxImage.tweeners[ 0 ].image != null )
                {
                    if( IsHoverParallaxImageFormatTexture() && hoverParallaxImageTexture != null )
                    {
                        uiColorTweenManager_ParallaxImage.tweeners[ 0 ].image.sprite = TextureHelper.CreateSpriteFromTexture( ref hoverParallaxImageTexture );
                    }
                    else if( IsHoverParallaxImageFormatSprite() && hoverParallaxImageSprite != null )
                    {
                        uiColorTweenManager_ParallaxImage.tweeners[ 0 ].image.sprite = hoverParallaxImageSprite;
                    }
                    else if( IsHoverParallaxImageFormatPath() )
                    {
                        uiColorTweenManager_ParallaxImage.tweeners[ 0 ].image.sprite = null;
                    }
                }
            }
            else
            {
                ValidateParallaxImage();
            }
        }

        [ShowIf( "IsHoverParallaxImageFormatTexture" ), OnValueChanged( "SetHoverParallaxImageTexture" ), FoldoutGroup( "Hover Settings" )]
        public Texture hoverParallaxImageTexture;
        [ShowIf( "IsHoverParallaxImageFormatSprite" ), OnValueChanged( "SetHoverParallaxImageTexture" ), FoldoutGroup( "Hover Settings" )]
        public Sprite hoverParallaxImageSprite;
        [ShowIf( "IsHoverParallaxImageFormatPath" ), OnValueChanged( "SetHoverParallaxImageTexture" ), FoldoutGroup( "Hover Settings" )]
        public string hoverParallaxImagePath;

        [ShowIf( "IsHoverParallaxImageFormatPath" ), FoldoutGroup( "Hover Settings" )]
        public bool hoverParallaxImageCacheIfWeb = true;

        private bool isHoverParallaxImageReady = false;

        [Range( .1f, 10f ), ShowIf( "ShowChangeParallaxImageDuringHoverOptions" ), OnValueChanged( "SetHoverParallaxImageScale" ), FoldoutGroup( "Hover Settings" )]
        public float hoverParallaxImageScale = 1f;
        private void SetHoverParallaxImageScale()
        {
            if( uiScaleTweener_ParallaxImage != null )
            {
                uiScaleTweener_ParallaxImage.SetScale( new Vector3( hoverParallaxImageScale, hoverParallaxImageScale, hoverParallaxImageScale ), UITweener.TweenValue.HoverEnter );
            }

            if( ShowHoverImagesAndTextChangesInEditor() && useParallaxImage && changeParallaxImageDuringHover &&
                uiScaleTweener_ParallaxImage != null )
            {
                uiScaleTweener_ParallaxImage.Force( UITweener.TweenValue.HoverEnter );
            }
            else
            {
                ValidateParallaxImage();
            }
        }

        [ShowIf( "ShowChangeParallaxImageDuringHoverOptions" ), OnValueChanged( "SetHoverParallaxImageColor" )]
        public Color hoverParallaxImageColor = Color.white;
        private void SetHoverParallaxImageColor()
        {
            if( uiColorTweenManager_ParallaxImage != null )
            {
                uiColorTweenManager_ParallaxImage.SetColor( hoverParallaxImageColor, UITweener.TweenValue.HoverEnter );
            }

            if( ShowHoverImagesAndTextChangesInEditor() && useParallaxImage && changeParallaxImageDuringHover &&
                uiColorTweenManager_ParallaxImage != null )
            {
                uiColorTweenManager_ParallaxImage.Force( UITweener.TweenValue.HoverEnter );
            }
            else
            {
                ValidateParallaxImage();
            }
        }
        
        [ShowIf( "ShowChangeParallaxImageDuringHoverOptions" ), OnValueChanged( "SetHoverEnterParallaxImageTweenSpeed" ), FoldoutGroup( "Hover Settings" )]
        public float hoverEnterParallaxImageTweenSpeed = 1f;
        private void SetHoverEnterParallaxImageTweenSpeed()
        {
            if( uiColorTweenManager_ParallaxImage != null )
            {
                uiColorTweenManager_ParallaxImage.SetTweenSpeed( hoverEnterParallaxImageTweenSpeed, UITweener.TweenValue.HoverEnter );
            }
            if( uiScaleTweener_ParallaxImage != null )
            {
                uiScaleTweener_ParallaxImage.SetTweenSpeed( hoverEnterParallaxImageTweenSpeed, UITweener.TweenValue.HoverEnter );
            }
        }

        [ShowIf( "ShowChangeParallaxImageDuringHoverOptions" ), OnValueChanged( "SetHoverExitParallaxImageTweenSpeed" ), FoldoutGroup( "Hover Settings" )]
        public float hoverExitParallaxImageTweenSpeed = 1f;
        private void SetHoverExitParallaxImageTweenSpeed()
        {
            if( uiColorTweenManager_ParallaxImage != null )
            {
                uiColorTweenManager_ParallaxImage.SetTweenSpeed( hoverExitParallaxImageTweenSpeed, UITweener.TweenValue.HoverExit );
            }
            if( uiScaleTweener_ParallaxImage != null )
            {
                uiScaleTweener_ParallaxImage.SetTweenSpeed( hoverExitParallaxImageTweenSpeed, UITweener.TweenValue.HoverExit );
            }
        }

        [ShowIf( "ShowChangeParallaxImageDuringHoverOptions" ), OnValueChanged( "SetHoverEnterParallaxImageEaseType" ), FoldoutGroup( "Hover Settings" )]
        public EaseCurve.EaseType hoverEnterParallaxImageEaseType = EaseCurve.EaseType.Linear;
        private void SetHoverEnterParallaxImageEaseType()
        {
            if( uiColorTweenManager_ParallaxImage != null )
            {
                uiColorTweenManager_ParallaxImage.SetEaseType( hoverEnterParallaxImageEaseType, UITweener.TweenValue.HoverEnter );
            }
            if( uiScaleTweener_ParallaxImage != null )
            {
                uiScaleTweener_ParallaxImage.SetEaseType( hoverEnterParallaxImageEaseType, UITweener.TweenValue.HoverEnter );
            }
        }

        [ShowIf( "ShowChangeParallaxImageDuringHoverOptions" ), OnValueChanged( "SetHoverExitParallaxImageEaseType" ), FoldoutGroup( "Hover Settings" )]
        public EaseCurve.EaseType hoverExitParallaxImageEaseType = EaseCurve.EaseType.Linear;
        private void SetHoverExitParallaxImageEaseType()
        {
            if( uiColorTweenManager_ParallaxImage != null )
            {
                uiColorTweenManager_ParallaxImage.SetEaseType( hoverExitParallaxImageEaseType, UITweener.TweenValue.HoverExit );
            }
            if( uiScaleTweener_ParallaxImage != null )
            {
                uiScaleTweener_ParallaxImage.SetEaseType( hoverExitParallaxImageEaseType, UITweener.TweenValue.HoverExit );
            }
        }







        private bool WhenHoveringChangeImagesAndTextAndShowText() { return showText && WhenHoveringChangeImagesAndText(); }

        [Space( 15f ), ShowIf( "WhenHoveringChangeImagesAndTextAndShowText" ), OnValueChanged( "ValidateHoverText" ), FoldoutGroup( "Hover Settings" )]
        public bool changeTextDuringHover = false;
        private bool ShowChangeTextDuringHoverOptions() { return WhenHoveringChangeImagesAndTextAndShowText() && changeTextDuringHover; }

        //Used to change the image in editor based on settings.
        private void ValidateHoverText()
        {
            if( ShowHoverImagesAndTextChangesInEditor() && showText && changeTextDuringHover )
            {
                SetHoverTextMessage();
                SetHoverTextScale();
                SetHoverTextColor();
                SetHoverTextFont();
                SetHoverTextSize();
                SetHoverEnterTextTweenSpeed();
                SetHoverExitTextTweenSpeed();
                SetHoverEnterTextEaseType();
                SetHoverExitTextEaseType();
            }
            else
            {
                ValidateText();
            }
        }

        [ShowIf( "ShowChangeTextDuringHoverOptions" ), OnValueChanged( "SetHoverTextMessage" ), FoldoutGroup( "Hover Settings" )]
        public string hoverTextMessage = "Hovering";
        private void SetHoverTextMessage()
        {
            if( ShowHoverImagesAndTextChangesInEditor() && showText && changeTextDuringHover )
            {
                if( name == debugWhenName ) Debug.Log( "SetHoverTextMessage() text = " + hoverTextMessage );
                uiTextField_Line1.text = hoverTextMessage;
            }
            else
            {
                if( name == debugWhenName ) Debug.Log( "SetHoverTextMessage() unable to set, calling ValidateText()" );
                ValidateText();
            }
        }

        [Range( .1f, 10f ), ShowIf( "ShowChangeTextDuringHoverOptions" ), OnValueChanged( "SetHoverTextScale" ), FoldoutGroup( "Hover Settings" )]
        public float hoverTextScale = 1f;
        private void SetHoverTextScale()
        {
            if( uiScaleTweener_TextMessage != null && uiScaleTweener_TextMessage.GetTransform() != null )
            {
                uiScaleTweener_TextMessage.SetScale( new Vector3( hoverTextScale, hoverTextScale, hoverTextScale ), UITweener.TweenValue.HoverEnter );
            }

            if( ShowHoverImagesAndTextChangesInEditor() && showText && changeTextDuringHover &&
                uiScaleTweener_TextMessage != null )
            {
                uiScaleTweener_TextMessage.Force( UITweener.TweenValue.HoverEnter );
            }
            else
            {
                ValidateText();
            }
        }

        [ShowIf( "ShowChangeTextDuringHoverOptions" ), OnValueChanged( "SetHoverTextColor" ), FoldoutGroup( "Hover Settings" )]
        public Color hoverTextColor = Color.white;
        private void SetHoverTextColor()
        {
            if( uiColorTweenManager_TextMessage != null )
            {
                uiColorTweenManager_TextMessage.SetColor( hoverTextColor, UITweener.TweenValue.HoverEnter );
            }

            if( showHoverStateInEditor && showText && changeTextDuringHover &&
                uiColorTweenManager_TextMessage != null )
            {
                uiColorTweenManager_TextMessage.Force( UITweener.TweenValue.HoverEnter );
            }
            else
            {
                ValidateText();
            }
        }

        [ShowIf( "ShowChangeTextDuringHoverOptions" ), OnValueChanged( "SetHoverTextFont" ), InfoBox( "If font is left blank, Liberation will be used" ), FoldoutGroup( "Hover Settings" )]
        public Font hoverTextFont;
        private void SetHoverTextFont()
        {
            if( showHoverStateInEditor && showText && changeTextDuringHover )
            {
                uiTextField_Line1.textField.font = hoverTextFont;
            }
            else
            {
                ValidateText();
            }
        }
        
        [ShowIf( "ShowChangeTextDuringHoverOptions" ), OnValueChanged( "SetHoverTextSize" ), FoldoutGroup( "Hover Settings" )]
        public int hoverTextFontSize = 35;
        private void SetHoverTextSize()
        {
            if( showHoverStateInEditor && showText && changeTextDuringHover )
            {
                uiTextField_Line1.textField.fontSize = hoverTextFontSize;
            }
            else
            {
                ValidateText();
            }
        }
        
        [ShowIf( "ShowChangeTextDuringHoverOptions" ), OnValueChanged( "SetHoverEnterTextTweenSpeed" ), FoldoutGroup( "Hover Settings" )]
        public float hoverEnterTextTweenSpeed = 1f;
        private void SetHoverEnterTextTweenSpeed()
        {
            if( uiColorTweenManager_TextMessage != null )
            {
                uiColorTweenManager_TextMessage.SetTweenSpeed( hoverEnterTextTweenSpeed, UITweener.TweenValue.HoverEnter );
            }
            if( uiScaleTweener_TextMessage != null )
            {
                uiScaleTweener_TextMessage.SetTweenSpeed( hoverEnterTextTweenSpeed, UITweener.TweenValue.HoverEnter );
            }
        }

        [ShowIf( "ShowChangeTextDuringHoverOptions" ), OnValueChanged( "SetHoverExitTextTweenSpeed" ), FoldoutGroup( "Hover Settings" )]
        public float hoverExitTextTweenSpeed = 1f;
        private void SetHoverExitTextTweenSpeed()
        {
            if( uiColorTweenManager_TextMessage != null )
            {
                uiColorTweenManager_TextMessage.SetTweenSpeed( hoverExitTextTweenSpeed, UITweener.TweenValue.HoverExit );
            }
            if( uiScaleTweener_TextMessage != null )
            {
                uiScaleTweener_TextMessage.SetTweenSpeed( hoverExitTextTweenSpeed, UITweener.TweenValue.HoverExit );
            }
        }

        [ShowIf( "ShowChangeTextDuringHoverOptions" ), OnValueChanged( "SetHoverEnterTextEaseType" ), FoldoutGroup( "Hover Settings" )]
        public EaseCurve.EaseType hoverEnterTextEaseType = EaseCurve.EaseType.Linear;
        private void SetHoverEnterTextEaseType()
        {
            if( uiColorTweenManager_TextMessage != null )
            {
                uiColorTweenManager_TextMessage.SetEaseType( hoverEnterTextEaseType, UITweener.TweenValue.HoverEnter );
            }
            if( uiScaleTweener_TextMessage != null )
            {
                uiScaleTweener_TextMessage.SetEaseType( hoverEnterTextEaseType, UITweener.TweenValue.HoverEnter );
            }
        }

        [ShowIf( "ShowChangeTextDuringHoverOptions" ), OnValueChanged( "SetHoverExitTextEaseType" ), FoldoutGroup( "Hover Settings" )]
        public EaseCurve.EaseType hoverExitTextEaseType = EaseCurve.EaseType.Linear;
        private void SetHoverExitTextEaseType()
        {
            if( uiColorTweenManager_TextMessage != null )
            {
                uiColorTweenManager_TextMessage.SetEaseType( hoverExitTextEaseType, UITweener.TweenValue.HoverExit );
            }
            if( uiScaleTweener_TextMessage != null )
            {
                uiScaleTweener_TextMessage.SetEaseType( hoverExitTextEaseType, UITweener.TweenValue.HoverExit );
            }
        }






        private bool WhenHoveringChangeImagesAndTextAndShowingFillImage() { return WhenHoveringChangeImagesAndText() && IsUseFillImageTrue(); }

        [Space( 10 ), ShowIf( "WhenHoveringChangeImagesAndTextAndShowingFillImage" ), OnValueChanged( "ValidateHoverFillImage" ), FoldoutGroup( "Hover Settings" )]
        public bool changeFillImageDuringHover = false;
        private bool ShowChangeFillImageDuringHoverOptions() { return WhenHoveringChangeImagesAndTextAndShowingFillImage() && changeFillImageDuringHover; }

        //Used to change the image in editor based on settings.
        private void ValidateHoverFillImage()
        {
            if( ShowHoverImagesAndTextChangesInEditor() && useFillImage && changeFillImageDuringHover )
            {
                SetHoverFillImageTexture();
                SetHoverFillImageScale();
                SetHoverFillImageColor();
                SetHoverFillMethod();
                SetHoverFillOriginMethod();
                SetHoverEnterFillImageTweenSpeed();
                SetHoverExitFillImageTweenSpeed();
                SetHoverEnterFillImageEaseType();
                SetHoverExitFillImageEaseType();
            }
            else
            {
                ValidateFillImage();
            }
        }

        [Space( 10 ), ShowIf( "ShowChangeFillImageDuringHoverOptions" ), OnValueChanged( "SetHoverFillImageTexture" ), FoldoutGroup( "Hover Settings" )]
        public ImageOptions hoverFillImageFormat = ImageOptions.Texture;
        private bool IsHoverFillImageFormatTexture() { return ShowChangeFillImageDuringHoverOptions() && hoverFillImageFormat == ImageOptions.Texture; }
        private bool IsHoverFillImageFormatSprite() { return ShowChangeFillImageDuringHoverOptions() && hoverFillImageFormat == ImageOptions.Sprite; }
        private bool IsHoverFillImageFormatPath() { return ShowChangeFillImageDuringHoverOptions() && hoverFillImageFormat == ImageOptions.Path; }
        private void SetHoverFillImageTexture()
        {
            if( ShowHoverImagesAndTextChangesInEditor() && useFillImage && changeFillImageDuringHover )
            {
                if( uiColorTweenManager_FillImage != null && uiColorTweenManager_FillImage.tweeners != null && uiColorTweenManager_FillImage.tweeners.Count > 0 && uiColorTweenManager_FillImage.tweeners[ 0 ].image != null )
                {
                    if( IsHoverFillImageFormatTexture() && hoverFillImageTexture != null )
                    {
                        uiColorTweenManager_FillImage.tweeners[ 0 ].image.sprite = TextureHelper.CreateSpriteFromTexture( ref hoverFillImageTexture );
                    }
                    else if( IsHoverFillImageFormatSprite() && hoverFillImageSprite != null )
                    {
                        uiColorTweenManager_FillImage.tweeners[ 0 ].image.sprite = hoverFillImageSprite;
                    }
                    else if( IsHoverFillImageFormatPath() )
                    {
                        uiColorTweenManager_FillImage.tweeners[ 0 ].image.sprite = null;
                    }
                }
            }
            else
            {
                ValidateFillImage();
            }
        }

        [ShowIf( "IsHoverFillImageFormatTexture" ), OnValueChanged( "SetHoverFillImageTexture" ), FoldoutGroup( "Hover Settings" )]
        public Texture hoverFillImageTexture;
        [ShowIf( "IsHoverFillImageFormatSprite" ), OnValueChanged( "SetHoverFillImageTexture" ), FoldoutGroup( "Hover Settings" )]
        public Sprite hoverFillImageSprite;
        [ShowIf( "IsHoverFillImageFormatPath" ), OnValueChanged( "SetHoverFillImageTexture" ), FoldoutGroup( "Hover Settings" )]
        public string hoverFillImagePath;

        [ShowIf( "IsHoverFillImageFormatPath" ), FoldoutGroup( "Hover Settings" )]
        public bool hoverFillImageCacheIfWeb = true;

        private bool isHoverFillImageReady = false;

        [Range( .1f, 10f ), ShowIf( "ShowChangeFillImageDuringHoverOptions" ), OnValueChanged( "SetHoverFillImageScale" ), FoldoutGroup( "Hover Settings" )]
        public float hoverFillImageScale = 1f;
        private void SetHoverFillImageScale()
        {
            if( uiScaleTweener_FillImage != null )
            {
                uiScaleTweener_FillImage.SetScale( new Vector3( hoverFillImageScale, hoverFillImageScale, hoverFillImageScale ), UITweener.TweenValue.HoverEnter );
            }

            if( ShowHoverImagesAndTextChangesInEditor() && useFillImage && changeFillImageDuringHover &&
                uiScaleTweener_FillImage != null )
            {
                uiScaleTweener_FillImage.Force( UITweener.TweenValue.HoverEnter );
            }
            else
            {
                ValidateFillImage();
            }
        }


        [ShowIf( "ShowChangeFillImageDuringHoverOptions" ), OnValueChanged( "SetHoverFillImageColor" ), FoldoutGroup( "Hover Settings" ) ]
        public Color hoverFillImageColor = Color.white;
        private void SetHoverFillImageColor()
        {
            if( uiColorTweenManager_FillImage != null )
            {
                uiColorTweenManager_FillImage.SetColor( hoverFillImageColor, UITweener.TweenValue.HoverEnter );
            }

            if( ShowHoverImagesAndTextChangesInEditor() && useFillImage && changeFillImageDuringHover &&
                uiColorTweenManager_FillImage != null)
            {
                uiColorTweenManager_FillImage.Force( UITweener.TweenValue.HoverEnter );
            }
            else
            {
                ValidateFillImage();
            }
        }



        [ShowIf( "ShowChangeFillImageDuringHoverOptions" ), OnValueChanged( "SetHoverFillMethod" ), FoldoutGroup( "Hover Settings" )]
        public Image.FillMethod hoverFillMethod = Image.FillMethod.Radial360;
        private void SetHoverFillMethod()
        {
            if( useFillImage )
            {
                if( uiColorTweenManager_FillImage != null && uiColorTweenManager_FillImage.tweeners.Count > 0 && uiColorTweenManager_FillImage.tweeners[ 0 ].image != null )
                {
                    uiColorTweenManager_FillImage.tweeners[ 0 ].image.fillMethod = hoverFillMethod;
                }
            }
        }

        private bool ShowHoverFillOrigin360() { return ShowChangeFillImageDuringHoverOptions() && hoverFillMethod == Image.FillMethod.Radial360; }
        private bool ShowHoverFillOrigin180() { return ShowChangeFillImageDuringHoverOptions() && hoverFillMethod == Image.FillMethod.Radial180; }
        private bool ShowHoverFillOrigin90() { return ShowChangeFillImageDuringHoverOptions() && hoverFillMethod == Image.FillMethod.Radial90; }
        private bool ShowHoverFillOriginVertical() { return ShowChangeFillImageDuringHoverOptions() && hoverFillMethod == Image.FillMethod.Vertical; }
        private bool ShowHoverFillOriginHorizontal() { return ShowChangeFillImageDuringHoverOptions() && hoverFillMethod == Image.FillMethod.Horizontal; }

        [ShowIf( "ShowHoverFillOrigin360" ), OnValueChanged( "SetHoverFillOriginMethod" ), FoldoutGroup( "Hover Settings" )]
        public Image.Origin360 hoverFillOrigin360 = Image.Origin360.Bottom;

        [ShowIf( "ShowHoverFillOrigin180" ), OnValueChanged( "SetHoverFillOriginMethod" ), FoldoutGroup( "Hover Settings" )]
        public Image.Origin180 hoverFillOrigin180 = Image.Origin180.Bottom;

        [ShowIf( "ShowHoverFillOrigin90" ), OnValueChanged( "SetHoverFillOriginMethod" ), FoldoutGroup( "Hover Settings" )]
        public Image.Origin90 hoverFillOrigin90 = Image.Origin90.BottomLeft;

        [ShowIf( "ShowHoverFillOriginVertical" ), OnValueChanged( "SetHoverFillOriginMethod" ), FoldoutGroup( "Hover Settings" )]
        public Image.OriginVertical hoverFillOriginVertical = Image.OriginVertical.Bottom;

        [ShowIf( "ShowHoverFillOriginHorizontal" ), OnValueChanged( "SetHoverFillOriginMethod" ), FoldoutGroup( "Hover Settings" )]
        public Image.OriginHorizontal hoverFillOriginHorizontal = Image.OriginHorizontal.Left;

        private void SetHoverFillOriginMethod()
        {
            if( ShowChangeFillImageDuringHoverOptions() )
            {
                if( uiColorTweenManager_FillImage != null && uiColorTweenManager_FillImage.tweeners.Count > 0 && uiColorTweenManager_FillImage.tweeners[ 0 ].image != null )
                {
                    uiColorTweenManager_FillImage.tweeners[ 0 ].image.fillOrigin = GetHoverFillOrigin();
                }
            }
        }
        
        private int GetHoverFillOrigin()
        {
            int origin = 0;

            if( hoverFillMethod == Image.FillMethod.Radial360 ) { origin = (int)hoverFillOrigin360; }
            else if( hoverFillMethod == Image.FillMethod.Radial180 ) { origin = (int)hoverFillOrigin180; }
            else if( hoverFillMethod == Image.FillMethod.Radial90 ) { origin = (int)hoverFillOrigin90; }
            else if( hoverFillMethod == Image.FillMethod.Vertical ) { origin = (int)hoverFillOriginVertical; }
            else if( hoverFillMethod == Image.FillMethod.Horizontal ) { origin = (int)hoverFillOriginHorizontal; }

            return origin;
        }



        [ShowIf( "ShowChangeFillImageDuringHoverOptions" ), OnValueChanged( "SetHoverEnterFillImageTweenSpeed" ), FoldoutGroup( "Hover Settings" )]
        public float hoverEnterFillImageTweenSpeed = 1f;
        private void SetHoverEnterFillImageTweenSpeed()
        {
            if( uiColorTweenManager_FillImage != null )
            {
                uiColorTweenManager_FillImage.SetTweenSpeed( hoverEnterFillImageTweenSpeed, UITweener.TweenValue.HoverEnter );
            }
            if( uiScaleTweener_FillImage != null )
            {
                uiScaleTweener_FillImage.SetTweenSpeed( hoverEnterFillImageTweenSpeed, UITweener.TweenValue.HoverEnter );
            }
        }

        [ShowIf( "ShowChangeFillImageDuringHoverOptions" ), OnValueChanged( "SetHoverExitFillImageTweenSpeed" ), FoldoutGroup( "Hover Settings" )]
        public float hoverExitFillImageTweenSpeed = 1f;
        private void SetHoverExitFillImageTweenSpeed()
        {
            if( uiColorTweenManager_FillImage != null )
            {
                uiColorTweenManager_FillImage.SetTweenSpeed( hoverExitFillImageTweenSpeed, UITweener.TweenValue.HoverExit );
            }
            if( uiScaleTweener_FillImage != null )
            {
                uiScaleTweener_FillImage.SetTweenSpeed( hoverExitFillImageTweenSpeed, UITweener.TweenValue.HoverExit );
            }
        }

        [ShowIf( "ShowChangeFillImageDuringHoverOptions" ), OnValueChanged( "SetHoverEnterFillImageEaseType" ), FoldoutGroup( "Hover Settings" )]
        public EaseCurve.EaseType hoverEnterFillImageEaseType = EaseCurve.EaseType.Linear;
        private void SetHoverEnterFillImageEaseType()
        {
            if( uiColorTweenManager_FillImage != null )
            {
                uiColorTweenManager_FillImage.SetEaseType( hoverEnterFillImageEaseType, UITweener.TweenValue.HoverEnter );
            }
            if( uiScaleTweener_FillImage != null )
            {
                uiScaleTweener_FillImage.SetEaseType( hoverEnterFillImageEaseType, UITweener.TweenValue.HoverEnter );
            }
        }

        [ShowIf( "ShowChangeFillImageDuringHoverOptions" ), OnValueChanged( "SetHoverExitFillImageEaseType" ), FoldoutGroup( "Hover Settings" )]
        public EaseCurve.EaseType hoverExitFillImageEaseType = EaseCurve.EaseType.Linear;
        private void SetHoverExitFillImageEaseType()
        {
            if( uiColorTweenManager_FillImage != null )
            {
                uiColorTweenManager_FillImage.SetEaseType( hoverExitFillImageEaseType, UITweener.TweenValue.HoverExit );
            }
            if( uiScaleTweener_FillImage != null )
            {
                uiScaleTweener_FillImage.SetEaseType( hoverExitFillImageEaseType, UITweener.TweenValue.HoverExit );
            }
        }






        private bool WhenHoveringChangeImagesAndTextAndShowingFillBackgroundImage() { return WhenHoveringChangeImagesAndText() && IsUseFillBackgroundImageTrue(); }

        [Space( 10 ), ShowIf( "WhenHoveringChangeImagesAndTextAndShowingFillBackgroundImage" ), OnValueChanged( "ValidateHoverFillBackgroundImage" ), FoldoutGroup( "Hover Settings" )]
        public bool changeFillBackgroundImageDuringHover = false;
        private bool ShowChangeFillBackgroundImageDuringHoverOptions() { return WhenHoveringChangeImagesAndTextAndShowingFillImage() && changeFillBackgroundImageDuringHover; }

        //Used to change the image in editor based on settings.
        private void ValidateHoverFillBackgroundImage()
        {
            if( ShowHoverImagesAndTextChangesInEditor() && useFillBackgroundImage && changeFillBackgroundImageDuringHover )
            {
                //Debug.Log( "ValidateHoverFillBackgroundImage()" );
                SetHoverFillBackgroundImageTexture();
                SetHoverFillBackgroundImageScale();
                SetHoverFillBackgroundImageColor();
                SetHoverEnterFillBackgroundImageTweenSpeed();
                SetHoverExitFillBackgroundImageTweenSpeed();
                SetHoverEnterFillBackgroundImageEaseType();
                SetHoverExitFillBackgroundImageEaseType();
            }
            else
            {
                ValidateFillBackgroundImage();
            }
        }

        [Space( 10 ), ShowIf( "ShowChangeFillBackgroundImageDuringHoverOptions" ), OnValueChanged( "SetHoverFillBackgroundImageTexture" ), FoldoutGroup( "Hover Settings" )]
        public ImageOptions hoverFillBackgroundImageFormat = ImageOptions.Texture;
        private bool IsHoverFillBackgroundImageFormatTexture() { return ShowChangeFillBackgroundImageDuringHoverOptions() && hoverFillBackgroundImageFormat == ImageOptions.Texture; }
        private bool IsHoverFillBackgroundImageFormatSprite() { return ShowChangeFillBackgroundImageDuringHoverOptions() && hoverFillBackgroundImageFormat == ImageOptions.Sprite; }
        private bool IsHoverFillBackgroundImageFormatPath() { return ShowChangeFillBackgroundImageDuringHoverOptions() && hoverFillBackgroundImageFormat == ImageOptions.Path; }
        private void SetHoverFillBackgroundImageTexture()
        {
            if( ShowHoverImagesAndTextChangesInEditor() && useFillBackgroundImage && changeFillBackgroundImageDuringHover )
            {
                if( uiColorTweenManager_FillBackgroundImage != null && uiColorTweenManager_FillBackgroundImage.tweeners != null && uiColorTweenManager_FillBackgroundImage.tweeners.Count > 0 && uiColorTweenManager_FillBackgroundImage.tweeners[ 0 ].image != null )
                {
                    if( IsHoverFillBackgroundImageFormatTexture() && hoverFillBackgroundImageTexture != null )
                    {
                        uiColorTweenManager_FillBackgroundImage.tweeners[ 0 ].image.sprite = TextureHelper.CreateSpriteFromTexture( ref hoverFillBackgroundImageTexture );
                    }
                    else if( IsHoverFillBackgroundImageFormatSprite() && hoverFillBackgroundImageSprite != null )
                    {
                        uiColorTweenManager_FillBackgroundImage.tweeners[ 0 ].image.sprite = hoverFillBackgroundImageSprite;
                    }
                    else if( IsHoverFillBackgroundImageFormatPath() )
                    {
                        uiColorTweenManager_FillBackgroundImage.tweeners[ 0 ].image.sprite = null;
                    }
                }
            }
            else
            {
                ValidateFillBackgroundImage();
            }
        }

        [ShowIf( "IsHoverFillBackgroundImageFormatTexture" ), OnValueChanged( "SetHoverFillBackgroundImageTexture" ), FoldoutGroup( "Hover Settings" )]
        public Texture hoverFillBackgroundImageTexture;
        [ShowIf( "IsHoverFillBackgroundImageFormatSprite" ), OnValueChanged( "SetHoverFillBackgroundImageTexture" ), FoldoutGroup( "Hover Settings" )]
        public Sprite hoverFillBackgroundImageSprite;
        [ShowIf( "IsHoverFillBackgroundImageFormatPath" ), OnValueChanged( "SetHoverFillBackgroundImageTexture" ), FoldoutGroup( "Hover Settings" )]
        public string hoverFillBackgroundImagePath;

        [ShowIf( "IsHoverFillBackgroundImageFormatPath" ), FoldoutGroup( "Hover Settings" )]
        public bool hoverFillBackgroundImageCacheIfWeb = true;

        private bool isHoverFillBackgroundImageReady = false;

        [Range( .1f, 10f ), ShowIf( "ShowChangeFillBackgroundImageDuringHoverOptions" ), OnValueChanged( "SetHoverFillBackgroundImageScale" ), FoldoutGroup( "Hover Settings" )]
        public float hoverFillBackgroundImageScale = 1f;
        private void SetHoverFillBackgroundImageScale()
        {
            if( uiScaleTweener_FillBackgroundImage != null )
            {
                uiScaleTweener_FillBackgroundImage.SetScale( new Vector3( hoverFillBackgroundImageScale, hoverFillBackgroundImageScale, hoverFillBackgroundImageScale ), UITweener.TweenValue.HoverEnter );
            }

            if( ShowHoverImagesAndTextChangesInEditor() && useFillBackgroundImage && changeFillBackgroundImageDuringHover &&
                uiScaleTweener_FillBackgroundImage != null)
            {
                uiScaleTweener_FillBackgroundImage.Force( UITweener.TweenValue.HoverEnter );
            }
            else
            {
                ValidateFillBackgroundImage();
            }
        }

        [ShowIf( "ShowChangeFillBackgroundImageDuringHoverOptions" ), OnValueChanged( "SetHoverFillBackgroundImageColor" ), FoldoutGroup( "Hover Settings" )]
        public Color hoverFillBackgroundImageColor;
        private void SetHoverFillBackgroundImageColor()
        {
            if( uiColorTweenManager_FillBackgroundImage != null )
            {
                Debug.Log( "SetHoverFillBackgroundImageColor() alpha = " + hoverFillBackgroundImageColor.a );
                uiColorTweenManager_FillBackgroundImage.SetColor( hoverFillBackgroundImageColor, UITweener.TweenValue.HoverEnter );
            }
            else
            {
                Debug.Log( "SetHoverFillBackgroundImageColor() uiColorTweenManager is null... alpha = " + hoverFillBackgroundImageColor.a );
            }

            if( ShowHoverImagesAndTextChangesInEditor() && useFillBackgroundImage && changeFillBackgroundImageDuringHover &&
                uiColorTweenManager_FillBackgroundImage != null )
            {
                uiColorTweenManager_FillBackgroundImage.Force( UITweener.TweenValue.HoverEnter );
            }
            else
            {
                ValidateFillBackgroundImage();
            }
        }

        [ShowIf( "ShowChangeFillBackgroundImageDuringHoverOptions" ), OnValueChanged( "SetHoverEnterFillBackgroundImageTweenSpeed" ), FoldoutGroup( "Hover Settings" )]
        public float hoverEnterFillBackgroundImageTweenSpeed = 1f;
        private void SetHoverEnterFillBackgroundImageTweenSpeed()
        {
            if( uiColorTweenManager_FillBackgroundImage != null )
            {
                uiColorTweenManager_FillBackgroundImage.SetTweenSpeed( hoverEnterFillBackgroundImageTweenSpeed, UITweener.TweenValue.HoverEnter );
            }
            if( uiScaleTweener_FillBackgroundImage != null )
            {
                uiScaleTweener_FillBackgroundImage.SetTweenSpeed( hoverEnterFillBackgroundImageTweenSpeed, UITweener.TweenValue.HoverEnter );
            }
        }

        [ShowIf( "ShowChangeFillBackgroundImageDuringHoverOptions" ), OnValueChanged( "SetHoverExitFillBackgroundImageTweenSpeed" ), FoldoutGroup( "Hover Settings" )]
        public float hoverExitFillBackgroundImageTweenSpeed = 1f;
        private void SetHoverExitFillBackgroundImageTweenSpeed()
        {
            if( uiColorTweenManager_FillBackgroundImage != null )
            {
                uiColorTweenManager_FillBackgroundImage.SetTweenSpeed( hoverExitFillBackgroundImageTweenSpeed, UITweener.TweenValue.HoverExit );
            }
            if( uiScaleTweener_FillBackgroundImage != null )
            {
                uiScaleTweener_FillBackgroundImage.SetTweenSpeed( hoverExitFillBackgroundImageTweenSpeed, UITweener.TweenValue.HoverExit );
            }
        }

        [ShowIf( "ShowChangeFillBackgroundImageDuringHoverOptions" ), OnValueChanged( "SetHoverEnterFillBackgroundImageEaseType" ), FoldoutGroup( "Hover Settings" )]
        public EaseCurve.EaseType hoverEnterFillBackgroundImageEaseType = EaseCurve.EaseType.Linear;
        private void SetHoverEnterFillBackgroundImageEaseType()
        {
            if( uiColorTweenManager_FillBackgroundImage != null )
            {
                uiColorTweenManager_FillBackgroundImage.SetEaseType( hoverEnterFillBackgroundImageEaseType, UITweener.TweenValue.HoverEnter );
            }
            if( uiScaleTweener_FillBackgroundImage != null )
            {
                uiScaleTweener_FillBackgroundImage.SetEaseType( hoverEnterFillBackgroundImageEaseType, UITweener.TweenValue.HoverEnter );
            }
        }

        [ShowIf( "ShowChangeFillBackgroundImageDuringHoverOptions" ), OnValueChanged( "SetHoverExitFillBackgroundImageEaseType" ), FoldoutGroup( "Hover Settings" )]
        public EaseCurve.EaseType hoverExitFillBackgroundImageEaseType = EaseCurve.EaseType.Linear;
        private void SetHoverExitFillBackgroundImageEaseType()
        {
            if( uiColorTweenManager_FillBackgroundImage != null )
            {
                uiColorTweenManager_FillBackgroundImage.SetEaseType( hoverExitFillBackgroundImageEaseType, UITweener.TweenValue.HoverExit );
            }
            if( uiScaleTweener_FillBackgroundImage != null )
            {
                uiScaleTweener_FillBackgroundImage.SetEaseType( hoverExitFillBackgroundImageEaseType, UITweener.TweenValue.HoverExit );
            }
        }







        private bool ShowWhenHoveringStopIdleAnimationOption() { return WhenHoveringChangeImagesAndText() && playAnimationDuringIdleState != AnimationOptions.NoAnimation; }

        [Space( 15f ), ShowIf( "ShowWhenHoveringStopIdleAnimationOption" ), FoldoutGroup( "Hover Settings" )]
        public bool whenHoveringStopIdleAnimation = false;

        [Space( 15f ), ShowIf( "WhenHoveringChange" ), FoldoutGroup( "Hover Settings" )]
        public bool whenNoInternetAccessDisableHoverState = false;

        #endregion

        #region ANIMATION OPTIONS
        //------------------------------------------------------------------//
        //------------------ ANIMATION OPTIONS -----------------------------//
        //------------------------------------------------------------------//
        public enum AnimationOptions
        {
            NoAnimation,
            ScaleInAndOut
        }
        



        [Space( 20 ), OnValueChanged( "ValidateIdleAnimation" ), FoldoutGroup( "Animation Options" )]
        public AnimationOptions playAnimationDuringIdleState = AnimationOptions.ScaleInAndOut;
        private bool PlayNoAnimationDuringIdle() { return playAnimationDuringIdleState == AnimationOptions.NoAnimation; }
        private bool PlayScaleInAndOutAnimationDuringIdle() { return playAnimationDuringIdleState == AnimationOptions.ScaleInAndOut; }
        private void ValidateIdleAnimation()
        {
            SetIdleAnimationScalesToAnimateTo();
            SetIdleAnimationAnimationSpeeds();
            SetIdleAnimationEaseTypes();
            SetIdleAnimationDelays();
        }

        [Space( 5 ), Range( .1f, 10f ), OnValueChanged( "SetIdleAnimationScalesToAnimateTo" ), ShowIf( "PlayScaleInAndOutAnimationDuringIdle" ), FoldoutGroup( "Animation Options" )]
        public float idleTweenToMinimumScale = 1f;
        private void SetIdleAnimationScalesToAnimateTo()
        {
            if( PlayScaleInAndOutAnimationDuringIdle() )
            {
                if( uiScaleAnimation_BlockButtonScaleInAndOut != null )
                {
                    uiScaleAnimation_BlockButtonScaleInAndOut.scalesToAnimateTo = new Vector3[] { new Vector3( idleTweenToMinimumScale, idleTweenToMinimumScale, idleTweenToMinimumScale ), new Vector3( idleTweenToMaximumScale, idleTweenToMaximumScale, idleTweenToMaximumScale ) };
                }
            }
        }

        [Range( .1f, 10f ), OnValueChanged( "SetIdleAnimationScalesToAnimateTo" ), ShowIf( "PlayScaleInAndOutAnimationDuringIdle" ), FoldoutGroup( "Animation Options" )]
        public float idleTweenToMaximumScale = 1.5f;
        
        
        [Space( 5 ), Range( .1f, 10f ), OnValueChanged( "SetIdleAnimationAnimationSpeeds" ), ShowIf( "PlayScaleInAndOutAnimationDuringIdle" ), FoldoutGroup( "Animation Options" )]
        public float idleTweenToMinimumSpeed = 1f;
        private void SetIdleAnimationAnimationSpeeds()
        {
            if( PlayScaleInAndOutAnimationDuringIdle() )
            {
                if( uiScaleAnimation_BlockButtonScaleInAndOut != null )
                {
                    uiScaleAnimation_BlockButtonScaleInAndOut.animationSpeeds = new float[] { idleTweenToMinimumSpeed, idleTweenToMaximumSpeed };
                }
            }
        }

        [ Range( .1f, 10f ), OnValueChanged( "SetIdleAnimationAnimationSpeeds" ), ShowIf( "PlayScaleInAndOutAnimationDuringIdle" ), FoldoutGroup( "Animation Options" )]
        public float idleTweenToMaximumSpeed = 1.75f;


        [Space( 5 ), OnValueChanged( "SetIdleAnimationEaseTypes" ), ShowIf( "PlayScaleInAndOutAnimationDuringIdle" ), FoldoutGroup( "Animation Options" )]
        public EaseCurve.EaseType idleTweenToMinimumEaseType = EaseCurve.EaseType.BounceEaseOut;
        private void SetIdleAnimationEaseTypes()
        {
            if( PlayScaleInAndOutAnimationDuringIdle() )
            {
                if( uiScaleAnimation_BlockButtonScaleInAndOut != null )
                {
                    uiScaleAnimation_BlockButtonScaleInAndOut.easeTypes = new EaseCurve.EaseType[] { idleTweenToMinimumEaseType, idleTweenToMaximumEaseType };
                }
            }
        }

        [ ShowIf( "PlayScaleInAndOutAnimationDuringIdle" ), OnValueChanged( "SetIdleAnimationEaseTypes" ), FoldoutGroup( "Animation Options" )]
        public EaseCurve.EaseType idleTweenToMaximumEaseType = EaseCurve.EaseType.QuadEaseOut;


        [Space( 5 ), OnValueChanged( "SetIdleAnimationDelays" ), ShowIf( "PlayScaleInAndOutAnimationDuringIdle" ), FoldoutGroup( "Animation Options" )]
        public float idleTweenToMinimumDelay = .25f;
        private void SetIdleAnimationDelays()
        {
            if( PlayScaleInAndOutAnimationDuringIdle() )
            {
                if( uiScaleAnimation_BlockButtonScaleInAndOut != null )
                {
                    uiScaleAnimation_BlockButtonScaleInAndOut.delays = new float[] { idleTweenToMinimumDelay, idleTweenToMaximumDelay };
                }
            }
        }

        [ ShowIf( "PlayScaleInAndOutAnimationDuringIdle" ), OnValueChanged( "SetIdleAnimationDelays" ), FoldoutGroup( "Animation Options" )]
        public float idleTweenToMaximumDelay = 1f;

        #endregion

        #region AUDIO OPTIONS
        //------------------------------------------------------------------//
        //------------------ AUDIO OPTIONS ---------------------------------//
        //------------------------------------------------------------------//
        [FoldoutGroup( "Audio Options" )]
        public bool playBlockSelectSound = true;

        public enum AudioOptions
        {
            AudioClip,
            Path
        }

        [ShowIf( "playBlockSelectSound", true ), FoldoutGroup( "Audio Options" )]
        public AudioOptions BlockSelectSoundLoadType = AudioOptions.AudioClip;
        private bool IsBlockSelectSoundLoadTypeAudioClip() { return playBlockSelectSound && BlockSelectSoundLoadType == AudioOptions.AudioClip; }
        private bool IsBlockSelectSoundLoadTypePath() { return playBlockSelectSound && BlockSelectSoundLoadType == AudioOptions.Path; }

        [ShowIf( "IsBlockSelectSoundLoadTypeAudioClip" ), FoldoutGroup( "Audio Options" )]
        public AudioClip blockSelectAudioClip;
        [ShowIf( "IsBlockSelectSoundLoadTypePath" ), FoldoutGroup( "Audio Options" )]
        public string blockSelectSoundPath;

        [ShowIf( "IsBlockSelectSoundLoadTypePath" ), FoldoutGroup( "Audio Options" )]
        public bool cacheBlockSelectAudioClipIfLoadedFromWeb = true;

        [Range( 0f, 1f ), ShowIf( "playBlockSelectSound", true ), FoldoutGroup( "Audio Options" )]
        public float blockSelectVolume = 1f;

        private bool isBlockSelectReadyToPlay;



        [Space( 10 ), FoldoutGroup( "Audio Options" )]
        public bool playBlockEnterSound = true;

        [ShowIf( "playBlockEnterSound", true ), FoldoutGroup( "Audio Options" )]
        public AudioOptions BlockEnterSoundLoadType = AudioOptions.AudioClip;
        private bool IsBlockEnterSoundLoadTypeAudioClip() { return playBlockEnterSound && BlockEnterSoundLoadType == AudioOptions.AudioClip; }
        private bool IsBlockEnterSoundLoadTypePath() { return playBlockEnterSound && BlockEnterSoundLoadType == AudioOptions.Path; }

        [ShowIf( "IsBlockEnterSoundLoadTypeAudioClip" ), FoldoutGroup( "Audio Options" )]
        public AudioClip blockEnterAudioClip;
        [ShowIf( "IsBlockEnterSoundLoadTypePath" ), FoldoutGroup( "Audio Options" )]
        public string blockEnterSoundPath;

        [ShowIf( "IsBlockEnterSoundLoadTypePath" ), FoldoutGroup( "Audio Options" )]
        public bool cacheBlockEnterAudioClipIfLoadedFromWeb = true;

        [Range( 0f, 1f ), ShowIf( "playBlockEnterSound", true ), FoldoutGroup( "Audio Options" )]
        public float blockEnterVolume = 1f;

        private bool isBlockEnterReadyToPlay;



        [Space( 10 ), FoldoutGroup( "Audio Options" )]
        public bool playBlockExitSound = true;

        [ShowIf( "playBlockExitSound", true ), FoldoutGroup( "Audio Options" )]
        public AudioOptions BlockExitSoundLoadType = AudioOptions.AudioClip;
        private bool IsBlockExitSoundLoadTypeAudioClip() { return playBlockExitSound && BlockExitSoundLoadType == AudioOptions.AudioClip; }
        private bool IsBlockExitSoundLoadTypePath() { return playBlockExitSound && BlockExitSoundLoadType == AudioOptions.Path; }

        [ShowIf( "IsBlockExitSoundLoadTypeAudioClip" ), FoldoutGroup( "Audio Options" )]
        public AudioClip blockExitAudioClip;
        [ShowIf( "IsBlockExitSoundLoadTypePath" ), FoldoutGroup( "Audio Options" )]
        public string blockExitSoundPath;

        [ShowIf( "IsBlockExitSoundLoadTypePath" ), FoldoutGroup( "Audio Options" )]
        public bool cacheBlockExitAudioClipIfLoadedFromWeb = true;

        [Range( 0f, 1f ), ShowIf( "playBlockExitSound", true ), FoldoutGroup( "Audio Options" )]
        public float blockExitVolume = 1f;
        
        private bool isBlockExitReadyToPlay;




        [Space( 10 ), FoldoutGroup( "Audio Options" ), ShowIf( "useGazeInput", true ), InfoBox("The looping fill sound only plays while gazing at this Block Button, sound ends when block button has been Selected or the user stops gazing at this Block Button")]
        public bool playBlockLoopingFillSound = true;
        private bool IsPlayBlockLoopingFillSoundTrue() { return useGazeInput && playBlockLoopingFillSound; }

        [ShowIf( "IsPlayBlockLoopingFillSoundTrue", true ), FoldoutGroup( "Audio Options" )]
        public AudioOptions BlockLoopingFillSoundLoadType = AudioOptions.AudioClip;
        private bool IsBlockLoopingSoundLoadTypeAudioClip() { return IsPlayBlockLoopingFillSoundTrue() && BlockLoopingFillSoundLoadType == AudioOptions.AudioClip; }
        private bool IsBlockLoopingSoundLoadTypePath() { return IsPlayBlockLoopingFillSoundTrue() && BlockLoopingFillSoundLoadType == AudioOptions.Path; }

        [ShowIf( "IsBlockLoopingSoundLoadTypeAudioClip" ), FoldoutGroup( "Audio Options" )]
        public AudioClip blockLoopingFillAudioClip;
        [ShowIf( "IsBlockLoopingSoundLoadTypePath" ), FoldoutGroup( "Audio Options" )]
        public string blockLoopingFillSoundPath;

        [ShowIf( "IsBlockLoopingSoundLoadTypePath" ), FoldoutGroup( "Audio Options" )]
        public bool cacheBlockLoopingFillAudioClipIfLoadedFromWeb = true;

        [Range( 0f, 1f ), ShowIf( "IsPlayBlockLoopingFillSoundTrue", true ), FoldoutGroup( "Audio Options" )]
        public float blockLoopingFillVolume = 1f;

        [ShowIf( "IsPlayBlockLoopingFillSoundTrue" ), FoldoutGroup( "Audio Options" )]
        public bool tweenPitchDuringFillSoundPlay = true;
        private bool IsTweenPitchTrue() { return IsPlayBlockLoopingFillSoundTrue() && tweenPitchDuringFillSoundPlay; }

        [ShowIf( "IsTweenPitchTrue" ), FoldoutGroup( "Audio Options" )]
        public float tweenPitchTo = 1.25f;
        [ShowIf( "IsTweenPitchTrue" ), FoldoutGroup( "Audio Options" )]
        public EaseCurve.EaseType tweenPitchEasing = EaseCurve.EaseType.Linear;


        private AudioSource blockLoopingFillSoundSource;
        
        private bool isBlockLoopingFillReadyToPlay;
        private Coroutine blockLoopingFillSoundCoroutine;

        #endregion

        #region EVENTS
        //------------------------------------------------------------------//
        //------------------ EVENTS ----------------------------------------//
        //------------------------------------------------------------------//
        [FoldoutGroup( "Events" )]
        public UnityEvent onEnable = new UnityEvent();
        [FoldoutGroup( "Events" )]
        public UnityEvent onDisable = new UnityEvent();

        [FoldoutGroup( "Events" )]
        public UnityEvent onShow = new UnityEvent();
        [FoldoutGroup( "Events" )]
        public UnityEvent onHide = new UnityEvent();
        [FoldoutGroup( "Events" )]
        public UnityEvent onForceShow = new UnityEvent();
        [FoldoutGroup( "Events" )]
        public UnityEvent onForceHide = new UnityEvent();

        [FoldoutGroup( "Events" )]
        public UnityEvent onColliderEnter = new UnityEvent();
        [FoldoutGroup( "Events" )]
        public UnityEvent onColliderHover = new UnityEvent();
        [FoldoutGroup( "Events" )]
        public UnityEvent onColliderExit = new UnityEvent();

        [FoldoutGroup( "Events" )]
        public UnityEvent onSelect = new UnityEvent();

        #endregion

        #region HOOKS
        //------------------------------------------------------------------//
        //----------------------------- HOOKS ------------------------------//
        //------------------------------------------------------------------//
        [FoldoutGroup( "Hooks" )]
        public UIScaleAnimation uiScaleAnimation_BlockButtonScaleInAndOut;

        [FoldoutGroup( "Hooks" )]
        public UIScaleTweener uiScaleTweener_BlockButtonScaleParent;

        [FoldoutGroup( "Hooks" )]
        public UIColorTweenManager uiColorTweenManager_CenterImage;

        [FoldoutGroup( "Hooks" )]
        public UIScaleTweener uiScaleTweener_CenterImage;

        [FoldoutGroup( "Hooks" )]
        public UIColorTweenManager uiColorTweenManager_BackgroundImage;

        [FoldoutGroup( "Hooks" )]
        public UIScaleTweener uiScaleTweener_BackgroundImage;

        [FoldoutGroup( "Hooks" )]
        public UIColorTweenManager uiColorTweenManager_ParallaxImage;

        [FoldoutGroup( "Hooks" )]
        public UIScaleTweener uiScaleTweener_ParallaxImage;

        [FoldoutGroup( "Hooks" )]
        public UIColorTweenManager uiColorTweenManager_FillBackgroundImage;

        [FoldoutGroup( "Hooks" )]
        public UIScaleTweener uiScaleTweener_FillBackgroundImage;

        [FoldoutGroup( "Hooks" )]
        public UIColorTweenManager uiColorTweenManager_FillImage;

        [FoldoutGroup( "Hooks" )]
        public UIScaleTweener uiScaleTweener_FillImage;

        [FoldoutGroup( "Hooks" )]
        public UIColorTweenManager uiColorTweenManager_TextMessage;

        [FoldoutGroup( "Hooks" )]
        public UIScaleTweener uiScaleTweener_TextMessage;

        [FoldoutGroup( "Hooks" )]
        public UITextField uiTextField_Line1;

        [FoldoutGroup( "Hooks" )]
        public Image fillCircleImageRenderer;
        
        private Coroutine handle_FadeOutFillSFX;
        private Coroutine handle_StartEnterTimer;

        [FoldoutGroup( "Hooks" )]
        public Collider collider3D;
        [FoldoutGroup( "Hooks" )]
        public GazeCollider gazeCollider;

        [FoldoutGroup( "Hooks" )]
        public Mesh colliderMeshSphere;
        [FoldoutGroup( "Hooks" )]
        public Mesh colliderMeshCube;
        [FoldoutGroup( "Hooks" )]
        public MeshRenderer colliderMeshRenderer;
        [FoldoutGroup( "Hooks" )]
        public MeshFilter colliderMeshFilter;

        [FoldoutGroup( "Hooks" )]
        public RawImage collider2DImage;
        [FoldoutGroup( "Hooks" )]
        public Texture circleTexture;

        [FoldoutGroup( "Hooks" )]
        public Font textFont;

        #endregion




        #region METHODS - START LOGIC
        //--------------------------------//
        public override void Start()
        //--------------------------------//
        {
            base.Start();
            
            //Check if the Input Manager exists, if it doesn't, then create it!
            if( GameObject.Find( "bxr_XRInputManager" ) == null )
            {
                GameObject go = PrefabManager.InstantiatePrefab( PrefabFactory.Prefabs.bxr_XRInputManager );
                BlockHelper.AddToBrandXRTechParent( go.transform );
            }

            if( gazeCollider != null ) { gazeCollider.AllowGazeFill = true; gazeCollider.AllowGazeGrow = true; }

            if( blockLoopingFillSoundSource == null )
            {
                blockLoopingFillSoundSource = gameObject.GetComponent<AudioSource>();
            }

            if( fillCircleImageRenderer != null )
            {
                fillCircleImageRenderer.fillAmount = 0f;
            }
            
            if( PlayScaleInAndOutAnimationDuringIdle() )
            { uiScaleAnimation_BlockButtonScaleInAndOut.Play(); }

            ForceHideCollider();

            //Make sure we've showing the appropriate images/text
            ValidateCenterImage();
            ValidateBackgroundImage();
            ValidateParallaxImage();
            ValidateFillImage();
            ValidateFillBackgroundImage();
            ValidateText();
            
            LoadAudioResources();

            LoadTextureResources();
            
            if( enabledAtStart == EnabledSate.Enabled ) { Enable(); }
            else if( enabledAtStart == EnabledSate.Disabled ) { Disable(); }
            
            if( visibleAtStart == ShowState.Showing ) { ForceShow(); }
            else if( visibleAtStart == ShowState.Hiding ) { ForceHide(); }
            
        } //END Start

        //--------------------------------//
        private void LoadAudioResources()
        //--------------------------------//
        {

            if( playBlockSelectSound )
            {
                isBlockSelectReadyToPlay = false;

                if( BlockSelectSoundLoadType == AudioOptions.AudioClip && blockSelectAudioClip != null )
                {
                    isBlockSelectReadyToPlay = true;
                }
                else if( BlockSelectSoundLoadType == AudioOptions.Path )
                {
                    blockSelectAudioClip = null;

                    WWWHelper.instance.GetAudioClip( blockSelectSoundPath, cacheBlockSelectAudioClipIfLoadedFromWeb,
                        (AudioClip audioClip ) => {
                            if( this != null )
                            {
                                isBlockSelectReadyToPlay = true;
                                blockSelectAudioClip = audioClip;
                            }
                        }, null );
                }
            }

            if( playBlockEnterSound )
            {
                isBlockEnterReadyToPlay = false;

                if( BlockEnterSoundLoadType == AudioOptions.AudioClip && blockEnterAudioClip != null )
                {
                    isBlockEnterReadyToPlay = true;
                }
                else if( BlockEnterSoundLoadType == AudioOptions.Path )
                {
                    blockEnterAudioClip = null;

                    WWWHelper.instance.GetAudioClip( blockEnterSoundPath, cacheBlockEnterAudioClipIfLoadedFromWeb,
                        ( AudioClip audioClip ) => {
                            if( this != null )
                            {
                                isBlockEnterReadyToPlay = true;
                                blockEnterAudioClip = audioClip;
                            }
                        }, null );
                }
            }

            if( playBlockExitSound )
            {
                isBlockExitReadyToPlay = false;

                if( BlockExitSoundLoadType == AudioOptions.AudioClip && blockExitAudioClip != null )
                {
                    isBlockExitReadyToPlay = true;
                }
                else if( BlockExitSoundLoadType == AudioOptions.Path )
                {
                    blockExitAudioClip = null;

                    WWWHelper.instance.GetAudioClip( blockExitSoundPath, cacheBlockExitAudioClipIfLoadedFromWeb, 
                        (AudioClip audioClip) => {
                            if( this != null )
                            {
                                isBlockExitReadyToPlay = true;
                                blockExitAudioClip = audioClip;
                            }
                        }, null );
                }
            }

            if( playBlockLoopingFillSound )
            {
                isBlockLoopingFillReadyToPlay = false;

                if( BlockLoopingFillSoundLoadType == AudioOptions.AudioClip && blockLoopingFillAudioClip != null )
                {
                    isBlockLoopingFillReadyToPlay = true;
                }
                else if( BlockLoopingFillSoundLoadType == AudioOptions.Path )
                {
                    blockLoopingFillAudioClip = null;

                    WWWHelper.instance.GetAudioClip( blockLoopingFillSoundPath, cacheBlockLoopingFillAudioClipIfLoadedFromWeb, 
                        (AudioClip audioClip) => {
                            if( this != null )
                            {
                                isBlockLoopingFillReadyToPlay = true;
                                blockLoopingFillAudioClip = audioClip;
                            }
                        }, null );
                }
            }

        } //END LoadAudioResources
        

        //--------------------------------//
        private void LoadTextureResources()
        //--------------------------------//
        {

            if( IsUseCenterImageTrue() )
            {
                isCenterImageReady = false;

                if( IsCenterImageFormatTexture() && centerImageTexture != null )
                {
                    centerImageSprite = TextureHelper.CreateSpriteFromTexture( ref centerImageTexture );
                    isCenterImageReady = true;
                }
                else if( IsCenterImageFormatSprite() && centerImageSprite != null )
                {
                    isCenterImageReady = true;
                }
                else if( IsCenterImageFormatPath() && centerImagePath != "" )
                {
                    centerImageTexture = null;
                    centerImageSprite = null;

                    WWWHelper.instance.GetTexture( centerImagePath, centerImageCacheIfWeb, (Texture texture) => {
                        if( this != null )
                        {
                            centerImageSprite = TextureHelper.CreateSpriteFromTexture( ref texture );
                            isCenterImageReady = true;
                        }
                    }, null );
                }
            }

            if( IsUseBackgroundImageTrue() )
            {
                isBackgroundImageReady = false;

                if( IsBackgroundImageFormatTexture() && backgroundImageTexture != null )
                {
                    backgroundImageSprite = TextureHelper.CreateSpriteFromTexture( ref backgroundImageTexture );
                    isBackgroundImageReady = true;
                }
                else if( IsBackgroundImageFormatSprite() && backgroundImageSprite != null )
                {
                    isBackgroundImageReady = true;
                }
                else if( IsBackgroundImageFormatPath() && backgroundImagePath != "" )
                {
                    backgroundImageTexture = null;
                    backgroundImageSprite = null;

                    WWWHelper.instance.GetTexture( backgroundImagePath, backgroundImageCacheIfWeb, (Texture texture) => {
                        if( this != null )
                        {
                            backgroundImageSprite = TextureHelper.CreateSpriteFromTexture( ref texture );
                            isBackgroundImageReady = true;
                        }
                    }, null );
                }
            }

            if( IsUseParallaxImageTrue() )
            {
                isParallaxImageReady = false;

                if( IsParallaxImageFormatTexture() && parallaxImageTexture != null )
                {
                    parallaxImageSprite = TextureHelper.CreateSpriteFromTexture( ref parallaxImageTexture );
                    isParallaxImageReady = true;
                }
                else if( IsParallaxImageFormatSprite() && parallaxImageSprite != null )
                {
                    isParallaxImageReady = true;
                }
                else if( IsParallaxImageFormatPath() && parallaxImagePath != "" )
                {
                    parallaxImageTexture = null;
                    parallaxImageSprite = null;

                    WWWHelper.instance.GetTexture( parallaxImagePath, parallaxImageCacheIfWeb, (Texture texture) => {
                        if( this != null )
                        {
                            parallaxImageSprite = TextureHelper.CreateSpriteFromTexture( ref texture );
                            isParallaxImageReady = true;
                        }
                    }, null );
                }
            }

            if( IsUseFillImageTrue() )
            {
                if( IsFillImageFormatTexture() && fillImageTexture != null )
                {
                    fillImageSprite = TextureHelper.CreateSpriteFromTexture( ref fillImageTexture );
                }
                else if( IsFillImageFormatPath() && fillImagePath != "" )
                {
                    fillImageTexture = null;
                    fillImageSprite = null;

                    WWWHelper.instance.GetTexture( fillImagePath, fillImageCacheIfWeb, (Texture texture) => {
                        if( this != null )
                        {
                            fillImageSprite = TextureHelper.CreateSpriteFromTexture( ref texture );
                            isFillImageReady = true;
                        }
                    }, null );
                }
                

                if( IsFillBackgroundImageFormatTexture() && fillBackgroundImageTexture != null )
                {
                    fillBackgroundImageSprite = TextureHelper.CreateSpriteFromTexture( ref fillBackgroundImageTexture );
                }
                else if( IsFillBackgroundImageFormatPath() && fillBackgroundImagePath != "" )
                {
                    fillBackgroundImageTexture = null;
                    fillBackgroundImageSprite = null;

                    WWWHelper.instance.GetTexture( fillBackgroundImagePath, fillBackgroundImageCacheIfWeb, (Texture texture) => {
                        if( this != null )
                        {
                            fillBackgroundImageSprite = TextureHelper.CreateSpriteFromTexture( ref texture );
                            isFillBackgroundImageReady = true;
                        }
                    }, null );
                }
            }

            if( WhenDisabledChangeImagesAndTextAndShowingCenterImage() )
            {
                isDisabledCenterImageReady = false;

                if( IsDisabledCenterImageFormatTexture() && disabledCenterImageTexture != null )
                {
                    disabledCenterImageSprite = TextureHelper.CreateSpriteFromTexture( ref disabledCenterImageTexture );
                    isDisabledCenterImageReady = true;
                }
                else if( IsDisabledCenterImageFormatSprite() && disabledCenterImageSprite != null )
                {
                    isDisabledCenterImageReady = true;
                }
                else if( IsDisabledCenterImageFormatPath() && disabledCenterImagePath != "" )
                {
                    disabledCenterImageTexture = null;
                    disabledCenterImageSprite = null;

                    WWWHelper.instance.GetTexture( disabledCenterImagePath, disabledCenterImageCacheIfWeb, (Texture texture) => {
                        if( this != null )
                        {
                            disabledCenterImageSprite = TextureHelper.CreateSpriteFromTexture( ref texture );
                            isDisabledCenterImageReady = true;
                        }
                    }, null );
                }
            }

            if( WhenDisabledChangeImagesAndTextAndShowingBackgroundImage() )
            {
                isDisabledBackgroundImageReady = false;

                if( IsDisabledBackgroundImageFormatTexture() && disabledBackgroundImageTexture != null )
                {
                    disabledBackgroundImageSprite = TextureHelper.CreateSpriteFromTexture( ref disabledBackgroundImageTexture );
                    isDisabledBackgroundImageReady = true;
                }
                else if( IsDisabledBackgroundImageFormatSprite() && disabledBackgroundImageSprite != null )
                {
                    isDisabledBackgroundImageReady = true;
                }
                else if( IsDisabledBackgroundImageFormatPath() && disabledBackgroundImagePath != "" )
                {
                    disabledBackgroundImageTexture = null;
                    disabledBackgroundImageSprite = null;

                    WWWHelper.instance.GetTexture( disabledBackgroundImagePath, disabledBackgroundImageCacheIfWeb, (Texture texture) => {
                        if( this != null )
                        {
                            disabledBackgroundImageSprite = TextureHelper.CreateSpriteFromTexture( ref texture );
                            isDisabledBackgroundImageReady = true;
                        }
                    }, null );
                }
            }

            if( WhenDisabledChangeImagesAndTextAndShowingParallaxImage() )
            {
                isDisabledParallaxImageReady = false;

                if( IsDisabledParallaxImageFormatTexture() && disabledParallaxImageTexture != null )
                {
                    disabledParallaxImageSprite = TextureHelper.CreateSpriteFromTexture( ref disabledParallaxImageTexture );
                    isDisabledParallaxImageReady = true;
                }
                else if( IsDisabledParallaxImageFormatSprite() && disabledParallaxImageSprite != null )
                {
                    isDisabledParallaxImageReady = true;
                }
                else if( IsDisabledParallaxImageFormatPath() && disabledParallaxImagePath != "" )
                {
                    disabledParallaxImageTexture = null;
                    disabledParallaxImageSprite = null;

                    WWWHelper.instance.GetTexture( disabledParallaxImagePath, disabledParallaxImageCacheIfWeb, (Texture texture) => {
                        if( this != null )
                        {
                            disabledParallaxImageSprite = TextureHelper.CreateSpriteFromTexture( ref texture );
                            isDisabledParallaxImageReady = true;
                        }
                    }, null );
                }
            }

            if( WhenHoveringChangeImagesAndTextAndShowingCenterImage() )
            {
                isHoverCenterImageReady = false;

                if( IsHoverCenterImageFormatTexture() && hoverCenterImageTexture != null )
                {
                    hoverCenterImageSprite = TextureHelper.CreateSpriteFromTexture( ref hoverCenterImageTexture );
                    isHoverCenterImageReady = true;
                }
                else if( IsHoverCenterImageFormatSprite() && hoverCenterImageSprite != null )
                {
                    isHoverCenterImageReady = true;
                }
                else if( IsHoverCenterImageFormatPath() && hoverCenterImagePath != "" )
                {
                    hoverCenterImageTexture = null;
                    hoverCenterImageSprite = null;

                    WWWHelper.instance.GetTexture( hoverCenterImagePath, hoverCenterImageCacheIfWeb, ( Texture texture ) => {
                        if( this != null )
                        {
                            hoverCenterImageSprite = TextureHelper.CreateSpriteFromTexture( ref texture );
                            isHoverCenterImageReady = true;
                        }
                    }, null );
                }
            }

            if( WhenHoveringChangeImagesAndTextAndShowingBackgroundImage() )
            {
                isHoverBackgroundImageReady = false;

                if( IsHoverBackgroundImageFormatTexture() && hoverBackgroundImageTexture != null )
                {
                    hoverBackgroundImageSprite = TextureHelper.CreateSpriteFromTexture( ref hoverBackgroundImageTexture );
                    isHoverBackgroundImageReady = true;
                }
                else if( IsHoverBackgroundImageFormatSprite() && hoverBackgroundImageSprite != null )
                {
                    isHoverBackgroundImageReady = true;
                }
                else if( IsHoverBackgroundImageFormatPath() && hoverBackgroundImagePath != "" )
                {
                    hoverBackgroundImageTexture = null;
                    hoverBackgroundImageSprite = null;

                    WWWHelper.instance.GetTexture( hoverBackgroundImagePath, hoverBackgroundImageCacheIfWeb, ( Texture texture ) => {
                        if( this != null )
                        {
                            hoverBackgroundImageSprite = TextureHelper.CreateSpriteFromTexture( ref texture );
                            isHoverBackgroundImageReady = true;
                        }
                    }, null );
                }
            }

            if( WhenHoveringChangeImagesAndTextAndShowingParallaxImage() )
            {
                isHoverParallaxImageReady = false;

                if( IsHoverParallaxImageFormatTexture() && hoverParallaxImageTexture != null )
                {
                    hoverParallaxImageSprite = TextureHelper.CreateSpriteFromTexture( ref hoverParallaxImageTexture );
                    isHoverParallaxImageReady = true;
                }
                else if( IsHoverParallaxImageFormatSprite() && hoverParallaxImageSprite != null )
                {
                    isHoverParallaxImageReady = true;
                }
                else if( IsHoverParallaxImageFormatPath() && hoverParallaxImagePath != "" )
                {
                    hoverParallaxImageTexture = null;
                    hoverParallaxImageSprite = null;

                    WWWHelper.instance.GetTexture( hoverParallaxImagePath, hoverParallaxImageCacheIfWeb, ( Texture texture ) => {
                        if( this != null )
                        {
                            hoverParallaxImageSprite = TextureHelper.CreateSpriteFromTexture( ref texture );
                            isHoverParallaxImageReady = true;
                        }
                    }, null );
                }
            }

            if( WhenHoveringChangeImagesAndTextAndShowingFillImage() )
            {
                isHoverFillImageReady = false;

                if( IsHoverFillImageFormatTexture() && hoverFillImageTexture != null )
                {
                    hoverFillImageSprite = TextureHelper.CreateSpriteFromTexture( ref hoverFillImageTexture );
                    isHoverFillImageReady = true;
                }
                else if( IsHoverFillImageFormatSprite() && hoverFillImageSprite != null )
                {
                    isHoverFillImageReady = true;
                }
                else if( IsHoverFillImageFormatPath() && hoverFillImagePath != "" )
                {
                    hoverFillImageTexture = null;
                    hoverFillImageSprite = null;

                    WWWHelper.instance.GetTexture( hoverFillImagePath, hoverFillImageCacheIfWeb, ( Texture texture ) => {
                        if( this != null )
                        {
                            hoverFillImageSprite = TextureHelper.CreateSpriteFromTexture( ref texture );
                            isHoverFillImageReady = true;
                        }
                    }, null );
                }
            }

            if( WhenHoveringChangeImagesAndTextAndShowingFillBackgroundImage() )
            {
                isHoverFillBackgroundImageReady = false;

                if( IsHoverFillBackgroundImageFormatTexture() && hoverFillBackgroundImageTexture != null )
                {
                    hoverFillBackgroundImageSprite = TextureHelper.CreateSpriteFromTexture( ref hoverFillBackgroundImageTexture );
                    isHoverFillBackgroundImageReady = true;
                }
                else if( IsHoverFillBackgroundImageFormatSprite() && hoverFillBackgroundImageSprite != null )
                {
                    isHoverFillBackgroundImageReady = true;
                }
                else if( IsHoverFillBackgroundImageFormatPath() && hoverFillBackgroundImagePath != "" )
                {
                    hoverFillBackgroundImageTexture = null;
                    hoverFillBackgroundImageSprite = null;

                    WWWHelper.instance.GetTexture( hoverFillBackgroundImagePath, hoverFillBackgroundImageCacheIfWeb, ( Texture texture ) => {
                        if( this != null )
                        {
                            hoverFillBackgroundImageSprite = TextureHelper.CreateSpriteFromTexture( ref texture );
                            isHoverFillBackgroundImageReady = true;
                        }
                    }, null );
                }
            }

        } //END LoadTextureResources
        


        //--------------------------------//
        public void ForceHideCollider()
        //--------------------------------//
        {
            Color color = colliderMeshRenderer.sharedMaterial.color;

            if( colliderMeshRenderer != null )
            {
                colliderMeshRenderer.sharedMaterial.color = new Color( color.r, color.g, color.b, 0f );
            }

            if( collider2DImage != null )
            {
                collider2DImage.color = new Color( color.r, color.g, color.b, 0f );
            }

        } //END ForceHideCollider
        #endregion

        #region METHODS - ENABLE/DISABLE STATES
        //--------------------------------//
        public override void EnableHoverState()
        //--------------------------------//
        {
            if( name == debugWhenName ) Debug.Log( "EnableHoverState()" );
            base.EnableHoverState();

            //Time to start the hover state, but if we're currently disabled we don't need to do anything
            if( enabledState == EnabledSate.Disabled ) { return; }

            if( WhenHoveringDoNotChange() ) { return; }

            else if( WhenHoveringChangeImagesAndText() )
            {
                UITweener.TweenValue state = UITweener.TweenValue.HoverEnter;

                if( ShowChangeCenterImageDuringHoverOptions() )
                {
                    SetToState( state, uiColorTweenManager_CenterImage, uiScaleTweener_CenterImage, 
                                        isHoverCenterImageReady, hoverCenterImageSprite );
                }
                if( ShowChangeBackgroundImageDuringHoverOptions() )
                {
                    SetToState( state, uiColorTweenManager_BackgroundImage, uiScaleTweener_BackgroundImage,
                                        isHoverBackgroundImageReady, hoverBackgroundImageSprite );
                }
                if( ShowChangeParallaxImageDuringHoverOptions() )
                {
                    SetToState( state, uiColorTweenManager_ParallaxImage, uiScaleTweener_ParallaxImage,
                                        isHoverParallaxImageReady, hoverParallaxImageSprite );
                }
                if( ShowChangeFillImageDuringHoverOptions() )
                {
                    SetToState( state, uiColorTweenManager_FillImage, uiScaleTweener_FillImage,
                                        isHoverFillImageReady, hoverFillImageSprite,
                                        hoverFillMethod, GetHoverFillOrigin() );
                }
                if( ShowChangeFillBackgroundImageDuringHoverOptions() )
                {
                    //DOB Debug.Log( "EnableHoverState FillBackgroundImage state = " + state + ", color = " + uiColorTweenManager_FillBackgroundImage.tweeners[0].color_HoverEnter );
                    SetToState( state, uiColorTweenManager_FillBackgroundImage, uiScaleTweener_FillBackgroundImage,
                                        isHoverFillBackgroundImageReady, hoverFillBackgroundImageSprite );
                }
                if( ShowChangeTextDuringHoverOptions() )
                {
                    SetToState( state, uiColorTweenManager_TextMessage, uiScaleTweener_TextMessage,
                                           hoverTextMessage, hoverTextFont, hoverTextFontSize );
                }
                
            }

        } //END EnableHoverState

        //--------------------------------//
        public override void DisableHoverState()
        //--------------------------------//
        {
            if( name == debugWhenName ) Debug.Log( "DisableHoverState()" );
            base.DisableHoverState();

            //Time to end the hover state, but if we're currently disabled we don't need to do anything
            if( enabledState == EnabledSate.Disabled ) { return; }

            if( WhenHoveringDoNotChange() ) { return; }

            else if( WhenHoveringChangeImagesAndText() )
            {
                UITweener.TweenValue state = UITweener.TweenValue.HoverExit;
                
                if( ShowChangeCenterImageDuringHoverOptions() )
                {
                    SetToState( state, uiColorTweenManager_CenterImage, uiScaleTweener_CenterImage,
                                        isCenterImageReady, centerImageSprite );
                }
                if( ShowChangeBackgroundImageDuringHoverOptions() )
                {
                    SetToState( state, uiColorTweenManager_BackgroundImage, uiScaleTweener_BackgroundImage,
                                        isBackgroundImageReady, backgroundImageSprite );
                }
                if( ShowChangeParallaxImageDuringHoverOptions() )
                {
                    SetToState( state, uiColorTweenManager_ParallaxImage, uiScaleTweener_ParallaxImage,
                                        isParallaxImageReady, parallaxImageSprite );
                }
                if( ShowChangeFillImageDuringHoverOptions() )
                {
                    SetToState( state, uiColorTweenManager_FillImage, uiScaleTweener_FillImage,
                                        isFillImageReady, fillImageSprite,
                                        fillMethod, GetFillOrigin() );
                }
                if( ShowChangeFillBackgroundImageDuringHoverOptions() )
                {
                    SetToState( state, uiColorTweenManager_FillBackgroundImage, uiScaleTweener_FillBackgroundImage,
                                        isFillBackgroundImageReady, fillBackgroundImageSprite );
                }
                if( ShowChangeTextDuringHoverOptions() )
                {
                    SetToState( state, uiColorTweenManager_TextMessage, uiScaleTweener_TextMessage,
                                           textMessage, textFont, textFontSize );
                }

            }

        } //END DisableHoverState


        //--------------------------------//
        public override void Enable()
        //--------------------------------//
        {
            if( name == debugWhenName ) Debug.Log( "Enable()" );
            base.Enable();

            if( WhenDisabledDoNotChange() ) { return; }

            if( WhenDisabledHide() && ShowAfterBeingReEnabled )
            {
                Show();
            }
            else if( WhenDisabledChangeImagesAndText() )
            {
                UITweener.TweenValue state = UITweener.TweenValue.Enabled;

                if( WhenDisabledChangeImagesAndTextAndShowingCenterImage() )
                {
                    SetToState( state, uiColorTweenManager_CenterImage, uiScaleTweener_CenterImage,
                                        isCenterImageReady, centerImageSprite );
                }
                if( WhenDisabledChangeImagesAndTextAndShowingBackgroundImage() )
                {
                    SetToState( state, uiColorTweenManager_BackgroundImage, uiScaleTweener_BackgroundImage,
                                        isBackgroundImageReady, backgroundImageSprite );
                }
                if( WhenDisabledChangeImagesAndTextAndShowingParallaxImage() )
                {
                    SetToState( state, uiColorTweenManager_ParallaxImage, uiScaleTweener_ParallaxImage,
                                        isParallaxImageReady, parallaxImageSprite );
                }
                if( WhenDisabledChangeImagesAndTextAndShowText() )
                {
                    SetToState( state, uiColorTweenManager_TextMessage, uiScaleTweener_TextMessage,
                                           textMessage, textFont, textFontSize );
                }

                //Check if we should show the Fill Circle
                if( whenDisabledHideFillImage && IsUseFillImageTrue() ) { uiColorTweenManager_FillImage.Play( UITweener.TweenValue.Show ); }
            }

            if( onEnable != null ) { onEnable.Invoke(); }

        } //END Enable

        //--------------------------------//
        public override void Disable()
        //--------------------------------//
        {
            if( name == debugWhenName ) Debug.Log( "Disable()" );
            base.Disable();

            if( WhenDisabledDoNotChange() ) { return; }

            if( WhenDisabledHide() )
            {
                Hide();
            }
            else if( WhenDisabledChangeImagesAndText() )
            {
                UITweener.TweenValue state = UITweener.TweenValue.Disabled;

                if( WhenDisabledChangeImagesAndTextAndShowingCenterImage() )
                {
                    SetToState( state, uiColorTweenManager_CenterImage, uiScaleTweener_CenterImage,
                                        isDisabledCenterImageReady, disabledCenterImageSprite );
                }
                if( WhenDisabledChangeImagesAndTextAndShowingBackgroundImage() )
                {
                    SetToState( state, uiColorTweenManager_BackgroundImage, uiScaleTweener_BackgroundImage,
                                        isDisabledBackgroundImageReady, disabledBackgroundImageSprite );
                }
                if( WhenDisabledChangeImagesAndTextAndShowingParallaxImage() )
                {
                    SetToState( state, uiColorTweenManager_ParallaxImage, uiScaleTweener_ParallaxImage,
                                        isDisabledParallaxImageReady, disabledParallaxImageSprite );
                }
                else if( WhenDisabledChangeImagesAndTextAndShowText() )
                {
                    SetToState( state, uiColorTweenManager_TextMessage, uiScaleTweener_TextMessage,
                                           disabledTextMessage, disabledTextFont, disabledTextFontSize );
                }

                //Always hide the Fill Circle
                if( whenDisabledHideFillImage && IsUseFillImageTrue() ) { uiColorTweenManager_FillImage.Play( UITweener.TweenValue.Hide ); }
            }

            if( onDisable != null ) { onDisable.Invoke(); }

        } //END Disable


        //--------------------------------//
        private void SetToState
            ( UITweener.TweenValue tweenValue, UIColorTweenManager colorTweenManager, UIScaleTweener scaleTweener,
                string textMessage, Font textFont, int textFontSize )
        //--------------------------------//
        {
            if( uiTextField_Line1 != null && uiTextField_Line1.textField != null )
            {
                uiTextField_Line1.textField.text = textMessage;
                uiTextField_Line1.textField.font = textFont;
                uiTextField_Line1.textField.fontSize = textFontSize;
            }

            //If this block is hidden, make sure these renderers and transforms stay at their hidden value
            if( showState == ShowState.Hiding ) { tweenValue = UITweener.TweenValue.Hide; }

            if( colorTweenManager != null ) { colorTweenManager.Play( tweenValue ); }
            if( scaleTweener != null ) { scaleTweener.Play( tweenValue ); }

        } //END SetTextToState

        //--------------------------------//
        private void SetToState
            ( UITweener.TweenValue tweenValue, UIColorTweenManager colorTweenManager, UIScaleTweener scaleTweener,
                bool isReady, Sprite imageSprite, Image.FillMethod fillMethod, int fillOrigin )
        //--------------------------------//
        {
            if( colorTweenManager != null && colorTweenManager.tweeners != null && colorTweenManager.tweeners.Count > 0 && colorTweenManager.tweeners[ 0 ].image != null )
            {
                colorTweenManager.tweeners[ 0 ].image.fillMethod = fillMethod;
                colorTweenManager.tweeners[ 0 ].image.fillOrigin = fillOrigin;
            }

            SetToState( tweenValue, colorTweenManager, scaleTweener, isReady, imageSprite );

        } //END SetToState

        //--------------------------------//
        private void SetToState
            ( UITweener.TweenValue tweenValue, UIColorTweenManager colorTweenManager, UIScaleTweener scaleTweener,
                bool isReady, Sprite imageSprite )
        //--------------------------------//
        {

            if( isReady && imageSprite != null && colorTweenManager != null && colorTweenManager.tweeners != null && colorTweenManager.tweeners.Count > 0 && colorTweenManager.tweeners[ 0 ].image != null )
            {
                colorTweenManager.tweeners[ 0 ].image.sprite = imageSprite;
            }
            
            //If this block is hidden, make sure these renderers and transforms stay at their hidden value
            if( showState == ShowState.Hiding ) { tweenValue = UITweener.TweenValue.Hide; }

            if( colorTweenManager != null ) { colorTweenManager.Play( tweenValue ); }
            if( scaleTweener != null ) { scaleTweener.Play( tweenValue ); }

        } //END SetToState

        #endregion

        #region METHODS - SHOW/HIDE/FORCESHOW/FORCEHIDE LOGIC
        //--------------------------------//
        public override void Show()
        //--------------------------------//
        {
            base.Show();

            if( name == debugWhenName ) Debug.Log( "Show()" );

            //Enable our colliders
            SetCollider( true );

            if( PlayScaleInAndOutAnimationDuringIdle() )
            {
                if( enabledState == EnabledSate.Enabled || 
                    ( enabledState == EnabledSate.Disabled && !whenDisabledStopIdleAnimation ) )
                {
                    uiScaleAnimation_BlockButtonScaleInAndOut.Play();
                }
            }

            TweenManager.Kill( fillCircleImageRenderer );
            fillCircleImageRenderer.fillAmount = 0f;

            //What tweenValue we use (Show/HoverEnter/Disabled) depends on our state
            UITweener.TweenValue tweenValue = GetShowTweenValue( UITweener.TweenValue.Show );

            //Set our images and text to tween to our tweenValue based on our state
            if( IsUseCenterImageTrue() )
            {
                uiScaleTweener_CenterImage.Play( tweenValue );
                uiColorTweenManager_CenterImage.Play( tweenValue );
            }
            if( IsUseBackgroundImageTrue() )
            {
                uiScaleTweener_BackgroundImage.Play( tweenValue );
                uiColorTweenManager_BackgroundImage.Play( tweenValue );
            }
            if( IsUseParallaxImageTrue() )
            {
                uiScaleTweener_ParallaxImage.Play( tweenValue );
                uiColorTweenManager_ParallaxImage.Play( tweenValue );
            }
            if( showText )
            {
                uiScaleTweener_TextMessage.Play( tweenValue );
                uiColorTweenManager_TextMessage.Play( tweenValue );
            }
            if( IsUseFillImageTrue() )
            {
                if( enabledState == EnabledSate.Enabled || ( enabledState == EnabledSate.Disabled && !whenDisabledHideFillImage ) )
                {
                    uiScaleTweener_FillImage.Play( tweenValue );
                    uiColorTweenManager_FillImage.Play( tweenValue );

                    if( useGazeInput )
                    {
                        gazeCollider.AllowGazeGrow = true;
                    }
                }
            }
            if( IsUseFillBackgroundImageTrue() )
            {
                if( enabledState == EnabledSate.Enabled || ( enabledState == EnabledSate.Disabled && !whenDisabledHideFillImage ) )
                {
                    uiScaleTweener_FillBackgroundImage.Play( tweenValue );
                    uiColorTweenManager_FillBackgroundImage.Play( tweenValue );
                }
            }
            
            if( onShow != null ) { onShow.Invoke(); }
        
        } //END Show
        
        //-----------------------------------------------------------------------------//
        private UITweener.TweenValue GetShowTweenValue( UITweener.TweenValue tweenValue )
        //-----------------------------------------------------------------------------//
        {
            if( hoverState == HoverState.Hovering ) { tweenValue = UITweener.TweenValue.HoverEnter; }
            if( enabledState == EnabledSate.Disabled ) { tweenValue = UITweener.TweenValue.Disabled; }

            return tweenValue;

        } //END GetShowTweenValue

        //--------------------------------//
        public override void Hide()
        //--------------------------------//
        {
            base.Hide();

            if( name == debugWhenName ) Debug.Log( "Hide()" );

            //Disable our colliders
            SetCollider( false );

            //Check if our Idle animation should be stopped
            if( PlayScaleInAndOutAnimationDuringIdle() )
            {
                uiScaleAnimation_BlockButtonScaleInAndOut.Stop();
            }

            //Regardless of what we're using in this block, there are some values that are safe to always reset
            TweenManager.Kill( fillCircleImageRenderer );
            fillCircleImageRenderer.fillAmount = 0f;
            gazeCollider.AllowGazeGrow = false;

            //Set the images and text in this block to the new tweenValue
            UITweener.TweenValue tweenValue = UITweener.TweenValue.Hide;

            if( IsUseCenterImageTrue() )
            {
                uiScaleTweener_CenterImage.Play( tweenValue );
                uiColorTweenManager_CenterImage.Play( tweenValue );
            }
            if( IsUseBackgroundImageTrue() )
            {
                uiScaleTweener_BackgroundImage.Play( tweenValue );
                uiColorTweenManager_BackgroundImage.Play( tweenValue );
            }
            if( IsUseParallaxImageTrue() )
            {
                uiScaleTweener_ParallaxImage.Play( tweenValue );
                uiColorTweenManager_ParallaxImage.Play( tweenValue );
            }
            if( showText )
            {
                uiScaleTweener_TextMessage.Play( tweenValue );
                uiColorTweenManager_TextMessage.Play( tweenValue );
            }
            if( IsUseFillImageTrue() )
            {
                uiScaleTweener_FillImage.Play( tweenValue );
                uiColorTweenManager_FillImage.Play( tweenValue );
            }
            if( IsUseFillBackgroundImageTrue() )
            {
                uiScaleTweener_FillBackgroundImage.Play( tweenValue );
                uiColorTweenManager_FillBackgroundImage.Play( tweenValue );
            }
            

            if( onHide != null ) { onHide.Invoke(); }
        
        } //END Hide
        
        //--------------------------------//
        public override void ForceShow()
        //--------------------------------//
        {
            base.ForceShow();

            if( name == debugWhenName ) Debug.Log( "ForceShow()" );

            //Enable our colliders
            SetCollider( true );

            //Check if we should be playing an idle animation
            if( PlayScaleInAndOutAnimationDuringIdle() )
            {
                if( enabledState == EnabledSate.Enabled ||
                    ( enabledState == EnabledSate.Disabled && !whenDisabledStopIdleAnimation ) )
                {
                    uiScaleAnimation_BlockButtonScaleInAndOut.Play();
                }
            }

            //Regardless of what settings this block has, there are some variables that are always safe to reset regardless
            TweenManager.Kill( fillCircleImageRenderer );
            fillCircleImageRenderer.fillAmount = 0f;

            //What tweenValue we use (Show/HoverEnter/Disabled) depends on our state
            UITweener.TweenValue tweenValue = GetShowTweenValue( UITweener.TweenValue.Show );

            //Set our images and text to tween to our tweenValue based on our state
            if( IsUseCenterImageTrue() )
            {
                uiScaleTweener_CenterImage.Force( tweenValue );
                uiColorTweenManager_CenterImage.Force( tweenValue );
            }
            if( IsUseBackgroundImageTrue() )
            {
                uiScaleTweener_BackgroundImage.Force( tweenValue );
                uiColorTweenManager_BackgroundImage.Force( tweenValue );
            }
            if( IsUseParallaxImageTrue() )
            {
                uiScaleTweener_ParallaxImage.Force( tweenValue );
                uiColorTweenManager_ParallaxImage.Force( tweenValue );
            }
            if( showText )
            {
                uiScaleTweener_TextMessage.Force( tweenValue );
                uiColorTweenManager_TextMessage.Force( tweenValue );
            }
            if( IsUseFillImageTrue() )
            {
                if( enabledState == EnabledSate.Enabled || ( enabledState == EnabledSate.Disabled && !whenDisabledHideFillImage ) )
                {
                    uiScaleTweener_FillImage.Force( tweenValue );
                    uiColorTweenManager_FillImage.Force( tweenValue );

                    if( useGazeInput )
                    {
                        gazeCollider.AllowGazeGrow = true;
                    }
                }
            }
            if( IsUseFillBackgroundImageTrue() )
            {
                if( enabledState == EnabledSate.Enabled || ( enabledState == EnabledSate.Disabled && !whenDisabledHideFillImage ) )
                {
                    uiScaleTweener_FillBackgroundImage.Force( tweenValue );
                    uiColorTweenManager_FillBackgroundImage.Force( tweenValue );
                }
            }

            if( onForceShow != null ) { onForceShow.Invoke(); }
        
        } //END ForceShow

        //--------------------------------//
        public override void ForceHide()
        //--------------------------------//
        {
        
            base.ForceHide();

            if( name == debugWhenName ) Debug.Log( "ForceHide()" );

            //Disable our colliders
            SetCollider( false );

            //Stop the idle animation
            if( PlayScaleInAndOutAnimationDuringIdle() )
            {
                uiScaleAnimation_BlockButtonScaleInAndOut.Stop();
            }

            //Regardless of what we're using in this block, there are some values that are safe to always reset
            TweenManager.Kill( fillCircleImageRenderer );
            fillCircleImageRenderer.fillAmount = 0f;
            gazeCollider.AllowGazeGrow = false;

            //Set the images and text in this block to the new tweenValue
            UITweener.TweenValue tweenValue = UITweener.TweenValue.Hide;

            if( IsUseCenterImageTrue() )
            {
                uiScaleTweener_CenterImage.Force( tweenValue );
                uiColorTweenManager_CenterImage.Force( tweenValue );
            }
            if( IsUseBackgroundImageTrue() )
            {
                uiScaleTweener_BackgroundImage.Force( tweenValue );
                uiColorTweenManager_BackgroundImage.Force( tweenValue );
            }
            if( IsUseParallaxImageTrue() )
            {
                uiScaleTweener_ParallaxImage.Force( tweenValue );
                uiColorTweenManager_ParallaxImage.Force( tweenValue );
            }
            if( showText )
            {
                uiScaleTweener_TextMessage.Force( tweenValue );
                uiColorTweenManager_TextMessage.Force( tweenValue );
            }
            if( IsUseFillImageTrue() )
            {
                uiScaleTweener_FillImage.Force( tweenValue );
                uiColorTweenManager_FillImage.Force( tweenValue );
            }
            if( IsUseFillBackgroundImageTrue() )
            {
                uiScaleTweener_FillBackgroundImage.Force( tweenValue );
                uiColorTweenManager_FillBackgroundImage.Force( tweenValue );
            }

            if( onForceHide != null ) { onForceHide.Invoke(); }
        
        } //END ForceHide

        #endregion

        #region METHODS - MOUSE/TOUCH INTERACTION
        //---------------------------------//
        public void OnMouseEnter( BlockButton blockButton )
        //---------------------------------//
        {
            if( !useMouseAndTouchInput ) { return; }
            if( this != blockButton ) { return; }

            if( enabledState == EnabledSate.Disabled || showState == ShowState.Hiding ) { return; }

            //Track the hover state!
            EnableHoverState();
            
            if( playBlockEnterSound && isBlockEnterReadyToPlay )
            {
                AudioSource.PlayClipAtPoint( blockEnterAudioClip, Camera.main.transform.position, blockEnterVolume );
            }

            if( onColliderEnter != null ) { onColliderEnter.Invoke(); }
            
        } //END OnMouseEnter

        //---------------------------------//
        public void OnMouseOver( BlockButton blockButton )
        //---------------------------------//
        {
            if( !useMouseAndTouchInput ) { return; }
            if( this != blockButton ) { return; }
            
            if( enabledState == EnabledSate.Disabled || showState == ShowState.Hiding ) { return; }

            if( onColliderHover != null ) { onColliderHover.Invoke(); }

        } //END OnMouseOver

        //---------------------------------//
        public void OnMouseExit( BlockButton blockButton )
        //---------------------------------//
        {
            if( !useMouseAndTouchInput ) { return; }
            if( this != blockButton ) { return; }

            if( enabledState == EnabledSate.Disabled || showState == ShowState.Hiding ) { return; }

            //Track the hover state!
            DisableHoverState();
            
            if( playBlockExitSound && isBlockExitReadyToPlay )
            {
                AudioSource.PlayClipAtPoint( blockExitAudioClip, Camera.main.transform.position, blockExitVolume );
            }

            if( onColliderExit != null ) { onColliderExit.Invoke(); }

        } //END OnMouseExit

        //---------------------------------//
        public void OnMouseDown( BlockButton blockButton )
        //---------------------------------//
        {
            if( !useMouseAndTouchInput ) { return; }
            if( this != blockButton ) { return; }
            if( blockGroup == null ) { return; }
            if( enabledState == EnabledSate.Disabled || showState == ShowState.Hiding ) { return; }

            BlockButtonSelected();

        } //END OnMouseDown

        //---------------------------------//
        public void OnMouseDownDuringGazeHover( BlockButton blockButton )
        //---------------------------------//
        {
            if( !useGazeInput ) { return; }
            if( this != blockButton ) { return; }
            if( blockGroup == null ) { return; }

            BlockButtonSelected();

        } //END OnMouseDownDuringGazeHover

        #endregion

        #region METHODS - TAKE ACTION WHEN OTHER BLOCKS ARE SELECTED
        //---------------------------------//
        protected override void OtherBlockButtonSelected( BlockButton blockButton )
        //---------------------------------//
        {
            //If another block button has been selected, we may have to show or hide this block button
            CallActionOnOtherBlockSelected( ActionOnSelection.Hide, hideWhen, blockButton, ref otherBlockButtonsThatCauseHide, ref otherBlockGroupsThatCauseHide );
            CallActionOnOtherBlockSelected( ActionOnSelection.Show, showWhen, blockButton, ref otherBlockButtonsThatCauseShow, ref otherBlockGroupsThatCauseShow );
            CallActionOnOtherBlockSelected( ActionOnSelection.Disable, disableWhen, blockButton, ref otherBlockButtonsThatCauseDisable, ref otherBlockGroupsThatCauseDisable );
            CallActionOnOtherBlockSelected( ActionOnSelection.Enable, enableWhen, blockButton, ref otherBlockButtonsThatCauseEnable, ref otherBlockGroupsThatCauseEnable );

        } //END OtherBlockButtonSelected

        //---------------------------------//
        private void CallActionOnOtherBlockSelected( ActionOnSelection actionOnSelect, ActionOnSelectionOptions options, BlockButton otherBlockButton, ref List<BlockButton> otherBlockButtons, ref List<BlockGroup> otherBlockGroups )
        //---------------------------------//
        {
            bool callAction = false;

            if( options == ActionOnSelectionOptions.WhenAnyOtherButtonSelected )
            {
                //Debug.Log( "CallAction() " + name + " action = " + actionOnSelect + " options = " + options );
                callAction = true;
            }
            else if( options == ActionOnSelectionOptions.WhenAnyOtherButtonInThisGroupSelected && 
                     otherBlockButton.blockGroup == blockGroup )
            {
                callAction = true;
            }
            else if( options == ActionOnSelectionOptions.WhenAnyOtherButtonInSpecificGroupSelected )
            {
                if( otherBlockGroups != null && otherBlockGroups.Count > 0 )
                {
                    foreach( BlockGroup group in otherBlockGroups )
                    {
                        if( otherBlockButton.blockGroup == group )
                        {
                            callAction = true;
                            break;
                        }
                    }
                }
            }
            else if( options == ActionOnSelectionOptions.WhenSpecificButtonsSelected )
            {
                if( otherBlockButtons != null && otherBlockButtons.Count > 0 )
                {
                    foreach( BlockButton block in otherBlockButtons )
                    {
                        if( otherBlockButton == block )
                        {
                            callAction = true;
                            break;
                        }
                    }
                }
            }

            if( callAction )
            {
                if( actionOnSelect == ActionOnSelection.Hide ) { Hide(); }
                else if( actionOnSelect == ActionOnSelection.Show ) { Show(); }
                else if( actionOnSelect == ActionOnSelection.Disable )
                { 
                    Disable(); 
                }
                else if( actionOnSelect == ActionOnSelection.Enable )
                {
                    Enable(); 
                }
            }

        } //END CallActionOnOtherBlockSelected

        #endregion

        #region METHODS - THIS BLOCK SELECTED
        //---------------------------------//
        private void BlockButtonSelected()
        //---------------------------------//
        {
            
            //Debug.Log( "blockGroup.GetIsBlockGroupActivated() = " + blockGroup.GetIsBlockGroupActivated() );

            //If this block group is already (activated/selected/clicked), then check if we should prevent further logic
            if( ActionOnThisButtonSelectedIsDisable() && BlockFocusManager.instance.IsFocused( blockGroup ) ) { return; }

            //Only perform the Select logic if this button or group isn't already selected
            if( blockGroup != null )
            {
                if( name == debugWhenName ) Debug.Log( name + " Selected" );
                
                //Let the focus manager know we are no longer focusing on this block (because it's about to be selected!)
                BlockFocusManager.instance.SetGazeNotFocusingOnBlock( this );

                //Make sure our focus manager knows that we're selecting this block
                BlockFocusManager.instance.SetFocus( this );

                //Let all the other views/groups/blocks know there's a new block that's been selected
                if( BlockManager.instance != null ) BlockManager.instance.BlockButtonSelected( this );
                

                //Hide the blockButton icon if needed
                if( ActionOnThisButtonSelectedIsHide() )
                {
                    Hide();
                    uiColorTweenManager_FillBackgroundImage.Force( UITweener.TweenValue.Hide );
                }
                
                //Hide the fill circle
                fillCircleImageRenderer.Kill();
                fillCircleImageRenderer.fillAmount = 0f;

                //Fade out the fill in sfx
                FadeOutFillSFX();

                if( playBlockSelectSound && isBlockSelectReadyToPlay && blockSelectAudioClip != null && Camera.main != null )
                {
                    AudioSource.PlayClipAtPoint( blockSelectAudioClip, Camera.main.transform.position, blockSelectVolume );
                }

                if( onSelect != null ) { onSelect.Invoke(); }
            }
            
        } //END BlockButtonSelected

        #endregion

        #region METHODS - GAZE ENTER/EXIT/SELECT LOGIC
        //---------------------------------//
        public override void BlockButtonGazeEnter( BlockButton blockButton )
        //---------------------------------//
        {
            //Debug.Log( "BlockButton.cs GazeEnter() blockButton.name( " + blockButton.name + " ) != this ? " + (blockButton != this) );
            
            if( blockButton != this ) { return; }

            //Debug.Log( "BlockButton.cs GazeEnter() bool_DisableOnLookEvents = " + bool_DisableOnLookEvents + ", block != null ? " + ( block != null ) );

            if( enabledState == EnabledSate.Disabled || showState == ShowState.Hiding ) { return; }

            if( !useGazeInput ) { return; }

            //If this block group is already (activated/selected/clicked), then prevent further logic
            if( ActionOnThisButtonSelectedIsDisable() && BlockFocusManager.instance.IsFocused( blockGroup ) ) { return; }

            //Track the Gaze state!
            EnableHoverState();

            //The blockButton component has a collider, 
            //and when the gaze raycast triggers the event, 
            //we pass on that this event occured to the other blocks
            if( blockGroup != null ) { BlockManager.instance.BlockButtonGazeEnter( this ); }

            if( blockGroup != null )
            {
                
                //Figure out how much time is remaining to fill in the fill circle (it doesn't reset to zero when a player exits the collider, we continue where we left off)
                float enterColliderSpeed = gazeSelectionSpeed - fillCircleImageRenderer.fillAmount;

                //Debug.Log( "BlockButton.cs GazeEnter() enterColliderSpeed(" + enterColliderSpeed + ") = blockSelectionSpeed(" + blockSelectionSpeedInSeconds + ") - fillAmount(" + fillCircleImageRenderer.fillAmount + ") " );
                
                //Tween the fill circle
                TweenManager.Kill( fillCircleImageRenderer );
                fillCircleImageRenderer.fillAmount = 0f;
                TweenManager.ImageFill( fillCircleImageRenderer, 1f, enterColliderSpeed, EaseCurve.Linear, fillCircleImageRenderer.fillAmount, 0f, false, null );

                //Play the fill sfx
                if( blockLoopingFillSoundCoroutine != null ) { AudioHelper.instance.StopCoroutine( blockLoopingFillSoundCoroutine ); }

                if( playBlockLoopingFillSound && isBlockLoopingFillReadyToPlay )
                {
                    blockLoopingFillSoundSource.clip = blockLoopingFillAudioClip;
                    blockLoopingFillSoundSource.volume = blockLoopingFillVolume;
                    blockLoopingFillSoundSource.loop = true;
                    blockLoopingFillSoundSource.pitch = 1f;
                    blockLoopingFillSoundSource.Play();
                }
                
                if( blockLoopingFillSoundSource != null && blockLoopingFillSoundSource.clip != null && blockLoopingFillSoundSource.isPlaying && IsTweenPitchTrue() )
                {
                    TweenManager.AudioPitch( blockLoopingFillSoundSource, tweenPitchTo, enterColliderSpeed, tweenPitchEasing, blockLoopingFillSoundSource.pitch, 0f, false, null );
                }

                if( playBlockEnterSound && isBlockEnterReadyToPlay )
                {
                    //Debug.Log( "GazeEnter() Calling Play(blockEnterAudioClip)" );
                    AudioSource.PlayClipAtPoint( blockEnterAudioClip, Camera.main.transform.position, blockEnterVolume );
                }

                //Keep track of when we are trying to load the button via Gaze
                BlockFocusManager.instance.SetGazeFocusingOnBlock( this );

                if( onColliderEnter != null ) { onColliderEnter.Invoke(); }

                //Set a timer to go off, when it does we'll fade out the fill sfx
                handle_FadeOutFillSFX = Timer.instance.In( enterColliderSpeed - .15f, FadeOutFillSFX, gameObject );

                //Set a timer to go off, when it finishes we'll enter the block
                handle_StartEnterTimer = Timer.instance.In( enterColliderSpeed + .01f, BlockButtonGazeSelected, gameObject );
                
            }

        } //END BlockButtonGazeEnter

        //---------------------------------//
        private void FadeOutFillSFX()
        //---------------------------------//
        {

            //Stop the fill up sfx and play the exit sfx
            if( blockLoopingFillSoundSource != null )
            {
                //Debug.Log( "FadeOutFillSFX" );
                blockLoopingFillSoundCoroutine = AudioHelper.instance.Fade( blockLoopingFillSoundSource, 0f, .25f, 0f );
            }

        } //END FadeOutFillSFX

        //---------------------------------//
        public override void BlockButtonGazeExit( BlockButton blockButton )
        //---------------------------------//
        {
            if( blockButton != this ) { return; }

            if( !useGazeInput ) { return; }

            if( enabledState == EnabledSate.Disabled || showState == ShowState.Hiding ) { return; }

            if( blockGroup != null ) { BlockManager.instance.BlockButtonGazeExit( this ); }

            if( onColliderExit != null ) { onColliderExit.Invoke(); }

            //Track the Gaze state!
            DisableHoverState();

            //Check if we're exiting the collider while the loading circle fill was occuring
            if( blockGroup != null && !BlockFocusManager.instance.IsFocused( blockGroup ) )
            {
                //Debug.Log( "BlockButton.cs GazeExit()" );

                //Let the group know that we're no longer trying to activate this Block Button
                BlockFocusManager.instance.SetGazeNotFocusingOnBlock( this );
                ResetAfterGazeExitedBeforeSelected();
            }

        } //END BlockButtonGazeExit

        //---------------------------------//
        public void BlockButtonGazeSelected()
        //---------------------------------//
        {
            if( enabledState == EnabledSate.Disabled || showState == ShowState.Hiding ) { return; }

            if( !useGazeInput ) { return; }

            if( blockGroup != null )
            {
                //Debug.Log( "BlockButton.cs ThisBlockButtonSelected()" );

                //Set a boolean to keep track of when we're loading and not loading the block
                BlockFocusManager.instance.SetGazeNotFocusingOnBlock( this );

                if( BlockManager.instance != null ) BlockManager.instance.BlockButtonSelected( this );
                
                //Hide the blockButton icon if needed
                if( ActionOnThisButtonSelectedIsHide() )
                {
                    Hide();
                    uiColorTweenManager_FillBackgroundImage.Force( UITweener.TweenValue.Hide );
                }
                
                //Hide the fill circle
                TweenManager.Kill( fillCircleImageRenderer );
                fillCircleImageRenderer.fillAmount = 0f;

                //Fade out the fill in sfx
                FadeOutFillSFX();

                if( playBlockSelectSound && isBlockSelectReadyToPlay )
                {
                    AudioSource.PlayClipAtPoint( blockSelectAudioClip, Camera.main.transform.position, blockSelectVolume );
                }

                if( onSelect != null ) { onSelect.Invoke(); }
            }

        } //END BlockButtonGazeSelected



        //---------------------------------//
        public void ResetAfterGazeExitedBeforeSelected()
        //---------------------------------//
        {
            
            //Cancel the existing timers
            Timer.instance.Cancel( handle_FadeOutFillSFX );
            FadeOutFillSFX();

            Timer.instance.Cancel( handle_StartEnterTimer );
            
            //Make sure the blockButton icon is showing
            Show();

            //Reset the fill circle
            if( IsUseFillImageTrue() )
            {
                uiColorTweenManager_FillBackgroundImage.Play( UITweener.TweenValue.Show );
            }
            else
            {
                uiColorTweenManager_FillBackgroundImage.Play( UITweener.TweenValue.Hide );
            }

            TweenManager.Kill( fillCircleImageRenderer );

            //Have the fill circle tween back down to zero
            TweenManager.ImageFill( fillCircleImageRenderer, 0f, .5f, EaseCurve.Linear, fillCircleImageRenderer.fillAmount, 0f, false, null );

            if( playBlockExitSound && isBlockExitReadyToPlay )
            {
                //Debug.Log( "ResetAfterGazeExitedBeforeSelected() Calling Play(blockExitAudioClip)" );
                AudioSource.PlayClipAtPoint( blockExitAudioClip, Camera.main.transform.position, blockExitVolume );
            }

        } //END ResetAfterGazeExitedBeforeSelected

        #endregion


        //--------------------------------//
        public override void PrepareForDestroy()
        //--------------------------------//
        {

            //Stops any running tweens or timers associated with this block

            //Set a boolean to keep track of when we're loading and not loading the block
            if( blockGroup != null )
            {
                BlockFocusManager.instance.SetGazeNotFocusingOnBlock( this );
            }

            if( handle_StartEnterTimer != null ) { Timer.instance.Cancel( handle_StartEnterTimer ); }

        } //END PrepareForDestroy
        
        


        //--------------------------------------------------//
        public void SetCollider( bool enabled )
        //--------------------------------------------------//
        {

            if( collider3D != null )
            {
                collider3D.enabled = enabled;
            }

        } //END SetCollider

        //--------------------------------------------------//
        public override void InternetConnectionLost()
        //--------------------------------------------------//
        {
            
            if( disableWhenNoInternetAccess )
            {
                Disable();
            }
            
        } //END InternetConnectionLost

        //--------------------------------------------------//
        public override void InternetConnectionRestored()
        //--------------------------------------------------//
        {
            
            if( disableWhenNoInternetAccess )
            {
                Enable();
            }
            
        } //END InternetConnectionRestored



        public void TestEnter() { Debug.Log( "Enter" ); }
        public void TestExit() { Debug.Log( "Exit" ); }
        public void TestOver() { Debug.Log( "Over" ); }
        public void TestDown() { Debug.Log( "Down" ); }

        //----------------------------------------//
        public override void SetToViewType( BlockView.ViewType viewType )
        //----------------------------------------//
        {
            //Store the 'face camera' and transform variables in case we change from 2D to 3D
            if( !initialGroupTypeSet )
            {
                _position = transform.position;
                _rotation = transform.rotation;
                _scale = transform.localScale;
            }

            if( viewType == BlockView.ViewType.TwoDimensional )
            {
                //Disable the 3D collider
                collider3D.gameObject.SetActive( false );

                //Enable the 2D collider
                collider2DImage.gameObject.SetActive( true );

                //Force this block into the proper transform values for 2D space
                transform.localPosition = new Vector3( transform.localPosition.x, transform.localPosition.y, 0f );
                //transform.localEulerAngles = Vector3.zero;
                //transform.localScale = new Vector3( 1f, 1f, 1f );

            }
            else if( viewType == BlockView.ViewType.ThreeDimensional )
            {
                //Enable the 3D collider
                collider3D.gameObject.SetActive( true );
             
                //Disable the 2D collider
                collider2DImage.gameObject.SetActive( false );

                //Check if we need to force the block to face the camera
                if( initialGroupTypeSet )
                {
                    transform.position = _position;
                    transform.rotation = _rotation;
                    //transform.localScale = _scale;
                }
                else
                {
                    //transform.localPosition = new Vector3( 0f, 0f, 30f );
                    //transform.localEulerAngles = Vector3.zero;
                }

                //transform.localScale = new Vector3( .1f, .1f, .1f );
            }

            initialGroupTypeSet = true;

        } //END SetToGroupType

    } //END Class

} //END Namespace