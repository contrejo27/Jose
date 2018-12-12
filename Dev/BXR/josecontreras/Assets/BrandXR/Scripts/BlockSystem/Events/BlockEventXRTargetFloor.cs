using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using Sirenix.OdinInspector;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace BrandXR
{
    public class BlockEventXRTargetFloor: BlockEventBase
    {

        //---------------- SETUP ERRORS -----------------------------//
        [ShowIf( "IsMissingXRCameraManager" ), Button( "Create XR Camera Manager", ButtonSizes.Large ), InfoBox( "WARNING: Missing bxr_XRCameraManager in hierarchy which is required for the BlockEventXRTargetFloor to work.\n\nPress the button below to create the bxr_XRCameraManager now", InfoMessageType.Error )]
        public void CreateXRCameraManager()
        {
#if UNITY_EDITOR
            if( IsMissingXRCameraManager() )
            {
                GameObject prefab = (GameObject)AssetDatabase.LoadAssetAtPath( "Assets/BrandXR/Prefabs/Cameras/bxr_XRCameraManager.prefab", typeof( GameObject ) );
                GameObject go = Instantiate( prefab );

                go.name = "bxr_XRCameraManager";
                BlockHelper.AddToBrandXRTechParent( go.transform );
            }
#endif
        }

        private bool IsMissingXRCameraManager()
        {
            if( GameObject.FindObjectOfType<XRCameraManager>() == null )
            {
                return true;
            }

            return false;
        }


        //---------------- SETUP ERRORS -----------------------------//
        [ShowIf( "IsMissingXRInputManager" ), Button( "Create XR Input Manager", ButtonSizes.Large ), InfoBox( "WARNING: Missing bxr_XRInputManager in hierarchy which is required for the BlockEventXRTargetFloor to work.\n\nPress the button below to create the bxr_XRInputManager now", InfoMessageType.Error )]
        public void CreateXRInputManager()
        {
#if UNITY_EDITOR
            if( IsMissingXRInputManager() )
            {
                GameObject prefab = (GameObject)AssetDatabase.LoadAssetAtPath( "Assets/BrandXR/Prefabs/bxr_XRInputManager.prefab", typeof( GameObject ) );
                GameObject go = Instantiate( prefab );

                go.name = "bxr_XRInputManager";
                BlockHelper.AddToBrandXRTechParent( go.transform );
            }
#endif
        }

        private bool IsMissingXRInputManager()
        {
            if( GameObject.FindObjectOfType<XRInputManager>() == null )
            {
                return true;
            }

            return false;
        }

        //----------------- ACTIONS --------------------------------//

        public enum Action
        {
            None,
            EnableSetTransformToFloor,
            DisableSetTransformToFloor,
            ReEnableSetTransformToFloor
        }
        public Action action = Action.None;
        private bool IsActionEnableSetTransformToFloor() { return action == Action.EnableSetTransformToFloor; }
        private bool IsActionDisableSetTransformToFloor() { return action == Action.DisableSetTransformToFloor; }
        private bool IsActionReEnableSetTransformToFloor() { return action == Action.ReEnableSetTransformToFloor; }
        private bool IsActionDisableOrReEnableSetTransformToFloor() { return action == Action.DisableSetTransformToFloor || action == Action.ReEnableSetTransformToFloor; }
        private bool IsActionEnableOrDisableSetTransformToFloor() { return action == Action.EnableSetTransformToFloor || action == Action.DisableSetTransformToFloor || action == Action.ReEnableSetTransformToFloor; }

        //----------------- "DISABLE XR TARGET FLOOR" VARIABLES -----------------------------//
        [Space( 15f ), ShowIf( "IsActionDisableOrReEnableSetTransformToFloor" ), InfoBox("Set this to the Block Event - XR Target Floor that has an action of 'EnableSetTransformToFloor'")]
        public BlockEventXRTargetFloor blockEventXRTargetFloor = null;

        //----------------- "ENABLE XR TARGET FLOOR" VARIABLES ------------------------------//
        [Space( 15f ), FoldoutGroup("Hooks")]
        public bool enableSetTransformToFloorEvent = false;

        [ Space( 15f ), ShowIf( "IsActionEnableSetTransformToFloor" ), InfoBox("The XRTargetFloor BlockGroup to use for floor pressed events")]
        public XRTargetFloor xrTargetFloor = null;

        public enum FloorRaycastStartsFrom
        {
            CenterOfScreen,
            CenterOfPress
        }
        [ShowIf( "IsActionEnableSetTransformToFloor" )]
        public FloorRaycastStartsFrom raycastStartsFrom = FloorRaycastStartsFrom.CenterOfPress;
        private bool IsFloorRaycastFromScreenCenter() { return IsActionEnableSetTransformToFloor() && raycastStartsFrom == FloorRaycastStartsFrom.CenterOfScreen; }
        private bool IsFloorRaycastFromPressCenter() { return IsActionEnableSetTransformToFloor() && raycastStartsFrom == FloorRaycastStartsFrom.CenterOfPress; }
        private bool IsFloorRaycastFromScreenOrPress() { return IsActionEnableSetTransformToFloor() && ( raycastStartsFrom == FloorRaycastStartsFrom.CenterOfPress || raycastStartsFrom == FloorRaycastStartsFrom.CenterOfScreen ); }

        public enum TransformType
        {
            Transform,
            BlockModel
        }
        [Space( 15f ), ShowIf( "IsActionEnableSetTransformToFloor" )]
        public TransformType transformType = TransformType.BlockModel;
        private bool IsTransformTypeBlockModel() { return IsActionEnableSetTransformToFloor() && transformType == TransformType.BlockModel; }
        private bool IsTransformTypeTransform() { return IsActionEnableSetTransformToFloor() && transformType == TransformType.Transform; }

        [ShowIf( "IsTransformTypeBlockModel" )]
        public BlockModel blockModelToMoveOnFloor = null;

        [ShowIf( "IsTransformTypeTransform" )]
        public Transform transformToMoveOnFloor = null;

        [ShowIf( "IsFloorRaycastFromScreenCenter" )]
        public bool setToRotation = false;

        [Space( 15f ), ShowIf( "IsFloorRaycastFromScreenOrPress" )]
        public bool setOnPress = false;
        private bool ShowSetOnPressEvent() { return IsFloorRaycastFromScreenOrPress() && setOnPress; }

        [ShowIf( "IsFloorRaycastFromScreenOrPress" )]
        public bool setOnDrag = false;
        private bool LookingForDragGesture() { return IsFloorRaycastFromScreenOrPress() && setOnDrag; }

        [Space( 15f ), ShowIf( "LookingForDragGesture" ), InfoBox("Enable this option to give the end of a drag event a soft tween out")]
        public bool easeOutDrag = true;
        private bool IsEaseOutDragEnabled() { return LookingForDragGesture() && easeOutDrag; }

        [ShowIf( "IsEaseOutDragEnabled" )]
        public float dragFinishEaseSpeed = 3f;


        [Space( 15f ), InfoBox( "Select to require that the user start a drag command by first pressing on a collider" ), ShowIf( "LookingForDragGesture" )]
        public bool requireDragStartOnCollider = false;
        private bool RequireDragCollider() { return LookingForDragGesture() && requireDragStartOnCollider; }
        
        public enum CollisionReference
        {
            BlockModel,
            BlockButton,
            Collider,
            ColliderWithTag,
            Collider2D,
            Collider2DWithTag
        }
        [ShowIf( "RequireDragCollider" )]
        public CollisionReference lookForWhenStartingDrag = CollisionReference.Collider;
        private bool LookForBlockModel() { return RequireDragCollider() && lookForWhenStartingDrag == CollisionReference.BlockModel; }
        private bool LookForBlockButton() { return RequireDragCollider() && lookForWhenStartingDrag == CollisionReference.BlockButton; }
        private bool LookForCollider() { return RequireDragCollider() && lookForWhenStartingDrag == CollisionReference.Collider; }
        private bool LookForTag3D() { return RequireDragCollider() && lookForWhenStartingDrag == CollisionReference.ColliderWithTag; }
        private bool LookForCollider2D() { return RequireDragCollider() && lookForWhenStartingDrag == CollisionReference.Collider2D; }
        private bool LookForTag2D() { return RequireDragCollider() && lookForWhenStartingDrag == CollisionReference.Collider2DWithTag; }
        private bool LookForTag() { return RequireDragCollider() && ( lookForWhenStartingDrag == CollisionReference.ColliderWithTag || lookForWhenStartingDrag == CollisionReference.Collider2DWithTag ); }

        [ShowIf( "LookForBlockModel" )]
        public BlockModel lookForBlockModel = null;

        [ShowIf( "LookForBlockButton" )]
        public BlockButton lookForBlockButton = null;

        [ShowIf( "LookForCollider" )]
        public Collider lookForCollider = null;

        [ShowIf( "LookForTag" )]
        public string lookForTag = "";

        [ShowIf( "LookForCollider2D" )]
        public Collider2D lookForCollider2D = null;


        private bool IsRepositionWhenPress() { return IsFloorRaycastFromScreenOrPress() && setOnPress; }
        private bool IsRepositionWhenDrag() { return IsFloorRaycastFromScreenOrPress() && setOnDrag; }

        private Vector3 pressDistance = Vector3.zero;
        private float pressPositionX = 0f;
        private float pressPositionY = 0f;

        //Track when the mouse is dragging
        private bool isFirstFrameOfMouseDown = false;
        private bool isMouseBeingDragged = false;

        //Track when the touch is dragging
        private bool isTouchBeingDragged = false;

        //Track when, during a floor press or floor drag gesture, there have been more than one finger on the screen (indicating we just ended a pinch or twist gesture)
        //Used to prevent a floor press or floor drag event
        private bool didTouchCountReachTwoDuringGesture = false;

        //Track when, during a floor drag gesture, there has been enough movement to prevent the Press To Move gesture from taking effect when the finger releases
        private bool didDragCountPreventPressToMoveGesture = false;
        private Vector2 initialDragPosition = Vector2.zero;
        private float currentDragDistance = 0f;
        private float maxDragDistanceBeforePreventingPressToMove = 75f;

        //----------------- "XR TARGET FLOOR" EVENTS ------------------------------//
        [ ShowIf( "ShowSetOnPressEvent" ), FoldoutGroup( "Event Messages" )]
        public UnityEvent onPressComplete = new UnityEvent();

        [ShowIf( "LookingForDragGesture" ), FoldoutGroup( "Event Messages" )]
        public UnityEvent onDragComplete = new UnityEvent();


        //---------------------------------------------------------//
        public override EventType GetEventType()
        //---------------------------------------------------------//
        {
            return EventType.XRTargetFloor;

        } //END GetEventType

        //---------------------------------------------------------//
        public void SetAction( Action action )
        //---------------------------------------------------------//
        {

            this.action = action;

        } //END SetAction

        //-------------------------------//
        public override void PrepareEvent()
        //-------------------------------//
        {

            if( action == Action.EnableSetTransformToFloor )
            {
                if( xrTargetFloor != null )
                {
                    if( IsTransformTypeBlockModel() && blockModelToMoveOnFloor != null )
                    {
                        blockModelToMoveOnFloor.SetDragEndEaseOutSpeed( dragFinishEaseSpeed );
                        eventReady = true;
                    }
                    else if( IsTransformTypeTransform() && transformToMoveOnFloor != null )
                    {
                        eventReady = true;
                    }
                }
            }
            else if( action == Action.DisableSetTransformToFloor )
            {
                if( blockEventXRTargetFloor != null )
                {
                    eventReady = true;
                }
            }
            else if( action == Action.ReEnableSetTransformToFloor )
            {
                if( blockEventXRTargetFloor != null )
                {
                    eventReady = true;
                }
            }

        } //END PrepareEvent
        

        //-------------------------------//
        protected override void _CallEvent()
        //-------------------------------//
        {
            base._CallEvent();

            if( eventReady )
            {
                if( action == Action.EnableSetTransformToFloor )
                {
                    EnableSetTransformToFloorEvent();
                }
                else if( action == Action.DisableSetTransformToFloor )
                {
                    CallDisableSetTransformToFloorEvent();
                }
                else if( action == Action.ReEnableSetTransformToFloor )
                {
                    CallReEnableTransformToFloorEvent();
                }
            }

        } //END CallEvent

        //------------------------------//
        public void Update()
        //------------------------------//
        {

            if( enableSetTransformToFloorEvent && eventReady )
            {
                
                if( xrTargetFloor != null && 
                    xrTargetFloor.IsFloorPlaneReady() )
                {
                    UpdateTouchCountTracking();

                    StoreInitialDragPosition();

                    UpdateDragCountTracking();

                    if( IsRepositionWhenPress() )
                    {
                        CheckForFloorPress();
                    }
                    if( IsRepositionWhenDrag() )
                    {
                        CheckForFloorDrag();
                    }

                    ResetTouchCountTracking();

                    ResetDragCountTracking();
                }
                
            }

        } //END Update

        //------------------------------//
        /// <summary>
        /// Tracks when, during a touch gesture, the user touches the screen with two fingers.
        /// If this occurs, then we prevent any Press To Move or Press To Drag events from occuring.
        /// This is to stop interference with Pinch and Twist gestures
        /// </summary>
        private void UpdateTouchCountTracking()
        //------------------------------//
        {
            //If we haven't already marked this gesture as having used 2 fingers, check for our finger count
            if( !didTouchCountReachTwoDuringGesture )
            {
                if( Input.touchSupported &&
                    Input.touchCount == 2 &&
                    Input.GetTouch(1).phase == TouchPhase.Began )
                {
                    didTouchCountReachTwoDuringGesture = true;
                }
            }

        } //END UpdateTouchCountTracking

        //---------------------------------------//
        /// <summary>
        /// If we've touched the screen with two fingers, then we've prevents Press to Move and Drag to Move events
        /// This function checks if we should reset the flag that will allow those events to occur in the next gesture
        /// </summary>
        private void ResetTouchCountTracking()
        //---------------------------------------//
        {

            if( didTouchCountReachTwoDuringGesture )
            {
                if( Input.touchSupported &&
                    Input.touchCount == 0 )
                {
                    didTouchCountReachTwoDuringGesture = false;
                }
            }

        } //END ResetTouchCountTracking

        //-------------------------------------//
        /// <summary>
        /// Track the starting position of the Mouse/Touch, used to see how far the input has dragged which determines if the Press To Move event should be prevented
        /// </summary>
        private void StoreInitialDragPosition()
        //-------------------------------------//
        {

            //Store the initial position of the mouse click
            if( Input.mousePresent &&
                Input.GetMouseButtonDown( 0 ) )
            {
                initialDragPosition = Input.mousePosition;
            }

            //Otherwise store the initial position of the touch input
            else if( Input.touchSupported &&
                Input.touchCount == 1 &&
                Input.GetTouch( 0 ).phase == TouchPhase.Began )
            {
                initialDragPosition = Input.GetTouch( 0 ).position;
            }

        } //END StoreInitialDragPosition
        
        
        //-------------------------------------//
        /// <summary>
        /// If a touch input drags for long enough, then we prevent the Press to Move event when the finger is lifted
        /// This prevents a drag from calling a Press To Move event accidentally
        /// </summary>
        private void UpdateDragCountTracking()
        //-------------------------------------//
        {
            
            //Count the amount of drag distance, but only if we haven't already prevent the Press To Move gesture due to too much movement
            if( !didDragCountPreventPressToMoveGesture )
            {
                //Add the distance the mouse input has moved
                if( Input.mousePresent &&
                    Input.GetMouseButton( 0 ) )
                {
                    //For mouse input, we check how much the mouse has moved, if it's over a threshold then we prevent the Press to Move gesture
                    currentDragDistance += Vector2.Distance( Input.mousePosition, initialDragPosition );
                    //Debug.Log( "Dragging.. current = " + currentDragDistance );
                }

                //Otherwise if we're using Touch input, store that change in distance instead
                else if( Input.touchSupported &&
                    Input.touchCount == 1 &&
                    Input.GetTouch( 0 ).phase == TouchPhase.Moved )
                {
                    currentDragDistance += Vector2.Distance( Input.GetTouch( 0 ).position, initialDragPosition );
                    //Debug.Log( "Dragging.. current = " + currentDragDistance );
                }

                //If the amount of distance we have dragged is greater than our threshhold, then set a flag that will prevent the Press To Move event once the input is released
                if( Mathf.Abs( currentDragDistance ) >= maxDragDistanceBeforePreventingPressToMove )
                {
                    //Debug.Log( "Prevent.. current(" + currentDragDistance + ") >= maxAllowed(" + maxDragDistanceBeforePreventingPressToMove + ")" );
                    didDragCountPreventPressToMoveGesture = true;
                }
            }
            
        } //END UpdateDragCountTracking

        //------------------------------//
        private void ResetDragCountTracking()
        //------------------------------//
        {
            //We only have to worry about resetting this flag if it's been set to true
            if( didDragCountPreventPressToMoveGesture )
            {
                //Reset the drag count when the mouse button is released
                if( Input.mousePresent &&
                    Input.GetMouseButtonUp( 0 ) )
                {
                    currentDragDistance = 0;
                    didDragCountPreventPressToMoveGesture = false;
                }

                //Reset the drag count when the Touch input is released
                else if( Input.touchSupported &&
                    Input.touchCount == 0 )
                {
                    currentDragDistance = 0;
                    didDragCountPreventPressToMoveGesture = false;
                }
            }
            
        } //END ResetDragCountTracking

        

        //------------------------------//
        private void CheckForFloorPress()
        //------------------------------//
        {

            bool callPressComplete = false;

            if( Input.mousePresent &&
                Input.GetMouseButtonUp( 0 ) &&
                !didDragCountPreventPressToMoveGesture )
            {
                if( DoRequiredComponentsExist() )
                {
                    if( IsFloorRaycastFromScreenCenter() )
                    {
                        if( IsTransformTypeBlockModel() && blockModelToMoveOnFloor != null )
                        {
                            callPressComplete = SetTransformOnFloorToPressPosition( blockModelToMoveOnFloor.GetMoveTransform() );
                        }
                        else if( IsTransformTypeTransform() && transformToMoveOnFloor != null )
                        {
                            callPressComplete = SetTransformOnFloorToPressPosition( transformToMoveOnFloor );
                        }
                    }
                    else if( IsFloorRaycastFromPressCenter() )
                    {
                        if( IsTransformTypeBlockModel() && blockModelToMoveOnFloor != null )
                        {
                            callPressComplete = SetTransformOnFloorToPressPosition( XRCameraManager.instance.GetXRCamera().GetCameras()[ 0 ], blockModelToMoveOnFloor.GetMoveTransform(), Input.mousePosition.x, Input.mousePosition.y );
                        }
                        else if( IsTransformTypeTransform() && transformToMoveOnFloor != null )
                        {
                            callPressComplete = SetTransformOnFloorToPressPosition( XRCameraManager.instance.GetXRCamera().GetCameras()[ 0 ], transformToMoveOnFloor, Input.mousePosition.x, Input.mousePosition.y );
                        }
                    }
                }
            }
            else if( Input.touchSupported &&
                     Input.touchCount == 1 &&
                     Input.GetTouch( 0 ).phase == TouchPhase.Ended &&
                     !didTouchCountReachTwoDuringGesture &&
                     !didDragCountPreventPressToMoveGesture )
            {
                if( DoRequiredComponentsExist() )
                {
                    if( IsFloorRaycastFromScreenCenter() )
                    {
                        if( IsTransformTypeBlockModel() && blockModelToMoveOnFloor != null )
                        {
                            callPressComplete = SetTransformOnFloorToPressPosition( blockModelToMoveOnFloor.GetMoveTransform() );
                        }
                        else if( IsTransformTypeTransform() && transformToMoveOnFloor != null )
                        {
                            callPressComplete = SetTransformOnFloorToPressPosition( transformToMoveOnFloor );
                        }
                    }
                    else if( IsFloorRaycastFromPressCenter() )
                    {
                        if( IsTransformTypeBlockModel() && blockModelToMoveOnFloor != null )
                        {
                            callPressComplete = SetTransformOnFloorToPressPosition( XRCameraManager.instance.GetXRCamera().GetCameras()[ 0 ], blockModelToMoveOnFloor.GetMoveTransform(), Input.GetTouch( 0 ).position.x, Input.GetTouch( 0 ).position.y );
                        }
                        else if( IsTransformTypeTransform() && transformToMoveOnFloor != null )
                        {
                            callPressComplete = SetTransformOnFloorToPressPosition( XRCameraManager.instance.GetXRCamera().GetCameras()[ 0 ], transformToMoveOnFloor, Input.GetTouch( 0 ).position.x, Input.GetTouch( 0 ).position.y );
                        }
                    }
                }
            }

            //Call the press complete event if it has been setup
            if( callPressComplete &&
                ShowSetOnPressEvent() &&
                onPressComplete != null )
            {
                //If this is the last frame of a drag, 
                //let the BlockModel know, this will stop any rigidbody drag that may be applied to the BlockModel
                if( LookForBlockModel() && lookForBlockModel != null )
                {
                    lookForBlockModel.PressEventEnd();
                }

                onPressComplete.Invoke();
            }

        } //END CheckForFloorPress
        
        //------------------------------//
        private void CheckForFloorDrag()
        //------------------------------//
        {
            
            if( DoRequiredComponentsExist() )
            {
                //If this is the last frame of a drag, let the BlockModel know if that is the collider type we are dragging
                if( ShouldBlockModelDragEventEndBeCalled() )
                {
                    lookForBlockModel.DragEventEnd();
                }

                if( CheckIfMouseIsBeingDragged() )
                {
                    if( ShouldBlockModelDragEventEndBeCalled() )
                    {
                        lookForBlockModel.DragEventOccuring();
                    }

                    if( IsFloorRaycastFromScreenCenter() )
                    {
                        if( IsTransformTypeBlockModel() && blockModelToMoveOnFloor != null )
                        {
                            SetTransformOnFloorToPressPosition( blockModelToMoveOnFloor.GetMoveTransform() );
                        }
                        else if( IsTransformTypeTransform() && transformToMoveOnFloor != null )
                        {
                            SetTransformOnFloorToPressPosition( transformToMoveOnFloor );
                        }
                    }
                    else if( IsFloorRaycastFromPressCenter() )
                    {
                        if( IsTransformTypeBlockModel() && blockModelToMoveOnFloor != null )
                        {
                            SetTransformOnFloorToPressPosition( XRCameraManager.instance.GetXRCamera().GetCameras()[ 0 ], blockModelToMoveOnFloor.GetMoveTransform(), Input.mousePosition.x, Input.mousePosition.y );
                        }
                        else if( IsTransformTypeTransform() && transformToMoveOnFloor != null )
                        {
                            SetTransformOnFloorToPressPosition( XRCameraManager.instance.GetXRCamera().GetCameras()[ 0 ], transformToMoveOnFloor, Input.mousePosition.x, Input.mousePosition.y );
                        }
                    }
                }
                else if( CheckIfTouchIsBeingDragged() )
                {
                    if( IsFloorRaycastFromScreenCenter() )
                    {
                        if( IsTransformTypeBlockModel() && blockModelToMoveOnFloor != null )
                        {
                            SetTransformOnFloorToPressPosition( blockModelToMoveOnFloor.GetMoveTransform() );
                        }
                        else if( IsTransformTypeTransform() && transformToMoveOnFloor != null )
                        {
                            SetTransformOnFloorToPressPosition( transformToMoveOnFloor );
                        }
                    }
                    else if( IsFloorRaycastFromPressCenter() )
                    {
                        if( IsTransformTypeBlockModel() && blockModelToMoveOnFloor != null )
                        {
                            SetTransformOnFloorToPressPosition( XRCameraManager.instance.GetXRCamera().GetCameras()[ 0 ], blockModelToMoveOnFloor.GetMoveTransform(), Input.GetTouch( 0 ).position.x, Input.GetTouch( 0 ).position.y );
                        }
                        else if( IsTransformTypeTransform() && transformToMoveOnFloor != null )
                        {
                            SetTransformOnFloorToPressPosition( XRCameraManager.instance.GetXRCamera().GetCameras()[ 0 ], transformToMoveOnFloor, Input.GetTouch( 0 ).position.x, Input.GetTouch( 0 ).position.y );
                        }
                    }
                }
            }

            //Call the drag complete event if it has been setup
            if( LookingForDragGesture() && 
                IsDragEnding() &&
                onDragComplete != null )
            {
                onDragComplete.Invoke();
            }

        } //END CheckForFloorDrag

        //---------------------------//
        private bool ShouldBlockModelDragEventEndBeCalled()
        //---------------------------//
        {

            if( IsEaseOutDragEnabled() &&
                ( LookForBlockModel() && lookForBlockModel != null ) &&
                ( isMouseBeingDragged || isTouchBeingDragged ) &&
                IsDragEnding() )
            {
                return true;
            }

            return false;

        } //END ShouldBlockModelDragEventEndBeCalled

        //---------------------------//
        private bool IsDragEnding()
        //---------------------------//
        {

            if( Input.mousePresent && 
                Input.GetMouseButtonUp(0) )
            {
                return true;
            }
            else if( Input.touchSupported &&
                Input.touchCount == 1 &&
                Input.GetTouch(0).phase == TouchPhase.Ended )
            {
                return true;
            }

            return false;

        } //END IsDragEnding

        //---------------------------//
        private bool CheckIfMouseIsBeingDragged()
        //---------------------------//
        {

            if( Input.mousePresent )
            {
                if( !isFirstFrameOfMouseDown &&
                Input.GetMouseButtonDown( 0 ) &&
                CheckIfDragStartedOnCollider( XRCameraManager.instance.GetXRCamera().GetCameras()[ 0 ], Input.mousePosition ) )
                {
                    isFirstFrameOfMouseDown = true;
                    isMouseBeingDragged = true;
                }
                else if( isFirstFrameOfMouseDown &&
                         Input.GetMouseButtonDown( 0 ) )
                {
                    isFirstFrameOfMouseDown = false;
                    isMouseBeingDragged = true;
                }
                if( isMouseBeingDragged &&
                    Input.GetMouseButtonUp( 0 ) )
                {
                    isFirstFrameOfMouseDown = false;
                    isMouseBeingDragged = false;
                }

                return isMouseBeingDragged;
            }

            return false;

        } //END CheckIfMouseIsBeingDragged

        //------------------------------//
        private bool CheckIfTouchIsBeingDragged()
        //------------------------------//
        {

            
            if( Input.touchSupported &&
                Input.touchCount == 1 &&
                !didTouchCountReachTwoDuringGesture )
            {
                //Check if the collider is being touched when the finger first touches the screen
                if( Input.GetTouch(0).phase == TouchPhase.Began &&
                    CheckIfDragStartedOnCollider( XRCameraManager.instance.GetXRCamera().GetCameras()[ 0 ], Input.GetTouch( 0 ).position ) )
                {
                    isTouchBeingDragged = true;
                    return true;
                }

                //If the touch is currently dragging
                else if( isTouchBeingDragged &&
                         ( Input.GetTouch(0).phase == TouchPhase.Moved ||
                           Input.GetTouch(0).phase == TouchPhase.Stationary ) )
                {
                    return true;
                }

                //If the touch was dragging but the event has ended
                else if( isTouchBeingDragged &&
                         ( Input.GetTouch(0).phase == TouchPhase.Canceled ||
                           Input.GetTouch(0).phase == TouchPhase.Ended ) )
                {
                    isTouchBeingDragged = false;
                    return false;
                }
            }
            
            return false;

        } //END CheckIfTouchIsBeingDragged

        //------------------------------//
        private bool CheckIfDragStartedOnCollider( Camera camera, Vector2 pressPosition )
        //------------------------------//
        {
            //if( showDebug ) Debug.Log( "BlockEventXRTargetFloor.cs CheckIfDragStartedOnCollider() START position = " + pressPosition );

            //Store whether we've located the collider we care about or not
            bool foundCollider = false;

            //If we care about checking for if this drag started on a collider
            if( requireDragStartOnCollider )
            {
                //Are we looking for a 3D collider?
                if( LookForBlockModel() ||
                    LookForBlockButton() ||
                    LookForCollider() ||
                    LookForTag3D() )
                {
                    foundCollider = CheckFor3DCollider( camera, pressPosition );
                }

                //Otherwise, check for a 2D collider
                if( !foundCollider &&
                    ( LookForBlockButton() ||
                      LookForCollider2D() ||
                      LookForTag2D() ) )
                {
                    foundCollider = CheckFor2DCollider( camera, pressPosition );
                }
                
            }

            //If we don't care about the drag event starting on a collider, just return true
            else
            {
                foundCollider = true;
            }

            //if( showDebug ) Debug.Log( "BlockEventXRTargetFloor.cs CheckIfDragStartedOnCollider() END returning = " + foundCollider );

            return foundCollider;

        } //END CheckIfDragStartedOnCollider

        //------------------------------//
        private bool CheckFor3DCollider( Camera camera, Vector2 pressPosition )
        //------------------------------//
        {
            //if( showDebug ) Debug.Log( "BlockEventXRTargetFloor.cs CheckFor3DCollider() position = " + pressPosition );

            //Send a Ray out at the position to see if we collide with any 3D Colliders
            Ray ray = camera.ScreenPointToRay( pressPosition );
            RaycastHit hit;

            if( Physics.Raycast( ray, out hit ) )
            {
                if( hit.collider != null )
                {
                    //Check out if the collider we hit matches the BlockModel
                    if( LookForBlockModel() && lookForBlockModel != null )
                    {
                        if( hit.collider.transform.GetComponentInParent<BlockModel>() != null &&
                            hit.collider.transform.GetComponentInParent<BlockModel>() == lookForBlockModel )
                        {
                            return true;
                        }
                    }

                    //We hit a collider! Check if it matches the BlockButton
                    if( LookForBlockButton() && lookForBlockButton != null )
                    {
                        if( hit.collider.transform.GetComponentInParent<BlockButton>() != null &&
                            hit.collider.transform.GetComponentInParent<BlockButton>() == lookForBlockButton )
                        {
                            return true;
                        }
                    }

                    //Check if the collider matches
                    if( LookForCollider() && lookForCollider != null )
                    {
                        if( lookForCollider == hit.collider )
                        {
                            //if( showDebug ) Debug.Log( "BlockEventXRTargetFloor.cs CheckFor3DCollider() Found Collider! returning true" );
                            return true;
                        }
                    }

                    //Check if the collider tag matches
                    if( LookForTag3D() && lookForTag != "" )
                    {
                        if( hit.collider.tag == lookForTag )
                        {
                            return true;
                        }
                    }
                }
            }

            return false;

        } //END CheckFor3DCollider

        //------------------------------//
        private bool CheckFor2DCollider( Camera camera, Vector2 pressPosition )
        //------------------------------//
        {

            //Send a Ray out at the position to see if we collide with any 2D Colliders
            RaycastHit2D hit = Physics2D.Raycast( camera.transform.position, pressPosition );

            if( hit != null && hit.collider != null )
            {
                //We hit a collider! Check if it matches the BlockButton
                if( LookForBlockButton() && lookForBlockButton != null )
                {
                    if( hit.collider.transform.GetComponentInParent<BlockButton>() != null &&
                        hit.collider.transform.GetComponentInParent<BlockButton>() == lookForBlockButton )
                    {
                        return true;
                    }
                }

                //Check if the collider matches
                if( LookForCollider2D() && lookForCollider2D != null )
                {
                    if( lookForCollider2D == hit.collider )
                    {
                        return true;
                    }
                }

                //Check if the collider tag matches
                if( LookForTag2D() && lookForTag != "" )
                {
                    if( hit.collider.tag == lookForTag )
                    {
                        return true;
                    }
                }
            }

            return false;

        } //END CheckFor2DCollider

        //------------------------------//
        private bool DoRequiredComponentsExist()
        //------------------------------//
        {

            if( ( ( IsTransformTypeBlockModel() && blockModelToMoveOnFloor ) ||
                ( IsTransformTypeTransform() && transformToMoveOnFloor != null ) ) &&
                XRCameraManager.instance != null &&
                XRCameraManager.instance.GetXRCamera() != null &&
                XRCameraManager.instance.GetXRCamera().GetCameras() != null &&
                XRCameraManager.instance.GetXRCamera().GetCameras().Count > 0 )
            {
                return true;
            }
            else
            {
                return false;
            }

        } //END DoRequiredComponentsExist

        //------------------------------//
        //Using reference logic from Stack Overflow conversation
        //https://stackoverflow.com/questions/27822276/ar-vuforia-move-object-by-touch-only-in-x-z-unity3d
        private bool SetTransformOnFloorToPressPosition( Camera camera, Transform transformToMove, float positionX, float positionY )
        //------------------------------//
        {
            //Create a ray from the camera based on the press position
            Ray ray = camera.ScreenPointToRay( new Vector2( positionX, positionY ) );
            
            //Create a logical plane at the objects position that is perpendicular to the world Y
            Plane plane = new Plane( Vector3.up, xrTargetFloor.GetLastFloorCenterOfScreenPosition() ); //transformToMove.position

            //Find the distance between the plane and the camera
            float distance = 0;

            //If the ray hit...
            if( plane.Raycast( ray, out distance ) )
            {
                //Get the point
                Vector3 position = ray.GetPoint( distance );

                //Set the transform to the position on the plane that was pressed on
                transformToMove.position = position;

                return true;
            }

            return false;

        } //END SetTransformOnFloorToPressPosition

        //------------------------------//
        private bool SetTransformOnFloorToPressPosition( Transform transformToMove )
        //------------------------------//
        {

            if( transformToMove != null && xrTargetFloor != null )
            {
                bool returnValue = false;

                if( setOnPress )
                {
                    returnValue = true;
                    transformToMove.localPosition = xrTargetFloor.GetLastFloorCenterOfScreenPosition();
                }

                if( setToRotation )
                {
                    returnValue = true;
                    transformToMove.rotation = xrTargetFloor.GetLastFloorCenterOfScreenRotation();
                }

                return returnValue;
            }

            return false;

        } //END SetTransformOnFloorToPressPosition
        
        //-------------------------------------------//
        private void CallDisableSetTransformToFloorEvent()
        //-------------------------------------------//
        {

            if( blockEventXRTargetFloor != null )
            {
                blockEventXRTargetFloor.DisableSetTransformToFloorEvent();
            }

        } //END CallDisableSetTransformToFloorEvent

        //------------------------------------------//
        public void EnableSetTransformToFloorEvent()
        //------------------------------------------//
        {

            enableSetTransformToFloorEvent = true;

        } //END EnableSetTransformToFloorEvent

        //------------------------------------------//
        public void DisableSetTransformToFloorEvent()
        //------------------------------------------//
        {

            enableSetTransformToFloorEvent = false;

        } //END DisableSetTransformToFloorEvent

        //------------------------------------------//
        private void CallReEnableTransformToFloorEvent()
        //------------------------------------------//
        {

            if( blockEventXRTargetFloor != null )
            {
                blockEventXRTargetFloor.EnableSetTransformToFloorEvent();
            }

        } //END CallReEnableTransformToFloorEvent

    } //END BlockEventUnity

} //END Namespace