/* 
*   NatMic
*   Copyright (c) 2018 Yusuf Olokoba
*/

namespace NatMicU.Core.Platforms {

    using UnityEngine;
    
    public sealed class NatMicOSX : NatMiciOS {

        public NatMicOSX () : base() {
            RecordingPath = 
            #if UNITY_EDITOR
            System.IO.Directory.GetCurrentDirectory();
            #else
            Application.persistentDataPath;
            #endif
            Debug.Log("NatMic: Initialized NatMic 1.0 macOS backend with iOS implementation");
        }
    }
}