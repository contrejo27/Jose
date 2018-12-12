/* BlockGroup.cs
 * A combination of other BlockGroups or Blocks
 * 
 * Use the SendCommand() function to affect all of the Blocks contained within this BlockGroup
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
    public class BlockGroup: MonoBehaviour
    {
        public bool showDebug = false;

        [ShowIf("ShowBlockGroupTitleGroup"), TitleGroup("BlockGroup", "Contains Blocks or additional nested BlockGroups")]
        public int dummyTitle1 = 0;

        public virtual bool ShowBlockGroupTitleGroup() { return true; }

        //-------------------------------------------------------------------//
        //------------------------ GAMEOBJECT HOOKS -------------------------//
        //-------------------------------------------------------------------//
        [FoldoutGroup( "Hooks" ), InfoBox( "Do Not Modify", InfoMessageType.Warning )]
        public GameObject _PrefabManager;

        //-------------------------------------------------------------------//
        //------------------------ VARIABLES --------------------------------//
        //-------------------------------------------------------------------//
        //The types of BlockGroups that can appear in any type of BlockView
        public enum BlockGroupType
        {
            BlockGroup,
            XRTarget
        }

        //The types of BlockGroups that can appear childed to a XRTargetBase
        public enum BlockGroupTypeXR
        {
            BlockGroup,
            ScalingGroup
        }

        //The types of BlockGroups that can appear in a 2D BlockView
        public enum BlockGroupType2D
        {
            BlockGroup
        }

        //The types of BlockGroups that can appear in a 3D BlockView
        public enum BlockGroupType3D
        {
            BlockGroup,
            XRTarget
        }

        public virtual BlockGroupType GetBlockGroupType()
        {
            //If a blockView parent exists, then this BlockView can be either 2D or 3D
            if( blockView != null && blockView.viewType == BlockView.ViewType.TwoDimensional )
            {
                return BlockGroupType.BlockGroup;
            }
            return BlockGroupType.BlockGroup;
        }

        //A BlockGroup must eventually be parented to a BlockView, regardless of how nested it is
        protected BlockView blockView = null;
        public BlockView GetView() { return blockView; }

        //A BlockGroup can contain nested BlockGroups
        protected List<BlockGroup> nestedBlockGroups = new List<BlockGroup>();
        public List<BlockGroup> GetNestedBlockGroups() { return nestedBlockGroups; }

        //A BlockGroup can contain Blocks
        protected List<Block> blocks = new List<Block>();

        // Position Variables for their place in a scaling group:
        private int prevH; // previous h value. Only update if the current and prev don't match to avoid killing the update process.
        private int prevV; // previous v value. Only update if the current and prev don't match to avoid killing the update process.
        private int prevX; // previous x value. Only update if the current and prev don't match to avoid killing the update process.
        private int prevY; // previous y value. Only update if the current and prev don't match to avoid killing the update process.

        //A simple way to add Blocks in the Unity Editor to this BlockGroup
#if UNITY_EDITOR
        [ShowIf( "ShouldShowBlockButton" ), FoldoutGroup( "Create Blocks" ), InfoBox("Convenience functionality to easily create Blocks and parent them to this BlockGroup. Blocks contain the core functionality in a BrandXR project.")]
        public Block.BlockType blockType = Block.BlockType.Event;

        [ShowIf( "ShouldShowBlockButton" ), Button( "Add Block", ButtonSizes.Large), FoldoutGroup( "Create Blocks" )]
        public void AddBlock()
        {
            GameObject block = (GameObject)AssetDatabase.LoadAssetAtPath( "Assets/BrandXR/Prefabs/BlockSystem/Blocks/bxr_Block" + blockType + ".prefab", typeof( GameObject ) );
            GameObject go = Instantiate( block );

            go.name = "bxr_Block" + blockType;
            go.transform.parent = transform;
        }

        public virtual bool ShouldShowBlockButton()
        {
            return true;
        }




        [ShowIf( "ShouldShowAddNestedBlockGroupButton2D" ), FoldoutGroup( "Create Nested Block Groups" ), InfoBox( "Convenience functionality to easily create Nested 2D Block Groups and parent them to this 2D BlockGroup.\n\nYou can have as many nested BlockGroups as your project needs." )]
        public BlockGroupType2D blockGroupType2D = BlockGroupType2D.BlockGroup;

        [ShowIf( "ShouldShowAddNestedBlockGroupButton2D" ), Button( "Add Nested Block Group", ButtonSizes.Large ), FoldoutGroup( "Create Nested Block Groups" )]
        public virtual BlockGroup AddNestedBlockGroup2D()
        {
            if( ShouldShowAddNestedBlockGroupButton2D() )
            {
                GameObject group = (GameObject)AssetDatabase.LoadAssetAtPath( "Assets/BrandXR/Prefabs/BlockSystem/BlockGroups/bxr_" + blockGroupType2D + ".prefab", typeof( GameObject ) );
                GameObject go = Instantiate( group );

                go.name = "bxr_" + blockGroupType2D;
                go.transform.parent = transform;
                nestedBlockGroups.Add( go.GetComponent<BlockGroup>() );

                return go.GetComponent<BlockGroup>();
            }

            return null;
        }

        public virtual bool ShouldShowAddNestedBlockGroupButton2D()
        {
            //If this gameobject has a parent that is an XRTarget, then we can only show XRTarget compatible BlockGroup options
            if( this.GetComponentInParent<XRTarget>() != null ) { return false; }

            if( blockView != null && blockView.viewType == BlockView.ViewType.TwoDimensional )
            {
                if( GetComponent<XRTargetBase>() != null || GetComponent<XRTarget>() != null )
                {
                    return false;
                }
                else
                {
                    return true;
                }
            }
            else if( transform.parent != null && transform.GetComponentInParent<BlockView>() != null && 
                     transform.parent.GetComponentInParent<BlockView>().viewType == BlockView.ViewType.TwoDimensional )
            {
                if( GetComponent<XRTargetBase>() != null || GetComponent<XRTarget>() != null )
                {
                    return false;
                }
                else
                {
                    return true;
                }
            }

            return false;
        }




        [ShowIf( "ShouldShowAddNestedBlockGroupButton3D" ), FoldoutGroup( "Create Nested Block Groups" ), InfoBox( "Convenience functionality to easily create Nested 3D Block Groups and parent them to this 3D BlockGroup. You can have as many nested BlockGroups as your project needs." )]
        public BlockGroupType3D blockGroupType3D = BlockGroupType3D.BlockGroup;

        [ShowIf( "ShouldShowAddNestedBlockGroupButton3D" ), Button( "Add Nested Block Group", ButtonSizes.Large ), FoldoutGroup( "Create Nested Block Groups" )]
        public virtual BlockGroup AddNestedBlockGroup3D()
        {
            if( ShouldShowAddNestedBlockGroupButton3D() )
            {
                GameObject group = (GameObject)AssetDatabase.LoadAssetAtPath( "Assets/BrandXR/Prefabs/BlockSystem/BlockGroups/bxr_" + blockGroupType3D + ".prefab", typeof( GameObject ) );
                GameObject go = Instantiate( group );

                go.name = "bxr_" + blockGroupType3D;
                go.transform.parent = transform;
                nestedBlockGroups.Add(go.GetComponent<BlockGroup>());

                return go.GetComponent<BlockGroup>();
            }

            return null;
        }

        public virtual bool ShouldShowAddNestedBlockGroupButton3D()
        {   
            //If this gameobject has a parent that is an XRTarget, then we can only show XRTarget compatible BlockGroup options
            if ( this.GetComponentInParent<XRTarget>() != null ) { return false; }

            if( blockView != null && blockView.viewType == BlockView.ViewType.ThreeDimensional )
            {
                if( GetComponent<XRTargetBase>() != null || GetComponent<XRTarget>() != null )
                {
                    return false;
                }
                else
                {
                    return true;
                }
            }
            else if( blockView != null && blockView.viewType == BlockView.ViewType.TwoDimensional )
            {
                return false;
            }
            else if( transform.parent != null && transform.GetComponentInParent<BlockView>() != null &&
                     transform.GetComponentInParent<BlockView>().viewType == BlockView.ViewType.ThreeDimensional )
            {
                if( GetComponent<XRTargetBase>() != null || GetComponent<XRTarget>() != null )
                {
                    return false;
                }
                else
                {
                    return true;
                }
            }
            else if( transform.parent != null && transform.GetComponentInParent<BlockView>() != null &&
                     transform.GetComponentInParent<BlockView>().viewType == BlockView.ViewType.TwoDimensional )
            {
                return false;
            }

            return false;
        }





        [ShowIf( "ShouldShowAddNestedBlockGroupButtonXR" ), FoldoutGroup( "Create Nested Block Groups" ), InfoBox( "Convenience functionality to easily create Nested Block Groups and parent them to this XRTargetBase. You can have as many nested BlockGroups as your project needs." )]
        public BlockGroupTypeXR blockGroupTypeXR = BlockGroupTypeXR.BlockGroup;

        [ShowIf( "ShouldShowAddNestedBlockGroupButtonXR" ), Button( "Add Nested Block Group", ButtonSizes.Large ), FoldoutGroup( "Create Nested Block Groups" )]
        public BlockGroup AddNestedBlockGroupXR()
        {
            GameObject group = (GameObject)AssetDatabase.LoadAssetAtPath( "Assets/BrandXR/Prefabs/BlockSystem/BlockGroups/bxr_" + blockGroupTypeXR + ".prefab", typeof( GameObject ) );
            GameObject go = Instantiate( group );

            go.name = "bxr_" + blockGroupTypeXR;
            go.transform.parent = transform;
            nestedBlockGroups.Add( go.GetComponent<BlockGroup>() );

            return go.GetComponent<BlockGroup>();
        }

        public virtual bool ShouldShowAddNestedBlockGroupButtonXR()
        {
            
            //If we have a parent XRTarget, then this BlockGroup should only be showing XRTarget options (limited selection)
            if( transform.GetComponentInParent<XRTarget>() != null )
            {
                return true;
            }


            if( blockView != null && blockView.viewType == BlockView.ViewType.ThreeDimensional )
            {
                return GetComponent<XRTargetBase>() != null;
            }
            else if( transform.parent != null && transform.GetComponentInParent<BlockView>() != null &&
                     transform.GetComponentInParent<BlockView>().viewType == BlockView.ViewType.ThreeDimensional )
            {
                return GetComponent<XRTargetBase>() != null;
            }

            return false;
        }






        

        [ShowIf("ShouldShowAdjustScalingButton"), Button("Adjust Scaling", ButtonSizes.Large), FoldoutGroup("Scaling Settings"), InfoBox("Settings for handling scaling and positioning if this block group is the child of a ScalingGroup.")]
        public void AdjustScaling()
        {
            ScalingGroup scaler = gameObject.GetComponentInParent<ScalingGroup>();
            if (scaler != null)
            {
                scaler.AdjustChildren();
            }
        }

        public virtual bool ShouldShowAdjustScalingButton()
        {
            ScalingGroup scaler = gameObject.GetComponentInParent<ScalingGroup>();
            if (scaler != null)
            {
                return true;
            }
            else
            {
                return false;
            }
        }


        [FoldoutGroup("Scaling Settings"), Range(0,99)]
        public int hPercent = 1; // the whole number horizontal percentage of space this object should take up
        [FoldoutGroup("Scaling Settings"), Range(0, 99)]
        public int vPercent = 1; // the whole number vertical percentage of space this object should take up
        [FoldoutGroup("Scaling Settings"), Range(0, 99)]
        public int xPosition = 1; // the whole number grid x position in a 100x100 grid that this object should start in
        [FoldoutGroup("Scaling Settings"), Range(0, 99)]
        public int yPosition = 1; // the whole number grid y position in a 100x100 grid that this object should start in

#endif


        //--------------------------------//
#if UNITY_EDITOR
        [ExecuteInEditMode]
#endif
        public virtual void Update()
        //--------------------------------//
        {

#if UNITY_EDITOR
            if (prevH != hPercent || prevV != vPercent || xPosition != prevX || yPosition != prevY)
            {
                // update the "prev" values
                prevH = hPercent;
                prevV = vPercent;
                prevX = xPosition;
                prevY = yPosition;
            }
#endif

        } //END Update
        


        //--------------------------------//
        public virtual void Start()
        //--------------------------------//
        {
            //All BrandXR code relies heavily upon calling Prefabs, make sure we can do that!
            AddPrefabManagerIfNeeded();

            AddBlockGroupsToList();

            AddBlocksToList();

            FindExistingBlockViewParent();

            CheckIfWeShouldCreateNewBlockView();

            AddBlockViewToNestedGroups();

            if( blockView != null )
            {
                SetBlocksToViewType( blockView.viewType );
            }

        } //END Start

        //---------------------------------//
        private void AddPrefabManagerIfNeeded()
        //---------------------------------//
        {
            //If the _prefabManager hook does not exist, grab it from the Asset database when in editor
            if( _PrefabManager == null )
            {
#if UNITY_EDITOR
                Debug.LogError( "BlockGroup.cs AddPrefabManagerIfNeeded() Unable to create PrefabManager as the _PrefabManager hook on this component is null, using AssetDatabase lookup as an Editor only backup" );
                _PrefabManager = UnityEditor.AssetDatabase.LoadAssetAtPath<GameObject>( "Assets/BrandXR/Prefabs/bxr_PrefabManager.prefab" );
#else
                Debug.LogError( "BlockGroup.cs AddPrefabManagerIfNeeded() Unable to create PrefabManager as the _PrefabManager hook on this component is null" );
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

            //Find all of the BlockGroups directly childed to this BlockGroup
            //Only add BlockGroups directly nested to this BlockGroup, not anything deeper than that
            nestedBlockGroups = new List<BlockGroup>();

            if( GetComponentsInChildren<BlockGroup>() != null && GetComponentsInChildren<BlockGroup>().Count() > 0 )
            {
                List<BlockGroup> childBlockGroups = GetComponentsInChildren<BlockGroup>().ToList();

                foreach( BlockGroup blockGroup in childBlockGroups )
                {
                    if( blockGroup.transform.parent != null &&
                        blockGroup.transform.parent == this.transform )
                    {
                        nestedBlockGroups.Add( blockGroup );
                    }
                }
            }

        } //END AddBlockGroupsToList

        //---------------------------------//
        private void AddBlocksToList()
        //---------------------------------//
        {
            //Find all of the blocks directly childed to this BlockGroup
            //Only add the Blocks from this direct BlockGroup, not any nested children
            blocks = new List<Block>();

            if( GetComponentsInChildren<Block>() != null && GetComponentsInChildren<Block>().Count() > 0 )
            {
                List<Block> childBlocks = GetComponentsInChildren<Block>().ToList();

                foreach( Block block in childBlocks )
                {
                    if( block != null )
                    {
                        //Only add blocks with this BlockGroup as their parent
                        if( block.transform.parent != null && 
                            block.transform.parent == this.transform )
                        {
                            blocks.Add( block );
                        }
                    }
                }
            }

        } //END AddBlocksToList

        //---------------------------------//
        private void FindExistingBlockViewParent()
        //---------------------------------//
        {

            //Search for the BlockView in this BlockGroups parents 
            //Will go all the way up the hierarchy or set the blockView to null if there is none
            if( transform.parent != null && 
                transform.GetComponentInParent<BlockView>() != null )
            {
                blockView = transform.GetComponentInParent<BlockView>();
            }
            
            
        } //END FindExistingBlockViewParent

        //---------------------------------//
        private void AddBlockViewToNestedGroups()
        //---------------------------------//
        {
            //Send the blockView to any nested BlockGroups as well
            if( blockView != null && nestedBlockGroups != null && nestedBlockGroups.Count > 0 )
            {
                foreach( BlockGroup blockGroup in nestedBlockGroups )
                {
                    //If the nested BlockGroup does not have a BlockView, or if it has a different BlockView reference,
                    //Update it to the BlockView we found/created within this BlockGroup
                    if( blockGroup != null && ( blockGroup.blockView == null || blockGroup.blockView != blockView ) )
                    {
                        blockGroup.blockView = blockView;

                        //If needed, set the blocks in this nested group to the ViewType
                        blockGroup.SetBlocksToViewType( blockView.viewType );
                    }
                }
            }

        } //END AddBlockViewToNestedGroups

        //---------------------------------//
        private void CheckIfWeShouldCreateNewBlockView()
        //---------------------------------//
        {
            //If we couldn't find a BlockView earlier, then there's something wrong!
            //Take this BlockGroup and all of its children, and parent it to a new BlockView that
            //will be attached to the BlockManager's 3D Camera
            if( blockView == null )
            {
                bool createBlockView = false;

                //Create a BlockView parent if this BlockGroup is not a child of another BlockGroup,
                if( this.transform.parent != null && 
                    transform.parent.GetComponent<BlockGroup>() == null )
                {
                    createBlockView = true;
                }
                
                //We should also create a BlockView if there is no parent object above this BlockGroup
                else if( this.transform.parent == null )
                {
                    createBlockView = true;
                }
                
                //If we should create a BlockView above this BlockGroup...
                if( createBlockView )
                {
                    //This is the top-most BlockGroup, but without a BlockView
                    //Create a BlockView and parent this BlockGroup to it
                    //Also, don't forget to create the LinkedBlockView and add it to the manager as well

                    //If we already have a parent, add a BlockView component to it!
                    if( this.transform.parent != null && this.transform.parent.name != "BrandXR Tech" )
                    {
                        //Debug.Log( "BlockGroup.cs CheckIfWeShouldCreateNewBlock() adding BlockView to parent" );

                        blockView = this.transform.parent.gameObject.AddComponent<BlockView>();
                        blockView._PrefabManager = _PrefabManager;

                        if( this.transform.parent.name == "GameObject" )
                        {
                            this.transform.parent.name = "bxr_BlockView 3D";
                        }
                    }

                    //Otherwise create the BlockView from scratch
                    else
                    {
                        //Make sure our PrefabManager exists...
                        if( PrefabManager.instance != null )
                        {
                            blockView = PrefabManager.InstantiatePrefab( PrefabFactory.Prefabs.bxr_BlockView ).GetComponent<BlockView>();
                            blockView.name = "bxr_BlockView 3D";
                        }
                    }

                    //Create a new BlockView to be the Linked Block View
                    blockView.linkedBlockView = PrefabManager.InstantiatePrefab( PrefabFactory.Prefabs.bxr_BlockView ).GetComponent<BlockView>();
                    blockView.linkedBlockView.name = "bxr_BlockView 2D";

                    //Set the linkedBlockView's linkedBlockView to this blockView to complete the connection
                    blockView.linkedBlockView.linkedBlockView = blockView;

                    //When we have to create a BlockView from scratch, we default to making it a 3D BlockView
                    blockView.viewType = BlockView.ViewType.ThreeDimensional;

                    //Don't forget to setup the linkedBlockView to the opposite (2D)
                    blockView.linkedBlockView.viewType = BlockView.ViewType.TwoDimensional;

                    //Add this new BlockView to the BlockManager
                    blockView.ParentToBlockManagerTransform();

                    //Do the same parenting step for the linkedBlockView
                    blockView.linkedBlockView.ParentToBlockManagerTransform();

                    //Finally add this BlockGroup to be a child of the BlockView
                    transform.SetParent( blockView.transform );

                    //Add a number to the name of the BlockViews so we can make it clear that these two siblings are linked
                    //Count how many BlockViews there currently are, add one to that. This will be used as part of the name for the new BlockViews to keep it clear that they are siblings
                    
                    int blockViewNumber = 0;
                    List<BlockView> blockViews = GameObject.FindObjectsOfType<BlockView>().ToList();

                    if( blockViews != null && blockViews.Count > 0 )
                    {
                        foreach( BlockView view in blockViews )
                        {
                            if( view != null && view.viewType == BlockView.ViewType.TwoDimensional )
                            {
                                blockViewNumber++;
                            }
                        }
                    }

                    blockView.name += " (" + ( blockViewNumber ) + ")";
                    blockView.linkedBlockView.name += " (" + ( blockViewNumber ) + ")";
                    

                    //Debug.Log( "view1.name = " + blockView.name + ", view2.name = " + blockView.linkedBlockView.name );

                    //Debug.Log( "BlockGroup.cs CheckIfWeShouldCreateNewBlockView() ... " + name + " Setting parent to blockView transform" );
                }
                else
                {
                    //Debug.Log( "BlockGroup.cs CheckIfWeShouldCreateNewBlockView() ... " + name + " blockView is null but transform.parent is " + this.transform.parent + ", does it have a BlockGroup component?= " + transform.parent.GetComponent<BlockGroup>() );
                }
            }
            else
            {
                //Debug.Log( "BlockGroup.cs CheckIfWeShouldCreateNewBlockView() ... blockView already exists" );
            }

        } //END CheckIfWeShouldCreateNewBlockView


        //---------------------------------//
        public void SetBlocksToViewType( BlockView.ViewType viewType )
        //---------------------------------//
        {

            if( blocks != null && blocks.Count > 0 )
            {
                foreach( Block block in blocks )
                {
                    block.SetToViewType( viewType );
                }
            }

        } //END SetBlocksToViewType


        //--------------------------------//
        private Block CreateBlock( Block.BlockType type )
        //--------------------------------//
        {
            Block block = PrefabManager.InstantiatePrefab( GetBlockPrefab( type ) ).GetComponent<Block>();

            block.transform.parent = transform;

            if( blocks == null ) { blocks = new List<Block>(); }

            blocks.Add( block );

            return block;

        } //END CreateBlock

        //-------------------------------//
        private PrefabFactory.Prefabs GetBlockPrefab( Block.BlockType type )
        //-------------------------------//
        {
            if( Enum.IsDefined( typeof( PrefabFactory.Prefabs ), "bxr_Block" + type.ToString() ) )
            {
                return (PrefabFactory.Prefabs)Enum.Parse( typeof( PrefabFactory.Prefabs ), "bxr_Block" + type.ToString() );
            }
            else
            {
                //Debug.Log( "BlockGroup.cs GetBlockPrefab( " + type + " ) not defined, returning BlockButton" );
                return PrefabFactory.Prefabs.bxr_BlockButton;
            }

        } //END GetBlockPrefab

        //----------------------------------------//
        public void RemoveBlock( Block block )
        //----------------------------------------//
        {
            if( block != null && blocks != null && blocks.Count > 0 && blocks.Contains( block ) )
            {
                blocks.Remove( block );
            }

            if( block != null )
            {
                block.PrepareForDestroy();

#if UNITY_EDITOR
                //Wait a moment before calling DestroyImmediate to make sure no logic is running
                UnityEditor.EditorApplication.delayCall += () =>
                {
                    DestroyImmediate( block.gameObject );
                };
#else
                Destroy( block.gameObject );
#endif
            }

        } //END RemoveBlock






        //--------------------------------------------//
        /// <summary>
        /// Sends a command to all of the Blocks contained within this BlockGroup
        /// </summary>
        /// <param name="commandType">The command to send</param>
        /// <param name="impactBlockTypes">OPTIONAL: The type of blocks to affect with this command</param>
        /// <param name="exclusions">OPTIONAL: Exclude any of these Blocks from this command</param>
        /// <param name="sendToNestedBlockGroups">OPTIONAL: Should we send this command to any nested BlockGroups also?</param>
        public void SendCommand( Block.CommandType commandType, List<Block.BlockType> impactBlockTypes = null, List<Block> exclusions = null, bool sendToNestedBlockGroups = false )
        //--------------------------------------------//
        {
            //Send this command to every block
            if( blocks != null && blocks.Count > 0 )
            {
                foreach( Block block in blocks )
                {
                    if( block != null )
                    {
                        //If we aren't excluding anything,
                        //or our list of exclusions does not contain this, continue to send the command
                        if( exclusions == null ||
                          ( exclusions != null && !exclusions.Contains( block ) ) )
                        {
                            //Check the list of impacted block types, if the list is not null, 
                            //and this block's type is not included, then don't send it the message
                            if( impactBlockTypes != null && impactBlockTypes.Count() > 0 &&
                                impactBlockTypes.Contains( block.GetBlockType() ) )
                            {
                                block.SendCommand( commandType );
                            }

                            //If the list of impacted block types is null or empty,
                            //then we don't care what BlockType this block is, just send the command
                            else if( impactBlockTypes == null || 
                                   ( impactBlockTypes != null && impactBlockTypes.Count() == 0 ) )
                            {
                                block.SendCommand( commandType );
                            }
                        }
                    }
                }
            }

            //If we were told to, then tell our nested block groups to also perform the command
            if( sendToNestedBlockGroups && nestedBlockGroups != null )
            {
                foreach( BlockGroup blockGroup in nestedBlockGroups )
                {
                    if( blockGroup != null )
                    {
                        blockGroup.SendCommand( commandType, impactBlockTypes, exclusions, false );
                    }
                }
            }

        } //END SendCommand
        

        //--------------------------------//
        public void SetBlockButtonColliders( bool enabled )
        //--------------------------------//
        {

            if( blocks != null && blocks.Count > 0 )
            {
                foreach( Block block in blocks )
                {
                    if( block.GetBlockType() == Block.BlockType.Button )
                    {
                        block.GetComponent<BlockButton>().SetCollider( enabled );
                    }
                }
            }

        } //END SetBlockButtonColliders



        //--------------------------------//
        public void BlockButtonGazeEnter( BlockButton blockButton )
        //--------------------------------//
        {
            if( blocks != null && blocks.Count > 0 )
            {
                foreach( Block block in blocks )
                {
                    if( block != null )
                    {
                        block.BlockButtonGazeEnter( blockButton );
                    }
                }
            }

        } //END BlockButtonGazeEnter

        //--------------------------------//
        public void BlockButtonGazeExit( BlockButton blockButton )
        //--------------------------------//
        {

            if( blocks != null && blocks.Count > 0 )
            {
                foreach( Block block in blocks )
                {
                    if( block != null )
                    {
                        block.BlockButtonGazeExit( blockButton );
                    }
                }
            }

        } //END BlockButtonGazeExit

        //---------------------------------//
        public void BlockButtonSelected( BlockButton blockButton )
        //---------------------------------//
        {
            
            //Inform other blocks that a button has been selected
            if( blocks != null && blocks.Count > 0 )
            {
                foreach( Block block in blocks )
                {
                    block.BlockButtonSelected( blockButton );
                }
            }

        } //END BlockButtonSelected



        //--------------------------------//
        /// <summary>
        /// Get all of the blocks from this BlockGroup, and optionally from the Nested Block Groups as well.
        /// </summary>
        /// <param name="blockType">OPTIONAL: The type of blocks you are looking for, if left null all blocks will be returned</param>
        /// <param name="alsoCheckNestedBlockGroups">OPTIONAL: Should we also return the blocks from any nested block groups?</param>
        /// <returns></returns>
        public List<Block> GetBlocks( Block.BlockType? blockType = null, bool alsoCheckNestedBlockGroups = true )
        //--------------------------------//
        {
            List<Block> blockList = new List<Block>();

            //If we should grab the blocks from any nested BlockGroups as well...
            if( alsoCheckNestedBlockGroups && GetComponentsInChildren<Block>() != null )
            {
                //If we do care about block types, 
                //then go through all of the childed blocks and only return the ones that match
                if( blockType != null )
                {
                    List<Block> blocksToCheck = GetComponentsInChildren<Block>().ToList();

                    foreach( Block block in blocksToCheck )
                    {
                        if( block != null )
                        {
                            if( block.GetBlockType() == blockType )
                            {
                                blockList.Add( block );
                            }
                        }
                    }
                }

                //If we don't care about any specific types, just return all of the childed blocks 
                else
                {
                    blockList = GetComponentsInChildren<Block>().ToList();
                }
                
            }

            //If we just want the blocks that directly belong to this BlockGroup...
            else
            {
                //If we need to check for a specific block type
                if( blockType != null )
                {
                    foreach( Block block in blocks )
                    {
                        if( block.GetBlockType() == blockType )
                        {
                            blockList.Add( block );
                        }
                    }
                }

                //If we don't care about specific block types, just get the whole list
                else
                {
                    blockList = blocks;
                }
            }
            
            return blockList;

        } //END GetBlocks

        //--------------------------------//
        /// <summary>
        /// Return the first instance of the blockType we can find, optionally you can also search the nested block groups as well
        /// </summary>
        /// <param name="blockType">The type of block we are looking for</param>
        /// <param name="alsoCheckNestedBlockGroups">Should we also check the nested block groups for the block you are looking for?</param>
        /// <returns></returns>
        public Block GetBlock( Block.BlockType blockType, bool alsoCheckNestedBlockGroups = true )
        //--------------------------------//
        {

            List<Block> blockList = GetComponentsInChildren<Block>().ToList();

            if( blockList != null && blockList.Count() > 0 )
            {
                foreach( Block block in blockList )
                {
                    //If we allow blocks from the nested groups,
                    //then we don't care about checking the parent of the Block
                    //to make sure it's coming from this BlockGroup
                    if( alsoCheckNestedBlockGroups )
                    {
                        if( block.GetBlockType() == blockType )
                        {
                            return block;
                        }
                    }

                    //Otherwise, make sure the parent of this block is this BlockGroup
                    else if( block.transform.parent == transform )
                    {
                        if( block.GetBlockType() == blockType )
                        {
                            return block;
                        }
                    }
                }
                
            }

            //Something went wrong and we didn't find a block, return null
            return null;
            
        } //END GetBlock


        //--------------------------------//
        /// <summary>
        /// Is this block within this BlockGroup? Optionally we can also check all of the nested block groups to see if it is there as well
        /// </summary>
        /// <param name="blockGroup">The block to look for</param>
        /// <param name="alsoCheckNestedBlockGroups">Should we also check the nested block groups?</param>
        /// <returns></returns>
        public bool IsInGroup( Block block, bool alsoCheckNestedBlockGroups = true )
        //--------------------------------//
        {
            //Is the Block within this BlockGroup?
            if( blocks != null && blocks.Count() > 0 )
            {
                if( blocks.Contains( block ) )
                {
                    return true;
                }
            }

            //If not, check the nested Block groups
            if( alsoCheckNestedBlockGroups )
            {
                if( nestedBlockGroups != null && nestedBlockGroups.Count > 0 )
                {
                    foreach( BlockGroup blockGroup in nestedBlockGroups )
                    {
                        if( blockGroup != null && blockGroup.IsInGroup( block ) )
                        {
                            return true;
                        }
                    }
                }
            }

            return false;

        } //END IsInGroup

        //--------------------------------//
        /// <summary>
        /// Is this BlockGroup nested within this BlockGroup? Optionally we can also check all of the nested block groups to see if it is there as well
        /// </summary>
        /// <param name="blockGroup">The BlockGroup to look for</param>
        /// <param name="alsoCheckNestedBlockGroups">Should we also check the nested block groups?</param>
        /// <returns></returns>
        public bool IsInGroup( BlockGroup blockGroup, bool alsoCheckNestedBlockGroups = true )
        //--------------------------------//
        {
            //Is the Block within this BlockGroup?
            if( nestedBlockGroups != null && nestedBlockGroups.Count() > 0 )
            {
                if( nestedBlockGroups.Contains( blockGroup ) )
                {
                    return true;
                }
            }

            //If not, check the nested Block groups
            if( alsoCheckNestedBlockGroups )
            {
                if( nestedBlockGroups != null && nestedBlockGroups.Count > 0 )
                {
                    foreach( BlockGroup _blockGroup in nestedBlockGroups )
                    {
                        if( _blockGroup != null && _blockGroup.IsInGroup( blockGroup ) )
                        {
                            return true;
                        }
                    }
                }
            }

            return false;

        } //END IsInGroup



        //--------------------------------//
        public void PrepareForDestroy()
        //--------------------------------//
        {

            if( blocks != null && blocks.Count > 0 )
            {
                foreach( Block block in blocks )
                {
                    block.PrepareForDestroy();
                }
            }
            
        } //END PrepareForDestroy

        //-------------------------------//
        public void InternetConnectionLost()
        //-------------------------------//
        {
            //We lost internet, and this block requires it!
            //Debug.Log( "BlockGroup.cs InternetConnectionLost() tryingToActivateBlockGroup = " + tryingToActivateBlockGroup + ", isBlockGroupActivated = " + isBlockGroupActivated );

            //If the block is being hovered over and it's in the process of being selected, do nothing, let the individual block deal with the problem so that the user doesn't get confused
            if( BlockFocusManager.instance.IsGazeFocusing( this, false ) ) { return; }

            //If the block is activated already, do nothing
            if( BlockFocusManager.instance.IsFocused( this ) ) { return; }

            //Debug.Log( "BlockGroup.cs HideDueToInternetRequirment() about to call HideDueToInternetRequirment on all blocks" );

            //Otherwise the block isn't being messed with in any way, and it's safe to hide it until internet returns
            if( blocks != null && blocks.Count > 0 )
            {
                foreach( Block block in blocks )
                {
                    //Debug.Log( "BlockGroup.cs InternetConnectionLost() calling Block.InternetConnectionLost()" );
                    block.InternetConnectionLost();
                }
            }
            
        } //END InternetConnectionLost

        //-------------------------------//
        public void InternetConnectionRestored()
        //-------------------------------//
        {
            //Debug.Log( "BlockGroup.cs InternetConnectionRestored() start" );
            
            //If the ImageManager is in the middle of switching images, don't show this block!
            if( ( SceneLoader.instance != null && SceneLoader.instance.IsLevelTransitionInProgress() ) || 
                ( XRSkyboxFactory.instance != null && XRSkyboxFactory.instance.IsTweening() ) )
            { return; }

            //Debug.Log( "BlockGroup.cs ShowDueToInternetRequirment() about to call ShowDueToInternetRequirment on all blocks" );

            //Inform the various blocks that internet has been restored
            if( blocks != null && blocks.Count > 0 )
            {
                foreach( Block block in blocks )
                {
                    //Debug.Log( "BlockGroup.cs InternetConnectionRestored() calling Block.InternetConnectionRestored()" );
                    block.InternetConnectionRestored();
                }
            }

        } //END InternetConnectionRestored

        [Button( "Remove Block Group", ButtonSizes.Large ), FoldoutGroup("Remove Block Group"), InfoBox( "Press to remove this BlockGroup from its parent.\n\nWARNING: This will delete all blocks and nested BlockGroups attached to this BlockGroup.\n\nYou cannot undo this action!", InfoMessageType.Warning )]
        //-------------------------------//
        /// <summary>
        /// Shows a Remove Block Group button in the editor, destroys this component and GameObject and removes it from the BlockView parent component
        /// </summary>
        public void RemoveBlockGroup()
        //-------------------------------//
        {

            if( transform.parent != null && transform.parent.GetComponent<BlockView>() != null )
            {
                transform.parent.GetComponent<BlockView>().RemoveBlockGroup( this );
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

        } //END RemoveBlockGroup

        //-------------------------------------//
        public virtual void OnEnable()
        //-------------------------------------//
        {


        } //END OnEnable


        //-------------------------------------//
        public virtual void OnDisable()
        //-------------------------------------//
        {

            //Let all the Blocks and nested blockGroups know that this gameObject was disabled
            if( blocks != null && blocks.Count > 0 )
            {
                foreach( Block block in blocks )
                {
                    if( block != null )
                    {
                        block.OnDisable();
                    }
                }
            }

            if( nestedBlockGroups != null && nestedBlockGroups.Count > 0 )
            {
                foreach( BlockGroup blockGroup in nestedBlockGroups )
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