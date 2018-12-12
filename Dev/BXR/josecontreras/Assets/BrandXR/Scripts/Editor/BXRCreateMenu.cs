
#if UNITY_EDITOR
using Sirenix.OdinInspector;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;
#endif

namespace BrandXR
{
#if UNITY_EDITOR
    public class BXRCreateMenu: EditorWindow
#else
    public class BXRCreateMenu
#endif
    {

#if UNITY_EDITOR
        [SerializeField]
        static GameObject BXR_BlockText = (GameObject)AssetDatabase.LoadAssetAtPath( "Assets/BrandXR/Prefabs/BlockSystem/Blocks/bxr_BlockText.prefab", typeof( GameObject ) );
        [SerializeField]
        static GameObject BXR_BlockImage = (GameObject)AssetDatabase.LoadAssetAtPath( "Assets/BrandXR/Prefabs/BlockSystem/Blocks/bxr_BlockImage.prefab", typeof( GameObject ) );
        [SerializeField]
        static GameObject BXR_BlockAudio = (GameObject)AssetDatabase.LoadAssetAtPath( "Assets/BrandXR/Prefabs/BlockSystem/Blocks/bxr_BlockAudio.prefab", typeof( GameObject ) );
        [SerializeField]
        static GameObject BXR_BlockVideo = (GameObject)AssetDatabase.LoadAssetAtPath( "Assets/BrandXR/Prefabs/BlockSystem/Blocks/bxr_BlockVideo.prefab", typeof( GameObject ) );
        [SerializeField]
        static GameObject BXR_BlockButton = (GameObject)AssetDatabase.LoadAssetAtPath( "Assets/BrandXR/Prefabs/BlockSystem/Blocks/bxr_BlockButton.prefab", typeof( GameObject ) );
        [SerializeField]
        static GameObject BXR_BlockModel = (GameObject)AssetDatabase.LoadAssetAtPath( "Assets/BrandXR/Prefabs/BlockSystem/Blocks/bxr_BlockModel.prefab", typeof( GameObject ) );
        [SerializeField]
        static GameObject BXR_BlockEvent = (GameObject)AssetDatabase.LoadAssetAtPath( "Assets/BrandXR/Prefabs/BlockSystem/Blocks/bxr_BlockEvent.prefab", typeof( GameObject ) );

        [SerializeField]
        static GameObject BXR_BlockGroup = (GameObject)AssetDatabase.LoadAssetAtPath( "Assets/BrandXR/Prefabs/BlockSystem/BlockGroups/bxr_BlockGroup.prefab", typeof( GameObject ) );
        [SerializeField]
        static GameObject BXR_XRTarget = (GameObject)AssetDatabase.LoadAssetAtPath( "Assets/BrandXR/Prefabs/BlockSystem/BlockGroups/bxr_XRTarget.prefab", typeof( GameObject ) );

        [SerializeField]
        static GameObject BXR_BlockView = (GameObject)AssetDatabase.LoadAssetAtPath( "Assets/BrandXR/Prefabs/BlockSystem/BlockViews/bxr_BlockView.prefab", typeof( GameObject ) );
        [SerializeField]
        static GameObject BXR_BlockManager = (GameObject)AssetDatabase.LoadAssetAtPath( "Assets/BrandXR/Prefabs/BlockSystem/bxr_BlockManager.prefab", typeof( GameObject ) );

        [SerializeField]
        static GameObject BXR_XRTechnologyManager = (GameObject)AssetDatabase.LoadAssetAtPath( "Assets/BrandXR/Prefabs/bxr_XRTechnologyManager.prefab", typeof( GameObject ) );
        [SerializeField]
        static GameObject BXR_XRCameraManager = (GameObject)AssetDatabase.LoadAssetAtPath( "Assets/BrandXR/Prefabs/Cameras/bxr_XRCameraManager.prefab", typeof( GameObject ) );
        [SerializeField]
        static GameObject BXR_XRInputManager = (GameObject)AssetDatabase.LoadAssetAtPath( "Assets/BrandXR/Prefabs/bxr_XRInputManager.prefab", typeof( GameObject ) );
        [SerializeField]
        static GameObject BXR_VideoTechnologyManager = (GameObject)AssetDatabase.LoadAssetAtPath( "Assets/BrandXR/Prefabs/bxr_VideoTechnologyManager.prefab", typeof( GameObject ) );


        // Add a menu item to create custom GameObjects.
        // Priority 1 ensures it is grouped with the other menu items of the same kind
        // and propagated to the hierarchy dropdown and hierarch context menus

        [MenuItem( "GameObject/BrandXR/BlockManager", false, 1 )]
        //-------------------------------------------------------//
        static void CreateBlockManager( MenuCommand menuCommand )
        //-------------------------------------------------------//
        {
            if( GameObject.FindObjectOfType<BlockManager>() == null )
            {
                BlockManager blockManager = AddBlockManagerIfNull();
                
                SetupGameObject( blockManager.gameObject, menuCommand );
            }
            else
            {
                Debug.Log( "BXRCreateMenu.cs CreateBlockManager() You already have a BlockManager!" );
                Selection.activeObject = GameObject.FindObjectOfType<BlockManager>().gameObject;
            }

        } //END CreateBlockManager

        //-----------------------------------------------//
        private static BlockManager AddBlockManagerIfNull()
        //-----------------------------------------------//
        {

            BlockManager blockManager = GameObject.FindObjectOfType<BlockManager>();

            //If we don't have a BlockManager, create one now
            if( blockManager == null )
            {
                blockManager = Instantiate( BXR_BlockManager ).GetComponent<BlockManager>();
                blockManager.gameObject.name = "bxr_BlockManager";
                BlockHelper.AddToBrandXRTechParent( blockManager.gameObject.transform );
            }

            return GameObject.FindObjectOfType<BlockManager>();

        } //END AddBlockManagerIfNull



        //Validation method for BlockManager
        [MenuItem( "GameObject/BrandXR/BlockManager", true )]
        //-------------------------------------------------------//
        private static bool CreateBlockManagerValidation()
        //-------------------------------------------------------//
        {
            if( GameObject.FindObjectOfType<BlockManager>() == null )
            {
                return true;
            }
            else
            {
                Debug.Log( "BXRCreateMenu.cs CreateBlockManagerValidation() You already have a BlockManager in your scene." );
                return false;
            }

        } //END CreateBlockManagerValidation





        [MenuItem( "GameObject/BrandXR/BlockView", false, 2 )]
        //-------------------------------------------------------//
        static void CreateBlockView( MenuCommand menuCommand )
        //-------------------------------------------------------//
        {
            List<BlockView> blockViews = CreateBlockView();

            if( blockViews != null && blockViews.Count > 0 )
            {
                foreach( BlockView view in blockViews )
                {
                    if( view != null )
                    {
                        Undo.RegisterCreatedObjectUndo( view.gameObject, "Create " + view.gameObject.name );
                        Selection.activeObject = view.gameObject;
                    }
                }

                Debug.Log( "BXRCreateMenu.cs CreateBlockView() Added BlockView 2D (" + blockViews[ 0 ].name.Substring( 18, 1 ) + ") and BlockView 3D (" + blockViews[ 1 ].name.Substring( 18, 1 ) + ")" );
            }
            
        } //END CreateBlockView

        //-------------------------------------------------------//
        private static List<BlockView> CreateBlockView()
        //-------------------------------------------------------//
        {
            AddBlockManagerIfNull();

            if( GameObject.FindObjectOfType<BlockManager>() != null )
            {
                return GameObject.FindObjectOfType<BlockManager>().AddBlockView();
            }

            return null;

        } //END CreateBlockView

        //-------------------------------------------------------//
        private static List<BlockView> AddBlockViewSiblingsIfNull()
        //-------------------------------------------------------//
        {
            AddBlockManagerIfNull();

            if( GameObject.FindObjectOfType<BlockManager>() != null )
            {
                if( GameObject.FindObjectOfType<BlockView>() == null )
                {
                    return GameObject.FindObjectOfType<BlockManager>().AddBlockView();
                }
            }

            return null;

        } //END AddBlockViewSiblingsIfNull

        //-------------------------------------------------------//
        private static BlockGroup AddBlockGroupToLatestBlockView()
        //-------------------------------------------------------//
        {
            AddBlockManagerIfNull();

            if( GameObject.FindObjectOfType<BlockManager>() != null )
            {
                BlockManager blockManager = GameObject.FindObjectOfType<BlockManager>();

                if( blockManager.blockViews != null && blockManager.blockViews.Count > 0 )
                {
                    BlockView view = blockManager.blockViews[ blockManager.blockViews.Count - 1 ];

                    SetBlockViewToBlockGroup( view );

                    return view.AddBlockGroup();
                }
            }

            return null;

        } //END AddBlockGroupToLatestBlockView

        //-------------------------------------------------------//
        private static void SetBlockViewToBlockGroup( BlockView view )
        //-------------------------------------------------------//
        {

            if( view != null )
            {
                if( view.IsViewType2D() )
                {
                    view.blockGroupType2D = BlockGroup.BlockGroupType2D.BlockGroup;
                }
                else if( view.IsViewType3D() )
                {
                    view.blockGroupType3D = BlockGroup.BlockGroupType3D.BlockGroup;
                }
            }

        } //END SetBlockViewToBlockGroup

        //-------------------------------------------------------//
        private static BlockGroup AddXRTargetToLatestBlockView()
        //-------------------------------------------------------//
        {
            AddBlockManagerIfNull();

            if( GameObject.FindObjectOfType<BlockManager>() != null )
            {
                BlockManager blockManager = GameObject.FindObjectOfType<BlockManager>();

                if( blockManager.blockViews != null && blockManager.blockViews.Count > 0 )
                {
                    BlockView view = blockManager.blockViews[ blockManager.blockViews.Count - 1 ];

                    SetBlockViewToXRTarget( view );

                    return view.AddBlockGroup();
                }
            }

            return null;

        } //END AddXRTargetToLatestBlockView

        //-------------------------------------------------------//
        private static void SetBlockViewToXRTarget( BlockView view )
        //-------------------------------------------------------//
        {

            if( view != null )
            {
                if( view != null && view.IsViewType3D() )
                {
                    view.blockGroupType3D = BlockGroup.BlockGroupType3D.XRTarget;
                }
            }

        } //END SetBlockViewToXRTarget




        [ MenuItem( "GameObject/BrandXR/BlockGroup", false, 3 )]
        //---------------------------------------------------------------//
        static BlockGroup CreateBlockGroup( MenuCommand menuCommand )
        //---------------------------------------------------------------//
        {
            BlockManager blockManager = AddBlockManagerIfNull();
            BlockGroup group = null;

            //If we have a GameObject for context, check if we can add a new BlockView to it directly
            if( menuCommand != null && menuCommand.context != null &&
                menuCommand.context.GetType() == typeof( GameObject ) )
            {
                //Is this a BlockManager?
                if( ( (GameObject)menuCommand.context ).GetComponent<BlockManager>() != null )
                {
                    //We are adding a BlockGroup to a BlockManager, check to see if there's a valid 3D BlockView available
                    
                    //If there's no BlockViews available, let's add one now!
                    AddBlockViewSiblingsIfNull();

                    group = AddBlockGroupToLatestBlockView();
                }

                //Is this a BlockView?
                else if( ( (GameObject)menuCommand.context ).GetComponent<BlockView>() != null )
                {
                    //We are adding a BlockGroup to a BlockView!
                    BlockView view = ( (GameObject)menuCommand.context ).GetComponent<BlockView>();
                    SetBlockViewToBlockGroup( view );
                    group = view.AddBlockGroup();
                }

                //If this is a BlockGroup?
                else if( ( (GameObject)menuCommand.context ).GetComponent<BlockGroup>() != null )
                {
                    //We are adding a Nested BlockGroup to an existing BlockGroup!
                    group = AddNestedBlockGroup( ( (GameObject)menuCommand.context ).GetComponent<BlockGroup>() );
                }

            }

            //We were unable to add the BlockGroup to the context, let's just add it to the most recent BlockView we can find
            if( group == null )
            {
                AddBlockViewSiblingsIfNull();
                group = AddBlockGroupToLatestBlockView();
            }
            
            Debug.Log( "BXRCreateMenu.cs CreateBlockGroup() Added BlockGroup to " + group.transform.parent.name );

            // Register the creation in the undo system
            Undo.RegisterCreatedObjectUndo( group.gameObject, "Create " + group.gameObject.name );
            Selection.activeObject = group.gameObject;

            return group;

        } //END CreateBlockGroup
        

        //--------------------------------------------------------//
        private static BlockGroup AddNestedBlockGroup( BlockGroup blockGroup )
        //--------------------------------------------------------//
        {

            Debug.Log( "BXRCreateMenu.cs AddNestedBlockGroup() Added Nested BlockGroup to " + blockGroup.name + ", BlockGroupType = " + blockGroup.GetBlockGroupType() );

            if( blockGroup.ShouldShowAddNestedBlockGroupButtonXR() )
            {
                blockGroup.blockGroupTypeXR = BlockGroup.BlockGroupTypeXR.BlockGroup;
                return blockGroup.AddNestedBlockGroupXR();
            }
            else if( blockGroup.ShouldShowAddNestedBlockGroupButton3D() )
            {
                blockGroup.blockGroupType3D = BlockGroup.BlockGroupType3D.BlockGroup;
                return blockGroup.AddNestedBlockGroup3D();
            }
            else if( blockGroup.ShouldShowAddNestedBlockGroupButton2D() )
            {
                blockGroup.blockGroupType2D = BlockGroup.BlockGroupType2D.BlockGroup;
                return blockGroup.AddNestedBlockGroup2D();
            }

            return null;

        } //END AddNestedBlockGroup

        //--------------------------------------------------------//
        private static BlockGroup AddNestedXRTargetBlockGroup( BlockGroup blockGroup )
        //--------------------------------------------------------//
        {

            Debug.Log( "BXRCreateMenu.cs AddNestedXRTargetBlockGroup() Added Nested XRTarget BlockGroup to " + blockGroup.name + ", BlockGroupType = " + blockGroup.GetBlockGroupType() );

            if( blockGroup.ShouldShowAddNestedBlockGroupButtonXR() )
            {
                blockGroup.blockGroupTypeXR = BlockGroup.BlockGroupTypeXR.BlockGroup;
                return blockGroup.AddNestedBlockGroupXR();
            }

            return null;

        } //END AddNestedXRTargetBlockGroup




        [ MenuItem( "GameObject/BrandXR/XRTarget", false, 4 )]
        //--------------------------------------------------------------//
        static XRTarget CreateXRTarget( MenuCommand menuCommand )
        //--------------------------------------------------------------//
        {

            BlockManager blockManager = AddBlockManagerIfNull();
            BlockGroup group = null;

            //If we have a GameObject for context, check if we can add a new BlockView to it directly
            if( menuCommand != null && menuCommand.context != null &&
                menuCommand.context.GetType() == typeof( GameObject ) )
            {
                //Is this a BlockManager?
                if( ( (GameObject)menuCommand.context ).GetComponent<BlockManager>() != null )
                {
                    //We are adding a BlockGroup to a BlockManager, check to see if there's a valid 3D BlockView available

                    //If there's no BlockViews available, let's add one now!
                    AddBlockViewSiblingsIfNull();

                    group = AddXRTargetToLatestBlockView();
                }

                //Is this a BlockView?
                else if( ( (GameObject)menuCommand.context ).GetComponent<BlockView>() != null )
                {
                    //We are adding a BlockGroup to a BlockView!
                    BlockView view = ( (GameObject)menuCommand.context ).GetComponent<BlockView>();
                    SetBlockViewToXRTarget( view );
                    group = view.AddBlockGroup();
                }

                //If this is a BlockGroup?
                else if( ( (GameObject)menuCommand.context ).GetComponent<BlockGroup>() != null )
                {
                    //We are adding a Nested BlockGroup to an existing BlockGroup!
                    group = AddNestedXRTargetBlockGroup( ( (GameObject)menuCommand.context ).GetComponent<BlockGroup>() );
                }

            }

            //We were unable to add the BlockGroup to the context, let's just add it to the most recent BlockView we can find
            if( group == null )
            {
                AddBlockViewSiblingsIfNull();
                group = AddXRTargetToLatestBlockView();
            }

            Debug.Log( "BXRCreateMenu.cs CreateXRTarget() Added XRTarget to " + group.transform.parent.name );

            // Register the creation in the undo system
            Undo.RegisterCreatedObjectUndo( group.gameObject, "Create " + group.gameObject.name );
            Selection.activeObject = group.gameObject;

            return group.GetComponent<XRTarget>();

        } //END CreateXRTarget



        [MenuItem( "GameObject/BrandXR/XRTechnologyManager", false, 5 )]
        //-----------------------------------------------------------------//
        static void CreateXRTechnologyManager( MenuCommand menuCommand )
        //-----------------------------------------------------------------//
        {
            if( GameObject.FindObjectOfType<XRTechnologyManager>() == null )
            {
                GameObject go = Instantiate( BXR_XRTechnologyManager );
                go.name = "bxr_XRTechnologyManager";

                SetupGameObject( go, menuCommand );
            }
            else
            {
                Debug.Log( "BXRCreateMenu.cs CreateXRTechnologyManager() You already have a XRTechnologyManager!" );
                Selection.activeObject = GameObject.FindObjectOfType<XRTechnologyManager>().gameObject;
            }

        } //END CreateXRTechnologyManager


        [MenuItem( "GameObject/BrandXR/XRCameraManager", false, 6 )]
        //-----------------------------------------------------------------//
        static void CreateXRCameraManager( MenuCommand menuCommand )
        //-----------------------------------------------------------------//
        {
            if( GameObject.FindObjectOfType<XRCameraManager>() == null )
            {
                GameObject go = Instantiate( BXR_XRCameraManager );
                go.name = "bxr_XRCameraManager";

                SetupGameObject( go, menuCommand );
            }
            else
            {
                Debug.Log( "BXRCreateMenu.cs CreateXRCameraManager() You already have a XRCameraManager!" );
                Selection.activeObject = GameObject.FindObjectOfType<XRCameraManager>().gameObject;
            }

        } //END CreateXRCameraManager


        [MenuItem( "GameObject/BrandXR/XRInputManager", false, 7 )]
        //-----------------------------------------------------------------//
        static void CreateXRInputManager( MenuCommand menuCommand )
        //-----------------------------------------------------------------//
        {
            
            if( GameObject.FindObjectOfType<XRInputManager>() == null )
            {
                GameObject go = Instantiate( BXR_XRInputManager );
                go.name = "bxr_XRInputManager";

                SetupGameObject( go, menuCommand );
            }
            else
            {
                Debug.Log( "BXRCreateMenu.cs CreateXRInputManager() You already have a XRInputManager!" );
                Selection.activeObject = GameObject.FindObjectOfType<XRInputManager>().gameObject;
            }

        } //END CreateXRInputManager


        [MenuItem( "GameObject/BrandXR/VideoTechnologyManager", false, 8 )]
        //-----------------------------------------------------------------//
        static void CreateVideoTechnologyManager( MenuCommand menuCommand )
        //-----------------------------------------------------------------//
        {
            
            if( GameObject.FindObjectOfType<VideoTechnologyManager>() == null )
            {
                GameObject go = Instantiate( BXR_VideoTechnologyManager );
                go.name = "bxr_VideoTechnologyManager";

                SetupGameObject( go, menuCommand );
            }
            else
            {
                Debug.Log( "BXRCreateMenu.cs CreateVideoTechnologyManager() You already have a VideoTechnologyManager!" );
                Selection.activeObject = GameObject.FindObjectOfType<VideoTechnologyManager>().gameObject;
            }

        } //END CreateVideoTechnologyManager


        //--------------------------------------------------------------------------//
        static void SetupGameObject( GameObject go, MenuCommand menuCommand )
        //--------------------------------------------------------------------------//
        {
            // Ensure it gets reparented if this was a context click (otherwise does nothing)
            GameObjectUtility.SetParentAndAlign( go, menuCommand.context as GameObject );

            BlockHelper.AddToBrandXRTechParent( go.transform );

            // Register the creation in the undo system
            Undo.RegisterCreatedObjectUndo( go, "Create " + go.name );
            Selection.activeObject = go;

        } //END SetupGameObject









        //------------------------------------------------------------------//
        private static BlockGroup AddBlockGroupForBlockIfNull()
        //------------------------------------------------------------------//
        {

            //Make sure there is a BlockGroup for us to add a Block to
            AddBlockManagerIfNull();
            AddBlockViewSiblingsIfNull();

            if( GameObject.FindObjectOfType<BlockView>() != null )
            {
                return AddBlockGroupToLatestBlockView();
            }

            return null;

        } //END AddBlockGroupForBlockIfNull

        //------------------------------------------------------------------//
        private static GameObject CreateBlock( GameObject blockReference, string name, BlockGroup group )
        //------------------------------------------------------------------//
        {

            GameObject go = Instantiate( blockReference );
            go.name = name;

            go.transform.SetParent( group.transform );

            Debug.Log( "BXRCreateMenu.cs CreateBlock() Added new Block" + blockReference.GetComponent<Block>().GetBlockType() + " to " + group.name );

            return go;

        } //END CreateBlock

        //-----------------------------------------------------------------//
        private static BlockGroup GetBlockGroupFromMenuCommand( MenuCommand menuCommand )
        //-----------------------------------------------------------------//
        {

            if( menuCommand != null && menuCommand.context != null && 
                ( menuCommand.context.GetType() == typeof(GameObject) ) &&
                ( menuCommand.context as GameObject ).GetComponent<BlockGroup>() != null )
            {
                return ( menuCommand.context as GameObject ).GetComponent<BlockGroup>();
            }

            return null;

        } //END GetBlockGroupFromMenuCommand





        [MenuItem( "GameObject/BrandXR/BlockText", false, 150 )]
        //-----------------------------------------------------------------//
        static void CreateBlockText( MenuCommand menuCommand )
        //-----------------------------------------------------------------//
        {
            BlockGroup group = GetBlockGroupFromMenuCommand( menuCommand );

            if( group == null )
            {
                group = AddBlockGroupForBlockIfNull();
            }
            
            GameObject go = CreateBlock( BXR_BlockText, "bxr_BlockText", group );
            
            // Register the creation in the undo system
            Undo.RegisterCreatedObjectUndo( go, "Create " + go.name );
            Selection.activeObject = go;

        } //END CreateBlockText



        [MenuItem( "GameObject/BrandXR/BlockImage", false, 151 )]
        //-----------------------------------------------------------------//
        static void CreateBlockImage( MenuCommand menuCommand )
        //-----------------------------------------------------------------//
        {
            BlockGroup group = GetBlockGroupFromMenuCommand( menuCommand );

            if( group == null )
            {
                group = AddBlockGroupForBlockIfNull();
            }

            GameObject go = CreateBlock( BXR_BlockImage, "bxr_BlockImage", group );

            // Register the creation in the undo system
            Undo.RegisterCreatedObjectUndo( go, "Create " + go.name );
            Selection.activeObject = go;

        } //END CreateBlockImage



        [MenuItem( "GameObject/BrandXR/BlockAudio", false, 152 )]
        //-----------------------------------------------------------------//
        static void CreateBlockAudio( MenuCommand menuCommand )
        //-----------------------------------------------------------------//
        {
            BlockGroup group = GetBlockGroupFromMenuCommand( menuCommand );

            if( group == null )
            {
                group = AddBlockGroupForBlockIfNull();
            }

            GameObject go = CreateBlock( BXR_BlockAudio, "bxr_BlockAudio", group );

            // Register the creation in the undo system
            Undo.RegisterCreatedObjectUndo( go, "Create " + go.name );
            Selection.activeObject = go;

        } //END CreateBlockAudio



        [MenuItem( "GameObject/BrandXR/BlockVideo", false, 153 )]
        //-----------------------------------------------------------------//
        static void CreateBlockVideo( MenuCommand menuCommand )
        //-----------------------------------------------------------------//
        {
            BlockGroup group = GetBlockGroupFromMenuCommand( menuCommand );

            if( group == null || (group != null && group.transform == null) )
            {
                group = AddBlockGroupForBlockIfNull();
            }

            GameObject go = CreateBlock( BXR_BlockVideo, "bxr_BlockVideo", group );

            // Register the creation in the undo system
            Undo.RegisterCreatedObjectUndo( go, "Create " + go.name );
            Selection.activeObject = go;
            
        } //END CreateBlockVideo



        [MenuItem( "GameObject/BrandXR/BlockButton", false, 154 )]
        //-----------------------------------------------------------------//
        static void CreateBlockButton( MenuCommand menuCommand )
        //-----------------------------------------------------------------//
        {
            BlockGroup group = GetBlockGroupFromMenuCommand( menuCommand );

            if( group == null )
            {
                group = AddBlockGroupForBlockIfNull();
            }

            GameObject go = CreateBlock( BXR_BlockButton, "bxr_BlockButton", group );

            // Register the creation in the undo system
            Undo.RegisterCreatedObjectUndo( go, "Create " + go.name );
            Selection.activeObject = go;

        } //END CreateBlockButton




        [MenuItem( "GameObject/BrandXR/BlockModel", false, 155 )]
        //-----------------------------------------------------------------//
        static void CreateBlockModel( MenuCommand menuCommand )
        //-----------------------------------------------------------------//
        {
            BlockGroup group = GetBlockGroupFromMenuCommand( menuCommand );

            if( group == null )
            {
                group = AddBlockGroupForBlockIfNull();
            }

            GameObject go = CreateBlock( BXR_BlockModel, "bxr_BlockModel", group );

            // Register the creation in the undo system
            Undo.RegisterCreatedObjectUndo( go, "Create " + go.name );
            Selection.activeObject = go;

        } //END CreateBlockModel




        [MenuItem( "GameObject/BrandXR/BlockEvent", false, 156 )]
        //-----------------------------------------------------------------//
        static void CreateBlockEvent( MenuCommand menuCommand )
        //-----------------------------------------------------------------//
        {
            BlockGroup group = GetBlockGroupFromMenuCommand( menuCommand );

            if( group == null )
            {
                group = AddBlockGroupForBlockIfNull();
            }

            GameObject go = CreateBlock( BXR_BlockEvent, "bxr_BlockEvent", group );

            // Register the creation in the undo system
            Undo.RegisterCreatedObjectUndo( go, "Create " + go.name );
            Selection.activeObject = go;

        } //END CreateBlockEvent

        
        
#endif

    } //END Class

} //END NameSpace