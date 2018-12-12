using UnityEngine;

namespace BrandXR
{
    public static class Vector3Extension
    {
        public static string Print( this Vector3 v )
        {
            return v.ToString( "G4" );
        }

        public static string PrintForDatabase( this Vector3 vector )
        {
            return vector.x + "_" + vector.y + "_" + vector.z;
        }

    } //END Class

} //END Namespace