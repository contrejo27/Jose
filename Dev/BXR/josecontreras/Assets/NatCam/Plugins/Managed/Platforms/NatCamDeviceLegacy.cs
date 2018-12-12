/* 
*   NatCam
*   Copyright (c) 2018 Yusuf Olokoba
*/

#pragma warning disable 0675

namespace NatCamU.Core.Platforms {

    using UnityEngine;
    using DeviceCamera = UnityEngine.WebCamTexture;

    public class NatCamDeviceLegacy : INatCamDevice {

        #region --Op vars--
        private struct Configuration {
            public Core.CameraResolution resolution;
            public float framerate;
        }
        Configuration[] configurations;
        #endregion


        #region --Properties--
        public bool IsFrontFacing (int camera) {
            return DeviceCamera.devices[camera].isFrontFacing;
        }

        public bool IsFlashSupported (int camera) {
            Debug.LogWarning("NatCam Error: Flash is not supported on legacy");
            return false;
        }

        public bool IsTorchSupported (int camera) {
            Debug.LogWarning("NatCam Error: Torch is not supported on legacy");
            return false;
        }

        public float HorizontalFOV (int camera) {
            Debug.LogWarning("NatCam Error: Field of view is not supported on legacy");
            return 0f;
        }

        public float VerticalFOV (int camera) {
            Debug.LogWarning("NatCam Error: Field of view is not supported on legacy");
            return 0f;
        }

        public float MinExposureBias (int camera) {
            Debug.LogWarning("NatCam Error: Exposure is not supported on legacy");
            return 0f;
        }

        public float MaxExposureBias (int camera) {
            Debug.LogWarning("NatCam Error: Exposure is not supported on legacy");
            return 0f;
        }

        public float MaxZoomRatio (int camera) {
            Debug.LogWarning("NatCam Error: Zoom is not supported on legacy");
            return 1f;
        }
        #endregion


        #region --Getters--

        public Core.CameraResolution GetPreviewResolution (int camera) {
            return NatCam.IsPlaying && NatCam.Camera == camera ? new Core.CameraResolution(NatCam.Preview.width, NatCam.Preview.height) : configurations[camera].resolution;
        }

        public Core.CameraResolution GetPhotoResolution (int camera) {
            return GetPreviewResolution(camera);
        }

        public float GetFramerate (int camera) {
            return configurations[camera].framerate;
        }
        
        public float GetExposure (int camera) {
            Debug.LogWarning("NatCam Error: Exposure is not supported on legacy");
            return 0f;
        }
        public int GetExposureMode (int camera) {
            Debug.LogWarning("NatCam Error: Exposure mode is not supported on legacy");
            return 0;
        }
        public int GetFocusMode (int camera) {
            Debug.LogWarning("NatCam Error: Focus mode is not supported on legacy");
            return 0;
        }
        public int GetFlash (int camera) {
            Debug.LogWarning("NatCam Error: Flash is not supported on legacy");
            return 0;
        }
        public bool GetTorchEnabled (int camera) {
            Debug.LogWarning("NatCam Error: Torch is not supported on legacy");
            return false;
        }
        public float GetZoom (int camera) {
            Debug.LogWarning("NatCam Error: Zoom is not supported on legacy");
            return 0f;
        }
        #endregion


        #region --Setters--
        public void SetPreviewResolution (int camera, Core.CameraResolution resolution) {
            configurations[camera].resolution = resolution;
        }

        public void SetPhotoResolution (int camera, Core.CameraResolution resolution) {
            Debug.LogWarning("NatCam Error: Photo resolution is not supported on legacy");
        }

        public void SetFramerate (int camera, float framerate) {
            configurations[camera].framerate = framerate;
        }

        public void SetFocus (int camera, float x, float y) {
            Debug.LogWarning("NatCam Error: Focus is not supported on legacy");
        }

        public void SetExposure (int camera, float bias) {
            Debug.LogWarning("NatCam Error: Exposure is not supported on legacy");
        }

        public void SetExposureMode (int camera, int state) {
            Debug.LogWarning("NatCam Error: Exposure mode is not supported on legacy");
        }

        public void SetFocusMode (int camera, int state) {
            Debug.LogWarning("NatCam Error: Focus mode is not supported on legacy");
        }

        public void SetFlash (int camera, int state) {
            Debug.LogWarning("NatCam Error: Flash is not supported on legacy");
        }

        public void SetTorchEnabled (int camera, bool enabled) {
            Debug.LogWarning("NatCam Error: Torch is not supported on legacy");
        }
        public void SetZoom (int camera, float ratio) {
            Debug.LogWarning("NatCam Error: Zoom is not supported on legacy");
        }
        #endregion


        #region --Ctor--

        public NatCamDeviceLegacy () {
            configurations = new Configuration[DeviceCamera.devices.Length];
        }
        #endregion
    }
}
#pragma warning restore 0675