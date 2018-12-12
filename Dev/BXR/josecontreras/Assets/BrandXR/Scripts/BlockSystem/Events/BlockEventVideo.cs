using Sirenix.OdinInspector;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;
using UnityEngine.Video;

namespace BrandXR
{
    public class BlockEventVideo: BlockEventBase
    {
        //----------------- ACTIONS --------------------------------//
        public enum Actions
        {
            None,
            PauseVideo,
            StopVideo,
            ResumeVideo,
            ChangeVideoAndPlay,
            SwitchToRenderer,
            SwitchToPlane,
            SwitchToSphere,
            SwitchToFullscreen,
            SwitchToMono,
            SwitchToStereoTopBottom,
            SwitchToStereoSideBySide
        }

        [TitleGroup( "Block Event - Video", "Used To Modify A Video Block" )]
        public Actions action = Actions.ChangeVideoAndPlay;
        private bool PauseVideoSelected() { return action == Actions.PauseVideo; }
        private bool StopVideoSelected() { return action == Actions.StopVideo; }
        private bool ResumeVideoSelected() { return action == Actions.ResumeVideo; }
        private bool ChangeVideoAndPlaySelected() { return action == Actions.ChangeVideoAndPlay; }

        private bool SwitchVideoToAnything() { return ( action == Actions.SwitchToRenderer || action == Actions.SwitchToPlane || action == Actions.SwitchToSphere || action == Actions.SwitchToFullscreen ); }
        private bool SwitchVideoToRenderer() { return action == Actions.SwitchToRenderer; }
        private bool SwitchVideoToPlane() { return action == Actions.SwitchToPlane; }
        private bool SwitchVideoToSphere() { return action == Actions.SwitchToSphere; }
        private bool SwitchVideoToFullscreen() { return action == Actions.SwitchToFullscreen; }

        private bool SwitchVideoStereoSettings() { return ( action == Actions.SwitchToMono || action == Actions.SwitchToStereoTopBottom || action == Actions.SwitchToStereoSideBySide ); }
        private bool SwitchVideoTo2D() { return action == Actions.SwitchToMono; }
        private bool SwitchVideoTo3DTopBottom() { return action == Actions.SwitchToStereoTopBottom; }
        private bool SwitchVideoTo3DSideBySide() { return action == Actions.SwitchToStereoSideBySide; }
        
        private bool ShowVideoActions() { return action != Actions.None; }
        
        //------------- VARIABLES ---------------------------------//
        [ShowIf( "ShowVideoActions" ), FoldoutGroup( "Video Settings" )]
        public BlockVideo blockVideo;
        
        public enum ChangeVideoUsing
        {
            InputField,
            VideoClip,
            Path
        }

        [ShowIf( "ChangeVideoAndPlaySelected" ), FoldoutGroup( "Video Settings" )]
        public ChangeVideoUsing changeVideoUsing = ChangeVideoUsing.VideoClip;
        private bool ChangeVideoUsingInputField() { return ChangeVideoAndPlaySelected() && changeVideoUsing == ChangeVideoUsing.InputField; }
        private bool ChangeVideoUsingClip() { return ChangeVideoAndPlaySelected() && changeVideoUsing == ChangeVideoUsing.VideoClip; }
        private bool ChangeVideoUsingPath() { return ChangeVideoAndPlaySelected() && changeVideoUsing == ChangeVideoUsing.Path; }

        [ShowIf( "ChangeVideoUsingInputField" ), FoldoutGroup( "Video Settings" )]
        public InputField changeVideoUsingInputField;

        [ShowIf( "ChangeVideoUsingClip" ), FoldoutGroup( "Video Settings" )]
        public VideoClip changeToVideoClip;

        [ShowIf( "ChangeVideoUsingPath" ), FoldoutGroup( "Video Settings" )]
        public string changeToVideoPath;



        [ShowIf( "ChangeVideoAndPlaySelected" ), FoldoutGroup( "Video Settings" )]
        public float videoVolume = 1f;
        [ShowIf( "ChangeVideoAndPlaySelected" ), FoldoutGroup( "Video Settings" )]
        public BlockVideo.WhenFinished whenVideoFinished = BlockVideo.WhenFinished.DoNothing;



        //-------------------- EVENT MESSAGES ---------------------//
        private bool ShowOnActionCompletedEventMessage() { return ShowVideoActions(); }

        [ShowIf( "ShowOnActionCompletedEventMessage" ), FoldoutGroup( "Event Messages" )]
        public UnityEvent onActionCompleted = new UnityEvent();
        



