using UnityEngine;
using System.Collections;
using UnityEngine.Events;
using UnityEngine.UI;

namespace BrandXR
{
    public static class TweenExtensions
    {

        //------------------------------------------------------------------//
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
        public static bxrTween Move<T>( this T tweenThis, Vector3 endValue, float length, AnimationCurve easeCurve, Vector3? startValue = null, bxrTweenPosition.LocalOrWorldSpace useLocalOrWorldSpace = bxrTweenPosition.LocalOrWorldSpace.Local, float delay = 0f, bool loop = false, UnityEvent onCompleteOrLoop = null ) where T : Transform
        //------------------------------------------------------------------//
        {
            
            return TweenManager.Move( tweenThis, endValue, length, easeCurve, startValue, useLocalOrWorldSpace, delay, loop, onCompleteOrLoop );

        } //END Move

        //------------------------------------------------------------------//
        /// <summary>
        /// Move a Transform across local or global space
        /// </summary>
        /// <typeparam name="T">Transform</typeparam>
        /// <param name="tweenThis">Accepts [Transform, RectTransform]</param>
        /// <param name="endValue">The position you would like to move this Transform to</param>
        /// <param name="length">How long tween should take</param>
        /// <param name="easeType">What easing you would like to use</param>
        /// <param name="startValue">[OPTIONAL] Value to start from. If not provided we will use the existing value from the 'tweenThis' object</param>
        /// <param name="useLocalOrWorldSpace">[OPTIONAL] Would you like this transform to move in local or global space?</param>
        /// <param name="delay">[OPTIONAL] How long we should wait before starting to tween</param>
        /// <param name="loop">[OPTIONAL] Keep the tween running via a loop indefinitely</param>
        /// <param name="onCompleteOrLoop">[OPTIONAL] A UnityEvent to call once the tween has finished or completed a single loop</param>
        /// <returns></returns>
        public static bxrTween Move<T>( this T tweenThis, Vector3 endValue, float length, EaseCurve.EaseType easeType, Vector3? startValue = null, bxrTweenPosition.LocalOrWorldSpace useLocalOrWorldSpace = bxrTweenPosition.LocalOrWorldSpace.Local, float delay = 0f, bool loop = false, UnityEvent onCompleteOrLoop = null ) where T : Transform
        //------------------------------------------------------------------//
        {

            return TweenManager.Move( tweenThis, endValue, length, EaseCurve.GetEaseCurve( easeType ), startValue, useLocalOrWorldSpace, delay, loop, onCompleteOrLoop );

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
        public static bxrTween Rotate<T>( this T tweenThis, Vector3 endValue, float length, AnimationCurve easeCurve, Vector3? startValue = null, float delay = 0f, bool loop = false, UnityEvent onCompleteOrLoop = null ) where T : Transform
        //---------------------------------------------//
        {

            return TweenManager.Rotate( tweenThis, endValue, length, easeCurve, startValue, delay, loop, onCompleteOrLoop );

        } //END Rotate

        //---------------------------------------------//
        /// <summary>
        /// Rotates a Transform or RectTransform using Quaternions. Supply this function with the localEulerAngles for the 'endValue' and 'startValue'
        /// </summary>
        /// <typeparam name="T">Transform</typeparam>
        /// <param name="tweenThis">Accepts [Transform, RectTransform]</param>
        /// <param name="endValue">What should the localEulerAngles for the transform be by the end of this rotation?</param>
        /// <param name="length">How long tween should take</param>
        /// <param name="easeType">What easing you would like to use</param>
        /// <param name="startValue">[OPTIONAL] Value to start from. If not provided we will use the existing value from the 'tweenThis' object</param>
        /// <param name="delay">[OPTIONAL] How long we should wait before starting to tween</param>
        /// <param name="loop">[OPTIONAL] Keep the tween running via a loop indefinitely</param>
        /// <param name="onCompleteOrLoop">[OPTIONAL] A UnityEvent to call once the tween has finished or completed a single loop</param>
        /// <returns></returns>
        public static bxrTween Rotate<T>( this T tweenThis, Vector3 endValue, float length, EaseCurve.EaseType easeType, Vector3? startValue = null, float delay = 0f, bool loop = false, UnityEvent onCompleteOrLoop = null ) where T : Transform
        //---------------------------------------------//
        {

            return TweenManager.Rotate( tweenThis, endValue, length, EaseCurve.GetEaseCurve( easeType ), startValue, delay, loop, onCompleteOrLoop );

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
        public static bxrTween Scale<T>( this T tweenThis, Vector3 endValue, float length, AnimationCurve easeCurve, Vector3? startValue = null, float delay = 0f, bool loop = false, UnityEvent onCompleteOrLoop = null ) where T : Transform
        //---------------------------------------------//
        {

            return TweenManager.Scale( tweenThis, endValue, length, easeCurve, startValue, delay, loop, onCompleteOrLoop );

        } //END Scale 

        //---------------------------------------------//
        /// <summary>
        /// Scale a transform in local space. For RectTransform this will change the 'sizeDelta' value
        /// </summary>
        /// <typeparam name="T">Transform</typeparam>
        /// <param name="tweenThis">Accepts [Transform, RectTransform]</param>
        /// <param name="endValue">What should the final scale be? For RectTransform we are scaling the 'sizeDelta' value so only the (X, Y) values will be used</param>
        /// <param name="length">How long tween should take</param>
        /// <param name="easeType">The easing you would like to use on this tween</param>
        /// <param name="startValue">[OPTIONAL] Value to start from. If not provided we will use the existing value from the 'tweenThis' object</param>
        /// <param name="delay">[OPTIONAL] How long we should wait before starting to tween</param>
        /// <param name="loop">[OPTIONAL] Keep the tween running via a loop indefinitely</param>
        /// <param name="onCompleteOrLoop">[OPTIONAL] A UnityEvent to call once the tween has finished or completed a single loop</param>
        /// <returns></returns>
        public static bxrTween Scale<T>( this T tweenThis, Vector3 endValue, float length, EaseCurve.EaseType easeType, Vector3? startValue = null, float delay = 0f, bool loop = false, UnityEvent onCompleteOrLoop = null ) where T : Transform
        //---------------------------------------------//
        {

            return TweenManager.Scale( tweenThis, endValue, length, EaseCurve.GetEaseCurve( easeType ), startValue, delay, loop, onCompleteOrLoop );

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
        public static bxrTween Scale<T>( this T tweenThis, float endValueMultiplier, float length, AnimationCurve easeCurve, Vector3? startValue = null, float delay = 0f, bool loop = false, UnityEvent onCompleteOrLoop = null ) where T : Transform
        //---------------------------------------------//
        {

            return TweenManager.Scale( tweenThis, endValueMultiplier, length, easeCurve, startValue, delay, loop, onCompleteOrLoop );

        } //END Scale 

        //---------------------------------------------//
        /// <summary>
        /// Scale a transform in local space. For RectTransform this will change the 'sizeDelta' value
        /// </summary>
        /// <typeparam name="T">Transform</typeparam>
        /// <param name="tweenThis">Accepts [Transform, RectTransform]</param>
        /// <param name="endValueMultiplier"> What multiplier should we apply onto the current scale?</param>
        /// <param name="length">How long tween should take</param>
        /// <param name="easeType">The easing you would like this tween to use</param>
        /// <param name="startValue">[OPTIONAL] Value to start from. If not provided we will use the existing value from the 'tweenThis' object</param>
        /// <param name="delay">[OPTIONAL] How long we should wait before starting to tween</param>
        /// <param name="loop">[OPTIONAL] Keep the tween running via a loop indefinitely</param>
        /// <param name="onCompleteOrLoop">[OPTIONAL] A UnityEvent to call once the tween has finished or completed a single loop</param>
        /// <returns></returns>
        public static bxrTween Scale<T>( this T tweenThis, float endValueMultiplier, float length, EaseCurve.EaseType easeType, Vector3? startValue = null, float delay = 0f, bool loop = false, UnityEvent onCompleteOrLoop = null ) where T : Transform
        //---------------------------------------------//
        {

            return TweenManager.Scale( tweenThis, endValueMultiplier, length, EaseCurve.GetEaseCurve( easeType ), startValue, delay, loop, onCompleteOrLoop );

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
        public static bxrTween Color<T>( this T tweenThis, Color endValue, float length, AnimationCurve easeCurve, Color? startValue = null, float delay = 0f, bool loop = false, UnityEvent onCompleteOrLoop = null )
        //---------------------------------------------//
        {

            return TweenManager.Color( tweenThis, endValue, length, easeCurve, startValue, delay, loop, onCompleteOrLoop );

        } //END Color

        //---------------------------------------------//
        /// <summary>
        /// Change the color for the passed in Renderer or Material
        /// </summary>
        /// <typeparam name="T">Renderer/Object</typeparam>
        /// <param name="tweenThis">Accepts [Renderer, Image, RawImage, SpriteRenderer, Text, Material, CanvasGroup]</param>
        /// <param name="endValue">Color to tween to</param>
        /// <param name="length">How long tween should take</param>
        /// <param name="easeType">What ease type you would like this tween to use</param>
        /// <param name="startValue">[OPTIONAL] Value to start from. If not provided we will use the existing value from the 'tweenThis' object</param>
        /// <param name="delay">[OPTIONAL] How long we should wait before starting to tween</param>
        /// <param name="loop">[OPTIONAL] Keep the tween running via a loop indefinitely</param>
        /// <param name="onCompleteOrLoop">[OPTIONAL] A UnityEvent to call once the tween has finished or completed a single loop</param>
        /// <returns></returns>
        public static bxrTween Color<T>( this T tweenThis, Color endValue, float length, EaseCurve.EaseType easeType, Color? startValue = null, float delay = 0f, bool loop = false, UnityEvent onCompleteOrLoop = null )
        //---------------------------------------------//
        {

            return TweenManager.Color( tweenThis, endValue, length, EaseCurve.GetEaseCurve( easeType ), startValue, delay, loop, onCompleteOrLoop );

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
        public static bxrTween ImageFill( this Image tweenThis, float endValue, float length, AnimationCurve easeCurve, float? startValue = null, float delay = 0f, bool loop = false, UnityEvent onCompleteOrLoop = null )
        //---------------------------------------------//
        {

            return TweenManager.TweenFloat( tweenThis, "fillAmount", endValue, length, easeCurve, startValue, delay, loop, onCompleteOrLoop );

        } //END ImageFill

        //---------------------------------------------//
        /// <summary>
        /// Change the fill value for an Image object
        /// </summary>
        /// <param name="tweenThis">Accepts [Image]</param>
        /// <param name="endValue">What should the final fill value be?</param>
        /// <param name="length">How long tween should take</param>
        /// <param name="easeType">What ease you would like this tween to use</param>
        /// <param name="startValue">[OPTIONAL] Value to start from. If not provided we will use the existing value from the 'tweenThis' object</param>
        /// <param name="delay">[OPTIONAL] How long we should wait before starting to tween</param>
        /// <param name="loop">[OPTIONAL] Keep the tween running via a loop indefinitely</param>
        /// <param name="onCompleteOrLoop">[OPTIONAL] A UnityEvent to call once the tween has finished or completed a single loop</param>
        /// <returns></returns>
        public static bxrTween ImageFill( this Image tweenThis, float endValue, float length, EaseCurve.EaseType easeType, float? startValue = null, float delay = 0f, bool loop = false, UnityEvent onCompleteOrLoop = null )
        //---------------------------------------------//
        {

            return TweenManager.TweenFloat( tweenThis, "fillAmount", endValue, length, EaseCurve.GetEaseCurve( easeType ), startValue, delay, loop, onCompleteOrLoop );

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
        public static bxrTween AudioPitch( this AudioSource tweenThis, float endValue, float length, AnimationCurve easeCurve, float? startValue = null, float delay = 0f, bool loop = false, UnityEvent onCompleteOrLoop = null )
        //---------------------------------------------//
        {

            return TweenManager.TweenFloat( tweenThis, "pitch", endValue, length, easeCurve, startValue, delay, loop, onCompleteOrLoop );

        } //END AudioPitch

        //---------------------------------------------//
        /// <summary>
        /// Change the pitch value for an AudioSource
        /// </summary>
        /// <param name="tweenThis">Accepts [AudioSource]</param>
        /// <param name="endValue">What should the final pitch value be?</param>
        /// <param name="length">How long tween should take</param>
        /// <param name="easeType">The ease type you would like this tween to use</param>
        /// <param name="startValue">[OPTIONAL] Value to start from. If not provided we will use the existing value from the 'tweenThis' object</param>
        /// <param name="delay">[OPTIONAL] How long we should wait before starting to tween</param>
        /// <param name="loop">[OPTIONAL] Keep the tween running via a loop indefinitely</param>
        /// <param name="onCompleteOrLoop">[OPTIONAL] A UnityEvent to call once the tween has finished or completed a single loop</param>
        /// <returns></returns>
        public static bxrTween AudioPitch( this AudioSource tweenThis, float endValue, float length, EaseCurve.EaseType easeType, float? startValue = null, float delay = 0f, bool loop = false, UnityEvent onCompleteOrLoop = null )
        //---------------------------------------------//
        {

            return TweenManager.TweenFloat( tweenThis, "pitch", endValue, length, EaseCurve.GetEaseCurve( easeType ), startValue, delay, loop, onCompleteOrLoop );

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
        public static bxrTween TweenInt<T>( this T tweenThis, string fieldName, int endValue, int length, AnimationCurve easeCurve, int? startValue = null, float delay = 0f, bool loop = false, UnityEvent onCompleteOrLoop = null ) where T : UnityEngine.Object
        //---------------------------------------------//
        {

            return TweenManager.TweenInt( tweenThis, fieldName, endValue, length, easeCurve, startValue, delay, loop, onCompleteOrLoop );

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
        /// <param name="easeType">The easing you would like this tween to use</param>
        /// <param name="startValue">[OPTIONAL] Value to start from. If not provided we will use the existing value from the 'tweenThis' object</param>
        /// <param name="delay">[OPTIONAL] How long we should wait before starting to tween</param>
        /// <param name="loop">[OPTIONAL] Keep the tween running via a loop indefinitely</param>
        /// <param name="onCompleteOrLoop">[OPTIONAL] A UnityEvent to call once the tween has finished or completed a single loop</param>
        /// <returns></returns>
        public static bxrTween TweenInt<T>( this T tweenThis, string fieldName, int endValue, int length, EaseCurve.EaseType easeType, int? startValue = null, float delay = 0f, bool loop = false, UnityEvent onCompleteOrLoop = null ) where T : UnityEngine.Object
            //---------------------------------------------//
        {

            return TweenManager.TweenInt( tweenThis, fieldName, endValue, length, EaseCurve.GetEaseCurve( easeType ), startValue, delay, loop, onCompleteOrLoop );

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
        public static bxrTween TweenFloat<T>( this T tweenThis, string fieldName, float endValue, float length, AnimationCurve easeCurve, float? startValue = null, float delay = 0f, bool loop = false, UnityEvent onCompleteOrLoop = null ) where T : UnityEngine.Object
        //---------------------------------------------//
        {

            return TweenManager.TweenFloat( tweenThis, fieldName, endValue, length, easeCurve, startValue, delay, loop, onCompleteOrLoop );

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
        /// <param name="easeType">The easing you would like this tween to use</param>
        /// <param name="startValue">[OPTIONAL] Value to start from. If not provided we will use the existing value from the 'tweenThis' object</param>
        /// <param name="delay">[OPTIONAL] How long we should wait before starting to tween</param>
        /// <param name="loop">[OPTIONAL] Keep the tween running via a loop indefinitely</param>
        /// <param name="onCompleteOrLoop">[OPTIONAL] A UnityEvent to call once the tween has finished or completed a single loop</param>
        /// <returns></returns>
        public static bxrTween TweenFloat<T>( this T tweenThis, string fieldName, float endValue, float length, EaseCurve.EaseType easeType, float? startValue = null, float delay = 0f, bool loop = false, UnityEvent onCompleteOrLoop = null ) where T : UnityEngine.Object
        //---------------------------------------------//
        {

            return TweenManager.TweenFloat( tweenThis, fieldName, endValue, length, EaseCurve.GetEaseCurve( easeType ), startValue, delay, loop, onCompleteOrLoop );

        } //END TweenFloat





        //--------------------------------------------//
        /// <summary>
        /// Pauses any running tweens that are modifying this component or object
        /// </summary>
        /// <typeparam name="T">Component/Object type</typeparam>
        /// <param name="pauseThis">Component or object to pause any running tweens on</param>
        public static void Pause<T>( this T pauseThis )
        //--------------------------------------------//
        {

            TweenManager.Pause( pauseThis );

        } //END Pause

        //--------------------------------------------//
        /// <summary>
        /// Resumes any paused tweens that are modifying this component or object
        /// </summary>
        /// <typeparam name="T">Component/Object type</typeparam>
        /// <param name="resumeThis">Component or object to resume any paused tweens on</param>
        public static void Resume<T>( this T resumeThis )
        //--------------------------------------------//
        {

            TweenManager.Resume( resumeThis );

        } //END Resume

        //--------------------------------------------//
        /// <summary>
        /// Stops any existing tweens that are modifying this component or object
        /// </summary>
        /// <typeparam name="T">Component/Object type</typeparam>
        /// <param name="stopThis">Component or object to stop any existing tweens</param>
        public static void Stop<T>( this T stopThis )
        //--------------------------------------------//
        {

            TweenManager.Stop( stopThis );

        } //END Stop

        //--------------------------------------------//
        /// <summary>
        /// Plays any existing tweens that are modifying this component or object
        /// </summary>
        /// <typeparam name="T">Component/Object type</typeparam>
        /// <param name="playThis">Component or object to play any existing tweens</param>
        public static void Play<T>( this T playThis )
        //--------------------------------------------//
        {

            TweenManager.Play( playThis );

        } //END Play



        //--------------------------------------------//
        /// <summary>
        /// Checks if there are any paused tweens that are modifying the values on your component or object
        /// </summary>
        /// <typeparam name="T">Component or Object</typeparam>
        /// <param name="checkThis">Checks this component/object for any paused tweens that might be modifying its values</param>
        /// <returns></returns>
        public static bool IsPaused<T>( this T checkThis )
        //--------------------------------------------//
        {

            return TweenManager.IsPaused( checkThis );

        } //END IsPaused

        //--------------------------------------------//
        /// <summary>
        /// Checks if there are any tweens that are modifying the values on your component
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="checkThis">Checks this component for any tweens that might be modifying its values</param>
        /// <returns></returns>
        public static bool IsTweenLinked<T>( this T checkThis )
        //--------------------------------------------//
        {

            return TweenManager.IsTweenLinked( checkThis );

        } //END IsTweenLinked


        //--------------------------------------------//
        /// <summary>
        /// Kills any tweens that are modifying the component or Object you pass in
        /// </summary>
        /// <typeparam name="T">Any Unity Component or UnityEngine.Object</typeparam>
        /// <param name="killThis">The Unity Component or Object you would like to kill any tweens from modifying</param>
        public static void Kill<T>( this T killThis )
        //--------------------------------------------//
        {

            TweenManager.Kill( killThis );

        } //END Kill

    } //END Class

} //END Namespace