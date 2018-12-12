/* 
*   NatMic
*   Copyright (c) 2018 Yusuf Olokoba
*/

namespace NatMicU.Core.Platforms {

    using UnityEngine;
    using System;

    public sealed class NatMicAndroid : AndroidJavaProxy, INatMic {

        #region --Op vars--
        private SampleBufferCallback callback;
        private readonly AndroidJavaObject natmic;
        #endregion


        #region --Operations--
        
        public bool IsRecording { get { return natmic.Call<bool>("isRecording"); }} // Please don't call from a non-main thread :(

        public string RecordingPath { get; private set; }

        public NatMicAndroid () : base("com.yusufolokoba.natmic.NatMicDelegate") {
            natmic = new AndroidJavaObject("com.yusufolokoba.natmic.NatMic", this);
            RecordingPath = Application.persistentDataPath;
            Debug.Log("NatMic: Initialized NatMic 1.0 Android backend");
        }

        public void StartRecording (Format format, SampleBufferCallback callback) {
            this.callback = callback;
            natmic.Call("startRecording", format.sampleRate);
        }

        public void StopRecording () {
            natmic.Call("stopRecording");
        }
        #endregion


        #region --Callbacks--

        private void onSampleBuffer (AudioEvent audioEvent, AndroidJavaObject frame, int sampleRate, int channelCount) {
            var format = new Format { sampleRate = sampleRate, channelCount = channelCount };
            float[] sampleBuffer = null;
            var timestamp = 0L;
            if (audioEvent == AudioEvent.OnSampleBuffer) {
                sampleBuffer = AndroidJNI.FromFloatArray(frame.Get<AndroidJavaObject>("sampleBuffer").GetRawObject());
                timestamp = frame.Get<long>("timestamp");
            }
            try {
                callback(audioEvent, sampleBuffer, timestamp, format);
            } catch (Exception ex) {
                Debug.LogError("NatMic Error: Sample buffer callback raised exception: "+ex);
            }
        }
        #endregion
    }
}