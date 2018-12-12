using Sirenix.OdinInspector;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace BrandXR
{
    public class BlockEventDropdown: BlockEventBase
    {
        //----------------- ACTIONS --------------------------------//
        public enum Actions
        {
            None,
            CallWhenSelected
        }

        [TitleGroup( "Block Event - Dropdown", "Used to modify what will happen when a dropdown option is selected" )]
        public Actions action = Actions.CallWhenSelected;
        private bool CallWhenSelected() { return action == Actions.CallWhenSelected; }

        //------------- "CALL WHEN SELECTED" VARIABLES ---------------------------------//
        [InfoBox( "When the dropdown option is selected with the given title, the actions you set up here will be called" )]

        [ShowIf( "CallWhenSelected" ), FoldoutGroup( "Dropdown Settings" )]
        public Dropdown dropdown;

        //https://docs.unity3d.com/ScriptReference/Events.UnityEvent_1.html
        [System.Serializable]
        public class DropdownAction: UnityEvent { }

        [System.Serializable]
        public class DropdownOptions
        {
            [Tooltip( "Pass the dropdown option title here" )]
            public string WhenSelected = "";

            [Tooltip( "What you want to happen when option is selected" )]
            public DropdownAction PerformAction;
        }

        [ShowIf( "CallWhenSelected" ), FoldoutGroup( "Dropdown Settings" )]
        public List<DropdownOptions> options;


        //----------------- "CALL WHEN SELECTED" EVENTS ------------------------------//
        private bool ShowOnActionCompletedEvent() { return action == Actions.CallWhenSelected; }
        
        [Space( 15f ), ShowIf( "ShowOnActionCompletedEvent" ), FoldoutGroup( "Event Messages" )]
        public UnityEvent onActionCompleted = new UnityEvent();



        //---------------------------------------------------------//
        public override EventType GetEventType()
        //---------------------------------------------------------//
        {
            return EventType.Dropdown;

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

            if( action == Actions.CallWhenSelected )
            {
                if( dropdown != null && dropdown.options.Count > 0 && options != null && options.Count > 0 )
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
                if( action == Actions.CallWhenSelected )
                {
                    CallWhenSelectedEvent();
                }
            }

        } //END CallEvent

        //------------------------------//
        private void CallWhenSelectedEvent()
        //------------------------------//
        {

            string dropdownValue = dropdown.options[ dropdown.value ].text;
            
            for( int i = 0; i < options.Count; i++ )
            {
                //Debug.Log( "DropdownValue(" + dropdownValue + ") ?= " + "options[" + i + "].WhenSelected( " + options[ i ].WhenSelected + " ) = " + ( options[ i ].WhenSelected == dropdownValue ) );

                if( options[ i ].WhenSelected == dropdownValue )
                {
                    options[ i ].PerformAction.Invoke();
                    break;
                }
            }

            if( onActionCompleted != null ) { onActionCompleted.Invoke(); }

        } //END CallWhenSelectedEvent

    } //END BlockEventDropdown

} //END Namespace