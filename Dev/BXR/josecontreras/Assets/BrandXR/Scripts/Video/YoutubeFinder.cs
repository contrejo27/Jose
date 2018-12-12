//DOB Note: This is a modified version of the YoutubeVideo.cs script that comes with the Youtube plugin.

using UnityEngine;
using System.Collections.Generic;
using YoutubeLight;

using System;
using System.Linq;
using System.Net;
using System.Security.Cryptography.X509Certificates;
using System.Net.Security;

namespace BrandXR
{

    //Used when the "Youtube video player 3" plugin is in the project
    //--------------------------------------------//
    [RequireComponent(typeof(RequestResolver))]
    public class YoutubeFinder : MonoBehaviour
    //--------------------------------------------//
    {
        private int videoQuality;
        private bool useHighestQuality;

        private string videoURL = "";
        public string GetVideoURL() { return videoURL; }
        
        private string audioURL = "";
        public string GetAudioURL() { return audioURL; }

        private bool foundVideo = false;
        private bool isVideoReady = false;
        private bool isAudioReady = false;

        private bool ShowDebug = false;
        
        private Action OnSuccess = null;
        private Action OnFailed = null;

        private RequestResolver requestResolver = null;

        //Singleton behavior
        private static YoutubeFinder _instance;

        //--------------------------------------------//
        public static YoutubeFinder instance
        //--------------------------------------------//
        {
            get
            {
                if( _instance == null )
                {
                    _instance = GameObject.FindObjectOfType<YoutubeFinder>();
                }

                return _instance;
            }

        } //END Instance

        //-----------------------------------//
        public void Start()
        //-----------------------------------//
        {

            AddOrLinkRequestResolver();

        } //END Start

        //------------------------------------//
        public void AddOrLinkRequestResolver()
        //------------------------------------//
        {
            if( requestResolver != null ) { return; }

            if( GetComponent<RequestResolver>() != null )
            {
                requestResolver = GetComponent<RequestResolver>();
            }
            else
            {
                requestResolver = gameObject.AddComponent<RequestResolver>();
            }

        } //AddOrLinkRequestResolver();

        //-------------------------------------------------------//
        public void RequestURL( string url, string quality, bool ShowDebug, Action onSuccess, Action onFailed )
        //-------------------------------------------------------//
        {
            AddOrLinkRequestResolver();

            foundVideo = false;
            isVideoReady = false;
            isAudioReady = false;

            videoURL = url;
            audioURL = url;
            videoQuality = GetVideoQuality( quality );
            useHighestQuality = (quality == "Best Quality");

            this.ShowDebug = ShowDebug;

            OnSuccess = onSuccess;
            OnFailed = onFailed;

            //Call the video system in another thread
            //getVideoThread = new System.Threading.Thread( GetVideoInAnotherThread );
            //getVideoThread.Start();

            GetVideoInAnotherThread();

        } //END RequestURL

        //-------------------------------------//
        private void GetVideoInAnotherThread()
        //-------------------------------------//
        {
            
            if( ShowDebug ) { Debug.Log( "YouTubeFinder.cs GetVideoInAnotherThread() about to call StartCoroutine( requestResolver.GetDownloadUrls() )" ); }

            //StartCoroutine( requestResolver.GetDownloadUrls( GetDownloadUrlsComplete, videoURL, false ) );

            //
            try
            {
                StartCoroutine( requestResolver.GetDownloadUrls( GetDownloadUrlsComplete, videoURL, false, OnFailed ) );
            }
            catch( Exception exception )
            {
                Debug.Log( "catch works ... exception = " + exception );
            }
            //

        } //END GetTheVideo

        //----------------------------------//
        private void GetDownloadUrlsComplete()
        //----------------------------------//
        {
            if( ShowDebug ) { Debug.Log( "YouTubeFinder.cs GetDownloadUrlsComplete() start" ); }

            List<VideoInfo> videoInfos = requestResolver.videoInfos;

            foundVideo = false;

            VideoInfo info = null;
            VideoInfo highestQualityVideoInfo = null;
            VideoInfo matchingVideoInfo = null;

            //Get the quality video we requested
            foreach( VideoInfo _info in videoInfos )
            {
                if( _info != null && _info.VideoType == VideoType.Mp4 && _info.Resolution != 0 )
                {
                    if( highestQualityVideoInfo == null )
                    {
                        highestQualityVideoInfo = _info;
                    }
                    else if( highestQualityVideoInfo.Resolution < _info.Resolution )
                    {
                        highestQualityVideoInfo = _info;
                    }

                    //If we're not looking for the "highest quality", then we need to try to find a match for the quality we're looking for
                    if( !useHighestQuality && _info.Resolution == ( videoQuality ) )
                    {
                        //We found a match! Let's break out of the search
                        matchingVideoInfo = _info;
                        if( ShowDebug ) { Debug.Log( "YoutubeFinder.cs GetVideoInAnotherThread() found matching video in foreach..." ); }
                        break;
                    }
                    else
                    {
                        if( ShowDebug ) { Debug.Log( "YoutubeFinder.cs GetVideoInAnotherThread() Video is not a match... useHighestQuality = " + useHighestQuality + ", SearchResolution = " + videoQuality + ", Video.Resolution = " + _info.Resolution ); }
                    }
                }
            }

            //Set the VideoInfo to what either our search results or the highest quality
            if( matchingVideoInfo != null )
            {
                info = matchingVideoInfo;
                if( ShowDebug ) { Debug.Log( "YoutubeFinder.cs GetVideoInAnotherThread() found the video we searched for! Resolution = " + info.Resolution + ", audio.Bitrate = " + info.AudioBitrate ); }
            }

            //If we found any valid videos to play...
            else if( highestQualityVideoInfo != null )
            {
                info = highestQualityVideoInfo;
                if( ShowDebug ) { Debug.Log( "YoutubeFinder.cs GetVideoInAnotherThread() using the highest quality we could find! Resolution = " + info.Resolution + ", audio.Bitrate = " + info.AudioBitrate ); }
            }

            //We found a video to play!
            if( info != null )
            {
                //Check if we need to decrypt the URL
                if( info.RequiresDecryption )
                {
                    StartCoroutine( requestResolver.DecryptDownloadUrl( VideoUrlDecryptComplete, info ) );
                }
                else
                {
                    VideoUrlDecryptComplete( info.DownloadUrl );
                }

                
                //Check if the video also contains audio, if it does we don't need to use a 2nd Unity video player to play audio from a different stream
                if( info.AudioBitrate != 0 )
                {
                    if( ShowDebug ) { Debug.Log( "YoutubeFinder.cs GetVideoInAnotherThread() video contains an audio stream, no need to use a 2nd video player just for audio" ); }
                    audioURL = "";
                    isAudioReady = true;
                }
                else
                {
                    if( ShowDebug ) { Debug.Log( "YoutubeFinder.cs GetVideoInAnotherThread() video does not contain an audio stream, so we need to use a 2nd video player just for audio" ); }
                    GetAudioURL( videoInfos );
                }
                
            }
            else //If we couldn't find any video to play...
            {
                if( ShowDebug ) { Debug.Log( "YoutubeFinder.cs GetVideoInAnotherThread() Unable to find any videos to play in MP4 format for the sent in URL = " + videoURL ); }
                foundVideo = false;
            }
            
        } //END GetDownloadUrlsComplete

        //----------------------------------//
        private void VideoUrlDecryptComplete( string url )
        //----------------------------------//
        {

            videoURL = url;
            foundVideo = true;
            isVideoReady = true;
            if( ShowDebug ) { Debug.Log( "YoutubeFinder.cs VideoUrlDecryptComplete() decrypted URL = " + videoURL ); }

        } //END VideoUrlDecryptComplete

        //----------------------------------//
        private void GetAudioURL( List<VideoInfo> videoInfo )
        //----------------------------------//
        {

            //Find the video with the lowest quality video stream
            VideoInfo lowestQualityVideoInfo = null;

            //Get the audio url
            foreach( VideoInfo info in videoInfo )
            {
                if( info.VideoType == VideoType.Mp4 && 
                    info.Resolution != 0 &&
                    info.AudioBitrate != 0 && info.AudioBitrate >= 128 )
                {
                    if( lowestQualityVideoInfo == null ) { lowestQualityVideoInfo = info; }
                    else if( lowestQualityVideoInfo.Resolution > info.Resolution ) { lowestQualityVideoInfo = info; }
                }

            }

            if( lowestQualityVideoInfo != null )
            {
                if( lowestQualityVideoInfo.RequiresDecryption )
                {
                    StartCoroutine( requestResolver.DecryptDownloadUrl( AudioUrlDecryptComplete, lowestQualityVideoInfo ) );
                }
                else
                {
                    AudioUrlDecryptComplete( lowestQualityVideoInfo.DownloadUrl );
                }
            }
            else
            {
                if( ShowDebug ) { Debug.Log( "YoutubeFinder.cs GetAudioURL() Unable to find a suitable video to use for audio playback!" ); }
            }
            
        } //END GetAudioURL

        //---------------------------------//
        private void AudioUrlDecryptComplete( string url )
        //---------------------------------//
        {

            audioURL = url;
            isAudioReady = true;
            if( ShowDebug ) { Debug.Log( "YoutubeFinder.cs AudioUrlDecryptComplete() decrypted URL = " + videoURL ); }

        } //END AudioUrlDecryptComplete

        //---------------------------------//
        private int GetVideoQuality( string videoQuality )
        //---------------------------------//
        {
            
            int quality = 360;

            if( videoQuality == "360" ) { quality = 360; }
            else if( videoQuality == "640" ) { quality = 640; }
            else if( videoQuality == "720" ) { quality = 720; }
            else if( videoQuality == "1080" ) { quality = 1080; }
            else if( videoQuality == "1440" ) { quality = 1440; }
            else if( videoQuality == "2160" ) { quality = 2160; }
            else if( videoQuality == "4320" ) { quality = 4320; }
            else if( videoQuality == "Best Quality" ) { quality = 4320; }

            return quality;

        } //END GetVideoQuality



        
        //------------------------------------//
        private void FixedUpdate()
        //------------------------------------//
        {
            //if( ShowDebug ) { Debug.Log( "YoutubeFinder.cs FixedUpdate() isVideoReady = " + isVideoReady + ", isAudioReady = " + isAudioReady ); }

            //Use this to play in main thread.
            if( isVideoReady && isAudioReady )
            {
                isVideoReady = false;
                isAudioReady = false;

                if( foundVideo )
                {
                    if( ShowDebug ) { Debug.Log( "YoutubeFinder.cs FixedUpdate() found the video, so calling OnSuccess()" ); }
                    if( OnSuccess != null ) { OnSuccess.Invoke(); }
                }
                else
                {
                    if( ShowDebug ) { Debug.Log( "YoutubeFinder.cs FixedUpdate() failed to find video, calling OnFailed()" ); }
                    if( OnFailed != null ) { OnFailed.Invoke(); }
                }
            }
            
        } //END FixedUpdate



    } //END Class











