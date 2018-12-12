using System.Collections;
using UnityEngine;

namespace BrandXR
{
    public class AudioHelper : MonoBehaviour
    {

        public bool showDebug = false;

        //Singleton behavior
        private static AudioHelper _instance;

        //--------------------------------------------//
        public static AudioHelper instance
        //--------------------------------------------//
        {
            get
            {
                if( _instance == null )
                {
                    if( GameObject.FindObjectOfType<AudioHelper>() == null) { PrefabManager.InstantiatePrefab( PrefabFactory.Prefabs.bxr_AudioHelper ); }
                    _instance = GameObject.FindObjectOfType<AudioHelper>();
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
        public Coroutine Fade( AudioSource audioSource, float targetVolume, float length, float delay )
        //-------------------------------------//
        {

            if( audioSource != null )
            {
                return StartCoroutine( _Fade( audioSource, targetVolume, length, delay ) );
            }
            else
            {
                return null;
            }

        } //END Fade

        //-------------------------------------//
        private IEnumerator _Fade( AudioSource audioSource, float targetVolume, float length, float delay )
        //-------------------------------------//
        {
            if( audioSource != null )
            {
                yield return new WaitForSeconds( delay );

                float start = audioSource.volume;
                float end = targetVolume;

                float i = 0f;

                float VolumeDiff = Mathf.Abs( audioSource.volume - targetVolume );
                float step = VolumeDiff / length;

                if( showDebug ) Debug.Log( "StartedFade Time:" + AudioSettings.dspTime + " Vol:" + audioSource.volume );

                while( i < VolumeDiff )
                {
                    i += step * Time.deltaTime;
                    audioSource.volume = Mathf.Lerp( start, end, ( i / VolumeDiff ) );
                    //Debug.Log("Start:" + (float)start + " End:" + (float)end + " Step:" + step + " i:" + i + " Deltatime:" + Time.deltaTime);
                    yield return null;
                }

                if( showDebug ) Debug.Log( "EndedFade Time:" + AudioSettings.dspTime + " Vol:" + audioSource.volume );
            }

        } //END _Fade





    } //END Class

} //END Namespace
