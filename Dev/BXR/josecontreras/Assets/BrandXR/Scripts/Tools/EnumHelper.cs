using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace BrandXR
{
    public class EnumHelper
    {

        //Enum comparer from http://answers.unity3d.com/questions/1318531/getting-a-enum-value-after-comparing-string-name.html
        //------------------------------------------------------------------------------------------------//
        public static bool TryParseEnum<TEnum>( string aName, out TEnum aValue ) where TEnum : struct
        //------------------------------------------------------------------------------------------------//
        {
            try
            {
                aValue = (TEnum)System.Enum.Parse( typeof( TEnum ), aName );
                return true;
            }
            catch
            {
                aValue = default( TEnum );
                return false;
            }

        } //END TryParseEnum



    } //END Class

} //END Namespace