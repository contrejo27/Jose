/* Block.cs
 * Blocks are the cornerstone of BrandXR tech, they are multimedia components attached to a
 * GameObject that perform a specific task. They are only used in their Prefab form (not as individual scripts).
 * 
 * For example, you would use a BlockVideo component to play video from a local or web source with a variety of customizable options.
 * The purpose of using Blocks over standard Unity components is to provide scripters and designers a powerful set of tools that all
 * work within the BrandXR SaaS system. When making additional Blocks you need to add a new BlockType to the list of enums contained here.
 * 
 * All Blocks must be parented to a BlockGroup, and you can have multiple Blocks parented to a BlockGroup to make a more complicated structure
 * EX: A BlockVideo with multiple BlockButton components to create a video player with controls
 * 
 * BlockGroups are infinitely nestable, and the top-level BlockGroup is always parented to a BlockView, which acts like a "Screen" or a Unity Scene.
 * 
 * BlockViews can have both 2D and 3D content, with some Blocks changing their behaviour depending on whether they are used in a 2D or 3D world space setting.
 * Each BlockView is split into a 2D gameObject + BlockView.cs script, and a 3D counterpart that are linked together.
 * 
 * You can then send commands to either a Block, BlockGroup, BlockView, 
 * or send a command to the highest level, the "BlockManager" that controls all BlockViews, and therefore all BlockGroups and Blocks
 * 
 * The structure looks like this
 * BlockManager -> 2D/3D Camera's/Canvases -> 2D & 3D BlockViews -> BlockGroups -> Blocks
 * 
 * 
 * You can use the SendCommand() function to send a specific action to a Block.
 * This ranges from more obvious commands such as Show() and Hide(), to more complicated things like Call() which performs a behaviour custom to that block
 * 
 * SendCommand() logic is also possible from the BlockGroup, BlockView, and BlockManager with various options for accessing the Blocks they each contain
 */

using Sirenix.OdinInspector;
using System.Collections.Generic;
using UnityEngine;

namespace BrandXR
{
    public class Block: MonoBehaviour
    {

        protected BlockGroup blockGroup;
        public void SetGroup( BlockGroup blockGroup ) { this.blockGroup = blockGroup; }
        public BlockGroup GetGroup() { return blockGroup; }

        //The various types of commands a Block can be sent
        public enum CommandType
        {
            Show,
            Hide,
            ForceShow,
            ForceHide,
            Enable,
            Disable,
            Call,
            FaceCamera
        }

        //The various types of blocks that make up a BlockGroup
        public enum BlockType
        {
            Text,
            Image,
            Audio,
            Video,
            Model,
            Button,
            Event
        }

        /// <summary>
        /// What BlockType is this Block set to?
        /// </summary>
        /// <returns>BlockType</returns>
        public virtual BlockType GetBlockType() { return BlockType.Button; }

        //-------------------------------------------------------------------//
        //------------------------ GAMEOBJECT HOOKS -------------------------//
        //-------------------------------------------------------------------//
        [FoldoutGroup( "Hooks" ), InfoBox("Do Not Modify", InfoMessageType.Warning)]
        public GameObject _PrefabManager;

        [Space( 15f ), FoldoutGroup("Hooks"), ShowIf("ShowFaceCameraEvent")]
        public Transform faceCameraTransform = null;

        [ Space( 15f ), FoldoutGroup( "Hooks" )]
        public bool showDebug = false;

        //------------------------------------------------------------------//
        //----------------------- FACE CAMERA OPTIONS ----------------------//
        //------------------------------------------------------------------//
        [ShowIf( "ShowFaceCameraEvent" ), FoldoutGroup( "Face Camera Event" ), InfoBox("Enable this to force the block to always face the camera")]
        public bool alwaysFaceCamera = false;
        private bool ShowForceFaceCameraVariables() { return ShowFaceCameraEvent() && alwaysFaceCamera; }

