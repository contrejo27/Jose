using UnityEngine;

namespace BrandXR
{
    public class SphereMirrorUVs: MonoBehaviour
    {

        //------------------------------//
        void Start()
        //------------------------------//
        {

            MirrorUVs();

        } //END Start

        //------------------------------//
        void MirrorUVs()
        //------------------------------//
        {

            Vector2[] vec2UVs = transform.GetComponent<MeshFilter>().mesh.uv;

            for( int i = 0; i < vec2UVs.Length; i++ )
            {
                vec2UVs[ i ] = new Vector2( 1.0f - vec2UVs[ i ].x, vec2UVs[ i ].y );
            }

            transform.GetComponent<MeshFilter>().mesh.uv = vec2UVs;

        } //END MirrorUVs
        
    } //END Class

} //END Namespace