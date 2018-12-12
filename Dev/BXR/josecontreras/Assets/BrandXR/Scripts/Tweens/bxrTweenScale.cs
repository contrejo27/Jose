using Sirenix.OdinInspector;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

namespace BrandXR
{
    public class bxrTweenScale: bxrTween
    {
        private List<Type> validTypes = new List<Type>
        { typeof( Transform ), typeof( RectTransform ) };

        public enum TransformType
        {
            Transform,
            RectTransform
        }
        [Tooltip( "What type of transform are we tweening?" )]
        public TransformType transformType = TransformType.Transform;

        [ShowIf( "transformType", TransformType.Transform )]
        [Tooltip( "Standard GameObject's Transform, all three values (X, Y, Z) in the local scale will be tweened" )]
        public Transform transformThis;

        [ShowIf( "transformType", TransformType.RectTransform )]
        [Tooltip( "Unity Canvas RectTransform, all three values (X, Y, Z) in the local scale will be tweened" )]
        public RectTransform rectTransform;
        
        public enum StartScaleType
        {
            UseCurrentScale,
            SetScale
        }
        [Tooltip( "Where should this tween start at?" )]
        public StartScaleType startScaleType = StartScaleType.UseCurrentScale;

        [ShowIf( "startScaleType", StartScaleType.SetScale )]
        public Vector3 startScale = Vector3.one;

        public enum EndScaleType
        {
            SetScale,
            UseMultiplier
        }
        [Tooltip("Change your scale via a multiplier or by explicitely setting the final value of the tween")]
        public EndScaleType endScaleType = EndScaleType.UseMultiplier;

        [ShowIf( "endScaleType", EndScaleType.SetScale ), Tooltip( "If using rectTransform from a canvas object, only the X & Y variables will be tweened to via the sizeDelta using this variable" )]
        public Vector3 endScale = Vector3.one;

        [ShowIf("endScaleType", EndScaleType.UseMultiplier), Tooltip("Multiplies your scale by this amount")]
        public float endScaleMultiplier = 1f;
        

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

            return TweenType.Scale;

        } //END GetTweenType

        //----------------------------//
        public override void Play()
        //----------------------------//
        {

            //If Play() is called from this inherited class, we need to make sure our values are set up before attempting to tween
            if( transformType == TransformType.Transform )
            {
                Play( transformThis, GetStartScale(), GetEndScale(), easeCurve, length, delay, loop, onCompleteOrLoop );
            }
            else if( transformType == TransformType.RectTransform )
            {
                Play( rectTransform, GetStartScale(), GetEndScale(), easeCurve, length, delay, loop, onCompleteOrLoop );
            }

        } //END Play

        //--------------------------------------------//
        private Vector3 GetStartScale()
        //--------------------------------------------//
        {
            
            //Used by the overriden Play() function of this class
            //Depending on user settings, we may want to get the "start" value from the current object, or use the passed in values
            if( startScaleType == StartScaleType.UseCurrentScale )
            {
                if( transformType == TransformType.Transform )
                {
                    return transformThis.localScale;
                }
                else if( transformType == TransformType.RectTransform )
                {
                    return rectTransform.localScale;
                }
            }

            //else, return set value
            return this.startScale;

        } //END GetStartScale

        //--------------------------------------------//
        private Vector3 GetEndScale()
        //--------------------------------------------//
        {
            
            //Used by the overriden Play() function of this class
            //Depending on user settings, we may want to get the "end" value via a multiplication of the start value, or use the passed in values
            if( endScaleType == EndScaleType.SetScale )
            {
                return endScale;
            }
            else if( endScaleType == EndScaleType.UseMultiplier )
            {
                return ComputeEndScaleUsingMultiplier( GetStartScale(), endScaleMultiplier );
            }

            //else, return set value
            return endScale;

        } //END GetEndScale

        //--------------------------------------------//
        private Vector3 ComputeEndScaleUsingMultiplier( Vector3 startS, float multiplier )
        //--------------------------------------------//
        {

            return startS * multiplier;

        } //END ComputeEndScaleUsingMultiplier



        //--------------------------------------------//
        public void Play<T>( T changeThis, Vector3 startScale, float endScaleMultiplier, AnimationCurve easeCurve, float length, float delay, bool loop, UnityEvent onCompleteOrLoop ) where T : Transform
        //--------------------------------------------//
        {
            this.endScaleType = EndScaleType.UseMultiplier;
            this.endScaleMultiplier = endScaleMultiplier;
            endScale = ComputeEndScaleUsingMultiplier( startScale, endScaleMultiplier );

            _Play<T>( changeThis, startScale, endScale, easeCurve, length, delay, loop, onCompleteOrLoop );

        } //END Play
        
