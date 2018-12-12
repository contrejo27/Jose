using System;
using System.Collections;
using UnityEngine;
using Sirenix.OdinInspector;
using UnityEngine.Events;
using System.Collections.Generic;
using UnityEngine.UI;
using System.Linq;

namespace BrandXR
{
    //Handles the creation and commands sent to individual tweens
    public class TweenManager: MonoBehaviour
    {

        public static bool showDebug = false;
        
        private static List<bxrTween> tweens = new List<bxrTween>();

        public enum TweenCommands
        {
            Pause,
            Resume,
            Stop,
            Play
        }


        //Singleton behavior
        private static TweenManager _instance;

        //--------------------------------------------//
        public static TweenManager instance
        //--------------------------------------------//
        {
            get
            {
                if( _instance == null )
                {
                    if( GameObject.FindObjectOfType<TweenManager>() == null ) { PrefabManager.InstantiatePrefab( PrefabFactory.Prefabs.bxr_TweenManager ); }
                    _instance = GameObject.FindObjectOfType<TweenManager>();
                    BlockHelper.AddToBrandXRTechParent( _instance.transform );
                }

                _instance.CreateEaseCurve();

                return _instance;
            }

        } //END Instance

        //------------------------------------------------------//
        private void CreateEaseCurve()
        //------------------------------------------------------//
        {

            //Make sure EaseCurve exists
            if( EaseCurve.instance == null )
            {
                PrefabManager.InstantiatePrefab( PrefabFactory.Prefabs.bxr_EaseCurve );
            }

        } //END CreateEaseCurve

        //------------------------------------------------------//
        public void Awake()
        //------------------------------------------------------//
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
        /// <summary>
        /// Move a Transform across local or global space
        /// </summary>
        /// <typeparam name="T">Transform</typeparam>
        /// <param name="tweenThis">Accepts [Transform, RectTransform]</param>
        /// <param name="endValue">The position you would like to move this Transform to</param>
        /// <param name="length">How long tween should take</param>
        /// <param name="easeCurve">Pass in your own AnimationCurve or use the defaults provided by the EaseCurve class( EX: EaseCurve.Linear )</param>
        /// <param name="startValue">[OPTIONAL] Value to start from. If not provided we will use the existing value from the 'tweenThis' object</param>
        /// <param name="useLocalOrWorldSpace">[OPTIONAL] Would you like this transform to move in local or global space?</param>
        /// <param name="delay">[OPTIONAL] How long we should wait before starting to tween</param>
        /// <param name="loop">[OPTIONAL] Keep the tween running via a loop indefinitely</param>
        /// <param name="onCompleteOrLoop">[OPTIONAL] A UnityEvent to call once the tween has finished or completed a single loop</param>
        /// <returns></returns>
        public static bxrTween Move<T>( T tweenThis, Vector3 endValue, float length, AnimationCurve easeCurve, Vector3? startValue = null, bxrTweenPosition.LocalOrWorldSpace useLocalOrWorldSpace = bxrTweenPosition.LocalOrWorldSpace.Local, float delay = 0f, bool loop = false, UnityEvent onCompleteOrLoop = null ) where T : Transform
        //---------------------------------------------//
        {
            //Make sure easing system exists
            instance.CreateEaseCurve();

            bxrTween tween = GetOrCreateTween( bxrTween.TweenType.Position, tweenThis );

            //Make sure tween is part of list
            if( !tweens.Contains( tween ) ) { tweens.Add( tween ); }

            //If startValue is null, ask the tween what the startValue should be for the component we're tweening
            if( !startValue.HasValue ) { startValue = ( tween as bxrTweenPosition ).GetDefaultStartValue( tweenThis ); }

            tween.SetToDestroyWhenComplete();
            ( tween as bxrTweenPosition ).Play( tweenThis, useLocalOrWorldSpace, startValue.Value, endValue, easeCurve, length, delay, loop, onCompleteOrLoop );

            return tween;

        } //END Move 

        //---------------------------------------------//
        /// <summary>
        /// Move a Transform across local or global space
        /// </summary>
        /// <typeparam name="T">Transform</typeparam>
        /// <param name="tweenThis">Accepts [Transform, RectTransform]</param>
        /// <param name="endValue">The position you would like to move this Transform to</param>
        /// <param name="length">How long tween should take</param>
        /// <param name="easeType">The easing you would like this tween to use</param>
        /// <param name="startValue">[OPTIONAL] Value to start from. If not provided we will use the existing value from the 'tweenThis' object</param>
        /// <param name="useLocalOrWorldSpace">[OPTIONAL] Would you like this transform to move in local or global space?</param>
        /// <param name="delay">[OPTIONAL] How long we should wait before starting to tween</param>
        /// <param name="loop">[OPTIONAL] Keep the tween running via a loop indefinitely</param>
        /// <param name="onCompleteOrLoop">[OPTIONAL] A UnityEvent to call once the tween has finished or completed a single loop</param>
        /// <returns></returns>
        public static bxrTween Move<T>( T tweenThis, Vector3 endValue, float length, EaseCurve.EaseType easeType, Vector3? startValue = null, bxrTweenPosition.LocalOrWorldSpace useLocalOrWorldSpace = bxrTweenPosition.LocalOrWorldSpace.Local, float delay = 0f, bool loop = false, UnityEvent onCompleteOrLoop = null ) where T : Transform
        //---------------------------------------------//
        {
            return Move( tweenThis, endValue, length, EaseCurve.GetEaseCurve( easeType ), startValue, useLocalOrWorldSpace, delay, loop, onCompleteOrLoop );

        } //END Move 

