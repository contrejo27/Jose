using UnityEngine;
using System.Collections;
using UnityEngine.Events;
using System;

namespace BrandXR
{
    public class WWWMessageReciever: MonoBehaviour
    {

        [System.Serializable]
        public class OnWWWMessageRecieved : UnityEvent<string>{};

        [Tooltip("Unique Identifier : Used to prevent every event from firing on every component with this script")]
        public string RecieverID = "";

        public OnWWWMessageRecieved onWWWMessageRecieved;
        

        //--------------------------------------------//
        public void Start()
        //--------------------------------------------//
        {

            CreateRequiredPrefabs();

            AddToWWWHelper();
            
        } //END Start

        //--------------------------------------------//
        public void CreateRequiredPrefabs()
        //--------------------------------------------//
        {

            if( PrefabManager.instance != null )
            {
                if( WWWHelper.instance == null )
                {
                    PrefabManager.InstantiatePrefab( PrefabFactory.Prefabs.bxr_WWWHelper );
                }
            }

        } //END CreateRequiredPrefabs

        //--------------------------------------------//
        public void AddToWWWHelper()
        //--------------------------------------------//
        {

            if( WWWHelper.instance != null )
            {
                WWWHelper.instance.AddMessageFromWebpageReciever( this );
            }

        } //END AddToWWWHelper

        //--------------------------------------------//
        public void MessageRecieved( string id, string message )
        //--------------------------------------------//
        {

            if( onWWWMessageRecieved != null && RecieverID == id )
            {
                onWWWMessageRecieved.Invoke( message );
            }

        } //END MessageRecieved

    } //END Class

} //END Namespace