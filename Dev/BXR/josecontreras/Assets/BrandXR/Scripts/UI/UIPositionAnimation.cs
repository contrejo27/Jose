using UnityEngine;
using UnityEngine.Events;

namespace BrandXR
{
    public class UIPositionAnimation: UITweenAnimation
    {

        public Transform[] transformsToAnimate;

        public Vector3[] positionsToAnimateTo;

        //-----------------------------------//
        protected override void FindInitialValues()
        //-----------------------------------//
        {

            if( transformsToAnimate == null )
            {
                transformsToAnimate = new Transform[] { this.transform };
            }

        } //END FindInitialValues

        //-----------------------------------//
        protected override void ForceDefaults()
        //-----------------------------------//
        {

            base.ForceDefaults();

            if( positionsToAnimateTo == null )
            {
                positionsToAnimateTo = new Vector3[] { Vector3.zero, Vector3.zero };
            }

        } //END ForceDefaults

        //--------------------------------------------//
        public override void Play()
        //--------------------------------------------//
        {

            base.Play();

            if( currentAnimationCounter >= positionsToAnimateTo.Length )
            {
                currentAnimationCounter = 0;
            }

            UnityEvent _event = new UnityEvent();
            _event.AddListener( Play );

            foreach( Transform _transform in transformsToAnimate )
            {
                bxrTween tween = _transform.Move( positionsToAnimateTo[ currentAnimationCounter ], animationSpeeds[ currentAnimationCounter ], easeTypes[ currentAnimationCounter ], _transform.localPosition, bxrTweenPosition.LocalOrWorldSpace.Local, 0f, false, _event );
                tween.PreventDestroyOnComplete();
                tweens.Add( tween );
            }

            currentAnimationCounter++;

        } //END Play


    } //END Class

} //END Namespace