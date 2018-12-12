using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.VR;

namespace BrandXR
{
    /// This script provides an implemention of Unity's `BaseInputModule` class, so
    /// that Canvas-based (_uGUI_) UI elements can be selected by looking at them and
    /// pulling the viewer's trigger or touching the screen.
    /// This uses the player's gaze and the trigger as a raycast generator.
    ///
    /// To use, attach to the scene's **EventSystem** object.  Be sure to move it above the
    /// other modules, such as 'TouchInputModule' and 'StandaloneInputModule', in order
    /// for the user's gaze to take priority in the event system.
    ///
    /// Next, set the Canvas object's Render Mode to 'World Space', and set its Event Camera
    /// to a (mono) camera that is controlled by a VRHead.  If you'd like gaze to work
    /// with 3D scene objects, add a PhysicsRaycaster to the gazing camera, and add a
    /// component that implements one of the Event interfaces (EventTrigger will work nicely).
    /// The objects must have colliders too.
    ///
    /// XRGazeInputModule emits the following events: _Enter_, _Exit_, _Down_, _Up_, _Click_, _Select_,
    /// _Deselect_, and _UpdateSelected_.  Scroll, move, and submit/cancel events are not emitted.
    public class XRGazeInputModule: BaseInputModule
    {

        // The GazePointer which will be responding to gaze events.
        public static XRGazePointer gazePointer;

        //Pointer data used to Raycast UGUI
        private PointerEventData pointerData;

        //The previous headpose
        private Vector2 lastHeadPose;

        // Time in seconds between the pointer down and up events sent by a trigger.
        // Allows time for the UI elements to make their state transitions.
        private const float clickTime = 0.1f;

        //What layers should we check for objects on?
        public LayerMask CollideWithLayers = -1;

        //What Tag is the object required to have in order to count towards the raycast?
        public List<string> RequiredTag = new List<string>();

        //Store the "Center of the Screen", which changes depending on plugins being used
        private Vector2 centerOfScreen = new Vector2();

        //Should we show the raycast in the editor?
        public bool showRaycast = true;
        public Color rayColor = Color.blue;

        //Singleton behavior
        private static XRGazeInputModule _instance;

        //--------------------------------------------//
        public static XRGazeInputModule instance
        //--------------------------------------------//
        {
            get
            {
                if( _instance == null )
                {
                    _instance = GameObject.FindObjectOfType<XRGazeInputModule>();
                }

                return _instance;
            }

        } //END Instance

        //-------------------------------//
        protected override void Start()
        //-------------------------------//
        {
            
            base.Start();

            CreateVRReticle();

            CreatePhysicsRaycaster();

            SetCenterOfScreen();
            
        } //END Start

        //-----------------------------------//
        private void CreateVRReticle()
        //-----------------------------------//
        {
            if( Camera.main != null && PrefabManager.instance != null && gazePointer == null )
            {
                XRReticleRing vrReticleRing = PrefabManager.InstantiatePrefab( PrefabFactory.Prefabs.bxr_XRRingReticle ).GetComponent<XRReticleRing>();

                vrReticleRing.transform.parent = Camera.main.transform;

                vrReticleRing.transform.localPosition = Vector3.zero;
                vrReticleRing.transform.localEulerAngles = Vector3.zero;
                vrReticleRing.transform.localScale = Vector3.one;
            }
            
        } //END CreateVRReticle

        //-----------------------------------//
        private void CreatePhysicsRaycaster()
        //-----------------------------------//
        {
            if( Camera.main != null && GameObject.FindObjectOfType<PhysicsRaycaster>() == null )
            {
                Camera.main.gameObject.AddComponent<PhysicsRaycaster>();
            }

        } //END CreatePhysicsRaycaster


        //-----------------------------------//
        public void SetCenterOfScreen()
        //-----------------------------------//
        {

            // Center of screen is center of eye texture width
            if( XRMode.IsVRModeOn() )
            {
                centerOfScreen = new Vector2( (float)UnityEngine.XR.XRSettings.eyeTextureWidth / 2, (float)UnityEngine.XR.XRSettings.eyeTextureHeight / 2 );
            }
            // VR support is not on (Using Cardboard SDK)
            else
            {
                // Get center of screen the normal way
                centerOfScreen = new Vector2( Screen.width / 2, Screen.height / 2 );
            }

            //Debug.Log( "SetCenterOfScreen() GazeInputEnabled... centerOfScreen = " + centerOfScreen );

        } //END SetCenterOfScreen



