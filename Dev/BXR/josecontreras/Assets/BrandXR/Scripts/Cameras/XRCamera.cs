/* XRCamera.cs
 *
 * Controls the various options available within the cameras in Unity.
 */
using UnityEngine;
using System.Collections;
using Sirenix.OdinInspector;
using System.Collections.Generic;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace BrandXR
{
    public class XRCamera: MonoBehaviour
    {
        //List of the cameras used
        public List<Camera> cameras = new List<Camera>();
        public List<Camera> GetCameras() { return cameras; }

        public enum XRCameraType
        {
            TwoDimensional,
            StereoThreeDimensional,
            Vuforia
        }

        public virtual XRCameraType GetXRCameraType() { return XRCameraType.TwoDimensional; }


        [ShowIf( "IsNotParentedToCameraManager" ), InfoBox("WARNING: XRCamera prefab must be parented to bxr_CameraManagePrefab", InfoMessageType.Error)]
        public int warning1 = 0;

        private bool IsNotParentedToCameraManager()
        {
            if( transform.parent == null )
            {
                return true;
            }
            else if( transform.parent != null && transform.parent.GetComponent<XRCameraManager>() == null )
            {
                return true;
            }

            return false;
        }

        [ShowIf( "IsMissingXRCameraManager" ), Button("Create XR Camera Manager", ButtonSizes.Large ), InfoBox("WARNING: Missing bxr_XRCameraManager in hierarchy which is required for an XRCamera to work.\n\nPress the button below to create the bxr_XRCameraManager now", InfoMessageType.Error )]
        public void CreateXRCameraManager()
        {
#if UNITY_EDITOR
            if( GameObject.FindObjectOfType<XRCameraManager>() == null )
            {
                GameObject prefab = (GameObject)AssetDatabase.LoadAssetAtPath( "Assets/BrandXR/Prefabs/Cameras/bxr_XRCameraManager.prefab", typeof( GameObject ) );
                GameObject go = Instantiate( prefab );

                go.name = "bxr_XRCameraManager";
                BlockHelper.AddToBrandXRTechParent( go.transform );

                this.transform.parent = go.transform;
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


        [Button("Destroy Camera", ButtonSizes.Large), FoldoutGroup("Destroy Camera"), InfoBox("Convenience functionality to destroy this camera")]
        public void DestroyCamera()
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
        } //END DestroyCamera


        //---------------------------------//
        public virtual void DisableCamera()
        //---------------------------------//
        {

            if( this.gameObject != null )
            {
                this.gameObject.SetActive( false );
            }
            
        } //END DisableCamera

        //---------------------------------//
        public virtual void EnableCamera()
        //---------------------------------//
        {

            if( this.gameObject != null )
            {
                this.gameObject.SetActive( true );
            }

        } //END EnableCamera

        //---------------------------------//
        public virtual bool IsFloorTrackerIconEnabled()
        //---------------------------------//
        {
            
            return false;

        } //END EnableFloorTrackerIcon

        //---------------------------------//
        public virtual void EnableFloorTrackerIcon()
        //---------------------------------//
        {

            

        } //END EnableFloorTrackerIcon

        //---------------------------------//
        public virtual void DisableFloorTrackerIcon()
        //---------------------------------//
        {

            

        } //END DisableFloorTrackerIcon

        //------------------------------------//
        public virtual void SetFloorTrackerIconTexture( Texture newFloorIconTexture )
        //------------------------------------//
        {



        } //END SetFloorTrackerIconTexture

    } //END Class

} //END Namespace