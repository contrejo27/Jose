/* BlockFocusManager.cs
 * Stores what BlockView, BlockGroup, and Block is being focused on by the user
 * 
 * Focus changes when a user selects a Block (mouse click, button pressed, touched, or gazed at for a certain amount of time)
 */

using UnityEngine;
using Sirenix.OdinInspector;

namespace BrandXR
{
    public class BlockFocusManager: MonoBehaviour
    {
        
        // singleton behavior
        private static BlockFocusManager _instance;

        //--------------------------------------------//
        public static BlockFocusManager instance
        //--------------------------------------------//
        {
            get
            {
                if( _instance == null )
                {
                    if( GameObject.FindObjectOfType<BlockFocusManager>() == null )
                    {
                        PrefabManager.InstantiatePrefab( PrefabFactory.Prefabs.bxr_BlockFocusManager, null );
                    }

                    _instance = GameObject.FindObjectOfType<BlockFocusManager>();
                    BlockHelper.AddToBrandXRTechParent( _instance.transform );
                }

                return _instance;
            }

        } //END Instance

        private BlockView blockView;
        private BlockGroup blockGroup;
        private Block block;

        //We store what block is being gazed at (usually used in VR Cardboard systems), 
        //we need to know if a BlockButton will soon be selected when the gaze fill completes
        private Block gazeFocusingOnBlock;

        //------------------------------------------------------------//
        /// <summary>
        /// Check if the Gaze system is currently focusing on this block AND this block has not already been selected
        /// </summary>
        /// <param name="isGazeFocusingOnThis">Block to check if being gazed at</param>
        /// <returns></returns>
        public bool IsGazeFocusing( Block isGazeFocusingOnThis )
        //------------------------------------------------------------//
        {
            return ( gazeFocusingOnBlock != null && isGazeFocusingOnThis == gazeFocusingOnBlock );

        } //END IsGazeFocusing

        //-------------------------------------------------------------------------------------//
        /// <summary>
        /// Check if the gaze system is currently focusing on an unselected block within this BlockGroup
        /// </summary>
        /// <param name="isGazeFocusingOnThis">The BlockGroup to check if there is a block that is being gazed at</param>
        /// <param name="checkNestedGroups">Should we also check all of the nested BlockGroups contained within?</param>
        /// <returns></returns>
        public bool IsGazeFocusing( BlockGroup isGazeFocusingOnThis, bool checkNestedGroups )
        //-------------------------------------------------------------------------------------//
        {
            if( gazeFocusingOnBlock != null )
            {
                foreach( Block block in isGazeFocusingOnThis.GetBlocks( null, checkNestedGroups ) )
                {
                    if( block != null && block == gazeFocusingOnBlock )
                    {
                        return true;
                    }
                }
            }

            return false;

        } //END IsGazeFocusing

        //-------------------------------------------------------------------------------------//
        /// <summary>
        /// Check if the gaze system is currently focusing on an unselected block within this BlockView
        /// </summary>
        /// <param name="isGazeFocusingOnThis">The BlockView to check if there is a block that is being gazed at</param>
        /// <param name="checkLinkedBlockView">Should we also check the linked block view if they have a block that is being gaze focused on and is not selected?</param>
        /// <param name="checkNestedGroupsWithinViews">Should we also check all of the nested BlockGroups contained within if they have a block that is being gazed focused on and is not selected?</param>
        /// <returns></returns>
        public bool IsGazeFocusing( BlockView isGazeFocusingOnThis, bool checkLinkedBlockView, bool checkNestedGroupsWithinViews )
        //-------------------------------------------------------------------------------------//
        {
            if( gazeFocusingOnBlock != null )
            {
                foreach( Block block in isGazeFocusingOnThis.GetBlocks( null, checkLinkedBlockView, checkNestedGroupsWithinViews ) )
                {
                    if( block != null && block == gazeFocusingOnBlock )
                    {
                        return true;
                    }
                }
            }

            return false;

        } //END IsGazeFocusing

        //---------------------------------------------------------//
        /// <summary>
        /// The Gaze system is focusing on a block that is not yet selected, keep track of this information
        /// </summary>
        /// <param name="gazingAtBlock">The block that the Gaze system is focusing on that is not yet selected</param>
        public void SetGazeFocusingOnBlock( Block gazingAtBlock )
        //---------------------------------------------------------//
        {
            gazeFocusingOnBlock = gazingAtBlock;

        } //END SetGazeFocusingOnBlock

        //---------------------------------------------------------//
        /// <summary>
        /// The Gaze system has stopped focusing on this block, keep track of this information
        /// </summary>
        /// <param name="noLongerGazingAtThisBlock">The block that was once before being gazed at while not being selected, but is no longer the case</param>
        public void SetGazeNotFocusingOnBlock( Block noLongerGazingAtThisBlock )
        //---------------------------------------------------------//
        {
            //If this block passed in is the same as the latest we've started gazing at, then we're no longer gazing at anything
            //Otherwise, we are gazing at something more recent, so do nothing
            if( gazeFocusingOnBlock == noLongerGazingAtThisBlock )
            {
                gazeFocusingOnBlock = null;
            }

        } //END DeselectGazeFocusingOnBlock

        //-------------------------------------------------------------------//
        //------------------------ GAMEOBJECT HOOKS -------------------------//
        //-------------------------------------------------------------------//
        [FoldoutGroup( "Hooks" ), InfoBox( "Do Not Modify", InfoMessageType.Warning )]
        public GameObject _PrefabManager;


