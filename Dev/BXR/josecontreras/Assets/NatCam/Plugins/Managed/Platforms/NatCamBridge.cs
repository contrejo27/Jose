/* 
*   NatCam
*   Copyright (c) 2018 Yusuf Olokoba
*/

namespace NatCamU.Core.Platforms {

    using System;
    using System.Runtime.InteropServices;

    public static partial class NatCamBridge {

        private const string Assembly =
        #if UNITY_IOS
        "__Internal";
        #else
        "NatCam";
        #endif

        #region ---Delegates---
        public delegate void StartCallback (IntPtr texPtr, int width, int height);
        public delegate void PreviewCallback (IntPtr texPtr);
        public delegate void PhotoCallback (IntPtr imgPtr, int width, int height);
        #endregion

        #if (UNITY_IOS || UNITY_ANDROID) && !UNITY_EDITOR
        [DllImport(Assembly, EntryPoint = "NCCoreRegisterCallbacks")]
        public static extern void RegisterCoreCallbacks (StartCallback startCallback,  PreviewCallback previewCallback, PhotoCallback photoCallback, string context);
        [DllImport(Assembly, EntryPoint = "NCCoreReleasePhoto")]
        public static extern void ReleasePhoto (IntPtr photo);
        [DllImport(Assembly, EntryPoint = "NCCoreCaptureFrame")]
        public static extern void CaptureFrame (out IntPtr ptr);
        [DllImport(Assembly, EntryPoint = "NCCoreInvertFrame")]
        public static extern void InvertFrame (IntPtr dest);
        #else
        public static void RegisterCoreCallbacks (StartCallback startCallback,  PreviewCallback previewCallback, PhotoCallback photoCallback, string context) {}
        public static void ReleasePhoto (IntPtr photo) {}
        public static void CaptureFrame (out IntPtr ptr) { ptr = IntPtr.Zero; }
        public static void InvertFrame (IntPtr dest) {}
        #endif
        

        #if UNITY_IOS && !UNITY_EDITOR
        [DllImport(Assembly, EntryPoint = "NCCoreGetCamera")]
        public static extern int GetCamera ();
        [DllImport(Assembly, EntryPoint = "NCCoreSetCamera")]
        public static extern void SetCamera (int camera);
        [DllImport(Assembly, EntryPoint = "NCCoreIsPlaying")]
        public static extern bool IsPlaying ();
        [DllImport(Assembly, EntryPoint = "NCCorePlay")]
        public static extern void Play ();
        [DllImport(Assembly, EntryPoint = "NCCorePause")]
        public static extern void Pause ();
        [DllImport(Assembly, EntryPoint = "NCCoreRelease")]
        public static extern void Release ();
        [DllImport(Assembly, EntryPoint = "NCCoreCapturePhoto")]
        public static extern void CapturePhoto ();
        [DllImport(Assembly, EntryPoint = "NCCoreOnOrient")]
        public static extern void OnOrient (int orientation);
        [DllImport(Assembly, EntryPoint = "NCCoreOnPause")]
        public static extern void OnPause (bool paused);
        [DllImport(Assembly, EntryPoint = "NCCoreHasPermissions")]
        public static extern bool HasPermissions ();
        #else
        public static int GetCamera () { return -1; }
        public static void SetCamera (int camera) {}
        public static bool IsPlaying () { return false; }
        public static void Play () {}
        public static void Pause () {}
        public static void Release () {}
        public static void CapturePhoto () {}
        public static void OnOrient (int orientation) {}
        public static void OnPause (bool paused) {}
        public static bool HasPermissions () { return false; }
        #endif
    }
}