/* 
*   NatCam
*   Copyright (c) 2018 Yusuf Olokoba
*/

namespace NatCamU.Core.Platforms {

    using AOT;
    using UnityEngine;
    using System;
    using System.Runtime.InteropServices;
    using Dispatch;

    public sealed class NatCamiOS : INatCam {

        #region --Events--
        public event PreviewCallback OnStart;
        public event PreviewCallback OnFrame;
        #endregion


        #region --Op vars--
        private Texture2D preview;
        private PhotoCallback photoCallback;
        private static NatCamiOS instance { get { return NatCam.Implementation as NatCamiOS; }}
        #endregion
        

        #region --Properties--
        public INatCamDevice Device { get; private set; }
        public int Camera {
            get { return NatCamBridge.GetCamera(); }
            set { NatCamBridge.SetCamera(value); }
        }
        public Texture Preview { get { return preview; }}
        public bool IsPlaying { get { return NatCamBridge.IsPlaying(); }}
        public bool HasPermissions { get { return NatCamBridge.HasPermissions(); }}
        #endregion


        #region --Ctor--

        public NatCamiOS () {
            NatCamBridge.RegisterCoreCallbacks(onStart, onFrame, onPhoto, null);
            Device = new NatCamDeviceiOS();
            OrientationUtility.onOrient += OnOrient;
            Debug.Log("NatCam: Initialized NatCam 2.0 iOS backend");
        }
        #endregion
        

        #region --Operations--

        public void Play () {
            OnOrient();
            NatCamBridge.Play();
        }

        public void Pause () {
            NatCamBridge.Pause();
        }

        public void Release () {
            OnStart = 
            OnFrame = null;
            NatCamBridge.Release();
            if (preview) Texture2D.Destroy(preview); preview = null;
        }

        public void CapturePhoto (PhotoCallback callback) {
            photoCallback = callback;
            NatCamBridge.CapturePhoto();
        }

        public void CaptureFrame (Texture2D frame) {
            // Get buffer info
            IntPtr ptr;
            NatCamBridge.CaptureFrame(out ptr);
            // Copy
            frame.LoadRawTextureData(ptr, preview.width * preview.height * 4);
            frame.Apply();
        }

        public void CaptureFrame (byte[] pixels, bool flip) {        
            // Handle flip specially
            if (flip) {
                var handle = GCHandle.Alloc(pixels, GCHandleType.Pinned);
                NatCamBridge.InvertFrame(handle.AddrOfPinnedObject());
                handle.Free();
            } else {
                IntPtr ptr;
                NatCamBridge.CaptureFrame(out ptr);
                Marshal.Copy(ptr, pixels, 0, preview.width * preview.height * 4);
            }
        }
        #endregion


        #region --Callbacks--

        [MonoPInvokeCallback(typeof(NatCamBridge.StartCallback))]
        private static void onStart (IntPtr texPtr, int width, int height) {
            if (!instance.preview)
                instance.preview = Texture2D.CreateExternalTexture(width, height, TextureFormat.RGBA32, false, false, texPtr);
            if (instance.preview.width != width || instance.preview.height != height)
                instance.preview.Resize(width, height, instance.preview.format, false);
            instance.preview.UpdateExternalTexture(texPtr);
            if (instance.OnStart != null)
                instance.OnStart();
        }

        [MonoPInvokeCallback(typeof(NatCamBridge.PreviewCallback))]
        private static void onFrame (IntPtr texPtr) {
            if (instance.preview == null)
                return;
            instance.preview.UpdateExternalTexture(texPtr);
            if (instance.OnFrame != null)
                instance.OnFrame();
        }
        
        [MonoPInvokeCallback(typeof(NatCamBridge.PhotoCallback))]
        private static void onPhoto (IntPtr imgPtr, int width, int height) {
            using (var dispatch = new MainDispatch()) {
                if (instance.photoCallback != null && imgPtr != IntPtr.Zero) {
                    var photo = new Texture2D(width, height, TextureFormat.BGRA32, false);
                    photo.LoadRawTextureData(unchecked((IntPtr)(long)(ulong)imgPtr), width * height * 4);
                    photo.Apply();
                    NatCamBridge.ReleasePhoto(imgPtr);
                    instance.photoCallback(photo);
                }
                instance.photoCallback = null;
            }
        }
        #endregion


        #region --Utility--

        private void OnOrient () {
            NatCamBridge.OnOrient((int)OrientationUtility.Orientation);
        }
        #endregion
    }
}