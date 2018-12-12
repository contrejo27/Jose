using Sirenix.OdinInspector;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace BrandXR
{
    public class bxrTweenColor: bxrTween
    {
        private List<Type> validTypes = new List<Type>
        { typeof( Renderer ), typeof( Image ), typeof( RawImage ),
          typeof( SpriteRenderer ), typeof( Text ), typeof( Material ), typeof( CanvasGroup ) };


        public enum RendererType
        {
            Renderer,
            Image,
            RawImage,
            SpriteRenderer,
            Text,
            Material,
            CanvasGroup
        }
        [Tooltip( "What type of renderer are we tweening?" )]
        public RendererType rendererType = RendererType.Renderer;

        [ShowIf( "rendererType", RendererType.Renderer )]
        [Tooltip( "Standard Renderer" )]
        public Renderer _Renderer;

        [ShowIf( "rendererType", RendererType.Image )]
        [Tooltip( "Canvas Image" )]
        public Image image;

        [ShowIf( "rendererType", RendererType.RawImage )]
        [Tooltip( "Raw Image (non-canvas based 2D Texture)" )]
        public RawImage rawImage;

        [ShowIf( "rendererType", RendererType.SpriteRenderer )]
        [Tooltip( "Canvas Sprite Renderer" )]
        public SpriteRenderer spriteRenderer;

        [ShowIf( "rendererType", RendererType.Text )]
        [Tooltip( "Canvas Text" )]
        public Text text;

        [ShowIf( "rendererType", RendererType.Material )]
        [Tooltip( "Standard Material" )]
        public Material material;

        [ShowIf( "rendererType", RendererType.CanvasGroup )]
        [Tooltip( "Canvas Group" )]
        public CanvasGroup canvasGroup;

        public enum StartColorType
        {
            UseCurrentColor,
            SetColor
        }
        [Tooltip( "What color should this tween start from?" )]
        public StartColorType startColorType = StartColorType.UseCurrentColor;

        [ShowIf( "startColorType", StartColorType.SetColor )]
        public Color startColor = Color.white;
        public Color endColor = Color.white;

        //--------------------------------------------//
        public override void Start()
        //--------------------------------------------//
        {

            if( playOnStart )
            {
                Play();
            }

        } //END Start


        //--------------------------------------------//
        public override TweenType GetTweenType()
        //--------------------------------------------//
        {

            return TweenType.Position;

        } //END GetTweenType


        //----------------------------//
        public override void Play()
        //----------------------------//
        {

            //If Play() is called from this inherited class, we need to make sure our values are set up before attempting to tween
            if( rendererType == RendererType.Renderer )
            {
                Play( _Renderer, GetStartColor(), endColor, easeCurve, length, delay, loop, onCompleteOrLoop );
            }
            else if( rendererType == RendererType.Image )
            {
                Play( image, GetStartColor(), endColor, easeCurve, length, delay, loop, onCompleteOrLoop );
            }
            else if( rendererType == RendererType.RawImage )
            {
                Play( rawImage, GetStartColor(), endColor, easeCurve, length, delay, loop, onCompleteOrLoop );
            }
            else if( rendererType == RendererType.SpriteRenderer )
            {
                Play( spriteRenderer, GetStartColor(), endColor, easeCurve, length, delay, loop, onCompleteOrLoop );
            }
            else if( rendererType == RendererType.Text )
            {
                Play( text, GetStartColor(), endColor, easeCurve, length, delay, loop, onCompleteOrLoop );
            }
            else if( rendererType == RendererType.Material )
            {
                Play( material, GetStartColor(), endColor, easeCurve, length, delay, loop, onCompleteOrLoop );
            }
            else if( rendererType == RendererType.CanvasGroup )
            {
                Play( canvasGroup, GetStartColor(), endColor, easeCurve, length, delay, loop, onCompleteOrLoop );
            }

        } //END Play

        //--------------------------------------------//
        private Color GetStartColor()
        //--------------------------------------------//
        {

            //Used by the overriden Play() function of this class
            //Depending on user settings, we may want to get the "start" value from the current object, or use the passed in values
            if( startColorType == StartColorType.UseCurrentColor )
            {
                if( rendererType == RendererType.Renderer )
                {
                    return _Renderer.sharedMaterial.color;
                }
                else if( rendererType == RendererType.Image )
                {
                    return image.color;
                }
                else if( rendererType == RendererType.RawImage )
                {
                    return rawImage.color;
                }
                else if( rendererType == RendererType.SpriteRenderer )
                {
                    return spriteRenderer.color;
                }
                else if( rendererType == RendererType.Text )
                {
                    return text.color;
                }
                else if( rendererType == RendererType.Material )
                {
                    return material.color;
                }
                else if( rendererType == RendererType.CanvasGroup )
                {
                    return new Color( Color.white.r, Color.white.g, Color.white.b, canvasGroup.alpha );
                }
            }

            //else, return set value
            return this.startColor;

        } //END GetStartColor


        //--------------------------------------------//
        public bool IsValidComponent<T>( T checkThis )
        //--------------------------------------------//
        {

            foreach( Type type in validTypes )
            {
                if( checkThis.GetType() == type )
                {
                    return true;
                }
                else if( checkThis.GetType().IsAssignableFrom( type ) )
                {
                    //Debug.Log("bxrTweenColor.cs IsValidComponent() checkThis( " + checkThis.GetType() + " ) is assignable from type( " + type + " )");
                    return true;
                }
                else if( type.IsAssignableFrom( checkThis.GetType() ) )
                {
                    //Debug.Log("bxrTweenColor.cs IsValidComponent() type( " + type + " ) is assignable from checkThis( " + checkThis.GetType() + " )");
                    return true;
                }
            }

            return false;

        } //END IsValidComponent

        //--------------------------------------------//
        public RendererType GetType<T>( T checkThis )
        //--------------------------------------------//
        {

            foreach( Type type in validTypes )
            {
                if( checkThis.GetType() == type )
                {
                    return (RendererType)Enum.Parse( typeof( RendererType ), type.Name );
                }
            }

            return RendererType.Renderer;

        } //END GetType

        //--------------------------------------------//
        private void SetComponentToValue<T>( T Component, RendererType type )
        //--------------------------------------------//
        {

            if( type == RendererType.Renderer ) { this._Renderer = Component as Renderer; }
            else if( type == RendererType.Image ) { this.image = Component as Image; }
            else if( type == RendererType.RawImage ) { this.rawImage = Component as RawImage; }
            else if( type == RendererType.SpriteRenderer ) { this.spriteRenderer = Component as SpriteRenderer; }
            else if( type == RendererType.Text ) { this.text = Component as Text; }
            else if( type == RendererType.Material ) { this.material = Component as Material; }
            else if( type == RendererType.CanvasGroup ) { this.canvasGroup = Component as CanvasGroup; }

        } //END SetComponentToValue

        //-------------------------------------------//
        public Color GetDefaultStartValue<T>( T component )
        //-------------------------------------------//
        {

            RendererType type = GetType( component );

            if( type == RendererType.Renderer ) { return ( component as Renderer ).sharedMaterial.color; }
            else if( type == RendererType.Image ) { return ( component as Image ).color; }
            else if( type == RendererType.RawImage ) { return ( component as RawImage ).color; }
            else if( type == RendererType.SpriteRenderer ) { return ( component as SpriteRenderer ).color; }
            else if( type == RendererType.Text ) { return ( component as Text ).color; }
            else if( type == RendererType.Material ) { return ( component as Material ).color; }
            else if( type == RendererType.CanvasGroup ) { return new Color( Color.white.r, Color.white.g, Color.white.b, ( component as CanvasGroup ).alpha ); }

            return Color.white;

        } //END GetDefaultStartValue

        //--------------------------------------------//
        public void Play<T>( T Component, Color startColor, Color endColor, AnimationCurve easeCurve, float length, float delay, bool loop, UnityEvent onCompleteOrLoop )
        //--------------------------------------------//
        {

            //Setup the object and its values for the tween
            if( Component != null && IsValidComponent(Component) )
            {
                rendererType = GetType( Component );
                SetComponentToValue( Component, rendererType );

                this.startColor = startColor;
                this.endColor = endColor;

                Setup( easeCurve, length, delay, loop, onCompleteOrLoop );

                //Debug.Log( "bxrTweenColor.cs Play(), about to call base.Play()" );

                //Now that the values are setup, call the Play() from the base class to begin the tween
                base.Play();
            }
            else
            {
                if( Component == null )
                {
                    Debug.Log("bxrTweenColor.cs Play() Unable to Play Tween, Component variable is null");
                }
                else if( Component != null && !IsValidComponent(Component) )
                {
                    Debug.Log("bxrTweenColor.cs Play() Unable to Play Tween, Component variable is not null but is not a valid component or object that can be tweened.\n Component type = " + Component.GetType() );
                }
            }

        } //END Play


        //----------------------------//
        protected override void SetNewValues( float timer )
        //----------------------------//
        {
            //Perform tween logic every coroutine update
            if( rendererType == RendererType.Renderer )
            {
                _Renderer.sharedMaterial.color = Color.Lerp( startColor, endColor, easeCurve.Evaluate( timer / length ) );
            }
            else if( rendererType == RendererType.Image )
            {
                image.color = Color.Lerp( startColor, endColor, easeCurve.Evaluate( timer / length ) );
            }
            else if( rendererType == RendererType.RawImage )
            {
                rawImage.color = Color.Lerp( startColor, endColor, easeCurve.Evaluate( timer / length ) );
            }
            else if( rendererType == RendererType.SpriteRenderer )
            {
                spriteRenderer.color = Color.Lerp( startColor, endColor, easeCurve.Evaluate( timer / length ) );
            }
            else if( rendererType == RendererType.Text )
            {
                text.color = Color.Lerp( startColor, endColor, easeCurve.Evaluate( timer / length ) );
            }
            else if( rendererType == RendererType.Material )
            {
                material.color = Color.Lerp( startColor, endColor, easeCurve.Evaluate( timer / length ) );
            }
            else if( rendererType == RendererType.CanvasGroup )
            {
                canvasGroup.alpha = Color.Lerp( startColor, endColor, easeCurve.Evaluate( timer / length ) ).a;
            }

        } //END SetNewValue

        //-----------------------//
        protected override void PrepareForLoop()
        //-----------------------//
        {

            //Flip the start and end values to prepare to loop back to the start
            Color temp = endColor;

            endColor = startColor;
            startColor = temp;

        } //END PrepareValuesForLoop

        //--------------------------------------------//
        public override Component GetLinkedComponent()
        //--------------------------------------------//
        {

            if( rendererType == RendererType.Renderer && _Renderer != null ) { return _Renderer as Component; }
            else if( rendererType == RendererType.Image && image != null ) { return image as Component; }
            else if( rendererType == RendererType.RawImage && rawImage != null ) { return rawImage as Component; }
            else if( rendererType == RendererType.SpriteRenderer && spriteRenderer != null ) { return spriteRenderer as Component; }
            else if( rendererType == RendererType.Text && text != null ) { return text as Component; }
            else if( rendererType == RendererType.CanvasGroup && canvasGroup != null ) { return canvasGroup as Component; }

            return null;

        } //END GetLinkedComponent

        //------------------------------------//
        public override UnityEngine.Object GetLinkedObject()
        //------------------------------------//
        {
            if( rendererType == RendererType.Material && material != null ) { return material as UnityEngine.Object; }

            return null;

        } //END GetLinkedObject

    } //END Class

} //END Namespace