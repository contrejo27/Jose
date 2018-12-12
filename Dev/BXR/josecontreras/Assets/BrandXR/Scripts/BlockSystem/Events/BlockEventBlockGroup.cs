using Sirenix.OdinInspector;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

namespace BrandXR
{
    public class BlockEventBlockGroup: BlockEventBase
    {
        //----------------- ACTIONS --------------------------------//
        public enum Actions
        {
            None,
            SendCommand
        }

        [TitleGroup( "Block Event - Block Group", "Used to send commands to all of the Blocks contained within a Block Group" )]
        public Actions action = Actions.SendCommand;
        private bool IsActionSendCommand() { return action == Actions.SendCommand; }

        //------------- SEND COMMAND VARIABLES ------------------------------//
        [Space( 15f ), ShowIf( "IsActionSendCommand" )]
        public BlockGroup blockGroup;

        [ShowIf( "IsActionSendCommand" )]
        public Block.CommandType command = Block.CommandType.Show;

        [Space( 15f ), ShowIf( "IsActionSendCommand" ), InfoBox( "Add the types of Blocks you want to send this command to, keep empty to send to all Block types" )]
        public List<Block.BlockType> impactBlockTypes = new List<Block.BlockType>();

        [Space( 15f ), ShowIf( "IsActionSendCommand" ), InfoBox( "Add any specific Blocks you want to ignore with this command, keep empty to send to all Block types" )]
        public List<Block> excludeBlocks = new List<Block>();

        [Space( 15f ), ShowIf( "IsActionSendCommand" ), InfoBox("Should this same command be sent to any of the nested BlockGroups contained within this BlockGroup?")]
        public bool sendCommandToNestedGroups = true;
        
        //-------------------- EVENT MESSAGES ---------------------//
        private bool ShowOnActionCompletedEvent() { return action != Actions.None; }

        [Space( 15f ), ShowIf( "ShowOnActionCompletedEvent" ), FoldoutGroup( "Event Messages" )]
        public UnityEvent onActionCompleted;


        //---------------------------------------------------------//
        public override EventType GetEventType()
        //---------------------------------------------------------//
        {
            return EventType.BlockGroup;

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
            if( blockGroup != null )
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

        } //END CallEvent

        //---------------------------------//
        private void CallSendCommand()
        //---------------------------------//
        {

            if( blockGroup != null )
            {
                blockGroup.SendCommand( command, impactBlockTypes, excludeBlocks, sendCommandToNestedGroups );
            }
            
            if( onActionCompleted != null ) { onActionCompleted.Invoke(); }

        } //END CallSendCommand
        


    } //END BlockEventBlockGroup

} //END Namespace