using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace BrandXR
{
    public class DisableOnPlatform: MonoBehaviour
    {

        public List<UnityEngine.RuntimePlatform> DisableOnPlatforms;


        //--------------------------------------//
        public void Awake()
        //--------------------------------------//
        {
            bool disable = false;

            if( DisableOnPlatforms != null && DisableOnPlatforms.Count > 0 )
            {
                foreach( UnityEngine.RuntimePlatform platform in DisableOnPlatforms )
                {
                    if( Application.platform == platform )
                    {
                        disable = true;
                        break;
                    }
                }
            }

            if( disable )
            {
                this.gameObject.SetActive( false );
            }

        } //END Awake

    } //END Class

} //END Namespace