        //---------------------------------//
        public void Start()
        //---------------------------------//
        {
            AddPrefabManagerIfNeeded();

        } //END Start

        //---------------------------------//
        private void AddPrefabManagerIfNeeded()
        //---------------------------------//
        {
            //If the _prefabManager hook does not exist, grab it from the Asset database when in editor
            if( _PrefabManager == null )
            {
#if UNITY_EDITOR
                Debug.LogError( "BlockFocusManager.cs AddPrefabManagerIfNeeded() Unable to create PrefabManager as the _PrefabManager hook on this component is null, using AssetDatabase lookup as an Editor only backup" );
                _PrefabManager = UnityEditor.AssetDatabase.LoadAssetAtPath<GameObject>( "Assets/BrandXR/Prefabs/bxr_PrefabManager.prefab" );
#else
                Debug.LogError( "BlockFocusManager.cs AddPrefabManagerIfNeeded() Unable to create PrefabManager as the _PrefabManager hook on this component is null" );
#endif
            }

            //If the PrefabManager does not exist, create it and parent it to the "BrandXdR Tech" GameObject
            if( GameObject.FindObjectOfType<PrefabManager>() == null )
            {
                GameObject go = GameObject.Instantiate( _PrefabManager );
                go.name = "bxr_PrefabManager";

                if( GameObject.Find( "BrandXR Tech" ) == null ) { new GameObject( "BrandXR Tech" ); }

                GameObject bxr = GameObject.Find( "BrandXR Tech" );
                go.transform.parent = bxr.transform;
            }

        } //END AddPrefabManagerIfNeeded

        //------------------------//
        public void SetFocus( Block block )
        //------------------------//
        {

            if( block != null )
            {
                this.block = block;

                if( block.GetGroup() != null )
                {
                    this.blockGroup = block.GetGroup();

                    if( block.GetGroup().GetView() )
                    {
                        this.blockView = block.GetGroup().GetView();
                    }
                    else
                    {
                        this.blockView = null;
                    }
                }
                else
                {
                    this.blockGroup = null;
                    this.blockView = null;
                }
            }
            else
            {
                this.block = null;
                this.blockGroup = null;
                this.blockView = null;
            }

        } //END SetFocus

        //-----------------------------//
        public Block GetFocusBlock()
        //-----------------------------//
        {

            if( block != null )
            {
                return block;
            }

            return null;

        } //END GetFocusBlock

        //-----------------------------//
        public BlockGroup GetFocusBlockGroup()
        //-----------------------------//
        {

            if( blockGroup != null )
            {
                return blockGroup;
            }

            return null;

        } //END GetFocusBlockGroup

        //-----------------------------//
        public BlockView GetFocusBlockView()
        //-----------------------------//
        {

            if( blockView != null )
            {
                return blockView;
            }

            return null;

        } //END GetFocusBlockView

        //------------------------------//
        /// <summary>
        /// Was this block that last one selected via Input?
        /// </summary>
        /// <param name="block">The block to check</param>
        /// <returns></returns>
        public bool IsFocused( Block block )
        //------------------------------//
        {

            if( block != null )
            {
                if( this.block != null )
                {
                    return this.block == block;
                }
            }

            return false;

        } //END IsFocused

        //------------------------------//
        /// <summary>
        /// Check if a blockGroup has a block that has been selected.
        /// </summary>
        /// <param name="blockGroup">The blockGroup to check if it has a block that has been selected</param>
        /// <param name="checkNestedGroups">Should we also check any nested groups to see if they have a block that has been selected?</param>
        /// <returns></returns>
        public bool IsFocused( BlockGroup blockGroup, bool checkNestedGroups = true )
        //------------------------------//
        {
            //If the blockGroup passed in is not null
            if( blockGroup != null )
            {
                //And if the last block group selected is not empty
                if( this.blockGroup != null )
                {
                    //Then check if this is the currently selected blockGroup
                    if( this.blockGroup == blockGroup )
                    {
                        return true;
                    }
                }
            }

            //So the passed in blockGroup was not the last selected, 
            //let's check the nestedBlockGroups and see if any of them are the one that's selected
            if( checkNestedGroups )
            {
                if( this.blockGroup != null && this.blockGroup.GetNestedBlockGroups() != null && this.blockGroup.GetNestedBlockGroups().Count > 0 )
                {
                    foreach( BlockGroup bGroup in this.blockGroup.GetNestedBlockGroups() )
                    {
                        if( this.blockGroup == bGroup )
                        {
                            return true;
                        }
                    }
                }
            }

            return false;

        } //END IsFocused

        //------------------------------//
        /// <summary>
        /// Check to see if a BlockView has a Block that has been selected
        /// </summary>
        /// <param name="blockView">The BlockView to check if there has been a Block that has been selected</param>
        /// <param name="checkLinkedBlockView">Should we check the linkedBlockView to see if it has a block that has been selected</param>
        /// <returns></returns>
        public bool IsFocused( BlockView blockView, bool checkLinkedBlockView = true )
        //------------------------------//
        {
            //Make sure the blockView passed in and the stored BlockView are not null
            if( blockView != null && this.blockView != null )
            {
                //First check the blockView passed in
                if( this.blockView == blockView )
                {
                    return true;
                }

                //Next check the linked block view
                else if( checkLinkedBlockView && blockView.linkedBlockView == this.blockView )
                {
                    return true;
                }

            }

            return false;

        } //END IsFocused

    } //END Class

} //END Namespace