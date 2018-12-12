/* TouchInputListener.cs
 * 
 * Listenes for and passes touch events to the XRMeshTrigger (on 3D objects) and EventTrigger (on 2D objects) components
 */
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Sirenix.OdinInspector;
using System.Linq;

namespace BrandXR
{
    public class TouchInputListener: MonoBehaviour
    {

        [TitleGroup( "Touch Input Listener", "Listenes for and passes touch events to the XRMeshTrigger (on 3D objects) and EventTrigger (on 2D objects) components" )]
        public bool listenForTouchInput = true;
        
        private bool showDebug = true;

        //---------------------------------------//
        public void Update()
        //---------------------------------------//
        {
            
            //Check for touch input every update
            if( Input.touchSupported && listenForTouchInput )
            {

                if( Input.touches != null && Input.touches.Length > 0 && Camera.main != null )
                {
                    if( showDebug ) Debug.Log( "Touch event is occuring, about to iterate through list of touches" );

                    List<Touch> myTouches = Input.touches.ToList();

                    for( int i = 0; i < myTouches.Count; i++ )
                    {
                        if( myTouches[ i ].phase == TouchPhase.Began )
                        {
                            //Construct a ray to convert from the normalized touch position to our world space
                            Ray ray = Camera.main.ScreenPointToRay( myTouches[ i ].position );

                            if( showDebug ) Debug.Log( "Touch at " + myTouches[ i ].position );

                            //Store what we collide with
                            RaycastHit hit = new RaycastHit();

                            //Is this touch happening over an object that can respond to it?
                            if( Physics.Raycast( ray, out hit, float.MaxValue ) )
                            {
                                if( showDebug ) Debug.Log( "Touched object '" + hit.transform.gameObject.name + "'" );

                                if( hit.transform.GetComponent<XRMeshTrigger>() != null )
                                {
                                    if( showDebug ) Debug.Log( "Found XRMeshTrigger on " + hit.transform.gameObject.name );

                                    //SendMessageToTrigger( hit, myTouches[ i ], hit.transform.GetComponent<XRMeshTrigger>() );
                                    hit.transform.GetComponent<XRMeshTrigger>().OnMouseDown();
                                }
                                else
                                {
                                    if( showDebug ) Debug.Log( "Could not locate XRMeshTrigger on " + hit.transform.gameObject.name );
                                }
                            }
                            else
                            {
                                if( showDebug ) Debug.Log( "Did not find object at raycast position " + myTouches[ i ].position );
                            }
                        }
                        else
                        {
                            if( showDebug ) Debug.Log( "Touch occured but phase is " + myTouches[ i ].phase );
                        }
                    }

                }
                
            }
            
        } //END Update

        //--------------------------------------------//
        private void SendMessageToTrigger( RaycastHit hit, Touch touch, XRMeshTrigger trigger )
        //--------------------------------------------//
        {
            
            if( touch.phase == TouchPhase.Began )
            {
                trigger.OnMouseDown();
            }

        } //END SendMessageToTrigger


    } //END TouchInputListener

} //END Namespace