        //---------------------------------------------//
        /// <summary>
        /// Rotates a Transform or RectTransform using Quaternions. Supply this function with the localEulerAngles for the 'endValue' and 'startValue'
        /// </summary>
        /// <typeparam name="T">Transform</typeparam>
        /// <param name="tweenThis">Accepts [Transform, RectTransform]</param>
        /// <param name="endValue">What should the localEulerAngles for the transform be by the end of this rotation?</param>
        /// <param name="length">How long tween should take</param>
        /// <param name="easeCurve">Pass in your own AnimationCurve or use the defaults provided by the EaseCurve class( EX: EaseCurve.Linear )</param>
        /// <param name="startValue">[OPTIONAL] Value to start from. If not provided we will use the existing value from the 'tweenThis' object</param>
        /// <param name="delay">[OPTIONAL] How long we should wait before starting to tween</param>
        /// <param name="loop">[OPTIONAL] Keep the tween running via a loop indefinitely</param>
        /// <param name="onCompleteOrLoop">[OPTIONAL] A UnityEvent to call once the tween has finished or completed a single loop</param>
        /// <returns></returns>
        public static bxrTween Rotate<T>( T tweenThis, Vector3 endValue, float length, AnimationCurve easeCurve, Vector3? startValue = null, float delay = 0f, bool loop = false, UnityEvent onCompleteOrLoop = null ) where T : Transform
        //---------------------------------------------//
        {
            //Make sure easing system exists
            instance.CreateEaseCurve();

            bxrTween tween = GetOrCreateTween( bxrTween.TweenType.Rotation, tweenThis );

            //Make sure tween is part of list
            if( !tweens.Contains( tween ) ) { tweens.Add( tween ); }

            //If startValue is null, ask the tween what the startValue should be for the component we're tweening
            if( !startValue.HasValue ) { startValue = ( tween as bxrTweenRotation ).GetDefaultStartValue( tweenThis ); }

            tween.SetToDestroyWhenComplete();
            ( tween as bxrTweenRotation ).Play( tweenThis, startValue.Value, endValue, easeCurve, length, delay, loop, onCompleteOrLoop );

            return tween;

        } //END Rotate

        //---------------------------------------------//
        /// <summary>
        /// Rotates a Transform or RectTransform using Quaternions. Supply this function with the localEulerAngles for the 'endValue' and 'startValue'
        /// </summary>
        /// <typeparam name="T">Transform</typeparam>
        /// <param name="tweenThis">Accepts [Transform, RectTransform]</param>
        /// <param name="endValue">What should the localEulerAngles for the transform be by the end of this rotation?</param>
        /// <param name="length">How long tween should take</param>
        /// <param name="easeType">The easing you would like the tween to use</param>
        /// <param name="startValue">[OPTIONAL] Value to start from. If not provided we will use the existing value from the 'tweenThis' object</param>
        /// <param name="delay">[OPTIONAL] How long we should wait before starting to tween</param>
        /// <param name="loop">[OPTIONAL] Keep the tween running via a loop indefinitely</param>
        /// <param name="onCompleteOrLoop">[OPTIONAL] A UnityEvent to call once the tween has finished or completed a single loop</param>
        /// <returns></returns>
        public static bxrTween Rotate<T>( T tweenThis, Vector3 endValue, float length, EaseCurve.EaseType easeType, Vector3? startValue = null, float delay = 0f, bool loop = false, UnityEvent onCompleteOrLoop = null ) where T : Transform
        //---------------------------------------------//
        {

            return Rotate( tweenThis, endValue, length, EaseCurve.GetEaseCurve( easeType ), startValue, delay, loop, onCompleteOrLoop );

        } //END Rotate

        //---------------------------------------------//
        /// <summary>
        /// Scale a transform in local space. For RectTransform this will change the 'sizeDelta' value
        /// </summary>
        /// <typeparam name="T">Transform</typeparam>
        /// <param name="tweenThis">Accepts [Transform, RectTransform]</param>
        /// <param name="endValue">What should the final scale be? For RectTransform we are scaling the 'sizeDelta' value so only the (X, Y) values will be used</param>
        /// <param name="length">How long tween should take</param>
        /// <param name="easeCurve">Pass in your own AnimationCurve or use the defaults provided by the EaseCurve class( EX: EaseCurve.Linear )</param>
        /// <param name="startValue">[OPTIONAL] Value to start from. If not provided we will use the existing value from the 'tweenThis' object</param>
        /// <param name="delay">[OPTIONAL] How long we should wait before starting to tween</param>
        /// <param name="loop">[OPTIONAL] Keep the tween running via a loop indefinitely</param>
        /// <param name="onCompleteOrLoop">[OPTIONAL] A UnityEvent to call once the tween has finished or completed a single loop</param>
        /// <returns></returns>
        public static bxrTween Scale<T>( T tweenThis, Vector3 endValue, float length, AnimationCurve easeCurve, Vector3? startValue = null, float delay = 0f, bool loop = false, UnityEvent onCompleteOrLoop = null ) where T : Transform
        //---------------------------------------------//
        {
            //Make sure easing system exists
            instance.CreateEaseCurve();

            bxrTween tween = GetOrCreateTween( bxrTween.TweenType.Scale, tweenThis );

            //Make sure tween is part of list
            if( !tweens.Contains( tween ) ) { tweens.Add( tween ); }

            //If startValue is null, ask the tween what the startValue should be for the component we're tweening
            if( !startValue.HasValue ) { startValue = ( tween as bxrTweenScale ).GetDefaultStartValue( tweenThis ); }

            tween.SetToDestroyWhenComplete();
            ( tween as bxrTweenScale ).Play( tweenThis, startValue.Value, endValue, easeCurve, length, delay, loop, onCompleteOrLoop );

            return tween;

        } //END Scale 

