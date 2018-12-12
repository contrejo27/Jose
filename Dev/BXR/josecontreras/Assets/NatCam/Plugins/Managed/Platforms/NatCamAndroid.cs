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

    public sealed class NatCamAndroid : INatCam {

        #region --Events--
        public event PreviewCallback OnStart;
        public event PreviewCallback OnFrame;
        #endregion


        #region --Op vars--
        private Texture2D preview;
        private IDispatch dispatch;
        private PhotoCallback photoCallback;
        private static NatCamAndroid instance { get { return NatCam.Implementation as NatCamAndroid; }}
        private readonly AndroidJavaObject natcam;
        #endregion


        #region --Properties--
        public INatCamDevice Device { get; private set; }
        public int Camera {
            get {
                try {
                    return natcam.Get<AndroidJavaObject>("activeCamera").Get<int>("index");
                } catch (Exception) {
                    return -1;
                }
            }
            set {
                if (IsPlaying) {
                    Pause();
                    natcam.Call("setCamera", value);
                    Play();
                } else 
                    natcam.Call("setCamera", value);
            }
        }
        public Texture Preview { get { return preview; }}
        public bool IsPlaying { get { return natcam.Call<bool>("isPlaying"); }}
        public bool HasPermissions { get { return natcam.Call<bool>("hasPermissions"); }}
        #endregion


        #region --Ctor--

        public NatCamAndroid () {
            using (var player = new AndroidJavaClass("com.unity3d.player.UnityPlayer")) {
                var cacheDir = player
                                .GetStatic<AndroidJavaObject>("currentActivity")
                                .Call<AndroidJavaObject>("getCacheDir")
                                .Call<string>("toString");
                NatCamBridge.RegisterCoreCallbacks(onStart, onFrame, onPhoto, cacheDir);
            }
            natcam = new AndroidJavaObject("com.yusufolokoba.natcam.NatCam");
            Device = new NatCamDeviceAndroid();
            RenderDispatch.Initialize();
            DispatchUtility.onPause += OnPause;
            OrientationUtility.onOrient += OnOrient;
            Debug.Log("NatCam: Initialized NatCam 2.0 Android backend");
        }
        #endregion
        

        #region --Operations--

        public void Play () {
            dispatch = dispatch ?? new MainDispatch();
            OnOrient();
            natcam.Call("play");
        }

        public void Pause () {
            natcam.Call("pause");
        }

        public void Release () {
            OnStart = 
            OnFrame = null;
            natcam.Call("release");
            if (preview != null) Texture2D.Destroy(preview); preview = null;
            if (dispatch != null) dispatch.Dispose(); dispatch = null;
        }

        public void CapturePhoto (PhotoCallback callback) {
            photoCallback = callback;
            natcam.Call("capturePhoto");
        }

        public void CaptureFrame (Texture2D frame) {
            // Get buffer info
            IntPtr ptr;
            NatCamBridge.CaptureFrame(out ptr);
            if (ptr == IntPtr.Zero) return; // Only happens in frame when app is suspended
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
                if (ptr == IntPtr.Zero) return; // Only happens in frame when app is suspended
                Marshal.Copy(ptr, pixels, 0, preview.width * preview.height * 4);
            }
        }
        #endregion


        #region --Callbacks--

        [MonoPInvokeCallback(typeof(NatCamBridge.StartCallback))]
        private static void onStart (IntPtr texPtr, int width, int height) {
            instance.dispatch.Dispatch(() => {
                if (!instance.preview)
                    instance.preview = Texture2D.CreateExternalTexture(width, height, TextureFormat.RGBA32, false, false, texPtr);
                if (instance.preview.width != width || instance.preview.height != height)
                    instance.preview.Resize(width, height, instance.preview.format, false);
                instance.preview.UpdateExternalTexture(texPtr);
                if (instance.OnStart != null)
                    instance.OnStart();
            });
        }

        [MonoPInvokeCallback(typeof(NatCamBridge.PreviewCallback))]
        private static void onFrame (IntPtr texPtr) {
            instance.dispatch.Dispatch(() => {
                if (instance.preview == null)
                    return;
                instance.preview.UpdateExternalTexture(texPtr);
                if (instance.OnFrame != null)
                    instance.OnFrame();
            });
        }

        [MonoPInvokeCallback(typeof(NatCamBridge.PhotoCallback))]
        private static void onPhoto (IntPtr imgPtr, int width, int height) {
            instance.dispatch.Dispatch(() => {
                if (imgPtr != IntPtr.Zero) {
                    var photo = new Texture2D(width, height, TextureFormat.RGBA32, false);
                    photo.LoadRawTextureData(unchecked((IntPtr)(long)(ulong)imgPtr), width * height * 4);
                    photo.Apply();
                    NatCamBridge.ReleasePhoto(imgPtr);
                    instance.photoCallback(photo);
                }
                instance.photoCallback = null;
            });
        }
        #endregion


        #region --Utility--
        
        private void OnPause (bool paused) {
            natcam.Call("onPause", paused);
        }

        private void OnOrient () {
            natcam.Call("onOrient", (int)OrientationUtility.Orientation);
        }
        #endregion
    }
}