using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Sirenix.OdinInspector;
using System.Linq;
//using UnityEditor.Compilation;

#if VUFORIA
using Vuforia;
#endif



namespace BrandXR
{
#if VUFORIA
    public class XRCameraVuforia: XRCamera, ITrackableEventHandler
#else
    public class XRCameraVuforia: XRCamera
#endif
    {

        public override XRCameraType GetXRCameraType() { return XRCameraType.Vuforia; }

        //Hooks
        [FoldoutGroup( "Hooks" ), InfoBox( "Do Not Modify", InfoMessageType.Warning )]
        public GameObject planeFinderGameObject;

        [Space(15f), FoldoutGroup("Hooks")]
        public Texture defaultFloorPlaneIconTexture = null;

#if VUFORIA
        //Hooks
        [Space( 15f ), FoldoutGroup("Hooks")]
        public AnchorInputListenerBehaviour anchorInputListenerBehaviour;

        [Space( 15f ), FoldoutGroup( "Hooks" )]
        public PlaneFinderBehaviour planeFinderBehaviour;

        [Space( 15f ), FoldoutGroup( "Hooks" )]
        public VuforiaBehaviour vuforiaBehaviour;

        [Space(15f), FoldoutGroup("Hooks")]
        public DefaultInitializationErrorHandler defaultInitializationErrorHandler;
#endif

        [FoldoutGroup("Tracker Icon Settings")]
        public bool enableFloorTrackerIcon = true;

        [ShowIf("enableFloorTrackerIcon"), FoldoutGroup("Tracker Icon Settings")]
        public Texture floorIconTexture = null;

        [Space( 15f ), ShowIf("enableFloorTrackerIcon"), FoldoutGroup("Tracker Icon Settings")]
        public Color showFloorIconColor = Color.white;

        [ShowIf("enableFloorTrackerIcon"), FoldoutGroup("Tracker Icon Settings")]
        public float showFloorIconTweenSpeed = 1f;

        [ShowIf("enableFloorTrackerIcon"), FoldoutGroup("Tracker Icon Settings")]
        public EaseCurve.EaseType showFloorIconEasing = EaseCurve.EaseType.ExpoEaseInOut;


        [Space(15f), ShowIf("enableFloorTrackerIcon"), FoldoutGroup("Tracker Icon Settings")]
        public Color hideFloorIconColor = new Color( Color.white.r, Color.white.g, Color.white.b, 0f );

        [ShowIf("enableFloorTrackerIcon"), FoldoutGroup("Tracker Icon Settings")]
        public float hideFloorIconTweenSpeed = 1f;

        [ShowIf("enableFloorTrackerIcon"), FoldoutGroup("Tracker Icon Settings")]
        public EaseCurve.EaseType hideFloorIconEasing = EaseCurve.EaseType.ExpoEaseInOut;


        private UIColorTweener floorIconColorTweener = null;

        //Keep track of what the previous gameobject active state was for the floor tracker icon
        private bool wasFloorTrackerEnabledLastUpdate = false;




        //-----------------------//
        public void Awake()
        //-----------------------//
        {
#if VUFORIA
            //Because of pre-processor directives, our linkages within these two Vuforia components to this script can break, make sure they're hooked up!
            CheckIfVuforiaComponentsAreConnected();
#endif

        } //END Awake

        //------------------------//
        public void Start()
        //------------------------//
        {

#if VUFORIA
            //Setup the floor icon
            ConnectToFloorIconRenderer();

            SetFloorTrackerIconTexture(floorIconTexture);

            Timer.instance.In( .01f, ForceDisableFloorTrackerIcon, this.gameObject );

            //Get the targets once tracking begins
            GetTargetsToTrack();
#endif

        } //END Start

        //------------------------//
        private void CheckIfVuforiaComponentsAreConnected()
        //------------------------//
        {

#if VUFORIA
            //Grab the VuforiaBehaviour component, if it doesn't exist then create it
            if( vuforiaBehaviour == null )
            {
                if( GetComponent<VuforiaBehaviour>() == null )
                {
                    gameObject.AddComponent<VuforiaBehaviour>();
                }

                vuforiaBehaviour = GetComponent<VuforiaBehaviour>();
            }

            //Grab the DefaultInitializationError component, if it doesn't exist then create it
            if( defaultInitializationErrorHandler == null )
            {
                if( GetComponent<DefaultInitializationErrorHandler>() == null )
                {
                    gameObject.AddComponent<DefaultInitializationErrorHandler>();
                }

                defaultInitializationErrorHandler = GetComponent<DefaultInitializationErrorHandler>();
            }

            //Grab the Plane finder child gameObject, if it doesn't exist then create it
            if( planeFinderGameObject == null )
            {
                if( this.GetComponentInChildren<Transform>() == null )
                {
                    planeFinderGameObject = new GameObject( "Plane Finder" );
                    planeFinderGameObject.transform.parent = this.transform;
                }
                else
                {
                    planeFinderGameObject = this.GetComponentInChildren<Transform>().gameObject;
                }
            }

            //If the scripts don't exist, grab them. If they're completely missing, then add them now
            if( anchorInputListenerBehaviour == null )
            {
                if( this.GetComponentInChildren<AnchorInputListenerBehaviour>() != null )
                {
                    anchorInputListenerBehaviour = this.GetComponentInChildren<AnchorInputListenerBehaviour>();
                }
                else
                {
                    anchorInputListenerBehaviour = planeFinderGameObject.AddComponent<AnchorInputListenerBehaviour>();
                }
            }

            if( planeFinderBehaviour == null )
            {
                if( this.GetComponentInChildren<PlaneFinderBehaviour>() != null )
                {
                    planeFinderBehaviour = this.GetComponentInChildren<PlaneFinderBehaviour>();
                }
                else
                {
                    planeFinderBehaviour = planeFinderGameObject.AddComponent<PlaneFinderBehaviour>();
                }
            }

            //Link the AnchorInputListener's OnInputRecieved event to the PlaneFinder HitTest if that link is missing
            if ( anchorInputListenerBehaviour.OnInputReceivedEvent != null &&
                ( anchorInputListenerBehaviour.OnInputReceivedEvent.GetPersistentEventCount() == 0 ||
                  anchorInputListenerBehaviour.OnInputReceivedEvent.GetPersistentTarget( 0 ) != planeFinderBehaviour ) )
            {
                //Add a Non-Persistent listener to this event. This will work but it won't be visible in the editor and won't be linked to the event after play mode ends
                anchorInputListenerBehaviour.OnInputReceivedEvent.AddListener( planeFinderBehaviour.PerformHitTest );
            }

            //Link the planeFinderBehaviour's OnInteractiveHitTest and OnAutomaticHitTest to this script if that linkage is missing
            if( planeFinderBehaviour.OnAutomaticHitTest != null &&
                ( planeFinderBehaviour.OnAutomaticHitTest.GetPersistentEventCount() == 0 ||
                  planeFinderBehaviour.OnAutomaticHitTest.GetPersistentTarget( 0 ) != this ) )
            {
                planeFinderBehaviour.OnInteractiveHitTest.AddListener( OnInteractiveHitTest );
            }

            if( planeFinderBehaviour.OnInteractiveHitTest != null &&
                ( planeFinderBehaviour.OnInteractiveHitTest.GetPersistentEventCount() == 0 ||
                  planeFinderBehaviour.OnInteractiveHitTest.GetPersistentTarget( 0 ) != this ) )
            {
                planeFinderBehaviour.OnAutomaticHitTest.AddListener( OnAutomaticHitTest );
            }
#endif

        } //END CheckIfVuforiaComponentsAreConnected



        //----------------------------------------------------------------//
        private void ConnectToFloorIconRenderer()
        //----------------------------------------------------------------//
        {

#if VUFORIA
            //Find the floor icon child gameObject generated by Vuforia, attach a UIColorTweener to it so we can fade it in and out
            if (floorIconColorTweener == null &&
                planeFinderBehaviour != null &&
                planeFinderBehaviour.GetComponentInChildren(typeof(MeshRenderer), true))
            {
                MeshRenderer renderer = planeFinderBehaviour.GetComponentInChildren(typeof(MeshRenderer), true) as MeshRenderer;

                if (renderer != null)
                {
                    if( renderer.GetComponent<UIColorTweener>() == null )
                    {
                        floorIconColorTweener = renderer.gameObject.AddComponent<UIColorTweener>();
                    }
                    else
                    {
                        floorIconColorTweener = renderer.gameObject.GetComponent<UIColorTweener>();
                    }

                    if( floorIconColorTweener != null )
                    {
                        floorIconColorTweener.Renderer = renderer;

                        floorIconColorTweener.color_Show = showFloorIconColor;
                        floorIconColorTweener.color_Hide = hideFloorIconColor;

                        floorIconColorTweener.tweenSpeed_Show = showFloorIconTweenSpeed;
                        floorIconColorTweener.tweenSpeed_Hide = hideFloorIconTweenSpeed;

                        floorIconColorTweener.easeType_Show = showFloorIconEasing;
                        floorIconColorTweener.easeType_Hide = hideFloorIconEasing;
                    }

                }
            }
#endif

        } //END ConnectToFloorIconRenderer

        public enum FocusModes
        {
            NORMAL,
            TRIGGERAUTO,
            CONTINOUSAUTO,
            INFINITY,
            MACRO
        }

#region Getters/Setters
#if VUFORIA
        public FocusModes FocusMode
        {
            get { return _focusMode; }
            set
            {
                _focusMode = value;
                UpdateFocusMode();
            }
        }
#endif
#endregion

        [BoxGroup( "Tracking Debugs", order: 1 ), OnValueChanged( "UpdateTargetsToTrack" )]
        public bool _trackObject;
        [BoxGroup( "Tracking Debugs" ), OnValueChanged( "UpdateTargetsToTrack" )]
        public bool _trackImage;
        [BoxGroup( "Tracking Debugs" ), OnValueChanged( "UpdateTargetsToTrack" )]
        public bool _trackFloor;

#if VUFORIA
        private TrackableBehaviour trackableBehavior;
#endif

        [ReadOnly, TextArea( 1, 5 )]
        public string _focusModeInfo;

#if VUFORIA
        [EnumToggleButtons, OnValueChanged( "UpdateFocusMode" ), SerializeField]
        private FocusModes _focusMode;
#endif

        private void GetTargetsToTrack()
        {
#if VUFORIA
            
            trackableBehavior = FindObjectOfType<TrackableBehaviour>();
            if( trackableBehavior )
            {
                trackableBehavior.RegisterTrackableEventHandler( this );
            }
            UpdateTargetsToTrack();
            FocusMode = FocusModes.CONTINOUSAUTO;
#endif
        }

        private void OnValidate()
        {
#if VUFORIA
            FocusMode = _focusMode;
#endif
        }

#if VUFORIA
        public void OnTrackableStateChanged(
                                       TrackableBehaviour.Status previousStatus,
                                       TrackableBehaviour.Status newStatus )
        {
            if( newStatus == TrackableBehaviour.Status.DETECTED ||
                newStatus == TrackableBehaviour.Status.TRACKED )
            {
                //Debug.Log( "Detected!" );
            }
            else
            {
                //Debug.Log( "Tracking lost!" );
            }
        }
#endif

#if VUFORIA
        private void UpdateFocusMode()
        {
            
            if( Resources.Load( "VuforiaConfiguration" ) == null )
                return;

            if( Vuforia.CameraDevice.Instance == null )
            {
                Debug.LogError( "Failed to set focus mode" );
                return;
            }
            switch( _focusMode )
            {
                case FocusModes.NORMAL:
                CameraDevice.Instance.SetFocusMode( CameraDevice.FocusMode.FOCUS_MODE_NORMAL );
                _focusModeInfo = "Sets the camera into the default mode as defined by the camera drive";
                break;
                case FocusModes.TRIGGERAUTO:
                CameraDevice.Instance.SetFocusMode( CameraDevice.FocusMode.FOCUS_MODE_TRIGGERAUTO );
                _focusModeInfo = "Triggers a single autofocus operation";
                break;
                case FocusModes.CONTINOUSAUTO:
                CameraDevice.Instance.SetFocusMode( CameraDevice.FocusMode.FOCUS_MODE_CONTINUOUSAUTO );
                _focusModeInfo = "Optimal for AR applications. Guarantees that the camera is focused on the target";
                break;
                case FocusModes.INFINITY:
                CameraDevice.Instance.SetFocusMode( CameraDevice.FocusMode.FOCUS_MODE_INFINITY );
                _focusModeInfo = "Sets the camera to infinity, as provided by the camera drive implementation (not supported on iOS)";
                break;
                case FocusModes.MACRO:
                CameraDevice.Instance.SetFocusMode( CameraDevice.FocusMode.FOCUS_MODE_MACRO );
                _focusModeInfo = "Provides a sharp camera image for distances of closeups, rarely used in AR setups (not supported on iOS)";
                break;
            }
        }
#endif

#region Specific Tracking Debugs
#if VUFORIA
        [BoxGroup( "Tracking Debugs" ), Button( ButtonSizes.Small )]
        public void TrackAllTargets()
        {
            _trackImage = _trackFloor = _trackObject = true;
            UpdateTargetsToTrack();
        }
        [BoxGroup( "Tracking Debugs" ), Button( ButtonSizes.Small )]
        public void TrackNoTargets()
        {
            _trackImage = _trackFloor = _trackObject = false;
            UpdateTargetsToTrack();
        }

        void UpdateTargetsToTrack()
        {
            
            if( TrackerManager.Instance == null )
            {
                Debug.LogWarning( "No [Tracker Manager] in scene!" );
                return;
            }

            // Image
            if( _trackImage )
            {
                ImageTargetBehaviour[] imageTargets = FindObjectsOfType<ImageTargetBehaviour>();
                foreach( ImageTargetBehaviour image in imageTargets )
                    image.enabled = true;

            }
            else
            {
                ImageTargetBehaviour[] imageTargets = FindObjectsOfType<ImageTargetBehaviour>();
                foreach( ImageTargetBehaviour image in imageTargets )
                    image.enabled = false;
            }

            // Object
            if( TrackerManager.Instance.GetTracker<ObjectTracker>() == null ) return;
            if( _trackObject )
                TrackerManager.Instance.GetTracker<ObjectTracker>().Start();
            else
                TrackerManager.Instance.GetTracker<ObjectTracker>().Stop();

            // Floor
            if( _trackFloor )
            {
                AnchorBehaviour[] groundPlanes = FindObjectsOfType<AnchorBehaviour>();
                foreach( AnchorBehaviour groundPlane in groundPlanes )
                    groundPlane.enabled = true;
            }
            else
            {
                AnchorBehaviour[] groundPlanes = FindObjectsOfType<AnchorBehaviour>();
                foreach( AnchorBehaviour groundPlane in groundPlanes )
                    groundPlane.enabled = false;
            }
        }
#endif
#endregion



#if VUFORIA
        /// <summary>
        /// Send the Plane Finder hit test result from the center of the screen every frame the plane exists
        /// </summary>
        /// <param name="hitTestResult"></param>
        //------------------------------//
        public void OnAutomaticHitTest( HitTestResult hitTestResult )
        //------------------------------//
        {

            if( hitTestResult != null && GameObject.FindObjectOfType<XRTargetBase>() != null )
            {
                List<XRTargetBase> xrTargetBases = GameObject.FindObjectsOfType<XRTargetBase>().ToList();

                if( xrTargetBases != null && xrTargetBases.Count > 0 )
                {
                    foreach( XRTargetBase xrTargetBase in xrTargetBases )
                    {
                        xrTargetBase.OnFloorTrackingAutomaticHitTest( hitTestResult );
                    }
                }
            }

        } //END OnAutomaticHitTest

        /// <summary>
        /// Send the Plane Finder hit test result to all of the XRTarget's in the scene
        /// </summary>
        /// <param name="hitTestResult"></param>
        //------------------------------//
        public void OnInteractiveHitTest( HitTestResult hitTestResult )
        //------------------------------//
        {

            if( hitTestResult != null && GameObject.FindObjectOfType<XRTargetBase>() != null )
            {
                List<XRTargetBase> xrTargetBases = GameObject.FindObjectsOfType<XRTargetBase>().ToList();

                if( xrTargetBases != null && xrTargetBases.Count > 0 )
                {
                    foreach( XRTargetBase xrTargetBase in xrTargetBases )
                    {
                        xrTargetBase.OnFloorTrackingHitTest( hitTestResult );
                    }
                }
            }

        } //END OnInteractiveHitTest
#endif

        //---------------------------------//
        public void RegisterTrackers()
        //---------------------------------//
        {

            if( GetComponent<XRTargetImage>() != null )
            {
                List<XRTargetImage> xrTargetImages = GetComponents<XRTargetImage>().ToList();

                foreach( XRTargetImage xrTargetImage in xrTargetImages )
                {
                    if( xrTargetImage != null )
                    {
                        xrTargetImage.RegisterTracker();
                    }
                }
            }

            if( GetComponent<XRTargetFloor>() != null )
            {
                List<XRTargetFloor> xrTargetFloors = GetComponents<XRTargetFloor>().ToList();

                foreach( XRTargetFloor xrTargetFloor in xrTargetFloors )
                {
                    if( xrTargetFloor != null )
                    {
                        xrTargetFloor.RegisterTracker();
                    }
                }
            }

        } //END RegisterTrackers

        //---------------------------------//
        public void UnregisterTrackers()
        //---------------------------------//
        {

            if( GetComponent<XRTargetImage>() != null )
            {
                List<XRTargetImage> xrTargetImages = GetComponents<XRTargetImage>().ToList();

                foreach( XRTargetImage xrTargetImage in xrTargetImages )
                {
                    if( xrTargetImage != null )
                    {
                        xrTargetImage.UnregisterTracker();
                    }
                }
            }

            if( GetComponent<XRTargetFloor>() != null )
            {
                List<XRTargetFloor> xrTargetFloors = GetComponents<XRTargetFloor>().ToList();

                foreach( XRTargetFloor xrTargetFloor in xrTargetFloors )
                {
                    if( xrTargetFloor != null )
                    {
                        xrTargetFloor.UnregisterTracker();
                    }
                }
            }

        } //END UnregisterTrackers

        //---------------------------------//
        public override void DisableCamera()
        //---------------------------------//
        {

#if VUFORIA
            //VuforiaRuntime.Instance.Deinit();

            UnregisterTrackers();

            if( vuforiaBehaviour != null )
            {
                vuforiaBehaviour.enabled = false;
            }

            if( defaultInitializationErrorHandler != null )
            {
                defaultInitializationErrorHandler.enabled = false;
            }
#endif

            base.DisableCamera();

        } //END DisableCamera

        //---------------------------------//
        public override void EnableCamera()
        //---------------------------------//
        {

            base.EnableCamera();

#if VUFORIA
            //VuforiaRuntime.Instance.InitVuforia();

            RegisterTrackers();

            if( vuforiaBehaviour != null )
            {
                vuforiaBehaviour.enabled = true;

                //Register all the trackables we can find in the scene
                if( FindObjectOfType<TrackableBehaviour>() != null )
                {
                    trackableBehavior = FindObjectOfType<TrackableBehaviour>();

                    if( trackableBehavior )
                    {
                        trackableBehavior.RegisterTrackableEventHandler( this );
                    }
                }
                
            }

            if( defaultInitializationErrorHandler != null )
            {
                defaultInitializationErrorHandler.enabled = true;
            }
#endif
            
        } //END EnableCamera


        //---------------------------------//
        public void Update()
        //---------------------------------//
        {

#if VUFORIA
            CheckIfFloorTrackerIconShouldBeShown();
#endif

        } //END Update

        //---------------------------------//
        private void CheckIfFloorTrackerIconShouldBeShown()
        //---------------------------------//
        {

#if VUFORIA
            //Check if we care about showing the floor tracker icon
            if (enableFloorTrackerIcon)
            {
                //If the floor icon gameobject has been enabled by Vuforia, check if we should tween it's color value to 'Show'
                if (!wasFloorTrackerEnabledLastUpdate &&
                     IsFloorTrackerIconEnabled() )
                {
                    EnableFloorTrackerIcon();
                }

                //Likewise if the floor icon gameobject is disabled, check if we should tween it's value to 'Hide'
                else if (wasFloorTrackerEnabledLastUpdate &&
                         !IsFloorTrackerIconEnabled() )
                {
                    DisableFloorTrackerIcon();
                }
            }

            wasFloorTrackerEnabledLastUpdate = IsFloorTrackerIconEnabled();
#endif

        } //END CheckIfFloorTrackerIconShouldBeShown

        //---------------------------------//
        public override bool IsFloorTrackerIconEnabled()
        //---------------------------------//
        {
#if VUFORIA
            if (floorIconColorTweener != null &&
                floorIconColorTweener.Renderer != null)
            {
                return floorIconColorTweener.Renderer.enabled;
            }

            return false;
#else
            return false;
#endif

        } //END IsFloorTrackerIconEnabled

        //------------------------------------//
        public override void EnableFloorTrackerIcon()
        //------------------------------------//
        {
#if VUFORIA
            if ( floorIconColorTweener != null )
            {
                UnityEngine.Events.UnityEvent _event = new UnityEngine.Events.UnityEvent();
                _event.AddListener(EnableFloorTrackerIconComplete);

                floorIconColorTweener.Play( UITweener.TweenValue.Show, _event );

                //DOB NOTE: This is a hack to track when floor tracking is restored by looking at when the tracking icon is re-enabled
                SendFloorTrackingFoundEvent();
            }
#endif

        } //END EnableFloorTrackerIcon

        //-----------------------------------------------//
        private void EnableFloorTrackerIconComplete()
        //-----------------------------------------------//
        {
#if VUFORIA
            //Debug.Log("XRCameraVuforia.cs EnableFloorTrackerIconComplete() color = " + floorIconColorTweener.GetColorFromRenderer() );
#endif

        } //END EnableFloorTrackerIconComplete

        //------------------------------//
        /// <summary>
        /// Floor Trackers don't normally recieve the floor tracking lost event, we can instead send the event ourselves by checking for when our floor tracker icon disappears
        /// </summary>
        public void SendFloorTrackingFoundEvent()
        //------------------------------//
        {
            
            if( GameObject.FindObjectOfType<XRTargetFloor>() != null )
            {
                List<XRTargetFloor> xrTargetFloors = GameObject.FindObjectsOfType<XRTargetFloor>().ToList();

                if (xrTargetFloors != null && xrTargetFloors.Count > 0)
                {
                    foreach (XRTargetFloor xrTargetFloor in xrTargetFloors)
                    {
                        if( xrTargetFloor != null )
                        {
                            xrTargetFloor.OnFloorTrackingRestored();
                        }
                    }
                }
            }
            
        } //END SendFloorTrackingFoundEvent

        //------------------------------------//
        public void ForceDisableFloorTrackerIcon()
        //------------------------------------//
        {
#if VUFORIA
            if (floorIconColorTweener != null)
            {
                floorIconColorTweener.Force(UITweener.TweenValue.Hide);
            }
#endif

        } //END ForceDisableFloorTrackerIcon

        //------------------------------------//
        public override void DisableFloorTrackerIcon()
        //------------------------------------//
        {
#if VUFORIA
            if (floorIconColorTweener != null )
            {
                UnityEngine.Events.UnityEvent _event = new UnityEngine.Events.UnityEvent();
                _event.AddListener(DisableFloorTrackerIconComplete);

                //Debug.Log("XRCameraVuforia.cs DisableFloorTrackerIcon() calling icon.Play( Hide ) color.a = " + floorIconColorTweener.color_Hide.a);
                floorIconColorTweener.Play( UITweener.TweenValue.Hide, _event );

                //DOB NOTE: This is a hack to track when floor tracking is lost by looking at the visual tracker component
                SendFloorTrackingLostEvent();
            }
#endif

        } //END DisableFloorTrackerIcon

        //-----------------------------------------------//
        private void DisableFloorTrackerIconComplete()
        //-----------------------------------------------//
        {
#if VUFORIA
            //Debug.Log("XRCameraVuforia.cs DisableFloorTrackerIconComplete() color = " + floorIconColorTweener.GetColorFromRenderer());
#endif

        } //END DisableFloorTrackerIconComplete

        
        //------------------------------//
        /// <summary>
        /// Floor Trackers don't normally recieve the floor tracking lost event, we can instead send the event ourselves by checking for when our floor tracker icon disappears
        /// </summary>
        public void SendFloorTrackingLostEvent()
        //------------------------------//
        {
            
            if( GameObject.FindObjectOfType<XRTargetFloor>() != null )
            {
                List<XRTargetFloor> xrTargetFloors = GameObject.FindObjectsOfType<XRTargetFloor>().ToList();

                if (xrTargetFloors != null && xrTargetFloors.Count > 0)
                {
                    foreach (XRTargetFloor xrTargetFloor in xrTargetFloors)
                    {
                        if( xrTargetFloor != null )
                        {
                            xrTargetFloor.OnFloorTrackingLost();
                        }
                    }
                }
            }
            
        } //END SendFloorTrackingLostEvent

        //--------------------------------------//
        public override void SetFloorTrackerIconTexture( Texture newFloorIconTexture )
        //--------------------------------------//
        {
#if VUFORIA
            if ( newFloorIconTexture != null )
            {
                floorIconTexture = newFloorIconTexture;
            }
            
            if( floorIconTexture == null )
            {
                floorIconTexture = defaultFloorPlaneIconTexture;
            }

            //Debug.Log("floorIconTexture = " + floorIconTexture.name );

            if (floorIconColorTweener != null)
            {
                floorIconColorTweener.SetTexture(floorIconTexture);
            }
#endif

        } //END SetFloorTrackerIconTexture

    } //END Class

} //END Namespace