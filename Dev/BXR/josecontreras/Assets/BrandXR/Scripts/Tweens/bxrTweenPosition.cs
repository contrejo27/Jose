using Sirenix.OdinInspector;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

namespace BrandXR
{
    public class bxrTweenPosition: bxrTween
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
        [Tooltip( "Standard GameObject's Transform, all three values (X, Y, Z) in the localPosition will be tweened" )]
        public Transform transformThis;
        
        [ShowIf( "transformType", TransformType.RectTransform )]
        [Tooltip( "If using rectTransform from a canvas object, only the (X, Y) variables will be tweened to in the 'End Position' variable" )]
        public RectTransform rectTransform;

        public enum LocalOrWorldSpace
        {
            Local,
            Global
        }
        [Tooltip("Should we tween the position in local or global world space?")]
        public LocalOrWorldSpace useLocalOrGlobalSpace = LocalOrWorldSpace.Local;

        public enum StartPositionType
        {
            UseCurrentPosition,
            SetPosition
        }
        [Tooltip("Where should this tween start from?")]
        public StartPositionType startPositionType = StartPositionType.UseCurrentPosition;

        [ShowIf( "startPositionType", StartPositionType.SetPosition )]
        public Vector3 startPosition = Vector3.zero;
        [Tooltip( "If using rectTransform from a canvas object, only the X & Y variables will be tweened to in the 'End Position' variable" )]
        public Vector3 endPosition = Vector3.zero;

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
            if( transformType == TransformType.Transform )
            {
                Play( transformThis, useLocalOrGlobalSpace, GetStartPosition(), endPosition, easeCurve, length, delay, loop, onCompleteOrLoop );
            }
            else if( transformType == TransformType.RectTransform )
            {
                Play( rectTransform, useLocalOrGlobalSpace, GetStartPosition(), endPosition, easeCurve, length, delay, loop, onCompleteOrLoop );
            }

        } //END Play

        //--------------------------------------------//
        private Vector3 GetStartPosition()
        //--------------------------------------------//
        {

            //Used by the overriden Play() function of this class
            //Depending on user settings, we may want to get the "start" value from the current object, or use the passed in values
            if( startPositionType == StartPositionType.UseCurrentPosition )
            {
                if( transformType == TransformType.Transform )
                {
                    if( useLocalOrGlobalSpace == LocalOrWorldSpace.Local )
                    {
                        return transformThis.localPosition;
                    }
                    else if( useLocalOrGlobalSpace == LocalOrWorldSpace.Global )
                    {
                        return transformThis.position;
                    }
                }
                else if( transformType == TransformType.RectTransform )
                {
                    if( useLocalOrGlobalSpace == LocalOrWorldSpace.Local )
                    {
                        return rectTransform.localPosition;
                    }
                    else if( useLocalOrGlobalSpace == LocalOrWorldSpace.Global )
                    {
                        return rectTransform.position;
                    }
                }
            }

            //else, return set value
            return this.startPosition;

        } //END GetStartPosition


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
        public TransformType GetType<T>( T checkThis )  where T : Transform
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

            if( type == TransformType.Transform ) { return ( component as Transform ).localPosition; }
            else if( type == TransformType.RectTransform ) { return ( component as RectTransform ).localPosition; }

            return Vector3.zero;

        } //END GetDefaultStartValue

        //--------------------------------------------//
        public void Play<T>( T changeThis, LocalOrWorldSpace useLocalOrWorldSpace, Vector3 startPosition, Vector3 endPosition, AnimationCurve easeCurve, float length, float delay, bool loop, UnityEvent onCompleteOrLoop ) where T : Transform
        //--------------------------------------------//
        {

            //Setup the object and its values for the tween
            if( changeThis != null && IsValidComponent( changeThis ) )
            {
                transformType = GetType( changeThis );
                SetComponentToValue( changeThis, transformType );

                this.useLocalOrGlobalSpace = useLocalOrWorldSpace;
                this.startPosition = startPosition;
                this.endPosition = endPosition;

                Setup( easeCurve, length, delay, loop, onCompleteOrLoop );

                //Now that the values are setup, call the Play() from the base class to begin the tween
                base.Play();
            }
            else
            {
                if( showDebug ) { Debug.Log( "bxrTweenPosition.cs Play() Unable to Play Tween, component variable is null or of the incorrect type" ); }
            }
            
        } //END Play
        

        //----------------------------//
        protected override void SetNewValues( float timer )
        //----------------------------//
        {
            //Perform tween logic every coroutine update
            if( transformType == TransformType.Transform )
            {
                if( useLocalOrGlobalSpace == LocalOrWorldSpace.Local )
                {
                    transformThis.localPosition = Vector3.Lerp( startPosition, endPosition, easeCurve.Evaluate( timer / length ) );
                }
                else if( useLocalOrGlobalSpace == LocalOrWorldSpace.Global )
                {
                    transformThis.position = Vector3.Lerp( startPosition, endPosition, easeCurve.Evaluate( timer / length ) );
                }
            }
            else if( transformType == TransformType.RectTransform )
            {
                if( useLocalOrGlobalSpace == LocalOrWorldSpace.Local )
                {
                    rectTransform.localPosition = Vector2.Lerp( startPosition, endPosition, easeCurve.Evaluate( timer / length ) );
                }
                else if( useLocalOrGlobalSpace == LocalOrWorldSpace.Global )
                {
                    rectTransform.position = Vector2.Lerp( startPosition, endPosition, easeCurve.Evaluate( timer / length ) );
                }
            }

        } //END SetNewValue

        //-----------------------//
        protected override void PrepareForLoop()
        //-----------------------//
        {
            
            //Flip the start and end values to prepare to loop back to the start
            Vector3 temp = endPosition;

            endPosition = startPosition;
            startPosition = temp;
            
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