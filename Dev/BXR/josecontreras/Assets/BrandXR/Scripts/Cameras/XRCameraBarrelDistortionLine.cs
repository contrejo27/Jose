using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace BrandXR
{
    public class XRCameraBarrelDistortionLine: MonoBehaviour
    {

        public Image line;

        //Singleton behavior
        private static XRCameraBarrelDistortionLine _instance;

        //--------------------------------------------//
        public static XRCameraBarrelDistortionLine instance
        //--------------------------------------------//
        {
            get
            {
                if( _instance == null )
                {
                    _instance = GameObject.FindObjectOfType<XRCameraBarrelDistortionLine>();
                }

                return _instance;
            }

        } //END Instance

        //--------------------------------------------//
        public void Awake()
        //--------------------------------------------//
        {

            DestroyDuplicateInstance();

            if( line == null && GetComponentInChildren<Image>() != null )
            {
                line = GetComponentInChildren<Image>();
            }

        } //END Awake

        //--------------------------------------------//
        private void DestroyDuplicateInstance()
        //--------------------------------------------//
        {

            //Ensure only one instance exists
            if( _instance == null )
            {
                _instance = this;
            }
            else if( this != _instance )
            {
                Destroy( this.gameObject );
            }

        } //END DestroyDuplicateInstance


        

        //------------------------------//
        public void SetDistortionLine( bool enabled )
        //------------------------------//
        {

            if( line != null )
            {
                line.enabled = enabled;
            }

        } //END SetDistortionLine

    } //END Class

} //END Namespace