        //---------------------------------//
        //Runs every frame
        public override void Process()
        //---------------------------------//
        {
            //Set the center of the screen, changes when VRMode changes
            SetCenterOfScreen();
            
            // Save the previous Game Object
            GameObject gazeObjectPrevious = GetCurrentGameObject();

            // Cast the ray from gaze
            CastRayFromGaze();

            //Show the raycast in editor (for debugging purposes)
            ShowDebugRay();

            // Check if there is a gameObject at the gaze point
            GameObject gameObjectAtGaze = pointerData.pointerCurrentRaycast.gameObject;

            //If there is a gameobject at the gaze position and if the object is in the correct layer and has the correct tag, then perform GUI logic
            //Also check if the object has a GazeCollider script attached, if it does, check if ShouldGazeCursorFill() returns true
            if( gameObjectAtGaze != null && ( gameObjectAtGaze.GetComponent<XRInteractiveUI>() != null || ( IsInLayerMask( gameObjectAtGaze.layer, CollideWithLayers ) && HasTag( gameObjectAtGaze, RequiredTag ) ) ) && GazeCollider.ShouldGazeCursorFill( gameObjectAtGaze ) )
            {
                //Debug.Log( "Found Object : Name = " + gameObjectAtGaze.name );
                UpdateCurrentObject();
                UpdateReticle( gazeObjectPrevious );
            }
            else
            {
                //Debug.Log( "No Acceptable Object... gameObjectAtGaze != null = " + ( gameObjectAtGaze != null ) );
                //if( gameObjectAtGaze != null ) { Debug.Log( "No Acceptable Object... IsInLayerMask = " + IsInLayerMask( gameObjectAtGaze.layer, CollideWithLayers ) ); }
                //if( gameObjectAtGaze != null ) { Debug.Log( "No Acceptable Object... HasTag = " + HasTag( gameObjectAtGaze, RequiredTag ) ); }
                
                HandlePointerExitAndEnter( pointerData, null );
                pointerData = new PointerEventData( eventSystem );
                UpdateReticle( gazeObjectPrevious );
            }

            //Is the trigger button being pressed Down on this frame?
            bool isTriggered = Input.GetMouseButtonDown( 0 );

            //Was the 'Click' button NOT being held down previously?
            bool handlePendingClickRequired = !Input.GetMouseButton( 0 );


            //If this is NOT the first frame of holding down the button, then process any Drag input that might be necessary
            if( !Input.GetMouseButtonDown( 0 ) && Input.GetMouseButton( 0 ) )
            {
                HandleDrag();
            }
            else if( Time.unscaledTime - pointerData.clickTime < clickTime )
            {
                // Delay new events until clickTime has passed.
            }
            else if( !pointerData.eligibleForClick && isTriggered )
            {
                // New trigger action.
                HandleTrigger();
            }
            else if( handlePendingClickRequired )
            {
                // Check if there is a pending click to handle.
                HandlePendingClick();
            }

        } //END Process


        //--------------------------------------//
        private void CastRayFromGaze()
        //--------------------------------------//
        {

            Vector2 headPose = NormalizedCartesianToSpherical( Camera.main.transform.rotation * Vector3.forward );

            if( pointerData == null )
            {
                pointerData = new PointerEventData( eventSystem );
                lastHeadPose = headPose;
            }

            // Cast a ray into the scene using the Unity UGUI pointer logic
            pointerData.Reset();

            pointerData.position = centerOfScreen;

            eventSystem.RaycastAll( pointerData, m_RaycastResultCache );

            pointerData.pointerCurrentRaycast = FindFirstRaycast( m_RaycastResultCache );

            if( pointerData.pointerCurrentRaycast.gameObject != null )
            {
                if( pointerData.pointerCurrentRaycast.gameObject.name == "VRMain" )
                {
                    if( m_RaycastResultCache.Count > 1 )
                    {
                        pointerData.pointerCurrentRaycast = m_RaycastResultCache[ 1 ];
                    }
                    else
                    {
                        pointerData.Reset();
                    }
                }
            }

            m_RaycastResultCache.Clear();

            pointerData.delta = headPose - lastHeadPose;

            lastHeadPose = headPose;
            
        } //END CastFromGaze

        //--------------------------------------//
        private void ShowDebugRay()
        //--------------------------------------//
        {
            
            #if UNITY_EDITOR
                if( showRaycast )
                {
                    Debug.DrawRay( Camera.main.transform.position, Camera.main.transform.forward * 9999f, rayColor );
                }
            #endif

        } //END ShowDebugRay

