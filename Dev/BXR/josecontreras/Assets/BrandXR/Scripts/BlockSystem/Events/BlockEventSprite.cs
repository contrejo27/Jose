using Sirenix.OdinInspector;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace BrandXR
{
    public class BlockEventSprite: BlockEventBase
    {
        //----------------- ACTIONS --------------------------------//
        public enum Actions
        {
            None,
            ChangeSprite
        }

        [TitleGroup( "Block Event - Sprite", "Used to modify a 2D Sprite" )]
        public Actions action = Actions.ChangeSprite;
        private bool ShowChangeSpriteAction() { return action == Actions.ChangeSprite; }

        //------------- "CHANGE SPRITE" VARIABLES ---------------------------------//
        public enum SetNewSpriteOn
        {
            Image,
            SpriteRenderer
        }

        [ShowIf( "ShowChangeSpriteAction" ), FoldoutGroup( "Sprite Settings" )]
        public SetNewSpriteOn setNewSpriteOn = SetNewSpriteOn.Image;
        private bool IsSetNewTextureOnImage() { return ShowChangeSpriteAction() && setNewSpriteOn == SetNewSpriteOn.Image; }
        private bool IsSetNewTextureOnSpriteRenderer() { return ShowChangeSpriteAction() && setNewSpriteOn == SetNewSpriteOn.SpriteRenderer; }

        [ShowIf( "IsSetNewTextureOnImage" ), FoldoutGroup( "Sprite Settings" )]
        public Image changeThisImage;

        [ShowIf( "IsSetNewTextureOnSpriteRenderer" ), FoldoutGroup( "Sprite Settings" )]
        public SpriteRenderer changeThisSpriteRenderer;

        [ShowIf( "ShowChangeSpriteAction" ), FoldoutGroup( "Sprite Settings" )]
        public Sprite changeToSprite;


        //-------------------- "CHANGE SPRITE" EVENT MESSAGES ---------------------//
        private bool ShowChangeSpriteEventMessages() { return action == Actions.ChangeSprite; }

        [ShowIf( "ShowChangeSpriteEventMessages" ), FoldoutGroup( "Event Messages" )]
        public UnityEvent onActionCompleted = new UnityEvent();


        //---------------------------------------------------------//
        public override EventType GetEventType()
        //---------------------------------------------------------//
        {
            return EventType.Sprite;

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

            if( action == Actions.ChangeSprite )
            {
                if( changeToSprite != null )
                {
                    eventReady = true;
                }
                else
                {
                    eventReady = false;
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
                if( action == Actions.ChangeSprite )
                {
                    CallChangeSpriteEvent();
                }
            }
            
        } //END CallEvent

        //---------------------------//
        private void CallChangeSpriteEvent()
        //---------------------------//
        {

            if( changeToSprite != null )
            {
                if( setNewSpriteOn == SetNewSpriteOn.Image && changeThisImage != null )
                {
                    changeThisImage.sprite = changeToSprite;
                    if( onActionCompleted != null ) { onActionCompleted.Invoke(); }
                }
                else if( setNewSpriteOn == SetNewSpriteOn.SpriteRenderer && changeThisSpriteRenderer != null )
                {
                    changeThisSpriteRenderer.sprite = changeToSprite;
                    if( onActionCompleted != null ) { onActionCompleted.Invoke(); }
                }
            }

        } //END CallChangeSpriteEvent

    } //END BlockEventSprite

} //END Namespace