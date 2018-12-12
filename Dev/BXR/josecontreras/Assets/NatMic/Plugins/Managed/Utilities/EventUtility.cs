/* 
*   NatMic
*   Copyright (c) 2018 Yusuf Olokoba
*/

namespace NatMicU.Utilities {

    using UnityEngine;
    using System;
    using System.Collections.Generic;

    [AddComponentMenu("")]
    public sealed class EventUtility : MonoBehaviour {

        #region --Client API--
        public static event Action onQuit;
        public static event Action<bool> onPause;
        public static long CurrentTimestamp {
            get {
                return (long)(((double)System.Diagnostics.Stopwatch.GetTimestamp() / System.Diagnostics.Stopwatch.Frequency) * 1e+9);
            }
        }

        public static void Dispatch (Action action) {
            lock (dispatchFence) if (action != null) dispatchQueue.Enqueue(action);
        }
        #endregion


        #region --Operations--

        private static Queue<Action> dispatchQueue = new Queue<Action>();
        private static readonly object dispatchFence = new object();

        private EventUtility () {}

        static EventUtility () {
            new GameObject("NatMic Event Utility").AddComponent<EventUtility>();
        }

        void Awake () {
            DontDestroyOnLoad(this.gameObject);
            DontDestroyOnLoad(this);
        }

        void Update () {
            lock (dispatchFence) while (dispatchQueue.Count > 0) dispatchQueue.Dequeue()();
        }
        
        void OnApplicationPause (bool paused) {
            if (onPause != null) onPause(paused);
        }
        
        void OnApplicationQuit () {
            if (onQuit != null) onQuit();
        }
        #endregion
    }
}