        public enum FaceCameraType
        {
            XRCameraManager,
            MainCamera,
            CustomCamera
        }
        [Space(15f), ShowIf("ShowForceFaceCameraVariables"), FoldoutGroup("Face Camera Event")]
        public FaceCameraType faceCameraType = FaceCameraType.XRCameraManager;
        private bool FaceXRCamera() { return ShowForceFaceCameraVariables() && faceCameraType == FaceCameraType.XRCameraManager; }
        private bool FaceMainCamera() { return ShowForceFaceCameraVariables() && faceCameraType == FaceCameraType.MainCamera; }
        private bool FaceCustomCamera() { return ShowForceFaceCameraVariables() && faceCameraType == FaceCameraType.CustomCamera; }

        [ShowIf("FaceCustomCamera"), FoldoutGroup("Face Camera Event")]
        public Camera faceCustomCamera = null;

        [Space( 15f ), ShowIf( "ShowForceFaceCameraVariables" ), FoldoutGroup("Face Camera Event"), InfoBox("Set the booleans below to change which axis will be impacted when this block rotates to face the camera")]
        public bool forceX = true;

        [FoldoutGroup("Face Camera Event"), ShowIf( "ShowForceFaceCameraVariables" )]
        public bool forceY = true;

        [FoldoutGroup("Face Camera Event"), ShowIf( "ShowForceFaceCameraVariables" )]
        public bool forceZ = true;

        protected virtual bool ShowFaceCameraEvent() { return true; }

        //----------------------------------------------------------------//
        //-------------------- SEND COMMAND ON START ---------------------//
        //----------------------------------------------------------------//
        [FoldoutGroup("Send Command On Start"), InfoBox("Enable this option to send a command to this block when it's Start() function is called")]
        public bool sendCommandOnStart = false;

        [FoldoutGroup("Send Command On Start"), ShowIf( "sendCommandOnStart", true)]
        public CommandType commandOnStart = CommandType.Call;



        public enum HoverState
        {
            NotHovering,
            Hovering
        }
        protected HoverState hoverState = HoverState.NotHovering;

        public enum GazeState
        {
            NotGazingAt,
            GazingAt
        }
        protected GazeState gazeState = GazeState.NotGazingAt;

        public enum ShowState
        {
            Showing,
            Hiding
        }
        protected ShowState showState = ShowState.Hiding;

        public enum EnabledSate
        {
            Enabled,
            Disabled
        }
        protected EnabledSate enabledState = EnabledSate.Enabled;

        /// <summary>
        /// Tween all visual components to be visible and enable colliders
        /// </summary>
        public virtual void Show() { showState = ShowState.Showing; }

        /// <summary>
        /// Tween all visual components to be hidden and disable colliders
        /// </summary>
        public virtual void Hide() { showState = ShowState.Hiding; }

        /// <summary>
        /// Force all visual components to be immediately visible and enable colliders
        /// </summary>
        public virtual void ForceShow() { showState = ShowState.Showing; }

        /// <summary>
        /// Force all visual components to be immediately hidden and disable colliders
        /// </summary>
        public virtual void ForceHide() { showState = ShowState.Hiding; }

        /// <summary>
        /// Set this block to its enabled state, generally this means restoring visual components to their normal state and enables colliders
        /// </summary>
        public virtual void Enable() { enabledState = EnabledSate.Enabled; }

        /// <summary>
        /// Set this block to its disabled state, generally this means setting visual components to their disabled state and disabling colliders
        /// </summary>
        public virtual void Disable() { enabledState = EnabledSate.Disabled; }

        /// <summary>
        /// Set this block to its hovered over state, generally this means setting visual components to their hovering state
        /// </summary>
        public virtual void EnableHoverState() { hoverState = HoverState.Hovering; }

        /// <summary>
        /// Set this block to its not being hovered over state, generally this means restoring visual components to their normal state
        /// </summary>
        public virtual void DisableHoverState() { hoverState = HoverState.NotHovering; }

        /// <summary>
        /// Does something different for each block, a customizable function that may or may not be implemented
        /// </summary>
        public virtual void Call() { }

