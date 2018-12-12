/* LeanSelectUpdate.cs
 * 
 * If there is a LeanSelect component in the scene, then this script will tell it to continually update to look for LeanSelectable components
 * 
 * We do this so we can tell LeanSelectable components that they are being selected by Mouse/Touch input, which is used to determine the eligibility of certain gestures
 */
using UnityEngine;
using System.Collections;

# if LEANTOUCH
using Lean.Touch;
#endif

namespace BrandXR
{
    public class LeanSelectUpdate: MonoBehaviour
    {

#if LEANTOUCH
        private LeanSelect leanSelect;
#endif

        //-----------------------------------//
        public void Start()
        //-----------------------------------//
        {

            GetLeanSelectComponent();
            
        } //END Start

        //-----------------------------------//
        public void Update()
        //-----------------------------------//
        {
            GetLeanSelectComponent();

            UpdateLeanSelectWithFingerPositions();
            
        } //END Update

        //----------------------------------//
        private void UpdateLeanSelectWithFingerPositions()
        //----------------------------------//
        {

#if LEANTOUCH
            if( LeanTouch.Fingers != null && LeanTouch.Fingers.Count > 0 )
            {
                foreach( LeanFinger finger in LeanTouch.Fingers )
                {
                    if( finger != null )
                    {
                        if( leanSelect != null )
                        {
                            leanSelect.SelectScreenPosition( finger );
                        }
                        
                    }
                }
            }
#endif

        } //END UpdateLeanSelectWithFingerPositions

        //-----------------------------------//
        private void GetLeanSelectComponent()
        //-----------------------------------//
        {

#if LEANTOUCH
            if( leanSelect == null )
            {
                if( GameObject.FindObjectOfType<LeanSelect>() != null )
                {
                    leanSelect = GameObject.FindObjectOfType<LeanSelect>();
                }
            }
#endif

        } //END GetLeanSelectComponent



    } //END Class

} //END namespace