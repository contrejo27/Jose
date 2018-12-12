/************************************************************
 * Author: Derrick Barra
 * Date: 05/23/13
 * Purpose: Eliminates the need to load Prefabs from the Resources folder
 *          Works together with the PrefabFactory.cs class to
 * 			hold onto a list of prefabs that can be accessed by using
 * 			the functions in this class.
 * *********************************************************/

using System.Collections.Generic;
using UnityEngine;

namespace BrandXR
{    
    public class PrefabManager: MonoBehaviour
    {
        private static bool showDebug = false;

        private static Transform _parentTo;
        
        //Singleton behavior
        private static PrefabManager _instance;

        //--------------------------------------------//
        public static PrefabManager instance
        //--------------------------------------------//
        {
            get
            {
                if( _instance == null )
                {
                    if( GameObject.FindObjectOfType<PrefabManager>() != null )
                    {
                        _instance = GameObject.FindObjectOfType<PrefabManager>();
                    }
                    else
                    {
                        GameObject go = null;

                        if( GameObject.Find("bxr_PrefabManager") != null )
                        {
                            go = GameObject.Find( "bxr_PrefabManager" );
                        }
                        else
                        {
                            go = new GameObject( "bxr_PrefabManager" );
                        }

                        System.Type type = ComponentHelper.FindType( "BrandXR.PrefabManager" );

                        if( type != null )
                        {
                            _instance = (PrefabManager)go.AddComponent( type );
                        }
                    }

                    if( _instance != null )
                    {
                        BlockHelper.AddToBrandXRTechParent( _instance.transform );
                    }
                }

                return _instance;
            }

        } //END Instance

        //--------------------------------------------//
        public void Reset()
        //--------------------------------------------//
        {
            BlockHelper.AddToBrandXRTechParent( transform );

        } //END Reset

        //--------------------------------------------//
        public void Awake()
        //--------------------------------------------//
        {
            BlockHelper.AddToBrandXRTechParent( transform );

            if( this.transform.parent != null )
            {
                _parentTo = this.transform.parent;
            }
            else
            {
                _parentTo = this.transform;
            }

            DestroyDuplicateInstance();
            
            if( transform.parent == null )
            {
                DontDestroyOnLoad( transform.gameObject );
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

        //------------------------------------------------------//
        public static GameObject GetPrefab( PrefabFactory.Prefabs prefab )
        //------------------------------------------------------//
        {

            return PrefabFactory.instance.PrefabsDictionary[ prefab.ToString() ];

        } //END GetPrefab()


        //------------------------------------------------------//
        public static GameObject GetExistingPrefab( PrefabFactory.Prefabs name )
        //------------------------------------------------------//
        {

            if( PrefabFactory.instance.ExistingPrefabs.ContainsKey( name.ToString() ) )
            {
                return PrefabFactory.instance.ExistingPrefabs[ name.ToString() ];
            }
            else
            {
                return null;
            }

        } //END GetPrefab()


        //------------------------------------------------------//
        public static GameObject InstantiatePrefab( PrefabFactory.Prefabs prefab )
        //------------------------------------------------------//
        {

            return InstantiatePrefab( prefab, _parentTo );

        } //END InstantiatePrefab()

        //------------------------------------------------------//
        public static GameObject InstantiatePrefab( PrefabFactory.Prefabs prefab, Transform addToParent )
        //------------------------------------------------------//
        {

            if( PrefabFactory.instance.PrefabsDictionary.ContainsKey( prefab.ToString() ) && PrefabFactory.instance.PrefabsDictionary[ prefab.ToString() ] != null )
            {
                if( showDebug ) { Debug.Log( "PrefabManager.cs InstantiatePrefab( " + prefab.ToString() + " )" ); }

                GameObject clone = (GameObject)Instantiate( PrefabFactory.instance.PrefabsDictionary[ prefab.ToString() ] );

                clone.name = RemoveCloneFromNameString( clone.name );

                clone.name = NameDifferentlyIfSameKeyExistsInPrefabsDictionary( clone.name );

                AddToExistingPrefabsDictionary( clone );

                if( addToParent != null )
                {
                    clone.transform.SetParent( addToParent );
                }

                return clone;
            }
            else
            {
                if( showDebug )
                {
                    Debug.Log( "InstantiatePrefab() error, unable to find " + prefab.ToString() + " in Dictionary below" );

                    foreach( KeyValuePair<string, GameObject> pair in PrefabFactory.instance.PrefabsDictionary )
                    {
                        Debug.Log( "PrefabFactory.cs ... PrefabsDictionary Key{ " + pair.Key + " } : Value{ " + pair.Value + " }" );
                    }
                }

                if( showDebug ) { Debug.Log( "Contains Key for " + prefab.ToString() + " = " + ( PrefabFactory.instance.PrefabsDictionary.ContainsKey( prefab.ToString() ) ) ); }
                if( showDebug ) { Debug.Log( "PrefabManager.cs InstantiatePrefab( " + prefab.ToString() + " ) failed to find prefab! PrefabFactory.instance =" + PrefabFactory.instance.ToString() + ", isPrefabNull = " + ( PrefabFactory.instance.PrefabsDictionary[ prefab.ToString() ] != null ).ToString() ); }

                return null;
            }

        } //END InstantiatePrefab()

        




        //------------------------------------------------------//
        private static string RemoveCloneFromNameString( string name )
        //------------------------------------------------------//
        {

            string[] words = name.Split( '(' );
            return words[ 0 ];

        } //END RemoveCloneFromNameString()

        //------------------------------------------------------//
        private static string NameDifferentlyIfSameKeyExistsInPrefabsDictionary( string name )
        //------------------------------------------------------//
        {

            int i = 0;

            while( PrefabFactory.instance.ExistingPrefabs.ContainsKey( name ) )
            {

                if( !name.EndsWith( ")" ) )
                {
                    name += "(" + i.ToString() + ")";
                }
                else
                {
                    name = name.Replace( ( i - 1 ).ToString(), ( i ).ToString() );
                }

                i++;

            }

            return name;

        } //END NameDifferentlyIfSameKeyExistsInPrefabsDictionary()


        //------------------------------------------------------//
        public static void AddToExistingPrefabsDictionary( GameObject clone )
        //------------------------------------------------------//
        {

            if( !PrefabFactory.instance.ExistingPrefabs.ContainsKey( clone.name ) )
            {
                PrefabFactory.instance.ExistingPrefabs.Add( clone.name, clone );
            }

        } //END AddToExistingPrefabsDictionary()


        //------------------------------------------------------//
        public static void DestroyExistingPrefab( PrefabFactory.Prefabs name )
        //------------------------------------------------------//
        {

            if( PrefabFactory.instance.ExistingPrefabs.ContainsKey( name.ToString() ) )
            {

                if( PrefabFactory.instance.ExistingPrefabs[ name.ToString() ] != null )
                {
                    Destroy( PrefabFactory.instance.ExistingPrefabs[ name.ToString() ] );
                    PrefabFactory.instance.ExistingPrefabs.Remove( name.ToString() );
                }

            }

        } //END DestroyExistingPrefab()

        //------------------------------------------------------//
        public static void DestroyExistingPrefab( string name )
        //------------------------------------------------------//
        {

            if( PrefabFactory.instance.ExistingPrefabs.ContainsKey( name ) )
            {
                Destroy( PrefabFactory.instance.ExistingPrefabs[ name ] );
                PrefabFactory.instance.ExistingPrefabs.Remove( name );
            }

        } //END DestroyExistingPrefab()

    } //END Class

} //END Namespace