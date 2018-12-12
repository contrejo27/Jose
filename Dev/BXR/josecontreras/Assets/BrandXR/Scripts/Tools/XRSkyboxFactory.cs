using System;
using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace BrandXR
{
    public class XRSkyboxFactory: MonoBehaviour
    {
        private bool showDebug = false;
        
        //A list of the various collections of images you can choose to view
        public enum ImageTag
        {
            All,
            TestImageOne,
            TestImageTwo
        }
        [Tooltip( "Image Tags are collections of images that belong together. Used when calling ImageManager.cs GetFirstImage() or GetNextImage()" )]
        public ImageTag currentImageTag = ImageTag.All;

        [Tooltip( "The ALL image tag contains every ImageType besides Black & White as they are effects used when fading between images. This tag collection is automatically setup on Awake()" )]
        public List<ImageType> list_TaggedImages_All;

        public List<ImageType> list_TaggedImages_TestImageOne;
        public List<ImageType> list_TaggedImages_TestImageTwo;
        
        public Dictionary<string, Texture> dictionary_Block_ImageComponent_Images = new Dictionary<string, Texture>();

        [Serializable]
        public class StereoCubeMap
        {
            public bxrCubemap left;
            public bxrCubemap right;

            public bxrCubemap GetCameraCubeMap( CameraType type )
            {
                if( type == CameraType.Left )
                    return left;
                else
                    return right;
            }

        } //END StereoCubeMap
        
        public Dictionary<ImageType, StereoCubeMap> dictionary_Cubemap_1024 = new Dictionary<ImageType, StereoCubeMap>();
        public Dictionary<ImageType, StereoCubeMap> dictionary_Cubemap_2048 = new Dictionary<ImageType, StereoCubeMap>();

        //A list of the images you can change between
        public enum ImageType
        {
            TestImageOne = 10,
            TestImageTwo = 11
        }
        public static ImageType currentImageType;
        public static int int_NumberOfImages = 0;

        //Sides of a cube
        public enum CubeSide
        {
            front,
            back,
            left,
            right,
            top,
            bottom
        }

        //Stereo camera, left or right
        public enum CameraType
        {
            Left,
            Right
        }

        //Camera skybox material, we have two different slots for textures that we can fade between
        public enum ImageSlot
        {
            First,
            Second
        }

        //The regular left and right images
        private Texture2D texture_Left_front = null;
        private bool bool_Left_front_loaded = false;

        private Texture2D texture_Left_back = null;
        private bool bool_Left_back_loaded = false;

        private Texture2D texture_Left_left = null;
        private bool bool_Left_left_loaded = false;

        private Texture2D texture_Left_right = null;
        private bool bool_Left_right_loaded = false;

        private Texture2D texture_Left_top = null;
        private bool bool_Left_top_loaded = false;

        private Texture2D texture_Left_bottom = null;
        private bool bool_Left_bottom_loaded = false;

        private Texture2D texture_Right_front = null;
        private bool bool_Right_front_loaded = false;

        private Texture2D texture_Right_back = null;
        private bool bool_Right_back_loaded = false;

        private Texture2D texture_Right_left = null;
        private bool bool_Right_left_loaded = false;

        private Texture2D texture_Right_right = null;
        private bool bool_Right_right_loaded = false;

        private Texture2D texture_Right_top = null;
        private bool bool_Right_top_loaded = false;

        private Texture2D texture_Right_bottom = null;
        private bool bool_Right_bottom_loaded = false;


        private bool isReadyToTweenToSecondSlot = false;




        private bool _IsTweening = false;


        
        public Dictionary<ImageType, Vector3> cameraRotations = new Dictionary<ImageType, Vector3>();


        // singleton behavior
        private static XRSkyboxFactory _instance;

        //--------------------------------------------//
        public static XRSkyboxFactory instance
        //--------------------------------------------//
        {
            get
            {
                if( _instance == null )
                {
                    _instance = GameObject.FindObjectOfType<XRSkyboxFactory>();
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

            //Fill the list of 'ALL' images
            foreach( ImageType imageType in Enum.GetValues( typeof( ImageType ) ) )
            {
                list_TaggedImages_All.Add( imageType );
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

        //-------------------------------//
        public void BeginLoadingTexture( CameraType cameraType, CubeSide cubeSide, ImageType imageType )
        //-------------------------------//
        {

            SetLoadedBool( cameraType, cubeSide, false );

            //if( showDebug ) { Debug.Log( "ImageFactory.cs BeginLoadingTexture() ( " + cameraType + ", " + cubeSide + ", " + imageType + " )" ); }

            if( XRSkyboxManager.instance.loadFrom == XRSkyboxManager.LoadFrom.DictionaryTexture )
            {
                SetDictionaryLoadedTexture( cameraType, cubeSide, imageType );
            }
            else if( XRSkyboxManager.instance.loadFrom == XRSkyboxManager.LoadFrom.ResourcesTexture )
            {
                SetTextureFromResourcesTexture( cameraType, cubeSide, imageType );
            }
            else if( XRSkyboxManager.instance.loadFrom == XRSkyboxManager.LoadFrom.ResourcesBytes )
            {
                StartCoroutine( SetTextureFromResourcesBytes( cameraType, cubeSide, imageType ) );
            }
            else if( XRSkyboxManager.instance.loadFrom == XRSkyboxManager.LoadFrom.StreamingAssetsBytes )
            {
                StartCoroutine( SetTextureFromStreamingAssetsBytes( cameraType, cubeSide, imageType ) );
            }

        } //END BeginLoadingTexture


        //-----------------------------------------------------------------------------------------------------//
        private bool SetDictionaryLoadedTexture( CameraType cameraType, CubeSide cubeSide, ImageType imageType )
        //-----------------------------------------------------------------------------------------------------//
        {
            if( showDebug ) { Debug.Log( "ImageFactory.cs SetDictionaryLoadedTexture() start" ); }

            Texture2D tempTexture = null;

            Dictionary<ImageType, StereoCubeMap> dictionary_Cubemap = dictionary_Cubemap_2048;

            if( PlatformHelper.IsLowEndDevice() )
            {
                dictionary_Cubemap = dictionary_Cubemap_1024;
            }

            if( dictionary_Cubemap.ContainsKey( imageType ) )
            {
                if( showDebug ) { Debug.Log( "SetDictionaryLoadedTexture( " + cameraType + ", " + cubeSide + ", " + imageType + ") dictionary_Cubemap.ContainsKey(), we found the texture!" ); }
                tempTexture = dictionary_Cubemap[ imageType ].GetCameraCubeMap( cameraType ).GetTexture( cubeSide );
            }
            else
            {
                if( showDebug ) { Debug.Log( "SetDictionaryLoadedTexture( " + cameraType + ", " + cubeSide + ", " + imageType + ") dictionary_Cubemap does NOT ContainKey(), we could not find any textures for this imageType!" ); }
                return false;
            }

            if( tempTexture == null )
            {
                if( dictionary_Cubemap[ imageType ].GetCameraCubeMap( XRSkyboxFactory.CameraType.Left ).GetTexture( cubeSide ) != null )
                {
                    if( showDebug ) { Debug.Log( "SetDictionaryLoadedTexture( " + cameraType + ", " + cubeSide + ", " + imageType + ") dictionary_Cubemap.ContainsKey(), but we could not find the texture for this cubeSide!, let's use the texture for the left cubeside instead!" ); }
                    tempTexture = dictionary_Cubemap[ imageType ].GetCameraCubeMap( XRSkyboxFactory.CameraType.Left ).GetTexture( cubeSide );
                }
                else
                {
                    if( showDebug ) { Debug.Log( "SetDictionaryLoadedTexture( " + cameraType + ", " + cubeSide + ", " + imageType + ") dictionary_Cubemap.ContainsKey(), but we cannot find the left nor the right cubeside texture! Returning FALSE" ); }
                    return false;
                }
            }


            if( cameraType == CameraType.Left )
            {
                if( cubeSide == CubeSide.front )
                {
                    texture_Left_front = tempTexture;
                }
                else if( cubeSide == CubeSide.back )
                {
                    texture_Left_back = tempTexture;
                }
                else if( cubeSide == CubeSide.left )
                {
                    texture_Left_left = tempTexture;
                }
                else if( cubeSide == CubeSide.right )
                {
                    texture_Left_right = tempTexture;
                }
                else if( cubeSide == CubeSide.top )
                {
                    texture_Left_top = tempTexture;
                }
                else if( cubeSide == CubeSide.bottom )
                {
                    texture_Left_bottom = tempTexture;
                }
            }
            else if( cameraType == CameraType.Right )
            {
                if( cubeSide == CubeSide.front )
                {
                    texture_Right_front = tempTexture;
                }
                else if( cubeSide == CubeSide.back )
                {
                    texture_Right_back = tempTexture;
                }
                else if( cubeSide == CubeSide.left )
                {
                    texture_Right_left = tempTexture;
                }
                else if( cubeSide == CubeSide.right )
                {
                    texture_Right_right = tempTexture;
                }
                else if( cubeSide == CubeSide.top )
                {
                    texture_Right_top = tempTexture;
                }
                else if( cubeSide == CubeSide.bottom )
                {
                    texture_Right_bottom = tempTexture;
                }
            }

            //The texture is now loaded! Set the appropriate boolean to true
            SetLoadedBool( cameraType, cubeSide, true );

            return true;

        } //END SetDictionaryLoadedTexture


        //-----------------------------//
        private void SetTextureFromResourcesTexture( CameraType cameraType, CubeSide cubeSide, ImageType imageType )
        //-----------------------------//
        {
            if( showDebug ) { Debug.Log( "ImageFactory.cs SetTextureFromResourcesTexture() start" ); }

            //Set the scale of the texture to grab based on whether this device can support high end textures
            int scale = 2048;
            string size = "Large";

            if( PlatformHelper.IsLowEndDevice() )
            {
                scale = 1024;
                size = "Small";
            }

            //Get the texture from the Resources folder
            string path = "Textures/Images/" + size.ToString() + "/" + imageType.ToString() + "/" + cameraType.ToString() + "/" + cubeSide.ToString();

            //If the texture doesn't exist, change the path to the opposite camera (allows us to use Mono images instead of Stereo)
            if( Resources.Load<Texture2D>( path ) == null )
            {
                CameraType camera = CameraType.Left;
                if( cameraType == CameraType.Left ) { camera = CameraType.Right; }
                path = "Textures/Images/" + size.ToString() + "/" + imageType.ToString() + "/" + camera.ToString() + "/" + cubeSide.ToString();
            }

            if( showDebug ) { Debug.Log( "ImageFactory.cs SetTextureFromResourcesTexture() path = " + path ); }


            //Create a new texture variable with the appropriate scale and settings
            SetLoadedTextureScaleAndSettings( cameraType, cubeSide, WWWHelper.GetPlatformPreferredTextureFormat(), false, scale, scale );

            //Set the texture's wrapping mode to clamped, which prevents seeing the edges of the skybox's cubemap
            SetLoadedTextureWrapMode( cameraType, cubeSide, TextureWrapMode.Clamp );

            //Set the bytes to the appropriate texture
            if( cameraType == CameraType.Left )
            {
                if( cubeSide == CubeSide.front )
                {
                    texture_Left_front = Resources.Load<Texture2D>( path );
                }
                else if( cubeSide == CubeSide.back )
                {
                    texture_Left_back = Resources.Load<Texture2D>( path );
                }
                else if( cubeSide == CubeSide.left )
                {
                    texture_Left_left = Resources.Load<Texture2D>( path );
                }
                else if( cubeSide == CubeSide.right )
                {
                    texture_Left_right = Resources.Load<Texture2D>( path );
                }
                else if( cubeSide == CubeSide.top )
                {
                    texture_Left_top = Resources.Load<Texture2D>( path );
                }
                else if( cubeSide == CubeSide.bottom )
                {
                    texture_Left_bottom = Resources.Load<Texture2D>( path );
                }
            }
            else if( cameraType == CameraType.Right )
            {
                if( cubeSide == CubeSide.front )
                {
                    texture_Right_front = Resources.Load<Texture2D>( path );
                }
                else if( cubeSide == CubeSide.back )
                {
                    texture_Right_back = Resources.Load<Texture2D>( path );
                }
                else if( cubeSide == CubeSide.left )
                {
                    texture_Right_left = Resources.Load<Texture2D>( path );
                }
                else if( cubeSide == CubeSide.right )
                {
                    texture_Right_right = Resources.Load<Texture2D>( path );
                }
                else if( cubeSide == CubeSide.top )
                {
                    texture_Right_top = Resources.Load<Texture2D>( path );
                }
                else if( cubeSide == CubeSide.bottom )
                {
                    texture_Right_bottom = Resources.Load<Texture2D>( path );
                }
            }

            //This texture is loaded, set the appropriate bool
            SetLoadedBool( cameraType, cubeSide, true );

        } //END SetTextureFromResourcesTexture

        //-----------------------------//
        private IEnumerator SetTextureFromResourcesBytes( CameraType cameraType, CubeSide cubeSide, ImageType imageType )
        //-----------------------------//
        {
            if( showDebug ) { Debug.Log( "ImageFactory.cs SetResourcesLoadedTexture() start" ); }

            //First we need to know how big the image scale is (width, height), we store a text file for each image
            //  at each image's parent folder (EX: Resources/Textures/Astronaut/scale) ... Resources calls don't use file format extension (EX: .jpg, .txt, .bytes )
            ResourceRequest request = Resources.LoadAsync<TextAsset>( "Textures/" + imageType.ToString() + "/scale" );


            //Wait until the file has been read
            while( !request.isDone )
            {
                yield return request;
            }

            //Now that the file has been read, turn it into a TextAsset, and from that we can read it as a string
            TextAsset textAsset = request.asset as TextAsset;

            //We have the scale text file, grab the x and y scale as ints
            string[] scales = textAsset.text.Split( new char[] { 'x' } );
            int scaleX = int.Parse( scales[ 0 ] );
            int scaleY = int.Parse( scales[ 1 ] );

            //When we're done, make sure we unload anything we grabbed from Resources
            Resources.UnloadAsset( textAsset );



            //Get the bytes from the Resources folder
            string bytesPath = "Textures/" + imageType.ToString() + "/" + cameraType.ToString() + "/" + cubeSide.ToString();

            //This has to be done as a TextAsset, and from there we can grab the bytes
            request = Resources.LoadAsync<TextAsset>( bytesPath );

            //Wait until the file has been read
            while( !request.isDone )
            {
                yield return request;
            }

            //Now that the file has been read, turn it into a TextAsset, and from that we can turn it into a .bytes
            textAsset = request.asset as TextAsset;

            //Create a new texture variable with the appropriate scale and settings
            SetLoadedTextureScaleAndSettings( cameraType, cubeSide, WWWHelper.GetPlatformPreferredTextureFormat(), false, scaleX, scaleY );

            //Set the texture's wrapping mode to clamped, which prevents seeing the edges of the skybox's cubemap
            SetLoadedTextureWrapMode( cameraType, cubeSide, TextureWrapMode.Clamp );

            //Set the bytes to the appropriate texture
            if( cameraType == CameraType.Left )
            {
                if( cubeSide == CubeSide.front )
                {
                    texture_Left_front.LoadImage( textAsset.bytes );
                }
                else if( cubeSide == CubeSide.back )
                {
                    texture_Left_back.LoadImage( textAsset.bytes );
                }
                else if( cubeSide == CubeSide.left )
                {
                    texture_Left_left.LoadImage( textAsset.bytes );
                }
                else if( cubeSide == CubeSide.right )
                {
                    texture_Left_right.LoadImage( textAsset.bytes );
                }
                else if( cubeSide == CubeSide.top )
                {
                    texture_Left_top.LoadImage( textAsset.bytes );
                }
                else if( cubeSide == CubeSide.bottom )
                {
                    texture_Left_bottom.LoadImage( textAsset.bytes );
                }
            }
            else if( cameraType == CameraType.Right )
            {
                if( cubeSide == CubeSide.front )
                {
                    texture_Right_front.LoadImage( textAsset.bytes );
                }
                else if( cubeSide == CubeSide.back )
                {
                    texture_Right_back.LoadImage( textAsset.bytes );
                }
                else if( cubeSide == CubeSide.left )
                {
                    texture_Right_left.LoadImage( textAsset.bytes );
                }
                else if( cubeSide == CubeSide.right )
                {
                    texture_Right_right.LoadImage( textAsset.bytes );
                }
                else if( cubeSide == CubeSide.top )
                {
                    texture_Right_top.LoadImage( textAsset.bytes );
                }
                else if( cubeSide == CubeSide.bottom )
                {
                    texture_Right_bottom.LoadImage( textAsset.bytes );
                }
            }

            //When we're done, make sure we unload anything we grabbed from Resources
            Resources.UnloadAsset( textAsset );

            //This texture is loaded, set the appropriate bool
            SetLoadedBool( cameraType, cubeSide, true );

        } //END SetResourcesLoadedTexture

        //-----------------------------//
        private IEnumerator SetTextureFromStreamingAssetsBytes( CameraType cameraType, CubeSide cubeSide, ImageType imageType )
        //-----------------------------//
        {
            if( showDebug ) { Debug.Log( "ImageFactory.cs SetStreamingAssetsLoadedTexture() start" ); }

            WWW www;

            //First we need to get the actual image files that are stored in the StreamingAssets folder as byte files
            string texturePath = DatabaseStringHelper.CreateStreamingAssetsPath( "Textures/" + imageType.ToString() + "/" + cameraType.ToString() + "/" + cubeSide.ToString() + ".bytes", DatabaseStringHelper.StringStyle.WithEscapeUriAndSystemPathCombine );
            string existsPath = DatabaseStringHelper.CreateStreamingAssetsPathForFileExistsCheck( "Textures/" + imageType.ToString() + "/" + cameraType.ToString() + "/" + cubeSide.ToString() + ".bytes" );


            bool exists = false;

            //Check if the image exists as bytes, this check is different by platform
            if( Application.platform == RuntimePlatform.Android )
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



            //If the texture does not exist in bytes format, try to load it as a .jpg file
            if( !exists )
            {
                //Debug.Log( "ImageFactory.cs image does not exist as bytes, trying as jpg... existsPath = " + existsPath );
                texturePath = "Textures/" + imageType.ToString() + "/" + cameraType.ToString() + "/" + cubeSide.ToString() + ".jpg";
                texturePath = DatabaseStringHelper.CreateStreamingAssetsPath( texturePath, DatabaseStringHelper.StringStyle.WithEscapeUriAndSystemPathCombine );
            }
            else
            {
                //Debug.Log( "ImageFactory.cs image exists as bytes! ... existsPath = " + existsPath );
            }

            //Download the image bytes from the StreamingAssets folder
            www = new WWW( texturePath );

            //Continue with the main thread until this finishes
            while( !www.isDone )
            {
                yield return www;
            }

            //Setup the appropriate amount of texture memory for the bytes
            SetLoadedTextureScaleAndSettings( cameraType, cubeSide, WWWHelper.GetPlatformPreferredTextureFormat(), false, www.textureNonReadable.width, www.textureNonReadable.height );

            //The bytes are loaded and the texture memory is set, let's turn the bytes into a texture in OpenGL
            if( cameraType == CameraType.Left )
            {
                if( cubeSide == CubeSide.front )
                {
                    texture_Left_front = www.textureNonReadable;
                }
                else if( cubeSide == CubeSide.back )
                {
                    texture_Left_back = www.textureNonReadable;
                }
                else if( cubeSide == CubeSide.left )
                {
                    texture_Left_left = www.textureNonReadable;
                }
                else if( cubeSide == CubeSide.right )
                {
                    texture_Left_right = www.textureNonReadable;
                }
                else if( cubeSide == CubeSide.top )
                {
                    texture_Left_top = www.textureNonReadable;
                }
                else if( cubeSide == CubeSide.bottom )
                {
                    texture_Left_bottom = www.textureNonReadable;
                }
            }
            else if( cameraType == CameraType.Right )
            {
                if( cubeSide == CubeSide.front )
                {
                    texture_Right_front = www.textureNonReadable;
                }
                else if( cubeSide == CubeSide.back )
                {
                    texture_Right_back = www.textureNonReadable;
                }
                else if( cubeSide == CubeSide.left )
                {
                    texture_Right_left = www.textureNonReadable;
                }
                else if( cubeSide == CubeSide.right )
                {
                    texture_Right_right = www.textureNonReadable;
                }
                else if( cubeSide == CubeSide.top )
                {
                    texture_Right_top = www.textureNonReadable;
                }
                else if( cubeSide == CubeSide.bottom )
                {
                    texture_Right_bottom = www.textureNonReadable;
                }
            }

            //Clear the existing bytes from memory
            www.Dispose();

            //Now that the texture is set in OpenGL, make sure the textures are set to Clamped (Hides seams between textures)
            SetLoadedTextureWrapMode( cameraType, cubeSide, TextureWrapMode.Clamp );



            //The texture is now loaded! Set the appropriate boolean to true
            SetLoadedBool( cameraType, cubeSide, true );

        } //END SetStreamingAssetsLoadedTexture


        //-----------------------------//
        private void SetLoadedTextureScaleAndSettings( CameraType cameraType, CubeSide cubeSide, TextureFormat format, bool useMipmap, int scaleX, int scaleY )
        //-----------------------------//
        {

            if( cameraType == CameraType.Left )
            {
                if( cubeSide == CubeSide.front )
                {
                    texture_Left_front = new Texture2D( scaleX, scaleY, format, useMipmap ); //, format, useMipmap );
                }
                else if( cubeSide == CubeSide.back )
                {
                    texture_Left_back = new Texture2D( scaleX, scaleY, format, useMipmap );
                }
                else if( cubeSide == CubeSide.left )
                {
                    texture_Left_left = new Texture2D( scaleX, scaleY, format, useMipmap );
                }
                else if( cubeSide == CubeSide.right )
                {
                    texture_Left_right = new Texture2D( scaleX, scaleY, format, useMipmap );
                }
                else if( cubeSide == CubeSide.top )
                {
                    texture_Left_top = new Texture2D( scaleX, scaleY, format, useMipmap );
                }
                else if( cubeSide == CubeSide.bottom )
                {
                    texture_Left_bottom = new Texture2D( scaleX, scaleY, format, useMipmap );
                }
            }
            else if( cameraType == CameraType.Right )
            {
                if( cubeSide == CubeSide.front )
                {
                    texture_Right_front = new Texture2D( scaleX, scaleY, format, useMipmap );
                }
                else if( cubeSide == CubeSide.back )
                {
                    texture_Right_back = new Texture2D( scaleX, scaleY, format, useMipmap );
                }
                else if( cubeSide == CubeSide.left )
                {
                    texture_Right_left = new Texture2D( scaleX, scaleY, format, useMipmap );
                }
                else if( cubeSide == CubeSide.right )
                {
                    texture_Right_right = new Texture2D( scaleX, scaleY, format, useMipmap );
                }
                else if( cubeSide == CubeSide.top )
                {
                    texture_Right_top = new Texture2D( scaleX, scaleY, format, useMipmap );
                }
                else if( cubeSide == CubeSide.bottom )
                {
                    texture_Right_bottom = new Texture2D( scaleX, scaleY, format, useMipmap );
                }
            }

        } //END SetLoadedTextureScale


        //-----------------------------//
        private void SetLoadedTextureWrapMode( CameraType cameraType, CubeSide cubeSide, TextureWrapMode wrapMode )
        //-----------------------------//
        {

            if( cameraType == CameraType.Left )
            {
                if( cubeSide == CubeSide.front )
                {
                    texture_Left_front.wrapMode = wrapMode;
                }
                else if( cubeSide == CubeSide.back )
                {
                    texture_Left_back.wrapMode = wrapMode;
                }
                else if( cubeSide == CubeSide.left )
                {
                    texture_Left_left.wrapMode = wrapMode;
                }
                else if( cubeSide == CubeSide.right )
                {
                    texture_Left_right.wrapMode = wrapMode;
                }
                else if( cubeSide == CubeSide.top )
                {
                    texture_Left_top.wrapMode = wrapMode;
                }
                else if( cubeSide == CubeSide.bottom )
                {
                    texture_Left_bottom.wrapMode = wrapMode;
                }
            }
            else if( cameraType == CameraType.Right )
            {
                if( cubeSide == CubeSide.front )
                {
                    texture_Right_front.wrapMode = wrapMode;
                }
                else if( cubeSide == CubeSide.back )
                {
                    texture_Right_back.wrapMode = wrapMode;
                }
                else if( cubeSide == CubeSide.left )
                {
                    texture_Right_left.wrapMode = wrapMode;
                }
                else if( cubeSide == CubeSide.right )
                {
                    texture_Right_right.wrapMode = wrapMode;
                }
                else if( cubeSide == CubeSide.top )
                {
                    texture_Right_top.wrapMode = wrapMode;
                }
                else if( cubeSide == CubeSide.bottom )
                {
                    texture_Right_bottom.wrapMode = wrapMode;
                }
            }

        } //END SetLoadedTextureWrapMode

        //-----------------------------//
        public Texture GetLoadedTexture( CameraType cameraType, CubeSide cubeSide )
        //-----------------------------//
        {

            if( cameraType == CameraType.Left )
            {
                if( cubeSide == CubeSide.front )
                {
                    return texture_Left_front;
                }
                else if( cubeSide == CubeSide.back )
                {
                    return texture_Left_back;
                }
                else if( cubeSide == CubeSide.left )
                {
                    return texture_Left_left;
                }
                else if( cubeSide == CubeSide.right )
                {
                    return texture_Left_right;
                }
                else if( cubeSide == CubeSide.top )
                {
                    return texture_Left_top;
                }
                else if( cubeSide == CubeSide.bottom )
                {
                    return texture_Left_bottom;
                }
            }
            else if( cameraType == CameraType.Right )
            {
                if( cubeSide == CubeSide.front )
                {
                    return texture_Right_front;
                }
                else if( cubeSide == CubeSide.back )
                {
                    return texture_Right_back;
                }
                else if( cubeSide == CubeSide.left )
                {
                    return texture_Right_left;
                }
                else if( cubeSide == CubeSide.right )
                {
                    return texture_Right_right;
                }
                else if( cubeSide == CubeSide.top )
                {
                    return texture_Right_top;
                }
                else if( cubeSide == CubeSide.bottom )
                {
                    return texture_Right_bottom;
                }
            }

            return null;

        } //END GetLoadedTexture


        //-----------------------------//
        public void SetLoadedBooleansFalse()
        //-----------------------------//
        {

            bool_Left_front_loaded = false;
            bool_Left_back_loaded = false;
            bool_Left_left_loaded = false;
            bool_Left_right_loaded = false;
            bool_Left_top_loaded = false;
            bool_Left_front_loaded = false;

            bool_Right_front_loaded = false;
            bool_Right_back_loaded = false;
            bool_Right_left_loaded = false;
            bool_Right_right_loaded = false;
            bool_Right_top_loaded = false;
            bool_Right_front_loaded = false;

        } //END SetLoadedBooleansFalse

        //-----------------------------//
        public bool AreAllLoadedBooleansTrue()
        //-----------------------------//
        {

            if( bool_Left_front_loaded &&
                bool_Left_back_loaded &&
                bool_Left_left_loaded &&
                bool_Left_right_loaded &&
                bool_Left_top_loaded &&
                bool_Left_front_loaded &&

                bool_Right_front_loaded &&
                bool_Right_back_loaded &&
                bool_Right_left_loaded &&
                bool_Right_right_loaded &&
                bool_Right_top_loaded &&
                bool_Right_front_loaded )
            {
                return true;
            }
            else
            {
                return false;
            }

        } //END AreAllLoadedBooleansTrue

        //-----------------------------//
        public bool GetLoadedBool( CameraType cameraType, CubeSide cubeSide )
        //-----------------------------//
        {
            bool loaded = false;

            if( cameraType == CameraType.Left )
            {
                if( cubeSide == CubeSide.front )
                {
                    loaded = bool_Left_front_loaded;
                }
                else if( cubeSide == CubeSide.back )
                {
                    loaded = bool_Left_back_loaded;
                }
                else if( cubeSide == CubeSide.left )
                {
                    loaded = bool_Left_left_loaded;
                }
                else if( cubeSide == CubeSide.right )
                {
                    loaded = bool_Left_right_loaded;
                }
                else if( cubeSide == CubeSide.top )
                {
                    loaded = bool_Left_top_loaded;
                }
                else if( cubeSide == CubeSide.bottom )
                {
                    loaded = bool_Left_bottom_loaded;
                }
            }
            else if( cameraType == CameraType.Right )
            {
                if( cubeSide == CubeSide.front )
                {
                    loaded = bool_Right_front_loaded;
                }
                else if( cubeSide == CubeSide.back )
                {
                    loaded = bool_Right_back_loaded;
                }
                else if( cubeSide == CubeSide.left )
                {
                    loaded = bool_Right_left_loaded;
                }
                else if( cubeSide == CubeSide.right )
                {
                    loaded = bool_Right_right_loaded;
                }
                else if( cubeSide == CubeSide.top )
                {
                    loaded = bool_Right_top_loaded;
                }
                else if( cubeSide == CubeSide.bottom )
                {
                    loaded = bool_Right_bottom_loaded;
                }
            }

            return loaded;

        } //END GetLoadedBool

        //-----------------------------//
        private void SetLoadedBool( CameraType cameraType, CubeSide cubeSide, bool loaded )
        //-----------------------------//
        {

            if( cameraType == CameraType.Left )
            {
                if( cubeSide == CubeSide.front )
                {
                    bool_Left_front_loaded = loaded;
                }
                else if( cubeSide == CubeSide.back )
                {
                    bool_Left_back_loaded = loaded;
                }
                else if( cubeSide == CubeSide.left )
                {
                    bool_Left_left_loaded = loaded;
                }
                else if( cubeSide == CubeSide.right )
                {
                    bool_Left_right_loaded = loaded;
                }
                else if( cubeSide == CubeSide.top )
                {
                    bool_Left_top_loaded = loaded;
                }
                else if( cubeSide == CubeSide.bottom )
                {
                    bool_Left_bottom_loaded = loaded;
                }
            }
            else if( cameraType == CameraType.Right )
            {
                if( cubeSide == CubeSide.front )
                {
                    bool_Right_front_loaded = loaded;
                }
                else if( cubeSide == CubeSide.back )
                {
                    bool_Right_back_loaded = loaded;
                }
                else if( cubeSide == CubeSide.left )
                {
                    bool_Right_left_loaded = loaded;
                }
                else if( cubeSide == CubeSide.right )
                {
                    bool_Right_right_loaded = loaded;
                }
                else if( cubeSide == CubeSide.top )
                {
                    bool_Right_top_loaded = loaded;
                }
                else if( cubeSide == CubeSide.bottom )
                {
                    bool_Right_bottom_loaded = loaded;
                }
            }

        } //END SetLoadedBool

        //-----------------------------//
        public List<ImageType> GetImageTypeListForTag( ImageTag imageTag )
        //-----------------------------//
        {

            List<ImageType> imageTypeList = new List<ImageType>();

            if( imageTag == ImageTag.All ) { imageTypeList = list_TaggedImages_All; }

            else if( imageTag == ImageTag.TestImageOne ) { imageTypeList = list_TaggedImages_TestImageOne; }
            else if( imageTag == ImageTag.TestImageTwo ) { imageTypeList = list_TaggedImages_TestImageTwo; }

            return imageTypeList;

        } //END GetImageTypeListForTag

        //-----------------------------//
        public int GetImageTypePositionInTagList( ImageType imageType, ImageTag imageTag )
        //-----------------------------//
        {

            int positionInList = 0;

            List<ImageType> imageTypeList = GetImageTypeListForTag( imageTag );

            if( imageTypeList.Contains( imageType ) )
            {
                positionInList = imageTypeList.IndexOf( imageType );
            }

            return positionInList;

        } //END GetImageTypePositionInTagList

        //-----------------------------//
        public int GetImageTypePositionInTagList( ImageType imageType, List<ImageType> imageTypeList )
        //-----------------------------//
        {

            int positionInList = 0;

            if( imageTypeList.Contains( imageType ) )
            {
                positionInList = imageTypeList.IndexOf( imageType );
            }

            return positionInList;

        } //END GetImageTypePositionInTagList

        //----------------------------//
        public void SetIsReadyToTweenToSecondSlot( bool isReady )
        //----------------------------//
        {
            isReadyToTweenToSecondSlot = isReady;

        } //END SetIsReadyToTweenToSecondSlot

        //----------------------------//
        public bool IsReadyToTweenToSecondSlot()
        //----------------------------//
        {
            return isReadyToTweenToSecondSlot;

        } //END IsReadyToTweenToSecondSlot

        //----------------------------//
        public void SetIsTweening( bool tweening )
        //----------------------------//
        {
            _IsTweening = tweening;

        } //END SetIsTweening

        //----------------------------//
        public bool IsTweening()
        //----------------------------//
        {
            return _IsTweening;

        } //END IsTweening

        public Texture CheckPinImageDictionary( string keyName )
        {
            Texture tempImage = null;

            if( dictionary_Block_ImageComponent_Images.ContainsKey( keyName ) )
                tempImage = dictionary_Block_ImageComponent_Images[ keyName ];

            return tempImage;
        }

    } //END Class

} //END Namespace