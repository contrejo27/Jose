using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.IO;

namespace BrandXR
{
    public class CopyVideosToPersistentDataPath: MonoBehaviour
    {
        public bool ShowDebug = true;

        public bool AllowCopyVideos = true;

        public List<string> videoURLs;

        //----------------------------------------------//
        public void Start()
        //----------------------------------------------//
        {

            if( AllowCopyVideos && PlatformHelper.ShouldPlayVideosFromPersistentDataPath() )
            {
                if( videoURLs != null && videoURLs.Count > 0 )
                {
                    //Make sure this object and it's scripts keep running regardless of the scene
                    if( transform.parent == null )
                    {
                        DontDestroyOnLoad( transform.gameObject );
                    }

                    foreach( string url in videoURLs )
                    {
                        StartCoroutine( CopyVideoToPersistentDataPath( url ) );
                    }
                }
            }

        } //END Start



        //---------------------------------------------//
        public IEnumerator CopyVideoToPersistentDataPath( string urlWithPath )
        //---------------------------------------------//
        {

            string fileName = DatabaseStringHelper.GetFileNameWithExtension( urlWithPath );
            string persistentPath = DatabaseStringHelper.CreatePersistentDataPath( fileName );

            string streamingPath = urlWithPath;
            if( urlWithPath.Contains( "StreamingAssets" ) ) { streamingPath = DatabaseStringHelper.CreateStreamingAssetsPath( urlWithPath.Replace( "StreamingAssets/", "" ), DatabaseStringHelper.StringStyle.WithEscapeUriAndSystemPathCombine ); }


            if( ShowDebug ) Debug.Log( "CopyVideoToPersistent( " + urlWithPath + " ) ... filename = " + fileName + ", persistentPath = " + persistentPath + ", streamingPath = " + streamingPath );


            //If the video does not exist in the persistent data path folder, we need to copy all of it's bytes over
            if( !File.Exists( persistentPath ) )
            {
                WWW www = new WWW( streamingPath );

                while( !www.isDone ) { yield return new WaitForSeconds( .1f ); }

                if( string.IsNullOrEmpty( www.error ) )
                {
                    if( ShowDebug ) Debug.Log( "CopyVideoToPersistent( " + urlWithPath + " ) we found the video in StreamingAssetsPath = " + streamingPath );

                    if( www.bytes != null && www.bytes.Length > 0 )
                    {
                        if( ShowDebug ) Debug.Log( "CopyVideoToPersistent( " + urlWithPath + " ) we found the www.bytes = " + www.bytes.Length.ToString() );


                        //We found the video, copy it's data over over time (not all at once to prevent a crash)
                        Stream stream = new MemoryStream( www.bytes );

                        using( FileStream output = new FileStream( persistentPath, FileMode.Create, FileAccess.Write ) )
                        {
                            byte[] buffer = new byte[ 32 * 1024 ]; //a 32KB-sized buffer is the most efficient
                            int bytesRead;

                            while( ( bytesRead = stream.Read( buffer, 0, buffer.Length ) ) > 0 )
                            {
                                output.Write( buffer, 0, bytesRead );
                            }

                            output.Flush();
                        }

                        //We found the video, copy it's data over
                        //File.WriteAllBytes( persistentPath, www.bytes );

                    }
                    else
                    {
                        if( ShowDebug ) Debug.Log( "CopyVideoToPersistent( " + urlWithPath + " ) we couldn't find any www.bytes = " + www.bytes.Length.ToString() );
                    }
                }
                else
                {
                    if( ShowDebug ) Debug.Log( "CopyVideoToPersistent( " + urlWithPath + " ) Getting the video from StreamingAssets had an error = " + www.error + ", streamingAssetsPath = " + streamingPath );
                }

                www.Dispose();
                www = null;
            }
            else
            {
                if( ShowDebug ) Debug.Log( "CopyVideoToPersistent( " + urlWithPath + " ) the video already exists" );
            }

        } //END CopyVideoToPersistentDataPath

    } //END Class

} //END Namespace