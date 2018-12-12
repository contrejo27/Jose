using System.Collections.Generic;
using System.IO;
using UnityEngine;

namespace BrandXR
{
    static class DatabaseStringHelper
    {
        public enum StringStyle
        {
            NoSettings,
            WithEscapeUriAndSystemPathCombine
        }

        //----------------------------------------------------------//
        public static bool IsEmptyDatabaseEntry( string entry )
        //----------------------------------------------------------//
        {
            if( string.IsNullOrEmpty( entry ) || entry == "-99" || entry == "emptystring" )
            {
                return true;
            }
            else
            {
                return false;
            }

        } //END IsEmptyDatabaseEntry

        //----------------------------------------------------------//
        public static string FormatDatabaseID( int intID )
        //----------------------------------------------------------//
        {
            if( intID != -99 )
            {
                return intID.ToString();
            }
            else
            {
                return "-99";
            }

        } //END FormatDatabaseID

        //----------------------------------------------------------//
        public static string FormatDatabaseID( string stringID )
        //----------------------------------------------------------//
        {
            if( !IsEmptyDatabaseEntry( stringID ) )
            {
                return stringID;
            }
            else
            {
                return "-99";
            }

        } //END FormatDatabaseID

        //----------------------------------------------------------//
        public static string FormatDatabaseIDs( int intIDs )
        //----------------------------------------------------------//
        {
            if( intIDs != -99 )
            {
                return intIDs.ToString();
            }
            else
            {
                return "-99";
            }

        } //END FormatDatabaseIDs

        //----------------------------------------------------------//
        public static string FormatDatabaseIDs( string stringIDs )
        //----------------------------------------------------------//
        {
            if( !IsEmptyDatabaseEntry( stringIDs ) )
            {
                return stringIDs;
            }
            else
            {
                return "-99";
            }

        } //END FormatDatabaseIDs

        //----------------------------------------------------------//
        public static string FormatDatabaseStrings( string strings )
        //----------------------------------------------------------//
        {
            if( !IsEmptyDatabaseEntry( strings ) )
            {
                return strings;
            }
            else
            {
                return "emptystring";
            }

        } //END FormatDatabaseStrings

        //----------------------------------------------------------//
        public static string FormatDatabaseBool( bool boolean )
        //----------------------------------------------------------//
        {

            if( boolean ) { return "1"; }

            return "0";

        } //END FormatDatabaseBool



        //----------------------------------------------------------//
        public static string FormatDatabaseText( string text, bool spacesToUnderscores )
        //----------------------------------------------------------//
        {
            if( !IsEmptyDatabaseEntry( text ) )
            {
                if( spacesToUnderscores ) { return text.Replace( ' ', '_' ); }
                else { return text; }
            }
            else
            {
                return "emptystring";
            }

        } //END FormatDatabaseText

        //----------------------------------------------------------//
        public static string FormatDatabaseEmailList( List<string> textList )
        //----------------------------------------------------------//
        {
            string combinedString = "";

            if( textList != null && textList.Count > 0 )
            {

                for( int i = 0; i < textList.Count; i++ )
                {

                    if( !IsEmptyDatabaseEntry( textList[ i ] ) )
                    {
                        combinedString += textList[ i ];

                        if( i < textList.Count - 1 )
                        {
                            combinedString += "_";
                        }
                    }
                }

            }

            return combinedString;

        } //END FormatDatabaseText



        //----------------------------------------------------------//
        public static string FormatDatabaseVector3( string stringVector3 )
        //----------------------------------------------------------//
        {
            if( !IsEmptyDatabaseEntry( stringVector3 ) )
            {
                return stringVector3;
            }
            else
            {
                return "emptystring";
            }

        } //END FormatDatabaseVector3

        //----------------------------------------------------------//
        public static string FormatDatabaseURL( string url )
        //----------------------------------------------------------//
        {
            if( !IsEmptyDatabaseEntry( url ) )
            {
                return url;
            }
            else
            {
                return "emptystring";
            }

        } //END FormatDatabaseURL

        //-----------------------------//
        public static string CreateStreamingAssetsPath( string filePath, StringStyle style )
        //-----------------------------//
        {

            if( style == StringStyle.NoSettings )
            {
                return CreateStreamingAssetsPathWithoutEscapeUriOrSytemPathCombine( filePath );
            }
            else if( style == StringStyle.WithEscapeUriAndSystemPathCombine )
            {
                return CreateStreamingAssetsPathWithAll( filePath );
            }

            return "";

        } //END CreateStreamingAssetsPath

        //-----------------------------//
        private static string CreateStreamingAssetsPathWithAll( string filePath )
        //-----------------------------//
        {

            string assetsPath = GetStreamingAssetPath();

            if( Application.platform == RuntimePlatform.WindowsEditor || Application.platform == RuntimePlatform.OSXEditor || Application.platform == RuntimePlatform.WindowsPlayer || Application.platform == RuntimePlatform.OSXPlayer )
            {
                return System.Uri.EscapeUriString( "file://" + System.IO.Path.Combine( assetsPath, filePath ) );
            }
            else if( Application.platform == RuntimePlatform.Android )
            {
                return System.Uri.EscapeUriString( System.IO.Path.Combine( assetsPath, filePath ) );
            }
            else if( Application.platform == RuntimePlatform.IPhonePlayer )
            {
                return System.Uri.EscapeUriString( "file://" + System.IO.Path.Combine( assetsPath, filePath ) );
            }

            return "";

        } //END CreateStreamingAssetsPathWithAll

