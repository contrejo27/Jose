/* 
*   NatMic
*   Copyright (c) 2018 Yusuf Olokoba
*/

namespace NatMicU.Core.Platforms {

    using UnityEngine;

    public sealed class NatMicWindows : NatMiciOS {

        public NatMicWindows () : base() {
            RecordingPath = 
            #if UNITY_EDITOR
            System.IO.Directory.GetCurrentDirectory();
            #else
            Application.persistentDataPath;
            #endif
            Debug.Log("NatMic: Initialized NatMic 1.0 Windows backend with iOS implementation");
        }
    }
}