        public virtual void OnDisable()
        {

        }


        //--------------------------------//
        /// <summary>
        /// Forces the block to rotate to face the camera
        /// </summary>
        /// <param name="forceX">Should the X axis be forced to face the camera?</param>
        /// <param name="forceY">Should the Y axis be forced to face the camera?</param>
        /// <param name="forceZ">Should the Z axis be forced to face the camera?</param>
        public virtual void FaceCamera( bool forceX, bool forceY, bool forceZ )
        //--------------------------------//
        {
            //Set the block to look at the camera
            if( faceCameraTransform != null )
            {
                //Find a camera to use
                Camera camera = null;

                if( FaceXRCamera() )
                {
                    if( XRCameraManager.instance != null &&
                        XRCameraManager.instance.GetXRCamera() != null &&
                        XRCameraManager.instance.GetXRCamera().GetCameras() != null )
                    {
                        camera = XRCameraManager.instance.GetXRCamera().GetCameras()[0];
                    }
                }
                else if( FaceMainCamera() )
                {
                    if (Camera.main != null)
                    {
                        camera = Camera.main;
                    }
                }
                else if( FaceCustomCamera() )
                {
                    if( faceCustomCamera != null )
                    {
                        camera = faceCustomCamera;
                    }
                }

                //If none of these options are true, try to face the MainCamera
                if( camera == null && Camera.main != null ) { camera = Camera.main; }

                //If we found our camera, let's face it!
                if( camera != null )
                {
                    faceCameraTransform.LookAt(faceCameraTransform.position + (faceCameraTransform.position - camera.transform.position) * 50f);

                    if (!forceX)
                    {
                        faceCameraTransform.localEulerAngles = new Vector3(0f, faceCameraTransform.localEulerAngles.y, faceCameraTransform.localEulerAngles.z);
                    }

                    if (!forceY)
                    {
                        faceCameraTransform.localEulerAngles = new Vector3(faceCameraTransform.localEulerAngles.x, 0f, faceCameraTransform.localEulerAngles.z);
                    }

                    if (!forceZ)
                    {
                        faceCameraTransform.localEulerAngles = new Vector3(faceCameraTransform.localEulerAngles.x, faceCameraTransform.localEulerAngles.y, 0f);
                    }
                }
                
            }

        } //END FaceCamera

        //--------------------------------//
        public virtual void Update()
        //--------------------------------//
        {

            if( alwaysFaceCamera )
            {
                FaceCamera( forceX, forceY, forceZ );
            }

        } //END Update

