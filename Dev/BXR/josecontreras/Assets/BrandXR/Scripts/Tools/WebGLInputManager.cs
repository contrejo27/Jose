using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace BrandXR
{
    public class WebGLInputManager: MonoBehaviour
    {

        public bool allowUnityToUseKeyboard = false;

        //-------------------------------//
        void Start()
        //-------------------------------//
        {

            if( !allowUnityToUseKeyboard )
            {
                #if !UNITY_EDITOR && UNITY_WEBGL
                 WebGLInput.captureAllKeyboardInput = false;
                #endif
            }

        } //END Start

    } //END Class
}