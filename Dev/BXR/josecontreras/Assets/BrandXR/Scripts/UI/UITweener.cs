using UnityEngine;
using Sirenix.OdinInspector;
using UnityEngine.Events;

namespace BrandXR
{
    public class UITweener: MonoBehaviour
    {
        public enum TweenValue
        {
            Default,
            Hide,
            Show,
            HoverEnter,
            HoverExit,
            Pressed,
            Hold,
            Enabled,
            Disabled
        }

        [FoldoutGroup("Easing")]
        public EaseCurve.EaseType easeType_Default = EaseCurve.EaseType.ExpoEaseInOut;
        [FoldoutGroup( "Easing" )]
        public EaseCurve.EaseType easeType_Hide = EaseCurve.EaseType.ExpoEaseInOut;
        [FoldoutGroup( "Easing" )]
        public EaseCurve.EaseType easeType_Show = EaseCurve.EaseType.ExpoEaseInOut;
        [FoldoutGroup( "Easing" )]
        public EaseCurve.EaseType easeType_HoverEnter = EaseCurve.EaseType.ExpoEaseInOut;
        [FoldoutGroup( "Easing" )]
        public EaseCurve.EaseType easeType_HoverExit = EaseCurve.EaseType.ExpoEaseInOut;
        [FoldoutGroup( "Easing" )]
        public EaseCurve.EaseType easeType_Pressed = EaseCurve.EaseType.ExpoEaseInOut;
        [FoldoutGroup( "Easing" )]
        public EaseCurve.EaseType easeType_Hold = EaseCurve.EaseType.ExpoEaseInOut;
        [FoldoutGroup( "Easing" )]
        public EaseCurve.EaseType easeType_Enabled = EaseCurve.EaseType.ExpoEaseInOut;
        [FoldoutGroup( "Easing" )]
        public EaseCurve.EaseType easeType_Disabled = EaseCurve.EaseType.ExpoEaseInOut;

        [FoldoutGroup("Tween Speed")]
        public float tweenSpeed_Default = 1f;
        [FoldoutGroup( "Tween Speed" )]
        public float tweenSpeed_Hide = 1f;
        [FoldoutGroup( "Tween Speed" )]
        public float tweenSpeed_Show = 1f;
        [FoldoutGroup( "Tween Speed" )]
        public float tweenSpeed_HoverEnter = 1f;
        [FoldoutGroup( "Tween Speed" )]
        public float tweenSpeed_HoverExit = 1f;
        [FoldoutGroup( "Tween Speed" )]
        public float tweenSpeed_Pressed = 1f;
        [FoldoutGroup( "Tween Speed" )]
        public float tweenSpeed_Hold = 1f;
        [FoldoutGroup( "Tween Speed" )]
        public float tweenSpeed_Enabled = 1f;
        [FoldoutGroup( "Tween Speed" )]
        public float tweenSpeed_Disabled = 1f;

        [FoldoutGroup("Delay")]
        public float delay_Default = 0f;
        [FoldoutGroup( "Delay" )]
        public float delay_Hide = 0f;
        [FoldoutGroup( "Delay" )]
        public float delay_Show = 0f;
        [FoldoutGroup( "Delay" )]
        public float delay_HoverEnter = 0f;
        [FoldoutGroup( "Delay" )]
        public float delay_HoverExit = 0f;
        [FoldoutGroup( "Delay" )]
        public float delay_Pressed = 0f;
        [FoldoutGroup( "Delay" )]
        public float delay_Hold = 0f;
        [FoldoutGroup( "Delay" )]
        public float delay_Enabled = 0f;
        [FoldoutGroup( "Delay" )]
        public float delay_Disabled = 0f;

        //Methods to call when tween completes or loops
        public UnityEvent onCompleteOrLoop = new UnityEvent();


        public bool ForceDefaultOnAwake = false;


        //Used to kill an existing tween while animating
        protected bxrTween tween;

        //------------------------------//
        public void Awake()
        //------------------------------//
        {
            FindInitialValues();

            if( ForceDefaultOnAwake )
            {
                ForceDefaults();
            }

        } //END Awake

        //-----------------------------------//
        protected virtual void FindInitialValues()
        //-----------------------------------//
        {

        } //END FindInitialValues

        //-----------------------------------//
        protected virtual void ForceDefaults()
        //-----------------------------------//
        {

        } //END ForceDefaults

