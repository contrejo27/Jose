using System;
using System.Collections;
using UnityEngine;

namespace BrandXR
{
    public class Timer: MonoBehaviour
    {

        public bool showDebug = false;


        //Singleton behavior
        private static Timer _instance;

        //--------------------------------------------//
        public static Timer instance
        //--------------------------------------------//
        {
            get
            {
                if( _instance == null )
                {
                    if( GameObject.FindObjectOfType<Timer>() == null )
                    {
                        if( PrefabManager.instance != null )
                        {
                            //Debug.Log("Timer.cs _instance() Attempting to Instantiate bxr_Timer");
                            PrefabManager.InstantiatePrefab(PrefabFactory.Prefabs.bxr_Timer);
                        }
                        else
                        {
                            //Debug.Log("Timer.cs _instance() creating bxr_Timer from script");
                            GameObject go = new GameObject();
                            go.name = "bxr_Timer";
                            go.AddComponent<Timer>();
                        }
                    }
                    _instance = GameObject.FindObjectOfType<Timer>();
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





        //-------------------------------------//
        public Coroutine In( float duration, Action callThis, GameObject owner )
        //-------------------------------------//
        {

            if( callThis != null && owner != null && owner.activeInHierarchy )
            {
                return StartCoroutine( _In( duration, callThis, owner ) );
            }
            else
            {
                return null;
            }

        } //END In

        //-------------------------------------//
        private IEnumerator _In( float duration, Action callThis, GameObject owner )
        //-------------------------------------//
        {
            yield return new WaitForSeconds( duration );

            if( callThis != null && owner != null && owner.activeInHierarchy ) { callThis.Invoke(); }

        } //END _In





        //-------------------------------------//
        public Coroutine Loop( float duration, Action callThis )
        //-------------------------------------//
        {

            if( callThis != null )
            {
                return StartCoroutine( _Loop( duration, callThis ) );
            }
            else
            {
                return null;
            }

        } //END Loop

        //-------------------------------------//
        private IEnumerator _Loop( float duration, Action callThis )
        //-------------------------------------//
        {
            while( true )
            {
                yield return new WaitForSeconds( duration );

                if( callThis != null ) { callThis.Invoke(); }
            }

        } //END _Loop




        //--------------------------------------//
        public void Cancel( Coroutine cancelThis )
        //--------------------------------------//
        {

            if( cancelThis != null ) { StopCoroutine( cancelThis ); }

        } //END Cancel




    } //END Class

} //END Namespace