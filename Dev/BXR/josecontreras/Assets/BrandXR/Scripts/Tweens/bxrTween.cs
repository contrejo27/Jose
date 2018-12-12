using Sirenix.OdinInspector;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

namespace BrandXR
{
    public class bxrTween : MonoBehaviour
    {
        [Tooltip("Useful messages when tracking down bugs")]
        public bool showDebug = true;

        [Tooltip("Should we automatically tween when Start() is called?")]
        public bool playOnStart = false;

        public enum TweenType
        {
            Position,
            Rotation,
            Scale,
            Color,
            Int,
            Float
        }
        
        [Tooltip("How should this tween feel?")]
        public AnimationCurve easeCurve = AnimationCurve.EaseInOut( 0f, 0f, 1f, 1f );
        
        [Tooltip("How long tween will take, in seconds")]
        public float length = 2f;
        [Tooltip("Delay between tween starts. If looping delay is also re-applied")]
        public float delay = 0f;
        [Tooltip("Should this tween return to the original value after reaching its destination?")]
        public bool loop = false;

        protected bool isPaused = false;
        protected bool destroyWhenComplete = false;

        [ HideInInspector]
        public float startTime;
        [HideInInspector]
        public float endTime;

        //Reference to the coroutine running the tween logic
        [HideInInspector]
        public Coroutine coroutine;

        //Methods to call when tween completes or loops
        public UnityEvent onCompleteOrLoop = new UnityEvent();



        //--------------------------------------------//
        public virtual TweenType GetTweenType()
        //--------------------------------------------//
        {

            return TweenType.Position;

        } //END GetTweenType

        //--------------------------------------------//
        public virtual void Start()
        //--------------------------------------------//
        {

            AddToTweenManager();

            if( playOnStart )
            {
                Play();
            }

        } //END Start

        //--------------------------------------------//
        private void AddToTweenManager()
        //--------------------------------------------//
        {

            //Check if this tween needs to be added to the TweenManager 
            //This can happen when a tween is created outside of the TweenManager system by
            //dropping a bxrTween component onto gameObject or by using a BlockEvent
            TweenManager.AddTween( this );

        } //END AddToTweenManager

        //--------------------------------------------//
        public virtual void Setup
            ( AnimationCurve animationCurve, float length, float delay, bool loop, UnityEvent onCompleteOrLoop )
        //--------------------------------------------//
        {
            
            this.easeCurve = animationCurve;

            this.length = length;
            this.delay = delay;
            this.loop = loop;

            this.onCompleteOrLoop = onCompleteOrLoop;
            
        } //END Setup


        //--------------------------------------------//
        public virtual void Play()
        //--------------------------------------------//
        {
            
            Stop();
            ResetValues();
            SetStartAndEndTimes();

            //Debug.Log("bxrTween.cs Play() Timer.In( " + delay + ", BeginUpdatingTween, " + gameObject.name + " )");
            Timer.instance.In( delay, BeginUpdatingTween, gameObject );
            
        } //END Play

        //--------------------------------------------//
        private void BeginUpdatingTween()
        //--------------------------------------------//
        {

            if( this != null )
            {
                //Debug.Log("bxrTween.cs BeginUpdatingTween() calling StartCoroutine()");
                coroutine = StartCoroutine( UpdateTween() );
            }
            else
            {
                //Debug.Log("bxrTween.cs BeginUpdatingTween() unable to call StartCoroutine, 'this' == null");
            }

        } //END BeginUpdatingTween
        
        //--------------------------------------------//
        private IEnumerator UpdateTween()
        //--------------------------------------------//
        {
            float timer = 0f;

            while( timer <= length )
            {
                //We're inside of the while loop
                //If the tween has been paused, skip updating the objects values
                if( isPaused )
                {
                    yield return null;
                }
                else
                {
                    SetNewValues( timer );

                    timer += Time.deltaTime;

                    yield return null;
                }

            }

            if( onCompleteOrLoop != null ) { onCompleteOrLoop.Invoke(); }

            if( loop )
            {
                PrepareForLoop();
                SetStartAndEndTimes();
                Play();
            }
            else if( destroyWhenComplete )
            {
                PrepareForDestroy();
                Destroy( this.gameObject );
            }

        } //END UpdateTween

        //--------------------------------------------//
        protected virtual void SetNewValues( float timer )
        //--------------------------------------------//
        {
            return;

        } //END SetNewValues

        //--------------------------------------------//
        public void Pause()
        //--------------------------------------------//
        {

            if( coroutine != null )
            {
                isPaused = true;
            }

        } //END Pause

        //--------------------------------------------//
        public void Resume()
        //--------------------------------------------//
        {

            isPaused = false;

        } //END Pause

        //--------------------------------------------//
        public void Stop()
        //--------------------------------------------//
        {

            if( coroutine != null )
            {
                StopCoroutine( coroutine );
            }

        } //END Stop

        //--------------------------------------------//
        public void ResetValues()
        //--------------------------------------------//
        {

            startTime = 0f;
            endTime = 0f;

            isPaused = false;

            coroutine = null;

        } //END ResetValues

