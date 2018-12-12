using Sirenix.OdinInspector;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace BrandXR
{
    public class BlockEventTexture: BlockEventBase
    {

        //----------------- ACTIONS --------------------------------//
        public enum Actions
        {
            None,
            ChangeTexture
        }

        [TitleGroup( "Block Event - Texture", "Used To Modify A 2D Texture" )]
        public Actions action = Actions.ChangeTexture;
        private bool ShowChangeTextureAction() { return action == Actions.ChangeTexture; }

        //------------- VARIABLES ---------------------------------//
        public enum SetNewTextureOn
        {
            MeshRenderer,
            Material,
            RawImage
        }

        [Space( 15f ), ShowIf( "ShowChangeTextureAction" ), FoldoutGroup( "Change Texture Settings" )]
        public SetNewTextureOn setNewTextureOn = SetNewTextureOn.MeshRenderer;
        private bool IsSetNewTextureOnMeshRenderer() { return ShowChangeTextureAction() && setNewTextureOn == SetNewTextureOn.MeshRenderer; }
        private bool IsSetNewTextureOnMaterial() { return ShowChangeTextureAction() && setNewTextureOn == SetNewTextureOn.Material; }
        private bool IsSetNewTextureOnRawImage() { return ShowChangeTextureAction() && setNewTextureOn == SetNewTextureOn.RawImage; }

        [ShowIf( "IsSetNewTextureOnMeshRenderer" ), FoldoutGroup( "Change Texture Settings" )]
        public MeshRenderer meshRendererToChangeTexture;

        [ShowIf( "IsSetNewTextureOnMaterial" ), FoldoutGroup( "Change Texture Settings" )]
        public Material materialToChangeTexture;

        [ShowIf( "IsSetNewTextureOnRawImage" ), FoldoutGroup( "Change Texture Settings" )]
        public RawImage rawImageToChangeTexture;

        public enum ChangeTextureUsing
        {
            Texture,
            Path
        }

        [ShowIf( "ShowChangeTextureAction" ), FoldoutGroup( "Change Texture Settings" )]
        public ChangeTextureUsing changeTextureUsing = ChangeTextureUsing.Texture;
        private bool IsChangeTextureUsingTexture() { return ShowChangeTextureAction() && changeTextureUsing == ChangeTextureUsing.Texture; }
        private bool IsChangeTextureUsingPath() { return ShowChangeTextureAction() && changeTextureUsing == ChangeTextureUsing.Path; }

        [ShowIf( "IsChangeTextureUsingTexture" ), FoldoutGroup( "Change Texture Settings" )]
        public Texture changeToTexture;

        [ShowIf( "IsChangeTextureUsingPath" ), FoldoutGroup( "Change Texture Settings" )]
        public string texturePath;

        [ShowIf( "IsChangeTextureUsingPath" ), FoldoutGroup( "Change Texture Settings" )]
        public bool cacheTextureIfLoadedFromWeb = true;

        //-------------------- "CHANGE TEXTURE" EVENT MESSAGES ---------------------//
        private bool ShowChangeTextureEventMessages() { return ShowChangeTextureAction(); }

        [ShowIf( "IsChangeTextureUsingPath" ), FoldoutGroup( "Event Messages" )]
        public UnityEvent LoadTextureFromPathSuccess = new UnityEvent();

        [ShowIf( "IsChangeTextureUsingPath" ), FoldoutGroup( "Event Messages" )]
        public UnityEvent LoadTextureFromPathFailed = new UnityEvent();

        [ShowIf( "ShowChangeTextureEventMessages" ), FoldoutGroup( "Event Messages" )]
        public UnityEvent onChangeTexture = new UnityEvent();


        //---------------------------------------------------------//
        public override EventType GetEventType()
        //---------------------------------------------------------//
        {
            return EventType.Texture;

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
            if( action == Actions.ChangeTexture )
            {
                if( changeTextureUsing == ChangeTextureUsing.Texture )
                {
                    //Do nothing, texture is already set
                    if( changeToTexture != null )
                    {
                        eventReady = true;
                    }
                    else
                    {
                        eventReady = false;
                    }
                }
                else if( changeTextureUsing == ChangeTextureUsing.Path )
                {
                    WWWHelper.instance.GetTexture( texturePath, cacheTextureIfLoadedFromWeb, OnGetTextureSuccess, OnGetTextureFailed );
                }
            }
            
        } //END PrepareEvent

        //---------------------------------//
        public void OnGetTextureSuccess( Texture texture )
        //---------------------------------//
        {
            changeToTexture = texture;
            eventReady = true;

            if( LoadTextureFromPathSuccess != null ) { LoadTextureFromPathSuccess.Invoke(); }

        } //END onGetTextureSuccess

        //---------------------------------//
        public void OnGetTextureFailed()
        //---------------------------------//
        {
            changeToTexture = null;
            eventReady = false;

            if( LoadTextureFromPathFailed != null ) { LoadTextureFromPathFailed.Invoke(); }

        } //END onGetTextureFailed



        //-------------------------------//
        protected override void _CallEvent()
        //-------------------------------//
        {
            base._CallEvent();

            if( eventReady )
            {
                if( action == Actions.ChangeTexture )
                {
                    CallChangeTextureAction();
                }
            }
            
        } //END CallEvent

        //----------------------------------//
        private void CallChangeTextureAction()
        //----------------------------------//
        {

            if( changeToTexture != null )
            {
                if( setNewTextureOn == SetNewTextureOn.MeshRenderer && meshRendererToChangeTexture != null )
                {
                    if( meshRendererToChangeTexture.sharedMaterial != null )
                    {
                        if( showDebug ) Debug.Log( "BlockEventTexture.cs CallEvent() sharedMaterial != null, setting to new texture" );
                        meshRendererToChangeTexture.sharedMaterial.mainTexture = changeToTexture;
                        if( onChangeTexture != null ) { onChangeTexture.Invoke(); }
                    }
                    else
                    {
                        if( showDebug ) Debug.Log( "BlockEventTexture.cs CallEvent() sharedMaterial == null, creating unlit material and assigning, setting to new texture" );
                        meshRendererToChangeTexture.material = new Material( Shader.Find( "Unlit/Texture" ) );
                        meshRendererToChangeTexture.sharedMaterial.mainTexture = changeToTexture;
                        if( onChangeTexture != null ) { onChangeTexture.Invoke(); }
                    }
                }
                else if( setNewTextureOn == SetNewTextureOn.Material && materialToChangeTexture != null )
                {
                    materialToChangeTexture.mainTexture = changeToTexture;
                    if( onChangeTexture != null ) { onChangeTexture.Invoke(); }
                }
                else if( setNewTextureOn == SetNewTextureOn.RawImage && rawImageToChangeTexture != null )
                {
                    rawImageToChangeTexture.texture = changeToTexture;
                    if( onChangeTexture != null ) { onChangeTexture.Invoke(); }
                }
            }

        } //END CallChangeTextureAction

    } //END BlockEventTexture

} //END Namespace