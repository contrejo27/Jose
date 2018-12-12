using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Networking;
using UnityEngine.Video;

namespace BrandXR
{
    public class WWWHelper: MonoBehaviour
    {
        public static bool showDebug = false;

        public ApplicationOnlineModeType ForcedOnlineModeType = ApplicationOnlineModeType.Null;

        public static bool IsInternetReachable = true;
        public static Coroutine handle_InternetReachabilityMessengerLoop;

        public enum ApplicationOnlineModeType
        {
            Null,
            Online,
            Offline
        }
        public static ApplicationOnlineModeType OnlineModeType = ApplicationOnlineModeType.Null;

        private List<WWWMessageReciever> wwwMessageRecievers = new List<WWWMessageReciever>();

        //native device variables
        private AndroidJavaObject extras;
        private AndroidJavaObject intent = null;
        private bool androidDataExists = false;
        private string arguments = "";

        //Singleton behavior
        private static WWWHelper _instance;

        public enum LocationType
        {
            TryAllLocations,
            StreamingAssets,
            PersistentDataPath,
            Resources,
            Web
        }


        #region INITIALIZATION
        //--------------------------------------------//
        public static WWWHelper instance
        //--------------------------------------------//
        {
            get
            {
                if( _instance == null )
                {
                    if( GameObject.FindObjectOfType<WWWHelper>() == null ) { PrefabManager.InstantiatePrefab( PrefabFactory.Prefabs.bxr_WWWHelper ); }
                    _instance = GameObject.FindObjectOfType<WWWHelper>();
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

            CheckIfRequiredPrefabsExist();

            InitializeNativeData();

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

        //--------------------------------------------//
        private void CheckIfRequiredPrefabsExist()
        //--------------------------------------------//
        {

            if( Timer.instance == null )
            {
                PrefabManager.InstantiatePrefab( PrefabFactory.Prefabs.bxr_Timer );
            }

        } //END CheckIfRequiredPrefabsExist




        //-----------------------------------------//
        private bool RequireWWWForFileExistsCheck()
        //-----------------------------------------//
        {
            if( Application.platform == RuntimePlatform.Android )
            {
                return true;
            }
            else if( Application.platform == RuntimePlatform.WebGLPlayer )
            {
                return true;
            }

            return false;

        } //END RequireWWWForFileExistsCheck

        //---------------------------------------------//
        public static TextureFormat GetPlatformPreferredTextureFormat()
        //---------------------------------------------//
        {

#if UNITY_ANDROID && !UNITY_EDITOR
                return TextureFormat.ETC_RGB4;
#elif UNITY_IOS && !UNITY_EDITOR
                return TextureFormat.PVRTC_RGB4;
#else
            return TextureFormat.DXT1;
#endif

        } //END GetPlatformPreferredTextureFormat

        //-----------------------------------------//
        public void Start()
        //-----------------------------------------//
        {

            Timer.instance.In( .01f, CreateInternetReachabilityMessengerLoop, gameObject );

        } //END Start
        #endregion

        #region UPDATE
        //-----------------------------------------//
        void Update()
        //-----------------------------------------//
        {
            if (androidDataExists)
            {
                extras = intent.Call<AndroidJavaObject>("getExtras");
                arguments = extras.Call<string>("getString", "arguments");
            }
        } //END Update
        #endregion

        #region INTERNET REACHABILITY
        //-----------------------------------------//
        public void KillAllTimersAndTweens()
        //-----------------------------------------//
        {

            KillInternetReachabilityMessengerLoop();

        } //END KillAllTimersAndTweens

        //-----------------------------------------//
        public void CreateInternetReachabilityMessengerLoop()
        //-----------------------------------------//
        {
            //Debug.Log( "WWWHelper.cs CreateInternetReachabilityMessengerLoop()" );

            KillInternetReachabilityMessengerLoop();

            handle_InternetReachabilityMessengerLoop = Timer.instance.Loop( .5f, SetInternetReachbility );

        } //END CreateInternetReachabilityMessengerLoop

        //-----------------------------------------//
        public void KillInternetReachabilityMessengerLoop()
        //-----------------------------------------//
        {
            if( handle_InternetReachabilityMessengerLoop != null )
            {
                Timer.instance.Cancel( handle_InternetReachabilityMessengerLoop );
            }

        } //END KillInternetReachabilityMessengerLoop

        //-----------------------------------------//
        public void SetInternetReachbility()
        //-----------------------------------------//
        {
            bool showDebug = false;

            if( ForcedOnlineModeType == ApplicationOnlineModeType.Offline )
            {
                //if( showDebug ) Debug.Log ( "SetInternetReachbility() Forced To Off" );
                IsInternetReachable = false;
                Messenger.Broadcast( "InternetIsNotReachable", MessengerMode.DONT_REQUIRE_LISTENER );

                if( OnlineModeType == ApplicationOnlineModeType.Online || OnlineModeType == ApplicationOnlineModeType.Null )
                {
                    if( showDebug ) Debug.Log( "SetInternetReachbility() OnlineMode changed to Offline!" );
                    OnlineModeType = ApplicationOnlineModeType.Offline;
                    Messenger.Broadcast( "ApplicationOnlineModeTypeChange_Offline", MessengerMode.DONT_REQUIRE_LISTENER );
                }
            }
            else if( ForcedOnlineModeType == ApplicationOnlineModeType.Online )
            {
                //if( showDebug ) Debug.Log ( "SetInternetReachbility() Forced To On" );
                IsInternetReachable = true;
                Messenger.Broadcast( "InternetIsReachable", MessengerMode.DONT_REQUIRE_LISTENER );

                if( OnlineModeType == ApplicationOnlineModeType.Offline || OnlineModeType == ApplicationOnlineModeType.Null )
                {
                    if( showDebug ) Debug.Log( "SetInternetReachbility() OnlineMode changed to Online!" );
                    OnlineModeType = ApplicationOnlineModeType.Online;
                    Messenger.Broadcast( "ApplicationOnlineModeTypeChange_Online", MessengerMode.DONT_REQUIRE_LISTENER );
                }
            }
            else
            {
                if( InternetReachabilityVerifier.bool_NetActivityMethodCompletedAtLeastOnce &&
                    InternetReachabilityVerifier.instance != null )
                {
                    //if( showDebug ) Debug.Log ( "SetInternetReachbility() InternetReachabilityVerifier completed at least once and Instance != null" );

                    if( InternetReachabilityVerifier.instance.status == InternetReachabilityVerifier.Status.NetVerified )
                    {
                        if( showDebug ) Debug.Log( "SetInternetReachbility() InternetReachabilityVerifier.Instance != null, internet is reachable, returning true" );
                        IsInternetReachable = true;
                        Messenger.Broadcast( "InternetIsReachable", MessengerMode.DONT_REQUIRE_LISTENER );

                        if( OnlineModeType == ApplicationOnlineModeType.Offline || OnlineModeType == ApplicationOnlineModeType.Null )
                        {
                            if( showDebug ) Debug.Log( "SetInternetReachbility() OnlineMode changed to Online!" );
                            OnlineModeType = ApplicationOnlineModeType.Online;
                            Messenger.Broadcast( "ApplicationOnlineModeTypeChange_Online", MessengerMode.DONT_REQUIRE_LISTENER );
                        }
                    }
                    else
                    {
                        //if( showDebug ) Debug.Log ( "SetInternetReachbility() InternetReachabilityVerifier.Instance != null, internet is not reachable, returning false" );
                        IsInternetReachable = false;
                        Messenger.Broadcast( "InternetIsNotReachable", MessengerMode.DONT_REQUIRE_LISTENER );

                        if( OnlineModeType == ApplicationOnlineModeType.Online || OnlineModeType == ApplicationOnlineModeType.Null )
                        {
                            if( showDebug ) Debug.Log( "SetInternetReachbility() OnlineMode changed to Offline!" );
                            OnlineModeType = ApplicationOnlineModeType.Offline;
                            Messenger.Broadcast( "ApplicationOnlineModeTypeChange_Offline", MessengerMode.DONT_REQUIRE_LISTENER );
                        }
                    }
                }
                else
                {
                    //if( showDebug ) Debug.Log ( "SetInternetReachbility() InternetReachabilityVerifier has not completed once or Instance == null" );
                    IsInternetReachable = true;
                    Messenger.Broadcast( "InternetIsReachable", MessengerMode.DONT_REQUIRE_LISTENER );

                    if( OnlineModeType == ApplicationOnlineModeType.Offline || OnlineModeType == ApplicationOnlineModeType.Null )
                    {
                        if( showDebug ) Debug.Log( "SetInternetReachbility() OnlineMode changed to Online!" );
                        OnlineModeType = ApplicationOnlineModeType.Online;
                        Messenger.Broadcast( "ApplicationOnlineModeTypeChange_Online", MessengerMode.DONT_REQUIRE_LISTENER );
                    }
                }
            }

        } //END SetInternetReachbility
        #endregion

        #region GET VIDEOCLIP FROM LOCAL OR WEB [DISABLED]
        /*
        //---------------------------------//
        private class VideoHolder
        //---------------------------------//
        {
            public string fileName = "";
            public VideoClip videoClip = null;
            public bool cacheIfWeb = false;
            public string path = "";

        } //END VideoHolder

        //---------------------------------//
        public void GetVideoClip( string path, bool cacheIfWeb, Action<VideoClip> onSuccess, Action onFailed, LocationType downloadLocation = LocationType.TryAllLocations, string fileName = "" )
        //---------------------------------//
        {
            if( showDebugLog ) { Debug.Log( "WWWHelper.cs GetVideoClip() path = " + path + ", cacheIfWeb = " + cacheIfWeb + ", onSuccess = " + onSuccess + ", onFailed = " + onFailed ); }

            VideoHolder videoHolder = new VideoHolder();
            videoHolder.fileName = fileName;
            videoHolder.path = path;
            videoHolder.cacheIfWeb = cacheIfWeb;

            if( downloadLocation == LocationType.TryAllLocations )
            {
                //Check Streaming Assets first, then Resources, then PersistentDataPath, then Web... if none of them have the file, then it probably doesn't exist
                StartCoroutine( GetVideoClipFromStreamingAssets( videoHolder, onSuccess, onFailed, GetVideoClip_StreamingAssets_Failed ) );
            }
            else if( downloadLocation == LocationType.StreamingAssets )
            {
                StartCoroutine( GetVideoClipFromStreamingAssets( videoHolder, onSuccess, onFailed, null ) );
            }
            else if( downloadLocation == LocationType.Resources )
            {
                StartCoroutine( GetVideoClipFromResources( videoHolder, onSuccess, onFailed, null ) );
            }
            else if( downloadLocation == LocationType.PersistentDataPath )
            {
                StartCoroutine( GetVideoClipFromPersistentData( videoHolder, onSuccess, onFailed, null ) );
            }
            else if( downloadLocation == LocationType.Web )
            {
                StartCoroutine( GetVideoClipFromWeb( videoHolder, onSuccess, onFailed ) );
            }

        } //END GetVideoClip

        //----------------------------------//
        private void GetVideoClip_StreamingAssets_Failed( VideoHolder videoHolder, Action<VideoClip> onSuccess, Action originalOnFailed )
        //----------------------------------//
        {
            if( showDebugLog ) { Debug.Log( "WWWHelper.cs GetVideoClip_StreamingAssets_Failed() path = " + videoHolder.path + ", cacheIfWeb = " + videoHolder.cacheIfWeb + ", onSuccess = " + onSuccess + ", onFailed = " + originalOnFailed ); }

            StartCoroutine( GetVideoClipFromResources( videoHolder, onSuccess, originalOnFailed, GetVideoClip_Resources_Failed ) );

        } //END GetVideoClip_StreamingAssets_Failed

        //----------------------------------//
        private void GetVideoClip_Resources_Failed( VideoHolder videoHolder, Action<VideoClip> onSuccess, Action originalOnFailed )
        //----------------------------------//
        {
            if( showDebugLog ) { Debug.Log( "WWWHelper.cs GetVideoClip_Resources_Failed() path = " + videoHolder.path + ", cacheIfWeb = " + videoHolder.cacheIfWeb + ", onSuccess = " + onSuccess + ", onFailed = " + originalOnFailed ); }

            StartCoroutine( GetVideoClipFromPersistentData( videoHolder, onSuccess, originalOnFailed, GetVideoClip_PersistentData_Failed ) );

        } //END GetVideoClip_Resources_Failed

        //----------------------------------//
        private void GetVideoClip_PersistentData_Failed( VideoHolder videoHolder, Action<VideoClip> onSuccess, Action originalOnFailed )
        //----------------------------------//
        {
            if( showDebugLog ) { Debug.Log( "WWWHelper.cs GetVideoClip_PersistentData_Failed() path = " + videoHolder.path + ", cacheIfWeb = " + videoHolder.cacheIfWeb + ", onSuccess = " + onSuccess + ", onFailed = " + originalOnFailed ); }

            StartCoroutine( GetVideoClipFromWeb( videoHolder, onSuccess, originalOnFailed ) );

        } //END GetVideoClip_PersistentData_Failed

        //---------------------------------//
        private IEnumerator GetVideoClipFromStreamingAssets
        ( VideoHolder videoHolder,
            Action<VideoClip> onSuccess,
            Action originalOnFailed,
            Action<VideoHolder, Action<VideoClip>, Action> onFailed )
        //---------------------------------//
        {
            string streamingAssetsPath = videoHolder.path;
            string originalName = videoHolder.path;

            WWW www;

            string existsPath = DatabaseStringHelper.CreateStreamingAssetsPathForFileExistsCheck( streamingAssetsPath );

            bool exists = false;

            //Check if the texture exists, this check is different by platform
            if( RequireWWWForFileExistsCheck() )
            {
                //Android can only check using www
                www = new WWW( existsPath );

                yield return www;

                exists = string.IsNullOrEmpty( www.error );
            }
            else
            {
                //All other platforms can check using System.IO
                exists = System.IO.File.Exists( existsPath );
            }

            //We couldn't locate the file, let's check again with a .png, .jpg, .gif appended to the file name and try each option
            if( !exists )
            {
                string pngPath = existsPath + ".png";

                //Check .png
                if( RequireWWWForFileExistsCheck() )
                {
                    //Android can only check using www
                    www = new WWW( pngPath );

                    yield return www;

                    exists = string.IsNullOrEmpty( www.error );
                }
                else
                {
                    //All other platforms can check using System.IO
                    exists = System.IO.File.Exists( pngPath );
                }

                if( exists )
                {
                    streamingAssetsPath += ".png";
                }
                else
                {
                    //Try again with .jpg
                    string jpgPath = existsPath + ".jpg";

                    //Check .wav
                    if( RequireWWWForFileExistsCheck() )
                    {
                        //Android can only check using www
                        www = new WWW( jpgPath );

                        yield return www;

                        exists = string.IsNullOrEmpty( www.error );
                    }
                    else
                    {
                        //All other platforms can check using System.IO
                        exists = System.IO.File.Exists( jpgPath );
                    }

                    if( exists )
                    {
                        streamingAssetsPath += ".jpg";
                    }
                    else
                    {
                        //Try again with .gif
                        string gifPath = existsPath + ".gif";

                        //Check .wav
                        if( RequireWWWForFileExistsCheck() )
                        {
                            //Android can only check using www
                            www = new WWW( gifPath );

                            yield return www;

                            exists = string.IsNullOrEmpty( www.error );
                        }
                        else
                        {
                            //All other platforms can check using System.IO
                            exists = System.IO.File.Exists( gifPath );
                        }

                        if( exists )
                        {
                            streamingAssetsPath += ".gif";
                        }
                    }
                }
            }

            if( exists )
            {
                string path = DatabaseStringHelper.CreateStreamingAssetsPath( streamingAssetsPath, DatabaseStringHelper.StringStyle.WithEscapeUriAndSystemPathCombine );

                //Download the bytes from the StreamingAssets folder
                www = new WWW( path );

                //Continue with the main thread until this finishes
                while( !www.isDone )
                {
                    yield return www;
                }

                videoHolder.texture = (VideoClip)www.;
                videoHolder.texture.name = originalName;

                if( videoHolder.texture != null )
                {
                    if( showDebugLog ) Debug.Log( "WWWHelper.cs GetTextureFromStreamingAssets() file exists, path = " + path );

                    if( onSuccess != null ) { onSuccess( videoHolder.texture ); }
                }
                else
                {
                    if( showDebugLog ) Debug.Log( "WWWHelper.cs GetTextureFromStreamingAssets() file exists, path = " + path + ", however unable to successfully use www.GetTexture" );

                    if( onFailed != null ) { onFailed( videoHolder, onSuccess, originalOnFailed ); }
                    else if( originalOnFailed != null ) { originalOnFailed.Invoke(); }
                }

            }
            else
            {
                if( showDebugLog ) Debug.Log( "WWWHelper.cs GetTextureFromStreamingAssets() file does not exist, path = " + streamingAssetsPath );

                if( onFailed != null ) { onFailed( videoHolder, onSuccess, originalOnFailed ); }
                else if( originalOnFailed != null ) { originalOnFailed.Invoke(); }
            }

        } //END GetVideoClipFromStreamingAssets
        */
        #endregion

        #region WEBPAGE COMMUNICATION
        //---------------------------------------------//
        //WEBGL COMMUNICATION EXAMPLE
        //---------------------------------------------//

        //HTML
        //<button type="button" onclick="SendMessageToUnity()">Press me</button>

        //Javascript
        /*
           function SendMessageToUnity() 
           {
                //WWWHelper GameObject, Reciever Method Name, RecieverID, Message
                gameInstance.SendMessage('WWWHelper', 'RecieveMessageFromWebpage', 'id', 'message');
           }
        */

        //----------------------------------//
        public void AddMessageFromWebpageReciever( WWWMessageReciever reciever )
        //----------------------------------//
        {

            if( reciever != null )
            {
                if( wwwMessageRecievers == null ) { wwwMessageRecievers = new List<WWWMessageReciever>(); }

                wwwMessageRecievers.Add( reciever );
            }

        } //END AddMessageFromWebpageReciever

        //----------------------------------//
        public void SendMessageToWebpage( string methodName, string message )
        //----------------------------------//
        {

            SendMessage( methodName, message );

        } //END SendMessageToWebpage

        //----------------------------------//
        public void RecieveMessageFromWebpage( string idAndMessage )
        //----------------------------------//
        {
            string[] _idAndMessage = idAndMessage.Split( '_' );

            if( wwwMessageRecievers != null && wwwMessageRecievers.Count > 0 )
            {
                for( int i = 0; i < wwwMessageRecievers.Count; i++ )
                {
                    wwwMessageRecievers[ i ].MessageRecieved( _idAndMessage[ 0 ], _idAndMessage[ 1 ] );
                }
            }

        } //END RecieveMessageFromWebpage
#endregion

        #region GET TEXT FROM LOCAL OR WEB
        //----------------------------------//
        private class TextHolder
        //----------------------------------//
        {
            public string fileName = "";
            public string text = "";
            public bool cacheIfWeb = false;
            public string path = "";

        } //END TextHolder

        //---------------------------------//
        public void GetText( string path, bool cacheIfWeb, Action<string> onSuccess, Action onFailed, LocationType downloadLocation = LocationType.TryAllLocations, string fileName = "" )
        //---------------------------------//
        {
            if( showDebug ) { Debug.Log( "WWWHelper.cs GetText() path = " + path + ", cacheIfWeb = " + cacheIfWeb + ", onSuccess = " + onSuccess + ", onFailed = " + onFailed ); }

            TextHolder textHolder = new TextHolder();
            textHolder.fileName = fileName;
            textHolder.path = path;
            textHolder.cacheIfWeb = cacheIfWeb;

            if( downloadLocation == LocationType.TryAllLocations )
            {
                //Check Streaming Assets first, then Resources, then PersistentDataPath, then Web... if none of them have the file, then it probably doesn't exist
                StartCoroutine( GetTextFromStreamingAssets( textHolder, onSuccess, onFailed, GetText_StreamingAssets_Failed ) );
            }
            else if( downloadLocation == LocationType.StreamingAssets )
            {
                StartCoroutine( GetTextFromStreamingAssets( textHolder, onSuccess, onFailed, null ) );
            }
            else if( downloadLocation == LocationType.Resources )
            {
                StartCoroutine( GetTextFromResources( textHolder, onSuccess, onFailed, null ) );
            }
            else if( downloadLocation == LocationType.PersistentDataPath )
            {
                StartCoroutine( GetTextFromPersistentData( textHolder, onSuccess, onFailed, null ) );
            }
            else if( downloadLocation == LocationType.Web )
            {
                StartCoroutine( GetTextFromWeb( textHolder, onSuccess, onFailed ) );
            }

        } //END GetText

        //----------------------------------//
        private void GetText_StreamingAssets_Failed( TextHolder textHolder, Action<string> onSuccess, Action originalOnFailed )
        //----------------------------------//
        {
            //if( showDebugLog ) { Debug.Log( "WWWHelper.cs GetText_StreamingAssets_Failed() path = " + textHolder.path + ", cacheIfWeb = " + textHolder.cacheIfWeb + ", onSuccess = " + onSuccess + ", onFailed = " + originalOnFailed ); }

            StartCoroutine( GetTextFromResources( textHolder, onSuccess, originalOnFailed, GetText_Resources_Failed ) );

        } //END GetText_StreamingAssets_Failed

        //----------------------------------//
        private void GetText_Resources_Failed( TextHolder textHolder, Action<string> onSuccess, Action originalOnFailed )
        //----------------------------------//
        {
            //if( showDebugLog ) { Debug.Log( "WWWHelper.cs GetText_Resources_Failed() path = " + textHolder.path + ", cacheIfWeb = " + textHolder.cacheIfWeb + ", onSuccess = " + onSuccess + ", onFailed = " + originalOnFailed ); }

            StartCoroutine( GetTextFromPersistentData( textHolder, onSuccess, originalOnFailed, GetText_PersistentData_Failed ) );

        } //END GetText_Resources_Failed

        //----------------------------------//
        private void GetText_PersistentData_Failed( TextHolder textHolder, Action<string> onSuccess, Action originalOnFailed )
        //----------------------------------//
        {
            //if( showDebugLog ) { Debug.Log( "WWWHelper.cs GetText_PersistentData_Failed() path = " + textHolder.path + ", cacheIfWeb = " + textHolder.cacheIfWeb + ", onSuccess = " + onSuccess + ", onFailed = " + originalOnFailed ); }

            StartCoroutine( GetTextFromWeb( textHolder, onSuccess, originalOnFailed ) );

        } //END GetText_PersistentData_Failed


        //---------------------------------//
        private IEnumerator GetTextFromStreamingAssets
        ( TextHolder textHolder,
            Action<string> onSuccess,
            Action originalOnFailed,
            Action<TextHolder, Action<string>, Action> onFailed )
        //---------------------------------//
        {
            string streamingAssetsPath = textHolder.path;

            WWW www;

            string existsPath = DatabaseStringHelper.CreateStreamingAssetsPathForFileExistsCheck( streamingAssetsPath );

            bool exists = false;

            //Check if the texture exists, this check is different by platform
            if( RequireWWWForFileExistsCheck() )
            {
                //Android can only check using www
                www = new WWW( existsPath );

                yield return www;

                exists = string.IsNullOrEmpty( www.error );
            }
            else
            {
                //All other platforms can check using System.IO
                exists = System.IO.File.Exists( existsPath );
            }

            //We couldn't locate the file, let's check again with a .txt appended to the file name and try each option
            if( !exists )
            {
                string textPath = existsPath + ".txt";

                //Check .txt
                if( RequireWWWForFileExistsCheck() )
                {
                    //Android can only check using www
                    www = new WWW( textPath );

                    yield return www;

                    exists = string.IsNullOrEmpty( www.error );
                }
                else
                {
                    //All other platforms can check using System.IO
                    exists = System.IO.File.Exists( textPath );
                }

                if( exists )
                {
                    streamingAssetsPath += ".txt";
                    if( showDebug ) { Debug.Log( "WWWHelper.cs GetTextFromStreamingAssets() found asset using appended .txt to file name! textPath = " + textPath ); }
                }
                else
                {
                    if( showDebug ) { Debug.Log( "WWWHelper.cs GetTextFromStreamingAssets() unable to find asset even with appended .txt to file name! textPath = " + textPath ); }
                }
            }

            if( exists )
            {
                string path = DatabaseStringHelper.CreateStreamingAssetsPath( streamingAssetsPath, DatabaseStringHelper.StringStyle.WithEscapeUriAndSystemPathCombine );

                //Download the bytes from the StreamingAssets folder
                www = new WWW( path );

                //Continue with the main thread until this finishes
                while( !www.isDone )
                {
                    yield return www;
                }

                textHolder.text = www.text;

                if( textHolder.text != null )
                {
                    if( showDebug ) Debug.Log( "WWWHelper.cs GetTextFromStreamingAssets() file exists, path = " + path );

                    if( onSuccess != null ) { onSuccess( textHolder.text ); }
                }
                else
                {
                    if( showDebug ) Debug.Log( "WWWHelper.cs GetTextFromStreamingAssets() file exists, path = " + path + ", however unable to successfully use www.GetTexture" );

                    if( onFailed != null ) { onFailed( textHolder, onSuccess, originalOnFailed ); }
                    else if( originalOnFailed != null ) { originalOnFailed.Invoke(); }
                }

            }
            else
            {
                if( showDebug ) Debug.Log( "WWWHelper.cs GetTextFromStreamingAssets() file does not exist, path = " + streamingAssetsPath );

                if( onFailed != null ) { onFailed( textHolder, onSuccess, originalOnFailed ); }
                else if( originalOnFailed != null ) { originalOnFailed.Invoke(); }
            }

        } //END GetTextFromStreamingAssets

        //---------------------------------//
        private IEnumerator GetTextFromResources
        ( TextHolder textHolder,
            Action<string> onSuccess,
            Action originalOnFailed,
            Action<TextHolder, Action<string>, Action> onFailed )
        //---------------------------------//
        {
            string resourcesPath = System.IO.Path.ChangeExtension( textHolder.path, null );

            ResourceRequest request = Resources.LoadAsync<TextAsset>( resourcesPath );

            while( !request.isDone )
            {
                yield return request;
            }

            if( request.asset != null )
            {
                if( showDebug ) Debug.Log( "WWWHelper.cs GetTextFromResources() Successfully found resources asset at path = " + resourcesPath );

                TextAsset textAsset = request.asset as TextAsset;
                textHolder.text = textAsset.text;

                if( onSuccess != null ) { onSuccess( textHolder.text ); }
            }
            else
            {
                if( showDebug ) Debug.Log( "WWWHelper.cs GetTextFromResources() Unable to find resources asset at path = " + resourcesPath );

                if( onFailed != null ) { onFailed( textHolder, onSuccess, originalOnFailed ); }
                else if( originalOnFailed != null ) { originalOnFailed.Invoke(); }
            }

        } //END GetTextFromResources

        //---------------------------------//
        private IEnumerator GetTextFromPersistentData
        ( TextHolder textHolder,
            Action<string> onSuccess,
            Action originalOnFailed,
            Action<TextHolder, Action<string>, Action> onFailed )
        //---------------------------------//
        {
            if( showDebug ) { Debug.Log( "WWWHelper.cs GetTextFromPersistentData() path = " + textHolder.path + ", cacheIfWeb = " + textHolder.cacheIfWeb + ", onSuccess = " + onSuccess + ", onFailed = " + originalOnFailed ); }

            string path = DatabaseStringHelper.CreatePersistentDataPath( textHolder.path );

            WWW www = new WWW( path );

            while( !www.isDone )
            {
                yield return www;
            }

            if( string.IsNullOrEmpty( www.error ) )
            {
                if( www.text != null )
                {
                    textHolder.text = www.text;

                    if( onSuccess != null ) { onSuccess( textHolder.text ); }
                }
                else
                {
                    if( onFailed != null ) { onFailed( textHolder, onSuccess, originalOnFailed ); }
                    else if( originalOnFailed != null ) { originalOnFailed.Invoke(); }
                }
            }
            else
            {
                if( onFailed != null ) { onFailed( textHolder, onSuccess, originalOnFailed ); }
                else if( originalOnFailed != null ) { originalOnFailed.Invoke(); }
            }

        } //END GetTextFromPersistentData

        //---------------------------------//
        private IEnumerator GetTextFromWeb
        ( TextHolder textHolder,
            Action<string> onSuccess,
            Action originalOnFailed )
        //---------------------------------//
        {
            string url = textHolder.path;

            WWW www = new WWW( url );

            while( !www.isDone )
            {
                yield return www;
            }

            if( string.IsNullOrEmpty( www.error ) )
            {
                if( www.text != null )
                {
                    textHolder.text = www.text;

                    if( textHolder.cacheIfWeb )
                    {
                        File.WriteAllBytes( Application.persistentDataPath + "/" + textHolder.path, www.bytes );
                    }

                    if( onSuccess != null ) { onSuccess( textHolder.text ); }
                }
                else
                {
                    if( originalOnFailed != null ) { originalOnFailed(); }
                    else if( originalOnFailed != null ) { originalOnFailed.Invoke(); }
                }
            }
            else
            {
                if( originalOnFailed != null ) { originalOnFailed(); }
                else if( originalOnFailed != null ) { originalOnFailed.Invoke(); }
            }

        } //END GetTextFromWeb
        #endregion

        #region GET CSV FILE FROM LOCAL OR WEB
        //----------------------------------//
        private class CSVHolder
        //----------------------------------//
        {

            public string fileName = "";
            public string csvText = "";
            public bool cacheIfWeb = false;
            public string path = "";

        } //END TextHolder

        //---------------------------------//
        public void GetCSV( string path, bool cacheIfWeb, Action<string> onSuccess, Action onFailed, LocationType downloadLocation = LocationType.TryAllLocations, string fileName = "" )
        //---------------------------------//
        {
            if( showDebug ) { Debug.Log( "WWWHelper.cs GetCSV() path = " + path + ", cacheIfWeb = " + cacheIfWeb + ", onSuccess = " + onSuccess + ", onFailed = " + onFailed ); }

            CSVHolder csvHolder = new CSVHolder();
            csvHolder.fileName = fileName;
            csvHolder.path = path;
            csvHolder.cacheIfWeb = cacheIfWeb;

            if( downloadLocation == LocationType.TryAllLocations )
            {
                //Check Streaming Assets first, then Resources, then PersistentDataPath, then Web... if none of them have the file, then it probably doesn't exist
                StartCoroutine( GetCSVFromStreamingAssets( csvHolder, onSuccess, onFailed, GetCSV_StreamingAssets_Failed ) );
            }
            else if( downloadLocation == LocationType.StreamingAssets )
            {
                StartCoroutine( GetCSVFromStreamingAssets( csvHolder, onSuccess, onFailed, null ) );
            }
            else if( downloadLocation == LocationType.Resources )
            {
                StartCoroutine( GetCSVFromResources( csvHolder, onSuccess, onFailed, null ) );
            }
            else if( downloadLocation == LocationType.PersistentDataPath )
            {
                StartCoroutine( GetCSVFromPersistentData( csvHolder, onSuccess, onFailed, null ) );
            }
            else if( downloadLocation == LocationType.Web )
            {
                StartCoroutine( GetCSVFromWeb( csvHolder, onSuccess, onFailed ) );
            }
            

        } //END GetCSV
        
        //----------------------------------//
        private void GetCSV_StreamingAssets_Failed( CSVHolder csvHolder, Action<string> onSuccess, Action originalOnFailed )
        //----------------------------------//
        {
            //if( showDebugLog ) { Debug.Log( "WWWHelper.cs GetCSV_StreamingAssets_Failed() path = " + csvHolder.path + ", cacheIfWeb = " + textHolder.cacheIfWeb + ", onSuccess = " + onSuccess + ", onFailed = " + originalOnFailed ); }

            StartCoroutine( GetCSVFromResources( csvHolder, onSuccess, originalOnFailed, GetCSV_Resources_Failed ) );

        } //END GetCSV_StreamingAssets_Failed

        //----------------------------------//
        private void GetCSV_Resources_Failed( CSVHolder csvHolder, Action<string> onSuccess, Action originalOnFailed )
        //----------------------------------//
        {
            //if( showDebugLog ) { Debug.Log( "WWWHelper.cs GetCSV_Resources_Failed() path = " + csvHolder.path + ", cacheIfWeb = " + csvHolder.cacheIfWeb + ", onSuccess = " + onSuccess + ", onFailed = " + originalOnFailed ); }

            StartCoroutine( GetCSVFromPersistentData( csvHolder, onSuccess, originalOnFailed, GetCSV_PersistentData_Failed ) );

        } //END GetCSV_Resources_Failed

        //----------------------------------//
        private void GetCSV_PersistentData_Failed( CSVHolder csvHolder, Action<string> onSuccess, Action originalOnFailed )
        //----------------------------------//
        {
            //if( showDebugLog ) { Debug.Log( "WWWHelper.cs GetCSV_PersistentData_Failed() path = " + csvHolder.path + ", cacheIfWeb = " + csvHolder.cacheIfWeb + ", onSuccess = " + onSuccess + ", onFailed = " + originalOnFailed ); }

            StartCoroutine( GetCSVFromWeb( csvHolder, onSuccess, originalOnFailed ) );

        } //END GetCSV_PersistentData_Failed


        //---------------------------------//
        private IEnumerator GetCSVFromStreamingAssets
        ( CSVHolder csvHolder,
            Action<string> onSuccess,
            Action originalOnFailed,
            Action<CSVHolder, Action<string>, Action> onFailed )
        //---------------------------------//
        {
            string streamingAssetsPath = csvHolder.path;

            WWW www;

            string existsPath = DatabaseStringHelper.CreateStreamingAssetsPathForFileExistsCheck( streamingAssetsPath );

            bool exists = false;

            //Check if the texture exists, this check is different by platform
            if( RequireWWWForFileExistsCheck() )
            {
                //Android can only check using www
                www = new WWW( existsPath );

                yield return www;

                exists = string.IsNullOrEmpty( www.error );
            }
            else
            {
                //All other platforms can check using System.IO
                exists = System.IO.File.Exists( existsPath );
            }

            //We couldn't locate the file, let's check again with a .csv appended to the file name and try each option
            if( !exists )
            {
                string csvPath = existsPath + ".csv";

                //Check .csv
                if( RequireWWWForFileExistsCheck() )
                {
                    //Android can only check using www
                    www = new WWW( csvPath );

                    yield return www;

                    exists = string.IsNullOrEmpty( www.error );
                }
                else
                {
                    //All other platforms can check using System.IO
                    exists = System.IO.File.Exists( csvPath );
                }

                if( exists )
                {
                    streamingAssetsPath += ".csv";
                    if( showDebug ) { Debug.Log( "WWWHelper.cs GetCSVFromStreamingAssets() found asset using appended .csv to file name! csvPath = " + csvPath ); }
                }
                else
                {
                    if( showDebug ) { Debug.Log( "WWWHelper.cs GetCSVFromStreamingAssets() unable to find asset even with appended .csv to file name! csvPath = " + csvPath ); }
                }
            }

            if( exists )
            {
                string path = DatabaseStringHelper.CreateStreamingAssetsPath( streamingAssetsPath, DatabaseStringHelper.StringStyle.WithEscapeUriAndSystemPathCombine );

                //Download the bytes from the StreamingAssets folder
                www = new WWW( path );

                //Continue with the main thread until this finishes
                while( !www.isDone )
                {
                    yield return www;
                }

                csvHolder.csvText = www.text;

                if( csvHolder.csvText != null )
                {
                    if( showDebug ) Debug.Log( "WWWHelper.cs GetCSVFromStreamingAssets() file exists, path = " + path );

                    if( onSuccess != null ) { onSuccess( csvHolder.csvText ); }
                }
                else
                {
                    if( showDebug ) Debug.Log( "WWWHelper.cs GetCSVFromStreamingAssets() file exists, path = " + path + ", however unable to successfully use www.text" );

                    if( onFailed != null ) { onFailed( csvHolder, onSuccess, originalOnFailed ); }
                    else if( originalOnFailed != null ) { originalOnFailed.Invoke(); }
                }

            }
            else
            {
                if( showDebug ) Debug.Log( "WWWHelper.cs GetCSVFromStreamingAssets() file does not exist, path = " + streamingAssetsPath );

                if( onFailed != null ) { onFailed( csvHolder, onSuccess, originalOnFailed ); }
                else if( originalOnFailed != null ) { originalOnFailed.Invoke(); }
            }

        } //END GetCSVFromStreamingAssets

        //---------------------------------//
        private IEnumerator GetCSVFromResources
        ( CSVHolder csvHolder,
            Action<string> onSuccess,
            Action originalOnFailed,
            Action<CSVHolder, Action<string>, Action> onFailed )
        //---------------------------------//
        {
            string resourcesPath = System.IO.Path.ChangeExtension( csvHolder.path, null );

            ResourceRequest request = Resources.LoadAsync<TextAsset>( resourcesPath );

            while( !request.isDone )
            {
                yield return request;
            }

            if( request.asset != null )
            {
                if( showDebug ) Debug.Log( "WWWHelper.cs GetCSVFromResources() Successfully found resources asset at path = " + resourcesPath );

                TextAsset textAsset = request.asset as TextAsset;
                csvHolder.csvText = textAsset.text;

                if( onSuccess != null ) { onSuccess( csvHolder.csvText ); }
            }
            else
            {
                if( showDebug ) Debug.Log( "WWWHelper.cs GetCSVFromResources() Unable to find resources asset at path = " + resourcesPath );

                if( onFailed != null ) { onFailed( csvHolder, onSuccess, originalOnFailed ); }
                else if( originalOnFailed != null ) { originalOnFailed.Invoke(); }
            }

        } //END GetCSVFromResources

        //---------------------------------//
        private IEnumerator GetCSVFromPersistentData
        ( CSVHolder csvHolder,
            Action<string> onSuccess,
            Action originalOnFailed,
            Action<CSVHolder, Action<string>, Action> onFailed )
        //---------------------------------//
        {
            if( showDebug ) { Debug.Log( "WWWHelper.cs GetCSVFromPersistentData() path = " + csvHolder.path + ", cacheIfWeb = " + csvHolder.cacheIfWeb + ", onSuccess = " + onSuccess + ", onFailed = " + originalOnFailed ); }

            string path = DatabaseStringHelper.CreatePersistentDataPath( csvHolder.path );

            WWW www = new WWW( path );

            while( !www.isDone )
            {
                yield return www;
            }

            if( string.IsNullOrEmpty( www.error ) )
            {
                if( www.text != null )
                {
                    csvHolder.csvText = www.text;

                    if( onSuccess != null ) { onSuccess( csvHolder.csvText ); }
                }
                else
                {
                    if( onFailed != null ) { onFailed( csvHolder, onSuccess, originalOnFailed ); }
                    else if( originalOnFailed != null ) { originalOnFailed.Invoke(); }
                }
            }
            else
            {
                if( onFailed != null ) { onFailed( csvHolder, onSuccess, originalOnFailed ); }
                else if( originalOnFailed != null ) { originalOnFailed.Invoke(); }
            }

        } //END GetCSVFromPersistentData

        //---------------------------------//
        private IEnumerator GetCSVFromWeb
        ( CSVHolder csvHolder,
            Action<string> onSuccess,
            Action originalOnFailed )
        //---------------------------------//
        {
            string url = csvHolder.path;

            WWW www = new WWW( url );

            while( !www.isDone )
            {
                yield return www;
            }

            if( string.IsNullOrEmpty( www.error ) )
            {
                if( www.text != null )
                {
                    csvHolder.csvText = www.text;

                    if( csvHolder.cacheIfWeb )
                    {
                        if( csvHolder.fileName != "" )
                        {
                            File.WriteAllText( Application.persistentDataPath + "/" + csvHolder.fileName, www.text );
                        }
                        else
                        {
                            //Use the file path as the name
                            File.WriteAllText( Application.persistentDataPath + "/" + csvHolder.path, www.text );
                        }
                    }

                    if( onSuccess != null ) { onSuccess( csvHolder.csvText ); }
                }
                else
                {
                    if( originalOnFailed != null ) { originalOnFailed(); }
                }
            }
            else
            {
                if( originalOnFailed != null ) { originalOnFailed(); }
            }

        } //END GetCSVFromWeb
        #endregion

        #region GET TEXTURE FROM LOCAL OR WEB
        //---------------------------------//
        private class TextureHolder
        //---------------------------------//
        {
            public string fileName = "";
            public Texture texture = null;
            public bool cacheIfWeb = false;
            public string path = "";

        } //END TextureHolder

        //---------------------------------//
        public void GetTexture( string path, bool cacheIfWeb, Action<Texture> onSuccess, Action onFailed, LocationType downloadLocation = LocationType.TryAllLocations, string fileName = "" )
        //---------------------------------//
        {
            if( showDebug ) { Debug.Log( "WWWHelper.cs GetTexture() path = " + path + ", cacheIfWeb = " + cacheIfWeb + ", onSuccess = " + onSuccess + ", onFailed = " + onFailed ); }

            TextureHolder textureHolder = new TextureHolder();
            textureHolder.fileName = fileName;
            textureHolder.path = path;
            textureHolder.cacheIfWeb = cacheIfWeb;

            if( downloadLocation == LocationType.TryAllLocations )
            {
                //Check Streaming Assets first, then Resources, then PersistentDataPath, then Web... if none of them have the file, then it probably doesn't exist
                StartCoroutine( GetTextureFromStreamingAssets( textureHolder, onSuccess, onFailed, GetTexture_StreamingAssets_Failed ) );
            }
            else if( downloadLocation == LocationType.StreamingAssets )
            {
                StartCoroutine( GetTextureFromStreamingAssets( textureHolder, onSuccess, onFailed, null ) );
            }
            else if( downloadLocation == LocationType.Resources )
            {
                StartCoroutine( GetTextureFromResources( textureHolder, onSuccess, onFailed, null ) );
            }
            else if( downloadLocation == LocationType.PersistentDataPath )
            {
                StartCoroutine( GetTextureFromPersistentData( textureHolder, onSuccess, onFailed, null ) );
            }
            else if( downloadLocation == LocationType.Web )
            {
                StartCoroutine( GetTextureFromWeb( textureHolder, onSuccess, onFailed ) );
            }
            
        } //END GetTexture

        //----------------------------------//
        private void GetTexture_StreamingAssets_Failed( TextureHolder textureHolder, Action<Texture> onSuccess, Action originalOnFailed )
        //----------------------------------//
        {
            if( showDebug ) { Debug.Log( "WWWHelper.cs GetTexture_StreamingAssets_Failed() path = " + textureHolder.path + ", cacheIfWeb = " + textureHolder.cacheIfWeb + ", onSuccess = " + onSuccess + ", onFailed = " + originalOnFailed ); }

            StartCoroutine( GetTextureFromResources( textureHolder, onSuccess, originalOnFailed, GetTexture_Resources_Failed ) );

        } //END GetTexture_StreamingAssets_Failed

        //----------------------------------//
        private void GetTexture_Resources_Failed( TextureHolder textureHolder, Action<Texture> onSuccess, Action originalOnFailed )
        //----------------------------------//
        {
            if( showDebug ) { Debug.Log( "WWWHelper.cs GetTexture_Resources_Failed() path = " + textureHolder.path + ", cacheIfWeb = " + textureHolder.cacheIfWeb + ", onSuccess = " + onSuccess + ", onFailed = " + originalOnFailed ); }

            StartCoroutine( GetTextureFromPersistentData( textureHolder, onSuccess, originalOnFailed, GetTexture_PersistentData_Failed ) );

        } //END GetTexture_Resources_Failed

        //----------------------------------//
        private void GetTexture_PersistentData_Failed( TextureHolder textureHolder, Action<Texture> onSuccess, Action originalOnFailed )
        //----------------------------------//
        {
            if( showDebug ) { Debug.Log( "WWWHelper.cs GetTexture_PersistentData_Failed() path = " + textureHolder.path + ", cacheIfWeb = " + textureHolder.cacheIfWeb + ", onSuccess = " + onSuccess + ", onFailed = " + originalOnFailed ); }

            StartCoroutine( GetTextureFromWeb( textureHolder, onSuccess, originalOnFailed ) );

        } //END GetTexture_PersistentData_Failed





        //---------------------------------//
        private IEnumerator GetTextureFromStreamingAssets
        ( TextureHolder textureHolder,
            Action<Texture> onSuccess,
            Action originalOnFailed,
            Action<TextureHolder, Action<Texture>, Action> onFailed )
        //---------------------------------//
        {
            string streamingAssetsPath = textureHolder.path;
            string originalName = textureHolder.path;

            WWW www;

            string existsPath = DatabaseStringHelper.CreateStreamingAssetsPathForFileExistsCheck( streamingAssetsPath );

            bool exists = false;

            //Check if the texture exists, this check is different by platform
            if( RequireWWWForFileExistsCheck() )
            {
                //Android can only check using www
                www = new WWW( existsPath );

                yield return www;

                exists = string.IsNullOrEmpty( www.error );
            }
            else
            {
                //All other platforms can check using System.IO
                exists = System.IO.File.Exists( existsPath );
            }

            //We couldn't locate the file, let's check again with a .png, .jpg, .gif appended to the file name and try each option
            if( !exists )
            {
                string pngPath = existsPath + ".png";

                //Check .png
                if( RequireWWWForFileExistsCheck() )
                {
                    //Android can only check using www
                    www = new WWW( pngPath );

                    yield return www;

                    exists = string.IsNullOrEmpty( www.error );
                }
                else
                {
                    //All other platforms can check using System.IO
                    exists = System.IO.File.Exists( pngPath );
                }

                if( exists )
                {
                    streamingAssetsPath += ".png";
                }
                else
                {
                    //Try again with .jpg
                    string jpgPath = existsPath + ".jpg";

                    //Check .wav
                    if( RequireWWWForFileExistsCheck() )
                    {
                        //Android can only check using www
                        www = new WWW( jpgPath );

                        yield return www;

                        exists = string.IsNullOrEmpty( www.error );
                    }
                    else
                    {
                        //All other platforms can check using System.IO
                        exists = System.IO.File.Exists( jpgPath );
                    }

                    if( exists )
                    {
                        streamingAssetsPath += ".jpg";
                    }
                    else
                    {
                        //Try again with .gif
                        string gifPath = existsPath + ".gif";

                        //Check .wav
                        if( RequireWWWForFileExistsCheck() )
                        {
                            //Android can only check using www
                            www = new WWW( gifPath );

                            yield return www;

                            exists = string.IsNullOrEmpty( www.error );
                        }
                        else
                        {
                            //All other platforms can check using System.IO
                            exists = System.IO.File.Exists( gifPath );
                        }

                        if( exists )
                        {
                            streamingAssetsPath += ".gif";
                        }
                    }
                }
            }

            if( exists )
            {
                string path = DatabaseStringHelper.CreateStreamingAssetsPath( streamingAssetsPath, DatabaseStringHelper.StringStyle.WithEscapeUriAndSystemPathCombine );

                //Download the bytes from the StreamingAssets folder
                www = new WWW( path );

                //Continue with the main thread until this finishes
                while( !www.isDone )
                {
                    yield return www;
                }

                textureHolder.texture = www.textureNonReadable;
                textureHolder.texture.name = originalName;

                if( textureHolder.texture != null )
                {
                    if( showDebug ) Debug.Log( "WWWHelper.cs GetTextureFromStreamingAssets() file exists, path = " + path );

                    if( onSuccess != null ) { onSuccess( textureHolder.texture ); }
                }
                else
                {
                    if( showDebug ) Debug.Log( "WWWHelper.cs GetTextureFromStreamingAssets() file exists, path = " + path + ", however unable to successfully use www.GetTexture" );

                    if( onFailed != null ) { onFailed( textureHolder, onSuccess, originalOnFailed ); }
                    else if( originalOnFailed != null ) { originalOnFailed.Invoke(); }
                }

            }
            else
            {
                if( showDebug ) Debug.Log( "WWWHelper.cs GetTextureFromStreamingAssets() file does not exist, path = " + streamingAssetsPath );

                if( onFailed != null ) { onFailed( textureHolder, onSuccess, originalOnFailed ); }
                else if( originalOnFailed != null ) { originalOnFailed.Invoke(); }
            }

        } //END GetTextureFromStreamingAssets


        //---------------------------------//
        private IEnumerator GetTextureFromResources
        ( TextureHolder textureHolder,
            Action<Texture> onSuccess,
            Action originalOnFailed,
            Action<TextureHolder, Action<Texture>, Action> onFailed )
        //---------------------------------//
        {
            string resourcesPath = System.IO.Path.ChangeExtension( textureHolder.path, null );

            ResourceRequest request = Resources.LoadAsync<Texture>( resourcesPath );

            while( !request.isDone )
            {
                yield return request;
            }

            if( request.asset != null )
            {
                if( showDebug ) Debug.Log( "WWWHelper.cs GetTextureFromResources() Successfully found resources asset at path = " + resourcesPath );

                textureHolder.texture = request.asset as Texture;
                textureHolder.texture.name = resourcesPath;

                if( onSuccess != null ) { onSuccess( textureHolder.texture ); }
            }
            else
            {
                if( showDebug ) Debug.Log( "WWWHelper.cs GetTextureFromResources() Unable to find resources asset at path = " + resourcesPath );

                if( onFailed != null ) { onFailed( textureHolder, onSuccess, originalOnFailed ); }
                else if( originalOnFailed != null ) { originalOnFailed.Invoke(); }
            }

        } //END GetTextureFromResources

        //---------------------------------//
        private IEnumerator GetTextureFromPersistentData
        ( TextureHolder textureHolder,
            Action<Texture> onSuccess,
            Action originalOnFailed,
            Action<TextureHolder, Action<Texture>, Action> onFailed )
        //---------------------------------//
        {
            if( showDebug ) { Debug.Log( "WWWHelper.cs GetTextureFromPersistentData() path = " + textureHolder.path + ", cacheIfWeb = " + textureHolder.cacheIfWeb + ", onSuccess = " + onSuccess + ", onFailed = " + originalOnFailed ); }

            string path = DatabaseStringHelper.CreatePersistentDataPath( textureHolder.path );

            WWW www = new WWW( path );

            while( !www.isDone )
            {
                yield return www;
            }

            if( string.IsNullOrEmpty( www.error ) )
            {
                if( www.textureNonReadable != null )
                {
                    textureHolder.texture = www.textureNonReadable;
                    textureHolder.texture.name = path;

                    if( onSuccess != null ) { onSuccess( textureHolder.texture ); }
                }
                else
                {
                    if( onFailed != null ) { onFailed( textureHolder, onSuccess, originalOnFailed ); }
                    else if( originalOnFailed != null ) { originalOnFailed.Invoke(); }
                }
            }
            else
            {
                if( onFailed != null ) { onFailed( textureHolder, onSuccess, originalOnFailed ); }
                else if( originalOnFailed != null ) { originalOnFailed.Invoke(); }
            }

        } //END GetTextureFromPersistentData


        //---------------------------------//
        private IEnumerator GetTextureFromWeb
        ( TextureHolder textureHolder,
            Action<Texture> onSuccess,
            Action originalOnFailed )
        //---------------------------------//
        {
            string url = textureHolder.path;

            WWW www = new WWW( url );

            while( !www.isDone )
            {
                yield return www;
            }

            if( string.IsNullOrEmpty( www.error ) )
            {
                if( www.textureNonReadable != null )
                {
                    textureHolder.texture = www.textureNonReadable;
                    textureHolder.texture.name = url;

                    if( textureHolder.cacheIfWeb )
                    {
                        File.WriteAllBytes( Application.persistentDataPath + "/" + textureHolder.path, www.bytes );
                    }

                    if( onSuccess != null ) { onSuccess( textureHolder.texture ); }
                }
                else
                {
                    if( originalOnFailed != null ) { originalOnFailed(); }
                }
            }
            else
            {
                if( originalOnFailed != null ) { originalOnFailed(); }
            }

        } //END GetTextureFromWeb
        #endregion

        #region GET AUDIOCLIP FROM LOCAL OR WEB
        //---------------------------------//
        private class AudioClipHolder
        //---------------------------------//
        {
            public string fileName = "";
            public AudioClip audioClip = null;
            public bool cacheIfWeb = false;
            public string path = "";

        } //END AudioClipHolder

        //---------------------------------//
        public void GetAudioClip( string path, bool cacheIfWeb, Action<AudioClip> onSuccess, Action onFailed, LocationType downloadLocation = LocationType.TryAllLocations, string fileName = "" )
        //---------------------------------//
        {
            //if( showDebugLog ) { Debug.Log( "WWWHelper.cs GetAudioClip() path = " + path + ", cacheIfWeb = " + cacheIfWeb + ", onSuccess = " + onSuccess + ", onFailed = " + onFailed ); }

            AudioClipHolder audioClipHolder = new AudioClipHolder();
            audioClipHolder.fileName = fileName;
            audioClipHolder.path = path;
            audioClipHolder.cacheIfWeb = cacheIfWeb;

            if( downloadLocation == LocationType.TryAllLocations )
            {
                //Check Streaming Assets first, then Resources, then PersistentDataPath, then Web... if none of them have the file, then it probably doesn't exist
                StartCoroutine( GetAudioClipFromStreamingAssets( audioClipHolder, onSuccess, onFailed, GetAudioClip_StreamingAssets_Failed ) );
            }
            else if( downloadLocation == LocationType.StreamingAssets )
            {
                StartCoroutine( GetAudioClipFromStreamingAssets( audioClipHolder, onSuccess, onFailed, null ) );
            }
            else if( downloadLocation == LocationType.Resources )
            {
                StartCoroutine( GetAudioClipFromResources( audioClipHolder, onSuccess, onFailed, null ) );
            }
            else if( downloadLocation == LocationType.PersistentDataPath )
            {
                StartCoroutine( GetAudioClipFromPersistentData( audioClipHolder, onSuccess, onFailed, null ) );
            }
            else if( downloadLocation == LocationType.Web )
            {
                StartCoroutine( GetAudioClipFromWeb( audioClipHolder, onSuccess, onFailed ) );
            }
            
        } //END GetAudioClip

        //----------------------------------//
        private void GetAudioClip_StreamingAssets_Failed( AudioClipHolder audioClipHolder, Action<AudioClip> onSuccess, Action originalOnFailed )
        //----------------------------------//
        {
            //if( showDebugLog ) { Debug.Log( "WWWHelper.cs GetAudioClip_StreamingAssets_Failed() path = " + audioClipHolder.path + ", cacheIfWeb = " + audioClipHolder.cacheIfWeb + ", onSuccess = " + onSuccess + ", onFailed = " + originalOnFailed ); }

            StartCoroutine( GetAudioClipFromResources( audioClipHolder, onSuccess, originalOnFailed, GetAudioClip_Resources_Failed ) );

        } //END GetAudioClip_StreamingAssets_Failed

        //----------------------------------//
        private void GetAudioClip_Resources_Failed( AudioClipHolder audioClipHolder, Action<AudioClip> onSuccess, Action originalOnFailed )
        //----------------------------------//
        {
            //if( showDebugLog ) { Debug.Log( "WWWHelper.cs GetAudioClip_Resources_Failed() path = " + audioClipHolder.path + ", cacheIfWeb = " + audioClipHolder.cacheIfWeb + ", onSuccess = " + onSuccess + ", onFailed = " + originalOnFailed ); }

            StartCoroutine( GetAudioClipFromPersistentData( audioClipHolder, onSuccess, originalOnFailed, GetAudioClip_PersistentData_Failed ) );

        } //END GetAudioClip_Resources_Failed

        //----------------------------------//
        private void GetAudioClip_PersistentData_Failed( AudioClipHolder audioClipHolder, Action<AudioClip> onSuccess, Action originalOnFailed )
        //----------------------------------//
        {
            if( showDebug ) { Debug.Log( "WWWHelper.cs GetAudioClip_PersistentData_Failed() path = " + audioClipHolder.path + ", cacheIfWeb = " + audioClipHolder.cacheIfWeb + ", onSuccess = " + onSuccess + ", onFailed = " + originalOnFailed ); }

            StartCoroutine( GetAudioClipFromWeb( audioClipHolder, onSuccess, originalOnFailed ) );

        } //END GetAudioClipHolder_PersistentData_Failed






        //---------------------------------//
        private IEnumerator GetAudioClipFromStreamingAssets
        ( AudioClipHolder audioClipHolder,
            Action<AudioClip> onSuccess,
            Action originalOnFailed,
            Action<AudioClipHolder, Action<AudioClip>, Action> onFailed )
        //---------------------------------//
        {
            //if( showDebugLog ) { Debug.Log( "WWWHelper.cs GetAudioClipFromStreamingAssets() path = " + audioClipHolder.path + ", cacheIfWeb = " + audioClipHolder.cacheIfWeb + ", onSuccess = " + onSuccess + ", onFailed = " + originalOnFailed ); }

            string streamingAssetsPath = audioClipHolder.path;
            string originalFileName = streamingAssetsPath;

            WWW www;

            string existsPath = DatabaseStringHelper.CreateStreamingAssetsPathForFileExistsCheck( streamingAssetsPath );

            bool exists = false;

            //Check if the audio exists, this check is different by platform
            if( RequireWWWForFileExistsCheck() )
            {
                //Android can only check using www
                www = new WWW( existsPath );

                yield return www;

                exists = string.IsNullOrEmpty( www.error );
            }
            else
            {
                //All other platforms can check using System.IO
                exists = System.IO.File.Exists( existsPath );
            }

            //We couldn't locate the file, let's check again with a .wav, .mp3, .ogg appended to the file name and try each option
            if( !exists )
            {
                string wavPath = existsPath + ".wav";

                //Check .wav
                if( RequireWWWForFileExistsCheck() )
                {
                    //Android can only check using www
                    www = new WWW( wavPath );

                    yield return www;

                    exists = string.IsNullOrEmpty( www.error );
                }
                else
                {
                    //All other platforms can check using System.IO
                    exists = System.IO.File.Exists( wavPath );
                }

                if( exists )
                {
                    streamingAssetsPath += ".wav";
                }
                else
                {
                    //Try again with .mp3
                    string mp3Path = existsPath + ".mp3";

                    //Check .wav
                    if( RequireWWWForFileExistsCheck() )
                    {
                        //Android can only check using www
                        www = new WWW( mp3Path );

                        yield return www;

                        exists = string.IsNullOrEmpty( www.error );
                    }
                    else
                    {
                        //All other platforms can check using System.IO
                        exists = System.IO.File.Exists( mp3Path );
                    }

                    if( exists )
                    {
                        streamingAssetsPath += ".mp3";
                    }
                    else
                    {
                        //Try again with .ogg
                        string oggPath = existsPath + ".ogg";

                        //Check .wav
                        if( RequireWWWForFileExistsCheck() )
                        {
                            //Android can only check using www
                            www = new WWW( oggPath );

                            yield return www;

                            exists = string.IsNullOrEmpty( www.error );
                        }
                        else
                        {
                            //All other platforms can check using System.IO
                            exists = System.IO.File.Exists( oggPath );
                        }

                        if( exists )
                        {
                            streamingAssetsPath += ".ogg";
                        }
                    }
                }
            }

            if( exists )
            {
                string path = DatabaseStringHelper.CreateStreamingAssetsPath( streamingAssetsPath, DatabaseStringHelper.StringStyle.WithEscapeUriAndSystemPathCombine );

                //Download the bytes from the StreamingAssets folder
                www = new WWW( path );

                //Continue with the main thread until this finishes
                while( !www.isDone )
                {
                    yield return www;
                }

                audioClipHolder.audioClip = www.GetAudioClipCompressed();
                audioClipHolder.audioClip.name = originalFileName;

                if( audioClipHolder.audioClip != null )
                {
                    if( showDebug ) Debug.Log( "WWWHelper.cs GetClipFromStreamingAssets() file exists, path = " + path );

                    if( onSuccess != null ) { onSuccess( audioClipHolder.audioClip ); }
                }
                else
                {
                    if( showDebug ) Debug.Log( "WWWHelper.cs GetClipFromStreamingAssets() file exists, path = " + path + ", however unable to successfully use www.GetAudioClipCompressed" );

                    if( onFailed != null ) { onFailed( audioClipHolder, onSuccess, originalOnFailed ); }
                    else if( originalOnFailed != null ) { originalOnFailed.Invoke(); }
                }

            }
            else
            {
                if( showDebug ) Debug.Log( "WWWHelper.cs GetClipFromStreamingAssets() file does not exist, path = " + streamingAssetsPath );

                if( onFailed != null ) { onFailed( audioClipHolder, onSuccess, originalOnFailed ); }
                else if( originalOnFailed != null ) { originalOnFailed.Invoke(); }
            }

        } //END GetAudioClipFromStreamingAssets



        //---------------------------------//
        private IEnumerator GetAudioClipFromResources
        ( AudioClipHolder audioClipHolder,
            Action<AudioClip> onSuccess,
            Action originalOnFailed,
            Action<AudioClipHolder, Action<AudioClip>, Action> onFailed )
        //---------------------------------//
        {
            //if( showDebugLog ) { Debug.Log( "WWWHelper.cs GetAudioClipFromResources() path = " + audioClipHolder.path + ", cacheIfWeb = " + audioClipHolder.cacheIfWeb + ", onSuccess = " + onSuccess + ", onFailed = " + originalOnFailed ); }

            string resourcesPath = System.IO.Path.ChangeExtension( audioClipHolder.path, null );

            ResourceRequest request = Resources.LoadAsync<AudioClip>( resourcesPath );

            while( !request.isDone )
            {
                yield return request;
            }

            if( request.asset != null )
            {
                if( showDebug ) Debug.Log( "WWWHelper.cs GetAudioClipFromResources() Successfully found resources asset at path = " + resourcesPath );

                audioClipHolder.audioClip = request.asset as AudioClip;
                audioClipHolder.audioClip.name = resourcesPath;

                if( onSuccess != null ) { onSuccess( audioClipHolder.audioClip ); }
            }
            else
            {
                if( showDebug ) Debug.Log( "WWWHelper.cs GetAudioClipFromResources() Unable to find resources asset at path = " + resourcesPath );

                if( onFailed != null ) { onFailed( audioClipHolder, onSuccess, originalOnFailed ); }
                else if( originalOnFailed != null ) { originalOnFailed.Invoke(); }
            }

        } //END GetAudioClipFromResources

        //---------------------------------//
        private IEnumerator GetAudioClipFromPersistentData
        ( AudioClipHolder audioClipHolder,
            Action<AudioClip> onSuccess,
            Action originalOnFailed,
            Action<AudioClipHolder, Action<AudioClip>, Action> onFailed )
        //---------------------------------//
        {
            //if( showDebugLog ) { Debug.Log( "WWWHelper.cs GetAudioClipFromPersistentData() path = " + audioClipHolder.path + ", cacheIfWeb = " + audioClipHolder.cacheIfWeb + ", onSuccess = " + onSuccess + ", onFailed = " + originalOnFailed ); }

            string path = DatabaseStringHelper.CreatePersistentDataPath( audioClipHolder.path );

            WWW www = new WWW( path );

            while( !www.isDone )
            {
                yield return www;
            }

            if( string.IsNullOrEmpty( www.error ) )
            {
                if( www.GetAudioClip() != null )
                {
                    audioClipHolder.audioClip = www.GetAudioClipCompressed();
                    audioClipHolder.audioClip.name = path;

                    if( onSuccess != null ) { onSuccess( audioClipHolder.audioClip ); }
                }
                else
                {
                    if( onFailed != null ) { onFailed( audioClipHolder, onSuccess, originalOnFailed ); }
                    else if( originalOnFailed != null ) { originalOnFailed.Invoke(); }
                }
            }
            else
            {
                if( onFailed != null ) { onFailed( audioClipHolder, onSuccess, originalOnFailed ); }
                else if( originalOnFailed != null ) { originalOnFailed.Invoke(); }
            }

        } //END GetAudioClipFromPersistentData

        //---------------------------------//
        private IEnumerator GetAudioClipFromWeb
        ( AudioClipHolder audioClipHolder,
            Action<AudioClip> onSuccess,
            Action originalOnFailed )
        //---------------------------------//
        {
            //if( showDebugLog ) { Debug.Log( "WWWHelper.cs GetAudioClipFromWeb() path = " + audioClipHolder.path + ", cacheIfWeb = " + audioClipHolder.cacheIfWeb + ", onSuccess = " + onSuccess + ", onFailed = " + originalOnFailed ); }

            WWW www = new WWW( audioClipHolder.path.Trim() );

            while( !www.isDone )
            {
                yield return www;
            }

            if( string.IsNullOrEmpty( www.error ) )
            {
                if( www.GetAudioClip() != null )
                {
                    if( showDebug ) { Debug.Log( "WWWHelper.cs GetAudioClipFromWeb() Successfully found AudioClip from path!" ); }

                    audioClipHolder.audioClip = www.GetAudioClipCompressed();
                    audioClipHolder.audioClip.name = audioClipHolder.path;

                    if( audioClipHolder.cacheIfWeb )
                    {
                        if( showDebug ) { Debug.Log( "WWWHelper.cs GetAudioClipFromWeb() Caching audioClip" ); }

                        File.WriteAllBytes( DatabaseStringHelper.CreatePersistentDataPathFilenameFromURL( DatabaseStringHelper.GenerateNameFromUrl( audioClipHolder.path ) ), www.bytes );

                        //ES2.Save<AudioClip>( audioClipHolder.audioClip, DatabaseStringHelper.GenerateNameFromUrl( audioClipHolder.path ).Trim() );
                    }

                    if( onSuccess != null )
                    {
                        if( showDebug ) { Debug.Log( "WWWHelper.cs GetAudioClipFromWeb() Calling onSuccess() and passing in audioClip ... " + audioClipHolder.audioClip ); }

                        onSuccess( audioClipHolder.audioClip );
                    }
                }
                else
                {
                    //if( showDebugLog ) { Debug.Log( "WWWHelper.cs GetAudioClipFromWeb() found data that wasn't an error, however we were unable to turn it into an audioClip via www.audioClip" ); }

                    if( originalOnFailed != null ) { originalOnFailed(); }
                }
            }
            else
            {
                //if( showDebugLog ) { Debug.Log( "WWWHelper.cs GetAudioClipFromWeb() Failed to find audioClip, error = " + www.error ); }

                if( originalOnFailed != null ) { originalOnFailed(); }
            }

        } //END GetAudioClipFromWeb
        #endregion

        #region GET ASSET BUNDLE

        public enum AssetBundleVersionType
        {
            none,
            integer,
            hash
        }

        [Serializable]
        //--------------------------------------//
        public class AssetBundleInfo
        //--------------------------------------//
        {
            public string name = "";
            public string path = "";

            public AssetBundleVersionType versionType = AssetBundleVersionType.none;
            public uint versionNumber = new uint();
            public string versionHash = "";
            
            public uint checksum = new uint();

            public float progress = 0f;
            public AssetBundle assetBundle;
            
            public List<AssetBundleRequest> assetBundleRequests = null;

            public string cachePath = "";

            public GameObject instantiatedGameObject;
            
        } //END AssetBundleInfo

        //----------------------------------//
        public void DownloadAssetBundle( AssetBundleInfo assetBundleInfo, Action<AssetBundleInfo> onSuccess, Action<AssetBundleInfo> onFailed, Action<AssetBundleInfo> onProgress )
        //----------------------------------//
        {

            if( assetBundleInfo != null )
            {
                StartCoroutine( DownloadAndCacheAssetBundle( assetBundleInfo, onSuccess, onFailed, onProgress ) );
            }

        } //END DownloadAssetBundle

        //----------------------------------//
        IEnumerator DownloadAndCacheAssetBundle( AssetBundleInfo assetBundleInfo, Action<AssetBundleInfo> onSuccess, Action<AssetBundleInfo> onFailed, Action<AssetBundleInfo> onProgress )
        //----------------------------------//
        {

            //---------------- Update our asset bundle path if it contains keywords to certain Unity folders

            //If the path contains the text 'StreamingAssets', then take all of the text after the next '/' and use that + the Application.StreamingAssetsPath
            if( assetBundleInfo.path.Contains( "StreamingAssets/" ) )
            {
                assetBundleInfo.path = DatabaseStringHelper.CreateStreamingAssetsPath( assetBundleInfo.path.Substring( assetBundleInfo.path.IndexOf( "StreamingAssets/" ) + 16 ), DatabaseStringHelper.StringStyle.NoSettings );
            }

            //Same goes for PersistentDataPath, if the original link contains a reference to it, then wipe the first part of the path away and create an PersistentDataPath link + the relative path to Asset Bundle
            else if( assetBundleInfo.path.Contains( "PersistentData/" ) )
            {
                assetBundleInfo.path = DatabaseStringHelper.CreatePersistentDataPath( assetBundleInfo.path.Substring( assetBundleInfo.path.IndexOf( "PersistentData/" ) + 15 ) );
            }


            //---------------- Setup our Asset Bundle caching folder

            //Check if the 'AssetBundles' path exists in PersistentDataPath, if it does not, create it!
            string persistentDataAssetBundlePath = DatabaseStringHelper.CreatePersistentDataPath( "AssetBundles" );

            if( !Directory.Exists( persistentDataAssetBundlePath ) )
            {
                Directory.CreateDirectory( persistentDataAssetBundlePath );
            }

            //Check if our PersistentData/AssetBundles location has a folder for this AssetBundle already, if not then create one
            if( !Directory.Exists( persistentDataAssetBundlePath + "/" + assetBundleInfo.name ) )
            {
                Directory.CreateDirectory( persistentDataAssetBundlePath + "/" + assetBundleInfo.name );
            }

            //Create new cache folder
            string cachePath = persistentDataAssetBundlePath + "/" + assetBundleInfo.name + "/" + DateTime.Today.ToLongDateString();
            Cache newCache = new Cache();

            if( !Directory.Exists( cachePath ) )
            {
                Directory.CreateDirectory( cachePath );
                newCache = Caching.AddCache( cachePath );

                //Set current cache for writing to the new cache if the cache is valid
                if( newCache.valid )
                {
                    Caching.currentCacheForWriting = newCache;
                    assetBundleInfo.cachePath = cachePath;
                }
            }
            else
            {
                assetBundleInfo.cachePath = cachePath;
            }
            

            //--------------------- Make the request for the Asset Bundle

            //Set up the UnityWebRequest option based on the choices made in the AssetBundleInfo
            UnityWebRequest www = null;
            
            if( assetBundleInfo.versionType == AssetBundleVersionType.none )
            {
                www = UnityWebRequestAssetBundle.GetAssetBundle( assetBundleInfo.path );
            }
            else if( assetBundleInfo.versionType == AssetBundleVersionType.integer )
            {
                www = UnityWebRequestAssetBundle.GetAssetBundle( assetBundleInfo.path, assetBundleInfo.versionNumber, assetBundleInfo.checksum );
            }
            else if( assetBundleInfo.versionType == AssetBundleVersionType.hash )
            {
                www = UnityWebRequestAssetBundle.GetAssetBundle( assetBundleInfo.path, Hash128.Parse( assetBundleInfo.versionHash ), assetBundleInfo.checksum );
            }

            //Now that our request is ready, let's send it off!
            www.SendWebRequest();
            
            //While the download is occuring, update user on it's progress
            while( !www.isDone )
            {
                //Send an update on the progress
                assetBundleInfo.progress = MathHelper.Map( www.downloadProgress, 0f, 1f, 0f, 100f );
                if( onProgress != null ) { onProgress.Invoke( assetBundleInfo ); }
                yield return null;
            }

            //Since the download has finished, let's mark it's progress as 100%
            assetBundleInfo.progress = 100f;
            if( onProgress != null ) { onProgress.Invoke( assetBundleInfo ); }

            //The download has finished, let's make sure there are no errors
            if( www.isNetworkError && www.downloadedBytes == 0 )
            {
                //Debug.Log( "WWWHelper.cs DownloadAndCacheAssetBundle() ERROR! = " + www.error + ", bytes = " + www.downloadedBytes );
                
                //There was an error, inform the user
                if( onFailed != null ) { onFailed.Invoke( assetBundleInfo ); }
            }
            else //There was no error, we found the asset bundle!
            {
                //Debug.Log( "WWWHelper.cs DownloadAndCacheAssetBundle() Download Complete, no errors found. error = " + www.isNetworkError + ", bytes = " + www.downloadedBytes );

                //Store the bundle to use it later
                assetBundleInfo.assetBundle = DownloadHandlerAssetBundle.GetContent( www );
                
                //Get all the cached versions of this bundle from the Caching system
                List<Hash128> listOfCachedVersions = new List<Hash128>();
                Caching.GetCachedVersions( assetBundleInfo.name, listOfCachedVersions );

                //Check if there's any reason to remove the downloaded asset bundle and replace it with a previously cached version
                if( ShouldBundleBeRemovedFromCache( assetBundleInfo, listOfCachedVersions ) )
                {
                    //If our criteria wasn't met, we can remove the new cache and revert back to the most recent one
                    Caching.currentCacheForWriting = Caching.GetCacheAt( Caching.cacheCount );
                    Caching.RemoveCache( newCache );

                    //Iterate backwards through the list of cached asset bundles
                    for( int i = listOfCachedVersions.Count - 1; i > 0; i-- )
                    {
                        //Load a different bundle from a different cache using the older hash
                        www = UnityWebRequestAssetBundle.GetAssetBundle( assetBundleInfo.path, listOfCachedVersions[ i ], assetBundleInfo.checksum );

                        yield return www.SendWebRequest();
                        assetBundleInfo.assetBundle = DownloadHandlerAssetBundle.GetContent( www );

                        //Check and see if the newly loaded bundle from the cache meets your criteria
                        if( !ShouldBundleBeRemovedFromCache( assetBundleInfo, listOfCachedVersions ) )
                        {
                            assetBundleInfo.cachePath = www.url;
                            break;
                        }
                    }
                }

                //Free up the data from the web stream
                www.Dispose();

                //If the asset bundle exists, then we're good to go!
                onSuccess( assetBundleInfo );
            }

        } //END DownloadAndCacheAssetBundle

        //------------------------------------------//
        private bool ShouldBundleBeRemovedFromCache( AssetBundleInfo assetBundleInfo, List<Hash128> listOfCachedVersions )
        //------------------------------------------//
        {
            //If the assetBundleInfo passed in is null, then we cannot perform this check
            if( assetBundleInfo == null || ( assetBundleInfo != null && assetBundleInfo.assetBundle == null ) )
            { return false; }

            //If the assetBundle is not using hashing, then we cannot compare it to the caches via a hash, so this check cannot be performed
            if( assetBundleInfo.versionType == AssetBundleVersionType.integer )
            { return false; }

            //If there are no cached versions, then return false since there's no bundle to remove
            if( listOfCachedVersions == null || ( listOfCachedVersions != null && listOfCachedVersions.Count > 0 ) )
            { return false; }

            //Look at the cached versions, and see if there's anything that would make us remove the latest bundle from the cache

            //Since we don't have anything for this method yet, just return false
            return false;

        } //END ShouldBundleBeRemovedFromCache



        //----------------------------------------------//
        public void LoadAllAssetBundleIntoMemory( AssetBundleInfo assetBundleInfo, Action<AssetBundleInfo> onSuccess, Action<AssetBundleInfo> onFailed )
        //----------------------------------------------//
        {

            if( assetBundleInfo != null && assetBundleInfo.assetBundle != null )
            {
                StartCoroutine( LoadAssetBundleAsync( assetBundleInfo, onSuccess, onFailed ) );
            }
            else
            {
                if( assetBundleInfo == null )
                {
                    if( onFailed != null ) { onFailed.Invoke( null ); }
                }
                else
                {
                    if( onFailed != null ) { onFailed.Invoke( assetBundleInfo ); }
                }
            }

        } //END LoadAllAssetBundleIntoMemory

        //----------------------------------------------//
        public void LoadAssetBundleIntoMemory( AssetBundleInfo assetBundleInfo, List<string> assetNamesToLoad, Action<AssetBundleInfo> onSuccess, Action<AssetBundleInfo> onFailed )
        //----------------------------------------------//
        {

            if( assetBundleInfo != null && assetBundleInfo.assetBundle != null )
            {
                StartCoroutine( LoadAssetBundleAsync( assetBundleInfo, assetNamesToLoad, onSuccess, onFailed ) );
            }
            else
            {
                if( assetBundleInfo == null )
                {
                    if( onFailed != null ) { onFailed.Invoke( null ); }
                }
                else
                {
                    if( onFailed != null ) { onFailed.Invoke( assetBundleInfo ); }
                }
            }

        } //END LoadAllAssetBundleIntoMemory

        //----------------------------------------------//
        public void LoadAssetBundleIntoMemory( AssetBundleInfo assetBundleInfo, List<string> assetNamesToLoad, Type assetType, Action<AssetBundleInfo> onSuccess, Action<AssetBundleInfo> onFailed )
        //----------------------------------------------//
        {

            if( assetBundleInfo != null && assetBundleInfo.assetBundle != null )
            {
                StartCoroutine( LoadAssetBundleAsync( assetBundleInfo, assetNamesToLoad, assetType, onSuccess, onFailed ) );
            }
            else
            {
                if( assetBundleInfo == null )
                {
                    if( onFailed != null ) { onFailed.Invoke( null ); }
                }
                else
                {
                    if( onFailed != null ) { onFailed.Invoke( assetBundleInfo ); }
                }
            }

        } //END LoadAllAssetBundleIntoMemory


        //-----------------------------------------------//
        private IEnumerator LoadAssetBundleAsync( AssetBundleInfo assetBundleInfo, Action<AssetBundleInfo> onSuccess, Action<AssetBundleInfo> onFailed )
        //-----------------------------------------------//
        {
            //Load everything from the asset bundle
            if( assetBundleInfo.assetBundleRequests == null ) { assetBundleInfo.assetBundleRequests = new List<AssetBundleRequest>(); }

            //Track how many successfull requests this AssetBundleInfo had to start with
            int numberOfSuccessfullRequests = 0;

            foreach( AssetBundleRequest requests in assetBundleInfo.assetBundleRequests )
            {
                if( requests.asset != null )
                {
                    numberOfSuccessfullRequests++;
                }
            }

            //Add a new request
            assetBundleInfo.assetBundleRequests.Add( assetBundleInfo.assetBundle.LoadAllAssetsAsync() );

            //Go grab the asset and put it into memory (use the latest request that was just added)
            yield return assetBundleInfo.assetBundleRequests[ assetBundleInfo.assetBundleRequests.Count - 1 ];

            //Find out how many successfull request this AssetBundleInfo has now
            int updatedNumberOfSuccessfullRequests = 0;

            foreach( AssetBundleRequest requests in assetBundleInfo.assetBundleRequests )
            {
                if( requests.asset != null )
                {
                    updatedNumberOfSuccessfullRequests++;
                }
            }

            //If our count of non-null assetBundleRequests has gone up, our request was successfull
            if( assetBundleInfo.assetBundleRequests != null &&
                updatedNumberOfSuccessfullRequests > numberOfSuccessfullRequests )
            {
                if( onSuccess != null ) { onSuccess.Invoke( assetBundleInfo ); }
            }
            else
            {
                if( onFailed != null ) { onFailed.Invoke( assetBundleInfo ); }
            }

        } //END LoadAssetBundleAsync

        //-----------------------------------------------//
        private IEnumerator LoadAssetBundleAsync( AssetBundleInfo assetBundleInfo, List<string> assetNamesToLoad, Action<AssetBundleInfo> onSuccess, Action<AssetBundleInfo> onFailed )
        //-----------------------------------------------//
        {
            //Load everything from the asset bundle
            if( assetBundleInfo.assetBundleRequests == null ) { assetBundleInfo.assetBundleRequests = new List<AssetBundleRequest>(); }

            //Track how many successfull requests this AssetBundleInfo had to start with
            int numberOfSuccessfullRequests = 0;

            foreach( AssetBundleRequest requests in assetBundleInfo.assetBundleRequests )
            {
                if( requests.asset != null )
                {
                    numberOfSuccessfullRequests++;
                }
            }

            //Add the new request for the list of assets to load
            foreach( string assetNameToLoad in assetNamesToLoad )
            {
                if( assetNameToLoad != "" )
                {
                    assetBundleInfo.assetBundleRequests.Add( assetBundleInfo.assetBundle.LoadAssetAsync( assetNameToLoad ) );

                    //Go grab the asset and put it into memory (use the latest request that was just added)
                    yield return assetBundleInfo.assetBundleRequests[ assetBundleInfo.assetBundleRequests.Count - 1 ];
                }
            }
            
            //Find out how many successfull request this AssetBundleInfo has now
            int updatedNumberOfSuccessfullRequests = 0;

            foreach( AssetBundleRequest requests in assetBundleInfo.assetBundleRequests )
            {
                if( requests.asset != null )
                {
                    updatedNumberOfSuccessfullRequests++;
                }
            }

            //If our count of non-null assetBundleRequests has gone up, our request was successfull
            if( assetBundleInfo.assetBundleRequests != null &&
                updatedNumberOfSuccessfullRequests > numberOfSuccessfullRequests )
            {
                if( onSuccess != null ) { onSuccess.Invoke( assetBundleInfo ); }
            }
            else
            {
                if( onFailed != null ) { onFailed.Invoke( assetBundleInfo ); }
            }

        } //END LoadAssetBundleAsync

        //-----------------------------------------------//
        private IEnumerator LoadAssetBundleAsync( AssetBundleInfo assetBundleInfo, List<string> assetNamesToLoad, Type assetType, Action<AssetBundleInfo> onSuccess, Action<AssetBundleInfo> onFailed )
        //-----------------------------------------------//
        {
            //Load everything from the asset bundle
            if( assetBundleInfo.assetBundleRequests == null ) { assetBundleInfo.assetBundleRequests = new List<AssetBundleRequest>(); }

            //Track how many successfull requests this AssetBundleInfo had to start with
            int numberOfSuccessfullRequests = 0;

            foreach( AssetBundleRequest requests in assetBundleInfo.assetBundleRequests )
            {
                if( requests.asset != null )
                {
                    numberOfSuccessfullRequests++;
                }
            }

            //Add the new request for the list of assets to load
            foreach( string assetNameToLoad in assetNamesToLoad )
            {
                if( assetNameToLoad != "" )
                {
                    assetBundleInfo.assetBundleRequests.Add( assetBundleInfo.assetBundle.LoadAssetAsync( assetNameToLoad, assetType ) );

                    //Go grab the asset and put it into memory (use the latest request that was just added)
                    yield return assetBundleInfo.assetBundleRequests[ assetBundleInfo.assetBundleRequests.Count - 1 ];
                }
            }

            //Find out how many successfull request this AssetBundleInfo has now
            int updatedNumberOfSuccessfullRequests = 0;

            foreach( AssetBundleRequest requests in assetBundleInfo.assetBundleRequests )
            {
                if( requests.asset != null )
                {
                    updatedNumberOfSuccessfullRequests++;
                }
            }

            //If our count of non-null assetBundleRequests has gone up, our request was successfull
            if( assetBundleInfo.assetBundleRequests != null &&
                updatedNumberOfSuccessfullRequests > numberOfSuccessfullRequests )
            {
                if( onSuccess != null ) { onSuccess.Invoke( assetBundleInfo ); }
            }
            else
            {
                if( onFailed != null ) { onFailed.Invoke( assetBundleInfo ); }
            }

        } //END LoadAssetBundleAsync

        //----------------------------------------------//
        public void UnloadAssetBundleFromMemory( AssetBundleInfo assetBundleInfo, Action<AssetBundleInfo> onSuccess, Action<AssetBundleInfo> onFailed )
        //----------------------------------------------//
        {

            if( assetBundleInfo != null && assetBundleInfo.assetBundle != null )
            {
                assetBundleInfo.assetBundle.Unload( false );

                if( onSuccess != null ) { onSuccess.Invoke( assetBundleInfo ); }
            }
            else
            {
                if( assetBundleInfo == null )
                {
                    if( onFailed != null ) { onFailed.Invoke( null ); }
                }
                else
                {
                    if( onFailed != null ) { onFailed.Invoke( assetBundleInfo ); }
                }
            }

        } //END UnloadAssetBundleFromMemory

        //--------------------------------------------//
        /// <summary>
        /// Clear the AssetBundles folder in our PersistentDataPath
        /// </summary>
        /// <returns>Did the operation complete successfully?</returns>
        public bool ClearAssetBundleCache( List<string> assetBundleCachesToDelete = null )
        //--------------------------------------------//
        {
            string cachePath = DatabaseStringHelper.CreatePersistentDataPath( "AssetBundles" );

            if( Directory.Exists( cachePath ) )
            {
                bool worked = Caching.ClearCache();

                DirectoryInfo cacheParentDirectoryInfo = new DirectoryInfo( cachePath );

                //Delete the subfolders within the 'AssetBundles' folder in PersistentData
                if( cacheParentDirectoryInfo != null && cacheParentDirectoryInfo.GetDirectories() != null && cacheParentDirectoryInfo.GetDirectories().Length > 0 )
                {
                    List<DirectoryInfo> directoryInfos = cacheParentDirectoryInfo.GetDirectories().ToList();

                    foreach( DirectoryInfo info in directoryInfos )
                    {
                        if( info != null && info.Exists )
                        {
                            //If this is one of the Asset Bundle caches we are supposed to delete, then get rid of it!
                            if( assetBundleCachesToDelete != null && assetBundleCachesToDelete.Contains( info.Name ) )
                            {
                                DirectoryHelper.DeleteDirectory( info.FullName );
                            }

                            //Otherwise if we're getting rid of all the Asset Bundle caches...
                            else if( assetBundleCachesToDelete == null )
                            {
                                DirectoryHelper.DeleteDirectory( info.FullName );
                            }
                            
                        }
                    }
                }

                return worked;
            }
            else
            {
                return false;
            }

        } //END ClearAssetBundleCache

        #endregion

        #region NATIVE DEVICE COMMUNICATION
        //-----------------------------------------//
        public void SendNativeDeviceCommand(string className, string functionCall, List<string> functionArgs, string argDelimiter)
        //-----------------------------------------//
        {
            InitializeNativeData();

#if UNITY_ANDROID
            // Retrieve the UnityPlayer class.
            AndroidJavaClass unityPlayerClass = new AndroidJavaClass("com.unity3d.player.UnityPlayer");

            // Retrieve the UnityPlayerActivity object ( a.k.a. the current context )
            AndroidJavaObject unityActivity = unityPlayerClass.GetStatic<AndroidJavaObject>("currentActivity");

            object[] parameters = new object[2];
            parameters[0] = unityActivity;
            parameters[1] = functionArgs;

            string combinedArgs = ""; 

            foreach (string arg in functionArgs)
            {
                combinedArgs += arg + argDelimiter;
            }

            unityActivity.Call(functionCall, combinedArgs);
#else
            Debug.LogWarning("Currently, native calls only work when unity is implemented inside native android app.");
#endif

        } //END SendNativeDeviceCommand

        //-----------------------------------------//
        public void InitializeNativeData()
        //-----------------------------------------//
        {
#if UNITY_ANDROID
            if (intent != null)
            {
                AndroidJavaClass UnityPlayer = new AndroidJavaClass("com.unity3d.player.UnityPlayer");
                AndroidJavaObject currentActivity = UnityPlayer.GetStatic<AndroidJavaObject>("currentActivity");

                try
                {
                    //comment
                    intent = currentActivity.Call<AndroidJavaObject>("getIntent");
                    androidDataExists = intent.Call<bool>("hasExtra", "arguments");

                }
                catch (Exception e)
                {
                    intent = null;
                }
            }
#else
            Debug.LogWarning("Currently, native calls only work when unity is implemented inside native android app.");
#endif
        }
        #endregion

    } //END Class

} //END Namespace