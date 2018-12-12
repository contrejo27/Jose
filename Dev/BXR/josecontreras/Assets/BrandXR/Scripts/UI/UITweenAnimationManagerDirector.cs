using UnityEngine;
using System.Collections.Generic;

namespace BrandXR
{
    public class UITweenAnimationManagerDirector: MonoBehaviour
    {

        public List<UIColorAnimation> colorAnimations;
        public List<UIPositionAnimation> positionAnimations;
        public List<UIRotationAnimation> rotationAnimations;
        public List<UIScaleAnimation> scaleAnimations;

        //-------------------------------------------//
        public void Play()
        //-------------------------------------------//
        {

            foreach( UIColorAnimation animationManager in colorAnimations )
            {
                animationManager.Play();
            }
            foreach( UIPositionAnimation animationManager in positionAnimations )
            {
                animationManager.Play();
            }
            foreach( UIRotationAnimation animationManager in rotationAnimations )
            {
                animationManager.Play();
            }
            foreach( UIScaleAnimation animationManager in scaleAnimations )
            {
                animationManager.Play();
            }

        } //END Play

        //-------------------------------------------//
        public void Stop()
        //-------------------------------------------//
        {

            foreach( UIColorAnimation animation in colorAnimations )
            {
                animation.Stop();
            }
            foreach( UIPositionAnimation animationManager in positionAnimations )
            {
                animationManager.Stop();
            }
            foreach( UIRotationAnimation animationManager in rotationAnimations )
            {
                animationManager.Stop();
            }
            foreach( UIScaleAnimation animationManager in scaleAnimations )
            {
                animationManager.Stop();
            }

        } //END Stop


    } //END Class

} //END Namespace