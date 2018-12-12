using UnityEngine;

namespace BrandXR
{
    public class ScreenOrientationManager: MonoBehaviour
    {

        private ScreenOrientation orientation = ScreenOrientation.LandscapeLeft;
        private bool forceOrientation = true;

        //Singleton behavior
        private static ScreenOrientationManager _instance;

        //--------------------------------------------//
        public static ScreenOrientationManager instance
        //--------------------------------------------//
        {
            get
            {
                if( _instance == null )
                {
                    _instance = GameObject.FindObjectOfType<ScreenOrientationManager>();
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

        //-------------------------------------//
        void Start()
        //-------------------------------------//
        {
            if( forceOrientation )
            {
                Screen.orientation = orientation;
            }

        } //END Start

    } //END Class

} //END Namespace