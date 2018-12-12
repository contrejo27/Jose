using System.Collections;
using UnityEngine;
using UnityEngine.UI;

namespace BrandXR
{
    public class UITextureDownloader: MonoBehaviour
    {

        public Material material;
        public RawImage rawImage;
        public string imageURL;

        //---------------------------------//
        public void Awake()
        //---------------------------------//
        {

            if( material == null && rawImage == null )
            {
                if( transform.GetComponent<RawImage>() != null )
                {
                    rawImage = transform.GetComponent<RawImage>();
                }
            }

        } //END Awake

        //---------------------------------//
        public void Start()
        //---------------------------------//
        {

            StartCoroutine( DownloadTexture( imageURL ) );

        } //END Start


        //---------------------------------//
        private IEnumerator DownloadTexture( string url )
        //---------------------------------//
        {
            if( url.Contains( "StreamingAssets" ) )
            {
                url = DatabaseStringHelper.CreateStreamingAssetsPath( url.Replace( "StreamingAssets/", "" ), DatabaseStringHelper.StringStyle.WithEscapeUriAndSystemPathCombine );
            }

            WWW www = new WWW( url );

            while( !www.isDone )
            {
                yield return new WaitForSeconds( .1f );
            }

            if( string.IsNullOrEmpty( www.error ) )
            {
                //Debug.Log( "DownloadTexture() url = " + url + ", Works! Width = " + www.textureNonReadable.width + ", Height = " + www.textureNonReadable.height );

                Texture2D texture = new Texture2D( www.textureNonReadable.width, www.textureNonReadable.height, WWWHelper.GetPlatformPreferredTextureFormat(), false );
                texture = www.textureNonReadable;

                if( material != null ) { material.mainTexture = texture; }
                if( rawImage != null ) { rawImage.texture = texture; }
            }
            else
            {
                //Debug.Log( "DownloadTexture() url = " + url + ", error = " + www.error );
            }

        } //END DownloadTexture

        //------------------------------------------------------------//
        private void PrepareForDestroy()
        //------------------------------------------------------------//
        {


        } //END PrepareForDestroy


    } //END Class

} //END Namespace