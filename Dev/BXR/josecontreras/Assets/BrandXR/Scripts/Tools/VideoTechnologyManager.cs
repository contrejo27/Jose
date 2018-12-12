using Sirenix.OdinInspector;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace BrandXR
{
    public class VideoTechnologyManager: MonoBehaviour
    {

        [Space( 15f ), TitleGroup( "Video Technology Manager", "Sets what video plugins are used in this app.")]
        public bool showDebug = false;

        //Singleton behavior
        private static VideoTechnologyManager _instance;

        //--------------------------------------------//
        public static VideoTechnologyManager instance
        //--------------------------------------------//
        {
            get
            {
                if( _instance == null )
                {
                    if( GameObject.FindObjectOfType<VideoTechnologyManager>() == null ) { PrefabManager.InstantiatePrefab( PrefabFactory.Prefabs.bxr_VideoTechnologyManager ); }
                    _instance = GameObject.FindObjectOfType<VideoTechnologyManager>();
                    BlockHelper.AddToBrandXRTechParent( _instance.transform );
                }

                return _instance;
            }

        } //END Instance

        //------------------------------------------------------//
        public void Awake()
        //------------------------------------------------------//
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



    } //END Class
}