using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using Sirenix.OdinInspector;

namespace BrandXR
{
    public class BlockEventUnity: BlockEventBase
    {

        //----------------- ACTIONS --------------------------------//
        public enum Actions
        {
            None,
            CallUnityEvent
        }

        [TitleGroup( "Block Event - Unity", "Used to call a Unity Action" )]
        public Actions action = Actions.CallUnityEvent;

        //----------------- "CALL UNITY EVENT" VARIABLES ------------------------------//
        
        //----------------- "CALL UNITY EVENT" EVENTS ------------------------------//
        private bool ShowOnActionCompletedEvent() { return action == Actions.CallUnityEvent; }

        [Space( 15f ), ShowIf( "ShowOnActionCompletedEvent" ), FoldoutGroup( "Event Messages" )]
        public UnityEvent onActionCompleted = new UnityEvent();



        //---------------------------------------------------------//
        public override EventType GetEventType()
        //---------------------------------------------------------//
        {
            return EventType.Unity;

        } //END GetEventType

        //---------------------------------------------------------//
        public void SetAction( Actions action )
        //---------------------------------------------------------//
        {

            this.action = action;

        } //END SetAction

        //-------------------------------//
        public override void PrepareEvent()
        //-------------------------------//
        {

            if( action == Actions.CallUnityEvent )
            {
                eventReady = true;
            }
            
        } //END PrepareEvent

        //-------------------------------//
        protected override void _CallEvent()
        //-------------------------------//
        {
            base._CallEvent();

            if( eventReady )
            {
                if( action == Actions.CallUnityEvent )
                {
                    CallUnityEvent();
                }
            }
            
        } //END CallEvent

        //------------------------------//
        private void CallUnityEvent()
        //------------------------------//
        {

            if( onActionCompleted != null ) { onActionCompleted.Invoke(); }

        } //END CallUnityEvent


    } //END BlockEventUnity
    
} //END Namespace