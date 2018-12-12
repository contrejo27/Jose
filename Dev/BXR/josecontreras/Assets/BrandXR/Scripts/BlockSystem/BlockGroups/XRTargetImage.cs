/* XRTargetImage.cs
 * 
 * Inherits from XRTargetBase.cs. Sends commands to blocks or nested BlockGroups when tracking is lost and found.
 * 
 * Must have an XRTarget component on the parent GameObject. There can only be one XRTargetBase attached to an XRTarget.
 * If more than one XRTargetBase is detected as being attached to the XRTarget, this script will attempt to destroy itself on Start()
 * 
 * Allows you to customize width and height of the ImageTarget.
 * 
 * Requires Vuforia's ImageTargetBehaviour to access the Vuforia's image database and set an ImageTarget.
 * 
 */

#if VUFORIA
using Vuforia;
#endif

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Sirenix.OdinInspector;
using UnityEngine.Events;

namespace BrandXR
{
#if VUFORIA
    public class XRTargetImage : XRTargetBase, ITrackableEventHandler
#else
    public class XRTargetImage: XRTargetBase
#endif
    {

        [FoldoutGroup("Readme"), InfoBox("Sends a command to any childed blocks and nested BlockGroups when the user views the selected image target.\n\nYou can only have one XRTargetBase attached to one XRTarget parent. If you want to make an additional XRTargetBase, create another XRTarget first in a BlockView")]
        public int dummy4 = 0;

        public override XRTargetBaseType_Vuforia GetXRTargetType() { return XRTargetBaseType_Vuforia.Image; }

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
        [Space(15f), FoldoutGroup("Hooks")]
        public ImageTargetBehaviour imageTargetBehaviour;

        [Space( 15f ), FoldoutGroup( "Hooks" )]
        public TrackableBehaviour trackableBehaviour;
#endif

        private float width;
        private float height;

#if VUFORIA
        public float Width { get { return width; } set { width = value; imageTargetBehaviour.SetWidth(width); } }
        public float Height { get { return height; } set { height = value; imageTargetBehaviour.SetHeight(height); } }
#else
        public float Width { get { return width; } set { width = value; } }
        public float Height { get { return height; } set { height = value; } }
#endif

        [FoldoutGroup("Tracker Found Event")]
        public bool sendCommandOnTrackerFound = false;

        [ShowIf("sendCommandOnTrackerFound"), FoldoutGroup("Tracker Found Event")]
        public UnityEvent onTrackerFound = new UnityEvent();
        
        [Space(15f ), ShowIf( "sendCommandOnTrackerFound" ), Tooltip( "Enable this to allow this event even when the GameObject is disabled." ), FoldoutGroup( "Tracker Found Event" )]
        public bool allowFoundEventWhenDisabled = false;



        [FoldoutGroup( "Extended Tracking Found Event" )]
        public bool sendCommandOnExtendedTrackingFound = false;

        [ShowIf("sendCommandOnExtendedTrackingFound"), FoldoutGroup("Extended Tracking Found Event")]
        public UnityEvent onExtendedTrackingFound = new UnityEvent();
        
        [Space( 15f ), ShowIf( "sendCommandOnExtendedTrackingFound" ), Tooltip( "Enable this to allow this event even when the GameObject is disabled." ), FoldoutGroup( "Extended Tracking Found Event" )]
        public bool allowExtendedTrackingFoundEventWhenDisabled = false;




        [FoldoutGroup( "Tracker Lost Event" )]
        public bool sendCommandOnTrackerLost = false;

        [ShowIf("sendCommandOnTrackerLost"), FoldoutGroup("Tracker Lost Event")]
        public UnityEvent onTrackerLost = new UnityEvent();

        [Space( 15f ), ShowIf( "sendCommandOnTrackerLost" ), Tooltip( "Enable this to allow this event even when the GameObject is disabled." ), FoldoutGroup( "Tracker Lost Event" )]
        public bool allowLostEventWhenDisabled = false;


        //------------------------//
        public override void Awake()
        //------------------------//
        {
            base.Awake();

            CheckIfVuforiaImageTargetBehaviourExists();

            CheckIfVuforiaTrackableBehaviourExists();

        } //END Awake

        //-------------------------//
        private void CheckIfVuforiaImageTargetBehaviourExists()
        //-------------------------//
        {

#if VUFORIA
            //We should already have a hook for the Vuforia ImageTargetBehaviour, if not create it now
            if ( GetComponent<ImageTargetBehaviour>() == null ) { gameObject.AddComponent<ImageTargetBehaviour>(); }

            if( imageTargetBehaviour == null )
            {
                imageTargetBehaviour = GetComponent<ImageTargetBehaviour>();
            }
#endif

        } //END CheckIfVuforiaImageTargetBehaviourExists

        //------------------------//
        private void CheckIfVuforiaTrackableBehaviourExists()
        //------------------------//
        {

#if VUFORIA
            if ( GetComponent<TrackableBehaviour>() == null )
            {
                trackableBehaviour = gameObject.AddComponent<TrackableBehaviour>();
            }

            if( trackableBehaviour == null )
            {
                trackableBehaviour = GetComponent<TrackableBehaviour>();
            }

            if( trackableBehaviour )
            {
                trackableBehaviour.RegisterTrackableEventHandler( this );
            }
#endif

        } //END CheckIfVuforiaTrackableBehaviourExists

#if VUFORIA
        //-------------------------//
        public void OnTrackableStateChanged( TrackableBehaviour.Status previousStatus, TrackableBehaviour.Status newStatus )
        //-------------------------//
        {
            
            if( sendCommandOnTrackerFound && 
                ( newStatus == TrackableBehaviour.Status.DETECTED ||
                  newStatus == TrackableBehaviour.Status.TRACKED ) )
            {
                //If our GameObject is disabled, we might need to cancel this event
                if( gameObject.activeInHierarchy ||
                    ( !gameObject.activeInHierarchy && allowFoundEventWhenDisabled ) )
                {
                    if(onTrackerFound != null) { onTrackerFound.Invoke(); }
                }
            }
            else if( sendCommandOnExtendedTrackingFound &&
                     ( newStatus == TrackableBehaviour.Status.EXTENDED_TRACKED) )
            {
                //If our GameObject is disabled, we might need to cancel this event
                if( gameObject.activeInHierarchy ||
                    ( !gameObject.activeInHierarchy && allowExtendedTrackingFoundEventWhenDisabled ) )
                {
                    if(onExtendedTrackingFound != null) { onExtendedTrackingFound.Invoke(); }
                }
            }
            else if( sendCommandOnTrackerLost && 
                     ( previousStatus == TrackableBehaviour.Status.TRACKED &&
                       newStatus == TrackableBehaviour.Status.NO_POSE ) )
            {
                //If our GameObject is disabled, we might need to cancel this event
                if( gameObject.activeInHierarchy ||
                    ( !gameObject.activeInHierarchy && allowLostEventWhenDisabled ) )
                {
                    if(onTrackerLost != null) { onTrackerLost.Invoke(); }
                }
            }

        } //END OnTrackableStateChanged
#endif

        //--------------------------------//
        public override void DestroyIfIncompatibleXRTechnology()
        //--------------------------------//
        {
            
#if VUFORIA
            if( trackableBehaviour != null )
            {
                trackableBehaviour.UnregisterTrackableEventHandler( this );
                Destroy( trackableBehaviour );
            }
#endif

            base.DestroyIfIncompatibleXRTechnology();

        } //END DestroyIfIncompatibleXRTechnology


        //--------------------------------------//
        public void RegisterTracker()
        //--------------------------------------//
        {

#if VUFORIA
            if( imageTargetBehaviour )
            {
                imageTargetBehaviour.RegisterTrackableEventHandler( this );
            }

            if( trackableBehaviour )
            {
                trackableBehaviour.RegisterTrackableEventHandler( this );
            }
#endif

        } //END RegisterTracker

        //--------------------------------------//
        public void UnregisterTracker()
        //--------------------------------------//
        {

#if VUFORIA
            if( imageTargetBehaviour )
            {
                imageTargetBehaviour.UnregisterTrackableEventHandler( this );
            }

            if( trackableBehaviour )
            {
                trackableBehaviour.UnregisterTrackableEventHandler( this );
            }
#endif

        } //END UnregisterTracker

        //--------------------------------------//
        public override void OnEnable()
        //--------------------------------------//
        {
            //Let our vuforia component know that this has been enabled
#if VUFORIA
            if( imageTargetBehaviour != null ) { imageTargetBehaviour.enabled = true; }

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
            if( imageTargetBehaviour != null ) { imageTargetBehaviour.enabled = false; }

            UnregisterTracker();
#endif

            base.OnDisable();

        } //END OnDisable


    } //END Class

} //END Namespace