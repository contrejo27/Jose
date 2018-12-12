/* 
*   NatCam
*   Copyright (c) 2018 Yusuf Olokoba
*/

namespace NatCamU.Core {

    using UnityEngine;
    using System;

    public sealed class OrientationUtility : MonoBehaviour {

        #region --Op vars--
        public static event Action onOrient;
        public static DeviceOrientation Orientation { get; private set; }
        #endregion


        #region --Operations--

        static OrientationUtility () {
            new GameObject("NatCam Orientation Utility").AddComponent<OrientationUtility>();
        }

        void Awake () {
            DontDestroyOnLoad(this.gameObject);
            DontDestroyOnLoad(this);
            Update();
        }

        void Update () {
            var reference = (DeviceOrientation)(int)Screen.orientation; // Input.deviceOrientation
            switch (reference) {
                case DeviceOrientation.FaceDown:
                case DeviceOrientation.FaceUp:
                case DeviceOrientation.Unknown: break;
                default:
                    if (Orientation != reference) {
                        Orientation = reference;
                        if (onOrient != null) onOrient();
                    }
                break;
            }
        }
        #endregion
    }
}