using Sirenix.OdinInspector;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

namespace BrandXR
{
    public class BlockEventBlockManager: BlockEventBase
    {
        //----------------- ACTIONS --------------------------------//
        public enum Actions
        {
            None,
            SendCommand
        }

        [TitleGroup( "Block Event - Block Manager", "Used to send commands to all Blocks contained within all of the Block Views" )]
        public Actions action = Actions.SendCommand;
        private bool IsActionSendCommand() { return action == Actions.SendCommand; }

        //------------- SEND COMMAND VARIABLES ---------------------------------//
        [Space(15f), ShowIf("IsActionSendCommand")]
        public BlockManager blockManager;

        [ShowIf("IsActionSendCommand")]
        public Block.CommandType command = Block.CommandType.Show;

        [Space( 15f ), ShowIf( "IsActionSendCommand" ), InfoBox( "Choose which Block Types you would like to send this command to, leave empty to impact all Block Types" )]
        List<Block.BlockType> impactBlockTypes = new List<Block.BlockType>();

        [Space( 15f ), ShowIf( "IsActionSendCommand" ), InfoBox("Add any BlockViews you want to exclude from this command, keep empty to send to all BlockViews")]
        public List<BlockView> excludeBlockViews = new List<BlockView>();

        [Space( 15f ), ShowIf( "IsActionSendCommand" ), InfoBox( "Add any BlockGroups you want to exclude from this command, keep empty to send to all BlockGroups" )]
        public List<BlockGroup> excludeBlockGroups = new List<BlockGroup>();

        [Space( 15f ), ShowIf( "IsActionSendCommand" ), InfoBox( "Add any Blocks you want to exclude from this command, keep empty to send to all Blocks" )]
        public List<Block> excludeBlocks = new List<Block>();

        //-------------------- EVENT MESSAGES ---------------------//
        private bool ShowOnActionCompletedEvent() { return action != Actions.None; }

        [Space( 15f ), ShowIf( "ShowOnActionCompletedEvent" ), FoldoutGroup( "Event Messages" )]
        public UnityEvent onActionCompleted;


        //--------------------------------------//
        public void Start()
        //--------------------------------------//
        {
            if( blockManager == null && BlockManager.instance != null )
            {
                blockManager = BlockManager.instance;
            }

        } //END Start

        //---------------------------------------------------------//
        public override EventType GetEventType()
        //---------------------------------------------------------//
        {
            return EventType.BlockManager;

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
            if( blockManager != null )
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

            if( blockManager != null )
            {
                blockManager.SendCommand( command, impactBlockTypes, excludeBlockViews, excludeBlockGroups, excludeBlocks );
            }

            if( onActionCompleted != null ) { onActionCompleted.Invoke(); }

        } //END CallSendCommand
        

    } //END BlockEventBlockManager

} //END Namespace