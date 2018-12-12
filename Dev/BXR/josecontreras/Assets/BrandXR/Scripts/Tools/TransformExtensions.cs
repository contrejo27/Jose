using UnityEngine;
using System.Collections;

namespace BrandXR
{
    public static class TransformExtensions
    {
        
        //-------------------------------------------------------------------//
        /// <summary>
        /// Set the offsets of a RectTransform's rectangle
        /// </summary>
        /// <param name="rectTransform">RectTransform to modify</param>
        /// <param name="left">Left Offset</param>
        /// <param name="top">Top Offset</param>
        /// <param name="right">Right Offset</param>
        /// <param name="bottom">Bottom Offset</param>
        public static void SetOffsets( this RectTransform rectTransform, float left, float top, float right, float bottom )
        //-------------------------------------------------------------------//
        {
            if( rectTransform != null )
            {
                rectTransform.offsetMin = new Vector2( left, bottom );
                rectTransform.offsetMax = new Vector2( -right, -top );
            }

        } //END SetOffsets

    } //END Class

} //END Namespace