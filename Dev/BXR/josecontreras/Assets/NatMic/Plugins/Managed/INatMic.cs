/* 
*   NatMic
*   Copyright (c) 2018 Yusuf Olokoba
*/

namespace NatMicU.Core.Platforms {

    using UnityEngine;

    public interface INatMic {
        bool IsRecording { get; }
        string RecordingPath { get; } // Used by Recorders
        void StartRecording (Format format, SampleBufferCallback dataCallback);
        void StopRecording ();
    }
}