        //---------------------------------------------//
        /// <summary>
        /// Scale a transform in local space. For RectTransform this will change the 'sizeDelta' value
        /// </summary>
        /// <typeparam name="T">Transform</typeparam>
        /// <param name="tweenThis">Accepts [Transform, RectTransform]</param>
        /// <param name="endValue">What should the final scale be? For RectTransform we are scaling the 'sizeDelta' value so only the (X, Y) values will be used</param>
        /// <param name="length">How long tween should take</param>
        /// <param name="easeType">The easing you would like this tween to use</param>
        /// <param name="startValue">[OPTIONAL] Value to start from. If not provided we will use the existing value from the 'tweenThis' object</param>
        /// <param name="delay">[OPTIONAL] How long we should wait before starting to tween</param>
        /// <param name="loop">[OPTIONAL] Keep the tween running via a loop indefinitely</param>
        /// <param name="onCompleteOrLoop">[OPTIONAL] A UnityEvent to call once the tween has finished or completed a single loop</param>
        /// <returns></returns>
        public static bxrTween Scale<T>( T tweenThis, Vector3 endValue, float length, EaseCurve.EaseType easeType, Vector3? startValue = null, float delay = 0f, bool loop = false, UnityEvent onCompleteOrLoop = null ) where T : Transform
        //---------------------------------------------//
        {

            return Scale( tweenThis, endValue, length, EaseCurve.GetEaseCurve( easeType ), startValue, delay, loop, onCompleteOrLoop );

        } //END Scale 

        //---------------------------------------------//
        /// <summary>
        /// Scale a transform in local space. For RectTransform this will change the 'sizeDelta' value
        /// </summary>
        /// <typeparam name="T">Transform</typeparam>
        /// <param name="tweenThis">Accepts [Transform, RectTransform]</param>
        /// <param name="endValueMultiplier"> What multiplier should we apply onto the current scale?</param>
        /// <param name="length">How long tween should take</param>
        /// <param name="easeCurve">Pass in your own AnimationCurve or use the defaults provided by the EaseCurve class( EX: EaseCurve.Linear )</param>
        /// <param name="startValue">[OPTIONAL] Value to start from. If not provided we will use the existing value from the 'tweenThis' object</param>
        /// <param name="delay">[OPTIONAL] How long we should wait before starting to tween</param>
        /// <param name="loop">[OPTIONAL] Keep the tween running via a loop indefinitely</param>
        /// <param name="onCompleteOrLoop">[OPTIONAL] A UnityEvent to call once the tween has finished or completed a single loop</param>
        /// <returns></returns>
        public static bxrTween Scale<T>( T tweenThis, float endValueMultiplier, float length, AnimationCurve easeCurve, Vector3? startValue = null, float delay = 0f, bool loop = false, UnityEvent onCompleteOrLoop = null ) where T : Transform
        //---------------------------------------------//
        {
            //Make sure easing system exists
            instance.CreateEaseCurve();

            bxrTween tween = GetOrCreateTween( bxrTween.TweenType.Scale, tweenThis );

            //Make sure tween is part of list
            if( !tweens.Contains( tween ) ) { tweens.Add( tween ); }

            //If startValue is null, ask the tween what the startValue should be for the component we're tweening
            if( !startValue.HasValue ) { startValue = ( tween as bxrTweenScale ).GetDefaultStartValue( tweenThis ); }

            tween.SetToDestroyWhenComplete();
            ( tween as bxrTweenScale ).Play<T>( tweenThis, startValue.Value, endValueMultiplier, easeCurve, length, delay, loop, onCompleteOrLoop );

            return tween;

        } //END Scale 

        //---------------------------------------------//
        /// <summary>
        /// Scale a transform in local space. For RectTransform this will change the 'sizeDelta' value
        /// </summary>
        /// <typeparam name="T">Transform</typeparam>
        /// <param name="tweenThis">Accepts [Transform, RectTransform]</param>
        /// <param name="endValueMultiplier"> What multiplier should we apply onto the current scale?</param>
        /// <param name="length">How long tween should take</param>
        /// <param name="easeType">What easing this tween should use</param>
        /// <param name="startValue">[OPTIONAL] Value to start from. If not provided we will use the existing value from the 'tweenThis' object</param>
        /// <param name="delay">[OPTIONAL] How long we should wait before starting to tween</param>
        /// <param name="loop">[OPTIONAL] Keep the tween running via a loop indefinitely</param>
        /// <param name="onCompleteOrLoop">[OPTIONAL] A UnityEvent to call once the tween has finished or completed a single loop</param>
        /// <returns></returns>
        public static bxrTween Scale<T>( T tweenThis, float endValueMultiplier, float length, EaseCurve.EaseType easeType, Vector3? startValue = null, float delay = 0f, bool loop = false, UnityEvent onCompleteOrLoop = null ) where T : Transform
        //---------------------------------------------//
        {

            return Scale( tweenThis, endValueMultiplier, length, EaseCurve.GetEaseCurve( easeType ), startValue, delay, loop, onCompleteOrLoop );

        } //END Scale

        //---------------------------------------------//
        /// <summary>
        /// Change the color for the passed in Renderer or Material
        /// </summary>
        /// <typeparam name="T">Renderer/Object</typeparam>
        /// <param name="tweenThis">Accepts [Renderer, Image, RawImage, SpriteRenderer, Text, Material, CanvasGroup]</param>
        /// <param name="endValue">Color to tween to</param>
        /// <param name="length">How long tween should take</param>
        /// <param name="easeCurve">Pass in your own AnimationCurve or use the defaults provided by the EaseCurve class( EX: EaseCurve.Linear )</param>
        /// <param name="startValue">[OPTIONAL] Value to start from. If not provided we will use the existing value from the 'tweenThis' object</param>
        /// <param name="delay">[OPTIONAL] How long we should wait before starting to tween</param>
        /// <param name="loop">[OPTIONAL] Keep the tween running via a loop indefinitely</param>
        /// <param name="onCompleteOrLoop">[OPTIONAL] A UnityEvent to call once the tween has finished or completed a single loop</param>
        /// <returns></returns>
        public static bxrTween Color<T>( T tweenThis, Color endValue, float length, AnimationCurve easeCurve, Color? startValue = null, float delay = 0f, bool loop = false, UnityEvent onCompleteOrLoop = null )
        //---------------------------------------------//
        {
            //Make sure easing system exists
            instance.CreateEaseCurve();

            bxrTween tween = GetOrCreateTween( bxrTween.TweenType.Color, tweenThis );

            //Make sure tween is part of list
            if( !tweens.Contains( tween ) ) { tweens.Add( tween ); }

            //If startValue is null, ask the tween what the startValue should be for the component we're tweening
            if( !startValue.HasValue ) { startValue = ( tween as bxrTweenColor ).GetDefaultStartValue( tweenThis ); }

            tween.SetToDestroyWhenComplete();

            ( tween as bxrTweenColor ).Play<T>( tweenThis, startValue.Value, endValue, easeCurve, length, delay, loop, onCompleteOrLoop );

            return tween;

        } //END Color

