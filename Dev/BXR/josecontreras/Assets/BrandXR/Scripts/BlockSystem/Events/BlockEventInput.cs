using Sirenix.OdinInspector;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

namespace BrandXR
{
    public class BlockEventInput: BlockEventBase
    {
        //----------------- ACTIONS --------------------------------//
        public enum Actions
        {
            None,
            ListenForInput
        }

        [TitleGroup( "Block Event - Input", "Used for listening for specific input connected to this device" )]
        public Actions action = Actions.ListenForInput;
        private bool ShowListenForInputAction() { return action == Actions.ListenForInput; }

        //------------- VARIABLES ---------------------------------//
        public enum InputAction
        {
            LeftPress,
            RightPress,
            LeftHold,
            RightHold,
            DoubleLeftPress,
            DoubleRightPress
        }

        [ShowIf( "ShowListenForInputAction" ), FoldoutGroup( "Listen For Input Settings" )]
        public InputAction listenFor = InputAction.LeftPress;

        private bool InputAction_AnyPress() { return ( listenFor == InputAction.LeftPress || listenFor == InputAction.RightPress ); }
        private bool InputAction_LeftPress() { return listenFor == InputAction.LeftPress; }
        private bool InputAction_RightPress() { return listenFor == InputAction.RightPress; }

        private bool InputAction_AnyHold() { return ( listenFor == InputAction.LeftHold || listenFor == InputAction.RightHold ); }
        private bool InputAction_LeftHold() { return listenFor == InputAction.LeftHold; }
        private bool InputAction_RightHold() { return listenFor == InputAction.RightHold; }

        private bool InputAction_AnyDoublePress() { return ( listenFor == InputAction.DoubleLeftPress || listenFor == InputAction.DoubleRightPress ); }
        private bool InputAction_DoubleLeftPress() { return listenFor == InputAction.DoubleLeftPress; }
        private bool InputAction_DoubleRightPress() { return listenFor == InputAction.DoubleRightPress; }

        public enum InputTrigger
        {
            EntireScreen,
            GameObject,
            Tag
        }

        [ShowIf( "ShowListenForInputAction" ), FoldoutGroup( "Listen For Input Settings" )]
        public InputTrigger inputTrigger = InputTrigger.EntireScreen;
        private bool IsInputTriggerEntireScreen() { return ShowListenForInputAction() && inputTrigger == InputTrigger.EntireScreen; }
        private bool IsInputTriggerGameObject() { return ShowListenForInputAction() && inputTrigger == InputTrigger.GameObject; }
        private bool IsInputTriggerTag() { return ShowListenForInputAction() && inputTrigger == InputTrigger.Tag; }

        [ShowIf( "IsInputTriggerGameObject" ), FoldoutGroup( "Listen For Input Settings" )]
        public GameObject inputGameObject;

        [ShowIf( "IsInputTriggerTag" ), FoldoutGroup( "Listen For Input Settings" )]
        public string inputTriggerTag = "";


        [ShowIf( "InputAction_AnyHold" ), FoldoutGroup( "Listen For Input Settings" )]
        public float RequiredHoldTime = 1f;

        private float hold_AccumulatedTime = 0f;
        private bool waitForButtonUp = false;

        [ShowIf( "InputAction_AnyDoublePress" ), FoldoutGroup( "Listen For Input Settings" )]
        public float AllowedTimeBetweenPresses = .5f;

        private float doubleClick_LastTime = -10f;

        
        //-------------------- "LISTEN FOR INPUT" EVENT MESSAGES ---------------------//
        private bool ShowListenForInputEventMessages() { return action == Actions.ListenForInput; }

        [Space( 15f ), ShowIf( "ShowListenForInputEventMessages" ), FoldoutGroup( "Event Messages" )]
        public UnityEvent onActionCompleted = new UnityEvent();


        //---------------------------------------------------------//
        public override EventType GetEventType()
        //---------------------------------------------------------//
        {
            return EventType.Input;

        } //END GetEventType

        //---------------------------------------------------------//
        public void SetAction( Actions action )
        //---------------------------------------------------------//
        {

            this.action = action;

        } //END SetAction


        //----------------------------------//
        public void Update()
        //----------------------------------//
        {

            if( eventReady )
            {
                if( action == Actions.ListenForInput )
                {
                    CheckForInputPress();

                    CheckForDoublePress();

                    CheckForPressAndHold();
                }
            }

        } //END Update

        //----------------------------------//
        private void CheckForInputPress()
        //----------------------------------//
        {

            //Keep a look out for Input events
            if( InputAction_AnyPress() )
            {
                if( Input.GetMouseButtonUp( 0 ) || Input.GetMouseButtonUp( 1 ) )
                {
                    CheckIfPressShouldCallEvent();
                }
            }

        } //END CheckForInput

        //-----------------------------------//
        private void CheckForDoublePress()
        //-----------------------------------//
        {

            if( InputAction_AnyDoublePress() )
            {
                if( Input.GetMouseButtonUp( 0 ) || Input.GetMouseButtonUp( 1 ) )
                {
                    float timeDelta = Time.time - doubleClick_LastTime;

                    if( timeDelta < AllowedTimeBetweenPresses )
                    {
                        //Reset the double click counter
                        doubleClick_LastTime = 0;

                        CheckIfPressShouldCallEvent();
                    }
                    else
                    {
                        doubleClick_LastTime = Time.time;
                    }
                }
            }

        } //END CheckForDoublePress

