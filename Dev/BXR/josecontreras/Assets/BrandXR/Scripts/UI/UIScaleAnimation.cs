using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using UnityEngine.Events;

namespace BrandXR
{
    public class UIScaleAnimation: UITweenAnimation
    {

        public Transform[] transformsToAnimate;

        public Vector3[] scalesToAnimateTo;

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

            if( scalesToAnimateTo == null )
            {
                scalesToAnimateTo = new Vector3[] { Vector3.one, Vector3.one };
            }

        } //END ForceDefaults

        //--------------------------------------------//
        public override void Play()
        //--------------------------------------------//
        {

            base.Play();

            if( currentAnimationCounter >= scalesToAnimateTo.Length )
            {
                currentAnimationCounter = 0;
            }

            UnityEvent _event = new UnityEvent();
            _event.AddListener( Play );

            if( transformsToAnimate != null )
            {
                foreach( Transform _transform in transformsToAnimate )
                {
                    if( _transform != null )
                    {
                        bxrTween tween = _transform.Scale( scalesToAnimateTo[ currentAnimationCounter ], animationSpeeds[ currentAnimationCounter ], easeTypes[ currentAnimationCounter ], _transform.localScale, delays[ currentAnimationCounter ], false, _event );
                        tween.PreventDestroyOnComplete();
                        tweens.Add( tween );
                    }
                }
            }
            

            currentAnimationCounter++;

        } //END Play


    } //END Class

} //END Namespace