        //---------------------------------------------//
        /// <summary>
        /// Change the color for the passed in Renderer or Material
        /// </summary>
        /// <typeparam name="T">Renderer/Object</typeparam>
        /// <param name="tweenThis">Accepts [Renderer, Image, RawImage, SpriteRenderer, Text, Material, CanvasGroup]</param>
        /// <param name="endValue">Color to tween to</param>
        /// <param name="length">How long tween should take</param>
        /// <param name="easeType">What easing this tween should use</param>
        /// <param name="startValue">[OPTIONAL] Value to start from. If not provided we will use the existing value from the 'tweenThis' object</param>
        /// <param name="delay">[OPTIONAL] How long we should wait before starting to tween</param>
        /// <param name="loop">[OPTIONAL] Keep the tween running via a loop indefinitely</param>
        /// <param name="onCompleteOrLoop">[OPTIONAL] A UnityEvent to call once the tween has finished or completed a single loop</param>
        /// <returns></returns>
        public static bxrTween Color<T>( T tweenThis, Color endValue, float length, EaseCurve.EaseType easeType, Color? startValue = null, float delay = 0f, bool loop = false, UnityEvent onCompleteOrLoop = null )
        //---------------------------------------------//
        {

            return Color( tweenThis, endValue, length, EaseCurve.GetEaseCurve( easeType ), startValue, delay, loop, onCompleteOrLoop );

        } //END Color

        //---------------------------------------------//
        /// <summary>
        /// Change the fill value for an Image object
        /// </summary>
        /// <param name="tweenThis">Accepts [Image]</param>
        /// <param name="endValue">What should the final fill value be?</param>
        /// <param name="length">How long tween should take</param>
        /// <param name="easeCurve">Pass in your own AnimationCurve or use the defaults provided by the EaseCurve class( EX: EaseCurve.Linear )</param>
        /// <param name="startValue">[OPTIONAL] Value to start from. If not provided we will use the existing value from the 'tweenThis' object</param>
        /// <param name="delay">[OPTIONAL] How long we should wait before starting to tween</param>
        /// <param name="loop">[OPTIONAL] Keep the tween running via a loop indefinitely</param>
        /// <param name="onCompleteOrLoop">[OPTIONAL] A UnityEvent to call once the tween has finished or completed a single loop</param>
        /// <returns></returns>
        public static bxrTween ImageFill( Image tweenThis, float endValue, float length, AnimationCurve easeCurve, float? startValue = null, float delay = 0f, bool loop = false, UnityEvent onCompleteOrLoop = null )
        //---------------------------------------------//
        {

            return TweenFloat( tweenThis, "fillAmount", endValue, length, easeCurve, startValue, delay, loop, onCompleteOrLoop );

        } //END ImageFill

        //---------------------------------------------//
        /// <summary>
        /// Change the fill value for an Image object
        /// </summary>
        /// <param name="tweenThis">Accepts [Image]</param>
        /// <param name="endValue">What should the final fill value be?</param>
        /// <param name="length">How long tween should take</param>
        /// <param name="easeType">What easing this tween should use</param>
        /// <param name="startValue">[OPTIONAL] Value to start from. If not provided we will use the existing value from the 'tweenThis' object</param>
        /// <param name="delay">[OPTIONAL] How long we should wait before starting to tween</param>
        /// <param name="loop">[OPTIONAL] Keep the tween running via a loop indefinitely</param>
        /// <param name="onCompleteOrLoop">[OPTIONAL] A UnityEvent to call once the tween has finished or completed a single loop</param>
        /// <returns></returns>
        public static bxrTween ImageFill( Image tweenThis, float endValue, float length, EaseCurve.EaseType easeType, float? startValue = null, float delay = 0f, bool loop = false, UnityEvent onCompleteOrLoop = null )
        //---------------------------------------------//
        {

            return TweenFloat( tweenThis, "fillAmount", endValue, length, easeType, startValue, delay, loop, onCompleteOrLoop );

        } //END ImageFill


        //---------------------------------------------//
        /// <summary>
        /// Change the pitch value for an AudioSource
        /// </summary>
        /// <param name="tweenThis">Accepts [AudioSource]</param>
        /// <param name="endValue">What should the final pitch value be?</param>
        /// <param name="length">How long tween should take</param>
        /// <param name="easeCurve">Pass in your own AnimationCurve or use the defaults provided by the EaseCurve class( EX: EaseCurve.Linear )</param>
        /// <param name="startValue">[OPTIONAL] Value to start from. If not provided we will use the existing value from the 'tweenThis' object</param>
        /// <param name="delay">[OPTIONAL] How long we should wait before starting to tween</param>
        /// <param name="loop">[OPTIONAL] Keep the tween running via a loop indefinitely</param>
        /// <param name="onCompleteOrLoop">[OPTIONAL] A UnityEvent to call once the tween has finished or completed a single loop</param>
        /// <returns></returns>
        public static bxrTween AudioPitch( AudioSource tweenThis, float endValue, float length, AnimationCurve easeCurve, float? startValue = null, float delay = 0f, bool loop = false, UnityEvent onCompleteOrLoop = null )
        //---------------------------------------------//
        {

            return TweenFloat( tweenThis, "pitch", endValue, length, easeCurve, startValue, delay, loop, onCompleteOrLoop );

        } //END AudioPitch

