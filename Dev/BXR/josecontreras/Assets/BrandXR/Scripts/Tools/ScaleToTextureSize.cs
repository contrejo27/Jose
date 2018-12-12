using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace BrandXR
{
    public class ScaleToTextureSize: MonoBehaviour
    {

        public bool setOnUpdate = true;

        public float multiplier = 1f;

        private float width = 0;
        private float height = 0;

        private MeshRenderer meshRenderer = null;

        //----------------------------------//
        public void Start()
        //----------------------------------//
        {

            GetRenderer();

            GetScale();

            SetTransformToScale();

        } //END Start

        //----------------------------------//
        public void Update()
        //----------------------------------//
        {

            if( setOnUpdate )
            {
                GetRenderer();
                GetScale();
                SetTransformToScale();
            }

        } //END Update

        //----------------------------------//
        private void GetRenderer()
        //----------------------------------//
        {

            if( GetComponent<MeshRenderer>() != null )
            {
                meshRenderer = GetComponent<MeshRenderer>();
            }

        } //END GetRenderer

        //----------------------------------//
        private void GetScale()
        //----------------------------------//
        {

            if( meshRenderer != null && meshRenderer.sharedMaterial != null && meshRenderer.sharedMaterial.mainTexture != null )
            {
                width = meshRenderer.sharedMaterial.mainTexture.width;
                height = meshRenderer.sharedMaterial.mainTexture.height;
            }

        } //END GetScale

        //------------------------------------//
        private void SetTransformToScale()
        //------------------------------------//
        {

            if( transform != null && width != 0 && height != 0 )
            {
                transform.localScale = new Vector3( multiplier * width, transform.localScale.y, multiplier * height );
                //transform.localScale = new Vector3( transform.localScale.x, transform.localScale.y, 1 * ( width / height ) );
            }

        } //END SetTransformToScale

    } //END Class

} //END Namespace