using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

namespace BrandXR
{
    public class EventTriggerHelper: MonoBehaviour
    {

        //----------------------------------------------------------------//
        public static void AddEventTriggerListener 
            ( 
                EventTrigger trigger,
                EventTriggerType eventType,
                System.Action<BaseEventData> callback 
            )
        //----------------------------------------------------------------//
        {
            EventTrigger.Entry entry = new EventTrigger.Entry();

            entry.eventID = eventType;

            entry.callback = new EventTrigger.TriggerEvent();

            entry.callback.AddListener( new UnityEngine.Events.UnityAction<BaseEventData>( callback ) );

            trigger.triggers.Add( entry );

        } //END AddEventTriggerListener



    } //END Class

} //END Namespace