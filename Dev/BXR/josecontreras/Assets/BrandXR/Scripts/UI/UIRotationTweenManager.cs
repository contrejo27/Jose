using UnityEngine;
using System.Collections.Generic;
using UnityEngine.Events;

namespace BrandXR
{
    public class UIRotationTweenManager: UITweenManager<UIRotationTweener>
    {
        //--------------------------------------------//
        public override void Play( UITweener.TweenValue tweenValue )
        //--------------------------------------------//
        {
            base.Play( tweenValue );

            foreach( UIRotationTweener ui in tweeners )
            {
                ui.Play( tweenValue );
            }

        } //END Play

        //--------------------------------------------//
        public override void Play( UITweener.TweenValue tweenValue, UnityEvent onComplete )
        //--------------------------------------------//
        {
            base.Play( tweenValue, onComplete );

            foreach( UIRotationTweener ui in tweeners )
            {
                ui.Play( tweenValue, onComplete );
            }

        } //END Play

        //--------------------------------------------//
        public void Play( UITweener.TweenValue tweenValue, Vector3 rotation, float tweenSpeed, float delay, EaseCurve.EaseType easeCurve, UnityEvent onComplete )
        //--------------------------------------------//
        {
            base.Play( tweenValue );

            foreach( UIRotationTweener ui in tweeners )
            {
                ui.Play( tweenValue, rotation, tweenSpeed, delay, easeCurve, onComplete );
            }

        } //END Play

        //--------------------------------------------//
        public void Force( Vector3 position, UITweener.TweenValue tweenValue )
        //--------------------------------------------//
        {
            SetRotation( position, tweenValue );

            base.Force( tweenValue );

        } //END Force

        //--------------------------------------------//
        public override void Force( UITweener.TweenValue tweenValue )
        //--------------------------------------------//
        {

            Stop();

            SetOnAllCompleteMethod( null );

            foreach( UIRotationTweener ui in tweeners )
            {
                ui.Force( tweenValue );
            }

        } //END Force

        //--------------------------------------------//
        public void SetRotation( Vector3 rotation, UITweener.TweenValue tweenValue )
        //--------------------------------------------//
        {
            foreach( UIRotationTweener ui in tweeners )
            {
                ui.SetRotation( rotation, tweenValue );
            }

        } //END SetRotation

    } //END Class

} //END Namespace