        //--------------------------------------//
        private void UpdateCurrentObject()
        //--------------------------------------//
        {
            // Send enter events and update the highlight.
            var go = pointerData.pointerCurrentRaycast.gameObject;

            HandlePointerExitAndEnter( pointerData, go );

            // Update the current selection, or clear if it is no longer the current object.
            var selected = ExecuteEvents.GetEventHandler<ISelectHandler>( go );

            if( selected == eventSystem.currentSelectedGameObject )
            {
                ExecuteEvents.Execute( eventSystem.currentSelectedGameObject, GetBaseEventData(), ExecuteEvents.updateSelectedHandler );
            }
            else
            {
                eventSystem.SetSelectedGameObject( null, pointerData );
            }

        } //END UpdateCurrentObject

        //--------------------------------------//
        private void HandleDrag()
        //--------------------------------------//
        {
            bool moving = pointerData.IsPointerMoving();

            if( moving && pointerData.pointerDrag != null && !pointerData.dragging )
            {
                ExecuteEvents.Execute( pointerData.pointerDrag, pointerData,
                    ExecuteEvents.beginDragHandler );
                pointerData.dragging = true;
            }

            // Drag notification
            if( pointerData.dragging && moving && pointerData.pointerDrag != null )
            {
                // Before doing drag we should cancel any pointer down state
                // And clear selection!
                if( pointerData.pointerPress != pointerData.pointerDrag )
                {
                    ExecuteEvents.Execute( pointerData.pointerPress, pointerData, ExecuteEvents.pointerUpHandler );

                    pointerData.eligibleForClick = false;
                    pointerData.pointerPress = null;
                    pointerData.rawPointerPress = null;
                }

                ExecuteEvents.Execute( pointerData.pointerDrag, pointerData, ExecuteEvents.dragHandler );
            }

        } //END HandleDrag

        //--------------------------------------//
        private void HandlePendingClick()
        //--------------------------------------//
        {
            if( !pointerData.eligibleForClick && !pointerData.dragging )
            {
                return;
            }

            if( gazePointer != null )
            {
                Camera camera = pointerData.enterEventCamera;
                gazePointer.OnGazeTriggerEnd( camera );
            }

            var go = pointerData.pointerCurrentRaycast.gameObject;

            // Send pointer up and click events.
            ExecuteEvents.Execute( pointerData.pointerPress, pointerData, ExecuteEvents.pointerUpHandler );

            if( pointerData.eligibleForClick )
            {
                ExecuteEvents.Execute( pointerData.pointerPress, pointerData, ExecuteEvents.pointerClickHandler );
            }
            else if( pointerData.dragging )
            {
                ExecuteEvents.ExecuteHierarchy( go, pointerData, ExecuteEvents.dropHandler );
                ExecuteEvents.Execute( pointerData.pointerDrag, pointerData, ExecuteEvents.endDragHandler );
            }

            // Clear the click state.
            pointerData.pointerPress = null;
            pointerData.rawPointerPress = null;
            pointerData.eligibleForClick = false;
            pointerData.clickCount = 0;
            pointerData.clickTime = 0;
            pointerData.pointerDrag = null;
            pointerData.dragging = false;

        } //END HandlePendingClick

        //--------------------------------------//
        private void HandleTrigger()
        //--------------------------------------//
        {
            var go = pointerData.pointerCurrentRaycast.gameObject;

            // Send pointer down event.
            pointerData.pressPosition = pointerData.position;
            pointerData.pointerPressRaycast = pointerData.pointerCurrentRaycast;
            pointerData.pointerPress =
                ExecuteEvents.ExecuteHierarchy( go, pointerData, ExecuteEvents.pointerDownHandler )
                ?? ExecuteEvents.GetEventHandler<IPointerClickHandler>( go );

            // Save the drag handler as well
            pointerData.pointerDrag = ExecuteEvents.GetEventHandler<IDragHandler>( go );
            if( pointerData.pointerDrag != null )
            {
                ExecuteEvents.Execute( pointerData.pointerDrag, pointerData, ExecuteEvents.initializePotentialDrag );
            }

            // Save the pending click state.
            pointerData.rawPointerPress = go;
            pointerData.eligibleForClick = true;
            pointerData.delta = Vector2.zero;
            pointerData.dragging = false;
            pointerData.useDragThreshold = true;
            pointerData.clickCount = 1;
            pointerData.clickTime = Time.unscaledTime;

            if( gazePointer != null )
            {
                gazePointer.OnGazeTriggerStart( pointerData.enterEventCamera );
            }

        } //END HandleTrigger

