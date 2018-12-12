using Sirenix.OdinInspector;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace BrandXR
{
    public class BlockEventColor: BlockEventBase
    {
        //----------------- ACTIONS --------------------------------//
        public enum Actions
        {
            None,
            ChangeColor
        }

        [TitleGroup( "Block Event - Color", "Used to change the color of a renderer or image" )]
        public Actions action = Actions.ChangeColor;
        private bool ShowChangeColor() { return action == Actions.ChangeColor; }

        //------------- VARIABLES ---------------------------------//
        public enum ChangeColorOn
        {
            MeshRenderer,
            Material,
            RawImage,
            Image,
            SpriteRenderer,
            Text,
            CanvasGroup,
            UIColorTweener,
            UIColorTweenManager,
            UIColorAnimation
        }

        [ShowIf( "ShowChangeColor" ), FoldoutGroup( "Color Settings" )]
        public ChangeColorOn changeColorOn = ChangeColorOn.MeshRenderer;
        private bool IsChangeColorOnMeshRenderer() { return ShowChangeColor() && changeColorOn == ChangeColorOn.MeshRenderer; }
        private bool IsChangeColorOnMaterial() { return ShowChangeColor() && changeColorOn == ChangeColorOn.Material; }
        private bool IsChangeColorIsChangingMaterial() { return ShowChangeColor() && ( changeColorOn == ChangeColorOn.MeshRenderer && changeColorOn == ChangeColorOn.Material ); }

        private bool IsChangeColorOnRawImage() { return ShowChangeColor() && changeColorOn == ChangeColorOn.RawImage; }
        private bool IsChangeColorOnImage() { return ShowChangeColor() && changeColorOn == ChangeColorOn.Image; }
        private bool IsChangeColorOnSpriteRenderer() { return ShowChangeColor() && changeColorOn == ChangeColorOn.SpriteRenderer; }

        private bool IsChangeColorOnText() { return ShowChangeColor() && changeColorOn == ChangeColorOn.Text; }
        private bool IsChangeColorOnCanvasGroup() { return ShowChangeColor() && changeColorOn == ChangeColorOn.CanvasGroup; }

        private bool IsChangeColorOnUIColorTweener() { return ShowChangeColor() && changeColorOn == ChangeColorOn.UIColorTweener; }
        private bool IsChangeColorOnUIColorTweenManager() { return ShowChangeColor() && changeColorOn == ChangeColorOn.UIColorTweenManager; }
        private bool IsChangeColorOnUIColorTweenerOrTweenManager() { return ShowChangeColor() && ( changeColorOn == ChangeColorOn.UIColorTweener || changeColorOn == ChangeColorOn.UIColorTweenManager ); }
        private bool IsChangeColorOnUIColorAnimation() { return ShowChangeColor() && changeColorOn == ChangeColorOn.UIColorAnimation; }

        [ShowIf( "IsChangeColorOnMeshRenderer" ), FoldoutGroup( "Color Settings" )]
        public MeshRenderer color_MeshRenderer;

        [ShowIf( "IsChangeColorOnMaterial" ), FoldoutGroup( "Color Settings" )]
        public Material color_Material;

        [ShowIf( "IsChangeColorOnRawImage" ), FoldoutGroup( "Color Settings" )]
        public RawImage color_RawImage;

        [ShowIf( "IsChangeColorOnImage" ), FoldoutGroup( "Color Settings" )]
        public Image color_Image;

        [ShowIf( "IsChangeColorOnSpriteRenderer" ), FoldoutGroup( "Color Settings" )]
        public SpriteRenderer color_SpriteRenderer;


        [ShowIf( "IsChangeColorOnText" ), FoldoutGroup( "Color Settings" )]
        public Text color_Text;

        

        [ShowIf( "IsChangeColorOnCanvasGroup" ), FoldoutGroup( "Color Settings" )]
        public CanvasGroup color_CanvasGroup;


        [ShowIf( "IsChangeColorOnUIColorTweener" ), FoldoutGroup( "Color Settings" )]
        public UIColorTweener color_UIColorTweener;

        [ShowIf( "IsChangeColorOnUIColorTweenManager" ), FoldoutGroup( "Color Settings" )]
        public UIColorTweenManager color_UIColorTweenManager;

        [ShowIf( "IsChangeColorOnUIColorAnimation" ), FoldoutGroup( "Color Settings" )]
        public UIColorAnimation color_UIColorAnimation;


        private bool IsNotColorAnimation() { return ShowChangeColor() && changeColorOn != ChangeColorOn.UIColorAnimation; }

        [ShowIf( "IsNotColorAnimation" ), FoldoutGroup( "Color Settings" )]
        public Color changeToColor; //Color animation uses a color array, everything else uses one color


        //--------- Non-UI Color Tweener variables

        [ShowIf( "IsChangeColorIsChangingMaterial" ), FoldoutGroup( "Color Settings" )]
        public bool changeMaterialColorProperty = false;
        private bool IsChangeMaterialColorPropertyTrue() { return IsChangeColorIsChangingMaterial() && changeMaterialColorProperty; }

        [ShowIf( "IsChangeMaterialColorPropertyTrue" ), FoldoutGroup( "Color Settings" )]
        public string materialProperty = "_Color";

        //----------

        public enum ChangeColorSettings
        {
            ChangeToColor,
            FadeToColor
        }

        [ShowIf( "IsNotColorAnimation" ), FoldoutGroup( "Color Settings" )]
        public ChangeColorSettings changeColorSettings = ChangeColorSettings.FadeToColor;
        private bool colorSettingsSetToChange() { return ShowChangeColor() && changeColorSettings == ChangeColorSettings.ChangeToColor; }
        private bool colorSettingsSetToFade() { return ShowChangeColor() && IsNotColorAnimation() && changeColorSettings == ChangeColorSettings.FadeToColor; }

        [ShowIf( "colorSettingsSetToFade" ), FoldoutGroup( "Color Settings" )]
        public float colorTweenSpeed = 1f;

        [ShowIf( "colorSettingsSetToFade" ), FoldoutGroup( "Color Settings" )]
        public float colorTweenDelay = 0f;

        [ShowIf( "colorSettingsSetToFade" ), FoldoutGroup( "Color Settings" )]
        public EaseCurve.EaseType colorEaseType = EaseCurve.EaseType.Linear;

        //--------- ColorTweener

        [ShowIf( "IsChangeColorOnUIColorTweener" ), FoldoutGroup( "Color Settings" )]
        public UITweener.TweenValue changeColorTweenValue = UITweener.TweenValue.Show;

        //--------- Color Array

        [ShowIf( "IsChangeColorOnUIColorAnimation" ), FoldoutGroup( "Color Settings" )]
        public Color[] changeToColorArray;

        [ShowIf( "IsChangeColorOnUIColorAnimation" ), FoldoutGroup( "Color Settings" )]
        public float[] changeColorAnimationSpeeds;

        [ShowIf( "IsChangeColorOnUIColorAnimation" ), FoldoutGroup( "Color Settings" )]
        public EaseCurve.EaseType[] changeColorAnimationEaseTypes;

        [ShowIf( "IsChangeColorOnUIColorAnimation" ), FoldoutGroup( "Color Settings" )]
        public float[] changeColorAnimationDelays;

        [ShowIf( "IsChangeColorOnUIColorAnimation" ), FoldoutGroup( "Color Settings" )]
        public bool playColorAnimation;


        //-------------------- "CHANGE COLOR" EVENT MESSAGES ---------------------//
        private bool ColorSettingsSetToChangeOrChangingUIAnimation() { return action == Actions.ChangeColor && ( colorSettingsSetToChange() || changeColorOn == ChangeColorOn.UIColorAnimation ); }

        [ShowIf( "ColorSettingsSetToChangeOrChangingUIAnimation" ), FoldoutGroup( "Event Messages" )]
        public UnityEvent onChangeColor = new UnityEvent();

        [ShowIf( "colorSettingsSetToFade" ), FoldoutGroup( "Event Messages" )]
        public UnityEvent onFadeToColor = new UnityEvent();



        //---------------------------------------------------------//
        public override EventType GetEventType()
        //---------------------------------------------------------//
        {
            return EventType.Color;

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

            if( action == Actions.ChangeColor )
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
                if( action == Actions.ChangeColor )
                {
                    CallChangeColorEvent();
                }
            }
            
        } //END CallEvent

        //----------------------------------//
        private void CallChangeColorEvent()
        //----------------------------------//
        {

            if( DoesChangeColorObjectExist() )
            {
                if( changeColorSettings == ChangeColorSettings.ChangeToColor && changeColorOn != ChangeColorOn.UIColorAnimation )
                {
                    if( changeColorOn == ChangeColorOn.MeshRenderer ) { color_MeshRenderer.sharedMaterial.color = changeToColor; }
                    else if( changeColorOn == ChangeColorOn.Material ) { color_Material.color = changeToColor; }
                    else if( changeColorOn == ChangeColorOn.RawImage ) { color_RawImage.color = changeToColor; }
                    else if( changeColorOn == ChangeColorOn.Image ) { color_Image.color = changeToColor; }
                    else if( changeColorOn == ChangeColorOn.SpriteRenderer ) { color_SpriteRenderer.color = changeToColor; }
                    else if( changeColorOn == ChangeColorOn.Text ) { color_Text.color = changeToColor; }
                    else if( changeColorOn == ChangeColorOn.CanvasGroup ) { color_CanvasGroup.alpha = changeToColor.a; }
                    else if( changeColorOn == ChangeColorOn.UIColorTweener ) { color_UIColorTweener.Force( changeToColor, changeColorTweenValue ); }
                    else if( changeColorOn == ChangeColorOn.UIColorTweenManager ) { color_UIColorTweenManager.Force( changeToColor, changeColorTweenValue ); }

                    if( colorSettingsSetToChange() && onChangeColor != null ) { onChangeColor.Invoke(); }
                }
                else if( changeColorSettings == ChangeColorSettings.FadeToColor && changeColorOn != ChangeColorOn.UIColorAnimation )
                {
                    /*
                    string property = "_Color";

                    if( IsChangeMaterialColorPropertyTrue() )
                    {
                        property = materialProperty;
                    }
                    */

                    UnityEvent _event = new UnityEvent();
                    _event.AddListener( LoadColorTweenComplete );

                    if( changeColorOn == ChangeColorOn.MeshRenderer )               { TweenManager.Color( color_MeshRenderer, changeToColor, colorTweenSpeed, colorEaseType, color_MeshRenderer.sharedMaterial.color, colorTweenDelay, false, _event ); }
                    else if( changeColorOn == ChangeColorOn.Material )              { TweenManager.Color( color_Material, changeToColor, colorTweenSpeed, colorEaseType, color_Material.color, colorTweenDelay, false, _event ); }
                    else if( changeColorOn == ChangeColorOn.RawImage )              { TweenManager.Color( color_RawImage, changeToColor, colorTweenSpeed, colorEaseType, color_RawImage.color, colorTweenDelay, false, _event ); }
                    else if( changeColorOn == ChangeColorOn.Image )                 { TweenManager.Color( color_Image, changeToColor, colorTweenSpeed, colorEaseType, color_Image.color, colorTweenDelay, false, _event ); }
                    else if( changeColorOn == ChangeColorOn.SpriteRenderer )        { TweenManager.Color( color_SpriteRenderer, changeToColor, colorTweenSpeed, colorEaseType, color_SpriteRenderer.color, colorTweenDelay, false, _event ); }
                    else if( changeColorOn == ChangeColorOn.Text )                  { TweenManager.Color( color_Text, changeToColor, colorTweenSpeed, colorEaseType, color_Text.color, colorTweenDelay, false, _event ); }
                    else if( changeColorOn == ChangeColorOn.CanvasGroup )           { TweenManager.Color( color_CanvasGroup, changeToColor, colorTweenSpeed, colorEaseType, new Color( Color.white.r, Color.white.g, Color.white.b, color_CanvasGroup.alpha ), colorTweenDelay, false, _event ); }
                    else if( changeColorOn == ChangeColorOn.UIColorTweener )        { color_UIColorTweener.Play( UITweener.TweenValue.Show, changeToColor, colorTweenSpeed, colorTweenDelay, colorEaseType, _event ); }
                    else if( changeColorOn == ChangeColorOn.UIColorTweenManager )   { color_UIColorTweenManager.Play( UITweener.TweenValue.Show, changeToColor, colorTweenSpeed, colorTweenDelay, colorEaseType, _event ); }

                }
                else if( changeColorOn == ChangeColorOn.UIColorAnimation )
                {
                    if( color_UIColorAnimation.colorsToAnimateTo == null ) { color_UIColorAnimation.colorsToAnimateTo = new Color[] { }; }
                    if( color_UIColorAnimation.animationSpeeds == null ) { color_UIColorAnimation.animationSpeeds = new float[] { }; }
                    if( color_UIColorAnimation.easeTypes == null ) { color_UIColorAnimation.easeTypes = new EaseCurve.EaseType[] { }; }
                    if( color_UIColorAnimation.delays == null ) { color_UIColorAnimation.delays = new float[] { }; }

                    color_UIColorAnimation.colorsToAnimateTo = changeToColorArray;
                    color_UIColorAnimation.animationSpeeds = changeColorAnimationSpeeds;
                    color_UIColorAnimation.easeTypes = changeColorAnimationEaseTypes;
                    color_UIColorAnimation.delays = changeColorAnimationDelays;

                    if( playColorAnimation ) { color_UIColorAnimation.Play(); }

                    if( colorSettingsSetToChange() && onChangeColor != null ) { onChangeColor.Invoke(); }
                }
            }

        } //END CallChangeColorEvent

        //----------------------------------//
        private bool DoesChangeColorObjectExist()
        //----------------------------------//
        {

            bool exists = false;

            if( IsChangeColorOnMeshRenderer() && color_MeshRenderer != null ) { exists = true; }
            else if( IsChangeColorOnMaterial() && color_Material != null ) { exists = true; }
            else if( IsChangeColorOnRawImage() && color_RawImage != null ) { exists = true; }
            else if( IsChangeColorOnImage() && color_Image != null ) { exists = true; }
            else if( IsChangeColorOnSpriteRenderer() && color_SpriteRenderer != null ) { exists = true; }
            else if( IsChangeColorOnText() && color_Text != null ) { exists = true; }
            else if( IsChangeColorOnCanvasGroup() && color_CanvasGroup != null ) { exists = true; }
            else if( IsChangeColorOnUIColorTweener() && color_UIColorTweener != null ) { exists = true; }
            else if( IsChangeColorOnUIColorTweenManager() && DoesUIColorTweenManagerHaveTweens() ) { exists = true; }
            else if( IsChangeColorOnUIColorAnimation() && color_UIColorAnimation != null ) { exists = true; }

            return exists;

        } //END DoesChangeColorObjectExist

        //---------------------------------//
        private bool DoesUIColorTweenManagerHaveTweens()
        //---------------------------------//
        {

            if( color_UIColorTweenManager != null && color_UIColorTweenManager.tweeners != null && color_UIColorTweenManager.tweeners.Count > 0 && color_UIColorTweenManager.tweeners[ 0 ] != null )
            {
                return true;
            }
            else
            {
                return false;
            }

        } //END DoesUIColorTweenManagerHaveTweens

        //----------------------------------//
        private void LoadColorTweenComplete()
        //----------------------------------//
        {

            if( colorSettingsSetToFade() && onFadeToColor != null ) { onFadeToColor.Invoke(); }

        } //END LoadColorTweenComplete

    } //END BlockEventColor

} //END Namespace