        //---------------------------------------------//
        /// <summary>
        /// Change the pitch value for an AudioSource
        /// </summary>
        /// <param name="tweenThis">Accepts [AudioSource]</param>
        /// <param name="endValue">What should the final pitch value be?</param>
        /// <param name="length">How long tween should take</param>
        /// <param name="easeType">What easing should this tween use</param>
        /// <param name="startValue">[OPTIONAL] Value to start from. If not provided we will use the existing value from the 'tweenThis' object</param>
        /// <param name="delay">[OPTIONAL] How long we should wait before starting to tween</param>
        /// <param name="loop">[OPTIONAL] Keep the tween running via a loop indefinitely</param>
        /// <param name="onCompleteOrLoop">[OPTIONAL] A UnityEvent to call once the tween has finished or completed a single loop</param>
        /// <returns></returns>
        public static bxrTween AudioPitch( AudioSource tweenThis, float endValue, float length, EaseCurve.EaseType easeType, float? startValue = null, float delay = 0f, bool loop = false, UnityEvent onCompleteOrLoop = null )
        //---------------------------------------------//
        {

            return TweenFloat( tweenThis, "pitch", endValue, length, easeType, startValue, delay, loop, onCompleteOrLoop );

        } //END AudioPitch

        //---------------------------------------------//
        /// <summary>
        /// Change any int value by passing in the UnityEngine Object and a fieldName
        /// </summary>
        /// <typeparam name="T">UnityEngine Object</typeparam>
        /// <param name="tweenThis">The UnityEngine Object that will have it's value tweened</param>
        /// <param name="fieldName">The variable ('field') we should tween</param>
        /// <param name="endValue">Float value to tween to</param>
        /// <param name="length">How long tween should take</param>
        /// <param name="easeCurve">Pass in your own AnimationCurve or use the defaults provided by the EaseCurve class( EX: EaseCurve.Linear )</param>
        /// <param name="startValue">[OPTIONAL] Value to start from. If not provided we will use the existing value from the 'tweenThis' object</param>
        /// <param name="delay">[OPTIONAL] How long we should wait before starting to tween</param>
        /// <param name="loop">[OPTIONAL] Keep the tween running via a loop indefinitely</param>
        /// <param name="onCompleteOrLoop">[OPTIONAL] A UnityEvent to call once the tween has finished or completed a single loop</param>
        /// <returns></returns>
        public static bxrTween TweenInt<T>( T tweenThis, string fieldName, int endValue, int length, AnimationCurve easeCurve, int? startValue = null, float delay = 0f, bool loop = false, UnityEvent onCompleteOrLoop = null ) where T : UnityEngine.Object
        //---------------------------------------------//
        {
            //Make sure easing system exists
            instance.CreateEaseCurve();

            bxrTween tween = GetOrCreateTween( bxrTween.TweenType.Int, tweenThis, fieldName );

            //Make sure tween is part of list
            if( !tweens.Contains( tween ) ) { tweens.Add( tween ); }

            //If startValue is null, ask the tween what the startValue should be for the component we're tweening
            if( !startValue.HasValue ) { startValue = ( tween as bxrTweenInt ).GetDefaultStartValue( tweenThis, fieldName ); }

            tween.SetToDestroyWhenComplete();
            ( tween as bxrTweenInt ).Play( tweenThis, fieldName, startValue.Value, endValue, easeCurve, length, delay, loop, onCompleteOrLoop );

            return tween;

        } //END TweenInt

        //---------------------------------------------//
        /// <summary>
        /// Change any int value by passing in the UnityEngine Object and a fieldName
        /// </summary>
        /// <typeparam name="T">UnityEngine Object</typeparam>
        /// <param name="tweenThis">The UnityEngine Object that will have it's value tweened</param>
        /// <param name="fieldName">The variable ('field') we should tween</param>
        /// <param name="endValue">Float value to tween to</param>
        /// <param name="length">How long tween should take</param>
        /// <param name="easeType">What easing should this tween use</param>
        /// <param name="startValue">[OPTIONAL] Value to start from. If not provided we will use the existing value from the 'tweenThis' object</param>
        /// <param name="delay">[OPTIONAL] How long we should wait before starting to tween</param>
        /// <param name="loop">[OPTIONAL] Keep the tween running via a loop indefinitely</param>
        /// <param name="onCompleteOrLoop">[OPTIONAL] A UnityEvent to call once the tween has finished or completed a single loop</param>
        /// <returns></returns>
        public static bxrTween TweenInt<T>( T tweenThis, string fieldName, int endValue, int length, EaseCurve.EaseType easeType, int? startValue = null, float delay = 0f, bool loop = false, UnityEvent onCompleteOrLoop = null ) where T : UnityEngine.Object
        //---------------------------------------------//
        {

            return TweenInt( tweenThis, fieldName, endValue, length, EaseCurve.GetEaseCurve( easeType ), startValue, delay, loop, onCompleteOrLoop );

        } //END TweenInt

