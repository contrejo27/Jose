using Sirenix.OdinInspector;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

namespace BrandXR
{
    public class BlockEventBlock: BlockEventBase
    {
        //----------------- ACTIONS --------------------------------//
        public enum Actions
        {
            None,
            SendCommand
        }

        [TitleGroup( "Block Event - Block", "Used to send a command to a specific Block" )]
        public Actions action = Actions.SendCommand;
        private bool IsActionSendCommand() { return action == Actions.SendCommand; }

        //------------- SEND COMMAND VARIABLES ---------------------------------//
        [Space(15f), ShowIf("IsActionSendCommand")]
        public Block block;

        [ShowIf( "IsActionSendCommand" )]
        public Block.CommandType command = Block.CommandType.Show;
        private bool IsCommandFaceCamera() { return IsActionSendCommand() && command == Block.CommandType.FaceCamera; }

        //------------- FACE CAMERA COMMAND VARIABLES --------------------------//
        [Space( 15f ), ShowIf( "IsCommandFaceCamera" ), InfoBox( "Set the booleans below to change which axis will be impacted when this block rotates to face the camera" )]
        public bool forceX = true;

        [ShowIf( "IsCommandFaceCamera" )]
        public bool forceY = true;

        [ShowIf( "IsCommandFaceCamera" )]
        public bool forceZ = true;

        //-------------------- EVENT MESSAGES ---------------------//
        private bool ShowOnActionCompletedEvent() { return action != Actions.None; }

        [Space( 15f ), ShowIf( "ShowOnActionCompletedEvent" ), FoldoutGroup( "Event Messages" )]
        public UnityEvent onActionCompleted;


        //---------------------------------------------------------//
        public override EventType GetEventType()
        //---------------------------------------------------------//
        {
            return EventType.Block;

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
            if( block != null )
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
                if( action == Actions.SendCommand )
                {
                    CallSendCommand();
                }
            }

        } //END _CallEvent

        //---------------------------------//
        private void CallSendCommand()
        //---------------------------------//
        {

            if( block != null )
            {
                if( command != Block.CommandType.FaceCamera )
                {
                    block.SendCommand( command );
                }
                else if( command == Block.CommandType.FaceCamera )
                {
                    block.FaceCamera( forceX, forceY, forceZ );
                }
            }

            if( onActionCompleted != null ) { onActionCompleted.Invoke(); }

        } //END CallSendCommand
        
    } //END BlockEventBlock

} //END Namespace