        //-----------------------------------//
        public void SetTweenSpeed( float tweenSpeed, TweenValue tweenValue )
        //-----------------------------------//
        {

            if( tweenValue == TweenValue.Default ) { tweenSpeed_Default = tweenSpeed; }
            else if( tweenValue == TweenValue.Hide ) { tweenSpeed_Hide = tweenSpeed; }
            else if( tweenValue == TweenValue.Show ) { tweenSpeed_Show = tweenSpeed; }
            else if( tweenValue == TweenValue.HoverEnter ) { tweenSpeed_HoverEnter = tweenSpeed; }
            else if( tweenValue == TweenValue.HoverExit ) { tweenSpeed_HoverExit = tweenSpeed; }
            else if( tweenValue == TweenValue.Pressed ) { tweenSpeed_Pressed = tweenSpeed; }
            else if( tweenValue == TweenValue.Hold ) { tweenSpeed_Hold = tweenSpeed; }
            else if( tweenValue == TweenValue.Enabled ) { tweenSpeed_Enabled = tweenSpeed; }
            else if( tweenValue == TweenValue.Disabled ) { tweenSpeed_Disabled = tweenSpeed; }

        } //END SetTweenSpeed

        //-----------------------------------//
        public float GetTweenSpeed( TweenValue tweenValue )
        //-----------------------------------//
        {

            if( tweenValue == TweenValue.Default ) { return tweenSpeed_Default; }
            else if( tweenValue == TweenValue.Hide ) { return tweenSpeed_Hide; }
            else if( tweenValue == TweenValue.Show ) { return tweenSpeed_Show; }
            else if( tweenValue == TweenValue.HoverEnter ) { return tweenSpeed_HoverEnter; }
            else if( tweenValue == TweenValue.HoverExit ) { return tweenSpeed_HoverExit; }
            else if( tweenValue == TweenValue.Pressed ) { return tweenSpeed_Pressed; }
            else if( tweenValue == TweenValue.Hold ) { return tweenSpeed_Hold; }
            else if( tweenValue == TweenValue.Enabled ) { return tweenSpeed_Enabled; }
            else if( tweenValue == TweenValue.Disabled ) { return tweenSpeed_Disabled; }

            return tweenSpeed_Default;

        } //END GetTweenSpeed

        //-------------------------//
        public void SetEaseType( EaseCurve.EaseType easeCurve, TweenValue tweenValue )
        //-------------------------//
        {

            if( tweenValue == TweenValue.Default ) { easeType_Default = easeCurve; }
            else if( tweenValue == TweenValue.Hide ) { easeType_Hide = easeCurve; }
            else if( tweenValue == TweenValue.Show ) { easeType_Show = easeCurve; }
            else if( tweenValue == TweenValue.HoverEnter ) { easeType_HoverEnter = easeCurve; }
            else if( tweenValue == TweenValue.HoverExit ) { easeType_HoverExit = easeCurve; }
            else if( tweenValue == TweenValue.Pressed ) { easeType_Pressed = easeCurve; }
            else if( tweenValue == TweenValue.Hold ) { easeType_Hold = easeCurve; }
            else if( tweenValue == TweenValue.Enabled ) { easeType_Enabled = easeCurve; }
            else if( tweenValue == TweenValue.Disabled ) { easeType_Disabled = easeCurve; }

        } //END SetEaseType
        

        //-------------------------//
        public EaseCurve.EaseType GetEaseType( TweenValue tweenValue )
        //-------------------------//
        {

            if( tweenValue == TweenValue.Default ) { return easeType_Default; }
            else if( tweenValue == TweenValue.Hide ) { return easeType_Hide; }
            else if( tweenValue == TweenValue.Show ) { return easeType_Show; }
            else if( tweenValue == TweenValue.HoverEnter ) { return easeType_HoverEnter; }
            else if( tweenValue == TweenValue.HoverEnter ) { return easeType_HoverExit; }
            else if( tweenValue == TweenValue.Pressed ) { return easeType_Pressed; }
            else if( tweenValue == TweenValue.Hold ) { return easeType_Hold; }
            else if( tweenValue == TweenValue.Enabled ) { return easeType_Hold; }
            else if( tweenValue == TweenValue.Disabled ) { return easeType_Hold; }

            return easeType_Default;

        } //END GetEaseType

