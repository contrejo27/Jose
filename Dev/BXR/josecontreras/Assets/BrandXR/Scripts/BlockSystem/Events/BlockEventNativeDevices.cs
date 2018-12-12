using Sirenix.OdinInspector;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

namespace BrandXR
{
    public class BlockEventNativeDevices: BlockEventBase
    {
        //To get more information on how to implement native functionality visit this wiki: https://docs.google.com/document/d/19Ulpy4P5QRPe5qMl_KTCHGdcwI1xJgfi6sQTZp828xU/edit?usp=sharing

        //----------------- ACTIONS --------------------------------//
        public enum Actions
        {
            None,
            CallNativeFunction
        }

        [TitleGroup( "Block Event - NativeDevices", "Used to communicate with native Android and IOS functionality." )]
        public Actions action = Actions.CallNativeFunction;
        private bool IsActionCallNativeFunction() { return action == Actions.CallNativeFunction; }

        [Space( 15f ), ShowIf("IsActionCallNativeFunction")]
        public string className = "";

        [Space( 15f ), ShowIf("IsActionCallNativeFunction")]
        public string functionCall = "";

        [Space(15f), ShowIf("IsActionCallNativeFunction")]
        public string argDelimiter = ",";

        [Space(15f), ShowIf("IsActionCallNativeFunction")]
        public List<string> functionArgs = new List<string>();

        //-------------------- EVENT MESSAGES ---------------------//
        private bool ShowOnActionCompletedEvent() { return action != Actions.None; }

        [Space( 15f ), ShowIf( "ShowOnActionCompletedEvent" ), FoldoutGroup( "Event Messages" )]
        public UnityEvent onActionCompleted;

        //---------------------------------------------------------//
        public override EventType GetEventType()
        //---------------------------------------------------------//
        {
            return EventType.NativeDevices;

        } //END GetEventType

        //---------------------------------------------------------//
        public void SetAction( Actions action )
        //---------------------------------------------------------//
        {

            this.action = action;

        } //END SetAction


        //---------------------------------------------------------//
        [SerializeField]
        public void SetAndCallNativeFunction( string classNameArg, string functionCallArg, List<string> args)
        //---------------------------------------------------------//
        {

            className = classNameArg;
            functionCall = functionCallArg;
            functionArgs = args;

            _CallEvent();

        } //END SetAndCallNativeFunction

        //-------------------------------//
        public override void PrepareEvent()
        //-------------------------------//
        {
            if( action == Actions.CallNativeFunction)
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
                if( action == Actions.CallNativeFunction)
                {
                    CallNativeEvent();
                }
            }

        } //END CallEvent
        //-------------------------------//

        //---------------------------------//
        private void CallNativeEvent()
        //---------------------------------//
        {
            //This funcionality only works when unity is working inside a native Android/IOS app. 
            //To get more information on how to implement visit this wiki: https://docs.google.com/document/d/19Ulpy4P5QRPe5qMl_KTCHGdcwI1xJgfi6sQTZp828xU/edit?usp=sharing
            WWWHelper.instance.SendNativeDeviceCommand(className, functionCall, functionArgs, argDelimiter);

            if (onActionCompleted != null) { onActionCompleted.Invoke(); }
        } //END CallNativeEvent

    } //END CallNativeEvent

} //END Namespace