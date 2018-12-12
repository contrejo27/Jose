/* BlockEvent.cs
 * 
 * Allows you to customize logic to be sent throughout the BrandXR block system when the Call() method is called.
 * You can have an unlimited number of these Events, and they will all be called in order when the Call() method is called
 * 
 * You can check out the hierarchy in editor, when you add a new Event via this script/prefab, it will add a new GameObject with 
 * a BlockEventBase component attached to it, that has it's own customization options
 */
using UnityEngine;
using System;
using Sirenix.OdinInspector;
using System.Collections.Generic;
using System.Linq;

namespace BrandXR
{
    public class BlockEvent: Block
    {
        [TitleGroup( "BlockEvent", "Sends logic to Blocks, Unity, or other BrandXR systems" )]
        public int dummy = 0;

        protected override bool ShowFaceCameraEvent() { return false; }


        public override BlockType GetBlockType() { return BlockType.Event; }

        //---------------------------------------------//
        public class BlockEventHelper
        //---------------------------------------------//
        {
            public BlockEventBase.EventType eventType;
            private BlockEventBase blockEventBase = null;

            //-------------------------------------------------------------//
            public BlockEventHelper( BlockEventBase.EventType eventType )
            //-------------------------------------------------------------//
            {
                this.eventType = eventType;

            } //END Constructor

            //-------------------------------------------------------------//
            public void SetBlockEventBase( BlockEventBase blockEventBase )
            //-------------------------------------------------------------//
            {
                this.blockEventBase = blockEventBase;

            } //END SetBlockEventBase

            //-------------------------------------------------------------//
            public BlockEventBase GetBlockEventBase()
            //-------------------------------------------------------------//
            {
                if( blockEventBase != null ) { return blockEventBase; }
                else { return null; }

            } //END GetBlockEventBase

        } //END BlockEventHelper class

        

        private List<BlockEventHelper> blockEventHelpers = new List<BlockEventHelper>();
        

        [FoldoutGroup("Add Event"), InfoBox( "Events are actions that can impact Blocks, BrandXR, or Unity systems.\n\nYou can have an unlimited number of BlockEventBase prefabs attached to this prefab.\n\nYou can call the logic to fire for all of the attached BlockEventBase prefabs by sending this BlockEvent the Call() command.\n\nAlternatively you can send the BlockEventBase scripts that are childed to this BlockEvent a Call() command to have it perform its logic by itself\n\nChoose the type of Event you would like to add, and then press the 'Add Event' button to create a child with the event options.\n\nDon't forget to edit the child event component that will be created!" )]
        public BlockEventBase.EventType eventType;

        [Button( "Add Event", ButtonSizes.Large ), FoldoutGroup("Add Event")]
        public void AddEvent() { _AddEvent( eventType ); }


        //---------------------------------//
        private void _AddEvent( BlockEventBase.EventType eventType )
        //---------------------------------//
        {
            GameObject go = new GameObject( eventType.ToString() + " Event" );
            go.transform.parent = transform;

            string newComponentName = "BrandXR.BlockEvent" + eventType.ToString();
            Type componentType = ComponentHelper.FindType( newComponentName );
            
            BlockEventHelper helper = new BlockEventHelper( eventType );
            helper.SetBlockEventBase( (BlockEventBase)go.AddComponent( componentType ) );
            helper.GetBlockEventBase()._PrefabManager = _PrefabManager;

            blockEventHelpers.Add( helper );

            if( showDebug )
            {
                Debug.Log( "BlockEvent.cs _AddEvent() Added new Helper(" + helper.GetBlockEventBase().name + ")! blockEventHelpers.Count = " + blockEventHelpers.Count );
            }

        } //END AddEvent

