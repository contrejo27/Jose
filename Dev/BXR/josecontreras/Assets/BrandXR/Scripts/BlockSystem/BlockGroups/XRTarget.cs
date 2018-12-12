/* XRTarget.cs
 * 
 * Inherits from BlockGroup.cs.
 * 
 * Controls childed XRTargetBase.cs child components, and provides convenience options 
 * for creating new XRTargetBase BlockGroups in the editor
 * 
 * If the technology used to handle XR is changed via the bxr_XRTargetManager prefab, 
 * then this script will change out all of the XRTargetBase components
 * to use the ones that work with the selected techonology
 */

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Sirenix.OdinInspector;
using System;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace BrandXR
{
    public class XRTarget : BlockGroup
    {

        //---------------- SETUP ERRORS -----------------------------//
        [ShowIf( "IsMissingXRCameraManager" ), Button( "Create XR Camera Manager", ButtonSizes.Large ), InfoBox( "WARNING: Missing bxr_XRCameraManager in hierarchy which is required for the XRTarget work.\n\nPress the button below to create the bxr_XRCameraManager now", InfoMessageType.Error )]
        public void CreateXRCameraManager()
        {
#if UNITY_EDITOR
            if( IsMissingXRCameraManager() )
            {
                GameObject prefab = (GameObject)AssetDatabase.LoadAssetAtPath( "Assets/BrandXR/Prefabs/Cameras/bxr_XRCameraManager.prefab", typeof( GameObject ) );
                GameObject go = Instantiate( prefab );

                go.name = "bxr_XRCameraManager";
                BlockHelper.AddToBrandXRTechParent( go.transform );
            }
#endif
        }

        private bool IsMissingXRCameraManager()
        {
            if( GameObject.FindObjectOfType<XRCameraManager>() == null )
            {
                return true;
            }

            return false;
        }


        //A simple way to add XRTargetBase's in the Unity Editor to this XRTarget
#if UNITY_EDITOR
        [ FoldoutGroup( "Create XR Target Base" ), ShowIf("ShowXRTargetBaseType"), InfoBox( "Convenience functionality to easily create an XR Target Base and attach it to this XRTarget.\n\nWhat options are available is based on what XR technology has been enabled via the bxr_XRTechnologyManager prefab." )]
        public XRTargetBase.XRTargetBaseType_Vuforia vuforiaTargets = XRTargetBase.XRTargetBaseType_Vuforia.Image;

        [Button( "Add XR Target Base", ButtonSizes.Large ), ShowIf( "ShowXRTargetBaseType" ), FoldoutGroup("Create XR Target Base")]
        //--------------------------------------//
        public void AddXRTargetBase()
        //--------------------------------------//
        {

            //Decide on which type of XRTargetBase to load based on what technology is enabled
            GameObject block = (GameObject)AssetDatabase.LoadAssetAtPath( "Assets/BrandXR/Prefabs/BlockSystem/BlockGroups/bxr_XRTarget" + vuforiaTargets + ".prefab", typeof( GameObject ) );
            GameObject go = Instantiate( block );

            go.name = "bxr_XRTarget" + vuforiaTargets;
            go.transform.parent = transform;

        } //END AddXRTarget

        [ShowIf("DoesXRTargetBaseExist"), FoldoutGroup("Create XR Target Base"), InfoBox("You can only have one XRTargetBase per XRTarget. If you'd like to change what XRTargetBase this XRTarget is using, then remove the child XRTargetBase first")]
        public int dummy5 = 0;

        public override bool ShouldShowBlockButton()
        {
            return false;
        }

        public override bool ShouldShowAddNestedBlockGroupButton2D()
        {
            return false;
        }

        public override bool ShouldShowAddNestedBlockGroupButton3D()
        {
            return false;
        }

        public override bool ShouldShowAddNestedBlockGroupButtonXR()
        {
            return false;
        }

        public bool ShowXRTargetBaseType()
        {
            if( ShowXRTechnologyManagerNotSetup() ) { return false; }

            if( GameObject.FindObjectOfType<XRTechnologyManager>() != null &&
                GameObject.FindObjectOfType<XRTechnologyManager>().xrTechnologyType == XRTechnologyManager.XRTechnologyType.None )
            {
                return false;
            }
            
            //If we already have a child XRTargetBase, then we disable our controls for creating additional XRTargetBase's
            if( GetComponentInChildren<XRTargetBase>() == null )
            {
                return true;
            }

            return false;
        }

        public bool DoesXRTargetBaseExist()
        {
            if( GetComponentInChildren<XRTargetBase>() != null )
            {
                return true;
            }

            return false;
        }

        [ SerializeField, FoldoutGroup("Create Blocks"), InfoBox("An XR Target can only be used to create an XR Target Base, it is not intended for directly holding onto Blocks or BlockGroups.\n\nOnce you've created an XR Target Base you can add as many BlockGroups or Blocks to that component as you would like.\n\nYou cannot have nested XRTargets")]
        private int dummy1 = 0;

        [SerializeField, FoldoutGroup( "Create Nested Block Groups" ), InfoBox( "An XR Target can only be used to create an XR Target Base, it is not intended for directly holding onto Blocks or BlockGroups.\n\nOnce you've created an XR Target Base you can add as many BlockGroups or Blocks to that component as you would like.\n\nYou cannot have nested XRTargets" )]
        private int dummy2 = 0;
#endif



#if UNITY_EDITOR
        [FoldoutGroup("Create XR Target Base" ), Button( "Add XRTechnologyManager", ButtonSizes.Large ), ShowIf( "ShowXRTechnologyManagerNotSetup" ), InfoBox( "WARNING: You must have a bxr_XRTechnologyManager prefab in this project to use an XRTarget. Press the button below to create this prefab now!", InfoMessageType.Error )]
        public void AddXRTechnologyManager()
        {
            if( GameObject.FindObjectOfType<XRTechnologyManager>() == null )
            {
                GameObject prefab = (GameObject)AssetDatabase.LoadAssetAtPath( "Assets/BrandXR/Prefabs/bxr_XRTechnologyManager.prefab", typeof( GameObject ) );
                GameObject go = Instantiate( prefab );

                go.name = "bxr_XRTechnologyManager";

                //If the BrandXR Tech gameObject does not exist, create it
                if( GameObject.Find( "BrandXR Tech" ) == null ) { BlockHelper.AddToBrandXRTechParent( go.transform ); }
            }
        }
#endif
        private bool ShowXRTechnologyManagerNotSetup()
        {
            if( GameObject.FindObjectOfType<XRTechnologyManager>() == null )
            {
                return true;
            }

            return false;
        }

        
        [FoldoutGroup("Create XR Target Base"), InfoBox("Unable to create XRTargetBase components, the bxr_XRTechnologyManager prefab has its 'xrTechnologyType' variable set to 'None'.\n\nPlease change the xrTechnologyType variable in the bxr_XRTechnologyManager prefab before using this prefab.\n\nIf you have XRTargetBase children attached to this XRTarget component that do not match the XRTechnology selected, those GameObjects and their children will be removed when a build is made.", InfoMessageType.Error), ShowIf( "ShowXRTechnologyManagerSetToNone" )]
        public int xrWarning1 = 0;

        private bool ShowXRTechnologyManagerSetToNone()
        {
            if( GameObject.FindObjectOfType<XRTechnologyManager>() != null &&
                GameObject.FindObjectOfType<XRTechnologyManager>().xrTechnologyType == XRTechnologyManager.XRTechnologyType.None )
            {
                return true;
            }

            return false;
        }


        /// <summary>
        /// In addition to the normal BlockGroup necessities, 
        /// a XRTarget requires that there is a XRTargetManager component in the scene to reference what technology XRTarget's should be using.
        /// This overrided Start() method checks for this.
        /// </summary>
        //--------------------------------------//
        public override void Start()
        //--------------------------------------//
        {
            base.Start();

            CheckForXRTechnologyManager();

        } //END Start

        //---------------------------------------//
        /// <summary>
        /// If the XRTechnologyManager does not exist, create it and parent it to the BrandXR_Tech prefab
        /// </summary>
        private void CheckForXRTechnologyManager()
        //---------------------------------------//
        {

            //If the XRTechnologyManager does not exist, create it and parent it to the "BrandXR Tech" GameObject
            if( GameObject.FindObjectOfType<XRTechnologyManager>() == null && PrefabManager.instance != null )
            {
                GameObject go = PrefabManager.InstantiatePrefab( PrefabFactory.Prefabs.bxr_XRTechnologyManager );

                if( GameObject.Find( "BrandXR Tech" ) == null ) { new GameObject( "BrandXR Tech" ); }

                GameObject bxr = GameObject.Find( "BrandXR Tech" );
                go.transform.parent = bxr.transform;
            }

        } //END CheckForXRTargetManager

    } //END Class

} //END Namespace
