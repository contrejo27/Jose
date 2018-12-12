using UnityEngine;
using Sirenix.OdinInspector;
using UnityEngine.Events;

namespace BrandXR
{
    public class UIRotationTweener: UITweener
    {

        public Transform transformToRotate;

        [FoldoutGroup("Rotations")]
        public Vector3 rotation_Default = Vector3.zero;
        [FoldoutGroup( "Rotations" )]
        public Vector3 rotation_Hide = Vector3.zero;
        [FoldoutGroup( "Rotations" )]
        public Vector3 rotation_Show = Vector3.zero;
        [FoldoutGroup( "Rotations" )]
        public Vector3 rotation_HoverEnter = Vector3.zero;
        [FoldoutGroup( "Rotations" )]
        public Vector3 rotation_HoverExit = Vector3.zero;
        [FoldoutGroup( "Rotations" )]
        public Vector3 rotation_Pressed = Vector3.zero;
        [FoldoutGroup( "Rotations" )]
        public Vector3 rotation_Hold = Vector3.zero;
        [FoldoutGroup( "Rotations" )]
        public Vector3 rotation_Enabled = Vector3.zero;
        [FoldoutGroup( "Rotations" )]
        public Vector3 rotation_Disabled = Vector3.zero;


        //-----------------------------------//
        protected override void FindInitialValues()
        //-----------------------------------//
        {

            if( transformToRotate == null )
            {
                transformToRotate = this.transform;
            }

        } //END FindInitialValues

        //------------------------------------------//
        protected override void ForceDefaults()
        //------------------------------------------//
        {
            SetRotation( transformToRotate.localEulerAngles, TweenValue.Default );

        } //END ForceDefaults




        //-----------------------------------//
        public void SetRotation( Vector3 rotation, TweenValue tweenValue )
        //-----------------------------------//
        {

            if( tweenValue == TweenValue.Default ) { rotation_Default = rotation; }
            else if( tweenValue == TweenValue.Hide ) { rotation_Hide = rotation; }
            else if( tweenValue == TweenValue.Show ) { rotation_Show = rotation; }
            else if( tweenValue == TweenValue.HoverEnter ) { rotation_HoverEnter = rotation; }
            else if( tweenValue == TweenValue.HoverExit ) { rotation_HoverExit = rotation; }
            else if( tweenValue == TweenValue.Pressed ) { rotation_Pressed = rotation; }
            else if( tweenValue == TweenValue.Hold ) { rotation_Hold = rotation; }
            else if( tweenValue == TweenValue.Enabled ) { rotation_Enabled = rotation; }
            else if( tweenValue == TweenValue.Disabled ) { rotation_Disabled = rotation; }

        } //END SetRotation

        //-----------------------------------//
        public Vector3 GetRotation( TweenValue tweenValue )
        //-----------------------------------//
        {

            if( tweenValue == TweenValue.Default ) { return rotation_Default; }
            else if( tweenValue == TweenValue.Hide ) { return rotation_Hide; }
            else if( tweenValue == TweenValue.Show ) { return rotation_Show; }
            else if( tweenValue == TweenValue.HoverEnter ) { return rotation_HoverEnter; }
            else if( tweenValue == TweenValue.HoverExit ) { return rotation_HoverExit; }
            else if( tweenValue == TweenValue.Pressed ) { return rotation_Pressed; }
            else if( tweenValue == TweenValue.Hold ) { return rotation_Hold; }
            else if( tweenValue == TweenValue.Enabled ) { return rotation_Enabled; }
            else if( tweenValue == TweenValue.Disabled ) { return rotation_Disabled; }

            return rotation_Default;

        } //END GetRotation

        //-----------------------------------//
        public void Force( Vector3 rotation, TweenValue tweenValue )
        //-----------------------------------//
        {

            SetRotation( rotation, tweenValue );
            Force( tweenValue );

        } //END Force

        //-----------------------------------//
        public override void Force( TweenValue tweenValue )
        //-----------------------------------//
        {
            base.Force( tweenValue );
            transformToRotate.localEulerAngles = GetRotation( tweenValue );

        } //END Force


        //------------------------------------//
        public void Play( TweenValue tweenValue, Vector3 rotation, float tweenSpeed, float delay, EaseCurve.EaseType easeCurve, UnityEvent CallOnComplete )
        //------------------------------------//
        {
            SetRotation( rotation, tweenValue );
            SetTweenSpeed( tweenSpeed, tweenValue );
            SetDelay( delay, tweenValue );
            SetEaseType( easeCurve, tweenValue );

            base.Play( tweenValue, CallOnComplete );

        } //END Play

        //------------------------------------//
        protected override void CallTween( TweenValue tweenValue )
        //------------------------------------//
        {
            tween = transformToRotate.Rotate( GetRotation( tweenValue ), GetTweenSpeed( tweenValue ), GetEaseType( tweenValue ), transformToRotate.localEulerAngles, GetDelay( tweenValue ), false, onCompleteOrLoop );
            
        } //END CallTween


    } //END Class

} //END Namespace