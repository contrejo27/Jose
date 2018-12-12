using System;
using System.Collections;
using UnityEngine;

namespace BrandXR
{
    public class InternetReachabilityVerifier: MonoBehaviour
    {

        public bool showDebug = false;

        private Coroutine coroutine;


        public float checkInterval = 1f; //1 Second
        public string checkURL = "http://www.google.com";

        public static bool bool_NetActivityMethodCompletedAtLeastOnce = false;

        public enum Status
        {
            NetVerified,
            NoConnection
        }
        [NonSerialized]
        public Status status = Status.NoConnection;
        


        //Singleton behavior
        private static InternetReachabilityVerifier _instance;

        //--------------------------------------------//
        public static InternetReachabilityVerifier instance
        //--------------------------------------------//
        {
            get
            {
                if( _instance == null )
                {
                    _instance = GameObject.FindObjectOfType<InternetReachabilityVerifier>();
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



        //---------------------------------------//
        public void Start()
        //---------------------------------------//
        {

            coroutine = StartCoroutine( CheckInternetAccess() );

        } //END Start
        

        //---------------------------------------//
        private IEnumerator CheckInternetAccess()
        //---------------------------------------//
        {

            WWW www;

            while(true)
            {
                yield return new WaitForSeconds( checkInterval );

                www = new WWW( checkURL );

                bool_NetActivityMethodCompletedAtLeastOnce = true;

                if( string.IsNullOrEmpty( www.error ) )
                {
                    status = Status.NetVerified;
                }
                else
                {
                    status = Status.NoConnection;
                }
            }
            
        } //END CheckInternetAccess




        






        //--------------------------------------//
        public void Cancel()
        //--------------------------------------//
        {

            if( coroutine != null ) { StopCoroutine( coroutine ); }

        } //END Cancel




    } //END Class

} //END Namespace