        //---------------------------------------------------------//
        public override EventType GetEventType()
        //---------------------------------------------------------//
        {
            return EventType.Video;

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

            if( ChangeVideoAndPlaySelected() )
            {
                if( ChangeVideoUsingInputField() && changeVideoUsingInputField != null )
                {
                    eventReady = true;
                }
                else if( ChangeVideoUsingClip() && changeToVideoClip != null )
                {
                    eventReady = true;
                }
                else if( ChangeVideoUsingPath() && !string.IsNullOrEmpty( changeToVideoPath ) )
                {
                    eventReady = true;
                }
                else
                {
                    eventReady = false;
                }
            }
            else
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
                if( action == Actions.PauseVideo )                  { CallPauseVideoAction(); }
                else if( action == Actions.StopVideo )              { CallStopVideoAction(); }
                else if( action == Actions.ResumeVideo )            { CallResumeVideoAction(); }
                else if( action == Actions.ChangeVideoAndPlay )     { CallChangeVideoAndPlayAction(); }
                else if( action == Actions.SwitchToRenderer )       { CallSwitchToRendererAction(); }
                else if( action == Actions.SwitchToPlane )          { CallSwitchToPlaneAction(); }
                else if( action == Actions.SwitchToSphere )         { CallSwitchToSphereAction(); }
                else if( action == Actions.SwitchToFullscreen )     { CallSwitchToFullscreenAction(); }
                else if( action == Actions.SwitchToMono )             { CallSwitchTo2DAction(); }
                else if( action == Actions.SwitchToStereoTopBottom )    { CallSwitchTo3DTopBottomAction(); }
                else if( action == Actions.SwitchToStereoSideBySide )   { CallSwitchTo3DSideBySideAction(); }
            }

        } //END CallEvent
        
        //-------------------------------//
        private void CallPauseVideoAction()
        //-------------------------------//
        {

            if( blockVideo != null )
            {
                blockVideo.Pause();

                if( onActionCompleted != null ) { onActionCompleted.Invoke(); }
            }

        } //END CallPauseVideoAction

        //-------------------------------//
        private void CallStopVideoAction()
        //-------------------------------//
        {

            if( blockVideo != null )
            {
                blockVideo.Stop();

                if( onActionCompleted != null ) { onActionCompleted.Invoke(); }
            }

        } //END CallStopVideoAction

        //-------------------------------//
        private void CallResumeVideoAction()
        //-------------------------------//
        {

            if( blockVideo != null )
            {
                blockVideo.Resume();

                if( onActionCompleted != null ) { onActionCompleted.Invoke(); }
            }

        } //END CallResumeVideoAction

        //-------------------------------//
        private void CallChangeVideoAndPlayAction()
        //-------------------------------//
        {

            if( blockVideo != null )
            {
                blockVideo.SetVolume( videoVolume );
                blockVideo.SetWhenFinishedAction( whenVideoFinished );

                if( ChangeVideoUsingInputField() )
                {
                    blockVideo.Play( changeVideoUsingInputField.text );
                }
                else if( ChangeVideoUsingClip() )
                {
                    blockVideo.Play( changeToVideoClip );
                }
                else if( ChangeVideoUsingPath() )
                {
                    blockVideo.Play( changeToVideoPath );
                }

                if( onActionCompleted != null ) { onActionCompleted.Invoke(); }
            }

        } //END CallChangeVideoAndPlayAction

        //-------------------------------------//
        private void CallSwitchToRendererAction()
        //-------------------------------------//
        {

            if( blockVideo != null )
            {
                blockVideo.SwitchVideoToRenderer();
                if( onActionCompleted != null ) { onActionCompleted.Invoke(); }
            }

        } //END CallSwitchToRendererAction

        //-------------------------------------//
        private void CallSwitchToPlaneAction()
        //-------------------------------------//
        {

            if( blockVideo != null )
            {
                blockVideo.SwitchVideoToPlane();
                if( onActionCompleted != null ) { onActionCompleted.Invoke(); }
            }

        } //END CallSwitchToPlaneAction

        //-------------------------------------//
        private void CallSwitchToSphereAction()
        //-------------------------------------//
        {

            if( blockVideo != null )
            {
                blockVideo.SwitchVideoToSphere();
                if( onActionCompleted != null ) { onActionCompleted.Invoke(); }
            }

        } //END CallSwitchToSphereAction

        //-------------------------------------//
        private void CallSwitchToFullscreenAction()
        //-------------------------------------//
        {

            if( blockVideo != null )
            {
                blockVideo.SwitchVideoToFullscreen();
                if( onActionCompleted != null ) { onActionCompleted.Invoke(); }
            }

        } //END CallSwitchToFullscreenAction

        //-------------------------------------//
        private void CallSwitchTo2DAction()
        //-------------------------------------//
        {

            if( blockVideo != null )
            {
                blockVideo.SetStereoSettings( BlockVideo.StereoType.Mono );
                if( onActionCompleted != null ) { onActionCompleted.Invoke(); }
            }

        } //END CallSwitchTo2DAction

        //-------------------------------------//
        private void CallSwitchTo3DTopBottomAction()
        //-------------------------------------//
        {

            if( blockVideo != null )
            {
                blockVideo.SetStereoSettings( BlockVideo.StereoType.StereoTopBottom );
                if( onActionCompleted != null ) { onActionCompleted.Invoke(); }
            }

        } //END CallSwitchTo3DTopBottomAction

        //-------------------------------------//
        private void CallSwitchTo3DSideBySideAction()
        //-------------------------------------//
        {

            if( blockVideo != null )
            {
                blockVideo.SetStereoSettings( BlockVideo.StereoType.StereoSideBySide );
                if( onActionCompleted != null ) { onActionCompleted.Invoke(); }
            }

        } //END CallSwitchTo3DSideBySideAction


    } //END BlockEventVideo

} //END Namespace