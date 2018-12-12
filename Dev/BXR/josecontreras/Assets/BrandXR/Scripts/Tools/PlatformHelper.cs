using UnityEngine;

namespace BrandXR
{
    public class PlatformHelper: MonoBehaviour
    {

        //Singleton behavior
        private static PlatformHelper _instance;

        //--------------------------------------------//
        public static PlatformHelper instance
        //--------------------------------------------//
        {
            get
            {
                if( _instance == null )
                {
                    _instance = GameObject.FindObjectOfType<PlatformHelper>();
                    BlockHelper.AddToBrandXRTechParent( _instance.transform );
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

        //----------------------------------------//
        public static bool ShouldPlayVideosFromPersistentDataPath()
        //----------------------------------------//
        {

            if( Application.platform == RuntimePlatform.Android
                //|| Application.platform == RuntimePlatform.WindowsEditor 
                )
            {
                return true;
            }

            return false;

        } //END ShouldPlayVideosFromPersistentDataPath

        //----------------------------------------//
        public static bool IsLowEndDevice()
        //----------------------------------------//
        {

            //If the larger part of the screen resolution is smaller than 1080p, then this is most likely a lower end device
            int res = Screen.currentResolution.width;
            if( res < Screen.currentResolution.height ) { res = Screen.currentResolution.height; }

            if( res < 1080 ) { return true; }

            return true;

        } //END IsLowEndDevice

        //------------------------------------------//
        public static bool IsMicConnected()
        //------------------------------------------//
        {

            return Microphone.devices.Length >= 1;

        } //END IsMicConnected

        //------------------------------------------//
        public static bool IsEditor()
        //------------------------------------------//
        {

            return Application.platform == RuntimePlatform.WindowsEditor || Application.platform == RuntimePlatform.OSXEditor || Application.platform == RuntimePlatform.LinuxEditor;

        } //END IsEditor

    } //END Class

} //END Namespace