using UnityEngine;
using System.Collections.Generic;
using UnityEngine.Events;

namespace BrandXR
{
    public class UIScaleTweenManager: UITweenManager<UIScaleTweener>
    {
        //--------------------------------------------//
        public override void Play( UITweener.TweenValue tweenValue )
        //--------------------------------------------//
        {
            base.Play( tweenValue );

            foreach( UIScaleTweener ui in tweeners )
            {
                ui.Play( tweenValue );
            }

        } //END Play

        //--------------------------------------------//
        public override void Play( UITweener.TweenValue tweenValue, UnityEvent onComplete )
        //--------------------------------------------//
        {
            base.Play( tweenValue, onComplete );

            foreach( UIScaleTweener ui in tweeners )
            {
                ui.Play( tweenValue, onComplete );
            }

        } //END Play

        //--------------------------------------------//
        public void Play( UITweener.TweenValue tweenValue, Vector3 scale, float tweenSpeed, float delay, EaseCurve.EaseType easeType, UnityEvent onComplete )
        //--------------------------------------------//
        {
            Stop();

            foreach( UIScaleTweener ui in tweeners )
            {
                ui.Play( tweenValue, scale, tweenSpeed, delay, easeType, onComplete );
            }

        } //END Play

        //--------------------------------------------//
        public void Force( Vector3 scale, UITweener.TweenValue tweenValue )
        //--------------------------------------------//
        {
            SetScale( scale, tweenValue );

            Force( tweenValue );

        } //END Force

        //--------------------------------------------//
        public override void Force( UITweener.TweenValue tweenValue )
        //--------------------------------------------//
        {

            Stop();

            SetOnAllCompleteMethod( null );

            foreach( UIScaleTweener ui in tweeners )
            {
                ui.Force( tweenValue );
            }

        } //END Force

        //--------------------------------------------//
        public void SetScale( Vector3 scale, UITweener.TweenValue tweenValue )
        //--------------------------------------------//
        {
            foreach( UIScaleTweener ui in tweeners )
            {
                ui.SetScale( scale, tweenValue );
            }

        } //END SetScale

        //--------------------------------------------//
        public void SetScaleByMultiplier( float multiplier, UITweener.TweenValue tweenValue )
        //--------------------------------------------//
        {
            foreach( UIScaleTweener ui in tweeners )
            {
                ui.SetMultiplier( multiplier, tweenValue );
            }

        } //END SetScaleByMultiplier


    } //END Class

} //END Namespace