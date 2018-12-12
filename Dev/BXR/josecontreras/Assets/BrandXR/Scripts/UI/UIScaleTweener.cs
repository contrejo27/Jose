using UnityEngine;
using UnityEngine.UI;
using Sirenix.OdinInspector;
using UnityEngine.Events;

namespace BrandXR
{
    public class UIScaleTweener: UITweener
    {

        private Transform transformToScale;

        public enum ScaleBy
        {
            Multiplier,
            Explicit
        }
        public ScaleBy scaleBy = ScaleBy.Multiplier;

        [ShowIf("scaleBy", ScaleBy.Multiplier )]
        private float multiplier_Default = 1f;
        [ShowIf( "scaleBy", ScaleBy.Multiplier )]
        public float multiplier_Hide = .5f;
        [ShowIf( "scaleBy", ScaleBy.Multiplier )]
        public float multiplier_Show = 2f;
        [ShowIf( "scaleBy", ScaleBy.Multiplier )]
        public float multiplier_HoverEnter = 1.5f;
        [ShowIf( "scaleBy", ScaleBy.Multiplier )]
        public float multiplier_HoverExit = 1f;
        [ShowIf( "scaleBy", ScaleBy.Multiplier )]
        public float multiplier_Pressed = 2f;
        [ShowIf( "scaleBy", ScaleBy.Multiplier )]
        public float multiplier_Hold = 2f;
        [ShowIf( "scaleBy", ScaleBy.Multiplier )]
        public float multiplier_Enabled = 1f;
        [ShowIf( "scaleBy", ScaleBy.Multiplier )]
        public float multiplier_Disabled = 1f;

        [ShowIf( "scaleBy", ScaleBy.Explicit )]
        public Vector3 explicitScale_Default = Vector3.one;
        [ShowIf( "scaleBy", ScaleBy.Explicit )]
        public Vector3 explicitScale_Hide = new Vector3( .5f, .5f, .5f );
        [ShowIf( "scaleBy", ScaleBy.Explicit )]
        public Vector3 explicitScale_Show = new Vector3( 2f, 2f, 2f );
        [ShowIf( "scaleBy", ScaleBy.Explicit )]
        public Vector3 explicitScale_HoverEnter = new Vector3( 1.5f, 1.5f, 1.5f );
        [ShowIf( "scaleBy", ScaleBy.Explicit )]
        public Vector3 explicitScale_HoverExit = Vector3.one;
        [ShowIf( "scaleBy", ScaleBy.Explicit )]
        public Vector3 explicitScale_Pressed = new Vector3( 2f, 2f, 2f );
        [ShowIf( "scaleBy", ScaleBy.Explicit )]
        public Vector3 explicitScale_Hold = new Vector3( 2f, 2f, 2f );
        [ShowIf( "scaleBy", ScaleBy.Explicit )]
        public Vector3 explicitScale_Enabled = Vector3.one;
        [ShowIf( "scaleBy", ScaleBy.Explicit )]
        public Vector3 explicitScale_Disabled = Vector3.one;

        //-----------------------------------//
        protected override void FindInitialValues()
        //-----------------------------------//
        {

            if( transformToScale == null )
            {
                transformToScale = this.GetComponent<Transform>();
            }

        } //END FindInitialValues

        //------------------------------------------//
        public Transform GetTransform()
        //------------------------------------------//
        {

            if( transformToScale == null ) { FindInitialValues(); }

            return transformToScale;

        } //END GetTransform

        //------------------------------------------//
        protected override void ForceDefaults()
        //------------------------------------------//
        {
            if( transformToScale == null ) { FindInitialValues(); }

            if( transformToScale != null )
            {
                SetScale( transformToScale.localScale, TweenValue.Default );
            }

        } //END ForceDefaults

        //-----------------------------------//
        public void Force( Vector3 scale, UITweener.TweenValue tweenValue )
        //-----------------------------------//
        {
            SetScale( scale, tweenValue );
            Force( tweenValue );

        } //END Force

        //-----------------------------------//
        public override void Force( UITweener.TweenValue tweenValue )
        //-----------------------------------//
        {
            base.Force( tweenValue );

            SetFinishScaleBasedOnMultiplier();
            
            if( transformToScale == null ) { FindInitialValues(); }

            if( transformToScale != null )
            {
                transformToScale.localScale = GetEndScale( tweenValue );
            }

        } //END Force

        //------------------------------------//
        public void Play( TweenValue tweenValue, Vector3 scale, float tweenSpeed, float delay, EaseCurve.EaseType easeCurve, UnityEvent CallOnComplete )
        //------------------------------------//
        {
            SetScale( scale, tweenValue );
            SetTweenSpeed( tweenSpeed, tweenValue );
            SetDelay( delay, tweenValue );
            SetEaseType( easeCurve, tweenValue );

            base.Play( tweenValue, CallOnComplete );

        } //END Play


        //------------------------------------//
        protected override void CallTween( UITweener.TweenValue tweenValue )
        //------------------------------------//
        {
            base.CallTween( tweenValue );

            SetFinishScaleBasedOnMultiplier();

            Vector3 startScale = GetStartScale();
            Vector3 endScale = GetEndScale( tweenValue );
            float tweenSpeed = GetTweenSpeed( tweenValue );
            EaseCurve.EaseType easeType = GetEaseType( tweenValue );
            float delay = GetDelay( tweenValue );

            if( transformToScale != null )
            {
                tween = transformToScale.Scale( endScale, tweenSpeed, easeType, startScale, delay, false, onCompleteOrLoop );
            }

        } //END CallTween


