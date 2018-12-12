using Sirenix.OdinInspector;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

#if LEANTOUCH
using Lean.Touch;
#endif

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace BrandXR
{
    public class BlockEventTransform: BlockEventBase
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
            if( IsActionEnablePinchOrTwist() &&
                GameObject.FindObjectOfType<XRCameraManager>() == null )
            {
                return true;
            }

            return false;
        }


        //---------------- SETUP ERRORS -----------------------------//
        [ShowIf( "IsMissingXRInputManager" ), Button( "Create XR Input Manager", ButtonSizes.Large ), InfoBox( "WARNING: Missing bxr_XRInputManager in hierarchy which is required for the BlockEventTransform to work.\n\nPress the button below to create the bxr_XRInputManager now", InfoMessageType.Error )]
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
            if( IsActionEnablePinchOrTwist() &&
                GameObject.FindObjectOfType<XRInputManager>() == null )
            {
                return true;
            }

            return false;
        }


        [ShowIf( "ShowLeanTouchNotSetupWarning" ), InfoBox( "WARNING: bxr_XRInputManager needs to have its 'xrTouchSupportType' set to 'LeanTouch' for this to work", InfoMessageType.Error)]
        public int XRInputWarning1 = 0;

        private bool ShowLeanTouchNotSetupWarning()
        {
            if( GameObject.FindObjectOfType<XRInputManager>() != null )
            {
                if( GameObject.FindObjectOfType<XRInputManager>().xrTouchSupportType != XRInputManager.XRTouchSupportType.LeanTouch )
                {
                    return true;
                }
            }

            return false;
        }

        //----------------- ACTIONS --------------------------------//
        public enum Actions
        {
            None,
            SetValues,
            CopyValues,
            EnablePinchToScale,
            DisablePinchToScale,
            ReEnablePinchToScale,
            EnableTwistToRotate,
            DisableTwistToRotate,
            ReEnableTwistToRotate
        }

        [TitleGroup( "Block Event - Transform", "Used to modify a Transform component" )]
        public Actions action = Actions.SetValues;
        private bool TransformAction_SetValues() { return action == Actions.SetValues; }
        private bool TransformAction_CopyValues() { return action == Actions.CopyValues; }

        private bool _SetTransformValues() { return TransformType_Transform() && TransformAction_SetValues(); }
        private bool _CopyTransformValues() { return transformType == TransformType.Transform && action == Actions.CopyValues; }
        private bool _SetOrCopyTransformValues() { return TransformType_Transform() && ( TransformAction_SetValues() || TransformAction_CopyValues() ); }

        private bool _SetRectTransformValues() { return TransformType_RectTransform() && TransformAction_SetValues(); }
        private bool _CopyRectTransformValues() { return transformType == TransformType.RectTransform && action == Actions.CopyValues; }
        private bool _SetOrCopyRectTransformValues() { return TransformType_RectTransform() && ( TransformAction_SetValues() || TransformAction_CopyValues() ); }

        private bool SetTransformOrRectTransformValues() { return ( transformType == TransformType.Transform || transformType == TransformType.RectTransform ) && TransformAction_SetValues(); }
        private bool CopyTransformOrRectTransformValues() { return ( transformType == TransformType.Transform || transformType == TransformType.RectTransform ) && TransformAction_CopyValues(); }

        private bool ActionIsSetOrCopyValues() { return action == Actions.SetValues || action == Actions.CopyValues; }

        private bool IsActionEnablePinchToScale() { return action == Actions.EnablePinchToScale; }
        private bool IsActionDisablePinchToScale() { return action == Actions.DisablePinchToScale; }
        private bool IsActionReEnablePinchToScale() { return action == Actions.ReEnablePinchToScale; }
        private bool IsActionEnableTwistToRotate() { return action == Actions.EnableTwistToRotate; }
        private bool IsActionDisableTwistToRotate() { return action == Actions.DisableTwistToRotate; }
        private bool IsActionReEnableTwistToRotate() { return action == Actions.ReEnableTwistToRotate; }
        private bool IsActionEnablePinchOrTwist() { return action == Actions.EnablePinchToScale || action == Actions.EnableTwistToRotate; }
        private bool IsActionReEnablePinchOrTwist() { return action == Actions.ReEnablePinchToScale || action == Actions.ReEnableTwistToRotate; }
        private bool IsActionEnableOrDisablePinchOrTwist() { return action == Actions.EnablePinchToScale || action == Actions.DisablePinchToScale || action == Actions.EnableTwistToRotate || action == Actions.DisableTwistToRotate; }

        //------------- SET VALUES && COPY VALUES VARIABLES ---------------------------------//
        public enum TransformType
        {
            Transform,
            RectTransform
        }

        [Space( 15f ), ShowIf( "ActionIsSetOrCopyValues" )]
        public TransformType transformType = TransformType.Transform;
        private bool TransformType_Transform() { return ActionIsSetOrCopyValues() && transformType == TransformType.Transform; }
        private bool TransformType_RectTransform() { return ActionIsSetOrCopyValues() && transformType == TransformType.RectTransform; }

        [ShowIf( "TransformType_Transform" )]
        public Transform ChangeTransform;

        [ShowIf( "TransformType_RectTransform" )]
        public RectTransform ChangeRectTransform;

        

        [ShowIf( "_CopyTransformValues" ), FoldoutGroup( "Transform Settings" )]
        public Transform copyToTransform;

        [ShowIf( "_CopyRectTransformValues" ), FoldoutGroup( "Transform Settings" )]
        public RectTransform copyToRectTransform;


        [ShowIf( "SetTransformOrRectTransformValues" ), FoldoutGroup( "Transform Settings" )]
        public Vector3 positionValues;
        [ShowIf( "_SetRectTransformValues" ), FoldoutGroup( "Transform Settings" )]
        public Vector2 widthAndHeightValues;
        [ShowIf( "SetTransformOrRectTransformValues" ), FoldoutGroup( "Transform Settings" )]
        public Vector3 rotationValues;
        [ShowIf( "SetTransformOrRectTransformValues" ), FoldoutGroup( "Transform Settings" )]
        public Vector3 scaleValues;

        [ShowIf( "CopyTransformOrRectTransformValues" ), FoldoutGroup( "Transform Settings" )]
        public bool copyPosition;
        [ShowIf( "_CopyRectTransformValues" ), FoldoutGroup( "Transform Settings" )]
        public bool copyWidthAndHeight;
        [ShowIf( "CopyTransformOrRectTransformValues" ), FoldoutGroup( "Transform Settings" )]
        public bool copyRotation;
        [ShowIf( "CopyTransformOrRectTransformValues" ), FoldoutGroup( "Transform Settings" )]
        public bool copyScale;


        //-------------------- RE-ENABLE SCALE OR TWIST VARIABLES ---------------------//
        [Space( 15f ), ShowIf( "IsActionReEnablePinchOrTwist" ), InfoBox("Set this to the BlockEvent - Transform that you want to Re-Enable the Scale or Twist event on")]
        public BlockEventTransform blockEventTransform = null;

        //-------------------- PINCH TO SCALE AND TWIST TO ROTATE VARIABLES -----------//
        public enum GestureTransformType
        {
            Transform,
            BlockModel
        }
        [Space( 15f ), ShowIf( "IsActionEnableOrDisablePinchOrTwist" )]
        public GestureTransformType gestureTransformType = GestureTransformType.BlockModel;
        private bool IsGestureTransformTypeBlockModel() { return IsActionEnableOrDisablePinchOrTwist() && gestureTransformType == GestureTransformType.BlockModel; }
        private bool IsGestureTransformTypeTransform() { return IsActionEnableOrDisablePinchOrTwist() && gestureTransformType == GestureTransformType.Transform; }

        [ShowIf( "IsGestureTransformTypeBlockModel" )]
        public BlockModel blockModelToChange = null;

        [ShowIf( "IsGestureTransformTypeTransform" )]
        public Transform transformToChange = null;

        [Space(15f), ShowIf( "IsActionEnablePinchToScale" ), InfoBox("Do you want to restrict the minimum & maximum values you can reach by pinching?")]
        public bool restrictScale = true;
        private bool RestrictScale() { return IsActionEnablePinchToScale() && restrictScale; }

        [Space( 15f ), Tooltip("Set the minimum value you will allow the pinch to reach"), ShowIf( "RestrictScale" )]
        public Vector3 scaleMinimum = new Vector3( .1f, .1f, .1f );

        [Tooltip( "Set the maximum value you will allow the pinch to reach" ), ShowIf( "RestrictScale" )]
        public Vector3 scaleMaximum = new Vector3( 10f, 10f, 10f );

        [Space( 15f ), ShowIf( "IsActionEnablePinchToScale" ), Tooltip( "Would you like the scaling system to be testable using your mouse wheel in editor?" )]
        public bool simulateWithMouseWheel = true;
        private bool ShowMouseWheelSensitivity() { return IsActionEnablePinchToScale() && simulateWithMouseWheel; }

        [Range( -1.0f, 1.0f ), ShowIf( "ShowMouseWheelSensitivity" ), Tooltip("The sensitivity of the pinch to the mouse wheel")]
        public float wheelSensitivity = .1f;


        [Space( 15f ), ShowIf( "IsActionEnableTwistToRotate" ), Tooltip("The multiplication factor for the rotation caused by the Twist")]
        public float rotationMultiplier = 1f;

        [Space( 15f ), ShowIf( "IsActionEnableTwistToRotate" ), Tooltip( "The Dampening factor for the rotation caused by the Twist" )]
        public float rotationDampening = 10f;

        [Space( 15f ), ShowIf( "IsActionEnableTwistToRotate" ), InfoBox("Set these booleans to control which axis the rotation will influence")]
        public bool rotateX = true;

        [ShowIf( "IsActionEnableTwistToRotate" )]
        public bool rotateY = true;

        [ShowIf( "IsActionEnableTwistToRotate" )]
        public bool rotateZ = true;
        


        [Space( 15f ), ShowIf( "IsActionEnablePinchOrTwist" ), InfoBox("Enable to require that a BlockButton, Collider, or Tag on a Collider be touched by both fingers simultaneously to begin the pinch/twist event")]
        public bool requireStartOnCollider = false;
        private bool RequireCollider() { return IsActionEnablePinchOrTwist() && requireStartOnCollider; }

        public enum CollisionReference
        {
            BlockModel,
            BlockButton,
            Collider,
            ColliderWithTag,
            Collider2D,
            Collider2DWithTag
        }
        [ShowIf( "RequireCollider" )]
        public CollisionReference lookForColliderType = CollisionReference.Collider;
        private bool LookForBlockModel() { return RequireCollider() && lookForColliderType == CollisionReference.BlockModel; }
        private bool LookForBlockButton() { return RequireCollider() && lookForColliderType == CollisionReference.BlockButton; }
        private bool LookForCollider() { return RequireCollider() && lookForColliderType == CollisionReference.Collider; }
        private bool LookForTag3D() { return RequireCollider() && lookForColliderType == CollisionReference.ColliderWithTag; }
        private bool LookForCollider2D() { return RequireCollider() && lookForColliderType == CollisionReference.Collider2D; }
        private bool LookForTag2D() { return RequireCollider() && lookForColliderType == CollisionReference.Collider2DWithTag; }
        private bool LookForTag() { return RequireCollider() && ( lookForColliderType == CollisionReference.ColliderWithTag || lookForColliderType == CollisionReference.Collider2DWithTag ); }

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
        
