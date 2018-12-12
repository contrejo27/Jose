/* XRTargetObject.cs
 * 
 * Inherits from XRTargetBase.cs. Sends commands to blocks or nested BlockGroups when tracking is lost and found.
 * 
 * Must have an XRTarget component on the parent GameObject. There can only be one XRTargetBase attached to an XRTarget.
 * If more than one XRTargetBase is detected as being attached to the XRTarget, this script will attempt to destroy itself on Start()
 * 
 * Allows you to customize width and height of the ModelTarget.
 * 
 * Requires Vuforia's ModelTargetBehaviour to access the Vuforia's model database and set an ModelTarget.
 * 
 */

using Sirenix.OdinInspector;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

#if VUFORIA
using Vuforia;
#endif

namespace BrandXR
{
#if VUFORIA
    public class XRTargetObject: XRTargetBase, ITrackableEventHandler
#else
    public class XRTargetObject : XRTargetBase
#endif
    {
        public override XRTargetBaseType_Vuforia GetXRTargetType() { return XRTargetBaseType_Vuforia.Object; }


        [FoldoutGroup( "Readme" ), InfoBox( "Sends a command to any childed blocks and nested BlockGroups when the user views the selected object target.\n\nYou can only have one XRTargetBase attached to one XRTarget parent. If you want to make an additional XRTargetBase, create another XRTarget first in a BlockView" )]
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
        public ModelTargetBehaviour modelTargetBehaviour;

        [Space( 15f ), FoldoutGroup( "Hooks" )]
        public TrackableBehaviour trackableBehaviour;
#endif

        private float length;
        private float width;
        private float height;

#if VUFORIA
        public float Length { get { return length; } set { length = value; modelTargetBehaviour.SetLength(length); } }
        public float Width { get { return width; } set { width = value; modelTargetBehaviour.SetWidth(width); } }
        public float Height { get { return height; } set { height = value; modelTargetBehaviour.SetHeight(height); } }
#else
        public float Length { get { return length; } set { length = value; } }
        public float Width { get { return width; } set { width = value; } }
        public float Height { get { return height; } set { height = value; } }
#endif

        [FoldoutGroup( "Tracker Found Event" )]
        public bool sendCommandOnTrackerFound = true;

        [ShowIf("sendCommandOnTrackerFound"), FoldoutGroup("Tracker Found Event")]
        public UnityEvent onTrackerFound = new UnityEvent();


        [FoldoutGroup( "Tracker Lost Event" )]
        public bool sendCommandOnTrackerLost = true;

        [ShowIf("sendCommandOnTrackerLost"), FoldoutGroup("Tracker Lost Event")]
        public UnityEvent onTrackerLost = new UnityEvent();

        
        //-----------------------------//
        public override void Awake()
        //-----------------------------//
        {
            base.Awake();

            CheckIfModelTargetBehaviourExists();

            CheckIfVuforiaTrackableBehaviourExists();

        } //END Awake

        //-------------------------------//
        private void CheckIfModelTargetBehaviourExists()
        //-------------------------------//
        {

#if VUFORIA
            //If the ModelTargetBehaviour doesn't exist, create it and link to it
            if( GetComponent<ModelTargetBehaviour>() == null )
            {
                modelTargetBehaviour = gameObject.AddComponent<ModelTargetBehaviour>();
            }

            //Otherwise if the script exists, link to it
            if( modelTargetBehaviour == null &&
                GetComponent<ModelTargetBehaviour>() != null )
            {
                modelTargetBehaviour = GetComponent<ModelTargetBehaviour>();
            }
#endif

        } //END CheckIfModelTargetBehaviourExists

        //------------------------//
        private void CheckIfVuforiaTrackableBehaviourExists()
        //------------------------//
        {

#if VUFORIA
            if( GetComponent<TrackableBehaviour>() == null )
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
                  newStatus == TrackableBehaviour.Status.TRACKED ||
                  newStatus == TrackableBehaviour.Status.EXTENDED_TRACKED ) )
            {
                if(onTrackerFound != null) { onTrackerFound.Invoke(); }
            }
            else if( sendCommandOnTrackerLost &&
                     ( previousStatus == TrackableBehaviour.Status.TRACKED &&
                       newStatus == TrackableBehaviour.Status.NO_POSE ) )
            {
                if(onTrackerLost != null) { onTrackerLost.Invoke(); }
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

                TrackerManager.Instance.GetStateManager().ReassociateTrackables();
            }
#endif

            base.DestroyIfIncompatibleXRTechnology();

        } //END DestroyIfIncompatibleXRTechnology


    } //END Class

} //END Namespace