        //--------------------------------------------//
        public void Play<T>( T changeThis, Vector3 startScale, Vector3 endScale, AnimationCurve easeCurve, float length, float delay, bool loop, UnityEvent onCompleteOrLoop ) where T : Transform
        //--------------------------------------------//
        {
            this.endScaleType = EndScaleType.SetScale;

            _Play( changeThis, startScale, endScale, easeCurve, length, delay, loop, onCompleteOrLoop );

        } //END Play





        //--------------------------------------------//
        public bool IsValidComponent<T>(T checkThis) where T : Transform
        //--------------------------------------------//
        {

            foreach (Type type in validTypes)
            {
                if (checkThis.GetType() == type)
                {
                    return true;
                }
                else if (checkThis.GetType().IsAssignableFrom(type))
                {
                    return true;
                }
                else if (type.IsAssignableFrom(checkThis.GetType()))
                {
                    return true;
                }
            }

            return false;

        } //END IsValidComponent

        //--------------------------------------------//
        public TransformType GetType<T>( T checkThis ) where T : Transform
        //--------------------------------------------//
        {

            foreach( Type type in validTypes )
            {
                if( checkThis.GetType() == type )
                {
                    return (TransformType)Enum.Parse( typeof( TransformType ), type.Name );
                }
            }

            return TransformType.Transform;

        } //END GetType

        //--------------------------------------------//
        private void SetComponentToValue<T>( T Component, TransformType type ) where T : Transform
        //--------------------------------------------//
        {

            if     ( type == TransformType.Transform )      { this.transformThis = Component as Transform; }
            else if( type == TransformType.RectTransform )  { this.rectTransform = Component as RectTransform; }

        } //END SetComponentToValue

        //-------------------------------------------//
        public Vector3 GetDefaultStartValue<T>( T component ) where T : Transform
        //-------------------------------------------//
        {

            TransformType type = GetType( component );

            if( type == TransformType.Transform ) { return ( component as Transform ).localScale; }
            else if( type == TransformType.RectTransform ) { return ( component as RectTransform ).localScale; }

            return Vector3.zero;

        } //END GetDefaultStartValue

        //--------------------------------------------//
        private void _Play<T>( T component, Vector3 startScale, Vector3 endScale, AnimationCurve easeCurve, float length, float delay, bool loop, UnityEvent onCompleteOrLoop ) where T : Transform
        //--------------------------------------------//
        {

            //Setup the object and its values for the tween
            if( component != null && IsValidComponent( component ) )
            {
                transformType = GetType( component );
                SetComponentToValue( component, transformType );

                this.startScale = startScale;
                this.endScale = endScale;

                Setup( easeCurve, length, delay, loop, onCompleteOrLoop );

                //Now that the values are setup, call the Play() from the base class to begin the tween
                base.Play();
            }
            else
            {
                if( showDebug ) { Debug.Log( "bxrTweenScale.cs Play() Unable to Play Tween, transform variable is null" ); }
            }

        } //END Play
        

        //----------------------------//
        protected override void SetNewValues( float timer )
        //----------------------------//
        {
            //Perform tween logic every coroutine update
            if( transformType == TransformType.Transform )
            {
                transformThis.localScale = Vector3.Lerp( startScale, endScale, easeCurve.Evaluate( timer / length ) );
            }
            else if( transformType == TransformType.RectTransform )
            {
                rectTransform.localScale = Vector3.Lerp( startScale, endScale, easeCurve.Evaluate( timer / length ) );
            }

        } //END SetNewValue

        //-----------------------//
        protected override void PrepareForLoop()
        //-----------------------//
        {
            
            //Flip the start and end values to prepare to loop back to the start
            Vector3 temp = endScale;

            endScale = startScale;
            startScale = temp;
            
        } //END PrepareValuesForLoop


        //--------------------------------------------//
        public override Component GetLinkedComponent()
        //--------------------------------------------//
        {

            if( transformType == TransformType.Transform && transformThis != null ) { return transformThis as Component; }
            else if( transformType == TransformType.RectTransform && rectTransform != null ) { return rectTransform as Component; }

            return null;

        } //END GetLinkedComponent


    } //END Class

} //END Namespace