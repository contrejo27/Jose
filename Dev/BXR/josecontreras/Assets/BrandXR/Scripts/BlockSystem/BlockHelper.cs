/* BlockHelper.cs
 * 
 * Contains convenience functions used in the BrandXR Block system
 */

using UnityEngine;
using System.Collections;

namespace BrandXR
{
    public class BlockHelper: MonoBehaviour
    {

        // singleton behavior
        private static BlockHelper _instance;

        //--------------------------------------------//
        public static BlockHelper instance
        //--------------------------------------------//
        {
            get
            {
                if( _instance == null )
                {
                    _instance = GameObject.FindObjectOfType<BlockHelper>();
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

        //---------------------------------------------//
        public PrefabFactory.Prefabs GetPrefabType( Block.BlockType type )
        //---------------------------------------------//
        {

            if( type == Block.BlockType.Button ) { return PrefabFactory.Prefabs.bxr_BlockButton; }
            else if( type == Block.BlockType.Text ) { return PrefabFactory.Prefabs.bxr_BlockText; }
            else if( type == Block.BlockType.Image ) { return PrefabFactory.Prefabs.bxr_BlockImage; }
            else if( type == Block.BlockType.Audio ) { return PrefabFactory.Prefabs.bxr_BlockAudio; }
            else if( type == Block.BlockType.Video ) { return PrefabFactory.Prefabs.bxr_BlockVideo; }
            else if( type == Block.BlockType.Event ) { return PrefabFactory.Prefabs.bxr_BlockEvent; }

            return PrefabFactory.Prefabs.bxr_BlockButton;

        } //END GetPrefabType
        

        //--------------------------------------------//
        public static void AddToBrandXRTechParent( Transform parentToBrandXRTech )
        //--------------------------------------------//
        {
            if( GameObject.Find( "BrandXR Tech" ) == null ) { new GameObject( "BrandXR Tech" ); }

            if( parentToBrandXRTech.parent == null || 
              ( parentToBrandXRTech.parent != null && parentToBrandXRTech.parent.name != "BrandXR Tech" ) )
            {
                parentToBrandXRTech.parent = GameObject.Find( "BrandXR Tech" ).transform;
            }

        } //END AddToBrandXRTechParent


    } //END Class

} //END Namespace