        //-----------------------------------//
        public void SetScale( Vector3 scale, UITweener.TweenValue tweenValue )
        //-----------------------------------//
        {

            if( tweenValue == UITweener.TweenValue.Default ) { explicitScale_Default = scale; }
            else if( tweenValue == UITweener.TweenValue.Hide ) { explicitScale_Hide = scale; }
            else if( tweenValue == UITweener.TweenValue.Show ) { explicitScale_Show = scale; }
            else if( tweenValue == UITweener.TweenValue.HoverEnter ) { explicitScale_HoverEnter = scale; }
            else if( tweenValue == UITweener.TweenValue.HoverExit ) { explicitScale_HoverExit = scale; }
            else if( tweenValue == UITweener.TweenValue.Pressed ) { explicitScale_Pressed = scale; }
            else if( tweenValue == UITweener.TweenValue.Hold ) { explicitScale_Hold = scale; }
            else if( tweenValue == UITweener.TweenValue.Enabled ) { explicitScale_Enabled = scale; }
            else if( tweenValue == UITweener.TweenValue.Disabled ) { explicitScale_Disabled = scale; }
        } //END SetScale

        
        //-----------------------------------//
        public Vector3 GetStartScale( )
        //-----------------------------------//
        {
            if( transformToScale == null ) { FindInitialValues(); }

            if( transformToScale != null )
            {
                return transformToScale.localScale;
            }
            else
            {
                return Vector3.zero;
            }

        } //END GetStartScale

        //-----------------------------------//
        public Vector3 GetEndScale( TweenValue tweenValue )
        //-----------------------------------//
        {
            if( tweenValue == UITweener.TweenValue.Default ) { return explicitScale_Default; }
            else if( tweenValue == UITweener.TweenValue.Hide ) { return explicitScale_Hide; }
            else if( tweenValue == UITweener.TweenValue.Show ) { return explicitScale_Show; }
            else if( tweenValue == UITweener.TweenValue.HoverEnter ) { return explicitScale_HoverEnter; }
            else if( tweenValue == UITweener.TweenValue.HoverExit ) { return explicitScale_HoverExit; }
            else if( tweenValue == UITweener.TweenValue.Pressed ) { return explicitScale_Pressed; }
            else if( tweenValue == UITweener.TweenValue.Hold ) { return explicitScale_Hold; }
            else if( tweenValue == UITweener.TweenValue.Enabled ) { return explicitScale_Enabled; }
            else if( tweenValue == UITweener.TweenValue.Disabled ) { return explicitScale_Disabled; }

            return explicitScale_Default;

        } //END GetEndScale


        //-------------------------//
        public void SetMultiplier( float multiplier, TweenValue tweenValue )
        //-------------------------//
        {

            if( tweenValue == TweenValue.Default ) { multiplier_Default = multiplier; }
            else if( tweenValue == TweenValue.Hide ) { multiplier_Hide = multiplier; }
            else if( tweenValue == TweenValue.Show ) { multiplier_Show = multiplier; }
            else if( tweenValue == TweenValue.HoverEnter ) { multiplier_HoverEnter = multiplier; }
            else if( tweenValue == TweenValue.HoverExit ) { multiplier_HoverExit = multiplier; }
            else if( tweenValue == TweenValue.Pressed ) { multiplier_Pressed = multiplier; }
            else if( tweenValue == TweenValue.Hold ) { multiplier_Hold = multiplier; }
            else if( tweenValue == TweenValue.Enabled ) { multiplier_Enabled = multiplier; }
            else if( tweenValue == TweenValue.Disabled ) { multiplier_Disabled = multiplier; }

        } //END SetMultiplier

        //-------------------------//
        public float GetMultiplier( TweenValue tweenValue )
        //-------------------------//
        {

            if( tweenValue == TweenValue.Default ) { return multiplier_Default; }
            else if( tweenValue == TweenValue.Hide ) { return multiplier_Hide; }
            else if( tweenValue == TweenValue.Show ) { return multiplier_Show; }
            else if( tweenValue == TweenValue.HoverEnter ) { return multiplier_HoverEnter; }
            else if( tweenValue == TweenValue.HoverExit ) { return multiplier_HoverExit; }
            else if( tweenValue == TweenValue.Pressed ) { return multiplier_Pressed; }
            else if( tweenValue == TweenValue.Hold ) { return multiplier_Hold; }
            else if( tweenValue == TweenValue.Enabled ) { return multiplier_Enabled; }
            else if( tweenValue == TweenValue.Disabled ) { return multiplier_Disabled; }

            return multiplier_Default;

        } //END GetMultiplier


        //-------------------------//
        private void SetFinishScaleBasedOnMultiplier()
        //-------------------------//
        {

            if( scaleBy == ScaleBy.Multiplier )
            {
                explicitScale_Hide = explicitScale_Default * multiplier_Hide;
                explicitScale_Show = explicitScale_Default * multiplier_Show;
                explicitScale_HoverEnter = explicitScale_Default * multiplier_HoverEnter;
                explicitScale_HoverExit = explicitScale_Default * multiplier_HoverExit;
                explicitScale_Pressed = explicitScale_Default * multiplier_Pressed;
                explicitScale_Hold = explicitScale_Default * multiplier_Hold;
                explicitScale_Enabled = explicitScale_Default * multiplier_Enabled;
                explicitScale_Disabled = explicitScale_Default * multiplier_Disabled;
            }

        } //END SetFinishScaleBasedOnMultiplier

    } //END Class

}//END Namespace