        //-------------------------//
        public void SetDelay( float delay, TweenValue tweenValue )
        //-------------------------//
        {

            if( tweenValue == TweenValue.Default ) { delay_Default = delay; }
            else if( tweenValue == TweenValue.Hide ) { delay_Hide = delay; }
            else if( tweenValue == TweenValue.Show ) { delay_Show = delay; }
            else if( tweenValue == TweenValue.HoverEnter ) { delay_HoverEnter = delay; }
            else if( tweenValue == TweenValue.HoverExit ) { delay_HoverExit = delay; }
            else if( tweenValue == TweenValue.Pressed ) { delay_Pressed = delay; }
            else if( tweenValue == TweenValue.Hold ) { delay_Hold = delay; }
            else if( tweenValue == TweenValue.Enabled ) { delay_Enabled = delay; }
            else if( tweenValue == TweenValue.Disabled ) { delay_Disabled = delay; }

        } //END SetDelay

        //-------------------------//
        public float GetDelay( TweenValue tweenValue )
        //-------------------------//
        {

            if( tweenValue == TweenValue.Default ) { return delay_Default; }
            else if( tweenValue == TweenValue.Hide ) { return delay_Hide; }
            else if( tweenValue == TweenValue.Show ) { return delay_Show; }
            else if( tweenValue == TweenValue.HoverEnter ) { return delay_HoverEnter; }
            else if( tweenValue == TweenValue.HoverExit ) { return delay_HoverExit; }
            else if( tweenValue == TweenValue.Pressed ) { return delay_Pressed; }
            else if( tweenValue == TweenValue.Hold ) { return delay_Hold; }
            else if( tweenValue == TweenValue.Enabled ) { return delay_Enabled; }
            else if( tweenValue == TweenValue.Disabled ) { return delay_Disabled; }

            return delay_Default;

        } //END GetDelay
        
        //-------------------------//
        public void SetOnCompleteEvent( UnityEvent onCompleteOrLoop )
        //-------------------------//
        {

            this.onCompleteOrLoop = onCompleteOrLoop;

        } //END SetOnCompleteEvent

        //-------------------------//
        public UnityEvent GetOnCompleteEvent()
        //-------------------------//
        {

            return this.onCompleteOrLoop;

        } //END GetOnCompleteEvent


        //-------------------------//
        public bool DoesTweenExist()
        //-------------------------//
        {
            //If the tween exists
            return (tween != null);

        } //END DoesTweenExist

        //-------------------------//
        public bool IsPaused()
        //-------------------------//
        {
            //If the tween exists and is paused
            if( tween != null )
            {
                return tween.IsPaused();
            }

            return false;

        } //END IsPaused

        //-------------------------//
        public void Stop()
        //-------------------------//
        {

            if( tween != null )
            {
                tween.Stop();
                tween.PrepareForDestroy();
                Destroy( tween.gameObject );
            }

        } //END Stop


        //--------------------------//
        public virtual void Force( TweenValue tweenValue )
        //--------------------------//
        {

            Stop();
            
        } //END Force


        public virtual void PlayDefault() { Play( TweenValue.Default ); }
        public virtual void PlayHide() { Play( TweenValue.Hide ); }
        public virtual void PlayShow() { Play( TweenValue.Show ); }
        public virtual void PlayHold() { Play( TweenValue.Hold ); }
        public virtual void PlayHoverEnter() { Play( TweenValue.HoverEnter ); }
        public virtual void PlayHoverExit() { Play( TweenValue.HoverExit ); }
        public virtual void PlayPressed() { Play( TweenValue.Pressed ); }
        public virtual void PlayEnabled() { Play( TweenValue.Enabled ); }
        public virtual void PlayDisabled() { Play( TweenValue.Disabled ); }

        //--------------------------//
        public virtual void Play( TweenValue tweenValue )
        //--------------------------//
        {
            Stop();
            
            CallTween( tweenValue );

        } //END Play

        //------------------------------------//
        public virtual void Play( TweenValue tweenValue, UnityEvent CallOnComplete )
        //------------------------------------//
        {
            Stop();

            SetOnCompleteEvent( CallOnComplete );

            CallTween( tweenValue );

        } //END Play


        //------------------------------------//
        protected virtual void CallTween( TweenValue tweenValue )
        //------------------------------------//
        {

        } //END CallTween
        

    } //END Class

} //END Namespace