using System.Collections.Generic;
using UnityEngine;

namespace BrandXR
{
    public class UITweenAnimation: MonoBehaviour
    {

        public float[] animationSpeeds;

        public EaseCurve.EaseType[] easeTypes;

        public float[] delays;

        protected int currentAnimationCounter = 0;

        public List<bxrTween> tweens = new List<bxrTween>();

        public bool PlayOnAwake = true;

        //------------------------------//
        public void Awake()
        //------------------------------//
        {
            FindInitialValues();
            ForceDefaults();

            if( PlayOnAwake )
            {
                Play();
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

            if( animationSpeeds == null )
            {
                animationSpeeds = new float[] { 1f, 1f };
            }

            if( easeTypes == null )
            {
                easeTypes = new EaseCurve.EaseType[] { EaseCurve.EaseType.Linear, EaseCurve.EaseType.Linear };
            }

            if( delays == null )
            {
                delays = new float[] { 1f, 1f };
            }

        } //END ForceDefaults

        //--------------------------------------------//
        public virtual void Play()
        //--------------------------------------------//
        {

            //Kill();

        } //END Play

        //--------------------------------------------//
        public void Stop()
        //--------------------------------------------//
        {

            currentAnimationCounter = 0;

            Kill();

        } //END Stop

        //--------------------------------------------//
        private void Kill()
        //--------------------------------------------//
        {

            if( tweens != null && tweens.Count > 0 )
            {
                //Loop through the tween in reverse and remove them
                for( int i = tweens.Count - 1; i >= 0; i-- )
                {
                    if( tweens[i] != null )
                    {
                        bxrTween tween = tweens[ i ];
                        tweens.Remove( tween );
                        tween.Kill();
                    }
                }
            }

        } //END Kill


    } //END Class

} //END Namespace