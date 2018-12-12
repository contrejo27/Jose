using UnityEngine;
using UnityEngine.Events;

namespace BrandXR
{
    public class ScreenFadeManager: MonoBehaviour
    {
        private bool showDebug = false;

        private UIColorTweenManager uiColorTweenManager_ScreenFader;

        //Singleton behavior
        private static ScreenFadeManager _instance;

        //--------------------------------------------//
        public static ScreenFadeManager instance
        //--------------------------------------------//
        {
            get
            {
                if( _instance == null )
                {
                    if( GameObject.FindObjectOfType<ScreenFadeManager>() == null ) { PrefabManager.InstantiatePrefab( PrefabFactory.Prefabs.bxr_ScreenFadeManager ); }
                    _instance = GameObject.FindObjectOfType<ScreenFadeManager>();
                }

                return _instance;
            }

        } //END Instance

        //--------------------------------------//
        public void Awake()
        //--------------------------------------//
        {
            DestroyDuplicateInstance();

            if( transform.parent == null )
            {
                DontDestroyOnLoad( transform.gameObject );
            }

            if( showDebug ) { Debug.Log( "ScreenFadeManager.cs Awake() start" ); }

            if( GetComponentInChildren<UIColorTweenManager>() != null )
            {
                uiColorTweenManager_ScreenFader = GetComponentInChildren<UIColorTweenManager>();
            }

            if( uiColorTweenManager_ScreenFader != null )
            {
                uiColorTweenManager_ScreenFader.ForceAlpha( 0f );
            }

            if( showDebug ) { Debug.Log( "ScreenFadeManager.cs Awake() end" ); }

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

        //---------------------------------------//
        public void Force( Color color )
        //---------------------------------------// 
        {

            if( showDebug ) { Debug.Log( "ScreenFadeManager.cs Force( Color ) = " + color ); }

            if( uiColorTweenManager_ScreenFader != null )
            {
                uiColorTweenManager_ScreenFader.SetColor( color, UITweener.TweenValue.Show );
                uiColorTweenManager_ScreenFader.Force( UITweener.TweenValue.Show );
            }

        } //END Force

        //---------------------------------------//
        public void Force( float alpha )
        //---------------------------------------// 
        {

            if( showDebug ) { Debug.Log( "ScreenFadeManager.cs Force( Alpha ) = " + alpha ); }

            if( uiColorTweenManager_ScreenFader != null )
            {
                uiColorTweenManager_ScreenFader.ForceAlpha( alpha );
            }

        } //END Force

        //---------------------------------------//
        public void Show( Color fadeToColor, float fadeSpeed, float delay, EaseCurve.EaseType easeCurve, UnityEvent onCompleteFunction )
        //---------------------------------------// 
        {

            if( showDebug ) { Debug.Log( "ScreenFadeManager.cs Show( onComplete ) " + onCompleteFunction.ToString() ); }

            if( uiColorTweenManager_ScreenFader != null )
            {
                uiColorTweenManager_ScreenFader.Play( UITweener.TweenValue.Show, fadeToColor, fadeSpeed, delay, easeCurve, onCompleteFunction );
            }

        } //END Show

        //---------------------------------------//
        public void Hide( float fadeSpeed, float delay, EaseCurve.EaseType easeCurve, UnityEvent onCompleteFunction )
        //---------------------------------------// 
        {

            if( showDebug ) { Debug.Log( "ScreenFadeManager.cs Hide( onComplete ) " + onCompleteFunction.ToString() ); }

            if( uiColorTweenManager_ScreenFader != null )
            {
                uiColorTweenManager_ScreenFader.PlayAlpha( 0f, fadeSpeed, delay, easeCurve, onCompleteFunction );
            }

        } //END Hide

    } //END Class

} //END Namespace