using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace BrandXR
{
    public class XRCamera2D: XRCamera
    {

        public override XRCameraType GetXRCameraType() { return XRCameraType.TwoDimensional; }

    }

} //END Class