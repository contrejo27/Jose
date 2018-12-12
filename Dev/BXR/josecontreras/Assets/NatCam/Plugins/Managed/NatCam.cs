/* 
*   NatCam
*   Copyright (c) 2018 Yusuf Olokoba
*/

namespace NatCamU.Core {

    using UnityEngine;
    using System;
    using Platforms;
    using Dispatch;
    using Docs;

    [Doc(@"NatCam")]
    public static class NatCam {

        #region --Events--
        
        /// <summary>
        /// Event fired when the preview starts
        /// </summary>
        [Doc(@"OnStart")]
        public static event PreviewCallback OnStart {
            add { Implementation.OnStart += value; }
            remove { Implementation.OnStart -= value; }
        }
        /// <summary>
        /// Event fired on each camera preview frame
        /// </summary>
        [Doc(@"OnFrame")]
        public static event PreviewCallback OnFrame {
            add { Implementation.OnFrame += value; }
            remove { Implementation.OnFrame -= value; }
        }
        #endregion


        #region --Properties--

        /// <summary>
        /// The backing implementation NatCam uses on this platform
        /// </summary>
        [Doc(@"Implementation")]
        public static readonly INatCam Implementation;
        /// <summary>
        /// The camera preview as a Texture
        /// </summary>
        [Doc(@"Preview")]
        public static Texture Preview { get { return Implementation.Preview; }}
        /// <summary>
        /// Get or set the active camera.
        /// </summary>
        [Doc(@"Camera")]
        public static DeviceCamera Camera {
            get { return Implementation.Camera; }
            set { if (value != -1) Implementation.Camera = value; } // This makes it impossible to nullify active camera
        }
        /// <summary>
        /// Is the preview running?
        /// </summary>
        [Doc(@"IsPlaying")]
        public static bool IsPlaying { get { return Implementation.IsPlaying; }}
        #endregion


        #region --Operations--

        /// <summary>
        /// Start the camera preview
        /// </summary>
        /// <param name="camera">Optional. Camera that the preview should start from</param>
        [Doc(@"Play")]
        public static void Play (DeviceCamera camera = null) {
            if (camera) Implementation.Camera = camera;
            Implementation.Play();
        }

        /// <summary>
        /// Pause the camera preview
        /// </summary>
        [Doc(@"Pause")]
        public static void Pause () {
            Implementation.Pause();
        }

        /// <summary>
        /// Release all NatCam resources
        /// </summary>
        [Doc(@"Release")]
        public static void Release () {
            Implementation.Release();
        }

        /// <summary>
        /// Capture a photo
        /// </summary>
        /// <param name="callback">The callback to be invoked when NatCam receives the captured photo</param>
        [Doc(@"CapturePhoto", @"CapturePhotoDiscussion"), Code(@"TakeAPhoto")]
        public static void CapturePhoto (PhotoCallback callback) {
            if (callback == null) {
                Debug.LogError("NatCam Error: Cannot capture photo when callback is null");
                return;
            }
            if (!IsPlaying) {
                Debug.LogError("NatCam Error: Cannot capture photo when session is not running");
                return;
            }
            Implementation.CapturePhoto(callback);
        }

        /// <summary>
        /// Capture the current preview frame
        /// </summary>
        /// <param name="frame">Destination texture</param>
        /// <returns>Was the preview frame captured?</returns>
        [Doc(@"CaptureFrame", @"CaptureFrameDiscussion"), Code(@"SaveScreenshot")]
        public static void CaptureFrame (Texture2D frame) {
            // Check
            if (!IsPlaying) {
                Debug.LogError("NatCam Error: Cannot capture frame when preview is not running");
                return;
            }
            if (!frame) {
                Debug.LogError("NatCam Error: Cannot capture frame to null texture");
                return;
            }
            if (frame.width != Preview.width || frame.height != Preview.height) {
                Debug.LogError("NatCam Error: Texture size must match that of NatCam.Preview");
                return;
            }
            if (frame.format != TextureFormat.RGBA32)
                Debug.LogWarning("NatCam: Frame texture format should be RGBA32");
            // Pass to implementation
            Implementation.CaptureFrame(frame);
        }

        /// <summary>
        /// Capture the current preview frame.
        /// The preview data is copied into the provided byte array.
        /// </summary>
        /// <param name="pixels">Destination pixel buffer</param>
        /// <param name="flip">Apply vertical flip? Useful for OpenCV</param>
        /// <returns>Was the preview frame read?</returns>
        [Doc(@"CaptureFrameData", @"CaptureFrameDataDiscussion"), Code(@"OpenCVMat")]
        public static void CaptureFrame (byte[] pixels, bool flip = false) {
            // Check
            if (!IsPlaying) {
                Debug.LogError("NatCam Error: Cannot capture frame when preview is not running");
                return;
            }
            if (pixels == null) {
                Debug.LogError("NatCam Error: Cannot capture frame to null pixel buffer");
                return;
            }
            if (pixels.Length < Preview.width * Preview.height * 4) {
                Debug.LogError("NatCam Error: Pixel buffer is not large enough");
                return;
            }
            // Pass to implementation
            Implementation.CaptureFrame(pixels, flip);
        }
        #endregion


        #region --Initialization--

        static NatCam () {
            // Instantiate implementation for this platform
            Implementation = 
            #if UNITY_EDITOR || UNITY_STANDALONE
            new NatCamLegacy();
            #elif UNITY_IOS
            new NatCamiOS();
            #elif UNITY_ANDROID
            new NatCamAndroid();
            #else
            new NatCamLegacy();
            #endif
            // Quit when app dies
            DispatchUtility.onQuit += Release;            
        }
        #endregion
    }
}