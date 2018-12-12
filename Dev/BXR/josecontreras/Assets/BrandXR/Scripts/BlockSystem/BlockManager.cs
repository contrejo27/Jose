/* BlockManager.cs
 * Contains controls for all of the BlockViews in a BrandXR project
 * 
 * All BlockViews must be parented to the BlockManager as a child of the 2D Parent or 3D parent
 * Use the SendCommand() function to control all of the BlockViews in the project
 * 
 * The Block View Hierarchy in a project should look something like this...
 * BlockManager -> 2D/3D Camera -> BlockView -> BlockGroup - Block
 * 
 */

using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System;
using Sirenix.OdinInspector;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace BrandXR
{
    public class BlockManager: MonoBehaviour
    {
        [TitleGroup( "Block Manager", "Parent to all Block Views, Contains controls for all BlockViews" )]
        public int dummy1 = 0;

        [FoldoutGroup("Hooks"), InfoBox("Do Not Modify", InfoMessageType.Warning )]
        public GameObject Group2DParent;
        [FoldoutGroup("Hooks")]
        public GameObject Group3DParent;

        //Holds onto a list of the different BlockViews in the scene
        [Space( 15f ), InfoBox( "Do Not Modify", InfoMessageType.Warning ), FoldoutGroup("Hooks")]
        public List<BlockView> blockViews = new List<BlockView>();

        //-------------------------------------------------------------------//
        //------------------------ VARIABLES --------------------------------//
        //-------------------------------------------------------------------//

        //A simple way to add BlockViews in the Unity Editor and have them auto-attach to this BlockManager
#if UNITY_EDITOR
        [FoldoutGroup( "Create Block Views" ), InfoBox( "Convenience functionality to easily create Block Views and parent them to this BlockManager.\n\nThis will automatically create both a 2D and 3D BlockView that are linked together.\n\nA BlockView can contain any number of BlockGroups and can be used like a Unity Scene or a Screen in your app" )]
        public BlockView.BlockViewType blockViewType = BlockView.BlockViewType.BlockView;

        [Button( "Add Block View", ButtonSizes.Large ), FoldoutGroup( "Create Block Views" )]
        public List<BlockView> AddBlockView()
        {
            List<BlockView> newViews = new List<BlockView>();

            //Count how many BlockViews there currently are, add one to that. This will be used as part of the name for the new BlockViews to keep it clear that they are siblings
            int newBlockViewNumber = 0;
            List<BlockView> blockViews = GameObject.FindObjectsOfType<BlockView>().ToList();

            if( blockViews != null && blockViews.Count > 0 )
            {
                foreach( BlockView view in blockViews )
                {
                    if( view != null && view.viewType == BlockView.ViewType.TwoDimensional )
                    {
                        newBlockViewNumber++;
                    }
                }
            }

            //Add a 2D BlockView and attach it to our 2D Canvas Group child GameObject
            GameObject reference = (GameObject)AssetDatabase.LoadAssetAtPath( "Assets/BrandXR/Prefabs/BlockSystem/BlockViews/bxr_" + blockViewType + ".prefab", typeof( GameObject ) );
            BlockView view2D = Instantiate( reference ).GetComponent<BlockView>();

            view2D.name = "bxr_" + blockViewType + " 2D (" + ( newBlockViewNumber + 1 ) + ")";
            view2D.transform.parent = Group2DParent.transform;
            view2D.transform.localPosition = Vector3.zero;

            newViews.Add( view2D );

            //Add a 3D BlockView and attach it to our 3D Canvas Group child GameObject
            BlockView view3D = Instantiate( reference ).GetComponent<BlockView>();

            view3D.name = "bxr_" + blockViewType + " 3D (" + ( newBlockViewNumber + 1 ) + ")";
            view3D.transform.parent = Group3DParent.transform;
            view3D.transform.localPosition = Vector3.zero;

            newViews.Add( view3D );

            //Link the two related BlockViews together
            view2D.linkedBlockView = view3D;
            view3D.linkedBlockView = view2D;

            //Link the two BlockViews to this BlockManager
            view2D.viewType = BlockView.ViewType.TwoDimensional;
            view3D.viewType = BlockView.ViewType.ThreeDimensional;

            AddBlockViewToManager( view2D, BlockView.ViewType.TwoDimensional );
            AddBlockViewToManager( view3D, BlockView.ViewType.ThreeDimensional );

            return newViews;
        }
#endif

        // singleton behavior
        private static BlockManager _instance;
        
        //--------------------------------------------//
        public static BlockManager instance
        //--------------------------------------------//
        {
            get
            {
                if( _instance == null )
                {
                    if( GameObject.FindObjectOfType<BlockManager>() == null ) { PrefabManager.InstantiatePrefab( PrefabFactory.Prefabs.bxr_BlockManager, null ); }
                    _instance = GameObject.FindObjectOfType<BlockManager>();
                    BlockHelper.AddToBrandXRTechParent( _instance.transform );
                }

                return _instance;
            }

        } //END Instance
        

        //--------------------------------------------//
        public void Awake()
        //--------------------------------------------//
        {

            DestroyDuplicateInstance();

            BlockHelper.AddToBrandXRTechParent( transform );

            gameObject.name = "bxr_BlockManager";

        } //END Awake

        //--------------------------------------------//
        public void Start()
        //--------------------------------------------//
        {

            FindBlockViewsInScene();

        } //END Start

        //------------------------------------//
        private void FindBlockViewsInScene()
        //------------------------------------//
        {

            BlockView[] foundBlockViews = GameObject.FindObjectsOfType<BlockView>();

            if( foundBlockViews != null && foundBlockViews.Length > 0 )
            {
                blockViews = foundBlockViews.ToList();
            }

        } //END FindBlockViewsInScene

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

        public virtual void OnEnable() { SuscribeToEvents(); }

        //------------------------------------------------------//
        public virtual void OnDisable()
        //------------------------------------------------------//
        {
            UnsuscribeFromEvents();

            //Let our BlockViews know that we are now disabled
            if( blockViews != null && blockViews.Count > 0 )
            {
                foreach( BlockView blockView in blockViews )
                {
                    if( blockView != null )
                    {
                        blockView.OnDisable();
                    }
                }
            }

        } //END OnDisable

        //------------------------------------//
        public void SuscribeToEvents()
        //------------------------------------//
        {
            
            Messenger.AddListener( "ApplicationOnlineModeTypeChange_Offline", InternetConnectionLost );
            Messenger.AddListener( "ApplicationOnlineModeTypeChange_Online", InternetConnectionRestored );
            
        } //END SuscribeToEvents

        //------------------------------------//
        public void UnsuscribeFromEvents()
        //------------------------------------//
        {
            
            Messenger.RemoveListener( "ApplicationOnlineModeTypeChange_Offline", InternetConnectionLost );
            Messenger.RemoveListener( "ApplicationOnlineModeTypeChange_Online", InternetConnectionRestored );
            
        } //END UnsuscribeToEvents
        

        //--------------------------------------------//
        public void RemoveBlockView( BlockView blockViewToDelete )
        //--------------------------------------------//
        {

            if( blockViewToDelete != null )
            {
                blockViewToDelete.PrepareForDestroy();

                if( blockViewToDelete.linkedBlockView != null )
                {
                    blockViewToDelete.linkedBlockView.PrepareForDestroy();
                }

                if( blockViews != null && blockViews.Count > 0 )
                {
                    if( blockViews.Contains( blockViewToDelete ) )
                    {
                        blockViews.Remove( blockViewToDelete );
                    }

                    if( blockViews.Contains( blockViewToDelete.linkedBlockView ) )
                    {
                        blockViews.Remove( blockViewToDelete.linkedBlockView );
                    }
                }

#if UNITY_EDITOR
                //Wait a moment before calling DestroyImmediate to make sure no logic is running
                UnityEditor.EditorApplication.delayCall += () =>
                {
                    if( blockViewToDelete.linkedBlockView != null )
                    {
                        DestroyImmediate( blockViewToDelete.linkedBlockView.gameObject );
                    }

                    DestroyImmediate( blockViewToDelete.gameObject );
                };
#else
                if( blockViewToDelete.linkedBlockView != null )
                {
                    Destroy( blockViewToDelete.linkedBlockView.gameObject );
                }
                
                Destroy( blockViewToDelete.gameObject );
#endif
            }

        } //END RemoveBlockView

        //--------------------------------------------//
        public void DestroyExistingBlockViews()
        //--------------------------------------------//
        {

            if( blockViews != null && blockViews.Count > 0 )
            {
                //Debug.Log( "BlockManager.cs DestroyExistingBlockViews() blockViews list is not null or empty, calling destroy for all blockViews" );

                foreach( BlockView blockView in blockViews )
                {
                    if( blockView != null )
                    {
                        blockView.PrepareForDestroy();

                        if( blockView.gameObject != null )
                        {
                            GameObject.Destroy( blockView.gameObject );
                        }
                    }
                }

                Resources.UnloadUnusedAssets();

                blockViews = new List<BlockView>();
            }
            else
            {
                //Debug.Log( "BlockManager.cs DestroyExistingBlockViews() blockViews list is null" );
            }

        } //END DestroyExistingBlockViews
        

        //--------------------------------------------//
        public void AddBlockViewToManager( BlockView blockView, BlockView.ViewType viewType )
        //--------------------------------------------//
        {

            if( blockViews != null && blockViews.Count > 0 )
            {
                blockViews.Add( blockView );
            }
            else
            {
                blockViews = new List<BlockView>();
                blockViews.Add( blockView );
            }

            SetBlockViewToParent( blockView, viewType );
            
        } //END AddBlockViewToManager

        //--------------------------------------------//
        public void SetBlockViewToParent( BlockView blockView, BlockView.ViewType viewType )
        //--------------------------------------------//
        {

            if( blockView != null )
            {
                if( viewType == BlockView.ViewType.ThreeDimensional )
                {
                    blockView.transform.SetParent( Group3DParent.transform );
                }
                else if( viewType == BlockView.ViewType.TwoDimensional )
                {
                    blockView.transform.SetParent( Group2DParent.transform );
                }
            }

        } //END SetBlockViewToParent


        //--------------------------------------------//
        /// <summary>
        /// Sends a command to the Blocks contained within all BlockViews
        /// </summary>
        /// <param name="commandType">The command to send to all of the Blocks contained within the BlockViews</param>
        /// <param name="impactBlockTypes">OPTIONAL: The types of blocks you wish to call this command</param>
        /// <param name="excludeBlockViews">OPTIONAL: Skip over these BlockViews</param>
        public void SendCommand( Block.CommandType commandType, List<Block.BlockType> impactBlockTypes = null, List<BlockView> excludeBlockViews = null, List<BlockGroup> excludeBlockGroups = null, List<Block> excludeBlocks = null )
        //--------------------------------------------//
        {
            //Send this command to all of the block views
            if( blockViews != null && blockViews.Count > 0 )
            {
                foreach( BlockView blockView in blockViews )
                {
                    if( blockView != null )
                    {
                        //If we aren't excluding anything,
                        //or our list of exclusions does not contain this, send the command
                        if( excludeBlockViews == null || 
                          ( excludeBlockViews != null && !excludeBlockViews.Contains( blockView ) ) )
                        {
                            //Send along our info on what blockTypes to effect.

                            //Don't inform linkedBlockViews of this command,
                            //as they will already recieve it from this function
                            blockView.SendCommand( commandType, impactBlockTypes, excludeBlockGroups, excludeBlocks, false );
                        }
                    }
                }
            }

        } //END SendCommand
        

        

        //--------------------------------------------//
        /// <summary>
        /// Called by a BlockButton when the Gaze system has entered its collider, passes this message down to the Blocks contained within
        /// </summary>
        /// <param name="blockButton">The block button that the gaze system has entered</param>
        public void BlockButtonGazeEnter( BlockButton blockButton )
        //--------------------------------------------//
        {
            //Inform all other blocks that a button has been gazed at
            if( blockViews != null && blockViews.Count > 0 )
            {
                foreach( BlockView blockView in blockViews )
                {
                    if( blockView != null )
                    {
                        blockView.BlockButtonGazeEnter( blockButton );
                    }
                }
            }

        } //END BlockButtonGazeEnter

        //--------------------------------------------//
        /// <summary>
        /// Called by a BlockButton when the Gaze system has exited its collider, passes this message down to the Blocks contained within
        /// </summary>
        /// <param name="blockButton">The Block Button that the gaze system has exited</param>
        public void BlockButtonGazeExit( BlockButton blockButton )
        //--------------------------------------------//
        {
            //Inform all other blocks that a button has been gazed at
            if( blockViews != null && blockViews.Count > 0 )
            {
                foreach( BlockView blockView in blockViews )
                {
                    if( blockView != null )
                    {
                        blockView.BlockButtonGazeExit( blockButton );
                    }
                }
            }

        } //END BlockButtonGazeExit

        //--------------------------------------------//
        /// <summary>
        /// Called by a BlockButton it has been selected by the player, passes this message down to the Blocks contained within
        /// </summary>
        /// <param name="blockButton">The Block button that was selected</param>
        public void BlockButtonSelected( BlockButton blockButton )
        //--------------------------------------------//
        {
            //Inform all other blocks that any button has been selected
            if( blockViews != null && blockViews.Count > 0 )
            {
                foreach( BlockView blockView in blockViews )
                {
                    if( blockView != null )
                    {
                        blockView.BlockButtonSelected( blockButton );
                    }
                }
            }

        } //END BlockButtonSelected
        


        //---------------------------------------------//
        public void InternetConnectionLost()
        //---------------------------------------------//
        {

            if( blockViews != null && blockViews.Count > 0 )
            {
                foreach( BlockView blockView in blockViews )
                {
                    blockView.InternetConnectionLost();
                }
            }

        } //END InternetConnectionLost

        //---------------------------------------------//
        public void InternetConnectionRestored()
        //---------------------------------------------//
        {

            if( blockViews != null && blockViews.Count > 0 )
            {
                foreach( BlockView blockView in blockViews )
                {
                    blockView.InternetConnectionRestored();
                }
            }

        } //END InternetConnectionRestored
        

    } //END Class

} //END Namespace