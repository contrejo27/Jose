using System.Collections;
using System.Collections.Generic;
using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace BrandXR
{
    public class TagManager: MonoBehaviour
    {
        
        //https://forum.unity3d.com/threads/adding-layer-by-script.41970/
        // will check if the specified layer names are present and add any missing ones
        // it will simply add them from the first open slots so do not depend on any
        // specific order but rather grab layers from the layer names at runtime
        //------------------------------------------------------------------------//
        public static void CheckTags( string[] tagNames )
        //------------------------------------------------------------------------//
        {
            #if UNITY_EDITOR

                SerializedObject manager = new SerializedObject( AssetDatabase.LoadAllAssetsAtPath( "ProjectSettings/TagManager.asset" )[ 0 ] );
                SerializedProperty tagsProp = manager.FindProperty( "tags" );

                List<string> DefaultTags = new List<string>() { "Untagged", "Respawn", "Finish", "EditorOnly", "MainCamera", "Player", "GameController" };

                foreach( string name in tagNames )
                {
                    if( DefaultTags.Contains( name ) ) continue;

                    // check if tag is present
                    bool found = false;
                    for( int i = 0; i < tagsProp.arraySize; i++ )
                    {
                        SerializedProperty t = tagsProp.GetArrayElementAtIndex( i );
                        if( t.stringValue.Equals( name ) ) { found = true; break; }
                    }

                    // if not found, add it
                    if( !found )
                    {
                        tagsProp.InsertArrayElementAtIndex( 0 );
                        SerializedProperty n = tagsProp.GetArrayElementAtIndex( 0 );
                        n.stringValue = name;
                    }
                }

                // save
                manager.ApplyModifiedProperties();

            #endif

        } //END CheckTags
        

    } //END Class

} //END Namespace