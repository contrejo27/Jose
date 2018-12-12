/* CustomMarker.cs
 * Allows you to customize and interact with a marker on the map.
 * Uses ETriggerType to denote which type of interaction is allowed for this marker.
*/ 

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Sirenix.OdinInspector;

namespace BrandXR
{
    public enum ETriggerType
    {
        InRange,
        OnClick,
        InRangeClick
    }

    //------------------------------------------------//
    public class CustomMarker : SerializedMonoBehaviour
    //------------------------------------------------//
    {
        // The type of trigger activation 
        public ETriggerType TriggerType { get { return triggerType; }}
        [BoxGroup("Event Attributes"), SerializeField, EnumToggleButtons] private ETriggerType triggerType = ETriggerType.InRange;

        public bool IsTriggered { get { return isTriggered; } set { isTriggered = value; }}
        private bool isTriggered = false;

        [SerializeField, BoxGroup("Event Attributes")] private BlockEvent blockEvent = null;

        [SerializeField] private bool showDebug = false; 

        //----------------------------------//
        public void Interact(bool isInRange)
        //----------------------------------//
        {
            if (isTriggered)
            {
                return;
            }
            
            if (blockEvent)
            {
                if(showDebug)
                {
                    Debug.Log("trigger event " + ((isInRange) ? "in range!" : "not in range"));
                }

                blockEvent.Call();
            }
            else
            {
                if(showDebug)
                {
                    Debug.LogWarning("No Block Event set on " + this.name);
                }
            }
        } // END Interact

        //----------------------//
        public void ResetMarker()
        //----------------------//
        {
            isTriggered = false;
        } // END ResetMarker

    } // END Class

} // END Namespace

