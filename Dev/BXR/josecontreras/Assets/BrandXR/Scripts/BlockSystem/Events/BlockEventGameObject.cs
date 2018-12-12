using Sirenix.OdinInspector;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace BrandXR
{
    public class BlockEventGameObject: BlockEventBase
    {

        //----------------- ACTIONS --------------------------------//
        public enum Actions
        {
            None,
            Enable,
            Disable,
            FlipActiveState,
            Delete,
            Create
        }

        [TitleGroup( "Block Event - GameObject", "Used to Modify, Create, Delete, or Activate a GameObject" )]
        public Actions action = Actions.Enable;
        private bool GameObjectActionIsEnable() { return action == Actions.Enable; }
        private bool GameObjectActionIsDisable() { return action == Actions.Disable; }
        private bool GameObjectActionIsFlipActiveState() { return action == Actions.FlipActiveState; }
        private bool GameObjectActionIsDelete() { return action == Actions.Delete; }
        private bool GameObjectActionIsCreate() { return action == Actions.Create; }
        private bool GameObjectActionIsEnableDisableFlipDelete() { return ( action == Actions.Enable || action == Actions.Disable || action == Actions.FlipActiveState || action == Actions.Delete ); }
        private bool GameObjectActionIsEnableDisableFlipDeleteCreate() { return ( action == Actions.Enable || action == Actions.Disable || action == Actions.FlipActiveState || action == Actions.Delete || action == Actions.Create ); }

        //------------------ VARIABLES ------------------------//

        [Space( 15f ), ShowIf( "GameObjectActionIsEnableDisableFlipDeleteCreate" )]
        public GameObject gameObjectToEffect;


        public enum GameObjectEffectOptions
        {
            PerformActionImmediately,
            HideAllChildrenBlocksFirst,
            ShowAllChildrenBlocksFirst
        }

        [ShowIf( "GameObjectActionIsEnableDisableFlipDelete" )]
        public GameObjectEffectOptions effectOptions = GameObjectEffectOptions.PerformActionImmediately;
        private bool GameEffectOptionsSetToHideOrShow() { return GameObjectActionIsEnableDisableFlipDelete() && ( effectOptions == GameObjectEffectOptions.HideAllChildrenBlocksFirst || effectOptions == GameObjectEffectOptions.ShowAllChildrenBlocksFirst ); }

        public enum GameObjectTweenSettings
        {
            UseExistingTweenValues,
            OverwriteAllTweenValues
        }

        [ShowIf( "GameEffectOptionsSetToHideOrShow" )]
        public GameObjectTweenSettings tweenSettings = GameObjectTweenSettings.UseExistingTweenValues;
        private bool GameObjectOverwriteTweenValues() { return GameEffectOptionsSetToHideOrShow() && tweenSettings == GameObjectTweenSettings.OverwriteAllTweenValues; }

        private Coroutine coroutine_GameObjectTweenComplete;

        [ShowIf( "GameObjectOverwriteTweenValues" )]
        public float gameObjectTweenLength = 1f;

        [ShowIf( "GameObjectOverwriteTweenValues" )]
        public float gameObjectTweenDelay = 0f;

        public enum GameObjectColliderOptions
        {
            DoNotChangeChildColliders,
            EnableColliders,
            DisableColliders,
            FlipActiveState
        }

        [ShowIf( "GameObjectActionIsEnableDisableFlipDelete" )]
        public GameObjectColliderOptions colliderOptions = GameObjectColliderOptions.DoNotChangeChildColliders;
        private bool GameObjectColliderIsChanging() { return GameObjectActionIsEnableDisableFlipDelete() && colliderOptions != GameObjectColliderOptions.DoNotChangeChildColliders; }

        public enum GameObjectColliderType
        {
            Colliders,
            Buttons,
            Images,
            UIColliderManager,
            AllTypes
        }

        [ShowIf( "GameObjectColliderIsChanging" )]
        public GameObjectColliderType colliderType = GameObjectColliderType.Colliders;

        public enum GameObjectNamingOptions
        {
            KeepCloneInName,
            RemoveCloneFromName,
            Rename
        }

        [ShowIf( "GameObjectActionIsCreate" )]
        public GameObjectNamingOptions namingOptions = GameObjectNamingOptions.RemoveCloneFromName;
        private bool GameObjectNamingOptionsIsRename() { return GameObjectActionIsCreate() && namingOptions == GameObjectNamingOptions.Rename; }

        [ShowIf( "GameObjectNamingOptionsIsRename" )]
        public string GameObjectNewName;

        public enum GameObjectParentOptions
        {
            DoNotParentToObject,
            ParentToObject
        }

        [ShowIf( "GameObjectActionIsCreate" )]
        public GameObjectParentOptions parentOptions = GameObjectParentOptions.DoNotParentToObject;
        private bool GameObjectActionIsCreateAndParentToObject() { return GameObjectActionIsCreate() && parentOptions == GameObjectParentOptions.ParentToObject; }

        [ShowIf( "GameObjectActionIsCreateAndParentToObject" )]
        public GameObject GameObjectParent;


        //------------------ EVENTS ---------------------------//
        private bool ShowOnGameObjectEffectFinished() { return action == Actions.Create || action == Actions.Delete || action == Actions.Disable || action == Actions.Enable || action == Actions.FlipActiveState; }

        [Space( 15f ), ShowIf( "ShowOnGameObjectEffectFinished" ), FoldoutGroup( "Event Messages" )]
        public UnityEvent onActionCompleted;



        //---------------------------------------------------------//
        public override EventType GetEventType()
        //---------------------------------------------------------//
        {
            return EventType.GameObject;

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

            if     ( action == Actions.Create )             { eventReady = true; }
            else if( action == Actions.Delete )             { eventReady = true; }
            else if( action == Actions.Disable )            { eventReady = true; }
            else if( action == Actions.Enable )             { eventReady = true; }
            else if( action == Actions.FlipActiveState )    { eventReady = true; }

        } //END PrepareEvent

        //-------------------------------//
        protected override void _CallEvent()
        //-------------------------------//
        {
            base._CallEvent();

            if( eventReady )
            {
                if     ( action == Actions.Create )             { CallCreateEvent(); }
                else if( action == Actions.Delete )             { CallDeleteEvent(); }
                else if( action == Actions.Disable )            { CallDisableEvent(); }
                else if( action == Actions.Enable )             { CallEnableEvent(); }
                else if( action == Actions.FlipActiveState )    { CallFlipActiveStateEvent(); }
            }
            
        } //END CallEvent

        //---------------------------------//
        private void CallCreateEvent()
        //---------------------------------//
        {

            if( gameObjectToEffect != null )
            {
                GameObject clone = Instantiate( gameObjectToEffect );

                if( namingOptions == GameObjectNamingOptions.RemoveCloneFromName )
                {
                    clone.name = gameObjectToEffect.name;
                }
                else if( namingOptions == GameObjectNamingOptions.Rename )
                {
                    clone.name = GameObjectNewName;
                }

                if( parentOptions == GameObjectParentOptions.ParentToObject )
                {
                    clone.transform.parent = GameObjectParent.transform;
                }

                if( onActionCompleted != null ) { onActionCompleted.Invoke(); }
            }

        } //END CallCreateEvent

        //---------------------------------//
        private void CallDeleteEvent()
        //---------------------------------//
        {

            if( gameObjectToEffect != null )
            {
                if( ShouldWePerformLogicImmediately() )
                {
                    DeleteEvent();
                }
                else
                {
                    WaitForTweenBeforePerformingLogic( DeleteEvent );
                }
            }

        } //END CallDeleteEvent
        
        //---------------------------------//
        private void DeleteEvent()
        //---------------------------------//
        {

            Destroy( gameObjectToEffect );

            if( colliderOptions != GameObjectColliderOptions.DoNotChangeChildColliders )
            {
                ActionGameObjectEffectColliders();
            }

            if( onActionCompleted != null ) { onActionCompleted.Invoke(); }

        } //END DeleteEvent

        //---------------------------------//
        private void CallDisableEvent()
        //---------------------------------//
        {

            if( gameObjectToEffect != null )
            {
                if( ShouldWePerformLogicImmediately() )
                {
                    DisableEvent();
                }
                else
                {
                    WaitForTweenBeforePerformingLogic( DisableEvent );
                }
            }

        } //END CallDisableEvent

        //---------------------------------//
        private void DisableEvent()
        //---------------------------------//
        {

            gameObjectToEffect.SetActive( false );

            if( colliderOptions != GameObjectColliderOptions.DoNotChangeChildColliders )
            {
                ActionGameObjectEffectColliders();
            }

            if( onActionCompleted != null ) { onActionCompleted.Invoke(); }

        } //END DisableEvent

        //---------------------------------//
        private void CallEnableEvent()
        //---------------------------------//
        {

            if( gameObjectToEffect != null )
            {
                if( ShouldWePerformLogicImmediately() )
                {
                    EnableEvent();
                }
                else
                {
                    WaitForTweenBeforePerformingLogic( EnableEvent );
                }
            }

        } //END CallEnableEvent

        //---------------------------------//
        private void EnableEvent()
        //---------------------------------//
        {

            gameObjectToEffect.SetActive( true );

            if( colliderOptions != GameObjectColliderOptions.DoNotChangeChildColliders )
            {
                ActionGameObjectEffectColliders();
            }

            if( onActionCompleted != null ) { onActionCompleted.Invoke(); }

        } //END EnableEvent

        //---------------------------------//
        private void CallFlipActiveStateEvent()
        //---------------------------------//
        {

            if( gameObjectToEffect != null )
            {
                if( ShouldWePerformLogicImmediately() )
                {
                    FlipActiveState();
                }
                else
                {
                    WaitForTweenBeforePerformingLogic( FlipActiveState );
                }
            }

        } //END CallFlipActiveStateEvent

        //---------------------------------//
        private void FlipActiveState()
        //---------------------------------//
        {

            gameObjectToEffect.SetActive( !gameObjectToEffect.activeSelf );

            if( colliderOptions != GameObjectColliderOptions.DoNotChangeChildColliders )
            {
                ActionGameObjectEffectColliders();
            }

            if( onActionCompleted != null ) { onActionCompleted.Invoke(); }

        } //END FlipActiveState

        //---------------------------------//
        private bool ShouldWePerformLogicImmediately()
        //---------------------------------//
        {
            //Call logic immediately
            if( effectOptions == GameObjectEffectOptions.PerformActionImmediately )
            {
                return true;
            }

            //If we want to show/hide all child blocks but there are none, 
            //then just perform the logic for the action right away
            else if( ( effectOptions == GameObjectEffectOptions.HideAllChildrenBlocksFirst ||
                       effectOptions == GameObjectEffectOptions.ShowAllChildrenBlocksFirst ) &&
                       gameObjectToEffect.GetComponentsInChildren<BlockGroup>() == null )
            {
                return true;
            }

            else
            {
                return false;
            }
            

        } //END ShouldWePerformLogicImmediately

        //---------------------------------//
        private void WaitForTweenBeforePerformingLogic( Action performAction )
        //---------------------------------//
        {

            if( performAction != null )
            {
                UITweener.TweenValue tweenValue;

                if( effectOptions == GameObjectEffectOptions.HideAllChildrenBlocksFirst ) { tweenValue = UITweener.TweenValue.Hide; }
                else { tweenValue = UITweener.TweenValue.Show; }

                if( GameObjectOverwriteTweenValues() )
                {
                    SetGameObjectChildrenUITweenerValues( gameObjectToEffect, tweenValue, gameObjectTweenLength, gameObjectTweenDelay );
                }

                //Find the tween in all the blocks with the longest TweenLength, we'll call the next code after that
                float longestTween = GetLongestTweenInChildren( gameObjectToEffect, tweenValue );

                //Debug.Log( "longestTween = " + longestTween );

                foreach( BlockGroup blockGroup in gameObjectToEffect.GetComponentsInChildren<BlockGroup>() )
                {
                    if( tweenValue == UITweener.TweenValue.Hide ) { blockGroup.SendCommand( Block.CommandType.Hide ); }
                    else { blockGroup.SendCommand( Block.CommandType.Show ); }
                }

                Timer.instance.In( longestTween + .01f, performAction, gameObject );
            }
            
        } //END WaitForTweenBeforePerformingLogic
        
        //---------------------------------//
        private void SetGameObjectChildrenUITweenerValues( GameObject go, UITweener.TweenValue tweenValue, float tweenLength, float tweenDelay )
        //---------------------------------//
        {

            foreach( UITweener ui in go.GetComponentsInChildren<UITweener>() )
            {
                ui.SetTweenSpeed( tweenLength, tweenValue );
                ui.SetDelay( tweenDelay, tweenValue );
            }

            foreach( BlockAudio audio in go.GetComponentsInChildren<BlockAudio>() )
            {
                if( tweenValue == UITweener.TweenValue.Show )
                {
                    audio.fadeInLength = tweenLength;
                    audio.fadeInDelay = tweenDelay;
                }
                else
                {
                    audio.fadeOutLength = tweenLength;
                    audio.fadeOutDelay = tweenDelay;
                }
            }

        } //END SetGameObjectChildrenUITweenerValues

        //---------------------------------//
        private float GetLongestTweenInChildren( GameObject go, UITweener.TweenValue tweenValue )
        //---------------------------------//
        {

            float longestTween = 0f;

            foreach( UITweener ui in go.GetComponentsInChildren<UITweener>() )
            {
                float length = ui.GetTweenSpeed( tweenValue );

                if( longestTween < length )
                {
                    longestTween = length;
                }
            }

            foreach( BlockAudio audio in go.GetComponentsInChildren<BlockAudio>() )
            {
                float length = 0f;

                if( tweenValue == UITweener.TweenValue.Show ) { length = audio.fadeInLength; }
                else { length = audio.fadeOutLength; }

                if( longestTween < length )
                {
                    longestTween = length;
                }
            }

            return longestTween;

        } //END GetLongestTweenInChildren

        //---------------------------------//
        private void ActionGameObjectEffectColliders()
        //---------------------------------//
        {

            if( colliderType == GameObjectColliderType.Colliders )
            {
                ActionGameObjectEffectColliders_Colliders();
            }
            else if( colliderType == GameObjectColliderType.Buttons )
            {
                ActionGameObjectEffectColliders_Buttons();
            }
            else if( colliderType == GameObjectColliderType.Images )
            {
                ActionGameObjectEffectColliders_Images();
            }
            else if( colliderType == GameObjectColliderType.UIColliderManager )
            {
                ActionGameObjectEffectColliders_UIColliderManager();
            }
            else if( colliderType == GameObjectColliderType.AllTypes )
            {
                ActionGameObjectEffectColliders_Colliders();
                ActionGameObjectEffectColliders_Buttons();
                ActionGameObjectEffectColliders_Images();
                ActionGameObjectEffectColliders_UIColliderManager();
            }

        } //END ActionGameObjectEffectColliders

        //---------------------------------//
        private void ActionGameObjectEffectColliders_Colliders()
        //---------------------------------//
        {

            foreach( Collider t in gameObjectToEffect.GetComponentsInChildren<Collider>() )
            {
                if( colliderOptions == GameObjectColliderOptions.EnableColliders )
                {
                    t.enabled = true;
                }
                else if( colliderOptions == GameObjectColliderOptions.DisableColliders )
                {
                    t.enabled = false;
                }
                if( colliderOptions == GameObjectColliderOptions.FlipActiveState )
                {
                    t.enabled = !t.enabled;
                }
            }

        } //END ActionGameObjectEffectColliders_Colliders

        //---------------------------------//
        private void ActionGameObjectEffectColliders_Buttons()
        //---------------------------------//
        {

            foreach( Button t in gameObjectToEffect.GetComponentsInChildren<Button>() )
            {
                if( colliderOptions == GameObjectColliderOptions.EnableColliders )
                {
                    t.enabled = true;
                }
                else if( colliderOptions == GameObjectColliderOptions.DisableColliders )
                {
                    t.enabled = false;
                }
                if( colliderOptions == GameObjectColliderOptions.FlipActiveState )
                {
                    t.enabled = !t.enabled;
                }
            }

        } //END ActionGameObjectEffectColliders_Buttons

        //---------------------------------//
        private void ActionGameObjectEffectColliders_Images()
        //---------------------------------//
        {

            foreach( Image t in gameObjectToEffect.GetComponentsInChildren<Image>() )
            {
                if( colliderOptions == GameObjectColliderOptions.EnableColliders )
                {
                    t.enabled = true;
                }
                else if( colliderOptions == GameObjectColliderOptions.DisableColliders )
                {
                    t.enabled = false;
                }
                if( colliderOptions == GameObjectColliderOptions.FlipActiveState )
                {
                    t.enabled = !t.enabled;
                }
            }

        } //END ActionGameObjectEffectColliders_Images

        //---------------------------------//
        private void ActionGameObjectEffectColliders_UIColliderManager()
        //---------------------------------//
        {

            foreach( UIColliderManager t in gameObjectToEffect.GetComponentsInChildren<UIColliderManager>() )
            {
                if( colliderOptions == GameObjectColliderOptions.EnableColliders )
                {
                    t.SetColliders( true );
                }
                else if( colliderOptions == GameObjectColliderOptions.DisableColliders )
                {
                    t.SetColliders( false );
                }
                if( colliderOptions == GameObjectColliderOptions.FlipActiveState )
                {
                    t.FlipColliders();
                }
            }

        } //END ActionGameObjectEffectColliders_UIColliderManager


    } //END BlockEventGameObject

} //END Namespace