using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Sirenix.OdinInspector;

namespace BrandXR
{
    public class XRCameraStereo3D: XRCamera
    {

        public override XRCameraType GetXRCameraType() { return XRCameraType.StereoThreeDimensional; }

        [System.Serializable]
        public struct CameraAttributes
        {
            public Camera _camera;
            public Rect _sideBySideRect;
            public Rect _topBottomRect;
        }

        [FoldoutGroup( "Cameras" )]
        public CameraAttributes _leftCamera;
        [FoldoutGroup( "Cameras" )]
        public CameraAttributes _rightCamera;

        private bool _isSideBySide = false;

        void Awake()
        {
            Screen.sleepTimeout = SleepTimeout.NeverSleep;
        }

        void Start()
        {
            Screen.sleepTimeout = SleepTimeout.NeverSleep;

            _isSideBySide = true;
            ToggleCameraOrientation( _isSideBySide );
        }

        public void ToggleCameraOrientation( bool isSideBySide )
        {
            if( isSideBySide )
            {
                _leftCamera._camera.rect = _leftCamera._sideBySideRect;
                _rightCamera._camera.rect = _rightCamera._sideBySideRect;
            }
            else
            {
                _leftCamera._camera.rect = _leftCamera._topBottomRect;
                _rightCamera._camera.rect = _rightCamera._topBottomRect;
            }
        }

        [BoxGroup( "Debugs" ), HideIf( "_isSideBySide" )]
        public void SwitchToSideBySide()
        {
            _isSideBySide = true;
            ToggleCameraOrientation( _isSideBySide );
        }
        [BoxGroup( "Debugs" ), LabelText( "Switch To Top-Bottom" ), ShowIf( "_isSideBySide" )]
        public void SwitchToTopBottom()
        {
            _isSideBySide = false;
            ToggleCameraOrientation( _isSideBySide );
        }

    } //END Class

} //END Namespace