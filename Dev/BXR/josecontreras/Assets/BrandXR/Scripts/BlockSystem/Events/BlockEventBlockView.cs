using Sirenix.OdinInspector;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

namespace BrandXR
{
    public class BlockEventBlockView: BlockEventBase
    {
        //----------------- ACTIONS --------------------------------//
        public enum Actions
        {
            None,
            SendCommand
        }

        [TitleGroup( "Block Event - Block View", "Used to send commands to all of the BlockGroups contained within a Block View" )]
        public Actions action = Actions.SendCommand;
        private bool IsActionSendCommand() { return action == Actions.SendCommand; }

        //------------- SEND COMMAND VARIABLES ---------------------------------//
        [Space(15f), ShowIf("IsActionSendCommand")]
        public BlockView blockView;

        [ShowIf("IsActionSendCommand")]
        public Block.CommandType command = Block.CommandType.Show;

        [Space( 15f ), ShowIf( "IsActionSendCommand" ), InfoBox("Add the types of Blocks you want to send this command to, keep empty to send to all Block types")]
        public List<Block.BlockType> impactBlockTypes = new List<Block.BlockType>();

        [Space( 15f ), ShowIf( "IsActionSendCommand" ), InfoBox( "Add any nested BlockGroups you want to ignore when sending this command, keep empty to send to all nested block groups" )]
        public List<BlockGroup> excludeBlockGroups = new List<BlockGroup>();

        [Space( 15f ), ShowIf( "IsActionSendCommand" ), InfoBox( "Add any specific Blocks you want to ignore with this command" )]
        public List<Block> excludeBlocks = new List<Block>();

        [Space( 15f ), ShowIf( "IsActionSendCommand" ), InfoBox("Should this command also be sent to the Linked Block View?")]
        public bool alsoSendToLinkedBlockView = true;

        //-------------------- EVENT MESSAGES ---------------------//
        private bool ShowOnActionCompletedEvent() { return action != Actions.None; }

        [Space( 15f ), ShowIf( "ShowOnActionCompletedEvent" ), FoldoutGroup( "Event Messages" )]
        public UnityEvent onActionCompleted;


        //---------------------------------------------------------//
        public override EventType GetEventType()
        //---------------------------------------------------------//
        {
            return EventType.BlockView;

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
            if( blockView != null )
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
                    CallSendCommandEvent();
                }
            }

        } //END CallEvent

        //---------------------------------//
        private void CallSendCommandEvent()
        //---------------------------------//
        {

            if( blockView != null )
            {
                blockView.SendCommand( command, impactBlockTypes, excludeBlockGroups, excludeBlocks, alsoSendToLinkedBlockView );
            }

            if( onActionCompleted != null ) { onActionCompleted.Invoke(); }

        } //END CallSendCommandEvent
        


    } //END BlockEventBlockView

} //END Namespace