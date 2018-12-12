using Sirenix.OdinInspector;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

namespace BrandXR
{
    public class bxrTweenRotation: bxrTween
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
        [Tooltip( "Standard GameObject's Transform, all three values (X, Y, Z) in the rotation will be tweened" )]
        public Transform transformThis;

        [ShowIf( "transformType", TransformType.RectTransform )]
        [Tooltip( "If using rectTransform from a canvas object, all three values (X, Y, Z) in the rotation will be tweened" )]
        public RectTransform rectTransform;

        public enum StartRotationType
        {
            UseCurrentRotation,
            SetRotation
        }
        [Tooltip( "Where should this tween start from?" )]
        public StartRotationType startRotationType = StartRotationType.UseCurrentRotation;

        [ShowIf( "startRotationType", StartRotationType.SetRotation )]
        public Vector3 startRotation = Vector3.zero;
        public Vector3 endRotation = Vector3.zero;
        private Quaternion startRotationQ = Quaternion.identity;
        private Quaternion endRotationQ = Quaternion.identity;

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

            return TweenType.Rotation;

        } //END GetTweenType

        //----------------------------//
        public override void Play()
        //----------------------------//
        {

            //If Play() is called from this inherited class, we need to make sure our values are set up before attempting to tween
            if( transformType == TransformType.Transform )
            {
                Play( transformThis, GetStartRotation(), endRotation, easeCurve, length, delay, loop, onCompleteOrLoop );
            }
            else if( transformType == TransformType.RectTransform )
            {
                Play( rectTransform, GetStartRotation(), endRotation, easeCurve, length, delay, loop, onCompleteOrLoop );
            }

        } //END Play

        //--------------------------------------------//
        private Vector3 GetStartRotation()
        //--------------------------------------------//
        {

            //Used by the overriden Play() function of this class
            //Depending on user settings, we may want to get the "start" value from the current object, or use the passed in values
            if( startRotationType == StartRotationType.UseCurrentRotation )
            {
                if( transformType == TransformType.Transform )
                {
                    return transformThis.localEulerAngles;
                }
                else if( transformType == TransformType.RectTransform )
                {
                    return rectTransform.localEulerAngles;
                }
            }

            //else, return set value
            return this.startRotation;

        } //END GetStartRotation

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

            if( type == TransformType.Transform ) { this.transformThis = Component as Transform; }
            else if( type == TransformType.RectTransform ) { this.rectTransform = Component as RectTransform; }

        } //END SetComponentToValue

        //-------------------------------------------//
        public Vector3 GetDefaultStartValue<T>( T component ) where T : Transform
        //-------------------------------------------//
        {

            TransformType type = GetType( component );

            if( type == TransformType.Transform ) { return ( component as Transform ).localEulerAngles; }
            else if( type == TransformType.RectTransform ) { return ( component as RectTransform ).localEulerAngles; }

            return Vector3.zero;

        } //END GetDefaultStartValue

        //--------------------------------------------//
        public void Play<T>( T changeThis, Vector3 startRotation, Vector3 endRotation, AnimationCurve easeCurve, float length, float delay, bool loop, UnityEvent onCompleteOrLoop ) where T : Transform
        //--------------------------------------------//
        {

            //Setup the object and its values for the tween
            if( changeThis != null && IsValidComponent( changeThis ) )
            {
                transformType = GetType( changeThis );
                SetComponentToValue( changeThis, transformType );
                
                this.startRotation = startRotation;
                this.endRotation = endRotation;

                this.startRotationQ = Quaternion.Euler( this.startRotation );
                this.endRotationQ = Quaternion.Euler( this.endRotation );

                Setup( easeCurve, length, delay, loop, onCompleteOrLoop );

                //Now that the values are setup, call the Play() from the base class to begin the tween
                base.Play();
            }
            else
            {
                if( showDebug ) { Debug.Log( "bxrTweenRotation.cs Play() Unable to Play Tween, component variable is null" ); }
            }

        } //END Play
        

        //----------------------------//
        protected override void SetNewValues( float timer )
        //----------------------------//
        {
            //Perform tween logic every coroutine update
            if( transformType == TransformType.Transform )
            {
                transformThis.rotation = Quaternion.Slerp( startRotationQ, endRotationQ, easeCurve.Evaluate( timer / length ) );
            }
            else if( transformType == TransformType.RectTransform )
            {
                rectTransform.rotation = Quaternion.Slerp( startRotationQ, endRotationQ, easeCurve.Evaluate( timer / length ) );
            }

        } //END SetNewValue

        //-----------------------//
        protected override void PrepareForLoop()
        //-----------------------//
        {

            //Flip the start and end values to prepare to loop back to the start
            Vector3 temp = endRotation;

            endRotation = startRotation;
            startRotation = temp;

            this.startRotationQ = Quaternion.Euler( this.startRotation );
            this.endRotationQ = Quaternion.Euler( this.endRotation );

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