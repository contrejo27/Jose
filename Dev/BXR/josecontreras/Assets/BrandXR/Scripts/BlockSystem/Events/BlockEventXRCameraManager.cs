using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using Sirenix.OdinInspector;

namespace BrandXR
{
    public class BlockEventXRCameraManager: BlockEventBase
    {

        //----------------- ACTIONS --------------------------------//
        public enum Actions
        {
            None,
            DisableCamera,
            EnableCamera,
            DisableFloorTrackerIcon,
            EnableFloorTrackerIcon,
            SetFloorTrackIconTexture
        }

        [TitleGroup( "Block Event - XRCameraManager", "Used to camera events such as Disable() and Enable()" )]
        public Actions action = Actions.None;

        //----------------- COMMON VARIABLES ------------------------------//
        public XRCameraManager xrCameraManager = null;

        //----------------- SET FLOOR TRACKER ICON TEXTURE VARIABLES -----------//
        [ShowIf("action", Actions.SetFloorTrackIconTexture)]
        public Texture changeFloorIconToTexture = null;

        //----------------- EVENTS ------------------------------//
        private bool ShowOnActionCompletedEvent() { return action == Actions.DisableCamera; }

        [Space( 15f ), ShowIf( "ShowOnActionCompletedEvent" ), FoldoutGroup( "Event Messages" )]
        public UnityEvent onActionCompleted = new UnityEvent();



        //---------------------------------------------------------//
        public override EventType GetEventType()
        //---------------------------------------------------------//
        {
            return EventType.XRCameraManager;

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

            if( action == Actions.DisableCamera )
            {
                if( xrCameraManager != null )
                {
                    eventReady = true;
                }
            }
            else if( action == Actions.EnableCamera )
            {
                if( xrCameraManager != null )
                {
                    eventReady = true;
                }
            }
            else if( action == Actions.DisableFloorTrackerIcon )
            {
                if (xrCameraManager != null)
                {
                    eventReady = true;
                }
            }
            else if (action == Actions.EnableFloorTrackerIcon )
            {
                if (xrCameraManager != null)
                {
                    eventReady = true;
                }
            }
            else if (action == Actions.SetFloorTrackIconTexture)
            {
                if (xrCameraManager != null && changeFloorIconToTexture != null )
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
                if( action == Actions.DisableCamera )
                {
                    CallDisableCameraEvent();
                }
                else if( action == Actions.EnableCamera )
                {
                    CallEnableCameraEvent();
                }
                else if (action == Actions.DisableFloorTrackerIcon)
                {
                    CallDisableFloorTrackerIconEvent();
                }
                else if (action == Actions.EnableFloorTrackerIcon)
                {
                    CallEnableFloorTrackerIconEvent();
                }
                else if (action == Actions.SetFloorTrackIconTexture)
                {
                    SetFloorTrackerIconTextureEvent();
                }
            }

        } //END CallEvent

        //------------------------------//
        private void CallDisableCameraEvent()
        //------------------------------//
        {

            if( xrCameraManager != null )
            {
                xrCameraManager.DisableCamera();
            }

            if( onActionCompleted != null ) { onActionCompleted.Invoke(); }

        } //END CallDisableCameraEvent

        //------------------------------//
        private void CallEnableCameraEvent()
        //------------------------------//
        {

            if( xrCameraManager != null )
            {
                xrCameraManager.EnableCamera();
            }

            if( onActionCompleted != null ) { onActionCompleted.Invoke(); }

        } //END CallEnableCameraEvent

        //------------------------------//
        private void CallDisableFloorTrackerIconEvent()
        //------------------------------//
        {

            if (xrCameraManager != null)
            {
                xrCameraManager.DisableFloorTrackerIcon();
            }

            if (onActionCompleted != null) { onActionCompleted.Invoke(); }

        } //END CallDisableFloorTrackerIconEvent

        //------------------------------//
        private void CallEnableFloorTrackerIconEvent()
        //------------------------------//
        {

            if (xrCameraManager != null)
            {
                xrCameraManager.EnableFloorTrackerIcon();
            }

            if (onActionCompleted != null) { onActionCompleted.Invoke(); }

        } //END CallEnableFloorTrackerIconEvent

        //------------------------------//
        private void SetFloorTrackerIconTextureEvent()
        //------------------------------//
        {

            if (xrCameraManager != null && 
                changeFloorIconToTexture != null)
            {
                xrCameraManager.SetFloorTrackerIconTexture( changeFloorIconToTexture );
            }

            if (onActionCompleted != null) { onActionCompleted.Invoke(); }

        } //END SetFloorTrackerIconTextureEvent


    } //END BlockEventXRCameraManager

} //END Namespace