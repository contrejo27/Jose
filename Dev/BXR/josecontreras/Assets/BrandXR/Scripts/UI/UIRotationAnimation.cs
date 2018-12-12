using UnityEngine;
using UnityEngine.Events;

namespace BrandXR
{
    public class UIRotationAnimation: UITweenAnimation
    {

        public Transform[] transformsToAnimate;

        public Vector3[] rotationsToAnimateTo;

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

            if( rotationsToAnimateTo == null )
            {
                rotationsToAnimateTo = new Vector3[] { Vector3.zero, Vector3.zero };
            }

        } //END ForceDefaults

        //--------------------------------------------//
        public override void Play()
        //--------------------------------------------//
        {

            base.Play();

            if( currentAnimationCounter >= rotationsToAnimateTo.Length )
            {
                //Debug.Log( "Resetting animation counter... because counter( " + currentAnimationCounter + " ) >= rotations.Length( " + rotationsToAnimateTo.Length + " )" );
                currentAnimationCounter = 0;
            }

            UnityEvent _event = new UnityEvent();
            _event.AddListener( Play );

            for( int i = 0; i < transformsToAnimate.Length; i++ )
            {
                bxrTween tween = transformsToAnimate[ i ].Rotate( rotationsToAnimateTo[ currentAnimationCounter ], animationSpeeds[ currentAnimationCounter ], easeTypes[ currentAnimationCounter ], transformsToAnimate[ i ].localEulerAngles, delays[ currentAnimationCounter ], false, _event );
                tween.PreventDestroyOnComplete();
                tweens.Add( tween );
            }

            //Debug.Log( "Play() counter = " + currentAnimationCounter + ", rotations.Length = " + rotationsToAnimateTo.Length + ", rotation = " + rotationsToAnimateTo[ currentAnimationCounter ] );

            currentAnimationCounter++;

        } //END Play


    } //END Class

} //END Namespace