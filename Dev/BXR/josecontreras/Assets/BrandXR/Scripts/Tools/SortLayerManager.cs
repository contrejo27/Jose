using System.Collections;
using System.Collections.Generic;
using UnityEngine;

#if UNITY_EDITOR
using System.Reflection;
using UnityEditor;
#endif

namespace BrandXR
{
    public class SortLayerManager: MonoBehaviour
    {

        //https://forum.unity3d.com/threads/adding-layer-by-script.41970/
        // will check if the specified layer names are present and add any missing ones
        // it will simply add them from the first open slots so do not depend on any
        // specific order but rather grab layers from the layer names at runtime
        //------------------------------------------------------------------------//
        public static void CheckSortLayers( string[] tagNames )
        //------------------------------------------------------------------------//
        {
            #if UNITY_EDITOR

                SerializedObject manager = new SerializedObject( AssetDatabase.LoadAllAssetsAtPath( "ProjectSettings/TagManager.asset" )[ 0 ] );
                SerializedProperty sortLayersProp = manager.FindProperty( "m_SortingLayers" );

                //for (int i = 0; i < sortLayersProp.arraySize; i++)
                //{ // used to figure out how all of this works and what properties values look like
                //    SerializedProperty entry = sortLayersProp.GetArrayElementAtIndex(i);
                //    SerializedProperty name = entry.FindPropertyRelative("name");
                //    SerializedProperty unique = entry.FindPropertyRelative("uniqueID");
                //    SerializedProperty locked = entry.FindPropertyRelative("locked");
                //    Debug.Log(name.stringValue + " => " + unique.intValue + " => " + locked.boolValue);
                //}

                foreach( string name in tagNames )
                {
                    // check if tag is present
                    bool found = false;
                    for( int i = 0; i < sortLayersProp.arraySize; i++ )
                    {
                        SerializedProperty entry = sortLayersProp.GetArrayElementAtIndex( i );
                        SerializedProperty t = entry.FindPropertyRelative( "name" );
                        if( t.stringValue.Equals( name ) ) { found = true; break; }
                    }

                    // if not found, add it
                    if( !found )
                    {
                        manager.ApplyModifiedProperties();
                        AddSortingLayer();
                        manager.Update();

                        int idx = sortLayersProp.arraySize - 1;
                        SerializedProperty entry = sortLayersProp.GetArrayElementAtIndex( idx );
                        SerializedProperty t = entry.FindPropertyRelative( "name" );
                        t.stringValue = name;
                    }
                }

                // save
                manager.ApplyModifiedProperties();

            #endif

        } //END CheckSortLayers



        #if UNITY_EDITOR
            // you need 'using System.Reflection;' for these
            private static Assembly editorAsm;
            private static MethodInfo AddSortingLayer_Method;
        #endif

        /// <summary> add a new sorting layer with default name </summary>
        //------------------------------------------------------------------------//
        public static void AddSortingLayer()
        //------------------------------------------------------------------------//
        {
            #if UNITY_EDITOR

                if( AddSortingLayer_Method == null )
                {
                    if( editorAsm == null ) editorAsm = Assembly.GetAssembly( typeof( Editor ) );
                    System.Type t = editorAsm.GetType( "UnityEditorInternal.InternalEditorUtility" );
                    AddSortingLayer_Method = t.GetMethod( "AddSortingLayer", ( BindingFlags.Static | BindingFlags.NonPublic ), null, new System.Type[ 0 ], null );
                }

                AddSortingLayer_Method.Invoke( null, null );

            #endif

        } //END AddSortingLayer




    } //END Class

} //END Namespace