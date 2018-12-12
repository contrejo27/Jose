using UnityEngine;

namespace BrandXR
{
    public class DontDestroyOnLoad: MonoBehaviour
    {

        //----------------------------------------------//
        public void Awake()
        //----------------------------------------------//
        {

            DontDestroyOnLoad( transform.gameObject );

        } //END Awake

    } //END Class

} //END Namespace