/* MarkerInteraction.cs
 * Handles interaction between the player and custom markers on the map
 */

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Sirenix.OdinInspector;

namespace BrandXR
{
    public class MarkerInteraction : SerializedMonoBehaviour
    {
#pragma warning disable
        // Time between checking for new markers and distacances
        [SerializeField, BoxGroup("Time Between Checks")] private float timeBetweenDistanceCheck;
        [SerializeField, BoxGroup("Time Between Checks")] private float timeBetweenMarkerSearch;

        // Distance the marker must be within from the player to trigger the event 
        [SerializeField] private float distToTriggerEvent;

        // List of markers currently on the map 
        [SerializeField, ReadOnly] private CustomMarker[] markers = new CustomMarker[0];
#pragma warning restore

        [SerializeField] private bool showDebug = false;

        //------------------//
        private void Start()
        //------------------//
        {
            StartCoroutine(CoFindNewMarkers());

            StartCoroutine(CoCheckForMarkersInRange());

        } // END Start

        //------------------//
        private void Update()
        //------------------//
        {
            //Check for mouse input, if there is none then check for touch input
            if( !CheckForMouseInput() )
            {
                CheckForTouchInput();
            }

        } // END Update

        //----------------------------//
        private bool CheckForMouseInput()
        //----------------------------//
        {
            //Does this device suppport mouse input and has the left mouse button been clicked?
            if( Input.mousePresent && Input.GetMouseButtonDown( 0 ) )
            {
                //Is there an object with a CustomMarker.cs component attached at the position of the mouse click?
                Ray ray = Camera.main.ScreenPointToRay( Input.mousePosition );

                RaycastHit hit;

                if( Physics.Raycast( ray, out hit ) )
                {
                    //We clicked on an object with a CustomMarker.cs component, 
                    //let's check if we should send an event message out to other components!
                    if( hit.transform.GetComponent<CustomMarker>() )
                    {
                        CustomMarker marker = hit.transform.GetComponent<CustomMarker>();

                        //If all we need to do is click on the CustomMarker, then send an event out now if it's in range
                        if( marker.TriggerType == ETriggerType.OnClick )
                        {
                            if( showDebug ) { Debug.Log( "MarkerInteraction.cs Update() Mouse clicked on marker! TriggerType = " + marker.TriggerType + ", IsInRange = " + IsInRange( marker.transform ) ); }
                            marker.Interact( IsInRange( marker.transform ) );
                            return true;
                        }

                        //Otherwise if we require that the marker is within range of the player...
                        else if( marker.TriggerType == ETriggerType.InRangeClick )
                        {
                            if( IsInRange( marker.transform ) )
                            {
                                if( showDebug ) { Debug.Log( "MarkerInteraction.cs Update() Mouse clicked on marker and it's in range! TriggerType = " + marker.TriggerType ); }
                                marker.Interact( true );
                                return true;
                            }
                            else
                            {
                                if( showDebug ) { Debug.Log( "MarkerInteraction.cs Update() Mouse clicked on marker but it's not in range! TriggerType = " + marker.TriggerType + ", IsInRange = " + IsInRange( marker.transform ) ); }
                            }
                        }
                    }
                }

            }

            return false;

        } //END CheckForMouseInput

        //----------------------------//
        private bool CheckForTouchInput()
        //----------------------------//
        {

            //If this device supports Touch Input and if there is currently a touch event happening
            if( Input.touchSupported && Input.touchCount > 0 )
            {
                //Is this touch input happening on a CustomMarker?
                Ray ray = Camera.main.ScreenPointToRay( Input.GetTouch( 0 ).position );

                RaycastHit hit;

                if( Physics.Raycast( ray, out hit ) )
                {
                    if( hit.transform.GetComponent<CustomMarker>() )
                    {

                        CustomMarker marker = hit.transform.GetComponent<CustomMarker>();

                        if( marker.TriggerType == ETriggerType.OnClick )
                        {
                            if( showDebug ) { Debug.Log( "MarkerInteraction.cs CheckForTouchInput() Tapped on marker! TriggerType = " + marker.TriggerType + ", IsInRange = " + IsInRange( marker.transform ) ); }
                            marker.Interact( IsInRange( marker.transform ) );
                            return true;
                        }
                        else if( marker.TriggerType == ETriggerType.InRangeClick )
                        {
                            if( IsInRange( marker.transform ) )
                            {
                                if( showDebug ) { Debug.Log( "MarkerInteraction.cs CheckForTouchInput() Tapped on marker and it's in range! TriggerType = " + marker.TriggerType ); }
                                marker.Interact( true );
                                return true;
                            }
                            else
                            {
                                if( showDebug ) { Debug.Log( "MarkerInteraction.cs CheckForTouchInput() Tapped on marker but it's not in range! TriggerType = " + marker.TriggerType ); }
                            }

                        }
                    }
                }
            }

            return false;

        } //END CheckForTouchInput

        //----------------------------//
        /// <summary>
        /// Updates the array of found markers every [timeBetweenMarkerSearch] seconds
        /// </summary>
        /// <returns></returns>
        IEnumerator CoFindNewMarkers()
        //----------------------------//
        {
            while (true)
            {
                float timeToSearch = Time.time + timeBetweenMarkerSearch;

                while (Time.time < timeToSearch)
                {
                    yield return null;
                }

                markers = FindObjectsOfType<CustomMarker>();
            }

        } // END CoFindNewMarkers

        //-----------------------------------//
        /// <summary>
        /// Checks if any found markers are within range every [timeBetweenDistanceCheck] seconds
        /// If marker is within range and it's trigger type is InRange, trigger that marker's event
        /// </summary>
        /// <returns></returns>
        IEnumerator CoCheckForMarkersInRange()
        //-----------------------------------//
        {
            while (true)
            {
                float timeToSearch = Time.time + timeBetweenMarkerSearch;

                while (Time.time < timeToSearch)
                {
                    yield return null;
                }

                // Loop through all found markers 
                foreach (CustomMarker marker in markers)
                {
                    // if marker can be triggered by range and is currently in the player's range 
                    if (marker.TriggerType == ETriggerType.InRange && IsInRange(marker.transform))
                    {
                        marker.Interact(true);
                    }
                }
            }

        } // END CoCheckForMarkersInRange

        //--------------------------------------//
        private bool IsInRange(Transform marker)
        //--------------------------------------//
        {
            return Vector3.Distance(transform.position, marker.transform.position) <= distToTriggerEvent;

        } // END IsInRange

    } // END Class

} // END Namespace