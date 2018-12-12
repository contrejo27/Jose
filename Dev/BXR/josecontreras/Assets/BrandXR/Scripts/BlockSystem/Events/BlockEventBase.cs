using Sirenix.OdinInspector;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace BrandXR
{
    //Base component for all BlockEvents, contains stubs for methods no actual logic itself
    public class BlockEventBase: MonoBehaviour
    {
        public enum EventType
        {
            Block,
            BlockGroup,
            BlockView,
            BlockManager,
            Unity,
            PlayerPrefs,
            Scene,
            GameObject,
            Transform,
            Canvas,
            Dropdown,
            Input,
            Texture,
            Sprite,
            Color,
            Audio,
            Video,
            XRSkybox,
            Tween,
            Web,
            Recorder,
            ExternalCamera,
            NativeSharing,
            XRTargetFloor,
            BlockModel,
            XRCameraManager,
            AssetBundle,
            Debug,
            Microphone,
            Vuforia,
            NativeDevices
        }
        
        protected bool eventReady = false;

        protected bool showDebug = false;

        private bool prepareEventCalled = false;

        [Tooltip( "Should the event be called when Start() is called?" )]
        public bool callEventOnStart = false;

        [Range( .1f, 999f ), Tooltip( "[milliseconds] How long should we wait before calling the event after Start()?" ), ShowIf( "callEventOnStart", true )]
        public float delay = .1f;

        [Tooltip("Should this event be delayed when CallEvent() is called?")]
        public bool delayEventWhenCalled = false;

        [Range( .1f, 999f ), Tooltip( "[milliseconds] How long should we wait before calling the event when it recieves a CallEvent() command?" ), ShowIf( "delayEventWhenCalled", true )]
        public float delayEvent = .1f;

        [Tooltip("Enable this to allow this event to be called even when the GameObject is disabled.")]
        public bool allowWhenDisabled = false;

        private Coroutine delayTimer = null;

        //-------------------------------------------------------------------//
        //------------------------ GAMEOBJECT HOOKS -------------------------//
        //-------------------------------------------------------------------//
        [FoldoutGroup( "Hooks" ), InfoBox( "Do Not Modify", InfoMessageType.Warning )]
        public GameObject _PrefabManager;

        //---------------------------------//
        protected virtual void Start()
        //---------------------------------//
        {
            //Make sure we have an active reference to the PrefabManager in our scene
            AddPrefabManagerIfNeeded();

            //Make sure this BlockEventBase has a BlockEvent parent
            AddBlockEventIfNeeded();

            //Check if we should start the event
            if( callEventOnStart )
            {
                if( transform.parent != null && transform.parent.GetComponent<BlockEvent>() != null )
                {
                    transform.parent.GetComponent<BlockEvent>().AddPrefabManagerIfNeeded();

                    if( !prepareEventCalled )
                    {
                        PrepareEvent();
                    }

                    if( delay == 0f ) { _CallEvent(); }
                    else { delayTimer = Timer.instance.In( delay, _CallEvent, gameObject ); }
                }
            }

        } //END Start

        //---------------------------------//
        protected void AddPrefabManagerIfNeeded()
        //---------------------------------//
        {
            //If the _prefabManager hook does not exist, grab it from the Asset database when in editor
            if( _PrefabManager == null )
            {
#if UNITY_EDITOR
                Debug.LogError( "BlockEventBase.cs AddPrefabManagerIfNeeded() Unable to create PrefabManager as the _PrefabManager hook on this component is null, using AssetDatabase lookup as an Editor only backup" );
                _PrefabManager = UnityEditor.AssetDatabase.LoadAssetAtPath<GameObject>( "Assets/BrandXR/Prefabs/bxr_PrefabManager.prefab" );
#else
                Debug.LogError( "BlockEventBase.cs AddPrefabManagerIfNeeded() Unable to create PrefabManager as the _PrefabManager hook on this component is null" );
#endif
            }

            //If the PrefabManager does not exist, create it and parent it to the "BrandXR Tech" GameObject
            if( GameObject.FindObjectOfType<PrefabManager>() == null )
            {
                GameObject go = GameObject.Instantiate( _PrefabManager );
                go.name = "bxr_PrefabManager";

                if( GameObject.Find( "BrandXR Tech" ) == null ) { new GameObject( "BrandXR Tech" ); }

                GameObject bxr = GameObject.Find( "BrandXR Tech" );
                go.transform.parent = bxr.transform;
            }

        } //END AddPrefabManagerIfNeeded

        //---------------------------------//
        protected void AddBlockEventIfNeeded()
        //---------------------------------//
        {

            //If this BlockEventBase does not have a parent with a BlockEvent component, make sure to add it!
            if( this.transform.parent == null ||
                ( this.transform.parent != null && this.transform.parent.GetComponent<BlockEvent>() == null ) )
            {
                BlockEvent blockEvent = null;

                //if we have a parent with no Block Components, make it into a BlockEvent
                if( this.transform.parent != null &&
                    this.transform.parent.GetComponent<BlockManager>() == null &&
                    this.transform.parent.GetComponent<BlockView>() == null &&
                    this.transform.parent.GetComponent<BlockGroup>() == null &&
                    this.transform.parent.GetComponent<Block>() )
                {
                    this.transform.parent.gameObject.AddComponent<BlockEvent>();

                    //Make sure the new blockEvent has a reference to the PrefabManager
                    blockEvent._PrefabManager = _PrefabManager;

                    //Rename the object if needed
                    if( this.transform.parent.name == "GameObject" )
                    {
                        this.transform.parent.name = "bxr_BlockEvent";
                    }
                }

                //If we have a parent, but it already is part of the Block system
                else if( this.transform.parent != null &&
                         ( this.transform.parent.GetComponent<BlockManager>() != null ||
                           this.transform.parent.GetComponent<BlockView>() != null ||
                           this.transform.parent.GetComponent<BlockGroup>() != null ||
                           this.transform.parent.GetComponent<Block>() != null ) )
                {
                    //Keep track of the original parent
                    Transform originalParent = this.transform.parent;

                    //Create a new BlockEvent and then parent this BlockEventBase to it
                    blockEvent = PrefabManager.InstantiatePrefab( PrefabFactory.Prefabs.bxr_BlockEvent ).GetComponent<BlockEvent>();
                    this.transform.parent = blockEvent.transform;

                    //Make sure the new blockEvent has a reference to the PrefabManager
                    blockEvent._PrefabManager = _PrefabManager;

                    //Then parent the blockEvent parent to the original parent
                    blockEvent.transform.parent = originalParent;
                }

                //If we don't have a parent, or we do but it cannot become a BlockEvent
                else
                {
                    //Create a new BlockEvent and parent this BlockEventBase to it
                    blockEvent = PrefabManager.InstantiatePrefab( PrefabFactory.Prefabs.bxr_BlockEvent ).GetComponent<BlockEvent>();
                    this.transform.parent = blockEvent.transform;

                    //Make sure the new blockEvent has a reference to the PrefabManager
                    blockEvent._PrefabManager = _PrefabManager;
                }

            }

        } //END AddBlockEventIfNeeded



        //-----------------------------------------------------//
        public virtual EventType GetEventType()
        //-----------------------------------------------------//
        {
            return EventType.Unity;

        } //END GetEventType

        //-------------------------------//
        public virtual void PrepareEvent()
        //-------------------------------//
        {

            //Called at the Start() of BlockEvent
            if( prepareEventCalled ) { return; }

        } //END PrepareEvent



        //-------------------------------//
        public virtual void OnDisable( )
        //-------------------------------//
        {
            //When this gameObject is disabled, cancel any timer it has running
            if( delayTimer != null )
            {
                Timer.instance.Cancel( delayTimer );
                delayTimer = null;
            }

        } //END OnDisable


        //-------------------------------//
        public void CallEvent( float delay )
        //-------------------------------//
        {
            //If we are currently waiting for a Coroutine to finish, then do not allow another event
            if( delayTimer != null ) { return; }

            if( delay == 0f ) { _CallEvent(); }
            else { delayTimer = Timer.instance.In( delay, _CallEvent, gameObject ); }

        } //END CallEvent

        //-------------------------------//
        public void CallEvent()
        //-------------------------------//
        {
            //If we are currently waiting for a Coroutine to finish, then do not allow another event
            if( delayTimer != null ) { return; }

            if( delayEventWhenCalled )
            {
                if( delayEvent == 0f ) { _CallEvent(); }
                else { delayTimer = Timer.instance.In( delayEvent, _CallEvent, gameObject ); }
            }
            else
            {
                _CallEvent();
            }

        } //END CallEvent

        //-------------------------------//
        protected virtual void _CallEvent()
        //-------------------------------//
        {
            //Make sure out timer coroutine is set to null,
            //Since if there was a timer, it is now finished
            delayTimer = null;
            
            //If we don't allow this Event to be called 
            //when this gameObject is disabled, and it is disabled,
            //then don't perform the CallEvent() logic
            if( !allowWhenDisabled && !gameObject.activeInHierarchy )
            {
                return;
            }


        } //END _CallEvent


        //--------------------------------//
        protected virtual void PrepareForDestroy()
        //--------------------------------//
        {

            //Stops any running tweens or timers associated with this component

        } //END PrepareForDestroy


        [Button("Remove Event", ButtonSizes.Large), FoldoutGroup("Remove Event"), InfoBox("Press This to Remove this event from the Block Event parent.\n\nWARNING: You cannot undo this!", InfoMessageType.Warning)]
        //-------------------------------//
        /// <summary>
        /// Shows a Remove Event button in the editor, destroys this component and GameObject and removes it from the BlockEvent parent component's list of events
        /// </summary>
        protected void RemoveEvent()
        //-------------------------------//
        {
            PrepareForDestroy();

            if( transform.parent != null && transform.parent.GetComponent<BlockEvent>() != null )
            {
                transform.parent.GetComponent<BlockEvent>().RemoveEvent( this );
            }
            
        } //END RemoveEvent

    } //END BlockEvent

} //END Namespace