        //---------------------------------------------//
        /// <summary>
        /// Change any float value by passing in the UnityEngine Object and a fieldName
        /// </summary>
        /// <typeparam name="T">UnityEngine Object</typeparam>
        /// <param name="tweenThis">The UnityEngine Object that will have it's value tweened</param>
        /// <param name="fieldName">The variable ('field') we should tween</param>
        /// <param name="endValue">Float value to tween to</param>
        /// <param name="length">How long tween should take</param>
        /// <param name="easeCurve">Pass in your own AnimationCurve or use the defaults provided by the EaseCurve class( EX: EaseCurve.Linear )</param>
        /// <param name="startValue">[OPTIONAL] Value to start from. If not provided we will use the existing value from the 'tweenThis' object</param>
        /// <param name="delay">[OPTIONAL] How long we should wait before starting to tween</param>
        /// <param name="loop">[OPTIONAL] Keep the tween running via a loop indefinitely</param>
        /// <param name="onCompleteOrLoop">[OPTIONAL] A UnityEvent to call once the tween has finished or completed a single loop</param>
        /// <returns></returns>
        public static bxrTween TweenFloat<T>( T tweenThis, string fieldName, float endValue, float length, AnimationCurve easeCurve, float? startValue = null, float delay = 0f, bool loop = false, UnityEvent onCompleteOrLoop = null ) where T : UnityEngine.Object
        //---------------------------------------------//
        {
            //Make sure easing system exists
            instance.CreateEaseCurve();

            bxrTween tween = GetOrCreateTween( bxrTween.TweenType.Float, tweenThis, fieldName );

            //Make sure tween is part of list
            if( !tweens.Contains( tween ) ) { tweens.Add( tween ); }

            //If startValue is null, ask the tween what the startValue should be for the component we're tweening
            if( !startValue.HasValue ) { startValue = ( tween as bxrTweenFloat ).GetDefaultStartValue( tweenThis, fieldName ); }

            tween.SetToDestroyWhenComplete();
            ( tween as bxrTweenFloat ).Play( tweenThis, fieldName, startValue.Value, endValue, easeCurve, length, delay, loop, onCompleteOrLoop );

            return tween;

        } //END TweenFloat

        //---------------------------------------------//
        /// <summary>
        /// Change any float value by passing in the UnityEngine Object and a fieldName
        /// </summary>
        /// <typeparam name="T">UnityEngine Object</typeparam>
        /// <param name="tweenThis">The UnityEngine Object that will have it's value tweened</param>
        /// <param name="fieldName">The variable ('field') we should tween</param>
        /// <param name="endValue">Float value to tween to</param>
        /// <param name="length">How long tween should take</param>
        /// <param name="easeType">What easing this tween should use</param>
        /// <param name="startValue">[OPTIONAL] Value to start from. If not provided we will use the existing value from the 'tweenThis' object</param>
        /// <param name="delay">[OPTIONAL] How long we should wait before starting to tween</param>
        /// <param name="loop">[OPTIONAL] Keep the tween running via a loop indefinitely</param>
        /// <param name="onCompleteOrLoop">[OPTIONAL] A UnityEvent to call once the tween has finished or completed a single loop</param>
        /// <returns></returns>
        public static bxrTween TweenFloat<T>( T tweenThis, string fieldName, float endValue, float length, EaseCurve.EaseType easeType, float? startValue = null, float delay = 0f, bool loop = false, UnityEvent onCompleteOrLoop = null ) where T : UnityEngine.Object
        //---------------------------------------------//
        {

            return TweenFloat( tweenThis, fieldName, endValue, length, EaseCurve.GetEaseCurve( easeType ), startValue, delay, loop, onCompleteOrLoop );

        } //END TweenFloat



        //--------------------------------------------//
        public static void AddTween( bxrTween tween )
        //--------------------------------------------//
        {

            if( tweens != null && tweens.Count > 0 )
            {
                if( !tweens.Contains( tween ) ) { tweens.Add( tween ); }
            }
            else
            {
                tweens = new List<bxrTween>() { tween };
            }

        } //END AddTween


        //--------------------------------------------//
        private static bxrTween GetOrCreateTween<T>( bxrTween.TweenType type, T tweenThis )
        //--------------------------------------------//
        {
            
            //If there is an existing tween for this object + field, return it
            if( IsTweenLinked( tweenThis ) )
            {
                return GetLinkedTween( tweenThis );
            }

            //No existing tweens exist for this object, make a new one and return it
            else if( Enum.IsDefined( typeof(PrefabFactory.Prefabs), "bxr_Tween" + type.ToString() ) )
            {
                if( showDebug ) { Debug.Log( "TweenManager.cs CreateTween() Found bxr_Tween" + type.ToString() + ", IsDefined = " + ( Enum.IsDefined( typeof( PrefabFactory.Prefabs ), "bxr_Tween" + type.ToString() ) ) ); }

                bxrTween tween = PrefabManager.InstantiatePrefab( (PrefabFactory.Prefabs)System.Enum.Parse( typeof( PrefabFactory.Prefabs ), "bxr_Tween" + type.ToString() ) ).GetComponent<bxrTween>();
                tween.gameObject.transform.SetParent( instance.transform );

                tween.gameObject.name = "bxr_Tween" + type.ToString();
                return tween;
            }

            else
            {
                if( showDebug ) { Debug.Log( "TweenManager.cs CreateTween() Couldn't Find bxr_Tween" + type.ToString() + ", IsDefined = " + ( Enum.IsDefined( typeof( PrefabFactory.Prefabs ), "bxr_Tween" + type.ToString() ) ) ); }
            }

            return null;

        } //END GetOrCreateTween

        //--------------------------------------------//
        private static bxrTween GetOrCreateTween<T>( bxrTween.TweenType type, T tweenThis, string fieldName )
        //--------------------------------------------//
        {

            //If there is an existing tween for this object + field, return it
            if( IsTweenLinked( tweenThis, fieldName ) )
            {
                return GetLinkedTween( tweenThis, fieldName );
            }

            //No existing tweens exist for this object, make a new one and return it
            else if( Enum.IsDefined( typeof( PrefabFactory.Prefabs ), "bxr_Tween" + type.ToString() ) )
            {
                if( showDebug ) { Debug.Log( "TweenManager.cs CreateTween() Found bxr_Tween" + type.ToString() + ", IsDefined = " + ( Enum.IsDefined( typeof( PrefabFactory.Prefabs ), "bxr_Tween" + type.ToString() ) ) ); }

                bxrTween tween = PrefabManager.InstantiatePrefab( (PrefabFactory.Prefabs)System.Enum.Parse( typeof( PrefabFactory.Prefabs ), "bxr_Tween" + type.ToString() ) ).GetComponent<bxrTween>();
                tween.gameObject.transform.SetParent( instance.transform );

                tween.gameObject.name = "bxr_Tween" + type.ToString();
                return tween;
            }

            else
            {
                if( showDebug ) { Debug.Log( "TweenManager.cs CreateTween() Couldn't Find bxr_Tween" + type.ToString() + ", IsDefined = " + ( Enum.IsDefined( typeof( PrefabFactory.Prefabs ), "bxr_Tween" + type.ToString() ) ) ); }
            }

            return null;

        } //END GetOrCreateTween





