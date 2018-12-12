using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Events;

namespace BrandXR
{
    public class UITweenManager<UIT>: MonoBehaviour
    where UIT : UITweener
    {

        public List<UIT> tweeners;

        public UnityEvent onAllCompleteMethod = null;

        private int tweensToWaitFor = 0;

        //--------------------------------------------//
        public virtual void Play( UITweener.TweenValue tweenValue )
        //--------------------------------------------//
        {
            Stop();

            SetOnAllCompleteMethod( null );


        } //END Play

        //--------------------------------------------//
        public virtual void Play( UITweener.TweenValue tweenValue, UnityEvent onAllCompleteMethod )
        //--------------------------------------------//
        {
            Stop();

            SetOnAllCompleteMethod( onAllCompleteMethod );
            
        } //END Play

        //-------------------------//
        protected void SetOnAllCompleteMethod( UnityEvent onAllCompleteMethod )
        //-------------------------//
        {

            if( onAllCompleteMethod != null )
            {
                this.onAllCompleteMethod = onAllCompleteMethod;
                tweensToWaitFor = tweeners.Count();
            }
            else
            {
                this.onAllCompleteMethod = null;
                tweensToWaitFor = 0;
            }

        } //END SetOnAllCompleteMethod

        //--------------------------------------------//
        protected void CallOnAllComplete()
        //--------------------------------------------//
        {

            tweensToWaitFor--;

            if( tweensToWaitFor <= 0 && onAllCompleteMethod != null )
            {
                onAllCompleteMethod.Invoke();
            }

        } //END CallOnAllComplete


        //--------------------------------------------//
        public virtual void Force( UITweener.TweenValue tweenValue )
        //--------------------------------------------//
        {
            Stop();

            SetOnAllCompleteMethod( null );


        } //END Force


        //--------------------------------------------//
        public void SetTweenSpeed( float tweenSpeed, UITweener.TweenValue tweenValue )
        //--------------------------------------------//
        {
            foreach( UITweener ui in tweeners )
            {
                ui.SetTweenSpeed( tweenSpeed, tweenValue );
            }

        } //END SetTweenSpeed

        //--------------------------------------------//
        public float GetTweenSpeed( int tweenerIndex, UITweener.TweenValue tweenValue )
        //--------------------------------------------//
        {
            if( tweeners.Count > tweenerIndex )
            {
                return tweeners[ tweenerIndex ].GetTweenSpeed( tweenValue );
            }
            else
            {
                return 0f;
            }

        } //END GetTweenSpeed

        //--------------------------------------------//
        public void SetDelay( float delay, UITweener.TweenValue tweenValue )
        //--------------------------------------------//
        {
            foreach( UITweener ui in tweeners )
            {
                ui.SetDelay( delay, tweenValue );
            }

        } //END SetDelay

        //--------------------------------------------//
        public float GetDelay( int tweenerIndex, UITweener.TweenValue tweenValue )
        //--------------------------------------------//
        {
            if( tweeners.Count > tweenerIndex )
            {
                return tweeners[ tweenerIndex ].GetDelay( tweenValue );
            }
            else
            {
                return 0f;
            }

        } //END GetDelay

        //--------------------------------------------//
        public void SetEaseType( EaseCurve.EaseType easeType, UITweener.TweenValue tweenValue )
        //--------------------------------------------//
        {
            foreach( UITweener ui in tweeners )
            {
                ui.SetEaseType( easeType, tweenValue );
            }

        } //END SetEaseType

        //--------------------------------------------//
        public EaseCurve.EaseType GetEaseType( int tweenerIndex, UITweener.TweenValue tweenValue )
        //--------------------------------------------//
        {
            if( tweeners.Count > tweenerIndex )
            {
                return tweeners[ tweenerIndex ].GetEaseType( tweenValue );
            }
            else
            {
                return EaseCurve.EaseType.Linear;
            }

        } //END GetEaseType

        //--------------------------------------------//
        public void Stop()
        //--------------------------------------------//
        {

            foreach( UITweener ui in tweeners )
            {
                ui.Stop();
            }

        } //END Stop

    } //END Class

} //END Namespace