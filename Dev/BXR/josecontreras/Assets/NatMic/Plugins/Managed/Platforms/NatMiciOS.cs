/* 
*   NatMic
*   Copyright (c) 2018 Yusuf Olokoba
*/

namespace NatMicU.Core.Platforms {

    using AOT;
    using UnityEngine;
    using System;
    using System.Runtime.InteropServices;

    public class NatMiciOS : INatMic {

        #region --Op vars--
        private SampleBufferCallback callback;
        private static NatMiciOS instance { get { return NatMic.Implementation as NatMiciOS; }}
        #endregion


        #region --Operations--

        public bool IsRecording { get { return NatMicBridge.IsRecording(); }}

        public string RecordingPath { get; protected set; }

        public NatMiciOS () {
            NatMicBridge.Initialize(OnSampleBuffer);
            RecordingPath = Application.persistentDataPath;
            Debug.Log("NatMic: Initialized NatMic 1.0 iOS backend");
        }

        public void StartRecording (Format format, SampleBufferCallback callback) {
            this.callback = callback;
            NatMicBridge.StartRecording(format.sampleRate);
        }

        public void StopRecording () {
            NatMicBridge.StopRecording();
        }
        #endregion


        #region --Callbacks--

        [MonoPInvokeCallback(typeof(NatMicBridge.AudioDataCallback))]
        private static void OnSampleBuffer (AudioEvent audioEvent, IntPtr sampleBuffer, int sampleCount, int sampleRate, int channelCount, long timestamp) {
            var format = new Format { sampleRate = sampleRate, channelCount = channelCount };
            float[] samples = null;
            if (audioEvent == AudioEvent.OnSampleBuffer) {
                samples = new float[sampleCount];
                Marshal.Copy(sampleBuffer, samples, 0, sampleCount);
            }
            try {
                instance.callback(audioEvent, samples, timestamp, format);
            } catch (Exception ex) {
                Debug.LogError("NatMic Error: Sample buffer callback raised exception: "+ex);
            }
        }
        #endregion
    }
}