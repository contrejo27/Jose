/* 
*   NatMic
*   Copyright (c) 2018 Yusuf Olokoba
*/

namespace NatMicU.Core.Platforms {

    using UnityEngine;

    public class NatMicNull : INatMic {
        
        public bool IsRecording { get { return false; }}

        public string RecordingPath { get { return ""; }}

        public NatMicNull () {
            Debug.Log("NatMic: NatMic 1.0 is not supported on this platform");
        }

        public void StartRecording (Format format, SampleBufferCallback dataCallback) {}

        public void StopRecording () {}
    }
}