        //--------------------------------------//
        Vector3 GetIntersectionPosition()
        //--------------------------------------//
        {
            // Check for camera
            Camera cam = pointerData.enterEventCamera;

            if( cam == null )
            {
                return Vector3.zero;
            }

            float intersectionDistance = pointerData.pointerCurrentRaycast.distance + cam.nearClipPlane;

            Vector3 intersectionPosition = cam.transform.position + cam.transform.forward * intersectionDistance;

            return intersectionPosition;

        } //END GetIntersectionPosition

        //-------------------------------------------------------------//
        private void UpdateReticle( GameObject previousGazedObject )
        //-------------------------------------------------------------//
        {
            if( gazePointer == null )
            {
                return;
            }

            Camera camera = pointerData.enterEventCamera; // Get the camera
            GameObject gazeObject = GetCurrentGameObject(); // Get the gaze target
            Vector3 intersectionPosition = GetIntersectionPosition();

            bool isInteractive = ( pointerData.pointerPress != null || ExecuteEvents.GetEventHandler<IPointerClickHandler>( gazeObject ) != null ) && GazeCollider.ShouldGazeCursorGrow( gazeObject );

            if( gazeObject == previousGazedObject )
            {
                if( gazeObject != null )
                {
                    gazePointer.OnGazeStay( camera, gazeObject, intersectionPosition, isInteractive );
                }
            }
            else
            {
                if( previousGazedObject != null )
                {
                    gazePointer.OnGazeExit( camera, previousGazedObject );
                }
                if( gazeObject != null )
                {
                    gazePointer.OnGazeStart( camera, gazeObject, intersectionPosition, isInteractive );
                }
            }

        } //END GazePointer

        //------------------------------------------//
        public override void DeactivateModule()
        //------------------------------------------//
        {
            
            DisableGazePointer();
            base.DeactivateModule();

            if( pointerData != null )
            {
                HandlePendingClick();
                HandlePointerExitAndEnter( pointerData, null );
                pointerData = null;
            }

            eventSystem.SetSelectedGameObject( null, GetBaseEventData() );
            
        } //END DeactivateModule

        //------------------------------------------//
        private void DisableGazePointer()
        //------------------------------------------//
        {
            if( gazePointer == null )
            {
                return;
            }

            GameObject currentGameObject = GetCurrentGameObject();

            if( currentGameObject )
            {
                Camera camera = pointerData.enterEventCamera;
                gazePointer.OnGazeExit( camera, currentGameObject );
            }

            gazePointer.OnGazeDisabled();

        } //END DisableGazePointer

        //---------------------------------------------------------------//
        private Vector2 NormalizedCartesianToSpherical( Vector3 cartCoords )
        //---------------------------------------------------------------//
        {
            cartCoords.Normalize();

            if( cartCoords.x == 0 )
                cartCoords.x = Mathf.Epsilon;

            float outPolar = Mathf.Atan( cartCoords.z / cartCoords.x );

            if( cartCoords.x < 0 )
                outPolar += Mathf.PI;

            float outElevation = Mathf.Asin( cartCoords.y );

            return new Vector2( outPolar, outElevation );

        } //END NormalizedCartesianToSpherical

        //---------------------------------------------------------------//
        public override bool IsPointerOverGameObject( int pointerId )
        //---------------------------------------------------------------//
        {
            return pointerData != null && pointerData.pointerEnter != null;

        } //END IsPointerOverGameObject

        //---------------------------------------------------------------//
        private GameObject GetCurrentGameObject()
        //---------------------------------------------------------------//
        {

            if( pointerData != null && pointerData.enterEventCamera != null )
            {
                return pointerData.pointerCurrentRaycast.gameObject;
            }

            return null;

        } //END GetCurrentGameObject

        //---------------------------------------------------------------//
        private bool IsInLayerMask( int layer, LayerMask layermask )
        //---------------------------------------------------------------//
        {
            return layermask == ( layermask | ( 1 << layer ) );

        } //END IsInLayerMask

        //---------------------------------------------------------------//
        private bool HasTag( GameObject go, List<string> tagList )
        //---------------------------------------------------------------//
        {
            if( tagList == null || ( tagList != null && tagList.Count == 0 ) )
            {
                return true;
            }
            else if( tagList != null && tagList.Contains( go.tag ) )
            {
                return true;
            }

            return false;

        } //END HasTag



    } //END Class

} //END Namespace