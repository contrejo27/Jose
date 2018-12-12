using UnityEngine;

namespace BrandXR
{
    public class KeepScreenAwake: MonoBehaviour
    {

        //-------------------------------//
        void Awake()
        //-------------------------------//
        {
            Screen.sleepTimeout = (int)SleepTimeout.NeverSleep;
            
        } //END Awake
        
    } //END Class

} //END Namespace
