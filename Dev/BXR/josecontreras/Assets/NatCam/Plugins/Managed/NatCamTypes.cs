/* 
*   NatCam
*   Copyright (c) 2018 Yusuf Olokoba
*/

namespace NatCamU.Core {

	using UnityEngine;
	using System;
    using Docs;

    #region --Delegates--
    /// <summary>
    /// A delegate type used to pass camera events
    /// </summary>
    [Doc(@"PreviewCallback")]
	public delegate void PreviewCallback ();
    /// <summary>
    /// A delegate type that used to pass captured photos
    /// </summary>
	[Doc(@"PhotoCallback")]
    public delegate void PhotoCallback (Texture2D photo);
    #endregion


    #region --Enumerations--
    /// <summary>
    /// Camera exposure mode
    /// </summary>
    [Doc(@"ExposureMode")]
    public enum ExposureMode : byte {
	    [Doc(@"AutoExpose")] AutoExpose = 0,
		[Doc(@"Locked")] Locked = 1
	}
    /// <summary>
    /// Camera flash mode
    /// </summary>
    [Doc(@"FlashMode")]
    public enum FlashMode : byte {
		[Doc(@"FlashOff")] Off = 0,
        [Doc(@"FlashOn")] On = 1,
        [Doc(@"FlashAuto")] Auto = 2
	}
    /// <summary>
    /// Camera focus mode
    /// </summary>
	[Doc(@"FocusMode"), Flags]
    public enum FocusMode : byte {
        [Doc(@"FocusOff")] Off = 0,
        [Doc(@"TapToFocus")] TapToFocus = 1,
        [Doc(@"AutoFocus")] AutoFocus = 2
    }
    #endregion


    #region --Value Types--
    
    /// <summary>
    /// Value type representing camera resolution (width and height)
    /// </summary>
    [Serializable, Doc(@"Resolution")]
    public struct CameraResolution {
        [Doc(@"ResolutionWidth")] public int width;
        [Doc(@"ResolutionHeight")] public int height;
        [Doc(@"ResolutionCtor")] public CameraResolution (int width, int height) { this.width = width; this.height = height; }
        [Doc(@"480p")] public static readonly CameraResolution _640x480 = new CameraResolution(640, 480);
        [Doc(@"720p")] public static readonly CameraResolution _1280x720 = new CameraResolution(1280, 720);
        [Doc(@"1080p")] public static readonly CameraResolution _1920x1080 = new CameraResolution(1920, 1080);
        [Doc(@"LowestResolution")] public static readonly CameraResolution Lowest = new CameraResolution(50, 50);       // NatCam will pick the highest resolution close to this
        [Doc(@"HighestResolution")] public static readonly CameraResolution Highest = new CameraResolution(9999, 9999);   // NatCam will pick the lowest resolution close to this
    }
    #endregion
}