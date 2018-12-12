/* XRTargetBase.cs
 * The base script for all AR Targets (Image/Object/etc). 
 * Behaves like a BlockGroup in the BrandXR system and stores Blocks and other BlockGroups.
 * An XRTargetBase should not ever store a nested XRTargetBase, only other BlockGroups
 */

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Sirenix.OdinInspector;
using System.Linq;

#if VUFORIA
using Vuforia;
#endif

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace BrandXR
{
    public class XRTargetBase : BlockGroup
    {

        //---------------- SETUP ERRORS -----------------------------//
        [ShowIf( "IsMissingXRCameraManager" ), Button( "Create XR Camera Manager", ButtonSizes.Large ), InfoBox( "WARNING: Missing bxr_XRCameraManager in hierarchy which is required for the XRTargetBase to work.\n\nPress the button below to create the bxr_XRCameraManager now", InfoMessageType.Error )]
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




        [ShowIf( "ShowDuplicateWarning" ), InfoBox("WARNING: You have more than one XRTargetBase attached to an XRTarget!\n\nDuplicates will be removed in Awake(). If you want multiple XRTargetBase components, instead you should create multiple XRTarget components attached to a BlockView", InfoMessageType.Error )]
        public int warning1 = 0;

        private bool ShowDuplicateWarning()
        {
            if( transform.GetComponentInParent<XRTarget>() != null )
            {
                XRTarget xrTargetParent = transform.GetComponentInParent<XRTarget>();

                if( xrTargetParent.GetComponentsInChildren<XRTargetBase>() != null &&
                    xrTargetParent.GetComponentsInChildren<XRTargetBase>().Length > 1 )
                {
                    return true;
                }
            }
            
            return false;
        }

        [ShowIf( "ShowNoXRTargetWarning" ), InfoBox("WARNING: You must have an XRTarget parented to this XRTargetBase for it to work properly.\n\nWe use a combination of an XRTarget parent and the bxr_XRTechnologyManager to determine what XR technology is available, so directly adding XRTargetBase prefabs that use specific kinds of XR technology is not allowed.\n\nTry adding an XRTarget prefab to a BlockView instead to get started.\n\nThis GameObject and its children will be destroyed on Awake() if left in its current state!", InfoMessageType.Error)]
        public int warning2 = 0;

        private bool ShowNoXRTargetWarning()
        {
            if( transform.GetComponentInParent<XRTarget>() == null )
            {
                return true;
            }

            return false;
        }

        [ShowIf( "ShowXRTechnologySetToNoneWarning" ), InfoBox( "To enable the creation of XRTargetBase components, you must set the bxr_XRTechnologyManager prefab's 'xrTechnologyType' variable to something other than 'NONE'.\n\nIf you have a child XRTargetBase already, it will be removed when making a build and will cause errors when playing in the editor.", InfoMessageType.Error )]
        public int warning3 = 0;

        private bool ShowXRTechnologySetToNoneWarning()
        {
            if( GameObject.FindObjectOfType<XRTechnologyManager>() != null
                && GameObject.FindObjectOfType<XRTechnologyManager>().xrTechnologyType == XRTechnologyManager.XRTechnologyType.None )
            {
                return true;
            }

            return false;
        }


#if UNITY_EDITOR
        [Button( "Add XRTechnologyManager", ButtonSizes.Large ), ShowIf( "ShowXRTechnologyManagerNotSetup" ), InfoBox( "WARNING: You must have a bxr_XRTechnologyManager prefab in this project to use an XRTargetBase derived component. Press the button below to create this prefab now!", InfoMessageType.Error )]
        public void AddXRTechnologyManager()
        {
            if( GameObject.FindObjectOfType<XRTechnologyManager>() == null )
            {
                GameObject prefab = (GameObject)AssetDatabase.LoadAssetAtPath( "Assets/BrandXR/Prefabs/bxr_XRTechnologyManager.prefab", typeof( GameObject ) );
                GameObject go = Instantiate( prefab );

                go.name = "bxr_XRTechnologyManager";

                //If the BrandXR Tech gameObject does not exist, create it
                if( GameObject.Find("BrandXR Tech") == null ) { BlockHelper.AddToBrandXRTechParent( go.transform ); }
                else { go.transform.parent = GameObject.Find( "BrandXR Tech" ).transform; }
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



        //This is a normal BlockGroup with some additional AR features specific to an AR platform (Vuforia Fusion, ARCore, ARKit)
        public override BlockGroupType GetBlockGroupType() { return BlockGroupType.XRTarget; }

        //Get the list of XRTechnologyTypes this XRTargetBase is supported by
        public virtual List<XRTechnologyManager.XRTechnologyType> GetXRTechnologyType()
        {
            return new List<XRTechnologyManager.XRTechnologyType>()
            {
                XRTechnologyManager.XRTechnologyType.None
            };
        }

        //The Base Types allowed if the technology used is from Vuforia or Vuforia Fusion
        public enum XRTargetBaseType_Vuforia
        {
            Image,
            Object,
            Floor
        };
        public virtual XRTargetBaseType_Vuforia GetXRTargetType() { return XRTargetBaseType_Vuforia.Image; }



        //-----------------------//
        public virtual void Awake()
        //-----------------------//
        {

            DestroyIfIncompatibleXRTechnology();

            DestroyIfAnotherXRTargetBaseExists();

            DestroyIfNoXRTargetParentExists();

        } //END Awake

        //-----------------------//
        public virtual void DestroyIfIncompatibleXRTechnology()
        //-----------------------//
        {
            bool destroy = false;

            if( GameObject.FindObjectOfType<XRTechnologyManager>() != null )
            {
                XRTechnologyManager xrTechnologyManager = GameObject.FindObjectOfType<XRTechnologyManager>();

                if( xrTechnologyManager.xrTechnologyType == XRTechnologyManager.XRTechnologyType.None ||
                    !GetXRTechnologyType().Contains( xrTechnologyManager.xrTechnologyType ) )
                {
                    destroy = true;
                }
            }
            else
            {
                destroy = true;
            }

            if( destroy )
            {
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

        } //END DestroyIfIncompatibleXRTechnology

        //-----------------------//
        private void DestroyIfAnotherXRTargetBaseExists()
        //-----------------------//
        {
            //If we have a parent gameObject...
            if( transform.parent != null )
            {
                //If we have two or more XRTargetBase objects...
                if( transform.parent.GetComponentsInChildren<XRTargetBase>() != null )
                {
                    List<XRTargetBase> xRTargetBases = transform.parent.GetComponentsInChildren<XRTargetBase>().ToList();

                    //We have more than one XRTargetBase, destroy the other XRTargetBase components and gameObjects
                    if( xRTargetBases != null && xRTargetBases.Count > 1 )
                    {
                        foreach( XRTargetBase xrTargetBase in xRTargetBases )
                        {
                            //We only destroy other XRTargetBases 
                            if( xrTargetBase != null )
                            {
                                xrTargetBase.RemoveBlockGroup();
                            }
                        }
                    }
                }
            }

        } //END DestroyIfAnotherXRTargetBaseExists

        //-------------------------------------//
        private void DestroyIfNoXRTargetParentExists()
        //-------------------------------------//
        {

            if( transform.GetComponentInParent<XRTarget>() == null )
            {
                RemoveBlockGroup();
            }

        } //END DestroyIfNoXRTargetParentExists

        //-----------------------------------------//
        public override void Update()
        //-----------------------------------------//
        {

        } //END Update

#if VUFORIA
        //--------------------------------------//
        public virtual void OnFloorTrackingAutomaticHitTest( HitTestResult hitTestResult )
        //--------------------------------------//
        {

        } //END OnFloorTrackingAutomaticHitTest

        //--------------------------------------//
        public virtual void OnFloorTrackingHitTest( HitTestResult hitTestResult )
        //--------------------------------------//
        {

        } //END OnFloorTrackingHitTest
#endif

    } //END Class

} //END Namespace