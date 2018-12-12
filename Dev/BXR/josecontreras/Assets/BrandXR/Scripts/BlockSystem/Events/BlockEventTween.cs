using Sirenix.OdinInspector;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

namespace BrandXR
{
    public class BlockEventTween: BlockEventBase
    {
        //----------------- ACTIONS --------------------------------//
        public enum Action
        {
            None,
            ModifyTween,
            CreateTween
        }

        [TitleGroup( "Block Event - Tween", "Used to Modify or Create a new Tween" )]
        [OnValueChanged( "TypeChanged" )]
        public Action action = Action.None;
        private bool Action_None() { return action == Action.ModifyTween; }
        private bool Action_Modify() { return action == Action.ModifyTween; }
        private bool Action_Create() { return action == Action.CreateTween; }
        private bool Action_ModifyOrCreate() { return action == Action.ModifyTween || action == Action.CreateTween; }
        
        [Space(15f), ShowIf( "Action_Modify")]
        public TweenManager.TweenCommands tweenCommand = TweenManager.TweenCommands.Play;
        private bool TweenAction_Play() { return Action_Modify() && tweenCommand == TweenManager.TweenCommands.Play; }
        private bool TweenAction_Stop() { return Action_Modify() && tweenCommand == TweenManager.TweenCommands.Stop; }
        private bool TweenAction_Pause() { return Action_Modify() && tweenCommand == TweenManager.TweenCommands.Pause; }
        private bool TweenAction_Resume() { return Action_Modify() && tweenCommand == TweenManager.TweenCommands.Resume; }
        
        [Space( 15f ), ShowIf( "Action_Create" ), OnValueChanged( "TypeChanged" )]
        public bxrTween.TweenType tweenType = bxrTween.TweenType.Position;

        [Space( 15f ), ShowIf( "Action_ModifyOrCreate" )]
        public bxrTween tween;




        //-------------------- EVENT MESSAGES ---------------------//
        private bool ShowOnActionCompletedEvent() { return action == Action.CreateTween || action == Action.ModifyTween; }

        [Space( 15f ), ShowIf( "ShowOnActionCompletedEvent" ), FoldoutGroup( "Event Messages" )]
        public UnityEvent onActionCompleted;





        //---------------------------------//
        public void TypeChanged()
        //---------------------------------//
        {

            RemoveExistingComponent();


            #if !UNITY_EDITOR
                AddNewComponent();
            #endif

        } //END TypeChanged

        //----------------------------------------------//
        private void RemoveExistingComponent()
        //----------------------------------------------//
        {

            //If this tween already exists, destroy it
            if( tween != null )
            {
                tween.PrepareForDestroy();

                #if UNITY_EDITOR
                    //Wait a moment before calling DestroyImmediate to make sure no logic is running
                    UnityEditor.EditorApplication.delayCall += () =>
                    {
                        DestroyImmediate( tween );

                        //After the component is deleted, add the new one!
                        AddNewComponent();
                    };
                #else
                    Destroy( tween );
                #endif
            }
            else
            {
                #if UNITY_EDITOR
                    //If in the editor and there was no tween to destroy, go ahead and make the new tween
                    AddNewComponent();
                #endif
            }
            
        } //END RemoveExistingComponent

        //----------------------------------------------//
        private void AddNewComponent()
        //----------------------------------------------//
        {
            //If we're adding a new tween via this event, create the tween and attach it to this gameObject for easy editing
            if( action == Action.CreateTween )
            {
                if( action == Action.None ) { return; }

                string newComponentName = "BrandXR.bxrTween" + tweenType.ToString();

                Type componentType = ComponentHelper.FindType( newComponentName );

                //showDebug = true;
                if( showDebug ) { Debug.Log( "BlockEventTween.cs AddNewComponent() name = " + newComponentName + ", Type = " + componentType ); }

                tween = (bxrTween)gameObject.AddComponent( componentType );
            }
            
        } //END AddNewComponent
        
        


        


        //---------------------------------------------------------//
        public override EventType GetEventType()
        //---------------------------------------------------------//
        {
            return EventType.Tween;

        } //END GetEventType

        //---------------------------------------------------------//
        public void SetAction( Action action )
        //---------------------------------------------------------//
        {

            this.action = action;

            TypeChanged();

        } //END SetAction


        //-------------------------------//
        public override void PrepareEvent()
        //-------------------------------//
        {

            if( action == Action.CreateTween )
            {
                if( tween != null )
                {
                    eventReady = true;
                }
            }
            else if( action == Action.ModifyTween )
            {
                if( tween != null )
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
                if( action == Action.CreateTween )
                {
                    CallCreateTweenEvent();
                }
                else if( action == Action.ModifyTween )
                {
                    CallModifyTweenEvent();
                }
            }

        } //END CallEvent

        //---------------------------------//
        private void CallCreateTweenEvent()
        //---------------------------------//
        {

            if( tween == null )
            {
                TypeChanged();
            }

            tween.Play();

            if( onActionCompleted != null ) { onActionCompleted.Invoke(); }

        } //END CallCreateTweenEvent

        //---------------------------------//
        private void CallModifyTweenEvent()
        //---------------------------------//
        {

            if( tween != null )
            {
                TweenManager.SendCommandToTweens( tween, tweenCommand );
            }

            if( onActionCompleted != null ) { onActionCompleted.Invoke(); }

        } //END CallModifyTweenEvent
        

    } //END BlockEventTween

} //END Namespace