    //Used when the Youtube Mobile Video Player + Youtube API V3 plugin is in the scene
    /*
    public class YoutubeFinder: YoutubeVideo
    {
        //Stores the info of the currently playing youtube video file
        public VideoInfo VideoInfo;

        public new static YoutubeVideo Instance;

        //-------------------------------//
        void Awake()
        //-------------------------------//
        {
            Instance = this;

        } //END Awake

        //-------------------------------------------------------//
        public string RequestVideo( string urlOrId, string quality, bool ShowDebug )
        //-------------------------------------------------------//
        {
            ShowDebug = true;

            //Check that this video is valid
            ServicePointManager.ServerCertificateValidationCallback = MyRemoteCertificateValidationCallback;

            //Make sure the url or video ID is valid
            Uri uriResult;
            bool result = Uri.TryCreate
            ( urlOrId, UriKind.Absolute, out uriResult )
                && ( uriResult.Scheme == Uri.UriSchemeHttp || uriResult.Scheme == Uri.UriSchemeHttps );

            //If what we sent in was not a valid url, try to use it as an Video ID instead
            if( !result )
                urlOrId = "https://youtube.com/watch?v=" + urlOrId;

            //Grab the list of different possible video files we can play
            IEnumerable<VideoInfo> videoInfos = DownloadUrlResolver.GetDownloadUrls( urlOrId, false );

            List<VideoInfo> videoFullList = videoInfos.ToList<VideoInfo>();
            List<VideoInfo> videoCompatibleList = new List<VideoInfo>();
            List<VideoInfo> videoUnknownList = new List<VideoInfo>();

            //DOB Debug, list all of the possible videos

            if( ShowDebug )
            {
                Debug.Log( "------------------------------------------------------- \n ------------------------------------------------------------------" );
                //Perform a print-out of all of our compatible videos
                foreach( VideoInfo info in videoFullList )
                {
                    Debug.Log( "FULL LIST - VideoType = " + info.VideoType.ToString() + ", AudioType = " + info.AudioType.ToString() + ", CanExtractAudio = " + info.CanExtractAudio.ToString() + ", Resolution = " + info.Resolution.ToString() + ", FormatCode = " + info.FormatCode.ToString() + ", Is3D = " + info.Is3D.ToString() );
                }
                Debug.Log( "------------------------------------------------------- \n ------------------------------------------------------------------" );
            }


            //DEBUG Testing, let's see what happens if we play an unsupported video format and unsupported audio
            foreach( VideoInfo info in videoFullList )
            {
                //Normally we would want to get all compatible videos, and see if our preferred format is on there
                //For now we can just get MP4, since it's what the MPMP video plugin prefers
                if( info.VideoType == VideoType.Unknown && info.Resolution == 0 ) //  )
                {
                    videoUnknownList.Add( info );
                }
            }

            //DOB Debug, list all of the unknown videos

            if( ShowDebug )
            {
                Debug.Log( "------------------------------------------------------- \n ------------------------------------------------------------------" );
                //Perform a print-out of all of our unknown videos
                foreach( VideoInfo info in videoUnknownList )
                {
                    Debug.Log( "UNKNOWN LIST - VideoType = " + info.VideoType.ToString() + ", AudioType = " + info.AudioType.ToString() + ", CanExtractAudio = " + info.CanExtractAudio.ToString() + ", Resolution = " + info.Resolution.ToString() + ", FormatCode = " + info.FormatCode.ToString() + ", Is3D = " + info.Is3D.ToString() );
                }
                Debug.Log( "------------------------------------------------------- \n ------------------------------------------------------------------" );
            }


            //Check this list of videos, if we cannot find any videos with audio that can be played, then allow all known videos even if they do not have audio
            bool foundAnyVideoWithCompatibleAudio = false;

            foreach( VideoInfo info in videoFullList )
            {
                if( info.VideoType != VideoType.Unknown && info.Resolution != 0 )
                {
                    if( info.AudioType != YoutubeLight.AudioType.Unknown )
                    {
                        foundAnyVideoWithCompatibleAudio = true;
                        break;
                    }
                }
            }

            //Remove all of the video options that are unplayable on this platform
            foreach( VideoInfo info in videoFullList )
            {
                //Normally we would want to get all compatible videos, and see if our preferred format is on there
                //For now we can just get MP4, since it's what the MPMP video plugin prefers
                if( info.VideoType != VideoType.Unknown && info.Resolution != 0 )
                {
                    //Next we need to only get videos with an audio format we can play
                    //For now we'll just use MP3, this should be changed to what's preferred on each platform
                    if( info.AudioType != YoutubeLight.AudioType.Unknown )
                    {
                        videoCompatibleList.Add( info );
                    }
                    //If we found compatible videos, but none of them have playable sound, then allow the video anyway (Video with no sound is better than nothing)
                    else if( !foundAnyVideoWithCompatibleAudio )
                    {
                        videoCompatibleList.Add( info );
                    }
                }
            }


            //Sort the compatible videos by Highest Quality
            videoCompatibleList.Sort
            (
                delegate ( VideoInfo info1, VideoInfo info2 )
                {
                    return info1.Resolution.CompareTo( info2.Resolution );
                }
            );
            videoCompatibleList.Reverse();

            
            if( ShowDebug )
            {
                //Perform a print-out of all of our compatible videos
                foreach( VideoInfo info in videoCompatibleList )
                {
                    Debug.Log( "VideoType = " + info.VideoType.ToString() + ", AudioType = " + info.AudioType.ToString() + ", CanExtractAudio = " + info.CanExtractAudio.ToString() + ", Resolution = " + info.Resolution.ToString() + ", FormatCode = " + info.FormatCode.ToString() + ", Is3D = " + info.Is3D.ToString() );
                }
            }
            

            int resolution = 1080;

            //DOB DEBUG false
            if( quality == "Automatic" && videoCompatibleList != null && videoCompatibleList.Count > 0 )
            {
                this.VideoInfo = videoCompatibleList[ 0 ];
            }
            else if( quality == "Automatic" || videoCompatibleList == null || ( videoCompatibleList != null && videoCompatibleList.Count == 0 ) )
            {
                this.VideoInfo = videoFullList.First( info => info.VideoType == VideoType.Mp4 );
            }

            //We chose a specific resolution, let's try to use it
            else if( quality != "Automatic" && int.TryParse( quality, out resolution ) )
            {
                this.VideoInfo = videoFullList.First( info => info.VideoType == VideoType.Mp4 && info.Resolution == resolution );
            }
            else
            {
                //Everything we tried failed, just grab the first compatible video we can find
                this.VideoInfo = videoFullList.First( info => info.VideoType == VideoType.Mp4 );
            }

            if( this.VideoInfo != null && this.VideoInfo.RequiresDecryption )
            {
                DownloadUrlResolver.DecryptDownloadUrl( this.VideoInfo );
            }

            if( this.VideoInfo != null )
            {
                if( ShowDebug ) Debug.Log( "VideoType = " + VideoInfo.VideoType.ToString() + ", AudioType = " + VideoInfo.AudioType.ToString() + ", Resolution = " + VideoInfo.Resolution.ToString() + ", FormatCode = " + VideoInfo.FormatCode.ToString() );

                return this.VideoInfo.DownloadUrl;
            }
            else
            {
                if( ShowDebug ) Debug.Log( "YoutubeFinder.cs RequestVideo() Failed to find a Youtube video at the URL with a compatible format on this platform!" );

                return "";
            }
            

        } //END RequestVideo

        //------------------------------------------------------------------//
        public bool MyRemoteCertificateValidationCallback( System.Object sender, X509Certificate certificate, X509Chain chain, SslPolicyErrors sslPolicyErrors )
        //------------------------------------------------------------------//
        {
            bool isOk = true;

            // If there are errors in the certificate chain, look at each error to determine the cause.
            if( sslPolicyErrors != SslPolicyErrors.None )
            {
                for( int i = 0; i < chain.ChainStatus.Length; i++ )
                {
                    if( chain.ChainStatus[ i ].Status != X509ChainStatusFlags.RevocationStatusUnknown )
                    {
                        chain.ChainPolicy.RevocationFlag = X509RevocationFlag.EntireChain;
                        chain.ChainPolicy.RevocationMode = X509RevocationMode.Online;
                        chain.ChainPolicy.UrlRetrievalTimeout = new TimeSpan( 0, 1, 0 );
                        chain.ChainPolicy.VerificationFlags = X509VerificationFlags.AllFlags;
                        bool chainIsValid = chain.Build( (X509Certificate2)certificate );
                        if( !chainIsValid )
                        {
                            isOk = false;
                        }
                    }
                }
            }

            return isOk;

        } //END MyRemoteCertificateValidationCallback


    } //END Class
    */

} //END Namespace