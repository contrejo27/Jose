using System.Collections;
using System.Collections.Generic;
using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace BrandXR
{
    public class LayerManager: MonoBehaviour
    {
        
        //https://forum.unity3d.com/threads/adding-layer-by-script.41970/
        // will check if the specified layer names are present and add any missing ones
        // it will simply add them from the first open slots so do not depend on any
        // specific order but rather grab layers from the layer names at runtime
        //-----------------------------------------------------------------------------------//
        public static void CheckLayers( string[] layerNames )
        //-----------------------------------------------------------------------------------//
        {
             #if UNITY_EDITOR

                SerializedObject manager = new SerializedObject( AssetDatabase.LoadAllAssetsAtPath( "ProjectSettings/TagManager.asset" )[ 0 ] );

                #if !UNITY_4
                    SerializedProperty layersProp = manager.FindProperty( "layers" );
                #endif

                foreach( string name in layerNames )
                {
                    // check if layer is present
                    bool found = false;

                    for( int i = 0; i <= 31; i++ )
                    {
                        #if UNITY_4
                            string nm = "User Layer " + i;
                            SerializedProperty sp = manager.FindProperty(nm);
                        #else
                            SerializedProperty sp = layersProp.GetArrayElementAtIndex( i );
                        #endif

                        if( sp != null && name.Equals( sp.stringValue ) )
                        {
                            found = true;
                            break;
                        }
                    }

                    // not found, add into 1st open slot
                    if( !found )
                    {
                        SerializedProperty slot = null;
                        for( int i = 8; i <= 31; i++ )
                        {
                            #if UNITY_4
                                string nm = "User Layer " + i;
                                SerializedProperty sp = manager.FindProperty(nm);
                            #else
                                SerializedProperty sp = layersProp.GetArrayElementAtIndex( i );
                            #endif

                            if( sp != null && string.IsNullOrEmpty( sp.stringValue ) )
                            {
                                slot = sp;
                                break;
                            }
                        }

                        if( slot != null )
                        {
                            slot.stringValue = name;
                        }
                        else
                        {
                            Debug.LogError( "Could not find an open Layer Slot for: " + name );
                        }
                    }
                }

                // save
                manager.ApplyModifiedProperties();

            #endif

        } //END CheckLayers


    } //END Class

} //END Namespace