        //--------------------------------//
        public virtual void Start()
        //--------------------------------//
        {
            //All BrandXR code relies heavily upon calling Prefabs, make sure we can do that!
            AddPrefabManagerIfNeeded();

            //If the faceCameraTransform is null, try to find a child transform with the proper name and set it
            if( faceCameraTransform == null && 
                transform.GetComponentInChildren<Transform>() != null &&
                transform.GetComponentInChildren<Transform>().name == "FaceCamera" )
            {
                faceCameraTransform = transform.GetComponentInChildren<Transform>();
            }

            //Set our current blockGroup to null just to wipe the slate clean
            //We'll find the proper BlockGroup parent as our next step
            blockGroup = null;

            //All Blocks need to be the children of a BlockGroup
            //If we have a parent, and it does contain a BlockGroup,
            //then set that as our BlockGroup
            if( transform.parent != null && GetComponentInParent<BlockGroup>() != null )
            {
                //Debug.Log( name + " Using existing BlockGroup parent" );
                
                //We have a parent gameObject, and it already has a BlockGroup
                blockGroup = GetComponentInParent<BlockGroup>();

                //If our parent gameObject has a generic name, it's safe to rename it to tidy up the editor hierarchy
                if( blockGroup.gameObject.name == "GameObject" ) { blockGroup.gameObject.name = "BlockGroup"; }
            }

            //If we have a parent, and it is a BlockView
            else if( transform.parent != null && transform.parent.GetComponent<BlockView>() != null )
            {
                BlockView blockView = transform.parent.GetComponent<BlockView>();
                
                //Add a intermediary BlockGroup in between the BlockView and the Block
                blockGroup = PrefabManager.InstantiatePrefab( PrefabFactory.Prefabs.bxr_BlockGroup ).GetComponent<BlockGroup>();

                //Parent this block to the newly created BlockGroup
                this.transform.SetParent( blockGroup.transform );

                //Parent the newly created BlockGroup to the blockView
                blockGroup.transform.SetParent( blockView.transform );
            }

            //If we have a parent, but it does not contain a BlockGroup
            //And it is not a BlockView
            else if( transform.parent != null && transform.parent.GetComponent<BlockView>() == null )
            {
                //Debug.Log( name + " Using Existing GameObject, adding new BlockGroup to GameObject" );

                blockGroup = transform.parent.gameObject.AddComponent<BlockGroup>();
                blockGroup._PrefabManager = _PrefabManager;

                //Tidy up the hierarchy naming if the gameObject name was generic
                if( blockGroup.gameObject.name == "GameObject" ) { blockGroup.gameObject.name = "BlockGroup"; }
            }

            //Otherwise, create a parent and give it a BlockGroup component
            else
            {
                //Debug.Log( name + " Creating new BlockGroup parent" );

                //Add a BlockGroup to be the parent of this Block
                blockGroup = PrefabManager.InstantiatePrefab( PrefabFactory.Prefabs.bxr_BlockGroup ).GetComponent<BlockGroup>();
                
                //If this component already has a parent, then set the newly created block to be a child of that gameObject
                if( transform.parent != null )
                {
                    blockGroup.transform.parent = transform.parent;
                }

                //Add this Block to be the child of this new BlockGroup
                transform.SetParent( blockGroup.transform );
                
                //Tidy up the editor hierarchy naming if needed
                if( blockGroup.gameObject.name == "GameObject" ) { blockGroup.gameObject.name = "BlockGroup"; }
            }

            //Check if we should perform a command on Start()
            if( sendCommandOnStart )
            {
                SendCommand( commandOnStart );
            }

        } //END Start

