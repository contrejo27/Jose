using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using System;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace BrandXR
{
    public static class DirectoryHelper
    {

        //------------------------------------------------------------------------//
        // Underlying logic pulled from: https://forum.unity.com/threads/cant-get-type-from-string.484723/
        /// <summary>
        /// Looks through all loaded assemblies and tries to find a matching Type for the passed in string
        /// </summary>
        /// <param name="typeName"> The name of the Type to locate within the loaded assemblies</param>
        /// <returns></returns>
        public static Type FindTypeInAssemblies( string typeName )
        //------------------------------------------------------------------------//
        {
            Type textType = null;

            var assemblies = AppDomain.CurrentDomain.GetAssemblies();

            foreach( var assembly in assemblies )
            {
                textType = assembly.GetType( typeName );

                //If we found a match in this assembly, return that!
                if( textType != null )
                {
                    break;
                }
                else
                {
                    //Debug.Log( "DirectoryHelper.cs FindTypeInAssemblies() Unable to find typeName within assembly ( " + assembly.GetName() + " )" );

                    /*
                    Type[] types = assembly.GetTypes();

                    for( int i = 0; i < types.Length; i++ )
                    {
                        Debug.Log( "DirectoryHelper.cs FindTypeInAssemblies() assembly ( " + assembly.GetName() + " ) : type( " + types[i].Name + " )" );
                    }
                    */
                }
            }

            return textType;

        } //END FindTypeInAssemblies

        //------------------------------------------------------------------------//
        // Example pulled from https://stackoverflow.com/questions/329355/cannot-delete-directory-with-directory-deletepath-true
        /// <summary>
        /// Depth-first recursive delete, with handling for descendant 
        /// directories open in Windows Explorer. Only works in Unity Editor
        /// </summary>
        public static void DeleteDirectory( string path )
        //------------------------------------------------------------------------//
        {
#if UNITY_EDITOR
            foreach( string directory in Directory.GetDirectories( path ) )
            {
                DeleteDirectory( directory );
            }

            try
            {
                Directory.Delete( path, true );
            }
            catch( IOException )
            {
                Directory.Delete( path, true );
            }
            catch( UnauthorizedAccessException )
            {
                Directory.Delete( path, true );
            }
#endif

        } //END DeleteDirectory

        //-------------------------------------------------------//
        // Example pulled from https://forum.unity.com/threads/how-to-get-list-of-assets-at-asset-path.18898/
        /// <summary>
        /// Get the assets within the Unity Editor Assets folder that are the file type you require. Cannot be used to check for AssetBundles!
        /// </summary>
        /// <typeparam name="T">The type of assets you would like to find</typeparam>
        /// <param name="path">The location to search for assets</param>
        /// <returns></returns>
        public static T[] EditorGetAtPath<T>( string path )
        //-------------------------------------------------------//
        {

#if UNITY_EDITOR
            ArrayList al = new ArrayList();
            string[] fileEntries = Directory.GetFiles( Application.dataPath + "/" + path );

            foreach( string fileName in fileEntries )
            {
                int assetPathIndex = fileName.IndexOf( "Assets" );
                string localPath = fileName.Substring( assetPathIndex );

                UnityEngine.Object t = AssetDatabase.LoadAssetAtPath( localPath, typeof( T ) );

                if( t != null )
                    al.Add( t );
            }
            T[] result = new T[ al.Count ];
            for( int i = 0; i < al.Count; i++ )
                result[ i ] = (T)al[ i ];

            return result;
#else
            return null;
#endif

        } //END EditorGetAtPath



    } //END Class

} //END Namespace
                 