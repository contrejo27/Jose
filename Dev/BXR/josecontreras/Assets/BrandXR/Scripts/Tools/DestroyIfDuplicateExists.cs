using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace BrandXR
{
    public class DestroyIfDuplicateExists: MonoBehaviour
    {

        //---------------------//
        public void Awake()
        //---------------------//
        {

            if( GameObject.FindObjectOfType<DestroyIfDuplicateExists>() != null )
            {
                DestroyIfDuplicateExists[] objectsToCheck = GameObject.FindObjectsOfType<DestroyIfDuplicateExists>();

                bool doesExist = false;

                foreach( DestroyIfDuplicateExists obj in objectsToCheck )
                {
                    if( this.name == obj.name && this.gameObject != obj.gameObject )
                    {
                        //Debug.Log( "this.name( " + this.name + " ) == obj.name( " + obj.name + " ) && this.gameObject( " + this.gameObject + " ) != obj( " + obj.gameObject + " )" );
                        doesExist = true;
                        break;
                    }
                }

                if( doesExist )
                {
                    //Debug.Log( "Found gameObject with same name" );
                    Destroy( this.gameObject );
                }
            }

        } //END Awake

    } //END Class

} //END Namespace