        //---------------------------------//
        public void AddPrefabManagerIfNeeded()
        //---------------------------------//
        {
            //If the _prefabManager hook does not exist, grab it from the Asset database when in editor
            if( _PrefabManager == null )
            {
#if UNITY_EDITOR
                Debug.LogError( "Block.cs AddPrefabManagerIfNeeded() Unable to create PrefabManager as the _PrefabManager hook on this component is null, using AssetDatabase lookup as an Editor only backup" );
                _PrefabManager = UnityEditor.AssetDatabase.LoadAssetAtPath<GameObject>( "Assets/BrandXR/Prefabs/bxr_PrefabManager.prefab" );
#else
                Debug.LogError( "Block.cs AddPrefabManagerIfNeeded() Unable to create PrefabManager as the _PrefabManager hook on this component is null" );
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

        //---------------------------------------------------//
        /// <summary>
        /// When using a Gaze system for interaction, this function is called when a BlockButton has a players gaze enter it's collider
        /// </summary>
        /// <param name="blockButton">Which BlockButton has the gaze entered the collider of?</param>
        public virtual void BlockButtonGazeEnter( BlockButton blockButton )
        //---------------------------------------------------//
        {
            //If the BlockButton in question is this block, perform different logic than if the BlockButton is NOT this block
            if( blockButton == this ) { ThisBlockButtonGazeEnter(); }
            else { OtherBlockButtonGazeEnter( blockButton ); }

        } //END BlockButtonGazeEnter

        protected virtual void ThisBlockButtonGazeEnter() { }
        protected virtual void OtherBlockButtonGazeEnter( BlockButton otherBlockButton ) { }




        //---------------------------------------------------//
        /// <summary>
        /// When using a Gaze system for interaction, this function is called when a BlockButton has a players gaze exit the collider
        /// </summary>
        /// <param name="blockButton">Which BlockButton has the gaze exited the collider of?</param>
        public virtual void BlockButtonGazeExit( BlockButton blockButton )
        //---------------------------------------------------//
        {
            //Perform different logic if the gaze exited this BlockButton, or if this block is different
            if( blockButton == this ) { ThisBlockButtonGazeExit(); }
            else { OtherBlockButtonGazeExit( blockButton ); }

        } //END BlockButtonGazeExit

        protected virtual void ThisBlockButtonGazeExit() { }
        protected virtual void OtherBlockButtonGazeExit( BlockButton otherBlockButton ) { }




        //---------------------------------//
        /// <summary>
        /// When selecting a button (either through Gaze, Click, Touch, or button/trigger press), this function is called
        /// </summary>
        /// <param name="blockButton">Which BlockButton has been selected</param>
        public void BlockButtonSelected( BlockButton blockButton )
        //---------------------------------//
        {
            //Perform different logic if the button selected is this Block, or a different block altogether
            if( blockButton == this ) { ThisBlockButtonSelected(); }
            else { OtherBlockButtonSelected( blockButton ); }

        } //END BlockButtonSelected
        
        protected virtual void ThisBlockButtonSelected() { }
        protected virtual void OtherBlockButtonSelected( BlockButton otherBlockButton ) { }


        /// <summary>
        /// Call this to setup a Block with specific values, each Block expects different json parameters and interpets json data differently
        /// </summary>
        /// <param name="jsonData">The json data string to interpret</param>
        public virtual void SetValues( string jsonData ) { }


        /// <summary>
        /// Respond to the internet on the device being lost
        /// </summary>
        public virtual void InternetConnectionLost() { }

        /// <summary>
        /// Respond to the internet on the device being restored after being lost
        /// </summary>
        public virtual void InternetConnectionRestored() { }
        
        /// <summary>
        /// Call this function before calling Destroy() on a Block to stop any timers, tweens, or coroutines and other logic that must be ended before destroying the object to prevent bugs or memory leaks
        /// </summary>
        public virtual void PrepareForDestroy() { }
        
        /// <summary>
        /// Change this Block based on whether the BlockView this is attached to is Two-Dimensional or Three-Dimensional
        /// </summary>
        /// <param name="viewType"></param>
        public virtual void SetToViewType( BlockView.ViewType viewType ) {  }
        
        

        //------------------------------------------//
        /// <summary>
        /// Tell this block to perform an action
        /// </summary>
        /// <param name="commandType">The action to perform</param>
        public void SendCommand( CommandType commandType )
        //------------------------------------------//
        {
            //Debug.Log( "Block.cs SendCommand( " + commandType + " )" );

            if     ( commandType == CommandType.Show )          { Show(); }
            else if( commandType == CommandType.ForceShow )     { ForceShow(); }
            else if( commandType == CommandType.Hide )          { Hide(); }
            else if( commandType == CommandType.ForceHide )     { ForceHide(); }
            else if( commandType == CommandType.Enable )        { Enable(); }
            else if( commandType == CommandType.Disable )       { Disable(); }
            else if( commandType == CommandType.Call )          { Call(); }
            else if( commandType == CommandType.FaceCamera )    { FaceCamera( forceX, forceY, forceZ ); }

        } //END CommandType

        [Button( "Remove Block", ButtonSizes.Large ), FoldoutGroup("Remove Block"), InfoBox( "Press to remove this block from the Block Group parent.\n\nWARNING: You cannot undo this action!", InfoMessageType.Warning )]
        //-------------------------------//
        /// <summary>
        /// Shows a Remove Block button in the editor, destroys this component and GameObject and removes it from the BlockGroup parent component
        /// </summary>
        public void RemoveBlock()
        //-------------------------------//
        {
            
            if( transform.parent != null && transform.parent.GetComponent<BlockGroup>() != null )
            {
                transform.parent.GetComponent<BlockGroup>().RemoveBlock( this );
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

        } //END RemoveBlock

    } //END Class

} //END Namespace