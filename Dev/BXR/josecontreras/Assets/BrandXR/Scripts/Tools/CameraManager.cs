using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace BrandXR
{
    public class CameraManager: MonoBehaviour
    {
        private enum StereoSetting
        {
            LeftEye,
            RightEye
        }

        //-------------------------------------------------//
        public static void CheckForStereoscopicCameras( bool showDebug )
        //-------------------------------------------------//
        {
            //Grab the cameras in the scene
            Camera[] cameras = GameObject.FindObjectsOfType<Camera>();
            
            //There are no cameras in the scene, lets create two with the LeftEye/RightEye settings
            if( cameras == null || ( cameras != null && cameras.Length == 0 ) )
            {
                //if( showDebug ) { Debug.Log( "CameraManager.cs CheckForStereoscopicCameras() No cameras in the scene, creating two cameras" ); }

                SetToStereoSettings( CreateCamera(), StereoSetting.LeftEye );
                SetToStereoSettings( CreateCamera(), StereoSetting.RightEye );
            }

            //If there is one camera already in the scene, copy and paste it and then set the older camera to LeftEye and the newer to RightEye
            else if( cameras != null && cameras.Length == 1 )
            {
                //if( showDebug ) { Debug.Log( "CameraManager.cs CheckForStereoscopicCameras() One camera already in the scene, adding an additional camera for Stereoscopic effects" ); }

                Camera rightEye = CreateCamera();
                rightEye.CopyFrom( cameras[ 0 ] );

                SetToStereoSettings( cameras[ 0 ], StereoSetting.LeftEye );
                SetToStereoSettings( rightEye, StereoSetting.RightEye );
            }

            //If we have two cameras in the scene already, then double check that the Stereo settings are correct
            else if( cameras != null && cameras.Length == 2 )
            {
                //if( showDebug ) { Debug.Log( "CameraManager.cs CheckForStereoscopicCameras() Two cameras already in the scene, applying stereo settings" ); }

                SetToStereoSettings( cameras[ 0 ], StereoSetting.LeftEye );
                SetToStereoSettings( cameras[ 1 ], StereoSetting.RightEye );
            }

            //If there's three or more cameras, then check to see if any of the camera names are a hint to determine what settings should be applied to them
            else if( cameras != null && cameras.Length >= 3 )
            {
                //if( showDebug ) { Debug.Log( "CameraManager.cs CheckForStereoscopicCameras() Three or more cameras in the scene, using Names to try to determine correct Stereo settings" ); }

                for( int i = 0; i < cameras.Length; i++ )
                {
                    if( cameras[ i ].name.ToLower().Contains( "left" ) )
                    {
                        //if( showDebug ) { Debug.Log( "CameraManager.cs CheckForStereoscopicCameras() Found camera with 'left' in the name, applying LeftEye stereoscopic settings" ); }
                        SetToStereoSettings( cameras[ i ], StereoSetting.LeftEye );
                    }
                    else if( cameras[ i ].name.ToLower().Contains( "right" ) )
                    {
                        //if( showDebug ) { Debug.Log( "CameraManager.cs CheckForStereoscopicCameras() Found camera with 'right' in the name, applying RightEye stereoscopic settings" ); }
                        SetToStereoSettings( cameras[ i ], StereoSetting.RightEye );
                    }
                }
            }


        } //END CheckForStereoscopicCameras

        //------------------------------------------------//
        private static Camera CreateCamera()
        //------------------------------------------------//
        {

            Camera camera = ( new GameObject() ).AddComponent<Camera>();
            
            

            return camera;

        } //END CreateCamera

        //------------------------------------------------//
        private static void SetToStereoSettings( Camera camera, StereoSetting stereoSetting )
        //------------------------------------------------//
        {
            int oldMask = camera.cullingMask;

            //LeftEye cannot see objects with the RightEye layermask
            if( stereoSetting == StereoSetting.LeftEye )
            {
                if( camera.name.ToLower().Contains( "gameobject" ) || camera.name.ToLower().Contains( "new" ) ) { camera.name = "Left Eye Camera"; }

                HideCameraCullingMask( camera, "RightEye" );
            }

            //RightEye cannot see object with the LeftEye layermask
            else if( stereoSetting == StereoSetting.RightEye )
            {
                if( camera.name.ToLower().Contains( "gameobject" ) || camera.name.ToLower().Contains( "new" ) ) { camera.name = "Right Eye Camera"; }

                HideCameraCullingMask( camera, "LeftEye" );
            }

            //Check if AudioSource exists, if not add it to this camera
            if( GameObject.FindObjectOfType<AudioListener>() == null )
            {
                camera.gameObject.AddComponent<AudioListener>();
            }

        } //END SetToStereoSettings





        //https://forum.unity3d.com/threads/how-to-toggle-on-or-off-a-single-layer-of-the-cameras-culling-mask.340369/

        //---------------------------------------------------------------------//
        private static void ShowCameraCullingMask( Camera camera, string layerName )
        //---------------------------------------------------------------------//
        {
            // Turn on the bit using an OR operation:
            camera.cullingMask |= 1 << LayerMask.NameToLayer( layerName );

        } //END ShowCameraCullingMask

        //---------------------------------------------------------------------//
        private static void HideCameraCullingMask( Camera camera, string layerName )
        //---------------------------------------------------------------------//
        {
            // Turn off the bit using an AND operation with the complement of the shifted int:
            camera.cullingMask &= ~( 1 << LayerMask.NameToLayer( layerName ) );

        } //END HideCameraCullingMask

        //---------------------------------------------------------------------//
        private static void ToggleCameraCullingMask( Camera camera, string layerName )
        //---------------------------------------------------------------------//
        {
            // Toggle the bit using a XOR operation:
            camera.cullingMask ^= 1 << LayerMask.NameToLayer( layerName );

        } //END ToggleCameraCullingMask


    } //END Class

} //END Namespace