using Sirenix.OdinInspector;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

namespace BrandXR
{
    public class BlockEventCanvas: BlockEventBase
    {
        //----------------- ACTIONS --------------------------------//
        public enum Actions
        {
            None,
            SetRenderMode
        }

        [TitleGroup( "Block Event - Canvas", "Used to modify a Unity Canvas settings" )]
        public Actions action = Actions.SetRenderMode;
        private bool CanvasAction_SetRenderMode() { return action == Actions.SetRenderMode; }
        
        //------------- VARIABLES ---------------------------------//
        [Space( 15f ), ShowIf( "CanvasAction_SetRenderMode" )]
        public Canvas canvas;
        
        [ShowIf( "CanvasAction_SetRenderMode" )]
        public RenderMode renderMode;

        //-------------------- "SET RENDER MODE" EVENT MESSAGES ---------------------//
        private bool ShowOnActionCompletedEvent() { return action == Actions.SetRenderMode; }

        [Space( 15f ), ShowIf( "ShowOnActionCompletedEvent" ), FoldoutGroup( "Event Messages" )]
        public UnityEvent onActionCompleted;


        //---------------------------------------------------------//
        public override EventType GetEventType()
        //---------------------------------------------------------//
        {
            return EventType.Canvas;

        } //END GetEventType

        //-------------------------------//
        public override void PrepareEvent()
        //-------------------------------//
        {

            if( action == Actions.SetRenderMode )
            {
                if( canvas != null )
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
                if( action == Actions.SetRenderMode )
                {
                    CallSetRenderModeEvent();
                }
            }
            
        } //END CallEvent
        
        //-------------------------------//
        private void CallSetRenderModeEvent()
        //-------------------------------//
        {

            if( canvas != null )
            {
                canvas.renderMode = renderMode;
            }

            if( onActionCompleted != null ) { onActionCompleted.Invoke(); }

        } //END CallSetRenderModeEvent

    } //END BlockEventCanvas

} //END Namespace