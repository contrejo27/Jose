using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;

namespace BrandXR
{
    public class UITweenAnimationManager<UIT>: MonoBehaviour
    where UIT : UITweenAnimation
    {

        public List<UITweenAnimation> animations;

        //-------------------------------------------//
        public void Play()
        //-------------------------------------------//
        {

            foreach( UITweenAnimation animation in animations )
            {
                animation.Play();
            }

        } //END Play

        //-------------------------------------------//
        public void Stop()
        //-------------------------------------------//
        {

            foreach( UITweenAnimation animation in animations )
            {
                animation.Stop();
            }

        } //END Stop


    } //END Class

} //END Namespace