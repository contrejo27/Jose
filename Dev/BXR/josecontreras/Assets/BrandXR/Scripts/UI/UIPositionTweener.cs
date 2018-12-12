using UnityEngine;
using UnityEngine.Events;
using Sirenix.OdinInspector;

namespace BrandXR
{
    public class UIPositionTweener: UITweener
    {

        public Transform transformToMove;

        [FoldoutGroup("Positions")]
        public Vector3 position_Default = Vector3.zero;
        [FoldoutGroup( "Positions" )]
        public Vector3 position_Hide = Vector3.zero;
        [FoldoutGroup( "Positions" )]
        public Vector3 position_Show = Vector3.zero;
        [FoldoutGroup( "Positions" )]
        public Vector3 position_HoverEnter = Vector3.zero;
        [FoldoutGroup( "Positions" )]
        public Vector3 position_HoverExit = Vector3.zero;
        [FoldoutGroup( "Positions" )]
        public Vector3 position_Pressed = Vector3.zero;
        [FoldoutGroup( "Positions" )]
        public Vector3 position_Hold = Vector3.zero;
        [FoldoutGroup( "Positions" )]
        public Vector3 position_Enabled = Vector3.zero;
        [FoldoutGroup( "Positions" )]
        public Vector3 position_Disabled = Vector3.zero;

        //-----------------------------------//
        protected override void FindInitialValues()
        //-----------------------------------//
        {

            if( transformToMove == null )
            {
                transformToMove = this.transform;
            }

        } //END FindInitialValues

        //------------------------------------------//
        protected override void ForceDefaults()
        //------------------------------------------//
        {
            SetPosition( transformToMove.localPosition, TweenValue.Default );

        } //END ForceDefaults




        //-----------------------------------//
        public void SetPosition( Vector3 position, TweenValue tweenValue )
        //-----------------------------------//
        {

            if( tweenValue == TweenValue.Default ) { position_Default = position; }
            else if( tweenValue == TweenValue.Hide ) { position_Hide = position; }
            else if( tweenValue == TweenValue.Show ) { position_Show = position; }
            else if( tweenValue == TweenValue.HoverEnter ) { position_HoverEnter = position; }
            else if( tweenValue == TweenValue.HoverExit ) { position_HoverExit = position; }
            else if( tweenValue == TweenValue.Pressed ) { position_Pressed = position; }
            else if( tweenValue == TweenValue.Hold ) { position_Hold = position; }
            else if( tweenValue == TweenValue.Enabled ) { position_Enabled = position; }
            else if( tweenValue == TweenValue.Disabled ) { position_Disabled = position; }

        } //END SetPosition

        //-----------------------------------//
        public Vector3 GetPosition( TweenValue tweenValue )
        //-----------------------------------//
        {

            if( tweenValue == TweenValue.Default ) { return position_Default; }
            else if( tweenValue == TweenValue.Hide ) { return position_Hide; }
            else if( tweenValue == TweenValue.Show ) { return position_Show; }
            else if( tweenValue == TweenValue.HoverEnter ) { return position_HoverEnter; }
            else if( tweenValue == TweenValue.HoverExit ) { return position_HoverExit; }
            else if( tweenValue == TweenValue.Pressed ) { return position_Pressed; }
            else if( tweenValue == TweenValue.Hold ) { return position_Hold; }
            else if( tweenValue == TweenValue.Enabled ) { return position_Enabled; }
            else if( tweenValue == TweenValue.Disabled ) { return position_Disabled; }

            return position_Default;

        } //END GetPosition

        //-----------------------------------//
        public void Force( Vector3 position, TweenValue tweenValue )
        //-----------------------------------//
        {

            SetPosition( position, tweenValue );
            Force( tweenValue );

        } //END Force

        //-----------------------------------//
        public override void Force( TweenValue tweenValue )
        //-----------------------------------//
        {
            base.Force( tweenValue );
            transformToMove.localPosition = GetPosition( tweenValue );

        } //END Force


        //------------------------------------//
        public void Play( TweenValue tweenValue, Vector3 position, float tweenSpeed, float delay, EaseCurve.EaseType easeCurve, UnityEvent onCompleteOrLoop )
        //------------------------------------//
        {
            SetPosition( position, tweenValue );
            SetTweenSpeed( tweenSpeed, tweenValue );
            SetDelay( delay, tweenValue );
            SetEaseType( easeCurve, tweenValue );

            base.Play( tweenValue, onCompleteOrLoop );

        } //END Play

        //------------------------------------//
        protected override void CallTween( TweenValue tweenValue )
        //------------------------------------//
        {
            tween = transformToMove.Move( GetPosition( tweenValue ), GetTweenSpeed( tweenValue ), GetEaseType( tweenValue ), transformToMove.localPosition, bxrTweenPosition.LocalOrWorldSpace.Local, GetDelay( tweenValue ), false, onCompleteOrLoop );

        } //END CallTween


    } //END Class

} //END Namespace