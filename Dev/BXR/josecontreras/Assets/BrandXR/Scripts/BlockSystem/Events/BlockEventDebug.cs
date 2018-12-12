using Sirenix.OdinInspector;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

namespace BrandXR
{
    public class BlockEventDebug: BlockEventBase
    {

        //----------------- ACTIONS --------------------------------//
        public enum Actions
        {
            None,
            CreateDebugLog
        }

        [TitleGroup( "Block Event - Debug", "Used to send debug messages to the Console" )]
        public Actions action = Actions.CreateDebugLog;
        private bool IsActionCreateDebugLog() { return action == Actions.CreateDebugLog; }

        //------------- SEND COMMAND VARIABLES ---------------------------------//
        public enum LogType
        {
            Normal,
            Warning,
            Error
        }
        
        [Space( 15f ), ShowIf( "IsActionCreateDebugLog" )]
        public LogType logType = LogType.Normal;

        [Space( 15f ), ShowIf( "IsActionCreateDebugLog" )]
        public string logMessage = "";

        //-------------------- EVENT MESSAGES ---------------------//
        private bool ShowOnActionCompletedEvent() { return action != Actions.None; }

        [Space( 15f ), ShowIf( "ShowOnActionCompletedEvent" ), FoldoutGroup( "Event Messages" )]
        public UnityEvent onActionCompleted;



        //---------------------------------------------------------//
        public override EventType GetEventType()
        //---------------------------------------------------------//
        {
            return EventType.Debug;

        } //END GetEventType

        //---------------------------------------------------------//
        public void SetAction( Actions action )
        //---------------------------------------------------------//
        {

            this.action = action;

        } //END SetAction

        //---------------------------------------------------------//
        [SerializeField]
        public void SetDebugLogMessage( string logText )
        //---------------------------------------------------------//
        {

            logMessage = logText;

        } //END SetDebugLogMessage

        //---------------------------------------------------------//
        [SerializeField]
        public void SetDebugLogMessageAndCall( string logText )
        //---------------------------------------------------------//
        {

            logMessage = logText;

            _CallEvent();

        } //END SetDebugLogMessageAndCall

        //-------------------------------//
        public override void PrepareEvent()
        //-------------------------------//
        {
            if( action == Actions.CreateDebugLog )
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
                if( action == Actions.CreateDebugLog )
                {
                    CallDebugLogEvent();
                }
            }

        } //END CallEvent

        //---------------------------------//
        private void CallDebugLogEvent()
        //---------------------------------//
        {

            if( logType == LogType.Normal )
            {
                Debug.Log( logMessage );
            }
            else if( logType == LogType.Warning )
            {
                Debug.LogWarning( logMessage );
            }
            else if( logType == LogType.Error )
            {
                Debug.LogError( logMessage );
            }

            if( onActionCompleted != null ) { onActionCompleted.Invoke(); }

        } //END CallDebugLogEvent



    } //END Class

} //END Namespace