        //-------------------------------------//
        private static void SendCommand( bxrTween tween, TweenCommands command )
        //-------------------------------------//
        {

            if( tween != null )
            {
                if( command == TweenCommands.Pause )
                {
                    tween.Pause();
                }
                else if( command == TweenCommands.Resume )
                {
                    tween.Resume();
                }
                else if( command == TweenCommands.Stop )
                {
                    tween.Stop();
                }
                else if( command == TweenCommands.Play )
                {
                    tween.Play();
                }
            }

        } //END SendCommand

        //-------------------------------------//
        /// <summary>
        /// Allows you to send commands to any tweens that are modifying the component
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="component">Unity Component you would like to check</param>
        /// <param name="command">What you would like the tween to do</param>
        public static void SendCommandToTweens<T>( T component, TweenCommands command ) where T : Component
        //-------------------------------------//
        {
            
            if( component != null && component.GetComponent<bxrTween>() )
            {
                if( tweens != null && tweens.Count > 0 )
                {
                    foreach( bxrTween tween in tweens )
                    {
                        if( tween.IsTweenLinked( component ) )
                        {
                            SendCommand( tween, command );
                        }
                    }
                }
            }

        } //END SendCommandToTweens

        //-------------------------------------//
        /// <summary>
        /// Sends a command to all tweens that are modifying a Unity Component
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="command">What you would like the tweens to do</param>
        public static void SendCommandToTweens<T>( TweenCommands command ) where T : bxrTween
        //-------------------------------------//
        {
            if( tweens != null && tweens.Count > 0 )
            {
                foreach( bxrTween tween in tweens )
                {
                    if( tween is T )
                    {
                        SendCommand( tween, command );
                    }
                }
            }

        } //END SendCommandToTweens

        //--------------------------------------------//
        /// <summary>
        /// Pauses any running tweens that are modifying this component or object
        /// </summary>
        /// <typeparam name="T">Component/Object type</typeparam>
        /// <param name="pauseThis">Component or object to pause any running tweens on</param>
        public static void Pause<T>( T pauseThis )
        //--------------------------------------------//
        {

            if( pauseThis != null && tweens != null && tweens.Count > 0 )
            {
                foreach( bxrTween tween in tweens )
                {
                    if( tween.IsTweenLinked( pauseThis ) )
                    {
                        tween.Pause();
                    }
                }
            }

        } //END Pause

        //--------------------------------------------//
        /// <summary>
        /// Resumes any paused tweens that are modifying this component or object
        /// </summary>
        /// <typeparam name="T">Component/Object type</typeparam>
        /// <param name="resumeThis">Component or object to resume any paused tweens on</param>
        public static void Resume<T>( T resumeThis )
        //--------------------------------------------//
        {

            if( resumeThis != null && tweens != null && tweens.Count > 0 )
            {
                foreach( bxrTween tween in tweens )
                {
                    if( tween.IsPaused( resumeThis ) )
                    {
                        tween.Resume();
                    }
                }
            }

        } //END Resume

        //--------------------------------------------//
        /// <summary>
        /// Stops any existing tweens that are modifying this component or object
        /// </summary>
        /// <typeparam name="T">Component/Object type</typeparam>
        /// <param name="stopThis">Component or object to stop any existing tweens</param>
        public static void Stop<T>( T stopThis )
        //--------------------------------------------//
        {

            if( stopThis != null && tweens != null && tweens.Count > 0 )
            {
                foreach( bxrTween tween in tweens )
                {
                    if( tween.IsTweenLinked( stopThis ) )
                    {
                        tween.Stop();
                    }
                }
            }

        } //END Stop

        //--------------------------------------------//
        /// <summary>
        /// Plays any existing tweens that are modifying this component or object
        /// </summary>
        /// <typeparam name="T">Component/Object type</typeparam>
        /// <param name="playThis">Component or object to play any existing tweens</param>
        public static void Play<T>( T playThis )
        //--------------------------------------------//
        {

            if( playThis != null && tweens != null && tweens.Count > 0 )
            {
                foreach( bxrTween tween in tweens )
                {
                    if( tween.IsTweenLinked( playThis ) )
                    {
                        tween.Play();
                    }
                }
            }

        } //END Play

        //--------------------------------------------//
        /// <summary>
        /// Checks if there are any paused tweens that are modifying the values on your component or object
        /// </summary>
        /// <typeparam name="T">Component or Object</typeparam>
        /// <param name="checkThis">Checks this component/object for any paused tweens that might be modifying its values</param>
        /// <returns></returns>
        public static bool IsPaused<T>( T checkThis )
        //--------------------------------------------//
        {

            if( checkThis != null )
            {
                if( tweens != null && tweens.Count > 0 )
                {
                    foreach( bxrTween tween in tweens )
                    {
                        if( tween != null && tween.IsPaused( checkThis ) )
                        {
                            return true;
                        }
                    }
                }
            }

            return false;

        } //END IsPaused

        //--------------------------------------------//
        /// <summary>
        /// Checks if there are any tweens that are modifying the values on your component or object
        /// </summary>
        /// <typeparam name="T">Component or Object</typeparam>
        /// <param name="checkThis">Checks this component/object for any tweens that might be modifying its values</param>
        /// <returns></returns>
        public static bool IsTweenLinked<T>( T checkThis )
        //--------------------------------------------//
        {

            if( checkThis != null )
            {
                if( tweens != null && tweens.Count > 0 )
                {
                    foreach( bxrTween tween in tweens )
                    {
                        if( tween != null && tween.IsTweenLinked( checkThis ) )
                        {
                            return true;
                        }
                    }
                }
            }

            return false;

        } //END IsTweenLinked