        //--------------------------------//
        public void RemoveEvent( BlockEventBase blockEventBase )
        //--------------------------------//
        {

            if( showDebug ) { Debug.Log( "BlockEvent.cs RemoveEvent() start, blockEventHelpers.Count = " + blockEventHelpers.Count ); }

            if( blockEventHelpers != null && blockEventHelpers.Count > 0 )
            {
                for( int i = blockEventHelpers.Count - 1; i >= 0; i-- )
                {
                    if( blockEventHelpers[i].GetBlockEventBase() == blockEventBase )
                    {
                        if( showDebug ) { Debug.Log( "BlockEvent.cs RemoveEvent() found matching blockEventBase, removing from the list of blockEventHelpers[" + i + "]" ); }

                        blockEventHelpers.RemoveAt( i );
                        
                        if( showDebug )
                        {
                            if( blockEventBase != null )
                            {
                                Debug.Log( "BlockEvent.cs RemoveEvent() removed from list, the passed in blockEventBase object is not null!" );
                            }
                            else
                            {
                                Debug.Log( "BlockEvent.cs RemoveEvent() removed from list, the passed in blockEventBase object is null..." );
                            }
                        }

                        #if UNITY_EDITOR
                            //Wait a moment before calling DestroyImmediate to make sure no logic is running
                            UnityEditor.EditorApplication.delayCall+=()=>
                            {
                                //If we're coming from a Tween Event, we might have a Tween component attached that should be removed
                                if( blockEventBase.GetComponent<bxrTween>() != null )
                                {
                                    DestroyImmediate( blockEventBase.GetComponent<bxrTween>() );
                                }

                                if( showDebug ) { Debug.Log( "Calling Destroy for blockEventBase = " + gameObject.name ); }
                                DestroyImmediate( blockEventBase.gameObject );
                            };
                        #else
                            //If we're coming from a Tween Event, we might have a Tween component attached that should be removed
                            if( blockEventBase.GetComponent<bxrTween>() != null )
                            {
                                Destroy( blockEventBase.GetComponent<bxrTween>() );
                            }

                            Destroy( blockEventBase.gameObject );
                        #endif

                        break;
                    }
                    else
                    {
                        if( showDebug ) { Debug.Log( "BlockEvent.cs RemoveEvent() blockEventHelpers[" + i + "].blockEventBase( " + blockEventBase.gameObject.name + " ) != blockEventBase(" + blockEventBase.gameObject.name + ")!" ); }
                    }
                }
            }
            else if( blockEventBase != null )
            {
                if( showDebug ) { Debug.Log( "BlockEvent.cs RemoveEvent() blockEventHelpers list is empty!" ); }

                //If the list is empty, we definitely should remove this stray BlockEventBase gameObject
                #if UNITY_EDITOR
                    //Wait a moment before calling DestroyImmediate to make sure no logic is running
                    UnityEditor.EditorApplication.delayCall+=()=>
                    {
                        //If we're coming from a Tween Event, we might have a Tween component attached that should be removed
                        if( blockEventBase.GetComponent<bxrTween>() != null )
                        {
                            DestroyImmediate( blockEventBase.GetComponent<bxrTween>() );
                        }

                        if( showDebug ) { Debug.Log( "Calling Destroy for blockEventBase = " + gameObject.name ); }
                        DestroyImmediate( blockEventBase.gameObject );
                    };
                #else
                    //If we're coming from a Tween Event, we might have a Tween component attached that should be removed
                    if( blockEventBase.GetComponent<bxrTween>() != null )
                    {
                        Destroy( blockEventBase.GetComponent<bxrTween>() );
                    }

                    Destroy( blockEventBase.gameObject );
                #endif
            }

        } //END RemoveEvent



        //---------------------------------//
        public override void Start()
        //---------------------------------//
        {
            base.Start();

            SetBlockEvent();

            PrepareEvent();
            
        } //END Start

        //----------------------------------//
        private void SetBlockEvent()
        //----------------------------------//
        {
            //If our events list is empty, yet there are BlockEvent as children GameObjects, add them to our list
            if( blockEventHelpers == null || ( blockEventHelpers != null && blockEventHelpers.Count() == 0 ) )
            {
                if( gameObject.GetComponentsInChildren<BlockEventBase>() != null )
                {
                    List<BlockEventBase> foundEventBases = gameObject.GetComponentsInChildren<BlockEventBase>().ToList();

                    foreach( BlockEventBase blockEventBase in foundEventBases )
                    {
                        BlockEventHelper helper = new BlockEventHelper( blockEventBase.GetEventType() );
                        helper.SetBlockEventBase( blockEventBase );

                        blockEventHelpers.Add( helper );
                    }
                }
            }

        } //END SetBlockEvent

        //----------------------------------//
        private void PrepareEvent()
        //----------------------------------//
        {

            if( blockEventHelpers != null && blockEventHelpers.Count > 0 )
            {
                foreach( BlockEventHelper blockEventHelper in blockEventHelpers )
                {
                    if( blockEventHelper != null )
                    {
                        blockEventHelper.GetBlockEventBase().PrepareEvent();
                    }
                }
            }

        } //END PrepareEvent

        

        //---------------------------------//
        public override void Call()
        //---------------------------------//
        {

            if( blockEventHelpers != null && blockEventHelpers.Count > 0 )
            {
                foreach( BlockEventHelper blockEventHelper in blockEventHelpers )
                {
                    if( blockEventHelper != null )
                    {
                        blockEventHelper.GetBlockEventBase().CallEvent();
                    }
                }
            }

        } //END Call
        

        //--------------------------------//
        public override void PrepareForDestroy()
        //--------------------------------//
        {

            //Stops any running tweens or timers associated with this block


        } //END PrepareForDestroy

        //---------------------------------//
        public override void SetValues( string jsonData )
        //---------------------------------//
        {

            
            
        } //END SetValues

        //-------------------------------//
        public override void OnDisable()
        //-------------------------------//
        {
            //When a BlockEvent is disabled, go through all the BlockEventBase components and let them know
            if( blockEventHelpers != null && blockEventHelpers.Count > 0 )
            {
                foreach( BlockEventHelper blockEventHelper in blockEventHelpers )
                {
                    if( blockEventHelper != null )
                    {
                        blockEventHelper.GetBlockEventBase().OnDisable();
                    }
                }
            }

        } //END OnDisable


        //--------------------------------//
        public void TestMessage( string message )
        //--------------------------------//
        {

            Debug.Log( message );

        } //END TestMessage


    } //END Class

} //END Namespace