        //-----------------------------//
        private static string CreateStreamingAssetsPathWithoutEscapeUri( string filePath )
        //-----------------------------//
        {

            string assetsPath = GetStreamingAssetPath();

            if( Application.platform == RuntimePlatform.WindowsEditor || Application.platform == RuntimePlatform.OSXEditor || Application.platform == RuntimePlatform.WindowsPlayer || Application.platform == RuntimePlatform.OSXPlayer )
            {
                return "file://" + System.IO.Path.Combine( assetsPath, filePath );
            }
            else if( Application.platform == RuntimePlatform.Android )
            {
                return System.IO.Path.Combine( assetsPath, filePath );
            }
            else if( Application.platform == RuntimePlatform.IPhonePlayer )
            {
                return "file://" + System.IO.Path.Combine( assetsPath, filePath );
            }

            return "";

        } //END CreateStreamingAssetsPathWithoutEscapeUri

        //-----------------------------//
        private static string CreateStreamingAssetsPathWithoutEscapeUriOrSytemPathCombine( string filePath )
        //-----------------------------//
        {

            string assetsPath = GetStreamingAssetPath();

            if( Application.platform == RuntimePlatform.WindowsEditor || Application.platform == RuntimePlatform.OSXEditor || Application.platform == RuntimePlatform.WindowsPlayer || Application.platform == RuntimePlatform.OSXPlayer )
            {
                return "file://" + assetsPath + "/" + filePath;
            }
            else if( Application.platform == RuntimePlatform.Android )
            {
                return assetsPath + "/" + filePath;
            }
            else if( Application.platform == RuntimePlatform.IPhonePlayer )
            {
                return "file://" + assetsPath + "/" + filePath;
            }

            return "";

        } //END CreateStreamingAssetsPathWithoutEscapeUriOrSytemPathCombine

        //-----------------------------//
        public static string CreateStreamingAssetsPathForFileExistsCheck( string filePath )
        //-----------------------------//
        {

            string assetsPath = GetStreamingAssetPath();

            if( Application.platform == RuntimePlatform.WindowsEditor || Application.platform == RuntimePlatform.OSXEditor || Application.platform == RuntimePlatform.WindowsPlayer || Application.platform == RuntimePlatform.OSXPlayer )
            {
                return assetsPath + "/" + filePath;
            }
            else if( Application.platform == RuntimePlatform.Android )
            {
                return assetsPath + "/" + filePath;
            }
            else if( Application.platform == RuntimePlatform.IPhonePlayer )
            {
                return assetsPath + "/" + filePath;
            }

            return "";

        } //END CreateStreamingAssetsPathForFileExistsCheck

        //-----------------------------//
        public static string CreatePersistentDataPath( string filePath )
        //-----------------------------//
        {

            return System.IO.Path.Combine( Application.persistentDataPath, filePath );

        } //END CreatePersistentDataPath

        //-----------------------------//
        public static string GetCorrectVideoPath()
        //-----------------------------//
        {

            if( Application.dataPath.Contains( ".obb" ) )
            {
                return Application.persistentDataPath;
            }
            else
            {
                return Application.streamingAssetsPath;
            }

        } //END GetCorrectVideoPath

        //-----------------------------//
        public static string GetStreamingAssetPath()
        //-----------------------------//
        {

            return Application.streamingAssetsPath;

        } //END GetStreamingAssetPath

        //----------------------------//
        public static string GetFileNameWithExtension( string url )
        //----------------------------//
        {

            List<string> paths = new List<string>();
            string fileName = "";

            //Remove any folder paths from the url
            if( url.Contains( "/" ) )
            {
                paths = new List<string>( url.Split( '/' ) );
                fileName = paths[ paths.Count - 1 ];
            }

            return fileName;

        } //END GetFileNameWithExtension


        
        //-------------------------------------//
        public static string CreatePersistentDataPathFilenameFromURL( string url )
        //-------------------------------------//
        {

            return Application.persistentDataPath + "/" + GenerateNameFromUrl( url );

        } //END CreatePersistentDataPathFilenameFromURL

        //-------------------------------------//
        public static string GenerateNameFromUrl( string url )
        //-------------------------------------//
        {

            // Replace useless chareacters with UNDERSCORE
            string uniqueName = url.Replace( "://", "_" ).Replace( ".", "_" ).Replace( "/", "_" ).Replace( "?", "_" ).Replace( "=", "_" );

            // Replace last UNDERSCORE with a DOT
            uniqueName = uniqueName.Substring( 0, uniqueName.LastIndexOf( '_' ) ) + Path.GetExtension( url );
            
            //Debug.Log( "GenerateNameFromURL() uniqueName = " + uniqueName );

            return uniqueName;

        } //END GenerateNameFromUrl

    } //END Class

} //END Namespace