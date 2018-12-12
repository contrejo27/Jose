using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace BrandXR
{
    public class GazeCollider : MonoBehaviour
    {

        public bool AllowGazeFill = false;
        public bool AllowGazeGrow = true;

        //-----------------------------------//
        //Used in VRGazeInputModule.cs Process()
        public static bool ShouldGazeCursorFill( GameObject raycastTarget )
        //-----------------------------------//
        {

            if( raycastTarget != null && raycastTarget.GetComponent<GazeCollider>() != null )
            {
                return raycastTarget.GetComponent<GazeCollider>().AllowGazeFill;
            }

            return true;

        } //END ShouldGazeCursorFill







        //-----------------------------------//
        //Used in VRGazeInputModule.cs UpdateReticle()
        public static bool ShouldGazeCursorGrow( GameObject raycastTarget )
        //-----------------------------------//
        {
            //Debug.Log( "ShouldGazeCursorGrow() target = " + raycastTarget.name );

            if( raycastTarget != null && raycastTarget.GetComponent<GazeCollider>() != null )
            {
                //Debug.Log( "ShouldGazeCursorGrow() AllowGazeGrow = " + raycastTarget.GetComponent<GazeCollider>().AllowGazeGrow );
                return raycastTarget.GetComponent<GazeCollider>().AllowGazeGrow;
            }

            return true;

        } //END ShouldGazeCursorGrow

    } //END Class

} //END Namespace