        //--------------------------------------------//
        /// <summary>
        /// Checks if there are any tweens that are modifying the variable with the specified fieldName on your component or object
        /// </summary>
        /// <typeparam name="T">Component or Object</typeparam>
        /// <param name="checkThis">Checks this component/object for any tweens that might be modifying its values</param>
        /// <param name="fieldName">The variable field to check to see if any tweens are modifying</param>
        /// <returns></returns>
        public static bool IsTweenLinked<T>( T checkThis, string fieldName )
        //--------------------------------------------//
        {

            if( checkThis != null )
            {
                if( tweens != null && tweens.Count > 0 )
                {
                    foreach( bxrTween tween in tweens )
                    {
                        if( tween != null && tween.IsTweenLinked( checkThis, fieldName ) )
                        {
                            return true;
                        }
                    }
                }
            }

            return false;

        } //END IsTweenLinked

        //--------------------------------------------//
        /// <summary>
        /// Checks if there are any tweens that are modifying the values on your component or object, if there are that bxrTween is returned
        /// </summary>
        /// <typeparam name="T">Component or Object</typeparam>
        /// <param name="checkThis">Checks this component/object for any tweens that might be modifying its values</param>
        /// <returns></returns>
        public static bxrTween GetLinkedTween<T>( T checkThis )
        //--------------------------------------------//
        {

            if( checkThis != null )
            {
                if( tweens != null && tweens.Count > 0 )
                {
                    foreach( bxrTween tween in tweens )
                    {
                        if( tween != null && tween.IsTweenLinked( checkThis ) )
                        {
                            return tween;
                        }
                    }
                }
            }

            return null;

        } //END GetLinkedTween

        //--------------------------------------------//
        /// <summary>
        /// Checks if there are any tweens that are modifying the values on your component or object, if there are that bxrTween is returned
        /// </summary>
        /// <typeparam name="T">Component or Object</typeparam>
        /// <param name="checkThis">Checks this component/object for any tweens that might be modifying its values</param>
        /// <param name="fieldName">The variable name you would like to check for active tweens</param>
        /// <returns></returns>
        public static bxrTween GetLinkedTween<T>( T checkThis, string fieldName )
        //--------------------------------------------//
        {

            if( checkThis != null )
            {
                if( tweens != null && tweens.Count > 0 )
                {
                    foreach( bxrTween tween in tweens )
                    {
                        if( tween != null && tween.IsTweenLinked( checkThis, fieldName ) )
                        {
                            return tween;
                        }
                    }
                }
            }

            return null;

        } //END GetLinkedTween


        //--------------------------------------------//
        /// <summary>
        /// Called by Tweens before they are destroyed, removes them from our list of active tweens
        /// </summary>
        /// <param name="tween">The tween to remove from the list of active tweens</param>
        public static void PrepareTweenForDestroy( bxrTween tween )
        //--------------------------------------------//
        {

            //Remove this tween from the list
            if( tween != null && tweens != null && tweens.Count > 0 && tweens.Contains( tween ) )
            {
                tweens.Remove( tween );
            }

        } //END PrepareTweenForDestroy

        //--------------------------------------------//
        /// <summary>
        /// Kills any tweens that are modifying the component or Object you pass in
        /// </summary>
        /// <typeparam name="T">Any Unity Component or UnityEngine.Object</typeparam>
        /// <param name="killThis">The Unity Component or Object you would like to kill any tweens from modifying</param>
        public static void Kill<T>( T killThis )
        //--------------------------------------------//
        {

            //If we find a tween referencing this Component, kill it
            if( killThis != null )
            {
                if( tweens != null & tweens.Count > 0 )
                {
                    
                    //Loop through the tween in reverse and remove them
                    for( int i = tweens.Count - 1; i >= 0; i-- )
                    {
                        if( tweens[ i ] != null )
                        {
                            if( killThis is Component )
                            {
                                if( tweens[i].GetLinkedComponent() == killThis as Component )
                                {
                                    //Debug.Log( "TweenManager.cs Kill() tweens[" + i + "](" + tweens[ i ].name + ").GetLinkedComponent(" + tweens[ i ].GetLinkedComponent() + ") == killThis( " + killThis + " )" );

                                    bxrTween tween = tweens[ i ];
                                    tweens.Remove( tween );
                                    tween.Kill();
                                }
                                else
                                {
                                    //Debug.Log( "TweenManager.cs Kill() tweens[" + i + "](" + tweens[i].name + ").GetLinkedComponent(" + tweens[i].GetLinkedComponent() + ") != killThis( " + killThis + " )" );
                                }
                            }
                            else if( killThis is UnityEngine.Object )
                            {
                                if( tweens[i].GetLinkedObject() == killThis as UnityEngine.Object )
                                {
                                    //Debug.Log( "TweenManager.cs Kill() tweens[" + i + "](" + tweens[ i ].name + ").GetLinkedObject(" + tweens[ i ].GetLinkedObject() + ") == killThis( " + killThis + " )" );

                                    bxrTween tween = tweens[ i ];
                                    tweens.Remove( tween );
                                    tween.Kill();
                                }
                                else
                                {
                                    //Debug.Log( "TweenManager.cs Kill() tweens[" + i + "](" + tweens[ i ].name + ").GetLinkedObject(" + tweens[ i ].GetLinkedObject() + ") != killThis( " + killThis + " )" );
                                }
                            }
                            else
                            {
                                //Debug.Log( "TweenManager.cs Kill() killThis is neither component or object... killThis = " + killThis );
                            }
                        }
                    }
                    
                }
            }

        } //END Kill
        

    } //END Class

} //END Namespace