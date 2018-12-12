/* XRTargetFloor.cs
 * 
 * Inherits from XRTargetBase.cs. Sends commands to blocks or nested BlockGroups when tracking is lost and found.
 * 
 * Must have an XRTarget component on the parent GameObject. There can only be one XRTargetBase attached to an XRTarget.
 * If more than one XRTargetBase is detected as being attached to the XRTarget, this script will attempt to destroy itself on Start()
 * 
 * Allows you to events when floor is tapped or when floor planes are being searched for, found, or lost
 * 
 */

using Sirenix.OdinInspector;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

#if UNITY_EDITOR
using UnityEditor;
#endif

#if VUFORIA
using Vuforia;
#endif

namespace BrandXR
{
#if VUFORIA
    public class XRTargetFloor: XRTargetBase, ITrackableEventHandler
#else
    public class XRTargetFloor: XRTargetBase
#endif
    {
        
        public override XRTargetBaseType_Vuforia GetXRTargetType() { return XRTargetBaseType_Vuforia.Floor; }

        [FoldoutGroup( "Readme" ), InfoBox( "Positions any childed blocks to the real world floor plane.\n\nSends a command to any childed blocks and nested BlockGroups when the user taps on the ground plane or an event occurs.\n\nYou can only have one XRTargetBase attached to one XRTarget parent. If you want to make an additional XRTargetBase, create another XRTarget first in a BlockView" )]
        public int dummy4 = 0;

        //Get the list of XRTechnologyTypes this XRTargetBase is supported by
        public override List<XRTechnologyManager.XRTechnologyType> GetXRTechnologyType()
        {
            return new List<XRTechnologyManager.XRTechnologyType>()
            {
                XRTechnologyManager.XRTechnologyType.Vuforia,
                XRTechnologyManager.XRTechnologyType.VuforiaFusion
            };
        }


#if VUFORIA
        [Space( 15f ), FoldoutGroup( "Hooks" )]
        public AnchorBehaviour anchorBehaviour;
        
        [Space( 15f ), FoldoutGroup( "Hooks" )]
        public ContentPositioningBehaviour contentPositioningBehaviour;
#endif

#region Floor Found Event
        //When the PlaneFinder component has 30 frames of the 'OnAutomaticHitTest' fire successfully, 
        //then we know it's safe to place an object or allow the user to press on the floor plane
        [Space( 15f ), FoldoutGroup( "Hooks" )]
        private bool isFloorPlaneReady = false;
        public bool IsFloorPlaneReady() { return isFloorPlaneReady; }

        [Space( 15f ), FoldoutGroup( "Hooks" )]
        private int floorPlaneFoundHits = 0;

        [Space( 15f ), FoldoutGroup( "Hooks")]
        public int requiredNumberOfFloorPlaneFoundHits = 30;

        [FoldoutGroup( "Floor Found Event" )]
        public bool sendCommandOnFloorFound = false;

        [ShowIf("sendCommandOnFloorFound"), FoldoutGroup("Floor Found Event")]
        public UnityEvent onFloorFound = new UnityEvent();
        #endregion

#region Floor Restored Event
        [FoldoutGroup("Floor Tracking Restored Event")]
        public bool sendCommandOnFloorRestored = false;

        [ShowIf("sendCommandOnFloorRestored"), FoldoutGroup("Floor Tracking Restored Event")]
        public UnityEvent onFloorRestored = new UnityEvent();
        #endregion

#region Floor Lost Event
        //To track when the floor is lost, we track when the automatic hit detection stops finding new raycast hits
        private bool floorTrackingDetected = false;

        //Keep track of what the floor tracking detection status was last frame
        private bool previousFloorTrackingDetected = false;

        [InfoBox( "Floor Tracking Lost event is currently not working in Vuforia, check out the forum topic here for updates.\n\nhttps://developer.vuforia.com/forum/ground-plane/need-ground-plane-tracking-lost-event", InfoMessageType.Warning ), FoldoutGroup( "Floor Lost Event" )]
        public bool sendCommandOnFloorLost = false;

        [ShowIf("sendCommandOnFloorLost"), FoldoutGroup("Floor Lost Event")]
        public UnityEvent onFloorLost = new UnityEvent();
#endregion



#region Floor Pressed Event

        //Store the last seen values for when the floor plane was pressed by the user
        private Vector3 lastFloorCenterOfScreenPosition = Vector3.zero;
        public Vector3 GetLastFloorCenterOfScreenPosition() { return lastFloorCenterOfScreenPosition; }

        private Quaternion lastFloorCenterOfScreenRotation = Quaternion.identity;
        public Quaternion GetLastFloorCenterOfScreenRotation() { return lastFloorCenterOfScreenRotation; }

        [InfoBox( "If you want to set the position or rotation of where the floor was pressed to a transform, use a child BlockEvent with a XRTargetFloor event type and set its action to 'SetTransformToFloor'\n\nYou can also create a BlockEvent with a Transform event type and set it's action to 'Pinch To Scale' or 'Twist To Rotate' to further manipulate an object.\n\nOtherwise you can use the below command to send an event whenever the user presses and the resulting raycast collides with the floor" ), FoldoutGroup( "Floor Pressed Event" )]
        public bool sendCommandOnFloorPressed = false;

        [ShowIf("sendCommandOnFloorPressed"), FoldoutGroup("Floor Pressed Event")]
        public UnityEvent onFloorPressed = new UnityEvent();
#endregion


        //-----------------------------//
        public override void Awake()
        //-----------------------------//
        {
            base.Awake();
            
            CheckIfAnchorBehaviourExists();

            CheckIfContentPositioningBehaviourExists();
            
        } //END Awake
        
        //-------------------------------//
        private void CheckIfAnchorBehaviourExists()
        //-------------------------------//
        {

#if VUFORIA
            //If the AnchorBehaviour doesn't exist, create it and link to it
            if( GetComponent<AnchorBehaviour>() == null )
            {
                anchorBehaviour = gameObject.AddComponent<AnchorBehaviour>();
            }

            //Otherwise if the script exists, link to it
            if( anchorBehaviour == null &&
                GetComponent<AnchorBehaviour>() != null )
            {
                anchorBehaviour = GetComponent<AnchorBehaviour>();
            }
#endif

        } //END CheckIfAnchorBehaviourExists

        //-------------------------------//
        private void CheckIfContentPositioningBehaviourExists()
        //-------------------------------//
        {

#if VUFORIA
            //If the PlaneFinderBehaviour doesn't exist, create it and link to it
            if( GetComponent<ContentPositioningBehaviour>() == null )
            {
                contentPositioningBehaviour = gameObject.AddComponent<ContentPositioningBehaviour>();
            }

            //Otherwise if the script exists, link to it
            if( contentPositioningBehaviour == null &&
                GetComponent<ContentPositioningBehaviour>() != null )
            {
                contentPositioningBehaviour = GetComponent<ContentPositioningBehaviour>();
            }
#endif

        } //END CheckIfContentPositioningBehaviourExists


        //--------------------------------//
        public override void Update()
        //--------------------------------//
        {
#if VUFORIA
            //CheckForFloorTrackingLost();
#endif
        } //END Update

        //--------------------------------//
        private void CheckForFloorTrackingLost()
        //--------------------------------//
        {
#if VUFORIA
            //We found floor tracking!
            if( !previousFloorTrackingDetected && floorTrackingDetected )
            {
                previousFloorTrackingDetected = floorTrackingDetected;
                floorTrackingDetected = false;
                //if( showDebug ) Debug.Log( "XRTargetFloor.cs CheckForFloorTrackingLost() TRACKING FOUND" );
            }

            //We lost floor tracking!
            else if( previousFloorTrackingDetected && !floorTrackingDetected)
            {
                previousFloorTrackingDetected = floorTrackingDetected;
                //if( showDebug ) Debug.Log( "XRTargetFloor.cs CheckForFloorTrackingLost() TRACKING LOST" );

                if(onFloorLost != null) { onFloorLost.Invoke(); }
            }
            else
            {
                //Debug.Log( "XRTargetFloor.cs CheckForFloorTrackingLost() previous = " + previousFloorTrackingDetected + ", current = " + floorTrackingDetected );
            }
#endif
        } //END CheckForFloorTrackingLost

        //--------------------------------//
        /// <summary>
        /// Informs other components via a UnityEvent that floor tracking has been lost
        /// </summary>
        public void OnFloorTrackingLost()
        //--------------------------------//
        {
            if (onFloorLost != null) { onFloorLost.Invoke(); }

        } //END OnFloorTrackingLost

#if VUFORIA
        //--------------------------------//
        public override void OnFloorTrackingAutomaticHitTest( HitTestResult hitTestResult )
        //--------------------------------//
        {
            
            if( hitTestResult != null )
            {
                //We found a floor this frame!
                //Debug.Log( "hitTestResult = " + hitTestResult.ToString() );
                floorTrackingDetected = true;

                //We wait until we have a certain amount of automatic floor plane found hits before declaring that it's okay to use
                if( !isFloorPlaneReady )
                {
                    if( floorPlaneFoundHits >= requiredNumberOfFloorPlaneFoundHits )
                    {
                        isFloorPlaneReady = true;

                        //We found the floor! Send a message out
                        if( sendCommandOnFloorFound )
                        {
                            if(onFloorFound != null) { onFloorFound.Invoke(); }
                        }
                    }
                    else
                    {
                        floorPlaneFoundHits++;
                        isFloorPlaneReady = false;
                    }
                }

                lastFloorCenterOfScreenPosition = hitTestResult.Position;
                lastFloorCenterOfScreenRotation = hitTestResult.Rotation;
            }

        } //END OnFloorTrackingAutomaticHitTest
#endif

        //--------------------------------//
        /// <summary>
        /// Informs other components via a UnityEvent that floor tracking has been restored. Called by the XRVuforiaCamera
        /// </summary>
        public void OnFloorTrackingRestored()
        //--------------------------------//
        {
            //We can only 'restore' the floor plane after it's been found initially
            if (isFloorPlaneReady)
            {
                if (onFloorRestored != null) { onFloorRestored.Invoke(); }
            }

        } //END OnFloorTrackingRestored

#if VUFORIA
        //--------------------------------//
        /// <summary>
        /// When the user presses on the ground plane, only allowed once the automatic hit test has occured over 30 times
        /// </summary>
        /// <param name="hitTestResult"></param>
        public override void OnFloorTrackingHitTest( HitTestResult hitTestResult )
        //--------------------------------//
        {

            if( isFloorPlaneReady )
            {
                if( hitTestResult != null )
                {
                    lastFloorCenterOfScreenPosition = hitTestResult.Position;
                    lastFloorCenterOfScreenRotation = hitTestResult.Rotation;

                    if(onFloorPressed != null) { onFloorPressed.Invoke(); }
                }
            }

        } //END OnFloorTrackingHitTest
#endif

        //--------------------------------//
        /// <summary>
        /// When the user taps on the device screen
        /// </summary>
        /// <param name="screenPosition"></param>
        public void OnInputRecievedEvent( Vector2 screenPosition )
        //--------------------------------//
        {

            //if( showDebug ) { Debug.Log( "XRTargetFloor.cs OnInputRecievedEvent() screenPosition = " + screenPosition ); }

        } //END OnInputRecievedEvent


        //--------------------------------//
        public override void DestroyIfIncompatibleXRTechnology()
        //--------------------------------//
        {
            
            base.DestroyIfIncompatibleXRTechnology();

        } //END DestroyIfIncompatibleXRTechnology


        //--------------------------------------//
        public void RegisterTracker()
        //--------------------------------------//
        {

#if VUFORIA
            if( anchorBehaviour )
            {
                anchorBehaviour.RegisterTrackableEventHandler( this );
            }
#endif

        } //END RegisterTracker

        //--------------------------------------//
        public void UnregisterTracker()
        //--------------------------------------//
        {

#if VUFORIA
            if( anchorBehaviour )
            {
                anchorBehaviour.UnregisterTrackableEventHandler( this );
            }
#endif

        } //END UnregisterTracker

        //--------------------------------------//
        public override void OnEnable()
        //--------------------------------------//
        {
            //Let our vuforia component know that this has been enabled
#if VUFORIA
            if( anchorBehaviour != null ) { anchorBehaviour.enabled = true; }

            RegisterTracker();
#endif

            base.OnEnable();

        } //END OnEnable

        //--------------------------------------//
        public override void OnDisable()
        //--------------------------------------//
        {
            //Let our vuforia component know that this has been disabled
#if VUFORIA
            if( anchorBehaviour != null ) { anchorBehaviour.enabled = false; }

            UnregisterTracker();
#endif

            base.OnDisable();

        } //END OnDisable

#if VUFORIA
        //---------------------------------------//
        public void OnTrackableStateChanged( TrackableBehaviour.Status previousStatus, TrackableBehaviour.Status newStatus )
        //---------------------------------------//
        {
            //throw new System.NotImplementedException();

        } //END OnTrackableStateChanged
#endif

    } //END Class

} //END Namespace