using UnityEngine;
using UnityEngine.Events;

namespace BrandXR
{
    public class UIColorTweenManager: UITweenManager<UIColorTweener>
    {
        //--------------------------------------------//
        public override void Play( UITweener.TweenValue tweenValue )
        //--------------------------------------------//
        {
            base.Play( tweenValue );

            foreach( UIColorTweener ui in tweeners )
            {
                //Debug.Log( "UIColorTweenManager.cs Play() ui.name = " + ui.name );
                ui.Play( tweenValue );
            }

        } //END Play

        //--------------------------------------------//
        public override void Play( UITweener.TweenValue tweenValue, UnityEvent onComplete )
        //--------------------------------------------//
        {
            base.Play( tweenValue, onComplete );

            foreach( UIColorTweener ui in tweeners )
            {
                ui.Play( tweenValue, onComplete );
            }

        } //END Play

        //--------------------------------------------//
        public void Play( Color color, UITweener.TweenValue tweenValue )
        //--------------------------------------------//
        {

            base.Play( tweenValue );

            foreach( UIColorTweener ui in tweeners )
            {
                ui.Play( color, tweenValue );
            }

        } //END Play

        //--------------------------------------------//
        public void Play( UITweener.TweenValue tweenValue, Color color, float tweenSpeed, float delay, EaseCurve.EaseType easeCurve, UnityEvent onComplete )
        //--------------------------------------------//
        {

            base.Play( tweenValue );

            foreach( UIColorTweener ui in tweeners )
            {
                ui.Play( tweenValue, color, tweenSpeed, delay, easeCurve, onComplete );
            }

        } //END Play

        //--------------------------------------------//
        public void PlayAlpha( float alpha, float tweenSpeed )
        //--------------------------------------------//
        {
            Stop();

            SetOnAllCompleteMethod( null );

            foreach( UIColorTweener ui in tweeners )
            {
                ui.PlayAlpha( alpha, tweenSpeed );
            }

        } //END PlayAlpha

        //--------------------------------------------//
        public void PlayAlpha( float alpha, float tweenSpeed, float delay, EaseCurve.EaseType easeCurve, UnityEvent onComplete )
        //--------------------------------------------//
        {
            Stop();

            SetOnAllCompleteMethod( null );

            foreach( UIColorTweener ui in tweeners )
            {
                ui.PlayAlpha( alpha, tweenSpeed, delay, easeCurve, onComplete );
            }

        } //END PlayAlpha

        //--------------------------------------------//
        public void ForceAlpha( float alpha )
        //--------------------------------------------//
        {
            Stop();

            SetOnAllCompleteMethod( null );

            foreach( UIColorTweener ui in tweeners )
            {
                ui.ForceAlpha( alpha );
            }

        } //END ForceAlpha

        //--------------------------------------------//
        public void SwapAlphaOnOff()
        //--------------------------------------------//
        {

            if( tweeners != null && tweeners.Count > 0 )
            {
                if( tweeners[0].GetColorFromRenderer().a > 0f )
                {
                    //The alpha is ON, so Force the alpha OFF
                    ForceAlpha( 0f );
                }
                else
                {
                    ForceAlpha( 1f );
                }
            }

        } //END SwapAlphaOnOff

        //--------------------------------------------//
        public void Force( Color color, UITweener.TweenValue tweenValue )
        //--------------------------------------------//
        {
            SetColor( color, tweenValue );

            Force( tweenValue );

        } //END Force

        //--------------------------------------------//
        public override void Force( UITweener.TweenValue tweenValue )
        //--------------------------------------------//
        {

            Stop();

            SetOnAllCompleteMethod( null );

            foreach( UIColorTweener ui in tweeners )
            {
                //Debug.Log( "UIColorTweenMaanger.cs Force( " + gameObject.name + " ) ui.name = " + ui.name + ", tweenValue = " + tweenValue );
                ui.Force( tweenValue );
            }

        } //END Force

        //--------------------------------------------//
        public void SetColor( Color color, UITweener.TweenValue tweenValue )
        //--------------------------------------------//
        {
            foreach( UIColorTweener ui in tweeners )
            {
                ui.SetColor( color, tweenValue );
            }

        } //END SetColor


    } //END Class

} //END Namespace