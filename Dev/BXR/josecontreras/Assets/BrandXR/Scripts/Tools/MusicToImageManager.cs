using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System;

namespace BrandXR
{
    public class MusicToImageManager: MonoBehaviour
    {

        public AudioSource musicAudioSource;
        

        public Dictionary<XRSkyboxFactory.ImageType, AudioClip> BGMChoices = new Dictionary<XRSkyboxFactory.ImageType, AudioClip>();

        public float crossfadeDuration = .25f;

        //Singleton behavior
        private static MusicToImageManager _instance;

        //--------------------------------------------//
        public static MusicToImageManager instance
        //--------------------------------------------//
        {
            get
            {
                if( _instance == null )
                {
                    _instance = GameObject.FindObjectOfType<MusicToImageManager>();
                }

                return _instance;
            }

        } //END Instance

        //--------------------------------------------//
        public void Awake()
        //--------------------------------------------//
        {

            DestroyDuplicateInstance();

            if( transform.parent == null )
            {
                DontDestroyOnLoad( transform.gameObject );
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

        //----------------------------------//
        void OnEnable()
        //----------------------------------//
        {

            Messenger.AddListener( "ImageManager_OnBlendToImageComplete", SwapMusicToCurrentImageType );

        } //END OnEnable

        //----------------------------------//
        void OnDisable()
        //----------------------------------//
        {

            Messenger.RemoveListener( "ImageManager_OnBlendToImageComplete", SwapMusicToCurrentImageType );

        } //END OnDisable

        //------------------------------------//
        public void SwapMusicToCurrentImageType()
        //------------------------------------//
        {

            if( BGMChoices != null && BGMChoices.Count > 0 )
            {
                if( BGMChoices.ContainsKey( XRSkyboxFactory.currentImageType ) )
                {
                    //Debug.Log( "MusicToImageManager.cs SwapMusicToCurrentImageType()" );

                    if( musicAudioSource != null && BGMChoices != null && BGMChoices.ContainsKey( XRSkyboxFactory.currentImageType ) )
                    {
                        musicAudioSource.Stop();
                        musicAudioSource.clip = BGMChoices[ XRSkyboxFactory.currentImageType ];
                    }
                    //AudioManager.Instance.PlayMusic( BGMChoices[ ImageFactory.currentImageType ], 1f, 0f, crossfadeDuration, false );
                    //Debug.Log( "MusicToImageManager.cs SwapMusicToCurrentImageType() complete" );
                }
            }

        } //END SwapMusicToCurrentImageType

    } //END Class

} //END Namespace