/* LocationMapExplanation.cs
 * Publicly displays the intention and use of this object
 */

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Sirenix.OdinInspector;

namespace BrandXR
{
    public class LocationMapExplanation : MonoBehaviour
    {
        [Space(2), TitleGroup("Location Provider Object", "Handles tracking the player's current location.")]

        [Space(2), TitleGroup("Player Object", "Used to customize visual representation of player's marker.  Also used to dictate how the player will interact with custom markers.")]

        [Space(2), TitleGroup("Map Object", "Contains the Map component to display a location based map, centered around the player's location." +
                                "Using the AbstractMap class on the Map object, you can customize the map visuals.")]

        [SerializeField, HideLabel] private int thisDoesNothing = 0;

    } // END Class

} // END Namespace

