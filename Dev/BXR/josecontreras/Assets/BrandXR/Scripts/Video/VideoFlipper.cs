using UnityEngine;

namespace BrandXR
{
    public class VideoFlipper: MonoBehaviour
    {

        public bool editor_FlipX;
        public bool editor_FlipY;
        public bool editor_FlipZ;

        public bool android_FlipX;
        public bool android_FlipY;
        public bool android_FlipZ;

        public bool ios_FlipX;
        public bool ios_FlipY;
        public bool ios_FlipZ;

        //---------------------------------//
        public void Start()
        //---------------------------------//
        {

#if UNITY_EDITOR
            Flip( editor_FlipX, editor_FlipY, editor_FlipZ );
#elif UNITY_ANDROID
                Flip( android_FlipX, android_FlipY, android_FlipZ );
#elif UNITY_IOS
                Flip( ios_FlipX, ios_FlipY, ios_FlipZ );
#endif

        } //END Start

        //--------------------------//
        private void Flip( bool flipX, bool flipY, bool flipZ )
        //--------------------------//
        {

            Transform t = this.transform;
            Vector3 scale = t.localScale;

            if( flipX ) { scale = new Vector3( scale.x * -1f, scale.y, scale.z ); }
            if( flipY ) { scale = new Vector3( scale.x, scale.y * -1f, scale.z ); }
            if( flipZ ) { scale = new Vector3( scale.x, scale.y, scale.z * -1f ); }

            if( this.GetComponent<TransformValuesEnforcer>() != null ) { this.GetComponent<TransformValuesEnforcer>().vector3_LocalScale = scale; }

        } //END Flip


    } //END Class

} //END Namespace