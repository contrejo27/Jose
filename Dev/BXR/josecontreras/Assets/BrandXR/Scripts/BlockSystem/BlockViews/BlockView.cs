/* BlockView.cs
 * Responsible for holding onto BlockGroups and accessing them
 * 
 * Each "Screen" in the BrandXR platform is split into two BlockView's, one for 2D and the other for 3D content
 * These two "Views" are connected via the linkedBlockView variable. Manipulating one View can change the other.
 * 
 * For Example, if you use SendCommand(), there is an optional parameter to call the same command on the 
 * corresponding linkedBlockView

 * This component must be attached to a GameObject that is parented to either the 2D or 3D Canvas of the BlockManager
 * based on the ViewType enum
 */

using Sirenix.OdinInspector;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace BrandXR
{
    public class BlockView: MonoBehaviour
    {
        [ShowIf( "IsViewType2D" ), TitleGroup( "BlockView", "Contains BlockGroups. Linked to a sibling BlockView" )]
        public int dummy1 = 0;

        //-------------------------------------------------------------------//
        //------------------------ GAMEOBJECT HOOKS -------------------------//
        //-------------------------------------------------------------------//
        [FoldoutGroup( "Hooks" ), InfoBox( "Do Not Modify", InfoMessageType.Warning )]
        public GameObject _PrefabManager;

        //-------------------------------------------------------------------//
        //------------------------ VARIABLES --------------------------------//
        //-------------------------------------------------------------------//
        public enum BlockViewType
        {
            BlockView
        }
        public virtual BlockViewType GetBlockViewType() { return BlockViewType.BlockView; }

        //A simple way to add BlockGroups in the Unity Editor and have them auto-attach to this BlockView
#if UNITY_EDITOR
        [ShowIf( "IsViewType2D" ), FoldoutGroup( "Create Block Groups" ), InfoBox( "Convenience functionality to easily create Block Groups that work in 2D space and parent them to this 2D BlockView.\n\nA BlockGroup can contain either Blocks or nested Block Groups." )]
        public BlockGroup.BlockGroupType2D blockGroupType2D = BlockGroup.BlockGroupType2D.BlockGroup;

        [ShowIf( "IsViewType3D" ), FoldoutGroup( "Create Block Groups" ), InfoBox( "Convenience functionality to easily create Block Groups that work in 3D space and parent them to this 3D BlockView.\n\nA BlockGroup can contain either Blocks or nested Block Groups." )]
        public BlockGroup.BlockGroupType3D blockGroupType3D = BlockGroup.BlockGroupType3D.BlockGroup;

        [Button( "Add Block Group", ButtonSizes.Large ), FoldoutGroup( "Create Block Groups" )]
        public BlockGroup AddBlockGroup()
        {
            GameObject group = null;

            if( IsViewType2D() )
            {
                group = (GameObject)AssetDatabase.LoadAssetAtPath( "Assets/BrandXR/Prefabs/BlockSystem/BlockGroups/bxr_" + blockGroupType2D + ".prefab", typeof( GameObject ) );
            }
            else if( IsViewType3D() )
            {
                group = (GameObject)AssetDatabase.LoadAssetAtPath( "Assets/BrandXR/Prefabs/BlockSystem/BlockGroups/bxr_" + blockGroupType3D + ".prefab", typeof( GameObject ) );
            }

            GameObject go = Instantiate( group );

            if( IsViewType2D() )
            {
                go.name = "bxr_" + blockGroupType2D;
            }
            else if( IsViewType3D() )
            {
                go.name = "bxr_" + blockGroupType3D;
            }
            
            go.transform.parent = transform;

            return go.GetComponent<BlockGroup>();
        }
#endif

        //Should this View be attached to the 2D or 3D Canvas in the BlockManager?
        public enum ViewType
        {
            TwoDimensional,
            ThreeDimensional
        }
        
        [Space( 15f ), FoldoutGroup("Hooks"), InfoBox("Do Not Modify.\n\nControls whether this BlockView contains BlockGroups that can work in 2D space or in 3D space", InfoMessageType.Warning)]
        public ViewType viewType = ViewType.ThreeDimensional;
        public bool IsViewType2D() { return viewType == ViewType.TwoDimensional; }
        public bool IsViewType3D() { return viewType == ViewType.ThreeDimensional; }

        //Link to the linked BlockView opposite of this one (If this ViewType is 2D, the opposite BlockView will have 3D)
        [Space(15f), FoldoutGroup("Hooks"), InfoBox("Reference to the sibling BlockView that is also attached to the BlockManager.\n\nWhen sending messages to this BlockView you can optionally send the same message to the linkedBlockView")]
        public BlockView linkedBlockView;
        
        //List of all the BlockGroups childed to this View
        protected List<BlockGroup> blockGroups = new List<BlockGroup>();


        

        //--------------------------------//
        public void Start()
        //--------------------------------//
        {
            AddPrefabManagerIfNeeded();

            AddBlockGroupsToList();

            AddLinkedBlockViewIfMissing();

            ParentToBlockManagerTransform();

            BlockTypeChanged();

            SetBlockGroupsToViewType();

        } //END Start

        //---------------------------------//
        private void AddPrefabManagerIfNeeded()
        //---------------------------------//
        {
            //If the _prefabManager hook does not exist, grab it from the Asset database when in editor
            if( _PrefabManager == null )
            {
#if UNITY_EDITOR
                Debug.LogError( "BlockView.cs AddPrefabManagerIfNeeded() Unable to create PrefabManager as the _PrefabManager hook on this component is null, using AssetDatabase lookup as an Editor only backup" );
                _PrefabManager = UnityEditor.AssetDatabase.LoadAssetAtPath<GameObject>( "Assets/BrandXR/Prefabs/bxr_PrefabManager.prefab" );
#else
                Debug.LogError( "BlockView.cs AddPrefabManagerIfNeeded() Unable to create PrefabManager as the _PrefabManager hook on this component is null" );
#endif
            }

            //If the PrefabManager does not exist, create it and parent it to the "BrandXR Tech" GameObject
            if( GameObject.FindObjectOfType<PrefabManager>() == null )
            {
                GameObject go = GameObject.Instantiate( _PrefabManager );
                go.name = "bxr_PrefabManager";

                if( GameObject.Find( "BrandXR Tech" ) == null ) { new GameObject( "BrandXR Tech" ); }

                GameObject bxr = GameObject.Find( "BrandXR Tech" );
                go.transform.parent = bxr.transform;
            }

        } //END AddPrefabManagerIfNeeded


        //---------------------------------//
        private void AddBlockGroupsToList()
        //---------------------------------//
        {
            blockGroups = null;

            if( GetComponentsInChildren<BlockGroup>() != null )
            {
                blockGroups = GetComponentsInChildren<BlockGroup>().ToList();
            }

        } //END AddBlockGroupsToList

        //---------------------------------//
        public void AddLinkedBlockViewIfMissing()
        //---------------------------------//
        {
            //If we don't have a linkedBlockView, make one and connect it to this BlockView
            if( linkedBlockView == null )
            {
                linkedBlockView = PrefabManager.InstantiatePrefab( PrefabFactory.Prefabs.bxr_BlockView ).GetComponent<BlockView>();
                linkedBlockView.linkedBlockView = this;

                if( viewType == ViewType.ThreeDimensional ) { linkedBlockView.viewType = ViewType.TwoDimensional; }
                else if( viewType == ViewType.TwoDimensional ) { linkedBlockView.viewType = ViewType.ThreeDimensional; }
            }

        } //END AddLinkedBlockViewIfMissing

        //---------------------------------//
        public void ParentToBlockManagerTransform()
        //---------------------------------//
        {

            BlockManager.instance.AddBlockViewToManager( this, viewType );

        } //END ParentToBlockManagerTransform

        //---------------------------------//
        private void BlockTypeChanged()
        //---------------------------------//
        {
            if( BlockManager.instance != null )
            {
                BlockManager.instance.SetBlockViewToParent( this, viewType );
            }

            //Set the transform to either a 'RectTransform' or a regular 'Transform', depending on 2D or 3D group type
            if( viewType == ViewType.TwoDimensional )
            {
                //BlockGroups always have a RectTransform, let's grab it and set it to be part of the 2D canvas
                RectTransform rect = this.gameObject.GetComponent<RectTransform>();

                rect.pivot = new Vector2( .5f, .5f );
                rect.anchorMin = new Vector2( 0f, 0f );
                rect.anchorMax = new Vector2( 1f, 1f );
                rect.SetOffsets( 0f, 0f, 0f, 0f );
            }
            else if( viewType == ViewType.ThreeDimensional )
            {
                //We don't care about anything from the RectTransform, just use the normal transform variables
                transform.localPosition = Vector3.zero;
                transform.localEulerAngles = Vector3.zero;
                transform.localScale = Vector3.one;
            }

            //Let all of the BlockGroups know we've changed
            SetBlockGroupsToViewType();

        } //END BlockTypeChanged

        //---------------------------------//
        public void SetBlockGroupsToViewType()
        //---------------------------------//
        {

            if( blockGroups != null && blockGroups.Count > 0 )
            {
                foreach( BlockGroup blockGroup in blockGroups )
                {
                    blockGroup.SetBlocksToViewType( viewType );
                }
            }

        } //END SetBlockGroupsToViewType




        //--------------------------------//
        private BlockGroup CreateBlockGroup()
        //--------------------------------//
        {
            BlockGroup blockGroup = PrefabManager.InstantiatePrefab( PrefabFactory.Prefabs.bxr_BlockGroup ).GetComponent<BlockGroup>();

            blockGroup.transform.parent = transform;

            if( blockGroups == null ) { blockGroups = new List<BlockGroup>(); }

            blockGroups.Add( blockGroup );

            return blockGroup;

        } //END CreateBlockGroup
        

        //--------------------------------//
        public void RemoveBlockGroup( BlockGroup blockGroup )
        //--------------------------------//
        {
            if( blockGroup != null )
            {
                if( blockGroups != null && blockGroups.Count > 0 && blockGroups.Contains( blockGroup ) )
                {
                    blockGroups.Remove( blockGroup );
                }

                blockGroup.PrepareForDestroy();

#if UNITY_EDITOR
                //Wait a moment before calling DestroyImmediate to make sure no logic is running
                UnityEditor.EditorApplication.delayCall += () =>
                {
                    DestroyImmediate( blockGroup.gameObject );
                };
#else
                Destroy( blockGroup.gameObject );
#endif
            }

        } //END RemoveBlockGroup



        //--------------------------------------------//
        /// <summary>
        /// Sends a command to all of the BlockGroups contained within this BlockView
        /// </summary>
        /// <param name="commandType">The command to send</param>
        /// <param name="impactBlockTypes">OPTIONAL: The type of blocks to affect with this command</param>
        /// <param name="excludeBlockGroups">OPTIONAL: Exclude any of these BlockGroups from this command</param>
        /// <param name="excludeBlocks">OPTIONAL: Exclude specific Blocks from this command</param>
        /// <param name="sendToLinkedBlockView">OPTIONAL: Should we send this command to the linkedBlockView also?</param>
        public void SendCommand( Block.CommandType commandType, List<Block.BlockType> impactBlockTypes = null, List<BlockGroup> excludeBlockGroups = null, List<Block> excludeBlocks = null, bool sendToLinkedBlockView = false )
        //--------------------------------------------//
        {
            //Debug.Log( "BlockView.cs SendCommand( " + commandType + " ) blockGroups = " + blockGroups + ", blockGroups.Count = " + blockGroups.Count() );

            //Send this command to every block group
            if( blockGroups != null && blockGroups.Count > 0 )
            {
                foreach( BlockGroup blockGroup in blockGroups )
                {
                    if( blockGroup != null )
                    {
                        //If we aren't excluding anything,
                        //or our list of exclusions does not contain this, send the command
                        if( excludeBlockGroups == null ||
                          ( excludeBlockGroups != null && !excludeBlockGroups.Contains( blockGroup ) ) )
                        {
                            //Send along our info on what blockTypes to effect.

                            //Don't inform linkedBlockViews of this command,
                            //as they will already recieve it from this function
                            blockGroup.SendCommand( commandType, impactBlockTypes, excludeBlocks );
                        }
                    }
                }
            }

            //If we were told to, then tell our linked block view to do the command
            if( sendToLinkedBlockView && linkedBlockView != null )
            {
                linkedBlockView.SendCommand( commandType, impactBlockTypes, excludeBlockGroups, excludeBlocks, false );
            }

        } //END SendCommand
        

        //--------------------------------//
        /// <summary>
        /// Tells all of the BlockButtons contained within this BlockView to enable/disable their colliders to prevent interactions
        /// </summary>
        /// <param name="enabled">Should this BlockButton's colliders be enabled or disabled</param>
        public void SetBlockButtonColliders( bool enabled )
        //--------------------------------//
        {

            if( blockGroups != null && blockGroups.Count > 0 )
            {
                foreach( BlockGroup blockGroup in blockGroups )
                {
                    if( blockGroup != null )
                    {
                        SetBlockButtonColliders( enabled );
                    }
                }
            }

        } //END SetBlockButtonColliders



        //--------------------------------//
        /// <summary>
        /// Called when a BlockButton has been entered by the Gaze system. Sends this message to all of the Blocks contained within this BlockView
        /// </summary>
        /// <param name="blockButton">The BlockButton that was entered by the gaze system</param>
        public void BlockButtonGazeEnter( BlockButton blockButton )
        //--------------------------------//
        {
            if( blockGroups != null && blockGroups.Count > 0 )
            {
                foreach( BlockGroup blockGroup in blockGroups )
                {
                    if( blockGroup != null )
                    {
                        blockGroup.BlockButtonGazeEnter( blockButton );
                    }
                }
            }

        } //END BlockButtonGazeEnter

        //--------------------------------//
        /// <summary>
        /// Called when a BlockButton has been exited by the Gaze system. Sends this message to all of the Blocks contained within this BlockView
        /// </summary>
        /// <param name="blockButton">The BlockButton that was exited by the gaze system</param>
        public void BlockButtonGazeExit( BlockButton blockButton )
        //--------------------------------//
        {

            if( blockGroups != null && blockGroups.Count > 0 )
            {
                foreach( BlockGroup blockGroup in blockGroups )
                {
                    if( blockGroup != null )
                    {
                        blockGroup.BlockButtonGazeExit( blockButton );
                    }
                }
            }

        } //END BlockButtonGazeExit

        //---------------------------------//
        /// <summary>
        /// Called when a BlockButton is Selected, sends this message down to all of the Blocks contained within this View
        /// </summary>
        /// <param name="blockButton">The BlockButton that was Selected</param>
        public void BlockButtonSelected( BlockButton blockButton )
        //---------------------------------//
        {

            //Inform other blocks that a button has been selected
            if( blockGroups != null && blockGroups.Count > 0 )
            {
                foreach( BlockGroup blockGroup in blockGroups )
                {
                    if( blockGroup != null )
                    {
                        blockGroup.BlockButtonSelected( blockButton );
                    }
                }
            }

        } //END BlockButtonSelected



        
        //--------------------------------//
        /// <summary>
        /// Get all of the blocks from this View, and optionally from the Linked Block View as well.
        /// </summary>
        /// <param name="blockType">OPTIONAL: The type of blocks you are looking for, if left null all blocks will be returned</param>
        /// <param name="alsoCheckLinkedBlockView">OPTIONAL: Should we check the Linked Block View to this one?</param>
        /// <param name="checkNestedGroupsWithinViews">OPTIONA: Should we also check any nested groups contained within this BlockView and the linkedBlockView?</param>
        /// <returns></returns>
        public List<Block> GetBlocks( Block.BlockType? blockType = null, bool alsoCheckLinkedBlockView = true, bool checkNestedGroupsWithinViews = true )
        //--------------------------------//
        {

            List<Block> blockList = new List<Block>();

            if( blockGroups != null && blockGroups.Count > 0 )
            {
                foreach( BlockGroup blockGroup in blockGroups )
                {
                    if( blockGroup != null && blockGroup.GetBlocks( null, checkNestedGroupsWithinViews ) != null && blockGroup.GetBlocks( null, checkNestedGroupsWithinViews ).Count > 0 )
                    {
                        foreach( Block block in blockGroup.GetBlocks( null, checkNestedGroupsWithinViews ) )
                        {
                            if( block != null )
                            {
                                if( blockType != null && block.GetBlockType() == blockType )
                                {
                                    blockList.Add( block );
                                }
                                else if( blockType == null )
                                {
                                    blockList.Add( block );
                                }
                            }
                        }
                    }
                }
            }

            if( alsoCheckLinkedBlockView && linkedBlockView != null )
            {
                List<Block> additionalBlocks = linkedBlockView.GetBlocks( blockType, checkNestedGroupsWithinViews );

                if( additionalBlocks != null && additionalBlocks.Count > 0 )
                {
                    foreach( Block block in additionalBlocks )
                    {
                        blockList.Add( block );
                    }
                }
            }

            return blockList;

        } //END GetBlocks

        //--------------------------------//
        /// <summary>
        /// Return the first instance of the blockType we can find, optionally you can also search the LinkedBlockView
        /// </summary>
        /// <param name="blockType">The type of block we are looking for</param>
        /// <param name="alsoCheckLinkedBlockView">Should we also check the linkedBlockView?</param>
        /// <returns></returns>
        public Block GetBlock( Block.BlockType blockType, bool alsoCheckLinkedBlockView = true )
        //--------------------------------//
        {
            
            if( blockGroups != null && blockGroups.Count > 0 )
            {
                foreach( BlockGroup blockGroup in blockGroups )
                {
                    if( blockGroup != null && blockGroup.GetBlocks() != null && blockGroup.GetBlocks().Count > 0 )
                    {
                        foreach( Block block in blockGroup.GetBlocks() )
                        {
                            if( block != null && block.GetBlockType() == blockType )
                            {
                                return block;
                            }
                        }
                    }
                }
            }

            if( alsoCheckLinkedBlockView && linkedBlockView != null )
            {
                return linkedBlockView.GetBlock( blockType, false );
            }

            return null;

        } //END GetBlock
        


        //--------------------------------//
        /// <summary>
        /// Is this block within this BlockView? Optionally we can also check the LinkedBlockView
        /// </summary>
        /// <param name="block">The block to look for</param>
        /// <param name="alsoCheckLinkedBlockView">Should we also check the LinkedBlockView?</param>
        /// <returns></returns>
        public bool IsInView( Block block, bool alsoCheckLinkedBlockView = true )
        //--------------------------------//
        {
            //Is the Block within this BlockView?
            if( blockGroups != null && blockGroups.Count > 0 )
            {
                foreach( BlockGroup blockGroup in blockGroups )
                {
                    if( blockGroup.IsInGroup( block ) )
                    {
                        return true;
                    }
                }
            }

            //Or maybe it's within the linkedBlockView?
            if( alsoCheckLinkedBlockView &&
                linkedBlockView.IsInView( block, false ) )
            {
                return true;
            }

            return false;

        } //END IsInView

        //--------------------------------//
        /// <summary>
        /// Checks to see if a BlockGroup is part of this BlockView, optionally you can also check the linkedBlockView
        /// </summary>
        /// <param name="blockGroup">The blockGroup to check</param>
        /// <param name="alsoCheckLinkedBlockView">Should we also check the linked BlockView?</param>
        /// <returns></returns>
        public bool IsInView( BlockGroup blockGroup, bool alsoCheckLinkedBlockView = true )
        //--------------------------------//
        {
            //Check the current BlockView for the BlockGroup
            if( blockGroups != null && blockGroups.Count > 0 )
            {
                if( blockGroups.Contains( blockGroup ) )
                {
                    return true;
                }
            }

            //Or maybe it's within the linkedBlockView?
            if( alsoCheckLinkedBlockView &&
                linkedBlockView.IsInView( blockGroup, false ) )
            {
                return true;
            }

            return false;

        } //END IsInView



        //--------------------------------//
        /// <summary>
        /// Call this method before destroying the object containing this component
        /// </summary>
        public void PrepareForDestroy()
        //--------------------------------//
        {

            if( blockGroups != null && blockGroups.Count > 0 )
            {
                foreach( BlockGroup blockGroup in blockGroups )
                {
                    blockGroup.PrepareForDestroy();
                }
            }

        } //END PrepareForDestroy

        //-------------------------------//
        /// <summary>
        /// Called when Internet connection is lost, passes this message all the way down to the Blocks contained
        /// </summary>
        public void InternetConnectionLost()
        //-------------------------------//
        {

            if( blockGroups != null && blockGroups.Count > 0 )
            {
                foreach( BlockGroup blockGroup in blockGroups )
                {
                    blockGroup.InternetConnectionLost();
                }
            }

        } //END InternetConnectionLost

        //-------------------------------//
        /// <summary>
        /// Called when Internet connection is restored, passes this message all the way down to the blocks contained
        /// </summary>
        public void InternetConnectionRestored()
        //-------------------------------//
        {
        
            if( blockGroups != null && blockGroups.Count > 0 )
            {
                foreach( BlockGroup blockGroup in blockGroups )
                {
                    blockGroup.InternetConnectionRestored();
                }
            }

        } //END InternetConnectionRestored

        [Button( "Remove Block Views", ButtonSizes.Large ), FoldoutGroup("Remove Block Views"), InfoBox( "Press to remove this BlockView and the linked BlockView from the Block Manager parent\n\nWARNING: Cannot be undone. This will remove all BlockGroups and Blocks childed to this BlockView and it's sibling linkedBlockView", InfoMessageType.Warning )]
        //-------------------------------//
        /// <summary>
        /// Shows a Remove Block View button in the editor, destroys this component and GameObject and removes it from the BlockManager parent component
        /// </summary>
        public void RemoveBlockView()
        //-------------------------------//
        {

            if( transform.parent != null && transform.parent.parent != null && transform.parent.parent.GetComponent<BlockManager>() != null )
            {
                transform.parent.parent.GetComponent<BlockManager>().RemoveBlockView( this );
            }
            else
            {
                PrepareForDestroy();

#if UNITY_EDITOR
                //Wait a moment before calling DestroyImmediate to make sure no logic is running
                UnityEditor.EditorApplication.delayCall += () =>
                {
                    DestroyImmediate( gameObject );
                };
#else
                Destroy( gameObject );
#endif

            }

        } //END RemoveBlockView



        //-------------------------------------//
        public virtual void OnDisable()
        //-------------------------------------//
        {
            
            if( blockGroups != null && blockGroups.Count > 0 )
            {
                foreach( BlockGroup blockGroup in blockGroups )
                {
                    if( blockGroup != null )
                    {
                        blockGroup.OnDisable();
                    }
                }
            }

        } //END OnDisable

    } //END Class

} //END Namespace