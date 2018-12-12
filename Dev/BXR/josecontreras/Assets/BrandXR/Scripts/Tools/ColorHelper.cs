using UnityEngine;
using System.Collections;
using System;
using UnityEngine.UI;

namespace BrandXR
{
    public class ColorHelper: MonoBehaviour
    {

        public bool showDebug = false;


        //Singleton behavior
        private static ColorHelper _instance;

        //--------------------------------------------//
        public static ColorHelper instance
        //--------------------------------------------//
        {
            get
            {
                if( _instance == null )
                {
                    _instance = GameObject.FindObjectOfType<ColorHelper>();
                }

                return _instance;
            }

        } //END Instance

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


        

        //-------------------------------------//
        public void CrossFade( Image image, float duration, float delay, Color changeToColor, Action onComplete )
        //-------------------------------------//
        {

            StartCoroutine( _CrossFade( image, duration, delay, changeToColor, onComplete ) );

        } //END CrossFade

        //-------------------------------------//
        private IEnumerator _CrossFade( Image image, float duration, float delay, Color changeToColor, Action onComplete )
        //-------------------------------------//
        {
            yield return new WaitForSeconds( delay );

            image.CrossFadeColor( changeToColor, duration, true, true );

            if( onComplete != null )
            {
                yield return new WaitForSeconds( duration );

                onComplete.Invoke();
            }

        } //END _CrossFade



        //-------------------------------------//
        public void CrossFade( RawImage image, float duration, float delay, Color changeToColor, Action onComplete )
        //-------------------------------------//
        {

            StartCoroutine( _CrossFade( image, duration, delay, changeToColor, onComplete ) );

        } //END CrossFade

        //-------------------------------------//
        private IEnumerator _CrossFade( RawImage image, float duration, float delay, Color changeToColor, Action onComplete )
        //-------------------------------------//
        {
            yield return new WaitForSeconds( delay );

            image.CrossFadeColor( changeToColor, duration, true, true );

            if( onComplete != null )
            {
                yield return new WaitForSeconds( duration );

                onComplete.Invoke();
            }

        } //END _CrossFade





        //-------------------------------------//
        public void CrossFade( Text text, float duration, float delay, Color changeToColor, Action onComplete )
        //-------------------------------------//
        {

            StartCoroutine( _CrossFade( text, duration, delay, changeToColor, onComplete ) );

        } //END CrossFade

        //-------------------------------------//
        private IEnumerator _CrossFade( Text text, float duration, float delay, Color changeToColor, Action onComplete )
        //-------------------------------------//
        {
            yield return new WaitForSeconds( delay );

            text.CrossFadeColor( changeToColor, duration, true, true );

            if( onComplete != null )
            {
                yield return new WaitForSeconds( duration );

                onComplete.Invoke();
            }

        } //END _CrossFade



        





        //--------------------------------------//
        public void Cancel( Coroutine cancelThis )
        //--------------------------------------//
        {

            if( cancelThis != null ) { StopCoroutine( cancelThis ); }

        } //END Cancel




    } //END Class

} //END Namespace