        //----------------------------------//
        private void CheckForPressAndHold()
        //----------------------------------//
        {

            //Keep a look out for Input events.. using mouse input
            if( InputAction_AnyHold() )
            {
                if( !Input.touchSupported )
                {
                    if( ( Input.GetMouseButton( 0 ) || Input.GetMouseButton( 1 ) ) && !waitForButtonUp )
                    {
                        hold_AccumulatedTime += Time.deltaTime;
                    }
                    else if( Input.GetMouseButtonUp( 0 ) || Input.GetMouseButtonUp( 1 ) )
                    {
                        // Do not forget to reset timer when the button is not pressed anymore
                        hold_AccumulatedTime = 0;
                        waitForButtonUp = false;
                    }

                    if( hold_AccumulatedTime > RequiredHoldTime && !waitForButtonUp )
                    {
                        //if( showDebug ) { Debug.Log( "BlockEventInput.cs CheckForPressAndHold() Non-Touch, calling CheckIfPressShouldCallEvent()" ); }
                        CheckIfPressShouldCallEvent();
                        waitForButtonUp = true;
                        hold_AccumulatedTime = 0;
                    }
                }
                else if( Input.touchSupported )
                {
                    if( Input.touchCount > 0 )
                    {
                        if( !waitForButtonUp )
                        {
                            hold_AccumulatedTime += Input.GetTouch( 0 ).deltaTime;
                        }

                        if( hold_AccumulatedTime >= RequiredHoldTime && !waitForButtonUp )
                        {
                            //if( showDebug ) { Debug.Log( "BlockEventInput.cs CheckForPressAndHold() Touch, calling CheckIfPressShouldCallEvent()" ); }
                            CheckIfPressShouldCallEvent();
                            waitForButtonUp = true;
                            hold_AccumulatedTime = 0;
                        }

                        if( Input.GetTouch( 0 ).phase == TouchPhase.Ended )
                        {
                            hold_AccumulatedTime = 0;
                            waitForButtonUp = false;
                        }
                    }
                }

            }

        } //END CheckForPressAndHold

        //-------------------------------------------//
        private void CheckIfPressShouldCallEvent()
        //-------------------------------------------//
        {

            if( IsInputTriggerEntireScreen() )
            {
                if( showDebug ) Debug.Log( "BlockEventInput.cs CheckForInput() Left or Right Press event" );
                _CallEvent();
            }
            else if( ( IsInputTriggerGameObject() && inputGameObject != null ) || ( IsInputTriggerTag() && !string.IsNullOrEmpty( inputTriggerTag ) ) )
            {
                showDebug = true;
                Ray ray = Camera.main.ScreenPointToRay( Input.mousePosition );
                //ray.origin = new Vector3( ray.origin.x, ray.origin.y, ray.origin.z + 10f );
                //Debug.Log( "ray.origin = " + ray.origin );
                RaycastHit[] hits = Physics.RaycastAll( ray, Mathf.Infinity );

                if( hits != null && hits.Length > 0 )
                {
                    foreach( RaycastHit hit in hits )
                    {
                        if( showDebug ) Debug.Log( "BlockEventInput.cs CheckForInput() Left or Right Press, GameObject hit.name( " + hit.collider.name + " )" );

                        if( IsInputTriggerGameObject() && hit.collider.gameObject == inputGameObject )
                        {
                            if( showDebug ) Debug.Log( "BlockEventInput.cs CheckForInput() Left or Right Press, found collider gameObject" );
                            _CallEvent();
                            break;
                        }
                        else if( IsInputTriggerGameObject() && hit.collider.gameObject != inputGameObject )
                        {
                            if( showDebug ) Debug.Log( "BlockEventInput.cs CheckForInput() Left or Right Press, GameObject hit.name( " + hit.collider.name + " ) != inputGameObject( " + inputGameObject.name + " )" );
                        }
                        else if( IsInputTriggerTag() && hit.collider.tag == inputTriggerTag )
                        {
                            if( showDebug ) Debug.Log( "BlockEventInput.cs CheckForInput() Left or Right Press, found matching tag" );
                            _CallEvent();
                            break;
                        }
                        else if( IsInputTriggerTag() && hit.collider.tag != inputTriggerTag )
                        {
                            if( showDebug ) Debug.Log( "BlockEventInput.cs CheckForInput() Left or Right Press, Tag hit.tag( " + hit.collider.tag + " ) != inputTriggerTag( " + inputTriggerTag + " )" );
                        }
                    }
                }
                else
                {
                    if( showDebug ) Debug.Log( "BlockEventInput.cs CheckForInput() Left or Right Press, no collider hit with raycast" );
                }
            }

        } //END CheckIfPressShouldCallEvent

        //-------------------------------//
        public override void PrepareEvent()
        //-------------------------------//
        {

            if( action == Actions.ListenForInput )
            {
                if( IsInputTriggerGameObject() && inputGameObject != null )
                {
                    eventReady = true;
                }
                else if( IsInputTriggerTag() && !string.IsNullOrEmpty( inputTriggerTag ) )
                {
                    eventReady = true;
                }
                else if( !IsInputTriggerGameObject() )
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
                if( action == Actions.ListenForInput )
                {
                    CallListenForInputEvent();
                }
            }

        } //END CallEvent

        //-------------------------------//
        private void CallListenForInputEvent()
        //-------------------------------//
        {

            if( onActionCompleted != null )
            {
                onActionCompleted.Invoke();
            }

        } //END CallListenForInputEvent


    } //END BlockEventInput

} //END Namespace