        //-----------------------//
        protected virtual void PrepareForLoop()
        //-----------------------//
        {
            
        } //END PrepareValuesForLoop

        //--------------------------------------------//
        protected void SetStartAndEndTimes()
        //--------------------------------------------//
        {

            startTime = Time.time;
            endTime = startTime + length;

        } //END SetStartAndEndTimes

        
        //--------------------------------------------//
        public void PreventDestroyOnComplete()
        //--------------------------------------------//
        {

            destroyWhenComplete = false;

        } //END PreventDestroyOnComplete

        //--------------------------------------------//
        public void SetToDestroyWhenComplete()
        //--------------------------------------------//
        {

            destroyWhenComplete = true;

        } //END SetToDestroyWhenComplete

        //--------------------------------------------//
        public void PrepareForDestroy()
        //--------------------------------------------//
        {

            if( coroutine != null )
            {
                StopCoroutine( coroutine );
            }

            TweenManager.PrepareTweenForDestroy( this );

        } //END PrepareForDestroy

        //--------------------------------------------//
        public void Kill()
        //--------------------------------------------//
        {
            //Debug.Log( "Kill() gameObject.name = " + gameObject.name );
            PrepareForDestroy();

            //If there's a gameObject just for this tween attached to the TweenManager, destroy the gameObject
            if( this.gameObject.transform.parent == TweenManager.instance.transform && 
                this.gameObject.name.Contains( "bxr_Tween" ) )
            {
                Destroy( this.gameObject );
            }

            //Just remove the component
            else
            {
                Destroy( this );
            }
            
        } //END Kill

        //--------------------------------------------//
        /// <summary>
        /// Is this tween running, but paused?
        /// </summary>
        /// <returns></returns>
        public bool IsPaused()
        //--------------------------------------------//
        {

            if( coroutine != null && isPaused )
            {
                return true;
            }

            return false;

        } //END IsPaused

        //--------------------------------------------//
        /// <summary>
        /// Is this tween running and modifying the passed in component, but paused?
        /// </summary>
        /// <returns></returns>
        public bool IsPaused<T>( T checkThis )
        //--------------------------------------------//
        {

            if( checkThis is Component )
            {
                Component component = GetLinkedComponent();

                if( component != null && checkThis != null )
                {
                    if( component == checkThis as Component )
                    {
                        return IsPaused();
                    }
                }
            }
            else if( checkThis is UnityEngine.Object )
            {
                UnityEngine.Object obj = GetLinkedObject();

                if( obj != null && checkThis != null )
                {
                    if( checkThis as UnityEngine.Object )
                    {
                        return IsPaused();
                    }
                }
            }
            
            return false;

        } //END IsPaused

        //--------------------------------------------//
        /// <summary>
        /// Is this tween linked to the passed in component
        /// </summary>
        /// <typeparam name="T">Component, Object</typeparam>
        /// <param name="checkThis">Component or Object you would like to check to see if tween is linked to</param>
        /// <returns></returns>
        public bool IsTweenLinked<T>( T checkThis )
        //--------------------------------------------//
        {
            if( checkThis is Component )
            {
                Component component = GetLinkedComponent();

                if( component != null && checkThis != null )
                {
                    return component == ( checkThis as Component );
                }
            }
            else if( checkThis is UnityEngine.Object )
            {
                UnityEngine.Object obj = GetLinkedObject();

                if( obj != null && checkThis != null )
                {
                    return obj == ( checkThis as UnityEngine.Object );
                }
            }
            
            return false;

        } //END IsTweenLinked

        //--------------------------------------------//
        /// <summary>
        /// Is this tween linked to the passed in component
        /// </summary>
        /// <typeparam name="T">Component, Object</typeparam>
        /// <param name="checkThis">Component or Object you would like to check to see if tween is linked to</param>
        /// <param name="fieldName">The variable name you would like to check for</param>
        /// <returns></returns>
        public bool IsTweenLinked<T>( T checkThis, string fieldName )
        //--------------------------------------------//
        {
            if( checkThis is UnityEngine.Object )
            {
                UnityEngine.Object obj = GetLinkedObject( fieldName );

                if( obj != null && checkThis != null )
                {
                    return obj == ( checkThis as UnityEngine.Object );
                }
            }

            return false;

        } //END IsTweenLinked

        //------------------------------------//
        public virtual Component GetLinkedComponent()
        //------------------------------------//
        {
            return null;

        } //END GetLinkedComponent

        //------------------------------------//
        public virtual UnityEngine.Component GetLinkedComponent( string fieldName )
        //------------------------------------//
        {
            return null;

        } //END GetLinkedComponent

        //------------------------------------//
        public virtual UnityEngine.Object GetLinkedObject()
        //------------------------------------//
        {
            return null;

        } //END GetLinkedObject

        //------------------------------------//
        public virtual UnityEngine.Object GetLinkedObject( string fieldName )
        //------------------------------------//
        {
            return null;

        } //END GetLinkedObject


        //-----------------------//
        public void TestMessage( string message )
        //-----------------------//
        {

            Debug.Log( message );

        } //END TestMessage

        
    } //END Class

} //END Namespace