#if LEANTOUCH
        private LeanMultiTwist leanMultiTwist = null;

        private LeanManualRotateSmooth leanManualRotateSmooth = null;

        private LeanScale leanScale = null;

        private LeanSelectable leanSelectable = null;
#endif


        //-------------------- EVENT MESSAGES ---------------------//
        private bool ShowOnSetOrCopyValuesCompletedEvent() { return action == Actions.SetValues || action == Actions.CopyValues; }

        [ShowIf( "ShowOnSetOrCopyValuesCompletedEvent" ), FoldoutGroup( "Event Messages" )]
        public UnityEvent onActionCompleted;


        private bool ShowOnPinchEvent() { return action == Actions.EnablePinchToScale; }

        [ShowIf( "ShowOnPinchEvent" ), FoldoutGroup( "Event Messages" )]
        public UnityEvent onPinchEventOccured;


        private bool ShowOnTwistEvent() { return action == Actions.EnableTwistToRotate; }

        [ShowIf( "ShowOnTwistEvent" ), FoldoutGroup( "Event Messages" )]
        public UnityEvent onTwistEventOccured;



        //---------------------------------------------------------//
        public override EventType GetEventType()
        //---------------------------------------------------------//
        {
            return EventType.Transform;

        } //END GetEventType

        //---------------------------------------------------------//
        public void SetAction( Actions action )
        //---------------------------------------------------------//
        {

            this.action = action;

        } //END SetAction

        //-------------------------------//
        public void Awake()
        //-------------------------------//
        {

            showDebug = true;

        } //END Awake

        //-------------------------------//
        public override void PrepareEvent()
        //-------------------------------//
        {
            
            if( action == Actions.CopyValues )
            {
                if( TransformAction_CopyValues() && copyToTransform != null )
                {
                    eventReady = true;
                }
                else if( TransformAction_CopyValues() && copyToRectTransform != null )
                {
                    eventReady = true;
                }
            }
            else if( action == Actions.SetValues )
            {
                if( TransformType_Transform() && ChangeTransform != null )
                {
                    eventReady = true;
                }
                else if( TransformType_RectTransform() && ChangeRectTransform != null )
                {
                    eventReady = true;
                }
            }
            else if( action == Actions.EnablePinchToScale )
            {
#if LEANTOUCH
                leanSelectable = null;
                leanManualRotateSmooth = null;
                leanMultiTwist = null;
                leanScale = null;
                
                if( IsGestureTransformTypeBlockModel() && blockModelToChange != null )
                {
                    eventReady = true;
                }
                else if( IsGestureTransformTypeTransform() && transformToChange != null )
                {
                    eventReady = true;
                }
                else
                {
                    eventReady = false;
                }
#else
                eventReady = false;
#endif
            }
            else if( action == Actions.EnableTwistToRotate )
            {
#if LEANTOUCH
                leanSelectable = null;
                leanManualRotateSmooth = null;
                leanMultiTwist = null;
                leanScale = null;

                if( IsGestureTransformTypeBlockModel() && blockModelToChange != null )
                {
                    eventReady = true;
                }
                else if( IsGestureTransformTypeTransform() && transformToChange != null )
                {
                    eventReady = true;
                }
                else
                {
                    eventReady = false;
                }
#else
                eventReady = false;
#endif
            }
            else if( action == Actions.DisablePinchToScale )
            {
#if LEANTOUCH
                if( IsGestureTransformTypeBlockModel() && blockModelToChange != null )
                {
                    eventReady = true;
                }
                else if( IsGestureTransformTypeTransform() && transformToChange != null )
                {
                    eventReady = true;
                }
                else
                {
                    eventReady = false;
                }
#else
                eventReady = false;
#endif
            }
            else if( action == Actions.DisableTwistToRotate )
            {
#if LEANTOUCH
                if( IsGestureTransformTypeBlockModel() && blockModelToChange != null )
                {
                    eventReady = true;
                }
                else if( IsGestureTransformTypeTransform() && transformToChange != null )
                {
                    eventReady = true;
                }
                else
                {
                    eventReady = false;
                }
#else
                eventReady = false;
#endif
            }
            else if( action == Actions.ReEnablePinchToScale )
            {
#if LEANTOUCH
                if( IsGestureTransformTypeBlockModel() && blockModelToChange != null )
                {
                    eventReady = true;
                }
                else if( IsGestureTransformTypeTransform() && transformToChange != null )
                {
                    eventReady = true;
                }
                else
                {
                    eventReady = false;
                }
#else
                eventReady = false;
#endif
            }
            else if( action == Actions.ReEnableTwistToRotate )
            {
#if LEANTOUCH
                if( IsGestureTransformTypeBlockModel() && blockModelToChange != null )
                {
                    eventReady = true;
                }
                else if( IsGestureTransformTypeTransform() && transformToChange != null )
                {
                    eventReady = true;
                }
                else
                {
                    eventReady = false;
                }
#else
                eventReady = false;
#endif
            }

        } //END PrepareEvent
        

        //-------------------------------//
        protected override void _CallEvent()
        //-------------------------------//
        {
            base._CallEvent();

            if( eventReady )
            {
                if( action == Actions.SetValues )
                {
                    CallSetValuesEvent();
                }
                else if( action == Actions.CopyValues )
                {
                    CallCopyValuesEvent();
                }
                else if( action == Actions.EnablePinchToScale )
                {
                    CallEnablePinchToScaleEvent();
                }
                else if( action == Actions.DisablePinchToScale )
                {
                    CallDisablePinchToScaleEvent();
                }
                else if( action == Actions.EnableTwistToRotate )
                {
                    CallEnableTwistToRotateEvent();
                }
                else if( action == Actions.DisableTwistToRotate )
                {
                    CallDisableTwistToRotateEvent();
                }
                else if( action == Actions.ReEnablePinchToScale )
                {
                    CallReEnablePinchToScaleEvent();
                }
                else if( action == Actions.ReEnableTwistToRotate )
                {
                    CallReEnableTwistToRotateEvent();
                }
            }

        } //END CallEvent

        //---------------------------------//
        private void CallSetValuesEvent()
        //---------------------------------//
        {

            if( TransformType_Transform() && ChangeTransform != null )
            {
                SetTransformValues( ChangeTransform, positionValues, rotationValues, scaleValues );
            }
            else if( TransformType_RectTransform() && ChangeRectTransform != null )
            {
                SetTransformValues( ChangeRectTransform, positionValues, widthAndHeightValues, rotationValues, scaleValues );
            }

            if( onActionCompleted != null ) { onActionCompleted.Invoke(); }

        } //END CallSetValuesEvent

        //---------------------------------//
        private void CallCopyValuesEvent()
        //---------------------------------//
        {

            if( TransformType_Transform() && ChangeTransform != null && copyToTransform != null )
            {
                CopyTransformValues( ChangeTransform, copyToTransform, copyPosition, copyRotation, copyScale );
            }
            else if( TransformType_RectTransform() && ChangeRectTransform != null && copyToRectTransform != null )
            {
                CopyTransformValues( ChangeRectTransform, copyToRectTransform, copyWidthAndHeight, copyPosition, copyRotation, copyScale );
            }

            if( onActionCompleted != null ) { onActionCompleted.Invoke(); }

        } //END CallCopyValuesEvent

        //---------------------------------//
        private void SetTransformValues( Transform _transform, Vector3 position, Vector3 rotation, Vector3 scale )
        //---------------------------------//
        {

            _transform.localPosition = position;
            _transform.localEulerAngles = rotation;
            _transform.localScale = scale;

        } //END SetTransformValues

        //---------------------------------//
        private void SetTransformValues( RectTransform _transform, Vector3 position, Vector2 widthAndHeight, Vector3 rotation, Vector3 scale )
        //---------------------------------//
        {

            _transform.localPosition = position;
            _transform.sizeDelta = widthAndHeight;
            _transform.localEulerAngles = rotation;
            _transform.localScale = scale;

        } //END SetTransformValues

        //---------------------------------//
        private void CopyTransformValues( Transform _transform, Transform copyToTransform, bool shouldCopyPosition, bool shouldCopyRotation, bool shouldCopyScale )
        //---------------------------------//
        {

            if( shouldCopyPosition )
            {
                copyToTransform.transform.position = _transform.position;
            }
            else if( shouldCopyRotation )
            {
                copyToTransform.transform.rotation = _transform.rotation;
            }
            else if( shouldCopyScale )
            {
                copyToTransform.transform.localScale = _transform.localScale;
            }

        } //END CopyTransformValues

        //---------------------------------//
        private void CopyTransformValues( RectTransform _transform, RectTransform copyToTransform, bool shouldCopyPosition, bool shouldCopyWidthAndHeight, bool shouldCopyRotation, bool shouldCopyScale )
        //---------------------------------//
        {

            if( shouldCopyPosition )
            {
                copyToTransform.position = _transform.position;
            }
            else if( shouldCopyWidthAndHeight )
            {
                copyToTransform.sizeDelta = _transform.sizeDelta;
            }
            else if( shouldCopyRotation )
            {
                copyToTransform.rotation = _transform.rotation;
            }
            else if( shouldCopyScale )
            {
                copyToTransform.localScale = _transform.localScale;
            }

        } //END CopyTransformValues


        //------------------------------------//
        private void CallEnablePinchToScaleEvent()
        //------------------------------------//
        {

#if LEANTOUCH
            Transform transformToPinch = null;

            if( IsGestureTransformTypeBlockModel() && blockModelToChange != null )
            {
                transformToPinch = blockModelToChange.GetPinchTransform();
            }
            else if( IsGestureTransformTypeTransform() && transformToChange != null )
            {
                transformToPinch = transformToChange;
            }

            if( transformToPinch != null )
            {

                AddOrEnableLeanSelectableOnGestureTarget( transformToPinch );

                //Add the LeanScale component to the object we want to scale
                if( leanScale != null && !leanScale.enabled )
                {
                    leanScale.enabled = true;
                }

                if( leanScale == null && transformToPinch.GetComponent<LeanScale>() == null )
                {
                    leanScale = transformToPinch.gameObject.AddComponent<LeanScale>();
                }

                if( leanScale == null && transformToPinch.GetComponent<LeanScale>() != null )
                {
                    leanScale = transformToPinch.gameObject.GetComponent<LeanScale>();
                }

                //If all our necessary components exist, then set them up for immediate use
                if( leanSelectable != null && leanScale != null )
                {
                    if( requireStartOnCollider )
                    {
                        leanScale.RequiredSelectable = leanSelectable;
                    }
                    else
                    {
                        leanScale.RequiredSelectable = null;
                    }

                    if( simulateWithMouseWheel )
                    {
                        leanScale.WheelSensitivity = wheelSensitivity;
                    }
                    else
                    {
                        leanScale.WheelSensitivity = 0f;
                    }

                    if( restrictScale )
                    {
                        leanScale.ScaleClamp = true;
                        leanScale.ScaleMin = scaleMinimum;
                        leanScale.ScaleMax = scaleMaximum;
                    }
                    else
                    {
                        leanScale.ScaleClamp = false;
                    }
                    
                    if( onActionCompleted != null ) { onActionCompleted.Invoke(); }
                }
            }
#endif

        } //END CallEnablePinchToScaleEvent

        //------------------------------------//
        public void Update()
        //------------------------------------//
        {

            CheckIfLeanSelectableShouldBeEnabled();

            CheckIfLeanSelectableShouldBeDisabled();

        } //END Update

        //------------------------------------//
        private void CheckIfLeanSelectableShouldBeEnabled()
        //------------------------------------//
        {

#if LEANTOUCH
            if( RequireCollider() && leanSelectable != null )
            {
                //If our Two-Touch Input is starting over a LeanSelectable, then enable the LeanSelectable until the gesture ends
                if( LeanTouch.Fingers != null && LeanTouch.Fingers.Count == 2 )
                {
                    bool fingersTouchingSelectable = true;

                    foreach( LeanFinger finger in LeanTouch.Fingers )
                    {
                        bool foundColliderOnFinger = false;

                        //If the finger is in any phase besides starting the input, prevent activation of the LeanSelectable
                        if( !IsInputInStartPhase() )
                        {
                            fingersTouchingSelectable = false;
                            break;
                        }

                        //Are we looking for a 3D collider?
                        if( LookForBlockModel() ||
                            LookForBlockButton() ||
                            LookForCollider() ||
                            LookForTag3D() )
                        {
                            //If we cannot locate the 3D collider we're looking for, 
                            //then prevent activation of the LeanSelectable (which will prevent the gesture)
                            if( !CheckFor3DCollider( Camera.main, finger.ScreenPosition ) )
                            {
                                fingersTouchingSelectable = false;
                                break;
                            }
                            else
                            {
                                foundColliderOnFinger = true;
                            }
                        }

                        //Otherwise, check for a 2D collider
                        if( !foundColliderOnFinger &&
                            ( LookForBlockButton() ||
                              LookForCollider2D() ||
                              LookForTag2D() ) )
                        {
                            if( !CheckFor2DCollider( Camera.main, finger.ScreenPosition ) )
                            {
                                fingersTouchingSelectable = false;
                                break;
                            }
                        }
                        else
                        {
                            foundColliderOnFinger = true;
                        }
                    }

                    //After finishing our check, if all of the fingers are touching the LeanSelectable, then enable it
                    if( fingersTouchingSelectable )
                    {
                        leanSelectable.Select();
                    }

                }

            }
#endif

        } //END CheckIfLeanSelectableShouldBeEnabled

        //------------------------------------//
        private bool IsInputInStartPhase()
        //------------------------------------//
        {

            if( Input.mousePresent && Input.GetMouseButtonDown(0) )
            {
                return true;
            }

            if( Input.touchSupported &&
                Input.touchCount == 2 &&
                Input.GetTouch(1).phase == TouchPhase.Began )
            {
                return true;
            }

            return false;

        } //END IsInputInStartPhase

        //------------------------------------//
        private void CheckIfLeanSelectableShouldBeDisabled()
        //------------------------------------//
        {

#if LEANTOUCH
            if( RequireCollider() && leanSelectable != null && leanSelectable.IsSelected )
            {
                if( LeanTouch.Fingers != null && LeanTouch.Fingers.Count < 1 )
                {
                    leanSelectable.Deselect();
                }
                else if( LeanTouch.Fingers == null )
                {
                    leanSelectable.Deselect();
                }
            }
#endif

        } //END CheckIfLeanSelectableShouldBeDisabled

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
                    //We hit a collider! Check if it matches the BlockModel
                    if( LookForBlockModel() && lookForBlockModel != null )
                    {
                        if( hit.collider.transform.GetComponentInParent<BlockModel>() != null &&
                            hit.collider.transform.GetComponentInParent<BlockModel>() == lookForBlockModel )
                        {
                            return true;
                        }
                    }

                    //Check if it matches the BlockButton
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

        

        //------------------------------------//
        private void AddOrEnableLeanSelectableOnGestureTarget( Transform transformToAddLeanSelectableTo )
        //------------------------------------//
        {
#if LEANTOUCH
            //Debug.Log( "BlockEventTransform.cs AddOrEnableLeanSelectableOnGestureTarget() name = " + transformToAddLeanSelectableTo.name + ", leanSelectable = " + leanSelectable );

            //If we already have a leanSelectable object, then just enable it
            if( leanSelectable != null && !leanSelectable.enabled )
            {
                leanSelectable.enabled = true;
            }
            
            //Otherwise if we cannot locate a LeanSelectable on the object we want to transform, then add one now
            if( leanSelectable == null && transformToAddLeanSelectableTo.GetComponent<LeanSelectable>() == null )
            {
                leanSelectable = transformToAddLeanSelectableTo.gameObject.AddComponent<LeanSelectable>();
            }

            //If there is already a LeanSelectable on the transform we are performing the gesture on, then use it
            if( leanSelectable == null && transformToAddLeanSelectableTo.GetComponent<LeanSelectable>() )
            {
                leanSelectable = transformToAddLeanSelectableTo.GetComponent<LeanSelectable>();
            }
#endif
        } //END AddOrEnableLeanSelectableOnGestureTarget

        //------------------------------------//
        private void CallDisablePinchToScaleEvent()
        //------------------------------------//
        {

#if LEANTOUCH
            Transform transformToPinch = null;

            if( IsGestureTransformTypeBlockModel() && blockModelToChange != null )
            {
                transformToPinch = blockModelToChange.GetPinchTransform();
            }
            else if( IsGestureTransformTypeTransform() && transformToChange != null )
            {
                transformToPinch = transformToChange;
            }

            if( transformToPinch != null )
            {
                //Disable our LeanTouch components
                if( leanSelectable != null )
                {
                    leanSelectable.enabled = false;
                }
                else if( transformToPinch.GetComponent<LeanSelectable>() != null )
                {
                    transformToPinch.GetComponent<LeanSelectable>().enabled = false;
                }

                if( leanScale != null )
                {
                    leanScale.enabled = false;
                }
                else if( transformToPinch.GetComponent<LeanScale>() != null )
                {
                    transformToPinch.GetComponent<LeanScale>().enabled = false;
                }
                
                if( onActionCompleted != null ) { onActionCompleted.Invoke(); }
            }
#endif
                
        } //END CallDisablePinchToScaleEvent

        //------------------------------------//
        private void CallEnableTwistToRotateEvent()
        //------------------------------------//
        {

#if LEANTOUCH
            Transform transformToTwist = null;

            if( IsGestureTransformTypeBlockModel() && blockModelToChange != null )
            {
                transformToTwist = blockModelToChange.GetTwistTransform();
            }
            else if( IsGestureTransformTypeTransform() && transformToChange != null )
            {
                transformToTwist = transformToChange;
            }

            //Debug.Log( "CallEnableTwistToRotateEvent() transformToTwist = " + transformToTwist.name );

            if( transformToTwist != null )
            {

                AddOrEnableLeanSelectableOnGestureTarget( transformToTwist );

                //Add the LeanMultiTwist component to the object we want to rotate
                if( leanMultiTwist != null && !leanMultiTwist.enabled )
                {
                    leanMultiTwist.enabled = true;
                }

                if( leanMultiTwist == null && transformToTwist.GetComponent<LeanMultiTwist>() == null )
                {
                    leanMultiTwist = transformToTwist.gameObject.AddComponent<LeanMultiTwist>();
                }

                if( leanMultiTwist == null && transformToTwist.GetComponent<LeanMultiTwist>() != null )
                {
                    leanMultiTwist = transformToTwist.gameObject.GetComponent<LeanMultiTwist>();
                }

                //Also add the leanManualRotateSmooth component to the object we want to rotate
                if( leanManualRotateSmooth != null && !leanManualRotateSmooth.enabled )
                {
                    leanManualRotateSmooth.enabled = true;
                }

                if( leanManualRotateSmooth == null && transformToTwist.GetComponent<LeanManualRotateSmooth>() == null )
                {
                    leanManualRotateSmooth = transformToTwist.gameObject.AddComponent<LeanManualRotateSmooth>();
                }

                if( leanManualRotateSmooth == null && transformToTwist.GetComponent<LeanManualRotateSmooth>() )
                {
                    leanManualRotateSmooth = transformToTwist.GetComponent<LeanManualRotateSmooth>();
                }

                if( leanSelectable != null && leanSelectable.enabled && 
                    leanMultiTwist != null && leanMultiTwist.enabled &&
                    leanManualRotateSmooth != null && leanManualRotateSmooth.enabled )
                {

                    if( requireStartOnCollider )
                    {
                        leanMultiTwist.RequiredSelectable = leanSelectable;
                    }
                    else
                    {
                        leanMultiTwist.RequiredSelectable = null;
                    }

                    Vector3 rotationAxis = Vector3.zero;

                    if( rotateX ) { rotationAxis = new Vector3( -1f, rotationAxis.y, rotationAxis.z ); }
                    if( rotateY ) { rotationAxis = new Vector3( rotationAxis.x, -1f, rotationAxis.z ); }
                    if( rotateZ ) { rotationAxis = new Vector3( rotationAxis.x, rotationAxis.y, -1f ); }

                    leanManualRotateSmooth.Axis = rotationAxis;

                    leanManualRotateSmooth.Multiplier = rotationMultiplier;

                    leanManualRotateSmooth.Dampening = rotationDampening;

                    //If our twist event listener is null, create one
                    if( leanMultiTwist.OnTwist == null )
                    {
                        leanMultiTwist.OnTwist = new LeanMultiTwist.FloatEvent();
                    }

                    //Add the Rotate command to the twist listener
                    if( leanMultiTwist.OnTwist != null &&
                        ( leanMultiTwist.OnTwist.GetPersistentEventCount() == 0 ||
                          leanMultiTwist.OnTwist.GetPersistentTarget( 0 ) != leanManualRotateSmooth ) )
                    {
                        //Add a Non-Persistent listener to this event. This will work but it won't be visible in the editor and won't be linked to the event after play mode ends
                        leanMultiTwist.OnTwist.AddListener( leanManualRotateSmooth.Rotate );
                    }
                    
                    if( onActionCompleted != null ) { onActionCompleted.Invoke(); }
                }
            }
#endif

        } //END CallEnableTwistToRotateEvent

        //------------------------------------//
        private void CallDisableTwistToRotateEvent()
        //------------------------------------//
        {

#if LEANTOUCH
            Transform transformToTwist = null;

            if( IsGestureTransformTypeBlockModel() && blockModelToChange != null )
            {
                transformToTwist = blockModelToChange.GetTwistTransform();
            }
            else if( IsGestureTransformTypeTransform() && transformToChange != null )
            {
                transformToTwist = transformToChange;
            }

            if( transformToTwist != null )
            {
                //Disable our LeanTouch components
                if( leanSelectable != null )
                {
                    leanSelectable.enabled = false;
                }
                else if( transformToTwist.GetComponent<LeanSelectable>() != null )
                {
                    transformToTwist.GetComponent<LeanSelectable>().enabled = false;
                }

                if( leanMultiTwist != null )
                {
                    leanMultiTwist.enabled = false;
                }
                else if( transformToTwist.GetComponent<LeanMultiTwist>() != null )
                {
                    transformToTwist.GetComponent<LeanMultiTwist>().enabled = false;
                }

                if( leanManualRotateSmooth != null )
                {
                    leanManualRotateSmooth.enabled = false;
                }
                else if( transformToTwist.GetComponent<LeanManualRotateSmooth>() != null )
                {
                    transformToTwist.GetComponent<LeanManualRotateSmooth>().enabled = false;
                }
                
                if( onActionCompleted != null ) { onActionCompleted.Invoke(); }
            }
#endif

        } //END CallDisableTwistToRotateEvent

        //--------------------------------------------//
        private void CallReEnablePinchToScaleEvent()
        //--------------------------------------------//
        {

#if LEANTOUCH
            if( blockEventTransform != null )
            {
                blockEventTransform.CallEnablePinchToScaleEvent();
            }
#endif

        } //END CallReEnablePinchToScaleEvent

        //--------------------------------------------//
        private void CallReEnableTwistToRotateEvent()
        //--------------------------------------------//
        {

#if LEANTOUCH
            if( blockEventTransform != null )
            {
                blockEventTransform.CallEnableTwistToRotateEvent();
            }
#endif

        } //END CallReEnableTwistToRotateEvent

    } //END BlockEventTransform

} //END Namespace