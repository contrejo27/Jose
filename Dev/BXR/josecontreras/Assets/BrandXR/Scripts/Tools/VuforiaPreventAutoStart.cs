using UnityEngine;
using UnityEngine.Events;
using UnityEngine.SceneManagement;

//#if VUFORIA
using Vuforia;
//#endif

using System.Reflection;
using System;

namespace BrandXR
{
    public class VuforiaPreventAutoStart: MonoBehaviour
    {

#if VUFORIA
        //----------------------//
        void Awake()
        //----------------------//
        {
            try
            {
                EventInfo evSceneLoaded = typeof( SceneManager ).GetEvent( "sceneLoaded" );
                Type tDelegate = evSceneLoaded.EventHandlerType;

                MethodInfo attachHandler = typeof( VuforiaRuntime ).GetMethod( "AttachVuforiaToMainCamera", BindingFlags.NonPublic | BindingFlags.Static );

                Delegate d = Delegate.CreateDelegate( tDelegate, attachHandler );
                SceneManager.sceneLoaded -= d as UnityEngine.Events.UnityAction<Scene, LoadSceneMode>;
            }
            catch( Exception e )
            {
                Debug.LogWarning( "VuforiaPreventCreationOnSceneLoad.cs Awake() Can't remove the AttachVuforiaToMainCamera action: " + e.ToString() );
            }

        } //END Awake
#endif

    } //END Class

} //END Namespace