using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Events;

namespace BrandXR
{
    public class UITweenManagerDirector: MonoBehaviour
    {

        public List<UIColorTweenManager> colorTweenManagers;
        public List<UIPositionTweenManager> positionTweenManagers;
        public List<UIRotationTweenManager> rotationTweenManagers;
        public List<UIScaleTweenManager> scaleTweenManagers;

        public UnityEvent onAllCompleteMethod = null;

        private int tweensToWaitFor = 0;

        //--------------------------------------------//
        public void Play( UITweener.TweenValue tweenValue )
        //--------------------------------------------//
        {
            Stop();

            SetOnAllCompleteMethod( null );

            foreach( UIColorTweenManager manager in colorTweenManagers )
            {
                manager.Play( tweenValue );
            }
            foreach( UIPositionTweenManager manager in positionTweenManagers )
            {
                manager.Play( tweenValue );
            }
            foreach( UIRotationTweenManager manager in rotationTweenManagers )
            {
                manager.Play( tweenValue );
            }
            foreach( UIScaleTweenManager manager in scaleTweenManagers )
            {
                manager.Play( tweenValue );
            }

        } //END Play

        //--------------------------------------------//
        public void Play( UITweener.TweenValue tweenValue, UnityEvent onAllComplete )
        //--------------------------------------------//
        {
            Stop();

            SetOnAllCompleteMethod( onAllComplete );

            foreach( UIColorTweenManager manager in colorTweenManagers )
            {
                manager.Play( tweenValue, onAllComplete );
            }
            foreach( UIPositionTweenManager manager in positionTweenManagers )
            {
                manager.Play( tweenValue );
            }
            foreach( UIRotationTweenManager manager in rotationTweenManagers )
            {
                manager.Play( tweenValue );
            }
            foreach( UIScaleTweenManager manager in scaleTweenManagers )
            {
                manager.Play( tweenValue );
            }

        } //END Play

        //--------------------------------------------//
        public void Force( UITweener.TweenValue tweenValue )
        //--------------------------------------------//
        {
            Stop();

            SetOnAllCompleteMethod( null );

            foreach( UIColorTweenManager manager in colorTweenManagers )
            {
                manager.Play( tweenValue );
            }
            foreach( UIPositionTweenManager manager in positionTweenManagers )
            {
                manager.Play( tweenValue );
            }
            foreach( UIRotationTweenManager manager in rotationTweenManagers )
            {
                manager.Play( tweenValue );
            }
            foreach( UIScaleTweenManager manager in scaleTweenManagers )
            {
                manager.Play( tweenValue );
            }

        } //END Force

        //-------------------------//
        protected void SetOnAllCompleteMethod( UnityEvent onAllCompleteMethod )
        //-------------------------//
        {

            if( onAllCompleteMethod != null )
            {
                this.onAllCompleteMethod = onAllCompleteMethod;
                tweensToWaitFor = colorTweenManagers.Count() + positionTweenManagers.Count() + rotationTweenManagers.Count() + scaleTweenManagers.Count();
            }
            else
            {
                this.onAllCompleteMethod = null;
                tweensToWaitFor = 0;
            }

        } //END SetOnAllCompleteMethod

        //--------------------------------------------//
        private void CallOnAllComplete()
        //--------------------------------------------//
        {

            tweensToWaitFor--;

            if( tweensToWaitFor <= 0 && onAllCompleteMethod != null )
            {
                onAllCompleteMethod.Invoke();
            }

        } //END CallOnAllComplete


        //--------------------------------------------//
        public void SetTweenSpeed( float tweenSpeed, UITweener.TweenValue tweenValue )
        //--------------------------------------------//
        {
            foreach( UIColorTweenManager manager in colorTweenManagers )
            {
                manager.SetTweenSpeed( tweenSpeed, tweenValue );
            }
            foreach( UIPositionTweenManager manager in positionTweenManagers )
            {
                manager.SetTweenSpeed( tweenSpeed, tweenValue );
            }
            foreach( UIRotationTweenManager manager in rotationTweenManagers )
            {
                manager.SetTweenSpeed( tweenSpeed, tweenValue );
            }
            foreach( UIScaleTweenManager manager in scaleTweenManagers )
            {
                manager.SetTweenSpeed( tweenSpeed, tweenValue );
            }

        } //END SetTweenSpeed

        //--------------------------------------------//
        public void SetDelay( float delay, UITweener.TweenValue tweenValue )
        //--------------------------------------------//
        {
            foreach( UIColorTweenManager manager in colorTweenManagers )
            {
                manager.SetDelay( delay, tweenValue );
            }
            foreach( UIPositionTweenManager manager in positionTweenManagers )
            {
                manager.SetDelay( delay, tweenValue );
            }
            foreach( UIRotationTweenManager manager in rotationTweenManagers )
            {
                manager.SetDelay( delay, tweenValue );
            }
            foreach( UIScaleTweenManager manager in scaleTweenManagers )
            {
                manager.SetDelay( delay, tweenValue );
            }

        } //END SetDelay

        //--------------------------------------------//
        public void SetEaseType( EaseCurve.EaseType easeType, UITweener.TweenValue tweenValue )
        //--------------------------------------------//
        {
            foreach( UIColorTweenManager manager in colorTweenManagers )
            {
                manager.SetEaseType( easeType, tweenValue );
            }
            foreach( UIPositionTweenManager manager in positionTweenManagers )
            {
                manager.SetEaseType( easeType, tweenValue );
            }
            foreach( UIRotationTweenManager manager in rotationTweenManagers )
            {
                manager.SetEaseType( easeType, tweenValue );
            }
            foreach( UIScaleTweenManager manager in scaleTweenManagers )
            {
                manager.SetEaseType( easeType, tweenValue );
            }

        } //END SetEaseType

        //--------------------------------------------//
        public void Stop()
        //--------------------------------------------//
        {

            foreach( UIColorTweenManager manager in colorTweenManagers )
            {
                manager.Stop();
            }
            foreach( UIPositionTweenManager manager in positionTweenManagers )
            {
                manager.Stop();
            }
            foreach( UIRotationTweenManager manager in rotationTweenManagers )
            {
                manager.Stop();
            }
            foreach( UIScaleTweenManager manager in scaleTweenManagers )
            {
                manager.Stop();
            }

        } //END Stop


    } //END Class

} //END Namespace