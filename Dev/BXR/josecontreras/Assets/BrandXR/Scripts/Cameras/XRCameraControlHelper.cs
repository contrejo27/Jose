using Sirenix.OdinInspector;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace BrandXR
{
    //Used in XRCameraController
    [System.Serializable]
    public class XRCameraControlHelper
    {

        [Space( 25f ), Tooltip( "Select the platform that you want to be affected" )]
        public RuntimePlatform PlatformType = RuntimePlatform.Android;

        public enum GyroOptions
        {
            Off,
            On
        }
        [Space( 15f )]
        [Tooltip( "If gyro is available, should we use it to move the camera?" )]
        public GyroOptions gyroOptions = GyroOptions.On;
        public bool IsGyroEnabled() { return gyroOptions == GyroOptions.On; }

        
        public enum MouseOptions
        {
            Off,
            On,
            HorizontalOnly,
            VerticalOnly
        }
        [Space( 15f )]
        [Tooltip( "If Mouse Input is available, should we use it to move the camera? If gyro is enabled, only horizontal movement will work" )]
        public MouseOptions mouseOptions = MouseOptions.On;
        public bool IsMouseEnabled() { return mouseOptions == MouseOptions.On || mouseOptions == MouseOptions.HorizontalOnly || mouseOptions == MouseOptions.VerticalOnly; }

        [ShowIf( "IsMouseEnabled" )]
        [Tooltip( "If any of these are held, move the camera. If list is empty no keyboard key is required" )]
        public KeyCode[] mouseRequiresOneOfTheseKeysHeldToUse = new KeyCode[] { KeyCode.LeftAlt, KeyCode.RightAlt, KeyCode.LeftCommand, KeyCode.RightCommand };

        public enum TouchOptions
        {
            Off,
            On,
            HorizontalOnly,
            VerticalOnly
        }
        [Space( 15f )]
        [InfoBox( "If gyro is enabled, only horizontal movement will work" )]
        [Tooltip( "If Touch Input is available, should we use it to move the camera? If gyro is enabled, only horizontal movement will work" )]
        public TouchOptions touchOptions = TouchOptions.On;
        public bool IsTouchEnabled() { return touchOptions == TouchOptions.On || touchOptions == TouchOptions.HorizontalOnly || touchOptions == TouchOptions.VerticalOnly; }


        public float landscapeFieldOfView = 60f;
        public float portraitFieldOfView = 80f;


        //Constructor
        //--------------------------------------------------//
        public XRCameraControlHelper( RuntimePlatform PlatformType, GyroOptions gyroOptions, MouseOptions mouseOptions, KeyCode[] mouseRequiresOneOfTheseKeysHeldToUse, TouchOptions touchOptions )
        //--------------------------------------------------//
        {
            this.PlatformType = PlatformType;
            this.gyroOptions = gyroOptions;
            this.mouseOptions = mouseOptions;
            this.mouseRequiresOneOfTheseKeysHeldToUse = mouseRequiresOneOfTheseKeysHeldToUse;
            this.touchOptions = touchOptions;

        } //END XRCameraControlHelper

        
    } //END Class

} //END Namespace