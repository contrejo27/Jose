using Sirenix.OdinInspector;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Events;

namespace BrandXR
{
    public class BlockEventBlockModel: BlockEventBase
    {
        //----------------- ACTIONS --------------------------------//
        public enum Actions
        {
            None,
            ChangeModel
        }

        [TitleGroup( "Block Event - BlockModel", "Used to send a specialized command to a BlockModel" )]
        public Actions action = Actions.ChangeModel;
        private bool IsActionChangeModel() { return action == Actions.ChangeModel; }

        //------------- COMMON VARIABLES ---------------------------------//
        [Space( 15f ), InfoBox( "The BlockModel that perform the action" ), ShowIf( "IsActionChangeModel" )]
        public BlockModel blockModel = null;

        //------------- CHANGE MODEL VARIABLES ---------------------------------//
        [Space( 15f ), InfoBox("Instantiates a Clone of this GameObject"), ShowIf( "IsActionChangeModel" )]
        public GameObject changeToModel = null;
        public void SetNewModelToChangeTo( GameObject gameObjectToClone )
        {
            if( gameObjectToClone != null )
            {
                changeToModel = gameObjectToClone;
            }
        }

        [Space( 15f ), ShowIf( "IsActionChangeModel" ), InfoBox( "Enable these options to keep the new model using the same values as the existing model" )]
        public bool useSamePosition = true;

        [ShowIf( "IsActionChangeModel" )]
        public bool useSameRotation = true;

        [ShowIf( "IsActionChangeModel" )]
        public bool useSameScale = true;

        //-------------------- EVENT MESSAGES ---------------------//
        private bool ShowOnActionCompletedEvent() { return action != Actions.None; }

        [Space( 15f ), ShowIf( "ShowOnActionCompletedEvent" ), FoldoutGroup( "Event Messages" )]
        public UnityEvent onActionCompleted;


        //---------------------------------------------------------//
        public override EventType GetEventType()
        //---------------------------------------------------------//
        {
            return EventType.BlockModel;

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
            if( blockModel != null )
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
                if( action == Actions.ChangeModel &&
                    changeToModel != null )
                {
                    CallChangeModelEvent();
                }
            }

        } //END CallEvent

        //---------------------------------//
        private void CallChangeModelEvent()
        //---------------------------------//
        {
            //Change over the model
            if( blockModel != null && changeToModel != null )
            {
                //If the blockModel does not have an existing model
                if( blockModel.model != null )
                {
                    Vector3 pos = blockModel.model.transform.localPosition;
                    Quaternion rot = blockModel.model.transform.rotation;
                    Vector3 scale = blockModel.model.transform.localScale;

                    if( useSamePosition )
                    {
                        blockModel.model.transform.localPosition = pos;
                    }

                    if( useSameRotation )
                    {
                        blockModel.model.transform.rotation = rot;
                    }

                    if( useSameScale )
                    {
                        blockModel.model.transform.localScale = scale;
                    }
                }

                blockModel.ChangeModel( changeToModel );
                
                //Create a list of all of the BlockEventXRTargetFloor and BlockEventTransform components that are using the existing model
                List<BlockEventXRTargetFloor> floorEvents = GameObject.FindObjectsOfType<BlockEventXRTargetFloor>().ToList();
                List<BlockEventTransform> transformEvents = GameObject.FindObjectsOfType<BlockEventTransform>().ToList();

                //Setup those BlockEvents for the new BlockModel
                if( floorEvents != null && floorEvents.Count > 0 )
                {
                    foreach( BlockEventXRTargetFloor floorEvent in floorEvents )
                    {
                        if( floorEvent != null )
                        {
                            if( floorEvent.action == BlockEventXRTargetFloor.Action.EnableSetTransformToFloor &&
                                floorEvent.transformType == BlockEventXRTargetFloor.TransformType.BlockModel &&
                                floorEvent.blockModelToMoveOnFloor == blockModel )
                            {
                                //Debug.Log( "BlockEventBlockModel.cs CallChangeModelEvent() calling floorEvent.CallEvent()" );
                                floorEvent.PrepareEvent();
                                floorEvent.CallEvent();
                            }
                            else
                            {
                                //Debug.Log( "BlockEventBlockModel.cs CallChangeModelEvent() action = " + floorEvent.action + ", transformType = " + floorEvent.transformType + ", blockModelToMoveOnFloor = " + floorEvent.blockModelToMoveOnFloor );
                            }
                        }
                    }
                }

                if( transformEvents != null && transformEvents.Count > 0 )
                {
                    foreach( BlockEventTransform transformEvent in transformEvents )
                    {
                        if( transformEvent != null )
                        {
                            if( ( transformEvent.action == BlockEventTransform.Actions.EnablePinchToScale ||
                                transformEvent.action == BlockEventTransform.Actions.EnableTwistToRotate ) && 
                                transformEvent.gestureTransformType == BlockEventTransform.GestureTransformType.BlockModel &&
                                transformEvent.blockModelToChange == blockModel )
                            {
                                //Debug.Log( "BlockEventBlockModel.cs CallChangeModelEvent() calling transformEvent.CallEvent()" );
                                transformEvent.PrepareEvent();
                                transformEvent.CallEvent();
                            }
                            else
                            {
                                //Debug.Log( "BlockEventBlockModel.cs CallChangeModelEvent() action = " + transformEvent.action + ", gestureType = " + transformEvent.gestureTransformType + ", blockModelToChange = " + transformEvent.blockModelToChange );
                            }
                        }
                    }
                }

            }

            if( onActionCompleted != null ) { onActionCompleted.Invoke(); }

        } //END CallChangeModelEvent



    } //END BlockEventBlock

} //END Namespace