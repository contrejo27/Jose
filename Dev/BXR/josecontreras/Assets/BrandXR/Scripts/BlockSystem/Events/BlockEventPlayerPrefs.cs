using Sirenix.OdinInspector;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

namespace BrandXR
{
    public class BlockEventPlayerPrefs: BlockEventBase
    {
        
        //----------------- ACTIONS --------------------------------//
        public enum Actions
        {
            None,
            SaveVariable
        }

        [TitleGroup( "Block Event - Player Prefs", "Used to save to Player Prefs" )]
        public Actions action = Actions.SaveVariable;

        //----------------- "SAVE VARIABLE" VARIABLES ------------------------------//
        [Space( 15f ), ShowIf("action", Actions.SaveVariable )]
        public string saveName;

        public enum SaveType
        {
            Int,
            String
        }

        [ShowIf( "action", Actions.SaveVariable )]
        public SaveType saveType = SaveType.Int;
        private bool SaveTypeIsInt() { return action == Actions.SaveVariable && saveType == SaveType.Int; }
        private bool SaveTypeIsString() { return action == Actions.SaveVariable && saveType == SaveType.String; }

        [ShowIf( "SaveTypeIsInt" )]
        public int saveInt = 0;

        [ShowIf( "SaveTypeIsString" )]
        public string saveString = "";


        //----------------- "CALL UNITY EVENT" EVENTS ------------------------------//
        private bool ShowOnActionCompletedEvent() { return action == Actions.SaveVariable; }

        [Space( 15f ), ShowIf( "ShowOnActionCompletedEvent" ), FoldoutGroup("Event Messages")]
        public UnityEvent onActionCompleted = new UnityEvent();


        //---------------------------------------------------------//
        public override BlockEventBase.EventType GetEventType()
        //---------------------------------------------------------//
        {
            return BlockEventBase.EventType.PlayerPrefs;

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

            if( action == Actions.SaveVariable )
            {
                if( !string.IsNullOrEmpty( saveName ) )
                {
                    base.eventReady = true;
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
                if( action == Actions.SaveVariable )
                {
                    CallSaveVariableEvent();
                }
            }
            
        } //END CallEvent

        //---------------------------------//
        private void CallSaveVariableEvent()
        //---------------------------------//
        {

            if( SaveTypeIsInt() )
            {
                PlayerPrefs.SetInt( saveName, saveInt );
            }
            else if( SaveTypeIsString() )
            {
                PlayerPrefs.SetString( saveName, saveString );
            }

            if( onActionCompleted != null ) { onActionCompleted.Invoke(); }

        } //END CallSaveVariableEvent

    } //END BlockEventPlayerPrefs

} //END Namespace