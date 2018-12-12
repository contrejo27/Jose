using UnityEngine;
using UnityEngine.Events;

namespace BrandXR
{
    public class UIPositionTweenManager: UITweenManager<UIPositionTweener>
    {
        //--------------------------------------------//
        public override void Play( UITweener.TweenValue tweenValue )
        //--------------------------------------------//
        {
            base.Play( tweenValue );

            foreach( UIPositionTweener ui in tweeners )
            {
                ui.Play( tweenValue );
            }

        } //END Play

        //--------------------------------------------//
        public override void Play( UITweener.TweenValue tweenValue, UnityEvent onComplete )
        //--------------------------------------------//
        {
            base.Play( tweenValue, onComplete );

            foreach( UIPositionTweener ui in tweeners )
            {
                ui.Play( tweenValue, onComplete );
            }

        } //END Play

        //--------------------------------------------//
        public void Play( UITweener.TweenValue tweenValue, Vector3 position, float tweenSpeed, float delay, EaseCurve.EaseType easeCurve, UnityEvent onComplete )
        //--------------------------------------------//
        {
            base.Play( tweenValue );

            foreach( UIPositionTweener ui in tweeners )
            {
                ui.Play( tweenValue, position, tweenSpeed, delay, easeCurve, onComplete );
            }

        } //END Play

        //--------------------------------------------//
        public void Force( Vector3 position, UITweener.TweenValue tweenValue )
        //--------------------------------------------//
        {
            SetPosition( position, tweenValue );

            Force( tweenValue );

        } //END Force

        //--------------------------------------------//
        public override void Force( UITweener.TweenValue tweenValue )
        //--------------------------------------------//
        {

            Stop();

            SetOnAllCompleteMethod( null );

            foreach( UIPositionTweener ui in tweeners )
            {
                ui.Force( tweenValue );
            }

        } //END Force



        //--------------------------------------------//
        public void SetPosition( Vector3 position, UITweener.TweenValue tweenValue )
        //--------------------------------------------//
        {
            foreach( UIPositionTweener ui in tweeners )
            {
                ui.SetPosition( position, tweenValue );
            }

        } //END SetPosition

    } //END Class

} //END Namespace