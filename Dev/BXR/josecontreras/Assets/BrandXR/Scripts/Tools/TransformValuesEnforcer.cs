using UnityEngine;

namespace BrandXR
{
    public class TransformValuesEnforcer: MonoBehaviour
    {
        public Transform transformToEnforce;

        public bool bool_EnforceLocalPosition = true;
        public Vector3 vector3_LocalPosition;

        public bool bool_EnforceLocalEulerAngles = true;
        public Vector3 vector3_LocalEulerAngles;

        public bool bool_EnforceLocalScale = true;
        public Vector3 vector3_LocalScale;

        //-------------------------------------------------//
        public void Awake()
        //-------------------------------------------------//
        {

            if( transformToEnforce == null )
            {
                transformToEnforce = this.transform;
            }

        } //END Awake

        //-------------------------------------------------//
        public void Update()
        //-------------------------------------------------//
        {
            if( bool_EnforceLocalPosition )
            {
                transformToEnforce.localPosition = vector3_LocalPosition;
            }
            if( bool_EnforceLocalEulerAngles )
            {
                transformToEnforce.localEulerAngles = vector3_LocalEulerAngles;
            }
            if( bool_EnforceLocalScale )
            {
                transformToEnforce.localScale = vector3_LocalScale;
            }
        }

    } //END Class

} //END Namespace