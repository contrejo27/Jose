/* 
*   NatMic
*   Copyright (c) 2018 Yusuf Olokoba
*/

namespace NatMicU.Core.Platforms {

    using System;
    using System.Runtime.InteropServices;

    public static class NatMicBridge {
        
        private const string Assembly =
        #if (UNITY_IOS || UNITY_WEBGL) && !UNITY_EDITOR
        "__Internal";
        #else
        "NatMic";
        #endif

        public delegate void AudioDataCallback (AudioEvent audioEvent, IntPtr sampleBuffer, int sampleCount, int sampleRate, int channelCount, long timestamp);

        #if UNITY_IOS || UNITY_STANDALONE || UNITY_EDITOR
        [DllImport(Assembly, EntryPoint = "NMInitialize")]
        public static extern void Initialize (AudioDataCallback dataCallback);
        [DllImport(Assembly, EntryPoint = "NMIsRecording")]
        public static extern bool IsRecording ();
        [DllImport(Assembly, EntryPoint = "NMStartRecording")]
        public static extern void StartRecording (int sampleRate);
        [DllImport(Assembly, EntryPoint = "NMStopRecording")]
        public static extern void StopRecording ();
        #else
        public static void Initialize (AudioDataCallback encodeCallback) {}
        public static bool IsRecording () { return false; }
        public static void StartRecording (int sampleRate) {}
        public static void StopRecording () {}
        #endif
    }
}