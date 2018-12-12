using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace BrandXR
{
    public class XRCameraBarrelDistortion: PostEffectsBase
    {
        
        public bool enableDistortion = false;
        private Vector2 strength = new Vector2( 1f, 1f );
        
        private Shader BarrelDistortionShader = null;
	    private Material BarrelDistortionMaterial = null;
        
        //Singleton behavior
        private static XRCameraBarrelDistortion _instance;

        //--------------------------------------------//
        public static XRCameraBarrelDistortion instance
        //--------------------------------------------//
        {
            get
            {
                if( _instance == null )
                {
                    _instance = GameObject.FindObjectOfType<XRCameraBarrelDistortion>();
                }

                return _instance;
            }

        } //END Instance

        //--------------------------------------------//
        public void Awake()
        //--------------------------------------------//
        {

            DestroyDuplicateInstance();

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

        
        //-----------------------------------//
        public void SetBarrelShader( ref Shader shader )
        //-----------------------------------//
        {

            this.BarrelDistortionShader = shader;

        } //END SetBarrelShader

        //-----------------------------------//
        public void SetCameraDistortionCorrection( bool enabled )
        //-----------------------------------//
        {

            this.enableDistortion = enabled;

        } //END SetCameraDistortionCorrection

        //-----------------------------------//
        public void SetCameraDistortionCorrection( bool enabled, Vector2 strength )
        //-----------------------------------//
        {

            this.enableDistortion = enabled;
            this.strength = strength;

        } //END SetCameraDistortionCorrection

        //---------------------------------------//
        public override bool CheckResources()
        //---------------------------------------//
        {

            CheckSupport( false );

            BarrelDistortionMaterial = CheckShaderAndCreateMaterial( BarrelDistortionShader, BarrelDistortionMaterial );

            if( !isSupported )
            {
                ReportAutoDisable();
            }

            return isSupported;

        } //END CheckResources

        //-----------------------------------------------------------------------------//
        public void OnRenderImage( RenderTexture source, RenderTexture destination )
        //-----------------------------------------------------------------------------//
        {

            if( CheckResources() == false || !enableDistortion)
            {
                Graphics.Blit( source, destination );
                return;
            }
            
            BarrelDistortionMaterial.SetFloat( "k", strength.x );
            BarrelDistortionMaterial.SetFloat( "kcube", strength.y );
            Graphics.Blit( source, destination, BarrelDistortionMaterial );
            
        } //END OnRenderImage




    } //END Class

} //END Namespace